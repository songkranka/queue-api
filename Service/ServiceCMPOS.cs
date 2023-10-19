using Microsoft.Extensions.Configuration;
using queueapi.Controllers;
using queueapi.Model;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Reflection;

namespace queueapi.Service
{
    public class ServiceCMPOS : IServiceCMPOS
    {
        private string strConnection;
        private static string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        //private static ServiceLogs logs;
        private readonly ILogger _logs;
        private readonly IConfiguration _configuration;
        public ServiceCMPOS(IConfiguration configuration)
        {
            //_configuration = configuration;

            _configuration = configuration;
            strConnection = _configuration.GetValue<string>("ConnectionStrings:DbConnection");

        }
        public async Task<int> ExecuteQBill(string query)
        {
            using (SqlConnection myConnection = new SqlConnection(strConnection))
            {
                using (SqlCommand myCommand = new SqlCommand(query))
                {
                    myConnection.Open();

                    myCommand.Connection = myConnection;
                    myCommand.CommandType = CommandType.Text;
                    try
                    {
                        int roweffect = await myCommand.ExecuteNonQueryAsync();
                        return roweffect;
                    }
                    catch (Exception ex)
                    {
                        _logs.LogError("ExecuteQBill:" + ex.Message + " Query =" + query);
                        return 0;
                    }
                    finally
                    {
                        myCommand.Dispose();
                        myConnection.Close();
                    }
                }
            }
        }
        public async Task<List<QueueBillHdrTBModel>> GetQBillHdrTB(string ShopCode)
        {
            string Query = ReadFileQueryByShopCode(ShopCode);
            List<QueueBillHdrTBModel> lst = new List<QueueBillHdrTBModel>();
            using (SqlConnection myConnection = new SqlConnection(strConnection))
            {
                using (SqlCommand myCommand = new SqlCommand(Query))
                {
                    myConnection.Open();
                    myCommand.Connection = myConnection;
                    myCommand.CommandType = CommandType.Text;
                    using (SqlDataReader reader = await myCommand.ExecuteReaderAsync())
                    {
                        try
                        {
                            DataTable table = new DataTable();
                            table.Load(reader);

                            lst = ConvertDataTable<QueueBillHdrTBModel>(table);

                            return lst.Where(d => d.ServiceTime == "False").ToList();
                        }
                        catch (Exception ex)
                        {
                            _logs.LogError("GetQBillHdrTB:" + ex.ToString());
                            return lst;
                        }
                        finally
                        {
                            reader.Close();
                            myCommand.Dispose();
                            myConnection.Close();
                        }
                    }
                }
            }
        }
        public async Task<string> ServeToSQLComman(string QueueNo, string Type)
        {
            string command = @"UPDATE QBillHdrTB";
            command += " SET ServeTime ='" + DateTime.Now.ToString("HH:mm:ss") + "',";
            command += " FinalDate = GETDATE() ,";
            if (!string.IsNullOrEmpty(Type))
            {
                command += " BillType = 'SLA',";
            }
            command += " QueueType = 'SERV'";
            command += " WHERE QueueNo = '" + QueueNo + "'";

            return command;
        }
        public async Task<string> ServeToSQLCommanAzure(string QueueNo, string Type)
        {
            string command = @"UPDATE QBillHdrTB";
            command += " SET ServeTime = convert(varchar, dbo.GETLOCALDATE(), 8),";
            command += " FinalDate = dbo.GETLOCALDATE(),";
            if (!string.IsNullOrEmpty(Type))
            {
                command += " BillType = 'SLA',";
            }
            command += " QueueType = 'SERV'";
            command += " WHERE QueueNo = '" + QueueNo + "'";

            return command;
        }
        public async Task<string> ReadyToSQLComman(string QueueNo)
        {
            string command = @"UPDATE QBillHdrTB";
            command += " SET FinalDate = SYSDATETIME(),";
            command += " QueueType = 'COMP'";
            command += " WHERE QueueNo = '" + QueueNo + "'";

            return command;
        }
        public async Task<string> ReadyToSQLCommanAzure(string QueueNo)
        {
            string command = @"UPDATE QBillHdrTB";
            command += " SET FinalDate = dbo.GETLOCALDATE(),";
            command += " QueueType = 'COMP'";
            command += " WHERE QueueNo = '" + QueueNo + "'";

            return command;
        }
        public string ConvertToSQLComman(List<QueueBillHdrTBModel> data)
        {
            string command = @"INSERT INTO QBillHdrTB VALUES ";
            int count = 1;
            foreach (var i in data)
            {
                command += @"('" + i.ShopCode + "', '" + i.Queue + "', '" + i.QueueNo + "', '" + i.BillNo + "', '" + i.BillTime + "', '" + DateTime.Now.ToString("HH:mm:ss") + "', NULL, '" + i.BillType + "', 'WAIT', '" + i.QueueMode + "', '" + i.QueueDes + "', SYSUTCDATETIME(), NULL)";
                if (count < data.Count())
                {
                    command += @",";
                }
                count++;
            }
            return command;
        }
        public async Task<string> ConvertToSQLCommanAzure(List<QueueBillHdrTBModel> data)
        {
            string command = @"INSERT INTO QBillHdrTB VALUES ";
            int count = 1;
            foreach (var i in data)
            {
                command += @"('" + i.ShopCode + "', '" + i.Queue + "', '" + i.QueueNo + "', '" + i.BillNo + "', '" + i.BillTime + "', convert(varchar, dbo.GETLOCALDATE(), 8), NULL, '" + i.BillType + "', 'WAIT', '" + i.QueueMode + "', '" + i.QueueDes + "', dbo.GETLOCALDATE(), NULL)";
                if (count < data.Count())
                {
                    command += @",";
                }
                count++;
            }
            return command;
        }
        public string ReadFileQuery()
        {
            try
            {
                var QueryDirectory = _configuration.GetValue<string>("BaseDirectory:Query");

                string FileSQL = string.Format("{0}{1}", BaseDirectory, QueryDirectory);
                if (File.Exists(FileSQL))
                {
                    return File.ReadAllText(FileSQL);
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logs.LogError("ReadFileQuery:" + ex.ToString());
                throw ex;
            }
        }
        public string ReadFileQueryByShopCode(string ShopCode)
        {
            try
            {
                string QueryString = "SELECT ShopCode, QueueType ,Queue, QueueNo, QueueMode, QueueDes, QueueDate, ";
                QueryString += " CASE";
                QueryString += "     WHEN BillType = 'SLA' and  DATEADD(second, 120, FinalDate) <= GETDATE() THEN 'True' ";
                QueryString += "     ELSE 'False' ";
                QueryString += " END AS ServiceTime ";
                QueryString += " FROM QBillHdrTB";
                QueryString += " WHERE ShopCode ='" + ShopCode + "' and  QueueType != 'COMP' and CONVERT(Date, QueueDate) = CONVERT(Date, GETDATE())";
                QueryString += " ORDER BY QueueType, FinalDate DESC, QueueDate DESC";

                return QueryString;
            }
            catch (Exception ex)
            {
                _logs.LogError("ReadFileQueryByShopCode:" + ex.ToString());
                throw ex;
            }
        }
        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

    }
}
