using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using DNC;
using SRKSDemo;
using SRKSDemo.Server_Model;

namespace SRKSDemo.Controllers
{
    public class ProgramTransferController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        //
        // GET: /ProgramTransfer/

        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            ViewBag.GetConnected = 0;
            ViewData["GetConnected"] = 0;
            ViewData["PlantID"] = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");

            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file, string PlantID, string ShopID = null, string CellID = null, string WorkCenterID = null)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            Session["MachineID"] = WorkCenterID;
            ViewBag.ReturnStatusMsg = null;
            if (Request.Files["file"].ContentLength > 0)
            {
                //Folder Names also MachineID
                //Based on MachineID Push the File into Respective Directory.
                //string Path = @"D:\SRKSSoftwareSolutions-Bangalore\Applications\ProgramTransferData";
                string Path = @"C:\TataReport\ProgramTransferData";
                int WCID = Convert.ToInt32(WorkCenterID);
                String MachineInv = db.tblmachinedetails.Where(m => m.MachineID == WCID).Select(m => m.MachineDisplayName).FirstOrDefault();
                string fileLocation = Server.MapPath("~/Content/") + Request.Files["file"].FileName;
                if (System.IO.File.Exists(fileLocation))
                {
                    System.IO.File.Delete(fileLocation);
                }

                Request.Files["file"].SaveAs(fileLocation);

                //Copy File into machine Folder
                string filename = Request.Files["file"].FileName;

                string MacWiseFolder = @Path + @"\" + MachineInv;
                if (!System.IO.Directory.Exists(MacWiseFolder))
                {
                    System.IO.Directory.CreateDirectory(MacWiseFolder);
                }
                string destinationPathWithFileName = @Path + @"\" + MachineInv + @"\" + Request.Files["file"].FileName;
                System.IO.File.Copy(fileLocation, destinationPathWithFileName, true);

                //var dirName = new DirectoryInfo(System.IO.Path.GetDirectoryName(destinationPathWithFileName)).Name; //Works
                ////string [] PathArray = destinationPathWithFileName.Split('\\');
                ////string FolderName = PathArray[PathArray.Length - 2];
                //var dirName1 = new DirectoryInfo(System.IO.Path.GetDirectoryName(dirName)).Name; //Works
                //var a = 0;

                //object ip = "192.168.0.1";   // "192.168.0.1" or "CNC-1.FACTORY"
                //ushort port = 0 ;            //  FOCAS1/Ethernet or FOCAS2/Ethernet (TCP) function
                //int timeout = 0;           //seconds if 0 infinitely waits

                //Log this event.
                int MacID = Convert.ToInt32(WorkCenterID);
                tblprogramtransferhistory pth = new tblprogramtransferhistory();
                pth.IsDeleted = 0;
                pth.MachineID = MacID;
                pth.ProgramName = Request.Files["file"].FileName;
                pth.UploadedTime = DateTime.Now;
                pth.Version = 1;
                pth.UserID = Convert.ToInt32(Session["UserId"]);
                db.tblprogramtransferhistories.Add(pth);
                db.SaveChanges();


                //Based on WorkCenterID get IpAddress.
                object ip = db.tblmachinedetails.Where(m => m.MachineID == MacID).Select(m => m.IPAddress).FirstOrDefault();
                ushort port = 8193;            //  FOCAS1/Ethernet or FOCAS2/Ethernet (TCP) function
                int timeout = 0;           //seconds if 0 infinitely waits

                int RetVal = 0;
                int pthID = pth.PTHID;
                //Session["pthID"] = pthID;
                //DownloadNCProg d = new DownloadNCProg(ip, port, timeout, fileLocation);
                //string retString = d.UploadCNCProgram(pthID, out RetVal);

                ProgramTransfer PT = new ProgramTransfer(ip.ToString());
                string retString = PT.UploadCNCProgram(pthID, fileLocation, out RetVal);
                //System.Threading.Thread.Sleep(100000);

                if (retString == "Success")
                {
                    var pthData = db.tblprogramtransferhistories.Find(pthID);
                    if (pthData != null)
                    {
                        pthData.ReturnStatus = 1;
                        pthData.ReturnDesc = "Success";
                        pthData.ReturnTime = DateTime.Now;
                        pthData.IsCompleted = 1;
                        db.Entry(pthData).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ViewBag.ReturnStatusMsg = "The NC Program : " + filename.Split('.')[0].ToString() + " wsa successfully uploaded on the CNC Machine " + MachineInv;
                        TempData["toaster_success"] = "The NC Program : " + filename.Split('.')[0].ToString() + " wsa successfully uploaded on the CNC Machine " + MachineInv;
                    }
                    else //Hope fully this don't get executed.
                    {
                        retString = "Success but Unable to comply.";
                        TempData["toaster_warning"] = retString;
                    }
                }
                else //Upload failed.
                {
                    var pthData = db.tblprogramtransferhistories.Find(pthID);
                    if (pthData != null)
                    {
                        pthData.ReturnStatus = 0;
                        pthData.ReturnDesc = retString;
                        pthData.ReturnTime = DateTime.Now;
                        pthData.IsCompleted = 1;
                        db.Entry(pthData).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ViewBag.ReturnStatusMsg = retString;
                    }
                    else //Hope fully this don't get executed.
                    {
                        retString += "Failure and Unable to comply.";
                        TempData["toaster_error"] = retString;
                    }
                }
            }
            else
            {
                //Not a Valid File.
                try
                {
                    //Log this event.
                    tblprogramtransferhistory pth = new tblprogramtransferhistory();
                    pth.IsDeleted = 0;
                    pth.MachineID = Convert.ToInt32(WorkCenterID);
                    pth.ProgramName = Request.Files["file"].FileName;
                    pth.UploadedTime = DateTime.Now;
                    pth.UserID = Convert.ToInt32(Session["UserId"]);
                    pth.ReturnTime = DateTime.Now;
                    pth.ReturnStatus = 999;
                    pth.ReturnDesc = "Not a Valid File(FileLength).";
                    pth.IsCompleted = 1;
                    db.tblprogramtransferhistories.Add(pth);
                    db.SaveChanges();

                    ViewBag.ReturnStatusMsg = "Not a Valid File(FileLength).";
                    TempData["toaster_error"] = "Not a Valid File(FileLength).";

                    // int pthID = pth.PTHID;
                    // Session["pthID"] = pthID;
                    //System.Threading.Thread.Sleep(30000);
                }
                catch (Exception e)
                {
                    ViewBag.ReturnStatusMsg = "Error." + e;
                }
            }
            ViewBag.GetConnected = 1;
            ViewData["GetConnected"] = 1;
            int PlantIDInt = 0, ShopIDInt = 0, CellIDInt = 0, MacIDInt = 0;
            int.TryParse(PlantID, out PlantIDInt);
            int.TryParse(ShopID, out ShopIDInt);
            int.TryParse(CellID, out CellIDInt);
            int.TryParse(WorkCenterID, out MacIDInt);
            ViewData["PlantID"] = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantIDInt);
            ViewData["ShopID"] = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantIDInt), "ShopID", "ShopName", ShopIDInt);
            ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantIDInt && m.ShopID == ShopIDInt), "CellID", "CellName", CellIDInt);
            ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantIDInt && m.ShopID == ShopIDInt && m.CellID == CellIDInt), "MachineID", "MachineDisplayName", MacIDInt);

            return View();
        }

        //0: upload failed, 
        //1: Successfull.
        public JsonResult CheckEndStatus()
        {
            string retValue = "";
            using (i_facility_shaktiEntities1 dbpupload = new i_facility_shaktiEntities1())
            {
                int macID = Convert.ToInt32(Session["MachineID"]);
                int pthistroyID = 0;
                Int32.TryParse(Convert.ToString(Session["pthID"]), out pthistroyID);

                if (pthistroyID == 0)
                {
                    pthistroyID = dbpupload.tblprogramtransferhistories.Where(m => m.MachineID == macID).OrderByDescending(m => m.UploadedTime).Select(m => m.PTHID).FirstOrDefault();
                }

                var RetStatusData = dbpupload.tblprogramtransferhistories.Where(m => m.PTHID == pthistroyID).FirstOrDefault();
                if (RetStatusData != null && RetStatusData.IsCompleted == 0)
                {
                    int retStatusInt = 0;
                    if (int.TryParse(Convert.ToString(RetStatusData.ReturnStatus), out retStatusInt))
                    {
                        if (retStatusInt == 0)
                        {
                            retValue = RetStatusData.ReturnDesc;
                            Session["pthID"] = null;
                        }
                        else
                        {
                            retValue = "Upload Successfull.";
                            Session["pthID"] = null;
                        }

                        //RetStatusData.IsCompleted = 1;
                        dbpupload.Entry(RetStatusData).State = System.Data.Entity.EntityState.Modified;
                        dbpupload.SaveChanges();

                    }
                    // else return null
                }
            }
            return Json(retValue, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditPrograms()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];

            ViewData["PlantID"] = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");

            List<Product> plist = new List<Product>();
            Product p1 = new Product();
            p1.Description = "p1";
            p1.Id = 1;
            p1.Quantity = 10;
            plist.Add(p1);

            Product p2 = new Product();
            p2.Description = "p2";
            p2.Id = 1;
            p2.Quantity = 10;
            plist.Add(p2);

            p1.plist = plist;

            return View(p1);

        }

        [HttpPost]
        public ActionResult EditPrograms(string PlantID, string ShopID = null, string CellID = null, string WorkCenterID = null)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];

            //Get All ProgramNo's for this WorkCenterID and send to view.


            int PlantIDInt = 0, ShopIDInt = 0, CellIDInt = 0, MacIDInt = 0;
            int.TryParse(PlantID, out PlantIDInt);
            int.TryParse(ShopID, out ShopIDInt);
            int.TryParse(CellID, out CellIDInt);
            int.TryParse(WorkCenterID, out MacIDInt);
            ViewData["PlantID"] = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantIDInt);
            ViewData["ShopID"] = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantIDInt), "ShopID", "ShopName", ShopIDInt);
            ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantIDInt && m.ShopID == ShopIDInt), "CellID", "CellName", CellIDInt);
            ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantIDInt && m.ShopID == ShopIDInt && m.CellID == CellIDInt), "MachineID", "MachineDisplayName", MacIDInt);

            return View();
        }

        public ActionResult GetView(int CellID = 1)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            string viewName = "_Product";
            object model = null;

            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                model = db.tblmachinedetails.Where(o => o.CellID == CellID)
                          .OrderBy(o => o.CellID).ToList();
            }

            return PartialView(viewName, model);
        }

        //get Program data
        public JsonResult ProgramData(int MacID, string ProgNo)
        {
            String ProgData = null;// = new StringBuilder();
            object ip = db.tblmachinedetails.Where(m => m.MachineID == MacID).Select(m => m.IPAddress).FirstOrDefault();
            ProgramTransfer PT = new ProgramTransfer(ip.ToString());
            PT.GetProgramDataNC(ProgNo, out ProgData);
            return Json(ProgData.ToString(), JsonRequestBehavior.AllowGet);
        }

        //Get Program data from the Version selected of the Program or by default the Latest Version Number Present
        public JsonResult ProgramDataPC(int MacID, string ProgNo, int VerNo)
        {
            String ProgData; //= "";
            object ip = db.tblmachinedetails.Where(m => m.MachineID == MacID).Select(m => m.IPAddress).FirstOrDefault();
            String MacInv = db.tblmachinedetails.Where(m => m.MachineID == MacID).Select(m => m.MachineDisplayName).FirstOrDefault();
            ProgramTransfer PT = new ProgramTransfer(ip.ToString());
            PT.GetProgramDataPC(MacInv, ProgNo.ToString(), VerNo, out ProgData);
            return Json(ProgData.ToString(), JsonRequestBehavior.AllowGet);
        }

        //var ProgData = @"%
        //O9816(REN*EXTL*CORNER) 
        //(F-4012-0519-0W) 
        //(COPYRIGHT*1990-2016*RENISHAW*PLC.*ALL*RIGHTS*RESERVED)
        //G65P9724 
        //#31=#5041
        //#32=#5042
        //#27=#5043-#116 
        //#149=170 
        //IF[#24EQ#0]GOTO32
        //#149=180 
        //IF[#25EQ#0]GOTO32
        //#149=230 
        //IF[#11NE#0]GOTO32
        //#1=135 
        //WHILE[#1LE149]DO1
        //#[#1]=#0
        //#1=#1+1
        //END1 
        //IF[#7EQ#0]GOTO2
        //#7=ABS[#7] 
        //IF[#24GT#31]GOTO1
        //#7=-#7 
        //N1 
        //#14=#7+#24 
        //GOTO3
        //N2 
        //#14=[#24*2]-#31
        //N3 
        //IF[#8EQ#0]GOTO5
        //#8=ABS[#8] 
        //IF[#25GT#32]GOTO4
        //#8=-#8 
        //N4 
        //#15=#8+#25 
        //GOTO6
        //%";

        List<ProgramListDet> PrgDetList;

        //Get Program List for the Machine Connected
        public JsonResult ProgramList(int MacID)
        {
            PrgDetList = new List<ProgramListDet>();
            object ip = db.tblmachinedetails.Where(m => m.MachineID == MacID).Select(m => m.IPAddress).FirstOrDefault();
            ProgramTransfer PT = new ProgramTransfer(ip.ToString());
            PrgDetList = PT.GetProgramList();
            return Json(PrgDetList, JsonRequestBehavior.AllowGet);
        }

        //Get the version list of the program from the Program selected for the connected machine.
        public JsonResult PCProgramList(int MacID, String ProgNo)
        {
            List<ProgramVersionListDet> PrgVerDetList = new List<ProgramVersionListDet>();
            object ip = db.tblmachinedetails.Where(m => m.MachineID == MacID).Select(m => m.IPAddress).FirstOrDefault();
            String MacInv = db.tblmachinedetails.Where(m => m.MachineID == MacID).Select(m => m.MachineDisplayName).FirstOrDefault();
            ProgramTransfer PT = new ProgramTransfer(ip.ToString());
            PT.GetVersionListPCProgram(MacInv.ToString(), ProgNo.ToString(), out PrgVerDetList);
            return Json(PrgVerDetList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetShop(int PlantID)
        {
            var ShopData = (from row in db.tblshops
                            where row.IsDeleted == 0 && row.PlantID == PlantID
                            select new { Value = row.ShopID, Text = row.Shopdisplayname });
            return Json(ShopData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCell(int ShopID)
        {
            var CellData = (from row in db.tblcells
                            where row.IsDeleted == 0 && row.ShopID == ShopID
                            select new { Value = row.CellID, Text = row.CelldisplayName });

            return Json(CellData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetWC_Cell(int CellID)
        {
            var MachineData = (from row in db.tblmachinedetails
                               where row.IsDeleted == 0 && row.CellID == CellID
                               select new { Value = row.MachineID, Text = row.MachineDisplayName });
            return Json(MachineData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProgramDelete(int MacID, string ProgNo)
        {
            string retStatus = null;
            object ip = db.tblmachinedetails.Where(m => m.MachineID == MacID).Select(m => m.IPAddress).FirstOrDefault();
            String MacInv = db.tblmachinedetails.Where(m => m.MachineID == MacID).Select(m => m.MachineDisplayName).FirstOrDefault();
            ProgramTransfer PT = new ProgramTransfer(ip.ToString());
            retStatus = PT.DeleteProgram(MacInv, ProgNo);

            return Json(retStatus, JsonRequestBehavior.AllowGet);
        }

        public string SaveProgramData(string updateOrsave, string ncProgramData,string MacId,string filename)
        {
            string data = "success";
            if (filename == "" || filename == null)
            {
                data = "filenull";
            }
            else
            {
                int userId = 0;
                try
                {
                    userId = Convert.ToInt32(Session["UserId"]);
                }
                catch
                {

                }
                try
                {
                    int macid = Convert.ToInt32(MacId);

                    var fileCheck = db.tblNcProgramTransferMains.Where(m => m.ProgramNumber == filename).ToList();

                    if (fileCheck.Count == 0)
                    {//add fresh data
                        tblNcProgramTransferMain dataObj = new tblNcProgramTransferMain();
                        dataObj.McId = macid;
                        dataObj.ProgramNumber = filename;
                        dataObj.VersionNumber = 1;
                        dataObj.ProgramData = ncProgramData;
                        dataObj.CreatedDate = DateTime.Now;
                        dataObj.CreatedBy = userId;
                        dataObj.IsDeleted = false;
                        db.tblNcProgramTransferMains.Add(dataObj);
                        db.SaveChanges();
                    }
                    else
                    {
                        //update old version or add new version
                        var item = db.tblNcProgramTransferMains.Where(m => m.ProgramNumber == filename).OrderByDescending(m => m.VersionNumber).Take(1).Single();
                        if (updateOrsave == "1")
                        {//update
                            item.ProgramData = ncProgramData;
                            db.SaveChanges();
                        }
                        else
                        {//add
                            int? versionNumber = item.VersionNumber;
                            tblNcProgramTransferMain dataObj = new tblNcProgramTransferMain();
                            dataObj.McId = macid;
                            dataObj.ProgramNumber = filename;
                            dataObj.VersionNumber = versionNumber + 1;
                            dataObj.ProgramData = ncProgramData;
                            dataObj.CreatedDate = DateTime.Now;
                            dataObj.CreatedBy = userId;
                            dataObj.IsDeleted = false;
                            db.tblNcProgramTransferMains.Add(dataObj);
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception e)
                {
                    data = "UnSuccessFul";
                }
            }
            return data;
        }

        public string PcToCNC(string ncProgramData, string MacId, string filename,string vernoval)
        {
            string reply = "";
            int RetVal = 0;
            int userId = Convert.ToInt32(Session["UserId"]);
            int MacID = Convert.ToInt32(MacId);
            int versionNo = Convert.ToInt32(vernoval);
            tblprogramtransferhistory pth = new tblprogramtransferhistory();
            pth.IsDeleted = 0;
            pth.MachineID = MacID;
            pth.ProgramName = filename;
            pth.UploadedTime = DateTime.Now;
            pth.Version = versionNo;
            pth.UserID = userId;
            db.tblprogramtransferhistories.Add(pth);
            db.SaveChanges();

            int programTransferHistoryId = pth.PTHID;
            object ip = db.tblmachinedetails.Where(m => m.MachineID == MacID).Select(m => m.IPAddress).FirstOrDefault();
            ushort port = 8193;
            ProgramTransfer PT = new ProgramTransfer(ip.ToString());
            string fileLocation = @"C:\NCProgram\NcProgram.txt";
            PT.CreateFileForProgramTransfer(ncProgramData);
            reply = PT.UploadCNCProgram(programTransferHistoryId, fileLocation, out RetVal);
            PT.DeleteFileProgramTransfer();
            return reply;
        }
    }

    public class DownloadNCProg
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        static object ip;   // "192.168.0.1" or "CNC-1.FACTORY"
        static ushort port;            //  FOCAS1/Ethernet or FOCAS2/Ethernet (TCP) function
        static int timeout;           //seconds if 0 infinitely waits
        static ushort FlibHndl;
        string FilePath;
        //ushort h;
        //Focas1.ODBST buf;


        public DownloadNCProg(object ip1, ushort port1, int timeout1, string FilePath)
        {
            ip = ip1;                 // "192.168.0.1" or "CNC-1.FACTORY"
            port = port1;            //  FOCAS1/Ethernet or FOCAS2/Ethernet (TCP) function
            timeout = timeout1;           //seconds if 0 infinitely waits
            //ushort FlibHndl = FlibHndl1; 
            this.FilePath = FilePath;
        }

        //DNC Program
        //public string BeginCalculation()
        //{
        //    string retValue = null;
        //    //try
        //    //{
        //    //    //Focas1.cnc_allclibhndl3(ip,  port,  timeout, out FlibHndl);
        //    //    Focas1.focas_ret retallclibhndl3 = (Focas1.focas_ret)Focas1.cnc_allclibhndl3(ip, port, timeout, out FlibHndl); //¨ú±olibrary handle 
        //    //    if (retallclibhndl3 == Focas1.focas_ret.EW_OK)
        //    //    {
        //    //        //send data to Machine
        //    //        //1) notify about start : FWLIBAPI short WINAPI cnc_dncstart(unsigned short FlibHndl);
        //    //        //Note : CNC parameters must be set. Mind it
        //    //        Focas1.focas_ret retdncstart = (Focas1.focas_ret)Focas1.cnc_dncstart(FlibHndl);
        //    //        if (retdncstart == Focas1.focas_ret.EW_OK)
        //    //        {
        //    //            //send nc command data to cnc(dnc)
        //    //            //FWLIBAPI short WINAPI cnc_cdnc(unsigned short FlibHndl,char *data, short number);
        //    //            //For example, to execute the commands such as
        //    //            //M3 S2000 ;        // T14 ;        // G0 X10. ;          // G0 Z-5. ;      // M30 ;  

        //    //            //send a following string using cnc_dnc function.
        //    //            //cnc_dnc( "\nM3S2000\nT14\nG0X10.\nG0Z-5.\nM30\n%", 32 ) ;  The string data can be sent by multiple cnc_dnc functions.
        //    //            // For above example, the commands can be sent block by block like this.
        //    //            //cnc_dnc( "\n", 1 ) ;      // cnc_dnc( "M3S2000\n", 8 ) ;
        //    //            // cnc_dnc( "T14\n", 4 ) ;       // cnc_dnc( "G0X10.\n", 7 ) ;
        //    //            // cnc_dnc( "G0Z-5.\n", 7 ) ;        // cnc_dnc( "M30\n", 4 ) ;      
        //    //            // cnc_dnc( "%", 1 ) ; 

        //    //            ushort number = 0;
        //    //            string DataString = "\nM3S2000\nT14\nG0X10.\nG0Z-5.\nM30\n%";

        //    //            //number = (ushort)System.Text.ASCIIEncoding.Unicode.GetByteCount(DataString);
        //    //            //number = System.Text.ASCIIEncoding.ASCII.GetByteCount(DataString);

        //    //            //As per example in given xml files (Program\cnc_dnc2.htm)
        //    //            number = (ushort)DataString.Length;

        //    //            //In a loop, keep downloading.
        //    //            var fileStream = new FileStream(@"c:\file.txt", FileMode.Open, FileAccess.Read);
        //    //            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
        //    //            {
        //    //                string line;
        //    //                while ((line = streamReader.ReadLine()) != null)
        //    //                {
        //    //                    // process the line
        //    //                    Object Data = line;
        //    //                    number = (ushort)DataString.Length;
        //    //                    Focas1.focas_ret retcdnc = (Focas1.focas_ret)Focas1.cnc_cdnc(FlibHndl, Data, number);

        //    //                    if (retcdnc == Focas1.focas_ret.EW_FUNC)
        //    //                    {
        //    //                        TextLogFile("cnc_dncstart function has not been executed. " + retcdnc.ToString());
        //    //                    }
        //    //                    else if (retcdnc == Focas1.focas_ret.EW_RESET)
        //    //                    {
        //    //                        TextLogFile("Reset or stop request. " + retcdnc.ToString());
        //    //                    }
        //    //                    else if (retcdnc == Focas1.focas_ret.EW_LENGTH)
        //    //                    {
        //    //                        TextLogFile("Data block length error. " + retcdnc.ToString());
        //    //                    }

        //    //                    //End dncstart: FWLIBAPI short WINAPI cnc_dncend(unsigned short FlibHndl);
        //    //                    Focas1.focas_ret retdncend = (Focas1.focas_ret)Focas1.cnc_dncend(FlibHndl);
        //    //                    if (retdncend == Focas1.focas_ret.EW_FUNC)
        //    //                    {
        //    //                        TextLogFile("cnc_dncstart function has not been executed. " + retdncend.ToString());
        //    //                    }
        //    //                    else if (retdncend == Focas1.focas_ret.EW_DATA)
        //    //                    {
        //    //                        TextLogFile("A character which is unavailable for NC program is detected. " + retdncend.ToString());
        //    //                    }
        //    //                }
        //    //            }



        //    //        }
        //    //        else if (retdncstart == Focas1.focas_ret.EW_BUSY)
        //    //        {
        //    //            TextLogFile("c_dncstart function has been executed. " + retdncstart.ToString());

        //    //            //End dncstart: FWLIBAPI short WINAPI cnc_dncend(unsigned short FlibHndl);
        //    //            Focas1.focas_ret retdncend = (Focas1.focas_ret)Focas1.cnc_dncend(FlibHndl);
        //    //            if (retdncend == Focas1.focas_ret.EW_FUNC)
        //    //            {
        //    //                TextLogFile("cnc_dncstart function has not been executed. " + retdncend.ToString());
        //    //            }
        //    //            else if (retdncend == Focas1.focas_ret.EW_DATA)
        //    //            {
        //    //                TextLogFile("A character which is unavailable for NC program is detected. " + retdncend.ToString());
        //    //            }

        //    //        }
        //    //        else if (retdncstart == Focas1.focas_ret.EW_PARAM)
        //    //        {
        //    //            TextLogFile("CNC parameter error. " + retdncstart.ToString());
        //    //        }

        //    //        Focas1.cnc_freelibhndl(h);
        //    //        retValue += retallclibhndl3.ToString();
        //    //    }
        //    //    else if (retallclibhndl3 == Focas1.focas_ret.EW_SOCKET)
        //    //    {
        //    //        TextLogFile("Socket communication error. " + retallclibhndl3.ToString());
        //    //    }
        //    //    else if (retallclibhndl3 == Focas1.focas_ret.EW_NODLL)
        //    //    {
        //    //        TextLogFile("There is no DLL file for each CNC series . " + retallclibhndl3.ToString());
        //    //    }
        //    //    else if (retallclibhndl3 == Focas1.focas_ret.EW_HANDLE)
        //    //    {
        //    //        TextLogFile("Allocation of handle number is failed. " + retallclibhndl3.ToString());
        //    //    }

        //    //}
        //    //catch (Exception e)
        //    //{
        //    //    retValue += e.ToString();
        //    //}

        //    return retValue;
        //}

        //CNC Program 
        public string UploadCNCProgram(int pthID, out int retValueInt)
        {
            string retValue = null;
            //int retStatusInt = 0; //failure.
            retValueInt = 0; //EW_OK.
            try
            {
                Focas1.focas_ret retallclibhndl3 = (Focas1.focas_ret)Focas1.cnc_allclibhndl3(ip, port, timeout, out FlibHndl); //¨ú±olibrary handle 
                if (retallclibhndl3 == Focas1.focas_ret.EW_OK)
                {
                    retValueInt = (int)Focas1.focas_ret.EW_OK;
                    short type = 0;
                    var fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
                    using (StreamReader sr = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        StringBuilder DataString = new StringBuilder();
                        while (!sr.EndOfStream)
                        {
                            //read the data line by line into Dictionary, then access it.
                            string line = null;
                            Dictionary<int, string> AllData = new Dictionary<int, string>();
                            int LineNo = 1;
                            while ((line = sr.ReadLine()) != null)
                            {
                                AllData.Add(LineNo, line + "\n");
                                LineNo++;
                            }
                            Focas1.focas_ret retncstart = (Focas1.focas_ret)Focas1.cnc_dwnstart3(FlibHndl, type);
                            if (retncstart == Focas1.focas_ret.EW_OK)
                            {
                                retValueInt = (int)Focas1.focas_ret.EW_OK;
                                for (int row = 1; row < AllData.Count; row++)
                                {
                                    if (DataString != null)
                                    {
                                        DataString.Clear();
                                    }
                                    if (row == 0)
                                    {
                                        DataString.Append("\n");
                                    }
                                    line = null;
                                    int stringSize = DataString.Length;
                                    ushort currentLineSize = line != null ? (ushort)line.Length : (ushort)0;
                                    while (stringSize + currentLineSize < 250) //current line + old data size < 256
                                    {
                                        if (line != null)
                                        {
                                            DataString.Append(line); //include currentline
                                        }
                                        stringSize = DataString.Length;
                                        line = null;
                                        if (AllData.ContainsKey(row))
                                        {
                                            line = AllData[row++];
                                        }
                                        else // Add "%" at the end of Program.
                                        {
                                            DataString.Append("%");
                                            break;
                                        }
                                        currentLineSize = (ushort)line.Length;
                                    }
                                    row -= 2; //Above whileLoop fails only after more data, so don't consider last row + Increment in the ForLoop so -1 => Total -2.
                                    // send cur+-rent data
                                    stringSize = DataString.Length;
                                    Object Data = DataString;
                                    {
                                        Focas1.focas_ret retcnc = (Focas1.focas_ret)Focas1.cnc_download3(FlibHndl, ref stringSize, Data);
                                        if (retcnc == Focas1.focas_ret.EW_OK)
                                        {
                                            retValue = "Executed Successfully. " + retcnc.ToString();
                                            retValueInt = (int)Focas1.focas_ret.EW_OK;
                                        }
                                        else
                                        {

                                            if (retcnc == Focas1.focas_ret.EW_FUNC)
                                            {
                                                retValueInt = (int)Focas1.focas_ret.EW_FUNC;
                                                retValue = "cnc_dwnstart3 function has not been executed. " + retcnc.ToString();
                                            }
                                            else if (retcnc == Focas1.focas_ret.EW_RESET)
                                            {
                                                retValueInt = (int)Focas1.focas_ret.EW_RESET;
                                                retValue = "Reset or stop request. " + retcnc.ToString();
                                            }
                                            else if (retcnc == Focas1.focas_ret.EW_LENGTH)
                                            {
                                                retValueInt = (int)Focas1.focas_ret.EW_LENGTH;
                                                retValue = "The size of character string is negative. " + retcnc.ToString();
                                            }
                                            else if (retcnc == Focas1.focas_ret.EW_DATA)
                                            {
                                                retValueInt = (int)Focas1.focas_ret.EW_DATA;
                                                retValue = "Data error. " + retcnc.ToString();
                                            }
                                            else if (retcnc == Focas1.focas_ret.EW_PROT)
                                            {
                                                retValueInt = (int)Focas1.focas_ret.EW_PROT;
                                                retValue = "Tape memory is write-protected by the CNC parameter setting. " + retcnc.ToString();
                                            }
                                            else if (retcnc == Focas1.focas_ret.EW_OVRFLOW)
                                            {
                                                retValueInt = (int)Focas1.focas_ret.EW_OVRFLOW;
                                                retValue = "Make enough free area in CNC memory. " + retcnc.ToString();
                                            }
                                            else if (retcnc == Focas1.focas_ret.EW_BUFFER)
                                            {
                                                retValueInt = (int)Focas1.focas_ret.EW_BUFFER;
                                                retValue = "Retry because the buffer is full. " + retcnc.ToString();
                                            }
                                            else if (retcnc == Focas1.focas_ret.EW_REJECT)
                                            {
                                                retValueInt = (int)Focas1.focas_ret.EW_REJECT;
                                                retValue = "Downloading is disable in the current CNC status " + retcnc.ToString();
                                            }
                                            else if (retcnc == Focas1.focas_ret.EW_ALARM)
                                            {
                                                retValueInt = (int)Focas1.focas_ret.EW_ALARM;
                                                retValue = "Alarm has occurred while downloading. " + retcnc.ToString();
                                            }

                                        }
                                        retValue = retcnc.ToString();
                                    }
                                }

                                Focas1.focas_ret retncend = (Focas1.focas_ret)Focas1.cnc_dwnend3(FlibHndl);
                                if (retncend == Focas1.focas_ret.EW_OK)
                                {
                                    retValueInt = (int)Focas1.focas_ret.EW_OK;
                                    retValue = "Success";
                                    retValue = "cnc_dwnend3 executed succesfully " + retncend.ToString();
                                }
                                else
                                {
                                    if (retncend == Focas1.focas_ret.EW_FUNC)
                                    {
                                        retValueInt = (int)Focas1.focas_ret.EW_FUNC;
                                        retValue = "cnc_dwnstart3 function has not been executed. " + retncend.ToString();
                                    }
                                    else if (retncend == Focas1.focas_ret.EW_DATA)
                                    {
                                        retValueInt = (int)Focas1.focas_ret.EW_DATA;
                                        retValue = "Data error. " + retncend.ToString();
                                    }
                                    else if (retncend == Focas1.focas_ret.EW_OVRFLOW)
                                    {
                                        retValueInt = (int)Focas1.focas_ret.EW_OVRFLOW;
                                        retValue = "Make enough free area in CNC memory. " + retncend.ToString();
                                    }
                                    else if (retncend == Focas1.focas_ret.EW_PROT)
                                    {
                                        retValueInt = (int)Focas1.focas_ret.EW_PROT;
                                        retValue = "Tape memory is write-protected by the CNC parameter setting. " + retncend.ToString();
                                    }
                                    else if (retncend == Focas1.focas_ret.EW_REJECT)
                                    {
                                        retValueInt = (int)Focas1.focas_ret.EW_REJECT;
                                        retValue = "Downloading is disable in the current CNC status. " + retncend.ToString();
                                    }
                                    else if (retncend == Focas1.focas_ret.EW_ALARM)
                                    {
                                        retValueInt = (int)Focas1.focas_ret.EW_ALARM;
                                        retValue = "Alarm has occurred while downloading. " + retncend.ToString();
                                    }

                                }
                                retValue = retncend.ToString();
                            }
                            else
                            {
                                if (retncstart == Focas1.focas_ret.EW_BUSY)
                                {
                                    retValueInt = (int)Focas1.focas_ret.EW_BUSY;
                                    retValue = "Busy. " + retncstart.ToString();
                                }
                                else if (retncstart == Focas1.focas_ret.EW_ATTRIB)
                                {
                                    retValueInt = (int)Focas1.focas_ret.EW_ATTRIB;
                                    retValue = "Data type (type) is illegal. " + retncstart.ToString();
                                }
                                else if (retncstart == Focas1.focas_ret.EW_NOOPT)
                                {
                                    retValueInt = (int)Focas1.focas_ret.EW_NOOPT;
                                    retValue = "No option. " + retncstart.ToString();
                                }
                                else if (retncstart == Focas1.focas_ret.EW_PARAM)
                                {
                                    retValueInt = (int)Focas1.focas_ret.EW_PARAM;
                                    retValue = "CNC parameter error. " + retncstart.ToString();
                                }
                                else if (retncstart == Focas1.focas_ret.EW_MODE)
                                {
                                    retValueInt = (int)Focas1.focas_ret.EW_MODE;
                                    retValue = "CNC mode error. " + retncstart.ToString();
                                }
                                else if (retncstart == Focas1.focas_ret.EW_REJECT)
                                {
                                    retValueInt = (int)Focas1.focas_ret.EW_REJECT;
                                    retValue = "CNC is machining, so Rejected. " + retncstart.ToString();
                                }
                                else if (retncstart == Focas1.focas_ret.EW_ALARM)
                                {
                                    retValueInt = (int)Focas1.focas_ret.EW_ALARM;
                                    retValue = "Alarm State error, reset the alarm on CNC. " + retncstart.ToString();
                                }
                                else if (retncstart == Focas1.focas_ret.EW_PASSWD)
                                {
                                    retValueInt = (int)Focas1.focas_ret.EW_PASSWD;
                                    retValue = "Specified CNC data cannot be written because the data is protected.. " + retncstart.ToString();
                                }

                            }
                            retValue = retncstart.ToString();
                        }
                    }
                }
                else
                {
                    if (retallclibhndl3 == Focas1.focas_ret.EW_SOCKET)
                    {
                        retValueInt = (int)Focas1.focas_ret.EW_SOCKET;
                        retValue = "Socket communication error. " + retallclibhndl3.ToString();
                    }
                    else if (retallclibhndl3 == Focas1.focas_ret.EW_NODLL)
                    {
                        retValueInt = (int)Focas1.focas_ret.EW_NODLL;
                        retValue = "There is no DLL file for each CNC series . " + retallclibhndl3.ToString();
                    }
                    else if (retallclibhndl3 == Focas1.focas_ret.EW_HANDLE)
                    {
                        retValueInt = (int)Focas1.focas_ret.EW_HANDLE;
                        retValue = "Allocation of handle number is failed. " + retallclibhndl3.ToString();
                    }

                    retValue = retallclibhndl3.ToString();
                }

                using (i_facility_shaktiEntities1 redb = new i_facility_shaktiEntities1())
                {
                    var RecordToUpdate = redb.tblprogramtransferhistories.Find(pthID);
                    if (RecordToUpdate != null)
                    {
                        RecordToUpdate.ReturnTime = DateTime.Now;
                        RecordToUpdate.ReturnStatus = retValueInt;
                        RecordToUpdate.ReturnDesc = retValue;
                        redb.Entry(RecordToUpdate).State = System.Data.Entity.EntityState.Modified;
                        redb.SaveChanges();

                    }
                    else
                    {
                        //TextLogFile("Unable to Find Latest Record to update Error/EndTime.");
                    }
                }

            }
            catch (Exception e)
            {
                retValue += e.ToString();
            }

            return retValue;
        }

        //public void TextLogFile(string msg)
        //{
        //    string FileName = @"~/Content/LogFile.txt";
        //    StreamWriter sw = new StreamWriter(FileName, true);
        //    sw.WriteLine("" + DateTime.Now);
        //    sw.WriteLine("" + msg);

        //}

    }

}
