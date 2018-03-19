using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PinhuaMaster.Data.Entities.Pinhua;

namespace PinhuaMaster.Pages.OrderManagement.Old.DeliveryOrder
{
    public class Delete2Model : PageModel
    {
        private PinhuaContext _pinhuaContext { get; set; }
        public Delete2Model(PinhuaContext pinhuaContext)
        {
            _pinhuaContext = pinhuaContext;
        }

        public string Id { get; set; }

        public void OnGet(string Id)
        {
            this.Id = Id;
        }

        public IActionResult OnPost(string Id)
        {
            var rcId = _pinhuaContext.Gi2Main.FirstOrDefault(x => x.DeliveryId == Id).ExcelServerRcid;
            var a = _pinhuaContext.EsRepCase.Where(x => x.RcId == rcId);
            if (a != null)
                _pinhuaContext.EsRepCase.RemoveRange(a);
            var b = _pinhuaContext.Gi2Main.Where(x => x.ExcelServerRcid == rcId);
            if (b != null)
                _pinhuaContext.Gi2Main.RemoveRange(b);
            var c = _pinhuaContext.Gi2Details.Where(x => x.ExcelServerRcid == rcId);
            if (c != null)
                _pinhuaContext.Gi2Details.RemoveRange(c);
            _pinhuaContext.SaveChanges();
            return RedirectToPage("Index2");
        }
    }
}