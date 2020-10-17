using i_facility.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;




using SRKSDemo.Server_Model;
using SRKSDemo;

namespace i_facility.Controllers
{
    public class UserController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        public ActionResult Redirect()
        {
            return RedirectToAction("Login", "Login", null);
        }

        public ActionResult UserManagement()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewData["roles"] = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0), "Role_Id", "RoleDesc").ToList();

            return View();
        }

        public string GetUserMenus()
        {
            List<UserMenus> menus = new List<UserMenus>();
            string res = "";

            DataTable dt = new DataTable();
            try
            {
                using (MsqlConnection mc = new MsqlConnection())
                {
                    using (SqlCommand cmd = new SqlCommand("GetUserMenus", mc.msqlConnection))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            mc.open();
                            cmd.CommandType = CommandType.StoredProcedure;
                            sda.Fill(dt);
                            mc.close();
                        }
                    }
                }
            }
            catch (Exception ex) { }


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                UserMenus menu = new UserMenus();

                menu.Id = dt.Rows[i]["MenuName"].ToString();
                menu.MenuId = Convert.ToInt32(dt.Rows[i]["Id"]);
                menu.MenuName = dt.Rows[i]["MenuName"].ToString();

                menus.Add(menu);
            }

            res = JsonConvert.SerializeObject(menus);

            return res;
        }

        public string GetCheckboxesForUser(int roleId)
        {
            List<UserMenus> menus = new List<UserMenus>();
            string res = "";

            DataTable dt = new DataTable();
            try
            {
                using (MsqlConnection mc = new MsqlConnection())
                {
                    using (SqlCommand cmd = new SqlCommand("GetCheckboxesForUser", mc.msqlConnection))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            mc.open();
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@roleId", roleId);
                            sda.Fill(dt);
                            mc.close();
                        }
                    }
                }
            }
            catch (Exception ex) { }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                UserMenus menu = new UserMenus();

                menu.Id = dt.Rows[i]["MenuName"].ToString();
                menu.MenuId = Convert.ToInt32(dt.Rows[i]["MenuId"]);
                menu.MenuName = dt.Rows[i]["MenuName"].ToString();

                string status = Convert.ToString(dt.Rows[i]["MenuStatus"]);
                menu.MenuStatus = status;

                menus.Add(menu);
            }

            res = JsonConvert.SerializeObject(menus);

            return res;
        }

        public string UpdateMenus(int roleId, string menuIds, string unMenuIds)
        {
            string res = "";

            if (menuIds != null)
            {
                menuIds = menuIds.Replace("[", "");
                menuIds = menuIds.Replace("]", "");
                menuIds = menuIds.Replace("\"", "");
                string[] menus = menuIds.Split(',');

                if (!menus.Contains(""))
                {
                    foreach (var item in menus)
                    {
                        int menuId = Convert.ToInt32(item);
                        var userMenu = db.user_menus.Where(m => m.IsDeleted == 0 && m.RoleId == roleId && m.MenuId == menuId).FirstOrDefault();
                        if (userMenu == null)
                        {
                            user_menus userMenus = new user_menus();
                            userMenus.RoleId = roleId;
                            userMenus.MenuId = menuId;
                            userMenus.MenuStatus = true;
                            userMenus.IsDeleted = 0;
                            db.user_menus.Add(userMenus);
                            db.SaveChanges();

                            TempData["toaster_success"] = "Menu Items Added Successfully";
                        }
                        else
                        {
                            userMenu.MenuStatus = true;
                            db.SaveChanges();

                            TempData["toaster_success"] = "Menu Items Updated Successfully";
                        }
                    }
                }
            }

            if (unMenuIds != null)
            {
                unMenuIds = unMenuIds.Replace("[", "");
                unMenuIds = unMenuIds.Replace("]", "");
                unMenuIds = unMenuIds.Replace("\"", "");
                string[] unMenus = unMenuIds.Split(',');

                if (!unMenus.Contains(""))
                {
                    foreach (var item in unMenus)
                    {
                        int unMenuId = Convert.ToInt32(item);
                        var userMenu = db.user_menus.Where(m => m.IsDeleted == 0 && m.RoleId == roleId && m.MenuId == unMenuId).FirstOrDefault();
                        userMenu.MenuStatus = false;
                        db.SaveChanges();

                        TempData["toaster_success"] = "Menu Items Updated Successfully";
                    }
                }
            }

            return res;
        }

        public string GetSideMenubar()
        {
            int roleId = Convert.ToInt32(Session["RoleID"]);

            string res = "";

            List<SideMenubar> sideMenubars = new List<SideMenubar>();

            DataTable dt = new DataTable();
            try
            {
                using (MsqlConnection mc = new MsqlConnection())
                {
                    using (SqlCommand cmd = new SqlCommand("GetSideMenubar", mc.msqlConnection))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            mc.open();
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@roleId", roleId);
                            sda.Fill(dt);
                            mc.close();
                        }
                    }
                }
            }
            catch (Exception ex) { }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                int menuId = Convert.ToInt32(dt.Rows[i]["MenuId"]);

                SideMenubar sideMenubar = new SideMenubar();

                sideMenubar.MenuName = dt.Rows[i]["MenuName"].ToString();
                sideMenubar.RoleId = roleId;

                var imageurl = db.sidebar_menus.Where(m => m.MenuName == sideMenubar.MenuName).Select(m => m.ImageURL).FirstOrDefault();
                sideMenubar.ImageURL = imageurl;

                var values = db.sidebar_menus.Where(m => m.SubMenuName != null && m.MenuId == menuId && m.IsDeleted == 0).OrderBy(m => m.id).Select(m => new { m.SubMenuName, m.SubMenuURL }).ToList();

                if (values.Count == 0)
                {
                    //sideMenubar.MenuURL = dt.Rows[i]["MenuURL"].ToString();
                    var menuurl = db.sidebar_menus.Where(m => m.MenuName == sideMenubar.MenuName).Select(m => m.MenuURL).FirstOrDefault();
                    sideMenubar.MenuURL = menuurl;
                }
                List<SubMenus> subMenusList = new List<SubMenus>();
                foreach (var item in values)
                {
                    SubMenus subMenus = new SubMenus();

                    subMenus.SubMenuName = item.SubMenuName;
                    subMenus.SubMenuURL = item.SubMenuURL;

                    subMenusList.Add(subMenus);
                }

                sideMenubar.SubMenus = subMenusList;
                sideMenubars.Add(sideMenubar);
            }
            res = JsonConvert.SerializeObject(sideMenubars);

            return res;
        }

        public string GetDashboardMenus()
        {

            string res = "";
            int roleId = Convert.ToInt32(Session["RoleID"]);

            List<DashboardMenu> dashboardMenus = new List<DashboardMenu>();

            DataTable dt = new DataTable();
            try
            {
                using (MsqlConnection mc = new MsqlConnection())
                {
                    using (SqlCommand cmd = new SqlCommand("GetDashboardMenus", mc.msqlConnection))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            mc.open();
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@roleId", roleId);
                            sda.Fill(dt);
                            mc.close();
                        }
                    }
                }
            }
            catch (Exception ex) { }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DashboardMenu dashboardMenu = new DashboardMenu();

                dashboardMenu.MenuName = dt.Rows[i]["MenuName"].ToString();
                dashboardMenu.MenuURL = dt.Rows[i]["MenuURL"].ToString();
                dashboardMenu.ImageURL = dt.Rows[i]["ImageUrl"].ToString();
                dashboardMenu.ColourDiv = dt.Rows[i]["ColourDiv"].ToString();
                dashboardMenu.Style = dt.Rows[i]["Style"].ToString();

                dashboardMenus.Add(dashboardMenu);
            }
            res = JsonConvert.SerializeObject(dashboardMenus);

            return res;
        }
    }
}