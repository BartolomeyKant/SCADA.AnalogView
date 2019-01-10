using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SCADA.AnalogView.AnalogParametrs.AnalogInterfaces;

namespace SCADA.AnalogView.AnalogParametrs
{

    delegate void UstConteinerChanged(UstavkiContainer newUstContainer);

    class AnalogParamsController
    {

        UstavkiContainer dataBaseUstavki;       //  уставки прочитанные из базы данных
        UstavkiContainer plcUstavki;            //  уставки прочитанные из контроллера
        UstavkiContainer resultUstavki;         //  ссылка на текущий используемый источник уставок

        AnalogValue analogValue;                // текущие значение параметра

        CommonAnalogParams commonParams;        // общие параметры аналогового сигнала

        IDBWorker DBWorker;                     // Объект для чтения/записи из / в базы данных
        IPLCAnalogValueReader PLCValueReader;   // Объект для чтения из контроллера значений аналогового сигнала
        IPLCAnalogValueWriter PLCValueWriter;   // Объект для записи команд управления аналоговым параметром
        IPLCAnalogUstavki PLCUstavki;           // Объект для чтения / записи уставок из / в контроллер

        public AnalogParamsController(IAnalogServiceBuilder builder)
        {
            DBWorker = builder.DBWorker;
            //  PLCValueReader = builder.PLCValueReader;
            //   PLCUstavki = builder.PLCUstavki;
            //  PLCValueWriter = builder.PLCValueWriter;

            UstavkiRead();

        }


        void UstavkiRead()
        {
            dataBaseUstavki = new UstavkiContainer();
            commonParams = new CommonAnalogParams();
            DBWorker.ReadUstavki(ref dataBaseUstavki ,ref commonParams);

        }


        /// <summary>
        /// Событие изменения выбранного контейнера для работы с уставками
        /// </summary>
        public event UstConteinerChanged OnUstConteinerChanged;


    }
}
