using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PinhuaMaster.Data.Entities.Pinhua;

namespace PinhuaMaster.Pages.Payroll
{
    public class IndexModel : PageModel
    {
        private readonly PinhuaContext _pinhuaContext;

        public IndexModel(PinhuaContext pinhuaContext)
        {
            _pinhuaContext = pinhuaContext;
        }

        public WageSchema Schema { get; set; }
        public IList<WageSchemaTime> SchemaDetails { get; set; }

        public IList<PayrollMain> PayrollList { get; set; }

        public void OnGet()
        {
            Schema = _pinhuaContext.WageSchema.FirstOrDefault(p => p.Id == "1");
            SchemaDetails = _pinhuaContext.WageSchemaTime.Where(p => p.SchemaId == Schema.Id).ToList();
            PayrollList = _pinhuaContext.PayrollMain.AsNoTracking().ToList();
        }
    }
}