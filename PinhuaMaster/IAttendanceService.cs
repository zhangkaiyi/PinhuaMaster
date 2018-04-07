using System;
using System.Collections.Generic;
using PinhuaMaster.Data.Entities.Pinhua;

namespace PinhuaMaster.Services
{
    public interface IAttendanceService
    {
        AttendanceServiceDTO GetAttendanceData(int Y, int M);
        AttendanceRule GetRule(int planId = 1);
        bool GetRuleTimeRange(int year, int month, out DateTime from, out DateTime to);
        TimeSpan GetOverlapSpan(DateTime range1Begin, DateTime range1End, DateTime range2Begin, DateTime range2End);
        IList<AttendanceTimeRecords> GetTimeRecrods(int year, int month);
        (DateTime? begin, DateTime? end) 指定日期的班段起止(WeixinWorkPlanDetail item, DateTime target);
        (DateTime? begin, DateTime? end) 指定日期的签到起止(WeixinWorkPlanDetail item, DateTime target);
        (DateTime? begin, DateTime? end) 指定日期的签退起止(WeixinWorkPlanDetail item, DateTime target);
    }
}