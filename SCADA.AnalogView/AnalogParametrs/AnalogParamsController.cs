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

        IDBWriter DBWriter;                     // объект для записи в базу данных

        /// <summary>
        /// Событие изменения выьранного контейнера для работы с уставками
        /// </summary>
        public event UstConteinerChanged OnUstConteinerChanged;


    }
}
