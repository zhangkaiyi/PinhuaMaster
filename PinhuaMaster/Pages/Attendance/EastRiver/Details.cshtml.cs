using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PinhuaMaster.Data.Entities.EastRiver;
using PinhuaMaster.Data.Entities.Pinhua;

namespace PinhuaMaster.Pages.Attendance.EastRiver
{
    public class DetailsModel : PageModel
    {
        private readonly EastRiverContext _eastRiverContext;
        private readonly PinhuaContext _pinhuaContext;

        public DetailsModel(EastRiverContext eastRiverContext, PinhuaContext pinhuaContext)
        {
            _eastRiverContext = eastRiverContext;
            _pinhuaContext = pinhuaContext;
        }

        public IEnumerable<TimeRecords> timeRecords { get; set; }

        public void OnGet(int? year, int? month, string id)
        {
            var file = _pinhuaContext.人员档案.AsNoTracking().FirstOrDefault(p => p.人员编号 == id);
            var cards = (from c in _pinhuaContext.考勤卡号变动
                        where c.ExcelServerRcid == file.ExcelServerRcid
                        select c).ToList();

            timeRecords = from p in _eastRiverContext.TimeRecords
                          join c in cards on p.CardId equals c.卡号
                          where p.SignTime.Year == year && p.SignTime.Month == month
                          select p;
        }
    }
}