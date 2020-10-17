using OfficeOpenXml;
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace SRKSDemo.Controllers
{
    public class MasterPartsController : Controller
    {
        private i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        public ActionResult Index(int takeValue = 10, int skipValue = 0, int pageNo = 1)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            Session["PageNo"] = pageNo;
            var tblmp = db.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0).OrderByDescending(x => x.PARTSSTSWID).Skip(skipValue).ToList().Take(takeValue);
            return View(tblmp.ToList());
        }

        public JsonResult IndexDetailsForLassyLoading(int takeValue = 0, int skipValue = 0, int pageNo = 1)
        {
            var tblmp = db.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0).OrderByDescending(x => x.PARTSSTSWID).Skip(skipValue).ToList().Take(takeValue);
            return Json(tblmp, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SearchSelectedItem(string partNo)
        {
            var partNoDet = db.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0 && m.PartNo == partNo).ToList();
            return Json(partNoDet, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            ViewBag.Part = new SelectList(db.tblparts.Where(m => m.IsDeleted == 0), "PartID", "PartName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tblmasterparts_st_sw tblmp)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            //ActiveLog Code
            int UserID = Convert.ToInt32(Session["UserId"]);
            //string CompleteModificationdetail = "New Creation";
            //Action = "Create";
            //ActiveLogStorage Obj = new ActiveLogStorage();
            //Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End

            string partno = tblmp.PartNo;
            string opno = tblmp.OpNo;
            var partInMP = db.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0 && m.PartNo == partno && m.OpNo == opno).ToList();
            if (partInMP.Count > 0)
            {
                Session["Error"] = "PartNo " + partno + " has details in Database";
                return View(tblmp);
            }


            tblmp.CreatedOn = DateTime.Now;
            tblmp.CreatedBy = UserID;
            tblmp.IsDeleted = 0;

            db.tblmasterparts_st_sw.Add(tblmp);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            tblmasterparts_st_sw masterpartsData = db.tblmasterparts_st_sw.Where(m => m.PARTSSTSWID == id).FirstOrDefault();
            return View(masterpartsData);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(tblmasterparts_st_sw tblmp)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            //ActiveLog Code
            int UserID = Convert.ToInt32(Session["UserId"]);
            //string CompleteModificationdetail = "New Creation";
            //Action = "Create";
            //ActiveLogStorage Obj = new ActiveLogStorage();
            //Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End

            string partno = tblmp.PartNo;
            string opno = tblmp.OpNo;
            int mpid = tblmp.PARTSSTSWID;
            var partInMP = db.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0 && (m.PartNo == partno && m.OpNo == opno) && m.PARTSSTSWID != mpid).ToList();
            if (partInMP.Count > 0)
            {
                Session["Error"] = "PartNo " + partno + " has details in Database";
                return View(tblmp);
            }


            tblmp.CreatedOn = DateTime.Now;
            tblmp.CreatedBy = UserID;
            tblmp.IsDeleted = 0;

            db.Entry(tblmp).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public string EditDetails(tblmasterparts_st_sw tblmp)
        {
            string ret = "";
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            //ActiveLog Code
            int UserID = Convert.ToInt32(Session["UserId"]);
            //string CompleteModificationdetail = "New Creation";
            //Action = "Create";
            //ActiveLogStorage Obj = new ActiveLogStorage();
            //Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End

            string partno = tblmp.PartNo;
            string opno = tblmp.OpNo;
            int mpid = tblmp.PARTSSTSWID;
            var partInMP = db.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0 && (m.PartNo == partno && m.OpNo == opno) && m.PARTSSTSWID != mpid).ToList();
            if (partInMP.Count > 0)
            {
                Session["Error"] = "PartNo " + partno + " has details in Database";
                //return View(tblmp);
            }


            tblmp.CreatedOn = DateTime.Now;
            tblmp.CreatedBy = UserID;
            tblmp.IsDeleted = 0;

            db.Entry(tblmp).State = EntityState.Modified;
            db.SaveChanges();
            ret = "Success";
            return ret;
        }

        public ActionResult Delete(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            //start Logging
            int UserID = Convert.ToInt32(Session["UserId"]);
            String Username = Session["Username"].ToString();
            //string CompleteModificationdetail = "Deleted MachineDetails";
            //Action = "Delete";
            //ActiveLogStorage Obj = new ActiveLogStorage();
            //Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End

            tblmasterparts_st_sw tblmp = db.tblmasterparts_st_sw.Find(id);
            tblmp.IsDeleted = 1;
            tblmp.ModifiedBy = UserID;
            tblmp.ModifiedOn = DateTime.Now;
            db.Entry(tblmp).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public string DeleteDet(int id)
        {
            string ret = "";
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            //start Logging
            int UserID = Convert.ToInt32(Session["UserId"]);
            String Username = Session["Username"].ToString();
            //string CompleteModificationdetail = "Deleted MachineDetails";
            //Action = "Delete";
            //ActiveLogStorage Obj = new ActiveLogStorage();
            //Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End

            tblmasterparts_st_sw tblmp = db.tblmasterparts_st_sw.Find(id);
            tblmp.IsDeleted = 1;
            tblmp.ModifiedBy = UserID;
            tblmp.ModifiedOn = DateTime.Now;
            db.Entry(tblmp).State = EntityState.Modified;
            db.SaveChanges();
            ret = "Success";
            return ret;
        }

        public ActionResult ImportMasterPartsstsw()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            return View();
        }
        [HttpPost]
        //public ActionResult ImportMasterPartsstsw(HttpPostedFileBase file,string UploadType)
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
                ViewBag.Logout = Session["Username"];
                ViewBag.roleid = Session["RoleID"];

                List<string> TimeUnits = new List<string>();
                TimeUnits.Add("Min");
                TimeUnits.Add("Hrs");
                TimeUnits.Add("Sec");

                List<string> WeightUnits = new List<string>();
                WeightUnits.Add("Kg");
                WeightUnits.Add("g");

                string ErrorMsg = null;
                if (UploadType == "OverWrite") // Accept only New Codes
                {
                    #region
                    for (int i = 1; i < ds.Tables[0].Rows.Count; i++)
                    {

                        tblmasterparts_st_sw tblmp = new tblmasterparts_st_sw();
                        String Username = Session["Username"].ToString();
                        tblmp.CreatedBy = Convert.ToInt32(Session["UserId"]);
                        tblmp.CreatedOn = DateTime.Now;
                        tblmp.IsDeleted = 0;

                        string PartName = null, OpNo = null;
                        PartName = Convert.ToString(ds.Tables[0].Rows[i][0]);
                        OpNo = Convert.ToString(ds.Tables[0].Rows[i][1]);

                        using (i_facility_shaktiEntities1 db1 = new i_facility_shaktiEntities1())
                        {
                            var MasterStdPWTData = db1.tblmasterparts_st_sw.Where(m => m.PartNo == PartName && m.OpNo == OpNo).FirstOrDefault();
                            if (MasterStdPWTData != null)
                            {
                                ErrorMsg = ErrorMsg + PartName + " and " + OpNo + " has details in Database.\n";
                                continue;
                            }
                            else if (string.IsNullOrEmpty(PartName) || string.IsNullOrEmpty(OpNo))
                            {
                                ErrorMsg += " PartName or OpNo cannot be empty for " + PartName + " and " + OpNo + " .\n";
                                continue;
                            }
                            else
                            {
                                tblmp.PartNo = PartName;
                                tblmp.OpNo = OpNo;
                            }
                        }
                        try
                        {
                            tblmp.StdSetupTime = Convert.ToDecimal(ds.Tables[0].Rows[i][2]);
                            string setupUnit = Convert.ToString(ds.Tables[0].Rows[i][3]);
                            if (TimeUnits.Contains(setupUnit))
                            {
                                tblmp.StdSetupTimeUnit = setupUnit;
                            }
                            else //Default Unit
                            {
                                tblmp.StdSetupTimeUnit = "Min";
                            }
                        }
                        catch
                        {
                            ErrorMsg = ErrorMsg + "StdSetupTime of Part Number" + PartName + " should be Number.\n";
                            continue;
                        }
                        decimal stdCuttingTime = 0;
                        try
                        {
                            stdCuttingTime = Convert.ToDecimal(ds.Tables[0].Rows[i][4]);
                        }
                        catch
                        {
                            ErrorMsg = ErrorMsg + "StdCuttingTime of " + PartName + " should be Numbers.\n";
                            continue;
                        }
                        //if (stdCuttingTime == 0)
                        //{
                        //    ErrorMsg = ErrorMsg + "StdCuttingTime of " + PartName + " cannot be Zero.\n";
                        //    continue;
                        //}
                        //else
                        {
                            tblmp.StdCuttingTime = stdCuttingTime;
                            string CuttingTimeUnit = Convert.ToString(ds.Tables[0].Rows[i][5]);
                            if (TimeUnits.Contains(CuttingTimeUnit))
                            {
                                tblmp.StdCuttingTimeUnit = CuttingTimeUnit;
                            }
                            else //Default Unit
                            {
                                tblmp.StdCuttingTimeUnit = "Min";
                            }
                        }

                        try
                        {
                            tblmp.StdChangeoverTime = Convert.ToDecimal(ds.Tables[0].Rows[i][6]);
                            string ChangeoverUnit = Convert.ToString(ds.Tables[0].Rows[i][7]);
                            if (TimeUnits.Contains(ChangeoverUnit))
                            {
                                tblmp.StdChangeoverTimeUnit = ChangeoverUnit;
                            }
                            else //Default Unit
                            {
                                tblmp.StdChangeoverTimeUnit = "Min";
                            }
                        }
                        catch
                        {
                            ErrorMsg = ErrorMsg + "StdChangeoverTime of Part Number" + PartName + " should be Number.\n";
                            continue;
                        }

                        try
                        {
                            tblmp.InputWeight = Convert.ToDecimal(ds.Tables[0].Rows[i][8]);
                            string InputWeightUnit = Convert.ToString(ds.Tables[0].Rows[i][9]);
                            if (WeightUnits.Contains(InputWeightUnit))
                            {
                                tblmp.InputWeightUnit = InputWeightUnit;
                            }
                            else //Default Unit
                            {
                                tblmp.InputWeightUnit = "Kg";
                            }

                            tblmp.OutputWeight = Convert.ToDecimal(ds.Tables[0].Rows[i][10]);
                            string OutputWeightUnit = Convert.ToString(ds.Tables[0].Rows[i][11]);
                            if (WeightUnits.Contains(OutputWeightUnit))
                            {
                                tblmp.OutputWeightUnit = OutputWeightUnit;
                            }
                            else //Default Unit
                            {
                                tblmp.OutputWeightUnit = "Kg";
                            }

                            tblmp.MaterialRemovedQty = Convert.ToDecimal(ds.Tables[0].Rows[i][12]);
                            string MaterialRemovedQtyUnit = Convert.ToString(ds.Tables[0].Rows[i][13]);
                            if (WeightUnits.Contains(MaterialRemovedQtyUnit))
                            {
                                tblmp.MaterialRemovedQtyUnit = MaterialRemovedQtyUnit;
                            }
                            else //Default Unit
                            {
                                tblmp.MaterialRemovedQtyUnit = "Kg";
                            }
                        }
                        catch
                        {
                            ErrorMsg = ErrorMsg + "I/O Weights & Materials Removed of " + PartName + " should be Numbers.\n";
                            continue;
                        }

                        db.tblmasterparts_st_sw.Add(tblmp);
                        db.SaveChanges();
                    }
                    #endregion
                }
                else if (UploadType == "New") // Delete Duplicate and Insert New. // if not Duplicate insert that
                {
                    #region
                    for (int i = 1; i < ds.Tables[0].Rows.Count; i++)
                    {

                        tblmasterparts_st_sw tblmp = new tblmasterparts_st_sw();
                        String Username = Session["Username"].ToString();
                        tblmp.CreatedBy = Convert.ToInt32(Session["UserId"]);
                        tblmp.CreatedOn = DateTime.Now;
                        tblmp.IsDeleted = 0;

                        string PartName = null, OpNo = null;
                        PartName = Convert.ToString(ds.Tables[0].Rows[i][0]);
                        OpNo = Convert.ToString(ds.Tables[0].Rows[i][1]);

                        using (i_facility_shaktiEntities1 db1 = new i_facility_shaktiEntities1())
                        {
                            //var MasterStdPWTData = db1.tblmasterparts_st_sw.Where(m => m.PartNo == PartName && m.OpNo == OpNo).FirstOrDefault();
                            //if (MasterStdPWTData != null)
                            //{
                            //    ErrorMsg +=  PartName + " and " + OpNo + " has details in Database.\n";
                            //    continue;
                            //}
                            //else 
                            if (string.IsNullOrEmpty(PartName) || string.IsNullOrEmpty(OpNo))
                            {
                                ErrorMsg += "PartNo and OperationNo cannot be null .\n";
                                continue;
                            }
                            else
                            {
                                tblmp.PartNo = PartName;
                                tblmp.OpNo = OpNo;
                            }
                        }
                        try
                        {
                            tblmp.StdSetupTime = Convert.ToDecimal(ds.Tables[0].Rows[i][2]);
                            string setupUnit = Convert.ToString(ds.Tables[0].Rows[i][3]);
                            if (TimeUnits.Contains(setupUnit))
                            {
                                tblmp.StdSetupTimeUnit = setupUnit;
                            }
                            else //Default Unit
                            {
                                tblmp.StdSetupTimeUnit = "Min";
                            }
                        }
                        catch
                        {
                            ErrorMsg = ErrorMsg + "StdSetupTime of Part Number" + PartName + " should be Number.\n";
                            continue;
                        }
                        decimal stdCuttingTime = 0;
                        try
                        {
                            stdCuttingTime = Convert.ToDecimal(ds.Tables[0].Rows[i][4]);
                        }
                        catch
                        {
                            ErrorMsg = ErrorMsg + "StdCuttingTime of " + PartName + " should be Numbers.\n";
                            continue;
                        }
                        //if (stdCuttingTime == 0)
                        //{
                        //    ErrorMsg = ErrorMsg + "StdCuttingTime of " + PartName + " cannot be Zero.\n";
                        //    continue;
                        //}
                        //else
                        {
                            tblmp.StdCuttingTime = stdCuttingTime;
                            string CuttingTimeUnit = Convert.ToString(ds.Tables[0].Rows[i][5]);
                            if (TimeUnits.Contains(CuttingTimeUnit))
                            {
                                tblmp.StdCuttingTimeUnit = CuttingTimeUnit;
                            }
                            else //Default Unit
                            {
                                tblmp.StdCuttingTimeUnit = "Min";
                            }
                        }

                        try
                        {
                            tblmp.StdChangeoverTime = Convert.ToDecimal(ds.Tables[0].Rows[i][6]);
                            string ChangeoverUnit = Convert.ToString(ds.Tables[0].Rows[i][7]);
                            if (TimeUnits.Contains(ChangeoverUnit))
                            {
                                tblmp.StdChangeoverTimeUnit = ChangeoverUnit;
                            }
                            else //Default Unit
                            {
                                tblmp.StdChangeoverTimeUnit = "Min";
                            }
                        }
                        catch
                        {
                            ErrorMsg = ErrorMsg + "StdChangeoverTime of Part Number" + PartName + " should be Number.\n";
                            continue;
                        }

                        try
                        {
                            tblmp.InputWeight = Convert.ToDecimal(ds.Tables[0].Rows[i][8]);
                            string InputWeightUnit = Convert.ToString(ds.Tables[0].Rows[i][9]);
                            if (WeightUnits.Contains(InputWeightUnit))
                            {
                                tblmp.InputWeightUnit = InputWeightUnit;
                            }
                            else //Default Unit
                            {
                                tblmp.InputWeightUnit = "Kg";
                            }

                            tblmp.OutputWeight = Convert.ToDecimal(ds.Tables[0].Rows[i][10]);
                            string OutputWeightUnit = Convert.ToString(ds.Tables[0].Rows[i][11]);
                            if (WeightUnits.Contains(OutputWeightUnit))
                            {
                                tblmp.OutputWeightUnit = OutputWeightUnit;
                            }
                            else //Default Unit
                            {
                                tblmp.OutputWeightUnit = "Kg";
                            }

                            tblmp.MaterialRemovedQty = Convert.ToDecimal(ds.Tables[0].Rows[i][12]);
                            string MaterialRemovedQtyUnit = Convert.ToString(ds.Tables[0].Rows[i][13]);
                            if (WeightUnits.Contains(MaterialRemovedQtyUnit))
                            {
                                tblmp.MaterialRemovedQtyUnit = MaterialRemovedQtyUnit;
                            }
                            else //Default Unit
                            {
                                tblmp.MaterialRemovedQtyUnit = "Kg";
                            }
                        }
                        catch
                        {
                            ErrorMsg = ErrorMsg + "I/O Weights & Materials Removed of " + PartName + " should be Numbers.\n";
                            continue;
                        }

                        //check for dup and delete previous one.
                        var Dupdata = db.tblmasterparts_st_sw.Where(m => m.PartNo == PartName && m.OpNo == OpNo && m.IsDeleted == 0).FirstOrDefault();
                        if (Dupdata != null)
                        {
                            Dupdata.IsDeleted = 1;
                            Dupdata.DeletedDate = DateTime.Now;
                            db.Entry(Dupdata).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        db.tblmasterparts_st_sw.Add(tblmp);
                        db.SaveChanges();
                    }
                    #endregion
                }
                else if (UploadType == "Update") // OverWrite Existing Values 
                {
                    #region
                    for (int i = 1; i < ds.Tables[0].Rows.Count; i++)
                    {
                        tblmasterparts_st_sw tblmp = new tblmasterparts_st_sw();
                        String Username = Session["Username"].ToString();
                        tblmp.CreatedBy = Convert.ToInt32(Session["UserId"]);
                        tblmp.CreatedOn = DateTime.Now;
                        tblmp.IsDeleted = 0;

                        string PartName = null, OpNo = null;
                        PartName = Convert.ToString(ds.Tables[0].Rows[i][0]);
                        OpNo = Convert.ToString(ds.Tables[0].Rows[i][1]);

                        DateTime createdOn = DateTime.Now;

                        //using (i_facility.Models.i_facility_tsalEntities db1 = new i_facility.Models.i_facility_tsalEntities())
                        //{
                        //var MasterStdPWTData = db1.tblmasterparts_st_sw.Where(m => m.PartNo == PartName && m.OpNo == OpNo).FirstOrDefault();
                        //if (MasterStdPWTData != null)
                        //{
                        //    ErrorMsg = ErrorMsg + PartName + " and " + OpNo + " has details in Database.\n";
                        //    continue;
                        //}
                        //else 
                        if (string.IsNullOrEmpty(PartName) || string.IsNullOrEmpty(OpNo))
                        {
                            ErrorMsg = ErrorMsg + PartName + " and " + OpNo + " has details in Database.\n";
                            continue;
                        }
                        else
                        {
                            tblmp.PartNo = PartName;
                            tblmp.OpNo = OpNo;
                        }
                        //}
                        try
                        {
                            tblmp.StdSetupTime = Convert.ToDecimal(ds.Tables[0].Rows[i][2]);
                            string setupUnit = Convert.ToString(ds.Tables[0].Rows[i][3]);
                            if (TimeUnits.Contains(setupUnit))
                            {
                                tblmp.StdSetupTimeUnit = setupUnit;
                            }
                            else //Default Unit
                            {
                                tblmp.StdSetupTimeUnit = "Min";
                            }
                        }
                        catch
                        {
                            ErrorMsg = ErrorMsg + "StdSetupTime of Part Number" + PartName + " should be Number.\n";
                            continue;
                        }
                        decimal stdCuttingTime = 0;
                        try
                        {
                            stdCuttingTime = Convert.ToDecimal(ds.Tables[0].Rows[i][4]);
                        }
                        catch
                        {
                            ErrorMsg = ErrorMsg + "StdCuttingTime of " + PartName + " should be Numbers.\n";
                            continue;
                        }
                        //if (stdCuttingTime == 0)
                        //{
                        //    ErrorMsg = ErrorMsg + "StdCuttingTime of " + PartName + " cannot be Zero.\n";
                        //    continue;
                        //}
                        //else
                        {
                            tblmp.StdCuttingTime = stdCuttingTime;
                            string CuttingTimeUnit = Convert.ToString(ds.Tables[0].Rows[i][5]);
                            if (TimeUnits.Contains(CuttingTimeUnit))
                            {
                                tblmp.StdCuttingTimeUnit = CuttingTimeUnit;
                            }
                            else //Default Unit
                            {
                                tblmp.StdCuttingTimeUnit = "Min";
                            }
                        }

                        try
                        {
                            tblmp.StdChangeoverTime = Convert.ToDecimal(ds.Tables[0].Rows[i][6]);
                            string ChangeoverUnit = Convert.ToString(ds.Tables[0].Rows[i][7]);
                            if (TimeUnits.Contains(ChangeoverUnit))
                            {
                                tblmp.StdChangeoverTimeUnit = ChangeoverUnit;
                            }
                            else //Default Unit
                            {
                                tblmp.StdChangeoverTimeUnit = "Min";
                            }
                        }
                        catch
                        {
                            ErrorMsg = ErrorMsg + "StdChangeoverTime of Part Number" + PartName + " should be Number.\n";
                            continue;
                        }

                        try
                        {
                            tblmp.InputWeight = Convert.ToDecimal(ds.Tables[0].Rows[i][8]);
                            string InputWeightUnit = Convert.ToString(ds.Tables[0].Rows[i][9]);
                            if (WeightUnits.Contains(InputWeightUnit))
                            {
                                tblmp.InputWeightUnit = InputWeightUnit;
                            }
                            else //Default Unit
                            {
                                tblmp.InputWeightUnit = "Kg";
                            }

                            tblmp.OutputWeight = Convert.ToDecimal(ds.Tables[0].Rows[i][10]);
                            string OutputWeightUnit = Convert.ToString(ds.Tables[0].Rows[i][11]);
                            if (WeightUnits.Contains(OutputWeightUnit))
                            {
                                tblmp.OutputWeightUnit = OutputWeightUnit;
                            }
                            else //Default Unit
                            {
                                tblmp.OutputWeightUnit = "Kg";
                            }

                            tblmp.MaterialRemovedQty = Convert.ToDecimal(ds.Tables[0].Rows[i][12]);
                            string MaterialRemovedQtyUnit = Convert.ToString(ds.Tables[0].Rows[i][13]);
                            if (WeightUnits.Contains(MaterialRemovedQtyUnit))
                            {
                                tblmp.MaterialRemovedQtyUnit = MaterialRemovedQtyUnit;
                            }
                            else //Default Unit
                            {
                                tblmp.MaterialRemovedQtyUnit = "Kg";
                            }
                        }
                        catch
                        {
                            ErrorMsg = ErrorMsg + "I/O Weights & Materials Removed of " + PartName + " should be Numbers.\n";
                            continue;
                        }

                        var MasterStdPWTData = db.tblmasterparts_st_sw.Where(m => m.PartNo == PartName && m.OpNo == OpNo && m.IsDeleted == 0).FirstOrDefault();
                        if (MasterStdPWTData == null)
                        {
                            db.tblmasterparts_st_sw.Add(tblmp);
                            db.SaveChanges();
                        }
                        else
                        {
                            MasterStdPWTData.InputWeight = tblmp.InputWeight;
                            MasterStdPWTData.InputWeightUnit = tblmp.InputWeightUnit;
                            MasterStdPWTData.MaterialRemovedQty = tblmp.MaterialRemovedQty;
                            MasterStdPWTData.MaterialRemovedQtyUnit = tblmp.MaterialRemovedQtyUnit;
                            MasterStdPWTData.ModifiedBy = Convert.ToInt32(Session["UserId"]);
                            MasterStdPWTData.ModifiedOn = DateTime.Now;
                            MasterStdPWTData.OpNo = tblmp.OpNo;
                            MasterStdPWTData.OutputWeight = tblmp.OutputWeight;
                            MasterStdPWTData.OutputWeightUnit = tblmp.OutputWeightUnit;
                            MasterStdPWTData.PartNo = tblmp.PartNo;
                            MasterStdPWTData.StdChangeoverTime = tblmp.StdChangeoverTime;
                            MasterStdPWTData.StdChangeoverTimeUnit = tblmp.StdChangeoverTimeUnit;
                            MasterStdPWTData.StdCuttingTime = tblmp.StdCuttingTime;
                            MasterStdPWTData.StdCuttingTimeUnit = tblmp.StdCuttingTimeUnit;
                            MasterStdPWTData.StdSetupTime = tblmp.StdSetupTime;
                            MasterStdPWTData.StdSetupTimeUnit = tblmp.StdSetupTimeUnit;

                            db.Entry(MasterStdPWTData).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                    }
                    #endregion
                }

                Session["PartNo"] = ErrorMsg;
            }

            //return RedirectToAction("Index", "MasterParts");
            return View();
        }

        public ActionResult ExportMasterPartsSTDWeightTime()
        {
            #region Excel and Stuff
            DateTime frda = DateTime.Now;
            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "Master Parts" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "Master Parts" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
                }
                catch
                {
                    TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
                    //return View();
                }
            }

            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\PartsStdWT.xlsx");
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

            //Header
            //worksheet.Cells["A" + 1].Value = "Level1Code";
            //worksheet.Cells["B" + 1].Value = "Level2Code";
            //worksheet.Cells["C" + 1].Value = "Level3Code";
            //worksheet.Cells["D" + 1].Value = "Level";
            //worksheet.Cells["E" + 1].Value = "Code Desc";
            //worksheet.Cells["F" + 1].Value = "EndCode";
            ////worksheet.Cells["G" + 1].Value = "Loss Type";
            ////worksheet.Cells["H" +1].Value = "MessageType";
            ////worksheet.Cells["I" +1].Value = "Contributes To";
            //worksheet.Cells["A1:I1"].Style.Font.Bold = true;




            #endregion

            var BreakDownLossesData = db.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0).ToList();
            int i = 3;
            foreach (var row in BreakDownLossesData)
            {

                worksheet.Cells["A" + i].Value = row.PartNo;
                worksheet.Cells["B" + i].Value = row.OpNo;
                worksheet.Cells["C" + i].Value = row.StdSetupTime;
                worksheet.Cells["D" + i].Value = row.StdSetupTimeUnit; ;
                worksheet.Cells["E" + i].Value = row.StdCuttingTime;
                worksheet.Cells["F" + i].Value = row.StdCuttingTimeUnit;
                worksheet.Cells["G" + i].Value = row.StdChangeoverTime;
                worksheet.Cells["H" + i].Value = row.StdChangeoverTimeUnit;
                worksheet.Cells["I" + i].Value = row.InputWeight;
                worksheet.Cells["J" + i].Value = row.InputWeightUnit;
                worksheet.Cells["K" + i].Value = row.OutputWeight;
                worksheet.Cells["L" + i].Value = row.OutputWeightUnit;
                worksheet.Cells["M" + i].Value = row.MaterialRemovedQty;
                worksheet.Cells["N" + i].Value = row.MaterialRemovedQtyUnit;

                i++;
            }

            #region Save and Download

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "Master Parts" + frda.ToString("yyyy-MM-dd") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "Master Parts" + frda.ToString("yyyy-MM-dd") + ".xlsx";
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

            return RedirectToAction("Index");
        }
    }
}