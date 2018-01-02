using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PinhuaMaster.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                //db.Database.Migrate();

                if (db.Users.Count() == 0)
                {
                    var list = new List<ApplicationUser>
                    {
                        new ApplicationUser
                        {
                            UserName = "116307766@qq.com",
                            Email = "116307766@qq.com",
                        }
                    };
                    var userManager = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                    list.ForEach(async user =>
                    {
                        await userManager.CreateAsync(user, "benny0922");
                    });
                }
                if (db.Menus.Count() == 0)
                {

                    var menus = new List<Menu>
                    {
                        new Menu
                        {
                            Id = "M99",
                            Icon = "glyphicon glyphicon-alert",
                            IndexCode = 1,
                            MenuType = MenuTypes.导航菜单,
                            ParentId = "无",
                            Url = "/Index",
                            Name = "测试菜单"
                        },
                        new Menu
                        {
                            Id = "M01",
                            Icon = "glyphicon glyphicon-alert",
                            IndexCode = 2,
                            MenuType = MenuTypes.导航菜单,
                            ParentId = "无",
                            Name = "菜单管理"
                        },
                        new Menu
                        {
                            Id = "M01_01",
                            Icon = "glyphicon glyphicon-alert",
                            IndexCode = 2,
                            MenuType = MenuTypes.操作菜单,
                            ParentId = "M01",
                            Url = "/NavMenus/Index",
                            Name = "列表"
                        },
                        new Menu
                        {
                            Id = "M01_02",
                            Icon = "glyphicon glyphicon-alert",
                            IndexCode = 3,
                            MenuType = MenuTypes.操作菜单,
                            ParentId = "M01",
                            Url = "/NavMenus/Create",
                            Name = "新建"
                        }
                    };
                    menus.ForEach(menu => db.Menus.Add(menu));
                    db.SaveChanges();
                }
            }

        }
    }
}
