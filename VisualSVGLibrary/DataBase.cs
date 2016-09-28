using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OracleClient;

namespace VisualSVGLibrary
{
    public enum DataBaseType { MsSql, MySql, Oracle }
    class DataBase
    {
        public static DataBaseType databaseType = DataBaseType.Oracle;
        public static string connectionString = "Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.0.200)(PORT = 1521))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = orcl)));User Id=power;Password=sa;";
        public static string errorInfo;

        public static DataTable GetDt(string sqlString, bool isShowMessage = true)
        {
            DataTable dt = new DataTable();
            try
            {
                if (databaseType == DataBaseType.Oracle)
                {
                    using (OracleConnection con = new OracleConnection(connectionString))
                    {
                        OracleDataAdapter adapter = new OracleDataAdapter(sqlString, con);
                        adapter.Fill(dt);
                    }
                    return dt;
                }
            }
            catch (Exception e)
            {
                errorInfo = e.Message;
            }

            if (isShowMessage)
            {
                System.Windows.MessageBox.Show("不能联接数据库或SQL语句错误，程序将关闭！" + sqlString, "错误信息");
                System.Windows.MessageBox.Show(errorInfo, "错误信息");
                Environment.Exit(0);
            }
            return null;
        }

        /// <returns>-1 表示失败，其他表示正常</returns>
        public static int ExecSql(string sqlString, bool isShowMessage = true)
        {
            try
            {
                if (databaseType == DataBaseType.Oracle)
                {
                    using (OracleConnection con = new OracleConnection(connectionString))
                    {
                        OracleCommand cmd = new OracleCommand(sqlString, con);
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                errorInfo = e.Message;
            }

            if (isShowMessage)
            {
                System.Windows.MessageBox.Show("执行SQL语句错误！" + sqlString, "错误信息");
                System.Windows.MessageBox.Show(errorInfo, "错误信息");
            }

            return -1;
        }
    }
}
