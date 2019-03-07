using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.AnalogView
{
    // класс - контейнер для конфигураций
    class ConfigurationWorker
    {

        public string LogPrefixName{get; set;}
        public string LogDebugLevel { get; set; }
        public string LogFilePath { get; set; }
        public int LogDaysStore { get; set; }

        public string ConnectionString { get; set; }
        public string ReadingTag { get; set; }

        public string ADCTag { get; set; }
        public string ValueTag { get; set; }
        public string AnalogStateTag { get; set; }

        public string OPCServerName { get; set; }
        public string Indexator { get; set; }
        public string[] UstavkiTags { get; set; }

        public string CMDIndexTag { get; set; }
        public string CMDCmdTag { get; set; }
        public string CMDValueTag { get; set; }

        public uint MaxHistoricalPoints { get; set; }
        public uint MaxHistoricalTimeDuration { get; set; }
        public uint HistoricalUpdateTime { get; set; }
        public string HistorianTagName { get; set; }

        public uint ChartUpdateTime { get; set; }
    }
}
