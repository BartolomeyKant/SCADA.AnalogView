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

            // инциализация логгера
            //===========================================================
            //TODO сделать чтение настроек из конфигурационного файла
            Logger.LogNamePrefix = "SCADA.AnalogViewLog";
            Logger.DebugLevel = DebugLevel.All;
            Logger.LoggerFilePath = Directory.GetCurrentDirectory();
            Logger.LogsDayCount = 1;
            Logger.InitializeLogger();
            //=========================================================
            Logger.AddMessages("Привет мир");
            Logger.AddWarning("Задолбал насморк");
            Logger.AddError(new Exception("Пойду выпилюсь", new Exception("Вот и все!!!")));
        }
    }
}
