using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PinhuaMaster.Data;
using PinhuaMaster.Extensions;
using PinhuaMaster.Services;

namespace PinhuaMaster.Pages.MenuSystem
{
    public class CreateModel : PageModel
    {
        public CreateModel(ApplicationDbContext dbContext, INavMenuService navMenuService, IFileProvider fileProvider)
        {
            _dbContext = dbContext;
            _navMenuService = navMenuService;
            _fileProvider = fileProvider;
        }

        private ApplicationDbContext _dbContext { get; set; }

        private INavMenuService _navMenuService { get; set; }

        private IFileProvider _fileProvider { get; set; }

        [BindProperty]
        public Menu _menu { get; set; } = new Menu
        {
            IndexCode = 1,
            Icon = "fa fa-circle-o"
        };

        public void OnGet()
        {
            UpdateDropDownList();
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                if (!_dbContext.Menus.Any(x => x.Id == _menu.Id))
                {
                    _dbContext.Add(_menu);
                    _dbContext.SaveChanges();

                    _navMenuService.InitOrUpdate();
                    return RedirectToPage("Index");
                }
                else
                {
                    ModelState.AddModelError("Id", "菜单编号已存在，请修改菜单编号.");
                }
            }
            UpdateDropDownList(_menu);
            return Page();
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