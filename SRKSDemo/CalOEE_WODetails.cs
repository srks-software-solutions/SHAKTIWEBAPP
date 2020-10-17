using i_facilitylibrary;
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace SRKSDemo
{
    public class CalOEE_WODetails
    {
        private i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        public void CalculateOEEForYesterday(DateTime? StartDate, DateTime? EndDate)
        {
            //MessageBox.Show("StartTime= " + StartDate + " EndTime= " + EndDate);

            DateTime fromdate = DateTime.Now.AddDays(-1), todate = DateTime.Now.AddDays(-1);
            if (StartDate != null && EndDate != null)
            {
                fromdate = Convert.ToDateTime(StartDate);
                todate = Convert.ToDateTime(EndDate);
            }

            //fromdate = StartDate ?? DateTime.Now.AddDays(-1);
            //todate = EndDate ?? DateTime.Now.AddDays(-1);

            //DateTime fromdate = DateTime.Now.AddDays(-1), todate = DateTime.Now.AddDays(-1);
            DateTime UsedDateForExcel = Convert.ToDateTime(fromdate.ToString("yyyy-MM-dd 00:00:00"));
            double TotalDay = todate.Subtract(fromdate).TotalDays;
            #region
            for (int i = 0; i < TotalDay + 1; i++)
            {
                // 2017-02-17
                var machineData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0).ToList();
                foreach (var macrow in machineData)
                {
                    int MachineID = macrow.MachineID;

                    try
                    {
                        var OEEDataPresent = db.tbloeedashboardvariables.Where(m => m.WCID == MachineID && m.StartDate == UsedDateForExcel).ToList();
                        if (OEEDataPresent.Count == 0)
                        {
                            double green, red, yellow, blue, setup = 0, scrap = 0, NOP = 0, OperatingTime = 0, DownTimeBreakdown = 0, ROALossess = 0, AvailableTime = 0, SettingTime = 0, PlannedDownTime = 0, UnPlannedDownTime = 0;
                            double SummationOfSCTvsPP = 0, MinorLosses = 0, ROPLosses = 0;
                            double ScrapQtyTime = 0, ReWOTime = 0, ROQLosses = 0;

                            MinorLosses = GetMinorLosses(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID, "yellow");
                            if (MinorLosses < 0)
                            {
                                MinorLosses = 0;
                            }
                            blue = GetOPIDleBreakDown(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID, "blue");
                            green = GetOPIDleBreakDown(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID, "green");

                            try
                            {
                                //Availability
                                SettingTime = GetSettingTime(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID);
                                if (SettingTime < 0)
                                {
                                    SettingTime = 0;
                                }
                                ROALossess = GetDownTimeLosses(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID, "ROA");
                                if (ROALossess < 0)
                                {
                                    ROALossess = 0;
                                }
                                DownTimeBreakdown = GetDownTimeBreakdown(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID);
                                if (DownTimeBreakdown < 0)
                                {
                                    DownTimeBreakdown = 0;
                                }

                                //Performance
                                SummationOfSCTvsPP = GetSummationOfSCTvsPP(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID);
                                if (SummationOfSCTvsPP <= 0)
                                {
                                    SummationOfSCTvsPP = 0;
                                }

                                //ROPLosses = GetDownTimeLosses(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID, "ROP");
                            }
                            catch (Exception e)
                            {

                            }

                            //Quality
                            try
                            {
                                ScrapQtyTime = GetScrapQtyTimeOfWO(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID);
                                if (ScrapQtyTime < 0)
                                {
                                    ScrapQtyTime = 0;
                                }
                                ReWOTime = GetScrapQtyTimeOfRWO(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID);
                                if (ReWOTime < 0)
                                {
                                    ReWOTime = 0;
                                }
                            }
                            catch (Exception e)
                            {

                            }
                            //Take care when using Available Time in Calculation of OEE and Stuff.
                            //if (TimeType == "GodHours")
                            //{
                            //    AvailableTime = AvailableTime = 24 * 60; //24Hours to Minutes;
                            //}

                            OperatingTime = green;

                            //To get Top 5 Losses for this WC
                            string todayAsCorrectedDate = UsedDateForExcel.ToString("yyyy-MM-dd");
                            DataTable DTLosses = new DataTable();
                            DTLosses.Columns.Add("lossCodeID", typeof(int));
                            DTLosses.Columns.Add("LossDuration", typeof(int));


                            using (i_facility_shaktiEntities1 dbLoss = new i_facility_shaktiEntities1())
                            {
                                var lossData = dbLoss.tbllossofentries.Where(m => m.CorrectedDate == todayAsCorrectedDate && m.MachineID == MachineID).ToList();
                                foreach (var row in lossData)
                                {
                                    int lossCodeID = Convert.ToInt32(row.MessageCodeID);
                                    DateTime startDate = Convert.ToDateTime(row.StartDateTime);
                                    DateTime endDate = Convert.ToDateTime(row.EndDateTime);
                                    int duration = Convert.ToInt32(endDate.Subtract(startDate).TotalMinutes);

                                    DataRow dr = DTLosses.Select("lossCodeID= '" + lossCodeID + "'").FirstOrDefault(); // finds all rows with id==2 and selects first or null if haven't found any
                                    if (dr != null)
                                    {
                                        int LossDurationPrev = Convert.ToInt32(dr["LossDuration"]); //get lossduration and update it.
                                        dr["LossDuration"] = (LossDurationPrev + duration);
                                    }
                                    //}
                                    else
                                    {
                                        DTLosses.Rows.Add(lossCodeID, duration);
                                    }
                                }
                            }
                            DataTable DTLossesTop5 = DTLosses.Clone();
                            //get only the rows you want
                            DataRow[] results = DTLosses.Select("", "LossDuration DESC");
                            //populate new destination table
                            if (DTLosses.Rows.Count > 0)
                            {
                                int num = DTLosses.Rows.Count;
                                for (var iDT = 0; iDT < num; iDT++)
                                {
                                    if (results[iDT] != null)
                                    {
                                        DTLossesTop5.ImportRow(results[iDT]);
                                    }
                                    else
                                    {
                                        DTLossesTop5.Rows.Add(0, 0);
                                    }
                                    if (iDT == 4)
                                    {
                                        break;
                                    }
                                }
                                if (num < 5)
                                {
                                    for (var iDT = num; iDT < 5; iDT++)
                                    {
                                        DTLossesTop5.Rows.Add(0, 0);
                                    }
                                }
                            }
                            else
                            {
                                for (var iDT = 0; iDT < 5; iDT++)
                                {
                                    DTLossesTop5.Rows.Add(0, 0);
                                }
                            }
                            //Gather LossValues
                            string lossCode1, lossCode2, lossCode3, lossCode4, lossCode5 = null;
                            int lossCodeVal1, lossCodeVal2, lossCodeVal3, lossCodeVal4, lossCodeVal5 = 0;

                            lossCode1 = Convert.ToString(DTLossesTop5.Rows[0][0]);
                            lossCode2 = Convert.ToString(DTLossesTop5.Rows[1][0]);
                            lossCode3 = Convert.ToString(DTLossesTop5.Rows[2][0]);
                            lossCode4 = Convert.ToString(DTLossesTop5.Rows[3][0]);
                            lossCode5 = Convert.ToString(DTLossesTop5.Rows[4][0]);
                            lossCodeVal1 = Convert.ToInt32(DTLossesTop5.Rows[0][1]);
                            lossCodeVal2 = Convert.ToInt32(DTLossesTop5.Rows[1][1]);
                            lossCodeVal3 = Convert.ToInt32(DTLossesTop5.Rows[2][1]);
                            lossCodeVal4 = Convert.ToInt32(DTLossesTop5.Rows[3][1]);
                            lossCodeVal5 = Convert.ToInt32(DTLossesTop5.Rows[4][1]);

                            //Gather Plant,Shop,Cell for WC.
                            //int PlantID = 0, ShopID = 0, CellID = 0;
                            string PlantIDS = null, ShopIDS = null, CellIDS = null;
                            int value;
                            var WCData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).FirstOrDefault();
                            string TempVal = WCData.PlantID.ToString();
                            if (int.TryParse(TempVal, out value))
                            {
                                PlantIDS = value.ToString();
                            }

                            TempVal = WCData.ShopID.ToString();
                            if (int.TryParse(TempVal, out value))
                            {
                                ShopIDS = value.ToString();
                            }

                            TempVal = WCData.CellID.ToString();
                            if (int.TryParse(TempVal, out value))
                            {
                                CellIDS = value.ToString();
                            }

                            //Now insert into table
                            using (MsqlConnection mcInsertRows = new MsqlConnection())
                            {
                                try
                                {
                                    mcInsertRows.open();
                                    SqlCommand cmdInsertRows = new SqlCommand("INSERT INTO tbloeedashboardvariables (PlantID,ShopID,CellID,WCID,StartDate,EndDate,MinorLosses,Blue,Green,SettingTime,ROALossess,DownTimeBreakdown,SummationOfSCTvsPP,ScrapQtyTime,ReWOTime,Loss1Name,Loss1Value,Loss2Name,Loss2Value,Loss3Name,Loss3Value,Loss4Name,Loss4Value,Loss5Name,Loss5Value,CreatedOn,CreatedBy,IsDeleted)VALUES('" + PlantIDS + "','" + ShopIDS + "','" + CellIDS + "','" + MachineID + "','" + UsedDateForExcel.ToString("yyyy-MM-dd") + "','" + UsedDateForExcel.ToString("yyyy-MM-dd") + "','" + Math.Round(MinorLosses / 60, 2) + "','" + Math.Round(blue / 60, 2) + "','" + Math.Round(green / 60, 2) + "','" + Math.Round(SettingTime, 2) + "','" + Math.Round(ROALossess, 2) + "','" + Math.Round(DownTimeBreakdown, 2) + "','" + Math.Round(SummationOfSCTvsPP, 2) + "','" + Math.Round(ScrapQtyTime, 2) + "','" + Math.Round(ReWOTime, 2) + "','" + lossCode1 + "','" + lossCodeVal1 + "','" + lossCode2 + "','" + lossCodeVal2 + "','" + lossCode3 + "','" + lossCodeVal3 + "','" + lossCode4 + "','" + lossCodeVal4 + "','" + lossCode5 + "','" + lossCodeVal5 + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + 1 + "','" + 0 + "');", mcInsertRows.msqlConnection);
                                    cmdInsertRows.ExecuteNonQuery();
                                }
                                catch (Exception e)
                                {
                                }
                                finally
                                {
                                    mcInsertRows.close();
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        //IntoFile("MacID: " + MachineID + e.ToString());
                    }
                }
                UsedDateForExcel = UsedDateForExcel.AddDays(+1);
            }
            #endregion

        }

        public double GetMinorLosses(string CorrectedDate, int MachineID, string Colour)
        {
            DateTime currentdate = Convert.ToDateTime(CorrectedDate);
            string dateString = currentdate.ToString("yyyy-MM-dd");

            double minorloss = 0;

            //int count = 0;
            //var Data = db.tbldailyprodstatus.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.CorrectedDate == CorrectedDate).OrderBy(m => m.StartTime).ToList();
            //foreach (var row in Data)
            //{
            //    if (row.ColorCode == "yellow")
            //    {
            //        count++;
            //    }
            //    else
            //    {
            //        if (count > 0 && count < 2)
            //        {
            //            minorloss += count;
            //            count = 0;

            //        }
            //        count = 0;
            //    }
            //}

            using (i_facility_shaktiEntities1 dbLoss = new i_facility_shaktiEntities1())
            {
                DateTime st = Convert.ToDateTime(dateString);
                var MinorLossSummation = dbLoss.tblmodes.Where(m => m.MachineID == MachineID && m.CorrectedDate == st && m.ColorCode == Colour && m.DurationInSec < 120).Sum(m => m.DurationInSec);
                minorloss = Convert.ToDouble(MinorLossSummation);
            }
            return minorloss;
        }
        public double GetOPIDleBreakDown(string CorrectedDate, int MachineID, string Colour)
        {
            DateTime currentdate = Convert.ToDateTime(CorrectedDate);
            string datetime = currentdate.ToString("yyyy-MM-dd");

            double count = 0;
            //MsqlConnection mc = new MsqlConnection();
            //mc.open();
            ////operating
            //mc.open();
            //String query1 = "SELECT count(ID) From tbldailyprodstatus WHERE CorrectedDate='" + CorrectedDate + "' AND MachineID=" + MachineID + " AND ColorCode='" + Colour + "'";
            //SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
            //DataTable OP = new DataTable();
            //da1.Fill(OP);
            //mc.close();
            //if (OP.Rows.Count != 0)
            //{
            //    count[0] = Convert.ToInt32(OP.Rows[0][0]);
            //}

            using (i_facility_shaktiEntities1 dbLoss = new i_facility_shaktiEntities1())
            {
                DateTime st = Convert.ToDateTime(CorrectedDate);
                var blah = dbLoss.tblmodes.Where(m => m.MachineID == MachineID && m.CorrectedDate == st && m.ColorCode == Colour).Sum(m => m.DurationInSec);
                count = Convert.ToDouble(blah);
            }
            return count;
        }

        public double GetSettingTime(string UsedDateForExcel, int MachineID)
        {
            double settingTime = 0;
            int setupid = 0;
            string settingString = "Setup";
            var setupiddata = db.tbllossescodes.Where(m => m.MessageType.Contains(settingString)).FirstOrDefault();
            if (setupiddata != null)
            {
                setupid = setupiddata.LossCodeID;
            }
            else
            {
                //Session["Error"] = "Unable to get Setup's ID";
                return -1;
            }
            //getting all setup's sublevels ids.
            using (i_facility_shaktiEntities1 dbLoss = new i_facility_shaktiEntities1())
            {
                var SettingIDs = dbLoss.tbllossescodes.Where(m => m.LossCodesLevel1ID == setupid || m.LossCodesLevel2ID == setupid).Select(m => m.LossCodeID).ToList();


                //settingTime = (from row in db.tbllossofentries
                //               where  row.CorrectedDate == UsedDateForExcel && row.MachineID == MachineID );


                var SettingData = dbLoss.tbllossofentries.Where(m => SettingIDs.Contains(m.MessageCodeID) && m.MachineID == MachineID && m.CorrectedDate == UsedDateForExcel && m.DoneWithRow == 1).ToList();
                foreach (var row in SettingData)
                {
                    DateTime startTime = Convert.ToDateTime(row.StartDateTime);
                    DateTime endTime = Convert.ToDateTime(row.EndDateTime);
                    settingTime += endTime.Subtract(startTime).TotalMinutes;
                }
            }
            return settingTime;
        }
        public double GetDownTimeLosses(string UsedDateForExcel, int MachineID, string contribute)
        {
            double LossTime = 0;
            //string contribute = "ROA";
            //getting all ROA sublevels ids. Only those of IDLE.

            using (i_facility_shaktiEntities1 dbLoss = new i_facility_shaktiEntities1())
            {
                var SettingIDs = dbLoss.tbllossescodes.Where(m => m.ContributeTo == contribute && (m.MessageType != "PM" || m.MessageType != "BREAKDOWN")).Select(m => m.LossCodeID).ToList();

                var SettingData = dbLoss.tbllossofentries.Where(m => SettingIDs.Contains(m.MessageCodeID) && m.MachineID == MachineID && m.CorrectedDate == UsedDateForExcel && m.DoneWithRow == 1).ToList();
                foreach (var row in SettingData)
                {
                    DateTime startTime = Convert.ToDateTime(row.StartDateTime);
                    DateTime endTime = Convert.ToDateTime(row.EndDateTime);
                    LossTime += endTime.Subtract(startTime).TotalMinutes;
                }
            }
            return LossTime;
        }
        public double GetDownTimeBreakdown(string UsedDateForExcel, int MachineID)
        {
            //if (MachineID == 18)
            //{
            //}
            double LossTime = 0;
            using (i_facility_shaktiEntities1 dbLoss = new i_facility_shaktiEntities1())
            {
                var BreakdownData = dbLoss.tblbreakdowns.Where(m => m.MachineID == MachineID && m.CorrectedDate == UsedDateForExcel && m.DoneWithRow == 1).ToList();
                foreach (var row in BreakdownData)
                {
                    if ((Convert.ToString(row.EndTime) == null) || row.EndTime == null)
                    {
                        //do nothing
                    }
                    else
                    {
                        DateTime startTime = Convert.ToDateTime(row.StartTime);
                        DateTime endTime = Convert.ToDateTime(row.EndTime);
                        LossTime += endTime.Subtract(startTime).TotalMinutes;
                    }
                }
            }
            return LossTime;
        }

        public double GetSummationOfSCTvsPP(string UsedDateForExcel, int MachineID)
        {
            double SummationofTime = 0;

            #region OLD 2017-02-10
            //var PartsData = db.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && m.isWorkOrder == 0 && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0)).ToList();
            //if (PartsData.Count == 0)
            //{
            //    //return -1;
            //}
            //foreach (var row in PartsData)
            //{
            //    string partno = row.PartNo;
            //    string operationno = row.OperationNo;
            //    int totalpartproduced = Convert.ToInt32(row.Delivered_Qty) + Convert.ToInt32(row.Rej_Qty);
            //    Double stdCuttingTime = 0;
            //    var stdcuttingTimeData = db.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0 && m.OpNo == operationno && m.PartNo == partno).FirstOrDefault();
            //    if (stdcuttingTimeData != null)
            //    {
            //        string stdcuttingvalString = Convert.ToString(stdcuttingTimeData.StdCuttingTime);
            //        Double stdcuttingval = 0;
            //        if (double.TryParse(stdcuttingvalString, out stdcuttingval))
            //        {
            //            stdcuttingval = stdcuttingval;
            //        }

            //        string Unit = Convert.ToString(stdcuttingTimeData.StdCuttingTimeUnit);
            //        if (Unit == "Hrs")
            //        {
            //            stdCuttingTime = stdcuttingval * 60;
            //        }
            //        else //Unit is Minutes
            //        {
            //            stdCuttingTime = stdcuttingval;
            //        }
            //    }
            //    SummationofTime += stdCuttingTime * totalpartproduced;
            //}

            ////To Extract MultiWorkOrder Cutting Time
            //PartsData = db.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && m.IsMultiWO == 1 && m.isWorkOrder == 0 && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0)).ToList();
            //if (PartsData.Count == 0)
            //{
            //    return SummationofTime;
            //}
            //foreach (var row in PartsData)
            //{
            //    int HMIID = row.HMIID;

            //    var DataInMultiwoSelection = db.tbl_multiwoselection.Where(m => m.HMIID == HMIID).ToList();
            //    foreach (var rowData in DataInMultiwoSelection)
            //    {
            //        string partno = rowData.PartNo;
            //        string operationno = rowData.OperationNo;
            //        int totalpartproduced = Convert.ToInt32(rowData.DeliveredQty) + Convert.ToInt32(rowData.ScrapQty);
            //        int stdCuttingTime = 0;
            //        var stdcuttingTimeData = db.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0 && m.OpNo == operationno && m.PartNo == partno).FirstOrDefault();
            //        if (stdcuttingTimeData != null)
            //        {
            //            int stdcuttingval = Convert.ToInt32(stdcuttingTimeData.StdCuttingTime);
            //            string Unit = Convert.ToString(stdcuttingTimeData.StdCuttingTimeUnit);
            //            if (Unit == "Hrs")
            //            {
            //                stdCuttingTime = stdcuttingval * 60;
            //            }
            //            else //Unit is Minutes
            //            {
            //                stdCuttingTime = stdcuttingval;
            //            }
            //        }
            //        SummationofTime += stdCuttingTime * totalpartproduced;
            //    }
            //}

            #endregion

            #region OLD 2017-02-10
            //List<string> OccuredWOs = new List<string>();
            ////To Extract Single WorkOrder Cutting Time
            //using (i_facility_shaktiEntities1 dbhmi = new i_facility_shaktiEntities1())
            //{
            //    var PartsDataAll = dbhmi.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && m.IsMultiWO == 0 && m.isWorkOrder == 0 && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0)).OrderByDescending(m => m.HMIID).ToList();
            //    if (PartsDataAll.Count == 0)
            //    {
            //        //return SummationofTime;
            //    }
            //    foreach (var row in PartsDataAll)
            //    {
            //        string partNo = row.PartNo;
            //        string woNo = row.Work_Order_No;
            //        string opNo = row.OperationNo;

            //        string occuredwo = partNo + "," + woNo + "," + opNo;
            //        if (!OccuredWOs.Contains(occuredwo))
            //        {
            //            OccuredWOs.Add(occuredwo);
            //            var PartsData = dbhmi.tblhmiscreens.
            //                Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && m.IsMultiWO == 0
            //                    && m.isWorkOrder == 0 && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0)
            //                    && m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == opNo).
            //                    OrderByDescending(m => m.HMIID).ToList();

            //            int totalpartproduced = 0;
            //            int ProcessQty = 0, DeliveredQty = 0;
            //            //Decide to select deliveredQty & ProcessedQty lastest(from HMI or tblmultiWOselection)

            //            #region new code

            //            //here 1st get latest of delivered and processed among row in tblHMIScreen & tblmulitwoselection
            //            int isHMIFirst = 2; //default NO History for that wo,pn,on

            //            var mulitwoData = dbhmi.tbl_multiwoselection.Where(m => m.WorkOrder == woNo && m.PartNo == partNo && m.OperationNo == opNo).OrderByDescending(m => m.MultiWOID).Take(1).ToList();
            //            //var hmiData = db.tblhmiscreens.Where(m => m.Work_Order_No == WONo && m.PartNo == Part && m.OperationNo == Operation && m.isWorkInProgress == 0).OrderByDescending(m => m.HMIID).Take(1).ToList();

            //            //Note: we are in this loop => hmiscreen table data is Available

            //            if (mulitwoData.Count > 0)
            //            {
            //                isHMIFirst = 1;
            //            }
            //            else if (PartsData.Count > 0)
            //            {
            //                isHMIFirst = 0;
            //            }
            //            else if (PartsData.Count > 0 && mulitwoData.Count > 0) //we both Dates now check for greatest amongst
            //            {
            //                int hmiIDFromMulitWO = row.HMIID;
            //                DateTime multiwoDateTime = Convert.ToDateTime(from r in db.tblhmiscreens
            //                                                              where r.HMIID == hmiIDFromMulitWO
            //                                                              select r.Time
            //                                                              );
            //                DateTime hmiDateTime = Convert.ToDateTime(row.Time);

            //                if (Convert.ToInt32(multiwoDateTime.Subtract(hmiDateTime).TotalSeconds) > 0)
            //                {
            //                    isHMIFirst = 1; // multiwoDateTime is greater than hmitable datetime
            //                }
            //                else
            //                {
            //                    isHMIFirst = 0;
            //                }
            //            }
            //            if (isHMIFirst == 1)
            //            {
            //                string delivString = Convert.ToString(mulitwoData[0].DeliveredQty);
            //                int.TryParse(delivString, out DeliveredQty);
            //                string processString = Convert.ToString(mulitwoData[0].ProcessQty);
            //                int.TryParse(processString, out ProcessQty);

            //            }
            //            else if (isHMIFirst == 0)//Take Data from HMI
            //            {
            //                string delivString = Convert.ToString(PartsData[0].Delivered_Qty);
            //                int.TryParse(delivString, out DeliveredQty);
            //                string processString = Convert.ToString(PartsData[0].ProcessQty);
            //                int.TryParse(processString, out ProcessQty);
            //            }

            //            #endregion

            //            //totalpartproduced = DeliveredQty + ProcessQty;
            //            totalpartproduced = DeliveredQty;

            //            #region InnerLogic Common for both ways(HMI or tblmultiWOselection)

            //            double stdCuttingTime = 0;
            //            var stdcuttingTimeData = db.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0 && m.OpNo == opNo && m.PartNo == partNo).FirstOrDefault();
            //            if (stdcuttingTimeData != null)
            //            {
            //                double stdcuttingval = Convert.ToDouble(stdcuttingTimeData.StdCuttingTime);
            //                string Unit = Convert.ToString(stdcuttingTimeData.StdCuttingTimeUnit);
            //                if (Unit == "Hrs")
            //                {
            //                    stdCuttingTime = stdcuttingval * 60;
            //                }
            //                else //Unit is Minutes
            //                {
            //                    stdCuttingTime = stdcuttingval;
            //                }
            //            }
            //            #endregion

            //            SummationofTime += stdCuttingTime * totalpartproduced;
            //        }
            //    }
            //}
            ////To Extract Multi WorkOrder Cutting Time
            //using (i_facility_shaktiEntities1 dbhmi = new i_facility_shaktiEntities1())
            //{
            //    var PartsDataAll = dbhmi.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && m.IsMultiWO == 1 && m.isWorkOrder == 0 && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0)).ToList();
            //    if (PartsDataAll.Count == 0)
            //    {
            //        //return SummationofTime;
            //    }
            //    foreach (var row in PartsDataAll)
            //    {
            //        string partNo = row.PartNo;
            //        string woNo = row.Work_Order_No;
            //        string opNo = row.OperationNo;

            //        string occuredwo = partNo + "," + woNo + "," + opNo;
            //        if (!OccuredWOs.Contains(occuredwo))
            //        {
            //            OccuredWOs.Add(occuredwo);
            //            var PartsData = dbhmi.tblhmiscreens.
            //                Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && m.IsMultiWO == 0
            //                    && m.isWorkOrder == 0 && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0)
            //                    && m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == opNo).
            //                    OrderByDescending(m => m.HMIID).ToList();

            //            int totalpartproduced = 0;
            //            int ProcessQty = 0, DeliveredQty = 0;
            //            //Decide to select deliveredQty & ProcessedQty lastest(from HMI or tblmultiWOselection)

            //            #region new code

            //            //here 1st get latest of delivered and processed among row in tblHMIScreen & tblmulitwoselection
            //            int isHMIFirst = 2; //default NO History for that wo,pn,on

            //            var mulitwoData = dbhmi.tbl_multiwoselection.Where(m => m.WorkOrder == woNo && m.PartNo == partNo && m.OperationNo == opNo).OrderByDescending(m => m.MultiWOID).Take(1).ToList();
            //            //var hmiData = db.tblhmiscreens.Where(m => m.Work_Order_No == WONo && m.PartNo == Part && m.OperationNo == Operation && m.isWorkInProgress == 0).OrderByDescending(m => m.HMIID).Take(1).ToList();

            //            //Note: we are in this loop => hmiscreen table data is Available

            //            if (mulitwoData.Count > 0)
            //            {
            //                isHMIFirst = 1;
            //            }
            //            else if (PartsData.Count > 0)
            //            {
            //                isHMIFirst = 0;
            //            }
            //            else if (PartsData.Count > 0 && mulitwoData.Count > 0) //we have both Dates now check for greatest amongst
            //            {
            //                int hmiIDFromMulitWO = row.HMIID;
            //                DateTime multiwoDateTime = Convert.ToDateTime(from r in db.tblhmiscreens
            //                                                              where r.HMIID == hmiIDFromMulitWO
            //                                                              select r.Time
            //                                                              );
            //                DateTime hmiDateTime = Convert.ToDateTime(row.Time);

            //                if (Convert.ToInt32(multiwoDateTime.Subtract(hmiDateTime).TotalSeconds) > 0)
            //                {
            //                    isHMIFirst = 1; // multiwoDateTime is greater than hmitable datetime
            //                }
            //                else
            //                {
            //                    isHMIFirst = 0;
            //                }
            //            }

            //            if (isHMIFirst == 1)
            //            {
            //                string delivString = Convert.ToString(mulitwoData[0].DeliveredQty);
            //                int.TryParse(delivString, out DeliveredQty);
            //                string processString = Convert.ToString(mulitwoData[0].ProcessQty);
            //                int.TryParse(processString, out ProcessQty);
            //            }
            //            else if (isHMIFirst == 0) //Take Data from HMI
            //            {
            //                string delivString = Convert.ToString(PartsData[0].Delivered_Qty);
            //                int.TryParse(delivString, out DeliveredQty);
            //                string processString = Convert.ToString(PartsData[0].ProcessQty);
            //                int.TryParse(processString, out ProcessQty);
            //            }

            //            #endregion

            //            //totalpartproduced = DeliveredQty + ProcessQty;
            //            totalpartproduced = DeliveredQty;
            //            #region InnerLogic Common for both ways(HMI or tblmultiWOselection)

            //            double stdCuttingTime = 0;
            //            var stdcuttingTimeData = db.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0 && m.OpNo == opNo && m.PartNo == partNo).FirstOrDefault();
            //            if (stdcuttingTimeData != null)
            //            {
            //                double stdcuttingval = Convert.ToDouble(stdcuttingTimeData.StdCuttingTime);
            //                string Unit = Convert.ToString(stdcuttingTimeData.StdCuttingTimeUnit);
            //                if (Unit == "Hrs")
            //                {
            //                    stdCuttingTime = stdcuttingval * 60;
            //                }
            //                else //Unit is Minutes
            //                {
            //                    stdCuttingTime = stdcuttingval;
            //                }
            //            }
            //            #endregion

            //            SummationofTime += stdCuttingTime * totalpartproduced;
            //        }
            //    }
            //}
            #endregion

            //new Code 2017-03-08
            using (i_facility_shaktiEntities1 dbhmi = new i_facility_shaktiEntities1())
            {
                var PartsDataAll = dbhmi.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && m.isWorkOrder == 0 && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0)).OrderByDescending(m => m.PartNo).ThenByDescending(m => m.OperationNo).ToList();
                if (PartsDataAll.Count == 0)
                {
                    //return SummationofTime;
                }
                foreach (var row in PartsDataAll)
                {
                    if (row.IsMultiWO == 0)
                    {
                        string partNo = row.PartNo;
                        string woNo = row.Work_Order_No;
                        string opNo = row.OperationNo;
                        int DeliveredQty = 0;
                        DeliveredQty = Convert.ToInt32(row.Delivered_Qty);
                        #region InnerLogic Common for both ways(HMI or tblmultiWOselection)
                        double stdCuttingTime = 0;
                        var stdcuttingTimeData = db.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0 && m.OpNo == opNo && m.PartNo == partNo).FirstOrDefault();
                        if (stdcuttingTimeData != null)
                        {
                            double stdcuttingval = Convert.ToDouble(stdcuttingTimeData.StdCuttingTime);
                            string Unit = Convert.ToString(stdcuttingTimeData.StdCuttingTimeUnit);
                            if (Unit == "Hrs")
                            {
                                stdCuttingTime = stdcuttingval * 60;
                            }
                            else if (Unit == "Sec") //Unit is Minutes
                            {
                                stdCuttingTime = stdcuttingval / 60;
                            }
                            else
                            {
                                stdCuttingTime = stdcuttingval;
                            }
                            // no need of else , its already in minutes
                        }
                        #endregion
                        //MessageBox.Show("CuttingTime " + stdCuttingTime +" DeliveredQty " +DeliveredQty );
                        SummationofTime += stdCuttingTime * DeliveredQty;
                        //MessageBox.Show("Single" + SummationofTime);
                    }
                    else
                    {
                        int hmiid = row.HMIID;
                        var multiWOData = dbhmi.tbl_multiwoselection.Where(m => m.HMIID == hmiid).ToList();
                        foreach (var rowMulti in multiWOData)
                        {
                            string partNo = rowMulti.PartNo;
                            string opNo = rowMulti.OperationNo;
                            int DeliveredQty = 0;
                            DeliveredQty = Convert.ToInt32(rowMulti.DeliveredQty);
                            #region
                            double stdCuttingTime = 0;
                            var stdcuttingTimeData = db.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0 && m.OpNo == opNo && m.PartNo == partNo).FirstOrDefault();
                            if (stdcuttingTimeData != null)
                            {
                                double stdcuttingval = Convert.ToDouble(stdcuttingTimeData.StdCuttingTime);
                                string Unit = Convert.ToString(stdcuttingTimeData.StdCuttingTimeUnit);
                                if (Unit == "Hrs")
                                {
                                    stdCuttingTime = stdcuttingval * 60;
                                }
                                else if (Unit == "Sec") //Unit is Minutes
                                {
                                    stdCuttingTime = stdcuttingval / 60;
                                }
                                else
                                {
                                    stdCuttingTime = stdcuttingval;
                                }

                            }
                            #endregion
                            //MessageBox.Show("CuttingTime " + stdCuttingTime + " DeliveredQty " + DeliveredQty);
                            SummationofTime += stdCuttingTime * DeliveredQty;
                            //MessageBox.Show("Multi" + SummationofTime);
                        }
                    }
                    //MessageBox.Show("" + SummationofTime);
                }
            }
            return SummationofTime;
        }

        public double GetScrapQtyTimeOfWO(string UsedDateForExcel, int MachineID)
        {
            double SQT = 0;
            using (i_facility_shaktiEntities1 dbhmi = new i_facility_shaktiEntities1())
            {
                var PartsData = dbhmi.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0) && m.isWorkOrder == 0).ToList();
                foreach (var row in PartsData)
                {
                    string partno = row.PartNo;
                    string operationno = row.OperationNo;
                    int scrapQty = 0;
                    int DeliveredQty = 0;
                    string scrapQtyString = Convert.ToString(row.Rej_Qty);
                    string DeliveredQtyString = Convert.ToString(row.Delivered_Qty);
                    string x = scrapQtyString;
                    int value;
                    if (int.TryParse(x, out value))
                    {
                        scrapQty = value;
                    }
                    x = DeliveredQtyString;
                    if (int.TryParse(x, out value))
                    {
                        DeliveredQty = value;
                    }

                    DateTime startTime = Convert.ToDateTime(row.Date);
                    DateTime endTime = Convert.ToDateTime(row.Time);
                    //Double WODuration = endTimeTemp.Subtract(startTime).TotalMinutes;
                    Double WODuration = GetGreen(UsedDateForExcel, startTime, endTime, MachineID);

                    if ((scrapQty + DeliveredQty) == 0)
                    {
                        SQT += 0;
                    }
                    else
                    {
                        SQT += ((WODuration / 60) / (scrapQty + DeliveredQty)) * scrapQty;
                    }
                }
            }
            return SQT;
        }
        //GOD
        public double GetScrapQtyTimeOfRWO(string UsedDateForExcel, int MachineID)
        {
            double SQT = 0;
            using (i_facility_shaktiEntities1 dbhmi = new i_facility_shaktiEntities1())
            {
                var PartsData = dbhmi.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0) && m.isWorkOrder == 1).ToList();
                foreach (var row in PartsData)
                {
                    string partno = row.PartNo;
                    string operationno = row.OperationNo;
                    int scrapQty = Convert.ToInt32(row.Rej_Qty);
                    int DeliveredQty = Convert.ToInt32(row.Delivered_Qty);
                    DateTime startTime = Convert.ToDateTime(row.Date);
                    DateTime endTime = Convert.ToDateTime(row.Time);
                    Double WODuration = GetGreen(UsedDateForExcel, startTime, endTime, MachineID);

                    //Double WODuration = endTime.Subtract(startTime).TotalMinutes;
                    //For Availability Loss
                    //double Settingtime = GetSetupForReworkLoss(UsedDateForExcel, startTime, endTime, MachineID);
                    //double green = GetOT(UsedDateForExcel, startTime, endTime, MachineID);
                    //double DownTime = GetDownTimeForReworkLoss(UsedDateForExcel, startTime, endTime, MachineID, "ROA");
                    //double BreakdownTime = GetBreakDownTimeForReworkLoss(UsedDateForExcel, startTime, endTime, MachineID);
                    //double AL = DownTime + BreakdownTime + Settingtime;

                    //For Performance Loss
                    //double downtimeROP = GetDownTimeForReworkLoss(UsedDateForExcel, startTime, endTime, MachineID, "ROP");
                    //double minorlossWO = GetMinorLossForReworkLoss(UsedDateForExcel, startTime, endTime, MachineID, "yellow");
                    //double PL = downtimeROP + minorlossWO;

                    SQT += (WODuration / 60);
                }
            }
            return SQT;
        }
        //Output in Seconds
        public double GetGreen(string UsedDateForExcel, DateTime StartTime, DateTime EndTime, int MachineID)
        {
            double settingTime = 0;

            DataTable lossesData = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                //String query1 = "SELECT Sum(DurationInSec) From tblmode WHERE MachineID = '" + MachineID + "' and CorrectedDate = '" + UsedDateForExcel + "' and ColorCode = 'green'"
                //    + " and ( StartTime >= '" + WOstarttimeDate + "' and EndTime <= '" + WOendtimeDate + "' )";

                String query1 = "SELECT StartTime,EndTime,ModeID From tblmode WHERE MachineID = '" + MachineID + "' and CorrectedDate = '" + UsedDateForExcel + "' and ColorCode = 'green'  and"
                   + "( StartTime <= '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( ( EndTime > '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( EndTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' or   EndTime > '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' )  ) ) or "
                   + " ( StartTime > '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( StartTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' ) ))";

                SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                da1.Fill(lossesData);
                mc.close();
                //if (lossesData.Rows.Count > 0)
                //{
                //    //settingTime = Convert.ToDouble(lossesData.Rows[0][0]);
                //    settingTime = 0;
                //}

                for (int i = 0; i < lossesData.Rows.Count; i++)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[i][0])) && !string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[i][1])))
                    {
                        DateTime LStartDate = Convert.ToDateTime(lossesData.Rows[i][0]);
                        DateTime LEndDate = Convert.ToDateTime(lossesData.Rows[i][1]);
                        double IndividualDur = LEndDate.Subtract(LStartDate).TotalSeconds;

                        //Get Duration Based on start & end Time.

                        if (LStartDate < StartTime)
                        {
                            double StartDurationExtra = StartTime.Subtract(LStartDate).TotalSeconds;
                            IndividualDur -= StartDurationExtra;
                        }
                        if (LEndDate > EndTime)
                        {
                            double EndDurationExtra = LEndDate.Subtract(EndTime).TotalSeconds;
                            IndividualDur -= EndDurationExtra;
                        }
                        settingTime += IndividualDur;
                    }
                }
            }
            return settingTime;
        }
        //Output: In Seconds
        public double GetSettingTimeForWO(string UsedDateForExcel, int MachineID, DateTime StartTime, DateTime EndTime)
        {
            double settingTime = 0;
            int setupid = 0;
            string settingString = "Setup";
            var setupiddata = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.MessageType.Equals(settingString, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (setupiddata != null)
            {
                setupid = setupiddata.LossCodeID;
            }
            else
            {
                return -1;
            }

            // var s = string.Join(",", products.Where(p => p.ProductType == someType).Select(p => p.ProductId.ToString()));
            //getting all setup's sublevels ids.
            var SettingIDs = db.tbllossescodes
                                .Where(m => m.LossCodesLevel1ID == setupid)
                                .Select(m => m.LossCodeID).ToList()
                                .Distinct();
            string SettingIDsString = null;
            int j = 0;
            foreach (var row in SettingIDs)
            {
                if (j != 0)
                {
                    SettingIDsString += "," + Convert.ToInt32(row);
                }
                else
                {
                    SettingIDsString = Convert.ToInt32(row).ToString();
                }
                j++;
            }
            DataTable lossesData = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String query1 = "SELECT StartDateTime,EndDateTime,LossID From tbllossofentry WHERE MachineID = '" + MachineID + "' and CorrectedDate = '" + UsedDateForExcel + "' and MessageCodeID IN ( " + SettingIDsString + " ) and DoneWithRow = 1  and "
                    + "( StartDateTime <= '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( ( EndDateTime > '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( EndDateTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' or   EndDateTime > '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' )  ) ) or "
                    + " ( StartDateTime > '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( StartDateTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' ) ))";
                SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                da1.Fill(lossesData);
                mc.close();
            }

            for (int i = 0; i < lossesData.Rows.Count; i++)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[i][0])) && !string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[i][1])))
                {
                    DateTime LStartDate = Convert.ToDateTime(lossesData.Rows[i][0]);
                    DateTime LEndDate = Convert.ToDateTime(lossesData.Rows[i][1]);
                    double IndividualDur = LEndDate.Subtract(LStartDate).TotalSeconds;

                    //Get Duration Based on start & end Time.

                    if (LStartDate < StartTime)
                    {
                        double StartDurationExtra = StartTime.Subtract(LStartDate).TotalSeconds;
                        IndividualDur -= StartDurationExtra;
                    }
                    if (LEndDate > EndTime)
                    {
                        double EndDurationExtra = LEndDate.Subtract(EndTime).TotalSeconds;
                        IndividualDur -= EndDurationExtra;
                    }
                    settingTime += IndividualDur;
                }
            }

            return settingTime;
        }
        // Output: In Seconds
        public double GetSelfInsepectionForWO(string UsedDateForExcel, int MachineID, DateTime StartTime, DateTime EndTime)
        {
            double SelfInspectionTime = 0;
            int SelfInspectionid = 112;

            DataTable lossesData = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String query1 = "SELECT StartDateTime,EndDateTime,LossID From tbllossofentry WHERE MachineID = '" + MachineID + "' and CorrectedDate = '" + UsedDateForExcel + "' and MessageCodeID IN ( " + SelfInspectionid + " ) and DoneWithRow = 1  and "
                    + "( StartDateTime <= '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( ( EndDateTime > '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( EndDateTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' or   EndDateTime > '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' )  ) ) or "
                    + " ( StartDateTime > '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( StartDateTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' ) ))";
                SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                da1.Fill(lossesData);
                mc.close();
            }

            for (int i = 0; i < lossesData.Rows.Count; i++)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[i][0])) && !string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[i][1])))
                {
                    DateTime LStartDate = Convert.ToDateTime(lossesData.Rows[i][0]);
                    DateTime LEndDate = Convert.ToDateTime(lossesData.Rows[i][1]);
                    double IndividualDur = LEndDate.Subtract(LStartDate).TotalSeconds;

                    //Get Duration Based on start & end Time.

                    if (LStartDate < StartTime)
                    {
                        double StartDurationExtra = StartTime.Subtract(LStartDate).TotalSeconds;
                        IndividualDur -= StartDurationExtra;
                    }
                    if (LEndDate > EndTime)
                    {
                        double EndDurationExtra = LEndDate.Subtract(EndTime).TotalSeconds;
                        IndividualDur -= EndDurationExtra;
                    }
                    SelfInspectionTime += IndividualDur;
                }
            }

            return SelfInspectionTime;
        }
        //Output: In Seconds
        public double GetAllLossesTimeForWO(string UsedDateForExcel, int MachineID, DateTime StartTime, DateTime EndTime)
        {
            double AllLossesTime = 0;
            DataTable lossesData = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                // String query1 = "SELECT StartDateTime,EndDateTime,LossID From tbllossofentry WHERE MachineID = '" + MachineID + "' and CorrectedDate = '" + UsedDateForExcel + "' and DoneWithRow = 1  and "
                String query1 = "SELECT StartDateTime,EndDateTime,LossID From tbllossofentry WHERE MachineID = '" + MachineID + "' and DoneWithRow = 1  and "
                    + "( StartDateTime <= '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( ( EndDateTime > '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( EndDateTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' or   EndDateTime > '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' )  ) ) or "
                    + " ( StartDateTime > '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( StartDateTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' ) ))";
                SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                da1.Fill(lossesData);
                mc.close();
            }

            for (int i = 0; i < lossesData.Rows.Count; i++)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[i][0])) && !string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[i][1])))
                {
                    DateTime LStartDate = Convert.ToDateTime(lossesData.Rows[i][0]);
                    DateTime LEndDate = Convert.ToDateTime(lossesData.Rows[i][1]);
                    double IndividualDur = LEndDate.Subtract(LStartDate).TotalSeconds;

                    //Get Duration Based on start & end Time.

                    if (LStartDate < StartTime)
                    {
                        double StartDurationExtra = StartTime.Subtract(LStartDate).TotalSeconds;
                        IndividualDur -= StartDurationExtra;
                    }
                    if (LEndDate > EndTime)
                    {
                        double EndDurationExtra = LEndDate.Subtract(EndTime).TotalSeconds;
                        IndividualDur -= EndDurationExtra;
                    }
                    AllLossesTime += IndividualDur;
                }
            }

            return AllLossesTime;
        }
        //Output: In Seconds
        public double GetDownTimeBreakdownForWO(string UsedDateForExcel, int MachineID, DateTime StartTime, DateTime EndTime)
        {
            double BreakdownTime = 0;
            DataTable lossesData = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String query1 = "SELECT StartTime,EndTime From tblbreakdown WHERE MachineID = '" + MachineID + "' and CorrectedDate = '" + UsedDateForExcel + "' and DoneWithRow = 1  and "
                    + "( StartTime <= '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( ( EndTime > '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( EndTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' or   EndTime > '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' )  ) ) or "
                    + " ( StartTime > '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( StartTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' ) ))";
                SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                da1.Fill(lossesData);
                mc.close();
            }

            for (int i = 0; i < lossesData.Rows.Count; i++)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[i][0])) && !string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[i][1])))
                {
                    DateTime LStartDate = Convert.ToDateTime(lossesData.Rows[i][0]);
                    DateTime LEndDate = Convert.ToDateTime(lossesData.Rows[i][1]);
                    double IndividualDur = LEndDate.Subtract(LStartDate).TotalSeconds;

                    //Get Duration Based on start & end Time.

                    if (LStartDate < StartTime)
                    {
                        double StartDurationExtra = StartTime.Subtract(LStartDate).TotalSeconds;
                        IndividualDur -= StartDurationExtra;
                    }
                    if (LEndDate > EndTime)
                    {
                        double EndDurationExtra = LEndDate.Subtract(EndTime).TotalSeconds;
                        IndividualDur -= EndDurationExtra;
                    }
                    BreakdownTime += IndividualDur;
                }
            }

            return BreakdownTime;
        }
        //Output in Seconds
        public double GetBlue(string UsedDateForExcel, DateTime StartTime, DateTime EndTime, int MachineID)
        {
            double settingTime = 0;
            DataTable lossesData = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String query1 = "SELECT StartTime,EndTime,ModeID From tblmode WHERE MachineID = '" + MachineID + "' and CorrectedDate = '" + UsedDateForExcel + "' and ColorCode = 'blue'  and"
                   + "( StartTime <= '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( ( EndTime > '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( EndTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' or   EndTime > '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' )  ) ) or "
                   + " ( StartTime > '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( StartTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' ) ))";

                SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                da1.Fill(lossesData);
                mc.close();

                for (int i = 0; i < lossesData.Rows.Count; i++)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[i][0])) && !string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[i][1])))
                    {
                        DateTime LStartDate = Convert.ToDateTime(lossesData.Rows[i][0]);
                        DateTime LEndDate = Convert.ToDateTime(lossesData.Rows[i][1]);
                        double IndividualDur = LEndDate.Subtract(LStartDate).TotalSeconds;

                        //Get Duration Based on start & end Time.

                        if (LStartDate < StartTime)
                        {
                            double StartDurationExtra = StartTime.Subtract(LStartDate).TotalSeconds;
                            IndividualDur -= StartDurationExtra;
                        }
                        if (LEndDate > EndTime)
                        {
                            double EndDurationExtra = LEndDate.Subtract(EndTime).TotalSeconds;
                            IndividualDur -= EndDurationExtra;
                        }
                        settingTime += IndividualDur;
                    }
                }
            }
            return settingTime;
        }
        //Output: In Seconds
        public double GetMinorLossForWO(string UsedDateForExcel, int MachineID, DateTime StartTime, DateTime EndTime)
        {
            double MinorLoss = 0;
            DataTable lossesData = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String query1 = "SELECT StartTime,EndTime,ModeID From tblmode WHERE MachineID = '" + MachineID + "' and CorrectedDate = '" + UsedDateForExcel + "' and ColorCode = 'yellow' and  DurationInSec < 120 and"
                   + "( StartTime <= '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( ( EndTime > '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( EndTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' or   EndTime > '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' )  ) ) or "
                   + " ( StartTime > '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( StartTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' ) ))";

                SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                da1.Fill(lossesData);
                mc.close();
                for (int i = 0; i < lossesData.Rows.Count; i++)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[i][0])) && !string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[i][1])))
                    {
                        DateTime LStartDate = Convert.ToDateTime(lossesData.Rows[i][0]);
                        DateTime LEndDate = Convert.ToDateTime(lossesData.Rows[i][1]);
                        double IndividualDur = LEndDate.Subtract(LStartDate).TotalSeconds;

                        //Get Duration Based on start & end Time.

                        if (LStartDate < StartTime)
                        {
                            double StartDurationExtra = StartTime.Subtract(LStartDate).TotalSeconds;
                            IndividualDur -= StartDurationExtra;
                        }
                        if (LEndDate > EndTime)
                        {
                            double EndDurationExtra = LEndDate.Subtract(EndTime).TotalSeconds;
                            IndividualDur -= EndDurationExtra;
                        }
                        MinorLoss += IndividualDur;
                    }
                }
            }

            return MinorLoss;
        }

        public void CalWODataForYesterday(DateTime? StartDate, DateTime? EndDate)
        {
            DateTime fromdate = DateTime.Now.AddDays(-1), todate = DateTime.Now.AddDays(-1);
            if (StartDate != null && EndDate != null)
            {
                fromdate = Convert.ToDateTime(StartDate);
                todate = Convert.ToDateTime(EndDate);
            }

            DateTime UsedDateForExcel = Convert.ToDateTime(fromdate.ToString("yyyy-MM-dd"));
            double TotalDay = todate.Subtract(fromdate).TotalDays;

            #region
            for (int i = 0; i < TotalDay + 1; i++)
            {
                // 2017-03-08 
                string CorrectedDate = UsedDateForExcel.ToString("yyyy-MM-dd");
                //Normal WorkCenter
                var machineData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0).ToList();
                foreach (var macrow in machineData)
                {
                    int MachineID = macrow.MachineID;
                    //WorkOrder Data
                    try
                    {
                        ////For Testing Just Losses
                        //    int a = 0;
                        //if (a == 1)
                        //{
                        #region
                        var WODataPresent = db.tblworeports.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate).ToList();
                        if (WODataPresent.Count == 0)
                        {
                            var HMIData = db.tblhmiscreens.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate && (m.isWorkInProgress == 0 || m.isWorkInProgress == 1)).ToList();
                            foreach (var hmirow in HMIData)
                            {
                                //Constants from table

                                int hmiid = hmirow.HMIID;
                                string OperatorName = hmirow.OperatorDet;
                                string shift = hmirow.Shift;
                                string hmiCorretedDate = hmirow.CorrectedDate;
                                string type = hmirow.Prod_FAI;
                                string program = hmirow.Project;
                                int isHold = 0;
                                isHold = hmirow.IsHold;
                                DateTime StartTime = Convert.ToDateTime(hmirow.Date);
                                DateTime EndTime = Convert.ToDateTime(hmirow.Time);
                                //Values from Calculation
                                double cuttingTime = 0, settingTime = 0, selfInspection = 0, idle = 0, breakdown = 0, MinorLoss = 0, SummationSCTvsPP = 0;
                                double Blue = 0, ScrapQtyTime = 0, ReworkTime = 0;

                                cuttingTime = GetGreen(CorrectedDate, StartTime, EndTime, MachineID);
                                cuttingTime = Math.Round(cuttingTime / 60, 2);
                                settingTime = GetSettingTimeForWO(CorrectedDate, MachineID, StartTime, EndTime);
                                settingTime = Math.Round(settingTime / 60, 2);
                                selfInspection = GetSelfInsepectionForWO(CorrectedDate, MachineID, StartTime, EndTime);
                                selfInspection = Math.Round(selfInspection / 60, 2);
                                double TotalLosses = GetAllLossesTimeForWO(CorrectedDate, MachineID, StartTime, EndTime);
                                TotalLosses = Math.Round(TotalLosses / 60, 2);
                                idle = TotalLosses;
                                breakdown = GetDownTimeBreakdownForWO(CorrectedDate, MachineID, StartTime, EndTime);
                                breakdown = Math.Round(breakdown / 60, 2);
                                MinorLoss = GetMinorLossForWO(CorrectedDate, MachineID, StartTime, EndTime);
                                MinorLoss = Math.Round(MinorLoss / 60, 2);

                                Blue = GetBlue(CorrectedDate, StartTime, EndTime, MachineID);
                                Blue = Math.Round(Blue / 60, 2); bool isRework = false;
                                isRework = hmirow.isWorkOrder == 0 ? false : true;
                                if (isRework)
                                {
                                    ReworkTime = cuttingTime;
                                }

                                int isSingleWo = 0;
                                isSingleWo = hmirow.IsMultiWO;

                                if (isSingleWo == 0)
                                {
                                    #region singleWO
                                    string SplitWO = hmirow.SplitWO;

                                    try
                                    {
                                        string PartNo = hmirow.PartNo;
                                        string WONo = hmirow.Work_Order_No;
                                        string OpNo = hmirow.OperationNo;


                                        int targetQty = Convert.ToInt32(hmirow.Target_Qty);
                                        int deliveredQty = Convert.ToInt32(hmirow.Delivered_Qty);
                                        int rejectedQty = Convert.ToInt32(hmirow.Rej_Qty);
                                        if (rejectedQty > 0)
                                        {
                                            ScrapQtyTime = (cuttingTime / (rejectedQty + deliveredQty)) * rejectedQty;
                                        }

                                        int IsPF = 0;
                                        if (hmirow.isWorkInProgress == 1)
                                        {
                                            IsPF = 1;
                                        }

                                        //Constants From DB
                                        double stdCuttingTime = 0, stdMRWeight = 0;
                                        var StdWeightTime = db.tblmasterparts_st_sw.Where(m => m.PartNo == PartNo && m.OpNo == OpNo && m.IsDeleted == 0).FirstOrDefault();
                                        if (StdWeightTime != null)
                                        {
                                            string stdCuttingTimeString = null, stdMRWeightString = null;
                                            string stdCuttingTimeUnitString = null, stdMRWeightUnitString = null;
                                            stdCuttingTimeString = Convert.ToString(StdWeightTime.StdCuttingTime);
                                            stdMRWeightString = Convert.ToString(StdWeightTime.MaterialRemovedQty);
                                            stdCuttingTimeUnitString = Convert.ToString(StdWeightTime.StdCuttingTimeUnit);
                                            stdMRWeightUnitString = Convert.ToString(StdWeightTime.MaterialRemovedQtyUnit);

                                            double.TryParse(stdCuttingTimeString, out stdCuttingTime);
                                            double.TryParse(stdMRWeightString, out stdMRWeight);

                                            if (stdCuttingTimeUnitString == "Hrs")
                                            {
                                                stdCuttingTime = stdCuttingTime * 60;
                                            }
                                            else if (stdCuttingTimeUnitString == "Sec") //Unit is Minutes
                                            {
                                                stdCuttingTime = stdCuttingTime / 60;
                                            }

                                            SummationSCTvsPP = stdCuttingTime * deliveredQty;



                                            // no need of else its already in minutes
                                        }

                                        double totalNCCuttingTime = deliveredQty * stdCuttingTime;
                                        //??
                                        string MRReason = null;

                                        double WOEfficiency = 0;
                                        if (cuttingTime != 0)
                                        {
                                            WOEfficiency = Math.Round((totalNCCuttingTime / cuttingTime), 2) * 100;
                                            //WOEfficiency = Convert.ToDouble(TotalNCCutTimeDIVCuttingTime) * 100;
                                        }
                                        //Now insert into table
                                        using (MsqlConnection mcInsertRows = new MsqlConnection())
                                        {
                                            try
                                            {
                                                mcInsertRows.open();
                                                SqlCommand cmdInsertRows = new SqlCommand("INSERT INTO tblworeport " +
                                                    "(MachineID,HMIID,OperatorName,Shift,CorrectedDate,PartNo,WorkOrderNo,OpNo,TargetQty,DeliveredQty,IsPF,IsHold,CuttingTime,SettingTime,SelfInspection,Idle,Breakdown,Type,NCCuttingTimePerPart,TotalNCCuttingTime,WOEfficiency,RejectedQty,Program,MRWeight,InsertedOn,IsMultiWO,MinorLoss,SplitWO,Blue,ScrapQtyTime,ReWorkTime,SummationOfSCTvsPP)"
                                                    + " VALUES('" + MachineID + "','" + hmiid + "','" + OperatorName + "','" + shift + "','" + hmiCorretedDate + "',\""
                                                    + PartNo + "\",\"" + WONo + "\",'" + OpNo + "','" + targetQty + "','" + deliveredQty + "','" + IsPF + "','" + isHold + "','" + Math.Round(cuttingTime, 2) + "','" + Math.Round(settingTime, 2) + "','" + Math.Round(selfInspection, 2) + "','" + Math.Round(idle, 2) + "','" + Math.Round(breakdown, 2) + "','" + type + "','" + stdCuttingTime + "','" + totalNCCuttingTime + "','" + WOEfficiency + "','" + rejectedQty + "','" + program + "','" + stdMRWeight + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + isSingleWo + "','" + Math.Round(MinorLoss, 2) + "','" + SplitWO + "','" + Math.Round(Blue, 2) + "','" + Math.Round(ScrapQtyTime, 2) + "','" + Math.Round(ReworkTime, 2) + "','" + Math.Round(SummationSCTvsPP, 2) + "');", mcInsertRows.msqlConnection);
                                                cmdInsertRows.ExecuteNonQuery();
                                            }
                                            catch (Exception e)
                                            {
                                                //IntoFile(e.ToString());
                                            }
                                        }
                                    }
                                    catch (Exception eSingle)
                                    {
                                        //IntoFile(eSingle.ToString());
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region MultiWO
                                    var MultiWOData = db.tbl_multiwoselection.Where(m => m.HMIID == hmiid).ToList();
                                    foreach (var multirow in MultiWOData)
                                    {
                                        string SplitWO = multirow.SplitWO;
                                        try
                                        {
                                            string PartNo = multirow.PartNo;
                                            string WONo = multirow.WorkOrder;
                                            string OpNo = multirow.OperationNo;
                                            int targetQty = Convert.ToInt32(multirow.TargetQty);
                                            int deliveredQty = Convert.ToInt32(multirow.DeliveredQty);
                                            int rejectedQty = Convert.ToInt32(multirow.ScrapQty);
                                            if (rejectedQty > 0)
                                            {
                                                ScrapQtyTime = (cuttingTime / (rejectedQty + deliveredQty)) * rejectedQty;
                                            }

                                            int IsPF = 0;
                                            if (multirow.IsCompleted == 1)
                                            {
                                                IsPF = 1;
                                            }
                                            //Constants From DB
                                            double stdCuttingTime = 0, stdMRWeight = 0;
                                            var StdWeightTime = db.tblmasterparts_st_sw.Where(m => m.PartNo == PartNo && m.OpNo == OpNo && m.IsDeleted == 0).FirstOrDefault();
                                            if (StdWeightTime != null)
                                            {
                                                string stdCuttingTimeString = null, stdMRWeightString = null;
                                                string stdCuttingTimeUnitString = null, stdMRWeightUnitString = null;
                                                stdCuttingTimeString = Convert.ToString(StdWeightTime.StdCuttingTime);
                                                stdMRWeightString = Convert.ToString(StdWeightTime.MaterialRemovedQty);
                                                stdCuttingTimeUnitString = Convert.ToString(StdWeightTime.StdCuttingTimeUnit);
                                                stdMRWeightUnitString = Convert.ToString(StdWeightTime.MaterialRemovedQtyUnit);

                                                double.TryParse(stdCuttingTimeString, out stdCuttingTime);
                                                double.TryParse(stdMRWeightString, out stdMRWeight);

                                                if (stdCuttingTimeUnitString == "Hrs")
                                                {
                                                    stdCuttingTime = stdCuttingTime * 60;
                                                }
                                                else if (stdCuttingTimeUnitString == "Sec") //Unit is Minutes
                                                {
                                                    stdCuttingTime = stdCuttingTime / 60;
                                                }
                                                SummationSCTvsPP = stdCuttingTime * deliveredQty;
                                            }
                                            double totalNCCuttingTime = deliveredQty * stdCuttingTime;
                                            //??
                                            string MRReason = null;

                                            double WOEfficiency = 0;
                                            if (cuttingTime != 0)
                                            {
                                                WOEfficiency = Math.Round((totalNCCuttingTime / cuttingTime), 2);
                                            }

                                            //Now insert into table
                                            using (MsqlConnection mcInsertRows = new MsqlConnection())
                                            {
                                                try
                                                {
                                                    mcInsertRows.open();
                                                    SqlCommand cmdInsertRows = new SqlCommand("INSERT INTO tblworeport " +
                                                        "(MachineID,HMIID,OperatorName,Shift,CorrectedDate,PartNo,WorkOrderNo,OpNo,TargetQty,DeliveredQty,IsPF,IsHold,CuttingTime,SettingTime,SelfInspection,Idle,Breakdown,Type,NCCuttingTimePerPart,TotalNCCuttingTime,WOEfficiency,RejectedQty,RejectedReason,Program,MRWeight,InsertedOn,IsMultiWO,MinorLoss,SplitWO,Blue,ScrapQtyTime,ReWorkTime,SummationOfSCTvsPP)"
                                                        + " VALUES('" + MachineID + "','" + hmiid + "','" + OperatorName + "','" + shift + "','" + hmiCorretedDate + "','"
                                                        + PartNo + "','" + WONo + "','" + OpNo + "','" + targetQty + "','" + deliveredQty + "','" + IsPF + "','" + isHold + "','" + Math.Round(cuttingTime, 2) + "','" + Math.Round(settingTime, 2) + "','" + Math.Round(selfInspection, 2) + "','" + Math.Round(idle, 2) + "','" + Math.Round(breakdown, 2) + "','" + type + "','" + stdCuttingTime + "','" + totalNCCuttingTime + "','" + WOEfficiency + "','" + rejectedQty + "','" + MRReason + "','" + program + "','" + stdMRWeight + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + isSingleWo + "','" + Math.Round(MinorLoss, 2) + "','" + SplitWO + "','" + Math.Round(Blue) + "','" + Math.Round(ScrapQtyTime) + "','" + Math.Round(ReworkTime) + "','" + Math.Round(SummationSCTvsPP) + "');", mcInsertRows.msqlConnection);
                                                    cmdInsertRows.ExecuteNonQuery();
                                                }
                                                catch (Exception e)
                                                {
                                                    //IntoFile("MultiWO Error: " + e.ToString());
                                                }
                                            }
                                        }
                                        catch (Exception eMulti)
                                        {
                                            //IntoFile(eMulti.ToString());
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                        #endregion
                        //}
                    }
                    catch (Exception e)
                    {
                        //IntoFile("WorkOrder Block " + e.ToString());
                    }

                    //LossesData for each WorkOrder
                    try
                    {
                        #region
                        ////Testing 
                        //MachineID = 1;
                        //CorrectedDate = "2017-03-22";

                        //var HMIData = db.tblhmiscreens.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate && (m.isWorkInProgress == 0 || m.isWorkInProgress == 1)).ToList();
                        var HMIData = db.tblhmiscreens.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate && (m.isWorkInProgress == 0 || m.isWorkInProgress == 1)).ToList();
                        foreach (var hmirow in HMIData)
                        {
                            int hmiid = hmirow.HMIID;
                            var WODataPresent = db.tblwolossesses.Where(m => m.HMIID == hmiid).ToList();
                            if (WODataPresent.Count == 0)
                            {
                                DateTime StartTime = Convert.ToDateTime(hmirow.Date);
                                DateTime EndTime = Convert.ToDateTime(hmirow.Time);

                                var LossesIDs = db.tbllossofentries.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate && m.DoneWithRow == 1).Select(m => m.MessageCodeID).Distinct().ToList();
                                foreach (var loss in LossesIDs)
                                {
                                    DataTable lossesData = new DataTable();
                                    double duration = 0;
                                    int lossID = loss;
                                    using (MsqlConnection mc = new MsqlConnection())
                                    {
                                        mc.open();
                                        String query1 = "SELECT StartDateTime,EndDateTime,LossID From tbllossofentry WHERE MachineID = '" + MachineID + "' and CorrectedDate = '" + CorrectedDate + "' and MessageCodeID = '" + lossID + "' and DoneWithRow = 1  and "
                                            + "( StartDateTime <= '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( ( EndDateTime > '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( EndDateTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' or   EndDateTime > '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' )  ) ) or "
                                            + " (  StartDateTime > '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( StartDateTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' ) ))";
                                        SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                                        da1.Fill(lossesData);
                                        mc.close();
                                    }

                                    for (int losslooper = 0; losslooper < lossesData.Rows.Count; losslooper++)
                                    {
                                        if (!string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[losslooper][0])) && !string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[losslooper][1])))
                                        {
                                            DateTime LStartDate = Convert.ToDateTime(lossesData.Rows[losslooper][0]);
                                            DateTime LEndDate = Convert.ToDateTime(lossesData.Rows[losslooper][1]);
                                            double IndividualDur = LEndDate.Subtract(LStartDate).TotalSeconds;

                                            //Get Duration Based on start & end Time.

                                            if (LStartDate < StartTime)
                                            {
                                                double StartDurationExtra = StartTime.Subtract(LStartDate).TotalSeconds;
                                                IndividualDur -= StartDurationExtra;
                                            }
                                            if (LEndDate > EndTime)
                                            {
                                                double EndDurationExtra = LEndDate.Subtract(EndTime).TotalSeconds;
                                                IndividualDur -= EndDurationExtra;
                                            }
                                            duration += IndividualDur;
                                        }
                                    }

                                    if (duration > 0)
                                    {
                                        duration = Math.Round(duration / 60, 2);
                                        //durationList.Add(new KeyValuePair<int, double>(lossID, duration));

                                        //Get Loss level, and hierarchical details
                                        int losslevel = 0, level1ID = 0, level2ID = 0;
                                        string LossName, Level1Name, Level2Name;
                                        var lossdata = db.tbllossescodes.Where(m => m.LossCodeID == lossID).FirstOrDefault();
                                        int level = lossdata.LossCodesLevel;
                                        string losscodeName = null;

                                        #region To Get LossCode Hierarchy and Push into table
                                        if (level == 3)
                                        {
                                            int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                                            int lossLevel2ID = Convert.ToInt32(lossdata.LossCodesLevel2ID);
                                            var lossdata1 = db.tbllossescodes.Where(m => m.LossCodeID == lossLevel1ID).FirstOrDefault();
                                            var lossdata2 = db.tbllossescodes.Where(m => m.LossCodeID == lossLevel2ID).FirstOrDefault();
                                            losscodeName = lossdata1.LossCode + " :: " + lossdata2.LossCode + " : " + lossdata.LossCode;
                                            Level1Name = lossdata1.LossCode;
                                            Level2Name = lossdata2.LossCode;
                                            LossName = lossdata.LossCode;

                                            //Now insert into table
                                            using (MsqlConnection mcInsertRows = new MsqlConnection())
                                            {
                                                try
                                                {
                                                    mcInsertRows.open();
                                                    SqlCommand cmdInsertRows = new SqlCommand("INSERT INTO [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblwolossess "
                                                        + "(HMIID,LossID,LossName,LossDuration,Level,LossCodeLevel1ID,LossCodeLevel1Name,LossCodeLevel2ID,LossCodeLevel2Name,InsertedOn,IsDeleted) "
                                                        + " VALUES('" + hmiid + "','" + lossID + "','" + LossName + "','" + duration + "','" + level + "','" + lossLevel1ID + "','"
                                                        + Level1Name + "','" + lossLevel2ID + "','" + Level2Name + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0)", mcInsertRows.msqlConnection);
                                                    cmdInsertRows.ExecuteNonQuery();
                                                }
                                                catch (Exception e)
                                                {
                                                    //IntoFile(e.ToString());
                                                }
                                            }

                                        }
                                        else if (level == 2)
                                        {
                                            int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                                            var lossdata1 = db.tbllossescodes.Where(m => m.LossCodeID == lossLevel1ID).FirstOrDefault();
                                            losscodeName = lossdata1.LossCode + ":" + lossdata.LossCode;
                                            Level1Name = lossdata1.LossCode;

                                            //Now insert into table
                                            using (MsqlConnection mcInsertRows = new MsqlConnection())
                                            {
                                                try
                                                {
                                                    mcInsertRows.open();
                                                    SqlCommand cmdInsertRows = new SqlCommand("INSERT INTO [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblwolossess "
                                                        + "(HMIID,LossID,LossName,LossDuration,Level,LossCodeLevel1ID,LossCodeLevel1Name,InsertedOn,IsDeleted) "
                                                        + " VALUES('" + hmiid + "','" + lossID + "','" + lossdata.LossCode + "','" + duration + "','" + level + "','" + lossLevel1ID + "','"
                                                        + Level1Name + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0)", mcInsertRows.msqlConnection);
                                                    cmdInsertRows.ExecuteNonQuery();
                                                }
                                                catch (Exception e)
                                                {
                                                    //IntoFile(e.ToString());
                                                }
                                            }

                                        }
                                        else if (level == 1)
                                        {
                                            if (lossID == 999)
                                            {
                                                losscodeName = "NoCode Entered";
                                            }
                                            else
                                            {
                                                losscodeName = lossdata.LossCode;
                                            }
                                            //Now insert into table
                                            using (MsqlConnection mcInsertRows = new MsqlConnection())
                                            {
                                                try
                                                {
                                                    mcInsertRows.open();
                                                    SqlCommand cmdInsertRows = new SqlCommand("INSERT INTO [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblwolossess "
                                                        + "(HMIID,LossID,LossName,LossDuration,Level,InsertedOn,IsDeleted) "
                                                        + " VALUES('" + hmiid + "','" + lossID + "','" + losscodeName + "','" + duration + "','" + level + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0);", mcInsertRows.msqlConnection);
                                                    cmdInsertRows.ExecuteNonQuery();
                                                }
                                                catch (Exception e)
                                                {
                                                    //IntoFile(e.ToString());
                                                }
                                            }
                                        }
                                        #endregion

                                    }

                                }
                            }
                        }

                        #endregion
                    }
                    catch (Exception e)
                    {
                        //IntoFile("Losses Block " + e.ToString());
                    }
                }

                //For Manual WorkCenters.
                var MWCData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 1 && m.ManualWCID.HasValue).ToList();
                foreach (var macrow in MWCData)
                {
                    int MachineID = macrow.MachineID;
                    try
                    {
                        #region
                        var WODataPresent = db.tblworeports.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate).ToList();
                        if (WODataPresent.Count == 0)
                        {
                            var HMIData = db.tblhmiscreens.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate && (m.isWorkInProgress == 0 || m.isWorkInProgress == 1)).ToList();
                            foreach (var hmirow in HMIData)
                            {
                                //Constants from table

                                int hmiid = hmirow.HMIID;
                                string OperatorName = hmirow.OperatorDet;
                                string shift = hmirow.Shift;
                                string hmiCorretedDate = hmirow.CorrectedDate;
                                string type = hmirow.Prod_FAI;
                                string program = hmirow.Project;
                                int isHold = 0;
                                isHold = hmirow.IsHold;
                                string SplitWO = hmirow.SplitWO;
                                int HoldID = 0; string HoldReasonID = null;
                                try
                                {
                                    HoldID = Convert.ToInt32(db.tblmanuallossofentries.Where(m => m.HMIID == hmiid).Select(m => m.MessageCodeID).FirstOrDefault());
                                }
                                catch (Exception e)
                                {
                                    //IntoFile("Hold ID Issue for HMIID: " + hmiid);
                                }
                                if (HoldID != 0)
                                {
                                    HoldReasonID = HoldID.ToString();
                                }

                                DateTime StartTime = Convert.ToDateTime(hmirow.Date);
                                DateTime EndTime = Convert.ToDateTime(hmirow.Time);
                                //Values from Calculation
                                double cuttingTime = 0, settingTime = 0, selfInspection = 0, idle = 0, breakdown = 0;
                                double Blue = 0, ScrapQtyTime = 0, ReworkTime = 0;

                                settingTime = GetSettingTimeForWO(CorrectedDate, MachineID, StartTime, EndTime);
                                settingTime = Math.Round(settingTime / 60, 2);
                                selfInspection = GetSelfInsepectionForWO(CorrectedDate, MachineID, StartTime, EndTime);
                                selfInspection = Math.Round(selfInspection / 60, 2);
                                double TotalLosses = GetAllLossesTimeForWO(CorrectedDate, MachineID, StartTime, EndTime);
                                TotalLosses = Math.Round(TotalLosses / 60, 2);
                                idle = TotalLosses;
                                breakdown = 0;

                                var HMIIDData = db.tblhmiscreens.Where(m => m.HMIID == hmiid).FirstOrDefault();
                                DateTime WOStartDateTime = Convert.ToDateTime(HMIIDData.Date);
                                DateTime WOEndDateTime = Convert.ToDateTime(HMIIDData.Time);
                                double TotalWODurationIsSec = WOEndDateTime.Subtract(WOStartDateTime).TotalMinutes;

                                cuttingTime = TotalWODurationIsSec - settingTime - selfInspection;

                                int isSingleWo = 0;
                                isSingleWo = hmirow.IsMultiWO;

                                try
                                {
                                    string PartNo = hmirow.PartNo;
                                    string WONo = hmirow.Work_Order_No;
                                    string OpNo = hmirow.OperationNo;
                                    int targetQty = Convert.ToInt32(hmirow.Target_Qty);
                                    int deliveredQty = Convert.ToInt32(hmirow.Delivered_Qty);
                                    int rejectedQty = Convert.ToInt32(hmirow.Rej_Qty);
                                    int IsPF = 0;
                                    if (hmirow.isWorkInProgress == 1)
                                    {
                                        IsPF = 1;
                                    }

                                    if (rejectedQty > 0)
                                    {
                                        ScrapQtyTime = (cuttingTime / (rejectedQty + deliveredQty)) * rejectedQty;
                                    }

                                    bool isRework = false;
                                    isRework = hmirow.isWorkOrder == 1 ? true : false;
                                    if (isRework)
                                    {
                                        ReworkTime = cuttingTime;
                                    }

                                    //Constants From DB
                                    double stdCuttingTime = 0, stdMRWeight = 0;
                                    var StdWeightTime = db.tblmasterparts_st_sw.Where(m => m.PartNo == PartNo && m.OpNo == OpNo && m.IsDeleted == 0).FirstOrDefault();
                                    if (StdWeightTime != null)
                                    {
                                        string stdCuttingTimeString = null, stdMRWeightString = null;
                                        string stdCuttingTimeUnitString = null, stdMRWeightUnitString = null;
                                        stdCuttingTimeString = Convert.ToString(StdWeightTime.StdCuttingTime);
                                        stdMRWeightString = Convert.ToString(StdWeightTime.MaterialRemovedQty);
                                        stdCuttingTimeUnitString = Convert.ToString(StdWeightTime.StdCuttingTimeUnit);
                                        stdMRWeightUnitString = Convert.ToString(StdWeightTime.MaterialRemovedQtyUnit);

                                        double.TryParse(stdCuttingTimeString, out stdCuttingTime);
                                        double.TryParse(stdMRWeightString, out stdMRWeight);

                                        stdCuttingTimeUnitString = StdWeightTime.StdCuttingTimeUnit;
                                        stdCuttingTimeUnitString = StdWeightTime.StdCuttingTimeUnit;

                                        if (stdCuttingTimeUnitString == "Hrs")
                                        {
                                            stdCuttingTime = stdCuttingTime * 60;
                                        }
                                        else if (stdCuttingTimeUnitString == "Sec") //Unit is Minutes
                                        {
                                            stdCuttingTime = stdCuttingTime / 60;
                                        }
                                    }
                                    double totalNCCuttingTime = deliveredQty * stdCuttingTime;
                                    //??
                                    string MRReason = null;

                                    double WOEfficiency = 0;
                                    if (cuttingTime != 0)
                                    {
                                        WOEfficiency = Math.Round((totalNCCuttingTime / cuttingTime), 2) * 100;
                                        //WOEfficiency = Convert.ToDouble(TotalNCCutTimeDIVCuttingTime) * 100;
                                    }
                                    //Now insert into table
                                    using (MsqlConnection mcInsertRows = new MsqlConnection())
                                    {
                                        try
                                        {
                                            mcInsertRows.open();
                                            SqlCommand cmdInsertRows = new SqlCommand("INSERT INTO tblworeport " +
                                                "(MachineID,HMIID,OperatorName,Shift,CorrectedDate,PartNo,WorkOrderNo,OpNo,TargetQty,DeliveredQty,IsPF,IsHold,CuttingTime,SettingTime,SelfInspection,Idle,Breakdown,Type,NCCuttingTimePerPart,TotalNCCuttingTime,WOEfficiency,RejectedQty,Program,MRWeight,InsertedOn,IsMultiWO,IsNormalWC,HoldReason,SplitWO,Blue,ScrapQtyTime,ReWorkTime)"
                                                + " VALUES('" + MachineID + "','" + hmiid + "','" + OperatorName + "','" + shift + "','" + hmiCorretedDate + "',\""
                                                + PartNo + "\",\"" + WONo + "\",'" + OpNo + "','" + targetQty + "','" + deliveredQty + "','" + IsPF + "','" + isHold + "','" + Math.Round(cuttingTime, 2) + "','" + Math.Round(settingTime, 2) + "','" + Math.Round(selfInspection, 2) + "','" + Math.Round(idle, 2) + "','" + Math.Round(breakdown, 2) + "','" + type + "','" + stdCuttingTime + "','" + totalNCCuttingTime + "','" + WOEfficiency + "','" + rejectedQty + "','" + program + "','" + stdMRWeight + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + isSingleWo + "',1,'" + HoldReasonID + "','" + SplitWO + "','" + Math.Round(Blue, 2) + "','" + Math.Round(ScrapQtyTime, 2) + "','" + Math.Round(ReworkTime, 2) + "');", mcInsertRows.msqlConnection);
                                            cmdInsertRows.ExecuteNonQuery();
                                        }
                                        catch (Exception e)
                                        {
                                            //IntoFile(e.ToString());
                                        }
                                    }
                                }
                                catch (Exception eSingle)
                                {
                                    //IntoFile(eSingle.ToString());
                                }

                            }
                        }
                        #endregion
                    }
                    catch (Exception e)
                    {
                        //IntoFile("WorkOrder Block " + e.ToString());
                    }

                    //LossesData for each WorkOrder
                    try
                    {
                        #region

                        var HMIData = db.tblhmiscreens.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate && (m.isWorkInProgress == 0 || m.isWorkInProgress == 1)).ToList();
                        foreach (var hmirow in HMIData)
                        {
                            int hmiid = hmirow.HMIID;
                            var WODataPresent = db.tblwolossesses.Where(m => m.HMIID == hmiid).ToList();
                            if (WODataPresent.Count == 0)
                            {
                                DateTime StartTime = Convert.ToDateTime(hmirow.Date);
                                DateTime EndTime = Convert.ToDateTime(hmirow.Time);

                                var LossesIDs = db.tbllossofentries.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate && m.DoneWithRow == 1).Select(m => m.MessageCodeID).Distinct().ToList();
                                foreach (var loss in LossesIDs)
                                {
                                    DataTable lossesData = new DataTable();
                                    double duration = 0;
                                    int lossID = loss;
                                    using (MsqlConnection mc = new MsqlConnection())
                                    {
                                        mc.open();
                                        String query1 = "SELECT StartDateTime,EndDateTime,LossID From tbllossofentry WHERE MachineID = '" + MachineID + "' and CorrectedDate = '" + CorrectedDate + "' and MessageCodeID = '" + lossID + "' and DoneWithRow = 1  and "
                                            + "( StartDateTime <= '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( ( EndDateTime > '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( EndDateTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' or   EndDateTime > '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' )  ) ) or "
                                            + " (  StartDateTime > '" + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( StartDateTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' ) ))";
                                        SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                                        da1.Fill(lossesData);
                                        mc.close();
                                    }

                                    for (int losslooper = 0; losslooper < lossesData.Rows.Count; losslooper++)
                                    {
                                        if (!string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[losslooper][0])) && !string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[losslooper][1])))
                                        {
                                            DateTime LStartDate = Convert.ToDateTime(lossesData.Rows[losslooper][0]);
                                            DateTime LEndDate = Convert.ToDateTime(lossesData.Rows[losslooper][1]);
                                            double IndividualDur = LEndDate.Subtract(LStartDate).TotalSeconds;

                                            //Get Duration Based on start & end Time.

                                            if (LStartDate < StartTime)
                                            {
                                                double StartDurationExtra = StartTime.Subtract(LStartDate).TotalSeconds;
                                                IndividualDur -= StartDurationExtra;
                                            }
                                            if (LEndDate > EndTime)
                                            {
                                                double EndDurationExtra = LEndDate.Subtract(EndTime).TotalSeconds;
                                                IndividualDur -= EndDurationExtra;
                                            }
                                            duration += IndividualDur;
                                        }
                                    }

                                    if (duration > 0)
                                    {
                                        duration = Math.Round(duration / 60, 2);
                                        //durationList.Add(new KeyValuePair<int, double>(lossID, duration));

                                        //Get Loss level, and hierarchical details
                                        string LossName, Level1Name, Level2Name;
                                        var lossdata = db.tbllossescodes.Where(m => m.LossCodeID == lossID).FirstOrDefault();
                                        int level = lossdata.LossCodesLevel;
                                        string losscodeName = null;

                                        #region To Get LossCode Hierarchy and Push into table
                                        if (level == 3)
                                        {
                                            int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                                            int lossLevel2ID = Convert.ToInt32(lossdata.LossCodesLevel2ID);
                                            var lossdata1 = db.tbllossescodes.Where(m => m.LossCodeID == lossLevel1ID).FirstOrDefault();
                                            var lossdata2 = db.tbllossescodes.Where(m => m.LossCodeID == lossLevel2ID).FirstOrDefault();
                                            losscodeName = lossdata1.LossCode + " :: " + lossdata2.LossCode + " : " + lossdata.LossCode;
                                            Level1Name = lossdata1.LossCode;
                                            Level2Name = lossdata2.LossCode;
                                            LossName = lossdata.LossCode;

                                            //Now insert into table
                                            using (MsqlConnection mcInsertRows = new MsqlConnection())
                                            {
                                                try
                                                {
                                                    mcInsertRows.open();
                                                    SqlCommand cmdInsertRows = new SqlCommand("INSERT INTO [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblwolossess "
                                                        + "(HMIID,LossID,LossName,LossDuration,Level,LossCodeLevel1ID,LossCodeLevel1Name,LossCodeLevel2ID,LossCodeLevel2Name,InsertedOn,IsDeleted) "
                                                        + " VALUES('" + hmiid + "','" + lossID + "','" + LossName + "','" + duration + "','" + level + "','" + lossLevel1ID + "','"
                                                        + Level1Name + "','" + lossLevel2ID + "','" + Level2Name + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0)", mcInsertRows.msqlConnection);
                                                    cmdInsertRows.ExecuteNonQuery();
                                                }
                                                catch (Exception e)
                                                {
                                                    //IntoFile(e.ToString());
                                                }
                                            }

                                        }
                                        else if (level == 2)
                                        {
                                            int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                                            var lossdata1 = db.tbllossescodes.Where(m => m.LossCodeID == lossLevel1ID).FirstOrDefault();
                                            losscodeName = lossdata1.LossCode + ":" + lossdata.LossCode;
                                            Level1Name = lossdata1.LossCode;

                                            //Now insert into table
                                            using (MsqlConnection mcInsertRows = new MsqlConnection())
                                            {
                                                try
                                                {
                                                    mcInsertRows.open();
                                                    SqlCommand cmdInsertRows = new SqlCommand("INSERT INTO [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblwolossess "
                                                        + "(HMIID,LossID,LossName,LossDuration,Level,LossCodeLevel1ID,LossCodeLevel1Name,InsertedOn,IsDeleted) "
                                                        + " VALUES('" + hmiid + "','" + lossID + "','" + lossdata.LossCode + "','" + duration + "','" + level + "','" + lossLevel1ID + "','"
                                                        + Level1Name + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0)", mcInsertRows.msqlConnection);
                                                    cmdInsertRows.ExecuteNonQuery();
                                                }
                                                catch (Exception e)
                                                {
                                                    //IntoFile(e.ToString());
                                                }
                                            }

                                        }
                                        else if (level == 1)
                                        {
                                            if (lossID == 999)
                                            {
                                                losscodeName = "NoCode Entered";
                                            }
                                            else
                                            {
                                                losscodeName = lossdata.LossCode;
                                            }
                                            //Now insert into table
                                            using (MsqlConnection mcInsertRows = new MsqlConnection())
                                            {
                                                try
                                                {
                                                    mcInsertRows.open();
                                                    SqlCommand cmdInsertRows = new SqlCommand("INSERT INTO [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblwolossess "
                                                        + "(HMIID,LossID,LossName,LossDuration,Level,InsertedOn,IsDeleted) "
                                                        + " VALUES('" + hmiid + "','" + lossID + "','" + losscodeName + "','" + duration + "','" + level + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0);", mcInsertRows.msqlConnection);
                                                    cmdInsertRows.ExecuteNonQuery();
                                                }
                                                catch (Exception e)
                                                {
                                                    //IntoFile(e.ToString());
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }
                        }

                        #endregion
                    }
                    catch (Exception e)
                    {
                        //IntoFile("Losses Block " + e.ToString());
                    }
                }

                UsedDateForExcel = UsedDateForExcel.AddDays(+1);
            }
            #endregion

        }

        //Output: Seconds.
        public double GetScrapQtyTimeOfWO(string UsedDateForExcel, DateTime StartTime, DateTime EndTime, int MachineID, int HMIID)
        {
            double SQT = 0;
            using (i_facility_shaktiEntities1 dbhmi = new i_facility_shaktiEntities1())
            {
                var PartsData = dbhmi.tblhmiscreens.Where(m => m.HMIID == HMIID).FirstOrDefault();
                if (PartsData != null)
                {
                    int scrapQty = Convert.ToInt32(PartsData.Rej_Qty);
                    int DeliveredQty = Convert.ToInt32(PartsData.Delivered_Qty);
                    Double WODuration = GetGreen(UsedDateForExcel, StartTime, EndTime, MachineID);
                    if ((scrapQty + DeliveredQty) == 0)
                    {
                        SQT += 0;
                    }
                    else
                    {
                        SQT += (WODuration / (scrapQty + DeliveredQty)) * scrapQty;
                    }
                }
            }
            return SQT;
        }

        //Output: Seconds
        public double GetScrapQtyTimeOfRWO(string UsedDateForExcel, DateTime StartTime, DateTime EndTime, int MachineID, int HMIID)
        {
            double SQT = 0;
            using (i_facility_shaktiEntities1 dbhmi = new i_facility_shaktiEntities1())
            {
                var PartsData = dbhmi.tblhmiscreens.Where(m => m.HMIID == HMIID).FirstOrDefault();
                if (PartsData != null)
                {
                    int scrapQty = Convert.ToInt32(PartsData.Rej_Qty);
                    int DeliveredQty = Convert.ToInt32(PartsData.Delivered_Qty);
                    SQT = GetGreen(UsedDateForExcel, StartTime, EndTime, MachineID);
                }
            }
            return SQT;
        }

        //Output: Minutes
        public double GetSummationOfSCTvsPPForWO(int HMIID)
        {
            double SummationofTime = 0;
            using (i_facility_shaktiEntities1 dbhmi = new i_facility_shaktiEntities1())
            {
                var PartsDataAll = dbhmi.tblhmiscreens.Where(m => m.HMIID == HMIID && m.isWorkOrder == 0 && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0)).OrderByDescending(m => m.PartNo).ThenByDescending(m => m.OperationNo).ToList();
                if (PartsDataAll.Count == 0)
                {
                    //return SummationofTime;
                }
                foreach (var row in PartsDataAll)
                {
                    if (row.IsMultiWO == 0)
                    {
                        string partNo = row.PartNo;
                        string woNo = row.Work_Order_No;
                        string opNo = row.OperationNo;
                        int DeliveredQty = 0;
                        DeliveredQty = Convert.ToInt32(row.Delivered_Qty);
                        #region InnerLogic Common for both ways(HMI or tblmultiWOselection)
                        double stdCuttingTime = 0;
                        var stdcuttingTimeData = db.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0 && m.OpNo == opNo && m.PartNo == partNo).FirstOrDefault();
                        if (stdcuttingTimeData != null)
                        {
                            double stdcuttingval = Convert.ToDouble(stdcuttingTimeData.StdCuttingTime);
                            string Unit = Convert.ToString(stdcuttingTimeData.StdCuttingTimeUnit);
                            if (Unit == "Hrs")
                            {
                                stdCuttingTime = stdcuttingval * 60;
                            }
                            else if (Unit == "Sec") //Unit is Minutes
                            {
                                stdCuttingTime = stdcuttingval / 60;
                            }
                            else
                            {
                                stdCuttingTime = stdcuttingval;
                            }
                        }
                        #endregion
                        SummationofTime += stdCuttingTime * DeliveredQty;
                    }
                    else
                    {
                        int hmiid = row.HMIID;
                        var multiWOData = dbhmi.tbl_multiwoselection.Where(m => m.HMIID == hmiid).ToList();
                        foreach (var rowMulti in multiWOData)
                        {
                            string partNo = rowMulti.PartNo;
                            string opNo = rowMulti.OperationNo;
                            int DeliveredQty = 0;
                            DeliveredQty = Convert.ToInt32(rowMulti.DeliveredQty);
                            #region
                            double stdCuttingTime = 0;
                            var stdcuttingTimeData = db.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0 && m.OpNo == opNo && m.PartNo == partNo).FirstOrDefault();
                            if (stdcuttingTimeData != null)
                            {
                                double stdcuttingval = Convert.ToDouble(stdcuttingTimeData.StdCuttingTime);
                                string Unit = Convert.ToString(stdcuttingTimeData.StdCuttingTimeUnit);
                                if (Unit == "Hrs")
                                {
                                    stdCuttingTime = stdcuttingval * 60;
                                }
                                else if (Unit == "Sec") //Unit is Minutes
                                {
                                    stdCuttingTime = stdcuttingval / 60;
                                }
                                else
                                {
                                    stdCuttingTime = stdcuttingval;
                                }

                            }
                            #endregion
                            SummationofTime += stdCuttingTime * DeliveredQty;
                        }
                    }
                }
            }
            return SummationofTime;
        }
    }
}