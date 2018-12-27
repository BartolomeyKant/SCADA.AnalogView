using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.AnalogView.AnalogParametrs.AnalogInterfaces
{
    /// <summary>
    /// Интерфейс чтения уставок из базы данных
    /// </summary>
    interface IDBReader
    {
        /// <summary>
        /// Чтение уставок из базы данных в контейнер уставок
        /// </summary>
        /// <param name="ustavkiContainer"> ссылка на контейнер уставок, в который будут прочитаны уставки из быза данных</param>
        void ReadUstavki(out UstavkiContainer ustavkiContainer);


        void ReadCommonParams();
    }
}
