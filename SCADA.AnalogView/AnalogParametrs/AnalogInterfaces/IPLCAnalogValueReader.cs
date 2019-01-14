using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.AnalogView.AnalogParametrs.AnalogInterfaces
{
    interface IPLCAnalogValueReader
    {
        /// <summary>
        /// натстройка тегов для чтения с контроллера
        /// </summary>
        /// <param name="ADCTag">тег для кодов АЦП</param>
        /// <param name="PLCValueTag">тег для текущего значения</param>
        /// <param name="AnalogState">тег для текущего состояния</param>
        void SetAnalogTags(string ADCTag, string PLCValueTag, string AnalogState);
        /// <summary>
        /// Чтение текущего значения аналогового сигнала
        /// </summary>
        /// <param name="value">ссылка на представление аналогового сигнала</param>
        void GetCurrentAnalogValue(out AnalogValue value);
        /// <summary>
        /// Подписка на изменение значений аналогового сигнала
        /// </summary>
        /// <param name="value">Ссылка на предсталение аналогового сигнала</param>
        void SubscribeToChangeAnalogValue(ref AnalogValue value);
    }
}
