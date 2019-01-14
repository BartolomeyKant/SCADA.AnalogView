using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.AnalogView.AnalogParametrs.AnalogInterfaces
{
    /// <summary>
    /// Строитель - объектов сервисов для контроллера
    /// </summary>
    interface IAnalogServiceBuilder
    {
        /// <summary>
        /// объект для работы с БД
        /// </summary>
        IDBWorker DBWorker { get; }
        /// <summary>
        /// Объект для чтения/ записи уставок с контроллера
        /// </summary>
        IPLCAnalogUstavki PLCUstavki { get; }
        /// <summary>
        /// Объект для чтения значений с контроллера
        /// </summary>
        IPLCAnalogValueReader PLCValueReader { get; }
        /// <summary>
        /// Объект для записи команд в контроллер
        /// </summary>
        IPLCAnalogValueWriter PLCValueWriter { get; }
    }
}
