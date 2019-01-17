using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Opc;
using Opc.Da;
using OpcCom;
using SCADA.Logging;

using SCADA.AnalogView.AnalogParametrs;
using SCADA.AnalogView.AnalogParametrs.AnalogInterfaces;

namespace SCADA.AnalogView
{
    /*
     ===========================================================
     В классе реализуются методы чтения / записи данных через OPCDA
         */
    class OPCDAWorker:IPLCAnalogUstavki, IPLCAnalogValueReader, IPLCAnalogValueWriter
    {
        string[] ustvakiTags;

        Opc.Da.Server server;

        Item[] ustavkiItems;

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

        public void SubscribeToChangeAnalogValue(ref AnalogValue value)
        {
            throw new NotImplementedException();
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
                if (result[i].Quality != Opc.Da.Quality.Good)
                {
                    flIsSuccess = false;
                    Logger.AddWarning($"Для тега {result[i].ItemName} не прочитано значение с ошибкой {result[i].ResultID}");
                }
            }
            if (!flIsSuccess)
                throw new UserMessageException("Считанные значения из PLC не достоверны", MessageType.Error);
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

            // Технологичнские уставки
            for (int i = 0; i < 12; i++)
            {
                values[8+i].Value = ustavkiContainer.UstValues[i].Value;
            }

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

        public void GetCurrentAnalogValue(out AnalogValue value)
        {
            throw new NotImplementedException();
        }

        public void SetAnalogTags(string ADCTag, string PLCValueTag, string AnalogState)
        {
            throw new NotImplementedException();
        }
    }
}
