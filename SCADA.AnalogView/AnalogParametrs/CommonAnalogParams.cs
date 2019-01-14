using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.AnalogView.AnalogParametrs
{
    /// <summary>
    /// Общие параметры аналоговго сигнала, такие как название, тег, индекс
    /// </summary>
    class CommonAnalogParams
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        public uint ControllerIndex { get; set; }
        public uint DBIndex { get; set; }

        public string Egu;

        public string AdcEgu;

        public byte format { get; set; }

    }
}
