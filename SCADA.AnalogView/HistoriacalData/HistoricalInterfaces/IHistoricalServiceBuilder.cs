

namespace SCADA.AnalogView.HistoriacalData.HistoricalInterfaces
{
    /// <summary>
    /// билдер для объектов предоставления исторических данных
    /// </summary>
    interface IHistoricalServiceBuilder
    {
        IHistGetData GetData { get; }
        IHistGetSubscribe GetSubscribe { get; }
    }
}
