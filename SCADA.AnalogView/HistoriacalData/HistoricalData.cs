using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SCADA.Logging;
using SCADA.AnalogView.HistoriacalData.HistoricalInterfaces;

namespace SCADA.AnalogView.HistoriacalData
{
    public delegate void HistoricalSetChanged();
    /// <summary>
    /// Класс опеределяет работу с историческими данными
    /// Заполнение и предоставление массива точек исторических данных
    /// Контроль количества точек данных
    /// </summary>
    class HistoricalData
    {
        List<HistoricalDataPoint> dataPoints;
        /// <summary>
        /// Набор исторических данных, если набор пустой, то возвращается пустой список
        /// </summary>
        public List<HistoricalDataPoint> DataPoints
        {
            get
            {
                if (dataPoints == null)
                {
                    return new List<HistoricalDataPoint>();
                }
                else
                {
                    return dataPoints;
                }
            }
        }

        ConfigurationWorker configuration;          // объект текущей конфигурации
        IHistGetData getData;                       // объект получения набора исторических данных
        IHistGetSubscribe dataSubscriber;           // объект предоставляет подписку на изменение исторических данных

        public HistoricalData(IHistoricalServiceBuilder serviceBuilder, ConfigurationWorker configuration)
        {
            Logger.AddMessages("Инициализация объекта работы с историческими данными");
            this.configuration = configuration;
            getData = serviceBuilder.GetData;
            dataSubscriber = serviceBuilder.GetSubscribe;

            InitDataPoints();
        }

        /// <summary>
        /// событие изменение набора исторических данных
        /// </summary>
        public event HistoricalSetChanged OnHistoricalSetChangeEvent;
        void OnHistoricalSetChange()
        {
            OnHistoricalSetChangeEvent?.Invoke();
        }

        // получение набора исторических данных
        async void  InitDataPoints()
        {
            dataPoints = new List<HistoricalDataPoint>((int)configuration.MaxHistoricalPoints);
            try
            {
                await Task.Run(() =>
                {
                    // получение исторических данных
                    Logger.AddMessages("Выполняется чтение исторических данных");
                    dataPoints.AddRange(getData.GetHistoricalData(configuration.MaxHistoricalTimeDuration, configuration.MaxHistoricalPoints));
                    // выполнение подписки на изменяемое значение
                    Logger.AddMessages("Выполняется подписка на изменения исторических данных");
                    dataSubscriber.GetSubscribe(HistDataChange, configuration.HistoricalUpdateTime);
                });
            }
            catch (Exception e)
            {
                Logger.AddError(e);
            }
        }

        void HistDataChange(HistoricalDataPoint[] points)
        {
            // добавляем новые точки в общий массив
            dataPoints.AddRange(points);
            // удаляем лишний точки
            if (dataPoints.Count > configuration.MaxHistoricalPoints)
            {
                dataPoints.RemoveRange(0, (int)(dataPoints.Count - configuration.MaxHistoricalPoints));
            }
        }

    }
}
