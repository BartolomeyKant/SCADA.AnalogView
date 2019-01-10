using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.AnalogView.AnalogParametrs.AnalogInterfaces
{
    interface IPLCAnalogUstavki
    {
        /// <summary>
        /// Настройка тегов для чтения и записи уставок
        /// </summary>
        /// <param name="tags">Набор тегов уставок</param>
        void SetUstavkiTags(string[] tags);
        /// <summary>
        /// Чтение уставок из контроллера
        /// </summary>
        /// <param name="ustavkiContainer">ссылка на контейнер уставок</param>
        void GetPLCUstavki(out UstavkiContainer ustavkiContainer);
        /// <summary>
        /// Запись уставок в контроллер
        /// </summary>
        /// <param name="ustavkiContainer">ссылка на контейнер уставок</param>
        void SetPLCUstavki(UstavkiContainer ustavkiContainer);
    }
}
