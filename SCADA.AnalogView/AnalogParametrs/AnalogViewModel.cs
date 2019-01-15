using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SCADA.Logging;

namespace SCADA.AnalogView.AnalogParametrs
{
    /// <summary>
    ///  Опсиание отображение аналогового параметра
    /// </summary>
    class AnalogViewModel
    {
        AnalogParamsController analogController;


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

        }

        /// <summary>
        /// Функция обработки введенного значения уставки
        /// </summary>
        /// <param name="txValue"></param>
        /// <param name="ust"></param>
        public void SetNewUstValue(string txValue, UstValue ust)
        {
            // первоначально парсинг нового значения
            float newValue = 0;
            try
            {
                try
                { // Сначала пробуем влоб
                    newValue = float.Parse(txValue);
                }
                catch (FormatException fe)
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
                // запись нового значения уставки
                try
                {
                    analogController.SetNewUstValue(newValue, ust);
                }
                catch (UserMessageException um)
                {
                    Logger.AddWarning(um);
                }
            }
            catch (FormatException fe)
            {
                ust.UpdateValue();          // пустое обновление значения
                Logger.AddWarning(new Exception($"Похоже пользователь ввел не число - '{txValue}'", fe));
                return;
            }
            catch (Exception e)
            {
                ust.UpdateValue();
                Logger.AddError(new Exception($"При попытке изменить значение уставки возникло исключние - '{txValue}'", e));
                return;
            }
        }

        /// <summary>
        ///  Запись уставок в базу данных
        /// </summary>
        public void WriteUsts()
        {
            // TODO  спросить пользователя
            Logger.AddMessages("Выполняется запись уставок");
            try
            {
                analogController.WriteUstavki();
            }
            catch (UserMessageException um)
            {
                // выввод сообщения пользователю
                Logger.AddWarning(um);
            }
            catch (Exception e)
            {
                Logger.AddError(e);
            }
        }
    }
}
