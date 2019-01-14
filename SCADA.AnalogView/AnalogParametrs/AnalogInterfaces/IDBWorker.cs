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
    interface IDBWorker
    {
        /// <summary>
        /// Задает тег для чтения из базы данных
        /// </summary>
        /// <param name="tag"></param>
        void SetDBTag(string tag);
        /// <summary>
        /// Чтение уставок из базы данных в контейнер уставок
        /// </summary>
        /// <param name="ustavkiContainer"> ссылка на контейнер уставок, в который будут прочитаны уставки из быза данных</param>
        void ReadUstavki(ref UstavkiContainer ustavkiContainer, ref CommonAnalogParams commonParams);

        /// <summary>
        /// метод для записи уставок в базу данных
        /// </summary>
        /// <param name="ustavkiContainer">ссылка на текущий актуальный контейнер уставок</param>
        void WriteUstavki(UstavkiContainer ustavkiContainer, CommonAnalogParams commonParams);
    }
}
