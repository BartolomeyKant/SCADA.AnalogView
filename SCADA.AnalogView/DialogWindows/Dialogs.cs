using System;
using System.Windows;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using SCADA.Logging;

namespace SCADA.AnalogView.DialogWindows
{

    public enum MessageType
    {
        Info,
        Question,
        JobDone,
        Warning,
        Error
    }

    public enum ResultDialog
    {
        None,
        TimeOut,
        No,
        Yes
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

    class Dialogs
    {

        public static Window ownerWindow { get; set; }
        static object locker = new object();                        // объект для синхронизации между разными потоками


        #region // Выдача информационных окон пользователю
        /// Информаицонные окна могут выдаваться из разных потоков
        /// поэтому требуется уделить внимание тому что бы они не отображались одновременно

        /// <summary>
        /// отображение простого сообщения
        /// </summary>
        /// <param name="Message">сообщение</param>
        public async static void ShowMessage(string Message)
        {
            try
            {
                await ShowMessageWindow(MessageType.Info, Message);
            }
            catch (Exception e)
            {
                Logger.AddError(new Exception("Не удалось открыть дилоговое окно", e));
            }
        }
        /// <summary>
        /// Отображение предупреждающего сообщения
        /// </summary>
        /// <param name="Message">сообщение</param>
        public async static void ShowWarning(string Message)
        {
            try
            {
                await ShowMessageWindow(MessageType.Warning, Message);
            }
            catch (Exception e)
            {
                Logger.AddError(new Exception("Не удалось открыть дилоговое окно", e));
            }
        }
        /// <summary>
        /// Отображение сообщения об ошибке
        /// </summary>
        /// <param name="Message">сообщение</param>
        public async static void ShowError(string Message)
        {
            try
            {
                await ShowMessageWindow(MessageType.Error, Message);
            }
            catch (Exception e)
            {
                Logger.AddError(new Exception("Не удалось открыть дилоговое окно", e));
            }
        }

        public async static void ShowException(UserMessageException exc)
        {
            // переописание сообщений
            string message = exc.Message;
            for (Exception e = exc.InnerException; e != null; e = e.InnerException)
            {
                message = message + "\n" + e.Message;
            }
            try
            {
                await ShowMessageWindow(exc.MessageType, message);
            }
            catch (Exception e)
            {
                Logger.AddError(new Exception("Не удалось открыть дилоговое окно", e));
            }
        }

        /// <summary>
        ///  Функция открывает информационное 
        /// </summary>
        /// <param name="messageType">тип сообщения</param>
        /// <param name="MessageText">текст сообщения</param>
        /// <returns>объект окна</returns>
        private async static Task<MessageWindow> ShowMessageWindow(MessageType messageType, string MessageText)
        {
            MessageWindow window = null;
            await Task.Run(() =>
            {
                lock (locker)
                {
                    if (!(ownerWindow is null))
                    {
                        ownerWindow.Dispatcher.Invoke(() =>
                        {
                            window = new MessageWindow() { MessageType = (byte)messageType, MessageText = MessageText };
                            window.Owner = ownerWindow;
                            window.ShowDialog();
                        });
                    }
                    else
                    {
                        throw new Exception($"Информационное окно, с текстом {MessageText}, не может быть открыто, система не проинициализирована");
                    }
                }
            });
            return window;
        }

        #endregion

        #region // Диологовые окна, для взаимодействия с пользователем
        // диалоговые окна следует вызывать только в одном потоке
        // слдовательно проблем с одновременным откртием быть не должно
        // с другой стороны диалоговое окно может перекрыться информационным окном

        /// <summary>
        /// Операция отображения диалогового окна пользователю
        /// </summary>
        /// <param name="Message"> Сообщение</param>
        /// <returns>Ответ пользователя</returns>
        public static ResultDialog ShowDialogMessage( string Message, MessageType messageType = MessageType.Question)
        {
            ResultDialog result = ResultDialog.None;
            try
            {
                DialogWindow window = new DialogWindow() { MessageType = (byte)MessageType.Question, MessageText = Message };
                if (!(ownerWindow is null))
                    window.Owner = ownerWindow;
                window.ShowDialog();
                result = window.Result;
            }
            catch (Exception e)
            {
                Logger.AddError(new Exception("Не удалось открыть дилоговое окно", e));
            }
            return result;
        }
        #endregion
    }
}
