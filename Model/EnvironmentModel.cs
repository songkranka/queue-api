namespace queueapi.Model
{
    public class EnvironmentModel
    {
        public bool DetailedErrors { get; set; }
        public Logging Logging { get; set; }
        public string AllowedHosts { get; set; }
        public UrlEndPoint urlEndPoint { get; set; }
        public ConnectionStrings connectionStrings { get; set; }
        public string BlobConnectionString { get; set; }
        public string BlobContainerName { get; set; }
        public string BlobSASTOKEN { get; set; }
    }
    public class Logging
    {

        public LogLevel LogLevel { get; set; }
    }
    public class LogLevel
    {
        public string Default { get; set; }

    }
    public class UrlEndPoint
    {
        public string BaseApi { get; set; }
        public string PunthaiApi { get; set; }
        public string SignedApi { get; set; }
    }
    public class BaseDirectory
    {

        public string Logs { get; set; }
        public string Query { get; set; }
        public string localStorage { get; set; }

    }

    public class ConnectionStrings
    {
        public string DbConnection { get; set; }
    }

}
