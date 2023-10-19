namespace queueapi.Model
{

    public class ResponseCMPOSModel
    {
        public string ShopCode { get; set; }
        public ResponseOrderQueue Serve { get; set; }
        public ResponseOrderQueue Wait { get; set; }
        public List<ResponseOrder> Order { get; set; }
    }
    public class ResponseOrder
    {
        public string ShopCode { get; set; }
        public string Queue { get; set; }
        public string QueueNo { get; set; }
        public string QueueType { get; set; }
        public string QueueMode { get; set; }
        public string QueueDes { get; set; }
        public string ServiceTime { get; set; }

    }
    public class ResponseOrderQueue
    {

        public string Take1 { get; set; } = "&nbsp;";
        public string Delivery1 { get; set; } = "&nbsp;";
        public string Take2 { get; set; } = "&nbsp;";
        public string Delivery2 { get; set; } = "&nbsp;";
        public string Take3 { get; set; } = "&nbsp;";
        public string Delivery3 { get; set; } = "&nbsp;";
        public string Take4 { get; set; } = "&nbsp;";
        public string Delivery4 { get; set; } = "&nbsp;";
        public string Take5 { get; set; } = "&nbsp;";
        public string Delivery5 { get; set; } = "&nbsp;";
    }
    public class ResponseResult
    {
        public string code { get; set; }
        public string result { get; set; }
    }
    public class ResponseSigned
    {
        public List<DataSigned> Serve { get; set; }
        public List<DataSigned> Wait { get; set; }
    }
    public class DataSigned
    {
        public string Queue { get; set; }
        public string QueueNo { get; set; }
        public string QueueType { get; set; }
        public string QueueMode { get; set; }
        public string QueueDes { get; set; }
        public string ServiceTime { get; set; }
    }

}
