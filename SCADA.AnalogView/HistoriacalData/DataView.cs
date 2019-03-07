using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SCADA.Logging;
using SCADA.AnalogView.HistoriacalData.HistoricalInterfaces;
using SCADA.AnalogView.AnalogParametrs;

namespace SCADA.AnalogView.HistoriacalData
{
    /// <summary>
    /// класс управляет отображением исторических данных на графике
    /// </summary>
    class DataView
    {

        IHistoricalView histView;           // объект для отображения на форме
        ConfigurationWorker configuration;
        AnalogParamsController _analogController;        // объект аналогового сигнала
        HistoricalData _data;                            // Объект представления исторических данных  

        Thread _updateTask;

        public DataView(IHistoricalView historicalView, ConfigurationWorker config, AnalogParamsController analog, HistoricalData data)
        {
            configuration = config;
            _analogController = analog;
            histView = historicalView;
            _data = data;

            try // инициализация поля графика
            {
                Logger.AddMessages("Выполняется инициализация объекта управления графиком");
                histView.InitChart(_analogController.Ustavki);
                Logger.AddMessages("Выполнена инициализация объекта управления графиком");
            }
            catch (Exception e)
            {
                histView = null;
                Logger.AddError(e);
                return;
            }

            InitUpdateChart();
        }

        // Функция периодического обновления данных на графике
        void InitUpdateChart()
        {
            Logger.AddMessages("Выполняется инициализация задачи циклического обновления графика");
            // функция обновления 
            _updateTask = new Thread(new ThreadStart(UpdateChart));
            _updateTask.Start();
            Logger.AddMessages("Выполнена инициализация задачи циклического обновления графика");
        }

        void UpdateChart()
        {
            try
            {
                while (true)
                {                // обновляем данные на таске
                    histView?.RefreshChart(_data.DataPoints, _analogController.Ustavki, configuration.MaxHistoricalTimeDuration);
                    // ожидаем время до следующего обновления
                    Thread.Sleep((int)configuration.ChartUpdateTime);
                }
            }
            catch (Exception e)
            {
                Logger.AddError(e);
            }

        }
    }
}
