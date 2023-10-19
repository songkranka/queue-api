namespace queueapi.Service
{
    public class ServiceLogs
    {
        private static string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        public ServiceLogs(IConfiguration configuration, ILogger logger) {

            _configuration = configuration;
            _logger = logger;
        }
        public void Write(string logMessage)
        {
            try
            {
                var PathLog = _configuration.GetValue<string>("BaseDirectory:Logs");

                string PathLogs = string.Format("{0}{1}{2}{3}", BaseDirectory, PathLog, DateTime.Now.ToString("yyyyMMdd"), ".txt");

                if (File.Exists(PathLogs))
                {
                    using (FileStream fs = new FileStream(PathLogs, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        using (StreamWriter writer = new StreamWriter(fs))
                        {
                            writer.BaseStream.Seek(0, SeekOrigin.End);
                            writer.WriteLine(DateTime.Now.TimeOfDay + ":" + logMessage);
                            writer.Close();
                        }
                        fs.Dispose();
                    }
                }
                else
                {
                    using (StreamWriter w = File.AppendText(PathLogs))
                    {
                        w.WriteLine("{0}:{1}", DateTime.Now.TimeOfDay, logMessage);
                        w.Close();
                        w.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
