using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using SCADA.AnalogView.AnalogParametrs.AnalogInterfaces;
using SCADA.Logging;
using SCADA.AnalogView.DialogWindows;

namespace SCADA.AnalogView.AnalogParametrs
{

    delegate void UstConteinerChanged(UstavkiContainer newUstContainer);
    delegate void UserMessageThrowed(UserMessageException exc);

    class AnalogParamsController
    {
        /// <summary>
        /// Событие изменения выбранного контейнера для работы с уставками
        /// </summary>
        public event UstConteinerChanged OnUstConteinerChanged;

        /// <summary>
        /// Событие отправки информационного сообщения  пользователю
        /// </summary>
        public event UserMessageThrowed OnSendUserMessage;

        // Объект конфигурации 
        ConfigurationWorker configuration;

        UstavkiContainer dataBaseUstavki;       //  уставки прочитанные из базы данных
        UstavkiContainer plcUstavki;            //  уставки прочитанные из контроллера
        UstavkiContainer resultUstavki;         //  ссылка на текущий используемый источник уставок
        /// <summary>
        /// Текущий выбранный набор уставок
        /// </summary>
        public UstavkiContainer Ustavki
        {
            get
            {
                return resultUstavki;
            }
            set
            {
                resultUstavki = value;
                OnUstConteinerChanged?.Invoke(resultUstavki);
            }
        }

        AnalogValue analogValue;                // текущие значение параметра
        public AnalogValue AnalogValue
        {
            get { return analogValue; }
        }

        CommonAnalogParams commonParams;        // общие параметры аналогового сигнала
        public CommonAnalogParams CommonParams
        {
            get { return commonParams; }
        }

        IDBWorker DBWorker;                     // Объект для чтения/записи из / в базы данных
        IPLCAnalogValue PLCValue;   // Объект для чтения из контроллера значений аналогового сигнала
        IPLCAnalogUstavki PLCUstavki;           // Объект для чтения / записи уставок из / в контроллер

        public AnalogParamsController(ConfigurationWorker config, IAnalogServiceBuilder builder)
        {
            configuration = config;
            DBWorker = builder.DBWorker;
            PLCValue = builder.PLCValue;
            PLCUstavki = builder.PLCUstavki;

            // функция чтения уставок из базы данных и из контроллера
            UstavkiRead();

            // чтение текущих значений из контроллера
            ReadPLCValue();
            // настройка тегов для команд
            SetCmdTags();       
        }


        /// <summary>
        /// Функция вызывает событие отправки сообщения пользователю
        /// При этом сообщение так же логируется как сообщение 
        /// </summary>
        /// <param name="exc"></param>
        void SendUserMessage(UserMessageException exc)
        {
            Logger.AddMessages($"Выдача сообщения пользователю - {exc.Message}");
            OnSendUserMessage?.Invoke(exc);

        }

        #region // Работа с уставками

        /// <summary>
        /// Чтение уставок из базы данных и  контроллера
        /// Чтение уставок из контролера выполняется ассинхронно
        /// При успешном чтении уставок из контроллера вызывается событие OnUstConteinerChanged
        /// </summary>
        async void UstavkiRead()
        {
            dataBaseUstavki = new UstavkiContainer();
            commonParams = new CommonAnalogParams();

            try
            {
                DBWorker.SetDBTag(configuration.ReadingTag);
                DBWorker.ReadUstavki(ref dataBaseUstavki, ref commonParams);

                Ustavki = dataBaseUstavki;              // принимаем уставки базы данных, как текущие

                // ассинхронное чтение уставок из контроллера
                Task plcReading = Task.Run(() =>
                 {

                     // клонируем уставки для сохранения параметров из базы данных
                     plcUstavki = (UstavkiContainer)dataBaseUstavki.Clone();

                     //// ---- тут проблема в архитектуре
                     //// ---- если адресация будет сильно отличаться, предется переделывать этот кусок здесь, а не в реализации интерфейса
                     //// ---- с другой стороны для реализации в интерфейсе возможно придется менять и сам интерфейс (добавлять параметры на вход)
                     // модифицируем теги для чтения , потому как получили индекс контроллера для чтения
                     for (int i = 0; i < configuration.UstavkiTags.Length; i++)
                     {
                         configuration.UstavkiTags[i] = configuration.UstavkiTags[i].Replace(configuration.Indexator, "[" + commonParams.ControllerIndex.ToString() + "]");
                     }
                     PLCUstavki.SetUstavkiTags(configuration.UstavkiTags);
                     PLCUstavki.GetPLCUstavki(ref plcUstavki);
                     // Выполнение сравнения уставок контроллера и базы данных
                     plcUstavki.Compare(dataBaseUstavki);
                     Ustavki = plcUstavki;              // принимаем уставки базы данных, как текущие
                 });
                await plcReading;
            }
            catch (UserMessageException exc)
            {
                SendUserMessage(exc);
            }
            catch (Exception e)
            {
                Logger.AddError(e);
                SendUserMessage(new UserMessageException("Ошибка в работе приложения", e, MessageType.Error));
                return;
            }
        }

        /// <summary>
        ///  Процедура записи уставок в базу данных и в контроллер
        /// </summary>
        public async void WriteUstavki()
        {
            // Определение наличия уставок которые можно записать
            bool flIsChangedOrDiffValues = false;
            // технологические уставки
            for (int i = 0; i < 12; i++)
            {
                flIsChangedOrDiffValues = flIsChangedOrDiffValues || (Ustavki.UstValues[i].Changed || Ustavki.UstValues[i].Different);
            }
            // общие уставки
            flIsChangedOrDiffValues = flIsChangedOrDiffValues || (Ustavki.ADCMax.Changed || Ustavki.ADCMax.Different);
            flIsChangedOrDiffValues = flIsChangedOrDiffValues || (Ustavki.ADCMin.Changed || Ustavki.ADCMin.Different);
            flIsChangedOrDiffValues = flIsChangedOrDiffValues || (Ustavki.EMax.Changed || Ustavki.EMax.Different);
            flIsChangedOrDiffValues = flIsChangedOrDiffValues || (Ustavki.EMin.Changed || Ustavki.EMin.Different);
            flIsChangedOrDiffValues = flIsChangedOrDiffValues || (Ustavki.VPD.Changed || Ustavki.VPD.Different);
            flIsChangedOrDiffValues = flIsChangedOrDiffValues || (Ustavki.NPD.Changed || Ustavki.NPD.Different);
            flIsChangedOrDiffValues = flIsChangedOrDiffValues || (Ustavki.Hister.Changed || Ustavki.Hister.Different);

            // Запись в базу данных и контроллер выполняется ассинхронно
            try
            {
                // если есть изменения или были отличия с архивом выолняем запис уставок
                if (flIsChangedOrDiffValues)
                {
                    // параллельная запись в базу и в PLC
                    // Запись в базу данных
                    Task t1 = Task.Run(() =>
                    {
                        Logger.AddMessages("Запись уставовк в базу данных");
                        DBWorker.WriteUstavki(Ustavki, commonParams);
                    });
                    // Запись в контроллер
                    Task t2 = Task.Run(() =>
                    {
                        Logger.AddMessages("Запись уставок в контроллер");
                        PLCUstavki.SetPLCUstavki(Ustavki);
                    });

                    await Task.WhenAll(new[] { t1, t2 });

                    // Сброс флагов состояния уставок
                    Ustavki.ClearState();
                    // Переописание уставок и повторное сравнение
                    if (Ustavki == plcUstavki)
                    {
                        dataBaseUstavki = (UstavkiContainer)plcUstavki.Clone();
                    }
                    else
                    {
                        plcUstavki = (UstavkiContainer)dataBaseUstavki.Clone();
                    }
                    plcUstavki.Compare(dataBaseUstavki);

                    throw new UserMessageException("Запись уставок выполнена!", MessageType.JobDone);
                }
                else
                {
                    throw new UserMessageException("Нет измененных уставок");
                }
            }
            catch (AggregateException ae)            // отлавлиавем исключения в await блоке, так как две функции могут выдать два исключения
            {
                foreach (Exception e in ae.InnerExceptions)
                {
                    if (e is UserMessageException)
                    {
                        SendUserMessage((UserMessageException)e);
                    }
                    else
                    {
                        Logger.AddError(e);
                        SendUserMessage(new UserMessageException("Ошибка в работе приложения", e, MessageType.Error));
                    }
                }
                return;
            }
            catch (UserMessageException exc)
            {
                SendUserMessage(exc);
            }
            catch (Exception e)
            {
                Logger.AddError(e);
                SendUserMessage(new UserMessageException("Ошибка в работе приложения", e, MessageType.Error));
                return;
            }
        }

        /// <summary>
        /// Метод обработки нового значения уставки
        /// </summary>
        /// <param name="newValue">Новое значение</param>
        /// <param name="ust">Ссылка на объект изменяемой уставки</param>
        public void SetNewUstValue(float newValue, UstValue ust)
        {
            float minValue = -100000;
            float maxValue = 100000;
            // проверка значения уставки выполняется в зависимости от кода уставки
            // присвоение минимального и максимального значения в зависимости от типа уставки
            switch (ust.UstCode)
            {
                case UstCode.ADCMax:
                    minValue = Ustavki.ADCMin.Value;
                    maxValue = 100000;
                    break;
                case UstCode.ADCMin:
                    minValue = -100000;
                    maxValue = Ustavki.ADCMax.Value;
                    break;
                case UstCode.EMax:
                    maxValue = 100000;
                    minValue = Ustavki.EMin.Value;
                    for (int i = 11; i >= 0; i--)
                    {
                        if (Ustavki.UstValues[i].Used)
                        {
                            minValue = Ustavki.UstValues[i].Value;
                            break;
                        }
                    }
                    break;
                case UstCode.EMin:
                    maxValue = Ustavki.EMax.Value;
                    minValue = -100000;
                    for (int i = 0; i <= 11; i++)
                    {
                        if (Ustavki.UstValues[i].Used)
                        {
                            maxValue = Ustavki.UstValues[i].Value;
                            break;
                        }
                    }
                    break;
                case UstCode.VPD:
                    maxValue = 100000;
                    minValue = Ustavki.NPD.Value;
                    break;
                case UstCode.NPD:
                    maxValue = Ustavki.VPD.Value;
                    minValue = -100000;
                    break;
                case UstCode.Hister:
                    maxValue = Ustavki.EMax.Value;
                    minValue = Ustavki.EMin.Value;
                    break;
                default:
                    if ((ust.UstCode < UstCode.ADCMax) && (ust.UstCode > 0))
                    {
                        maxValue = Ustavki.EMax.Value;
                        minValue = Ustavki.EMin.Value;
                        for (int i = ((int)ust.UstCode) - 2; i >= 0; i--)
                        {
                            if (Ustavki.UstValues[i].Used)
                            {
                                minValue = Ustavki.UstValues[i].Value;
                                break;
                            }
                        }
                        for (int i = ((int)ust.UstCode); i <= 11; i++)
                        {
                            if (Ustavki.UstValues[i].Used)
                            {
                                maxValue = Ustavki.UstValues[i].Value;
                                break;
                            }
                        }
                    }
                    else
                    {
                        ust.UpdateValue();
                        throw new Exception($"Задан неверный тип уставки, код - {ust.UstCode}");
                    }
                    break;
            }
            // -------------------- запись нового значения в объект уставки -------------------------------------------
            if ((newValue >= minValue) && (newValue <= maxValue))
                ust.SetNewValue(newValue);
            else
            {
                ust.UpdateValue();
                throw new UserMessageException($"Значение уставки должно быть в диапазоне от {minValue} до {maxValue}", MessageType.Warning);
            }

        }
        #endregion

        #region // работа со значениями

        /// <summary>
        /// Чтение текущих значений из PLC
        /// </summary>
        async void ReadPLCValue()
        {
            // объект значения аналогового сигнала
            // подписывается на изменение объекта уставок
            // формат представления кодов АЦП времено хардкодится
            analogValue = new AnalogValue() { ADCFormat = 3, ValueFormat = commonParams.format };
            OnUstConteinerChanged += analogValue._OnConteinerChanged;
            analogValue._OnConteinerChanged(Ustavki);

            // подготовка тегов аналогового сигнала
            configuration.ADCTag = configuration.ADCTag.Replace(configuration.Indexator, "[" + commonParams.ControllerIndex + "]");
            configuration.ValueTag = configuration.ValueTag.Replace(configuration.Indexator, "[" + commonParams.ControllerIndex + "]");
            configuration.AnalogStateTag = configuration.AnalogStateTag.Replace(configuration.Indexator, "[" + commonParams.ControllerIndex + "]");

            PLCValue.SetAnalogTags(configuration.ADCTag, configuration.ValueTag, configuration.AnalogStateTag);     // настройка тегов для чтения значений
            // Чтение значений из PLC выполняется ассинхронно
            try
            {
                await Task.Run(() =>
                {
                    PLCValue.GetCurrentAnalogValue(ref analogValue);                             // чтение текущих значений
                });
                // так как подключение к серверу выполнять точно не нужно, нет смысла выполнять ассинхронно
                PLCValue.SubscribeToChangeAnalogValue(ref analogValue);                          // подписка на изменение значения аналогового сигнала
            }
            catch (UserMessageException exc)
            {
                SendUserMessage(exc);
            }
            catch (Exception e)
            {
                Logger.AddError(e);
                SendUserMessage(new UserMessageException("Ошибка в работе приложения", e, MessageType.Error));
                return;
            }
        }

        void SetCmdTags()
        {
            PLCValue.SetCmdTags(configuration.CMDIndexTag, configuration.CMDCmdTag, configuration.CMDValueTag);
        }


        #region Команды изменения значений и состояния аналогового сигнала
        //команда установить имитацию аналогового сигнала
       public void CmdSetImit()
        {
            try
            {
                PLCValue.CmdSetImit(analogValue.PLCValue, commonParams.ControllerIndex);
            }
            catch (UserMessageException um)
            {
                SendUserMessage(um);
            }
            catch (Exception e)
            {
                Logger.AddError(e);
            }
            
        }
        // команда снять имитацию аналогового сигнала
        public void CmdUnsetImit()
        {
            try
            {
                PLCValue.CmdUnsetImit(commonParams.ControllerIndex);
            }
            catch (UserMessageException um)
            {
                SendUserMessage(um);
            }
            catch (Exception e)
            {
                Logger.AddError(e);
            }
        }
        // изменить значение имитации
        public void CmdChangeImitValue(float value)
        {
            analogValue.PLCValue = value;           // предварительно изменяем текущее значение аналогового сигнала
            try
            {
                // записываем новое имитированное значение сигнала
                PLCValue.CmdChangeImitValue(analogValue.PLCValue,commonParams.ControllerIndex);
            }
            catch (UserMessageException um)
            {
                SendUserMessage(um);
            }
            catch (Exception e)
            {
                Logger.AddError(e);
            }
        }
        #endregion
        #endregion
    }
}
