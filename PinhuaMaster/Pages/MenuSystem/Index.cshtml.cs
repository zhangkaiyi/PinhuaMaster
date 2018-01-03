using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PinhuaMaster.Data;
using PinhuaMaster.Extensions;

namespace PinhuaMaster.Pages.MenuSystem
{
    public class IndexModel : PageModel
    {
        public IndexModel(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ApplicationDbContext _dbContext { get; set; }

        public IEnumerable<Menu> _menus { get; set; }

        public void OnGet()
        {
            _menus = _dbContext.Menus.AsNoTracking();
        }

        /// <summary>
        /// 初始化下拉选择框
        /// </summary>
        /// <param name="menu"></param>
        private void UpdateDropDownList(Menu menu = null)
        {
            var menusParent = _dbContext.Menus.AsNoTracking().Where(s => s.MenuType == MenuTypes.导航菜单);
            List<SelectListItem> listMenusParent = new List<SelectListItem>();
            foreach (var menuParent in menusParent)
            {
                listMenusParent.Add(new SelectListItem
                {
                    Value = menuParent.Id,
                    Text = menuParent.Id + $"({menuParent.Name})",
                    Selected = (menu != null && menuParent.Id == menu.ParentId)
                });
            }
            ViewData["ParentIds"] = listMenusParent;

            if (menu == null)
            {
                ViewData["MenuTypes"] = MenuTypes.导航菜单.GetSelectListByEnum();
            }
            else
            {
                ViewData["MenuTypes"] = MenuTypes.导航菜单.GetSelectListByEnum(Convert.ToInt32(menu.MenuType));
            }
        }
    }
}