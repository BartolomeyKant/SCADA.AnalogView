using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.AnalogView.HistoriacalData
{
    /// <summary>
    ///  представляет значения качества данных
    /// </summary>
    public enum HistDataQuality
    {
        Bad,
        Good
    }
    /// <summary>
    /// класс представляет историческое значение
    /// хранит, отображаемое значение
    /// качество сигнала
    /// времеенную метку
    /// </summary>
    class HistoricalDataPoint
    {
        /// <summary>
        /// отображаемое значение
        /// </summary>
        public float Value;
        /// <summary>
        /// метка времени
        /// </summary>
        public DateTime TimeStamp;
        /// <summary>
        /// качество сигнала
        /// </summary>
        public HistDataQuality Quality;

        public HistoricalDataPoint(float value, DateTime time, HistDataQuality quality)
        {
            Value = value;
            TimeStamp = time;
            Quality = quality;
        }
    }
}
