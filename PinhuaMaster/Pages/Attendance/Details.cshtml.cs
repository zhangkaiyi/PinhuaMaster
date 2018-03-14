using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PinhuaMaster.Data.Entities.Pinhua;
namespace PinhuaMaster.Pages.Attendance
{
    public class DetailsModel : PageModel
    {
        private PinhuaContext _pinhuaContext { get; set; }
        public DetailsModel(PinhuaContext pinhuaContext)
        {
            _pinhuaContext = pinhuaContext;
        }

        public class LinqModel
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public string Name { get; set; }
            public IEnumerable<考勤明细> Details { get; set; }
        }

        public LinqModel AttendanceData { get; set; }

        public void OnGet(int Year, int Month, string Id)
        {
            AttendanceData = (from p in _pinhuaContext.考勤期间.Where(k => k.年 == Year && k.月 == Month)
                              join d in _pinhuaContext.考勤明细.Where(m => m.人员编号 == Id) on p.ExcelServerRcid equals d.ExcelServerRcid into details
                              select new LinqModel
                              {
                                  Year = p.年,
                                  Month = p.月,
                                  Name = details.FirstOrDefault().姓名,
                                  Details = details
                              }).FirstOrDefault();
        }
    }
}