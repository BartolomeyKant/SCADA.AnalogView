using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;

using SCADA.Logging;

namespace SCADA.AnalogView
{
    public enum MessageType
    {
        Info,
        Question,
        JobDone,
        Warning,
        Error
    }
    /// <summary>
    /// Специальный тип исключения для выдачи сообщений пользователю из контроллера
    /// </summary>
   public class UserMessageException : Exception
    {
        
        public UserMessageException(string message, MessageType type = MessageType.Info) : base(message)
        {
            MessageType = type;
        }
        public UserMessageException(string message, Exception innerException, MessageType type = MessageType.Info) : base(message, innerException)
        {
            MessageType = type;
        }

        public MessageType MessageType;
    }

    /// <summary>
    /// Логика взаимодействия для MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window, INotifyPropertyChanged
    {
        // тип сообщения 
        public byte MessageType { get; set; }
        public string MessageText { get; set; }
        public static Window ownerWindow { get; set; }

        string toCloseMessage;
        public string ToCloseMessage
        {
            set
            {
                toCloseMessage = value;
                OnPropertyChanged("ToCloseMessage");
            }
            get
            { return toCloseMessage; }
        }

        const int timeToClose = 30;
         int timer;

        Task timerTask;

        // реализация интерфейса INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public MessageWindow()
        {
            InitializeComponent();
            ContentGrid.DataContext = this;
            TopPanel.DataContext = this;
        }

        /// <summary>
        /// отображение простого сообщения
        /// </summary>
        /// <param name="Message">сообщение</param>
        public static void ShowMessage(string Message)
        {
            Window window = new MessageWindow() { MessageType = 0, MessageText = Message };
            if (!(ownerWindow is null))
                window.Owner = ownerWindow;
            window.ShowDialog();
        }
        /// <summary>
        /// Отображение предупреждающего сообщения
        /// </summary>
        /// <param name="Message">сообщение</param>
        public static void ShowWarning(string Message)
        {
            Window window = new MessageWindow() { MessageType = 3, MessageText = Message };
            if (!(ownerWindow is null))
                window.Owner = ownerWindow;
            window.ShowDialog();
        }
        /// <summary>
        /// Отображение сообщения об ошибке
        /// </summary>
        /// <param name="Message">сообщение</param>
        public static void ShowError(string Message)
        {
            Window window = new MessageWindow() { MessageType = 4, MessageText = Message };
            if (!(ownerWindow is null))
                window.Owner = ownerWindow;
            window.ShowDialog();
        }

        public static void ShowException(UserMessageException exc)
        {
            // переописание сообщений
            string message = exc.Message;
            for (Exception e = exc.InnerException; e != null; e = e.InnerException)
            {
                message = message + "\n" + e.Message;
            }
            Window window = new MessageWindow() { MessageType = (byte)exc.MessageType, MessageText = message };
            if (!(ownerWindow is null))
                window.Owner = ownerWindow;
            window.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Logger.AddMessages("Пользователь закрыл диалогово окно");
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Logger.AddMessages($"Отображение окна с сообзщением {MessageText} типа - {MessageType}");
            timerTask = Task.Run(() =>
            {
                timer = timeToClose;
                while (true)
                {
                    ToCloseMessage = $"До закрытия окна осталось {timer} сек.";
                    Thread.Sleep(1000);
                    timer--;
                    if (timer == 0) // если таймер досчитал, то закрываем окно
                    {
                        this.Dispatcher.Invoke(()=> { this.Close(); });
                        return;
                    }
                }
            });
        }
    }
}
