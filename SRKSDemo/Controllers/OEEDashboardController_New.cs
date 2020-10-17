using SRKSDemo.Server_Model;
using i_facilitylibrary;
using i_facilitylibrary.DAL;
using i_facilitylibrary.DAO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using SRKSDemo;

namespace i_facility.Controllers
{

    public class OEEDashboardController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        string databaseName = ConfigurationManager.AppSettings["databasename"];
        IConnectionFactory _conn;
        Dao obj1 = new Dao();
        Dao1 obj = new Dao1();
        public ActionResult Index(int PlantID = 0, int ShopID = 0, int CellID = 0, int WorkCenterID = 0, string StartDate = null, string EndDate = null, string SummarizeAs = null, string ButtonClicked = null)
        {
            string additionToPath = ButtonClicked;
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            //string  frmDate = '2019-06-14';
            DateTime frmDate = new DateTime();
            DateTime toDate = new DateTime();
            bool dateError = false;

            //To cope up with name change for button
            if (ButtonClicked == "AdjustedOEE")
            {
                ButtonClicked = "NoBlue";
            }

            if (ButtonClicked != "Today")
            {
                if (!String.IsNullOrEmpty(Convert.ToString(StartDate)) && !String.IsNullOrEmpty(Convert.ToString(EndDate)))
                {
                    //DateTime format is expected in this format "dd-mm-yy"
                    int FrmDateDay = Convert.ToInt32(StartDate.Split('-')[0]);
                    int FrmDateMonth = Convert.ToInt32(StartDate.Split('-')[1]);
                    int FrmDateYear = Convert.ToInt32(StartDate.Split('-')[2]);
                    int EndDateDay = Convert.ToInt32(EndDate.Split('-')[0]);
                    int EndDateMonth = Convert.ToInt32(EndDate.Split('-')[1]);
                    int EndDateYear = Convert.ToInt32(EndDate.Split('-')[2]);
                    frmDate = new DateTime(FrmDateYear, FrmDateMonth, FrmDateDay, 0, 0, 0);
                    toDate = new DateTime(EndDateYear, EndDateMonth, EndDateDay, 0, 0, 0);

                }
                else
                {
                    dateError = true;
                }
            }
            else
            {
                frmDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                if(DateTime.Now.Hour >= 0 && DateTime.Now.Hour < 6)
                {
                    frmDate = frmDate.AddDays(-1);
                }
                toDate = frmDate;
            }

            if (!dateError)
            {
                string ipAddress = GetIPAddressOf();
                string lowestLevel = null;
                string TimingVar = ButtonClicked;

                if (WorkCenterID == 0)
                {
                    if (CellID == 0)
                    {
                        if (ShopID == 0)
                        {
                            if (PlantID == 0)
                            {
                                //donothing
                            }
                            else
                            {
                                lowestLevel = "Plant";
                                SummarizeAs = lowestLevel;

                                try
                                {
                                    obj.deletetbloeedashboardvariablestodaysDetails2(ipAddress,WorkCenterID);
                                    //dboee.tbloeedashboardvariablestodays.RemoveRange(dboee.tbloeedashboardvariablestodays.Where(x => x.IPAddress == ipAddress));
                                    //dboee.SaveChanges();
                                }
                                catch (Exception e)
                                {
                                }
                                try
                                {
                                    obj.deletetbloeedashboardfinalvariablesDetails2(ipAddress,WorkCenterID);
                                    //dboee.tbloeedashboardfinalvariables.RemoveRange(dboee.tbloeedashboardfinalvariables.Where(x => x.IPAddress == ipAddress));
                                    //dboee.SaveChanges();
                                }
                                catch (Exception e)
                                {
                                }
                                var machData = obj.GetmachineListforoee(PlantID);
                                //var machData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.IsNormalWC == 0).ToList();
                                foreach (var row in machData)
                                {
                                    int MachineID = row.MachineID;
                                    CalculateOEE(frmDate, toDate, MachineID, TimingVar, SummarizeAs);
                                }
                            }
                        }
                        else
                        {
                            lowestLevel = "Shop";
                            SummarizeAs = lowestLevel;

                            try
                            {
                                obj.deletetbloeedashboardvariablestodaysDetails2(ipAddress,WorkCenterID);
                                //dboee.tbloeedashboardvariablestodays.RemoveRange(dboee.tbloeedashboardvariablestodays.Where(x => x.IPAddress == ipAddress));
                                //    dboee.SaveChanges();
                            }
                            catch (Exception e)
                            {
                            }
                            try
                            {
                                obj.deletetbloeedashboardfinalvariablesDetails2(ipAddress,WorkCenterID);
                                //dboee.tbloeedashboardfinalvariables.RemoveRange(dboee.tbloeedashboardfinalvariables.Where(x => x.IPAddress == ipAddress));
                                //dboee.SaveChanges();
                            }
                            catch (Exception e)
                            {
                            }
                            var machData = obj.GetShopList(ShopID);
                            //var machData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID && m.IsNormalWC == 0).ToList();
                            if (TimingVar == "Today")
                            {
                                foreach (var row in machData)
                                {
                                    int MachineID = row.MachineID;
                                    CalculateOEEToday(frmDate, toDate, MachineID, TimingVar, SummarizeAs);
                                }
                            }
                            else
                            {
                                foreach (var row in machData)
                                {
                                    int MachineID = row.MachineID;
                                    CalculateOEE(frmDate, toDate, MachineID, TimingVar, SummarizeAs);
                                }
                            }
                            if (machData.Count > 0)
                            {
                                CalculateSummarizedOEEWC(frmDate, toDate, ShopID, TimingVar, SummarizeAs);
                                CalculateSummarizedOEECell(frmDate, toDate, ShopID, TimingVar, SummarizeAs);
                                CalculateSummarizedOEEShop(frmDate, toDate, ShopID, TimingVar, SummarizeAs);
                            }
                        }
                    }
                    else
                    {
                        lowestLevel = "Cell";
                        SummarizeAs = lowestLevel;

                        try
                        {
                            obj.deletetbloeedashboardvariablestodaysDetails2(ipAddress,WorkCenterID);
                            //dboee.tbloeedashboardvariablestodays.RemoveRange(dboee.tbloeedashboardvariablestodays.Where(x => x.IPAddress == ipAddress));
                            //dboee.SaveChanges();
                        }
                        catch (Exception e)
                        {
                        }

                        try
                        {
                            obj.deletetbloeedashboardfinalvariablesDetails2(ipAddress,WorkCenterID);
                            //dboee.tbloeedashboardfinalvariables.RemoveRange(dboee.tbloeedashboardfinalvariables.Where(x => x.IPAddress == ipAddress));
                            //dboee.SaveChanges();
                        }
                        catch (Exception e)
                        {
                        }
                        var machData = obj.GetcellListforoee(CellID);
                        //var machData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID && m.IsNormalWC == 0).ToList();
                        if (TimingVar == "Today")
                        {
                            foreach (var row in machData)
                            {
                                int MachineID = row.MachineID;
                                CalculateOEEToday(frmDate, toDate, MachineID, TimingVar, SummarizeAs);
                            }
                        }
                        else
                        {
                            foreach (var row in machData)
                            {
                                int MachineID = row.MachineID;
                                CalculateOEE(frmDate, toDate, MachineID, TimingVar, SummarizeAs);
                            }
                        }
                        if (machData.Count > 0)
                        {
                            CalculateSummarizedOEEWC(frmDate, toDate, CellID, TimingVar, SummarizeAs);
                            CalculateSummarizedOEECell(frmDate, toDate, CellID, TimingVar, SummarizeAs);
                        }
                    }
                }
                else
                {
                    lowestLevel = "WorkCentre";
                    SummarizeAs = lowestLevel;
                    var machData = obj.GetMachineListforoee(WorkCenterID);
                    //var machData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == WorkCenterID && m.IsNormalWC == 0).ToList();

                    try
                    {
                        obj.deletetbloeedashboardvariablestodaysDetails2(machData[0].IPAddress.ToString(),WorkCenterID);
                        //dboee.tbloeedashboardvariablestodays.RemoveRange(dboee.tbloeedashboardvariablestodays.Where(x => x.IPAddress == ipAddress));
                        //dboee.SaveChanges();
                    }
                    catch (Exception e)
                    {
                    }

                    try
                    {
                        obj.deletetbloeedashboardfinalvariablesDetails2(machData[0].IPAddress,WorkCenterID);
                        //dboee.tbloeedashboardfinalvariables.RemoveRange(dboee.tbloeedashboardfinalvariables.Where(x => x.IPAddress == ipAddress));
                        //dboee.SaveChanges();
                    }
                    catch (Exception e)
                    {
                    }
                    if (TimingVar == "Today")
                    {
                        foreach (var row in machData)
                        {
                            int MachineID = row.MachineID;
                            CalculateOEEToday(frmDate, toDate, MachineID, TimingVar, SummarizeAs);
                            CalculateSummarizedOEEWC(frmDate, toDate, MachineID, TimingVar, SummarizeAs);
                        }
                    }
                    else
                    {
                        foreach (var row in machData)
                        {
                            int MachineID = row.MachineID;
                            CalculateOEE(frmDate, toDate, MachineID, TimingVar, SummarizeAs);
                            CalculateSummarizedOEEWC(frmDate, toDate, MachineID, TimingVar, SummarizeAs);
                        }
                    }
                }

                ViewData["CellWiseWCCount"] = 0;
                List<string> LossColors = new List<string> { "#9a4d6f", "#c76c47", "#30432E", "#d9b099", "#d4ba2f", "#CD5C5C", "#E9967A", "#800000", "#8C2E3A", "#808000", "#00FF00", "#0000FF", "#000080", "#C0C0C0", "#C71585", "#DC143C", "#625658", "#751B2A", "#A52A2A", "#87CEFA", "#7CFC00", "#2F4F4F", "#9400D3", "#CA979F", "#E9967A", "#AFC20F", "#DDA0DD", "#4169E1", "#9370DB", "#4B2F64", "#008080", "#90EE90", "#B0E0E6", "#EEE8AA", "#18850B", "#3CB371 ", "#9ACD32", "#0000CD", "#0000DD", "#c76c47", "#30432E", "#d9b099", "#d4ba2f", "#CD5C5C", "#E9967A", "#800000", "#8C2E3A", "#FFFF00", "#00FF00", "#FF00FF", "#AF00AF", "#DA70D6", "#FFFACD", "#F4A460", "#FFF5EE", "#FFFFF0", "#FFFAF0", "#6A5ACD", "#191970", "#40E0D0", "#E9967A", "#DC143C", "#FF6347", "#CD5C5C" };

                #region LowestLevel == WC
                if (lowestLevel == "WorkCentre")
                {
                    ViewBag.lowestLevel = "WorkCentre";
                    ViewBag.NoLevelsToShow = 1;
                    var OEEDataSummarizedToView = obj.GettbloeeDet(WorkCenterID, ipAddress, frmDate, toDate);
                    //var OEEDataSummarizedToView = db.tbloeedashboardfinalvariables.Where(m => m.IsDeleted == 0 && m.WCID == WorkCenterID && m.IPAddress == ipAddress && m.StartDate == frmDate && m.EndDate == toDate).FirstOrDefault();

                    if (OEEDataSummarizedToView != null)
                    {
                        Session["Error"] = "";
                        ViewBag.OverAllMax = obj.GettbloeeDet1(WorkCenterID, ipAddress, frmDate, toDate);
                        // ViewBag.OverAllMax = db.tbloeedashboardfinalvariables.Where(m => m.IsDeleted == 0 && m.WCID == WorkCenterID && m.IPAddress == ipAddress && m.StartDate == frmDate && m.EndDate == toDate).Max(m => m.Loss1Value);
                        ViewBag.Oee = OEEDataSummarizedToView.OEE;
                        ViewBag.Aval = OEEDataSummarizedToView.Availability;
                        if (OEEDataSummarizedToView.Availability > 0)
                        {
                            ViewBag.Perf = OEEDataSummarizedToView.Performance;
                            ViewBag.Qual = OEEDataSummarizedToView.Quality;
                        }
                        else
                        {
                            ViewBag.Perf = 0;
                            ViewBag.Qual = 0;
                        }

                        string CellWiseWCLossVals = null;
                        string CellWiseWCLossNames = null;

                        //string a = j1 + "," + j1 + "," + j1 + "," + j1 + "," + j1;
                        List<int> LossList = new List<int>();
                        if (OEEDataSummarizedToView.Loss1Name != "")
                        {
                            CellWiseWCLossVals = OEEDataSummarizedToView.Loss1Value.ToString();
                            CellWiseWCLossNames = OEEDataSummarizedToView.Loss1Name;
                            LossList.Add(Convert.ToInt32(OEEDataSummarizedToView.Loss1Name));
                        }
                        if (OEEDataSummarizedToView.Loss2Name != "")
                        {
                            CellWiseWCLossVals += "," + OEEDataSummarizedToView.Loss2Value.ToString();
                            CellWiseWCLossNames += "," + OEEDataSummarizedToView.Loss2Name;
                            LossList.Add(Convert.ToInt32(OEEDataSummarizedToView.Loss2Name));
                        }
                        if (OEEDataSummarizedToView.Loss3Name != "")
                        {
                            CellWiseWCLossVals += "," + OEEDataSummarizedToView.Loss3Value.ToString();
                            CellWiseWCLossNames += "," + OEEDataSummarizedToView.Loss3Name;
                            LossList.Add(Convert.ToInt32(OEEDataSummarizedToView.Loss3Name));
                        }
                        if (OEEDataSummarizedToView.Loss4Name != "")
                        {
                            CellWiseWCLossVals += "," + OEEDataSummarizedToView.Loss4Value.ToString();
                            CellWiseWCLossNames += "," + OEEDataSummarizedToView.Loss4Name;
                            LossList.Add(Convert.ToInt32(OEEDataSummarizedToView.Loss4Name));
                        }
                        if (OEEDataSummarizedToView.Loss5Name != "")
                        {
                            CellWiseWCLossVals += "," + OEEDataSummarizedToView.Loss5Value.ToString();
                            CellWiseWCLossNames += "," + OEEDataSummarizedToView.Loss5Name;
                            LossList.Add(Convert.ToInt32(OEEDataSummarizedToView.Loss5Name));
                        }

                        ViewData["CellWiseWCLossVals"] = CellWiseWCLossVals;
                        ViewData["CellWiseWCLossNames"] = CellWiseWCLossNames;

                        string ColorsAndLossCodes = null;
                        //LossList.Sort();

                        ColorsAndLossCodes = "<table><tr><div class='parent'>";
                        for (int Looper = 0; Looper < LossList.Count; Looper++)
                        {
                            if (Looper == 0)
                            {
                                ViewData["CellWiseWCLossColors"] = Convert.ToString(LossColors[Looper]);
                            }
                            else
                            {
                                ViewData["CellWiseWCLossColors"] += " , " + Convert.ToString(LossColors[Looper]);
                            }
                            int losscode = Convert.ToInt32(LossList[Looper]);
                            string lossHierarchy = null;
                            if (losscode != 0)
                            {
                                lossHierarchy = LossHierarchy(losscode);
                            }
                            string color = Convert.ToString(LossColors[Looper]);
                            if (Looper % 4 == 0 && Looper != 0)
                            {
                                ColorsAndLossCodes += "</div></tr><tr><td><div class='parent'><div class='childtext' style='margin-left: .1vw;'>" + lossHierarchy + " </div><div class='childcolor' style='background-color: " + color + "; color:  " + color + "'>.</div></td>";
                            }
                            else
                            {
                                ColorsAndLossCodes += "<td><div class='childtext' style='margin-left: .1vw;'>" + lossHierarchy + " </div><div class='childcolor' style='background-color: " + color + "; color:  " + color + "'>.</div></td>";
                            }
                        }
                        ColorsAndLossCodes += "</div></tr></table>";
                        ViewBag.ColorsAndLossCodes = ColorsAndLossCodes;
                    }
                    else
                    {
                        Session["Error"] = "No Data For the Selected Criteria.";
                    }
                }
                #endregion

                #region LowestLevel == Cell
                if (lowestLevel == "Cell")
                {
                    ViewBag.lowestLevel = "Cell";
                    //OverAll
                    var OEEDataSummarizedToView = obj.GettbloeecellDet(CellID, ipAddress, frmDate, toDate);
                    //var OEEDataSummarizedToView = db.tbloeedashboardfinalvariables.Where(m => m.IsDeleted == 0 && m.CellID == CellID && m.IPAddress == ipAddress && m.StartDate == frmDate && m.EndDate == toDate && m.IsOverallCellWise == 1).FirstOrDefault();
                    if (OEEDataSummarizedToView != null)
                    {
                        Session["Error"] = "";
                        ViewBag.Oee = OEEDataSummarizedToView.OEE;
                        ViewBag.Aval = OEEDataSummarizedToView.Availability;
                        if (OEEDataSummarizedToView.Availability > 0)
                        {
                            ViewBag.Perf = OEEDataSummarizedToView.Performance;
                            ViewBag.Qual = OEEDataSummarizedToView.Quality;
                        }
                        else
                        {
                            ViewBag.Perf = 0;
                            ViewBag.Qual = 0;
                        }

                        string CellWiseWCLossVals = null;
                        string CellWiseWCLossNames = null;

                        var firstItem = LossColors.ElementAt(0);
                        //Get Distinct Color codes and Unique lossCodes Lists.
                        //['#9a4d6f', '#c76c47', '#f85115', '#d9b099', '#d4ba2f'];

                        string GetDistinctLossQuery = null;
                        if (TimingVar != "Today")
                        {
                            GetDistinctLossQuery = "Select Distinct f.lossname from" +
                                                     "(" +
                                                     "Select Distinct a.Loss1Name as lossname from " + databaseName + ".tbloeedashboardvariables a where CellID = " + CellID + "  and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                     "Union All " +
                                                     "Select Distinct b.Loss2Name  as lossname from "+ databaseName + ".tbloeedashboardvariables b where CellID = " + CellID + "  and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                     "Union All " +
                                                     "Select Distinct c.Loss3Name as lossname from "+ databaseName + ".tbloeedashboardvariables c where CellID = " + CellID + "  and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                     "Union All " +
                                                     "Select Distinct d.Loss4Name as lossname from "+ databaseName + ".tbloeedashboardvariables d where CellID = " + CellID + "  and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                     "Union All " +
                                                     "Select Distinct e.Loss5Name as lossname from "+ databaseName + ".tbloeedashboardvariables e where CellID = " + CellID + "  and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                     ") f order by f.lossname desc";

                        }
                        else
                        {
                            GetDistinctLossQuery = "Select Distinct f.lossname from" +
                                                     "(" +
                                                     "Select Distinct a.Loss1Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday a where CellID = " + CellID + " and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                     "Union All " +
                                                     "Select Distinct b.Loss2Name  as lossname from "+ databaseName + ".tbloeedashboardvariablestoday b where CellID = " + CellID + " and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                     "Union All " +
                                                     "Select Distinct c.Loss3Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday c where CellID = " + CellID + " and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                     "Union All " +
                                                     "Select Distinct d.Loss4Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday d where CellID = " + CellID + " and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                     "Union All " +
                                                     "Select Distinct e.Loss5Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday e where CellID = " + CellID + " and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                     ") f order by f.lossname desc";
                        }
                        DataTable GetDistinctLossDT = new DataTable();
                        using (MsqlConnection mc = new MsqlConnection())
                        {
                            mc.open();
                            SqlDataAdapter GetDistinctLossDA = new SqlDataAdapter(GetDistinctLossQuery, mc.msqlConnection);
                            GetDistinctLossDA.Fill(GetDistinctLossDT);
                            mc.close();
                        }

                        List<string> LosscodesList = new List<string>();
                        List<int> LosscodesListIntegers = new List<int>();

                        for (int DTLooper = 0; DTLooper < GetDistinctLossDT.Rows.Count; DTLooper++)
                        {
                            LosscodesListIntegers.Add(Convert.ToInt32(GetDistinctLossDT.Rows[DTLooper][0]));
                        }
                        LosscodesListIntegers.Sort();
                        for (int DTLooper = 0; DTLooper < GetDistinctLossDT.Rows.Count; DTLooper++)
                        {
                            LosscodesList.Add(LosscodesListIntegers[DTLooper].ToString());
                        }

                        //Create a Color and LossCode matching List.
                        var ColorLoseList = new List<KeyValuePair<string, string>>();
                        for (int LossIndex = 0; LossIndex < LosscodesList.Count; LossIndex++)
                        {
                            string losscode = LosscodesList.ElementAt(LossIndex).ToString();
                            string color = LossColors.ElementAt(LossIndex).ToString();
                            ColorLoseList.Add(new KeyValuePair<string, string>(losscode, color));
                        }

                        if (OEEDataSummarizedToView.Loss1Name != "" && OEEDataSummarizedToView.Loss1Name != null)
                        {
                            CellWiseWCLossVals = OEEDataSummarizedToView.Loss1Value.ToString();
                            CellWiseWCLossNames = OEEDataSummarizedToView.Loss1Name;
                            string lossName = OEEDataSummarizedToView.Loss1Name.ToString();
                            List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                            ViewData["CellWiseWCLossColors"] = colorBasedOnLoss[0];
                        }
                        if (OEEDataSummarizedToView.Loss2Name != "" && OEEDataSummarizedToView.Loss2Name !=null)
                        {
                            CellWiseWCLossVals += "," + OEEDataSummarizedToView.Loss2Value.ToString();
                            CellWiseWCLossNames += "," + OEEDataSummarizedToView.Loss2Name;
                            string lossName = OEEDataSummarizedToView.Loss2Name.ToString();
                            List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                            ViewData["CellWiseWCLossColors"] += "," + colorBasedOnLoss[0];

                        }
                        if (OEEDataSummarizedToView.Loss3Name != "" && OEEDataSummarizedToView.Loss3Name != null)
                        {
                            CellWiseWCLossVals += "," + OEEDataSummarizedToView.Loss3Value.ToString();
                            CellWiseWCLossNames += "," + OEEDataSummarizedToView.Loss3Name;
                            string lossName = OEEDataSummarizedToView.Loss3Name.ToString();
                            List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                            ViewData["CellWiseWCLossColors"] += "," + colorBasedOnLoss[0];
                        }
                        if (OEEDataSummarizedToView.Loss4Name != "" && OEEDataSummarizedToView.Loss4Name != null)
                        {
                            CellWiseWCLossVals += "," + OEEDataSummarizedToView.Loss4Value.ToString();
                            CellWiseWCLossNames += "," + OEEDataSummarizedToView.Loss4Name;
                            string lossName = OEEDataSummarizedToView.Loss4Name.ToString();
                            List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                            ViewData["CellWiseWCLossColors"] += "," + colorBasedOnLoss[0];
                        }
                        if (OEEDataSummarizedToView.Loss5Name != "" && OEEDataSummarizedToView.Loss5Name != null)
                        {
                            CellWiseWCLossVals += "," + OEEDataSummarizedToView.Loss5Value.ToString();
                            CellWiseWCLossNames += "," + OEEDataSummarizedToView.Loss5Name;
                            string lossName = OEEDataSummarizedToView.Loss5Name.ToString();
                            List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                            ViewData["CellWiseWCLossColors"] += "," + colorBasedOnLoss[0];
                        }



                        string CellWiseWCOEEVals = null;
                        string CellWiseWCOEEWCNames = null;
                        string CellWiseWCAvailVals = null;
                        string CellWiseWCAvailWCNames = null;
                        string CellWiseWCPerfVals = null;
                        string CellWiseWCPerfWCNames = null;
                        string CellWiseWCQualVals = null;
                        string CellWiseWCQualWCNames = null;

                        var OEEDataOfWCToView = obj.GettbloeecellDet1(CellID, ipAddress, frmDate, toDate);
                        // var OEEDataOfWCToView = db.tbloeedashboardfinalvariables.Where(m => m.IsDeleted == 0 && m.CellID == CellID && m.IPAddress == ipAddress && m.StartDate == frmDate && m.EndDate == toDate && m.IsOverallCellWise == 0).ToList();
                        //to set max value for Y-Axis in Lossess.
                        //ViewBag.Max = ColorLoseList.Max(item => int.Parse(item.Value)); //Wrong
                        ViewBag.OverAllMax = obj.GettbloeeDet2(CellID, ipAddress, frmDate, toDate);
                        //ViewBag.OverAllMax = db.tbloeedashboardfinalvariables.Where(m => m.IsDeleted == 0 && m.CellID == CellID && m.IPAddress == ipAddress && m.StartDate == frmDate && m.EndDate == toDate && m.IsOverallCellWise == 1).Max(m => m.Loss1Value);
                        int once = 0;
                        foreach (var row in OEEDataOfWCToView)
                        {
                            if (once == 0)
                            {
                            }
                            else
                            {
                                CellWiseWCOEEVals += ",";
                                CellWiseWCOEEWCNames += ",";
                                CellWiseWCAvailVals += ",";
                                CellWiseWCAvailWCNames += ",";
                                CellWiseWCPerfVals += ",";
                                CellWiseWCPerfWCNames += ",";
                                CellWiseWCQualVals += ",";
                                CellWiseWCQualWCNames += ",";
                            }

                            int machineId = Convert.ToInt32(row.WCID);
                            string MacInvNo = GetMacInvNo(machineId);

                            CellWiseWCOEEVals += Convert.ToInt32(row.OEE);
                            CellWiseWCOEEWCNames += MacInvNo;
                            CellWiseWCAvailVals += Convert.ToInt32(row.Availability);
                            CellWiseWCAvailWCNames += MacInvNo;
                            CellWiseWCPerfVals += Convert.ToInt32(row.Performance);
                            CellWiseWCPerfWCNames += MacInvNo;
                            CellWiseWCQualVals += Convert.ToInt32(row.Quality);
                            CellWiseWCQualWCNames += MacInvNo;

                            ViewData["CellWiseWCLossValsMac" + once + ""] = MacInvNo;
                            if (row.Loss1Name != "")
                            {
                                ViewData["CellWiseWCLossVals" + once + ""] = row.Loss1Value.ToString();
                                ViewData["CellWiseWCLossNames" + once + ""] = row.Loss1Name;
                                string lossName = row.Loss1Name.ToString();
                                List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                                ViewData["CellWiseWCLossColors" + once + ""] = colorBasedOnLoss[0];
                            }
                            if (row.Loss2Name != "")
                            {
                                ViewData["CellWiseWCLossVals" + once + ""] += "," + row.Loss2Value.ToString();
                                ViewData["CellWiseWCLossNames" + once + ""] += "," + row.Loss2Name;
                                string lossName = row.Loss2Name.ToString();
                                List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                                ViewData["CellWiseWCLossColors" + once + ""] += "," + colorBasedOnLoss[0];
                            }
                            if (row.Loss3Name != "")
                            {
                                ViewData["CellWiseWCLossVals" + once + ""] += "," + row.Loss3Value.ToString();
                                ViewData["CellWiseWCLossNames" + once + ""] += "," + row.Loss3Name;
                                string lossName = row.Loss3Name.ToString();
                                List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                                ViewData["CellWiseWCLossColors" + once + ""] += "," + colorBasedOnLoss[0];
                            }
                            if (row.Loss4Name != "")
                            {
                                ViewData["CellWiseWCLossVals" + once + ""] += "," + row.Loss4Value.ToString();
                                ViewData["CellWiseWCLossNames" + once + ""] += "," + row.Loss4Name;
                                string lossName = row.Loss4Name.ToString();
                                List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                                ViewData["CellWiseWCLossColors" + once + ""] += "," + colorBasedOnLoss[0];
                            }
                            if (row.Loss5Name != "")
                            {
                                ViewData["CellWiseWCLossVals" + once + ""] += "," + row.Loss5Value.ToString();
                                ViewData["CellWiseWCLossNames" + once + ""] += "," + row.Loss5Name;
                                string lossName = row.Loss5Name.ToString();
                                List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                                ViewData["CellWiseWCLossColors" + once + ""] += "," + colorBasedOnLoss[0];
                            }
                            once++;
                        }
                        ViewData["CellWiseWCCount"] = once;

                        ViewData["CellWiseWCOEEVals"] = CellWiseWCOEEVals;
                        ViewData["CellWiseWCOEEWCNames"] = CellWiseWCOEEWCNames;
                        ViewData["CellWiseWCAvailVals"] = CellWiseWCAvailVals;
                        ViewData["CellWiseWCAvailWCNames"] = CellWiseWCAvailWCNames;
                        ViewData["CellWiseWCPerfVals"] = CellWiseWCPerfVals;
                        ViewData["CellWiseWCPerfWCNames"] = CellWiseWCPerfWCNames;
                        ViewData["CellWiseWCQualVals"] = CellWiseWCQualVals;
                        ViewData["CellWiseWCQualWCNames"] = CellWiseWCQualWCNames;

                        ViewData["CellWiseWCLossVals"] = CellWiseWCLossVals;
                        ViewData["CellWiseWCLossNames"] = CellWiseWCLossNames;

                        //ViewData["CellWiseWCLossNames" + 1 + ""] = "blah";

                        //['OEE', j1, j6, j3, j4, j5];
                        //['MS-02-04', 'MS-02-05', 'MS-02-06', 'MS-03-04', 'MS-03-05'];


                        //Colors and Losses Hierarchy Data. from ColorLoseList
                        //string 
                        //<div class="childtext" style="margin-left: .1vw;"> PowerOff  </div>
                        //<div class="childcolor" style="background-color: #9a4d6f; color: white"></div>

                        string ColorsAndLossCodes = null;
                        ColorsAndLossCodes = "<table><tr><div class='parent'>";
                        for (int Looper = 0; Looper < ColorLoseList.Count; Looper++)
                        {
                            int losscode = Convert.ToInt32(ColorLoseList[Looper].Key);
                            if (losscode != 0)
                            {
                                string lossHierarchy = null;
                                if (losscode != 0)
                                {
                                    lossHierarchy = LossHierarchy(losscode);
                                }
                                string color = Convert.ToString(ColorLoseList[Looper].Value);
                                if (Looper % 4 == 0 && Looper != 0)
                                {
                                    ColorsAndLossCodes += "</div></tr><tr><td><div class='parent'><div class='childtext' style='margin-left: .1vw;'>" + lossHierarchy + " </div><div class='childcolor' style='background-color: " + color + "; color:  " + color + "'>.</div></td>";
                                }
                                else
                                {
                                    ColorsAndLossCodes += "<td><div class='childtext' style='margin-left: .1vw;'>" + lossHierarchy + " </div><div class='childcolor' style='background-color: " + color + "; color:  " + color + "'>.</div></td>";
                                }
                            }
                        }
                        ColorsAndLossCodes += "</div></tr></table>";
                        ViewBag.ColorsAndLossCodes = ColorsAndLossCodes;
                    }
                    else
                    {
                        Session["Error"] = "No Data For the Selected Criteria.";
                    }
                }
                #endregion

                #region LowestLevel == Shop
                if (lowestLevel == "Shop")
                {
                    ViewBag.lowestLevel = "Shop";
                    var OEEDataSummarizedToViewShop = obj.GettbloeeshopDet(ShopID, ipAddress, frmDate, toDate);
                    //var OEEDataSummarizedToViewShop = db.tbloeedashboardfinalvariables.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID && m.IPAddress == ipAddress && m.StartDate == frmDate && m.EndDate == toDate && m.IsOverallShopWise == 1).FirstOrDefault();
                    if (OEEDataSummarizedToViewShop != null)
                    {
                        //Guage OEE
                        Session["Error"] = "";
                        ViewBag.Oee = OEEDataSummarizedToViewShop.OEE;
                        ViewBag.Aval = OEEDataSummarizedToViewShop.Availability;
                        if (OEEDataSummarizedToViewShop.Availability > 0)
                        {
                            ViewBag.Perf = OEEDataSummarizedToViewShop.Performance;
                            ViewBag.Qual = OEEDataSummarizedToViewShop.Quality;
                        }
                        else
                        {
                            ViewBag.Perf = 0;
                            ViewBag.Qual = 0;
                        }

                        //Cell OEE
                        string ShopWiseCellLossVals = null;
                        string ShopWiseCellLossNames = null;

                        var firstItem = LossColors.ElementAt(0);
                        //Get Distinct Color codes and Unique lossCodes Lists.
                        //['#9a4d6f', '#c76c47', '#f85115', '#d9b099', '#d4ba2f']; and IPAddress = " + ipAddress + "

                        string GetDistinctLossQuery = null;
                        if (TimingVar != "Today")
                        {
                            GetDistinctLossQuery = "Select Distinct f.lossname from" +
                                                 "(" +
                                                 "Select Distinct a.Loss1Name as lossname from "+ databaseName + ".tbloeedashboardvariables a where ShopID = " + ShopID + " and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                 "Union All " +
                                                 "Select Distinct b.Loss2Name  as lossname from "+ databaseName + ".tbloeedashboardvariables b where ShopID = " + ShopID + " and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                 "Union All " +
                                                 "Select Distinct c.Loss3Name as lossname from "+ databaseName + ".tbloeedashboardvariables c where ShopID = " + ShopID + " and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                 "Union All " +
                                                 "Select Distinct d.Loss4Name as lossname from "+ databaseName + ".tbloeedashboardvariables d where ShopID = " + ShopID + " and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                 "Union All " +
                                                 "Select Distinct e.Loss5Name as lossname from "+ databaseName + ".tbloeedashboardvariables e where ShopID = " + ShopID + " and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                 ") f order by f.lossname desc";
                        }
                        else
                        {
                            GetDistinctLossQuery = "Select Distinct f.lossname from" +
                                                "(" +
                                                "Select Distinct a.Loss1Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday a where ShopID = " + ShopID + " and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                "Union All " +
                                                "Select Distinct b.Loss2Name  as lossname from "+ databaseName + ".tbloeedashboardvariablestoday b where ShopID = " + ShopID + " and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                "Union All " +
                                                "Select Distinct c.Loss3Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday c where ShopID = " + ShopID + " and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                "Union All " +
                                                "Select Distinct d.Loss4Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday d where ShopID = " + ShopID + " and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                "Union All " +
                                                "Select Distinct e.Loss5Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday e where ShopID = " + ShopID + " and StartDate Between '" + frmDate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                ") f order by f.lossname desc";
                        }
                        DataTable GetDistinctLossDT = new DataTable();
                        using (MsqlConnection mc = new MsqlConnection())
                        {
                            mc.open();
                            SqlDataAdapter GetDistinctLossDA = new SqlDataAdapter(GetDistinctLossQuery, mc.msqlConnection);
                            GetDistinctLossDA.Fill(GetDistinctLossDT);
                            mc.close();
                        }
                        List<string> LosscodesList = new List<string>();
                        List<int> LosscodesListIntegers = new List<int>();

                        for (int DTLooper = 0; DTLooper < GetDistinctLossDT.Rows.Count; DTLooper++)
                        {
                            LosscodesListIntegers.Add(Convert.ToInt32(GetDistinctLossDT.Rows[DTLooper][0]));
                        }
                        LosscodesListIntegers.Sort();
                        for (int DTLooper = 0; DTLooper < GetDistinctLossDT.Rows.Count; DTLooper++)
                        {
                            LosscodesList.Add(LosscodesListIntegers[DTLooper].ToString());
                        }

                        //Create a Color and LossCode matching List.
                        var ColorLoseList = new List<KeyValuePair<string, string>>();
                        for (int LossIndex = 0; LossIndex < LosscodesList.Count; LossIndex++)
                        {
                            string losscode = LosscodesList.ElementAt(LossIndex).ToString();
                            string color = LossColors.ElementAt(LossIndex).ToString();
                            ColorLoseList.Add(new KeyValuePair<string, string>(losscode, color));
                        }

                        if (OEEDataSummarizedToViewShop.Loss1Name != "" && OEEDataSummarizedToViewShop.Loss1Name !=null)
                        {
                            ShopWiseCellLossVals = OEEDataSummarizedToViewShop.Loss1Value.ToString();
                            ShopWiseCellLossNames = OEEDataSummarizedToViewShop.Loss1Name;
                            string lossName = OEEDataSummarizedToViewShop.Loss1Name.ToString();
                            List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                            ViewData["ShopWiseCellLossColors"] = colorBasedOnLoss[0];
                        }
                        if (OEEDataSummarizedToViewShop.Loss2Name != "" && OEEDataSummarizedToViewShop.Loss2Name !=null)
                        {
                            ShopWiseCellLossVals += "," + OEEDataSummarizedToViewShop.Loss2Value.ToString();
                            ShopWiseCellLossNames += "," + OEEDataSummarizedToViewShop.Loss2Name;
                            string lossName = OEEDataSummarizedToViewShop.Loss2Name.ToString();
                            List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                            ViewData["ShopWiseCellLossColors"] += "," + colorBasedOnLoss[0];

                        }
                        if (OEEDataSummarizedToViewShop.Loss3Name != "" && OEEDataSummarizedToViewShop.Loss3Name !=null)
                        {
                            ShopWiseCellLossVals += "," + OEEDataSummarizedToViewShop.Loss3Value.ToString();
                            ShopWiseCellLossNames += "," + OEEDataSummarizedToViewShop.Loss3Name;
                            string lossName = OEEDataSummarizedToViewShop.Loss3Name.ToString();
                            List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                            ViewData["ShopWiseCellLossColors"] += "," + colorBasedOnLoss[0];
                        }
                        if (OEEDataSummarizedToViewShop.Loss4Name != "" && OEEDataSummarizedToViewShop.Loss4Name !=null)
                        {
                            ShopWiseCellLossVals += "," + OEEDataSummarizedToViewShop.Loss4Value.ToString();
                            ShopWiseCellLossNames += "," + OEEDataSummarizedToViewShop.Loss4Name;
                            string lossName = OEEDataSummarizedToViewShop.Loss4Name.ToString();
                            List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                            ViewData["ShopWiseCellLossColors"] += "," + colorBasedOnLoss[0];
                        }
                        if (OEEDataSummarizedToViewShop.Loss5Name != "" && OEEDataSummarizedToViewShop.Loss5Name !=null)
                        {
                            ShopWiseCellLossVals += "," + OEEDataSummarizedToViewShop.Loss5Value.ToString();
                            ShopWiseCellLossNames += "," + OEEDataSummarizedToViewShop.Loss5Name;
                            string lossName = OEEDataSummarizedToViewShop.Loss5Name.ToString();
                            List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                            ViewData["ShopWiseCellLossColors"] += "," + colorBasedOnLoss[0];
                        }

                        ViewData["ShopWiseCellLossVals"] = ShopWiseCellLossVals;
                        ViewData["ShopWiseCellLossNames"] = ShopWiseCellLossNames;
                        // OEE And Losses for Shop. Done

                        // Now For Each Cell, gather its OEE And Losses


                        //string ShopWiseCellAvailVals = null;
                        //string ShopWiseCellAvailNames = null;
                        //string ShopWiseCellPerfVals = null;
                        //string ShopWiseCellPerfNames = null;
                        //string ShopWiseCellQualVals = null;
                        //string ShopWiseCellQualNames = null;

                        var OEEDataOfCellToView = obj.GettbloeeshopDet1(ShopID, ipAddress, frmDate, toDate);
                        //var OEEDataOfCellToView = db.tbloeedashboardfinalvariables.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID && m.IPAddress == ipAddress && m.StartDate == frmDate && m.EndDate == toDate && m.IsOverallCellWise == 1).ToList();
                        //to set max value for Y-Axis in Lossess.
                        //ViewBag.Max = ColorLoseList.Max(item => int.Parse(item.Value)); //Wrong
                        ViewBag.Max = obj.GettbloeeshopDe2(ShopID, ipAddress, frmDate, toDate);
                        //ViewBag.Max = db.tbloeedashboardfinalvariables.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID && m.IPAddress == ipAddress && m.StartDate == frmDate && m.EndDate == toDate && m.IsOverallCellWise == 1).Max(m => m.Loss1Value);
                        ViewBag.OverAllMax = obj.GettbloeeshopDet3(ShopID, ipAddress, frmDate, toDate);
                        //ViewBag.OverAllMax = db.tbloeedashboardfinalvariables.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID && m.IPAddress == ipAddress && m.StartDate == frmDate && m.EndDate == toDate && m.IsOverallShopWise == 1).Max(m => m.Loss1Value);
                        int once = 0;
                        if(OEEDataOfCellToView.Count !=0)
                        {
                            foreach (var row in OEEDataOfCellToView)
                            {

                                //#region Inside Cell

                                int CellId = Convert.ToInt32(row.CellID);
                                string CellName = GetCellName(CellId);

                                string ShopWiseCellOEEStuffVals = null;
                                string ShopWiseCellOEEStuffNames = null;

                                ShopWiseCellOEEStuffVals += Convert.ToInt32(row.OEE);
                                ShopWiseCellOEEStuffNames += "OEE";
                                ShopWiseCellOEEStuffVals += "," + Convert.ToInt32(row.Availability);
                                ShopWiseCellOEEStuffNames += ",Avail";
                                if (Convert.ToInt32(row.Availability) > 0)
                                {
                                    ShopWiseCellOEEStuffVals += "," + Convert.ToInt32(row.Performance);
                                    ShopWiseCellOEEStuffNames += ",Perf";
                                    ShopWiseCellOEEStuffVals += "," + Convert.ToInt32(row.Quality);
                                    ShopWiseCellOEEStuffNames += ",Qual";
                                }
                                else
                                {
                                    ShopWiseCellOEEStuffVals += "," + Convert.ToInt32(0);
                                    ShopWiseCellOEEStuffNames += ",Perf";
                                    ShopWiseCellOEEStuffVals += "," + Convert.ToInt32(0);
                                    ShopWiseCellOEEStuffNames += ",Qual";
                                }

                                ViewData["ShopWiseCellOEEStuffCell" + once + ""] = CellName;
                                ViewData["ShopWiseCellOEEStuffVals" + once + ""] = ShopWiseCellOEEStuffVals;
                                ViewData["ShopWiseCellOEEStuffNames" + once + ""] = ShopWiseCellOEEStuffNames;

                                //#region ShopWiseCellLoss Vals & Names
                                ViewData["ShopWiseCellLossValsCell" + once + ""] = CellName; //ShopWiseCellLossValsCell 1
                                if (row.Loss1Name != "")
                                {
                                    ViewData["ShopWiseCellLossVals" + once + ""] = row.Loss1Value.ToString();
                                    ViewData["ShopWiseCellLossNames" + once + ""] = row.Loss1Name;
                                    string lossName = row.Loss1Name.ToString();
                                    List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                                    ViewData["ShopWiseCellLossColors" + once + ""] = colorBasedOnLoss[0];
                                }
                                if (row.Loss2Name != "")
                                {
                                    ViewData["ShopWiseCellLossVals" + once + ""] += "," + row.Loss2Value.ToString();
                                    ViewData["ShopWiseCellLossNames" + once + ""] += "," + row.Loss2Name;
                                    string lossName = row.Loss2Name.ToString();
                                    List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                                    ViewData["ShopWiseCellLossColors" + once + ""] += "," + colorBasedOnLoss[0];
                                }
                                if (row.Loss3Name != "")
                                {
                                    ViewData["ShopWiseCellLossVals" + once + ""] += "," + row.Loss3Value.ToString();
                                    ViewData["ShopWiseCellLossNames" + once + ""] += "," + row.Loss3Name;
                                    string lossName = row.Loss3Name.ToString();
                                    List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                                    ViewData["ShopWiseCellLossColors" + once + ""] += "," + colorBasedOnLoss[0];
                                }
                                if (row.Loss4Name != "")
                                {
                                    ViewData["ShopWiseCellLossVals" + once + ""] += "," + row.Loss4Value.ToString();
                                    ViewData["ShopWiseCellLossNames" + once + ""] += "," + row.Loss4Name;
                                    string lossName = row.Loss4Name.ToString();
                                    List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                                    ViewData["ShopWiseCellLossColors" + once + ""] += "," + colorBasedOnLoss[0];
                                }
                                if (row.Loss5Name != "")
                                {
                                    ViewData["ShopWiseCellLossVals" + once + ""] += "," + row.Loss5Value.ToString();
                                    ViewData["ShopWiseCellLossNames" + once + ""] += "," + row.Loss5Name;
                                    string lossName = row.Loss5Name.ToString();
                                    List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                                    ViewData["ShopWiseCellLossColors" + once + ""] += "," + colorBasedOnLoss[0];
                                }


                                // OEE And Losses for This Cell. Done

                                // Now For Each WC gather OEE And Losses


                                var OEEDataOfWCToView = obj.GettbloeeshopDet2(CellID, ShopID, ipAddress, frmDate, toDate);
                                //var OEEDataOfWCToView = db.tbloeedashboardfinalvariables.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID && m.CellID == CellId && m.IPAddress == ipAddress && m.StartDate == frmDate && m.EndDate == toDate && m.IsOverallWCWise == 1).ToList();
                                //to set max value for Y-Axis in Lossess.
                                //ViewBag.Max = ColorLoseList.Max(item => int.Parse(item.Value)); //Wrong
                                //ViewBag.Max = db.tbloeedashboardfinalvariables.Where(m => m.IsDeleted == 0 && m.CellID == CellID && m.IPAddress == ipAddress && m.StartDate == frmDate && m.EndDate == toDate && m.IsOverallWCWise == 0).Max(m => m.Loss1Value);
                                //ViewBag.OverAllMax = db.tbloeedashboardfinalvariables.Where(m => m.IsDeleted == 0 && m.CellID == CellID && m.IPAddress == ipAddress && m.StartDate == frmDate && m.EndDate == toDate && m.IsOverallWCWise == 1).Max(m => m.Loss1Value);


                                int Cellonce = 0;
                                #region WC Data for 1 Cell
                                foreach (var WCrow in OEEDataOfWCToView)
                                {

                                    int machineId = Convert.ToInt32(WCrow.WCID);
                                    string MacInvNo = GetMacInvNo(machineId);

                                    string CellWiseWCOEEAndStuffVals = null;
                                    string CellWiseWCOEEAndStuffNames = null;

                                    CellWiseWCOEEAndStuffVals += Convert.ToInt32(WCrow.OEE);
                                    CellWiseWCOEEAndStuffNames += "OEE";
                                    CellWiseWCOEEAndStuffVals += "," + Convert.ToInt32(WCrow.Availability);
                                    CellWiseWCOEEAndStuffNames += ",Avail";

                                    if (Convert.ToInt32(row.Availability) > 0)
                                    {
                                        CellWiseWCOEEAndStuffVals += "," + Convert.ToInt32(WCrow.Performance);
                                        CellWiseWCOEEAndStuffNames += ",Perf";
                                        CellWiseWCOEEAndStuffVals += "," + Convert.ToInt32(WCrow.Quality);
                                        CellWiseWCOEEAndStuffNames += ",Qual";
                                    }
                                    else
                                    {
                                        CellWiseWCOEEAndStuffVals += "," + Convert.ToInt32(0);
                                        CellWiseWCOEEAndStuffNames += ",Perf";
                                        CellWiseWCOEEAndStuffVals += "," + Convert.ToInt32(0);
                                        CellWiseWCOEEAndStuffNames += ",Qual";
                                    }

                                    ViewData["CellWiseWCOEEStuffMac" + Cellonce + ""] = MacInvNo;
                                    ViewData["CellWiseWCOEEStuffVals" + Cellonce + ""] = CellWiseWCOEEAndStuffVals;
                                    ViewData["CellWiseWCOEEStuffNames" + Cellonce + ""] = CellWiseWCOEEAndStuffNames;

                                    ViewData["CellWiseWCLossValsMac" + Cellonce + ""] = MacInvNo;
                                    if (WCrow.Loss1Name != "")
                                    {
                                        ViewData["CellWiseWCLossVals" + Cellonce + ""] = WCrow.Loss1Value.ToString();
                                        ViewData["CellWiseWCLossNames" + Cellonce + ""] = WCrow.Loss1Name;
                                        string lossName = WCrow.Loss1Name.ToString();
                                        List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                                        ViewData["CellWiseWCLossColors" + Cellonce + ""] = colorBasedOnLoss[0];
                                    }
                                    if (WCrow.Loss2Name != "")
                                    {
                                        ViewData["CellWiseWCLossVals" + Cellonce + ""] += "," + WCrow.Loss2Value.ToString();
                                        ViewData["CellWiseWCLossNames" + Cellonce + ""] += "," + WCrow.Loss2Name;
                                        string lossName = WCrow.Loss2Name.ToString();
                                        List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                                        ViewData["CellWiseWCLossColors" + Cellonce + ""] += "," + colorBasedOnLoss[0];
                                    }
                                    if (WCrow.Loss3Name != "")
                                    {
                                        ViewData["CellWiseWCLossVals" + Cellonce + ""] += "," + WCrow.Loss3Value.ToString();
                                        ViewData["CellWiseWCLossNames" + Cellonce + ""] += "," + WCrow.Loss3Name;
                                        string lossName = WCrow.Loss3Name.ToString();
                                        List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                                        ViewData["CellWiseWCLossColors" + Cellonce + ""] += "," + colorBasedOnLoss[0];
                                    }
                                    if (WCrow.Loss4Name != "")
                                    {
                                        ViewData["CellWiseWCLossVals" + Cellonce + ""] += "," + WCrow.Loss4Value.ToString();
                                        ViewData["CellWiseWCLossNames" + Cellonce + ""] += "," + WCrow.Loss4Name;
                                        string lossName = WCrow.Loss4Name.ToString();
                                        List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                                        ViewData["CellWiseWCLossColors" + Cellonce + ""] += "," + colorBasedOnLoss[0];
                                    }
                                    if (WCrow.Loss5Name != "")
                                    {
                                        ViewData["CellWiseWCLossVals" + Cellonce + ""] += "," + WCrow.Loss5Value.ToString();
                                        ViewData["CellWiseWCLossNames" + Cellonce + ""] += "," + WCrow.Loss5Name;
                                        string lossName = WCrow.Loss5Name.ToString();
                                        List<string> colorBasedOnLoss = (from kvp in ColorLoseList where kvp.Key == lossName select kvp.Value).ToList();
                                        ViewData["CellWiseWCLossColors" + Cellonce + ""] += "," + colorBasedOnLoss[0];
                                    }
                                    Cellonce++;
                                }
                                #endregion

                                ViewData["CellWiseWCCount" + once + ""] = Cellonce;
                                once++;//Cells Indexer
                            }
                        }
                        
                        //#endregion
                        ViewData["ShopWiseCellCount"] = once;

                        //Colors and Losses Hierarchy Data. from ColorLoseList
                        #region Purely for Loss<->Colors Display
                        string ColorsAndLossCodes = null;
                        ColorsAndLossCodes = "<table><tr><div class='parent'>";
                        for (int Looper = 0; Looper < ColorLoseList.Count; Looper++)
                        {
                            int losscode = Convert.ToInt32(ColorLoseList[Looper].Key);
                            if (losscode != 0)
                            {
                                string lossHierarchy = null;
                                if (losscode != 0)
                                {
                                    lossHierarchy = LossHierarchy(losscode);
                                }
                                string color = Convert.ToString(ColorLoseList[Looper].Value);
                                if (Looper % 4 == 0 && Looper != 0)
                                {
                                    ColorsAndLossCodes += "</div></tr><tr><td><div class='parent'><div class='childtext' style='margin-left: .1vw;'>" + lossHierarchy + " </div><div class='childcolor' style='background-color: " + color + "; color:  " + color + "'>.</div></td>";
                                }
                                else
                                {
                                    ColorsAndLossCodes += "<td><div class='childtext' style='margin-left: .1vw;'>" + lossHierarchy + " </div><div class='childcolor' style='background-color: " + color + "; color:  " + color + "'>.</div></td>";
                                }
                            }
                        }
                        ColorsAndLossCodes += "</div></tr></table>";
                        ViewBag.ColorsAndLossCodes = ColorsAndLossCodes;
                        #endregion

                    }
                    else
                    {
                        Session["Error"] = "No Data For the Selected Criteria.";
                    }
                }
                #endregion
            }
            ViewData["PlantID"] = new SelectList(obj.GetPlantList(), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(obj.GetShop1List(PlantID), "ShopID", "ShopName", ShopID);
            ViewData["CellID"] = new SelectList(obj.GetCellList1(ShopID, PlantID), "CellID", "CellName", CellID);            //ViewData["PlantID"] = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            //ViewData["ShopID"] = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName", ShopID);
            //ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID), "CellID", "CellName", CellID);
            ViewData["WorkCenterID"] = new SelectList(obj.GetmachineList(PlantID, ShopID, WorkCenterID), "MachineID", "MachineInvNo");
            // ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.MachineID == WorkCenterID && m.IsNormalWC == 0), "MachineID", "MachineInvNo");

            var plantName = obj.GettbplantDet(PlantID);
            //string plantName = db.tblplants.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID).Select(m => m.PlantName).FirstOrDefault();
            var shopName = obj.GettbShopIDDet(ShopID);
            // string shopName = db.tblshops.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).Select(m => m.ShopName).FirstOrDefault();
            if (PlantID != 0)
            {
                if (CellID != 0)
                {
                    var cellName = obj.GettbcellDet(CellID);
                    //string cellName = db.tblcells.Where(m => m.IsDeleted == 0 && m.CellID == CellID).Select(m => m.CellName).FirstOrDefault();
                    ViewData["CellID"] = new SelectList(obj.GetCellList1(ShopID, PlantID), "CellID", "CellName", CellID);
                    //ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID), "CellID", "CellName", CellID);

                    if (WorkCenterID != 0)
                    {
                        var wcName = obj.GettbMachineDet(WorkCenterID);
                        //string wcName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == WorkCenterID).Select(m => m.MachineInvNo).FirstOrDefault();
                        ViewBag.SelectedPath = additionToPath + " => "+ plantName + " => " + shopName + " => " + cellName + " => " + wcName.MachineDisplayName + " From: " + frmDate.ToString("yyyy-MM-dd") + " To: " + toDate.ToString("yyyy-MM-dd");
                        ViewData["WorkCenterID"] = new SelectList(obj.GetmachineList(PlantID, ShopID, WorkCenterID), "MachineID", "MachineInvNo");
                        //ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID && m.IsNormalWC == 0), "MachineID", "MachineInvNo");
                    }
                    else
                    {
                        ViewBag.SelectedPath = additionToPath + " => " + plantName + " => " + shopName + " => " + cellName + " From: " + frmDate.ToString("yyyy-MM-dd") + " To: " + toDate.ToString("yyyy-MM-dd");
                        ViewData["WorkCenterID"] = new SelectList(obj.GetmachineList(PlantID, ShopID, WorkCenterID), "MachineID", "MachineInvNo");
                        //ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.IsNormalWC == 0), "MachineID", "MachineInvNo");                        
                    }
                    Session["PSMCDet"] = ViewBag.SelectedPath;
                }
                else
                {
                    ViewBag.SelectedPath = additionToPath + " => " + plantName + " => " + shopName + " From: " + frmDate.ToString("yyyy-MM-dd") + " To: " + toDate.ToString("yyyy-MM-dd");
                    ViewData["CellID"] = new SelectList(obj.GetCellList1(ShopID, PlantID), "CellID", "CellName", CellID);
                    // ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID), "CellID", "CellName");
                    if (WorkCenterID != 0)
                    {
                        var wcName = obj.GettbMachineDet(WorkCenterID);
                        //string wcName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == WorkCenterID).Select(m => m.MachineInvNo).FirstOrDefault();
                        ViewBag.SelectedPath = additionToPath + " => " + plantName + " => " + shopName + " => " + wcName.MachineDisplayName + " From: " + frmDate.ToString("yyyy-MM-dd") + " To: " + toDate.ToString("yyyy-MM-dd");
                        ViewData["WorkCenterID"] = new SelectList(obj.GetmachineList(PlantID, ShopID, WorkCenterID), "MachineID", "MachineInvNo");
                        //ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.IsNormalWC == 0), "MachineID", "MachineInvNo");
                    }
                    else
                    {
                        ViewBag.SelectedPath = additionToPath + " => " + plantName + " => " + shopName + " From: " + frmDate.ToString("yyyy-MM-dd") + " To: " + toDate.ToString("yyyy-MM-dd");
                        ViewData["WorkCenterID"] = new SelectList(obj.GetmachineList(PlantID, ShopID, WorkCenterID), "MachineID", "MachineInvNo");
                        //ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.IsNormalWC == 0), "MachineID", "MachineInvNo");
                    }
                    Session["PSMCDet"] = ViewBag.SelectedPath;
                }
            }
            return View();
        }

        public string GetMacInvNo(int machineId)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            string macinv = null;
            macinv = Convert.ToString(obj.GettbMachineDet1(machineId));
            // macinv = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == machineId && m.IsNormalWC == 0).Select(m => m.MachineInvNo).FirstOrDefault();
            return macinv;
        }

        public string GetCellName(int CellId)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            string cellName = null;
            cellName = Convert.ToString(obj.GettbcellDet(CellId));
            //cellName = db.tblcells.Where(m => m.IsDeleted == 0 && m.CellID == CellId).Select(m => m.CellName).FirstOrDefault();
            return cellName;
        }

        public string GetShopName(int ShopId)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            string shopName = null;
            shopName = Convert.ToString(obj.GettbShopIDDet(ShopId));
            //shopName = db.tblshops.Where(m => m.IsDeleted == 0 && m.ShopID == ShopId).Select(m => m.ShopName).FirstOrDefault();
            return shopName;
        }

        //OEE for Plant
        public void CalculateSummarizedOEEPlant(DateTime fromdate, DateTime todate, int FactorID, string TimeType, string SummarizeAs = null)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);

            DateTime UsedDateForExcel = Convert.ToDateTime(fromdate);
            double TotalDay = todate.Subtract(fromdate).TotalDays;
            if (TimeType == "Today")
            {

                //now push to tbloeedashboardFinalVariables.
                double OEEFactor, AvaillabilityFactor, EfficiencyFactor, QualityFactor;
                //Gather Hierarchy Details
                string PlantIDS = null, ShopIDS = null, CellIDS = null, WCID = null;
                string ipAddress = GetIPAddressOf();
                var getPlant = obj.GettbloeeListDet1(FactorID, ipAddress, fromdate, todate);
                //var getPlant = db.tbloeedashboardvariablestodays.Where(m => m.IsDeleted == 0 && m.PlantID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress).Select(m => m.PlantID).Distinct().ToList();

                if (SummarizeAs == "Plant")
                {
                    getPlant = obj.GettbloeeListDet1(FactorID, ipAddress, fromdate, todate);
                    // getPlant = dboee.tbloeedashboardvariablestodays.Where(m => m.IsDeleted == 0 && m.PlantID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress).Select(m => m.PlantID).Distinct().ToList();

                }

                foreach (var PlantRow in getPlant)
                {
                    double green = 0, red = 0, yellow = 0, blue = 0, setup = 0, scrap = 0, OperatingTime = 0, DownTimeBreakdown = 0, ROALossess = 0, AvailableTime = 0, SettingTime = 0, PlannedDownTime = 0, UnPlannedDownTime = 0;
                    double SummationOfSCTvsPP = 0, MinorLosses = 0, ROPLosses = 0;
                    double ScrapQtyTime = 0, ReWOTime = 0, ROQLosses = 0;
                    int id = Convert.ToInt32(PlantRow);
                    var OEEDataToSummarize = obj.GettbloeeListDet2(id, ipAddress, fromdate, todate);
                    //var OEEDataToSummarize = db.tbloeedashboardvariablestodays.Where(m => m.PlantID == PlantRow && m.StartDate >= PlantRow && m.StartDate <= todate && m.IPAddress == ipAddress).ToList();
                    CellIDS = FactorID.ToString();
                    int once = 0;
                    foreach (var row in OEEDataToSummarize)
                    {
                        if (once == 0)
                        {
                            PlantIDS = Convert.ToString(row.PlantID);
                            ShopIDS = Convert.ToString(row.ShopID);
                            CellIDS = Convert.ToString(row.CellID);
                            WCID = Convert.ToString(row.WCID);
                            once++;
                        }

                        MinorLosses += Convert.ToDouble(row.MinorLosses);
                        blue += Convert.ToDouble(row.Blue);
                        green += Convert.ToDouble(row.Green);

                        //Availability
                        SettingTime += Convert.ToDouble(row.SettingTime);
                        ROALossess += Convert.ToDouble(row.ROALossess);
                        DownTimeBreakdown += Convert.ToDouble(row.DownTimeBreakdown);

                        //Performance
                        SummationOfSCTvsPP += Convert.ToDouble(row.SummationOfSCTvsPP);

                        //Quality
                        ScrapQtyTime += Convert.ToDouble(row.ScrapQtyTime);
                        ReWOTime += Convert.ToDouble(row.ReWOTime);
                    }
                    if (TimeType == "Today")
                    {
                        if (DateTime.Now.Hour > 6)
                        {
                            AvailableTime = (DateTime.Now - Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 06:00:00"))).TotalMinutes;
                        }
                        else
                        {
                            AvailableTime = (DateTime.Now - Convert.ToDateTime(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 06:00:00"))).TotalMinutes;
                        }
                    }
                    #region Getting the Top 5 Losses
                    //Have to Calculate based on Summarization condition, use code in CalculateOEE().
                    string GetDistinctLossQuery = "Select Distinct f.lossname from" +
                                             "(" +
                                             "Select Distinct a.Loss1Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday a where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct b.Loss2Name  as lossname from "+ databaseName + ".tbloeedashboardvariablestoday b where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct c.Loss3Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday c where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct d.Loss4Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday d where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct e.Loss5Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday e where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             ") f order by f.lossname desc";
                    DataTable GetDistinctLossDT = new DataTable();
                    using (MsqlConnection mc = new MsqlConnection())
                    {
                        mc.open();
                        SqlDataAdapter GetDistinctLossDA = new SqlDataAdapter(GetDistinctLossQuery, mc.msqlConnection);
                        GetDistinctLossDA.Fill(GetDistinctLossDT);
                        mc.close();
                    }
                    //Gather LossValues
                    string lossCode1 = null, lossCode2 = null, lossCode3 = null, lossCode4 = null, lossCode5 = null;
                    int lossCodeVal1 = 0, lossCodeVal2 = 0, lossCodeVal3 = 0, lossCodeVal4 = 0, lossCodeVal5 = 0;
                    List<KeyValuePair<String, Double>> LossDetails = new List<KeyValuePair<string, double>>();
                    int LossCount = GetDistinctLossDT.Rows.Count;
                    for (int i = 0; i < LossCount; i++)
                    {
                        String LossName = GetDistinctLossDT.Rows[i][0].ToString();
                        double LossDuration = 0;
                        String GetLossDurationQuery = "Select SUM(Total) From " +
                                       "(Select SUM(Loss1Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday " +
                                       "Where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss1Name = '" + LossName + "'  " +
                                       "Union all " +
                                       "Select SUM(Loss2Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday  " +
                                       "Where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss2Name = '" + LossName + "'  " +
                                       "Union All " +
                                       "Select SUM(Loss3Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday  " +
                                       "Where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss3Name = '" + LossName + "' " +
                                       "Union All " +
                                       "Select SUM(Loss4Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday  " +
                                       "Where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss4Name = '" + LossName + "' " +
                                       "Union All " +
                                       "Select SUM(Loss5Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday " +
                                       "Where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss5Name = '" + LossName + "' " +
                                       ") z;";
                        DataTable GetLossDurationDT = new DataTable();
                        using (MsqlConnection mc = new MsqlConnection())
                        {
                            mc.open();
                            SqlDataAdapter GetLossDurationDA = new SqlDataAdapter(GetLossDurationQuery, mc.msqlConnection);
                            GetLossDurationDA.Fill(GetLossDurationDT);
                            mc.close();
                        }
                        if (GetLossDurationDT.Rows.Count > 0)
                        {
                            string value = Convert.ToString(GetLossDurationDT.Rows[0][0]);
                            if (Double.TryParse(value, out LossDuration))
                                LossDuration = LossDuration;
                        }
                        LossDetails.Add(new KeyValuePair<string, double>(LossName, LossDuration));
                    }
                    var losslist = LossDetails.OrderByDescending(m => m.Value).ToList();
                    int losscount = losslist.Count;

                    for (int i = 0; i < losscount; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                lossCode1 = losslist[i].Key.ToString();
                                lossCodeVal1 = (int)losslist[i].Value;
                                break;
                            case 1:
                                lossCode2 = losslist[i].Key.ToString();
                                lossCodeVal2 = (int)losslist[i].Value;
                                break;
                            case 2:
                                lossCode3 = losslist[i].Key.ToString();
                                lossCodeVal3 = (int)losslist[i].Value;
                                break;
                            case 3:
                                lossCode4 = losslist[i].Key.ToString();
                                lossCodeVal4 = (int)losslist[i].Value;
                                break;
                            case 4:
                                lossCode5 = losslist[i].Key.ToString();
                                lossCodeVal5 = (int)losslist[i].Value;
                                break;
                        }
                    }
                    #endregion
                    //OEE Values
                    OperatingTime = green;
                    //Availability Factor
                    double val = OperatingTime / AvailableTime;
                    AvaillabilityFactor = Math.Round(val * 100, 2);
                    if (AvaillabilityFactor > 0 && AvaillabilityFactor < 100)
                    {
                        AvaillabilityFactor = Math.Round(val * 100, 2);
                    }
                    else
                    {
                        if (AvaillabilityFactor > 100)
                        {
                            EfficiencyFactor = 100;
                        }
                        else if (AvaillabilityFactor < 0)
                        {
                            EfficiencyFactor = 0;
                        }
                    }

                    //Performance(Efficiency) Factor
                    if (OperatingTime == 0)
                    {
                        EfficiencyFactor = 100;
                    }
                    else
                    {
                        EfficiencyFactor = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 2);
                        if (SummationOfSCTvsPP == -1 || SummationOfSCTvsPP == 0)
                        {
                            EfficiencyFactor = 100;
                        }
                        else
                        {
                            EfficiencyFactor = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 2);
                            if (EfficiencyFactor >= 0 && EfficiencyFactor <= 100)
                            {
                            }
                            else if (EfficiencyFactor > 100 || EfficiencyFactor < 0)
                            {
                                EfficiencyFactor = 100;
                            }
                        }
                    }

                    //QualityFactor
                    if (OperatingTime == 0)
                    {
                        QualityFactor = 0;
                    }
                    else
                    {
                        QualityFactor = (OperatingTime - ScrapQtyTime - ReWOTime) / OperatingTime;
                        if (QualityFactor >= 0 && QualityFactor <= 100)
                        {
                            QualityFactor = Math.Round(QualityFactor * 100, 2);
                        }
                        else if (QualityFactor > 100 || QualityFactor < 0)
                        {
                            QualityFactor = 100;
                        }
                    }

                    //OEE Factor
                    if (AvaillabilityFactor <= 0 || EfficiencyFactor <= 0 || QualityFactor <= 0)
                    {
                        OEEFactor = 0;
                    }
                    else
                    {
                        OEEFactor = Math.Round((AvaillabilityFactor / 100) * (EfficiencyFactor / 100) * (QualityFactor / 100) * 100, 2);
                        if (OEEFactor >= 0 && OEEFactor <= 100)
                        {
                            OEEFactor = Math.Round(OEEFactor, 2);
                        }
                        else if (OEEFactor > 100)
                        {
                            OEEFactor = 100;
                        }
                        else if (OEEFactor < 0)
                        {
                            OEEFactor = 0;
                        }
                    }
                    string IPAddress = GetIPAddressOf();
                    int isShopWise = 0;
                    int isCellWise = 1;

                    //Now insert into table
                    using (MsqlConnection mcFinalInsertRows = new MsqlConnection())
                    {
                        mcFinalInsertRows.open();
                        try
                        {
                            try
                            {
                                obj.deletetbloeedashboardfinalvariablesDetails2(ipAddress,Convert.ToInt32(WCID));
                                //db.tbloeedashboardfinalvariables.RemoveRange(db.tbloeedashboardfinalvariables.Where(x => x.IPAddress == IPAddress));
                                //db.SaveChanges();

                            }
                            catch (Exception e)
                            {
                            }

                            SqlCommand cmdFinalInsertRows = new SqlCommand("INSERT INTO tbloeedashboardfinalvariables(PlantID,ShopID,CellID,WCID,StartDate,EndDate,OEE,Availability,Performance,Quality,IsOverallShopWise,IsOverallWCWise,Loss1Name,Loss1Value,Loss2Name,Loss2Value,Loss3Name,Loss3Value,Loss4Name,Loss4Value,Loss5Name,Loss5Value,IPAddress,CreatedOn,CreatedBy,IsDeleted,IsToday)VALUES('" + Convert.ToInt32(PlantRow) + "','" + Convert.ToInt32(ShopIDS) + "','" + Convert.ToInt32(CellIDS) + "','" + Convert.ToInt32(WCID) + "','" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + OEEFactor + "','" + AvaillabilityFactor + "','" + EfficiencyFactor + "','" + QualityFactor + "','" + isShopWise + "','" + isCellWise + "','" + lossCode1 + "','" + lossCodeVal1 + "','" + lossCode2 + "','" + lossCodeVal2 + "','" + lossCode3 + "','" + lossCodeVal3 + "','" + lossCode4 + "','" + lossCodeVal4 + "','" + lossCode5 + "','" + lossCodeVal5 + "','" + IPAddress + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + 1 + "','" + 0 + "','" + 1 + "');", mcFinalInsertRows.msqlConnection);
                            cmdFinalInsertRows.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                        }
                        finally
                        {
                            mcFinalInsertRows.close();
                        }
                    }
                }
            }
            else if (TimeType == "GodHours" || TimeType == "NoBlue")
            {
                //now push to tbloeedashboardFinalVariables.
                double OEEFactor, AvaillabilityFactor, EfficiencyFactor, QualityFactor;
                //Gather Hierarchy Details
                string PlantIDS = null, ShopIDS = null, CellIDS = null, WCID = null;
                var getPlant = obj.GettbloeeListDet3(FactorID, fromdate, todate);
                //var getPlant = db.tbloeedashboardvariables.Where(m => m.IsDeleted == 0 && m.PlantID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate).Select(m => m.PlantID).Distinct().ToList();

                if (SummarizeAs == "Plant")
                {
                    getPlant = obj.GettbloeeListDet3(FactorID, fromdate, todate);
                    //getPlant = db.tbloeedashboardvariables.Where(m => m.IsDeleted == 0 && m.PlantID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate).Select(m => m.PlantID).Distinct().ToList();
                }

                foreach (var PlantRow in getPlant)
                {
                    double green = 0, red = 0, yellow = 0, blue = 0, setup = 0, scrap = 0, OperatingTime = 0, DownTimeBreakdown = 0, ROALossess = 0, AvailableTime = 0, SettingTime = 0, PlannedDownTime = 0, UnPlannedDownTime = 0;
                    double SummationOfSCTvsPP = 0, MinorLosses = 0, ROPLosses = 0;
                    double ScrapQtyTime = 0, ReWOTime = 0, ROQLosses = 0;
                    int id = Convert.ToInt32(PlantRow);
                    var OEEDataToSummarize = obj.GettbloeeListDet4(id, fromdate, todate);
                    //var OEEDataToSummarize = db.tbloeedashboardvariables.Where(m => m.PlantID == PlantRow && m.StartDate >= fromdate && m.StartDate <= todate).ToList();
                    CellIDS = FactorID.ToString();
                    int once = 0;
                    foreach (var row in OEEDataToSummarize)
                    {
                        if (once == 0)
                        {
                            PlantIDS = Convert.ToString(row.PlantID);
                            ShopIDS = Convert.ToString(row.ShopID);
                            CellIDS = Convert.ToString(row.CellID);
                            WCID = Convert.ToString(row.WCID);
                            once++;
                        }

                        MinorLosses += Convert.ToDouble(row.MinorLosses);
                        blue += Convert.ToDouble(row.Blue);
                        green += Convert.ToDouble(row.Green);

                        //Availability
                        SettingTime += Convert.ToDouble(row.SettingTime);
                        ROALossess += Convert.ToDouble(row.ROALossess);
                        DownTimeBreakdown += Convert.ToDouble(row.DownTimeBreakdown);

                        //Performance
                        SummationOfSCTvsPP += Convert.ToDouble(row.SummationOfSCTvsPP);

                        //Quality
                        ScrapQtyTime += Convert.ToDouble(row.ScrapQtyTime);
                        ReWOTime += Convert.ToDouble(row.ReWOTime);
                    }
                    int Days = Convert.ToInt32(todate.Subtract(fromdate).TotalDays) + 1;
                    if (TimeType == "GodHours")
                    {
                        AvailableTime = OEEDataToSummarize.Count * 24 * 60 * Days; //24Hours to Minutes;
                    }
                    else if (TimeType == "NoBlue")
                    {
                        AvailableTime = (OEEDataToSummarize.Count * 24 * 60 * Days) - blue; //24Hours to Minutes;
                    }
                    #region Getting the Top 5 Losses
                    //Have to Calculate based on Summarization condition, use code in CalculateOEE().
                    string GetDistinctLossQuery = "Select Distinct f.lossname from" +
                                             "(" +
                                             "Select Distinct a.Loss1Name as lossname from "+ databaseName + ".tbloeedashboardvariables a where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct b.Loss2Name  as lossname from "+ databaseName + ".tbloeedashboardvariables b where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct c.Loss3Name as lossname from "+ databaseName + ".tbloeedashboardvariables c where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct d.Loss4Name as lossname from "+ databaseName + ".tbloeedashboardvariables d where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct e.Loss5Name as lossname from "+ databaseName + ".tbloeedashboardvariables e where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             ") f order by f.lossname desc";
                    DataTable GetDistinctLossDT = new DataTable();
                    using (MsqlConnection mc = new MsqlConnection())
                    {
                        mc.open();
                        SqlDataAdapter GetDistinctLossDA = new SqlDataAdapter(GetDistinctLossQuery, mc.msqlConnection);
                        GetDistinctLossDA.Fill(GetDistinctLossDT);
                        mc.close();
                    }
                    //Gather LossValues
                    string lossCode1 = null, lossCode2 = null, lossCode3 = null, lossCode4 = null, lossCode5 = null;
                    int lossCodeVal1 = 0, lossCodeVal2 = 0, lossCodeVal3 = 0, lossCodeVal4 = 0, lossCodeVal5 = 0;
                    List<KeyValuePair<String, Double>> LossDetails = new List<KeyValuePair<string, double>>();
                    int LossCount = GetDistinctLossDT.Rows.Count;
                    for (int i = 0; i < LossCount; i++)
                    {
                        String LossName = GetDistinctLossDT.Rows[i][0].ToString();
                        double LossDuration = 0;
                        DataTable GetLossDurationDT = new DataTable();
                        String GetLossDurationQuery = "Select SUM(Total) From " +
                                       "(Select SUM(Loss1Value)  as Total From "+ databaseName + ".tbloeedashboardvariables " +
                                       "Where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss1Name = '" + LossName + "'  " +
                                       "Union all " +
                                       "Select SUM(Loss2Value)  as Total From "+ databaseName + ".tbloeedashboardvariables  " +
                                       "Where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss2Name = '" + LossName + "'  " +
                                       "Union All " +
                                       "Select SUM(Loss3Value)  as Total From "+ databaseName + ".tbloeedashboardvariables  " +
                                       "Where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss3Name = '" + LossName + "' " +
                                       "Union All " +
                                       "Select SUM(Loss4Value)  as Total From "+ databaseName + ".tbloeedashboardvariables  " +
                                       "Where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss4Name = '" + LossName + "' " +
                                       "Union All " +
                                       "Select SUM(Loss5Value)  as Total From "+ databaseName + ".tbloeedashboardvariables " +
                                       "Where PlantID = " + PlantRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss5Name = '" + LossName + "' " +
                                       ") z;";
                        using (MsqlConnection mc = new MsqlConnection())
                        {
                            mc.open();
                            SqlDataAdapter GetLossDurationDA = new SqlDataAdapter(GetLossDurationQuery, mc.msqlConnection);
                            GetLossDurationDA.Fill(GetLossDurationDT);
                            mc.close();
                        }
                        if (GetLossDurationDT.Rows.Count > 0)
                        {
                            string value = Convert.ToString(GetLossDurationDT.Rows[0][0]);
                            if (Double.TryParse(value, out LossDuration))
                                LossDuration = LossDuration;
                        }
                        LossDetails.Add(new KeyValuePair<string, double>(LossName, LossDuration));
                    }
                    var losslist = LossDetails.OrderByDescending(m => m.Value).ToList();
                    int losscount = losslist.Count;

                    for (int i = 0; i < losscount; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                lossCode1 = losslist[i].Key.ToString();
                                lossCodeVal1 = (int)losslist[i].Value;
                                break;
                            case 1:
                                lossCode2 = losslist[i].Key.ToString();
                                lossCodeVal2 = (int)losslist[i].Value;
                                break;
                            case 2:
                                lossCode3 = losslist[i].Key.ToString();
                                lossCodeVal3 = (int)losslist[i].Value;
                                break;
                            case 3:
                                lossCode4 = losslist[i].Key.ToString();
                                lossCodeVal4 = (int)losslist[i].Value;
                                break;
                            case 4:
                                lossCode5 = losslist[i].Key.ToString();
                                lossCodeVal5 = (int)losslist[i].Value;
                                break;
                        }
                    }
                    #endregion
                    //OEE Values
                    OperatingTime = green;
                    //Availability Factor
                    double val = OperatingTime / AvailableTime;
                    AvaillabilityFactor = Math.Round(val * 100, 2);
                    if (AvaillabilityFactor > 0 && AvaillabilityFactor < 100)
                    {
                        AvaillabilityFactor = Math.Round(val * 100, 2);
                    }
                    else
                    {
                        if (AvaillabilityFactor > 100)
                        {
                            EfficiencyFactor = 100;
                        }
                        else if (AvaillabilityFactor < 0)
                        {
                            EfficiencyFactor = 0;
                        }
                    }

                    //Performance(Efficiency) Factor
                    EfficiencyFactor = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 2);
                    if (SummationOfSCTvsPP == -1 || SummationOfSCTvsPP == 0)
                    {
                        EfficiencyFactor = 100;
                    }
                    else
                    {
                        EfficiencyFactor = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 2);
                        if (EfficiencyFactor >= 0 && EfficiencyFactor <= 100)
                        {
                        }
                        else if (EfficiencyFactor > 100 || EfficiencyFactor < 0)
                        {
                            EfficiencyFactor = 100;
                        }
                    }

                    //QualityFactor
                    QualityFactor = (OperatingTime - ScrapQtyTime - ReWOTime) / OperatingTime;
                    if (QualityFactor >= 0 && QualityFactor <= 100)
                    {
                        QualityFactor = Math.Round(QualityFactor * 100, 2);
                    }
                    else if (QualityFactor > 100 || QualityFactor < 0)
                    {
                        QualityFactor = 100;
                    }

                    //OEE Factor
                    if (AvaillabilityFactor <= 0 || EfficiencyFactor <= 0 || QualityFactor <= 0)
                    {
                        OEEFactor = 0;
                    }
                    else
                    {
                        OEEFactor = Math.Round((AvaillabilityFactor / 100) * (EfficiencyFactor / 100) * (QualityFactor / 100) * 100, 2);
                        if (OEEFactor >= 0 && OEEFactor <= 100)
                        {
                            OEEFactor = Math.Round(OEEFactor, 2);
                        }
                        else if (OEEFactor > 100)
                        {
                            OEEFactor = 100;
                        }
                        else if (OEEFactor < 0)
                        {
                            OEEFactor = 0;
                        }
                    }
                    string IPAddress = GetIPAddressOf();
                    int isShopWise = 0;
                    int isCellWise = 1;

                    //Now insert into table
                    using (MsqlConnection mcFinalInsertRows = new MsqlConnection())
                    {
                        mcFinalInsertRows.open();
                        try
                        {

                            try
                            {
                                obj.deletetbloeedashboardfinalvariablesDetails2(IPAddress,Convert.ToInt32(WCID));
                                //db.tbloeedashboardfinalvariables.RemoveRange(dboee.tbloeedashboardfinalvariables.Where(x => x.IPAddress == IPAddress));
                                //    db.SaveChanges();
                            }
                            catch (Exception e)
                            {
                            }

                            SqlCommand cmdFinalInsertRows = new SqlCommand("INSERT INTO tbloeedashboardfinalvariables(PlantID,ShopID,CellID,WCID,StartDate,EndDate,OEE,Availability,Performance,Quality,IsOverallShopWise,IsOverallWCWise,Loss1Name,Loss1Value,Loss2Name,Loss2Value,Loss3Name,Loss3Value,Loss4Name,Loss4Value,Loss5Name,Loss5Value,IPAddress,CreatedOn,CreatedBy,IsDeleted)VALUES('" + Convert.ToInt32(PlantRow) + "','" + Convert.ToInt32(ShopIDS) + "','" + Convert.ToInt32(CellIDS) + "','" + Convert.ToInt32(WCID) + "','" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + OEEFactor + "','" + AvaillabilityFactor + "','" + EfficiencyFactor + "','" + QualityFactor + "','" + isShopWise + "','" + isCellWise + "','" + lossCode1 + "','" + lossCodeVal1 + "','" + lossCode2 + "','" + lossCodeVal2 + "','" + lossCode3 + "','" + lossCodeVal3 + "','" + lossCode4 + "','" + lossCodeVal4 + "','" + lossCode5 + "','" + lossCodeVal5 + "','" + IPAddress + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + 1 + "','" + 0 + "');", mcFinalInsertRows.msqlConnection);
                            cmdFinalInsertRows.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                        }
                        finally
                        {
                            mcFinalInsertRows.close();
                        }
                    }
                }
            }
        }

        //OEE for Shop
        public void CalculateSummarizedOEEShop(DateTime fromdate, DateTime todate, int FactorID, string TimeType, string SummarizeAs = null)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            DateTime UsedDateForExcel = Convert.ToDateTime(fromdate);
            double TotalDay = todate.Subtract(fromdate).TotalDays;
            string ipAddress = GetIPAddressOf();
            if (TimeType == "Today")
            {
                //now push to tbloeedashboardFinalVariables.
                double OEEFactor, AvaillabilityFactor, EfficiencyFactor, QualityFactor;
                //Gather Hierarchy Details
                string PlantIDS = null, ShopIDS = null, CellIDS = null, WCID = null;

                var getShop = obj.GettbloeeShopListDet1(FactorID, ipAddress, fromdate, todate);
                //var getShop = db.tbloeedashboardvariablestodays.Where(m => m.IsDeleted == 0 && m.ShopID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress).Select(m => m.ShopID).Distinct().ToList();

                if (SummarizeAs == "Plant")
                {
                    getShop = obj.GettbplantListDet1(FactorID, ipAddress, fromdate, todate);
                    //getShop = db.tbloeedashboardvariablestodays.Where(m => m.IsDeleted == 0 && m.PlantID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress).Select(m => m.ShopID).Distinct().ToList();
                }
                else if (SummarizeAs == "Shop")
                {
                    getShop = obj.GettbloeeShopListDet1(FactorID, ipAddress, fromdate, todate);
                    //getShop = db.tbloeedashboardvariablestodays.Where(m => m.IsDeleted == 0 && m.ShopID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress).Select(m => m.ShopID).Distinct().ToList();
                }

                foreach (var ShopRow in getShop)
                {
                    double green = 0, red = 0, yellow = 0, blue = 0, setup = 0, scrap = 0, OperatingTime = 0, DownTimeBreakdown = 0, ROALossess = 0, AvailableTime = 0, SettingTime = 0, PlannedDownTime = 0, UnPlannedDownTime = 0;
                    double SummationOfSCTvsPP = 0, MinorLosses = 0, ROPLosses = 0;
                    double ScrapQtyTime = 0, ReWOTime = 0, ROQLosses = 0;
                    int id = Convert.ToInt32(ShopRow.ShopID);
                    var OEEDataToSummarize = obj.GettbloeeShopList1Det1(id, ipAddress, fromdate, todate);
                    // var OEEDataToSummarize = db.tbloeedashboardvariablestodays.Where(m => m.ShopID == ShopRow && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress).ToList();
                    CellIDS = FactorID.ToString();
                    int once = 0;
                    foreach (var row in OEEDataToSummarize)
                    {
                        if (once == 0)
                        {
                            PlantIDS = Convert.ToString(row.PlantID);
                            ShopIDS = Convert.ToString(row.ShopID);
                            CellIDS = Convert.ToString(row.CellID);
                            WCID = Convert.ToString(row.WCID);
                            once++;
                        }

                        MinorLosses += Convert.ToDouble(row.MinorLosses);
                        blue += Convert.ToDouble(row.Blue);
                        green += Convert.ToDouble(row.Green);

                        //Availability
                        SettingTime += Convert.ToDouble(row.SettingTime);
                        ROALossess += Convert.ToDouble(row.ROALossess);
                        DownTimeBreakdown += Convert.ToDouble(row.DownTimeBreakdown);

                        //Performance
                        SummationOfSCTvsPP += Convert.ToDouble(row.SummationOfSCTvsPP);

                        //Quality
                        ScrapQtyTime += Convert.ToDouble(row.ScrapQtyTime);
                        ReWOTime += Convert.ToDouble(row.ReWOTime);
                    }
                    if (TimeType == "Today")
                    {
                        int MachineCount = 1;
                        if (DateTime.Now.Hour > 6)
                        {
                            id = Convert.ToInt32(ShopRow.ShopID);
                            MachineCount = obj.GettbloeeListDetails(id, ipAddress, fromdate, todate).Count();
                            //MachineCount = db.tbloeedashboardfinalvariables.Where(m => m.ShopID == ShopRow && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress && m.IsOverallWCWise == 1).Count();

                            AvailableTime = ((DateTime.Now - Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 06:00:00"))).TotalMinutes * MachineCount);
                        }
                        else
                        {
                            AvailableTime = ((DateTime.Now - Convert.ToDateTime(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 06:00:00"))).TotalMinutes * MachineCount);
                        }
                    }

                    #region Getting the Top 5 Losses
                    //Have to Calculate based on Summarization condition, use code in CalculateOEE().
                    string GetDistinctLossQuery = "Select Distinct f.lossname from" +
                                             "(" +
                                             "Select Distinct a.Loss1Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday a where ShopID = " + ShopRow.ShopID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct b.Loss2Name  as lossname from "+ databaseName + ".tbloeedashboardvariablestoday b where ShopID = " + ShopRow.ShopID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct c.Loss3Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday c where ShopID = " + ShopRow.ShopID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct d.Loss4Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday d where ShopID = " + ShopRow.ShopID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct e.Loss5Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday e where ShopID = " + ShopRow.ShopID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             ") f order by f.lossname desc";
                    DataTable GetDistinctLossDT = new DataTable();
                    using (MsqlConnection mc = new MsqlConnection())
                    {
                        mc.open();
                        SqlDataAdapter GetDistinctLossDA = new SqlDataAdapter(GetDistinctLossQuery, mc.msqlConnection);
                        GetDistinctLossDA.Fill(GetDistinctLossDT);
                        mc.close();
                    }

                    //Gather LossValues
                    string lossCode1 = null, lossCode2 = null, lossCode3 = null, lossCode4 = null, lossCode5 = null;
                    int lossCodeVal1 = 0, lossCodeVal2 = 0, lossCodeVal3 = 0, lossCodeVal4 = 0, lossCodeVal5 = 0;
                    List<KeyValuePair<String, Double>> LossDetails = new List<KeyValuePair<string, double>>();
                    int LossCount = GetDistinctLossDT.Rows.Count;
                    for (int i = 0; i < LossCount; i++)
                    {
                        String LossName = GetDistinctLossDT.Rows[i][0].ToString();
                        double LossDuration = 0;
                        String GetLossDurationQuery = "Select SUM(Total) From " +
                                       "(Select SUM(Loss1Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday " +
                                       "Where ShopID = " + ShopRow.ShopID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss1Name = '" + LossName + "'  " +
                                       "Union all " +
                                       "Select SUM(Loss2Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday  " +
                                       "Where ShopID = " + ShopRow.ShopID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss2Name = '" + LossName + "'  " +
                                       "Union All " +
                                       "Select SUM(Loss3Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday  " +
                                       "Where ShopID = " + ShopRow.ShopID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss3Name = '" + LossName + "' " +
                                       "Union All " +
                                       "Select SUM(Loss4Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday  " +
                                       "Where ShopID = " + ShopRow.ShopID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss4Name = '" + LossName + "' " +
                                       "Union All " +
                                       "Select SUM(Loss5Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday " +
                                       "Where ShopID = " + ShopRow.ShopID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss5Name = '" + LossName + "' " +
                                       ") z;";
                        DataTable GetLossDurationDT = new DataTable();
                        using (MsqlConnection mc = new MsqlConnection())
                        {
                            mc.open();
                            SqlDataAdapter GetLossDurationDA = new SqlDataAdapter(GetLossDurationQuery, mc.msqlConnection);
                            GetLossDurationDA.Fill(GetLossDurationDT);
                            mc.close();
                        }
                        if (GetLossDurationDT.Rows.Count > 0)
                        {
                            string value = Convert.ToString(GetLossDurationDT.Rows[0][0]);
                            if (Double.TryParse(value, out LossDuration))
                                LossDuration = LossDuration;
                        }
                        LossDetails.Add(new KeyValuePair<string, double>(LossName, LossDuration));
                    }
                    var losslist = LossDetails.OrderByDescending(m => m.Value).ToList();
                    int losscount = losslist.Count;

                    for (int i = 0; i < losscount; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                lossCode1 = losslist[i].Key.ToString();
                                lossCodeVal1 = (int)losslist[i].Value;
                                break;
                            case 1:
                                lossCode2 = losslist[i].Key.ToString();
                                lossCodeVal2 = (int)losslist[i].Value;
                                break;
                            case 2:
                                lossCode3 = losslist[i].Key.ToString();
                                lossCodeVal3 = (int)losslist[i].Value;
                                break;
                            case 3:
                                lossCode4 = losslist[i].Key.ToString();
                                lossCodeVal4 = (int)losslist[i].Value;
                                break;
                            case 4:
                                lossCode5 = losslist[i].Key.ToString();
                                lossCodeVal5 = (int)losslist[i].Value;
                                break;
                        }
                    }
                    #endregion

                    //OEE Values

                    OperatingTime = green;
                    //Availability Factor
                    double val = OperatingTime / AvailableTime;
                    AvaillabilityFactor = Math.Round(val * 100, 2);
                    if (AvaillabilityFactor > 0 && AvaillabilityFactor < 100)
                    {
                        AvaillabilityFactor = Math.Round(val * 100, 2);
                    }
                    else
                    {
                        if (AvaillabilityFactor > 100)
                        {
                            EfficiencyFactor = 100;
                        }
                        else if (AvaillabilityFactor < 0)
                        {
                            EfficiencyFactor = 0;
                        }
                    }

                    //Performance(Efficiency) Factor
                    if (OperatingTime == 0)
                    {
                        EfficiencyFactor = 100;
                    }
                    else
                    {
                        EfficiencyFactor = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 2);
                        if (SummationOfSCTvsPP == -1 || SummationOfSCTvsPP == 0)
                        {
                            EfficiencyFactor = 100;
                        }
                        else
                        {
                            EfficiencyFactor = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 2);
                            if (EfficiencyFactor >= 0 && EfficiencyFactor <= 100)
                            {
                            }
                            else if (EfficiencyFactor > 100 || EfficiencyFactor < 0)
                            {
                                EfficiencyFactor = 100;
                            }
                        }
                    }

                    //QualityFactor
                    if (OperatingTime == 0)
                    {
                        QualityFactor = 0;
                    }
                    else
                    {
                        QualityFactor = (OperatingTime - ScrapQtyTime - ReWOTime) / OperatingTime;
                        if (QualityFactor >= 0 && QualityFactor <= 100)
                        {
                            QualityFactor = Math.Round(QualityFactor * 100, 2);
                        }
                        else if (QualityFactor > 100 || QualityFactor < 0)
                        {
                            QualityFactor = 100;
                        }
                    }

                    //OEE Factor
                    if (AvaillabilityFactor <= 0 || EfficiencyFactor <= 0 || QualityFactor <= 0)
                    {
                        OEEFactor = 0;
                    }
                    else
                    {
                        OEEFactor = Math.Round((AvaillabilityFactor / 100) * (EfficiencyFactor / 100) * (QualityFactor / 100) * 100, 2);
                        if (OEEFactor >= 0 && OEEFactor <= 100)
                        {
                            OEEFactor = Math.Round(OEEFactor, 2);
                        }
                        else if (OEEFactor > 100)
                        {
                            OEEFactor = 100;
                        }
                        else if (OEEFactor < 0)
                        {
                            OEEFactor = 0;
                        }
                    }
                    string IPAddress = GetIPAddressOf();
                    int isShopWise = 1;
                    int isCellWise = 0;
                    int isWCWise = 0;

                    //Now insert into table
                    using (MsqlConnection mcFinalInsertRows = new MsqlConnection())
                    {
                        mcFinalInsertRows.open();
                        try
                        {
                            //db.tbloeedashboardfinalvariables.RemoveRange(db.tbloeedashboardfinalvariables.Where(x => x.IPAddress == IPAddress && x.IsOverallShopWise == 1));
                            //db.SaveChanges();

                            SqlCommand cmdFinalInsertRows = new SqlCommand("INSERT INTO tbloeedashboardfinalvariables(PlantID,ShopID,CellID,WCID,StartDate,EndDate,OEE,Availability,Performance,Quality,IsOverallShopWise,IsOverallCellWise,IsOverallWCWise,Loss1Name,Loss1Value,Loss2Name,Loss2Value,Loss3Name,Loss3Value,Loss4Name,Loss4Value,Loss5Name,Loss5Value,IPAddress,CreatedOn,CreatedBy,IsDeleted,IsToday)VALUES('" + Convert.ToInt32(PlantIDS) + "','" + Convert.ToInt32(ShopRow.ShopID) + "','" + Convert.ToInt32(CellIDS) + "','" + Convert.ToInt32(WCID) + "','" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + OEEFactor + "','" + AvaillabilityFactor + "','" + EfficiencyFactor + "','" + QualityFactor + "','" + isShopWise + "','" + isCellWise + "','" + isWCWise + "','" + lossCode1 + "','" + lossCodeVal1 + "','" + lossCode2 + "','" + lossCodeVal2 + "','" + lossCode3 + "','" + lossCodeVal3 + "','" + lossCode4 + "','" + lossCodeVal4 + "','" + lossCode5 + "','" + lossCodeVal5 + "','" + IPAddress + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + 1 + "','" + 0 + "','" + 1 + "');", mcFinalInsertRows.msqlConnection);
                            cmdFinalInsertRows.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                        }
                        finally
                        {
                            mcFinalInsertRows.close();
                        }
                    }
                }
            }
            else if (TimeType == "GodHours" || TimeType == "NoBlue")
            {
                double OEEFactor, AvaillabilityFactor, EfficiencyFactor, QualityFactor;
                //Gather Hierarchy Details
                string PlantIDS = null, ShopIDS = null, CellIDS = null, WCID = null;
                var getShop = obj.GettbloeeShopListDet2(FactorID, fromdate, todate);
                //var getShop = db.tbloeedashboardvariables.Where(m => m.IsDeleted == 0 && m.ShopID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate).Select(m => m.ShopID).Distinct().ToList();

                if (SummarizeAs == "Plant")
                {
                    getShop = obj.GettbplantListDet3(FactorID, fromdate, todate);
                    // getShop = db.tbloeedashboardvariables.Where(m => m.IsDeleted == 0 && m.PlantID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate).Select(m => m.ShopID).Distinct().ToList();
                }
                else if (SummarizeAs == "Shop")
                {
                    getShop = obj.GettbloeeShopListDet2(FactorID, fromdate, todate);
                    // getShop = db.tbloeedashboardvariables.Where(m => m.IsDeleted == 0 && m.ShopID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate).Select(m => m.ShopID).Distinct().ToList();
                }

                foreach (var Row in getShop)
                {
                    int ShopRow = Convert.ToInt32(Row.ShopID);
                    double green = 0, red = 0, yellow = 0, blue = 0, setup = 0, scrap = 0, OperatingTime = 0, DownTimeBreakdown = 0, ROALossess = 0, AvailableTime = 0, SettingTime = 0, PlannedDownTime = 0, UnPlannedDownTime = 0;
                    double SummationOfSCTvsPP = 0, MinorLosses = 0, ROPLosses = 0;
                    double ScrapQtyTime = 0, ReWOTime = 0, ROQLosses = 0;
                    int id = Convert.ToInt32(ShopRow);
                    var OEEDataToSummarize = obj.GettbloeeShopList1Det2(id, fromdate, todate);
                    //var OEEDataToSummarize = db.tbloeedashboardvariables.Where(m => m.ShopID == ShopRow && m.StartDate >= fromdate && m.StartDate <= todate).ToList();
                    CellIDS = FactorID.ToString();
                    int once = 0;
                    foreach (var row in OEEDataToSummarize)
                    {
                        if (once == 0)
                        {
                            PlantIDS = Convert.ToString(row.PlantID);
                            ShopIDS = Convert.ToString(row.ShopID);
                            CellIDS = Convert.ToString(row.CellID);
                            WCID = Convert.ToString(row.WCID);
                            once++;
                        }

                        MinorLosses += Convert.ToDouble(row.MinorLosses);
                        blue += Convert.ToDouble(row.Blue);
                        green += Convert.ToDouble(row.Green);

                        //Availability
                        SettingTime += Convert.ToDouble(row.SettingTime);
                        ROALossess += Convert.ToDouble(row.ROALossess);
                        DownTimeBreakdown += Convert.ToDouble(row.DownTimeBreakdown);

                        //Performance
                        SummationOfSCTvsPP += Convert.ToDouble(row.SummationOfSCTvsPP);

                        //Quality
                        ScrapQtyTime += Convert.ToDouble(row.ScrapQtyTime);
                        ReWOTime += Convert.ToDouble(row.ReWOTime);
                    }
                    int MachineCount = 0;
                    MachineCount = obj.GettbloeeListDetails(id, ipAddress, fromdate, todate).Count();
                    // MachineCount = db.tbloeedashboardfinalvariables.Where(m => m.ShopID == ShopRow && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress && m.IsOverallWCWise == 1).Count();

                    int Days = Convert.ToInt32(todate.Subtract(fromdate).TotalDays) + 1;
                    if (TimeType == "GodHours")
                    {
                        AvailableTime = MachineCount * 24 * 60 * Days; //24Hours to Minutes;
                    }
                    else if (TimeType == "NoBlue")
                    {
                        AvailableTime = (MachineCount * 24 * 60 * Days) - blue; //24Hours to Minutes;
                    }

                    #region Getting the Top 5 Losses
                    //Have to Calculate based on Summarization condition, use code in CalculateOEE().
                    string GetDistinctLossQuery = "Select Distinct f.lossname from" +
                                             "(" +
                                             "Select Distinct a.Loss1Name as lossname from "+ databaseName + ".tbloeedashboardvariables a where ShopID = " + ShopRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct b.Loss2Name  as lossname from "+ databaseName + ".tbloeedashboardvariables b where ShopID = " + ShopRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct c.Loss3Name as lossname from "+ databaseName + ".tbloeedashboardvariables c where ShopID = " + ShopRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct d.Loss4Name as lossname from "+ databaseName + ".tbloeedashboardvariables d where ShopID = " + ShopRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct e.Loss5Name as lossname from "+ databaseName + ".tbloeedashboardvariables e where ShopID = " + ShopRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             ") f order by f.lossname desc";
                    MsqlConnection mc = new MsqlConnection();
                    mc.open();
                    SqlDataAdapter GetDistinctLossDA = new SqlDataAdapter(GetDistinctLossQuery, mc.msqlConnection);
                    DataTable GetDistinctLossDT = new DataTable();
                    GetDistinctLossDA.Fill(GetDistinctLossDT);
                    mc.close();

                    //Gather LossValues
                    string lossCode1 = null, lossCode2 = null, lossCode3 = null, lossCode4 = null, lossCode5 = null;
                    int lossCodeVal1 = 0, lossCodeVal2 = 0, lossCodeVal3 = 0, lossCodeVal4 = 0, lossCodeVal5 = 0;
                    List<KeyValuePair<String, Double>> LossDetails = new List<KeyValuePair<string, double>>();
                    int LossCount = GetDistinctLossDT.Rows.Count;
                    for (int i = 0; i < LossCount; i++)
                    {
                        String LossName = GetDistinctLossDT.Rows[i][0].ToString();
                        double LossDuration = 0;
                        String GetLossDurationQuery = "Select SUM(Total) From " +
                                       "(Select SUM(Loss1Value)  as Total From "+ databaseName + ".tbloeedashboardvariables " +
                                       "Where ShopID = " + ShopRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss1Name = '" + LossName + "'  " +
                                       "Union all " +
                                       "Select SUM(Loss2Value)  as Total From "+ databaseName + ".tbloeedashboardvariables  " +
                                       "Where ShopID = " + ShopRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss2Name = '" + LossName + "'  " +
                                       "Union All " +
                                       "Select SUM(Loss3Value)  as Total From "+ databaseName + ".tbloeedashboardvariables  " +
                                       "Where ShopID = " + ShopRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss3Name = '" + LossName + "' " +
                                       "Union All " +
                                       "Select SUM(Loss4Value)  as Total From "+ databaseName + ".tbloeedashboardvariables  " +
                                       "Where ShopID = " + ShopRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss4Name = '" + LossName + "' " +
                                       "Union All " +
                                       "Select SUM(Loss5Value)  as Total From "+ databaseName + ".tbloeedashboardvariables " +
                                       "Where ShopID = " + ShopRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss5Name = '" + LossName + "' " +
                                       ") z;";
                        mc.open();
                        SqlDataAdapter GetLossDurationDA = new SqlDataAdapter(GetLossDurationQuery, mc.msqlConnection);
                        DataTable GetLossDurationDT = new DataTable();
                        GetLossDurationDA.Fill(GetLossDurationDT);
                        mc.close();
                        if (GetLossDurationDT.Rows.Count > 0)
                        {
                            string value = Convert.ToString(GetLossDurationDT.Rows[0][0]);
                            if (Double.TryParse(value, out LossDuration))
                                LossDuration = LossDuration;
                        }
                        LossDetails.Add(new KeyValuePair<string, double>(LossName, LossDuration));
                    }
                    var losslist = LossDetails.OrderByDescending(m => m.Value).ToList();
                    int losscount = losslist.Count;

                    for (int i = 0; i < losscount; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                lossCode1 = losslist[i].Key.ToString();
                                lossCodeVal1 = (int)losslist[i].Value;
                                break;
                            case 1:
                                lossCode2 = losslist[i].Key.ToString();
                                lossCodeVal2 = (int)losslist[i].Value;
                                break;
                            case 2:
                                lossCode3 = losslist[i].Key.ToString();
                                lossCodeVal3 = (int)losslist[i].Value;
                                break;
                            case 3:
                                lossCode4 = losslist[i].Key.ToString();
                                lossCodeVal4 = (int)losslist[i].Value;
                                break;
                            case 4:
                                lossCode5 = losslist[i].Key.ToString();
                                lossCodeVal5 = (int)losslist[i].Value;
                                break;
                        }
                    }
                    #endregion

                    //OEE Values

                    OperatingTime = green;
                    //Availability Factor
                    double val = OperatingTime / AvailableTime;
                    AvaillabilityFactor = Math.Round(val * 100, 2);
                    if (AvaillabilityFactor > 0 && AvaillabilityFactor < 100)
                    {
                        AvaillabilityFactor = Math.Round(val * 100, 2);
                    }
                    else
                    {
                        if (AvaillabilityFactor > 100)
                        {
                            EfficiencyFactor = 100;
                        }
                        else if (AvaillabilityFactor < 0)
                        {
                            EfficiencyFactor = 0;
                        }
                    }

                    //Performance(Efficiency) Factor
                    EfficiencyFactor = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 2);
                    if (SummationOfSCTvsPP == -1 || SummationOfSCTvsPP == 0)
                    {
                        EfficiencyFactor = 100;
                    }
                    else
                    {
                        EfficiencyFactor = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 2);
                        if (EfficiencyFactor >= 0 && EfficiencyFactor <= 100)
                        {
                        }
                        else if (EfficiencyFactor > 100 || EfficiencyFactor < 0)
                        {
                            EfficiencyFactor = 100;
                        }
                    }

                    //QualityFactor
                    QualityFactor = (OperatingTime - ScrapQtyTime - ReWOTime) / OperatingTime;
                    if (QualityFactor >= 0 && QualityFactor <= 100)
                    {
                        QualityFactor = Math.Round(QualityFactor * 100, 2);
                    }
                    else if (QualityFactor > 100 || QualityFactor < 0)
                    {
                        QualityFactor = 100;
                    }

                    //OEE Factor
                    if (AvaillabilityFactor <= 0 || EfficiencyFactor <= 0 || QualityFactor <= 0)
                    {
                        OEEFactor = 0;
                    }
                    else
                    {
                        OEEFactor = Math.Round((AvaillabilityFactor / 100) * (EfficiencyFactor / 100) * (QualityFactor / 100) * 100, 2);
                        if (OEEFactor >= 0 && OEEFactor <= 100)
                        {
                            OEEFactor = Math.Round(OEEFactor, 2);
                        }
                        else if (OEEFactor > 100)
                        {
                            OEEFactor = 100;
                        }
                        else if (OEEFactor < 0)
                        {
                            OEEFactor = 0;
                        }
                    }
                    string IPAddress = GetIPAddressOf();
                    int isShopWise = 1;
                    int isCellWise = 0;
                    int isWCWise = 0;

                    //Now insert into table
                    MsqlConnection mcFinalInsertRows = new MsqlConnection();
                    mcFinalInsertRows.open();
                    try
                    {
                        //db.tbloeedashboardfinalvariables.RemoveRange(db.tbloeedashboardfinalvariables.Where(x => x.IPAddress == IPAddress && x.IsOverallShopWise == 1));
                        //db.SaveChanges();

                        SqlCommand cmdFinalInsertRows = new SqlCommand("INSERT INTO tbloeedashboardfinalvariables(PlantID,ShopID,CellID,WCID,StartDate,EndDate,OEE,Availability,Performance,Quality,IsOverallShopWise,IsOverallCellWise,IsOverallWCWise,Loss1Name,Loss1Value,Loss2Name,Loss2Value,Loss3Name,Loss3Value,Loss4Name,Loss4Value,Loss5Name,Loss5Value,IPAddress,CreatedOn,CreatedBy,IsDeleted)VALUES('" + Convert.ToInt32(PlantIDS) + "','" + Convert.ToInt32(ShopRow) + "','" + Convert.ToInt32(CellIDS) + "','" + Convert.ToInt32(WCID) + "','" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + OEEFactor + "','" + AvaillabilityFactor + "','" + EfficiencyFactor + "','" + QualityFactor + "','" + isShopWise + "','" + isCellWise + "','" + isWCWise + "','" + lossCode1 + "','" + lossCodeVal1 + "','" + lossCode2 + "','" + lossCodeVal2 + "','" + lossCode3 + "','" + lossCodeVal3 + "','" + lossCode4 + "','" + lossCodeVal4 + "','" + lossCode5 + "','" + lossCodeVal5 + "','" + IPAddress + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + 1 + "','" + 0 + "');", mcFinalInsertRows.msqlConnection);
                        cmdFinalInsertRows.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        mcFinalInsertRows.close();
                    }
                }
            }
        }

        //OEE for Cell
        public void CalculateSummarizedOEECell(DateTime fromdate, DateTime todate, int FactorID, string TimeType, string SummarizeAs = null)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            DateTime UsedDateForExcel = Convert.ToDateTime(fromdate);
            double TotalDay = todate.Subtract(fromdate).TotalDays;
            string ipAddress = GetIPAddressOf();

            if (TimeType == "Today")
            {
                //now push to tbloeedashboardFinalVariables.
                double OEEFactor, AvaillabilityFactor, EfficiencyFactor, QualityFactor;


                //Gather Hierarchy Details
                string PlantIDS = null, ShopIDS = null, CellIDS = null, WCID = null;
                var getCell = obj.GettbloeeListDetforcell1(FactorID, ipAddress, fromdate, todate);
                //var getCell = db.tbloeedashboardvariablestodays.Where(m => m.IsDeleted == 0 && m.CellID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress).Select(m => m.CellID).Distinct().ToList();

                if (SummarizeAs == "Plant")
                {
                    getCell = obj.GettbloeeListDetforplant1(FactorID, ipAddress, fromdate, todate);
                    // getCell = db.tbloeedashboardvariablestodays.Where(m => m.IsDeleted == 0 && m.PlantID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress).Select(m => m.CellID).Distinct().ToList();
                }
                else if (SummarizeAs == "Shop")
                {
                    getCell = obj.GettbloeeListDetforshop1(FactorID, ipAddress, fromdate, todate);
                    //getCell = db.tbloeedashboardvariablestodays.Where(m => m.IsDeleted == 0 && m.ShopID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress).Select(m => m.CellID).Distinct().ToList();
                }
                else if (SummarizeAs == "Cell")
                {
                    getCell = obj.GettbloeeListDetforcell1(FactorID, ipAddress, fromdate, todate);
                    // getCell = db.tbloeedashboardvariablestodays.Where(m => m.IsDeleted == 0 && m.CellID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress).Select(m => m.CellID).Distinct().ToList();
                }

                foreach (var CellRow in getCell)
                {
                    double green = 0, red = 0, yellow = 0, blue = 0, setup = 0, scrap = 0, OperatingTime = 0, DownTimeBreakdown = 0, ROALossess = 0, AvailableTime = 0, SettingTime = 0, PlannedDownTime = 0, UnPlannedDownTime = 0;
                    double SummationOfSCTvsPP = 0, MinorLosses = 0, ROPLosses = 0;
                    double ScrapQtyTime = 0, ReWOTime = 0, ROQLosses = 0;
                    int id = Convert.ToInt32(CellRow.CellID);
                    var OEEDataToSummarize = obj.GettbloeeListDetforcell(id, ipAddress, fromdate, todate);
                    //var OEEDataToSummarize = db.tbloeedashboardvariablestodays.Where(m => m.CellID == CellRow && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress).ToList();
                    CellIDS = FactorID.ToString();
                    int once = 0;
                    foreach (var row in OEEDataToSummarize)
                    {
                        if (once == 0)
                        {
                            PlantIDS = Convert.ToString(row.PlantID);
                            ShopIDS = Convert.ToString(row.ShopID);
                            CellIDS = Convert.ToString(row.CellID);
                            WCID = Convert.ToString(row.WCID);
                            once++;
                        }

                        MinorLosses += Convert.ToDouble(row.MinorLosses);
                        blue += Convert.ToDouble(row.Blue);
                        green += Convert.ToDouble(row.Green);

                        //Availability
                        SettingTime += Convert.ToDouble(row.SettingTime);
                        ROALossess += Convert.ToDouble(row.ROALossess);
                        DownTimeBreakdown += Convert.ToDouble(row.DownTimeBreakdown);

                        //Performance
                        SummationOfSCTvsPP += Convert.ToDouble(row.SummationOfSCTvsPP);

                        //Quality
                        ScrapQtyTime += Convert.ToDouble(row.ScrapQtyTime);
                        ReWOTime += Convert.ToDouble(row.ReWOTime);
                    }
                    if (TimeType == "Today")
                    {
                        int MachineCount = 0;
                        MachineCount = obj.GettbloeeListDetails(id, ipAddress, fromdate, todate).Count();
                        // MachineCount = db.tbloeedashboardfinalvariables.Where(m => m.CellID == CellRow && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress && m.IsOverallWCWise == 1).Count();

                        if (DateTime.Now.Hour > 6)
                        {
                            AvailableTime = ((DateTime.Now - Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 06:00:00"))).TotalMinutes * MachineCount);
                        }
                        else
                        {
                            AvailableTime = ((DateTime.Now - Convert.ToDateTime(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 06:00:00"))).TotalMinutes * MachineCount);
                        }
                    }

                    #region Getting the Top 5 Losses
                    //Have to Calculate based on Summarization condition, use code in CalculateOEE().
                    string GetDistinctLossQuery = "Select Distinct f.lossname from" +
                                             "(" +
                                             "Select Distinct a.Loss1Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday a where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct b.Loss2Name  as lossname from "+ databaseName + ".tbloeedashboardvariablestoday b where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct c.Loss3Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday c where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct d.Loss4Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday d where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct e.Loss5Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday e where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             ") f order by f.lossname desc";
                    DataTable GetDistinctLossDT = new DataTable();
                    using (MsqlConnection mc = new MsqlConnection())
                    {
                        mc.open();
                        SqlDataAdapter GetDistinctLossDA = new SqlDataAdapter(GetDistinctLossQuery, mc.msqlConnection);
                        GetDistinctLossDA.Fill(GetDistinctLossDT);
                        mc.close();
                    }
                    //Gather LossValues
                    string lossCode1 = null, lossCode2 = null, lossCode3 = null, lossCode4 = null, lossCode5 = null;
                    int lossCodeVal1 = 0, lossCodeVal2 = 0, lossCodeVal3 = 0, lossCodeVal4 = 0, lossCodeVal5 = 0;
                    List<KeyValuePair<String, Double>> LossDetails = new List<KeyValuePair<string, double>>();
                    int LossCount = GetDistinctLossDT.Rows.Count;
                    for (int i = 0; i < LossCount; i++)
                    {
                        String LossName = GetDistinctLossDT.Rows[i][0].ToString();
                        double LossDuration = 0;

                        String GetLossDurationQuery = "Select SUM(Total) From " +
                                       "(Select SUM(Loss1Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday " +
                                       "Where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss1Name = '" + LossName + "'  " +
                                       "Union all " +
                                       "Select SUM(Loss2Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday  " +
                                       "Where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss2Name = '" + LossName + "'  " +
                                       "Union All " +
                                       "Select SUM(Loss3Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday  " +
                                       "Where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss3Name = '" + LossName + "' " +
                                       "Union All " +
                                       "Select SUM(Loss4Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday  " +
                                       "Where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss4Name = '" + LossName + "' " +
                                       "Union All " +
                                       "Select SUM(Loss5Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday " +
                                       "Where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss5Name = '" + LossName + "' " +
                                       ") z;";
                        DataTable GetLossDurationDT = new DataTable();
                        using (MsqlConnection mc = new MsqlConnection())
                        {
                            mc.open();
                            SqlDataAdapter GetLossDurationDA = new SqlDataAdapter(GetLossDurationQuery, mc.msqlConnection);
                            GetLossDurationDA.Fill(GetLossDurationDT);
                            mc.close();
                        }
                        if (GetLossDurationDT.Rows.Count > 0)
                        {
                            string value = Convert.ToString(GetLossDurationDT.Rows[0][0]);
                            if (Double.TryParse(value, out LossDuration))
                                LossDuration = LossDuration;
                        }
                        LossDetails.Add(new KeyValuePair<string, double>(LossName, LossDuration));
                    }
                    var losslist = LossDetails.OrderByDescending(m => m.Value).ToList();
                    int losscount = losslist.Count;

                    for (int i = 0; i < losscount; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                lossCode1 = losslist[i].Key.ToString();
                                lossCodeVal1 = (int)losslist[i].Value;
                                break;
                            case 1:
                                lossCode2 = losslist[i].Key.ToString();
                                lossCodeVal2 = (int)losslist[i].Value;
                                break;
                            case 2:
                                lossCode3 = losslist[i].Key.ToString();
                                lossCodeVal3 = (int)losslist[i].Value;
                                break;
                            case 3:
                                lossCode4 = losslist[i].Key.ToString();
                                lossCodeVal4 = (int)losslist[i].Value;
                                break;
                            case 4:
                                lossCode5 = losslist[i].Key.ToString();
                                lossCodeVal5 = (int)losslist[i].Value;
                                break;
                        }
                    }
                    #endregion

                    //OEE Values
                    OperatingTime = green;
                    //Availability Factor
                    double val = OperatingTime / AvailableTime;
                    AvaillabilityFactor = Math.Round(val * 100, 2);
                    if (AvaillabilityFactor > 0 && AvaillabilityFactor < 100)
                    {
                        AvaillabilityFactor = Math.Round(val * 100, 2);
                    }
                    else
                    {
                        if (AvaillabilityFactor > 100)
                        {
                            EfficiencyFactor = 100;
                        }
                        else if (AvaillabilityFactor < 0)
                        {
                            EfficiencyFactor = 0;
                        }
                    }

                    //Performance(Efficiency) Factor
                    if (OperatingTime == 0)
                    {
                        EfficiencyFactor = 100;
                    }
                    else
                    {
                        EfficiencyFactor = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 2);
                        if (SummationOfSCTvsPP == -1 || SummationOfSCTvsPP == 0)
                        {
                            EfficiencyFactor = 100;
                        }
                        else
                        {
                            EfficiencyFactor = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 2);
                            if (EfficiencyFactor >= 0 && EfficiencyFactor <= 100)
                            {
                            }
                            else if (EfficiencyFactor > 100 || EfficiencyFactor < 0)
                            {
                                EfficiencyFactor = 100;
                            }
                        }
                    }

                    //QualityFactor
                    if (OperatingTime == 0)
                    {
                        QualityFactor = 100;
                    }
                    else
                    {
                        QualityFactor = (OperatingTime - ScrapQtyTime - ReWOTime) / OperatingTime;
                        if (QualityFactor >= 0 && QualityFactor <= 100)
                        {
                            QualityFactor = Math.Round(QualityFactor * 100, 2);
                        }
                        else if (QualityFactor > 100 || QualityFactor < 0)
                        {
                            QualityFactor = 100;
                        }
                    }

                    //OEE Factor
                    if (AvaillabilityFactor <= 0 || EfficiencyFactor <= 0 || QualityFactor <= 0)
                    {
                        OEEFactor = 0;
                    }
                    else
                    {
                        OEEFactor = Math.Round((AvaillabilityFactor / 100) * (EfficiencyFactor / 100) * (QualityFactor / 100) * 100, 2);
                        if (OEEFactor >= 0 && OEEFactor <= 100)
                        {
                            OEEFactor = Math.Round(OEEFactor, 2);
                        }
                        else if (OEEFactor > 100)
                        {
                            OEEFactor = 100;
                        }
                        else if (OEEFactor < 0)
                        {
                            OEEFactor = 0;
                        }
                    }
                    string IPAddress = GetIPAddressOf();
                    int isShopWise = 0;
                    int isCellWise = 1;
                    int isWCWise = 0;

                    //Now insert into table
                    using (MsqlConnection mcFinalInsertRows = new MsqlConnection())
                    {
                        mcFinalInsertRows.open();
                        try
                        {
                            //db.tbloeedashboardfinalvariables.RemoveRange(db.tbloeedashboardfinalvariables.Where(x => x.IPAddress == IPAddress && x.IsOverallCellWise == isCellWise));
                            //db.SaveChanges();

                            SqlCommand cmdFinalInsertRows = new SqlCommand("INSERT INTO tbloeedashboardfinalvariables(PlantID,ShopID,CellID,WCID,StartDate,EndDate,OEE,Availability,Performance,Quality,IsOverallShopWise,IsOverallCellWise,IsOverallWCWise,Loss1Name,Loss1Value,Loss2Name,Loss2Value,Loss3Name,Loss3Value,Loss4Name,Loss4Value,Loss5Name,Loss5Value,IPAddress,CreatedOn,CreatedBy,IsDeleted,IsToday)VALUES('" + Convert.ToInt32(PlantIDS) + "','" + Convert.ToInt32(ShopIDS) + "','" + Convert.ToInt32(CellRow.CellID) + "','" + Convert.ToInt32(WCID) + "','" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + OEEFactor + "','" + AvaillabilityFactor + "','" + EfficiencyFactor + "','" + QualityFactor + "','" + isShopWise + "','" + isCellWise + "','" + isWCWise + "','" + lossCode1 + "','" + lossCodeVal1 + "','" + lossCode2 + "','" + lossCodeVal2 + "','" + lossCode3 + "','" + lossCodeVal3 + "','" + lossCode4 + "','" + lossCodeVal4 + "','" + lossCode5 + "','" + lossCodeVal5 + "','" + IPAddress + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + 1 + "','" + 0 + "','" + 1 + "');", mcFinalInsertRows.msqlConnection);
                            cmdFinalInsertRows.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                        }
                        finally
                        {
                            mcFinalInsertRows.close();
                        }
                    }
                }
            }
            else if (TimeType == "GodHours" || TimeType == "NoBlue")
            {
                //now push to tbloeedashboardFinalVariables.
                double OEEFactor, AvaillabilityFactor, EfficiencyFactor, QualityFactor;

                //Gather Hierarchy Details
                string PlantIDS = null, ShopIDS = null, CellIDS = null, WCID = null;

                var getCell = obj.GettbloeecelllistDet1(FactorID, fromdate, todate);
                //var getCell = db.tbloeedashboardvariables.Where(m => m.IsDeleted == 0 && m.CellID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate).Select(m => m.CellID).Distinct().ToList();

                if (SummarizeAs == "Plant")
                {
                    getCell = obj.GettbloeeplantlistDet1(FactorID, fromdate, todate);
                    //getCell = db.tbloeedashboardvariables.Where(m => m.IsDeleted == 0 && m.PlantID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate).Select(m => m.CellID).Distinct().ToList();
                }
                else if (SummarizeAs == "Shop")
                {
                    getCell = obj.GettbloeeshoplistDet1(FactorID, fromdate, todate);
                    //getCell = db.tbloeedashboardvariables.Where(m => m.IsDeleted == 0 && m.ShopID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate).Select(m => m.CellID).Distinct().ToList();
                }
                else if (SummarizeAs == "Cell")
                {
                    getCell = obj.GettbloeecelllistDet1(FactorID, fromdate, todate);
                    // getCell = db.tbloeedashboardvariables.Where(m => m.IsDeleted == 0 && m.CellID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate).Select(m => m.CellID).Distinct().ToList();
                }

                foreach (var Row in getCell)
                {
                    int CellRow = Convert.ToInt32(Row.CellID);
                    double green = 0, red = 0, yellow = 0, blue = 0, setup = 0, scrap = 0, OperatingTime = 0, DownTimeBreakdown = 0, ROALossess = 0, AvailableTime = 0, SettingTime = 0, PlannedDownTime = 0, UnPlannedDownTime = 0;
                    double SummationOfSCTvsPP = 0, MinorLosses = 0, ROPLosses = 0;
                    double ScrapQtyTime = 0, ReWOTime = 0, ROQLosses = 0;
                    int id = Convert.ToInt32(CellRow);
                    var OEEDataToSummarize = obj.GettbloeecelllistDet1(id, fromdate, todate);
                    //var OEEDataToSummarize = db.tbloeedashboardvariables.Where(m => m.CellID == CellRow && m.StartDate >= fromdate && m.StartDate <= todate).ToList();
                    CellIDS = FactorID.ToString();
                    int once = 0;
                    foreach (var row in OEEDataToSummarize)
                    {
                        if (once == 0)
                        {
                            PlantIDS = Convert.ToString(row.PlantID);
                            ShopIDS = Convert.ToString(row.ShopID);
                            CellIDS = Convert.ToString(row.CellID);
                            WCID = Convert.ToString(row.WCID);
                            once++;
                        }

                        MinorLosses += Convert.ToDouble(row.MinorLosses);
                        blue += Convert.ToDouble(row.Blue);
                        green += Convert.ToDouble(row.Green);

                        //Availability
                        SettingTime += Convert.ToDouble(row.SettingTime);
                        ROALossess += Convert.ToDouble(row.ROALossess);
                        DownTimeBreakdown += Convert.ToDouble(row.DownTimeBreakdown);

                        //Performance
                        SummationOfSCTvsPP += Convert.ToDouble(row.SummationOfSCTvsPP);

                        //Quality
                        ScrapQtyTime += Convert.ToDouble(row.ScrapQtyTime);
                        ReWOTime += Convert.ToDouble(row.ReWOTime);
                    }
                    int MachineCount = 0;
                    MachineCount = obj.GettbloeeListDetailsmacdata(id, ipAddress, fromdate, todate).Count();
                    //MachineCount = db.tbloeedashboardfinalvariables.Where(m => m.CellID == CellRow && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress && m.IsOverallWCWise == 1).Count();

                    int Days = Convert.ToInt32(todate.Subtract(fromdate).TotalDays) + 1;

                    if (TimeType == "GodHours")
                    {
                        AvailableTime = MachineCount * 24 * 60 * Days; //24Hours to Minutes;
                    }
                    else if (TimeType == "NoBlue")
                    {
                        AvailableTime = (MachineCount * 24 * 60 * Days) - blue; //24Hours to Minutes;
                    }

                    #region Getting the Top 5 Losses
                    //Have to Calculate based on Summarization condition, use code in CalculateOEE().
                    string GetDistinctLossQuery = "Select Distinct f.lossname from" +
                                             "(" +
                                             "Select Distinct a.Loss1Name as lossname from "+ databaseName + ".tbloeedashboardvariables a where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct b.Loss2Name  as lossname from "+ databaseName + ".tbloeedashboardvariables b where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct c.Loss3Name as lossname from "+ databaseName + ".tbloeedashboardvariables c where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct d.Loss4Name as lossname from "+ databaseName + ".tbloeedashboardvariables d where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct e.Loss5Name as lossname from "+ databaseName + ".tbloeedashboardvariables e where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             ") f order by f.lossname desc";
                    DataTable GetDistinctLossDT = new DataTable();
                    using (MsqlConnection mc = new MsqlConnection())
                    {
                        mc.open();
                        SqlDataAdapter GetDistinctLossDA = new SqlDataAdapter(GetDistinctLossQuery, mc.msqlConnection);
                        GetDistinctLossDA.Fill(GetDistinctLossDT);
                        mc.close();
                    }
                    //Gather LossValues
                    string lossCode1 = null, lossCode2 = null, lossCode3 = null, lossCode4 = null, lossCode5 = null;
                    int lossCodeVal1 = 0, lossCodeVal2 = 0, lossCodeVal3 = 0, lossCodeVal4 = 0, lossCodeVal5 = 0;
                    List<KeyValuePair<String, Double>> LossDetails = new List<KeyValuePair<string, double>>();
                    int LossCount = GetDistinctLossDT.Rows.Count;
                    for (int i = 0; i < LossCount; i++)
                    {
                        String LossName = GetDistinctLossDT.Rows[i][0].ToString();
                        double LossDuration = 0;

                        String GetLossDurationQuery = "Select SUM(Total) From " +
                                       "(Select SUM(Loss1Value)  as Total From "+ databaseName + ".tbloeedashboardvariables " +
                                       "Where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss1Name = '" + LossName + "'  " +
                                       "Union all " +
                                       "Select SUM(Loss2Value)  as Total From "+ databaseName + ".tbloeedashboardvariables  " +
                                       "Where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss2Name = '" + LossName + "'  " +
                                       "Union All " +
                                       "Select SUM(Loss3Value)  as Total From "+ databaseName + ".tbloeedashboardvariables  " +
                                       "Where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss3Name = '" + LossName + "' " +
                                       "Union All " +
                                       "Select SUM(Loss4Value)  as Total From "+ databaseName + ".tbloeedashboardvariables  " +
                                       "Where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss4Name = '" + LossName + "' " +
                                       "Union All " +
                                       "Select SUM(Loss5Value)  as Total From "+ databaseName + ".tbloeedashboardvariables " +
                                       "Where CellID = " + id + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss5Name = '" + LossName + "' " +
                                       ") z;";

                        //String GetLossDurationQuery = "Select SUM(Loss1Value),SUM(Loss2Value),SUM(Loss3Value),SUM(Loss4Value),SUM(Loss5Value) FROM tbloeedashboardvariables " +
                        //    "WHERE CellID = " + CellRow + " and StartDate Between '" + fromdate + "' AND '" + todate + "' AND ( Loss1Name = '" + LossName + "' OR Loss2Name = '" + LossName + "' OR Loss3Name = '" + LossName + "' OR Loss4Name = '" + LossName + "' OR Loss5Name = '" + LossName + "') ";
                        DataTable GetLossDurationDT = new DataTable();
                        using (MsqlConnection mc = new MsqlConnection())
                        {
                            mc.open();
                            SqlDataAdapter GetLossDurationDA = new SqlDataAdapter(GetLossDurationQuery, mc.msqlConnection);
                            GetLossDurationDA.Fill(GetLossDurationDT);
                            mc.close();
                        }
                        if (GetLossDurationDT.Rows.Count > 0)
                        {
                            string value = Convert.ToString(GetLossDurationDT.Rows[0][0]);
                            if (Double.TryParse(value, out LossDuration))
                                LossDuration = LossDuration;
                        }
                        LossDetails.Add(new KeyValuePair<string, double>(LossName, LossDuration));
                    }
                    var losslist = LossDetails.OrderByDescending(m => m.Value).ToList();
                    int losscount = losslist.Count;

                    for (int i = 0; i < losscount; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                lossCode1 = losslist[i].Key.ToString();
                                lossCodeVal1 = (int)losslist[i].Value;
                                break;
                            case 1:
                                lossCode2 = losslist[i].Key.ToString();
                                lossCodeVal2 = (int)losslist[i].Value;
                                break;
                            case 2:
                                lossCode3 = losslist[i].Key.ToString();
                                lossCodeVal3 = (int)losslist[i].Value;
                                break;
                            case 3:
                                lossCode4 = losslist[i].Key.ToString();
                                lossCodeVal4 = (int)losslist[i].Value;
                                break;
                            case 4:
                                lossCode5 = losslist[i].Key.ToString();
                                lossCodeVal5 = (int)losslist[i].Value;
                                break;
                        }
                    }
                    #endregion

                    //OEE Values
                    OperatingTime = green;
                    //Availability Factor
                    double val = OperatingTime / AvailableTime;
                    AvaillabilityFactor = Math.Round(val * 100, 2);
                    if (AvaillabilityFactor > 0 && AvaillabilityFactor < 100)
                    {
                        AvaillabilityFactor = Math.Round(val * 100, 2);
                    }
                    else
                    {
                        if (AvaillabilityFactor > 100)
                        {
                            EfficiencyFactor = 100;
                        }
                        else if (AvaillabilityFactor < 0)
                        {
                            EfficiencyFactor = 0;
                        }
                    }

                    //Performance(Efficiency) Factor
                    if (OperatingTime == 0)
                    {
                        EfficiencyFactor = 100;
                    }
                    else
                    {
                        EfficiencyFactor = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 2);
                        if (SummationOfSCTvsPP == -1 || SummationOfSCTvsPP == 0)
                        {
                            EfficiencyFactor = 100;
                        }
                        else
                        {
                            EfficiencyFactor = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 2);
                            if (EfficiencyFactor >= 0 && EfficiencyFactor <= 100)
                            {
                            }
                            else if (EfficiencyFactor > 100 || EfficiencyFactor < 0)
                            {
                                EfficiencyFactor = 100;
                            }
                        }
                    }

                    //QualityFactor
                    if (OperatingTime == 0)
                    {
                        QualityFactor = 0;
                    }
                    else
                    {
                        QualityFactor = (OperatingTime - ScrapQtyTime - ReWOTime) / OperatingTime;
                        if (QualityFactor >= 0 && QualityFactor <= 100)
                        {
                            QualityFactor = Math.Round(QualityFactor * 100, 2);
                        }
                        else if (QualityFactor > 100 || QualityFactor < 0)
                        {
                            QualityFactor = 100;
                        }
                    }

                    //OEE Factor
                    if (AvaillabilityFactor <= 0 || EfficiencyFactor <= 0 || QualityFactor <= 0)
                    {
                        OEEFactor = 0;
                    }
                    else
                    {
                        OEEFactor = Math.Round((AvaillabilityFactor / 100) * (EfficiencyFactor / 100) * (QualityFactor / 100) * 100, 2);
                        if (OEEFactor >= 0 && OEEFactor <= 100)
                        {
                            OEEFactor = Math.Round(OEEFactor, 2);
                        }
                        else if (OEEFactor > 100)
                        {
                            OEEFactor = 100;
                        }
                        else if (OEEFactor < 0)
                        {
                            OEEFactor = 0;
                        }
                    }
                    string IPAddress = GetIPAddressOf();
                    int isShopWise = 0;
                    int isCellWise = 1;
                    int isWCWise = 0;

                    //Now insert into table
                    using (MsqlConnection mcFinalInsertRows = new MsqlConnection())
                    {
                        mcFinalInsertRows.open();
                        try
                        {
                            //db.tbloeedashboardfinalvariables.RemoveRange(db.tbloeedashboardfinalvariables.Where(x => x.IPAddress == IPAddress && x.IsOverallCellWise == isCellWise));
                            //db.SaveChanges();

                            SqlCommand cmdFinalInsertRows = new SqlCommand("INSERT INTO tbloeedashboardfinalvariables(PlantID,ShopID,CellID,WCID,StartDate,EndDate,OEE,Availability,Performance,Quality,IsOverallShopWise,IsOverallCellWise,IsOverallWCWise,Loss1Name,Loss1Value,Loss2Name,Loss2Value,Loss3Name,Loss3Value,Loss4Name,Loss4Value,Loss5Name,Loss5Value,IPAddress,CreatedOn,CreatedBy,IsDeleted)VALUES('" + Convert.ToInt32(PlantIDS) + "','" + Convert.ToInt32(ShopIDS) + "','" + Convert.ToInt32(CellRow) + "','" + Convert.ToInt32(WCID) + "','" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + OEEFactor + "','" + AvaillabilityFactor + "','" + EfficiencyFactor + "','" + QualityFactor + "','" + isShopWise + "','" + isCellWise + "','" + isWCWise + "','" + lossCode1 + "','" + lossCodeVal1 + "','" + lossCode2 + "','" + lossCodeVal2 + "','" + lossCode3 + "','" + lossCodeVal3 + "','" + lossCode4 + "','" + lossCodeVal4 + "','" + lossCode5 + "','" + lossCodeVal5 + "','" + IPAddress + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + 1 + "','" + 0 + "');", mcFinalInsertRows.msqlConnection);
                            cmdFinalInsertRows.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                        }
                        finally
                        {
                            mcFinalInsertRows.close();
                        }
                    }
                }
            }
        }

        //OEE & Losses for all the Work Centre
        public void CalculateSummarizedOEEWC(DateTime fromdate, DateTime todate, int FactorID, string TimeType, string SummarizeAs = null)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            DateTime UsedDateForExcel = Convert.ToDateTime(fromdate);
            double TotalDay = todate.Subtract(fromdate).TotalDays;

            if (TimeType == "Today")
            {
                todate = fromdate;
                #region
                //now push to tbloeedashboardFinalVariables.
                double OEEFactor, AvaillabilityFactor, EfficiencyFactor, QualityFactor;
                string ipAddress = GetIPAddressOf();

                //Gather Hierarchy Details
                string PlantIDS = null, ShopIDS = null, CellIDS = null, WCID = null;
                var getWC = obj.GettbloeecellListDet1(FactorID, ipAddress, fromdate, todate);
                //var getWC = db.tbloeedashboardvariablestodays.Where(m => m.IsDeleted == 0 && m.CellID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress).Select(m => m.WCID).Distinct().ToList();

                if (SummarizeAs == "Plant")
                {
                    getWC = obj.GettbloeepalntListDet1(FactorID, ipAddress, fromdate, todate);
                    //getWC = db.tbloeedashboardvariablestodays.Where(m => m.IsDeleted == 0 && m.PlantID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress).Select(m => m.WCID).Distinct().ToList();
                }
                else if (SummarizeAs == "Shop")
                {
                    getWC = obj.GettbloeeshopListDet1(FactorID, ipAddress, fromdate, todate);
                    // getWC = db.tbloeedashboardvariablestodays.Where(m => m.IsDeleted == 0 && m.ShopID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress).Select(m => m.WCID).Distinct().ToList();
                }
                else if (SummarizeAs == "Cell")
                {
                    getWC = obj.GettbloeecellListDet1(FactorID, ipAddress, fromdate, todate);
                    // getWC = db.tbloeedashboardvariablestodays.Where(m => m.IsDeleted == 0 && m.CellID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress).Select(m => m.WCID).Distinct().ToList();
                }
                else if (SummarizeAs == "WorkCentre")
                {
                    getWC = obj.GettbloeeWCListDet1(FactorID, ipAddress, fromdate, todate);
                    //getWC = db.tbloeedashboardvariablestodays.Where(m => m.IsDeleted == 0 && m.WCID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress).Select(m => m.WCID).Distinct().ToList();
                }
                int once = 0;

                foreach (var WCRow in getWC)
                {
                    double green = 0, red = 0, yellow = 0, blue = 0, setup = 0, scrap = 0, OperatingTime = 0, DownTimeBreakdown = 0, ROALossess = 0, AvailableTime = 0, SettingTime = 0, PlannedDownTime = 0, UnPlannedDownTime = 0;
                    double SummationOfSCTvsPP = 0, MinorLosses = 0, ROPLosses = 0;
                    double ScrapQtyTime = 0, ReWOTime = 0, ROQLosses = 0;
                    int id = Convert.ToInt32(WCRow.WCID);
                    var OEEDataToSummarize = obj.GettbloeeWCListDet2(id, ipAddress, fromdate, todate);
                    //var OEEDataToSummarize = db.tbloeedashboardvariablestodays.Where(m => m.WCID == WCRow && m.StartDate >= fromdate && m.StartDate <= todate && m.IPAddress == ipAddress).ToList();
                    foreach (var row in OEEDataToSummarize)
                    {
                        if (once == 0)
                        {
                            PlantIDS = Convert.ToString(row.PlantID);
                            ShopIDS = Convert.ToString(row.ShopID);
                            CellIDS = Convert.ToString(row.CellID);
                            WCID = Convert.ToString(row.WCID);
                            once++;
                        }

                        MinorLosses += Convert.ToDouble(row.MinorLosses);
                        blue += Convert.ToDouble(row.Blue);
                        green += Convert.ToDouble(row.Green);

                        //Availability
                        SettingTime += Convert.ToDouble(row.SettingTime);
                        ROALossess += Convert.ToDouble(row.ROALossess);
                        DownTimeBreakdown += Convert.ToDouble(row.DownTimeBreakdown);

                        //Performance
                        SummationOfSCTvsPP += Convert.ToDouble(row.SummationOfSCTvsPP);

                        //Quality
                        ScrapQtyTime += Convert.ToDouble(row.ScrapQtyTime);
                        ReWOTime += Convert.ToDouble(row.ReWOTime);
                    }

                    if (TimeType == "Today")
                    {
                        if (DateTime.Now.Hour > 6)
                        {
                            AvailableTime = (DateTime.Now - Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 06:00:00"))).TotalMinutes;
                        }
                        else
                        {
                            AvailableTime = (DateTime.Now - Convert.ToDateTime(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 06:00:00"))).TotalMinutes;
                        }
                    }

                    #region Getting the Top 5 Losses
                    //Have to Calculate based on Summarization condition, use code in CalculateOEE().
                    string GetDistinctLossQuery = "Select Distinct f.lossname from" +
                                             "(" +
                                             "Select Distinct a.Loss1Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday a where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct b.Loss2Name  as lossname from "+ databaseName + ".tbloeedashboardvariablestoday b where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct c.Loss3Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday c where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct d.Loss4Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday d where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct e.Loss5Name as lossname from "+ databaseName + ".tbloeedashboardvariablestoday e where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             ") f order by f.lossname desc";
                    DataTable GetDistinctLossDT = new DataTable();
                    using (MsqlConnection mc = new MsqlConnection())
                    {
                        mc.open();
                        SqlDataAdapter GetDistinctLossDA = new SqlDataAdapter(GetDistinctLossQuery, mc.msqlConnection);
                        GetDistinctLossDA.Fill(GetDistinctLossDT);
                        mc.close();
                    }
                    //Gather LossValues
                    string lossCode1 = null, lossCode2 = null, lossCode3 = null, lossCode4 = null, lossCode5 = null;
                    int lossCodeVal1 = 0, lossCodeVal2 = 0, lossCodeVal3 = 0, lossCodeVal4 = 0, lossCodeVal5 = 0;
                    List<KeyValuePair<String, Double>> LossDetails = new List<KeyValuePair<string, double>>();
                    int LossCount = GetDistinctLossDT.Rows.Count;
                    for (int i = 0; i < LossCount; i++)
                    {
                        String LossName = GetDistinctLossDT.Rows[i][0].ToString();
                        double LossDuration = 0;
                        String GetLossDurationQuery = "Select SUM(Total) From " +
                                       "(Select SUM(Loss1Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday " +
                                       "Where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss1Name = '" + LossName + "'  " +
                                       "Union all " +
                                       "Select SUM(Loss2Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday  " +
                                       "Where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss2Name = '" + LossName + "'  " +
                                       "Union All " +
                                       "Select SUM(Loss3Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday  " +
                                       "Where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss3Name = '" + LossName + "' " +
                                       "Union All " +
                                       "Select SUM(Loss4Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday  " +
                                       "Where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss4Name = '" + LossName + "' " +
                                       "Union All " +
                                       "Select SUM(Loss5Value)  as Total From "+ databaseName + ".tbloeedashboardvariablestoday " +
                                       "Where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss5Name = '" + LossName + "' " +
                                       ") z;";

                        DataTable GetLossDurationDT = new DataTable();
                        using (MsqlConnection mc = new MsqlConnection())
                        {
                            mc.open();
                            SqlDataAdapter GetLossDurationDA = new SqlDataAdapter(GetLossDurationQuery, mc.msqlConnection);
                            GetLossDurationDA.Fill(GetLossDurationDT);
                            mc.close();
                        }
                        if (GetLossDurationDT.Rows.Count > 0)
                        {
                            string value = Convert.ToString(GetLossDurationDT.Rows[0][0]);
                            if (Double.TryParse(value, out LossDuration))
                                LossDuration = LossDuration;
                        }
                        LossDetails.Add(new KeyValuePair<string, double>(LossName, LossDuration));
                    }
                    #endregion

                    var losslist = LossDetails.OrderByDescending(m => m.Value).ToList();
                    int losscount = losslist.Count;

                    for (int i = 0; i < losscount; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                lossCode1 = losslist[i].Key.ToString();
                                lossCodeVal1 = (int)losslist[i].Value;
                                break;
                            case 1:
                                lossCode2 = losslist[i].Key.ToString();
                                lossCodeVal2 = (int)losslist[i].Value;
                                break;
                            case 2:
                                lossCode3 = losslist[i].Key.ToString();
                                lossCodeVal3 = (int)losslist[i].Value;
                                break;
                            case 3:
                                lossCode4 = losslist[i].Key.ToString();
                                lossCodeVal4 = (int)losslist[i].Value;
                                break;
                            case 4:
                                lossCode5 = losslist[i].Key.ToString();
                                lossCodeVal5 = (int)losslist[i].Value;
                                break;
                        }
                    }

                    //OEE Values

                    OperatingTime = green;
                    //Availability Factor
                    double val = OperatingTime / AvailableTime;
                    AvaillabilityFactor = Math.Round(val * 100, 2);
                    if (AvaillabilityFactor > 0 && AvaillabilityFactor < 100)
                    {
                        AvaillabilityFactor = Math.Round(val * 100, 2);
                    }
                    else
                    {
                        if (AvaillabilityFactor > 100)
                        {
                            AvaillabilityFactor = 100;
                        }
                        else if (AvaillabilityFactor < 0)
                        {
                            AvaillabilityFactor = 0;
                        }
                    }

                    if (AvaillabilityFactor > 0)
                    {
                        //Performance(Efficiency) Factor
                        if (OperatingTime == 0)
                        {
                            EfficiencyFactor = 100;
                        }
                        else
                        {
                            EfficiencyFactor = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 2);
                            if (SummationOfSCTvsPP == -1 || SummationOfSCTvsPP == 0)
                            {
                                EfficiencyFactor = 100;
                            }
                            else
                            {
                                EfficiencyFactor = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 2);
                                if (EfficiencyFactor >= 0 && EfficiencyFactor <= 100)
                                {
                                }
                                else if (EfficiencyFactor > 100 || EfficiencyFactor < 0)
                                {
                                    EfficiencyFactor = 100;
                                }
                            }
                        }

                        //QualityFactor
                        if (OperatingTime == 0)
                        {
                            QualityFactor = 0;
                        }
                        else
                        {
                            QualityFactor = (OperatingTime - ScrapQtyTime - ReWOTime) / OperatingTime;
                            if (QualityFactor >= 0 && QualityFactor <= 100)
                            {
                                QualityFactor = Math.Round(QualityFactor * 100, 2);
                            }
                            else if (QualityFactor > 100 || QualityFactor < 0)
                            {
                                QualityFactor = 100;
                            }
                        }
                    }
                    else
                    {
                        EfficiencyFactor = 0; QualityFactor = 0;
                    }

                    //OEE Factor
                    if (AvaillabilityFactor <= 0 || EfficiencyFactor <= 0 || QualityFactor <= 0)
                    {
                        OEEFactor = 0;
                    }
                    else
                    {
                        OEEFactor = Math.Round((AvaillabilityFactor / 100) * (EfficiencyFactor / 100) * (QualityFactor / 100) * 100, 2);
                        if (OEEFactor >= 0 && OEEFactor <= 100)
                        {
                            OEEFactor = Math.Round(OEEFactor, 2);
                        }
                        else if (OEEFactor > 100)
                        {
                            OEEFactor = 100;
                        }
                        else if (OEEFactor < 0)
                        {
                            OEEFactor = 0;
                        }
                    }

                    string IPAddress = GetIPAddressOf();
                    int isShopWise = 0;
                    int isCellWise = 0;
                    int isWCWise = 1;

                    //Now insert into table
                    using (MsqlConnection mcFinalInsertRows = new MsqlConnection())
                    {
                        mcFinalInsertRows.open();
                        try
                        {
                            string query = "INSERT INTO tbloeedashboardfinalvariables(PlantID,ShopID,CellID,WCID,StartDate,EndDate,OEE,Availability,Performance,Quality,IsOverallShopWise,IsOverallCellWise,IsOverallWCWise,Loss1Name,Loss1Value,Loss2Name,Loss2Value,Loss3Name,Loss3Value,Loss4Name,Loss4Value,Loss5Name,Loss5Value,IPAddress,CreatedOn,CreatedBy,IsDeleted,IsToday)VALUES('" + Convert.ToInt32(PlantIDS) + "','" + Convert.ToInt32(ShopIDS) + "','" + Convert.ToInt32(CellIDS) + "','" + Convert.ToInt32(WCRow.WCID) + "','" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + OEEFactor + "','" + AvaillabilityFactor + "','" + EfficiencyFactor + "','" + QualityFactor + "','" + isShopWise + "','" + isCellWise + "','" + isWCWise + "','" + lossCode1 + "','" + lossCodeVal1 + "','" + lossCode2 + "','" + lossCodeVal2 + "','" + lossCode3 + "','" + lossCodeVal3 + "','" + lossCode4 + "','" + lossCodeVal4 + "','" + lossCode5 + "','" + lossCodeVal5 + "','" + IPAddress + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + 1 + "','" + 0 + "','" + 1 + "');";
                            SqlCommand cmdFinalInsertRows = new SqlCommand("INSERT INTO tbloeedashboardfinalvariables(PlantID,ShopID,CellID,WCID,StartDate,EndDate,OEE,Availability,Performance,Quality,IsOverallShopWise,IsOverallCellWise,IsOverallWCWise,Loss1Name,Loss1Value,Loss2Name,Loss2Value,Loss3Name,Loss3Value,Loss4Name,Loss4Value,Loss5Name,Loss5Value,IPAddress,CreatedOn,CreatedBy,IsDeleted,IsToday)VALUES('" + Convert.ToInt32(PlantIDS) + "','" + Convert.ToInt32(ShopIDS) + "','" + Convert.ToInt32(CellIDS) + "','" + Convert.ToInt32(WCRow.WCID) + "','" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + OEEFactor + "','" + AvaillabilityFactor + "','" + EfficiencyFactor + "','" + QualityFactor + "','" + isShopWise + "','" + isCellWise + "','" + isWCWise + "','" + lossCode1 + "','" + lossCodeVal1 + "','" + lossCode2 + "','" + lossCodeVal2 + "','" + lossCode3 + "','" + lossCodeVal3 + "','" + lossCode4 + "','" + lossCodeVal4 + "','" + lossCode5 + "','" + lossCodeVal5 + "','" + IPAddress + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + 1 + "','" + 0 + "','" + 1 + "');", mcFinalInsertRows.msqlConnection);
                            cmdFinalInsertRows.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            //operationlog log1 = new operationlog();
                            //log1.MachineID = FactorID;
                            string OpMsg = " Value of Query " + WCRow + " " + e.Message;
                            DateTime? OpDate = System.DateTime.Now.Date;
                            DateTime? OpDateTime = System.DateTime.Now;
                            TimeSpan? OpTime = System.DateTime.Now.TimeOfDay;

                            obj.InsertoperationlogDetails(FactorID, OpMsg, OpDate, OpDateTime, OpTime);
                        }
                        finally
                        {
                            mcFinalInsertRows.close();
                        }
                    }
                }
                #endregion
            }
            else if (TimeType == "GodHours" || TimeType == "NoBlue")
            {
                //now push to tbloeedashboardFinalVariables.
                double OEEFactor, AvaillabilityFactor, EfficiencyFactor, QualityFactor;

                //Gather Hierarchy Details
                string PlantIDS = null, ShopIDS = null, CellIDS = null, WCID = null;
                var getWC = obj.GettbloeecellvariableListDet1(FactorID, fromdate, todate);
                //var getWC = db.tbloeedashboardvariables.Where(m => m.IsDeleted == 0 && m.CellID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate).Select(m => m.WCID).Distinct().ToList();

                if (SummarizeAs == "Plant")
                {
                    getWC = obj.GettbloeepalntvariableListDet1(FactorID, fromdate, todate);
                    //getWC = db.tbloeedashboardvariables.Where(m => m.IsDeleted == 0 && m.PlantID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate).Select(m => m.WCID).Distinct().ToList();
                }
                else if (SummarizeAs == "Shop")
                {
                    getWC = obj.GettbloeeshopvariableListDet1(FactorID, fromdate, todate);
                    //getWC = db.tbloeedashboardvariables.Where(m => m.IsDeleted == 0 && m.ShopID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate).Select(m => m.WCID).Distinct().ToList();
                }
                else if (SummarizeAs == "Cell")
                {
                    getWC = obj.GettbloeecellvariableListDet1(FactorID, fromdate, todate);
                    //getWC = db.tbloeedashboardvariables.Where(m => m.IsDeleted == 0 && m.CellID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate).Select(m => m.WCID).Distinct().ToList();
                }
                else if (SummarizeAs == "WorkCentre")
                {
                    getWC = obj.GettbloeeWCvariableListDet1(FactorID, fromdate, todate);
                    //getWC = db.tbloeedashboardvariables.Where(m => m.IsDeleted == 0 && m.WCID == FactorID && m.StartDate >= fromdate && m.StartDate <= todate).Select(m => m.WCID).Distinct().ToList();
                }
                int once = 0;

                foreach (var WCRow in getWC)
                {
                    double green = 0, red = 0, yellow = 0, blue = 0, setup = 0, scrap = 0, OperatingTime = 0, DownTimeBreakdown = 0, ROALossess = 0, AvailableTime = 0, SettingTime = 0, PlannedDownTime = 0, UnPlannedDownTime = 0;
                    double SummationOfSCTvsPP = 0, MinorLosses = 0, ROPLosses = 0;
                    double ScrapQtyTime = 0, ReWOTime = 0, ROQLosses = 0;
                    int id = Convert.ToInt32(WCRow.WCID);
                    var OEEDataToSummarize = obj.GettbloeeWCvariableListDet1(id, fromdate, todate);
                    // var OEEDataToSummarize = db.tbloeedashboardvariables.Where(m => m.WCID == WCRow && m.StartDate >= fromdate && m.StartDate <= todate).ToList();
                    foreach (var row in OEEDataToSummarize)
                    {
                        if (once == 0)
                        {
                            PlantIDS = Convert.ToString(row.PlantID);
                            ShopIDS = Convert.ToString(row.ShopID);
                            CellIDS = Convert.ToString(row.CellID);
                            WCID = Convert.ToString(row.WCID);
                            once++;
                        }

                        MinorLosses += Convert.ToDouble(row.MinorLosses);
                        blue += Convert.ToDouble(row.Blue);
                        green += Convert.ToDouble(row.Green);

                        //Availability
                        SettingTime += Convert.ToDouble(row.SettingTime);
                        ROALossess += Convert.ToDouble(row.ROALossess);
                        DownTimeBreakdown += Convert.ToDouble(row.DownTimeBreakdown);

                        //Performance
                        SummationOfSCTvsPP += Convert.ToDouble(row.SummationOfSCTvsPP);

                        //Quality
                        ScrapQtyTime += Convert.ToDouble(row.ScrapQtyTime);
                        ReWOTime += Convert.ToDouble(row.ReWOTime);
                    }

                    int Days = Convert.ToInt32(todate.Subtract(fromdate).TotalDays) + 1;
                    if (TimeType == "GodHours")
                    {
                        AvailableTime = 24 * 60 * Days; //24Hours to Minutes;
                    }
                    else if (TimeType == "NoBlue")
                    {
                        AvailableTime = (24 * 60 * Days) - blue; //24Hours to Minutes;
                    }

                    #region Getting the Top 5 Losses
                    //Have to Calculate based on Summarization condition, use code in CalculateOEE().
                    /*     string GetDistinctLossQuery = "Select Distinct f.lossname from" +
                                                  "(" +
                                                  "Select Distinct a.Loss1Name as lossname from "+ databaseName + ".tbloeedashboardvariables a where WCID = " + WCRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                  "Union All " +
                                                  "Select Distinct b.Loss2Name  as lossname from "+ databaseName + ".tbloeedashboardvariables b where WCID = " + WCRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                  "Union All " +
                                                  "Select Distinct c.Loss3Name as lossname from "+ databaseName + ".tbloeedashboardvariables c where WCID = " + WCRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                  "Union All " +
                                                  "Select Distinct d.Loss4Name as lossname from "+ databaseName + ".tbloeedashboardvariables d where WCID = " + WCRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                  "Union All " +
                                                  "Select Distinct e.Loss5Name as lossname from "+ databaseName + ".tbloeedashboardvariables e where WCID = " + WCRow + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                                  ") f order by f.lossname desc";*/
                    string GetDistinctLossQuery = "Select Distinct f.lossname from" +
                                             "(" +
                                             "Select Distinct a.Loss1Name as lossname from "+ databaseName + ".tbloeedashboardvariables a where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct b.Loss2Name  as lossname from "+ databaseName + ".tbloeedashboardvariables b where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct c.Loss3Name as lossname from "+ databaseName + ".tbloeedashboardvariables c where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct d.Loss4Name as lossname from "+ databaseName + ".tbloeedashboardvariables d where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             "Union All " +
                                             "Select Distinct e.Loss5Name as lossname from "+ databaseName + ".tbloeedashboardvariables e where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' " +
                                             ") f order by f.lossname desc";
                    MsqlConnection mc = new MsqlConnection();
                    mc.open();
                    SqlDataAdapter GetDistinctLossDA = new SqlDataAdapter(GetDistinctLossQuery, mc.msqlConnection);
                    DataTable GetDistinctLossDT = new DataTable();
                    GetDistinctLossDA.Fill(GetDistinctLossDT);
                    mc.close();
                    //Gather LossValues
                    string lossCode1 = null, lossCode2 = null, lossCode3 = null, lossCode4 = null, lossCode5 = null;
                    int lossCodeVal1 = 0, lossCodeVal2 = 0, lossCodeVal3 = 0, lossCodeVal4 = 0, lossCodeVal5 = 0;
                    List<KeyValuePair<String, Double>> LossDetails = new List<KeyValuePair<string, double>>();
                    int LossCount = GetDistinctLossDT.Rows.Count;
                    for (int i = 0; i < LossCount; i++)
                    {
                        String LossName = GetDistinctLossDT.Rows[i][0].ToString();
                        double LossDuration = 0;
                        String GetLossDurationQuery = "Select SUM(Total) From " +
                                       "(Select SUM(Loss1Value)  as Total From "+ databaseName + ".tbloeedashboardvariables " +
                                       "Where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss1Name = '" + LossName + "'  " +
                                       "Union all " +
                                       "Select SUM(Loss2Value)  as Total From "+ databaseName + ".tbloeedashboardvariables  " +
                                       "Where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss2Name = '" + LossName + "'  " +
                                       "Union All " +
                                       "Select SUM(Loss3Value)  as Total From "+ databaseName + ".tbloeedashboardvariables  " +
                                       "Where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss3Name = '" + LossName + "' " +
                                       "Union All " +
                                       "Select SUM(Loss4Value)  as Total From "+ databaseName + ".tbloeedashboardvariables  " +
                                       "Where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss4Name = '" + LossName + "' " +
                                       "Union All " +
                                       "Select SUM(Loss5Value)  as Total From "+ databaseName + ".tbloeedashboardvariables " +
                                       "Where WCID = " + WCRow.WCID + " and StartDate Between '" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "' AND Loss5Name = '" + LossName + "' " +
                                       ") z;";
                        mc.open();
                        SqlDataAdapter GetLossDurationDA = new SqlDataAdapter(GetLossDurationQuery, mc.msqlConnection);
                        DataTable GetLossDurationDT = new DataTable();
                        GetLossDurationDA.Fill(GetLossDurationDT);
                        mc.close();
                        if (GetLossDurationDT.Rows.Count > 0)
                        {
                            string value = Convert.ToString(GetLossDurationDT.Rows[0][0]);
                            if (Double.TryParse(value, out LossDuration))
                                LossDuration = LossDuration;
                        }
                        LossDetails.Add(new KeyValuePair<string, double>(LossName, LossDuration));
                    }
                    #endregion

                    var losslist = LossDetails.OrderByDescending(m => m.Value).ToList();
                    int losscount = losslist.Count;

                    for (int i = 0; i < losscount; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                lossCode1 = losslist[i].Key.ToString();
                                lossCodeVal1 = (int)losslist[i].Value;
                                break;
                            case 1:
                                lossCode2 = losslist[i].Key.ToString();
                                lossCodeVal2 = (int)losslist[i].Value;
                                break;
                            case 2:
                                lossCode3 = losslist[i].Key.ToString();
                                lossCodeVal3 = (int)losslist[i].Value;
                                break;
                            case 3:
                                lossCode4 = losslist[i].Key.ToString();
                                lossCodeVal4 = (int)losslist[i].Value;
                                break;
                            case 4:
                                lossCode5 = losslist[i].Key.ToString();
                                lossCodeVal5 = (int)losslist[i].Value;
                                break;
                        }
                    }

                    //OEE Values

                    OperatingTime = green;
                    //Availability Factor
                    double val = OperatingTime / AvailableTime;
                    AvaillabilityFactor = Math.Round(val * 100, 2);
                    if (AvaillabilityFactor > 0 && AvaillabilityFactor < 100)
                    {
                        AvaillabilityFactor = Math.Round(val * 100, 2);
                    }
                    else
                    {
                        if (AvaillabilityFactor > 100)
                        {
                            AvaillabilityFactor = 100;
                        }
                        else if (AvaillabilityFactor < 0)
                        {
                            AvaillabilityFactor = 0;
                        }
                    }

                    if (AvaillabilityFactor > 0)
                    {
                        //Performance(Efficiency) Factor
                        if (OperatingTime == 0)
                        {
                            EfficiencyFactor = 100;
                        }
                        else
                        {

                            EfficiencyFactor = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 2);
                            if (SummationOfSCTvsPP == -1 || SummationOfSCTvsPP == 0)
                            {
                                EfficiencyFactor = 100;
                            }
                            else
                            {
                                EfficiencyFactor = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 2);
                                if (EfficiencyFactor >= 0 && EfficiencyFactor <= 100)
                                {
                                }
                                else if (EfficiencyFactor > 100 || EfficiencyFactor < 0)
                                {
                                    EfficiencyFactor = 100;
                                }
                            }
                        }
                        //QualityFactor
                        if (OperatingTime == 0)
                        {
                            QualityFactor = 0;
                        }
                        else
                        {
                            QualityFactor = (OperatingTime - ScrapQtyTime - ReWOTime) / OperatingTime;
                            if (QualityFactor >= 0 && QualityFactor <= 100)
                            {
                                QualityFactor = Math.Round(QualityFactor * 100, 2);
                            }
                            else if (QualityFactor > 100 || QualityFactor < 0)
                            {
                                QualityFactor = 100;
                            }
                        }
                    }
                    else
                    {
                        EfficiencyFactor = 0; QualityFactor = 0;
                    }

                    //OEE Factor
                    if (AvaillabilityFactor <= 0 || EfficiencyFactor <= 0 || QualityFactor <= 0)
                    {
                        OEEFactor = 0;
                    }
                    else
                    {
                        OEEFactor = Math.Round((AvaillabilityFactor / 100) * (EfficiencyFactor / 100) * (QualityFactor / 100) * 100, 2);
                        if (OEEFactor >= 0 && OEEFactor <= 100)
                        {
                            OEEFactor = Math.Round(OEEFactor, 2);
                        }
                        else if (OEEFactor > 100)
                        {
                            OEEFactor = 100;
                        }
                        else if (OEEFactor < 0)
                        {
                            OEEFactor = 0;
                        }
                    }

                    string IPAddress = GetIPAddressOf();
                    int isShopWise = 0;
                    int isCellWise = 0;
                    int isWCWise = 1;

                    //Now insert into table
                    MsqlConnection mcFinalInsertRows = new MsqlConnection();
                    mcFinalInsertRows.open();
                    try
                    {
                        SqlCommand cmdFinalInsertRows = new SqlCommand("INSERT INTO tbloeedashboardfinalvariables(PlantID,ShopID,CellID,WCID,StartDate,EndDate,OEE,Availability,Performance,Quality,IsOverallShopWise,IsOverallCellWise,IsOverallWCWise,Loss1Name,Loss1Value,Loss2Name,Loss2Value,Loss3Name,Loss3Value,Loss4Name,Loss4Value,Loss5Name,Loss5Value,IPAddress,CreatedOn,CreatedBy,IsDeleted)VALUES('" + Convert.ToInt32(PlantIDS) + "','" + Convert.ToInt32(ShopIDS) + "','" + Convert.ToInt32(CellIDS) + "','" + Convert.ToInt32(WCRow.WCID) + "','" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + OEEFactor + "','" + AvaillabilityFactor + "','" + EfficiencyFactor + "','" + QualityFactor + "','" + isShopWise + "','" + isCellWise + "','" + isWCWise + "','" + lossCode1 + "','" + lossCodeVal1 + "','" + lossCode2 + "','" + lossCodeVal2 + "','" + lossCode3 + "','" + lossCodeVal3 + "','" + lossCode4 + "','" + lossCodeVal4 + "','" + lossCode5 + "','" + lossCodeVal5 + "','" + IPAddress + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + 1 + "','" + 0 + "');", mcFinalInsertRows.msqlConnection);
                        // cmdFinalInsertRows.ExecuteNonQuery();

                        //SqlCommand cmdFinalInsertRows = new SqlCommand("INSERT INTO tbloeedashboardfinalvariables(PlantID,ShopID,CellID,WCID,StartDate,EndDate,OEE,Availability,Performance,Quality,IsOverallShopWise,IsOverallWCWise,Loss1Name,Loss1Value,Loss2Name,Loss2Value,Loss3Name,Loss3Value,Loss4Name,Loss4Value,Loss5Name,Loss5Value,IPAddress,CreatedOn,CreatedBy,IsDeleted,IsToday)VALUES('" + Convert.ToInt32(PlantIDS) + "','" + Convert.ToInt32(ShopIDS) + "','" + Convert.ToInt32(CellIDS) + "','" + Convert.ToInt32(WCID) + "','" + fromdate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + todate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + OEEFactor + "','" + AvaillabilityFactor + "','" + EfficiencyFactor + "','" + QualityFactor + "','" + isShopWise + "','" + isCellWise + "','" + lossCode1 + "','" + lossCodeVal1 + "','" + lossCode2 + "','" + lossCodeVal2 + "','" + lossCode3 + "','" + lossCodeVal3 + "','" + lossCode4 + "','" + lossCodeVal4 + "','" + lossCode5 + "','" + lossCodeVal5 + "','" + IPAddress + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + 1 + "','" + 0 + "','" + 1 + "');", mcFinalInsertRows.msqlConnection);
                        cmdFinalInsertRows.ExecuteNonQuery();
                        mcFinalInsertRows.close();
                    }
                    catch (Exception e)
                    {
                        string OpMsg = " Value of Query " + WCRow + " " + e.Message;
                        DateTime? OpDate = System.DateTime.Now.Date;
                        DateTime? OpDateTime = System.DateTime.Now;
                        TimeSpan? OpTime = System.DateTime.Now.TimeOfDay;

                        obj.InsertoperationlogDetails(FactorID, OpMsg, OpDate, OpDateTime, OpTime);
                        //operationlog log1 = new operationlog();
                        //log1.MachineID = FactorID;
                        //log1.OpMsg = " Value of Query " + WCRow + " " + e.Message;
                        //log1.OpDate = System.DateTime.Now.Date;
                        //log1.OpDateTime = System.DateTime.Now;
                        //log1.OpTime = System.DateTime.Now.TimeOfDay;
                        //db.operationlogs.Add(log1);
                        //db.SaveChanges();
                    }
                    finally
                    {
                        mcFinalInsertRows.close();
                    }
                }
            }
        }

        //Push to First Table . WC Wise Independent.
        public void CalculateOEE(DateTime fromdate, DateTime todate, int MachineID, string TimeType, string SummarizeAs = null)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            DateTime UsedDateForExcel = Convert.ToDateTime(fromdate);
            double TotalDay = todate.Subtract(fromdate).TotalDays;
            #region
            for (int i = 0; i < TotalDay + 1; i++)
            {
                var OEEDataPresent = obj.GettbloeeWCvariableListDet2(MachineID, UsedDateForExcel);
                // var OEEDataPresent = db.tbloeedashboardvariables.Where(m => m.WCID == MachineID && m.StartDate == UsedDateForExcel).ToList();
                if (OEEDataPresent.Count == 0)
                {
                    //Don't do anything 2017-04-05
                    //double green, red, yellow, blue, setup = 0, scrap = 0, NOP = 0, OperatingTime = 0, DownTimeBreakdown = 0, ROALossess = 0, AvailableTime = 0, SettingTime = 0, PlannedDownTime = 0, UnPlannedDownTime = 0;
                    //double SummationOfSCTvsPP = 0, MinorLosses = 0, ROPLosses = 0;
                    //double ScrapQtyTime = 0, ReWOTime = 0, ROQLosses = 0;

                    //MinorLosses = GetMinorLosses(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID, "yellow");
                    //if (MinorLosses < 0)
                    //{
                    //    MinorLosses = 0;
                    //}
                    //blue = GetOPIDleBreakDown(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID, "blue");
                    //green = GetOPIDleBreakDown(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID, "green");

                    ////Availability
                    //SettingTime = GetSettingTime(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID);
                    //if (SettingTime < 0)
                    //{
                    //    SettingTime = 0;
                    //}
                    //ROALossess = GetDownTimeLosses(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID, "ROA");
                    //if (ROALossess < 0)
                    //{
                    //    ROALossess = 0;
                    //}
                    //DownTimeBreakdown = GetDownTimeBreakdown(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID);
                    //if (DownTimeBreakdown < 0)
                    //{
                    //    DownTimeBreakdown = 0;
                    //}

                    ////Performance
                    //SummationOfSCTvsPP = GetSummationOfSCTvsPP(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID);
                    //if (SummationOfSCTvsPP <= 0)
                    //{
                    //    SummationOfSCTvsPP = 0;
                    //}
                    ////ROPLosses = GetDownTimeLosses(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID, "ROP");

                    ////Quality
                    //ScrapQtyTime = GetScrapQtyTimeOfWO(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID);
                    //if (ScrapQtyTime < 0)
                    //{
                    //    ScrapQtyTime = 0;
                    //}
                    //ReWOTime = GetScrapQtyTimeOfRWO(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID);
                    //if (ReWOTime < 0)
                    //{
                    //    ReWOTime = 0;
                    //}

                    //if (TimeType == "GodHours")
                    //{
                    //    AvailableTime = AvailableTime = 24 * 60; //24Hours to Minutes;
                    //}
                    //else if (TimeType == "NoBlue")
                    //{
                    //    AvailableTime = AvailableTime = (24 * 60) - blue; //24Hours to Minutes - Blue;
                    //}

                    //OperatingTime = green;

                    ////To get Top 5 Losses for this WC
                    //string todayAsCorrectedDate = UsedDateForExcel.ToString("yyyy-MM-dd");
                    //var lossData = db.tbllossofentries.Where(m => m.CorrectedDate == todayAsCorrectedDate && m.MachineID == MachineID).ToList();

                    //DataTable DTLosses = new DataTable();
                    //DTLosses.Columns.Add("lossCodeID", typeof(int));
                    //DTLosses.Columns.Add("LossDuration", typeof(int));
                    //foreach (var row in lossData)
                    //{
                    //    int lossCodeID = Convert.ToInt32(row.MessageCodeID);
                    //    DateTime startDate = Convert.ToDateTime(row.StartDateTime);
                    //    DateTime endDate = Convert.ToDateTime(row.EndDateTime);
                    //    int duration = Convert.ToInt32(endDate.Subtract(startDate).TotalMinutes);

                    //    DataRow dr = DTLosses.Select("lossCodeID= '" + lossCodeID + "'").FirstOrDefault(); // finds all rows with id==2 and selects first or null if haven't found any
                    //    if (dr != null)
                    //    {
                    //        int LossDurationPrev = Convert.ToInt32(dr["LossDuration"]); //get lossduration and update it.
                    //        dr["LossDuration"] = (LossDurationPrev + duration);
                    //    }
                    //    //}
                    //    else
                    //    {
                    //        DTLosses.Rows.Add(lossCodeID, duration);
                    //    }
                    //}
                    //DataTable DTLossesTop5 = DTLosses.Clone();
                    ////get only the rows you want
                    //DataRow[] results = DTLosses.Select("", "LossDuration DESC");
                    ////populate new destination table
                    //if (DTLosses.Rows.Count > 0)
                    //{
                    //    int num = DTLosses.Rows.Count;
                    //    for (var iDT = 0; iDT < num; iDT++)
                    //    {
                    //        if (results[iDT] != null)
                    //        {
                    //            DTLossesTop5.ImportRow(results[iDT]);
                    //        }
                    //        else
                    //        {
                    //            DTLossesTop5.Rows.Add(0, 0);
                    //        }
                    //        if (iDT == 4)
                    //        {
                    //            break;
                    //        }
                    //    }
                    //    if (num < 5)
                    //    {
                    //        for (var iDT = num; iDT < 5; iDT++)
                    //        {
                    //            DTLossesTop5.Rows.Add(0, 0);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    for (var iDT = 0; iDT < 5; iDT++)
                    //    {
                    //        DTLossesTop5.Rows.Add(0, 0);
                    //    }
                    //}
                    ////Gather LossValues
                    //string lossCode1, lossCode2, lossCode3, lossCode4, lossCode5 = null;
                    //int lossCodeVal1, lossCodeVal2, lossCodeVal3, lossCodeVal4, lossCodeVal5 = 0;

                    //lossCode1 = Convert.ToString(DTLossesTop5.Rows[0][0]);
                    //lossCode2 = Convert.ToString(DTLossesTop5.Rows[1][0]);
                    //lossCode3 = Convert.ToString(DTLossesTop5.Rows[2][0]);
                    //lossCode4 = Convert.ToString(DTLossesTop5.Rows[3][0]);
                    //lossCode5 = Convert.ToString(DTLossesTop5.Rows[4][0]);
                    //lossCodeVal1 = Convert.ToInt32(DTLossesTop5.Rows[0][1]);
                    //lossCodeVal2 = Convert.ToInt32(DTLossesTop5.Rows[1][1]);
                    //lossCodeVal3 = Convert.ToInt32(DTLossesTop5.Rows[2][1]);
                    //lossCodeVal4 = Convert.ToInt32(DTLossesTop5.Rows[3][1]);
                    //lossCodeVal5 = Convert.ToInt32(DTLossesTop5.Rows[4][1]);

                    ////Gather Plant,Shop,Cell for WC.
                    //int PlantID = 0, ShopID = 0, CellID = 0;
                    //string PlantIDS = null, ShopIDS = null, CellIDS = null;
                    //int value;
                    //var WCData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.IsNormalWC == 0).FirstOrDefault();
                    //string TempVal = WCData.PlantID.ToString();
                    //if (int.TryParse(TempVal, out value))
                    //{
                    //    PlantIDS = value.ToString();
                    //}

                    //TempVal = WCData.ShopID.ToString();
                    //if (int.TryParse(TempVal, out value))
                    //{
                    //    ShopIDS = value.ToString();
                    //}

                    //TempVal = WCData.CellID.ToString();
                    //if (int.TryParse(TempVal, out value))
                    //{
                    //    CellIDS = value.ToString();
                    //}


                    ////Now insert into table
                    //MsqlConnection mcInsertRows = new MsqlConnection();
                    //try
                    //{
                    //    mcInsertRows.open();
                    //    SqlCommand cmdInsertRows = new SqlCommand("INSERT INTO tbloeedashboardvariables(PlantID,ShopID,CellID,WCID,StartDate,EndDate,MinorLosses,Blue,Green,SettingTime,ROALossess,DownTimeBreakdown,SummationOfSCTvsPP,ScrapQtyTime,ReWOTime,Loss1Name,Loss1Value,Loss2Name,Loss2Value,Loss3Name,Loss3Value,Loss4Name,Loss4Value,Loss5Name,Loss5Value,CreatedOn,CreatedBy,IsDeleted)VALUES('" + PlantIDS + "','" + ShopIDS + "','" + CellIDS + "','" + MachineID + "','" + UsedDateForExcel.ToString("yyyy-MM-dd") + "','" + UsedDateForExcel.ToString("yyyy-MM-dd") + "','" + MinorLosses + "','" + blue + "','" + green + "','" + SettingTime + "','" + ROALossess + "','" + DownTimeBreakdown + "','" + SummationOfSCTvsPP + "','" + ScrapQtyTime + "','" + ReWOTime + "','" + lossCode1 + "','" + lossCodeVal1 + "','" + lossCode2 + "','" + lossCodeVal2 + "','" + lossCode3 + "','" + lossCodeVal3 + "','" + lossCode4 + "','" + lossCodeVal4 + "','" + lossCode5 + "','" + lossCodeVal5 + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + 1 + "','" + 0 + "');", mcInsertRows.msqlConnection);
                    //    cmdInsertRows.ExecuteNonQuery();
                    //}
                    //catch (Exception e)
                    //{
                    //}
                    //finally
                    //{
                    //    mcInsertRows.close();
                    //}
                }
                UsedDateForExcel = UsedDateForExcel.AddDays(+1);
            }
            #endregion

            UsedDateForExcel = Convert.ToDateTime(fromdate);
            //now push to tbloeedashboardFinalVariables.
        }

        public string GetIPAddressOf()
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            string IP_Address = null;
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    IP_Address = addresses[0];
                }
            }
            //Use this for client IP Address
            IP_Address = context.Request.ServerVariables["REMOTE_ADDR"];

            return IP_Address;
        }

        public JsonResult GetShop(int PlantID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            var ShopData = new SelectList(obj.GetShop1List(PlantID), "ShopID", "ShopName");
            //var ShopData = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName");
            return Json(ShopData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCell(int ShopID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            var CellData = new SelectList(obj.GetCell1List(ShopID), "CellID", "CellName");
            //var CellData = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID), "CellID", "CellName");
            return Json(CellData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetWC_Cell(int CellID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            var MachineData = new SelectList(obj.GetmachinecellList(CellID), "MachineID", "MachineInvNo");
            //var MachineData = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID && m.IsNormalWC == 0), "MachineID", "MachineInvNo");
            return Json(MachineData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetWC_Shop(int ShopID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            var MachineData = new SelectList(obj.GetmachineshopcellList(ShopID), "MachineID", "MachineInvNo");
            //var MachineData = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID && m.CellID.Equals(null) && m.IsNormalWC == 0), "MachineID", "MachineInvNo");
            return Json(MachineData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSummarizeDropDownValues(string Factor)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            List<SelectListItem> SummerizeAs = new List<SelectListItem>();
            if (Factor == "Plant")
            {
                SummerizeAs.Add(new SelectListItem
                {
                    Text = "Shop",
                    Value = "Shop",
                });
                SummerizeAs.Add(new SelectListItem
                {
                    Text = "Cell",
                    Value = "Cell",
                });
                SummerizeAs.Add(new SelectListItem
                {
                    Text = "WorkCenter",
                    Value = "WorkCenter",
                });
            }
            if (Factor == "Shop")
            {
                SummerizeAs.Add(new SelectListItem
                {
                    Text = "Cell",
                    Value = "Cell",
                });
                SummerizeAs.Add(new SelectListItem
                {
                    Text = "WorkCenter",
                    Value = "WorkCenter",
                });
            }
            if (Factor == "Cell")
            {
                SummerizeAs.Add(new SelectListItem
                {
                    Text = "WorkCenter",
                    Value = "WorkCenter",
                });
            }
            if (Factor == "WC")
            {
            }

            return Json(SummerizeAs, JsonRequestBehavior.AllowGet);
        }

        //2017-04-05 GetMinorLosses //public int GetMinorLosses(string CorrectedDate, int MachineID, string Colour)
        //{
        //    DateTime currentdate = Convert.ToDateTime(CorrectedDate);
        //    string datetime = currentdate.ToString("yyyy-MM-dd");

        //    int minorloss = 0;
        //    int count = 0;
        //    var Data = db.tbldailyprodstatus.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.CorrectedDate == CorrectedDate).OrderBy(m => m.StartTime).ToList();
        //    foreach (var row in Data)
        //    {
        //        if (row.ColorCode == "yellow")
        //        {
        //            count++;
        //        }
        //        else
        //        {
        //            if (count > 0 && count < 2)
        //            {
        //                minorloss += count;
        //                count = 0;

        //            }
        //            count = 0;
        //        }
        //    }
        //    return minorloss;
        //}

        public int GetMinorLosses(string CorrectedDate, int MachineID, string Colour)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            DateTime currentdate = Convert.ToDateTime(CorrectedDate);
            string datetime = currentdate.ToString("yyyy-MM-dd");

            int minorloss = 0;
            var modeData = obj.GetmodeList(MachineID, CorrectedDate);
            //var modeData = dbmode.tblmodes.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.CorrectedDate == CorrectedDate).OrderBy(m => m.InsertedOn).ToList();
            for (int j = 0; j < modeData.Count(); j++)
            {
                if (modeData[j].Mode == "IDLE")
                {
                    DateTime endTime = modeData[j].InsertedOn;
                    DateTime startTime = modeData[j].InsertedOn;
                    TimeSpan span = endTime.Subtract(startTime);
                    int Duration = Convert.ToInt32(span.Minutes);
                    if (Duration <= 2)
                    {
                        minorloss += Duration;
                    }
                }
            }
            return minorloss;
        }

        public int GetOPIDleBreakDown(string CorrectedDate, int MachineID, string Colour)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            DateTime currentdate = Convert.ToDateTime(CorrectedDate);
            string datetime = currentdate.ToString("yyyy-MM-dd");

            int[] count = new int[4];
            DataTable OP = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                //operating
                mc.open();
                String query1 = "SELECT count(ID) From tbldailyprodstatus WHERE CorrectedDate='" + CorrectedDate + "' AND MachineID=" + MachineID + " AND ColorCode='" + Colour + "'";
                SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                da1.Fill(OP);
                mc.close();
            }
            if (OP.Rows.Count != 0)
            {
                count[0] = Convert.ToInt32(OP.Rows[0][0]);
            }
            return count[0];
        }

        public double GetSettingTime(string UsedDateForExcel, int MachineID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            double settingTime = 0;
            int setupid = 0;
            string settingString = "Setup";
            var setupiddata = obj.GettbloeelossDet3(settingString);
            // var setupiddata = dbloss.tbllossescodes.Where(m => m.MessageType.Contains(settingString)).FirstOrDefault();
            if (setupiddata != null)
            {
                setupid = setupiddata.LossCodeID;
            }
            else
            {
                Session["Error"] = "Unable to get Setup's ID";
                return -1;
            }
            //getting all setup's sublevels ids.
            var SettingIDs = obj.GettbllossescodeDet1(setupid);
            // var SettingIDs = dbloss.tbllossescodes.Where(m => m.LossCodesLevel1ID == setupid || m.LossCodesLevel2ID == setupid).Select(m => m.LossCodeID).ToList();

            //settingTime = (from row in db.tbllossofentries
            //               where  row.CorrectedDate == UsedDateForExcel && row.MachineID == MachineID );

            var SettingData = obj.GettbllossofentryDet1(SettingIDs, MachineID, UsedDateForExcel);
            // var SettingData = dbloss.tbllossofentries.Where(m => SettingIDs.Contains(m.MessageCodeID) && m.MachineID == MachineID && m.CorrectedDate == UsedDateForExcel && m.DoneWithRow == 1).ToList();
            foreach (var row in SettingData)
            {
                DateTime startTime = Convert.ToDateTime(row.StartDateTime);
                DateTime endTime = Convert.ToDateTime(row.EndDateTime);
                settingTime += endTime.Subtract(startTime).TotalMinutes;
            }
            return settingTime;
        }
        public double GetDownTimeLosses(string UsedDateForExcel, int MachineID, string contribute)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            double LossTime = 0;
            //string contribute = "ROA";
            //getting all ROA sublevels ids. Only those of IDLE.
            var SettingIDs = obj.GettbllossescodeDet2(contribute);
            //var SettingIDs = dbloss.tbllossescodes.Where(m => m.ContributeTo == contribute && (m.MessageType != "PM" || m.MessageType != "BREAKDOWN")).Select(m => m.LossCodeID).ToList();
            var SettingData = obj.GettbllossofentryDet1(SettingIDs, MachineID, UsedDateForExcel);
            //var SettingData = dbloss.tbllossofentries.Where(m => SettingIDs.Contains(m.MessageCodeID) && m.MachineID == MachineID && m.CorrectedDate == UsedDateForExcel && m.DoneWithRow == 1).ToList();
            foreach (var row in SettingData)
            {
                DateTime startTime = Convert.ToDateTime(row.StartDateTime);
                DateTime endTime = Convert.ToDateTime(row.EndDateTime);
                LossTime += endTime.Subtract(startTime).TotalMinutes;
            }
            return LossTime;
        }
        public double GetDownTimeBreakdown(string UsedDateForExcel, int MachineID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn); double LossTime = 0;
            var BreakdownData = obj.GettblbreakdownDet1(MachineID, UsedDateForExcel);
            // var BreakdownData = dbbreak.tblbreakdowns.Where(m => m.MachineID == MachineID && m.CorrectedDate == UsedDateForExcel && m.DoneWithRow == 1).ToList();
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
            return LossTime;
        }

        public double GetSummationOfSCTvsPP(string UsedDateForExcel, int MachineID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn); double SummationofTime = 0;

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

            List<string> OccuredWOs = new List<string>();
            //To Extract Single WorkOrder Cutting Time
            var PartsDataAll = obj.GettblhmiscreensDet1(MachineID, UsedDateForExcel);
            // var PartsDataAll = dbhmi.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && m.IsMultiWO == 0 && m.isWorkOrder == 0 && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0)).OrderByDescending(m => m.HMIID).ToList();
            if (PartsDataAll.Count == 0)
            {
                //return SummationofTime;
            }
            foreach (var row in PartsDataAll)
            {
                string partNo = row.PartNo;
                string woNo = row.Work_Order_No;
                string opNo = row.OperationNo;

                string occuredwo = partNo + "," + woNo + "," + opNo;
                if (!OccuredWOs.Contains(occuredwo))
                {
                    OccuredWOs.Add(occuredwo);
                    var PartsData = obj.GettblhmiscreensDet2(MachineID, UsedDateForExcel, woNo, partNo, opNo);
                    //var PartsData = dbhmi.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && m.IsMultiWO == 0
                    //        && m.isWorkOrder == 0 && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0)
                    //        && m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == opNo).
                    //        OrderByDescending(m => m.HMIID).ToList();

                    int totalpartproduced = 0;
                    int ProcessQty = 0, DeliveredQty = 0;
                    //Decide to select deliveredQty & ProcessedQty lastest(from HMI or tblmultiWOselection)

                    #region new code

                    //here 1st get latest of delivered and processed among row in tblHMIScreen & tblmulitwoselection
                    int isHMIFirst = 2; //default NO History for that wo,pn,on

                    var mulitwoData = obj.Gettbl_multiwoselectionDet1(woNo, partNo, opNo);
                    //var mulitwoData = dbhmi.tbl_multiwoselection.Where(m => m.WorkOrder == woNo && m.PartNo == partNo && m.OperationNo == opNo).OrderByDescending(m => m.MultiWOID).Take(1).ToList();
                    //var hmiData = db.tblhmiscreens.Where(m => m.Work_Order_No == WONo && m.PartNo == Part && m.OperationNo == Operation && m.isWorkInProgress == 0).OrderByDescending(m => m.HMIID).Take(1).ToList();

                    //Note: we are in this loop => hmiscreen table data is Available

                    if (mulitwoData.Count > 0)
                    {
                        isHMIFirst = 1;
                    }
                    else if (PartsData.Count > 0)
                    {
                        isHMIFirst = 0;
                    }
                    else if (PartsData.Count > 0 && mulitwoData.Count > 0) //we both Dates now check for greatest amongst
                    {
                        int hmiIDFromMulitWO = row.HMIID;
                        DateTime multiwoDateTime = Convert.ToDateTime(obj.GetttblhmiscreenDet3(hmiIDFromMulitWO));
                        //DateTime multiwoDateTime = Convert.ToDateTime(from r in dbhmi.tblhmiscreens
                        //                                              where r.HMIID == hmiIDFromMulitWO
                        //                                              select r.Time
                        //                                              );
                        DateTime hmiDateTime = Convert.ToDateTime(row.Time);

                        if (Convert.ToInt32(multiwoDateTime.Subtract(hmiDateTime).TotalSeconds) > 0)
                        {
                            isHMIFirst = 1; // multiwoDateTime is greater than hmitable datetime
                        }
                        else
                        {
                            isHMIFirst = 0;
                        }
                    }
                    if (isHMIFirst == 1)
                    {
                        string delivString = Convert.ToString(mulitwoData[0].DeliveredQty);
                        int.TryParse(delivString, out DeliveredQty);
                        string processString = Convert.ToString(mulitwoData[0].ProcessQty);
                        int.TryParse(processString, out ProcessQty);

                    }
                    else if (isHMIFirst == 0)//Take Data from HMI
                    {
                        string delivString = Convert.ToString(PartsData[0].Delivered_Qty);
                        int.TryParse(delivString, out DeliveredQty);
                        string processString = Convert.ToString(PartsData[0].ProcessQty);
                        int.TryParse(processString, out ProcessQty);
                    }

                    #endregion

                    totalpartproduced = DeliveredQty + ProcessQty;

                    #region InnerLogic Common for both ways(HMI or tblmultiWOselection)

                    int stdCuttingTime = 0;
                    var stdcuttingTimeData = obj.Gettblmasterparts_st_swDet3(opNo, partNo);
                    //var stdcuttingTimeData = dbhmi.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0 && m.OpNo == opNo && m.PartNo == partNo).FirstOrDefault();
                    if (stdcuttingTimeData != null)
                    {
                        int stdcuttingval = Convert.ToInt32(stdcuttingTimeData.StdCuttingTime);
                        string Unit = Convert.ToString(stdcuttingTimeData.StdCuttingTimeUnit);
                        if (Unit == "Hrs")
                        {
                            stdCuttingTime = stdcuttingval * 60;
                        }
                        else //Unit is Minutes
                        {
                            stdCuttingTime = stdcuttingval;
                        }
                    }
                    #endregion

                    SummationofTime += stdCuttingTime * totalpartproduced;
                }
            }


            //To Extract Multi WorkOrder Cutting Time
            PartsDataAll = obj.GettblhmiscreensDet2(MachineID, UsedDateForExcel);
            // PartsDataAll = dbhmi.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && m.IsMultiWO == 1 && m.isWorkOrder == 0 && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0)).ToList();
            if (PartsDataAll.Count == 0)
            {
                //return SummationofTime;
            }
            foreach (var row in PartsDataAll)
            {
                string partNo = row.PartNo;
                string woNo = row.Work_Order_No;
                string opNo = row.OperationNo;

                string occuredwo = partNo + "," + woNo + "," + opNo;
                if (!OccuredWOs.Contains(occuredwo))
                {
                    OccuredWOs.Add(occuredwo);
                    var PartsData = obj.GettblhmiscreensDet2(MachineID, UsedDateForExcel, woNo, partNo, opNo);
                    //var PartsData = dbhmi.tblhmiscreens.
                    //        Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && m.IsMultiWO == 0
                    //            && m.isWorkOrder == 0 && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0)
                    //            && m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == opNo).
                    //            OrderByDescending(m => m.HMIID).ToList();


                    int totalpartproduced = 0;
                    int ProcessQty = 0, DeliveredQty = 0;
                    //Decide to select deliveredQty & ProcessedQty lastest(from HMI or tblmultiWOselection)

                    #region new code

                    //here 1st get latest of delivered and processed among row in tblHMIScreen & tblmulitwoselection
                    int isHMIFirst = 2; //default NO History for that wo,pn,on

                    var mulitwoData = obj.Gettbl_multiwoselectionDet1(woNo, partNo, opNo);
                    // var mulitwoData = dbhmi.tbl_multiwoselection.Where(m => m.WorkOrder == woNo && m.PartNo == partNo && m.OperationNo == opNo).OrderByDescending(m => m.MultiWOID).Take(1).ToList();
                    //var hmiData = db.tblhmiscreens.Where(m => m.Work_Order_No == WONo && m.PartNo == Part && m.OperationNo == Operation && m.isWorkInProgress == 0).OrderByDescending(m => m.HMIID).Take(1).ToList();

                    //Note: we are in this loop => hmiscreen table data is Available

                    if (mulitwoData.Count > 0)
                    {
                        isHMIFirst = 1;
                    }
                    else if (PartsData.Count > 0)
                    {
                        isHMIFirst = 0;
                    }
                    else if (PartsData.Count > 0 && mulitwoData.Count > 0) //we have both Dates now check for greatest amongst
                    {
                        int hmiIDFromMulitWO = row.HMIID;
                        DateTime multiwoDateTime = Convert.ToDateTime(obj.GetttblhmiscreenDet3(hmiIDFromMulitWO));
                        //DateTime multiwoDateTime = Convert.ToDateTime(from r in dbhmi.tblhmiscreens
                        //                                                  where r.HMIID == hmiIDFromMulitWO
                        //                                                  select r.Time
                        //                                                  );
                        DateTime hmiDateTime = Convert.ToDateTime(row.Time);

                        if (Convert.ToInt32(multiwoDateTime.Subtract(hmiDateTime).TotalSeconds) > 0)
                        {
                            isHMIFirst = 1; // multiwoDateTime is greater than hmitable datetime
                        }
                        else
                        {
                            isHMIFirst = 0;
                        }
                    }

                    if (isHMIFirst == 1)
                    {
                        string delivString = Convert.ToString(mulitwoData[0].DeliveredQty);
                        int.TryParse(delivString, out DeliveredQty);
                        string processString = Convert.ToString(mulitwoData[0].ProcessQty);
                        int.TryParse(processString, out ProcessQty);
                    }
                    else if (isHMIFirst == 0) //Take Data from HMI
                    {
                        string delivString = Convert.ToString(PartsData[0].Delivered_Qty);
                        int.TryParse(delivString, out DeliveredQty);
                        string processString = Convert.ToString(PartsData[0].ProcessQty);
                        int.TryParse(processString, out ProcessQty);
                    }

                    #endregion

                    totalpartproduced = DeliveredQty + ProcessQty;

                    #region InnerLogic Common for both ways(HMI or tblmultiWOselection)

                    int stdCuttingTime = 0;

                    var stdcuttingTimeData = obj.Gettblmasterparts_st_swDet3(opNo, partNo);
                    //var stdcuttingTimeData = dbhmi.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0 && m.OpNo == opNo && m.PartNo == partNo).FirstOrDefault();
                    if (stdcuttingTimeData != null)
                    {
                        int stdcuttingval = Convert.ToInt32(stdcuttingTimeData.StdCuttingTime);
                        string Unit = Convert.ToString(stdcuttingTimeData.StdCuttingTimeUnit);
                        if (Unit == "Hrs")
                        {
                            stdCuttingTime = stdcuttingval * 60;
                        }
                        else //Unit is Minutes
                        {
                            stdCuttingTime = stdcuttingval;
                        }
                    }
                    #endregion

                    SummationofTime += stdCuttingTime * totalpartproduced;
                }
            }

            return SummationofTime;
        }

        public double GetScrapQtyTimeOfWO(string UsedDateForExcel, int MachineID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn); double SQT = 0;
            var PartsData = obj.GettblhmiscreensDet2(MachineID, UsedDateForExcel);
            //var PartsData = dbhmi.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0) && m.isWorkOrder == 0).ToList();
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
                    SQT += (WODuration / (scrapQty + DeliveredQty)) * scrapQty;
                }
            }
            return SQT;
        }
        //GOD
        public double GetScrapQtyTimeOfRWO(string UsedDateForExcel, int MachineID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn); double SQT = 0;
            var PartsData = obj.GettblhmiscreensDet3(MachineID, UsedDateForExcel);
            //var PartsData = dbhmi.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0) && m.isWorkOrder == 1).ToList();
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

                SQT += WODuration;
            }
            return SQT;
        }

        // 2017-04-05 public double GetGreen(string UsedDateForExcel, DateTime TSstartTime, DateTime TSendTime, int MachineID)
        //{
        //    double settingTime = 0;
        //    DateTime WOstarttimeDate = Convert.ToDateTime(TSstartTime);
        //    DateTime WOendtimeDate = Convert.ToDateTime(TSendTime);

        //    DataTable lossesData = new DataTable();
        //    using (MsqlConnection mc = new MsqlConnection())
        //    {
        //        mc.open();
        //        String query1 = "SELECT Count(ID) From tbldailyprodstatus WHERE MachineID = '" + MachineID + "' and CorrectedDate = '" + UsedDateForExcel + "' and ColorCode = 'green'"
        //            + " and ( StartTime >= '" + WOstarttimeDate + "' and EndTime <= '" + WOendtimeDate + "' )";
        //        SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
        //        da1.Fill(lossesData);
        //        mc.close();
        //    }
        //    if (lossesData.Rows.Count > 0)
        //    {
        //        settingTime = Convert.ToDouble(lossesData.Rows[0][0]);
        //    }
        //    return settingTime;
        //}

        //Output: In Minutes
        public double GetGreen(string UsedDateForExcel, DateTime TSstartTime, DateTime TSendTime, int MachineID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            double OperatingTime = 0;
            double FirstRowDur = 0;
            double LastRowDur = 0;
            try
            {
                DataTable GreenData = new DataTable();
                using (MsqlConnection mc = new MsqlConnection())
                {
                    mc.open();
                    String query = "SELECT * From tblmode WHERE MachineID = '" + MachineID + "' and ColorCode = 'green' and IsCompleted = 1  and "
                   + "( StartTime <= '" + TSstartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( ( EndTime > '" + TSstartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( EndTime < '" + TSendTime.ToString("yyyy-MM-dd HH:mm:ss") + "' or   EndTime > '" + TSendTime.ToString("yyyy-MM-dd HH:mm:ss") + "' )  ) ) or "
                   + " ( StartTime > '" + TSstartTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ( StartTime < '" + TSendTime.ToString("yyyy-MM-dd HH:mm:ss") + "' ) ))";

                    SqlDataAdapter da1 = new SqlDataAdapter(query, mc.msqlConnection);
                    da1.Fill(GreenData);
                    mc.close();
                }

                for (int i = 0; i < GreenData.Rows.Count; i++)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(GreenData.Rows[i][0])) && !string.IsNullOrEmpty(Convert.ToString(GreenData.Rows[i][1])))
                    {
                        DateTime LStartDate = Convert.ToDateTime(GreenData.Rows[i][9]);
                        DateTime LEndDate = Convert.ToDateTime(GreenData.Rows[i][10]);
                        double IndividualDur = Convert.ToDouble(GreenData.Rows[i][13]);

                        //Get Duration Based on start & end Time.

                        if (LStartDate < TSstartTime)
                        {
                            double StartDurationExtra = TSstartTime.Subtract(LStartDate).TotalSeconds;
                            IndividualDur -= StartDurationExtra;
                        }
                        if (LEndDate > TSendTime)
                        {
                            double EndDurationExtra = LEndDate.Subtract(TSendTime).TotalSeconds;
                            IndividualDur -= EndDurationExtra;
                        }
                        OperatingTime += IndividualDur;
                    }
                }
            }
            catch (Exception e)
            { }
            return Math.Round(OperatingTime / 60, 2);

        }

        public string LossHierarchy(int LossCode)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn); string losshierarchy = LossCode + " : ";
            var lossdata = obj1.GetLossDet(LossCode);
            //var lossdata = dbloss.tbllossescodes.Where(m => m.LossCodeID == LossCode).FirstOrDefault();

            int level = lossdata.LossCodesLevel;
            if (level == 1)
            {
                if (LossCode == 999)
                {
                    losshierarchy += "NoCode Entered";
                }
                else
                {
                    losshierarchy += lossdata.LossCode;
                }
            }
            else if (level == 2)
            {
                int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                var lossdata1 = obj1.GetLossDet(lossLevel1ID);
                //var lossdata1 = dbloss.tbllossescodes.Where(m => m.LossCodeID == lossLevel1ID).FirstOrDefault();

                losshierarchy += lossdata1.LossCode + "->" + lossdata.LossCode;
            }
            else if (level == 3)
            {
                int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                int lossLevel2ID = Convert.ToInt32(lossdata.LossCodesLevel2ID);
                var lossdata1 = obj1.GetLossDet(lossLevel1ID);
                //var lossdata1 = dbloss.tbllossescodes.Where(m => m.LossCodeID == lossLevel1ID).FirstOrDefault();
                var lossdata2 = obj1.GetLossDet(lossLevel2ID);
                //var lossdata2 = dbloss.tbllossescodes.Where(m => m.LossCodeID == lossLevel2ID).FirstOrDefault();
                losshierarchy += lossdata1.LossCode + "->" + lossdata2.LossCode + "->" + lossdata.LossCode;
            }
            return losshierarchy;
        }

        #region OEE Calculation for TODAY

        //For Today OEE Calculation
        public void CalculateOEEToday(DateTime fromdate, DateTime todate, int MachineID, string TimeType, string SummarizeAs = null)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn); DateTime CurrentTime = System.DateTime.Now;
            DateTime StartTime = Convert.ToDateTime(System.DateTime.Now.ToString("yyyy-MM-dd 06:00:00"));

            if (CurrentTime.Hour >= 0 && CurrentTime.Hour < 6)
            {
                StartTime = StartTime.AddDays(-1);
                //fromdate = fromdate.AddDays(-1);
            }

            DateTime UsedDateForExcel = Convert.ToDateTime(fromdate);
            //double TotalDay = todate.Subtract(fromdate).TotalDays;
            #region

            string ipAddress = GetIPAddressOf();
            var OEEDataPresent = obj.GettbloeeListDet3(MachineID, ipAddress, UsedDateForExcel);
            // var OEEDataPresent = db.tbloeedashboardvariablestodays.Where(m => m.WCID == MachineID && m.StartDate == UsedDateForExcel && m.IPAddress == ipAddress).ToList();
            if (OEEDataPresent.Count == 0)
            {
                double green, red, yellow, blue, setup = 0, scrap = 0, NOP = 0, OperatingTime = 0, DownTimeBreakdown = 0, ROALossess = 0, AvailableTime = 0, SettingTime = 0, PlannedDownTime = 0, UnPlannedDownTime = 0;
                double SummationOfSCTvsPP = 0, MinorLosses = 0, ROPLosses = 0;
                double ScrapQtyTime = 0, ReWOTime = 0, ROQLosses = 0;

                MinorLosses = GetMinorLossesToday(UsedDateForExcel.ToString("yyyy-MM-dd"), StartTime.ToString("yyyy-MM-dd HH:mm:ss"), CurrentTime.ToString("yyyy-MM-dd HH:mm:ss"), MachineID, "yellow");
                if (MinorLosses < 0)
                {
                    MinorLosses = 0;
                }
                blue = GetOPIDleBreakDownToday(UsedDateForExcel.ToString("yyyy-MM-dd"), StartTime.ToString("yyyy-MM-dd HH:mm:ss"), CurrentTime.ToString("yyyy-MM-dd HH:mm:ss"), MachineID, "blue");
                green = GetOPIDleBreakDownToday(UsedDateForExcel.ToString("yyyy-MM-dd"), StartTime.ToString("yyyy-MM-dd HH:mm:ss"), CurrentTime.ToString("yyyy-MM-dd HH:mm:ss"), MachineID, "green");

                //Availability
                SettingTime = GetSettingTimeToday(UsedDateForExcel.ToString("yyyy-MM-dd"), StartTime.ToString("yyyy-MM-dd HH:mm:ss"), CurrentTime.ToString("yyyy-MM-dd HH:mm:ss"), MachineID);
                if (SettingTime < 0)
                {
                    SettingTime = 0;
                }
                ROALossess = GetDownTimeLossesToday(UsedDateForExcel.ToString("yyyy-MM-dd"), StartTime.ToString("yyyy-MM-dd HH:mm:ss"), CurrentTime.ToString("yyyy-MM-dd HH:mm:ss"), MachineID, "ROA");
                if (ROALossess < 0)
                {
                    ROALossess = 0;
                }
                DownTimeBreakdown = GetDownTimeBreakdownToday(UsedDateForExcel.ToString("yyyy-MM-dd"), StartTime.ToString("yyyy-MM-dd HH:mm:ss"), CurrentTime.ToString("yyyy-MM-dd HH:mm:ss"), MachineID);
                if (DownTimeBreakdown < 0)
                {
                    DownTimeBreakdown = 0;
                }

                //Performance
                SummationOfSCTvsPP = GetSummationOfSCTvsPPToday(UsedDateForExcel.ToString("yyyy-MM-dd"), StartTime.ToString("yyyy-MM-dd HH:mm:ss"), CurrentTime.ToString("yyyy-MM-dd HH:mm:ss"), MachineID);
                if (SummationOfSCTvsPP <= 0)
                {
                    SummationOfSCTvsPP = 0;
                }
                //ROPLosses = GetDownTimeLosses(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID, "ROP");

                //Quality
                ScrapQtyTime = GetScrapQtyTimeOfWOToday(UsedDateForExcel.ToString("yyyy-MM-dd"), StartTime.ToString("yyyy-MM-dd HH:mm:ss"), CurrentTime.ToString("yyyy-MM-dd HH:mm:ss"), MachineID);
                if (ScrapQtyTime < 0)
                {
                    ScrapQtyTime = 0;
                }
                ReWOTime = GetScrapQtyTimeOfRWOToday(UsedDateForExcel.ToString("yyyy-MM-dd"), StartTime.ToString("yyyy-MM-dd HH:mm:ss"), CurrentTime.ToString("yyyy-MM-dd HH:mm:ss"), MachineID);
                if (ReWOTime < 0)
                {
                    ReWOTime = 0;
                }

                //if (DateTime.Now.Hour > 6)
                //{
                //    AvailableTime = (DateTime.Now - Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 06:00:00"))).TotalMinutes;
                //}
                //else
                //{
                //    AvailableTime = (DateTime.Now - Convert.ToDateTime(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 06:00:00"))).TotalMinutes;
                //}
                OperatingTime = green;

                //To get Top 5 Losses for this WC
                string todayAsCorrectedDate = UsedDateForExcel.ToString("yyyy-MM-dd");
                var lossData = obj.GettbllossofentryDet2(MachineID, todayAsCorrectedDate);
                //var lossData = db.tbllossofentries.Where(m => m.CorrectedDate == todayAsCorrectedDate && m.MachineID == MachineID).ToList();

                DataTable DTLosses = new DataTable();
                DTLosses.Columns.Add("lossCodeID", typeof(int));
                DTLosses.Columns.Add("LossDuration", typeof(int));
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
                int PlantID = 0, ShopID = 0, CellID = 0;
                string PlantIDS = null, ShopIDS = null, CellIDS = null;
                int value;
                var WCData = obj1.GetmacDetails(MachineID);
                //var WCData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).FirstOrDefault();
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
                        SqlCommand cmdInsertRows = new SqlCommand("INSERT INTO tbloeedashboardvariablestoday(PlantID,ShopID,CellID,WCID,StartDate,EndDate,MinorLosses,Blue,Green,SettingTime,ROALossess,DownTimeBreakdown,SummationOfSCTvsPP,ScrapQtyTime,ReWOTime,Loss1Name,Loss1Value,Loss2Name,Loss2Value,Loss3Name,Loss3Value,Loss4Name,Loss4Value,Loss5Name,Loss5Value,CreatedOn,CreatedBy,IsDeleted,IPAddress)VALUES('" + PlantIDS + "','" + ShopIDS + "','" + CellIDS + "','" + MachineID + "','" + UsedDateForExcel.ToString("yyyy-MM-dd") + "','" + UsedDateForExcel.ToString("yyyy-MM-dd") + "','" + MinorLosses + "','" + blue + "','" + green + "','" + SettingTime + "','" + ROALossess + "','" + DownTimeBreakdown + "','" + SummationOfSCTvsPP + "','" + ScrapQtyTime + "','" + ReWOTime + "','" + lossCode1 + "','" + lossCodeVal1 + "','" + lossCode2 + "','" + lossCodeVal2 + "','" + lossCode3 + "','" + lossCodeVal3 + "','" + lossCode4 + "','" + lossCodeVal4 + "','" + lossCode5 + "','" + lossCodeVal5 + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + 1 + "','" + 0 + "','" + ipAddress + "' );", mcInsertRows.msqlConnection);
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
            UsedDateForExcel = UsedDateForExcel.AddDays(+1);
            #endregion

            //now push to tbloeedashboardFinalVariables.
        }

        //2017-04-05 //Get Minor Loss for Today OEE Calculation - Completed
        public int GetMinorLossesToday(string CorrectedDate, String StartTime, String EndTime, int MachineID, string Colour)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn); DateTime currentdate = Convert.ToDateTime(CorrectedDate);
            string datetime = currentdate.ToString("yyyy-MM-dd");
            int minorloss = 0;
            //int count = 0;
            DataTable GetMinorLossDT = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                //mc.open();
                //String GetMinorLoss = "SELECT ColorCode FROM tbllivedailyprodstatus WHERE MachineID = " + MachineID + " AND IsDeleted = 0 AND CorrectedDate = '" + CorrectedDate + "' AND StartTime BETWEEN '" + StartTime + "' AND '" + EndTime + "';";
                //SqlDataAdapter GetMinorLossDA = new SqlDataAdapter(GetMinorLoss, mc.msqlConnection);
                //GetMinorLossDA.Fill(GetMinorLossDT);
                //mc.close();
                mc.open();
                String GetMinorLossNew = "SELECT SUM(DurationInSec) from [i_facility_tsal].[dbo].[tbllivemodedb] where MachineID = "+ MachineID +" and IsDeleted = 0 and CorrectedDate = '" + CorrectedDate + "' and ColorCode = '" + Colour + "' and DurationInSec < 120  and IsCompleted = 1;";
                SqlDataAdapter GetMinorLossNewDA = new SqlDataAdapter(GetMinorLossNew, mc.msqlConnection);
                GetMinorLossNewDA.Fill(GetMinorLossDT);
                mc.close();
            }
            int DataCount = GetMinorLossDT.Rows.Count;
            if (DataCount > 0)
            {
                String Val = GetMinorLossDT.Rows[0][0].ToString();
                if (GetMinorLossDT.Rows[0][0].ToString() != null && Val != "")
                {
                    minorloss = Convert.ToInt32(GetMinorLossDT.Rows[0][0]) / 60;
                }
            }
            
            ////var Data = db.tbldailyprodstatus.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.CorrectedDate == CorrectedDate).OrderBy(m => m.StartTime).ToList();
            //for (int i = 0; i < DataCount; i++)
            //{
            //    //foreach (var row in Data)
            //    {
            //        if (GetMinorLossDT.Rows[i][0].ToString().ToUpper() == "YELLOW")
            //        {
            //            count++;
            //        }
            //        else
            //        {
            //            if (count > 0 && count < 2)
            //            {
            //                minorloss += count;
            //                count = 0;

            //            }
            //            count = 0;
            //        }
            //    }
            //}
            return minorloss;
        }

        //Get Idle/BD Loss for Today OEE Calculation - Completed
        public int GetOPIDleBreakDownToday(string CorrectedDate, String StartTime, String EndTime, int MachineID, string Colour)
        {
            int ModeDuration = 0;
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn); DateTime currentdate = Convert.ToDateTime(CorrectedDate);
            string datetime = currentdate.ToString("yyyy-MM-dd");

            int[] count = new int[4];
            DataTable OP = new DataTable();
            DataTable RunningOP = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                ////operating
                //mc.open();
                //String query1 = "SELECT count(ID) From tbllivedailyprodstatus WHERE CorrectedDate='" + CorrectedDate + "' AND MachineID=" + MachineID + " AND ColorCode='" + Colour + "' AND StartTime BETWEEN '" + StartTime + "' AND '" + EndTime + "';";
                //SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                //da1.Fill(OP);
                //mc.close();

                mc.open();
                String GetDurationQuery = "SELECT SUM(DurationInSec) from [i_facility_tsal].[dbo].[tbllivemodedb] where MachineID = " + MachineID + " and IsDeleted = 0 and CorrectedDate = '" + CorrectedDate + "' and ColorCode = '" + Colour + "' and IsCompleted = 1;";
                SqlDataAdapter GetDurationDA = new SqlDataAdapter(GetDurationQuery, mc.msqlConnection);
                GetDurationDA.Fill(OP);
                mc.close();

                mc.open();
                String GetRunningDurationQuery = "SELECT StartTime from [i_facility_tsal].[dbo].[tbllivemodedb] where MachineID = " + MachineID + " and IsDeleted = 0 and CorrectedDate = '" + CorrectedDate + "' and ColorCode = '" + Colour + "' and IsCompleted = 0;";
                SqlDataAdapter GetRunningDurationDA = new SqlDataAdapter(GetRunningDurationQuery, mc.msqlConnection);
                GetRunningDurationDA.Fill(RunningOP);
                mc.close();
            }
            if (OP.Rows.Count != 0)
            {
                String Val = OP.Rows[0][0].ToString();
                if (OP.Rows[0][0].ToString() != null && Val != "")
                {
                    ModeDuration = Convert.ToInt32(OP.Rows[0][0]) / 60;
                }
            }
            if (RunningOP.Rows.Count != 0)
            {
                DateTime StartTimeRunnning = Convert.ToDateTime(RunningOP.Rows[0][0]);
                int DurationRunning = (int) DateTime.Now.Subtract(StartTimeRunnning).TotalSeconds / 60;
                ModeDuration += DurationRunning;
            }
            return ModeDuration;
        }
        //Get Setting Time for Today OEE Calculation - Completed
        public double GetSettingTimeToday(string UsedDateForExcel, String StartTime, String EndTime, int MachineID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            double settingTime = 0;
            int setupid = 0;
            string settingString = "Setup";
            var setupiddata = obj.GettbloeelossDet3(settingString);
            //var setupiddata = db.tbllossescodes.Where(m => m.MessageType.Contains(settingString)).FirstOrDefault();
            if (setupiddata != null)
            {
                setupid = setupiddata.LossCodeID;
            }
            else
            {
                Session["Error"] = "Unable to get Setup's ID";
                return -1;
            }
            //getting all setup's sublevels ids.
            var SettingIDs = obj.GettbllossescodeDet1(setupid);
            //var SettingIDs = dbloss.tbllossescodes.Where(m => (m.LossCodesLevel1ID == setupid || m.LossCodesLevel2ID == setupid)).Select(m => m.LossCodeID).ToList();

            //settingTime = (from row in db.tbllossofentries
            //               where  row.CorrectedDate == UsedDateForExcel && row.MachineID == MachineID );
            foreach (var Setting in SettingIDs)
            {
                DataTable GetSettingTimeDT = new DataTable();
                using (MsqlConnection mc = new MsqlConnection())
                {
                    mc.open();
                    String GetSettingTime = "SELECT * FROM tbllivelossofentry WHERE MachineID = " + MachineID + " AND MessageCodeID = " + Setting + " AND CorrectedDate = '" + UsedDateForExcel + "' AND DoneWithRow = 1 AND StartDateTime BETWEEN '" + StartTime + "' AND '" + EndTime + "';";
                    SqlDataAdapter GetSettingTimeDA = new SqlDataAdapter(GetSettingTime, mc.msqlConnection);
                    GetSettingTimeDA.Fill(GetSettingTimeDT);
                    mc.close();
                }
                int DataCount = GetSettingTimeDT.Rows.Count;
                //var SettingData = db.tbllossofentries.Where(m => SettingIDs.Contains(m.MessageCodeID) && m.MachineID == MachineID && m.CorrectedDate == UsedDateForExcel && m.DoneWithRow == 1).ToList();
                for (int i = 0; i < DataCount; i++)
                {
                    DateTime startTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][2].ToString());
                    DateTime endTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][3].ToString());
                    settingTime += endTime.Subtract(startTime).TotalMinutes;
                }
            }
            return settingTime;
        }
        //Get Downtime Loss for Today OEE Calculation - Completed
        public double GetDownTimeLossesToday(string UsedDateForExcel, String StartTime, String EndTime, int MachineID, string contribute)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn); double LossTime = 0;
            //string contribute = "ROA";
            //getting all ROA sublevels ids. Only those of IDLE.
            var SettingIDs = obj.GettbllossescodeDet2(contribute);
            // var SettingIDs = db.tbllossescodes.Where(m => m.ContributeTo == contribute && (m.MessageType != "PM" || m.MessageType != "BREAKDOWN" || m.MessageType != "Setup")).Select(m => m.LossCodeID).ToList();
            DataTable GetSettingTimeDT = new DataTable();
            foreach (var Setting in SettingIDs)
            {
                GetSettingTimeDT.Clear();
                using (MsqlConnection mc = new MsqlConnection())
                {
                    mc.open();
                    String GetSettingTime = "SELECT * FROM tbllivelossofentry WHERE MachineID = " + MachineID + " AND MessageCodeID = " + Setting + " AND CorrectedDate = '" + UsedDateForExcel + "' AND DoneWithRow = 1 AND StartDateTime BETWEEN '" + StartTime + "' AND '" + EndTime + "';";
                    SqlDataAdapter GetSettingTimeDA = new SqlDataAdapter(GetSettingTime, mc.msqlConnection);
                    GetSettingTimeDA.Fill(GetSettingTimeDT);
                    mc.close();
                }
                int DataCount = GetSettingTimeDT.Rows.Count;
                for (int i = 0; i < DataCount; i++)
                {
                    //var SettingData = db.tbllossofentries.Where(m => SettingIDs.Contains(m.MessageCodeID) && m.MachineID == MachineID && m.CorrectedDate == UsedDateForExcel && m.DoneWithRow == 1).ToList();
                    //foreach (var row in SettingData)
                    {
                        DateTime startTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][2].ToString());
                        DateTime endTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][3].ToString());
                        LossTime += endTime.Subtract(startTime).TotalMinutes;
                    }
                }
            }
            return LossTime;
        }
        //Get BreakdownLoss for Today OEE Calculation - Completed
        public double GetDownTimeBreakdownToday(string UsedDateForExcel, String StartTime, String EndTime, int MachineID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn); double LossTime = 0;
            //var BreakdownData = db.tblbreakdowns.Where(m => m.MachineID == MachineID && m.CorrectedDate == UsedDateForExcel && m.DoneWithRow == 1).ToList();
            DataTable GetSettingTimeDT = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String GetSettingTime = "SELECT * FROM tblbreakdown WHERE MachineID = " + MachineID + " AND CorrectedDate = '" + UsedDateForExcel + "' AND DoneWithRow = 1 AND StartTime BETWEEN '" + StartTime + "' AND '" + EndTime + "';";
                SqlDataAdapter GetSettingTimeDA = new SqlDataAdapter(GetSettingTime, mc.msqlConnection);
                GetSettingTimeDA.Fill(GetSettingTimeDT);
                mc.close();
            }
            int DataCount = GetSettingTimeDT.Rows.Count;
            for (int i = 0; i < DataCount; i++)
            {
                {
                    if ((Convert.ToString(GetSettingTimeDT.Rows[i][2]) == null) || GetSettingTimeDT.Rows[i][2].ToString() == null)
                    {
                        //do nothing
                    }
                    else
                    {
                        DateTime startTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][1].ToString());
                        DateTime endTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][2].ToString());
                        LossTime += endTime.Subtract(startTime).TotalMinutes;
                    }
                }
            }
            return LossTime;
        }
        //Get SCTVsPP for Today OEE Calculation - Completed
        public double GetSummationOfSCTvsPPToday(string UsedDateForExcel, String StartTime, String EndTime, int MachineID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn); double SummationofTime = 0;
            DataTable GetSettingTimeDT = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String GetSettingTime = "SELECT * FROM tbllivehmiscreen WHERE MachineID = " + MachineID + " AND CorrectedDate = '" + UsedDateForExcel + "' AND isWorkOrder = 0 AND (isWorkInProgress = 1 OR isWorkInProgress = 0);";
                SqlDataAdapter GetSettingTimeDA = new SqlDataAdapter(GetSettingTime, mc.msqlConnection);
                GetSettingTimeDA.Fill(GetSettingTimeDT);
                mc.close();
            }
            int DataCount = GetSettingTimeDT.Rows.Count;
            //var PartsData = db.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && m.isWorkOrder == 0 && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0)).ToList();
            if (DataCount == 0)
            {
                return -1;
            }
            for (int i = 0; i < DataCount; i++)
            //foreach (var row in PartsData)
            {
                string partno = GetSettingTimeDT.Rows[i][7].ToString();
                string operationno = GetSettingTimeDT.Rows[i][8].ToString();
                int rejectedQty = 0, deliveredQty = 0;
                string deliveredQtyString = GetSettingTimeDT.Rows[i][12].ToString();
                string rejectedQtyString = GetSettingTimeDT.Rows[i][9].ToString();
                rejectedQty = rejectedQtyString != "" ? Convert.ToInt32(rejectedQtyString) : 0;
                deliveredQty = deliveredQtyString != "" ? Convert.ToInt32(deliveredQtyString) : 0;
                int totalpartproduced = deliveredQty + rejectedQty;
                double stdCuttingTime = 0;
                var stdcuttingTimeData = obj.Gettblmasterparts_st_swDet3(operationno, partno);
                //var stdcuttingTimeData = db.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0 && m.OpNo == operationno && m.PartNo == partno).FirstOrDefault();
                //foreach (var row1 in stdcuttingTimeData)
                if (stdcuttingTimeData != null)
                {
                    string stdcuttingvalString = Convert.ToString(stdcuttingTimeData.StdCuttingTime);
                    Double stdcuttingval = 0;
                    if (double.TryParse(stdcuttingvalString, out stdcuttingval))
                    {
                        stdcuttingval = stdcuttingval;
                    }
                    string Unit = Convert.ToString(stdcuttingTimeData.StdCuttingTimeUnit);
                    if (Unit == "Hrs")
                    {
                        stdCuttingTime = stdcuttingval * 60;
                    }
                    else //Unit is Minutes
                    {
                        stdCuttingTime = stdcuttingval;
                    }
                }
                SummationofTime += stdCuttingTime * totalpartproduced;
            }
            return SummationofTime;
        }
        //Get Scrap Qty Operating Time for Today OEE Calculation - Completed
        public double GetScrapQtyTimeOfWOToday(string UsedDateForExcel, String StartTime, String EndTime, int MachineID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn); double SQT = 0;
            DataTable GetSettingTimeDT = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String GetSettingTime = "SELECT * FROM tbllivehmiscreen WHERE MachineID = " + MachineID + " AND CorrectedDate = '" + UsedDateForExcel + "' AND isWorkOrder = 0 AND (isWorkInProgress = 1 OR isWorkInProgress = 0) ;";
                SqlDataAdapter GetSettingTimeDA = new SqlDataAdapter(GetSettingTime, mc.msqlConnection);
                GetSettingTimeDA.Fill(GetSettingTimeDT);
                mc.close();
            }
            int DataCount = GetSettingTimeDT.Rows.Count;

            //var PartsData = db.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0) && m.isWorkOrder == 0).ToList();
            for (int i = 0; i < DataCount; i++)
            //foreach (var row in PartsData)
            {
                string partno = GetSettingTimeDT.Rows[i][7].ToString();
                string operationno = GetSettingTimeDT.Rows[i][8].ToString();
                //int scrapQty = Convert.ToInt32(GetSettingTimeDT.Rows[i][9].ToString());
                //int DeliveredQty = Convert.ToInt32(GetSettingTimeDT.Rows[i][12].ToString());

                int scrapQty = 0;
                int DeliveredQty = 0;
                string scrapQtyString = Convert.ToString(GetSettingTimeDT.Rows[i][9]);
                string DeliveredQtyString = Convert.ToString(GetSettingTimeDT.Rows[i][12]);
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

                if (scrapQty != 0)
                {
                    DateTime startTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][4].ToString());
                    DateTime endTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][5].ToString());
                    //Double WODuration = endTimeTemp.Subtract(startTime).TotalMinutes;
                    Double WODuration = GetGreenToday(UsedDateForExcel, startTime, endTime, MachineID);

                    if ((scrapQty + DeliveredQty) == 0)
                    {
                        SQT += 0;
                    }
                    else
                    {
                        SQT += (WODuration / (scrapQty + DeliveredQty)) * scrapQty;
                    }
                }
                else
                {
                    //do nothing
                }
            }
            return SQT;
        }
        //Get ReWork Order Time for Today OEE Calculation - Completed

        /*GOD*/
        public double GetScrapQtyTimeOfRWOToday(string UsedDateForExcel, String StartTime, String EndTime, int MachineID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn); double SQT = 0;
            DataTable GetSettingTimeDT = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String GetSettingTime = "SELECT * FROM tbllivehmiscreen WHERE MachineID = " + MachineID + " AND CorrectedDate = '" + UsedDateForExcel + "' AND isWorkOrder = 1 AND (isWorkInProgress = 1 OR isWorkInProgress = 0) ;";
                SqlDataAdapter GetSettingTimeDA = new SqlDataAdapter(GetSettingTime, mc.msqlConnection);
                GetSettingTimeDA.Fill(GetSettingTimeDT);
                mc.close();
            }
            int DataCount = GetSettingTimeDT.Rows.Count;
            //var PartsData = db.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0) && m.isWorkOrder == 1).ToList();
            for (int i = 0; i < DataCount; i++)
            //foreach (var row in PartsData)
            {
                string partno = GetSettingTimeDT.Rows[i][7].ToString();
                string operationno = GetSettingTimeDT.Rows[i][8].ToString();
                //int scrapQty = Convert.ToInt32(GetSettingTimeDT.Rows[i][9].ToString());
                //int DeliveredQty = Convert.ToInt32(GetSettingTimeDT.Rows[i][12].ToString());

                int scrapQty = 0;
                int DeliveredQty = 0;
                string scrapQtyString = Convert.ToString(GetSettingTimeDT.Rows[i][9]);
                string DeliveredQtyString = Convert.ToString(GetSettingTimeDT.Rows[i][12]);
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

                DateTime startTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][4].ToString());
                DateTime endTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][5].ToString());
                Double WODuration = GetGreenToday(UsedDateForExcel, startTime, endTime, MachineID);

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

                SQT += WODuration;
            }
            return SQT;
        }
        public double GetGreenToday(string UsedDateForExcel, DateTime TSstartTime, DateTime TSendTime, int MachineID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn);
            double settingTime = 0;
            DateTime WOstarttimeDate = Convert.ToDateTime(TSstartTime);
            DateTime WOendtimeDate = Convert.ToDateTime(TSendTime);

            DataTable lossesData = new DataTable();

            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String query1 = "SELECT Count(ID) From tbllivedailyprodstatus WHERE MachineID = '" + MachineID + "' and CorrectedDate = '" + UsedDateForExcel + "' and ColorCode = 'green'"
                    + " and ( StartTime >= '" + WOstarttimeDate + "' and EndTime <= '" + WOendtimeDate + "' )";
                SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                da1.Fill(lossesData);
                mc.close();
            }
            if (lossesData.Rows.Count > 0)
            {
                settingTime = Convert.ToDouble(lossesData.Rows[0][0]);
            }
            return settingTime;
        }
        #endregion

        public JsonResult GetDonutData()
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj = new Dao1(_conn); DataTable dt = new DataTable();
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("upload", typeof(int));

            //for (int i = 0; i < 4; i++)
            //{
            //    string nameVal = "Name" + i;
            //    int uploadVal = i + 1 * 10;
            //    dt.Rows.Add(nameVal, uploadVal);
            //}

            DataTable GetDistinctLossDT = new DataTable();
            string GetDistinctLossQuery = "SELECT Blue FROM tbloeedashboardvariables where WCID = 1";
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                SqlDataAdapter GetDistinctLossDA = new SqlDataAdapter(GetDistinctLossQuery, mc.msqlConnection);
                GetDistinctLossDA.Fill(GetDistinctLossDT);
                mc.close();
            }

            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dt);

            return Json(JSONString, JsonRequestBehavior.AllowGet);
        }

        public string FetchPlant()
        {
            string res = "";
            List<PSCDetails> listPSCDetails = new List<PSCDetails>();
            var sensormachine = db.tblmachinedetails.Where(m => m.IsDeleted == 0).Select(m => m.PlantID).Distinct().ToList();
            foreach (var sensormachinedet in sensormachine)
            {
                PSCDetails objPSCDetails = new PSCDetails();
                var shopdetails = (from s in db.tblplants where s.PlantID == sensormachinedet && s.IsDeleted == 0 select new { Value = s.PlantID, Text = s.PlantName }).FirstOrDefault();
                objPSCDetails.Value = shopdetails.Value;
                objPSCDetails.Text = shopdetails.Text;
                listPSCDetails.Add(objPSCDetails);
                
            }
            string data = Convert.ToString(Session["PSMCDet"]);
            if(data!=" ")
            {
               
            }
            res = JsonConvert.SerializeObject(listPSCDetails);
            return res;
        }

        public string FetchShop(int PlantID)
        {
            string res = "";
            List<PSCDetails> listPSCDetails = new List<PSCDetails>();
            var sensormachine = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID==PlantID).Select(m => m.ShopID).Distinct().ToList();
            foreach (var sensormachinedet in sensormachine)
            {
                PSCDetails objPSCDetails = new PSCDetails();
                var shopdetails = (from s in db.tblshops where s.PlantID == PlantID && s.IsDeleted == 0 && s.ShopID == sensormachinedet select new { Value = s.ShopID, Text = s.ShopName }).FirstOrDefault();
                objPSCDetails.Value = shopdetails.Value;
                objPSCDetails.Text = shopdetails.Text;
                listPSCDetails.Add(objPSCDetails);
            }
            res = JsonConvert.SerializeObject(listPSCDetails);
            return res;
        }

        public string Fetch_Shop(int ShopID)
        {
            string res = "";
            List<PSCDetails> listPSCDetails = new List<PSCDetails>();
            var sensormachine = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).Select(m => m.ShopID).Distinct().ToList();
            foreach (var sensormachinedet in sensormachine)
            {
                PSCDetails objPSCDetails = new PSCDetails();
                var shopdetails = (from s in db.tblshops where s.ShopID == ShopID && s.IsDeleted == 0 && s.ShopID == sensormachinedet select new { Value = s.ShopID, Text = s.ShopName }).FirstOrDefault();
                objPSCDetails.Value = shopdetails.Value;
                objPSCDetails.Text = shopdetails.Text;
                listPSCDetails.Add(objPSCDetails);
            }
            res = JsonConvert.SerializeObject(listPSCDetails);
            return res;
        }


        public string Fetchcell(int ShopID)
        {
            string res = "";
            List<PSCDetails> listPSCDetails = new List<PSCDetails>();
            var sensormachine = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID==ShopID).Select(m => m.CellID).Distinct().ToList();
            foreach (var sensormachinedet in sensormachine)
            {
                PSCDetails objPSCDetails = new PSCDetails();
                var celldetails = (from s in db.tblcells where s.ShopID == ShopID && s.IsDeleted == 0 && s.CellID == sensormachinedet select new { Value = s.CellID, Text = s.CellName }).FirstOrDefault();
                objPSCDetails.Value = celldetails.Value;
                objPSCDetails.Text = celldetails.Text;
                listPSCDetails.Add(objPSCDetails);
            }
            res = JsonConvert.SerializeObject(listPSCDetails);
            return res;
        }

        public string FetchMachine(int CellID)
        {
            string res = "";
            var celldetails = (from s in db.tblmachinedetails where s.CellID == CellID && s.IsDeleted == 0 select new { Value = s.MachineID, Text = s.MachineDisplayName }).ToList();
            res = JsonConvert.SerializeObject(celldetails);
            return res;

        }

        public class PSCDetails
        {
           public int Value { get; set; }
           public string Text { get; set; }
        }

    }
}
