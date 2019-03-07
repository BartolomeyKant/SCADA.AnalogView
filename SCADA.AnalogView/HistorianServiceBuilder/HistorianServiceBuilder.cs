
using SCADA.AnalogView.HistoriacalData.HistoricalInterfaces;
using SCADA.AnalogView.AnalogParametrs;

namespace SCADA.AnalogView
{
    /// <summary>
    /// класс реализует создание объектов для интерфейсов IHistGetData и IHistGetSubscribe
    /// </summary>
    class HistorianServiceBuilder :IHistoricalServiceBuilder
    {
        ConfigurationWorker configuration;

        HistorianWorker historian;                  // объект хисториана

        public HistorianServiceBuilder(ConfigurationWorker config, AnalogParamsController analog)
        {
            configuration = config;

            // получение актуального тега по индексу
            configuration.HistorianTagName = configuration.HistorianTagName.Replace(configuration.Indexator, "[" + analog.CommonParams.ControllerIndex + "]");
            
            historian = new HistorianWorker(configuration.HistorianTagName);
        }

        public IHistGetData GetData => historian;

        public IHistGetSubscribe GetSubscribe => historian;
    }
}
