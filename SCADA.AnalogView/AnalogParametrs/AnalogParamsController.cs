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

        Thread UstavkiReadingThread;

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
        /// Событие изменения выбранного контейнера для работы с уставками
        /// </summary>
        public event UstConteinerChanged OnUstConteinerChanged;


    }
}
