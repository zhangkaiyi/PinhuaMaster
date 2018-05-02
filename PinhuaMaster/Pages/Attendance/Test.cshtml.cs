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

        public IList<YMList> ymList { get; set; }

        public void OnGet()
        {
            var a = _eastRiverContext.TimeRecords.AsNoTracking().Select(p => p.SignTime.Year).Distinct().ToList();
            var reports = _pinhuaContext.AttendanceReport.AsNoTracking().ToList();
            ymList = (from y in a
                      join m in _eastRiverContext.TimeRecords.AsNoTracking() on y equals m.SignTime.Year into ms
                      select new YMList
                      {
                          Year = y,
                          MonthList = ms.Select(p => p.SignTime.Month).Distinct().OrderByDescending(p => p)
                          .Select(p => new MType
                          {
                              Month = p,
                              State = reports.Where(r => r.Y == y && r.M == p).Count() > 0 ? "已存在" : ""
                          })
                      }).OrderByDescending(p => p.Year).ToList();


        }
    }

    public class YMList
    {
        public int Year { get; set; }
        public IEnumerable<MType> MonthList { get; set; }
    }

    public class MType
    {
        public int Month { get; set; }
        public string State { get; set; }
    }
}