using System;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;

using SCADA.Logging;
using SCADA.AnalogView.AnalogParametrs;
using SCADA.AnalogView.DialogWindows;
using SCADA.AnalogView.HistoriacalData;

namespace SCADA.AnalogView
{

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        AnalogParamsController analogController = null;
        AnalogViewModel analogView = null;

        HistoricalData histData = null;
        DataView histView = null;

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
                },

                ADCTag = "PLC!arElInput[x]",                // код АЦП
                ValueTag = "PLC!arAIValue[x]",              // тег значения
                AnalogStateTag = "PLC!awAIKCReg[x]",         // тег состояния

                CMDIndexTag = "PLC!arrCmdAI[1].id",         // команда - индекс сигнала
                CMDCmdTag = "PLC!arrCmdAI[1].cmd",           // команад - команда
                CMDValueTag = "PLC!arrCmdAI[1].value",          // команда - значение

                MaxHistoricalPoints = 2000,                    // максимальное количество точек
                MaxHistoricalTimeDuration = 600,               // максимальный промеэуток времени cек
                HistoricalUpdateTime = 500,                    // время обновления тегов при подписке мсек
                HistorianTagName = "Fix.PLC!arAIValue[x]",      // тег в хисториан
                ChartUpdateTime = 1000                          // время обновления чарта
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
                analogController = new AnalogParamsController(config, new AnalogServiceBuilder(config));
            }
            catch (Exception e)
            {
                Logger.AddError(e);
                return;
            }

            // ============== создание модели представления аналогового сигнала =================
            try
            {
                Logger.AddMessages("Создание объекта визуального предсталвения аналогового параметра");
                analogView = new AnalogViewModel(analogController);
                InitializeUstavki();
                InitializeValueGrid();
            }
            catch (Exception e)
            {
                Logger.AddError(e);
                return;
            }

            // ================ создание объекта для подключения к историческим данным
            try
            {
                histData = new HistoricalData(new HistorianServiceBuilder(config, analogController), config);
            }
            catch (Exception e)
            {
                Logger.AddError(e);
                return;
            }

            // =============== создание объектов для отображения исторических данных
            try
            {
                histView = new DataView(new IDDOperativeChart(HPlotter), config, analogController, histData);
            }
            catch (Exception e)
            {
                Logger.AddError(e);
                return;
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

        void InitializeValueGrid()
        {
            ValueGrid.DataContext = analogView.ValueViewModel;
            ENGValueGrid.DataContext = analogView.ValueViewModel.EngValue;
            ADCValueGrid.DataContext = analogView.ValueViewModel.ADCValue;
            PLCValueGrid.DataContext = analogView.ValueViewModel.PLCValue;
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
            Dialogs.ownerWindow = this;
        }

        // переключить имитацию
        private void ImitBtn_Click(object sender, RoutedEventArgs e)
        {
            analogView.TogleImit();
        }
        // задать ноовое значение имитации
        private void ImitValueKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tb = (TextBox)sender;
                analogView.ChangeImitValue(tb.Text);
            }
        }
    }
}
