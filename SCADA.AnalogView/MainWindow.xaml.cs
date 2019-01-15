using System;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;

using SCADA.Logging;
using SCADA.AnalogView.AnalogParametrs;

namespace SCADA.AnalogView
{

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        AnalogParamsController analogController = null;
        AnalogViewModel analogView = null;

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

                OPCServerName = "localhost/Schneider-Aut.OFS.2",

                Indexator = "[x]",                        // указатель индекса

                UstavkiTags = new string[] {
                "PLC!arrAIParam[x].Range.Bottom",      //0
                "PLC!arrAIParam[x].Range.Top",         //1
                "PLC!arrAIParam[x].Scale.Bottom",      //2
                "PLC!arrAIParam[x].Scale.Top",         //3
                "PLC!arrAIParam[x].ScaleExt.Bottom",   //4
                "PLC!arrAIParam[x].ScaleExt.Top",      //5
                "PLC!arrAIParam[x].Hist",              //6
                "PLC!arrAIParam[x].UstEnable",         //7
                "PLC!arrAIParam[x].UstMin[6]",         //8
                "PLC!arrAIParam[x].UstMin[5]",         //9
                "PLC!arrAIParam[x].UstMin[4]",         //10
                "PLC!arrAIParam[x].UstMin[3]",         //11
                "PLC!arrAIParam[x].UstMin[2]",         //12
                "PLC!arrAIParam[x].UstMin[1]",         //13
                "PLC!arrAIParam[x].UstMax[1]",         //14
                "PLC!arrAIParam[x].UstMax[2]",         //15
                "PLC!arrAIParam[x].UstMax[3]",         //16
                "PLC!arrAIParam[x].UstMax[4]",         //17
                "PLC!arrAIParam[x].UstMax[5]",         //18
                "PLC!arrAIParam[x].UstMax[6]",         //19
                "PLC!arrAIParam[x].IsChanged"          //20
                }
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

            Logger.AddMessages("Тестовое сообщение");
            Logger.AddWarning("Тестовое предупреждение");
            Logger.AddError(new Exception("Тестовая ошибка", new Exception("Продолжение")));

            // ============ создание контроллера для аналогового параметра
            try
            {
                Logger.AddMessages("Создание объекта контроллера аналогового параметра");
                analogController = new AnalogParamsController(config, new AnalogServiceBuilder(config));
            }
            catch (Exception e)
            {
                Logger.AddError(e);
            }

            // ============== создание модели представления аналогового сигнала =================
            try
            {
                Logger.AddMessages("Создание объекта визуального предсталвения аналогового параметра");
                analogView = new AnalogViewModel(analogController);
                InitializeUstavki();
            }
            catch (Exception e)
            {
                Logger.AddError(e);
            }

        }

        void InitializeUstavki()
        {
            // установка контекста данных для элементов уставок
            UstavkiPanel.ItemsSource = analogView.UstContainer.UstValues;
            ADCMax.DataContext = analogView.UstContainer.ADCMax;
            ADCMin.DataContext = analogView.UstContainer.ADCMin;
            EMax.DataContext = analogView.UstContainer.EMax;   
            EMin.DataContext = analogView.UstContainer.EMin;
            VPD.DataContext = analogView.UstContainer.VPD;
            NPD.DataContext = analogView.UstContainer.NPD;
            Hister.DataContext = analogView.UstContainer.Hister;
        }

        void UstavkiFieldKewDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tb = (TextBox)sender;
                UstValue ust = ((UstViewModel)((Grid)tb.Parent).DataContext).Controller;            //получение объекта уставки через дата контекст родительского грида
                analogView.SetNewUstValue(tb.Text, ust);
            }
        }

        void WriteUstavki(object o, EventArgs e)
        {
            analogView.WriteUsts();
        }


        // после загрузки формы
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MessageWindow.ownerWindow = this;

            MessageWindow.ShowMessage("Сообщение 1");
            MessageWindow.ShowWarning("Предупреждение");
            MessageWindow.ShowError("Ошибка");
            MessageWindow.ShowException(new UserMessageException("Ехал грека черех реку",
                new Exception("Видит грека в реке рак",
                new Exception("Сунул грека в реку руку",
                new Exception("Рак за руку грека цап!"))), MessageType.JobDone));
        }
    }
}
