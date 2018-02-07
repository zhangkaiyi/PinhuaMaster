using System.Collections.Generic;
using PinhuaMaster.Data;

namespace PinhuaMaster.Services
{
    public interface INavMenuService
    {
        IList<NavMenu> GetNavMenus();
        NavMenu GetOneNavMenu(Menu menu);
        void InitOrUpdate();
        IList<NavbarMenu> GetNavbarMenus();
        void UpdateNavbarMenus(string navbarMenus);
    }
}