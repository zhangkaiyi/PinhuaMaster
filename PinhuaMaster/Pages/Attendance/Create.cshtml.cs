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
    }

    public class DTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public DateTime? SignTime { get; set; }
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
        public DateTime? Time1 { get; set; }
        public DateTime? Time1fix { get; set; }
        public DateTime? Time2 { get; set; }
        public DateTime? Time2fix { get; set; }
        public decimal? Hours { get; set; }
        public string State { get; set; }
    }


}