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
using PinhuaMaster.Extensions;

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
        [BindProperty]
        public List<SelectListItem> DeliveryTypes { get; set; } = new List<SelectListItem>();
        [BindProperty]
        public List<SelectListItem> Customers { get; set; } = new List<SelectListItem>();

        public void OnGet()
        {
            DeliveryTypes = BuildTypes();
            Customers = BuildCustomers();
        }

        public IActionResult OnPost(InputModel input)
        {
            if (ModelState.IsValid)
            {
                var Rcid = _pinhuaContext.GetNewRcId();
                var rtId = "157.1";
                var repCase = new PinhuaMaster.Data.Entities.Pinhua.EsRepCase
                {
                    RcId = Rcid,
                    RtId = rtId,
                    LstFiller = 2,
                    LstFillerName = User.Identity.Name,
                    LstFillDate = DateTime.Now,
                    //fillDate = DateTime.Now,
                    //wiId = "",
                    //state = 1,
                };
                var main = new Gi2Main
                {
                    ExcelServerRcid = Rcid,
                    ExcelServerRtid = rtId,
                    DeliveryId = input.DeliveryId,
                    DeliveryType = input.DeliveryType,
                    Contact = input.Contact,
                    CreatedDate = input.CreatedDate,
                    CreatedBy = input.CreatedBy,
                    ContactNumber = input.ContactNumber,
                    CustomerId = input.CustomerId,
                    CustomerName = _pinhuaContext.往来单位.AsNoTracking().FirstOrDefault(p => p.单位编号 == input.CustomerId).单位名称,
                    DeliveryAddress = input.DeliveryAddress,
                    DeliveryDate = DateTime.Parse(input.DeliveryDate),
                    Remarks = input.Remarks
                };
                var details = new List<Gi2Details>();
                foreach (var item in input.DeliveryItems)
                {
                    if (string.IsNullOrEmpty(item.Description))
                        continue;
                    details.Add(new Gi2Details
                    {
                        ExcelServerRcid = main.ExcelServerRcid,
                        ExcelServerRtid = rtId,
                        DeliveryId = main.DeliveryId,
                        Id = int.Parse(item.Index),
                        Description = item.Description,
                        Specification = item.Specification,
                        Length = item.Length,
                        Width = item.Width,
                        Height = item.Height,
                        Qty = item.Qty,
                        UnitQty = item.UnitQty,
                        Unit = item.Unit,
                        Price = item.Price,
                        Amount = item.Amount,
                    });
                }
                if (details.Count == 0)
                {
                    DeliveryTypes = BuildTypes();
                    Customers = BuildCustomers();
                    return Page();
                }
                _pinhuaContext.EsRepCase.Add(repCase);
                _pinhuaContext.Gi2Main.Add(main);
                _pinhuaContext.Gi2Details.AddRange(details);
                _pinhuaContext.SaveChanges();

                return RedirectToPage("Index2");
            }
            else
            {
                DeliveryTypes = BuildTypes();
                Customers = BuildCustomers();
                return Page();
            }
        }

        private List<SelectListItem> BuildTypes()
        {
            var types = from p in _pinhuaContext.业务类型.AsNoTracking()
                        where p.状态 == "Yes" && p.MvP == "GI"
                        select p;
            var groups = from p in types
                         group p by p.MvP into g
                         select g.Key;
            var groupingTypes = new List<SelectListItem>();
            foreach (var group in groups)
            {
                var optGroup = new SelectListGroup
                {
                    Name = group
                };
                foreach (var option in types)
                {
                    if (option.MvP == group)
                    {
                        groupingTypes.Add(new SelectListItem
                        {
                            Group = optGroup,
                            Text = option.业务类型1 + " - " + option.类型描述,
                            Value = option.业务类型1,
                        });
                    }
                }
            }
            return groupingTypes;
        }

        private List<SelectListItem> BuildCustomers()
        {
            var customers = from p in _pinhuaContext.往来单位.ToList()
                            select p;

            var groups = from p in customers
                         orderby p.Rank descending
                         group p by p.Rank into g
                         select g.Key;
            var groupingCustomers = new List<SelectListItem>();
            foreach (var key in groups)
            {
                var optGroup = new SelectListGroup
                {
                    Name = key?.ToString()
                };
                foreach (var customer in customers)
                {
                    if (customer.Rank == key)
                    {
                        groupingCustomers.Add(new SelectListItem
                        {
                            Group = optGroup,
                            Text = customer.单位编号 + " - " + customer.单位名称,
                            Value = customer.单位编号
                        });
                    }
                }
            }
            return groupingCustomers;
        }

        public class InputModel
        {
            [Display(Name = "关联号")]
            public string Rcid { get; set; }
            [Required, Display(Name = "单号")]
            public string DeliveryId { get; set; }
            [Required, Display(Name = "客户单位")]
            public string CustomerId { get; set; }
            [Display(Name = "客户单位名称")]
            public string CustomerName { get; set; }
            [Required, Display(Name = "业务类型")]
            public string DeliveryType { get; set; }
            [Display(Name = "业务描述")]
            public string DeliveryTypeDescription { get; set; }
            [Required, Display(Name = "日期")]
            public string DeliveryDate { get; set; }
            [Display(Name = "地址")]
            public string DeliveryAddress { get; set; }
            [Display(Name = "备注")]
            public string Remarks { get; set; }
            [Display(Name = "联系人")]
            public string Contact { get; set; }
            [Display(Name = "联系电话")]
            public string ContactNumber { get; set; }
            [Display(Name = "制单人")]
            public string CreatedBy { get; set; }
            [Display(Name = "制单日期")]
            public DateTime? CreatedDate { get; set; }
            public List<ItemModel> DeliveryItems { get; set; }
        }

        public class ItemModel
        {
            public string Index { get; set; }
            public string DeliveryId { get; set; }
            public string Description { get; set; }
            public string Specification { get; set; }
            public decimal? Qty { get; set; }
            public decimal? UnitQty { get; set; }
            public string Unit { get; set; }
            public decimal? Price { get; set; }
            public decimal? Amount { get; set; }
            public decimal? Length { get; set; }
            public decimal? Width { get; set; }
            public decimal? Height { get; set; }
            public string Remarks { get; set; }
        }
    }
}