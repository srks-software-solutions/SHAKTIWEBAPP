using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SRKSDemo.Server_Model;
using SRKSDemo.Models;

namespace SRKSDemo.Controllers
{
    public class IMTEXDashboardController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        // GET: Dashboard
        public ActionResult Index()
        {

            return View();
        }
        public string GetCurrentMachineStatus()
        {
            string res = "";
            //string correctedDate = DateTime.Now.ToString("yyyy-MM-19");
            string correctedDate = DateTime.Now.ToString("yyyy-MM-dd");
            DateTime correctedDate1 = Convert.ToDateTime(correctedDate);

            GetOEE objoee = OEE(correctedDate);
            res = JsonConvert.SerializeObject(objoee);


            return res;
        }
        public string GetPreviousMachineStatus()
        {
            string res = "";
            //string correctedDate = DateTime.Now.ToString("yyyy-MM-19");
            string correctedDate = DateTime.Now.ToString("yyyy-MM-19");
            DateTime correctedDate1 = Convert.ToDateTime(correctedDate);

            GetOEE objoee = OEE(correctedDate);
            res = JsonConvert.SerializeObject(objoee);


            return res;
        }
        public GetOEE OEE(string correctedDate)
        {
            GetOEE objoee = new GetOEE();
            DateTime correctedDate1 = Convert.ToDateTime(correctedDate);
            double TotalOperatingTime = 0;
            double TotalDownTime = 0;
            double TotalAcceptedQty = 0;
            double TotalRejectedQty = 0;
            double TotalPerformanceFactor = 0;
            int MachineID = 1;
            double DayOperatingTime = 0;
            double DayDownTime = 0;
            double DayAcceptedQty = 0;
            double DayRejectedQty = 0;
            double DayPerformanceFactor = 0;
            insertManMacProd(MachineID, correctedDate1);
            var GetMainLossList = db.tbllossescodes.Where(m => m.LossCodesLevel == 1 && m.IsDeleted == 0 && m.MessageType != "SETUP").OrderBy(m => m.LossCodeID).ToList();
            var prodplanchine = db.tbl_ProdManMachine.Where(m => m.MachineID == MachineID && m.CorrectedDate == correctedDate1.Date).ToList();
            if (prodplanchine.Count > 0)
            {
                foreach (var ProdRow in prodplanchine)
                {

                    TotalOperatingTime += (double)ProdRow.TotalOperatingTime;
                    TotalDownTime += (double)ProdRow.TotalLoss + (double)ProdRow.TotalMinorLoss + (double)ProdRow.TotalSetup;
                    //TotalAcceptedQty += ProdRow.tblworkorderentry.Yield_Qty;
                    //TotalRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
                    //TotalPerformanceFactor += ProdRow.PerfromaceFactor;
                    //int TotalQty = ProdRow.tblworkorderentry.Yield_Qty + ProdRow.tblworkorderentry.ScrapQty;
                    //if (TotalQty == 0)
                    //    TotalQty = 1;
                    List<IDLELosses> IdleLosslist = new List<IDLELosses>();
                    List<BreakdownLosses> brkdwnlosslist = new List<BreakdownLosses>();
                    List<Losses> losslist = new List<Losses>();
                    //foreach (var LossRow in GetMainLossList)
                    //{
                    // var getWoLossList1 = db.tblmodes.Where(m => m.CorrectedDate == correctedDate1.Date && m.LossCodeID == LossRow.LossCodeID && m.IsCompleted==1 ).OrderBy(m=>new { m.ModeID,m.StartTime}).ToList();

                    var getmodes = db.tblmodes.Where(m => m.CorrectedDate == correctedDate1.Date && m.IsCompleted == 1 && m.ModeTypeEnd==1 && m.IsPiWeb==0 ).OrderBy(m => new { m.ModeID, m.StartTime }).ToList();

                    var TotalLossDuration = getmodes.Where(m => m.ModeType == "IDLE").Sum(m => m.DurationInSec).ToString();
                    decimal TotalLossDura = Convert.ToDecimal(TotalLossDuration);

                    var getIdleLosses = getmodes.Where(m => m.ModeType == "IDLE").ToList();
                    var TotalBreakdownDuration = getmodes.Where(m => m.ModeType == "MNT").Sum(m => m.DurationInSec).ToString();
                    var getbrklosses = getmodes.Where(m => m.ModeType == "MNT").ToList();
                    if (getIdleLosses.Count > 0)
                    {
                        decimal LossDuration = 0;
                        int count = 0;
                        foreach (var row in getmodes)
                        {
                            IDLELosses objloss = new IDLELosses();

                            //var lossrow = ;

                            if (row.LossCodeID != null)
                            {
                                count += 1;
                                objloss.ID = count;
                                objloss.LossID = row.LossCodeID;
                                objloss.LossName = row.tbllossescode1.LossCodeDesc;
                                LossDuration = Convert.ToDecimal(row.DurationInSec);
                                decimal percent = ((LossDuration) / (TotalLossDura));
                                objloss.LossPercent = (double)((LossDuration / TotalLossDura) * 100);



                                IdleLosslist.Add(objloss);
                            }
                        }

                    }
                    if (getbrklosses.Count > 0)
                    {
                        int count = 0;
                        decimal LossDuration = 0;
                        foreach (var row in getbrklosses)
                        {
                            BreakdownLosses objloss = new BreakdownLosses();
                            if (row.LossCodeID != null)
                            {
                                count += 1;
                                objloss.ID = count;
                                objloss.LossID = row.tbllossescode1.LossCodeID;
                                objloss.LossName = row.tbllossescode1.LossCodeDesc;
                                LossDuration = Convert.ToDecimal(row.DurationInSec);
                                decimal percent = ((LossDuration) / (TotalLossDura));
                                objloss.LossPercent = (double)((LossDuration / Convert.ToInt32(TotalLossDuration)) * 100);
                                brkdwnlosslist.Add(objloss);
                            }
                        }
                    }

                    if (getmodes.Count > 0)
                    {
                        int count = 0;
                        decimal LossDuration = 0;
                        foreach (var row in getmodes)
                        {
                            Losses objloss = new Losses();
                            if (row.LossCodeID != null)
                            {
                                count += 1;
                                objloss.ID = count;
                                objloss.LossID = row.tbllossescode1.LossCodeID;
                                objloss.LossName = row.tbllossescode1.LossCodeDesc;
                                LossDuration = Convert.ToDecimal(row.DurationInSec);
                                objloss.Duration = (double)((LossDuration / Convert.ToInt32(TotalLossDuration)) * 100);
                                losslist.Add(objloss);
                            }
                        }
                    }

                    //}
                    List<IDLELosses> IdleLosslistdist = new List<IDLELosses>();
                    List<BreakdownLosses> brkdwnlosslistdist = new List<BreakdownLosses>();
                    List<Losses> losslistdist = new List<Losses>();
                    var IdleLosslist1 = IdleLosslist.Select(m => new { m.LossName, m.LossID }).Distinct().ToList();
                    foreach (var row in IdleLosslist1)
                    {

                        var lossrow = IdleLosslist.Where(m => m.LossName == row.LossName).OrderByDescending(m => m.LossPercent).FirstOrDefault();
                        IdleLosslistdist.Add(lossrow);

                    }

                    var brkLosslist1 = brkdwnlosslist.Select(m => new { m.LossName, m.LossID }).Distinct().ToList();
                    foreach (var row in brkLosslist1)
                    {

                        var lossrow = brkdwnlosslist.Where(m => m.LossName == row.LossName).OrderByDescending(m => m.LossPercent).FirstOrDefault();
                        brkdwnlosslistdist.Add(lossrow);

                    }

                    var losslistdist1 = losslist.Select(m => new { m.LossName, m.LossID }).Distinct().ToList();
                    foreach (var row in losslistdist1)
                    {

                        var lossrow = losslist.Where(m => m.LossName == row.LossName).OrderByDescending(m => m.Duration).FirstOrDefault();
                        losslistdist.Add(lossrow);

                    }

                    #region Commented
                    //IdleLosslist = IdleLosslist.OrderByDescending(m => m.LossPercent).Take(5).ToList();

                    //brkdwnlosslist = brkdwnlosslist.OrderBy(m => m.LossName).Distinct().ToList();
                    //brkdwnlosslist = brkdwnlosslist.OrderByDescending(m => m.LossPercent).Take(5).ToList();

                    //losslist = losslist.OrderBy(m => m.LossName).Distinct().ToList();
                    //losslist = losslist.OrderByDescending(m => m.Duration).Take(5).ToList();
                    #endregion

                    objoee.TopIDLELosses = IdleLosslistdist.OrderByDescending(m => m.LossPercent).Take(5).ToList();
                    objoee.TopbrkdwnLosses = brkdwnlosslistdist.OrderByDescending(m => m.LossPercent).Take(5).ToList();
                    objoee.TopLosses = losslistdist.OrderByDescending(m => m.Duration).Take(5).ToList();
                    DayOperatingTime += (double)ProdRow.TotalOperatingTime;
                    DayDownTime += (double)ProdRow.TotalLoss + (double)ProdRow.TotalMinorLoss;
                    //DayAcceptedQty += ProdRow.tblworkorderentry.Yield_Qty;
                    //DayRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
                    DayPerformanceFactor += ProdRow.PerfromaceFactor;
                }
                int TotQty = (int)(DayAcceptedQty + DayRejectedQty);
                if (TotQty == 0)
                    TotQty = 1;
                double DayOpTime = DayOperatingTime;
                if (DayOperatingTime == 0)
                    DayOpTime = 1;
                decimal DayAvailPercent = (decimal)Math.Round(DayOperatingTime / (24 * 1), 2);
                decimal DayPerformancePercent = (decimal)Math.Round(DayPerformanceFactor / DayOpTime, 2);
                decimal DayQualityPercent = (decimal)Math.Round((DayAcceptedQty / (TotQty)), 2);
                decimal DayOEEPercent = (decimal)Math.Round((double)(DayAvailPercent) * (double)(DayPerformancePercent) * (double)(DayQualityPercent), 2);
                objoee.Machinetimes = getModeTimings(MachineID, correctedDate1);

                if (DayOperatingTime == 0 && objoee.Machinetimes!=null)
                    DayOperatingTime =(Double) objoee.Machinetimes.RunningTime;
                 DayAvailPercent = (decimal)Math.Round(DayOperatingTime / (24 * 1), 2);
                 DayPerformancePercent = (decimal)Math.Round(DayPerformanceFactor / DayOpTime, 2);
                 DayQualityPercent = (decimal)Math.Round((DayAcceptedQty / (TotQty)), 2);
                 DayOEEPercent = (decimal)Math.Round((double)(DayAvailPercent) * (double)(DayPerformancePercent) * (double)(DayQualityPercent), 2);
                objoee.Availability = Convert.ToInt32(DayAvailPercent);// AvailabilityPercentage
                objoee.Quality = Convert.ToInt32(DayQualityPercent);
                objoee.Performance = Convert.ToInt32(DayPerformancePercent);
                objoee.OEE = Convert.ToInt32(DayOEEPercent);

            }
            objoee.Machinetimes = getModeTimings(MachineID, correctedDate1);
            return objoee;
        }
        public void insertManMacProd(int MachineID, DateTime CorrectedDate)
        {
            try
            {
                var getProdList = db.tblworkorderentries.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate.Date).OrderBy(m => m.WOStart).ToList();

                foreach (var ProdRow in getProdList)
                {
                    var GetEntry = db.tbl_ProdManMachine.Where(m => m.WOID == ProdRow.HMIID).FirstOrDefault();
                    if (GetEntry == null)
                    {
                        DateTime ProdStartTime = ProdRow.WOStart;
                        DateTime ProdEndtime = DateTime.Now;
                        try
                        {
                            if (ProdRow.WOEnd.HasValue)
                            {
                                ProdEndtime = Convert.ToDateTime(ProdRow.WOEnd);
                            }
                        }
                        catch { }

                        decimal OperatingTime = 0;
                        decimal LossTime = 0;
                        decimal MinorLossTime = 0;
                        decimal MntTime = 0;
                        decimal SetupTime = 0;
                        decimal SetupMinorTime = 0;
                        decimal PowerOffTime = 0;

                        #region Logic to get the Mode Durations between a Production Order which are completed
                        var GetModeDurations = db.tblmodes.Where(m => m.MachineID == MachineID && m.StartTime >= ProdStartTime && m.StartTime < ProdEndtime && m.EndTime > ProdStartTime && m.EndTime <= ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).ToList();
                        foreach (var ModeRow in GetModeDurations)
                        {
                            if (ModeRow.ModeType == "PROD")
                            {
                                OperatingTime += (decimal)(ModeRow.DurationInSec / 60.00);
                            }
                            else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec > 600)
                            {
                                LossTime += (decimal)(ModeRow.DurationInSec / 60.00);
                                decimal LossDuration = (decimal)(ModeRow.DurationInSec / 60.00);
                                if (ModeRow.LossCodeID != null)
                                    insertProdlosses(ProdRow.HMIID, (int)ModeRow.LossCodeID, LossDuration, CorrectedDate, MachineID);
                            }
                            else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec < 600)
                            {
                                MinorLossTime += (decimal)(ModeRow.DurationInSec / 60.00);
                            }
                            else if (ModeRow.ModeType == "MNT")
                            {
                                MntTime += (decimal)(ModeRow.DurationInSec / 60.00);
                            }
                            else if (ModeRow.ModeType == "POWEROFF")
                            {
                                PowerOffTime += (decimal)(ModeRow.DurationInSec / 60.00);
                            }
                            else if (ModeRow.ModeType == "SETUP")
                            {
                                try
                                {
                                    SetupTime += (decimal)Convert.ToDateTime(ModeRow.LossCodeEnteredTime).Subtract(Convert.ToDateTime(ModeRow.StartTime)).TotalMinutes;
                                    SetupMinorTime += (decimal)(db.tblSetupMaints.Where(m => m.ModeID == ModeRow.ModeID).Select(m => m.MinorLossTime).First() / 60.00);
                                }
                                catch { }
                            }
                        }
                        #endregion

                        #region Logic to get the Mode Duration Which Was started before this Production and Ended during this Production
                        var GetEndModeDuration = db.tblmodes.Where(m => m.MachineID == MachineID && m.StartTime < ProdStartTime && m.EndTime > ProdStartTime && m.EndTime <= ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
                        if (GetEndModeDuration != null)
                        {
                            if (GetEndModeDuration.ModeType == "PROD")
                            {
                                OperatingTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
                            }
                            else if (GetEndModeDuration.ModeType == "IDLE")
                            {
                                LossTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
                                decimal LossDuration = (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
                                if (GetEndModeDuration.LossCodeID != null)
                                    insertProdlosses(ProdRow.HMIID, (int)GetEndModeDuration.LossCodeID, LossDuration, CorrectedDate, MachineID);
                                //insertProdlosses(WOID, LossID, LossDuration, CorrectedDate);
                            }
                            else if (GetEndModeDuration.ModeType == "MNT")
                            {
                                MntTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
                            }
                            else if (GetEndModeDuration.ModeType == "POWEROFF")
                            {
                                PowerOffTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
                            }
                        }
                        #endregion

                        #region Logic to get the Mode Duration Which Was Started during the Production and Ended after the Production
                        var GetStartModeDuration = db.tblmodes.Where(m => m.MachineID == MachineID && m.StartTime >= ProdStartTime && m.EndTime >= ProdStartTime && m.StartTime < ProdEndtime && m.EndTime > ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
                        if (GetStartModeDuration != null)
                        {
                            if (GetStartModeDuration.ModeType == "PROD")
                            {
                                OperatingTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
                            }
                            else if (GetStartModeDuration.ModeType == "IDLE")
                            {
                                LossTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
                                decimal LossDuration = (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
                                if (GetStartModeDuration.LossCodeID != null)
                                    insertProdlosses(ProdRow.HMIID, (int)GetStartModeDuration.LossCodeID, LossDuration, CorrectedDate, MachineID);
                                //insertProdlosses(WOID, LossID, LossDuration, CorrectedDate);
                            }
                            else if (GetStartModeDuration.ModeType == "MNT")
                            {
                                MntTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
                            }
                            else if (GetStartModeDuration.ModeType == "POWEROFF")
                            {
                                PowerOffTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
                            }
                        }
                        #endregion

                        int TotlaQty = ProdRow.Total_Qty;
                        if (TotlaQty == 0)
                            TotlaQty = 1;
                        decimal GetOptime = OperatingTime;
                        if (GetOptime == 0)
                        {
                            GetOptime = 1;
                        }
                        decimal IdealCycleTimeVal = 0;
                        var IdealCycTime = db.tblparts.Where(m => m.FGCode == ProdRow.FGCode && m.OperationNo == ProdRow.OperationNo).FirstOrDefault();
                        if (IdealCycTime != null)
                            IdealCycleTimeVal = IdealCycTime.IdealCycleTime;
                        double TotalTime = ProdEndtime.Subtract(ProdStartTime).TotalMinutes;
                        decimal UtilPercent = (decimal)Math.Round((double)OperatingTime / TotalTime * 100, 2);
                        decimal Quality = (decimal)Math.Round((double)ProdRow.Yield_Qty / TotlaQty * 100, 2);
                        decimal Performance = (decimal)Math.Round((double)IdealCycleTimeVal * (double)ProdRow.Total_Qty / (double)GetOptime * 100, 2);
                        int PerformanceFactor = (int)IdealCycleTimeVal * ProdRow.Total_Qty;
                        tbl_ProdManMachine PRA = new tbl_ProdManMachine();
                        PRA.MachineID = MachineID;
                        PRA.WOID = ProdRow.HMIID;
                        PRA.CorrectedDate = CorrectedDate.Date;
                        PRA.TotalLoss = LossTime;
                        PRA.TotalOperatingTime = Math.Round(OperatingTime, 2);
                        PRA.TotalSetup = Math.Round(SetupTime + SetupMinorTime, 2);
                        PRA.TotalMinorLoss = Math.Round(MinorLossTime - SetupMinorTime, 2);
                        PRA.TotalSetupMinorLoss = Math.Round(SetupMinorTime, 2);
                        PRA.TotalPowerLoss = Math.Round(PowerOffTime, 2);
                        PRA.UtilPercent = Math.Round(UtilPercent, 2);
                        PRA.QualityPercent = Math.Round(Quality, 2);
                        PRA.PerformancePerCent = Math.Round(Performance, 2);
                        PRA.PerfromaceFactor = PerformanceFactor;
                        PRA.InsertedOn = DateTime.Now;
                        db.tbl_ProdManMachine.Add(PRA);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
        public void insertProdlosses(int WOID, int LossID, decimal LossDuration, DateTime CorrectedDate, int MachineID)
        {
            var Presentloss = db.tbl_ProdOrderLosses.Where(m => m.WOID == WOID && m.LossID == LossID).FirstOrDefault();
            if (Presentloss == null)
            {
                tbl_ProdOrderLosses PRA = new tbl_ProdOrderLosses();
                PRA.LossID = LossID;
                if (LossID != 0)
                {
                    var GetLossDet = db.tbllossescodes.Find(LossID);
                    if (GetLossDet.LossCodesLevel1ID == null)
                    {
                        PRA.LossCodeL1ID = LossID;
                    }
                    else
                    {
                        PRA.LossCodeL1ID = (int)GetLossDet.LossCodesLevel1ID;
                        PRA.LossID = (int)GetLossDet.LossCodesLevel1ID;
                    }
                    if (GetLossDet.LossCodesLevel2ID != null)
                        PRA.LossCodeL2ID = (int)GetLossDet.LossCodesLevel2ID;
                }
                //PRA.MachineID = MachineID;
                PRA.WOID = WOID;
                PRA.CorrectedDate = CorrectedDate.Date;
                PRA.LossDuration = (int)LossDuration;
                PRA.MachineID = MachineID;
                db.tbl_ProdOrderLosses.Add(PRA);
                db.SaveChanges();
            }
            else
            {
                Presentloss.LossDuration = (int)(Presentloss.LossDuration + LossDuration);
                db.Entry(Presentloss).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
        }
        private MachineTime getModeTimings(int MachineID, DateTime correctedDate)
        {
            decimal OperatingTime = 0;
            decimal LossTime = 0;
            decimal MinorLossTime = 0;
            decimal MntTime = 0;
            decimal SetupTime = 0;
            decimal SetupMinorTime = 0;
            decimal PowerOffTime = 0;
            decimal PowerONTime = 0;
            MachineTime objmachine = new MachineTime();
            var GetModeDurations = db.tblmodes.Where(m => m.MachineID == MachineID && m.CorrectedDate == correctedDate.Date && m.IsCompleted == 1 && m.ModeTypeEnd == 1).ToList();
            foreach (var ModeRow in GetModeDurations)
            {
                if (ModeRow.ModeType == "PROD")
                {
                    OperatingTime += (decimal)(ModeRow.DurationInSec / 60.00);
                }
                else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec > 600)
                {
                    LossTime += (decimal)(ModeRow.DurationInSec / 60.00);
                    decimal LossDuration = (decimal)(ModeRow.DurationInSec / 60.00);
                    //if (ModeRow.LossCodeID != null)
                    // insertProdlosses(ProdRow.HMIID, (int)ModeRow.LossCodeID, LossDuration, CorrectedDate, MachineID);
                }
                else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec < 600)
                {
                    MinorLossTime += (decimal)(ModeRow.DurationInSec / 60.00);
                }
                else if (ModeRow.ModeType == "MNT")
                {
                    MntTime += (decimal)(ModeRow.DurationInSec / 60.00);
                }
                else if (ModeRow.ModeType == "POWEROFF")
                {
                    PowerOffTime += (decimal)(ModeRow.DurationInSec / 60.00);
                }
                else if (ModeRow.ModeType == "SETUP")
                {
                    try
                    {
                        SetupTime += (decimal)Convert.ToDateTime(ModeRow.LossCodeEnteredTime).Subtract(Convert.ToDateTime(ModeRow.StartTime)).TotalMinutes;
                        SetupMinorTime += (decimal)(db.tblSetupMaints.Where(m => m.ModeID == ModeRow.ModeID).Select(m => m.MinorLossTime).First() / 60.00);
                    }
                    catch { }
                }
                else if (ModeRow.ModeType == "POWERON")
                {
                    PowerONTime += (decimal)(ModeRow.DurationInSec / 60.00);
                }
            }
            objmachine.RunningTime = OperatingTime;
            objmachine.IDLETime = LossTime + MinorLossTime;
            objmachine.BreakDownTime = PowerOffTime + MntTime;
            objmachine.PowerON = PowerONTime;

            objmachine.RunningTimePerc = (OperatingTime / 1440) * 100;
            objmachine.IDLETimePerc = (objmachine.IDLETime / 1440) * 100;
            objmachine.BreakDownTimePerc = (objmachine.BreakDownTime / 1440) * 100;
            objmachine.PowerONPerc = (objmachine.PowerON / 1440) * 100;
            return objmachine;
        }
        public string DayWiseOEE()
        {
            string res = "";
            ViewData oeedata = new ViewData();

            List<dataPoints> objOEEList = new List<dataPoints>();

            DateTime FromDate = DateTime.Now;
            DateTime ToDate = DateTime.Now.AddDays(5);

            TimeSpan dif = ToDate.Subtract(FromDate);

            int datediff = Convert.ToInt32(dif.TotalDays);

            for (int i = 0; i < datediff; i++)
            {
                DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
                DateTime correctedDate = QueryDate;
                double TotalOperatingTime = 0;
                double TotalDownTime = 0;
                double TotalAcceptedQty = 0;
                double TotalRejectedQty = 0;
                double TotalPerformanceFactor = 0;
                int MachineID = 1;
                double DayOperatingTime = 0;
                double DayDownTime = 0;
                double DayAcceptedQty = 0;
                double DayRejectedQty = 0;
                double DayPerformanceFactor = 0;
                insertManMacProd(MachineID, correctedDate);
                var prodplanchine = db.tbl_ProdManMachine.Where(m => m.MachineID == MachineID && m.CorrectedDate == correctedDate.Date).ToList();
                dataPoints objoee = new dataPoints();
                if (prodplanchine.Count > 0)
                {
                  
                    foreach (var ProdRow in prodplanchine)
                    {
                        TotalOperatingTime += (double)ProdRow.TotalOperatingTime;
                        TotalDownTime += (double)ProdRow.TotalLoss + (double)ProdRow.TotalMinorLoss + (double)ProdRow.TotalSetup;
                        //TotalAcceptedQty += ProdRow.tblworkorderentry.Yield_Qty;
                        //TotalRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
                        //TotalPerformanceFactor += ProdRow.PerfromaceFactor;
                        //int TotalQty = ProdRow.tblworkorderentry.Yield_Qty + ProdRow.tblworkorderentry.ScrapQty;
                        //if (TotalQty == 0)
                        //    TotalQty = 1;

                        //DayOperatingTime += (double)ProdRow.TotalOperatingTime;
                        //DayDownTime += (double)ProdRow.TotalLoss + (double)ProdRow.TotalMinorLoss;
                        //DayAcceptedQty += ProdRow.tblworkorderentry.Yield_Qty;
                        //DayRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
                        DayPerformanceFactor += ProdRow.PerfromaceFactor;
                    }
                   
                   
                   
                }
                int TotQty = (int)(DayAcceptedQty + DayRejectedQty);
                if (TotQty == 0)
                    TotQty = 1;
              

                double DayOpTime = DayOperatingTime;
                if (DayOpTime == 0)
                    DayOpTime = 1;
              
                 decimal DayAvailPercent = (decimal)Math.Round(DayOperatingTime / (24 * 1), 2);
                decimal DayPerformancePercent = (decimal)Math.Round(DayPerformanceFactor / DayOpTime, 2);
                decimal DayQualityPercent = (decimal)Math.Round((DayAcceptedQty / (TotQty)), 2);
                decimal DayOEEPercent = (decimal)Math.Round((double)(DayAvailPercent) * (double)(DayPerformancePercent) * (double)(DayQualityPercent), 2);
                objoee.label = correctedDate.ToString("yyyy-MM-dd");
                //objoee.Y = DayOEEPercent;
                if (i == 0)
                    objoee.y = 30;
                else if (i == 1)
                    objoee.y = 70;
                else if (i == 2)
                    objoee.y = 50;
                else if (i == 3)
                    objoee.y = 10;
                else if (i == 4)
                    objoee.y = 20;
                objOEEList.Add(objoee);
            }
            oeedata.type = "line";
            oeedata.lineColor = "orange";
            oeedata.type = "splineArea";
            oeedata.color = "rgba(83, 223, 128, .6)";
            oeedata.dataPoints = objOEEList;
            res = JsonConvert.SerializeObject(oeedata);
            return res;
        }
    }
}