using System;
using System.Globalization;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SCADA.Logging;
using SCADA.AnalogView.DialogWindows;

namespace SCADA.AnalogView.AnalogParametrs
{
    /// <summary>
    ///  Опсиание отображение аналогового параметра
    /// </summary>
    class AnalogViewModel
    {
        AnalogParamsController analogController;

        

        Dispatcher dispatcher;

        UstContainerViewModel ustContainer;
        /// <summary>
        /// модель представления уставок
        /// </summary>
        public UstContainerViewModel UstContainer
        {
            get
            {
                return ustContainer;
            }
        }

        ValueViewModel valueViewModel;
        /// <summary>
        /// модель представления текущего значения
        /// </summary>
        public ValueViewModel ValueViewModel
        {
            get
            {
                return valueViewModel;
            }
        }

        /// <summary>
        /// наименование аналогового сигнала
        /// </summary>
        public string Name
        {
            get { return analogController.CommonParams.Name; }
        }
        /// <summary>
        /// Тег аналогового сигнала
        /// </summary>
        public string Tag
        {
            get { return analogController.CommonParams.Tag; }
        }

        /// <summary>
        /// Создание объекта пердставления аналогового параметра
        /// </summary>
        /// <param name="controller"></param>
        public AnalogViewModel(AnalogParamsController controller)
        {
            analogController = controller;
            ustContainer = new UstContainerViewModel(controller.Ustavki);           // преставление контейнера уставок
            analogController.OnUstConteinerChanged += ustContainer.OnContextChange;        // привязка к событию изменния контекста уставок

            valueViewModel = new ValueViewModel(analogController.AnalogValue, analogController.CommonParams.AdcEgu, analogController.CommonParams.Egu);          // представление текущего значения аналогового сигнала

            // получения диспетчера для текущего потока
            // создание данного объекта будет выполняться в главном потоке
            dispatcher = Dispatcher.CurrentDispatcher;

            analogController.OnSendUserMessage += SendUSerMessageEventHandler;          // привязка к событию выдачи сообщений оператору
        }

        /// <summary>
        /// Обработка события выдачи информационного сообщения оператору
        /// </summary>
        /// <param name="exc"></param>
        void SendUSerMessageEventHandler(UserMessageException exc)
        {
            // открытие информационного окна, обязательно через диспетчер, так как событие может вылетать в разных потоках
            Dialogs.ShowException(exc);
        }

        /// <summary>
        /// Функция проверяет введенное значение и переводит его в число
        /// </summary>
        /// <param name="txValue"></param>
        /// <returns></returns>
        float CheckNewValue(string txValue)
        {
            // первоначально парсинг нового значения
            float newValue = 0;
            try
            {
                try
                { // Сначала пробуем влоб
                    newValue = float.Parse(txValue);
                }
                catch (FormatException)
                {
                    // пробуем поменять DecimalSeparator
                    string separator = "";
                    if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
                        separator = ".";
                    else separator = ",";
                    IFormatProvider fp = new NumberFormatInfo { NumberDecimalSeparator = separator };
                    newValue = float.Parse(txValue, fp);
                }
                Logger.AddMessages($"Пользователь ввел число - {newValue}");
            }
            catch (FormatException fe)
            {
                Logger.AddWarning(new Exception($"Похоже пользователь ввел не число - '{txValue}'", fe));
                throw new UserMessageException("Введенное значение должно быть числом!", MessageType.Warning);
            }
            catch (Exception e)
            {
                Logger.AddError(new Exception($"При попытке изменить значение уставки возникло исключние - '{txValue}'", e));
                throw new UserMessageException("При обработке значения возникло исключение!", e, MessageType.Error);
            }
            return newValue;
        }

        /// <summary>
        /// Функция обработки введенного значения уставки
        /// </summary>
        /// <param name="txValue"></param>
        /// <param name="ust"></param>
        public void SetNewUstValue(string txValue, UstValue ust)
        {
            float newValue = 0;
            // запись нового значения уставки
            try
            {
                newValue = CheckNewValue(txValue);                          // преобразуем текстовое значение в float
                analogController.SetNewUstValue(newValue, ust);             // записываем новое значение в уставки
            }
            catch (UserMessageException um)
            {
                Dialogs.ShowException(um);
                Logger.AddWarning(um);
                ust.UpdateValue();          // пустое обновление значения
            }
        }

        /// <summary>
        ///  Запись уставок в базу данных
        /// </summary>
        public void WriteUsts()
        {
            // TODO сделать проверку прав пользователя
            ResultDialog res = Dialogs.ShowDialogMessage("Выполнить ввод уставок?", MessageType.Question);
            if (res != ResultDialog.Yes)            // если пользователь не ответил ДА
                return;
            Logger.AddMessages("Выполняется запись уставок");
            try
            {
                analogController.WriteUstavki();
            }
            catch (UserMessageException um)
            {
                // выввод сообщения пользователю
                Dialogs.ShowException(um);
                Logger.AddWarning(um);
            }
            catch (Exception e)
            {
                Logger.AddError(e);
            }
        }

        // команад установки/снятия имитации
        public void TogleImit()
        {
            Logger.AddMessages("Переключение режима имитации");
            // TODO сделать проверку прав пользователя
            string dialogMessage;                       // TODO сделать проверку текущего состояния аналога без имитации и выдать соответствующее соообщение
            if (valueViewModel.IsImit)
            {
                dialogMessage = "Вы уверены что хотите снять режим имитации?";
            }
            else
            {
                dialogMessage = "Вы уверены что хотите установить режим имитации?";
            }
            // TODO MessageType тоже выставлять в зависимости от состояния аналогового сигнала без имитации
            ResultDialog res = Dialogs.ShowDialogMessage(dialogMessage, MessageType.Question);
            if (res == ResultDialog.Yes)
            {
                Logger.AddMessages("Пользователь решил переключить режим имитации");
                if (valueViewModel.IsImit)
                {
                    analogController.CmdUnsetImit();            // снять иимитацию
                }
                else
                {
                    analogController.CmdSetImit();            // установить иимитацию
                }
            }
            else
            {
                Logger.AddMessages("Пользователь отказался от переключения режима иммитации");
            }
        }

        /// <summary>
        /// Функция изменения текущего имитированного значения
        /// </summary>
        /// <param name="txValue">текстовое значение</param>
        public void ChangeImitValue(string txValue)
        {
            float value = 0;
            try
            {
                value = CheckNewValue(txValue);
                // TODO сделать коррректировку диалогового сообщения с учетом текущего состояния аналогового сигнала без имитации
                ResultDialog res = Dialogs.ShowDialogMessage($"Вы уверены, что хотите установить значение имитации: {value}", MessageType.Question);
                if (res == ResultDialog.Yes)            // если пользователь согласился
                {
                    analogController.CmdChangeImitValue(value);
                }
            }
            catch (UserMessageException um)
            {
                Dialogs.ShowException(um);
                Logger.AddWarning(um);
            }

            
        }
        
    }
}
