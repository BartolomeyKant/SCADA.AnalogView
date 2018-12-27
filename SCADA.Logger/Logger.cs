using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.Logging
{
    /// <summary>
    /// Уровни логгирования
    /// </summary>
    public enum DebugLevel
    {
        None,
        All,                            // печатать все сообщения
        Warnings,                       // печатать только ошибки и предупреждения
        Errors                          // печатать только ошибки
    }
    public class Logger
    {
        /// <summary>
        ///  префикс имени для файла логов. окончание имени будет дата
        /// </summary>
        public static string LogNamePrefix;
        /// <summary>
        /// Уровень логгирования
        /// </summary>
        public static DebugLevel DebugLevel = DebugLevel.All;
        /// <summary>
        /// Дирректория для хранения файлов логов
        /// </summary>
        public static string LoggerFilePath;

        /// <summary>
        /// Время хранения логов в днях
        /// </summary>
        public static int LogsDayCount;

        static Queue<Exception> messages;            // просто сообщения
        static Queue<Exception> warnings;           // предупреждения
        static Queue<Exception> errors;             // ошибки

        static FileInfo logFile;
        static StreamWriter writter;

        /// <summary>
        /// Инициализация логгера
        /// </summary>
        public static void InitializeLogger()
        {
            if (LoggerFilePath == "")
                throw new Exception("Ошибка инициализации логгера. Не задан путь для хранения фалов логов");
            if (LogsDayCount <= 0)
                throw new Exception("Ошибка инициализации логгера. Не правильно указано количество дней для хранения логов, значение должно быть больше 0");
            if (LogNamePrefix == "")
                throw new Exception("Ошибка инициализации логгера. Не правильно задан префикс имени фалов логов");

            DirectoryInfo dir = new DirectoryInfo(LoggerFilePath);
            if (!dir.Exists)
                throw new Exception("Ошибка инициализации логгера. Дирректория для хранения логов не существует");

            FileInfo[] files = dir.GetFiles(LogNamePrefix + "*");
            DateTime nowTime = DateTime.Now;
            // удаление файлов старше указанного времени
            foreach (FileInfo file in files)
            {

                if (nowTime.Subtract(file.CreationTime).Days >= LogsDayCount)
                {
                    file.Delete();
                }
            }
            // созданеи файла для логов
            logFile = new FileInfo(LoggerFilePath + "\\" + LogNamePrefix + "_" + nowTime.ToString("yyyy_MM_dd") + ".log");
            if (!logFile.Exists)
            {
                logFile.Create();
            }

            // инциализация очередей для разных видов сообщений
            switch (DebugLevel)
            {
                case DebugLevel.All:
                    messages = new Queue<Exception>();
                    goto case DebugLevel.Warnings;
                case DebugLevel.Warnings:
                    warnings = new Queue<Exception>();
                    goto case DebugLevel.Errors;
                case DebugLevel.Errors:
                    errors = new Queue<Exception>();
                    break;
            }

            Task.Factory.StartNew(PrintLogs);


        }
        /// <summary>
        /// добавление нового сообщения для печати логов
        /// </summary>
        /// <param name="message">сообщение в лог</param>
        public static void AddMessages(string message)
        {
            if (DebugLevel == DebugLevel.All)
            {
                Exception exc = new Exception(message);
                messages.Enqueue(exc);
            }
        }

        /// <summary>
        /// добавление нового предупреждающего сообщения для печати логов
        /// </summary>
        /// <param name="message">сообщение в лог</param>
        public static void AddWarning(string message)
        {
            if (DebugLevel == DebugLevel.All || DebugLevel == DebugLevel.Warnings)
            {
                Exception exc = new Exception(message);
                warnings.Enqueue(exc);
            }
        }
        /// <summary>
        /// доьбавление нового предупреждающего сообщения для печати логов
        /// </summary>
        /// <param name="exception">исключение для передачи в лог</param>
        public static void AddWarning(Exception exception)
        {
            if (DebugLevel == DebugLevel.All || DebugLevel == DebugLevel.Warnings)
            {
                warnings.Enqueue(exception);
            }
        }
        /// <summary>
        /// добавление нового сообщения ошибки для печати в лог
        /// </summary>
        /// <param name="exception">исключение для печати в лог</param>
        public static void AddError(Exception exception)
        {
            if (DebugLevel == DebugLevel.All || DebugLevel == DebugLevel.Warnings || DebugLevel == DebugLevel.Errors)
            {
                errors.Enqueue(exception);
            }
        }

        /// <summary>
        /// функция печати сообщений из очередей
        /// </summary>
        async static void PrintLogs()
        {
            while (true)
            {
                try
                {
                    writter = logFile.AppendText();
                }
                catch (IOException e)               // если ошибка ввода-вывод, то пробем еще раз
                {
                    await Task.Delay(100);
                    continue;
                }
                catch (Exception e)                 // если ошибка другая, то пробуем пробросить наверх
                {
                    throw e;
                }
                //получение сообщений
                while (messages != null && messages.Count != 0)
                {
                    printExsceptionToFile(messages.Dequeue());
                }

                // получение предупреждений
                while (warnings != null && warnings.Count != 0)
                {
                    printExsceptionToFile(warnings.Dequeue());
                }

                // получение ошибок
                while (errors != null && errors.Count != 0)
                {
                    printExsceptionToFile(errors.Dequeue());
                }
                writter.Close();            // закрываем поток, чтобы другие могли открыть
                await Task.Delay(500);
            }
        }
        // печать сообщений в файл логов
        static void printExsceptionToFile(Exception exc)
        {
            string mess = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.ffff") + " - " + exc.Message;
            writter.Write(mess);
            if (exc.InnerException != null)
            {
                writter.Write(" -->\n\t |______ ");
                printExsceptionToFile(exc.InnerException);
            }
            writter.Write("\n");
            writter.Flush();
        }
    }
}
