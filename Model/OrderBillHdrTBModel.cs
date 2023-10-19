namespace queueapi.Model
{
    public class OrderBillHdrTBModel
    {
        public string ShopCode { get; set; }
        public string TerminalNo { get; set; }
        public DateTime BillDate { get; set; }
        public string BillType { get; set; }
        public string BillNo { get; set; }
        public string BillTime { get; set; }
        public string PriceMode { get; set; }
        public string OrderNo { get; set; }
        public string TableNo { get; set; }
        public string ModeDes1 { get; set; }
        public string QueueNumber
        {
            get
            {
                return PriceMode.PadLeft(2) + BillNo.Substring(2);
            }
        }
    }
}
