

namespace SCADA.AnalogView.HistoriacalData.HistoricalInterfaces
{
    interface IHistGetData
    {
        /// <summary>
        /// Получение массива исторических значений
        /// Тег для которого читается значение, определяется в объекте, который реализует интерфейс
        /// </summary>
        /// <param name="MaxTimeDuration">Максимальный период времени, для которого вычитываются значения</param>
        /// <param name="MaxPointCount">Максимальное количество точек</param>
        /// <returns>Массив точек исторических данных</returns>
        HistoricalDataPoint[] GetHistoricalData(uint MaxTimeDuration, uint MaxPointCount);
    }
}
