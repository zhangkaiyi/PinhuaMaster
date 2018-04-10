using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PinhuaMaster.Data.Entities.EastRiver;
using PinhuaMaster.Data.Entities.Pinhua;

namespace PinhuaMaster.Pages.Attendance
{
    public class TestModel : PageModel
    {
        private readonly PinhuaContext _pinhuaContext;
        private readonly EastRiverContext _eastRiverContext;

        public TestModel(PinhuaContext pinhuaContext, EastRiverContext eastRiverContext)
        {
            _pinhuaContext = pinhuaContext;
            _eastRiverContext = eastRiverContext;
        }

        public int? Y { get; set; }
        public int? M { get; set; }

        public IList<ABC> ABC { get; set; }

        public void OnGet()
        {
            var a = _eastRiverContext.TimeRecords.AsNoTracking().Select(p => p.SignTime.Year).Distinct().ToList();
            ABC = (from y in a
                   join m in _eastRiverContext.TimeRecords.AsNoTracking() on y equals m.SignTime.Year into ms
                   select new ABC
                   {
                       Year = y,
                       MonthList = ms.Select(p => p.SignTime.Month).Distinct().OrderByDescending(p => p)
                   }).OrderByDescending(p => p.Year).ToList();


        }
    }

    public class ABC
    {
        public int Year { get; set; }
        public IEnumerable<int> MonthList { get; set; }
    }
}