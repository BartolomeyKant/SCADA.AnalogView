

namespace SCADA.AnalogView.HistoriacalData.HistoricalInterfaces
{
    /// <summary>
    /// делегат обработки события изменения набора исторических значений 
    /// </summary>
    /// <param name="points">points - массив новых значений</param>
    delegate void HistoricalDataChange(HistoricalDataPoint[] points);
    interface IHistGetSubscribe
    {
        void GetSubscribe(HistoricalDataChange datachangeHandler, uint UpdateRate);          // реализуется подписка на изменение истоических значений
    }
}
