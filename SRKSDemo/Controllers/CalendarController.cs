using HiQPdf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SRKSDemo.Server_Model;

namespace SRKSDemo.Controllers
{
    public class CalendarController : Controller
    {

        i_facility_shaktiEntities1 Serverdb = new i_facility_shaktiEntities1();
        public ActionResult Calendar()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            Session["CellId"] = 1;
            Session["colordata"] = null;
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            ViewData["HolidayTypeId"] = new SelectList(Serverdb.tblHolidayTypeMasters.Where(m => m.isDeleted == false), "HolidayTypeId", "HolidayTypeName");
            //var holidayManagment = Servercondb.configuration_tblholidaymanagments.Where(m => m.isDelete == false).ToList();
            return View();
        }

        [HttpPost]
        public string saveCalendarItem(CalendarItem dbItem)
        {
            string response = "";
            try
            {
                int userid = Convert.ToInt32(Session["UserId"]);
                string fDat = dbItem.startDate;
                string[] frDate = fDat.Split(' ');
                fDat = frDate[2] + '-' + frDate[1] + '-' + frDate[3];
                string tDat = dbItem.endDate;
                string[] toDate = tDat.Split(' ');
                tDat = toDate[2] + '-' + toDate[1] + '-' + toDate[3];

                DateTime fromdate = Convert.ToDateTime(fDat);
                DateTime todate = Convert.ToDateTime(tDat);
                int daydiffrnc = Convert.ToInt32((todate - fromdate).TotalDays) + 1;
                if (daydiffrnc == 0)
                {
                    daydiffrnc = 1;
                }
                if (dbItem.id != 0)
                {
                    //updated
                    var obj = Serverdb.tblHolidayManagments.Where(m => m.HolidayManagmentId == dbItem.id).FirstOrDefault();
                    obj.HolidayManagmentName = dbItem.name;
                    obj.HolidayManagmentDesc = dbItem.name;
                    obj.HolidayType = dbItem.location;
                    obj.FromDate = fromdate;
                    obj.ToDate = todate;
                    obj.daysDuration = daydiffrnc;
                    obj.ModifiedBy = userid;
                    obj.ModifiedOn = DateTime.Now;
                    Serverdb.SaveChanges();
                    response = "Added";//this is same because in jquery using this string for success message displaying purpose
                }
                else
                {
                    // add new
                    var item = Serverdb.tblHolidayManagments.Where(m => m.FromDate == fromdate && m.ToDate == todate && m.HolidayManagmentName == dbItem.name).ToList();
                    if (item.Count == 0)
                    {
                        if (dbItem.name != null)
                        {
                            tblHolidayManagment obj = new tblHolidayManagment();
                            obj.HolidayManagmentName = dbItem.name;
                            obj.HolidayManagmentDesc = dbItem.name;
                            obj.HolidayType = dbItem.location;
                            obj.FromDate = fromdate;
                            obj.ToDate = todate;
                            obj.daysDuration = daydiffrnc;
                            obj.isActive = true;
                            obj.isDelete = false;
                            obj.CreatedBy = userid;
                            obj.CreatedOn = DateTime.Now;
                            Serverdb.tblHolidayManagments.Add(obj);
                            Serverdb.SaveChanges();
                            response = "Added";
                        }
                        else
                        {
                            response = "NotAdded";
                        }
                    }
                    else
                    {
                        response = "NotAdded";
                    }
                }

            }
            catch
            {
                response = "NotAdded";
            }
            return response;
        }

        public JsonResult getALlCalendarItem()
        {
            var holidayManagment = Serverdb.tblHolidayManagments.Select(
                m => new
                {
                    id = m.HolidayManagmentId,
                    name = m.HolidayManagmentName,
                    location = Serverdb.tblHolidayTypeMasters.Where(x => x.HolidayTypeId == m.HolidayType).Select(x => x.HolidayTypeName).FirstOrDefault(),
                    startDate = m.FromDate,
                    endDate = m.ToDate,
                    isDeleted = m.isDelete
                }).Where(m => m.isDeleted == false).ToList();
            List<CalendarItemString> calItem = new List<CalendarItemString>();
            foreach (var Item in holidayManagment)
            {
                CalendarItemString obj = new CalendarItemString();
                obj.id = Item.id;
                obj.name = Item.name;
                obj.location = Convert.ToString(Item.location);
                DateTime st = Convert.ToDateTime(Item.startDate);
                DateTime et = Convert.ToDateTime(Item.endDate);
                obj.startDate = st.ToString("yyyy-MM-dd");
                obj.endDate = et.ToString("yyyy-MM-dd");
                calItem.Add(obj);
            }
            return Json(calItem, JsonRequestBehavior.AllowGet);
        }

        public string deleteItemIndb(int id)
        {
            string ret = "";
            int userid = Convert.ToInt32(Session["UserId"]);
            var obj = Serverdb.tblHolidayManagments.Where(m => m.HolidayManagmentId == id).FirstOrDefault();
            obj.ModifiedBy = userid;
            obj.ModifiedOn = DateTime.Now;
            obj.isDelete = true;
            Serverdb.SaveChanges();
            ret = "Deleted";

            return ret;
        }

        public ActionResult SundayHoliday(int year)
        {
            List<DateTime> Dates;
            int userid = Convert.ToInt32(Session["UserId"]);
            Dates = GetAllSatAndSuns(year);

            foreach (var date in Dates)
            {
                var item = Serverdb.tblHolidayManagments.Where(m => m.FromDate == date && m.ToDate == date && m.HolidayManagmentName == "Sunday").ToList();
                if (item.Count == 0)
                {
                    tblHolidayManagment obj = new tblHolidayManagment();
                    obj.HolidayManagmentName = "Sunday";
                    obj.HolidayManagmentDesc = "Sunday";
                    obj.HolidayType = 3;
                    obj.FromDate = date;
                    obj.ToDate = date;
                    obj.isActive = true;
                    obj.isDelete = false;
                    obj.CreatedBy = userid;
                    obj.CreatedOn = DateTime.Now;
                    obj.daysDuration = 1;
                    Serverdb.tblHolidayManagments.Add(obj);
                    Serverdb.SaveChanges();
                }
            }

            return Redirect("Calendar");
        }

        public List<DateTime> GetAllSatAndSuns(int year)
        {
            List<DateTime> Dates = new List<DateTime>();


            DateTime Date = new DateTime(year, 1, 1);


            while (Date.Year == year)
            {
                if (Date.DayOfWeek == DayOfWeek.Sunday)
                {
                    Dates.Add(Date);
                }
                Date = Date.AddDays(1);
            }
            return Dates;
        }

        public string dataListHtml(int year)
        {
            string html = "";
            List<HTMLBadgeMaker> listObj = new List<HTMLBadgeMaker>();
            string st = year + "-01-01" + " 00:00:01";
            string et = year + "-12-31" + " 23:59:59";
            DateTime startDate = Convert.ToDateTime(st);
            DateTime endDate = Convert.ToDateTime(et);
            var holidayType = Serverdb.tblHolidayTypeMasters.Where(m => m.isDeleted == false).ToList();
            foreach (var item in holidayType)
            {
                HTMLBadgeMaker obj = new HTMLBadgeMaker();
                int count = 0;
                var dbItem = Serverdb.tblHolidayManagments.Where(m => m.FromDate >= startDate && m.ToDate <= endDate).Select(m => new { m.daysDuration, m.HolidayType }).Where(m => m.HolidayType == item.HolidayTypeId).ToList();
                if (dbItem.Count > 0)
                {
                    count = Serverdb.tblHolidayManagments.Where(m => m.FromDate >= startDate && m.ToDate <= endDate).Select(m => new { m.daysDuration, m.HolidayType }).Where(m => m.HolidayType == item.HolidayTypeId).Sum(m => m.daysDuration);
                }
                obj.holidayTypeColor = item.HolidayTypeColorCode;
                obj.holidayTypeCount = count;
                obj.holidayTypeName = item.HolidayTypeName;
                listObj.Add(obj);
            }

            html = htmlMaker(listObj);

            return html;
        }

        public string htmlMaker(List<HTMLBadgeMaker> items)
        {
            string html = "";
            foreach (var item in items)
            {
                html = html + "<span class='badge' style='font-size: 15px;background-color: " + item.holidayTypeColor + ";padding: 7px;margin-bottom: 10px;'>" + item.holidayTypeName + " - " + item.holidayTypeCount + "</span></br>";
            }
            return html;
        }

        public string dataHolidayListHtml(int year)
        {
            string html = "";
            List<HTMLHolidayBadgeMaker> listObj = new List<HTMLHolidayBadgeMaker>();
            string st = year + "-01-01" + " 00:00:00";
            string et = year + "-12-31" + " 23:59:59";
            DateTime startDate = Convert.ToDateTime(st);
            DateTime endDate = Convert.ToDateTime(et);
            var holidayList = Serverdb.tblHolidayManagments.Where(m => m.isDelete == false && m.HolidayType != 3 && m.HolidayType != 4).Where(m => m.FromDate >= startDate && m.ToDate <= endDate).OrderBy(m => m.FromDate).ToList();
            foreach (var item in holidayList)
            {
                HTMLHolidayBadgeMaker obj = new HTMLHolidayBadgeMaker();

                obj.holidayName = item.HolidayManagmentName;
                obj.daysCounter = item.daysDuration; ;
                obj.fromDate = item.FromDate.ToString();
                obj.toDate = item.ToDate.ToString();
                obj.holidayTypeColor = Serverdb.tblHolidayTypeMasters.Where(m => m.isDeleted == false && m.HolidayTypeId == item.HolidayType).Select(m => m.HolidayTypeColorCode).FirstOrDefault();
                listObj.Add(obj);
            }

            html = htmlHolidayMaker(listObj);

            return html;
        }

        public string htmlHolidayMaker(List<HTMLHolidayBadgeMaker> items)
        {

            string html = "";
            foreach (var item in items)
            {
                #region code for iis
                //sample date : 1/17/2018 12:00:00 AM
                //html = item.fromDate + ' ' + item.toDate;
                //string[] frmDate = item.fromDate.Split(' ');
                //frmDate = frmDate[0].Split('/');
                //string[] toDate = item.toDate.Split(' ');
                //toDate = toDate[0].Split('/');
                //string frmdate = frmDate[1];
                //string todate = toDate[1];
                //string frmMonth = monthByNumber(Convert.ToInt32(frmDate[0]) - 1);
                //string toMonth = monthByNumber(Convert.ToInt32(toDate[0]) - 1);

                //if (item.daysCounter == 1)
                //{
                //    html = html + "<span class='badge' style='font-size: 12px; background-color:" + item.holidayTypeColor + ";padding: 7px;margin-bottom:10px;'>" + item.holidayName + " " + frmdate + "-" + frmMonth + "</span></br>";
                //}
                //else
                //{
                //    html = html + "<span class='badge' style='font-size: 12px; background-color:" + item.holidayTypeColor + ";padding: 7px;margin-bottom:10px;'>" + item.holidayName + " " + frmdate + "-" + frmMonth + " to " + todate + "-" + toMonth + "</span></br>";
                //}
                #endregion

                #region code for visual studio
                //sample date : 2018-01-17 00:00:00
                string[] frmDate = item.fromDate.Split(' ');
                frmDate = frmDate[0].Split('-');
                string[] toDate = item.toDate.Split(' ');
                toDate = toDate[0].Split('-');
                string frmdate = frmDate[2];
                string todate = toDate[2];
                string frmMonth = monthByNumber(Convert.ToInt32(frmDate[1]) - 1);
                string toMonth = monthByNumber(Convert.ToInt32(toDate[1]) - 1);

                if (item.daysCounter == 1)
                {
                    html = html + "<span class='badge' style='font-size: 12px; background-color:" + item.holidayTypeColor + ";padding: 7px;margin-bottom:10px;'>" + item.holidayName + " " + frmdate + "-" + frmMonth + "</span></br>";
                }
                else
                {
                    html = html + "<span class='badge' style='font-size: 12px; background-color:" + item.holidayTypeColor + ";padding: 7px;margin-bottom:10px;'>" + item.holidayName + " " + frmdate + "-" + frmMonth + " to " + todate + "-" + toMonth + "</span></br>";
                }
                #endregion

            }
            return html;
        }

        public ActionResult goback()
        {
            return RedirectToAction("Dashboard", "Dashboard");
        }

        public string monthByNumber(int monthNo)
        {
            string month = "";
            string[] monthsShort = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            month = monthsShort[monthNo];
            return month;
        }

        public string RenderViewAsString(string viewName, object model)
        {
            // create a string writer to receive the HTML code
            StringWriter stringWriter = new StringWriter();

            // get the view to render
            ViewEngineResult viewResult = ViewEngines.Engines.FindView(ControllerContext,
                      viewName, null);
            //ViewData["HolidayTypeId"] = new SelectList(Servercondb.configuration_tblholidaytypemasters.Where(m => m.isDeleted == false), "HolidayTypeId", "HolidayTypeName");
            object viewData = new SelectList(Serverdb.tblHolidayTypeMasters.Where(m => m.isDeleted == false), "HolidayTypeId", "HolidayTypeName");

            ViewDataDictionary viewDataDic = new ViewDataDictionary();
            viewDataDic.Add("HolidayTypeId", viewData);
            // create a context to render a view based on a model
            ViewContext viewContext = new ViewContext(
                ControllerContext,
                viewResult.View,
                new ViewDataDictionary(viewDataDic),
                new TempDataDictionary(),
                stringWriter
            );

            // render the view to a HTML code
            viewResult.View.Render(viewContext, stringWriter);

            // return the HTML code
            return stringWriter.ToString();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ConvertHtmlPageToPdf(string html)
        {

            // get the HTML code of this view
            string htmlToConvert = RenderViewAsString("Calendar", null);
            string itemToFind = @"<div data-provide=""calendar"" class=""calendar-dis""  id=""htmlbody"">";
            htmlToConvert = htmlToConvert.Replace(itemToFind, itemToFind + html);
            // the base URL to resolve relative images and css
            String thisPageUrl = this.ControllerContext.HttpContext.Request.Url.AbsoluteUri;
            String baseUrl = thisPageUrl.Substring(0, thisPageUrl.Length -
                "Calendar/ConvertThisPageToPdf".Length);

            // instantiate the HiQPdf HTML to PDF converter
            HtmlToPdf htmlToPdfConverter = new HtmlToPdf();

            // hide the button in the created PDF
            //htmlToPdfConverter.HiddenHtmlElements = new string[] { "#convertThisPageButtonDiv" };

            // render the HTML code as PDF in memory
            byte[] pdfBuffer = htmlToPdfConverter.ConvertHtmlToMemory(htmlToConvert, baseUrl);
            DateTime dateObj = new DateTime();
            string Year = dateObj.Year.ToString();
            string pdfName = "Holiday List " + Year + ".pdf";
            // send the PDF file to browser
            FileResult fileResult = new FileContentResult(pdfBuffer, "application/pdf");
            fileResult.FileDownloadName = pdfName;

            return fileResult;
        }

    }



    public class CalendarItem
    {
        public int id { get; set; }
        public string name { get; set; }
        public int location { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
    }
    public class CalendarItemString
    {
        public int id { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
    }
    public class HTMLBadgeMaker
    {
        public string holidayTypeName { get; set; }
        public int holidayTypeCount { get; set; }
        public string holidayTypeColor { get; set; }
    }
    public class HTMLHolidayBadgeMaker
    {
        public string holidayName { get; set; }
        public string fromDate { get; set; }
        public string toDate { get; set; }
        public int daysCounter { get; set; }
        public string holidayTypeColor { get; set; }
    }
}