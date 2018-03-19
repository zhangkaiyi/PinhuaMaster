using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PinhuaMaster.Data.Entities.Pinhua;

namespace PinhuaMaster.Pages.OrderManagement.Old.DeliveryOrder
{
    public class Index2Model : PageModel
    {
        private PinhuaContext _pinhuaContext { get; set; }
        public Index2Model(PinhuaContext pinhuaContext)
        {
            _pinhuaContext = pinhuaContext;
        }

        public List<Gi2Main> DeliveryOrders { get; set; }

        public void OnGet()
        {
            DeliveryOrders = _pinhuaContext.Gi2Main.ToList();
        }

        public IActionResult OnGetAjaxDeliveryOrders()
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            //EF Core中默认为驼峰样式序列化处理key
            //settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            //使用默认方式，不更改元数据的key的大小写
            settings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();

            var orders = from p in _pinhuaContext.Gi2Main
                         join d in _pinhuaContext.Gi2Details on p.ExcelServerRcid equals d.ExcelServerRcid into details
                         join u in _pinhuaContext.往来单位 on p.CustomerId equals u.单位编号
                         join t in _pinhuaContext.业务类型 on p.DeliveryType equals t.业务类型1
                         select new DeliveryOrder
                         {
                             Type = p.DeliveryType,
                             TypeDescription = t.类型描述,
                             DeliveryId = p.DeliveryId,
                             CustomerId = p.CustomerId,
                             CustomerName = u.单位名称,
                             DeliveryAddress = p.DeliveryAddress,
                             DeliveryDate = p.DeliveryDate,
                             Remarks = p.Remarks,
                             CreatedBy = p.CreatedBy,
                             CreatedDate = p.CreatedDate,
                             Contact = p.Contact,
                             ContactNumber = p.ContactNumber,
                             Amount = details.Sum(x => x.Amount),
                             Items = new List<Item>()
                         };
            return new JsonResult(orders, settings);
        }

        public class DeliveryOrder
        {
            public string Type { get; set; }
            public string TypeDescription { get; set; }
            public string DeliveryId { get; set; }
            public string CustomerId { get; set; }
            public string CustomerName { get; set; }
            public string DeliveryAddress { get; set; }
            [Newtonsoft.Json.JsonConverter(typeof(MyDateConvert))]
            public DateTime? DeliveryDate { get; set; }
            public string Remarks { get; set; }
            public string CreatedBy { get; set; }
            public string Contact { get; set; }
            public string ContactNumber { get; set; }
            [Newtonsoft.Json.JsonConverter(typeof(MyDateTimeConvert))]
            public DateTime? CreatedDate { get; set; }
            public decimal? Amount { get; set; }
            public IEnumerable<Item> Items { get; set; }
        }

        public class Item
        {
            public string DeliveryId { get; set; }
            public int Id { get; set; }
            public string Description { get; set; }
            public string Specification { get; set; }
            public decimal? Length { get; set; }
            public decimal? Width { get; set; }
            public decimal? Height { get; set; }
            public decimal? Qty { get; set; }
            public decimal? UnitQty { get; set; }
            public string Unit { get; set; }
            public decimal? Price { get; set; }
            public decimal? Amount { get; set; }
            public string Remarks { get; set; }
        }

        public class MyDateTimeConvert : Newtonsoft.Json.Converters.IsoDateTimeConverter
        {
            public MyDateTimeConvert() : base()
            {
                base.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            }
        }

        public class MyDateConvert : Newtonsoft.Json.Converters.IsoDateTimeConverter
        {
            public MyDateConvert() : base()
            {
                base.DateTimeFormat = "yyyy-MM-dd";
            }
        }
    }
}