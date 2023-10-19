using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using queueapi.Model;
using queueapi.Service;

namespace queueapi.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class QueueController : ControllerBase
    {
        private readonly ILogger<QueueController> _logger;
        private readonly IServiceCMPOS _servicecmpos;
        //public QueueController(ILogger<QueueController> logger, IConfiguration configuration)
        //{
        //    _logger = logger;
        //    _configuration = configuration;
        //}
        public QueueController(ILogger<QueueController> logger, IServiceCMPOS servicecmpos)
        {
            _logger = logger;
            _servicecmpos = servicecmpos;
        }
        [HttpGet]
        [Route("Test")]
        [EnableRateLimiting("fixed")]
        public async Task<ResponseCMPOSModel> GetQueueTest([FromBody] ShopCodeModel shop)
        {
            ResponseCMPOSModel Punthai = new ResponseCMPOSModel();
            try
            {
                _logger.LogInformation("test api success");
                return Punthai;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return Punthai;
            }
        }
        [HttpPost]
        [Route("GetQueue")]
        public async Task<ResponseCMPOSModel> GetQueue([FromBody] ShopCodeModel shop)
        {
            ResponseCMPOSModel Punthai = new ResponseCMPOSModel();
            ResponseOrderQueue Serve = new ResponseOrderQueue();
            ResponseOrderQueue Wait = new ResponseOrderQueue();
            List<ResponseOrder> lstOrder = new List<ResponseOrder>();

            //ServiceCMPOS _service = new ServiceCMPOS();
            try
            {
                List<QueueBillHdrTBModel> lst = await _servicecmpos.GetQBillHdrTB(shop.ShopCode);
                if (lst.Count() > 0)
                {
                    Punthai.ShopCode = lst.FirstOrDefault().ShopCode;

                    var lstserve = lst.Where(d => d.QueueType == "SERV").ToList();
                    var TakeAway = lstserve.Where(d => d.QueueMode.Length == 2).Take(5).ToList();

                    Serve.Take1 = TakeAway.Count() > 0 ? TakeAway.ElementAt(0).Queue : "&nbsp;";
                    Serve.Take2 = TakeAway.Count() > 1 ? TakeAway.ElementAt(1).Queue : "&nbsp;";
                    Serve.Take3 = TakeAway.Count() > 2 ? TakeAway.ElementAt(2).Queue : "&nbsp;";
                    Serve.Take4 = TakeAway.Count() > 3 ? TakeAway.ElementAt(3).Queue : "&nbsp;";
                    Serve.Take5 = TakeAway.Count() > 4 ? TakeAway.ElementAt(4).Queue : "&nbsp;";

                    var Delivery = lstserve.Where(d => d.QueueMode.Length > 2).Take(5).ToList();

                    Serve.Delivery1 = Delivery.Count() > 0 ? Delivery.ElementAt(0).Queue : "&nbsp;";
                    Serve.Delivery2 = Delivery.Count() > 1 ? Delivery.ElementAt(1).Queue : "&nbsp;";
                    Serve.Delivery3 = Delivery.Count() > 2 ? Delivery.ElementAt(2).Queue : "&nbsp;";
                    Serve.Delivery4 = Delivery.Count() > 3 ? Delivery.ElementAt(3).Queue : "&nbsp;";
                    Serve.Delivery5 = Delivery.Count() > 4 ? Delivery.ElementAt(4).Queue : "&nbsp;";

                    var lstwait = lst.Where(d => d.QueueType == "WAIT").ToList();
                    var ServeTake = lstwait.Where(d => d.QueueMode.Length == 2).Take(5).ToList();

                    Wait.Take1 = ServeTake.Count() > 0 ? ServeTake.ElementAt(0).Queue : "&nbsp;";
                    Wait.Take2 = ServeTake.Count() > 1 ? ServeTake.ElementAt(1).Queue : "&nbsp;";
                    Wait.Take3 = ServeTake.Count() > 2 ? ServeTake.ElementAt(2).Queue : "&nbsp;";
                    Wait.Take4 = ServeTake.Count() > 3 ? ServeTake.ElementAt(3).Queue : "&nbsp;";
                    Wait.Take5 = ServeTake.Count() > 4 ? ServeTake.ElementAt(4).Queue : "&nbsp;";

                    var ServeDelivery = lstwait.Where(d => d.QueueMode.Length > 2).Take(5).ToList();

                    Wait.Delivery1 = ServeDelivery.Count() > 0 ? ServeDelivery.ElementAt(0).Queue : "&nbsp;";
                    Wait.Delivery2 = ServeDelivery.Count() > 1 ? ServeDelivery.ElementAt(1).Queue : "&nbsp;";
                    Wait.Delivery3 = ServeDelivery.Count() > 2 ? ServeDelivery.ElementAt(2).Queue : "&nbsp;";
                    Wait.Delivery4 = ServeDelivery.Count() > 3 ? ServeDelivery.ElementAt(3).Queue : "&nbsp;";
                    Wait.Delivery5 = ServeDelivery.Count() > 4 ? ServeDelivery.ElementAt(4).Queue : "&nbsp;";
                }

                foreach (var i in lst.Take(20))
                {
                    ResponseOrder obj = new ResponseOrder();
                    obj.ShopCode = i.ShopCode;
                    obj.Queue = i.Queue;
                    obj.QueueNo = i.QueueNo;
                    obj.QueueType = i.QueueType;
                    obj.QueueMode = i.QueueMode;
                    obj.QueueDes = i.QueueDes;
                    obj.ServiceTime = i.ServiceTime;
                    lstOrder.Add(obj);
                }

                Punthai.Serve = Serve;
                Punthai.Wait = Wait;
                Punthai.Order = lstOrder;

                return Punthai;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("api/GetQueue:" + ex.Message);
                return Punthai;
            }
        }
        [HttpPost]
        [Route("PunthaiServe")]
        public async Task<ResponseResult> ServeOrder([FromBody] OrderNoModel order)
        {
            ResponseResult obj = new ResponseResult();
            try
            {
                if (order != null)
                {
                    if (!string.IsNullOrEmpty(order.Orderno))
                    {
                        //ServiceCMPOS _service = new ServiceCMPOS();

                        var query = await _servicecmpos.ServeToSQLComman(order.Orderno, order.OrderType);
                        //var query = await service.ServeToSQLCommanAzure(order.Orderno, order.OrderType);

                        int effect = await _servicecmpos.ExecuteQBill(query);

                        if (effect > 0)
                        {
                            obj.code = "00";
                            obj.result = "success";
                        }
                        else
                        {
                            obj.code = "40";
                            obj.result = "fail:DB Fail";
                        }
                    }
                    else
                    {
                        obj.code = "40";
                        obj.result = "fail:Orderno in valid";
                    }
                }
                return obj;
            }
            catch (Exception ex)
            {
                obj.code = "404";
                obj.result = "error:" + ex.Message;
                _logger.LogWarning("api/v1/PunthaiServe:" + ex.Message);
                return obj;
            }
        }
        [HttpPost]
        [Route("PunthaiReady")]
        public async Task<ResponseResult> ReadyOrder([FromBody] OrderNoModel order)
        {
            ResponseResult obj = new ResponseResult();
            try
            {
                if (order != null)
                {
                    if (!string.IsNullOrEmpty(order.Orderno))
                    {
                        //ServiceCMPOS _service = new ServiceCMPOS();

                        var query = await _servicecmpos.ReadyToSQLComman(order.Orderno);
                        //var query = await service.ReadyToSQLCommanAzure(order.Orderno);

                        int effect = await _servicecmpos.ExecuteQBill(query);

                        if (effect > 0)
                        {
                            obj.code = "00";
                            obj.result = "success";
                        }
                        else
                        {
                            obj.code = "40";
                            obj.result = "fail:DB Fail";
                        }
                    }
                    else
                    {
                        obj.code = "40";
                        obj.result = "fail:Orderno in valid";
                    }
                }
                return obj;
            }
            catch (Exception ex)
            {
                obj.code = "404";
                obj.result = "error:" + ex.Message;
                _logger.LogWarning("api/PunthaiReady:" + ex.Message);
                return obj;
            }
        }
        [HttpPost]
        [Route("Signed")]
        public async Task<ResponseSigned> OrderSigned([FromBody] ShopCodeModel shop)
        {
            ResponseSigned Signed = new ResponseSigned();
            try
            {
                if (!string.IsNullOrEmpty(shop.ShopCode))
                {
                    //ServiceCMPOS _service = new ServiceCMPOS();

                    List<QueueBillHdrTBModel> lst = await _servicecmpos.GetQBillHdrTB(shop.ShopCode);
                    if (lst.Count() > 0)
                    {
                        List<DataSigned> lstServe = new List<DataSigned>();
                        foreach (var i in lst.Where(d => d.QueueType == "SERV").Take(4).ToList())
                        {
                            DataSigned obj = new DataSigned();
                            obj.Queue = i.Queue;
                            obj.QueueDes = i.QueueDes;
                            obj.QueueMode = i.QueueMode;
                            obj.QueueType = i.QueueType;
                            obj.QueueNo = i.QueueNo;
                            obj.ServiceTime = i.ServiceTime;
                            lstServe.Add(obj);
                        }
                        List<DataSigned> lstWait = new List<DataSigned>();
                        foreach (var i in lst.Where(d => d.QueueType == "WAIT").Take(4).ToList())
                        {
                            DataSigned obj = new DataSigned();
                            obj.Queue = i.Queue;
                            obj.QueueDes = i.QueueDes;
                            obj.QueueMode = i.QueueMode;
                            obj.QueueType = i.QueueType;
                            obj.QueueNo = i.QueueNo;
                            obj.ServiceTime = i.ServiceTime;
                            lstWait.Add(obj);
                        }
                        Signed.Serve = lstServe;
                        Signed.Wait = lstWait;
                    }
                }
                return Signed;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("api/Signed :" + ex.Message);
                return Signed;
            }
        }
        [HttpPost]
        [Route("DataCMPOS")]
        public async Task<ResponseResult> DataCMPOS([FromBody] List<QueueBillHdrTBModel> lst)
        {
            ResponseResult obj = new ResponseResult();
            try
            {
                if (lst != null && lst.Count() > 0)
                {
                    var cmdsql = await _servicecmpos.ConvertToSQLCommanAzure(lst);

                    var effectdata = await _servicecmpos.ExecuteQBill(cmdsql);
                    if (effectdata > 0)
                    {
                        obj.code = "200";
                        obj.result = "success";
                    }
                    else
                    {
                        obj.code = "40";
                        obj.result = "insert error";
                    }
                }
                else
                {
                    obj.code = "40";
                    obj.result = "Req data count 0";
                }
            }
            catch (Exception ex)
            {
                obj.code = "400";
                obj.result = "Exception:" + ex.Message;
                _logger.LogWarning("api/DataCMPOS:" + obj.result);
            }
            return obj;
        }

    }
}
