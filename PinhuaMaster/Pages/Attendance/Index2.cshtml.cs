using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PinhuaMaster.Data.Entities.Pinhua;
using PinhuaMaster.Data.Entities.EastRiver;
using Microsoft.EntityFrameworkCore;
using PinhuaMaster.Extensions.TagHelpers;

namespace PinhuaMaster.Pages.Attendance
{
    public class Index2Model : PageModel
    {
        private PinhuaContext _pinhuaContext { get; set; }
        private EastRiverContext _eastRiverContext { get; set; }
        public Index2Model(PinhuaContext pinhuaContext, EastRiverContext eastRiverContext)
        {
            _pinhuaContext = pinhuaContext;
            _eastRiverContext = eastRiverContext;
        }

        public IActionResult OnGet(int? year, int? month, int pageSize = 12, int pageIndex = 1)
        {
            if (!year.HasValue || !month.HasValue)
            {
                var latest = _pinhuaContext.考勤期间.OrderByDescending(p => p.年).ThenByDescending(p => p.月).FirstOrDefault();
                if (latest != null)
                    return RedirectToPage("Index", new { year = latest.年, month = latest.月 });
            }

            InitData(year.Value, month.Value);
            return Page();
        }

        public class AttendanceData
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public IEnumerable<AttendanceDataDetails> Data { get; set; }
        }

        public class AttendanceDataDetails
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public IEnumerable<PinhuaMaster.Data.Entities.Pinhua.考勤明细> Details { get; set; }
        }

        public class EsPayrollData
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public IEnumerable<EsPayrollDataDetails> Data { get; set; }
        }

        public class EsPayrollDataDetails
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public IEnumerable<Test明细> Details { get; set; }
        }

        public class PayrollOption
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public int daysInMonth { get; set; }
            public IList<PayrollOptionDetails> Details { get; set; } = new List<PayrollOptionDetails>();
        }

        public class PayrollOptionDetails
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int DaysOfRest { get; set; }
            public int DaysOfWork { get; set; }
            public bool IsFullAttendance { get; set; }
            public decimal HoursOfDaytime { get; set; }
            public decimal HoursOfOvertime { get; set; }
            public decimal HoursOfAll { get; set; }
            public int Meals { get; set; }
            public decimal PriceOfDaytime { get; set; }
            public decimal PriceOfOvertime { get; set; }
            public decimal NewPayroll { get; set; }
            public decimal OldPayroll { get; set; }
        }

        public AttendanceData attendanceData { get; set; }
        public EsPayrollData esPayrollData { get; set; }
        public PayrollOption payrollOption { get; set; }

        private void InitData(int year, int month)
        {
            attendanceData = (from p in _pinhuaContext.考勤期间.Where(x => x.年 == year && x.月 == month)
                              join d in _pinhuaContext.考勤明细 on p.ExcelServerRcid equals d.ExcelServerRcid into details
                              select new AttendanceData
                              {
                                  Year = p.年,
                                  Month = p.月,
                                  Data = from p2 in details
                                         orderby p2.人员编号 ascending
                                         group p2 by new { p2.人员编号, p2.姓名 } into g
                                         select new AttendanceDataDetails
                                         {
                                             Id = g.Key.人员编号,
                                             Name = g.Key.姓名,
                                             Details = g.Select(p3 => p3)
                                         }
                              }).FirstOrDefault();

            esPayrollData = (from p in _pinhuaContext.Test主表.Where(p => p.年份 == year && p.月份 == month)
                             join pd in _pinhuaContext.Test明细 on p.ExcelServerRcid equals pd.ExcelServerRcid into details
                             select new EsPayrollData
                             {
                                 Year = p.年份,
                                 Month = p.月份,
                                 Data = from p2 in details
                                        group p2 by new { p2.编号, p2.姓名 } into g
                                        select new EsPayrollDataDetails
                                        {
                                            Id = g.Key.编号,
                                            Name = g.Key.姓名,
                                            Details = g.Select(p3 => p3)
                                        }
                             }).FirstOrDefault();

            payrollOption = new PayrollOption
            {
                Year = year,
                Month = month,
                daysInMonth = DateTime.DaysInMonth(year, month)
            };
            foreach (var d in attendanceData.Data)
            {
                var x = from p in d.Details
                        let All = (p.正常出勤 ?? 0) + (p.加班 ?? 0)
                        let Hours = All >= 9 ? 9 : All
                        let Extra = All >= 9 ? All - 9 : 0
                        select new
                        {
                            Data = p,
                            Hours,
                            Extra,
                            All
                        };
                var daysInMonth = payrollOption.daysInMonth;
                var restDays = x.Count(p => p.Data.状态 == "异常") + x.Count(p => p.Data.状态 == "请假");
                var workDays = x.Count(p => p.Data.状态 == "正常");
                var isFullWork = restDays == 0 ? true : false;
                var monthlyHours = x.Sum(p => p.Hours);
                var monthlyExtra = x.Sum(p => p.Extra);
                var monthlyAll = x.Sum(p => p.All);
                var monthlyEat = x.Count(p => !string.IsNullOrEmpty(p.Data.上班1)) + x.Count(p => !string.IsNullOrEmpty(p.Data.上班3));
                const decimal 标准白天工价 = 11.2M;
                const decimal 标准加班工价 = 13;
                const decimal 全勤奖励 = 0.8M;
                const decimal 女工白天工价 = 标准白天工价;
                const decimal 男工白天工价 = 女工白天工价 + 0.5M;
                const decimal 女工加班工价 = 标准加班工价;
                const decimal 男工加班工价 = 女工加班工价 + 2;
                var sex = _pinhuaContext.人员档案.FirstOrDefault(p => p.人员编号 == d.Id);
                var newPrice = (decimal)(isFullWork 
                    ? (sex.性别 == "男" ? 男工白天工价 + 全勤奖励 : 女工白天工价 + 全勤奖励) 
                    : (sex.性别 == "男" ? 男工白天工价 : 女工白天工价));
                var newExtraPrice = sex.性别 == "男" ? 男工加班工价 : 女工加班工价;
                var newPayroll = monthlyHours * newPrice + monthlyExtra * newExtraPrice;
                var newPayrollWithoutEat = monthlyHours * newPrice + monthlyExtra * newExtraPrice - monthlyEat * 2;

                var oldPayroll = (monthlyAll - daysInMonth * 9 > 0 ? monthlyAll - daysInMonth * 9 : 0) * 13
                + (monthlyAll > daysInMonth * 9 ? 2900 : 2900 / daysInMonth / 9 * monthlyAll)
                + (isFullWork ? 100 / daysInMonth * (workDays + restDays) : 0)
                - monthlyEat * 2;

                payrollOption.Details.Add(new PayrollOptionDetails
                {
                    Id = d.Id,
                    Name = d.Name,
                    DaysOfRest = restDays,
                    DaysOfWork = workDays,
                    IsFullAttendance = isFullWork,
                    HoursOfDaytime = monthlyHours,
                    HoursOfOvertime = monthlyExtra,
                    HoursOfAll = monthlyAll,
                    Meals = monthlyEat,
                    PriceOfDaytime = newPrice,
                    PriceOfOvertime = newExtraPrice,
                    NewPayroll = newPayroll,
                    OldPayroll = oldPayroll + monthlyEat * 2
                });
            }
        }
    }
}