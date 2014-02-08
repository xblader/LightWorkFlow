using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.OracleClient;
using System.Data;
using WorkFlow.Configuration;
using WorkFlow.Utils;

namespace WorkFlow.DAO
{
    public class DAOOracleDataBase : IDAO
    {
        public string GetJson()
        {       
            return GetBlob();          
        }

        private string GetBlob()
        {
            byte[] data = new byte[] { };           
            string sql = WorkFlowConfiguration.Binder.Setting.Parameter.Query;
            string strconn = WorkFlowConfiguration.Binder.Setting.Parameter.ConnectionString;

            using (OracleConnection conn = new OracleConnection(strconn))
            {
                conn.Open();
                using (OracleCommand cmd = new OracleCommand(sql, conn))
                {
                    using (IDataReader dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            data = (Byte[])dataReader[0];
                        }
                    }
                }
            }

            return string.Join("", WorkFlowUtils.BinaryToStrings(data, 1024));            
        }        
    }
}
