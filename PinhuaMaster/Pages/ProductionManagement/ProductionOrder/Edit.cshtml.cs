using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PinhuaMaster.Data.Entities.Pinhua;
using PinhuaMaster.Pages.ProductionManagement.ProductionOrder.ViewModel;

namespace PinhuaMaster.Pages.ProductionManagement.ProductionOrder
{
    public class EditModel : PageModel
    {
        private readonly PinhuaContext _pinhuaContext;
        private readonly IMapper _mapper;

        public EditModel(PinhuaContext pinhuaContext, IMapper mapper)
        {
            _pinhuaContext = pinhuaContext;
            _mapper = mapper;
        }

        public void OnGet()
        {

        }

        public IActionResult OnPost()
        {
            return RedirectToPage("Index");
        }

        public IActionResult OnPostFinishOrder(string Id)
        {
            var order = _pinhuaContext.生产单.FirstOrDefault(p => p.单号 == Id);
            if (order != null)
            {
                order.状态 = "完成";
                _pinhuaContext.SaveChanges();

                return new JsonResult("result:ok");
            }
            else
                return new JsonResult("result:fail");
        }
    }
}