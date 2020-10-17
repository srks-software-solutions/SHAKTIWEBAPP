using Newtonsoft.Json;
using SRKSDemo.OperatorEntryModelClass;
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;

namespace SRKSDemo.Controllers
{
    public class OperatorEntryModelController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        // GET: OperatorEntryModel
        public ActionResult Index()
        {
            MachineStatusMode();
            int OperatorloginID = Convert.ToInt32(Session["OUserID"]);
            string OperatorName = Convert.ToString(Session["OUsername"]);
            int NumberOfMachine = Convert.ToInt32(Session["OMachineNo"]);
            string LoginTime = Convert.ToString(Session["LoginTime"]);
            int NoMachine = Convert.ToInt32(Session["OMachineNo"]);
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            string ShiftName = GetShift();
            ViewBag.Shift = ShiftName;
            ViewBag.NoMachines = NoMachine;
            ViewBag.LoginTime = LoginTime;
            ViewBag.Operatorname = OperatorName;
            ViewBag.OperatorIDs = Operatorid;
            DateTime Corr = DateTime.Now.Date;
            List<MainDetails> MainDetailsListObj = new List<MainDetails>();
            var OperatorMachineDetails = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorloginID).ToList();
            foreach (var MachRow in OperatorMachineDetails)
            {
                int Machineid = Convert.ToInt32(MachRow.machineId);
                var WorkOrderentryDet = db.tblworkorderentries.Where(m => m.OperatorID == Operatorid && m.IsStarted == 1 && m.IsFinished == 0 && m.CorrectedDate == Corr && m.MachineID == Machineid).ToList();
                ViewBag.OperationNo = db.tblmachinedetails.Where(m => m.MachineID == Machineid).Select(m => m.OperationNumber).FirstOrDefault();
                if (WorkOrderentryDet.Count != 0)
                {
                    string CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
                    //CorrectedDate = "2019-06-24";
                    int Hour = DateTime.Now.Hour;
                    int NextHour = Hour + 1;
                    if (NextHour == 24)
                    {
                        NextHour = 00;
                    }
                    DateTime StartHour = Convert.ToDateTime(CorrectedDate + " " + Hour + ":00:00");
                    DateTime EndHour = Convert.ToDateTime(CorrectedDate + " " + NextHour + ":00:00");
                    string ShiftGet = GetShift();
                    var Shift = db.tblshift_mstr.Where(m => m.IsDeleted == 0 && m.ShiftName == ShiftGet).FirstOrDefault();
                    DateTime ShiftStratTime = Convert.ToDateTime(CorrectedDate + " " + Shift.StartTime);
                    DateTime ShiftEndTime = Convert.ToDateTime(CorrectedDate + " " + Shift.EndTime);
                    foreach (var Row in WorkOrderentryDet)
                    {
                        int ShiftID = Row.ShiftID;
                        int PartsActual = 0, PartsTarget = 0, ShiftActual = 0, ShiftTarget = 0;
                        var PartsActualA = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime == StartHour && m.EndTime == EndHour).FirstOrDefault();
                        var PartsTargetT = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime == StartHour && m.EndTime == EndHour).FirstOrDefault();
                        var ShiftCountA = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).ToList();
                        if (PartsActualA != null)
                        {
                            PartsActualA = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime == StartHour && m.EndTime == EndHour).FirstOrDefault();
                            PartsActual = Convert.ToInt32(PartsActualA.WoPartCount);
                        }
                        else
                        {
                            PartsActual = 0;
                        }
                        if (PartsTargetT != null)
                        {
                            PartsTargetT = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime == StartHour && m.EndTime == EndHour).FirstOrDefault();
                            PartsTarget = Convert.ToInt32(PartsTargetT.woTargetQty);
                        }
                        else
                        {
                            PartsTarget = 0;
                        }
                        if (ShiftCountA.Count == 0)
                        {
                            ShiftActual = 0;
                        }
                        else
                        {
                            ShiftActual = Convert.ToInt32(db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).Sum(m => m.WoPartCount));
                        }
                        var ShiftCountT = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).ToList();
                        if (ShiftCountT.Count == 0)
                        {
                            ShiftTarget = 0;
                        }
                        else
                        {
                            ShiftTarget = Convert.ToInt32(db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).Sum(m => m.woTargetQty));
                        }
                        int PartPerCycle = Convert.ToInt32(Row.PartsPerCycle);
                        MainDetails MainDet = new MainDetails();
                        MainDet.MachineName = db.tblmachinedetails.Where(m => m.MachineID == Row.MachineID).Select(m => m.MachineDisplayName).FirstOrDefault();
                        MainDet.MachineStatusColor = db.tbllivemodes.Where(m => m.MachineID == Row.MachineID && m.IsCompleted == 0).OrderByDescending(m => m.ModeID).Select(m => m.ColorCode).FirstOrDefault();
                        MainDet.PartsCountActual = (PartsActual) /** PartPerCycle*/;
                        MainDet.PartsCountTarget = (PartsTarget) /**PartPerCycle*/;
                        MainDet.ShiftCountAtcual = (ShiftActual) /**PartPerCycle*/;
                        MainDet.ShiftCountTarget = (ShiftTarget) /**PartPerCycle*/;
                        MainDet.WONumber = Row.Prod_Order_No;
                        MainDet.PartNumber = Row.FGCode;
                        MainDet.OperationNo = Row.OperationNo;
                        MainDet.WOStartTime = Convert.ToString(Row.WOStart);
                        //MainDet.Shift = db.tblshift_mstr.Where(m => m.IsDeleted == 0 && m.ShiftID == ShiftID).Select(m => m.ShiftName).FirstOrDefault();
                        MainDet.MachineId = Row.MachineID;
                        MainDet.WOQty = Row.Total_Qty;
                        MainDetailsListObj.Add(MainDet);
                    }
                }
                else
                {
                    string CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
                    //CorrectedDate = "2019-06-24";
                    MainDetails MainDet = new MainDetails();
                    MainDet.MachineName = db.tblmachinedetails.Where(m => m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();
                    MainDet.MachineStatusColor = db.tbllivemodes.Where(m => m.MachineID == Machineid && m.IsCompleted == 0).OrderByDescending(m => m.ModeID).Select(m => m.ColorCode).FirstOrDefault();
                    MainDet.PartsCountActual = 0;
                    MainDet.PartsCountTarget = 0;
                    MainDet.ShiftCountAtcual = 0;
                    MainDet.ShiftCountTarget = 0;
                    MainDet.WONumber = "0";
                    MainDet.PartNumber = "0";
                    MainDet.OperationNo = "0";
                    //MainDet.Shift = GetShift();
                    MainDet.MachineId = Machineid;
                    MainDet.WOQty = 0;
                    MainDet.WOStartTime = "";
                    MainDetailsListObj.Add(MainDet);
                }
            }
            return View(MainDetailsListObj);
        }

        public string getmachindet()
        {
            string res = "";
            int OperatorloginID = Convert.ToInt32(Session["OUserID"]);
            string OperatorName = Convert.ToString(Session["OUsername"]);
            int NumberOfMachine = Convert.ToInt32(Session["OMachineNo"]);
            string LoginTime = Convert.ToString(Session["LoginTime"]);
            int NoMachine = Convert.ToInt32(Session["OMachineNo"]);
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            string ShiftName = GetShift();
            ViewBag.Shift = ShiftName;
            ViewBag.NoMachines = NoMachine;
            ViewBag.LoginTime = LoginTime;
            ViewBag.Operatorname = OperatorName;
            ViewBag.OperatorIDs = Operatorid;
            DateTime Corr = DateTime.Now.Date;
            List<MainDetails> MainDetailsListObj = new List<MainDetails>();
            var OperatorMachineDetails = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorloginID).ToList();
            foreach (var MachRow in OperatorMachineDetails)
            {
                int Machineid = Convert.ToInt32(MachRow.machineId);
                var WorkOrderentryDet = db.tblworkorderentries.Where(m => m.OperatorID == Operatorid && m.IsStarted == 1 && m.IsFinished == 0 && m.CorrectedDate == Corr && m.MachineID == Machineid).ToList();
                if (WorkOrderentryDet.Count != 0)
                {
                    string CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
                    //CorrectedDate = "2019-06-24";
                    int Hour = DateTime.Now.Hour;
                    int NextHour = Hour + 1;
                    if (NextHour == 24)
                    {
                        NextHour = 00;
                    }
                    DateTime StartHour = Convert.ToDateTime(CorrectedDate + " " + Hour + ":00:00");
                    DateTime EndHour = Convert.ToDateTime(CorrectedDate + " " + NextHour + ":00:00");
                    string ShiftGet = GetShift();
                    var Shift = db.tblshift_mstr.Where(m => m.IsDeleted == 0 && m.ShiftName == ShiftGet).FirstOrDefault();
                    DateTime ShiftStratTime = Convert.ToDateTime(CorrectedDate + " " + Shift.StartTime);
                    DateTime ShiftEndTime = Convert.ToDateTime(CorrectedDate + " " + Shift.EndTime);
                    foreach (var Row in WorkOrderentryDet)
                    {
                        int ShiftID = Row.ShiftID;
                        int PartsActual = 0, PartsTarget = 0, ShiftActual = 0, ShiftTarget = 0;
                        var PartsActualA = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime == StartHour && m.EndTime == EndHour).FirstOrDefault();
                        var PartsTargetT = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime == StartHour && m.EndTime == EndHour).FirstOrDefault();
                        var ShiftCountA = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).ToList();
                        if (PartsActualA != null)
                        {
                            PartsActualA = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime == StartHour && m.EndTime == EndHour).FirstOrDefault();
                            PartsActual = Convert.ToInt32(PartsActualA.WoPartCount);
                        }
                        else
                        {
                            PartsActual = 0;
                        }
                        if (PartsTargetT != null)
                        {
                            PartsTargetT = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime == StartHour && m.EndTime == EndHour).FirstOrDefault();
                            PartsTarget = Convert.ToInt32(PartsTargetT.woTargetQty);
                        }
                        else
                        {
                            PartsTarget = 0;
                        }
                        if (ShiftCountA.Count == 0)
                        {
                            ShiftActual = 0;
                        }
                        else
                        {
                            ShiftActual = Convert.ToInt32(db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).Sum(m => m.WoPartCount));
                        }
                        var ShiftCountT = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).ToList();
                        if (ShiftCountT.Count == 0)
                        {
                            ShiftTarget = 0;
                        }
                        else
                        {
                            ShiftTarget = Convert.ToInt32(db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).Sum(m => m.woTargetQty));
                        }
                        int PartPerCycle = Convert.ToInt32(Row.PartsPerCycle);
                        MainDetails MainDet = new MainDetails();
                        MainDet.MachineName = db.tblmachinedetails.Where(m => m.MachineID == Row.MachineID).Select(m => m.MachineDisplayName).FirstOrDefault();
                        MainDet.MachineStatusColor = db.tbllivemodes.Where(m => m.MachineID == Row.MachineID && m.IsCompleted == 0).OrderByDescending(m => m.ModeID).Select(m => m.ColorCode).FirstOrDefault();
                        MainDet.PartsCountActual = (PartsActual) /** PartPerCycle*/;
                        MainDet.PartsCountTarget = (PartsTarget) /**PartPerCycle*/;
                        MainDet.ShiftCountAtcual = (ShiftActual) /**PartPerCycle*/;
                        MainDet.ShiftCountTarget = (ShiftTarget) /**PartPerCycle*/;
                        MainDet.WONumber = Row.Prod_Order_No;
                        MainDet.PartNumber = Row.FGCode;
                        MainDet.OperationNo = Row.OperationNo;
                        MainDet.WOStartTime = Convert.ToString(Row.WOStart);
                        //MainDet.Shift = db.tblshift_mstr.Where(m => m.IsDeleted == 0 && m.ShiftID == ShiftID).Select(m => m.ShiftName).FirstOrDefault();
                        MainDet.MachineId = Row.MachineID;
                        MainDet.WOQty = Row.Total_Qty;
                        MainDetailsListObj.Add(MainDet);
                    }
                }
                else
                {
                    string CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
                    //CorrectedDate = "2019-06-24";
                    MainDetails MainDet = new MainDetails();
                    MainDet.MachineName = db.tblmachinedetails.Where(m => m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();
                    MainDet.MachineStatusColor = db.tbllivemodes.Where(m => m.MachineID == Machineid && m.IsCompleted == 0).OrderByDescending(m => m.ModeID).Select(m => m.ColorCode).FirstOrDefault();
                    MainDet.PartsCountActual = 0;
                    MainDet.PartsCountTarget = 0;
                    MainDet.ShiftCountAtcual = 0;
                    MainDet.ShiftCountTarget = 0;
                    MainDet.WONumber = "0";
                    MainDet.PartNumber = "0";
                    MainDet.OperationNo = "0";
                    //MainDet.Shift = GetShift();
                    MainDet.MachineId = Machineid;
                    MainDet.WOQty = 0;
                    MainDet.WOStartTime = "";
                    MainDetailsListObj.Add(MainDet);
                }
                res = JsonConvert.SerializeObject(MainDetailsListObj);
            }
            return res;
        }

        public string MachineColor()
        {
            string Result = "";
            int OperatorloginID = Convert.ToInt32(Session["OUserID"]);
            var OperatorMachineDetails = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorloginID).ToList();
            List<MachNameCol> MachNameColList = new List<MachNameCol>();
            foreach (var MachRow in OperatorMachineDetails)
            {
                MachNameCol MachNameColobj = new MachNameCol();
                int Machineid = Convert.ToInt32(MachRow.machineId);
                var ModeDetCom = db.tbllivemodes.Where(m => m.IsCompleted == 0 && m.IsDeleted == 0 && m.MachineID == Machineid).FirstOrDefault();
                MachNameColobj.MachineID = Machineid;
                MachNameColobj.Colour = ModeDetCom.ColorCode;
                MachNameColobj.MchineName = db.tblmachinedetails.Where(m => m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();
                MachNameColList.Add(MachNameColobj);
            }
            Result = JsonConvert.SerializeObject(MachNameColList);
            return Result;
        }
        //public string GetShift()
        //{
        //    string shift = "";
        //    DateTime Time1 = DateTime.Now;
        //    //TimeSpan Tm1 = new TimeSpan(Time1.Hour, Time1.Minute, Time1.Second);
        //    //var Shiftdetails = db.tblshift_mstr.Where(m => m.StartTime <= Tm1 && m.EndTime >= Tm1).FirstOrDefault();
        //    //if (Shiftdetails != null)
        //    //{
        //    //    shift = Shiftdetails.ShiftName;
        //    //}
        //    int DayHours = Time1.Hour;
        //    if (DayHours >= 6 && DayHours < 14)
        //    {
        //        shift = "A";
        //    }
        //    else if (DayHours >= 14 && DayHours < 22)
        //    {
        //        shift = "B";
        //    }
        //    else
        //    {
        //        shift = "C";
        //    }

        //    return shift;
        //}

        public string GetShift()
        {
            string ShiftValue = "";
            DateTime DateNow = DateTime.Now;
            var ShiftDetails = db.tblshift_mstr.Where(m => m.IsDeleted == 0).ToList();
            foreach (var row in ShiftDetails)
            {
                int ShiftStartHour = row.StartTime.Value.Hours;
                int ShiftEndHour = row.EndTime.Value.Hours;
                int CurrentHour = DateNow.Hour;
                if (CurrentHour >= ShiftStartHour && CurrentHour < ShiftEndHour)
                {
                    ShiftValue = row.ShiftName;
                    break;
                }
            }

            if(ShiftValue=="")
            {
                ShiftValue = "C";
            }

            return ShiftValue;
        }

        public string GetPartsCount()
        {
            string result = "";
            DateTime Corr = DateTime.Now.Date;
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            int OperatorloginID = Convert.ToInt32(Session["OUserID"]);
            List<PartsCountDet> PartCountList = new List<PartsCountDet>();
            var OpMachineDet = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorloginID).ToList();
            foreach (var Row in OpMachineDet)
            {
                int MachineID = Convert.ToInt32(Row.machineId);
                var WorkOrderEntryDet = db.tblworkorderentries.Where(m => m.OperatorID == Operatorid && m.IsStarted == 1 && m.IsFinished == 0 && m.CorrectedDate == Corr && m.MachineID == MachineID).ToList();
                if (WorkOrderEntryDet != null)
                {
                    int PartPerCycle = Convert.ToInt32(Session["PartPerCycle"]);
                    string CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
                    CorrectedDate = "2019-06-24";
                    int Hour = DateTime.Now.Hour;
                    int NextHour = Hour + 1;
                    DateTime StartHour = Convert.ToDateTime(CorrectedDate + " " + Hour + ":00:00");
                    DateTime EndHour = Convert.ToDateTime(CorrectedDate + " " + NextHour + ":00:00");
                    string ShiftGet = GetShift();
                    var Shift = db.tblshift_mstr.Where(m => m.IsDeleted == 0 && m.ShiftName == ShiftGet).FirstOrDefault();
                    DateTime ShiftStratTime = Convert.ToDateTime(CorrectedDate + " " + Shift.StartTime);
                    DateTime ShiftEndTime = Convert.ToDateTime(CorrectedDate + " " + Shift.EndTime);

                    int PartsActual = 0, PartsTarget = 0, ShiftActual = 0, ShiftTarget = 0;
                    var PartsActualA = db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.StartTime == StartHour && m.EndTime == EndHour).FirstOrDefault();
                    var PartsTargetT = db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.StartTime == StartHour && m.EndTime == EndHour).FirstOrDefault();
                    var ShiftCountA = db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).ToList();
                    if (PartsActualA != null)
                    {
                        PartsActualA = db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.StartTime == StartHour && m.EndTime == EndHour).FirstOrDefault();
                        PartsActual = Convert.ToInt32(PartsActualA.WoPartCount);
                    }
                    else
                    {
                        PartsActual = 0;
                    }
                    if (PartsTargetT != null)
                    {
                        PartsTargetT = db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.StartTime == StartHour && m.EndTime == EndHour).FirstOrDefault();
                        PartsTarget = Convert.ToInt32(PartsTargetT.woTargetQty);
                    }
                    else
                    {
                        PartsTarget = 0;
                    }
                    if (ShiftCountA.Count == 0)
                    {
                        ShiftActual = 0;
                    }
                    else
                    {
                        ShiftActual = Convert.ToInt32(db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).Sum(m => m.WoPartCount));
                    }
                    var ShiftCountT = db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).ToList();
                    if (ShiftCountT.Count() == 0)
                    {
                        ShiftTarget = 0;
                    }
                    else
                    {
                        ShiftTarget = Convert.ToInt32(db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).Sum(m => m.woTargetQty));
                    }
                    PartsCountDet PartsCountObj = new PartsCountDet();
                    PartsCountObj.MachineID = MachineID;
                    PartsCountObj.PartsCountActual = (PartsActual) /** PartPerCycle*/;
                    PartsCountObj.PartsCountTarget = (PartsTarget) /** PartPerCycle*/;
                    PartsCountObj.ShiftCountAtcual = (ShiftActual) /** PartPerCycle*/;
                    PartsCountObj.ShiftCountTarget = (ShiftTarget)/* * PartPerCycle*/;
                    PartCountList.Add(PartsCountObj);
                }
            }
            result = JsonConvert.SerializeObject(PartCountList);
            return result;
        }

        public string GetShiftDet()
        {
            string Result = "";
            var ShiftDetails = db.tblshift_mstr.Where(m => m.IsDeleted == 0).ToList();
            Result = JsonConvert.SerializeObject(ShiftDetails);
            return Result;
        }

        public string GetPartNoDet(string opNo)
        {
            string Result = "";
            var PartDetails = db.tblparts.Where(m => m.OperationNo == opNo && m.IsDeleted == 0).Select(m => new { m.FGCode, m.PartID }).ToList();
            Result = JsonConvert.SerializeObject(PartDetails);
            return Result;
        }

        public string GetHoldDet()
        {
            string Result = "";
            var ShiftDetails = db.tblholdcodes.Where(m => m.IsDeleted == 0).ToList();
            Result = JsonConvert.SerializeObject(ShiftDetails);
            return Result;
        }

        public string InsertData(int machineID, int Shift, string PartNo, string OPNO, string WONo, int WOQValue, string OperatorID, int PartPerCycle)
        {
            string result = "Fail";
            DateTime Corr = DateTime.Now.Date;
            var WOdet = db.tblworkorderentries.Where(m => m.MachineID == machineID && m.IsFinished == 0 && m.CorrectedDate == Corr).FirstOrDefault();
            if (WOdet == null)
            {
                if (machineID != 0 && PartNo != null)
                {
                    //Session["PartPerCycle"] = PartPerCycle;
                    DateTime Correcteddate = DateTime.Now;
                    tblworkorderentry obj = new tblworkorderentry();
                    obj.MachineID = machineID;
                    obj.WOStart = Correcteddate;
                    obj.PartNo = PartNo;
                    obj.ShiftID = Shift;
                    obj.OperatorID = OperatorID;
                    obj.Prod_Order_No = WONo;
                    obj.OperationNo = OPNO;
                    obj.Yield_Qty = 0;
                    obj.ScrapQty = 0;
                    obj.Total_Qty = WOQValue;
                    obj.ProcessQty = 0;
                    obj.Status = 0;
                    obj.CorrectedDate = Correcteddate.Date;
                    obj.IsStarted = 1;
                    obj.IsFinished = 0;
                    obj.IsPartialFinish = 0;
                    obj.isWorkOrder = 1;
                    obj.PEStartTime = Correcteddate;
                    obj.FGCode = PartNo;
                    obj.PartsPerCycle = PartPerCycle;
                    db.tblworkorderentries.Add(obj);
                    db.SaveChanges();
                    result = "Success";
                }
            }
            return result;

        }

        public string OperatorEntryDetails(int id)
        {
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            DateTime CorrectedDate = DateTime.Now.Date;
            string result = "FAIL";
            List<WOEntry> WOEntryList = new List<WOEntry>();
            var WorkOrderentryDet = db.tblworkorderentries.Where(m => m.OperatorID == Operatorid && m.IsStarted == 1 && m.IsFinished == 0 && m.CorrectedDate == CorrectedDate && m.MachineID == id).FirstOrDefault();
            if (WorkOrderentryDet != null)
            {
                int ShiftID = WorkOrderentryDet.ShiftID;
                WOEntry WOEntryObj = new WOEntry();
                WOEntryObj.OperationNo = WorkOrderentryDet.OperationNo;
                WOEntryObj.PartNo = WorkOrderentryDet.FGCode;

                WOEntryObj.ShiftID = db.tblshift_mstr.Where(m => m.IsDeleted == 0 && m.ShiftID == ShiftID).Select(m => m.ShiftName).FirstOrDefault();
                WOEntryObj.WONO = WorkOrderentryDet.Prod_Order_No;
                WOEntryObj.WOQTY = WorkOrderentryDet.Total_Qty;
                WOEntryObj.PartPerCycle = Convert.ToInt32(WorkOrderentryDet.PartsPerCycle);
                WOEntryList.Add(WOEntryObj);
                result = JsonConvert.SerializeObject(WOEntryList);
            }
            return result;
        }

        public string GetMachineBaseStart()
        {
            string Result = "";
            DateTime Corr = DateTime.Now.Date;
            int OperatorloginID = Convert.ToInt32(Session["OUserID"]);
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            List<MachineTrue> MachineTrueList = new List<MachineTrue>();
            var OperatorMachineDetails = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorloginID).ToList();
            foreach (var MachRow in OperatorMachineDetails)
            {
                MachineTrue MachineTrueobj = new MachineTrue();
                int Machineid = Convert.ToInt32(MachRow.machineId);
                MachineTrueobj.MachineID = Machineid;
                var WorkOrderentryDet = db.tblworkorderentries.Where(m => m.OperatorID == Operatorid && m.IsStarted == 1 && m.IsFinished == 0 && m.CorrectedDate == Corr && m.MachineID == Machineid).ToList();
                if (WorkOrderentryDet.Count != 0)
                {
                    MachineTrueobj.TrueorFalse = "True";
                }
                else
                {
                    MachineTrueobj.TrueorFalse = "False";
                }
                MachineTrueList.Add(MachineTrueobj);
            }
            Result = JsonConvert.SerializeObject(MachineTrueList);
            return Result;
        }

        public string UpdateData(int machineID, string PartNo, string OPNO, string WONo, int WOQValue, string OperatorID)
        {
            string result = "Fail";
            if (machineID != 0 && PartNo != null)
            {
                var WorkOrder = db.tblworkorderentries.Where(m => m.IsFinished == 0 && m.IsStarted == 1 && m.PartNo == PartNo && m.MachineID == machineID && m.Prod_Order_No == WONo).FirstOrDefault();
                if (WorkOrder.ReWorkStart == 1 && WorkOrder.ReWorkStart == 1)
                {
                    DateTime Correcteddate = DateTime.Now;
                    WorkOrder.WOEnd = Correcteddate;
                    WorkOrder.IsFinished = 1;
                    db.SaveChanges();
                    result = "Success";
                }
                else if (WorkOrder.ReWorkStart == 0 && WorkOrder.ReWorkEnd == 0)
                {
                    DateTime Correcteddate = DateTime.Now;
                    WorkOrder.WOEnd = Correcteddate;
                    WorkOrder.IsFinished = 1;
                    db.SaveChanges();
                    result = "Success";
                }
                else
                {
                    result = "Fail";
                }
            }

            return result;
        }

        private int ShiftDet()
        {
            int ShiftValue = 0;
            DateTime DateNow = DateTime.Now;
            var ShiftDetails = db.tblshift_mstr.Where(m => m.IsDeleted == 0).ToList();
            foreach (var row in ShiftDetails)
            {
                int ShiftStartHour = row.StartTime.Value.Hours;
                int ShiftEndHour = row.EndTime.Value.Hours;
                int CurrentHour = DateNow.Hour;
                if (CurrentHour >= ShiftStartHour && CurrentHour <= ShiftEndHour)
                {
                    ShiftValue = row.ShiftID;
                }
            }

            return ShiftValue;
        }

        public void ModeCheckBreakdown(int Machineid)
        {            
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            DateTime Corr = DateTime.Now.Date;
            var ModeDet = db.tbllivemodes.Where(m => m.IsCompleted == 0 && m.MachineID == Machineid).OrderByDescending(m => m.ModeID).FirstOrDefault();
            if (ModeDet != null)
            {
                DateTime ET = DateTime.Now;
                string MacMode = ModeDet.MacMode;
                string MColour = ModeDet.ColorCode;
                if (MColour != "RED" && MacMode != "MNT")
                {
                    DateTime St = Convert.ToDateTime(ModeDet.StartTime);
                    double Duration = ET.Subtract(St).TotalSeconds;
                    ModeDet.IsCompleted = 1;
                    ModeDet.EndTime = ET;
                    ModeDet.ModeTypeEnd = 1;
                    ModeDet.DurationInSec = Convert.ToInt32(Duration);
                    db.SaveChanges();

                    int BreakDownID = Convert.ToInt32(db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.ProdFinished == null).OrderByDescending(m => m.id).Select(m => m.reasonId).FirstOrDefault());
                    int ShiftID = ShiftDet();
                    try
                    {
                        tbllivemode RowAdd = new tbllivemode();
                        RowAdd.MachineID = Machineid;
                        RowAdd.MacMode = "MNT";
                        RowAdd.InsertedOn = ET;
                        RowAdd.InsertedBy = 1;
                        RowAdd.CorrectedDate = ET.Date;
                        RowAdd.IsDeleted = 0;
                        RowAdd.StartTime = ET;
                        RowAdd.ColorCode = "RED";
                        RowAdd.breakDownCodeID = BreakDownID;                        
                        RowAdd.IsCompleted = 0;
                        RowAdd.ModeType = "MNT";
                        RowAdd.ModeTypeEnd = 0;
                        RowAdd.IsShiftEnd = ShiftID;
                        db.tbllivemodes.Add(RowAdd);
                        db.SaveChanges();
                    }
                    catch(Exception ex)
                    {

                    }
                }
                else if (MColour == "RED" && MacMode == "MNT")
                {
                    var BreakDownTicket = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.operatorId== Operatorid && m.correctedDate== Corr && m.reasonId!=null).OrderByDescending(m => m.id).ToList();
                    tblBreakDownTickect data = new tblBreakDownTickect();
                    tblBreakDownTickect data1 = new tblBreakDownTickect();
                    if (BreakDownTicket.Count>1)
                    {
                        data = BreakDownTicket.Skip(1).Take(1).Single();
                        data1 = BreakDownTicket.Take(1).Single();
                    }
                    else
                    {
                        data = BreakDownTicket.Take(1).Single();
                        data1 = BreakDownTicket.Take(1).Single();
                    }
                    
                    if (data.mntRrejectReason == null && data.prodRejId==null)
                    {
                        DateTime St = Convert.ToDateTime(ModeDet.StartTime);
                        double Duration = ET.Subtract(St).TotalSeconds;
                        ModeDet.IsCompleted = 1;
                        ModeDet.EndTime = ET;
                        ModeDet.ModeTypeEnd = 1;
                        ModeDet.DurationInSec = Convert.ToInt32(Duration);
                        db.SaveChanges();

                        int ShiftID = ShiftDet();
                        try
                        {
                            tbllivemode RowAdd = new tbllivemode();
                            RowAdd.MachineID = Machineid;
                            RowAdd.MacMode = "IDLE";
                            RowAdd.InsertedOn = ET;
                            RowAdd.InsertedBy = 1;
                            RowAdd.CorrectedDate = ET.Date;
                            RowAdd.IsDeleted = 0;
                            RowAdd.StartTime = ET;
                            RowAdd.ColorCode = "YELLOW";
                            RowAdd.IsCompleted = 0;
                            RowAdd.ModeType = "IDLE";
                            RowAdd.IsShiftEnd = ShiftID;
                            db.tbllivemodes.Add(RowAdd);
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else if(data1.ProdFinished==1 && data1.prodRejId==null)
                    {
                        DateTime St = Convert.ToDateTime(ModeDet.StartTime);
                        double Duration = ET.Subtract(St).TotalSeconds;
                        ModeDet.IsCompleted = 1;
                        ModeDet.EndTime = ET;
                        ModeDet.ModeTypeEnd = 1;
                        ModeDet.DurationInSec = Convert.ToInt32(Duration);
                        db.SaveChanges();

                        int ShiftID = ShiftDet();
                        try
                        {
                            tbllivemode RowAdd = new tbllivemode();
                            RowAdd.MachineID = Machineid;
                            RowAdd.MacMode = "IDLE";
                            RowAdd.InsertedOn = ET;
                            RowAdd.InsertedBy = 1;
                            RowAdd.CorrectedDate = ET.Date;
                            RowAdd.IsDeleted = 0;
                            RowAdd.StartTime = ET;
                            RowAdd.ColorCode = "YELLOW";
                            RowAdd.IsCompleted = 0;
                            RowAdd.ModeType = "IDLE";
                            RowAdd.IsShiftEnd = ShiftID;
                            db.tbllivemodes.Add(RowAdd);
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                    {
                        ModeDet.breakDownCodeID = data1.reasonId;
                        db.SaveChanges();
                    }
                }
            }
        }

        //public string BreakDownDet(int Machineid)
        //{
        //    DateTime Correcteddate = DateTime.Now.Date;
        //    string Result = "";
        //    string Operatorid = Convert.ToString(Session["OperatorID"]);
        //    var BreakDownTickectDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.operatorId == Operatorid && m.reasonId != null && m.correctedDate == Correcteddate).OrderByDescending(m => m.id).FirstOrDefault();

        //    if (BreakDownTickectDet != null)
        //    {
        //        if (BreakDownTickectDet.MaintFinished == 1 && BreakDownTickectDet.ProdFinished == 1)
        //        {
        //            var BreakdownDet = db.tblBreakdowncodes.Where(m => m.IsDeleted == 0 && m.BreakdownLevel == 1).Select(m => new { m.BreakdownID, m.BreakdownCode }).ToList();
        //            Result = JsonConvert.SerializeObject(BreakdownDet);
        //        }
        //        else if (BreakDownTickectDet.mntStatus == true)
        //        {

        //            string UserName = Convert.ToString(Session["maintUser"]);
        //            string Password = Convert.ToString(Session["maintpwd"]);
        //            var OperatorLoginDet = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorUserName == UserName && m.operatorPwd == Password && m.roleId == 9).FirstOrDefault();
        //            if (OperatorLoginDet != null)
        //            {
        //                Result = "maintAccept";
        //                MacintanceAccp MacintanceAccpobj = new MacintanceAccp();
        //                MacintanceAccpobj.MachineName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();/*&& m.operatorId == Operatorid && m.correctedDate == dt*/
        //                var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.correctedDate == Correcteddate).OrderByDescending(m => m.id).FirstOrDefault();/**/
        //                int ReasonID = Convert.ToInt32(BreakdownticketDet.reasonId);
        //                int OperatorID = Convert.ToInt32(BreakdownticketDet.operatorId);
        //                MacintanceAccpobj.Reason = GetReson(ReasonID);
        //                MacintanceAccpobj.DateTimeDis = Convert.ToString(BreakdownticketDet.bdTktDateTime);
        //                MacintanceAccpobj.Operatorname = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorId == OperatorID).Select(m => m.operatorName).FirstOrDefault();
        //                MacintanceAccpobj.MaintName = OperatorLoginDet.operatorName;
        //                MacintanceAccpobj.AcceptTime = Convert.ToString(BreakdownticketDet.mntAcp_RejDateTime);
        //                MacintanceAccpobj.finish = Convert.ToString(BreakdownticketDet.MaintFinished);
        //                MacintanceAccpobj.result = Result;
        //                Result = JsonConvert.SerializeObject(MacintanceAccpobj);
        //            }
        //            else if (BreakDownTickectDet.prodStatus == true)
        //            {

        //                UserName = Convert.ToString(Session["maintUser"]);
        //                Password = Convert.ToString(Session["maintpwd"]);
        //                OperatorLoginDet = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorUserName == UserName && m.operatorPwd == Password && m.roleId == 6).FirstOrDefault();
        //                if (OperatorLoginDet != null)
        //                {
        //                    Result = "ProdAccept";
        //                    ProuctioneAccp MacintanceAccpobj = new ProuctioneAccp();
        //                    MacintanceAccpobj.MachineName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();/*&& m.operatorId == Operatorid && m.correctedDate == dt*/
        //                    var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.correctedDate == Correcteddate).OrderByDescending(m => m.id).FirstOrDefault();/**/
        //                    int ReasonID = Convert.ToInt32(BreakdownticketDet.reasonId);
        //                    int OperatorID = Convert.ToInt32(BreakdownticketDet.operatorId);
        //                    MacintanceAccpobj.Reason = GetReson(ReasonID);
        //                    MacintanceAccpobj.DateTimeDis = Convert.ToString(BreakdownticketDet.bdTktDateTime);
        //                    MacintanceAccpobj.Operatorname = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorId == OperatorID).Select(m => m.operatorName).FirstOrDefault();
        //                    MacintanceAccpobj.MaintName = OperatorLoginDet.operatorName;
        //                    MacintanceAccpobj.AcceptTime = Convert.ToString(BreakdownticketDet.prodAcp_RejDateTime);
        //                    MacintanceAccpobj.finish = Convert.ToString(BreakdownticketDet.ProdFinished);
        //                    MacintanceAccpobj.result = Result;
        //                    Result = JsonConvert.SerializeObject(MacintanceAccpobj);
        //                }
        //                else
        //                {
        //                    Result = "ProdLogin";
        //                }
        //            }
        //            else
        //            {
        //                var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.correctedDate == Correcteddate).OrderByDescending(m => m.id).FirstOrDefault();/**/
        //                if (BreakdownticketDet.MaintFinished == 1)
        //                {
        //                    Result = "ProdLogin";
        //                }
        //                else
        //                {
        //                    Result = "Login";
        //                }
        //            }
        //        }
        //        else if (BreakDownTickectDet.prodStatus == true)
        //        {

        //            string UserName = Convert.ToString(Session["maintUser"]);
        //            string Password = Convert.ToString(Session["maintpwd"]);
        //            var OperatorLoginDet = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorUserName == UserName && m.operatorPwd == Password && m.roleId == 6).FirstOrDefault();
        //            if (OperatorLoginDet != null)
        //            {
        //                Result = "ProdAccept";
        //                ProuctioneAccp MacintanceAccpobj = new ProuctioneAccp();
        //                MacintanceAccpobj.MachineName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();/*&& m.operatorId == Operatorid && m.correctedDate == dt*/
        //                var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.correctedDate == Correcteddate).OrderByDescending(m => m.id).FirstOrDefault();/**/
        //                int ReasonID = Convert.ToInt32(BreakdownticketDet.reasonId);
        //                int OperatorID = Convert.ToInt32(BreakdownticketDet.operatorId);
        //                MacintanceAccpobj.Reason = GetReson(ReasonID);
        //                MacintanceAccpobj.DateTimeDis = Convert.ToString(BreakdownticketDet.bdTktDateTime);
        //                MacintanceAccpobj.Operatorname = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorId == OperatorID).Select(m => m.operatorName).FirstOrDefault();
        //                MacintanceAccpobj.MaintName = OperatorLoginDet.operatorName;
        //                MacintanceAccpobj.AcceptTime = Convert.ToString(BreakdownticketDet.prodAcp_RejDateTime);
        //                MacintanceAccpobj.finish = Convert.ToString(BreakdownticketDet.ProdFinished);
        //                MacintanceAccpobj.result = Result;
        //                Result = JsonConvert.SerializeObject(MacintanceAccpobj);
        //            }
        //            else
        //            {
        //                Result = "Login";
        //            }
        //        }
        //        else if (BreakDownTickectDet.reasonId == null)
        //        {
        //            var BreakdownDet = db.tblBreakdowncodes.Where(m => m.IsDeleted == 0 && m.BreakdownLevel == 1).Select(m => new { m.BreakdownID, m.BreakdownCode }).ToList();
        //            Result = JsonConvert.SerializeObject(BreakdownDet);
        //        }
        //    }

        //    else
        //    {
        //        var BreakdownDet = db.tblBreakdowncodes.Where(m => m.IsDeleted == 0 && m.BreakdownLevel == 1).Select(m => new { m.BreakdownID, m.BreakdownCode }).ToList();
        //        Result = JsonConvert.SerializeObject(BreakdownDet);

        //    }
        //    return Result;
        //}

        public string BreakDownDetLeve1(int Level1)
        {
            string Result = "";
            var BreakdownDet = db.tblBreakdowncodes.Where(m => m.IsDeleted == 0 && m.BreakdownLevel1ID == Level1).Select(m => new { m.BreakdownID, m.BreakdownCode }).ToList();
            Result = JsonConvert.SerializeObject(BreakdownDet);
            return Result;
        }

        public string BreakDownDetLeve2(int Level2)
        {
            string Result = "";
            var BreakdownDet = db.tblBreakdowncodes.Where(m => m.IsDeleted == 0 && m.BreakdownLevel2ID == Level2).Select(m => new { m.BreakdownID, m.BreakdownCode }).ToList();
            Result = JsonConvert.SerializeObject(BreakdownDet);
            return Result;
        }

        //public string BreakDownReasonStore(int BreakDownID, int id)
        //{
        //    int Machineid = id;
        //    string Result = "";
        //    DateTime CorrectedDate = DateTime.Now.Date;
        //    DateTime StartTime = DateTime.Now;
        //    string Operatorid = Convert.ToString(Session["OperatorID"]);
        //    var WoDet = db.tblworkorderentries.Where(m => m.IsStarted == 1 && m.MachineID == Machineid && m.IsFinished == 0 && m.OperatorID == Operatorid && m.CorrectedDate == CorrectedDate).OrderByDescending(m => m.HMIID).FirstOrDefault();
        //    if (WoDet != null)
        //    {
        //        tblBreakDownTickect obj = new tblBreakDownTickect();
        //        obj.machineId = Machineid;
        //        obj.reasonId = BreakDownID;
        //        obj.operatorId = Operatorid;
        //        obj.woId = WoDet.HMIID;
        //        obj.bdTktDateTime = StartTime;
        //        obj.isDeleted = 0;
        //        obj.createdBy = Operatorid;
        //        obj.createdOn = DateTime.Now;
        //        obj.correctedDate = CorrectedDate;
        //        db.tblBreakDownTickects.Add(obj);
        //        db.SaveChanges();
        //        ModeCheckBreakdown(Machineid);
        //        Result = "Success";
        //    }
        //    return Result;
        //}

        public string LoginCheckMaint(string UserName, string Password, int Machineid)
        {
            string Result = "Fail";
            DateTime dt = DateTime.Now.Date;

            string Operatorid = Convert.ToString(Session["OperatorID"]);
            var OperatorLoginDet = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorUserName == UserName && m.operatorPwd == Password && m.roleId == 9).FirstOrDefault();
            if (OperatorLoginDet != null)
            {
                Session["maintUser"] = UserName;
                Session["maintpwd"] = Password;
                MacintanceAccp MacintanceAccpobj = new MacintanceAccp();
                var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.correctedDate == dt).OrderByDescending(m => m.id).FirstOrDefault();/**/
                BreakdownticketDet.mntOpId = OperatorLoginDet.operatorLoginId;
                BreakdownticketDet.mntStatus = true;
                db.SaveChanges();
                MacintanceAccpobj.MachineName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();/*&& m.operatorId == Operatorid && m.correctedDate == dt*/
                int ReasonID = Convert.ToInt32(BreakdownticketDet.reasonId);
                int OperatorID = Convert.ToInt32(BreakdownticketDet.operatorId);
                MacintanceAccpobj.Reason = GetReson(ReasonID);
                MacintanceAccpobj.DateTimeDis = Convert.ToString(BreakdownticketDet.bdTktDateTime);
                MacintanceAccpobj.Operatorname = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorId == OperatorID).Select(m => m.operatorName).FirstOrDefault();
                MacintanceAccpobj.MaintName = OperatorLoginDet.operatorName;
                Result = JsonConvert.SerializeObject(MacintanceAccpobj);
            }
            return Result;
        }

        public string GetReson(int id)
        {
            string result = "";
            var Reasonleve = db.tblBreakdowncodes.Where(m => m.BreakdownID == id && m.IsDeleted == 0).FirstOrDefault();
            if (Reasonleve != null)
            {
                if (Reasonleve.BreakdownLevel == 3)
                {
                    result = result + Reasonleve.BreakdownCode;
                    var det = db.tblBreakdowncodes.Where(m => m.BreakdownLevel1ID == Reasonleve.BreakdownLevel2ID && m.IsDeleted == 0).FirstOrDefault();
                    result = result + det.BreakdownCode;
                    var det1 = db.tblBreakdowncodes.Where(m => m.BreakdownLevel1ID == det.BreakdownLevel1ID && m.IsDeleted == 0).FirstOrDefault();
                    result = result + det.BreakdownCode;
                }
                else if (Reasonleve.BreakdownLevel == 2)
                {
                    result = result + Reasonleve.BreakdownCode;
                    var det = db.tblBreakdowncodes.Where(m => m.BreakdownLevel1ID == Reasonleve.BreakdownLevel2ID && m.IsDeleted == 0).FirstOrDefault();
                    result = result + det.BreakdownCode;
                }
                else
                {
                    result = result + Reasonleve.BreakdownCode;
                }
            }
            return result;
        }

        public string UpdateMaint(int Machineid)
        {
            DateTime Correcteddate = DateTime.Now.Date;
            string Result = "Fail";
            var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.mntStatus == true && m.correctedDate == Correcteddate).OrderByDescending(m => m.id).FirstOrDefault();
            if (BreakdownticketDet != null)
            {
                MacintanceAccp MacintanceAccpobj = new MacintanceAccp();
                BreakdownticketDet.mntAcp_RejDateTime = DateTime.Now;
                db.SaveChanges();
                MacintanceAccpobj.MachineName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();/*&& m.operatorId == Operatorid && m.correctedDate == dt*/
                int ReasonID = Convert.ToInt32(BreakdownticketDet.reasonId);
                int OperatorID = Convert.ToInt32(BreakdownticketDet.operatorId);
                MacintanceAccpobj.AcceptTime = Convert.ToString(BreakdownticketDet.mntAcp_RejDateTime);
                MacintanceAccpobj.Reason = GetReson(ReasonID);
                MacintanceAccpobj.DateTimeDis = Convert.ToString(BreakdownticketDet.bdTktDateTime);
                MacintanceAccpobj.Operatorname = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorId == OperatorID).Select(m => m.operatorName).FirstOrDefault();
                Result = JsonConvert.SerializeObject(MacintanceAccpobj);

            }
            return Result;
        }

        public string UpdateMaintProd(int Machineid)
        {
            DateTime Correcteddate = DateTime.Now.Date;
            string Result = "Fail";
            var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.prodStatus == true && m.correctedDate == Correcteddate).OrderByDescending(m => m.id).FirstOrDefault();
            if (BreakdownticketDet != null)
            {
                MacintanceAccp MacintanceAccpobj = new MacintanceAccp();
                BreakdownticketDet.prodAcp_RejDateTime = DateTime.Now;
                db.SaveChanges();
                MacintanceAccpobj.MachineName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();/*&& m.operatorId == Operatorid && m.correctedDate == dt*/
                int ReasonID = Convert.ToInt32(BreakdownticketDet.reasonId);
                int OperatorID = Convert.ToInt32(BreakdownticketDet.operatorId);
                MacintanceAccpobj.AcceptTime = Convert.ToString(BreakdownticketDet.prodAcp_RejDateTime);
                MacintanceAccpobj.Reason = GetReson(ReasonID);
                MacintanceAccpobj.DateTimeDis = Convert.ToString(BreakdownticketDet.bdTktDateTime);
                MacintanceAccpobj.Operatorname = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorId == OperatorID).Select(m => m.operatorName).FirstOrDefault();
                Result = JsonConvert.SerializeObject(MacintanceAccpobj);

            }
            return Result;
        }

        public string UpdateRemarks(int id, string RemartsData)
        {
            DateTime Correcteddate = DateTime.Now.Date;
            int Machineid = id;
            string Result = "Fail";
            var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.mntStatus == true && m.correctedDate == Correcteddate).OrderByDescending(m => m.id).FirstOrDefault();
            if (BreakdownticketDet != null)
            {
                MacintanceAccp MacintanceAccpobj = new MacintanceAccp();
                BreakdownticketDet.mntRemarks = RemartsData;
                BreakdownticketDet.MaintFinished = 1;
                db.SaveChanges();
                //ModeCheckBreakdown(Machineid);
                Result = "Success";
            }
            return Result;
        }

        public string UpdateRemarksProd(int id, string RemartsData)
        {
            DateTime Correcteddate = DateTime.Now.Date;
            int Machineid = id;
            string Result = "Fail";
            var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.prodStatus == true && m.correctedDate == Correcteddate).OrderByDescending(m => m.id).FirstOrDefault();
            if (BreakdownticketDet != null)
            {
                MacintanceAccp MacintanceAccpobj = new MacintanceAccp();
                BreakdownticketDet.prodRemarks = RemartsData;
                BreakdownticketDet.prodAcp_RejDateTime = DateTime.Now;
                BreakdownticketDet.ProdFinished = 1;
                db.SaveChanges();
                ModeCheckBreakdown(id);
                Result = "Success";
            }
            return Result;
        }

        public string LoginCheckProd(string UserName, string Password, int Machineid)
        {
            DateTime Correcteddate = DateTime.Now.Date;
            string Result = "Fail";
            DateTime dt = DateTime.Now.Date;
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            var OperatorLoginDet = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorUserName == UserName && m.operatorPwd == Password && m.roleId == 6).FirstOrDefault();
            if (OperatorLoginDet != null)
            {
                Session["maintUser"] = UserName;
                Session["maintpwd"] = Password;
                ProuctioneAccp MacintanceAccpobj = new ProuctioneAccp();
                var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.correctedDate == Correcteddate).OrderByDescending(m => m.id).FirstOrDefault();/**/
                BreakdownticketDet.mntOpId = OperatorLoginDet.operatorLoginId;
                BreakdownticketDet.prodStatus = true;
                db.SaveChanges();
                MacintanceAccpobj.MachineName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();/*&& m.operatorId == Operatorid && m.correctedDate == dt*/
                int ReasonID = Convert.ToInt32(BreakdownticketDet.reasonId);
                int OperatorID = Convert.ToInt32(BreakdownticketDet.operatorId);
                MacintanceAccpobj.Reason = GetReson(ReasonID);
                MacintanceAccpobj.DateTimeDis = Convert.ToString(BreakdownticketDet.bdTktDateTime);
                MacintanceAccpobj.Operatorname = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorId == OperatorID).Select(m => m.operatorName).FirstOrDefault();
                MacintanceAccpobj.MaintName = OperatorLoginDet.operatorName;
                Result = JsonConvert.SerializeObject(MacintanceAccpobj);
            }
            return Result;
        }

        public string RejectReasonDet(int Mainineid)
        {
            string result = "";
            var RejectReason = db.tblrejectreasons.Where(m => m.isDeleted == 0 && m.Machineid == Mainineid).ToList();
            if (RejectReason != null)
            {
                result = JsonConvert.SerializeObject(RejectReason);
            }
            return result;
        }

        public string clrsessionuser()
        {
            Session["maintUser"] = "";
            Session["maintpwd"] = "";
            string res = "success";
            return res;
        }


        public JsonResult CheckIdle()
        {
            GetMode GM = new GetMode();
            int Data = 0;

            int OperatorLoginID = Convert.ToInt32(Session["OUserID"]);
            var OperatorMachineDet = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorLoginID).ToList();
            foreach (var item in OperatorMachineDet)
            {
                int MachineID = Convert.ToInt32(item.machineId);
                //var toolCounter = db.tbltoollifeoperators.Where(m => m.toollifecounter == m.StandardToolLife).Where(m => m.IsCompleted == false && m.IsReset == 0 && m.IsDeleted == 0).ToList();

                bool IdleStatus = GM.CheckIdleEntry(MachineID);
                if (IdleStatus)
                    Data = 1;
                //int toolcount = toolCounter.Count();
                //if (Data == 1 && toolcount == 0)
                //{
                //    Data = 1;
                //}
                //else if (Data == 1 && toolcount > 0)
                //{
                //    Data = 1;
                //}
                //else if (Data != 1 && toolcount > 0)
                //{
                //    Data = 2;
                //}
                //else
                //{
                //    Data = 0;
                //}
                if (Data == 1)
                {
                    return Json(Data, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(Data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult IDLEPopup(FormCollection form, int LossSelect = 0)
        {
            GetMode GM = new GetMode();
            int OperatorLoginID = Convert.ToInt32(Session["OUserID"]);
            var OperatorMachineDet = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorLoginID).ToList();
            foreach (var item in OperatorMachineDet)
            {
                int MachineID = Convert.ToInt32(item.machineId);
                var prvmode = db.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
                        .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
                if (prvmode != null)
                {
                    ViewBag.SetUpStarted = "1";
                    ViewBag.MachineMode = "Setting";
                }

                var prvmodeMaint = db.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
                        .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
                if (prvmodeMaint != null)
                {
                    ViewBag.MNTStarted = "1";
                    ViewBag.MachineMode = "MNT";
                }

                var tblLossCodes = db.tbllossescodes.Where(m => m.MessageType == "IDLE").ToList();
                ViewBag.lossCodes = tblLossCodes;

                if (LossSelect == 0)//first time ,show level 1
                {
                    int lossCodeID = tblLossCodes.Find(a => a.MessageType == "IDLE").LossCodeID;
                    ViewBag.lossCodeID = lossCodeID;
                    ViewBag.level = 1;
                }
                else if (tblLossCodes.Where(m => m.LossCodesLevel1ID == LossSelect).ToList().Count > 0)// show level 2
                {
                    int lossCodeID = LossSelect;
                    ViewBag.lossCodeID = lossCodeID;
                    ViewBag.level = 2;
                }
                else //show level 3
                {
                    int lossCodeID = LossSelect;
                    ViewBag.lossCodeID = lossCodeID;
                    ViewBag.level = 3;
                }

                #region Lock the Machine
                var MacDet = db.tblmachinedetails.Find(MachineID);

                if (MacDet.MachineModelType == 1)
                {
                    AddFanucMachineWithConn AC = new AddFanucMachineWithConn(MacDet.IPAddress);
                    //AC.setmachinelock(true, (ushort)MacDet.MachineLockBit, (ushort)MacDet.MachineIdleBit, (ushort)MacDet.MachineUnlockBit);
                }

                #endregion
            }
            return View();
        }

        public ActionResult SaveIdle(int LossSelect = 0, int machineid = 0, bool flage = false, int count = 0)
        {
            //request came from level 2 and was a last node .Level 3  code will come as parameter.

            #region Update TblMode

            GetMode GM = new GetMode();
            String IPAddress = GM.GetIPAddressofTabSystem();
            if (flage == false)
            {
                var machinedet = db.tblmachinedetails.Where(m => m.MachineID == machineid && m.IsDeleted == 0).ToList();
                foreach (var item in machinedet)
                {
                    int machineID = item.MachineID;

                    Session["MachineID"] = machineID;
                    DateTime correctedDate = DateTime.Now;
                    SRKSDemo.Server_Model.tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
                    TimeSpan Start = StartTime.StartTime;
                    if (Start.Hours <= DateTime.Now.Hour)
                    {
                        correctedDate = DateTime.Now;
                    }
                    else
                    {
                        correctedDate = DateTime.Now.AddDays(-1);
                    }
                    int durationinsec = 0;
                    //var correctedDate = "2017-11-17";   // Hard coding for time being
                    string colorCode = "YELLOW";
                    //Update TblMode with the Loss Code
                    var mode = db.tbllivemodes.Where(m => m.MachineID == machineID && m.ColorCode == colorCode && m.IsCompleted == 0 && m.StartIdle == 1)
                                .OrderByDescending(m => m.ModeID).FirstOrDefault();
                    DateTime ModeStartTime = DateTime.Now;
                    if (mode != null)
                    {
                        if (item.LossFlag == 1)
                        {
                            ModeStartTime = (DateTime)mode.StartTime;
                            durationinsec = Convert.ToInt32(DateTime.Now.Subtract(ModeStartTime).TotalSeconds);
                            mode.LossCodeID = null;
                            mode.ModeType = "IDLE";
                            mode.LossCodeEnteredTime = DateTime.Now;
                            mode.LossCodeEnteredBy = "";
                            mode.ModeTypeEnd = 1;
                            mode.IsCompleted = 1;
                            mode.StartIdle = 0;
                            mode.EndTime = DateTime.Now;
                            mode.DurationInSec = durationinsec;
                            mode.ModifiedOn = DateTime.Now; // doing now for testing purpose
                            mode.ModifiedBy = Convert.ToInt32(Session["UserID"]);
                            db.Entry(mode).State = EntityState.Modified;
                            db.SaveChanges();

                            DateTime StartNow = DateTime.Now;
                            SRKSDemo.Server_Model.tblmode tm = new SRKSDemo.Server_Model.tblmode();
                            tm.MachineID = mode.MachineID;
                            tm.MacMode = mode.MacMode;
                            tm.InsertedBy = Convert.ToInt32(Session["UserID"]);
                            tm.InsertedOn = StartNow;
                            tm.CorrectedDate = correctedDate;
                            tm.IsDeleted = 0;
                            tm.StartTime = StartNow;
                            tm.ColorCode = "YELLOW";
                            tm.IsCompleted = 0;
                            tm.LossCodeID = LossSelect;
                            tm.ModeType = "IDLE";
                            tm.ModeTypeEnd = 0;
                            tm.StartIdle = 0;
                            tm.LossCodeEnteredTime = DateTime.Now;
                            tm.LossCodeEnteredBy = Convert.ToString(Session["UserID"]);
                            tm.IsInserted = 1;
                            db.tblmodes.Add(tm);
                            db.SaveChanges();
                        }
                        else
                        {

                        }
                        ModeStartTime = (DateTime)mode.StartTime;
                        durationinsec = Convert.ToInt32(DateTime.Now.Subtract(ModeStartTime).TotalSeconds);
                        mode.LossCodeID = LossSelect;
                        mode.ModeType = "IDLE";
                        mode.LossCodeEnteredTime = DateTime.Now;
                        mode.LossCodeEnteredBy = "";
                        mode.ModeTypeEnd = 1;
                        mode.IsCompleted = 1;
                        mode.StartIdle = 0;
                        mode.EndTime = DateTime.Now;
                        mode.DurationInSec = durationinsec;
                        mode.ModifiedOn = DateTime.Now; // doing now for testing purpose
                        mode.ModifiedBy = Convert.ToInt32(Session["UserID"]);
                        db.Entry(mode).State = EntityState.Modified;
                        db.SaveChanges();

                    }
                    else
                    {

                    }
                    #endregion

                    //#region UnLock the Machine
                    //var MacDet = _UWcontext.tblmachinedetails.Find(machineID);
                    //if (MacDet.MachineModelType == 1)
                    //{
                    //    AddFanucMachineWithConn AC = new AddFanucMachineWithConn(MacDet.IPAddress);
                    //    AC.SetMachineUnlock((ushort)MacDet.MachineUnlockBit, (ushort)MacDet.MachineLockBit);
                    //}
                    //#endregion   
                }

                if (count > 1)
                {
                    return RedirectToAction("IDLEPopup");
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                int OperatorloginID = Convert.ToInt32(Session["OUserID"]);
                var OperatorMachineDetails = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorloginID).ToList();
                foreach (var item in OperatorMachineDetails)
                {
                    int machineID = Convert.ToInt32(item.machineId);
                    var MachDet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == machineid).FirstOrDefault();
                    Session["MachineID"] = machineID;
                    DateTime correctedDate = DateTime.Now;
                    SRKSDemo.Server_Model.tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
                    TimeSpan Start = StartTime.StartTime;
                    if (Start.Hours <= DateTime.Now.Hour)
                    {
                        correctedDate = DateTime.Now;
                    }
                    else
                    {
                        correctedDate = DateTime.Now.AddDays(-1);
                    }
                    int durationinsec = 0;
                    //var correctedDate = "2017-11-17";   // Hard coding for time being
                    string colorCode = "YELLOW";
                    //Update TblMode with the Loss Code
                    var mode = db.tbllivemodes.Where(m => m.MachineID == machineID && m.ColorCode == colorCode && m.IsCompleted == 0 && m.StartIdle == 1)
                                .OrderByDescending(m => m.ModeID).FirstOrDefault();
                    DateTime ModeStartTime = DateTime.Now;
                    if (mode != null)
                    {
                        if (MachDet.LossFlag == 1)
                        {
                            ModeStartTime = (DateTime)mode.StartTime;
                            durationinsec = Convert.ToInt32(DateTime.Now.Subtract(ModeStartTime).TotalSeconds);
                            mode.LossCodeID = LossSelect;
                            mode.ModeType = "IDLE";
                            mode.LossCodeEnteredTime = DateTime.Now;
                            mode.LossCodeEnteredBy = "";
                            mode.ModeTypeEnd = 1;
                            mode.IsCompleted = 1;
                            mode.StartIdle = 0;
                            mode.EndTime = DateTime.Now;
                            mode.DurationInSec = durationinsec;
                            mode.ModifiedOn = DateTime.Now; // doing now for testing purpose
                            mode.ModifiedBy = Convert.ToInt32(Session["UserID"]);
                            db.Entry(mode).State = EntityState.Modified;
                            db.SaveChanges();

                            DateTime StartNow = DateTime.Now;
                            SRKSDemo.Server_Model.tbllivemode tm = new SRKSDemo.Server_Model.tbllivemode();
                            tm.MachineID = mode.MachineID;
                            tm.MacMode = mode.MacMode;
                            tm.InsertedBy = Convert.ToInt32(Session["UserID"]);
                            tm.InsertedOn = StartNow;
                            tm.CorrectedDate = correctedDate;
                            tm.IsDeleted = 0;
                            tm.StartTime = StartNow;
                            tm.ColorCode = "YELLOW";
                            tm.IsCompleted = 0;
                            tm.LossCodeID = LossSelect;
                            tm.ModeType = "IDLE";
                            tm.ModeTypeEnd = 0;
                            tm.StartIdle = 0;
                            tm.LossCodeEnteredTime = DateTime.Now;
                            tm.LossCodeEnteredBy = Convert.ToString(Session["UserID"]);
                            tm.IsInserted = 1;
                            db.tbllivemodes.Add(tm);
                            db.SaveChanges();
                        }
                        else
                        {
                            ModeStartTime = (DateTime)mode.StartTime;
                            durationinsec = Convert.ToInt32(DateTime.Now.Subtract(ModeStartTime).TotalSeconds);
                            mode.LossCodeID = LossSelect;
                            mode.ModeType = "IDLE";
                            mode.LossCodeEnteredTime = DateTime.Now;
                            mode.LossCodeEnteredBy = "";
                            mode.ModeTypeEnd = 1;
                            mode.IsCompleted = 1;
                            mode.StartIdle = 0;
                            mode.EndTime = DateTime.Now;
                            mode.DurationInSec = durationinsec;
                            mode.ModifiedOn = DateTime.Now; // doing now for testing purpose
                            mode.ModifiedBy = Convert.ToInt32(Session["UserID"]);
                            db.Entry(mode).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                    }
                    else
                    {

                    }

                }

            }

            //return RedirectToAction("DashboardProduction");
            return RedirectToAction("Index");
        }
        [HttpPost]
        public string ServerPing()
        {
            string Status = "Connected";
            GetMode GM = new GetMode();
            Ping ping = new Ping();
            String TabIPAddress = GM.GetIPAddressofTabSystem();
            var MachineDetails = db.tblmachinedetails.Where(m => m.TabIPAddress == TabIPAddress && m.IsDeleted == 0).FirstOrDefault();

            //try
            //{
            //    PingReply pingresult = ping.Send(MachineDetails.ServerIPAddress);
            //    if (pingresult.Status.ToString() == "Success")
            //    {
            //        Status = "Connected";
            //    }
            //}
            //catch
            //{
            //    Status = "Disconnected";
            //}
            return Status;
        }

        public JsonResult GetMachinePopup()
        {
            DateTime correcteddate = DateTime.Now.Date;
            List<IdlePopupMachine> IdelListObj = new List<IdlePopupMachine>();
            GetMode GM = new GetMode();
            int OperatorLoginID = Convert.ToInt32(Session["OUserID"]);
            var OperatorMachineDet = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorLoginID).ToList();
            foreach (var item in OperatorMachineDet)
            {
                int MachineID = Convert.ToInt32(item.machineId);
                bool IdleStatus = GM.CheckIdleEntry(MachineID);
                if (IdleStatus)
                {
                    List<LossCode> LoosObj = new List<LossCode>();
                    var tblLossCodes = db.tbllossescodes.Where(m => m.MessageType == "IDLE" && m.IsDeleted == 0).ToList();
                    foreach (var data in tblLossCodes)
                    {
                        LossCode Lobj = new LossCode();
                        Lobj.losscodeid = data.LossCodeID;
                        Lobj.losscode = data.LossCode;
                        Lobj.losslevel = Convert.ToInt32(data.LossCodesLevel);
                        LoosObj.Add(Lobj);
                    }
                    var IdleEntry = db.tblmodes.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.IsCompleted == 0 && m.StartIdle == 1 && m.ColorCode == "YELLOW").OrderByDescending(m => m.ModeID).FirstOrDefault();
                    //var optdet = db.tbloperatordashboards.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.CorrectedDate == correcteddate).ToList();
                    //int machinecount = optdet.Count();
                    //cellIdlecount = cellIdlecount + machinecount;
                    IdlePopupMachine obj = new IdlePopupMachine();
                    obj.MachineID = Convert.ToInt32(item.machineId);
                    obj.machinename = db.tblmachinedetails.Where(m => m.MachineID == item.machineId).Select(m => m.MachineDisplayName).FirstOrDefault();
                    obj.starttimeidle = Convert.ToString(IdleEntry.StartTime);
                    obj.LLoss = LoosObj;
                    IdelListObj.Add(obj);
                }
            }
            IdelListObj.OrderBy(m => m.starttimeidle);
            return Json(IdelListObj, JsonRequestBehavior.AllowGet);
        }

        public ContentResult lastNodeIdleCheck(int id, int lev)
        {
            var tblLossCodes = db.tbllossescodes.ToList();

            if (lev == 1)
            {
                if (tblLossCodes.Find(level => level.LossCodesLevel == 2 && level.LossCodesLevel1ID == id && level.IsDeleted == 0) == null) { return Content("true/" + id); }
                else
                {
                    return Content("false/" + id);
                }
            }
            else
            {
                if (tblLossCodes.Find(level => level.LossCodesLevel == 3 && level.LossCodesLevel2ID == id && level.IsDeleted == 0) == null) { return Content("true/" + id); }

                return Content("false/" + id);
            }
        }



        public string ReworkReasonStartStore(int Machid, int ReworkID)
        {
            string Result = "";
            DateTime Correcteddate = DateTime.Now;
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            var Wodet = db.tblworkorderentries.Where(m => m.MachineID == Machid && m.OperatorID == Operatorid && m.IsStarted == 1 && m.IsFinished == 0 && m.CorrectedDate == Correcteddate.Date).FirstOrDefault();
            if (Wodet != null)
            {
                Wodet.IsReWork = 1;
                Wodet.ReWorkStart = 1;
                Wodet.ReWorkReasonID = ReworkID;
                Wodet.reworkStartTime = DateTime.Now;
                db.SaveChanges();
                Result = "true";
            }
            return Result; ;
        }

        public string ReworkReasonEndStore(int Machid)
        {
            string Result = "";
            DateTime Correcteddate = DateTime.Now;
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            var Wodet = db.tblworkorderentries.Where(m => m.MachineID == Machid && m.OperatorID == Operatorid && m.IsStarted == 1 && m.IsFinished == 0 && m.IsReWork == 1 && m.ReWorkStart == 1 && m.CorrectedDate == Correcteddate.Date).FirstOrDefault();
            if (Wodet != null)
            {
                Wodet.ReWorkEnd = 1;
                Wodet.reworkEndTime = DateTime.Now;
                db.SaveChanges();
                Result = "true";
            }
            return Result; ;
        }

        public string ReWorkCheck(int MachineID)
        {
            string Result = "False";
            DateTime Correcteddate = DateTime.Now;
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            List<string> Slist = new List<string>();
            var Wodet = db.tblworkorderentries.Where(m => m.MachineID == MachineID && m.OperatorID == Operatorid && m.IsReWork == 1 && m.ReWorkEnd == 0 && m.ReWorkStart == 1 && m.IsStarted == 1 && m.IsFinished == 0 && m.CorrectedDate == Correcteddate.Date).FirstOrDefault();
            if (Wodet != null)
            {
                int ReworkReason = Convert.ToInt32(Wodet.ReWorkReasonID);
                var ReworkDet = db.tblReworkReasons.Where(m => m.IsDeleted == 0 && m.ReWorkID == ReworkReason).Select(m => m.ReworkName).FirstOrDefault();
                Slist.Add(ReworkDet);
                Result = JsonConvert.SerializeObject(Slist);
                ViewBag.Result = "True";
            }
            else
            {
                var ReworkDet = db.tblReworkReasons.Where(m => m.IsDeleted == 0).ToList();
                Result = JsonConvert.SerializeObject(ReworkDet);
                ViewBag.Result = "False";
            }
            return Result;
        }

        public string ReworkBreakdownStartTime()
        {
            string Result = "";
            DateTime Correcteddate = DateTime.Now;
            int OperatorloginID = Convert.ToInt32(Session["OUserID"]);
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            List<ReworkBreakDown> ListObj = new List<ReworkBreakDown>();
            var OperatormachiDet = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorloginID).ToList();
            foreach (var row in OperatormachiDet)
            {
                //int machid = id;
                int machid = Convert.ToInt32(row.machineId);
                ReworkBreakDown Obj = new ReworkBreakDown();
                Obj.MachineID = machid;
                var Wodet = db.tblworkorderentries.Where(m => m.MachineID == machid && m.OperatorID == Operatorid && m.IsReWork == 1 && m.ReWorkEnd == 0 && m.ReWorkStart == 1 && m.IsStarted == 1 && m.IsFinished == 0 && m.CorrectedDate == Correcteddate.Date).OrderByDescending(m=>m.HMIID).FirstOrDefault();
                if (Wodet != null)
                {
                    Obj.ReworkStartTime = Convert.ToString(Wodet.reworkStartTime);
                    Obj.ReworkStart = 1;
                }
                else
                {
                    Wodet = db.tblworkorderentries.Where(m => m.MachineID == machid && m.OperatorID == Operatorid && m.IsReWork == 1 && m.ReWorkEnd == 1 && m.ReWorkStart == 1 && m.IsStarted == 1 && m.IsFinished == 0 && m.CorrectedDate == Correcteddate.Date).OrderByDescending(m => m.HMIID).FirstOrDefault();
                    if (Wodet != null)
                    {
                        Obj.ReworkStart = 2;
                    }
                    else
                    {
                        Obj.ReworkStart = 3;
                    }
                }
                var BreakDownTicketdet = db.tblBreakDownTickects.Where(m => m.machineId == machid && m.operatorId == Operatorid && m.reasonId != null && m.MaintFinished == 1 && m.mntStatus == true && m.correctedDate == Correcteddate.Date).OrderByDescending(m => m.id).FirstOrDefault();
                if (BreakDownTicketdet != null)
                {
                    Obj.BreakDownStart = 2;
                }
                else
                {
                    BreakDownTicketdet = db.tblBreakDownTickects.Where(m => m.machineId == machid && m.operatorId == Operatorid && m.reasonId != null && m.mntStatus == null || m.mntStatus == true && m.correctedDate == Correcteddate.Date).OrderByDescending(m => m.id).FirstOrDefault();
                    if (BreakDownTicketdet != null)
                    {
                        Obj.BreakDownStart = 1;
                        Obj.BreakDownStartTime = Convert.ToString(BreakDownTicketdet.bdTktDateTime);
                    }
                    else
                    {
                        Obj.BreakDownStart = 3;
                    }
                }

                ListObj.Add(Obj);
            }

            Result = JsonConvert.SerializeObject(ListObj);
            return Result;
        }


        public string RejectMaintance(int Machineid, int rejectid)
        {
            string res = "";
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.operatorId == Operatorid && m.mntStatus == true).OrderByDescending(m => m.id).FirstOrDefault();/**/
            if (BreakdownticketDet != null)
            {
                BreakdownticketDet.MaintFinished = 1;
                BreakdownticketDet.ProdFinished = 1;
                BreakdownticketDet.prodStatus = true;
                BreakdownticketDet.mntRrejectReason = Convert.ToString(rejectid);
                BreakdownticketDet.mntAcp_RejDateTime = DateTime.Now;
                db.SaveChanges();
                res = "true";
            }
            return res;
        }

        public string MaintReject()
        {
            string Result = "";
            var MRejectDet = db.tblrejectreasons.Where(m => m.isDeleted == 0).Select(m => new { m.RID, m.RejectName }).ToList();
            if (MRejectDet != null)
            {
                Result = JsonConvert.SerializeObject(MRejectDet);
            }
            return Result;
        }

        public string ProdReject()
        {
            string Result = "";
            var MRejectDet = db.tblrejectreasons.Where(m => m.isDeleted == 0).Select(m => new { m.RID, m.RejectName }).ToList();
            if (MRejectDet != null)
            {
                Result = JsonConvert.SerializeObject(MRejectDet);
            }
            return Result;
        }

        public string RejectProduction(int machineid, int Rid)
        {
            string res = "";
            DateTime CorrectedDate = DateTime.Now.Date;
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == machineid && m.operatorId == Operatorid).OrderByDescending(m => m.id).FirstOrDefault();/**/
            if (BreakdownticketDet != null)
            {                
                BreakdownticketDet.prodRejId = Rid;
                BreakdownticketDet.ProdFinished = 1;
                BreakdownticketDet.mntAcp_RejDateTime = DateTime.Now;
                db.SaveChanges();
                tblBreakDownTickect obj = new tblBreakDownTickect();
                obj.machineId = BreakdownticketDet.machineId;
                obj.reasonId = BreakdownticketDet.reasonId;
                obj.operatorId = BreakdownticketDet.operatorId;
                obj.woId = BreakdownticketDet.woId;
                obj.bdTktDateTime = BreakdownticketDet.bdTktDateTime;
                obj.isDeleted = 0;
                obj.createdBy = Operatorid;
                obj.createdOn = DateTime.Now;
                obj.correctedDate = CorrectedDate;
                db.tblBreakDownTickects.Add(obj);
                db.SaveChanges();
                res = "true";
            }
            return res;

        }

        #region Machine Status

        public string MachineStatusMode()
        {
            List<MachineStatusData> MachineListObj = new List<MachineStatusData>();
            string Result = "";
            DateTime Correcteddate = DateTime.Now.Date;
            Correcteddate = Convert.ToDateTime("2019-06-20").Date;
            int ShiftID = ShiftDet();
            int OperatorloginID = Convert.ToInt32(Session["OUserID"]);
            var OperatormachiDet = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorloginID).ToList();
            foreach (var row in OperatormachiDet)
            {
                MachineStatusData MachineObj = new MachineStatusData();
                int MachineID = Convert.ToInt32(row.machineId);
                int IdleTime = 0, ProductionTime = 0, BreakDownTime = 0, PowerOffTime = 0;
                int FinalIdleTime = 0, FinalProductionTime = 0, FinalBreakDownTime = 0, FinalPowerOffTime = 0,FinalTotalDuration=0;
                var ModeDetYellow = db.tbllivemodes.Where(m => m.IsDeleted == 0 && m.IsCompleted == 1 && m.IsShiftEnd == ShiftID && m.MachineID == MachineID && m.ColorCode=="YELLOW" && m.MacMode=="IDLE" && m.CorrectedDate== Correcteddate).Select(m => m.DurationInSec).Sum();
                if(ModeDetYellow != null)
                {
                    IdleTime += Convert.ToInt32(ModeDetYellow);
                }
                var ModeDetGreen = db.tbllivemodes.Where(m => m.IsDeleted == 0 && m.IsCompleted == 1 && m.IsShiftEnd == ShiftID && m.MachineID == MachineID && m.ColorCode == "GREEN" && m.MacMode == "PROD" && m.CorrectedDate == Correcteddate).Select(m => m.DurationInSec).Sum();
                if (ModeDetGreen != null)
                {
                    ProductionTime += Convert.ToInt32(ModeDetGreen);
                }
                var ModeDetBlue = db.tbllivemodes.Where(m => m.IsDeleted == 0 && m.IsCompleted == 1 && m.IsShiftEnd == ShiftID && m.MachineID == MachineID && m.ColorCode == "BLUE" && m.MacMode == "POWEROFF" && m.CorrectedDate == Correcteddate).Select(m => m.DurationInSec).Sum();
                if (ModeDetBlue != null)
                {
                    PowerOffTime += Convert.ToInt32(ModeDetBlue);
                }
                var ModeDetRed = db.tbllivemodes.Where(m => m.IsDeleted == 0 && m.IsCompleted == 1 && m.IsShiftEnd == ShiftID && m.MachineID == MachineID && m.ColorCode == "RED" && m.MacMode == "MNT" && m.CorrectedDate == Correcteddate).Select(m => m.DurationInSec).Sum();
                if (ModeDetRed != null)
                {
                    BreakDownTime += Convert.ToInt32(ModeDetRed);
                }

                var ModeDetRun = db.tbllivemodes.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.IsCompleted == 0 && m.IsShiftEnd == ShiftID && m.CorrectedDate == Correcteddate).FirstOrDefault();
                if(ModeDetRun!=null)
                {
                    DateTime Stime = Convert.ToDateTime(ModeDetRun.StartTime);
                    DateTime Etime = DateTime.Now;
                    double Duration = Stime.Subtract(Etime).TotalSeconds;
                    string Mode = ModeDetRun.MacMode;
                    if(Mode=="PROD")
                    {
                        ProductionTime += Convert.ToInt32(Duration);
                    }
                    else if(Mode == "IDLE")
                    {
                        IdleTime += Convert.ToInt32(Duration);
                    }
                    else if (Mode == "POWEROFF")
                    {
                        PowerOffTime += Convert.ToInt32(Duration); 
                    }
                    else if (Mode == "MNT")
                    {
                        BreakDownTime += Convert.ToInt32(Duration); 
                    }
                }
                
                FinalIdleTime = (IdleTime / 60);
                FinalBreakDownTime = (BreakDownTime / 60);
                FinalProductionTime = (ProductionTime / 60);
                FinalPowerOffTime=(PowerOffTime/60);
                FinalTotalDuration = FinalIdleTime + FinalBreakDownTime + FinalProductionTime + FinalPowerOffTime;

                MachineObj.MachineID = MachineID;
                MachineObj.IdleDuration = FinalIdleTime;
                MachineObj.BreakdownDuration = FinalBreakDownTime;
                MachineObj.PoweroffDuration = FinalPowerOffTime;
                MachineObj.ProductionDuration = FinalProductionTime;
                MachineObj.TotalDuration = FinalTotalDuration; //03024

                MachineListObj.Add(MachineObj);

            }

            Result = JsonConvert.SerializeObject(MachineListObj);
            return Result;
        }

        #endregion




        #region new breakdwon flow

        public string BreakdDownReasons(int Machineid)
        {
            string Result = "";
            List<string> ListObj = new List<string>();
            DateTime Correctedate = DateTime.Now.Date;
            var BreakdwonTicket = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.correctedDate == Correctedate && m.AcceptFlage == null).OrderByDescending(m => m.id).FirstOrDefault();
            if (BreakdwonTicket != null)
            {
                if (BreakdwonTicket.MaintFlage == null)
                {
                    Result = "MaintLogin";
                    ListObj.Add(Result);
                    Result = JsonConvert.SerializeObject(ListObj);
                }
                else if (BreakdwonTicket.MaintFlage != null && BreakdwonTicket.AcceptFlage == null && BreakdwonTicket.MaintFinished==null)
                {
                    Result = "MaintClosureLogin";
                    ListObj.Add(Result);
                    Result = JsonConvert.SerializeObject(ListObj);
                }
                else if (BreakdwonTicket.MaintFlage != null && BreakdwonTicket.MaintFinished == 1 && BreakdwonTicket.ProdFinished==null)
                {
                    Result = "ProdLogin";
                    ListObj.Add(Result);
                    Result = JsonConvert.SerializeObject(ListObj);
                }
                else
                {
                    var BreakdownDet = db.tblBreakdowncodes.Where(m => m.IsDeleted == 0 && m.BreakdownLevel == 1).Select(m => new { m.BreakdownID, m.BreakdownCode }).ToList();
                    Result = JsonConvert.SerializeObject(BreakdownDet);
                }
            }
            else
            {
                var BreakdownDet = db.tblBreakdowncodes.Where(m => m.IsDeleted == 0 && m.BreakdownLevel == 1).Select(m => new { m.BreakdownID, m.BreakdownCode }).ToList();
                Result = JsonConvert.SerializeObject(BreakdownDet);
            }
            return Result;
        }

        public string DocumnetLoadReasons()
        {
            string Result = "";
            int OperatorloginID = Convert.ToInt32(Session["OUserID"]);
            List<MachRea> ListObj = new List<MachRea>();
            DateTime Correctedate = DateTime.Now.Date;
            var OperatorMachineDetails = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorloginID).ToList();
            foreach (var MachRow in OperatorMachineDetails)
            {
                MachRea obj = new MachRea();
                int Machineid = Convert.ToInt32(MachRow.machineId);
                var BreakdwonTicket = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.correctedDate == Correctedate && m.AcceptFlage == null).OrderByDescending(m => m.id).FirstOrDefault();
                if (BreakdwonTicket != null)
                {
                    if (BreakdwonTicket.MaintFlage == null)
                    {
                        Result = "MaintLogin";
                        obj.Content=Result;
                        obj.Machineid = Machineid;
                        ListObj.Add(obj);
                        Result = JsonConvert.SerializeObject(ListObj);
                    }
                    else if (BreakdwonTicket.MaintFlage != null && BreakdwonTicket.AcceptFlage == null && BreakdwonTicket.MaintFinished == null)
                    {
                        Result = "MaintClosureLogin";
                        obj.Content = Result;
                        obj.Machineid = Machineid;
                        ListObj.Add(obj);
                        Result = JsonConvert.SerializeObject(ListObj);
                    }
                    else if (BreakdwonTicket.MaintFlage != null && BreakdwonTicket.MaintFinished == 1 && BreakdwonTicket.ProdFinished == null)
                    {
                        Result = "ProdLogin";
                        obj.Content = Result;
                        obj.Machineid = Machineid;
                        ListObj.Add(obj);
                        Result = JsonConvert.SerializeObject(ListObj);
                    }
                    else
                    {
                        Result = "BreakReason";
                        obj.Content = Result;
                        obj.Machineid = Machineid;
                        ListObj.Add(obj);
                        Result = JsonConvert.SerializeObject(ListObj);
                    }
                }
                else
                {
                    Result = "BreakReason";
                    obj.Content = Result;
                    obj.Machineid = Machineid;
                    ListObj.Add(obj);
                    Result = JsonConvert.SerializeObject(ListObj);
                }
            }
            return Result;
        }

        public string BreakDownReasonStore1(int BreakDownID, int Mid)
        {
            int Machineid = Mid;
            string Result = "";
            DateTime CorrectedDate = DateTime.Now.Date;
            DateTime StartTime = DateTime.Now;
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            var WoDet = db.tblworkorderentries.Where(m => m.IsStarted == 1 && m.MachineID == Machineid && m.IsFinished == 0 && m.OperatorID == Operatorid && m.CorrectedDate == CorrectedDate).OrderByDescending(m => m.HMIID).FirstOrDefault();
            if (WoDet != null)
            {
                var BreadkDownTicketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.correctedDate == CorrectedDate && m.machineId == Machineid && m.maintRejId != 1).OrderByDescending(m => m.id).FirstOrDefault();
                tblBreakDownTickect obj = new tblBreakDownTickect();
                obj.machineId = Machineid;
                obj.reasonId = BreakDownID;
                obj.operatorId = Operatorid;
                obj.woId = WoDet.HMIID;
                obj.bdTktDateTime = StartTime;
                obj.isDeleted = 0;
                obj.createdBy = Operatorid;
                obj.createdOn = DateTime.Now;
                obj.correctedDate = CorrectedDate;
                db.tblBreakDownTickects.Add(obj);
                db.SaveChanges();
                ModeCheckBreakdown(Machineid);
                Result = "Success";
            }
            return Result;
        }

        public string LoginCheckMaint1(string UserName, string Password, int Machineid)
        {
            string Result = "Fail";
            DateTime dt = DateTime.Now.Date;

            string Operatorid = Convert.ToString(Session["OperatorID"]);
            var OperatorLoginDet = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorUserName == UserName && m.operatorPwd == Password && m.roleId == 9).FirstOrDefault();
            if (OperatorLoginDet != null)
            {
                Session["maintUser"] = UserName;
                Session["maintpwd"] = Password;
                MacintanceAccp MacintanceAccpobj = new MacintanceAccp();
                var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.correctedDate == dt && m.MaintFlage == null).OrderByDescending(m => m.id).FirstOrDefault();/**/
                BreakdownticketDet.mntOpId = OperatorLoginDet.operatorLoginId;
                BreakdownticketDet.MaintFlage = 1;
                db.SaveChanges();
                MacintanceAccpobj.MachineName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();/*&& m.operatorId == Operatorid && m.correctedDate == dt*/
                int ReasonID = Convert.ToInt32(BreakdownticketDet.reasonId);
                int OperatorID = Convert.ToInt32(BreakdownticketDet.operatorId);
                MacintanceAccpobj.Reason = GetReson(ReasonID);
                MacintanceAccpobj.DateTimeDis = Convert.ToString(BreakdownticketDet.bdTktDateTime);
                MacintanceAccpobj.Operatorname = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorId == OperatorID).Select(m => m.operatorName).FirstOrDefault();
                MacintanceAccpobj.MaintName = OperatorLoginDet.operatorName;
                Result = JsonConvert.SerializeObject(MacintanceAccpobj);
            }
            return Result;
        }

        public string UpdateMaint1(int Machineid)
        {
            DateTime Correcteddate = DateTime.Now.Date;
            string Result = "Fail";
            var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.mntOpId != null && m.MaintFlage == 1 && m.correctedDate == Correcteddate).OrderByDescending(m => m.id).FirstOrDefault();
            if (BreakdownticketDet != null)
            {
                //MacintanceAccp MacintanceAccpobj = new MacintanceAccp();
                BreakdownticketDet.mntAcp_RejDateTime = DateTime.Now;
                BreakdownticketDet.mntStatus = true;
                db.SaveChanges();
                //MacintanceAccpobj.MachineName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();/*&& m.operatorId == Operatorid && m.correctedDate == dt*/
                //int ReasonID = Convert.ToInt32(BreakdownticketDet.reasonId);
                //int OperatorID = Convert.ToInt32(BreakdownticketDet.operatorId);
                //MacintanceAccpobj.AcceptTime = Convert.ToString(BreakdownticketDet.mntAcp_RejDateTime);
                //MacintanceAccpobj.Reason = GetReson(ReasonID);
                //MacintanceAccpobj.DateTimeDis = Convert.ToString(BreakdownticketDet.bdTktDateTime);
                //MacintanceAccpobj.Operatorname = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorId == OperatorID).Select(m => m.operatorName).FirstOrDefault();
                Result = "Success";
                //Result = JsonConvert.SerializeObject(MacintanceAccpobj);

            }
            return Result;
        }

        public string RejectMaintance1(int Machineid, int rejectid)
        {
            string res = "";
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.operatorId == Operatorid && m.mntStatus == null).OrderByDescending(m => m.id).FirstOrDefault();/**/
            if (BreakdownticketDet != null)
            {
                BreakdownticketDet.mntRrejectReason = Convert.ToString(rejectid);
                BreakdownticketDet.mntAcp_RejDateTime = DateTime.Now;
                BreakdownticketDet.AcceptFlage = 1;
                db.SaveChanges();
                res = "true";
            }
            return res;
        }


        public string LoginCheckMaint2(string UserName, string Password, int Machineid)
        {
            string Result = "Fail";
            DateTime Corrected = DateTime.Now.Date;
            var OperatorLoginDet = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorUserName == UserName && m.operatorPwd == Password && m.roleId == 9).FirstOrDefault();
            if (OperatorLoginDet != null)
            {
                ProuctioneAccp MacintanceAccpobj = new ProuctioneAccp();
                MacintanceAccpobj.MachineName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();/*&& m.operatorId == Operatorid && m.correctedDate == dt*/
                var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.correctedDate == Corrected && m.mntStatus!=null).OrderByDescending(m => m.id).FirstOrDefault();/**/
                BreakdownticketDet.mntClosureOpId = OperatorLoginDet.operatorId;
                db.SaveChanges();
                int ReasonID = Convert.ToInt32(BreakdownticketDet.reasonId);
                int OperatorID = Convert.ToInt32(BreakdownticketDet.operatorId);
                MacintanceAccpobj.Reason = GetReson(ReasonID);
                MacintanceAccpobj.DateTimeDis = Convert.ToString(BreakdownticketDet.bdTktDateTime);
                MacintanceAccpobj.Operatorname = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorId == OperatorID).Select(m => m.operatorName).FirstOrDefault();
                MacintanceAccpobj.MaintName = OperatorLoginDet.operatorName;
                MacintanceAccpobj.AcceptTime = Convert.ToString(BreakdownticketDet.mntAcp_RejDateTime);
                MacintanceAccpobj.finish = Convert.ToString(BreakdownticketDet.ProdFinished);
                MacintanceAccpobj.result = Result;
                Result = JsonConvert.SerializeObject(MacintanceAccpobj);
            }
            return Result;
        }

        public string UpdateRemarks1(int id, string RemartsData)
        {
            DateTime Correcteddate = DateTime.Now.Date;
            int Machineid = id;
            string Result = "Fail";
            var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.mntStatus == true && m.MaintFinished==null && m.AcceptFlage==null && m.correctedDate == Correcteddate).OrderByDescending(m => m.id).FirstOrDefault();
            if (BreakdownticketDet != null)
            {
                MacintanceAccp MacintanceAccpobj = new MacintanceAccp();
                BreakdownticketDet.mntRemarks = RemartsData;
                BreakdownticketDet.MaintFinished = 1;
                BreakdownticketDet.tktClosingTime = DateTime.Now;
                db.SaveChanges();
                //ModeCheckBreakdown(Machineid);
                Result = "Success";
            }
            return Result;
        }

        public string LoginCheckProd1(string UserName, string Password, int Machineid)
        {
            DateTime Correcteddate = DateTime.Now.Date;
            string Result = "Fail";
            DateTime dt = DateTime.Now.Date;
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            var OperatorLoginDet = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorUserName == UserName && m.operatorPwd == Password && m.roleId == 6).FirstOrDefault();
            if (OperatorLoginDet != null)
            {
                Session["maintUser"] = UserName;
                Session["maintpwd"] = Password;
                ProuctioneAccp MacintanceAccpobj = new ProuctioneAccp();
                var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.correctedDate == Correcteddate).OrderByDescending(m => m.id).FirstOrDefault();/**/
                BreakdownticketDet.prodOpId = OperatorLoginDet.operatorLoginId;
                BreakdownticketDet.prodStatus = true;
                db.SaveChanges();
                int MNTId = Convert.ToInt32(BreakdownticketDet.mntOpId);
                MacintanceAccpobj.MachineName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();/*&& m.operatorId == Operatorid && m.correctedDate == dt*/
                int ReasonID = Convert.ToInt32(BreakdownticketDet.reasonId);
                int OperatorID = Convert.ToInt32(BreakdownticketDet.operatorId);
                MacintanceAccpobj.Reason = GetReson(ReasonID);
                MacintanceAccpobj.DateTimeDis = Convert.ToString(BreakdownticketDet.bdTktDateTime);
                MacintanceAccpobj.AcceptTime = Convert.ToString(BreakdownticketDet.mntAcp_RejDateTime);
                MacintanceAccpobj.finish = Convert.ToString(BreakdownticketDet.tktClosingTime);
                MacintanceAccpobj.Operatorname = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorId == OperatorID).Select(m => m.operatorName).FirstOrDefault();
                MacintanceAccpobj.result = BreakdownticketDet.mntRemarks;
                MacintanceAccpobj.MaintName = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == MNTId).Select(m => m.operatorName).FirstOrDefault();
                Result = JsonConvert.SerializeObject(MacintanceAccpobj);
            }
            return Result;
        }

        public string ReworkBreakdownStartTime1()
        {
            string Result = "";
            DateTime Correcteddate = DateTime.Now;
            int OperatorloginID = Convert.ToInt32(Session["OUserID"]);
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            List<ReworkBreakDown> ListObj = new List<ReworkBreakDown>();
            var OperatormachiDet = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorloginID).ToList();
            foreach (var row in OperatormachiDet)
            {
                //int machid = id;
                int machid = Convert.ToInt32(row.machineId);
                ReworkBreakDown Obj = new ReworkBreakDown();
                Obj.MachineID = machid;
                var Wodet = db.tblworkorderentries.Where(m => m.MachineID == machid && m.OperatorID == Operatorid && m.IsReWork == 1 && m.ReWorkEnd == 0 && m.ReWorkStart == 1 && m.IsStarted == 1 && m.IsFinished == 0 && m.CorrectedDate == Correcteddate.Date).OrderByDescending(m => m.HMIID).FirstOrDefault();
                if (Wodet != null)
                {
                    Obj.ReworkStartTime = Convert.ToString(Wodet.reworkStartTime);
                    Obj.ReworkStart = 1;
                }
                else
                {
                    Wodet = db.tblworkorderentries.Where(m => m.MachineID == machid && m.OperatorID == Operatorid && m.IsReWork == 1 && m.ReWorkEnd == 1 && m.ReWorkStart == 1 && m.IsStarted == 1 && m.IsFinished == 0 && m.CorrectedDate == Correcteddate.Date).OrderByDescending(m => m.HMIID).FirstOrDefault();
                    if (Wodet != null)
                    {
                        Obj.ReworkStart = 2;
                    }
                    else
                    {
                        Obj.ReworkStart = 3;
                    }
                }
                //var BreakDownTicketdet = db.tblBreakDownTickects.Where(m => m.machineId == machid && m.operatorId == Operatorid && m.reasonId != null && m.ProdFinished == null && m.correctedDate == Correcteddate.Date).OrderByDescending(m => m.id).FirstOrDefault();
                var BreakDownTicketdet = db.tblBreakDownTickects.Where(m => m.machineId == machid && m.operatorId == Operatorid && m.correctedDate == Correcteddate.Date).OrderByDescending(m => m.id).FirstOrDefault();
                if (BreakDownTicketdet != null)
                {
                    string Colisntime = Convert.ToString(BreakDownTicketdet.tktClosingTime);
                    string AccTime = Convert.ToString(BreakDownTicketdet.mntAcp_RejDateTime);
                    string MntRejID = Convert.ToString(BreakDownTicketdet.mntRrejectReason);  
                    string ProdRRID= Convert.ToString(BreakDownTicketdet.prodRejId);
                    string ProdFinished= Convert.ToString(BreakDownTicketdet.prodAcp_RejDateTime);
                    if (ProdRRID != "")
                    {
                        Obj.ContentName = "Break Down Prod Reject Time";
                        Obj.BreakDownStartTime = ProdFinished;
                    }
                    else if (ProdFinished != "")
                    {
                        Obj.ContentName = "Break Down Prod Finish Time";
                        Obj.BreakDownStartTime = ProdFinished;
                    }
                    else if (Colisntime!="")
                    {
                        Obj.ContentName = "Break Down Closing Time";
                        Obj.BreakDownStartTime = Colisntime;
                    }
                    else if(MntRejID!=null)
                    {
                        Obj.ContentName = "Break Down Mnt Reject Time";
                        Obj.BreakDownStartTime = AccTime;
                    }
                    else if(AccTime!="")
                    {
                        Obj.ContentName = "Break Down Accepting Time";
                        Obj.BreakDownStartTime = AccTime;
                    }  
                    else
                    {
                        Obj.ContentName = "Break Down Start Time";
                        Obj.BreakDownStartTime = Convert.ToString(BreakDownTicketdet.bdTktDateTime);
                    }
                }
                //else
                //{
                //    BreakDownTicketdet = db.tblBreakDownTickects.Where(m => m.machineId == machid && m.operatorId == Operatorid && m.reasonId != null && m.ProdFinished != null && m.prodRejId==null && m.correctedDate == Correcteddate.Date).OrderByDescending(m => m.id).FirstOrDefault();
                //    if (BreakDownTicketdet != null)
                //    {
                //        Obj.ContentName = "Break Down Finished Time";
                //        Obj.BreakDownStartTime = Convert.ToString(BreakDownTicketdet.prodAcp_RejDateTime);
                //    }
                //    else
                //    {
                //        BreakDownTicketdet = db.tblBreakDownTickects.Where(m => m.machineId == machid && m.operatorId == Operatorid && m.reasonId != null && m.ProdFinished != null && m.prodRejId != null && m.correctedDate == Correcteddate.Date).OrderByDescending(m => m.id).FirstOrDefault();
                //        if (BreakDownTicketdet != null)
                //        {
                //            Obj.ContentName = "Break Down Prod Reject Time";
                //            Obj.BreakDownStartTime = Convert.ToString(BreakDownTicketdet.prodAcp_RejDateTime);
                //        }
                //    }
                //}
                
                ListObj.Add(Obj);
            }

            Result = JsonConvert.SerializeObject(ListObj);
            return Result;
        }

        #endregion
    }

}