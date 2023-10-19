using queueapi.Model;

namespace queueapi.Service
{
    public interface IServiceCMPOS
    {
        public Task<int> ExecuteQBill(string query);
        public Task<List<QueueBillHdrTBModel>> GetQBillHdrTB(string ShopCode);
        public Task<string> ServeToSQLComman(string QueueNo, string Type);
        public Task<string> ReadyToSQLComman(string QueueNo);
        public Task<string> ConvertToSQLCommanAzure(List<QueueBillHdrTBModel> lst);
    }
}
