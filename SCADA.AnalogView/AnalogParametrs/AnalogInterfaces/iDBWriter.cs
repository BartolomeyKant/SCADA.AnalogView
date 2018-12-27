using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.AnalogView.AnalogParametrs.AnalogInterfaces
{
    /// <summary>
    /// Интерфейс для записи уставок в базу данных
    /// </summary>
    interface IDBWriter
    {
        /// <summary>
        /// метод для записи уставок в базу данных
        /// </summary>
        /// <param name="ustavkiContainer">ссылка на текущий актуальный контейнер уставок</param>
        void WriteUstavki(UstavkiContainer ustavkiContainer);
    }
}
