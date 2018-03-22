using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PinhuaMaster.Data.Entities.Pinhua;
using PinhuaMaster.Pages.OrderManagement.EasyDelivery.ViewModel;

namespace PinhuaMaster.Pages.OrderManagement.EasyDelivery
{
    public class IndexModel : PageModel
    {
        private PinhuaContext _pinhuaContext { get; set; }
        public IndexModel(PinhuaContext pinhuaContext)
        {
            _pinhuaContext = pinhuaContext;
        }

        public List<Gi2Main> DeliveryOrders { get; set; }

        public void OnGet()
        {
            DeliveryOrders = _pinhuaContext.Gi2Main.ToList();
        }

        public IActionResult OnGetAjaxEasyDelivery()
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
                         orderby p.DeliveryDate descending, p.CreatedDate descending
                         select new Gi2MainDTO
                         {
                             DeliveryType = p.DeliveryType,
                             DeliveryTypeDescription = t.类型描述,
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
                         };
            return new JsonResult(orders, settings);
        }
    }
}