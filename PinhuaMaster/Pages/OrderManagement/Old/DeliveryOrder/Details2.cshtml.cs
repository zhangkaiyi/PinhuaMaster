using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PinhuaMaster.Data.Entities.Pinhua;

namespace PinhuaMaster.Pages.OrderManagement.Old.DeliveryOrder
{
    public class Details2Model : PageModel
    {
        private PinhuaContext _pinhuaContext { get; set; }

        public Details2Model(PinhuaContext pinhuaContext)
        {
            _pinhuaContext = pinhuaContext;
        }

        [BindProperty]
        public CreateModel.InputModel Input { get; set; }
        public List<SelectListItem> DeliveryTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Customers { get; set; } = new List<SelectListItem>();

        public void OnGet(string Id)
        {
            DeliveryTypes = BuildTypes();
            Customers = BuildCustomers();

            var order = _pinhuaContext.Gi2Main.FirstOrDefault(p => p.DeliveryId == Id);
            Input = new CreateModel.InputModel
            {
                Rcid = order.ExcelServerRcid,
                DeliveryId = order.DeliveryId,
                Contact = order.Contact,
                ContactNumber = order.ContactNumber,
                CreatedBy = order.CreatedBy,
                CreatedDate = order.CreatedDate,
                CustomerId = order.CustomerId,
                CustomerName = order.CustomerName,
                DeliveryAddress = order.DeliveryAddress,
                DeliveryDate = order.DeliveryDate.Value.ToString("yyyy-MM-dd"),
                DeliveryType = order.DeliveryType,
                DeliveryTypeDescription = _pinhuaContext.业务类型.AsNoTracking().FirstOrDefault(p => p.业务类型1 == order.DeliveryType)?.类型描述 ?? string.Empty,
                Remarks = order.Remarks,
                DeliveryItems = new List<CreateModel.ItemModel>()
            };
            var items = from p in _pinhuaContext.Gi2Details.AsNoTracking()
                        where p.ExcelServerRcid == Input.Rcid
                        select new CreateModel.ItemModel
                        {
                            DeliveryId = p.DeliveryId,
                            Index = p.Id.ToString(),
                            Description = p.Description,
                            Specification = p.Specification,
                            Length = p.Length,
                            Width = p.Width,
                            Height = p.Height,
                            Qty = p.Qty,
                            UnitQty = p.UnitQty,
                            Unit = p.Unit,
                            Price = p.Price,
                            Amount = p.Amount,
                            Remarks = string.Empty
                        };
            Input.DeliveryItems.AddRange(items);
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

    }
}