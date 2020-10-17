using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using SRKSDemo.Server_Model;

namespace SRKSDemo.ReportsCalcClass
{
    public class OEEReportData
    {
        i_facility_shaktiEntities1 Serverdb = new i_facility_shaktiEntities1();

        public OEEReportData()
        {

        }

        // Get Production Data for Man Machine Ticket.
        public void insertManMacProd(int MachineID, DateTime CorrectedDate)
        {
            var getProdList = Serverdb.tblworkorderentries.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate.Date).OrderBy(m => m.WOStart).ToList();

            foreach (var ProdRow in getProdList)
            {
                var GetEntry = Serverdb.tbl_ProdManMachine.Where(m => m.WOID == ProdRow.HMIID).FirstOrDefault();
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

                    int OperatingTime = 0;
                    int LossTime = 0;
                    int MinorLossTime = 0;
                    int MntTime = 0;
                    int SetupTime = 0;
                    int SetupMinorTime = 0;
                    int PowerOffTime = 0;

                    #region Logic to get the Mode Durations between a Production Order which are completed
                    var GetModeDurations = Serverdb.tblmodes.Where(m => m.MachineID == MachineID && m.StartTime >= ProdStartTime && m.StartTime < ProdEndtime && m.EndTime > ProdStartTime && m.EndTime < ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).ToList();
                    foreach (var ModeRow in GetModeDurations)
                    {
                        if (ModeRow.ModeType == "PROD")
                        {
                            OperatingTime += (int)(ModeRow.DurationInSec / 60);
                        }
                        else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec > 600)
                        {
                            LossTime += (int)(ModeRow.DurationInSec / 60);
                            int LossDuration = (int)(ModeRow.DurationInSec / 60);
                            if (ModeRow.LossCodeID != null)
                                insertProdlosses(ProdRow.HMIID, (int)ModeRow.LossCodeID, LossDuration, CorrectedDate);
                        }
                        else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec < 600)
                        {
                            MinorLossTime += (int)(ModeRow.DurationInSec / 60);
                        }
                        else if (ModeRow.ModeType == "MNT")
                        {
                            MntTime += (int)(ModeRow.DurationInSec / 60);
                        }
                        else if (ModeRow.ModeType == "POWEROFF")
                        {
                            PowerOffTime += (int)(ModeRow.DurationInSec / 60);
                        }
                        else if (ModeRow.ModeType == "SETUP")
                        {
                            try
                            {
                                SetupTime += (int)(Serverdb.tblSetupMaints.Where(m => m.ModeID == ModeRow.ModeID).Select(m => m.DurationInSec).First() / 60);
                                SetupMinorTime += (int)(Serverdb.tblSetupMaints.Where(m => m.ModeID == ModeRow.ModeID).Select(m => m.MinorLossTime).First() / 60);
                            }
                            catch { }
                        }
                    }
                    #endregion

                    #region Logic to get the Mode Duration Which Was started before this Production and Ended during this Production
                    var GetEndModeDuration = Serverdb.tblmodes.Where(m => m.MachineID == MachineID && m.StartTime < ProdStartTime && m.EndTime > ProdStartTime && m.EndTime < ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
                    if (GetEndModeDuration != null)
                    {
                        if (GetEndModeDuration.ModeType == "PROD")
                        {
                            OperatingTime += (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                        }
                        else if (GetEndModeDuration.ModeType == "IDLE")
                        {
                            LossTime += (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                            int LossDuration = (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                            if (GetEndModeDuration.LossCodeID != null)
                                insertProdlosses(ProdRow.HMIID, (int)GetEndModeDuration.LossCodeID, LossDuration, CorrectedDate);
                            //insertProdlosses(WOID, LossID, LossDuration, CorrectedDate);
                        }
                        else if (GetEndModeDuration.ModeType == "MNT")
                        {
                            MntTime += (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                        }
                        else if (GetEndModeDuration.ModeType == "POWEROFF")
                        {
                            PowerOffTime += (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                        }
                    }
                    #endregion

                    #region Logic to get the Mode Duration Which Was Started during the Production and Ended after the Production
                    var GetStartModeDuration = Serverdb.tblmodes.Where(m => m.MachineID == MachineID && m.StartTime >= ProdStartTime && m.EndTime >= ProdStartTime && m.StartTime < ProdEndtime && m.EndTime > ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
                    if (GetStartModeDuration != null)
                    {
                        if (GetStartModeDuration.ModeType == "PROD")
                        {
                            OperatingTime += (int)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60);
                        }
                        else if (GetStartModeDuration.ModeType == "IDLE")
                        {
                            LossTime += (int)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60);
                            int LossDuration = (int)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60);
                            if (GetStartModeDuration.LossCodeID != null)
                                insertProdlosses(ProdRow.HMIID, (int)GetStartModeDuration.LossCodeID, LossDuration, CorrectedDate);
                            //insertProdlosses(WOID, LossID, LossDuration, CorrectedDate);
                        }
                        else if (GetStartModeDuration.ModeType == "MNT")
                        {
                            MntTime += (int)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60);
                        }
                        else if (GetStartModeDuration.ModeType == "POWEROFF")
                        {
                            PowerOffTime += (int)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60);
                        }
                    }
                    #endregion
                    int TotlaQty = ProdRow.Total_Qty;
                    if (TotlaQty == 0)
                        TotlaQty = 1;
                    decimal IdealCycleTimeVal = 0;
                    var IdealCycTime = Serverdb.tblparts.Where(m => m.FGCode == ProdRow.FGCode && m.OperationNo == ProdRow.OperationNo).FirstOrDefault();
                    if (IdealCycTime != null)
                        IdealCycleTimeVal = IdealCycTime.IdealCycleTime;
                    double TotalTime = ProdEndtime.Subtract(ProdStartTime).TotalMinutes;
                    decimal UtilPercent = (decimal)Math.Round(OperatingTime / TotalTime * 100, 2);
                    decimal Quality = (decimal)Math.Round((double)ProdRow.Yield_Qty / TotlaQty * 100, 2);
                    decimal Performance = (decimal)Math.Round((double)IdealCycleTimeVal * (double)ProdRow.Total_Qty / OperatingTime * 100, 2);
                    int PerformanceFactor = (int)IdealCycleTimeVal * ProdRow.Total_Qty;

                    tbl_ProdManMachine PRA = new tbl_ProdManMachine();
                    PRA.MachineID = MachineID;
                    PRA.WOID = ProdRow.HMIID;
                    PRA.CorrectedDate = CorrectedDate.Date;
                    PRA.TotalLoss = LossTime;
                    PRA.TotalOperatingTime = OperatingTime;
                    PRA.TotalSetup = SetupTime + SetupMinorTime;
                    PRA.TotalMinorLoss = MinorLossTime - SetupMinorTime;
                    PRA.TotalSetupMinorLoss = SetupMinorTime;
                    PRA.TotalPowerLoss = PowerOffTime;
                    PRA.UtilPercent = UtilPercent;
                    PRA.QualityPercent = Quality;
                    PRA.PerformancePerCent = Performance;
                    PRA.PerfromaceFactor = PerformanceFactor;
                    PRA.InsertedOn = DateTime.Now;
                    Serverdb.tbl_ProdManMachine.Add(PRA);
                    Serverdb.SaveChanges();
                }
            }
        }

        //Insert Losses Data for Man Machine Ticket.
        public void insertProdlosses(int WOID, int LossID, int LossDuration, DateTime CorrectedDate)
        {
            var Presentloss = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == WOID && m.LossID == LossID).FirstOrDefault();
            if (Presentloss == null)
            {
                tbl_ProdOrderLosses PRA = new tbl_ProdOrderLosses();
                PRA.LossID = LossID;
                if (LossID != 0)
                {
                    var GetLossDet = Serverdb.tbllossescodes.Find(LossID);
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
                PRA.LossDuration = LossDuration;
                Serverdb.tbl_ProdOrderLosses.Add(PRA);
                Serverdb.SaveChanges();
            }
            else
            {
                Presentloss.LossDuration = Presentloss.LossDuration + LossDuration;
                Serverdb.Entry(Presentloss).State = System.Data.Entity.EntityState.Modified;
                Serverdb.SaveChanges();
            }
        }
    }
}