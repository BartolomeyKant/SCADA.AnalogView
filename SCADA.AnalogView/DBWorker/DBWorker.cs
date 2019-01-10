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
            finally {
                connection.Close();
            }
        }

        public void SetDBTag(string tag)
        {
            this.tag = tag;
        }

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
                                      ",[Ustavki].[MAX6] " +
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
                                      ",[Ustavki].[MIN6] " +
                                      ",[Ustavki].[LL] " +
                                      ",[Ustavki].[scale_min] " +
                                      ",[Ustavki].[scale_max] " +
                                      ",[Ustavki].[HISTER] " +
                                      ",[Ustavki].[D_NPD] " +
                                      ",[Ustavki].[D_VPD] " +
                                      ",[UstavkiNM].[MAX6] " +
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
                                      ",[UstavkiNM].[MIN6] " +
                                      "  FROM[asupt].[dbo].[Ustavki], [asupt].[dbo].[UstavkiNM]  " +
                                      $"  Where[UstavkiNM].ID_NM = [Ustavki].ID_NM and[Ustavki].f_enabled = 1 and[Ustavki].TAG = '{tag}';";
                IDataReader reader = command.ExecuteReader();

                if (reader.FieldCount > 0)
                {
                    while (reader.Read())
                    {
                        Logger.AddMessages("Переописание уставок из базы данных");

                        
                        commonParam.DBIndex = (uint)(int)reader["index"];           // двойное преобразование потому-что в базе лежат в интах, хотя логичнее в уинтах
                        commonParam.ControllerIndex = (uint)(int)reader["controller_index"];
                        commonParam.Name = reader["DESCRIPTION"] is DBNull ? "" : (string)reader["DESCRIPTION"];
                        commonParam.Tag = (string)reader["TAG"];


                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"При чтении уставок из базхы данных для тега - '{tag}' возникло исключение", e);
            }
        }


        public void WriteUstavki(UstavkiContainer ustavkiContainer)
        {
            throw new NotImplementedException();
        }

    }
}
