using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PinhuaMaster.Data.Entities.Pinhua;
using PinhuaMaster.Extensions;
using PinhuaMaster.Pages.OrderManagement.EasyDelivery.ViewModel;

namespace PinhuaMaster.Pages.OrderManagement.EasyDelivery
{
    public class CreateModel : PageModel
    {
        private readonly PinhuaContext _pinhuaContext;
        private readonly IMapper _mapper;

        public CreateModel(PinhuaContext pinhuaContext, IMapper mapper)
        {
            _pinhuaContext = pinhuaContext;
            _mapper = mapper;
        }

        [BindProperty]
        public Gi2ViewModel Order { get; set; }
        public List<SelectListItem> DeliveryTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> CustomerSelectList { get; set; } = new List<SelectListItem>();

        public void OnGet()
        {
            DeliveryTypes = BuildTypes();
            CustomerSelectList = _pinhuaContext.GetCustomerSelectList();
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                var Rcid = _pinhuaContext.GetNewRcId();
                var rtId = "157.1";
                var repCase = new EsRepCase
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

                var main = _mapper.Map<Gi2MainDTO, Gi2Main>(Order.Main);
                main.ExcelServerRcid = Rcid;
                main.ExcelServerRtid = rtId;
                main.CustomerName = _pinhuaContext.往来单位.AsNoTracking().FirstOrDefault(p => p.单位编号 == Order.Main.CustomerId).单位名称;

                var details = _mapper.Map<List<Gi2DetaislDTO>, List<Gi2Details>>(Order.Details);
                details.ForEach(i =>
                {
                    i.ExcelServerRcid = Rcid;
                    i.ExcelServerRtid = rtId;
                    i.DeliveryId = main.DeliveryId;
                });

                if (details.Count == 0)
                {
                    ModelState.AddModelError("", "出库清单不可为空");
                    DeliveryTypes = BuildTypes();
                    CustomerSelectList = _pinhuaContext.GetCustomerSelectList();
                    return Page();
                }
                _pinhuaContext.EsRepCase.Add(repCase);
                _pinhuaContext.Gi2Main.Add(main);
                _pinhuaContext.Gi2Details.AddRange(details);
                _pinhuaContext.SaveChanges();

                return RedirectToPage("Index");
            }
            else
            {
                DeliveryTypes = BuildTypes();
                CustomerSelectList = _pinhuaContext.GetCustomerSelectList();
                return Page();
            }
        }

        private List<SelectListItem> BuildTypes()
        {
            var types = (from p in _pinhuaContext.业务类型.AsNoTracking()
                        where p.状态 == "Yes" && p.MvP == "GI"
                        select p).ToList();
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

    }
}