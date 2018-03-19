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
    public class Edit2Model : PageModel
    {
        private PinhuaContext _pinhuaContext { get; set; }

        public Edit2Model(PinhuaContext pinhuaContext)
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

            Input = (from p in _pinhuaContext.Gi2Main.AsNoTracking().Where(p => p.DeliveryId == Id)
                     join d in _pinhuaContext.Gi2Details.AsNoTracking() on p.ExcelServerRcid equals d.ExcelServerRcid into details
                     select new CreateModel.InputModel
                     {
                         Rcid = p.ExcelServerRcid,
                         DeliveryId = p.DeliveryId,
                         CustomerId = p.CustomerId,
                         CustomerName = p.CustomerName,
                         DeliveryDate = p.DeliveryDate.Value.ToString("yyyy-MM-dd"),
                         DeliveryType = p.DeliveryType,
                         Contact = p.Contact,
                         ContactNumber = p.ContactNumber,
                         CreatedBy = p.CreatedBy,
                         CreatedDate = p.CreatedDate,
                         DeliveryAddress = p.DeliveryAddress,
                         Remarks = p.Remarks,
                         DeliveryItems = details.Select(item => new CreateModel.ItemModel
                         {
                             DeliveryId = item.DeliveryId,
                             Index = item.Id.ToString(),
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
                             Remarks = string.Empty
                         }).ToList()
                     }).FirstOrDefault();
        }

        public IActionResult OnPost()
        {
            var order = _pinhuaContext.Gi2Main.FirstOrDefault(p => p.DeliveryId == Input.DeliveryId);
            order.Contact = Input.Contact;
            order.ContactNumber = Input.ContactNumber;
            order.CustomerId = Input.CustomerId;
            order.CustomerName = _pinhuaContext.往来单位.FirstOrDefault(p => p.单位编号 == Input.CustomerId).单位名称;
            order.DeliveryAddress = Input.DeliveryAddress;
            order.DeliveryDate = DateTime.Parse(Input.DeliveryDate);
            order.DeliveryType = Input.DeliveryType;
            order.Remarks = Input.Remarks;

            if (Input.DeliveryItems.Count > 0)
            {
                foreach (var item in Input.DeliveryItems)
                {
                    var i = _pinhuaContext.Gi2Details.FirstOrDefault(p => p.DeliveryId == Input.DeliveryId && p.Id == int.Parse(item.Index));
                    if (i != null)
                    {
                        i.ExcelServerRcid = order.ExcelServerRcid;
                        i.ExcelServerRtid = order.ExcelServerRtid;
                        i.DeliveryId = order.DeliveryId;
                        i.Id = int.Parse(item.Index);
                        i.Description = item.Description;
                        i.Specification = item.Specification;
                        i.Length = item.Length;
                        i.Width = item.Width;
                        i.Height = item.Height;
                        i.Qty = item.Qty;
                        i.UnitQty = item.UnitQty;
                        i.Unit = item.Unit;
                        i.Price = item.Price;
                        i.Amount = item.Amount;
                    }
                    else
                    {
                        _pinhuaContext.Gi2Details.Add(new Gi2Details
                        {
                            ExcelServerRcid = order.ExcelServerRcid,
                            ExcelServerRtid = order.ExcelServerRtid,
                            DeliveryId = order.DeliveryId,
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
                }

                foreach (var detail in _pinhuaContext.Gi2Details.Where(p => p.DeliveryId == Input.DeliveryId))
                {
                    var i = Input.DeliveryItems.FirstOrDefault(p=>int.Parse(p.Index) == detail.Id);
                    if (i == null)
                        _pinhuaContext.Gi2Details.Remove(detail);
                }
            }
            _pinhuaContext.SaveChanges();
            return RedirectToPage("Index2");
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