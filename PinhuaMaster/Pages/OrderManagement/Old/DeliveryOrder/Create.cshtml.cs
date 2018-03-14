using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PinhuaMaster.Data.Entities.Pinhua;

namespace PinhuaMaster.Pages.OrderManagement.Old.DeliveryOrder
{
    public class CreateModel : PageModel
    {
        private PinhuaContext _pinhuaContext { get; set; }
        public CreateModel(PinhuaContext pinhuaContext)
        {
            _pinhuaContext = pinhuaContext;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<SelectListItem> DeliveryTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Customers { get; set; } = new List<SelectListItem>();

        public void OnGet()
        {
            var types = from p in _pinhuaContext.业务类型.AsNoTracking()
                        where p.状态 == "Yes" && p.MvP == "GI"
                        select p;
            var typeGroups = from p in types
                             group p by p.MvP into g
                             select g.Key;

            foreach (var group in typeGroups)
            {
                var optGroup = new SelectListGroup
                {
                    Name = group
                };
                foreach (var option in types)
                {
                    if (option.MvP == group)
                    {
                        DeliveryTypes.Add(new SelectListItem
                        {
                            Group = optGroup,
                            Text = option.业务类型1 + " - " + option.类型描述,
                            Value = option.业务类型1,
                        });
                    }
                }
            }

            var customers = from p in _pinhuaContext.往来单位.ToList()
                            select p;

            var customerGroups = from p in customers
                                 orderby p.Rank descending
                                 group p by p.Rank into g
                                 select g.Key;

            foreach (var key in customerGroups)
            {
                var optGroup = new SelectListGroup
                {
                    Name = key?.ToString()
                };
                foreach (var customer in customers)
                {
                    if (customer.Rank == key)
                    {
                        Customers.Add(new SelectListItem
                        {
                            Group = optGroup,
                            Text = customer.单位编号 + " - " + customer.单位名称,
                            Value = customer.单位编号
                        });
                    }
                }
            }
        }

        public class InputModel
        {
            [Required]
            [Display(Name = "关联号")]
            public string rcId { get; set; }
            [Required]
            [Display(Name = "单号")]
            public string orderId { get; set; }
            [Required]
            [Display(Name = "客户单位")]
            public string customerId { get; set; }
            [Display(Name = "客户单位名称")]
            public string customerName { get; set; }
            [Required]
            [Display(Name = "业务类型")]
            public string stockoutType { get; set; }
            [Display(Name = "业务类型")]
            public string stockoutTypeDescription { get; set; }
            [Required]
            [Display(Name = "日期")]
            public string stockoutDate { get; set; }
            [Display(Name = "地址")]
            public string stockoutAddress { get; set; }
            [Display(Name = "备注")]
            public string stockoutRemarks { get; set; }
            [Display(Name = "联系人")]
            public string stockoutContact { get; set; }
            [Display(Name = "联系电话")]
            public string stockoutContactNumber { get; set; }
            public List<ItemModel> stockoutItems { get; set; }
            [Display(Name = "制单人")]
            public string CreatedBy { get; set; }
            [Display(Name = "制单日期")]
            public DateTime? CreatedDate { get; set; }
        }

        public class ItemModel
        {
            [Required]
            [Display(Name = "行号")]
            public int? rN { get; set; }
            public string 编号 { get; set; }
            public string 描述 { get; set; }
            public string 规格 { get; set; }
            public decimal? 个数 { get; set; }
            public decimal? 数量 { get; set; }
            public string 单位 { get; set; }
            public decimal? 单价 { get; set; }
            public decimal? 金额 { get; set; }
            public decimal? Length { get; set; }
            public decimal? Width { get; set; }
            public decimal? Height { get; set; }
        }
    }
}