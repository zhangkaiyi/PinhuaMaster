using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PinhuaMaster.Data;
using PinhuaMaster.Services;
using Microsoft.Extensions.FileProviders;
using System.IO;
using PinhuaMaster.Data.Entities.Pinhua;

namespace PinhuaMaster
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add DbContext
            services.AddDbContext<PinhuaContext>(
                options => options.UseSqlServer("Data Source=122.225.47.230,6012;Initial Catalog=Pinhua;Persist Security Info=True;User ID=sa;Password=Benny0922")
                );

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("IdentityConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc()
                .AddRazorPagesOptions(options =>
                {
                    //options.Conventions.AuthorizeFolder("/Account/Manage");
                    //options.Conventions.AuthorizePage("/Account/Logout");
                    options.Conventions.AuthorizeFolder("/", "Permissons").AllowAnonymousToFolder("/Account");
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Permissons", policy =>
                    {
                        policy.Requirements.Add(new PermissionRequirement());
                    });
            });

            // Register no-op EmailSender used by account confirmation and password reset during development
            // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
            services.AddSingleton<IEmailSender, EmailSender>();
            // 注册导航菜单生成服务
            services.AddTransient<INavMenuService, NavMenuService>();
            // 注册PinhuaTimeline服务
            services.AddTransient<ITimelineService,TimelineService>();
            // 访问本地文件所需的服务
            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
            // 提取网页字体中图标到数据库的服务
            services.AddTransient<IWebfontExtractor, WebfontExtractor>();

            services.AddTransient<IActionPermissionService,ActionPermissionService>();
            //初始化应用配置
            //InitAppConfig(services);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }

        /// <summary>
        /// 初始化配置
        /// </summary>
        /// <param name="services"></param>
        //private void InitAppConfig(IServiceCollection services)
        //{
        //    var builder = new ConfigurationBuilder()
        //        .SetBasePath(Directory.GetCurrentDirectory())
        //        .AddJsonFile("NavbarMenus.json", optional: true, reloadOnChange: true);

        //    var config = builder.Build();

        //    services.Configure<List<NavbarMenu>>(config.GetSection("NavBarMenus"));
        //}
    }
}
