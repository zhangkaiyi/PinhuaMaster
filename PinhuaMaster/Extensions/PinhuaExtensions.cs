using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PinhuaMaster.Data.Entities.Pinhua;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace PinhuaMaster.Extensions
{
    public static class PinhuaContextExtensions
    {
        /// <summary>
        /// 获取最大的Id
        /// </summary>
        /// <param name="context"></param>
        /// <param name="idName">要获取Id的代号</param>
        /// <param name="n">Id要增大的数</param>
        /// <returns></returns>
        public static int GetNewId(this PinhuaContext context, int idName, int n)
        {
            context.Database.ExecuteSqlCommand($"exec GetNewId_s @id, @n", new[]{
                 new SqlParameter("id", idName) ,new SqlParameter("n", n)
            });

            var obj = context.EsSysIdS
                .Where(p => p.IdName == idName && p.IdDate.Year == DateTime.Now.Year && p.IdDate.Month == DateTime.Now.Month && p.IdDate.Day == DateTime.Now.Day)
                .FirstOrDefault();

            return obj == null ? 0 : obj.MaxId;
        }

        /// <summary>
        /// 获取新的rcid文本
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetNewRcId(this PinhuaContext context)
        {
            var newId = context.GetNewId(26, 1);
            var rcId = "rc" + DateTime.Now.ToString("yyyyMMdd") + newId.ToString("D5");

            return newId == 0 ? "" : rcId;
        }

        /// <summary>
        /// 获取指定模板对应的RtId
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        static public string GetRtId(this PinhuaContext context, string templateName)
        {
                var rtId = from p in context.EsTmp
                           where p.RtName == templateName
                           select p.RtId;
                return rtId.First() ?? string.Empty;
        }

        /// <summary>
        /// 获取客户列表的下拉框数据
        /// </summary>
        /// <param name="_pinhuaContext"></param>
        /// <returns></returns>
        static public List<SelectListItem> GetCustomerSelectList(this PinhuaContext _pinhuaContext)
        {
            var customers = from p in _pinhuaContext.往来单位.ToList()
                            select p;

            var groups = from p in customers
                         orderby p.Rank descending
                         group p by p.Rank into g
                         select g.Key;
            var groupingCustomers = new List<SelectListItem>();
            foreach (var key in groups)
            {
                var optGroup = new SelectListGroup
                {
                    Name = key?.ToString()
                };
                foreach (var customer in customers)
                {
                    if (customer.Rank == key)
                    {
                        groupingCustomers.Add(new SelectListItem
                        {
                            Group = optGroup,
                            Text = customer.单位编号 + " - " + customer.单位名称,
                            Value = customer.单位编号
                        });
                    }
                }
            }
            return groupingCustomers;
        }

        /// <summary>
        /// 获取收款付款类型的下拉列表
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        static public List<SelectListItem> GetCollectionTypeSelectList(this PinhuaContext context)
        {
            var collectionTypes = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "银行"
                },
                new SelectListItem
                {
                    Text = "现金"
                },
                new SelectListItem
                {
                    Text = "微信"
                },
                new SelectListItem
                {
                    Text = "支付宝"
                },
            };
            return collectionTypes;
        }

        public static class Copy
        {
            public static void ShadowCopy(object from, object to)
            {
                if (from.GetType() == to.GetType())
                {
                    System.Reflection.PropertyInfo[] properties = from.GetType().GetProperties();

                    foreach (var p in properties)
                    {
                        var value = p.GetValue(from, null);
                        if (value != GetDefault(p.GetType()))
                            p.SetValue(to, value, null);
                    }
                }
            }

            public static object GetDefault(Type type)
            {
                if (type.IsValueType)
                {
                    return Activator.CreateInstance(type);
                }
                return null;
            }
        }
    }
}
