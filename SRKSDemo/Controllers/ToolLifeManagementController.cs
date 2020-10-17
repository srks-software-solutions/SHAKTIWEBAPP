using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Xml;
using System.Data.Entity.Validation;
using SRKSDemo.Server_Model;
using SRKSDemo.Models;

namespace SRKSDemo.Controllers
{
    public class ToolLifeManagementController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        // GET: ToolLifeManagement
        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();

            StdToolLife STL = new StdToolLife();
            tblStdToolLife tstl = new tblStdToolLife();
            List<tblStdToolLife> tstlList = new List<tblStdToolLife>();

            tstlList = db.tblStdToolLives.Where(m => m.IsDeleted == false).ToList();
            STL.tblStdToolLifeList = tstlList;
            return View(STL);
        }

        [HttpPost]
        public ActionResult Create(StdToolLife tblstl)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            int UserID = Convert.ToInt32(Session["UserId"]);
            string partno = tblstl.tblStdToolLife.FGCode;
            string opno = tblstl.tblStdToolLife.OperationNo;
            string ToolNo = tblstl.tblStdToolLife.ToolNo;
            bool check = ValidationCheckForInsertion(partno, opno,ToolNo);
            if (check == true)
            {
                tblstl.tblStdToolLife.CreatedOn = DateTime.Now;
                tblstl.tblStdToolLife.CreatedBy = UserID;
                tblstl.tblStdToolLife.IsDeleted = false;
                db.tblStdToolLives.Add(tblstl.tblStdToolLife);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Edit(StdToolLife tblmp, int hdnpid = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            int UserID = Convert.ToInt32(Session["UserId"]);
            string partno = tblmp.tblStdToolLife.FGCode;
            string opno = tblmp.tblStdToolLife.OperationNo;
            string ToolNo = tblmp.tblStdToolLife.ToolNo;
            int mid = hdnpid;
            bool check = ValidationCheckForInsertion(partno, opno, ToolNo, mid);
            if (check == true)
            {
                tblStdToolLife obj = db.tblStdToolLives.Find(mid);
                obj.StdToolLife = tblmp.tblStdToolLife.StdToolLife;
                obj.OperationNo = tblmp.tblStdToolLife.OperationNo;
                obj.ModifiedOn = DateTime.Now; ;
                obj.ModifiedBy = UserID;
                obj.FGCode = tblmp.tblStdToolLife.FGCode;
                obj.ToolNo = tblmp.tblStdToolLife.ToolNo;
                obj.ToolName = tblmp.tblStdToolLife.ToolName;
                obj.CTCode = tblmp.tblStdToolLife.CTCode;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public JsonResult EditValidate(string partnum, int StdPId, string opnum,string toolno)
        {
            string Val = "No";
            if (StdPId != 0)
            {
                var partInMP = db.tblStdToolLives.Where(m => m.IsDeleted == false && (m.FGCode == partnum && m.OperationNo == opnum && m.ToolNo== toolno) && m.StdToolLifeId != StdPId).ToList();
                if (partInMP.Count > 0)
                {
                    Val = "Yes";
                }
                return Json(Val, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var partInMP = db.tblStdToolLives.Where(m => m.IsDeleted == false && m.FGCode == partnum && m.OperationNo == opnum && m.ToolNo == toolno).ToList();
                if (partInMP.Count > 0)
                {
                    Val = "Yes";
                }
                return Json(Val, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Delete(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            int UserID = Convert.ToInt32(Session["UserId"]);
            String Username = Session["Username"].ToString();
            tblStdToolLife tblmp = db.tblStdToolLives.Find(id);
            tblmp.IsDeleted = true;
            tblmp.ModifiedBy = UserID;
            tblmp.ModifiedOn = DateTime.Now;
            db.Entry(tblmp).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public bool ValidationCheckForInsertion(string partno, string opno, string ToolNo,int StdToolLifeId=0 )
        {
            bool count = true;
            int varcount = 0;
            if (StdToolLifeId == 0)
            {
                var countQuery = db.tblStdToolLives.Where(m => m.FGCode == partno && m.OperationNo == opno && m.ToolNo == ToolNo && m.IsDeleted == false).ToList();
                varcount = countQuery.Count();
            }
            else
            {
                var countQuery = db.tblStdToolLives.Where(m => m.StdToolLifeId != StdToolLifeId).Where(m => m.FGCode == partno && m.OperationNo == opno && m.ToolNo == ToolNo && m.IsDeleted == false).ToList();
                varcount = countQuery.Count();
            }

            if (varcount == 0)
            {
                count = true;
            }
            else
            {
                count = false;
            }
            return count;
        }

        public JsonResult GetToolLifeManagementdata(int Id)
        {
            ViewBag.id = Id;
            i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
            var Data = db.tblStdToolLives.Where(m => m.IsDeleted == false && m.StdToolLifeId == Id).Select(m => new { StdToolLifeIdV = m.StdToolLifeId, FGCodeV = m.FGCode,ToolNum=m.ToolNo, OPNo = m.OperationNo, CTCodeV = m.CTCode, ToolNameV = m.ToolName, StdToolLifeV = m.StdToolLife });
            return Json(Data, JsonRequestBehavior.AllowGet);
        }

        #region // upload and download function
        [HttpPost]
        //public ActionResult ImportMasterPartsstsw(HttpPostedFileBase file, string UploadType)
        public ActionResult ImportToolLifeData(HttpPostedFileBase file, string UploadType)
        {
           

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            string UserId = Session["UserId"].ToString();
            //Deleting Excel file
            #region
            string fileLocation1 = Server.MapPath("~/Content/");
            DirectoryInfo di = new DirectoryInfo(fileLocation1);
            FileInfo[] files = di.GetFiles("*.xlsx").Where(p => p.Extension == ".xlsx").ToArray();
            foreach (FileInfo file1 in files)
                try
                {
                    file1.Attributes = FileAttributes.Normal;
                    System.IO.File.Delete(file1.FullName);
                }
                catch { }
            #endregion

            DataSet ds = new DataSet();
            if (Request.Files["file"].ContentLength > 0)
            {

                string fileExtension = System.IO.Path.GetExtension(Request.Files["file"].FileName);
                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    string fileLocation = Server.MapPath("~/Content/") + Request.Files["file"].FileName;
                    if (System.IO.File.Exists(fileLocation))
                    {
                        System.IO.File.Delete(fileLocation);
                    }
                    Request.Files["file"].SaveAs(fileLocation);
                    string excelConnectionString = string.Empty;
                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                    fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    //connection String for xls file format.
                    if (fileExtension == ".xls")
                    {
                        excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    }
                    //connection String for xlsx file format.
                    else if (fileExtension == ".xlsx")
                    {
                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    }
                    //Create Connection to Excel work book and add oledb namespace
                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();
                    DataTable dt = new DataTable();
                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dt == null)
                    {
                        return null;
                    }
                    String[] excelSheets = new String[dt.Rows.Count];
                    int t = 0;
                    //excel data saves in temp file here.
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[t] = row["TABLE_NAME"].ToString();
                        t++;
                    }
                    OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);
                    string query = string.Format("Select * from [{0}]", excelSheets[0]);
                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                    {
                        dataAdapter.Fill(ds);
                    }
                    excelConnection.Close();
                    excelConnection1.Close();
                }
                if (fileExtension.ToString().ToLower().Equals(".xml"))
                {
                    string fileLocation = Server.MapPath("~/Content/") + Request.Files["FileUpload"].FileName;
                    if (System.IO.File.Exists(fileLocation))
                    {
                        System.IO.File.Delete(fileLocation);
                    }
                    Request.Files["FileUpload"].SaveAs(fileLocation);
                    XmlTextReader xmlreader = new XmlTextReader(fileLocation);
                    // DataSet ds = new DataSet();
                    ds.ReadXml(xmlreader);
                    xmlreader.Close();
                }
                if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
                {
                    return RedirectToAction("Login", "Login", null);
                }
                ViewBag.Logout = Session["Username"].ToString().ToUpper();
                ViewBag.roleid = Session["RoleID"];

           
                string text = "";

                string ErrorMsg = null;
         
                if (UploadType == "New") // Delete Duplicate and Insert New. // if not Duplicate insert that
                {
                    #region
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {

                        tblStdToolLife tblmp = new tblStdToolLife();
                        String Username = Session["Username"].ToString();
                        tblmp.CreatedBy = Convert.ToInt32(Session["UserId"]);
                        tblmp.CreatedOn = DateTime.Now;
                        tblmp.IsDeleted = false;
                        
                        string PartName = null, OpNo = null, ToolNo = null, CTCode = null, ToolName = null, StdTooolLife = null ;
                        PartName = Convert.ToString(ds.Tables[0].Rows[i][0]);
                        OpNo = Convert.ToString(ds.Tables[0].Rows[i][1]);
                        ToolNo = Convert.ToString(ds.Tables[0].Rows[i][2]);
                        CTCode = Convert.ToString(ds.Tables[0].Rows[i][3]);
                        ToolName = Convert.ToString(ds.Tables[0].Rows[i][4]);
                        StdTooolLife = Convert.ToString(ds.Tables[0].Rows[i][5]);
                       
                        bool check = ValidationCheckForInsertion(PartName, OpNo,ToolNo);
                        if (check == true)
                        {
                            using (i_facility_shaktiEntities1 db1 = new i_facility_shaktiEntities1())
                            {
                             
                                if (string.IsNullOrEmpty(PartName) || string.IsNullOrEmpty(OpNo) || string.IsNullOrEmpty(ToolNo) || string.IsNullOrEmpty(CTCode) || string.IsNullOrEmpty(StdTooolLife))
                                {
                                    text = text + htmlerrorMaker(PartName, OpNo, "FGCode or OpNo cannot be empty/Check the format");
                                    continue;
                                }
                                else
                                {
                                    try
                                    {
                                        tblmp.FGCode = PartName.Trim();
                                        tblmp.OperationNo = OpNo.Trim();
                                        tblmp.ToolNo = ToolNo.Trim();
                                        tblmp.CTCode = CTCode.Trim();
                                        tblmp.ToolName = ToolName.Trim();
                                        tblmp.StdToolLife = Convert.ToInt32(StdTooolLife.Trim());
                                        tblmp.CreatedBy = Convert.ToInt32(UserId);
                                        tblmp.CreatedOn = DateTime.Now;
                                    }
                                    catch {

                                    }
                                }
                            }
                            //check for dup and delete previous one.
                            var Dupdata = db.tblStdToolLives.Where(m => m.FGCode == PartName && m.OperationNo == OpNo && m.ToolNo==ToolNo && m.IsDeleted == false).FirstOrDefault();
                            if (Dupdata != null)
                            {
                                Dupdata.IsDeleted = true;
                                Dupdata.ModifiedOn = DateTime.Now;
                                Dupdata.ModifiedBy = Convert.ToInt32(UserId);
                                db.Entry(Dupdata).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }

                            db.tblStdToolLives.Add(tblmp);
                            try { 
                            db.SaveChanges();
                            }
                            catch (DbEntityValidationException e)
                            {
                                foreach (var eve in e.EntityValidationErrors)
                                {
                                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                    foreach (var ve in eve.ValidationErrors)
                                    {
                                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                            ve.PropertyName, ve.ErrorMessage);
                                    }
                                }
                                throw;
                            }
                        }
                    }
                    #endregion
                }
                else if (UploadType == "Update") // OverWrite Existing Values 
                {
                    #region
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        tblStdToolLife tblmp = new tblStdToolLife();
                        String Username = Session["Username"].ToString();
                        tblmp.CreatedBy = Convert.ToInt32(Session["UserId"]);
                        tblmp.CreatedOn = DateTime.Now;
                        tblmp.IsDeleted = false;

                        string PartName = null, OpNo = null, ToolNo = null, CTCode = null, ToolName = null, StdTooolLife = null;
                        PartName = Convert.ToString(ds.Tables[0].Rows[i][0]);
                        OpNo = Convert.ToString(ds.Tables[0].Rows[i][1]);
                        ToolNo = Convert.ToString(ds.Tables[0].Rows[i][2]);
                        CTCode = Convert.ToString(ds.Tables[0].Rows[i][3]);
                        ToolName = Convert.ToString(ds.Tables[0].Rows[i][4]);
                        StdTooolLife = Convert.ToString(ds.Tables[0].Rows[i][5]);

                        DateTime createdOn = DateTime.Now;

                        if (string.IsNullOrEmpty(PartName) || string.IsNullOrEmpty(OpNo) || string.IsNullOrEmpty(ToolNo) || string.IsNullOrEmpty(CTCode) || string.IsNullOrEmpty(StdTooolLife))
                        {
                            text = text + htmlerrorMaker(PartName, OpNo, "FGCode or OpNo cannot be empty/Check the format");
                            continue;
                        }
                        else
                        {
                            tblmp.FGCode = PartName.Trim();
                            tblmp.OperationNo = OpNo.Trim();
                            tblmp.ToolNo = ToolNo.Trim();
                            tblmp.CTCode = CTCode.Trim();
                            tblmp.ToolName = ToolName.Trim();
                            tblmp.StdToolLife = Convert.ToInt32(StdTooolLife.Trim());
                            tblmp.CreatedBy = Convert.ToInt32(UserId);
                            tblmp.CreatedOn = DateTime.Now;
                        }
                       
                        var MasterStdPWTData = db.tblStdToolLives.Where(m => m.FGCode == PartName && m.OperationNo == OpNo && m.ToolNo == ToolNo && m.IsDeleted == false).FirstOrDefault();
                        if (MasterStdPWTData == null)
                        {
                            db.tblStdToolLives.Add(tblmp);
                            try
                            {
                                db.SaveChanges();
                            }
                            catch (DbEntityValidationException e)
                            {
                                foreach (var eve in e.EntityValidationErrors)
                                {
                                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                    foreach (var ve in eve.ValidationErrors)
                                    {
                                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                            ve.PropertyName, ve.ErrorMessage);
                                    }
                                }
                                throw;
                            }
                        }
                        else
                        {

                            MasterStdPWTData.ModifiedBy = Convert.ToInt32(Session["UserId"]);
                            MasterStdPWTData.ModifiedOn = DateTime.Now;
                            MasterStdPWTData.IsDeleted = true;
                            db.Entry(MasterStdPWTData).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.tblStdToolLives.Add(tblmp);
                                try
                                {
                                    db.SaveChanges();
                            }
                            catch (DbEntityValidationException e)
                            {
                                foreach (var eve in e.EntityValidationErrors)
                                {
                                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                    foreach (var ve in eve.ValidationErrors)
                                    {
                                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                            ve.PropertyName, ve.ErrorMessage);
                                    }
                                }
                                throw;
                            }
                        }

                    }
                    #endregion
                }
                TempData["txtShow"] = text;
                // ViewBag.text = text;
                Session["PartNo"] = ErrorMsg;
            }

            //return RedirectToAction("Index", "MasterParts");
            return RedirectToAction("Index", "ToolLifeManagement");
        }
       
        public ActionResult ExportToolLifeData()
        {
            #region Excel and Stuff
            DateTime frda = DateTime.Now;
            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "StandardToolLife" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "StandardToolLife" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
                }
                catch
                {
                    TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
                    //return View();
                }
            }
            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\StdToolLifeFormat.xlsx");
            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
            //Using the File for generation and populating it
            ExcelPackage p = null;
            p = new ExcelPackage(newFile);
            ExcelWorksheet worksheet = null;

            //Creating the WorkSheet for populating
            try
            {
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
            }
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            #endregion

            var BreakDownLossesData = db.tblStdToolLives.Where(m => m.IsDeleted == false).ToList();

            int i = 2;
            foreach (var row in BreakDownLossesData)
            {

                worksheet.Cells["A" + i].Value = row.FGCode;
                worksheet.Cells["B" + i].Value = row.OperationNo;
                worksheet.Cells["C" + i].Value = row.ToolNo;
                worksheet.Cells["D" + i].Value = row.CTCode; 
                worksheet.Cells["E" + i].Value = row.ToolName;
                worksheet.Cells["F" + i].Value = row.StdToolLife;
                i++;
            }

            if (BreakDownLossesData.Count != 0)
            {
                #region Save and Download

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                p.Save();

                //Downloding Excel
                string path1 = System.IO.Path.Combine(FileDir, "StandardToolLife" + frda.ToString("yyyy-MM-dd") + ".xlsx");
                System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
                string Outgoingfile = "StandardToolLife" + frda.ToString("yyyy-MM-dd") + ".xlsx";
                if (file1.Exists)
                {
                    Response.Clear();
                    Response.ClearContent();
                    Response.ClearHeaders();
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + Outgoingfile);
                    Response.AddHeader("Content-Length", file1.Length.ToString());
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.WriteFile(file1.FullName);
                    Response.Flush();
                    Response.Close();
                }
                #endregion
            }
            else
            {
                // return Content("<script language='javascript' type='text/javascript'>alert('Data Doesnt Exists');</script>");
            }
            return RedirectToAction("Index");

        }
        #endregion
        public string htmlerrorMaker(string partno, string opno, string message)
        {
            string val = "";

            val = "<tr><td>" + partno + "</td><td>" + opno + "</td><td>" + message + "</td></tr>";

            return val;
        }
    }
}