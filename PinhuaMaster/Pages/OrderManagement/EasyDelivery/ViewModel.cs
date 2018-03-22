﻿using AutoMapper;
using PinhuaMaster.Data.Entities.Pinhua;
using PinhuaMaster.Extensions.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PinhuaMaster.Pages.OrderManagement.EasyDelivery.ViewModel
{
    public class Gi2ViewModel
    {
        public Gi2MainDTO Main { get; set; }
        public List<Gi2DetaislDTO> Details { get; set; }
    }

    public class Gi2MainDTO
    {
        [Required, Display(Name = "单号")]
        public string DeliveryId { get; set; }
        [Required, Display(Name = "客户单位")]
        public string CustomerId { get; set; }
        [Display(Name = "客户单位名称")]
        public string CustomerName { get; set; }
        [Required, Display(Name = "业务类型")]
        public string DeliveryType { get; set; }
        [Display(Name = "业务描述")]
        public string DeliveryTypeDescription { get; set; }
        [Required, Display(Name = "日期")]
        [Newtonsoft.Json.JsonConverter(typeof(MyDateConverter))]
        public DateTime? DeliveryDate { get; set; }
        [Display(Name = "地址")]
        public string DeliveryAddress { get; set; }
        [Display(Name = "金额")]
        public decimal? Amount { get; set; }
        [Display(Name = "备注")]
        public string Remarks { get; set; }
        [Display(Name = "联系人")]
        public string Contact { get; set; }
        [Display(Name = "联系电话")]
        public string ContactNumber { get; set; }
        [Display(Name = "制单人")]
        public string CreatedBy { get; set; }
        [Display(Name = "制单日期")]
        [Newtonsoft.Json.JsonConverter(typeof(MyDateTimeConverter))]
        public DateTime? CreatedDate { get; set; }
        [Display(Name = "关联号")]
        public string ExcelServerRcid { get; set; }
        [Display(Name = "模板号")]
        public string ExcelServerRtid { get; set; }
        //public int? ExcelServerRn { get; set; }
        //public int? ExcelServerCn { get; set; }
        //public string ExcelServerRc1 { get; set; }
        //public string ExcelServerWiid { get; set; }
        //public int? ExcelServerChg { get; set; }
    }

    public class Gi2DetaislDTO
    {
        public string Description { get; set; }
        public string Specification { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Qty { get; set; }
        public decimal? UnitQty { get; set; }
        public string Unit { get; set; }
        public decimal? Price { get; set; }
        public decimal? Amount { get; set; }
        public int Id { get; set; }
        public string Remarks { get; set; }
        public string DeliveryId { get; set; }
        public string ExcelServerRcid { get; set; }
        public string ExcelServerRtid { get; set; }
        //public int? ExcelServerRn { get; set; }
        //public int? ExcelServerCn { get; set; }
        //public string ExcelServerRc1 { get; set; }
        //public string ExcelServerWiid { get; set; }
        //public int? ExcelServerChg { get; set; }
    }

    public class Gi2Profile : Profile
    {
        public Gi2Profile()
        {
            CreateMap<Gi2Main, Gi2MainDTO>();
            //.ForMember(dst => dst.ExcelServerRcid, map => map.MapFrom(src => src.ExcelServerRcid))
            //.ForMember(dst => dst.DeliveryDate, map => map.MapFrom(src => src.DeliveryDate.Value.ToString("yyyy-MM-dd")));
            CreateMap<Gi2MainDTO, Gi2Main>();
            //.ForMember(dst => dst.ExcelServerRcid, map => map.MapFrom(src => src.ExcelServerRcid))
            //.ForMember(dst => dst.DeliveryDate, map => map.MapFrom(src => src.DeliveryDate));

            CreateMap<Gi2Details, Gi2DetaislDTO>();
            //.ForMember(dst => dst.DeliveryId, map => map.MapFrom(src => src.Id.ToString()));
            CreateMap<Gi2DetaislDTO, Gi2Details>();
            //.ForMember(dst => dst.Id, map => map.MapFrom(src => int.Parse(src.Index)));
        }
    }
}
