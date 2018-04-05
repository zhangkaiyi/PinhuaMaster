using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PinhuaMaster.Data.Entities.Pinhua;
using PinhuaMaster.Pages.Payroll.ViewModel;

namespace PinhuaMaster.Pages.Payroll
{
    public class DetailsModel : PageModel
    {
        private readonly PinhuaContext _pinhuaContext;
        private readonly IMapper _mapper;

        public DetailsModel(PinhuaContext pinhuaContext, IMapper mapper)
        {
            _pinhuaContext = pinhuaContext;
            _mapper = mapper;
        }

        public string RcId { get; set; }
        public int Y { get; set; }
        public int M { get; set; }

        public IEnumerable<PayrollDetailsDTO> PayrollList { get; set; }

        public void OnGet(int Y, int M)
        {
            this.Y = Y;
            this.M = M;
            RcId = _pinhuaContext.PayrollMain.FirstOrDefault(p => p.Y == Y && p.M == M)?.ExcelServerRcid;
            var list = _pinhuaContext.PayrollDetails.AsNoTracking().Where(p => p.Y == Y && p.M == M);
            PayrollList = _mapper.Map<IEnumerable<PayrollDetails>, IEnumerable<PayrollDetailsDTO>>(list);
        }

        public IActionResult OnGetAjaxGetDetails(int Y, int M)
        {
            var details = _pinhuaContext.PayrollDetails.AsNoTracking().Where(p => p.Y == Y && p.M == M);
            var dto = _mapper.Map<IEnumerable<PayrollDetails>, IEnumerable<PayrollDetailsDTO>>(details);
            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            //EF Core中默认为驼峰样式序列化处理key
            //settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            //使用默认方式，不更改元数据的key的大小写
            settings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();

            return new JsonResult(dto, settings);
        }
    }
}