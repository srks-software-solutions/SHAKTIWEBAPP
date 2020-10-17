using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using OfficeOpenXml;
using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Xml;
using System.Text;
using System.Data.Entity.Validation;
using SRKSDemo.Server_Model;

namespace SRKSDemo.Controllers
{
    public class PartsManagementController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        // GET: PartsManagement
        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();

            PartsManagement PM = new PartsManagement();
            tblpart mp = new tblpart();
            List<tblpart> mplist = new List<tblpart>();
            PM.MasterParts = mp;
            mplist = db.tblparts.Where(m => m.IsDeleted == 0).ToList();
            PM.MasterPartsList = mplist;
            ViewBag.Unit = new SelectList(db.tblunits.Where(p => p.IsDeleted == 0), "U_ID", "UnitDesc");
            // ViewData["txtShow"] = "";
            //ViewBag.text = "";
            return View(PM);
        }

        [HttpPost]
        public ActionResult Create(PartsManagement tblmp)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            int UserID = Convert.ToInt32(Session["UserId"]);
            string partno = tblmp.MasterParts.FGCode;
            string opno = tblmp.MasterParts.OperationNo;
            bool check = ValidationCheckForInsertion(partno, opno);
            if (check == true)
            {
                tblmp.MasterParts.CreatedOn = DateTime.Now;
                tblmp.MasterParts.CreatedBy = UserID;
                tblmp.MasterParts.IsDeleted = 0;
                db.tblparts.Add(tblmp.MasterParts);
                db.SaveChanges();
            }
            ViewBag.Unit = new SelectList(db.tblunits.Where(p => p.IsDeleted == 0), "U_ID", "UnitDesc");
            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult Edit(PartsManagement tblmp, int hdnpid = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            int UserID = Convert.ToInt32(Session["UserId"]);
            string partno = tblmp.MasterParts.FGCode;
            string opno = tblmp.MasterParts.OperationNo;
            int mid = hdnpid;
            bool check = ValidationCheckForInsertion(partno, opno, mid);
            if (check == true)
            {
                var item = db.tblparts.Find(mid);
                item.FGCode = partno;
                item.OperationNo = opno;
                item.IdealCycleTime = tblmp.MasterParts.IdealCycleTime;
                item.StdMinorLoss = tblmp.MasterParts.StdMinorLoss;
                item.UnitDesc = tblmp.MasterParts.UnitDesc;
                db.SaveChanges();
            }
            ViewBag.Unit = new SelectList(db.tblunits.Where(p => p.IsDeleted == 0), "U_ID", "UnitDesc");
            return RedirectToAction("Index");
        }

        public JsonResult EditValidate(string partnum, int PkId, string opnum)
        {
            string Val = "No";
            if (PkId != 0)
            {
                var partInMP = db.tblparts.Where(m => m.IsDeleted == 0 && (m.FGCode == partnum && m.OperationNo == opnum) && m.PartID != PkId).ToList();
                if (partInMP.Count > 0)
                {
                    Val = "Yes";
                }
                return Json(Val, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var partInMP = db.tblparts.Where(m => m.IsDeleted == 0 && m.OperationNo == opnum).ToList();
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
            tblpart tblmp = db.tblparts.Find(id);
            tblmp.IsDeleted = 1;
            tblmp.ModifiedBy = UserID;
            tblmp.ModifiedOn = DateTime.Now;
            tblmp.DeletedDate = DateTime.Now;
            db.Entry(tblmp).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public bool ValidationCheckForInsertion(string partno, string opno, int partId = 0)
        {
            bool count = true;
            int varcount = 0;
            if (partId == 0)
            {
                var countQuery = db.tblparts.Where(m => m.FGCode == partno && m.OperationNo == opno && m.IsDeleted == 0).ToList();
                varcount = countQuery.Count();
            }
            else
            {
                var countQuery = db.tblparts.Where(m => m.PartID != partId).Where(m => m.FGCode == partno && m.OperationNo == opno && m.IsDeleted == 0).ToList();
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

        #region
        [HttpPost]
        //public ActionResult ImportMasterPartsstsw(HttpPostedFileBase file, string UploadType)
        public ActionResult ImportMasterPartsstsw(HttpPostedFileBase file, string UploadType)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

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

                var unitsItem = db.tblunits.Where(m => m.IsDeleted == 0).ToList();

                List<string> TimeUnits = new List<string>();
                foreach (var item in unitsItem)
                {
                    TimeUnits.Add(item.Unit.ToString());
                }

                string text = "";

                string ErrorMsg = null;
                if (UploadType == "OverWrite") // Accept only New Codes
                {
                    #region
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {

                        tblpart tblmp = new tblpart();
                        String Username = Session["Username"].ToString();
                        tblmp.CreatedBy = Convert.ToInt32(Session["UserId"]);
                        tblmp.CreatedOn = DateTime.Now;
                        tblmp.IsDeleted = 0;

                        string PartName = null, OperationNo = null;
                        PartName = Convert.ToString(ds.Tables[0].Rows[i][0]);
                        OperationNo = Convert.ToString(ds.Tables[0].Rows[i][1]);

                        using (i_facility_shaktiEntities1 db1 = new i_facility_shaktiEntities1())
                        {
                            var MasterStdPWTData = db1.tblparts.Where(m => m.FGCode == PartName && m.OperationNo == OperationNo).FirstOrDefault();

                            if (MasterStdPWTData != null)
                            {
                                try
                                {
                                    try
                                    {
                                        string unitDesc = Convert.ToString(ds.Tables[0].Rows[i][4]);
                                        if (TimeUnits.Contains(unitDesc))
                                        {
                                            int? unitId = db.tblunits.Where(m => m.Unit == unitDesc).Select(m => m.U_ID).FirstOrDefault();
                                            MasterStdPWTData.UnitDesc = Convert.ToInt32(unitId);
                                        }
                                        else //Default Unit
                                        {
                                            MasterStdPWTData.UnitDesc = 1;
                                        }
                                    }
                                    catch
                                    {
                                        text = text + htmlerrorMaker(PartName, OperationNo, "Please check with Unit");
                                        continue;
                                    }

                                    //ErrorMsg = ErrorMsg + PartName + " and " + OperationNo + " has details in Database.\n";
                                    try
                                    {
                                        MasterStdPWTData.IdealCycleTime = Convert.ToDecimal(ds.Tables[0].Rows[i][2]);

                                    }
                                    catch
                                    {
                                        text = text + htmlerrorMaker(PartName, OperationNo, "Please check with Std Cycle Time");
                                        continue;
                                    }
                                    try
                                    {
                                        MasterStdPWTData.StdMinorLoss = Convert.ToString(ds.Tables[0].Rows[i][3]);

                                    }
                                    catch
                                    {
                                        text = text + htmlerrorMaker(PartName, OperationNo, "Please check with Std Minor Loss");
                                        continue;
                                    }
                                    MasterStdPWTData.ModifiedOn = DateTime.Now;
                                    MasterStdPWTData.ModifiedBy = Convert.ToInt32(Session["UserId"]);
                                    //db.Entry(MasterStdPWTData).State = System.Data.Entity.EntityState.Modified;
                                    db1.SaveChanges();
                                    continue;
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
                                    //throw;
                                }
                            }
                            else if (string.IsNullOrEmpty(PartName) || string.IsNullOrEmpty(OperationNo))
                            {
                                text = text + htmlerrorMaker(PartName, OperationNo, "PartName or OperationNo cannot be empty");
                                //ErrorMsg += " PartName or OperationNo cannot be empty for " + PartName + " and " + OperationNo + " .\n";
                                continue;
                            }
                            else
                            {
                                tblmp.FGCode = PartName;
                                tblmp.OperationNo = OperationNo;
                            }
                        }
                        try
                        {
                            string unitDesc = Convert.ToString(ds.Tables[0].Rows[i][4]);
                            if (TimeUnits.Contains(unitDesc))
                            {
                                int? unitId = db.tblunits.Where(m => m.Unit == unitDesc).Select(m => m.U_ID).FirstOrDefault();
                                tblmp.UnitDesc = Convert.ToInt32(unitId);
                            }
                            else //Default Unit
                            {
                                tblmp.UnitDesc = 1;
                            }
                        }
                        catch
                        {
                            text = text + htmlerrorMaker(PartName, OperationNo, "Please check with Unit");
                            continue;
                        }
                        try
                        {
                            tblmp.IdealCycleTime = Convert.ToDecimal(ds.Tables[0].Rows[i][2]);

                        }
                        catch
                        {
                            text = text + htmlerrorMaker(PartName, OperationNo, "Please check with IdealCycleTime ");
                            continue;
                        }
                        try
                        {
                            tblmp.StdMinorLoss = Convert.ToString(ds.Tables[0].Rows[i][3]);

                        }
                        catch
                        {
                            text = text + htmlerrorMaker(PartName, OperationNo, "Please check with Std Minor Loss");
                            continue;
                        }

                        db.tblparts.Add(tblmp);
                        db.SaveChanges();

                    }
                    #endregion
                }
                else if (UploadType == "New") // Delete Duplicate and Insert New. // if not Duplicate insert that
                {
                    #region
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {

                        tblpart tblmp = new tblpart();
                        String Username = Session["Username"].ToString();
                        tblmp.CreatedBy = Convert.ToInt32(Session["UserId"]);
                        tblmp.CreatedOn = DateTime.Now;
                        tblmp.IsDeleted = 0;

                        string PartName = null, OperationNo = null;
                        PartName = Convert.ToString(ds.Tables[0].Rows[i][0]);
                        OperationNo = Convert.ToString(ds.Tables[0].Rows[i][1]);
                        bool check = ValidationCheckForInsertion(PartName, OperationNo);
                        if (check == true)
                        {
                            using (i_facility_shaktiEntities1 db1 = new i_facility_shaktiEntities1())
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(PartName) || string.IsNullOrEmpty(OperationNo))
                                    {
                                        text = text + htmlerrorMaker(PartName, OperationNo, "PartName or OperationNo cannot be empty");
                                        continue;
                                    }
                                    else
                                    {
                                        tblmp.FGCode = PartName;
                                        tblmp.OperationNo = OperationNo;
                                    }

                                    try
                                    {
                                        string unitDesc = Convert.ToString(ds.Tables[0].Rows[i][4]);
                                        if (TimeUnits.Contains(unitDesc))
                                        {
                                            int? unitId = db.tblunits.Where(m => m.Unit == unitDesc).Select(m => m.U_ID).FirstOrDefault();
                                            tblmp.UnitDesc = Convert.ToInt32(unitId);
                                        }
                                        else //Default Unit
                                        {
                                            tblmp.UnitDesc = 1;
                                        }
                                    }
                                    catch
                                    {
                                        text = text + htmlerrorMaker(PartName, OperationNo, "Please check with Unit");
                                        continue;
                                    }
                                    try
                                    {
                                        tblmp.IdealCycleTime = Convert.ToDecimal(ds.Tables[0].Rows[i][2]);

                                    }
                                    catch
                                    {
                                        text = text + htmlerrorMaker(PartName, OperationNo, "Please check with IdealCycleTime ");
                                        continue;
                                    }
                                    try
                                    {
                                        tblmp.StdMinorLoss = Convert.ToString(ds.Tables[0].Rows[i][3]);

                                    }
                                    catch
                                    {
                                        text = text + htmlerrorMaker(PartName, OperationNo, "Please check with Std Minor Loss");
                                        continue;
                                    }

                                    //check for dup and delete previous one.
                                    var Dupdata = db.tblparts.Where(m => m.FGCode == PartName && m.OperationNo == OperationNo && m.IsDeleted == 0).FirstOrDefault();
                                    if (Dupdata != null)
                                    {
                                        Dupdata.IsDeleted = 1;
                                        Dupdata.DeletedDate = DateTime.Now;
                                        //db.Entry(Dupdata).State = System.Data.Entity.EntityState.Modified;
                                        db1.SaveChanges();
                                    }

                                    db1.tblparts.Add(tblmp);
                                    db1.SaveChanges();
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
                                    //throw;
                                }
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
                        tblpart tblmp = new tblpart();
                        String Username = Session["Username"].ToString();
                        tblmp.CreatedBy = Convert.ToInt32(Session["UserId"]);
                        tblmp.CreatedOn = DateTime.Now;
                        tblmp.IsDeleted = 0;

                        string PartName = null, OperationNo = null;
                        PartName = Convert.ToString(ds.Tables[0].Rows[i][0]);
                        OperationNo = Convert.ToString(ds.Tables[0].Rows[i][1]);

                        DateTime createdOn = DateTime.Now;

                        if (string.IsNullOrEmpty(PartName) || string.IsNullOrEmpty(OperationNo))
                        {
                            text = text + htmlerrorMaker(PartName, OperationNo, "PartName or OperationNo cannot be empty");
                            continue;
                        }
                        else
                        {
                            tblmp.FGCode = PartName;
                            tblmp.OperationNo = OperationNo;
                        }
                        try
                        {
                            string unitDesc = Convert.ToString(ds.Tables[0].Rows[i][4]);
                            if (TimeUnits.Contains(unitDesc))
                            {
                                int? unitId = db.tblunits.Where(m => m.Unit == unitDesc).Select(m => m.U_ID).FirstOrDefault();
                                tblmp.UnitDesc = Convert.ToInt32(unitId);
                            }
                            else //Default Unit
                            {
                                tblmp.UnitDesc = 1;
                            }
                        }
                        catch
                        {
                            text = text + htmlerrorMaker(PartName, OperationNo, "Please check with Unit");
                            continue;
                        }
                        try
                        {
                            tblmp.IdealCycleTime = Convert.ToDecimal(ds.Tables[0].Rows[i][2]);

                        }
                        catch
                        {
                            text = text + htmlerrorMaker(PartName, OperationNo, "Please check with IdealCycleTime ");
                            continue;
                        }
                        try
                        {
                            tblmp.StdMinorLoss = Convert.ToString(ds.Tables[0].Rows[i][3]);

                        }
                        catch
                        {
                            text = text + htmlerrorMaker(PartName, OperationNo, "Please check with Std Minor Loss");
                            continue;
                        }


                        var MasterStdPWTData = db.tblparts.Where(m => m.FGCode == PartName && m.OperationNo == OperationNo && m.IsDeleted == 0).FirstOrDefault();
                        if (MasterStdPWTData == null)
                        {
                            db.tblparts.Add(tblmp);
                            db.SaveChanges();
                        }
                        else
                        {

                            MasterStdPWTData.ModifiedBy = Convert.ToInt32(Session["UserId"]);
                            MasterStdPWTData.ModifiedOn = DateTime.Now;
                            MasterStdPWTData.IsDeleted = 1;
                            MasterStdPWTData.DeletedDate = DateTime.Now;
                            db.Entry(MasterStdPWTData).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.tblparts.Add(tblmp);
                            db.SaveChanges();
                        }

                    }
                    #endregion
                }
                TempData["txtShow"] = text;
                // ViewBag.text = text;
                Session["FGCode"] = ErrorMsg;
            }

            //return RedirectToAction("Index", "MasterParts");
            return RedirectToAction("Index", "PartsManagement");
        }
        #endregion

        public JsonResult GetpartsManagementdata(int Id)
        {
            ViewBag.id = Id;
            i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
            var Data = db.tblparts.Where(m => m.IsDeleted == 0 && m.PartID == Id).Select(m => new { Pid = m.PartID, partname = m.FGCode, Operationum = m.OperationNo, StdMinorLoss = m.IdealCycleTime, IdealCycleTime = m.IdealCycleTime, UnitDesc = m.UnitDesc, pid = Id });
            return Json(Data, JsonRequestBehavior.AllowGet);
        }
        //  public JsonResult GetSetUnittime(int Id)
        // {
        //    var MachineData = (from row in db.tblpart
        //                       where row.IsDeleted == 0 && row.PartID == Id
        //                       select new { Value = row.PartID, Text = row.StdSetupTimeUnit });
        //    return Json(MachineData, JsonRequestBehavior.AllowGet);
        /// }

        public ActionResult ExportMasterPartsSTDWeightTime()
        {
            #region Excel and Stuff
            DateTime frda = DateTime.Now;
            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "MasterParts" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "MasterParts" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
                }
                catch
                {
                    TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
                    //return View();
                }
            }
            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\IdealCycleTimeFormat.xlsx");
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

            var BreakDownLossesData = db.tblparts.Where(m => m.IsDeleted == 0).ToList();

            int i = 2;
            foreach (var row in BreakDownLossesData)
            {

                worksheet.Cells["A" + i].Value = row.FGCode;
                worksheet.Cells["B" + i].Value = row.OperationNo;
                worksheet.Cells["C" + i].Value = row.IdealCycleTime;
                if (row.StdMinorLoss == null)
                {
                    worksheet.Cells["D" + i].Value = 0;
                }
                else
                {
                    worksheet.Cells["D" + i].Value = row.StdMinorLoss;
                }
                var unitdet = db.tblunits.Where(m => m.U_ID == row.UnitDesc).Select(m => m.Unit).FirstOrDefault();
                if(unitdet !=null)
                {
                    worksheet.Cells["E" + i].Value = unitdet;
                }
              

                i++;
            }

            if (BreakDownLossesData.Count != 0)
            {
                #region Save and Download

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                p.Save();

                //Downloding Excel
                string path1 = System.IO.Path.Combine(FileDir, "MasterParts" + frda.ToString("yyyy-MM-dd") + ".xlsx");
                System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
                string Outgoingfile = "MasterParts" + frda.ToString("yyyy-MM-dd") + ".xlsx";
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
                TempData["toaster_warning"] = "Data doesn't exists";
                // return Content("<script language='javascript' type='text/javascript'>alert('Data Doesnt Exists');</script>");
            }
            return RedirectToAction("Index");

        }

        public string htmlerrorMaker(string partno, string opno, string message)
        {
            string val = "";

            val = "<tr><td>" + partno + "</td><td>" + opno + "</td><td>" + message + "</td></tr>";

            return val;
        }

    }
}