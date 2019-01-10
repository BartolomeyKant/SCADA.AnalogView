using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


using SCADA.Logging;
using SCADA.AnalogView.AnalogParametrs;

namespace SCADA.AnalogView
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //==========================================================
            // Создание объекта текущей конфигурации
            // Сделать чтение из конфигурационного файла
            ConfigurationWorker config = new ConfigurationWorker()
            {
                LogPrefixName = "SCADA.AnalogViewLog",
                LogDebugLevel = "All",
                LogFilePath = Directory.GetCurrentDirectory(),
                LogDaysStore = 1,

                ConnectionString = @"Server=.\SQLExpress;Database=asupt;Trusted_Connection=True;",
                ReadingTag = "TT702",

            };

            // инциализация логгера
            //===========================================================
            //TODO сделать чтение настроек из конфигурационного файла
            Logger.LogNamePrefix = config.LogPrefixName;
            switch(config.LogDebugLevel) 
            {
                case "All":
                    Logger.DebugLevel = DebugLevel.All;
                    break;
                case "Warning":
                    Logger.DebugLevel = DebugLevel.Warnings;
                    break;
                case "Error":
                    Logger.DebugLevel = DebugLevel.Errors;
                    break;
                default:
                    Logger.DebugLevel = DebugLevel.None;
                    break;
            }
           
            Logger.LoggerFilePath = config.LogFilePath;
            Logger.LogsDayCount = config.LogDaysStore;
            Logger.InitializeLogger();

            // ============ создание контроллера для аналогового параметра
            try
            {
                Logger.AddMessages("Создание объекта контроллера аналогового параметра");
                AnalogParamsController analogController = new AnalogParamsController(new AnalogServiceBuilder(config));
            }
            catch (Exception e)
            {
                Logger.AddError(e);
            }
        }
    }
}
