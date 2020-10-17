using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace i_facility.Models
{
    public class UserMenus
    {
        public int MenuId { get; set; }
        public string Id { get; set; }
        public string MenuName { get; set; }
        public string MenuStatus { get; set; }
        public string MenuURL { get; set; }
    }

    public class SideMenubar
    {
        public string MenuName { get; set; }
        public string MenuURL { get; set; }
        public string ImageURL { get; set; }
        public List<SubMenus> SubMenus { get; set; }
        public int RoleId { get; set; }
    }

    public class DashboardMenu
    {
        public string MenuName { get; set; }
        public string MenuURL { get; set; }
        public string ImageURL { get; set; }
        public string ColourDiv { get; set; }
        public string Style { get; set; }
    }

    public class SubMenus
    {
        public string SubMenuName { get; set; }
        public string SubMenuURL { get; set; }
        public int Id { get; set; }
        public string EditUrl { get; set; }
    }

    public class DropdownClass
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }
}