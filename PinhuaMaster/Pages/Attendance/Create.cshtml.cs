using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PinhuaMaster.Data.Entities.Pinhua;
using PinhuaMaster.Data.Entities.EastRiver;
using PinhuaMaster.Extensions;
using Microsoft.EntityFrameworkCore;
using PinhuaMaster.Data.Entities;

namespace PinhuaMaster.Pages.Attendance
{
    public class CreateModel : PageModel
    {
        private readonly PinhuaContext _pinhuaContext;
        private readonly EastRiverContext _eastRiverContext;

        public CreateModel(PinhuaContext pinhuaContext, EastRiverContext eastRiverContext)
        {
            _pinhuaContext = pinhuaContext;
            _eastRiverContext = eastRiverContext;
        }

        public void OnGet()
        {

        }


        /// <summary>
        /// 获取打卡规则
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public List<WeixinWorkPlanDetail> GetAttendancePlan(int planId = 1)
        {
            var plan = _pinhuaContext.WeixinWorkPlan.AsNoTracking().FirstOrDefault(p => p.Id == planId);
            if (plan == null)
                return null;
            var ranges = _pinhuaContext.WeixinWorkPlanDetail.AsNoTracking().Where(p => p.ExcelServerRcid == plan.ExcelServerRcid);
            return ranges.ToList();
        }

        public bool GetTargetMonthClockRangesBorder(int year, int month, out DateTime from, out DateTime to)
        {
            var first = new DateTime(year, month, 1);
            var last = first.AddMonths(1).AddDays(-1);
            from = DateTime.MaxValue;
            to = DateTime.MinValue;

            var ranges = GetAttendancePlan();
            if (ranges == null)
                return false;

            foreach (var range in ranges)
            {
                var t1 = range.Beginning.Value.ConvertToTargetDate(first).AddMinutes(-range.MoveUp.Value);
                if (t1 < from)
                    from = t1;
                var t2 = range.Ending.Value.ConvertToTargetDate(last).AddMinutes(range.PutOff.Value);
                if (t2 > to)
                    to = t2;
            }

            return true;
        }

        public List<DTO> GetTargetMonthClockRecords(int year, int month)
        {
            var bret = _pinhuaContext.GetAttendanceTimeRange(year, month, out var earliest, out var latest);
            if (!bret)
                return null;

            var eastriver = _eastRiverContext.TimeRecords.AsNoTracking().Where(p => p.SignTime.Year == year && p.SignTime.Month == month).ToList();
            // 正常打卡
            var records1 = (from f in _pinhuaContext.人员档案.AsNoTracking()
                            join c in _pinhuaContext.考勤卡号变动.AsNoTracking() on f.ExcelServerRcid equals c.ExcelServerRcid
                            join r in eastriver on c.卡号 equals r.CardId
                            where r.SignTime.IsBetween(earliest, latest)
                            select new DTO
                            {
                                Id = f.人员编号,
                                Name = f.姓名,
                                Source = "考勤机",
                                SignTime = r.SignTime
                            });
            // 异常处理，漏打补打
            var records2 = from p in _pinhuaContext.Wx异常说明.AsNoTracking()
                           where p.时间.Value.IsBetween(earliest, latest) && p.是否处理 == 1
                           select new DTO
                           {
                               Source = "异常处理",
                               SignTime = p.时间.Value,
                               Name = p.姓名,
                               Id = p.用户号,
                           };
            var result = records1.Union(records2).OrderBy(p => p.SignTime).ToList();

            return result;
        }
    }

    public class DTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public DateTime SignTime { get; set; }
    }

    public class Person
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Result> Results { get; set; }
    }

    public class Result
    {
        public DateTime Date { get; set; }
        public decimal TotalHours { get; set; }
        public List<ResultDetail> Details { get; set; }
    }

    public class ResultDetail
    {
        public string Range { get; set; }
        public string Time1 { get; set; }
        public string Time1fix { get; set; }
        public string Time2 { get; set; }
        public string Time2fix { get; set; }
        public decimal Hours { get; set; }
        public string State { get; set; }
    }
}