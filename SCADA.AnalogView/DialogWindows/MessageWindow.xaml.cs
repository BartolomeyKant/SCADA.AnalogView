﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;

using SCADA.Logging;

namespace SCADA.AnalogView.DialogWindows
{
    /// <summary>
    /// Логика взаимодействия для MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window, INotifyPropertyChanged
    {
        // тип сообщения 
        public byte MessageType { get; set; }
        public string MessageText { get; set; }

        public ResultDialog Result;

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Logger.AddMessages("Пользователь закрыл диалоговое окно");
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Logger.AddMessages($"Отображение окна с сообщением {MessageText} типа - {MessageType}");

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
                        Result = ResultDialog.TimeOut;
                        this.Dispatcher.Invoke(()=> { this.Close(); });
                        return;
                    }
                }
            });
        }

        /// <summary>
        ///  при нажатии Enter делаем выход
        /// </summary>
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Close();
            }
        }
    }
}
