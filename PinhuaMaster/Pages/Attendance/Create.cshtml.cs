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
using PinhuaMaster.Services;
using AutoMapper;

namespace PinhuaMaster.Pages.Attendance
{
    public class CreateModel : PageModel
    {
        private readonly PinhuaContext _pinhuaContext;
        private readonly EastRiverContext _eastRiverContext;
        private readonly IAttendanceService _attendanceService;
        private readonly IMapper _mapper;

        public CreateModel(PinhuaContext pinhuaContext, EastRiverContext eastRiverContext, IAttendanceService attendanceService, IMapper mapper)
        {
            _pinhuaContext = pinhuaContext;
            _eastRiverContext = eastRiverContext;
            _attendanceService = attendanceService;
            _mapper = mapper;
        }

        public int? Y { get; set; }
        public int? M { get; set; }

        public void OnGet(int? Y, int? M)
        {
            this.Y = Y;
            this.M = M;
        }

        public IActionResult OnGetAjax(int? Y, int? M)
        {
            var result = _attendanceService.GetAttendanceData(Y, M);

            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            //EF Core中默认为驼峰样式序列化处理key
            //settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            //使用默认方式，不更改元数据的key的大小写
            settings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();

            return new JsonResult(result, settings);
        }

        public IActionResult OnPost(string jsonStr)
        {
            if (string.IsNullOrEmpty(jsonStr))
                return Page();
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<AttendanceServiceDTO>(jsonStr);

            if (data == null)
                return Page();


            var Rcid = _pinhuaContext.GetNewRcId();
            var rtId = _pinhuaContext.GetRtId("AttendanceReport");
            var repCase = new EsRepCase
            {
                RcId = Rcid,
                RtId = rtId,
                LstFiller = 2,
                LstFillerName = User.Identity.Name,
                LstFillDate = DateTime.Now,
            };

            var reportMain = new AttendanceReport
            {
                ExcelServerRcid = Rcid,
                ExcelServerRtid = rtId,
                Y = data.Y.Value,
                M = data.M.Value
            };

            var reportDetails = new List<AttendanceReportDetails>();

            foreach (var person in data.PersonList)
            {
                var detail = new AttendanceReportDetails
                {
                    ExcelServerRcid = Rcid,
                    ExcelServerRtid = rtId,
                    Y = data.Y.Value,
                    M = data.M.Value,
                    编号 = person.Id,
                    姓名 = person.Name,
                    是否全勤 = person.IsFullAttendance ? "是" : "否",
                    正班 = person.DaytimeHours,
                    加班 = person.OvertimeHours,
                    总工时 = person.TotalHours,
                    缺勤 = person.TimesOfAbsent,
                    迟到 = person.TimesOfLate,
                    早退 = person.TimesOfLeaveEarly,
                    请假 = person.TimesOfAskForLeave,
                    用餐 = person.TimesOfDinner,
                };
                reportDetails.Add(detail);
            }

            _pinhuaContext.EsRepCase.Add(repCase);
            _pinhuaContext.AttendanceReport.Add(reportMain);
            _pinhuaContext.AttendanceReportDetails.AddRange(reportDetails);
            _pinhuaContext.SaveChanges();

            return RedirectToPage("Index");
        }
    }
}