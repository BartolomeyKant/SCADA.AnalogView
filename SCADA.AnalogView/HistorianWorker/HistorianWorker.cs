using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SCADA.AnalogView.DialogWindows;
using Proficy.Historian.ClientAccess.API;
using SCADA.AnalogView.HistoriacalData;
using SCADA.AnalogView.HistoriacalData.HistoricalInterfaces;
using SCADA.Logging;
namespace SCADA.AnalogView
{
    class HistorianWorker : IHistGetData, IHistGetSubscribe
    {
        ServerConnection server;        // объект historian сервера

        string TagName;         // имя тега для которого выполянется чтение
        HistoricalDataChange DataChangeHandler;     // делегат для обновления данных


        /// <summary>
        /// Создание объекта для работы с сервером Historian
        /// </summary>
        /// <param name="tagName">тег для чтения</param>
        /// <param name="updateTime">время обновления при подписке на изменения</param>
        public HistorianWorker( string tagName)
        {
            TagName = tagName;
            try
            {
                Logger.AddMessages("Подключение к локалькому хисториан серверу");
                // подключение к локальному Historian Server
                server = new ServerConnection(new ConnectionProperties { ServerCertificateValidationMode = CertificateValidationMode.None });
            }
            catch (Exception e)
            {
                Logger.AddError(e);
            }
        }

        /// <summary>
        ///  получение набора исторических данных
        /// </summary>
        /// <param name="MaxTimeDuration"></param>
        /// <param name="MaxPointCount"></param>
        /// <returns></returns>
        public HistoricalDataPoint[] GetHistoricalData(uint MaxTimeDuration, uint MaxPointCount)
        {
            try
            {           //попытка подключения к серверу
                if (!server.IsConnected())
                    server.Connect();
            }
            catch (Exception e)
            {
                Exception err = new Exception("При попытке подключения к серверу Historian возникло исключение", e);
                throw err;
            }

            HistoricalDataPoint[] points = new HistoricalDataPoint[MaxPointCount];

            // Формируем запрос на чтение данных
            DataQueryParams query = new RawByTimeQuery(DateTime.Now.AddSeconds(-1 * MaxTimeDuration), TagName);
            ItemErrors errors;
            DataSet dataSet;

            try
            {
                server.IData.Query(ref query, out dataSet, out errors);
            }
            catch (Exception e)
            {
                Exception err = new Exception("При попытке чтения исторических данных из Historian возникло исключение ", e);
                throw err;
            }

            if (errors.Count > 0)
            {
                foreach (object sts in errors)
                {
                    Logger.AddError(new Exception($"При считывании исторических данных для тега {TagName} были получены ошибки {sts}"));
                }
                throw new UserMessageException("Ошибка чтения исторических данных из сервера Historian!", MessageType.Error);
            }

            try
            {
                // переописания набора данных в массив точек исторических значений
                for (int i = 0; i < dataSet[TagName].Count(); i++)
                {
                    if (i < MaxPointCount)          //берем не больше максимального числа точек
                    {
                        points[i] = new HistoricalDataPoint((float)dataSet[TagName].GetValue(i), dataSet[TagName].GetTime(i),
                                                            (dataSet[TagName].GetQuality(i).IsGood()) ? HistDataQuality.Good : HistDataQuality.Bad);
                    }
                }
            }
            catch (Exception e)
            {
                Exception err = new Exception("При переописании значений исторических данных возникло исключение ", e);
                throw err;
            }
            // возвращаем массив точек прочитанных значений
            return points;
        }

        public void GetSubscribe(HistoricalDataChange datachangeHandler, uint UpdateRate)
        {
            try
            {           //попытка подключения к серверу
                if (!server.IsConnected())
                    server.Connect();
            }
            catch (Exception e)
            {
                Exception err = new Exception("При попытке подключения к серверу Historian возникло исключение", e);
                throw err;
            }
            try
            {
                DataChangeHandler = datachangeHandler;
                server.DataChangedEvent += Server_DataChangedEvent;
                server.IData.Subscribe(new DataSubscriptionInfo() { Tagname = TagName, MinimumElapsedMilliSeconds = (int)UpdateRate });
            }
            catch (Exception e)
            {
                Exception err = new Exception("При выполнении подписки на обновления значений исторических данных возникло исключение", e);
            }
        }

        private void Server_DataChangedEvent(List<CurrentValue> values)
        {
            HistoricalDataPoint[] points = new HistoricalDataPoint[values.Count];
            // переописание новых значений в массив
             for (int i = 0; i < values.Count; i++)
            {
                points[i] = new HistoricalDataPoint((float)values[i].Value, values[i].Time,
                                                    values[i].Quality.IsGood ? HistDataQuality.Good : HistDataQuality.Bad);
            }
            DataChangeHandler?.Invoke(points);      // вызов внешнго метода для обновления данных
        }
    }
}
