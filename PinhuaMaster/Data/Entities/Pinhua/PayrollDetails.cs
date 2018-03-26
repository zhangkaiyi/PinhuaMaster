using System;
using System.Collections.Generic;

namespace PinhuaMaster.Data.Entities.Pinhua
{
    public partial class PayrollDetails
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal? AttendanceHours { get; set; }
        public decimal? DaytimeHours { get; set; }
        public decimal? OvertimeHours { get; set; }
        public string FullAttendance { get; set; }
        public decimal? Amount { get; set; }
        public int Y { get; set; }
        public int M { get; set; }
        public string ExcelServerRcid { get; set; }
        public int? ExcelServerRn { get; set; }
        public int? ExcelServerCn { get; set; }
        public string ExcelServerRc1 { get; set; }
        public string ExcelServerWiid { get; set; }
        public string ExcelServerRtid { get; set; }
        public int? ExcelServerChg { get; set; }
    }
}
