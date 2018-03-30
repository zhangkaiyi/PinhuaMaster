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

        public IEnumerable<PayrollDetailsDTO> PayrollList { get; set; }

        public void OnGet(int Y, int M)
        {
            RcId = _pinhuaContext.PayrollMain.FirstOrDefault(p => p.Y == Y && p.M == M)?.ExcelServerRcid;
            var list = _pinhuaContext.PayrollDetails.AsNoTracking().Where(p => p.Y == Y && p.M == M);
            PayrollList = _mapper.Map<IEnumerable<PayrollDetails>, IEnumerable<PayrollDetailsDTO>>(list);
        }
    }
}