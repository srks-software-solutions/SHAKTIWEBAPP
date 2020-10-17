
using i_facilitylibrary;
using i_facilitylibrary.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAO
{
    public class ReportsDao
    {

        IConnectionFactory _connectionFactory;
        public ReportsDao()
        {

        }
        public ReportsDao(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public List<tblmachinedetail> GettbMachineDetails(int plantId)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT * from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmachinedetails WHERE IsDeleted = 0  and PlantID = " + plantId + " ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblmachinedetail> Getplant2List1(int MachineID)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT * From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmachinedetails WHERE IsDeleted = 0 and MachineID = " + MachineID + " ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public int Gettblhmidet(int hmiid)
        {
            int det = 0;
            tblhmiscreen ddldet = new tblhmiscreen();
            Repository<tblhmiscreen> lista = new Repository<tblhmiscreen>();
            try
            {
                string query = "SELECT * from [" + ConnectionFactory.DB + "].[dbo].tblhmiscreen WHERE HMIID =" + hmiid + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                det = ddldet.prevQty;
            }
            catch (Exception ex)
            {
                det = 0;
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public int GettblhmidetFornestingmachine(int hmiid)
        {
            int det = 0;
            tblhmiscreen ddldet = new tblhmiscreen();
            Repository<tblhmiscreen> lista = new Repository<tblhmiscreen>();
            try
            {
                string query = "SELECT * from [" + ConnectionFactory.DB + "].[dbo].tblhmiscreen WHERE HMIID =" + hmiid + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                det = (int)ddldet.SheetNoList;
            }
            catch (Exception ex)
            {
                det = 0;
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblmachinedetail> GettbMachineDetails1(int ShopId)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT * from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmachinedetails WHERE IsDeleted = 0  and ShopId = " + ShopId + " ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public tbloeedashboardvariable GettbloeedashboardvariablesDet4(int MachineID, DateTime DateTimeValue)
        {
            tbloeedashboardvariable ddldet = new tbloeedashboardvariable();
            Repository<tbloeedashboardvariable> lista = new Repository<tbloeedashboardvariable>();
            try
            {
                string query = "SELECT * from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tbloeedashboardvariables WHERE WCID =" + MachineID + " and StartDate = '" + DateTimeValue + "'";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public List<tblmachinedetail> GettbMachineDetails2(int CellId)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT * from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmachinedetails WHERE IsDeleted = 0  and CellId = " + CellId + " ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblmachinedetail> GettbMachineDetails3(int MachineID)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT * from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmachinedetails WHERE ManualWCID = " + MachineID + " and IsDeleted = 0 ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<int> GetmachineListforoee(int PlantID)
        {
            List<int> macList = new List<int>();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineID From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmachinedetails WHERE IsDeleted = 0 and IsNormalWC = 0 and PlantID =" + PlantID + "";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
                foreach (var item in det)
                {
                    macList.Add(item.MachineID);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }


            return macList;
        }
        public List<int> GetmachineListforoee1(int plantId)
        {
            List<int> macList = new List<int>();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineID From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmachinedetails WHERE IsDeleted = 0 and PlantID =" + plantId + " and IsNormalWC = 1 and ManualWCID != null";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
                foreach (var item in det)
                {
                    macList.Add(item.MachineID);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return macList;
        }
        public List<int> GetmachineShopListfor(int shopId)
        {
            List<int> macList = new List<int>();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineID From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmachinedetails WHERE IsDeleted = 0 and IsNormalWC = 0 and ShopID =" + shopId + "";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
                foreach (var item in det)
                {
                    macList.Add(item.MachineID);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return macList;
        }
        public List<int> GetmachineShopListfor1(int shopId)
        {
            List<int> macList = new List<int>();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineID From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmachinedetails WHERE IsDeleted = 0 and ShopID =" + shopId + " and IsNormalWC = 1 and ManualWCID != null";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
                foreach (var item in det)
                {
                    macList.Add(item.MachineID);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return macList;
        }
        public List<int> GetmachineCellListfor(int cellId)
        {
            List<int> macList = new List<int>();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineID From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmachinedetails WHERE IsDeleted = 0 and IsNormalWC = 0 and CellID =" + cellId + "";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
                foreach (var item in det)
                {
                    macList.Add(item.MachineID);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return macList;
        }
        public List<int> GetmachineCellListfor1(int cellId)
        {
            List<int> macList = new List<int>();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineID From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmachinedetails WHERE IsDeleted = 0 and CellID =" + cellId + " and IsNormalWC = 1 and ManualWCID != null";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
                foreach (var item in det)
                {
                    macList.Add(item.MachineID);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return macList;
        }
        public List<int> GetmachineListfor1(int wcId)
        {
            List<int> macList = new List<int>();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineID From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmachinedetails WHERE IsDeleted = 0 and IsNormalWC = 1 and ManualWCID = " + wcId + "";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
                foreach (var item in det)
                {
                    macList.Add(item.MachineID);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return macList;
        }

        public List<tbl_multiwoselection> Gettbl_multiwoselectionfor1(int hmiid)
        {
            Repository<tbl_multiwoselection> lista = new Repository<tbl_multiwoselection>();
            List<tbl_multiwoselection> det = new List<tbl_multiwoselection>();
            try
            {
                string query = "SELECT * From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tbl_multiwoselection WHERE HMIID =" + hmiid + " ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblmasterparts_st_sw> Gettblmasterparts_st_swDet4(string opno, string partno)
        {
            Repository<tblmasterparts_st_sw> lista = new Repository<tblmasterparts_st_sw>();
            List<tblmasterparts_st_sw> det = new List<tblmasterparts_st_sw>();
            try
            {
                string query = "SELECT * From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmasterparts_st_sw WHERE IsDeleted = 0 and OpNo ='" + opno + "' and PartNo ='" + partno + "' ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tbldaytiming> GetttbldaytimingsDet4()
        {
            Repository<tbldaytiming> lista = new Repository<tbldaytiming>();
            List<tbldaytiming> det = new List<tbldaytiming>();
            try
            {
                string query = "SELECT * From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tbldaytiming WHERE IsDeleted = 0 ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public int GetmultiselectionDet3(int HmiID, string PartNoS, string WorkOrderNoS, string OpNoS)
        {
            int det = 0;
            tbl_multiwoselection ddldet = new tbl_multiwoselection();
            Repository<tbl_multiwoselection> lista = new Repository<tbl_multiwoselection>();
            try
            {
                string query = "SELECT TargetQty from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tbl_multiwoselection WHERE HMIID =" + HmiID + " and PartNo ='" + PartNoS + "' and WorkOrder ='" + WorkOrderNoS + "' and OperationNo ='" + OpNoS + "'";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                det = (int)ddldet.TargetQty;
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public int GetmultiselectionDet4(int HmiID, string PartNoS, string WorkOrderNoS, string OpNoS)
        {
            int det = 0;
            tbl_multiwoselection ddldet = new tbl_multiwoselection();
            Repository<tbl_multiwoselection> lista = new Repository<tbl_multiwoselection>();
            try
            {
                string query = "SELECT DeliveredQty from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tbl_multiwoselection WHERE HMIID =" + HmiID + " and PartNo ='" + PartNoS + "' and WorkOrder ='" + WorkOrderNoS + "' and OperationNo ='" + OpNoS + "'";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                det = (int)ddldet.DeliveredQty;
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public tblmasterparts_st_sw Gettblmasterparts_st_swDet3(string opNo, string partNo)
        {
            tblmasterparts_st_sw ddldet = new tblmasterparts_st_sw();
            Repository<tblmasterparts_st_sw> lista = new Repository<tblmasterparts_st_sw>();
            try
            {
                string query = "SELECT * from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmasterparts_st_sw WHERE IsDeleted = 0 and OpNo ='" + opNo + "' and PartNo ='" + partNo + "'";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public decimal Gettblmasterparts_st_swDet1(string opNo, string partNo)
        {
            decimal det = 0;
            tblmasterparts_st_sw ddldet = new tblmasterparts_st_sw();
            Repository<tblmasterparts_st_sw> lista = new Repository<tblmasterparts_st_sw>();
            try
            {
                string query = "SELECT StdCuttingTime from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmasterparts_st_sw WHERE IsDeleted = 0 and OpNo ='" + opNo + "' and PartNo ='" + partNo + "'";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                det = (decimal)ddldet.StdCuttingTime;
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public decimal Gettblmasterparts_st_swDet2(string OperationNoString, string PartNoString)
        {
            decimal det = 0;
            tblmasterparts_st_sw ddldet = new tblmasterparts_st_sw();
            Repository<tblmasterparts_st_sw> lista = new Repository<tblmasterparts_st_sw>();
            try
            {
                string query = "SELECT StdCuttingTime from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmasterparts_st_sw WHERE OpNo ='" + OperationNoString + "' and PartNo ='" + PartNoString + "'";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                det = (decimal)ddldet.StdCuttingTime;
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public string Gettblmasterparts_st_swDet(string OperationNoString, string PartNoString)
        {
            string det = "";
            tblmasterparts_st_sw ddldet = new tblmasterparts_st_sw();
            Repository<tblmasterparts_st_sw> lista = new Repository<tblmasterparts_st_sw>();
            try
            {
                string query = "SELECT StdCuttingTimeUnit from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmasterparts_st_sw WHERE OpNo ='" + OperationNoString + "' and PartNo ='" + PartNoString + "'";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                det = ddldet.StdCuttingTimeUnit;
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }

        public double GettbloeedashboardvariablesDet3(int MachineID, DateTime StartDateFormatted)
        {
            IntoFile("GettbloeedashboardvariablesDet3" + MachineID);
            IntoFile("GettbloeedashboardvariablesDet3" + StartDateFormatted);
            double det = 0.0;
            tbloeedashboardvariable ddldet = new tbloeedashboardvariable();
            Repository<tbloeedashboardvariable> lista = new Repository<tbloeedashboardvariable>();
            try
            {
                string query = "SELECT MinorLosses from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tbloeedashboardvariables WHERE WCID =" + MachineID + " and StartDate = '" + StartDateFormatted + "'";
                IntoFile("GettbloeedashboardvariablesDet3" + query);
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                det = (double)ddldet.MinorLosses;
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public decimal GettblworeportsDet3(int MachineID, string correctedDate, string ProdFAI)
        {
            decimal det = 0;
            tblworeport ddldet = new tblworeport();
            Repository<tblworeport> lista = new Repository<tblworeport>();
            try
            {
                string query = "SELECT Sum(MinorLoss) from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblworeport WHERE MachineID =" + MachineID + " and CorrectedDate ='" + correctedDate + "' and Type = '" + ProdFAI + "'";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                det = (decimal)ddldet.MinorLoss;
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblworeport> Gettblworeportfor1(int hmiid)
        {
            Repository<tblworeport> lista = new Repository<tblworeport>();
            List<tblworeport> det = new List<tblworeport>();
            try
            {
                string query = "SELECT * From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblworeport WHERE HMIID =" + hmiid + " ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblhmiscreen> GettblhmiscreensDet(int? MachineID, string UsedDateForExcel)
        {
            Repository<tblhmiscreen> lista = new Repository<tblhmiscreen>();
            List<tblhmiscreen> det = new List<tblhmiscreen>();
            try
            {
                string query = "SELECT * from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblhmiscreen WHERE CorrectedDate ='" + UsedDateForExcel + "' and MachineID = " + MachineID + " and isWorkOrder = 0 and (isWorkInProgress = 1 || isWorkInProgress = 0) order by HMIID desc";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblhmiscreen> GettblhmiscreensDet1(DateTime startDateTimeInFormat, DateTime endDateTimeInFormat, string Prefix)
        {
            Repository<tblhmiscreen> lista = new Repository<tblhmiscreen>();
            List<tblhmiscreen> det = new List<tblhmiscreen>();
            try
            {
                string query = "SELECT * from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblhmiscreen WHERE PEStartTime > '" + startDateTimeInFormat + "' and Time < '" + endDateTimeInFormat + "' and PartNo like '" + Prefix + "%' and IsMultiWO = 0 order by PartNo desc";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblhmiscreen> GettblhmiscreensDet2(DateTime startDateTimeInFormat, DateTime endDateTimeInFormat, string Prefix)
        {
            Repository<tblhmiscreen> lista = new Repository<tblhmiscreen>();
            List<tblhmiscreen> det = new List<tblhmiscreen>();
            try
            {
                string query = "SELECT * from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblhmiscreen WHERE PEStartTime > '" + startDateTimeInFormat + "' and Time < '" + endDateTimeInFormat + "' and PartNo like '" + Prefix + "%' and IsMultiWO = 1 order by PartNo desc";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tbldailyprodstatu> Gettbldailyprodstatusfor1(int MachineID, string CorrectedDate)
        {
            Repository<tbldailyprodstatu> lista = new Repository<tbldailyprodstatu>();
            List<tbldailyprodstatu> det = new List<tbldailyprodstatu>();
            try
            {
                string query = "SELECT * From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tbldailyprodstatu WHERE IsDeleted = 0 MachineID =" + MachineID + " and CorrectedDate = '" + CorrectedDate + "' order by StartTime asc";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblmode> Gettblmodefor1(int MachineID, string CorrectedDate)
        {
            Repository<tblmode> lista = new Repository<tblmode>();
            List<tblmode> det = new List<tblmode>();
            try
            {
                string query = "SELECT * From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmode WHERE IsDeleted =0 and MachineID =" + MachineID + " and CorrectedDate ='" + CorrectedDate + "' OrderBy InsertedOn asc ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public tbllossescode GettbloeelossDet3(string settingString)
        {
            tbllossescode ddldet = new tbllossescode();
            Repository<tbllossescode> lista = new Repository<tbllossescode>();
            try
            {
                string query = "SELECT * from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tbllossescode WHERE IsDeleted =0 and  MessageType like '%" + settingString + "%'";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public List<tblhmiscreen> Gettblhmiscreensfor1(string today, int machineid)
        {
            Repository<tblhmiscreen> lista = new Repository<tblhmiscreen>();
            List<tblhmiscreen> det = new List<tblhmiscreen>();
            try
            {
                string query = "SELECT * From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblhmiscreen WHERE CorrectedDate ='" + today + "' and MachineID =" + machineid + " and isWorkInProgress = 1";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblbreakdown> Gettbltblbreakdownfor1(int MachineID, string UsedDateForExcel)
        {
            Repository<tblbreakdown> lista = new Repository<tblbreakdown>();
            List<tblbreakdown> det = new List<tblbreakdown>();
            try
            {
                string query = "SELECT * From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblhmiscreen WHERE MachineID =" + MachineID + " and CorrectedDate ='" + UsedDateForExcel + "' and DoneWithRow = 1";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblmachinedetail> Gettblmachinedetfor1(string countryId)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT * From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmachinedetails WHERE IsDeleted = 0 and ShopNo ='" + countryId + "' ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<int> Gettblmachinedet()
        {
            List<int> macList = new List<int>();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineID From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblmachinedetails WHERE IsDeleted = 0";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
                foreach (var item in det)
                {
                    macList.Add(item.MachineID);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return macList;
        }
        public List<tbluser> Gettbluserfor1()
        {
            Repository<tbluser> lista = new Repository<tbluser>();
            List<tbluser> det = new List<tbluser>();
            try
            {
                string query = "SELECT * From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblusers WHERE IsDeleted =0 and PrimaryRole = 3 ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblworeport> Gettblworeportfor2(int MachineID, string correctedDate)
        {
            Repository<tblworeport> lista = new Repository<tblworeport>();
            List<tblworeport> det = new List<tblworeport>();
            try
            {
                string query = "SELECT * From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblworeport WHERE MachineID = " + MachineID + " and CorrectedDate ='" + correctedDate + "'";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblworeport> Gettblworeportfor2(string PartNoString, string WorkOrderNo, string OperationNoString)
        {
            Repository<tblworeport> lista = new Repository<tblworeport>();
            List<tblworeport> det = new List<tblworeport>();
            try
            {
                string query = "SELECT * From [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblworeport WHERE PartNo ='" + PartNoString + "' and WorkOrderNo ='" + WorkOrderNo + "' and OpNo ='" + OperationNoString + "' ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblwolossess> Gettblwolossessfor1(int hmiid)
        {
            Repository<tblwolossess> lista = new Repository<tblwolossess>();
            List<tblwolossess> det = new List<tblwolossess>();
            try
            {
                string query = "SELECT * From [" + ConnectionFactory.DB + "].[dbo].tblwolossess WHERE HMIID =" + hmiid + " ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public int GettblmachineDet3(int MachineID)
        {
            int det = 0;
            tblmachinedetail ddldet = new tblmachinedetail();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            try
            {
                string query = "SELECT IsNormalWC from [" + ConnectionFactory.DB + "].[dbo].tblmachinedetails WHERE MachineID =" + MachineID + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                det = (int)ddldet.IsNormalWC;
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public tbluser GetuserDet3(int OperatorID)
        {
            tbluser ddldet = new tbluser();
            Repository<tbluser> lista = new Repository<tbluser>();
            try
            {
                string query = "SELECT * from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblusers WHERE UserID =" + OperatorID + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public tblpartwiseworkcenter GettblpartwiseworkcenterDet3(int MachineIDRow)
        {
            tblpartwiseworkcenter ddldet = new tblpartwiseworkcenter();
            Repository<tblpartwiseworkcenter> lista = new Repository<tblpartwiseworkcenter>();
            try
            {
                string query = "SELECT * from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblpartwiseworkcenter WHERE WorkCenterId =" + MachineIDRow + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public double GettblpartwisespDet3(string PartNumber)
        {
            double det = 0;
            tblpartwisesp ddldet = new tblpartwisesp();
            Repository<tblpartwisesp> lista = new Repository<tblpartwisesp>();
            try
            {
                string query = "SELECT SurfaceArea from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblpartwisesp WHERE PartName ='" + PartNumber + "'";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                det = ddldet.SurfaceArea;
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public double GettblpartwisespDet1(string PartNumber)
        {
            double det = 0;
            tblpartwisesp ddldet = new tblpartwisesp();
            Repository<tblpartwisesp> lista = new Repository<tblpartwisesp>();
            try
            {
                string query = "SELECT Perimeter from [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].tblpartwisesp WHERE PartName ='" + PartNumber + "'";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                det = ddldet.Perimeter;
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }

        public void IntoFile(string Msg)
        {
            try
            {
                string appPath = @"C://Tsal_log.txt";
                using (StreamWriter writer = new StreamWriter(appPath, true)) //true => Append Text
                {
                    writer.WriteLine(System.DateTime.Now + ":  " + Msg + "\r \n");
                }
            }
            catch (Exception e7)
            {
                IntoFile("IntoFile" + e7.ToString());
            }

        }
        //Anjali 

        public tblholdcode GetHoldCodeDetails(int HoldCodeID)
        {
            tblholdcode user = new tblholdcode();
            try
            {
                Repository<tblholdcode> lista = new Repository<tblholdcode>();

                string qry = "SELECT * FROM [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].[tblholdcode] WHERE HoldCodeID =" + HoldCodeID + "";
                user = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return user;
        }

        public tbllossescode GetLossCodeDetails(int LossCodeID)
        {
            tbllossescode user = new tbllossescode();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].[tbllossescode] WHERE LossCodeID =" + LossCodeID + "";
                user = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return user;
        }

        public tbllossescode GetLossCodeDetails1(string MessageType)
        {
            tbllossescode user = new tbllossescode();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].[tbllossescode] WHERE IsDeleted = 0 and LossCodesLevel1ID = 1 and MessageType ='" + MessageType + "'";
                user = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return user;
        }

        public List<int> GetLossCodeDetails2(int ChangeOverid)
        {
            List<int> macList = new List<int>();
            List<tbllossescode> user = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT DISTINCT LossCodeID FROM [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].[tbllossescode] WHERE LossCodesLevel1ID = " + ChangeOverid + "";
                user = lista.GetList(qry, _connectionFactory.GetConnection);
                foreach (var item in user)
                {
                    macList.Add(item.LossCodeID);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return macList;
        }

        public tbllossescode GetLossCodeDetails3(string MessageType)
        {
            tbllossescode user = new tbllossescode();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].[tbllossescode] WHERE IsDeleted = 0 and MessageType ='" + MessageType + "'";
                user = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return user;
        }

        public List<int> GetLossofEntryDetails(int MachineID, string CorrectedDate)
        {
            List<int> macList = new List<int>();
            List<tbllossofentry> user = new List<tbllossofentry>();
            try
            {
                Repository<tbllossofentry> lista = new Repository<tbllossofentry>();

                string qry = "SELECT DISTINCT MessageCodeID FROM [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].[tbllossofentry] WHERE MachineID = " + MachineID + " and CorrectedDate ='" + CorrectedDate + "' and DoneWithRow = 1";
                IntoFile("GetAllLossesDurationSeconds" + qry);
                user = lista.GetList(qry, _connectionFactory.GetConnection);
                foreach (var item in user)
                {
                    macList.Add(item.MessageCodeID);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return macList;
        }

        public List<int> GetBreakDownDetails(int MachineID, string CorrectedDate)
        {
            List<int> macList = new List<int>();
            List<tblbreakdown> user = new List<tblbreakdown>();
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();

                string qry = "SELECT DISTINCT BreakDownCode FROM [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].[tblbreakdown] WHERE MachineID = " + MachineID + " and CorrectedDate ='" + CorrectedDate + "' and DoneWithRow = 1";
                user = lista.GetList(qry, _connectionFactory.GetConnection);
                foreach (var item in user)
                {
                    macList.Add((int)item.BreakDownCode);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return macList;
        }


    }
}
