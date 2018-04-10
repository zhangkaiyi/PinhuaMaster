using Microsoft.EntityFrameworkCore;
using PinhuaMaster.Data.Entities.Pinhua;
using PinhuaMaster.Data.Entities.EastRiver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PinhuaMaster.Extensions;

namespace PinhuaMaster.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly PinhuaContext _pinhuaContext;
        private readonly EastRiverContext _eastRiverContext;
        public AttendanceService(PinhuaContext pinhuaContext, EastRiverContext eastRiverContext)
        {
            _pinhuaContext = pinhuaContext;
            _eastRiverContext = eastRiverContext;
        }

        public IList<AttendanceTimeRecords> GetTimeRecrods(int Y, int M)
        {
            var bret = GetRuleTimeRange(Y, M, out var earliest, out var latest);
            if (!bret)
                return null;

            var eastriver = _eastRiverContext.TimeRecords.AsNoTracking().Where(p => p.SignTime.Year == Y && p.SignTime.Month == M).ToList();
            // 正常打卡
            var records1 = (from f in _pinhuaContext.人员档案.AsNoTracking().ToList()
                            join c in _pinhuaContext.考勤卡号变动.AsNoTracking().ToList() on f.ExcelServerRcid equals c.ExcelServerRcid
                            join r in eastriver on c.卡号 equals r.CardId
                            where r.SignTime.IsBetween(earliest, latest)
                            select new AttendanceTimeRecords
                            {
                                Id = f.人员编号,
                                Name = f.姓名,
                                Source = "考勤机",
                                SignTime = r.SignTime
                            }).ToList();

            // 勤哲补卡
            var records2 = (from f in _pinhuaContext.人员档案.AsNoTracking().ToList()
                            join r in _pinhuaContext.打卡登记.AsNoTracking() on f.人员编号 equals r.人员编号
                            where r.时间.Value.IsBetween(earliest, latest)
                            select new AttendanceTimeRecords
                            {
                                Id = f.人员编号,
                                Name = f.姓名,
                                Source = "勤哲",
                                SignTime = r.时间
                            }).ToList();

            // 异常处理，漏打补打
            //var records2 = from p in _pinhuaContext.Wx异常说明.AsNoTracking()
            //               where p.时间.Value.IsBetween(earliest, latest) && p.是否处理 == 1
            //               select new DTO
            //               {
            //                   Source = "异常处理",
            //                   SignTime = p.时间.Value,
            //                   Name = p.姓名,
            //                   Id = p.用户号,
            //               };
            //var result = records1.Union(records2).OrderBy(p => p.SignTime).ToList();

            //return result;
            return records1.Union(records2).ToList();
        }

        public AttendanceServiceDTO GetAttendanceData(int? Y, int? M)
        {
            if (Y == null || M == null)
                return null;

            var records = GetTimeRecrods(Y.Value, M.Value).OrderBy(p => p.Id).ThenBy(p => p.SignTime);
            if (records.Count() == 0)
                return null;

            var recordsGroupBy = from p in records
                                 group p by new { p.Id, p.Name } into g
                                 select new
                                 {
                                     g.Key.Id,
                                     g.Key.Name,
                                     SignTimes = g.Select(p => p.SignTime).ToList()
                                 };
            var rule = GetRule();
            var days = DateTime.DaysInMonth(Y.Value, M.Value);
            var dto = new AttendanceServiceDTO
            {
                Y = Y,
                M = M,
                Rule = rule,
                PersonList = new List<AttendanceServicePerson>()
            };
            foreach (var record in recordsGroupBy)
            {
                var person = new AttendanceServicePerson
                {
                    Id = record.Id,
                    Name = record.Name,
                    Results = new List<AttendanceServiceDayResult>()
                };
                for (int i = 0; i != days; i++)
                {
                    var D = i + 1;

                    var result = new AttendanceServiceDayResult
                    {
                        Date = new DateTime(Y.Value, M.Value, D),
                        Details = new List<AttendanceServiceDayDetail>()
                    };

                    foreach (var range in rule.Ranges)
                    {
                        (var workfrom, var workto) = 指定日期的班段起止(range, new DateTime(Y.Value, M.Value, D));

                        (var clockinfrom, var clockinto) = 指定日期的签到起止(range, new DateTime(Y.Value, M.Value, D));
                        var checkin = record.SignTimes.Where(p => p.Value.IsBetween(clockinfrom, clockinto)).OrderBy(p => p).FirstOrDefault();
                        record.SignTimes.Remove(checkin);
                        (var clockoutfrom, var clockoutto) = 指定日期的签退起止(range, new DateTime(Y.Value, M.Value, D));
                        IEnumerable<DateTime?> set = null;
                        if (checkin.HasValue)
                            set = record.SignTimes.Where(p => p.Value.IsBetween(clockoutfrom, clockoutto) && p.Value.Subtract(checkin ?? DateTime.MaxValue).TotalHours >= 1).OrderBy(p => p);
                        else
                            set = record.SignTimes.Where(p => p.Value.IsBetween(clockoutfrom, clockoutto)).OrderBy(p => p);
                        DateTime? checkout = null;
                        if (set.Count() > 0)
                        {
                            checkout = set.FirstOrDefault();
                            record.SignTimes.Remove(checkout);

                        }
                        var detail = new AttendanceServiceDayDetail
                        {
                            Range = range.ToRangeString(),
                            Time1 = checkin,
                            Time1Fix = (checkin < workfrom && checkin.HasValue) ? workfrom : checkin,
                            Time2 = checkout,
                            Time2Fix = checkout > workto ? (range.延迟算加班 == "是" ? checkout : workto) : checkout,
                        };

                        if (checkin.HasValue && checkout.HasValue)
                        {
                            detail.State = "正常";

                            var workSpan = detail.Time2Fix.Value.DropSeconds().Subtract(detail.Time1Fix.Value.DropSeconds());
                            //var minutes = workSpan.TotalMinutes > 0 ? Math.Floor(workSpan.TotalMinutes) : 0;
                            var restSpan = new TimeSpan();
                            if (range.中途休息 == 1)
                            {
                                var restfrom = range.休息开始时间.Value.ConvertToTargetDate(new DateTime(Y.Value, M.Value, D));
                                var restto = range.休息结束时间.Value.ConvertToTargetDate(new DateTime(Y.Value, M.Value, D));
                                restSpan = GetOverlapSpan(detail.Time1Fix.Value, detail.Time2Fix.Value, restfrom, restto);
                            }
                            workSpan = workSpan - restSpan;
                            var hours = Math.Floor(workSpan.TotalHours * 10) / 10;
                            detail.Hours = (decimal)hours;
                            result.Details.Add(detail);
                        }
                        else
                        {
                            if (range.计算全勤 == "是")
                            {
                                detail.State = "缺勤";
                            }
                            result.Details.Add(detail);
                        }
                    }
                    result.DayHours = result.Details.Sum(p => p.Hours);
                    result.State = GetState(result.Details);
                    person.Results.Add(result);
                }
                person.TotalHours = person.Results.Sum(p => p.DayHours);
                person.IsFullAttendance = IsFullWorkAttendance(person.Results);
                int times = 0;
                foreach (var x in person.Results)
                {
                    foreach (var y in x.Details)
                    {
                        if (y.Time1 != null && y.Time2 != null)
                            times++;
                    }
                }
                person.DaytimeHours = person.Results.Sum(p => p.DayHours > 9 ? 9 : p.DayHours);
                person.OvertimeHours = person.Results.Sum(p => p.DayHours > 9 ? p.DayHours - 9 : 0);
                person.TimesOfDinner = times;
                dto.PersonList.Add(person);
            }
            dto.TotalHours = dto.PersonList.Sum(p => p.TotalHours);
            return dto;
        }

        /// <summary>
        /// 获取打卡规则
        /// </summary>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        public AttendanceRule GetRule(int ruleId = 1)
        {
            var rule = _pinhuaContext.WeixinWorkPlan.AsNoTracking().FirstOrDefault(p => p.Id == ruleId);
            if (rule == null) return null;

            var dto = new AttendanceRule
            {
                Rule = rule,
            };
            var ranges = _pinhuaContext.WeixinWorkPlanDetail.AsNoTracking().Where(p => p.ExcelServerRcid == rule.ExcelServerRcid);
            dto.Ranges = ranges.ToList();
            return dto;
        }

        public bool GetRuleTimeRange(int Y, int M, out DateTime from, out DateTime to)
        {
            var first = new DateTime(Y, M, 1);
            var last = first.AddMonths(1).AddDays(-1);
            from = DateTime.MaxValue;
            to = DateTime.MinValue;

            var rule = GetRule();
            if (rule == null || rule.Ranges.Count == 0)
                return false;

            foreach (var range in rule.Ranges)
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


        private string GetState(IEnumerable<AttendanceServiceDayDetail> details)
        {
            var ret = "正常";
            foreach (var detail in details)
            {
                if (detail.State == "缺勤")
                {
                    ret = "缺勤";
                    break;
                }
            }
            return ret;
        }

        private bool IsEveryDatetimeNotNull(WeixinWorkPlanDetail item)
        {
            if (item == null)
                return false;
            if (item.MoveUp.HasValue && item.Beginning.HasValue && item.Ending.HasValue && item.PutOff.HasValue)
                return true;
            else
                return false;
        }

        private bool IsFullWorkAttendance(IEnumerable<AttendanceServiceDayResult> results)
        {
            return !results.Any(p => p.State == "缺勤");
        }

        public (DateTime? begin, DateTime? end) 指定日期的班段起止(WeixinWorkPlanDetail item, DateTime target)
        {
            var now = DateTime.Now;
            if (!IsEveryDatetimeNotNull(item))
                return (null, null);

            var begin = item.Beginning.Value.ConvertToTargetDate(target);
            var end = item.Ending.Value.ConvertToTargetDate(target);
            return (begin, end);
        }

        public (DateTime? begin, DateTime? end) 指定日期的签到起止(WeixinWorkPlanDetail item, DateTime target)
        {
            var now = DateTime.Now;
            if (!IsEveryDatetimeNotNull(item))
                return (null, null);

            var begin = item.Beginning.Value.ConvertToTargetDate(target).AddMinutes(-item.MoveUp.Value);
            var end = item.Ending.Value.ConvertToTargetDate(target);

            return (begin, end);
        }

        public (DateTime? begin, DateTime? end) 指定日期的签退起止(WeixinWorkPlanDetail item, DateTime target)
        {
            var now = DateTime.Now;
            if (!IsEveryDatetimeNotNull(item))
                return (null, null);

            var begin = item.Beginning.Value.ConvertToTargetDate(target);
            var end = item.Ending.Value.ConvertToTargetDate(target).AddMinutes(item.PutOff.Value);

            return (begin, end);
        }

        public TimeSpan GetOverlapSpan(DateTime range1Begin, DateTime range1End, DateTime range2Begin, DateTime range2End)
        {
            var span = new TimeSpan();
            if (range1Begin < range2Begin && range2End < range1End)
            {
                span = range2End.Subtract(range2Begin);
            }
            else if (range1Begin < range2Begin && range1End < range2End)
            {
                span = range1End.Subtract(range2Begin);
            }
            else if (range1Begin > range2Begin && range1Begin < range2End && range1End > range2End)
            {
                span = range2End.Subtract(range1Begin);
            }

            return span;
        }
    }

    public class AttendanceRule
    {
        public WeixinWorkPlan Rule { get; set; }
        public IList<WeixinWorkPlanDetail> Ranges { get; set; }
    }
    public class AttendanceTimeRecords
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public DateTime? SignTime { get; set; }
    }

    public class AttendanceServiceDTO
    {
        public int? Y { get; set; }
        public int? M { get; set; }
        public decimal? TotalHours { get; set; }
        public AttendanceRule Rule { get; set; }
        public IList<AttendanceServicePerson> PersonList { get; set; }
    }

    public class AttendanceServicePerson
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal? DaytimeHours { get; set; }
        public decimal? OvertimeHours { get; set; }
        public decimal? TotalHours { get; set; }
        public bool IsFullAttendance { get; set; }
        public int? TimesOfAbsent { get; set; }         //缺勤
        public int? TimesOfLate { get; set; }           //迟到
        public int? TimesOfLeaveEarly { get; set; }     //早退
        public int? TimesOfAskForLeave { get; set; }    //请假
        public int? TimesOfDinner { get; set; }         //用餐
        public IList<AttendanceServiceDayResult> Results { get; set; }
    }

    public class AttendanceServiceDayResult
    {
        public DateTime Date { get; set; }
        public decimal? DayHours { get; set; }
        public IList<AttendanceServiceDayDetail> Details { get; set; }
        public string State { get; set; }
    }

    public class AttendanceServiceDayDetail
    {
        public string Range { get; set; }
        public DateTime? Time1 { get; set; }
        public DateTime? Time1Fix { get; set; }
        public DateTime? Time2 { get; set; }
        public DateTime? Time2Fix { get; set; }
        public decimal? Hours { get; set; }
        public string State { get; set; }
    }
}
