using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Opc;
using Opc.Da;
using OpcCom;
using SCADA.Logging;

using SCADA.AnalogView.DialogWindows;
using SCADA.AnalogView.AnalogParametrs;
using SCADA.AnalogView.AnalogParametrs.AnalogInterfaces;

namespace SCADA.AnalogView
{
    /*
     ===========================================================
     В классе реализуются методы чтения / записи данных через OPCDA
         */
    class OPCDAWorker : IPLCAnalogUstavki, IPLCAnalogValue
    {
        string[] ustvakiTags;
        string[] cmdTags;
        string[] valueTags;

        Opc.Da.Server server;

        Item[] ustavkiItems;
        Item[] valueItems;
        Item[] cmdItems;

        SubscriptionState grState;
        Subscription grReadAnalogValue;

        AnalogValue subscribeValue;

        /// <summary>
        /// Создание объекта для работы с OPC DA serverom
        /// </summary>
        /// <param name="opcConnectionString"></param>
        public OPCDAWorker(string opcConnectionString)
        {
            try
            {
                server = new Opc.Da.Server(new OpcCom.Factory(), new URL("opcda://" + opcConnectionString));
                Logger.AddMessages($"Создан объект OPC сервера с URL {server.Url}");
            }
            catch (Exception e)
            {
                throw new Exception($"При создании OPC DA servera со строкой подключения {opcConnectionString} возникло исключение", e);
            }
        }


        #region Работа с уставками контроллера
        /// <summary>
        /// задание тегов для работы с уставками
        /// </summary>
        /// <param name="tags"></param>
        public void SetUstavkiTags(string[] tags)
        {
            ustvakiTags = tags;
            ustavkiItems = new Item[ustvakiTags.Length];
            for (int i = 0; i < ustvakiTags.Length; i++)
            {
                ustavkiItems[i] = new Item() { ItemName = ustvakiTags[i] };
            }
            Logger.AddMessages($"В OPC севрер добавлено {ustavkiItems.Length} тегов для работы с уставками");
        }

        /// <summary>
        /// Чтение устовок из контроллера
        /// </summary>
        /// <param name="ustavkiContainer"></param>
        public void GetPLCUstavki(ref UstavkiContainer ustavkiContainer)
        {
            try                 //Подключение к OPC серверу
            {
                Logger.AddMessages($"Подключние к OPC серверу c URL - '{server.Url}'");
                if (!server.IsConnected)
                    server.Connect();
            }
            catch (Exception e)
            {
                throw new Exception($"При попытке покдлючения к OPC DA серверу {server.Url} возникло исключение", e);
            }
            ItemValueResult[] result = server.Read(ustavkiItems);
            // определение успешности операции
            bool flIsSuccess = true;
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i].Quality != Quality.Good)
                {
                    flIsSuccess = false;
                    Logger.AddWarning($"Для тега {result[i].ItemName} не прочитано значение с качеством {result[i].Quality} и ошибкой {result[i].ResultID}");
                }
            }
            if (!flIsSuccess)
                throw new UserMessageException("Считанные уставки из контроллера не достоверны", MessageType.Warning);
            try
            {
                // Переопсиание считанных параметров из контроллера в массив хранения уставок
                // Для успешного переописания, порядок тегов заданных для чтения должен быть строго заданным
                ustavkiContainer.EMin.Value = (float)result[0].Value;
                ustavkiContainer.EMax.Value = (float)result[1].Value;
                ustavkiContainer.ADCMin.Value = (float)result[2].Value;
                ustavkiContainer.ADCMax.Value = (float)result[3].Value;
                ustavkiContainer.NPD.Value = (float)result[4].Value;
                ustavkiContainer.VPD.Value = (float)result[5].Value;
                ustavkiContainer.Hister.Value = (float)result[6].Value;
                uint UstEnable = (ushort)result[7].Value;
                for (int i = 0; i < 12; i++)
                {
                    ustavkiContainer.UstValues[i].Value = (float)result[8 + i].Value;
                    ustavkiContainer.UstValues[i].Used = (UstEnable & (1 << i)) > 0;
                }
            }
            catch (Exception e)
            {
                throw new Exception("При переописание уставок из контроллера возникло исключение", e);
            }
        }

        /// <summary>
        /// Запись уставок в контроллер
        /// </summary>
        /// <param name="ustavkiContainer"></param>
        public void SetPLCUstavki(UstavkiContainer ustavkiContainer)
        {
            try                 //Подключение к OPC серверу
            {
                Logger.AddMessages($"Подключние к OPC серверу c URL - '{server.Url}'");
                if (!server.IsConnected)
                    server.Connect();
            }
            catch (Exception e)
            {
                throw new Exception($"При попытке покдлючения к OPC DA серверу {server.Url} возникло исключение", e);
            }
            Logger.AddMessages("Выполняется запись уставок в OPC сервер");
            // Создаем набор тегов для записи в OPC сервер
            ItemValue[] values = new ItemValue[ustavkiItems.Length];
            for (int i = 0; i < ustavkiItems.Length; i++)
            {
                values[i] = new ItemValue(ustavkiItems[i]);
            }
            // Общие уставки
            values[0].Value = ustavkiContainer.EMin.Value;
            values[1].Value = ustavkiContainer.EMax.Value;
            values[2].Value = ustavkiContainer.ADCMin.Value;
            values[3].Value = ustavkiContainer.ADCMax.Value;
            values[4].Value = ustavkiContainer.NPD.Value;
            values[5].Value = ustavkiContainer.VPD.Value;
            values[6].Value = ustavkiContainer.Hister.Value;

            uint ustEnable = 32768;
            // Технологичнские уставки
            for (int i = 0; i < 12; i++)
            {
                values[8 + i].Value = ustavkiContainer.UstValues[i].Value;
                ustEnable = ustEnable + (uint)((ustavkiContainer.UstValues[i].Used ? 1 : 0) << i);
            }
            values[7].Value = ustEnable;

            values[20].Value = 1;           // Команда записи уставок с ВУ в СУ

            try
            {
                bool flSuccess = true;
                IdentifiedResult[] result = server.Write(values);
                for (int i = 0; i < result.Length; i++)
                {
                    if (result[i].ResultID != ResultID.S_OK)
                    {
                        flSuccess = false;
                        Logger.AddWarning($"Для уставки с тегом {result[i].ItemName} значение записано с ошибкой {result[i].ResultID}");
                    }
                }
                if (!flSuccess)
                {
                    throw new UserMessageException("Уставки не записанны в OPC сервер", MessageType.Error);
                }
                Logger.AddMessages("Выполнена запись уставок в OPC севрер");
            }
            catch (UserMessageException exc)
            { throw exc; }
            catch (Exception e)
            {
                throw new Exception($"При записи уставок в OPC сервер {server.Url} возникло исключение", e);
            }
        }

        #endregion

        #region Работа с текущими значениями контроллера
        /// <summary>
        /// настройка тегов аналогового сигнала 
        /// </summary>
        /// <param name="ADCTag">тег кода АЦП</param>
        /// <param name="PLCValueTag">тег текущего значения аналогового сигнала</param>
        /// <param name="AnalogState">тег состояния аналогового сигнала</param>
        public void SetAnalogTags(string ADCTag, string PLCValueTag, string AnalogState)
        {
            // теги добавляются строго в заданном порядке
            valueTags = new string[3];
            valueTags[0] = ADCTag;
            valueTags[1] = PLCValueTag;
            valueTags[2] = AnalogState;

            // формирование набора итемов
            valueItems = new Item[3];
            for (int i = 0; i < 3; i++)
            {
                valueItems[i] = new Item() { ItemName = valueTags[i] };
            }

        }

        public void GetCurrentAnalogValue(ref AnalogValue value)
        {
            try
            {
                Logger.AddMessages($"Попытка подключения к opc da серуеру {server.Url}");
                if (!server.IsConnected)
                    server.Connect();
            }
            catch (Exception e)
            {
                throw new Exception($"Ошибка подключения к OPC DA серверу {server.Url} для считывания значений аналогового сигнала", e);
            }

            ItemValueResult[] result = server.Read(valueItems);         // чтение значений

            // проверка качества значений
            bool flSuccess = true;
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i].Quality != Quality.Good)
                {
                    flSuccess = false;
                    Logger.AddWarning($"Для тега {result[i].ItemName} не прочитано значение с качеством {result[i].Quality} и ошибкой {result[i].ResultID}");
                }
            }
            if (!flSuccess)
                throw new UserMessageException("Считанные данные из контроллера недостоверны", MessageType.Warning);

            // переописание значений
            try
            {
                value.ADCValue = (float)result[0].Value;        //  код АЦП
                value.PLCValue = (float)result[1].Value;        // текущее значение
                value.AnalogState = (ushort)result[2].Value;        // состояние сигнала
            }
            catch (Exception e)
            {
                throw new Exception($"При переописании значения аналогового сигнала возникло исключение", e);
            }
        }


        public void SubscribeToChangeAnalogValue(ref AnalogValue value)
        {
            Logger.AddMessages("Выполняется пожписка на изменене аналогового значения");
            try
            {
                subscribeValue = value;         // берем ссылку на значение с которым будем работать

                grState = new SubscriptionState() { Name = "SubscribeToAnalogValue", Active = true, UpdateRate = 300 };

                grReadAnalogValue = (Subscription)server.CreateSubscription(grState);
                grReadAnalogValue.AddItems(valueItems);
                // Добавляем обработчик изменения аналогового значения
                grReadAnalogValue.DataChanged += (object subsHandle, object requestHandle, ItemValueResult[] result) =>
                {
                    foreach (ItemValueResult res in result)         // массив результирующих значений может быть переменной длинны
                    {
                        if (res.Quality == Quality.Good)
                        {
                            // В зависимости от имени тега присваиваем значение
                            // почему switch работает только с константными значениями
                            if (res.ItemName == valueTags[0])
                            {
                                subscribeValue.ADCValue = (float)res.Value;
                            }
                            else if (res.ItemName == valueTags[1])
                            {
                                subscribeValue.PLCValue = (float)res.Value;
                            }
                            else if (res.ItemName == valueTags[2])
                            {
                                subscribeValue.AnalogState = (ushort)res.Value;
                            }
                            else
                            {
                                Logger.AddWarning($"Прочитан неизвестный тег {res.ItemName}, значение не будет обработано");
                            }
                        }
                    }
                };
                subscribeValue.SubscribeComplite = true;            // отмечаем, что подписка оформлена
            }
            catch (Exception e)
            {
                throw new Exception("При выполнении подписки на изменения значений аналогового сигнала возникло исключение", e);
            }
        }
        #endregion
        /// <summary>
        /// Задать теги для команд
        /// </summary>
        /// <param name="idTag"></param>
        /// <param name="cmdTag"></param>
        /// <param name="valueTag"></param>
        public void SetCmdTags(string idTag, string cmdTag, string valueTag)
        {
            cmdTags = new string[3];
            cmdTags[0] = idTag;
            cmdTags[1] = cmdTag;
            cmdTags[2] = valueTag;

            cmdItems = new Item[cmdTags.Length];
            for (int i = 0; i < cmdTags.Length; i++)
            {
                cmdItems[i] = new Item() { ItemName = cmdTags[i] };
            }
        }

        /// <summary>команда установить имитацию</summary> 
        public void CmdSetImit(float ImitValue, uint id)
        {
            SetValue(id, 1, ImitValue);         // записываем в контроллер команду - установить имитацию
        }
        /// <summary>
        /// команда снять имитацию
        /// </summary>
        /// <param name="id"></param>
        public void CmdUnsetImit(uint id)
        {
            SetValue(id, 2, 0);         // записываем в контроллер команду - снять имитацию
        }

        /// <summary>
        ///  команда изменить значение
        /// </summary>
        /// <param name="ImitValue"></param>
        /// <param name="id"></param>
        public void CmdChangeImitValue(float ImitValue, uint id)
        {
            SetValue(id, 3, ImitValue);         // записываем в контроллер команду - изменить имитированное значение
        }

        // записываем команду в OPC сервер
        void SetValue(uint id, int cmd, float value)
        {
            // если сервер не подключен, пробуем подключить
            try
            {
                Logger.AddMessages($"Попытка подключения к opc da серуеру {server.Url}");
                if (!server.IsConnected)
                    server.Connect();
            }
            catch (Exception e)
            {
                throw new Exception($"Ошибка подключения к OPC DA серверу {server.Url} для установки команды аналогового сигнала", e);
            }

            // создаем объекты значений и переписываем значения в них
            ItemValue[] values = new ItemValue[cmdItems.Length];
            for (int i = 0; i < cmdItems.Length; i++)
            {
                values[i] = new ItemValue(cmdItems[i]);
            }
            values[0].Value = id;               // идентификатор сигнала
            values[1].Value = cmd;              // 1- установить имитацию, 2- снять имитацию, 3 - поменять значение
            values[2].Value = value;            // новое значение

            try
            {
                bool flSuccess = true;
                IdentifiedResult[] result = server.Write(values);
                for (int i = 0; i < result.Length; i++)
                {
                    if (result[i].ResultID != ResultID.S_OK)
                    {
                        flSuccess = false;
                        Logger.AddWarning($"Для уставки с тегом {result[i].ItemName} значение записано с ошибкой {result[i].ResultID}");
                    }
                }
                if (!flSuccess)
                {
                    throw new UserMessageException("Команда не установлена", MessageType.Error);
                }
                Logger.AddMessages("Выполнена запись команды");
            }
            catch (UserMessageException exc)
            { throw exc; }
            catch (Exception e)
            {
                throw new Exception($"При записи команды в OPC сервер {server.Url} возникло исключение", e);
            }
        }
    }
}
