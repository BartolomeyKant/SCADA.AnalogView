using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using SCADA.AnalogView.AnalogParametrs.AnalogInterfaces;
using SCADA.Logging;

namespace SCADA.AnalogView.AnalogParametrs
{

    delegate void UstConteinerChanged(UstavkiContainer newUstContainer);

    class AnalogParamsController
    {


        /// <summary>
        /// Событие изменения выбранного контейнера для работы с уставками
        /// </summary>
        public event UstConteinerChanged OnUstConteinerChanged;

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

        CommonAnalogParams commonParams;        // общие параметры аналогового сигнала
        public CommonAnalogParams CommonParams
        {
            get { return commonParams; }
        }

        IDBWorker DBWorker;                     // Объект для чтения/записи из / в базы данных
        IPLCAnalogValueReader PLCValueReader;   // Объект для чтения из контроллера значений аналогового сигнала
        IPLCAnalogValueWriter PLCValueWriter;   // Объект для записи команд управления аналоговым параметром
        IPLCAnalogUstavki PLCUstavki;           // Объект для чтения / записи уставок из / в контроллер

        public AnalogParamsController(ConfigurationWorker config, IAnalogServiceBuilder builder)
        {
            configuration = config;
            DBWorker = builder.DBWorker;
            PLCValueReader = builder.PLCValueReader;
            PLCUstavki = builder.PLCUstavki;
            PLCValueWriter = builder.PLCValueWriter;

            // функция чтения уставок из базы данных и из контроллера
            UstavkiRead();

        }


        /// <summary>
        /// Чтение уставок из базы данных и  контроллера
        /// Чтение уставок из контролера выполняется ассинхронно
        /// При успешном чтении уставок из контроллера вызывается событие OnUstConteinerChanged
        /// </summary>
        async void UstavkiRead()
        {
            dataBaseUstavki = new UstavkiContainer();
            commonParams = new CommonAnalogParams();
            DBWorker.SetDBTag(configuration.ReadingTag);
            DBWorker.ReadUstavki(ref dataBaseUstavki ,ref commonParams);

            Ustavki = dataBaseUstavki;              // принимаем уставки базы данных, как текущие

            // ассинхронное чтение уставок из контроллера
            await Task.Run(() =>
            {
                // клонируем уставки для сохранения параметров из базы данных
                plcUstavki = (UstavkiContainer)dataBaseUstavki.Clone();

                // модифицируем теги для чтения , потому как получили индекс контроллера для чтения
                for (int i = 0; i < configuration.UstavkiTags.Length; i++)
                {
                    configuration.UstavkiTags[i] = configuration.UstavkiTags[i].Replace(configuration.Indexator, "[" + commonParams.ControllerIndex.ToString() + "]");
                }

                try
                {
                    PLCUstavki.SetUstavkiTags(configuration.UstavkiTags);
                    PLCUstavki.GetPLCUstavki(ref plcUstavki);
                    // Выполнение сравнения уставок контроллера и базы данных
                    plcUstavki.Compare(dataBaseUstavki);
                    Ustavki = plcUstavki;              // принимаем уставки базы данных, как текущие
                }
                catch (Exception e)
                {
                    Logger.AddError(e);
                    return;
                }
            });
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

            // если есть изменения или были отличия с архивом выолняем запис уставок
            if (flIsChangedOrDiffValues)
            {
                // Запись в базу данных и контроллер выполняется ассинхронно
                // Запись в базу данных
                await Task.Run(() =>
                {
                    try
                    {
                        Logger.AddMessages("Запись уставовк в базу данных");
                        DBWorker.WriteUstavki(Ustavki, commonParams);
                    }
                    catch (Exception e)
                    {
                        Logger.AddError(e);
                        throw new UserMessageException("При записи уставок в базу данных возникло исключение", e);
                    }
                });
                // Запись в контроллер
                await Task.Run(() =>
                {
                    try
                    {
                        Logger.AddMessages("Запись уставок в контроллер");
                        PLCUstavki.SetPLCUstavki(Ustavki);
                    }
                    catch (Exception e)
                    {
                        Logger.AddError(e);
                        throw new UserMessageException("При запсии уставок в контроллер возникло исключение", e);
                    }
                });
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
            }
            else
            {
                throw new UserMessageException("Нет измененных уставок");
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
                throw new UserMessageException($"Значение уставки должно быть в диапазоне от {minValue} до {maxValue}");
            }

        }
    }
}
