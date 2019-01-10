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

        IPLCAnalogUstavki IAnalogServiceBuilder.PLCUstavki => throw new NotImplementedException();

        IPLCAnalogValueReader IAnalogServiceBuilder.PLCValueReader => throw new NotImplementedException();

        IPLCAnalogValueWriter IAnalogServiceBuilder.PLCValueWriter => throw new NotImplementedException();

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
            Logger.AddMessages($"Для чтения из базы данных задан тег - '{configuration.ReadingTag}'");
            dbWorker.SetDBTag(configuration.ReadingTag);

        }
    }
}
