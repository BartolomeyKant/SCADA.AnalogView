using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using SCADA.AnalogView.AnalogParametrs;
using SCADA.AnalogView.AnalogParametrs.AnalogInterfaces;
using SCADA.Logging;

namespace SCADA.AnalogView
{
    /*  ============================================================
     Класс для работы с базой данных объектов
      - чтения общих параметров 
      - чтения уставок
      - записи уставок
    ===================================================================*/
    class DBWorker : IDBWorker
    {
        string tag;

        IDbConnection connection;
        IDbCommand command;

        /// <summary>
        /// Создание DBWorker'a с заданным коннектион стринг
        /// </summary>
        /// <param name="connectionString"></param>
        public DBWorker(string connectionString)
        {
            // подключение к базе данных
            try { connection = new SqlConnection(connectionString); }
            catch (Exception e)
            {
                throw new Exception($"При попытке подключения к базе данных объектов со строкой подключения {connectionString} возникло исключение", e);
            }

            // создание объекта команд\
            try
            {
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
            }
            catch (Exception e)
            {
                throw new Exception($"При попытке создания объекта команд базы данных, со строкой подключения {connectionString} возникло исключение", e);
            }
            finally
            {
                connection.Close();
            }
        }

        public void SetDBTag(string tag)
        {
            this.tag = tag;
        }

        /// <summary>
        /// Чтение уставок из базы данных
        /// </summary>
        /// <param name="ustavkiContainer"></param>
        /// <param name="commonParam"></param>
        public void ReadUstavki(ref UstavkiContainer ustavkiContainer, ref CommonAnalogParams commonParam)
        {
            connection.Open();
            try
            {
                command.CommandText = "SELECT [Ustavki].[index] " +
                                      ",[Ustavki].[controller_index] " +
                                      ",[Ustavki].[TAG] " +
                                      ",[Ustavki].[DESCRIPTION] " +
                                      ",[Ustavki].[EGUDESC] " +
                                      ",[Ustavki].[format] " +
                                      ",[Ustavki].[CtrlUst] " +
                                      ",[Ustavki].[HL] " +
                                      ",[Ustavki].[MAX6] " + // 8
                                      ",[Ustavki].[MAX5] " +
                                      ",[Ustavki].[MAX4] " +
                                      ",[Ustavki].[MAX3] " +
                                      ",[Ustavki].[MAX2] " +
                                      ",[Ustavki].[MAX1] " +
                                      ",[Ustavki].[MIN1] " +
                                      ",[Ustavki].[MIN2] " +
                                      ",[Ustavki].[MIN3] " +
                                      ",[Ustavki].[MIN4] " +
                                      ",[Ustavki].[MIN5] " +
                                      ",[Ustavki].[MIN6] " + // 19
                                      ",[Ustavki].[LL] " +
                                      ",[Ustavki].[scale_min] " +
                                      ",[Ustavki].[scale_max] " +
                                      ",[Ustavki].[HISTER] " +
                                      ",[Ustavki].[D_NPD] " +
                                      ",[Ustavki].[D_VPD] " +
                                      ",[UstavkiNM].[MAX6] " + // 26
                                      ",[UstavkiNM].[MAX5] " +
                                      ",[UstavkiNM].[MAX4] " +
                                      ",[UstavkiNM].[MAX3] " +
                                      ",[UstavkiNM].[MAX2] " +
                                      ",[UstavkiNM].[MAX1] " +
                                      ",[UstavkiNM].[MIN1] " +
                                      ",[UstavkiNM].[MIN2] " +
                                      ",[UstavkiNM].[MIN3] " +
                                      ",[UstavkiNM].[MIN4] " +
                                      ",[UstavkiNM].[MIN5] " +
                                      ",[UstavkiNM].[MIN6] " + // 37
                                      "  FROM[asupt].[dbo].[Ustavki], [asupt].[dbo].[UstavkiNM]  " +
                                      $"  Where[UstavkiNM].ID_NM = [Ustavki].ID_NM and[Ustavki].f_enabled = 1 and[Ustavki].TAG = '{tag}';";
                IDataReader reader = command.ExecuteReader();

                while (reader.Read())               // Чтение всей выборки
                {
                    Logger.AddMessages("Переописание уставок из базы данных");

                    // ------------------------ Чтение общих параметров ---------------------------------
                    commonParam.DBIndex = (uint)(int)reader["index"];           // двойное преобразование потому-что в базе лежат в интах, хотя логичнее в уинтах
                    commonParam.ControllerIndex = (uint)(int)reader["controller_index"];
                    commonParam.Name = reader["DESCRIPTION"] is DBNull ? "" : (string)reader["DESCRIPTION"];
                    commonParam.Tag = (string)reader["TAG"];
                    commonParam.format = (byte)(int)reader["format"];
                    commonParam.Egu = reader["EGUDESC"] is DBNull ? "" : (string)reader["EGUDESC"];
                    commonParam.AdcEgu = "мА";

                    // ----------------------- Чтение уставок ------------------------------------------
                    // --------------- Технологические --------------------
                    uint CtrlUst = (uint)(short)reader["CtrlUst"];

                    List<UstValue> ustValues = new List<UstValue>(12);
                    for (int i = 1; i <= 12; i++)
                    {
                        string ustName;
                        float value;
                        bool used = (CtrlUst & (1 << (i - 1))) > 0;

                        ustName = reader.GetValue(38 - i) is DBNull ? "" : reader.GetString(38 - i);
                        value = reader.GetValue(20 - i) is DBNull ? 0 : (float)reader.GetDouble(20 - i);
                        // создание нового экземпляра уставки
                        ustValues.Add(new UstValue(value, used, ustName, commonParam.Egu, commonParam.format, (UstCode)i));
                    }
                    ustavkiContainer.UstValues = ustValues;             // переописание уставок в контейнер

                    ustavkiContainer.ADCMax = new UstValue(
                        reader["scale_max"] is DBNull ? 0 : (float)reader["scale_max"],
                        true,
                        "Код АЦП максимальный", commonParam.AdcEgu, 3, UstCode.ADCMax
                          );
                    ustavkiContainer.ADCMin = new UstValue(
                        reader["scale_min"] is DBNull ? 0 : (float)reader["scale_min"],
                        true,
                        "Код АЦП минимальный", commonParam.AdcEgu, 3, UstCode.ADCMin
                        );
                    ustavkiContainer.EMax = new UstValue(
                        reader["HL"] is DBNull ? 0 : (float)(double)reader["HL"],
                        true,
                        "Технологический максимум", commonParam.Egu, commonParam.format, UstCode.EMax
                        );
                    ustavkiContainer.EMin = new UstValue(
                        reader["LL"] is DBNull ? 0 : (float)(double)reader["LL"],
                        true,
                        "Технологический минимум", commonParam.Egu, commonParam.format, UstCode.EMin
                        );
                    ustavkiContainer.VPD = new UstValue(
                        reader["D_VPD"] is DBNull ? 0 : (float)(double)reader["D_VPD"],
                        true,
                        "Верхний предел достоверности", commonParam.AdcEgu, 3, UstCode.VPD
                        );
                    ustavkiContainer.NPD = new UstValue(
                        reader["D_NPD"] is DBNull ? 0 : (float)(double)reader["D_NPD"],
                        true,
                        "Нижний предел достоверности", commonParam.AdcEgu, 3, UstCode.NPD
                        );
                    ustavkiContainer.Hister = new UstValue(
                        reader["HISTER"] is DBNull ? 0 : (float)(double)reader["HISTER"],
                        true,
                        "Гистерезис", commonParam.Egu, (byte)(commonParam.format + 1), UstCode.Hister
                        );
                }
                reader.Close();
            }
            catch (Exception e)
            {
                throw new Exception($"При чтении уставок из базхы данных для тега - '{tag}' возникло исключение", e);
            }
            finally
            {
                connection.Close();
            }
        }
        /// <summary>
        /// Запись уставок в базу данных
        /// </summary>
        /// <param name="ustavkiContainer"></param>
        public void WriteUstavki(UstavkiContainer ustavkiContainer, CommonAnalogParams commonParams)
        {
            connection.Open();
            try
            {
                command.CommandText = "UPDATE Ustavki SET " +
                                      "HL = @EMax, " + 
                                      "LL = @EMin, " +
                                      "MAX6 = @UST12, " +
                                      "MAX5 = @UST11, " +
                                      "MAX4 = @UST10, " +
                                      "MAX3 = @UST9, " +
                                      "MAX2 = @UST8, " +
                                      "MAX1 = @UST7, " +
                                      "MIN1 = @UST6, " +
                                      "MIN2 = @UST5, " +
                                      "MIN3 = @UST4, " +
                                      "MIN4 = @UST3, " +
                                      "MIN5 = @UST2, " +
                                      "MIN6 = @UST1, " +
                                      "scale_max = @ADCMax, " +
                                      "scale_min = @ADCMin, " +
                                      "D_VPD = @VPD, " +
                                      "D_NPD = @NPD, " +
                                      "HISTER = @Hister " +
                                      "WHERE [index] = @index ";
                // создание параметров для запроса
                // основные уставки
                command.Parameters.Add(new SqlParameter("@EMax", ustavkiContainer.EMax.Value));
                command.Parameters.Add(new SqlParameter("@EMin", ustavkiContainer.EMin.Value));
                command.Parameters.Add(new SqlParameter("@ADCMax", ustavkiContainer.ADCMax.Value));
                command.Parameters.Add(new SqlParameter("@ADCMin", ustavkiContainer.ADCMin.Value));
                command.Parameters.Add(new SqlParameter("@VPD", ustavkiContainer.VPD.Value));
                command.Parameters.Add(new SqlParameter("@NPD", ustavkiContainer.NPD.Value));
                command.Parameters.Add(new SqlParameter("@Hister", ustavkiContainer.Hister.Value));
                // технологические уставки
                for (int i = 0; i < 12; i++)
                {
                    command.Parameters.Add(new SqlParameter($"@UST{i + 1}", ustavkiContainer.UstValues[i].Value));
                }
                command.Parameters.Add(new SqlParameter("@index", (int)commonParams.DBIndex));

                // выполнение запроса
                command.ExecuteNonQuery();
                command.Parameters.Clear();         // Очищаем список параметров для возможности повторной записи

                Logger.AddMessages($"Выполнена запись уставок в базу данных {connection.Database}" );
            }
            catch (Exception e)
            {
                throw new Exception($"При записи уставок в базу данных для тега - '{commonParams.Tag}' возникло исключение", e);
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
