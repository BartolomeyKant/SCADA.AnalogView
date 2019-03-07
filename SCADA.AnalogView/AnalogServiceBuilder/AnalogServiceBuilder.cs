using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCADA.AnalogView.AnalogParametrs.AnalogInterfaces;
using SCADA.Logging;

namespace SCADA.AnalogView
{
    /// <summary>
    ///  Класс построитель сервисов для контроллера аналогового параметра
    /// </summary>
    class AnalogServiceBuilder : IAnalogServiceBuilder
    {
        IDBWorker dbWorker;
        IDBWorker IAnalogServiceBuilder.DBWorker => dbWorker;

        OPCDAWorker opcDAWorker;
        IPLCAnalogUstavki IAnalogServiceBuilder.PLCUstavki => opcDAWorker;

        IPLCAnalogValue IAnalogServiceBuilder.PLCValue => opcDAWorker;

        public AnalogServiceBuilder(ConfigurationWorker configuration)
        {
            try
            {
                Logger.AddMessages("Создание объекта для работы с базой данных");
                dbWorker = new DBWorker(configuration.ConnectionString);                // Создание нового воркера
            }
            catch (Exception e)
            {
                Logger.AddError(e);
                return;
            }

            try
            {
                Logger.AddMessages("Создание объекта для работы с OPC DA сервером");
                opcDAWorker = new OPCDAWorker(configuration.OPCServerName);
            }
            catch (Exception e)
            {
                Logger.AddError(e);
                return;
            }
        }
    }
}
