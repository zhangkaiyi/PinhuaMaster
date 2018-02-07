using Microsoft.EntityFrameworkCore;
using PinhuaMaster.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PinhuaMaster.Services
{
    /// <summary>
    /// 导航菜单项
    /// </summary>
    public class NavMenu
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public MenuTypes MenuType { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public bool IsOpen { get; set; }

        /// <summary>
        /// 子菜单
        /// </summary>
        public IList<NavMenu> SubNavMenus = new List<NavMenu>();
    }

    /// <summary>
    /// 左侧导航菜单视图模型
    /// </summary>
    public class NavMenuVM
    {
        public IList<NavMenu> NavMenus { get; set; }
    }

    public class NavbarMenu
    {
        public string name { get; set; }
        public string icon { get; set; }
        public string url { get; set; }

        public IList<NavbarMenu> children { get; set; }
    }

    /// <summary>
    /// 菜单服务
    /// </summary>
    public class NavMenuService : INavMenuService
    {
        private readonly ApplicationDbContext _context;
        public NavMenuService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static IList<NavMenu> NavMenus { get; set; }

        /// <summary>
        /// 获取导航菜单
        /// </summary>
        /// <returns></returns>
        public IList<NavMenu> GetNavMenus()
        {
            if (NavMenus == null)
                InitOrUpdate();

            return NavMenus;
        }
        /// <summary>
        /// 生成导航菜单
        /// </summary>
        /// <returns></returns>
        public void InitOrUpdate()
        {
            NavMenus = new List<NavMenu>();

            var rootMenus = _context.Menus
                .Where(s => string.IsNullOrEmpty(s.ParentId))
                .AsNoTracking()
                .OrderBy(s => s.IndexCode)
                .ToList();

            foreach (var rootMenu in rootMenus)
            {
                NavMenus.Add(GetOneNavMenu(rootMenu));
            }
        }
        /// <summary>
        /// 根据给定的Menu，生成对应的导航菜单
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public NavMenu GetOneNavMenu(Menu menu)
        {
            //构建菜单项
            var navMenu = new NavMenu
            {
                Id = menu.Id,
                Name = menu.Name,
                MenuType = menu.MenuType.Value,
                Url = menu.Url,
                Icon = menu.Icon
            };

            //构建子菜单
            var subMenus = _context.Menus
                .Where(s => s.ParentId == menu.Id)
                .AsNoTracking()
                .OrderBy(s => s.IndexCode)
                .ToList();

            foreach (var subMenu in subMenus)
            {
                navMenu.SubNavMenus.Add(GetOneNavMenu(subMenu));
            }

            return navMenu;
        }

        public IList<NavbarMenu> GetNavbarMenus()
        {
            var file_path = "NavbarMenus.json";
            using (var fs = new FileStream(file_path, FileMode.Open))
            using (var sr = new StreamReader(fs, System.Text.Encoding.Default))
            {
                var jsonString = sr.ReadToEnd();
                return Newtonsoft.Json.Linq.JObject.Parse(jsonString)["NavbarMenus"].ToObject<List<NavbarMenu>>();
            }
        }

        public void UpdateNavbarMenus(string navbarMenus)
        {
            var file_path = "NavbarMenus.json";

            using (var fs = new FileStream(file_path, FileMode.Create))
            using (var sw = new StreamWriter(fs, System.Text.Encoding.Default))
            {
                sw.Write(navbarMenus);
            }
        }
    }
}