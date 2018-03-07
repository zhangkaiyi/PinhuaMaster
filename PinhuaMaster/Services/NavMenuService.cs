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

        private string _filename { get; set; } = "NavbarMenus.json";

        public IList<NavbarMenu> GetNavbarMenus()
        {
            using (var fs = new FileStream(_filename, FileMode.Open))
            using (var sr = new StreamReader(fs, System.Text.Encoding.Default))
            {
                var jsonString = sr.ReadToEnd();
                return Newtonsoft.Json.Linq.JObject.Parse(jsonString)["NavbarMenus"].ToObject<List<NavbarMenu>>();
            }
        }

        public void UpdateNavbarMenus(string navbarMenus)
        {
            using (var fs = new FileStream(_filename, FileMode.Create))
            using (var sw = new StreamWriter(fs, System.Text.Encoding.Default))
            {
                sw.Write(navbarMenus);
            }
        }

        public IList<string> GetPathName(string path)
        {
            var menus = GetNavbarMenus();
            var succeeded = false;
            var pathName = new List<string>();
            BuildPathName(path, menus, ref pathName, ref succeeded);
            return pathName;
        }

        private void BuildPathName(string path, IList<NavbarMenu> menus, ref List<string> pathName, ref bool succeeded)
        {
            foreach (var menu in menus)
            {
                if (menu.children == null)
                {
                    if (menu.url == path)
                    {
                        pathName.Add(menu.name);  // 找到匹配菜单，跳出循环
                        succeeded = true;
                        break;
                    }
                    if (menu.url != path)
                    {
                        if (menus.IndexOf(menu) == menus.Count - 1) // 找不到匹配菜单，清空列表，找下一个父节点
                            pathName.Clear();
                        else
                            continue;   // 找不到，继续循环
                    }
                }
                if (menu.children != null && menu.children.Any())
                {
                    pathName.Add(menu.name);
                    BuildPathName(path, menu.children, ref pathName, ref succeeded);    // 递归查找子节点
                    if (succeeded)  // 找到匹配菜单，就跳出循环，不再尝试匹配后面的节点
                        break;
                }
            }

        }
    }
}