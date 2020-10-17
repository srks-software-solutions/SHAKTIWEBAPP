using i_facility.Models;
using Newtonsoft.Json;


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
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using SRKSDemo;
using context = System.Web.HttpContext;
namespace i_facility.Controllers
{
    public class MenuController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        // GET: Menu
        public ActionResult SubMenusList()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewData["Menus"] = new SelectList(db.menus.Where(m => m.IsDeleted == 0), "id", "MenuName").ToList();

            return View();
        }

        [HttpGet]
        public ActionResult CreateSubMenus(string Menus)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Menus = Menus;
            return View();
        }

        [HttpPost]
        public ActionResult CreateSubMenus(sidebar_menus sidebarMenu)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            int menuId = Convert.ToInt32(sidebarMenu.MenuId);

            string menuName = db.menus.Where(m => m.Id == menuId).Select(m => m.MenuName).FirstOrDefault();
            if (menuName == "Auto Report/Escalation")
            {
                string[] splitString = menuName.Split('/');
                string name = splitString[0] + "/" + "<br/>" + splitString[1];
                menuName = name;
            }

            var sidebarMenuList = db.sidebar_menus.Where(m => m.MenuName == menuName).ToList();
            var subMenu = db.sidebar_menus.Where(m => m.MenuName == menuName).Select(m => m.SubMenuName).FirstOrDefault();

            if (sidebarMenuList.Count == 1 && subMenu == null)
            {
                foreach (var item in sidebarMenuList)
                {
                    item.SubMenuName = sidebarMenu.SubMenuName;
                    item.SubMenuURL = sidebarMenu.SubMenuURL;
                    db.SaveChanges();

                    break;
                }
            }
            else
            {
                sidebar_menus sidebar_Menus = new sidebar_menus();
                sidebar_Menus.SubMenuName = sidebarMenu.SubMenuName;
                sidebar_Menus.MenuName = menuName;
                sidebar_Menus.SubMenuURL = sidebarMenu.SubMenuURL;
                sidebar_Menus.IsDeleted = 0;
                sidebar_Menus.MenuId = menuId;
                db.sidebar_menus.Add(sidebar_Menus);
                db.SaveChanges();
            }
            TempData["toaster_success"] = "SubMenu Added Successfully";

            ViewData["Menus"] = new SelectList(db.menus.Where(m => m.IsDeleted == 0), "id", "MenuName").ToList();
            return View("SubMenusList");
        }

        [HttpGet]
        public ActionResult EditSubMenus(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            sidebar_menus sidebarMenus = db.sidebar_menus.Find(id);

            return View(sidebarMenus);
        }

        [HttpPost]
        public ActionResult EditSubMenus(sidebar_menus sidebarMenu)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            sidebar_menus sidebar_Menus = db.sidebar_menus.Find(sidebarMenu.id);

            sidebar_Menus.SubMenuName = sidebarMenu.SubMenuName;
            sidebar_Menus.SubMenuURL = sidebarMenu.SubMenuURL;
            db.SaveChanges();

            TempData["toaster_success"] = "SubMenu Updated Successfully";

            ViewData["Menus"] = new SelectList(db.menus.Where(m => m.IsDeleted == 0), "id", "MenuName").ToList();
            return View("SubMenusList");
        }

        public string Delete(int id)
        {
            string message = "Success";
            sidebar_menus sidebar_Menus = db.sidebar_menus.Find(id);
            sidebar_Menus.IsDeleted = 1;
            sidebar_Menus.ModifiedOn = DateTime.Now;
            db.SaveChanges();

            return message;
        }

        public ActionResult MenusList()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            var menusList = db.menus.Where(m => m.IsDeleted == 0).OrderBy(m => m.Id).ToList();
            return View(menusList.ToList());
        }

        [HttpGet]
        public ActionResult CreateMenus()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewData["DisplayOrder"] = new SelectList(db.menus.Where(m => m.IsDeleted == 0 && m.DisplayOrder != null).OrderBy(m => m.DisplayOrder), "DisplayOrder", "DisplayOrder");
            ViewData["MenuStyle"] = new SelectList(db.dashboard_menus.Where(m => m.IsDeleted == 0 && m.ColourDiv != null), "Id", "ColourDiv").ToList();
            return View();
        }

        [HttpPost]
        public ActionResult CreateMenus(sidebar_menus menus, HttpPostedFileBase imageFile, string MenuStyle, int DashboardAccess = 0, int SidebarAccess = 0, int roleid = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            string imageUrl = "", imageUrl1 = "";
            if (imageFile != null)
            {
                int imageSize = imageFile.ContentLength;
                if ((imageSize / 1024 / 1024) < 1)
                {
                    string fileExtension = Path.GetExtension(imageFile.FileName);
                    string imgName = imageFile.FileName;
                    string[] imgSplit = imgName.Split('.');
                    Image img = Image.FromStream(imageFile.InputStream, true, true);
                    if (DashboardAccess == 1)
                    {
                        Image imgSave = ReduceImageSize(img, "DashImage", null);
                        string imgUrl = SaveImage(imgSave, imageFile.FileName, "DashImage", null);
                        imageUrl = "/images/" + imageFile.FileName;
                    }
                    if (SidebarAccess == 1)
                    {
                        Image imgSave1 = ReduceImageSize(img, null, "SideImage");
                        string imgUrl1 = SaveImage(imgSave1, imgSplit[0], null, "SideImage", fileExtension);
                        imageUrl1 = "/images/" + imgSplit[0] + "(1)" + fileExtension;
                    }
                }
                else
                {
                    ViewBag.DisplayOrder = new SelectList(db.menus.Where(m => m.IsDeleted == 0).OrderBy(m => m.DisplayOrder), "DisplayOrder", "DisplayOrder");
                    ViewData["MenuStyle"] = new SelectList(db.dashboard_menus.Where(m => m.IsDeleted == 0 && m.ColourDiv != null), "Id", "ColourDiv", MenuStyle).ToList();
                    TempData["toaster_warning"] = "Image Size is larger";
                    return View();
                }
            }

            var value = db.menus.Where(m => m.DisplayOrder == menus.DisplayOrder).Select(m => m.DisplayOrder).FirstOrDefault();
            if (value != null)
            {
                int displayOrder = Convert.ToInt32(value);

                var menuList = db.menus.Where(m => m.IsDeleted == 0 && m.DisplayOrder >= displayOrder).ToList();
                foreach (var item in menuList)
                {
                    item.DisplayOrder = item.DisplayOrder + 1;
                    db.SaveChanges();
                }
            }
            menu menu = new menu();
            menu.MenuName = menus.MenuName;
            menu.IsDeleted = 0;
            menu.IsDashboard = DashboardAccess;
            menu.IsSideMenubar = SidebarAccess;
            menu.DisplayOrder = menus.DisplayOrder;
            db.menus.Add(menu);
            db.SaveChanges();

            var menuId = db.menus.Where(m => m.MenuName == menus.MenuName).Select(m => m.Id).FirstOrDefault();
            if (SidebarAccess == 1)
            {
                sidebar_menus sidebarMenus = new sidebar_menus();
                sidebarMenus.MenuName = menus.MenuName;
                sidebarMenus.MenuId = menuId;
                sidebarMenus.MenuURL = menus.MenuURL;
                if (imageUrl1 != "")
                    sidebarMenus.ImageURL = imageUrl1;
                sidebarMenus.IsDeleted = 0;
                db.sidebar_menus.Add(sidebarMenus);
                db.SaveChanges();
            }



            if (DashboardAccess == 1)
            {
                dashboard_menus dashboardMenus = new dashboard_menus();
                dashboardMenus.MenuName = menus.MenuName;
                dashboardMenus.MenuUrl = menus.MenuURL;
                dashboardMenus.MenuId = menuId;
                dashboardMenus.ColourDiv = MenuStyle;
                if (imageUrl != "")
                    dashboardMenus.ImageUrl = imageUrl;
                dashboardMenus.IsDeleted = 0;
                db.dashboard_menus.Add(dashboardMenus);
                db.SaveChanges();
            }


            //date: 25-8-2020 (issue in side bar menu not displaying newly created menu item)
            int roleId = Convert.ToInt32(Session["RoleID"]);
            user_menus userval = new user_menus();
            userval.MenuId = menuId;
            userval.RoleId = roleid;
            userval.MenuStatus = true;
            userval.IsDeleted = 0;
            db.user_menus.Add(userval);
            db.SaveChanges();


            TempData["toaster_success"] = "Menu Added Successfully";
            return RedirectToAction("MenusList");
        }

        [HttpGet]
        public ActionResult EditMenus(int id, string Message = null)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            var displayOrder = db.menus.Where(m => m.IsDeleted == 0 && m.Id == id).Select(m => m.DisplayOrder).FirstOrDefault();
            var colourDiv = db.dashboard_menus.Where(m => m.IsDeleted == 0 && m.MenuId == id).Select(m => m.id).FirstOrDefault();

            ViewBag.MenuName = db.menus.Where(m => m.IsDeleted == 0 && m.Id == id).Select(m => m.MenuName).FirstOrDefault();
            ViewBag.Dashboard = db.menus.Where(m => m.IsDeleted == 0 && m.Id == id).Select(m => m.IsDashboard).FirstOrDefault();
            ViewBag.Sidebar = db.menus.Where(m => m.IsDeleted == 0 && m.Id == id).Select(m => m.IsSideMenubar).FirstOrDefault();
            ViewBag.MenuUrl = db.dashboard_menus.Where(m => m.IsDeleted == 0 && m.MenuId == id).Select(m => m.MenuUrl).FirstOrDefault();
            if (ViewBag.MenuUrl == null)
                ViewBag.MenuUrl = db.sidebar_menus.Where(m => m.IsDeleted == 0 && m.MenuId == id).Select(m => m.MenuURL).FirstOrDefault();
            ViewBag.DisplayOrder = new SelectList(db.menus.Where(m => m.IsDeleted == 0).OrderBy(m => m.DisplayOrder), "DisplayOrder", "DisplayOrder", displayOrder);
            ViewBag.MenuStyle = new SelectList(db.dashboard_menus.Where(m => m.IsDeleted == 0 && m.ColourDiv != null), "id", "ColourDiv", colourDiv);
            ViewBag.Image = db.dashboard_menus.Where(m => m.IsDeleted == 0 && m.MenuId == id).Select(m => m.ImageUrl).FirstOrDefault();
            if (ViewBag.Image == null)
                ViewBag.Image = db.sidebar_menus.Where(m => m.IsDeleted == 0 && m.MenuId == id).Select(m => m.ImageURL).FirstOrDefault();

            if (Message != null)
            {
                TempData["toaster_warning"] = Message;
            }

            return View();
        }

        [HttpPost]
        public ActionResult EditMenus(sidebar_menus menus, HttpPostedFileBase imageFile, string MenuStyle, int DashboardAccess = 0, int SidebarAccess = 0, string getImage = null)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            if (DashboardAccess == 2) DashboardAccess = 0;
            if (SidebarAccess == 2) SidebarAccess = 0;

            if (imageFile != null) getImage = null;

            string imageUrl = "", imageUrl1 = "";
            if (imageFile != null && getImage == null)
            {
                int imageSize = imageFile.ContentLength;
                if ((imageSize / 1024 / 1024) < 1)
                {
                    string fileExtension = Path.GetExtension(imageFile.FileName);
                    string imgName = imageFile.FileName;
                    string[] imgSplit = imgName.Split('.');
                    Image img = Image.FromStream(imageFile.InputStream, true, true);
                    if (DashboardAccess == 1)
                    {
                        Image imgSave = ReduceImageSize(img, "DashImage", null);
                        string imgUrl = SaveImage(imgSave, imageFile.FileName, "DashImage", null);
                        imageUrl = "/images/" + imageFile.FileName;
                    }
                    if (SidebarAccess == 1)
                    {
                        Image imgSave1 = ReduceImageSize(img, null, "SideImage");
                        string imgUrl1 = SaveImage(imgSave1, imgSplit[0], null, "SideImage", fileExtension);
                        imageUrl1 = "/images/" + imgSplit[0] + "(1)" + fileExtension;
                    }
                }
                else
                {
                    int id = Convert.ToInt32(menus.id);
                    string Message = "Image Size is larger";
                    return RedirectToAction("EditMenus", new { id, Message });
                }
            }

            menu menu = db.menus.Find(menus.id);
            menu.MenuName = menus.MenuName;
            var value = menu.DisplayOrder;
            int displayOrder = Convert.ToInt32(value);

            if (displayOrder != menus.DisplayOrder)
            {
                if (displayOrder < menus.DisplayOrder)
                {
                    var menuList = db.menus.Where(m => m.DisplayOrder > displayOrder && m.DisplayOrder <= menus.DisplayOrder).ToList();
                    foreach (var item in menuList)
                    {
                        item.DisplayOrder = item.DisplayOrder - 1;
                        db.SaveChanges();
                    }
                }
                else if (displayOrder > menus.DisplayOrder)
                {
                    var menuList = db.menus.Where(m => m.DisplayOrder >= menus.DisplayOrder && m.DisplayOrder <= displayOrder).ToList();
                    foreach (var item in menuList)
                    {
                        item.DisplayOrder = item.DisplayOrder + 1;
                        db.SaveChanges();
                    }
                }
                menu.DisplayOrder = menus.DisplayOrder;
            }
            menu.IsDashboard = DashboardAccess;
            menu.IsSideMenubar = SidebarAccess;
            db.SaveChanges();

            var menuId = db.menus.Where(m => m.MenuName == menus.MenuName).Select(m => m.Id).FirstOrDefault();
            if (SidebarAccess == 1)
            {
                sidebar_menus sidebarMenus = db.sidebar_menus.Where(m => m.IsDeleted == 0 && m.MenuId == menus.id).FirstOrDefault();
                sidebarMenus.MenuName = menus.MenuName;
                sidebarMenus.MenuId = menuId;
                if (menus.MenuURL != null)
                    sidebarMenus.MenuURL = menus.MenuURL;
                if (imageFile != null && getImage == null && imageUrl1 != "")
                    sidebarMenus.ImageURL = imageUrl1;
                db.SaveChanges();
            }

            if (DashboardAccess == 1)
            {
                var dashboardMenu = db.dashboard_menus.Where(m => m.IsDeleted == 0 && m.MenuId == menuId).FirstOrDefault();
                dashboardMenu.MenuName = menus.MenuName;
                dashboardMenu.MenuId = menuId;
                if (menus.MenuURL != null)
                    dashboardMenu.MenuUrl = menus.MenuURL;
                if (imageFile != null && getImage == null && imageUrl != "")
                    dashboardMenu.ImageUrl = imageUrl;
                if (MenuStyle != "")
                    dashboardMenu.ColourDiv = MenuStyle;
                db.SaveChanges();
            }

            TempData["toaster_success"] = "Menu Updated Successfully";
            return RedirectToAction("MenusList");
        }

        public ActionResult DeleteMenus(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            menu menu = db.menus.Find(id);
            menu.IsDeleted = 1;
            menu.ModifiedOn = DateTime.Now;
            db.SaveChanges();

            return RedirectToAction("MenusList");
        }

        public string GetSubMenus(int menuId)
        {
            string res = "";
            List<SubMenus> subMenus = new List<SubMenus>();

            var subMenusList = db.sidebar_menus.Where(m => m.IsDeleted == 0 && m.MenuId == menuId && m.SubMenuName != null).Select(m => new { m.SubMenuName, m.SubMenuURL, m.id }).ToList();

            foreach (var item in subMenusList)
            {
                SubMenus subMenu = new SubMenus();
                subMenu.SubMenuName = item.SubMenuName;
                subMenu.SubMenuURL = item.SubMenuURL;
                subMenu.EditUrl = "/Menu/EditSubMenus";
                subMenu.Id = item.id;

                subMenus.Add(subMenu);
            }
            res = JsonConvert.SerializeObject(subMenus);

            return res;
        }

        static Image ReduceImageSize(Image imgPhoto, string DashImage = null, string SideImage = null)
        {

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;

            int destX = 0;
            int destY = 0;
            int destWidth = 0, destHeight = 0;
            if (DashImage != null)
            {
                destWidth = 64;
                destHeight = 64;
            }

            if (SideImage != null)
            {
                destWidth = 24;
                destHeight = 24;
            }

            Bitmap bmPhoto = new Bitmap(destWidth, destHeight,
                                     PixelFormat.Format32bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                                    imgPhoto.VerticalResolution);

            Color backColor = bmPhoto.GetPixel(0, 0);
            Color backColorGray = Color.Gray;
            Color backColorGrayLight = Color.LightGray;
            Color backColorWhiteSmoke = Color.WhiteSmoke;

            bmPhoto.MakeTransparent(backColor);
            bmPhoto.MakeTransparent(backColorGray);
            bmPhoto.MakeTransparent(backColorGrayLight);
            bmPhoto.MakeTransparent(backColorWhiteSmoke);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        public string SaveImage(Image ImageFile, string Filename, string DashImage = null, string SideImage = null, string Extension = null)
        {
            string path = "";
            try
            {
                string filepath = context.Current.Server.MapPath("~/images/");

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }
                if (DashImage != null)
                    path = Path.Combine(context.Current.Server.MapPath("~/images/"), Filename);
                if (SideImage != null)
                    path = Path.Combine(context.Current.Server.MapPath("~/images/"), Filename + "(1)" + Extension);

                ImageFile.Save(path);
            }
            catch (Exception e)
            {
                throw e;
            }
            return path;
        }

        public JsonResult FetchDispItems(int length)
        {
            List<DropdownClass> dropdownList = new List<DropdownClass>();
            for (int i = 1; i <= length; i++)
            {
                DropdownClass dropdownClass = new DropdownClass();
                dropdownClass.Value = i;
                dropdownClass.Text = i.ToString();

                dropdownList.Add(dropdownClass);
            }

            return Json(dropdownList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FetchMenuStyles()
        {
            List<DropdownClass> dropdownList = new List<DropdownClass>();

            DataTable dt = new DataTable();
            try
            {
                using (MsqlConnection mc = new MsqlConnection())
                {
                    using (SqlCommand cmd = new SqlCommand("GetMenuStyles", mc.msqlConnection))
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
                DropdownClass dropdownClass = new DropdownClass();

                dropdownClass.Value = Convert.ToInt32(dt.Rows[i][0]);
                dropdownClass.Text = dt.Rows[i][1].ToString();

                dropdownList.Add(dropdownClass);
            }
            return Json(dropdownList, JsonRequestBehavior.AllowGet);
        }
    }
}

