using Dapper;
using i_facilitylibrary.DAL;
using i_facilitylibrary.DAO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace i_facilitylibrary.DAO
{
    public class Dao : ConnectionFactory
    {
        IConnectionFactory _connectionFactory;
        string databaseName = "";
        public Dao()
        {

        }
        public Dao(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            databaseName = ConfigurationManager.AppSettings["databasename"];
        }
        public List<tblmachinedetail> GetMachineDetails2()
        {
            List<tblmachinedetail> machinelist = new List<tblmachinedetail>();
            try
            {
                Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();

                string qry = "SELECT * FROM " + databaseName + ".[tblmachinedetails] WHERE IsDeleted = 0 ";
                machinelist = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return machinelist;
        }

        public tbluser GetUserDetails(int MachineID)
        {
            tbluser user = new tbluser();
            try
            {
                Repository<tbluser> lista = new Repository<tbluser>();

                string qry = "SELECT * FROM " + databaseName + ".[tblusers] WHERE MachineID =" + MachineID + " and IsDeleted = 0";
                user = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return user;
        }

        public tbldaytiming GetDaytimingDetails()
        {
            tbldaytiming dayTime = new tbldaytiming();
            try
            {
                Repository<tbldaytiming> lista = new Repository<tbldaytiming>();

                string qry = "SELECT * FROM " + databaseName + ".[tbldaytiming] WHERE IsDeleted = 0";
                dayTime = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return dayTime;
        }

        public tbllivehmiscreen GetLiveHMIDetails3(string CorrectedDate, int opid, int MachineID)
        {
            tbllivehmiscreen LiveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * from " + databaseName + ".[tbllivehmiscreen] WHERE CorrectedDate ='" + CorrectedDate + "' and OperatiorID = " + opid + " and MachineID=" + MachineID;
                LiveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return LiveHMI;
        }

        public int InsertLiveHMIDetails(int MachineID, string CorrectedDate, string shift, int opid)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "INSERT INTO " + databaseName + ".[tbllivehmiscreen](MachineID,CorrectedDate,Shift,Status,OperatiorID,isWorkInProgress) VALUES(" + MachineID + ",'" + CorrectedDate + "','" + shift + "'," + 0 + "," + opid + "," + 2 + ")";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public tblshift_mstr GetShiftDetails(TimeSpan Tm)
        {
            tblshift_mstr shift = new tblshift_mstr();
            try
            {
                Repository<tblshift_mstr> lista = new Repository<tblshift_mstr>();

                string qry = "SELECT * FROM " + databaseName + ".[tblshift_mstr] WHERE StartTime <='" + Tm + "' and EndTime>='" + Tm + "'";
                shift = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return shift;
        }

        public List<tbllivehmiscreen> GetLiveHMIScreenDetails(string CorrectedDate, int MachineID, int opid)
        {
            List<tbllivehmiscreen> liveHMI = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT TOP 1 * from " + databaseName + ".[tbllivehmiscreen] WHERE CorrectedDate ='" + CorrectedDate + "' and MachineID=" + MachineID + " and OperatiorID =" + opid + " order by HMIID desc";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public List<tbllivehmiscreen> GetLiveHMIScreenDetails1( int MachineID, int opid)
        {
            List<tbllivehmiscreen> liveHMI = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT  * from " + databaseName + ".[tbllivehmiscreen] WHERE MachineID=" + MachineID + " and OperatiorID =" + opid + " and Date is not null and Time is null order by HMIID desc";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }
        public List<tblshift_mstr> GetShiftDetails1(TimeSpan Tm)
        {
            List<tblshift_mstr> liveHMI = new List<tblshift_mstr>();
            try
            {
                Repository<tblshift_mstr> lista = new Repository<tblshift_mstr>();

                string qry = "SELECT * FROM " + databaseName + ".[tblshift_mstr] WHERE StartTime <='" + Tm + "' and EndTime>='" + Tm + "'";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public List<tblmachineallocation> GetMachineAllocationDetails(string CorrectedDate, int opid, int ShiftID)
        {
            List<tblmachineallocation> liveHMI = new List<tblmachineallocation>();
            try
            {
                Repository<tblmachineallocation> lista = new Repository<tblmachineallocation>();

                string qry = "SELECT * FROM " + databaseName + ".[tblmachineallocation] WHERE IsDeleted = 0 and CorrectedDate = '" + CorrectedDate + "' and UserID =" + opid + " and ShiftID =" + ShiftID;
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public tblmachinedetail GetMachineDetails(int MachineID)
        {
            tblmachinedetail machine = new tblmachinedetail();
            try
            {
                Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();

                string qry = "SELECT * FROM " + databaseName + ".[tblmachinedetails] WHERE MachineID =" + MachineID;
                machine = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return machine;
        }

        public tbllivehmiscreen GetLiveHMIDetails(int MachineID)
        {
            tbllivehmiscreen LiveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * from " + databaseName + ".[tbllivehmiscreen] WHERE MachineID=" + MachineID + " order by desc HMIID";
                LiveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return LiveHMI;
        }

        public List<tbllivelossofentry> GetLossOfEntryDetails(int MachineID, string CorrectedDate, int MessageCodeID)
        {
            List<tbllivelossofentry> lossofEntry = new List<tbllivelossofentry>();
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivelossofentry] WHERE MachineID =" + MachineID + " and CorrectedDate='" + CorrectedDate + "' and EndDateTime IS NULL and MessageCodeID =" + MessageCodeID + "";
                lossofEntry = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossofEntry;
        }
        public List<tbllivelossofentry> GetLossOfEntryDetails12(int MachineID)
        {
            List<tbllivelossofentry> lossofEntry = new List<tbllivelossofentry>();
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

                string qry = "SELECT TOP 1 * FROM " + databaseName + ".[tbllivelossofentry] WHERE MachineID = " + MachineID + " and IsScreen = 1 or IsStart = 1 Order By StartDateTime desc";
                lossofEntry = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossofEntry;
        }

        public List<tbllivehmiscreen> GetLiveHMIDetails5(int MachineID, int opid, int id, string CorrectedDate)
        {
            List<tbllivehmiscreen> liveHMI = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT TOP 1 * FROM " + databaseName + ".[tbllivehmiscreen] WHERE MachineID =" + MachineID + " and OperatiorID=" + opid + " and HMIID =" + id + " and Status != 2 and CorrectedDate ='" + CorrectedDate + "'";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public List<tbllivehmiscreen> GetLHMIDetails(int MachineID, int opid, int id, string CorrectedDate)
        {
            List<tbllivehmiscreen> liveHMI = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE MachineID =" + MachineID + " and OperatiorID = " + opid + " and HMIID =" + id + " and CorrectedDate ='" + CorrectedDate + "'";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public tbllivehmiscreen GetLivHMIDetails(int MachineID, int opid, string CorrectedDate)
        {
            tbllivehmiscreen liveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT TOP 1 * FROM " + databaseName + ".[tbllivehmiscreen] WHERE MachineID =" + MachineID + " and OperatiorID = " + opid + " and Status = 0 and CorrectedDate ='" + CorrectedDate + "' order by HMIID desc";
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public List<tbllivehmiscreen> GetLiveHMISDetails(int MachineID, int opid, string CorrectedDate, string Shift)
        {
            List<tbllivehmiscreen> liveHMI = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE MachineID =" + MachineID + " and OperatiorID=" + opid + " and Status = 0 and CorrectedDate ='" + CorrectedDate + "' and Shift ='" + Shift + "' order by HMIID desc";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                //MessageBox.Show(ex.ToString());
            }
            return liveHMI;
        }

        public List<tbllivehmiscreen> GetLiveHMIDetails4(int MachineID)
        {
            List<tbllivehmiscreen> liveHMI = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE MachineID =" + MachineID + " and Status = 0 order by HMIID desc";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                //MessageBox.Show(ex.ToString());
            }
            return liveHMI;
        }

        public List<tbllivemultiwoselection> GetMWOtDetails(int HMIID)
        {
            List<tbllivemultiwoselection> MWork = new List<tbllivemultiwoselection>();
            try
            {
                Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivemultiwoselection] WHERE HMIID =" + HMIID;
                MWork = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return MWork;
        }

        public object GetLiveHMIScreenDetails(object correctedDate, int machineID, int opid)
        {
            throw new NotImplementedException();
        }

        public tbllivehmiscreen GetLiveHMIDetails2(string WONo, string Part, string Operation)
        {
            tbllivehmiscreen liveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE Work_Order_No ='" + WONo + "' and PartNo='" + Part + "' and OperationNo =" + Operation + " and isWorkInProgress = 1";
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public List<tblddl> GetddlDetails(string WONo, string Part, string Operation)
        {
            List<tblddl> ddl = new List<tblddl>();
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();

                string qry = "SELECT * FROM " + databaseName + ".[tblddl] WHERE IsCompleted = 0 and WorkOrder='" + WONo + "' and MaterialDesc ='" + Part + "' and OperationNo != " + Operation + " order by WorkOrder,MaterialDesc,OperationNo";
                ddl = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ddl;
        }
        public List<tblddl> GetddlDetails1(string Prefix, List<string> existingWO)
        {
            List<tblddl> ddl = new List<tblddl>();
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();

                string qry = "SELECT DISTINCT MaterialDesc FROM " + databaseName + ".[tblddl] WHERE WorkOrder Like '" + Prefix + "%' and MaterialDesc not Like '%" + existingWO + "%' and IsDeleted = 0";
                ddl = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ddl;
        }
        public List<tblddl> GetddlDetails2(string Prefix, List<string> existingWO)
        {
            List<tblddl> ddl = new List<tblddl>();
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();

                string qry = "SELECT DISTINCT WorkOrder FROM " + databaseName + ".[tblddl] WHERE WorkOrder Like '" + Prefix + "%' and WorkOrder not Like '%" + existingWO + "%' and IsDeleted = 0";
                ddl = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ddl;
        }
        public List<tblddl> GetddlDetails3(string Prefix, List<string> existingWO)
        {
            List<tblddl> ddl = new List<tblddl>();
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();

                string qry = "SELECT DISTINCT OperationNo FROM " + databaseName + ".[tblddl] WHERE WorkOrder Like '" + Prefix + "%' and OperationNo not Like '%" + existingWO + "%' and IsDeleted =0";
                ddl = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ddl;
        }

        public tbllivehmiscreen GetLive1HMIDetails(string WONo, string Part, string localOPNoString)
        {
            tbllivehmiscreen liveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE Work_Order_No ='" + WONo + "' and PartNo='" + Part + "' and OperationNo =" + localOPNoString;
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }
        public tblddl GetLive1HMIDetails1(string WONo, string Part, int localOPNoString)
        {
            tblddl liveHMI = new tblddl();
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();

                string qry = "SELECT * FROM " + databaseName + ".[tblddl] WHERE WorkOrder ='" + WONo + "' and MaterialDesc='" + Part + "' and OperationNo <" + localOPNoString + " and IsCompleted=0 order by Convert(int,OperationNo) asc";
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public tbllivehmiscreen GetLiveHMIDetails6(int hmiid)
        {
            tbllivehmiscreen liveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE HMIID =" + hmiid;
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public tblddl GetddlDetails1(int DDLID)
        {
            tblddl ddl = new tblddl();
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();

                string qry = "SELECT * FROM " + databaseName + ".[tblddl] WHERE IsCompleted = 0 and DDLID=" + DDLID;
                ddl = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ddl;
        }

        public tbllivehmiscreen GetLiveHMIDetails7(int hmiid)
        {

            tbllivehmiscreen liveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE HMIID =" + hmiid;
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public tblddl GetddlDetails2(string woNo, string partNo, string opNo)
        {
            tblddl ddl = new tblddl();
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();

                string qry = "SELECT * FROM " + databaseName + ".[tblddl] WHERE IsCompleted = 0 and WorkOrder=" + woNo + " and MaterialDesc=" + partNo + " and OperationNo =" + opNo;
                ddl = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ddl;
        }

        public int DeleteHMIScreenDetails(int hmiid)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "DELETE FROM " + databaseName + ".[tbllivehmiscreen] WHERE HMIID = " + hmiid;
                res = Convert.ToInt32(lista.delete(qry, _connectionFactory.GetConnection));
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public tbllivehmiscreen GetLiveHMIDetails13(string WONo, string Part, string opNo, int HMIID)
        {
            tbllivehmiscreen liveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE Work_Order_No ='" + WONo + "' and PartNo='" + Part + "' and OperationNo =" + opNo + " and HMIID != " + HMIID + " and isWorkInProgress != 2 order by Time desc";
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public List<tbllivemultiwoselection> GetMultiWODetails(string WONo, string Part, string opNo)
        {
            List<tbllivemultiwoselection> liveHMI = new List<tbllivemultiwoselection>();
            try
            {
                Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();

                string qry = "SELECT TOP 1 * FROM " + databaseName + ".[tbllivemultiwoselection] AS WOSelection JOIN " + databaseName + ".[tbllivehmiscreen] AS LiveHMI ON WOSelection.HMIID = LiveHMI.HMIID  WHERE WOSelection.WorkOrder ='" + WONo + "' and WOSelection.PartNo ='" + Part + "' and WOSelection.OperationNo =" + opNo + " and LiveHMI.isWorkInProgress != 2 order by LiveHMI.Time desc";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public tbllivehmiscreen GetLiveHMIDetails1(int WONo, int Part, string Operation)
        {
            tbllivehmiscreen liveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE Work_Order_No =" + WONo + " and PartNo=" + Part + " and OperationNo ='" + Operation + "' and IsCompleted = 1";
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public List<tbllivemultiwoselection> GetMultiWODetails1(int HMIID)
        {
            List<tbllivemultiwoselection> mwo = new List<tbllivemultiwoselection>();
            try
            {
                Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();

                string qry = "SELECT TOP 1 * FROM " + databaseName + ".[tbllivemultiwoselection] WHERE HMIID = " + HMIID + " and IsSubmit = 0";
                mwo = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return mwo;
        }

        public tbllivelossofentry GetLossOfEntryDetails1(int MachineID)
        {
            tbllivelossofentry lossofEntry = new tbllivelossofentry();
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivelossofentry] WHERE MachineID =" + MachineID + " order by StartDateTime desc";
                lossofEntry = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossofEntry;
        }

        public tbllivelossofentry GetLossOfEntryDetails2(int MachineID)
        {
            tbllivelossofentry lossofEntry = new tbllivelossofentry();
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivelossofentry] WHERE MachineID =" + MachineID + " and IsUpdate = 1 and DoneWithRow = 1 order by StartDateTime desc";
                lossofEntry = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
                IntoFile("GetIdleStartTime get starttime:" + qry);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossofEntry;
        }

        public DateTime GetLiveModeDetails(string correcteddate, int machineid)
        {
            DateTime time = DateTime.Now;
            tbllivemode lossofEntry = new tbllivemode();
            try
            {
                Repository<tbllivemode> lista = new Repository<tbllivemode>();

                string qry = "SELECT StartTime FROM " + databaseName + ".[tbllivemode] WHERE CorrectedDate ='" + correcteddate + "' and MachineID =" + machineid + " order by StartTime desc";
                lossofEntry = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
                time = (DateTime)lossofEntry.StartTime;
                IntoFile("GetIdleStartTime status !=4 query:" + qry);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return time;
        }

        public tbllivemode Getlivemodestatus(int MachineID, string correcteddate)
        {
            tbllivemode lossofEntry = new tbllivemode();
            try
            {
                Repository<tbllivemode> lista = new Repository<tbllivemode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivemode] WHERE MachineID =" + MachineID + " and iscompleted = 0 and MacMode = 'POWEROFF' order by Modeid desc";
                lossofEntry = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
                //IntoFile("Getlivemodestatus get starttime:" + qry);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossofEntry;
        }



        public int UpdateLiveHMIScreenDetails(int HMIID)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set isWorkOrder = 1 WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        //public int Operatoridvalid(int Roleid)
        //{
        //    int res = 0;
        //    try
        //    {
        //        Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

        //        string qry = "SELECT * FROM " + databaseName + ".[tblroles] WHERE Role_ID ='4'";
        //        res = lista.update(qry, _connectionFactory.GetConnection);
        //    }
        //    catch (Exception ex)
        //    {
        //        IntoFile(ex.ToString());
        //    }
        //    return res;
        //}

        //public int Operatorpwd(int Roleid)
        //{
        //    int res = 0;
        //    try
        //    {
        //        Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

        //        string qry = "SELECT * FROM " + databaseName + ".[tblroles] WHERE Role_ID ='4'";
        //        res = lista.update(qry, _connectionFactory.GetConnection);
        //    }
        //    catch (Exception ex)
        //    {
        //        IntoFile(ex.ToString());
        //    }
        //    return res;
        //}

        public int UpdateLiveHMIScreenDetails1(int HMIID, string value)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "update " + databaseName + ".[tbllivehmiscreen] set Shift = '" + value + "' WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLiveHMIScreenDetails2(int HMIID, string value, DateTime dateTime)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "update " + databaseName + ".[tbllivehmiscreen] set OperatorDet = '" + value + "', PEStartTime = '" + dateTime + "',isUpdate = 1 WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLiveHMIScreenDetails3(int HMIID, string value)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "update " + databaseName + ".[tbllivehmiscreen] set Project = '" + value + "' WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLiveHMIScreenDetails4(int HMIID, string value)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "update " + databaseName + ".[tbllivehmiscreen] set Prod_FAI = '" + value + "' WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLiveHMIScreenDetails5(int HMIID, string value)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "update " + databaseName + ".[tbllivehmiscreen] set PartNo = '" + value + "' WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLiveHMIScreenDetails6(int HMIID, string value)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "update " + databaseName + ".[tbllivehmiscreen] set Work_Order_No = '" + value + "' WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLiveHMIScreenDetails7(int HMIID, string value)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "update " + databaseName + ".[tbllivehmiscreen] set OperationNo = '" + value + "' WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLiveHMIScreenDetails8(int HMIID, string value)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "update " + databaseName + ".[tbllivehmiscreen] set Target_Qty = '" + value + "' WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLiveHMIScreenDetailsD(int HMIID, string value)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "update " + databaseName + ".[tbllivehmiscreen] set Delivered_Qty = '" + value + "' WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public string GetMachineDetails1(int macid)
        {
            string det = "";
            tblmachinedetail machinelist = new tblmachinedetail();
            try
            {
                Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();

                string qry = "SELECT * FROM " + databaseName + ".[tblmachinedetails] WHERE IsDeleted = 0 and MachineID =" + macid + "";
                machinelist = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
                det = machinelist.MachineDisplayName;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }

        public tbllivelossofentry GetLossOfEntryDetails3(int macid)
        {
            tbllivelossofentry lossofEntry = new tbllivelossofentry();
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivelossofentry] WHERE MachineID =" + macid + " and DoneWithRow = 0 order by LossID desc";
                lossofEntry = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossofEntry;
        }

        public tbllivelossofentry GetLossOfEntryDetails4(int macid)
        {
            tbllivelossofentry lossofEntry = new tbllivelossofentry();
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivelossofentry] WHERE MachineID =" + macid + " order by LossID desc";
                lossofEntry = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossofEntry;
        }

        public tbllossescode GetLossCodeDetails(int Bid)
        {
            tbllossescode lossCode = new tbllossescode();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE LossCodeID =" + Bid + "";
                lossCode = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public tbllossescode GetLossCodeDets(int LossCodeID)
        {
            tbllossescode lossCode = new tbllossescode();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT LossCode FROM " + databaseName + ".[tbllossescodes] WHERE LossCodeID =" + LossCodeID + "";
                lossCode = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public List<tbllossescode> GetLossCodeDetails1(int Bid)
        {
            List<tbllossescode> lossCode = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE LossCodesLevel1ID =" + Bid + " and IsDeleted = 0 and LossCodesLevel = 2 and LossCodesLevel2ID IS NULL and MessageType != 'MNT'";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public List<tbllossescode> GetLossCodeDetails2(int level)
        {
            List<tbllossescode> lossCode = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE LossCodesLevel =" + level + " and IsDeleted = 0 and LossCodesLevel1ID IS NULL and LossCodesLevel2ID IS NULL and MessageType != 'NoCode' and MessageType != 'MNT' and MessageType != 'PM'";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public List<tbllossescode> GetLossCodeDetails3(int Bid)
        {
            List<tbllossescode> lossCode = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE LossCodesLevel2ID =" + Bid + " and LossCodesLevel = 3 and MessageType != 'MNT' and IsDeleted = 0";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public string GetLossCodeDetails4(int prevLevelId)
        {
            string det = "";
            tbllossescode lossCode = new tbllossescode();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE LossCodeID =" + prevLevelId + " and IsDeleted = 0";
                lossCode = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
                det = lossCode.LossCode;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public tbllossescode GetLossCodeDetails43(int prevLevelId)
        {
            tbllossescode lossCode = new tbllossescode();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE LossCodeID =" + prevLevelId + " and IsDeleted = 0";
                lossCode = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public tbllossescode GetLossCodeDetailslist(int prevLevelId)
        {

            tbllossescode lossCode = new tbllossescode();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT LossCode FROM " + databaseName + ".[tbllossescodes] WHERE LossCodeID =" + prevLevelId + " and IsDeleted = 0";
                lossCode = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);

            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public string GetLiveHMIScreenDetails43(int machineId)
        {
            string item = "";
            tbllivehmiscreen lossCode = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT OperatorDet FROM " + databaseName + ".[tbllivehmiscreen] WHERE MachineID =" + machineId + " and Status = 0";
                lossCode = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
                item = lossCode.OperatorDet;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return item;
        }
        public string GetLiveHMIScreenDetails44(int machineId)
        {
            string item = "";
            tbllivehmiscreen lossCode = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT Shift FROM " + databaseName + ".[tbllivehmiscreen] WHERE MachineID =" + machineId + " and Status = 0";
                lossCode = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
                item = lossCode.Shift;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return item;
        }

        public List<tbllossescode> GetLossCodeDetails5(int prevLevelId)
        {
            List<tbllossescode> lossCode = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE IsDeleted = 0 and LossCodesLevel1ID =" + prevLevelId + " and LossCodesLevel2ID IS NULL";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public List<tbllossescode> GetLossCodeDetails6(int prevLevelId)
        {
            List<tbllossescode> lossCode = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE LossCodesLevel2ID =" + prevLevelId + " and LossCodesLevel = 3 and IsDeleted = 0";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public List<message_code_master> GetMessageCodeDetails()
        {
            List<message_code_master> lossCode = new List<message_code_master>();
            try
            {
                Repository<message_code_master> lista = new Repository<message_code_master>();

                string qry = "SELECT * FROM " + databaseName + ".[message_code_master] WHERE IsDeleted = 0 and (MessageType = 'IDLE' or MessageType = 'SETUP')";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public List<tbllivelossofentry> GetLossOfEntryDetails5(int macid)
        {
            List<tbllivelossofentry> lossofEntry = new List<tbllivelossofentry>();
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

                string qry = "SELECT TOP 1 * FROM " + databaseName + ".[tbllivelossofentry] WHERE MachineID =" + macid + " order by LossID desc";
                //IntoFile("Query To Get Latest Record:" + qry);
                lossofEntry = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossofEntry;
        }

        public tbllivelossofentry GetLossOfEntryDetails6(int lossid)
        {
            tbllivelossofentry lossofEntry = new tbllivelossofentry();
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivelossofentry] WHERE LossID =" + lossid + "";
                lossofEntry = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossofEntry;
        }

        public message_code_master GetMessageCodeDetails1(int abc)
        {
            message_code_master lossCode = new message_code_master();
            try
            {
                Repository<message_code_master> lista = new Repository<message_code_master>();

                string qry = "SELECT * FROM " + databaseName + ".[message_code_master] WHERE MessageCodeID =" + abc + "";
                lossCode = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        //public int UpdateLossofEntryDetails(int LossID, DateTime StartDateTime, DateTime EntryTime, DateTime EndDateTime, int machid, string shift, int MessageCodeID, string MessageDesc, string MessageCode, int IsUpdate, int DoneWithRow, int IsStart, int IsScreen, int ForRefresh)
        //{
        //    int res = 0;
        //    try
        //    {
        //        Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

        //        string qry = "UPDATE "+databaseName+".[tbllivelossofentry] set StartDateTime ='" + StartDateTime + "',EntryTime = '" + EntryTime + "',EndDateTime ='" + EndDateTime + "',MachineID =" + machid + ",Shift ='" + shift + "',MessageCodeID = " + MessageCodeID + ",MessageDesc ='" + MessageDesc + "',MessageCode = '" + MessageCode + "',IsUpdate =" + IsUpdate + ",DoneWithRow =" + DoneWithRow + ",IsStart = " + IsStart + " ,IsScreen=" + IsScreen + ",ForRefresh =" + ForRefresh + " WHERE LossID =" + LossID;
        //        res = lista.update(qry, _connectionFactory.GetConnection);
        //    }
        //    catch (Exception ex)
        //    {
        //        IntoFile(ex.ToString());
        //    }
        //    return res;
        //}

        public int UpdateLossofEntryDetails(int LossID, DateTime EntryTime, DateTime EndDateTime, int machid, string shift, int MessageCodeID, string MessageDesc, string MessageCode, int IsUpdate, int DoneWithRow, int IsStart, int IsScreen, int ForRefresh)
        {
            int res = 0;
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

                string qry = "UPDATE " + databaseName + ".[tbllivelossofentry] set EntryTime = '" + EntryTime + "',EndDateTime ='" + EndDateTime + "',MachineID =" + machid + ",Shift ='" + shift + "',MessageCodeID = " + MessageCodeID + ",MessageDesc ='" + MessageDesc + "',MessageCode = '" + MessageCode + "',IsUpdate =" + IsUpdate + ",DoneWithRow =" + DoneWithRow + ",IsStart = " + IsStart + " ,IsScreen=" + IsScreen + ",ForRefresh =" + ForRefresh + " WHERE LossID =" + LossID;
                //IntoFile("Update: the previous record when isupdate and donewithrow is 0:" + qry);
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLossofEntryDetails1(int LossID, DateTime EndDateTime, int DoneWithRow, int IsStart, int IsScreen, int ForRefresh)
        {
            int res = 0;
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

                string qry = "UPDATE " + databaseName + ".[tbllivelossofentry] set EndDateTime ='" + EndDateTime + "' ,DoneWithRow =" + DoneWithRow + " ,IsStart = " + IsStart + " ,IsScreen=" + IsScreen + ",ForRefresh =" + ForRefresh + " WHERE LossID=" + LossID;
                //IntoFile("Update: the previous record when isupdate is 1 and donewithrow is 0:" + qry);
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateLossofEntryDetails11(int LossID)
        {
            int res = 0;
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

                string qry = "UPDATE " + databaseName + ".[tbllivelossofentry] set ForRefresh =1 WHERE LossID=" + LossID;
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateLossofEntryDetails12(int LossID)
        {
            int res = 0;
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

                string qry = "UPDATE " + databaseName + ".[tbllivelossofentry] set ForRefresh =2 WHERE LossID=" + LossID;
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public List<tbllivelossofentry> GetLossOfEntryDetailsPrev(int macid)
        {
            List<tbllivelossofentry> lossofEntry = new List<tbllivelossofentry>();
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivelossofentry] WHERE MachineID =" + macid + " and DoneWithRow =0 and EndDateTime is null and IsScreen=1 ";
                lossofEntry = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossofEntry;
        }

        public int UpdatePrevRecInLossofEntryDetails(int LossID, DateTime? EndTime)
        {
            int res = 0;
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

                string qry = "UPDATE " + databaseName + ".[tbllivelossofentry] set DoneWithRow =1,EndDateTime='" + EndTime + "',IsScreen=0 WHERE LossID=" + LossID;
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public tbllivelossofentry GetPresentDet(int machineId)
        {
            tbllivelossofentry liveHMI = new tbllivelossofentry();
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivelossofentry] WHERE DoneWithRow=0 and MachineID=" + machineId + " order by LossID desc";
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public int InsertLossofEntryDetails(DateTime StartDateTime, DateTime EntryTime, DateTime EndDateTime, string CorrectedDate, int machid, string shift, int MessageCodeID, string MessageDesc, string MessageCode, int IsUpdate, int DoneWithRow, int IsStart, int IsScreen, int ForRefresh)
        {
            int res = 0;
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

                string qry = "INSERT into " + databaseName + ".[tbllivelossofentry] (StartDateTime,EntryTime,EndDateTime,CorrectedDate,MachineID, Shift,MessageCodeID ,MessageDesc,MessageCode,IsUpdate,DoneWithRow,IsStart,IsScreen,ForRefresh) VALUES(' " + StartDateTime + "','" + EntryTime + "','" + EndDateTime + "','" + CorrectedDate + "'," + machid + ",'" + shift + "'," + MessageCodeID + ",'" + MessageDesc + "','" + MessageCode + "'," + IsUpdate + "," + DoneWithRow + "," + IsStart + "," + IsScreen + "," + ForRefresh + ")";
                //IntoFile("Insert: query to insert new record when they enter new loss code" + qry);
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int InsertLiveHMIScreenDetails(int MachineID, int OperatiorID, string Shift, DateTime Date, DateTime Time, string Project, string PartNo, string OperationNo, int Rej_Qty, string Work_Order_No, int Target_Qty, int Delivered_Qty, int Status, string CorrectedDate, string Prod_FAI, int isUpdate, int DoneWithRow, int isWorkInProgress, int isWorkOrder, string OperatorDet, DateTime PEStartTime, int ProcessQty, string DDLWokrCentre, int IsMultiWO, int IsHold, string SplitWO, int batchNo)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string qry = "INSERT into " + databaseName + ".[tbllivehmiscreen] (MachineID,OperatiorID,Shift,Date,Time,Project,PartNo,OperationNo,Rej_Qty,Work_Order_No,Target_Qty,Delivered_Qty,Status,CorrectedDate,Prod_FAI,isUpdate,DoneWithRow,isWorkInProgress,isWorkOrder,OperatorDet,PEStartTime,ProcessQty,DDLWokrCentre,IsMultiWO,IsHold,SplitWO,batchNo,batchCount) VALUES (" + MachineID + "," + OperatiorID + ",'" + Shift + "','" + Date + "','" + Time + "','" + Project + "','" + PartNo + "','" + OperationNo + "'," + Rej_Qty + ",'" + Work_Order_No + "'," + Target_Qty + "," + Delivered_Qty + "," + Status + ",'" + CorrectedDate + "','" + Prod_FAI + "'," + isUpdate + "," + DoneWithRow + "," + isWorkInProgress + "," + isWorkOrder + ",'" + OperatorDet + "','" + PEStartTime + "'," + ProcessQty + ",'" + DDLWokrCentre + "'," + IsMultiWO + "," + IsHold + ",'" + SplitWO + "'," + batchNo + ")";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public List<tbllivemultiwoselection> GetMultiWODetails2(string WONo, string Part, string opNo)
        {
            List<tbllivemultiwoselection> liveHMI = new List<tbllivemultiwoselection>();
            try
            {
                Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();

                string qry = "SELECT TOP 1 * FROM " + databaseName + ".[tbllivemultiwoselection] AS WOSelection JOIN " + databaseName + ".[tbllivehmiscreen] AS LiveHMI ON WOSelection.HMIID = LiveHMI.HMIID  WHERE WOSelection.WorkOrder ='" + WONo + "' and WOSelection.PartNo ='" + Part + "' and WOSelection.OperationNo =" + opNo + " and LiveHMI.isWorkInProgress != 2 order by LiveHMI.Time desc";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public List<tbllivehmiscreen> GetLiveHMIDetails11(string WONo, string Part, string opNo)
        {
            List<tbllivehmiscreen> liveHMI = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT TOP 1* FROM " + databaseName + ".[tbllivehmiscreen] WHERE Work_Order_No ='" + WONo + "' and PartNo='" + Part + "' and OperationNo =" + opNo + " and isWorkInProgress != 2 order by Time desc";
                return lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public int UpdateddlDetails(int DDLID)
        {
            int res = 0;
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();

                string qry = "UPDATE " + databaseName + ".[tblddl] set IsCompleted = 1 WHERE DDLID =" + DDLID;
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public tbllivehmiscreen GetLiveHMIDetails9(string WONo, string Part, string opNo, int HMIID, int machineID)
        {
            tbllivehmiscreen liveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE Work_Order_No ='" + WONo + "' and PartNo='" + Part + "' and OperationNo =" + opNo + " and HMIID != " + HMIID + " and isWorkInProgress = 2 and MachineID != " + machineID;
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public int GetLiveHMIDetails10(int HMIID)
        {
            int det = 0;
            tbllivehmiscreen liveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT MachineID FROM " + databaseName + ".[tbllivehmiscreen] WHERE HMIID =" + HMIID;
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
                det = liveHMI.MachineID;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }

        public tbllivehmiscreen GetLiveHMIDetails101(int HMIID)
        {
            tbllivehmiscreen liveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE HMIID =" + HMIID;
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }
        public tbllivemultiwoselection GetMultiWODetails3(string WorkOrder, string OperationNo, string PartNo, int HMIID)
        {
            tbllivemultiwoselection mwo = new tbllivemultiwoselection();
            try
            {
                Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();

                string qry = "SELECT TOP 1 * FROM " + databaseName + ".[tbllivemultiwoselection] WHERE WorkOrder = '" + WorkOrder + "' and OperationNo ='" + OperationNo + "' and PartNo='" + PartNo + "' and HMIID !=" + HMIID + " and IsCompleted = 0";
                mwo = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return mwo;
        }

        public tblddl GetddlDetails3(string woNo, string partNo, string opNo)
        {
            tblddl ddl = new tblddl();
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();

                string qry = "SELECT * FROM " + databaseName + ".[tblddl] WHERE WorkOrder='" + woNo + "' and MaterialDesc='" + partNo + "' and OperationNo ='" + opNo + "'";
                ddl = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ddl;
        }

        public tbllivemultiwoselection GetMultiWODetails4(string WONo, string Part, string opNo, int HMIID)
        {
            tbllivemultiwoselection mwo = new tbllivemultiwoselection();
            try
            {
                Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();

                string qry = "SELECT TOP 1 * FROM " + databaseName + ".[tbllivemultiwoselection] AS WOSelection JOIN " + databaseName + ".[tbllivehmiscreen] AS LiveHMI ON WOSelection.HMIID = LiveHMI.HMIID  WHERE WOSelection.WorkOrder ='" + WONo + "' and WOSelection.PartNo ='" + Part + "' and WOSelection.OperationNo =" + opNo + " and WOSelection.HMIID != " + HMIID + " and LiveHMI.isWorkInProgress = 2";
                mwo = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return mwo;
        }

        public int UpdateMWoDetails(int MultiWOID)
        {
            int res = 0;
            try
            {
                Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();

                string qry = "UPDATE " + databaseName + ".[tbllivemultiwoselection] set IsCompleted = 1 WHERE MultiWOID =" + MultiWOID;
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateddlDetails1(int DDLID, int deliveredQty)
        {
            int res = 0;
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();

                string qry = "UPDATE " + databaseName + ".[tblddl] set IsCompleted = 1,DeliveredQty = " + deliveredQty + " WHERE DDLID =" + DDLID;
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLiveHMIDetails(int HMIID, string SplitWO, int Status, int ProcessQty, int Delivered_Qty, DateTime Time, int isWorkInProgress, int isWorkOrder)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set SplitWO ='" + SplitWO + "', Status =" + Status + ",ProcessQty =" + ProcessQty + ",Delivered_Qty =" + Delivered_Qty + ",Time ='" + Time + "',isWorkInProgress =" + isWorkInProgress + ",isWorkOrder =" + isWorkOrder + " where HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public tbllivehmiscreen GetLiveHMIDetails12(int MachineID, string CorrectedDate)
        {
            tbllivehmiscreen liveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT TOP 1 * FROM " + databaseName + ".[tbllivehmiscreen] WHERE MachineID =" + MachineID + " and Status = 0 and CorrectedDate ='" + CorrectedDate + "'";
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public int InsertLiveHMIScreenDetails1(int MachineID, int OperatiorID, string Shift, int Status, string CorrectedDate, int isWorkInProgress, string OperatorDet, DateTime PEStartTime)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string qry = "INSERT into " + databaseName + ".[tbllivehmiscreen] (MachineID,OperatiorID,Shift,OperationNo,Status, CorrectedDate,isWorkInProgress,OperatorDet,PEStartTime) VALUES (" + MachineID + "," + OperatiorID + ",'" + Shift + "'," + Status + ",'" + CorrectedDate + "'," + isWorkInProgress + ",'" + OperatorDet + "','" + PEStartTime + "')";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLiveHMIDetails1(int HMIID, int MachineID, int OperatiorID, string Shift, DateTime Date, DateTime Time, string Project, string PartNo, string OperationNo, int Rej_Qty, string Work_Order_No, int Target_Qty, int Delivered_Qty, int Status, string CorrectedDate, string Prod_FAI, int isUpdate, int DoneWithRow, int isWorkInProgress, int isWorkOrder, string OperatorDet, DateTime PEStartTime, int ProcessQty, string DDLWokrCentre, int IsMultiWO, int IsHold, string SplitWO, int batchNo)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set MachineID =" + MachineID + ", Shift ='" + Shift + "',Date ='" + Date + "',Delivered_Qty =" + Delivered_Qty + ",Time ='" + Time + "',Project ='" + Project + "',PartNo ='" + PartNo + "',OperationNo='" + OperationNo + "',Rej_Qty=" + Rej_Qty + " ,Work_Order_No ='" + Work_Order_No + "',Target_Qty =" + Target_Qty + " ,Delivered_Qty =" + Delivered_Qty + ",Status=" + Status + ",CorrectedDate='" + CorrectedDate + "',Prod_FAI='" + Prod_FAI + "',isUpdate=" + isUpdate + " ,DoneWithRow=" + DoneWithRow + ",isWorkInProgress =" + isWorkInProgress + ",isWorkOrder =" + isWorkOrder + ",OperatorDet ='" + OperatorDet + "',PEStartTime ='" + PEStartTime + "',ProcessQty=" + ProcessQty + ",DDLWokrCentre='" + DDLWokrCentre + "' ,IsMultiWO=" + IsMultiWO + ",IsHold =" + IsHold + ",IsHold=" + IsHold + " ,SplitWO='" + SplitWO + "',batchNo=" + batchNo + " WHERE HMIID=" + HMIID;

                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int DeleteMWoDetails()
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "DELETE From " + databaseName + ".[tbllivemultiwoselection] WHERE HMIID in (SELECT MWO.HMIID FROM" + databaseName + ".[tbllivemultiwoselection] AS MWO JOIN " + databaseName + ".[tbllivehmiscreen] AS Live ON MWO.HMIID = Live.HMIID)";
                res = Convert.ToInt32(lista.delete(qry, _connectionFactory.GetConnection));
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLossofEntryDetails(int LossID, int MessageCodeID, DateTime StartDateTime, DateTime EndDateTime, DateTime EntryTime, string CorrectedDate, int MachineID, string Shift, string MessageDesc, string MessageCode, int IsUpdate, int DoneWithRow, int IsStart, int IsScreen, int ForRefresh)
        {
            int res = 0;
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();
                string qry = "UPDATE " + databaseName + ".[tbllivelossofentry] set MessageCodeID=" + MessageCodeID + " ,StartDateTime='" + StartDateTime + "' ,EndDateTime='" + EndDateTime + "',EntryTime='" + EntryTime + "',CorrectedDate='" + CorrectedDate + "' ,MachineID=" + MachineID + "  ,Shift='" + Shift + "',MessageDesc='" + MessageDesc + "',MessageCode='" + MessageCode + "',IsUpdate" + IsUpdate + ",DoneWithRow=" + DoneWithRow + ",IsStart=" + IsStart + " ,IsScreen=" + IsScreen + " ,ForRefresh=" + ForRefresh + " WHERE LossID=" + LossID;
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public List<tblbreakdown> GetBreakDownDetails1(int id)
        {
            List<tblbreakdown> lossCode = new List<tblbreakdown>();
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();

                string qry = "SELECT TOP 1 * FROM " + databaseName + ".[tblbreakdown] WHERE MachineID =" + id + " and DoneWithRow =0 order by BreakdownID desc";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public int UpdateBreakDownDetails(int BreakdownID, DateTime StartTime, DateTime EndTime, int BreakDownCode, int MachineID, string CorrectedDate, string Shift, string MessageDesc, string MessageCode, int DoneWithRow)
        {
            int res = 0;
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();

                string qry = "UPDATE " + databaseName + ".[tblbreakdown] set StartTime='" + StartTime + "',EndTime='" + EndTime + "' ,BreakDownCode=" + BreakDownCode + " ,MachineID=" + MachineID + " ,CorrectedDate='" + CorrectedDate + "',Shift='" + Shift + "',MessageDesc='" + MessageDesc + "',MessageCode='" + MessageCode + "',DoneWithRow=" + DoneWithRow + " WHERE BreakdownID=" + BreakdownID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public tblbreakdown GetBreakDownDetails2(int id)
        {
            tblbreakdown lossCode = new tblbreakdown();
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();

                string qry = "SELECT TOP 1 * FROM " + databaseName + ".[tblbreakdown] WHERE MachineID =" + id + " and DoneWithRow =0 order by BreakdownID desc";
                lossCode = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public tblbreakdown GetBreakDownDetails3(int id)
        {
            tblbreakdown lossCode = new tblbreakdown();
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();

                string qry = "SELECT * FROM " + databaseName + ".[tblbreakdown] WHERE BreakdownID =" + id + "";
                lossCode = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public List<tbllossescode> GetLossCodeDetails7(int Bid)
        {
            List<tbllossescode> lossCode = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                //string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE LossCodesLevel1ID =" + Bid + " and IsDeleted = 0 and LossCodesLevel2ID IS NULL and MessageType = 'MNT' and MessageType = 'BREAKDOWN' ";
                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE LossCodesLevel1ID =" + Bid + " and IsDeleted = 0 and LossCodesLevel2ID IS NULL and (MessageType = 'MNT' or MessageType = 'BREAKDOWN') ";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public List<tbllossescode> GetLossCodeDetails8()
        {
            List<tbllossescode> lossCode = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE LossCodesLevel1ID IS NULL and LossCodesLevel2ID IS NULL and LossCodesLevel = 1 and IsDeleted = 0 and MessageType = 'MNT' ";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public List<tbllossescode> GetLossCodeDetails10(int Bid)
        {
            List<tbllossescode> lossCode = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE LossCodesLevel2ID =" + Bid + " and (MessageType = 'MNT' or MessageType = 'BREAKDOWN') and IsDeleted = 0";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public List<tbllossescode> GetLossCodeDetails9()
        {
            List<tbllossescode> lossCode = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                //string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE LossCodesLevel = 1 and IsDeleted = 0 and MessageType = 'MNT' and LossCode != '9999' and MessageType = 'BREAKDOWN'";
                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE LossCodesLevel = 1 and IsDeleted = 0 and(MessageType = 'MNT' or MessageType = 'BREAKDOWN')";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public int InsertBreakDownDetails(int BreakDownCode, string CorrectedDate, int DoneWithRow, int MachineID, string MessageDesc, string MessageCode, string Shift, DateTime StartTime)
        {
            int res = 0;
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();
                string qry = "INSERT into " + databaseName + ".[tblbreakdown] (BreakDownCode,CorrectedDate,DoneWithRow,MachineID,MessageDesc, MessageCode,Shift,StartTime) VALUES (" + BreakDownCode + ",'" + CorrectedDate + "'," + DoneWithRow + "," + MachineID + ",'" + MessageDesc + "','" + MessageCode + "','" + Shift + "','" + StartTime + "')";

                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public tbllivemode GetLiveModeDetails1(int machineid)
        {
            tbllivemode lossofEntry = new tbllivemode();
            try
            {
                Repository<tbllivemode> lista = new Repository<tbllivemode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivemode] WHERE MachineID =" + machineid + " and IsCompleted = 0 order by StartTime desc";
                lossofEntry = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossofEntry;
        }

        public int UpdateLiveModeDetails(int ModeID, DateTime EndTime, double diff)
        {
            int res = 0;
            try
            {
                Repository<tbllivemode> lista = new Repository<tbllivemode>();

                string qry = "UPDATE " + databaseName + ".[tbllivemode] set IsCompleted = 1 ,EndTime ='" + EndTime + "', ModeTypeEnd = 1 ,DurationInSec=" + diff + " WHERE ModeID=" + ModeID + "";
                //IntoFile("Update query to livemodedb table:" + qry);
                res = lista.update(qry, _connectionFactory.GetConnection);

            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public List<tbllivehmiscreen> LiveHMIDetails1(string WONoInner, string OperationInner)
        {
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            List<tbllivehmiscreen> det = new List<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivehmiscreen WHERE Work_Order_No = '" + WONoInner + "' and OperationNo = '" + OperationInner + "' and Date is null order by HMIID desc";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tbllivehmiscreen> LiveHMIDetails(string WONoInner, string OperationInner)
        {
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            List<tbllivehmiscreen> det = new List<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivehmiscreen WHERE Work_Order_No = '" + WONoInner + "' and OperationNo = " + OperationInner + " and Date is not null and Time is null order by HMIID desc";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public string tblLiveHMIDetails(string WONoInner)
        {
            string opno = "";
            tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT OperationNo From " + databaseName + ".tbllivehmiscreen WHERE Work_Order_No ='" + WONoInner + "' order by HMIID desc";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                opno = ddldet.OperationNo;
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return opno;
        }
        public List<tbllivehmiscreen> LiveHMIDetails2(string WONoInner, string OperationInner)
        {
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            List<tbllivehmiscreen> det = new List<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivehmiscreen WHERE Work_Order_No = '" + WONoInner + "' and OperationNo = " + OperationInner + " order by HMIID desc";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public int InsertLiveHMIDetailsddl(DateTime PEStartTime, string OperationNo, string MaterialDesc, string Project, string CorrectedDate, string Shift, string Type, int OperatiorID, string OpName, int TargetQty, string WorkOrder, int isWorkOrder, int TotalProcessQty, int ishold, string WorkCenter, int machineId)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "INSERT INTO " + databaseName + ".[tbllivehmiscreen](MachineID,OperatiorID,Shift,Prod_FAI,Project,PartNo,OperationNo,Work_Order_No,Target_Qty,Status,CorrectedDate,isWorkOrder,OperatorDet,PEStartTime,ProcessQty,DDLWokrCentre,IsHold) VALUES(" + machineId + "," + OperatiorID + ",'" + Shift + "','" + Type + "','" + Project + "','" + MaterialDesc + "','" + OperationNo + "','" + WorkOrder + "'," + TargetQty + ",0,'" + CorrectedDate + "'," + isWorkOrder + ",'" + OpName + "','" + PEStartTime + "'," + TotalProcessQty + ",'" + WorkCenter + "'," + ishold + ")";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {

            }
            return res;
        }
        public tbllivehmiscreen HMIList(string WONoInner, string OperationInner)
        {
            tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tbllivehmiscreen WHERE OperationNo = '" + OperationInner + "' and Work_Order_No='" + WONoInner + "' order by HMIID desc";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public tblhmiscreen OLDHMI(string WONoInner, string OperationInner)
        {
            tblhmiscreen ddldet = new tblhmiscreen();
            Repository<tblhmiscreen> lista = new Repository<tblhmiscreen>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tblhmiscreen WHERE OperationNo = " + OperationInner + " and Work_Order_No='" + WONoInner + "' order by HMIID desc";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public List<tblhmiscreen> OldHistDet(string WONoInner, string OperationInner)
        {
            Repository<tblhmiscreen> lista = new Repository<tblhmiscreen>();
            List<tblhmiscreen> det = new List<tblhmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblhmiscreen WHERE Work_Order_No = '" + WONoInner + "' and OperationNo = " + OperationInner + " and Date is not null and Time is null order by HMIID desc";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tblhmiscreen> OldHistDet1(string WONoInner, string OperationInner)
        {
            Repository<tblhmiscreen> lista = new Repository<tblhmiscreen>();
            List<tblhmiscreen> det = new List<tblhmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblhmiscreen WHERE Work_Order_No = '" + WONoInner + "' and OperationNo = '" + OperationInner + "' and Date is null order by HMIID desc";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public tblddl DDLList(string WONoInner, string OperationInner)
        {
            tblddl ddldet = new tblddl();
            Repository<tblddl> lista = new Repository<tblddl>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tblddl WHERE OperationNo < " + OperationInner + " and WorkOrder='" + WONoInner + "' and IsCompleted = 0 order by DDLID desc";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public List<tblhmiscreen> OldHistDet2(string WONoInner, string OperationInner)
        {
            Repository<tblhmiscreen> lista = new Repository<tblhmiscreen>();
            List<tblhmiscreen> det = new List<tblhmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblhmiscreen WHERE Work_Order_No = '" + WONoInner + "' and OperationNo = " + OperationInner + " order by HMIID desc";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public tblddl DDLList1(string WONoInner, string OperationInner)
        {
            tblddl ddldet = new tblddl();
            Repository<tblddl> lista = new Repository<tblddl>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tblddl WHERE WorkOrder ='" + WONoInner + "' and OperationNo >" + OperationInner + " order by Convert(int,OperationNo) asc";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public string tblHistoryHMIDet(string WONoInner)
        {
            string opno = "";
            tblhmiscreen ddldet = new tblhmiscreen();
            Repository<tblhmiscreen> lista = new Repository<tblhmiscreen>();
            try
            {
                string query = "SELECT OperationNo From " + databaseName + ".tblhmiscreen WHERE Work_Order_No ='" + WONoInner + "' order by HMIID desc";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                opno = ddldet.OperationNo;
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return opno;
        }
        public int InsertLiveModeDetails(int MachineID, string CorrectedDate, int InsertedBy, DateTime? StartTime, string ColorCode, int shift, string ModeType, DateTime InsertedOn, int IsDeleted, string Mode, int IsCompleted)
        {
            int res = 0;
            try
            {
                Repository<tbllivemode> lista = new Repository<tbllivemode>();
                string qry = "INSERT into " + databaseName + ".[tbllivemode] (MachineID,CorrectedDate,InsertedBy,StartTime,ColorCode, InsertedOn,IsDeleted,MacMode,IsCompleted,ModeType,IsShiftEnd) " +
                    "VALUES (" + MachineID + ",'" + CorrectedDate + "'," + InsertedBy + ",'" + StartTime + "','" + ColorCode + "','" + InsertedOn + "'," + IsDeleted + ",'" + Mode + "'," + IsCompleted + ",'" + ModeType + "'," + shift + ")";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

            }
            return res;
        }

        public tblbreakdown GetBreakDownDetails4(int BreakDownCode, int MachineID)
        {
            tblbreakdown lossCode = new tblbreakdown();
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();

                string qry = "SELECT * FROM " + databaseName + ".[tblbreakdown] WHERE BreakDownCode =" + BreakDownCode + " and MachineID=" + MachineID + " and DoneWithRow = 0 order by BreakdownID desc";
                lossCode = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public int UpdateBreakDownDetails(int BreakdownID, DateTime EndTime)
        {
            int res = 0;
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();

                string qry = "UPDATE " + databaseName + ".[tblbreakdown] set DoneWithRow = 1 ,EndTime ='" + EndTime + "' WHERE BreakdownID =" + BreakdownID + "";

                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public List<tbllossescode> GetLossCodeDetail8(int idlecode)
        {
            List<tbllossescode> lossCode = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE LossCodeID =" + idlecode + " and IsDeleted = 0";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public int InsertLiveHMIDetails1(int MachineID, string CorrectedDate, string shift, int Status, int OperatiorID, int isWorkInProgress)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "INSERT INTO " + databaseName + ".[tbllivehmiscreen](MachineID,CorrectedDate,Shift,Status,OperatiorID,isWorkInProgress) VALUES(" + MachineID + ",'" + CorrectedDate + "','" + shift + "'," + Status + "," + OperatiorID + "," + isWorkInProgress + ")";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public tbluser GetUserDetails1(int opid)
        {
            tbluser user = new tbluser();
            try
            {
                Repository<tbluser> lista = new Repository<tbluser>();

                string qry = "SELECT * FROM " + databaseName + ".[tblusers] WHERE UserID =" + opid + "";
                user = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return user;
        }

        public int UpdateLiveHMIScreenDetails9(int HMIID, int isUpdate, int isWorkInProgress, DateTime Time)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "update " + databaseName + ".[tbllivehmiscreen] set isUpdate = " + isUpdate + " ,isWorkInProgress =" + isWorkInProgress + ",Time ='" + Time + "' WHERE HMIID =" + HMIID;
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int InsertLiveHMIDetails2(int MachineID, string CorrectedDate, string shift, DateTime PEStartTime, int Status, int isWorkInProgress, int isWorkOrder, int OperatiorID)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "INSERT INTO " + databaseName + ".[tbllivehmiscreen](MachineID,CorrectedDate,Shift,PEStartTime,Status,isWorkInProgress,isWorkOrder,OperatiorID) VALUES(' " + MachineID + ",'" + CorrectedDate + "','" + shift + "','" + PEStartTime + "'," + Status + "," + isWorkInProgress + "," + isWorkOrder + "," + OperatiorID + ")";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLiveHMIScreenDetails10(int HMIID, string Shift, string CorrectedDate, DateTime PEStartTime)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "update " + databaseName + ".[tbllivehmiscreen] set Shift = '" + Shift + "' ,CorrectedDate ='" + CorrectedDate + "', PEStartTime ='" + PEStartTime + "' WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLHMIScreenDetails(int HMIID)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "update " + databaseName + ".[tbllivehmiscreen] set Date IS NULL WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLivHMIDetails(int HMIID, string Prod_FAI, int Target_Qty, string OperationNo, string PartNo, string Work_Order_No, string Project, DateTime Date, string DDLWokrCentre, int ProcessQty)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set Prod_FAI = '" + Prod_FAI + ", Target_Qty =" + Target_Qty + " , OperationNo ='" + OperationNo + "',PartNo ='" + PartNo + "', Work_Order_No='" + Work_Order_No + "', Project ='" + Project + "' , Date='" + Date + "' ,DDLWokrCentre='" + DDLWokrCentre + "',ProcessQty=" + ProcessQty + " WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public tbllivemultiwoselection GetMultiWorkDetails(string WorkOrder, string OperationNo, string PartNo)
        {
            tbllivemultiwoselection mwo = new tbllivemultiwoselection();
            try
            {
                Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivemultiwoselection] WHERE WorkOrder = '" + WorkOrder + "' and OperationNo ='" + OperationNo + "' and PartNo='" + PartNo + "' and IsCompleted = 1";
                mwo = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return mwo;
        }

        public List<tbllivemultiwoselection> GetMultiWOtDetails(string WONo, string Part, string localOPNo)
        {
            List<tbllivemultiwoselection> MWork = new List<tbllivemultiwoselection>();
            try
            {
                Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();

                string qry = "SELECT TOP 1 * from " + databaseName + ".[tbllivemultiwoselection] where WorkOrder ='" + WONo + "' and PartNo = '" + Part + "' and OperationNo ='" + localOPNo + "' order by MultiWOID";
                MWork = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return MWork;
        }

        public List<tbl_multiwoselection> GetMultiWorkSelectionDetails(string WONo, string Part, string localOPNo)
        {
            List<tbl_multiwoselection> MWork = new List<tbl_multiwoselection>();
            try
            {
                Repository<tbl_multiwoselection> lista = new Repository<tbl_multiwoselection>();

                string qry = "SELECT TOP 1 * from " + databaseName + ".[tbl_multiwoselection] where WorkOrder ='" + WONo + "' and PartNo = '" + Part + "' and OperationNo ='" + localOPNo + "' order by MultiWOID";
                MWork = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return MWork;
        }

        public tbllivehmiscreen GetHMIDetails(int HMIID)
        {
            tbllivehmiscreen machine = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE HMIID =" + HMIID + "";
                machine = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return machine;
        }

        public tblhmiscreen GetHMIDetails1(string WONo, string Part, string localOPNoString)
        {
            tblhmiscreen liveHMI = new tblhmiscreen();
            try
            {
                Repository<tblhmiscreen> lista = new Repository<tblhmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tblhmiscreen] WHERE Work_Order_No ='" + WONo + "' and PartNo='" + Part + "' and OperationNo ='" + localOPNoString + "'";
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public tblddl GettblddlDetails(string WorkOrder, string Part, string Operation)
        {
            tblddl liveHMI = new tblddl();
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();

                string qry = "SELECT * FROM " + databaseName + ".[tblddl] WHERE WorkOrder ='" + WorkOrder + "' and MaterialDesc='" + Part + "' and OperationNo ='" + Operation + "' and IsCompleted = 1";
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public tblhmiscreen GetHisHMIDetails(string WONo, string Part, string opNo, int HMIID)
        {
            tblhmiscreen liveHMI = new tblhmiscreen();
            try
            {
                Repository<tblhmiscreen> lista = new Repository<tblhmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tblhmiscreen] WHERE Work_Order_No ='" + WONo + "' and PartNo='" + Part + "' and OperationNo ='" + opNo + "' and HMIID != " + HMIID + " and isWorkInProgress != 2 order by Time desc";
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public List<tbl_multiwoselection> GetMultiWorkOrderDetails(string WONo, string Part, string opNo)
        {
            List<tbl_multiwoselection> liveHMI = new List<tbl_multiwoselection>();
            try
            {
                Repository<tbl_multiwoselection> lista = new Repository<tbl_multiwoselection>();

                string qry = "SELECT TOP 1 * FROM " + databaseName + ".[tbl_multiwoselection] AS WOSelection JOIN " + databaseName + ".[tbllivehmiscreen] AS LiveHMI ON WOSelection.HMIID = LiveHMI.HMIID  WHERE WOSelection.WorkOrder ='" + WONo + "' and WOSelection.PartNo ='" + Part + "' and WOSelection.OperationNo ='" + opNo + "' and LiveHMI.isWorkInProgress != 2 order by LiveHMI.Time desc";
                return lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public int UpdateLivHMIDets(int HMIID, int Target_Qty, int ProcessQty, string SplitWO, int isWorkInProgress, int Status, DateTime Time, int Delivered_Qty)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set Target_Qty =" + Target_Qty + ",ProcessQty=" + ProcessQty + " ,SplitWO='" + SplitWO + "',isWorkInProgress=" + isWorkInProgress + ",Status =" + Status + ",Time='" + Time + "',Delivered_Qty=" + Delivered_Qty + " WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public List<tblddl> GetddlDets(string woNo, string partNo, string opNo)
        {
            List<tblddl> ddl = new List<tblddl>();
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();

                string qry = "SELECT * FROM " + databaseName + ".[tblddl] WHERE IsCompleted = 0 and WorkOrder='" + woNo + "' and MaterialDesc='" + partNo + "' and OperationNo ='" + opNo + "'";
                ddl = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ddl;
        }

        public int UpdateLivHMI1Dets(int HMIID, int ProcessQty, DateTime Date)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set ProcessQty=" + ProcessQty + ",Date='" + Date + "' WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

            }
            return res;
        }

        public int UpdateLHMIDetails(int HMIID, string OperatorDet, string Shift, string Prod_FAI, int ProcessQty, DateTime Date)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set OperatorDet ='" + OperatorDet + "',Shift ='" + Shift + "', Prod_FAI='" + Prod_FAI + "', ProcessQty=" + ProcessQty + "',Date ='" + Date + "' WHERE HMIID=" + HMIID + "";

                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateMultoWODetails(int MultiWOID)
        {
            int res = 0;
            try
            {
                Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();

                string qry = "UPDATE " + databaseName + ".[tbllivemultiwoselection] set IsSubmit = 1 WHERE MultiWOID=" + MultiWOID + "";

                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public tbllivehmiscreen GetLiHMIDetailsfirst(string WONo, string Part, string opNo, int machineID)
        {
            tbllivehmiscreen liveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                //string qry = "SELECT * FROM "+databaseName+".[tbllivehmiscreen] WHERE Work_Order_No ='" + WONo + "' and PartNo='" + Part + "' and OperationNo ='" + opNo + "' and isWorkInProgress = 2 and MachineID != " + machineID + "";  //Commented by monika bcz we are not checking date 
                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE Work_Order_No ='" + WONo + "' and PartNo='" + Part + "' and OperationNo ='" + opNo + "' and isWorkInProgress = 2 and MachineID != " + machineID + " and Date is not null";
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public List<tbllivehmiscreen> GetLiHMIDetails(string WONo, string Part, string opNo, int machineID)
        {
            List<tbllivehmiscreen> liveHMI = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE Work_Order_No ='" + WONo + "' and PartNo='" + Part + "' and OperationNo ='" + opNo + "' and isWorkInProgress = 2 and MachineID != " + machineID + "";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public int UpdateLivHMI2Dets(int HMIID, int ProcessQty)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set ProcessQty=" + ProcessQty + " WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public List<tbllivemultiwoselection> GetMultiWOListDetails(string WONo, string Part, string opNo, int HMIID)
        {
            List<tbllivemultiwoselection> mwo = new List<tbllivemultiwoselection>();
            try
            {
                Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();

                string qry = "SELECT TOP 1 * FROM " + databaseName + ".[tbllivemultiwoselection] AS WOSelection JOIN " + databaseName + ".[tbllivehmiscreen] AS LiveHMI ON WOSelection.HMIID = LiveHMI.HMIID  WHERE WOSelection.WorkOrder ='" + WONo + "' and WOSelection.PartNo ='" + Part + "' and WOSelection.OperationNo ='" + opNo + "' and WOSelection.HMIID != " + HMIID + " and LiveHMI.isWorkInProgress = 2";
                mwo = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return mwo;
        }

        public int UpdateMultiWork2Dets(int MultiWOID, int ProcessQty)
        {
            int res = 0;
            try
            {
                Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();

                string qry = "UPDATE " + databaseName + ".[tbllivemultiwoselection] set ProcessQty=" + ProcessQty + " WHERE MultiWOID =" + MultiWOID;
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLiv1HMIDetails(int HMIID, string SplitWO, int Status, int Delivered_Qty, DateTime Time, int isWorkInProgress, int isWorkOrder)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set SplitWO ='" + SplitWO + "' ,Status =" + Status + ",Delivered_Qty =" + Delivered_Qty + ",Time ='" + Time + "',isWorkInProgress =" + isWorkInProgress + ",isWorkOrder =" + isWorkOrder + " where HMIID=" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public tbllivehmiscreen GetLiveHMIScreenDetails3(string CorrectedDate, int MachineID)
        {
            tbllivehmiscreen LiveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * from " + databaseName + ".[tbllivehmiscreen] WHERE CorrectedDate ='" + CorrectedDate + "' and MachineID=" + MachineID + " and Status = 0";
                LiveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return LiveHMI;
        }

        public List<tblbreakdown> GetbreakdownDetails(int MachineID, string CorrectedDate)
        {
            List<tblbreakdown> mwo = new List<tblbreakdown>();
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();

                string qry = "SELECT Project1.BreakdownID, Project1.StartTime, Project1.EndTime, Project1.BreakDownCode, Project1.MachineID, Project1.CorrectedDate, Project1.Shift, Project1.MessageDesc, Project1.MessageCode, Project1.DoneWithRow, Project1.LossCodeID, Project1.LossCode, Project1.LossCodeDesc, Project1.MessageType, Project1.LossCodesLevel, Project1.LossCodesLevel1ID, Project1.LossCodesLevel2ID, Project1.ContributeTo, Project1.IsDeleted, Project1.CreatedOn, Project1.CreatedBy, Project1.ModifiedOn, Project1.ModifiedBy, Project1.EndCode, Project1.DeletedDate FROM(SELECT Extent1.BreakdownID, Extent1.StartTime, Extent1.EndTime, Extent1.BreakDownCode, Extent1.MachineID, Extent1.CorrectedDate,Extent1.Shift, Extent1.MessageDesc, Extent1.MessageCode, Extent1.DoneWithRow, Extent2.LossCodeID, Extent2.LossCode, Extent2.LossCodeDesc, Extent2.MessageType, Extent2.LossCodesLevel, Extent2.LossCodesLevel1ID, Extent2.LossCodesLevel2ID, Extent2.ContributeTo, Extent2.IsDeleted, Extent2.CreatedOn, Extent2.CreatedBy, Extent2.ModifiedOn, Extent2.ModifiedBy, Extent2.EndCode, Extent2.DeletedDate FROM " + databaseName + ".[tblbreakdown] AS Extent1 LEFT OUTER JOIN " + databaseName + ".[tbllossescodes] AS Extent2 ON Extent1.BreakDownCode = Extent2.LossCodeID WHERE (Extent1.MachineID = " + MachineID + " AND Extent1.CorrectedDate = '" + CorrectedDate + "' AND Extent1.DoneWithRow = 1)) AS Project1 ORDER BY Project1.StartTime DESC";
                return lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return mwo;
        }

        public List<tbllivelossofentry> GetLossofEntriesDetails(int MachineID, string CorrectedDate)
        {
            List<tbllivelossofentry> mwo = new List<tbllivelossofentry>();
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

                string qry = "SELECT Project1.LossID, Project1.MessageCodeID, Project1.StartDateTime, Project1.EndDateTime, Project1.EntryTime, Project1.CorrectedDate, Project1.MachineID, Project1.Shift, Project1.MessageDesc, Project1.MessageCode, Project1.IsUpdate, Project1.DoneWithRow, Project1.IsStart, Project1.IsScreen, Project1.ForRefresh, Project1.LossCodeID, Project1.LossCode, Project1.LossCodeDesc, Project1.MessageType, Project1.LossCodesLevel, Project1.LossCodesLevel1ID, Project1.LossCodesLevel2ID, Project1.ContributeTo, Project1.IsDeleted, Project1.CreatedOn, Project1.CreatedBy, Project1.ModifiedOn, Project1.ModifiedBy, Project1.EndCode, Project1.DeletedDate FROM(SELECT Extent1.LossID, Extent1.MessageCodeID, Extent1.StartDateTime, Extent1.EndDateTime, Extent1.EntryTime, Extent1.CorrectedDate, Extent1.MachineID, Extent1.Shift, Extent1.MessageDesc, Extent1.MessageCode, Extent1.IsUpdate, Extent1.DoneWithRow, Extent1.IsStart, Extent1.IsScreen, Extent1.ForRefresh, Extent2.LossCodeID, Extent2.LossCode, Extent2.LossCodeDesc, Extent2.MessageType, Extent2.LossCodesLevel, Extent2.LossCodesLevel1ID, Extent2.LossCodesLevel2ID, Extent2.ContributeTo, Extent2.IsDeleted, Extent2.CreatedOn, Extent2.CreatedBy, Extent2.ModifiedOn, Extent2.ModifiedBy, Extent2.EndCode, Extent2.DeletedDate FROM " + databaseName + ".[tbllivelossofentry] AS Extent1 INNER JOIN " + databaseName + ".[tbllossescodes] AS Extent2 ON Extent1.MessageCodeID = Extent2.LossCodeID WHERE(Extent1.MachineID =" + MachineID + "AND Extent1.CorrectedDate ='" + CorrectedDate + "'AND Extent1.DoneWithRow = 1)) AS Project1 ORDER BY Project1.StartDateTime DESC";
                return lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return mwo;
        }

        public tbllivehmiscreen GetLiveHMIScreen1Details(int MachineID)
        {
            tbllivehmiscreen LiveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * from " + databaseName + ".[tbllivehmiscreen] WHERE MachineID =" + MachineID + " and Status= 0 and isUpdate = 1 order by HMIID desc";
                LiveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return LiveHMI;
        }

        public int InsertLiveHMIScreenDetails2(int MachineID, int OperatiorID, string Shift, int Status, string CorrectedDate, int isWorkInProgress, string OperatorDet, DateTime PEStartTime, int isUpdate)
        {
            int res = 0;
            try
            {
                DateTime startTime = Convert.ToDateTime(PEStartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string qry = "INSERT into " + databaseName + ".[tbllivehmiscreen] (MachineID,OperatiorID,Shift,Status, CorrectedDate,isWorkInProgress,PEStartTime,isUpdate)" +
                    " VALUES (" + MachineID + "," + OperatiorID + ",'" + Shift + "'," + Status + ",'" + CorrectedDate + "'," + isWorkInProgress + ",'" + startTime + "'," + isUpdate + ")";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int InsertLiveHMIScreenDetails3(int MachineID, int OperatiorID, string Shift, int Status, string CorrectedDate, int isWorkInProgress, DateTime PEStartTime, int isUpdate)
        {
            int res = 0;
            try
            {
                DateTime startTime = Convert.ToDateTime(PEStartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string qry = "INSERT into " + databaseName + ".[tbllivehmiscreen] (MachineID,OperatiorID,Shift,Status, CorrectedDate,isWorkInProgress,PEStartTime,isUpdate)" +
                    " VALUES (" + MachineID + "," + OperatiorID + ",'" + Shift + "'," + Status + ",'" + CorrectedDate + "'," + isWorkInProgress + ",'" + startTime + "',0)";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public List<tbllivehmiscreen> GetListHMIDetails(int MachineID, int opid)
        {
            List<tbllivehmiscreen> liveHMI = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE MachineID =" + MachineID + " and OperatiorID=" + opid + " and Status = 0 order by HMIID desc";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

            }
            return liveHMI;
        }

        public List<tbllivehmiscreen> GetList1HMIDetails(int MachineID, int opid)
        {
            List<tbllivehmiscreen> liveHMI = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE MachineID =" + MachineID + " and OperatiorID=" + opid + " and Status = 0 order by Date";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public List<tbllivehmiscreen> GetList2HMIDetails(int MachineID)
        {
            List<tbllivehmiscreen> liveHMI = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE MachineID =" + MachineID + " and Status = 0 order by Date";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                //MessageBox.Show(ex.ToString());
            }
            return liveHMI;
        }

        public tbllivehmiscreen GetLiveHMI1Details(string WONo, string Part, string opNo, int HMIID, int MachineID)
        {
            tbllivehmiscreen liveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE Work_Order_No ='" + WONo + "' and PartNo='" + Part + "' and OperationNo ='" + opNo + "' and Status = 0 and MachineID =" + MachineID + " and HMIID != " + HMIID + "";
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                //MessageBox.Show(ex.ToString());
            }
            return liveHMI;
        }

        public List<tbllivehmiscreen> GetList3HMIDetails(string woNo, string partNo, string opNo)
        {
            List<tbllivehmiscreen> liveHMI = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * from " + databaseName + ".[tbllivehmiscreen] where  HMIID IN ( SELECT HMIID from " + databaseName + ".[tbllivehmiscreen] where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + opNo + "')";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                //MessageBox.Show(ex.ToString());
            }
            return liveHMI;
        }

        public List<tblhmiscreen> GetListH3HMIDetails(string woNo, string partNo, string opNo)
        {
            List<tblhmiscreen> liveHMI = new List<tblhmiscreen>();
            try
            {
                Repository<tblhmiscreen> lista = new Repository<tblhmiscreen>();

                string qry = "SELECT * from " + databaseName + ".[tblhmiscreen] where  HMIID IN ( SELECT HMIID from " + databaseName + ".[tbllivehmiscreen] where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + opNo + "')";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                //MessageBox.Show(ex.ToString());
            }
            return liveHMI;
        }

        public List<tblhmiscreen> GetLive1HScreenDetails(string woNo, string partNo, string InnerOpNo)
        {
            List<tblhmiscreen> liveHMI = new List<tblhmiscreen>();
            try
            {
                Repository<tblhmiscreen> lista = new Repository<tblhmiscreen>();

                string qry = "SELECT TOP 1 * from " + databaseName + ".[tblhmiscreen] where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by HMIID";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                //MessageBox.Show(ex.ToString());
            }
            return liveHMI;
        }

        public List<tbllivehmiscreen> GetLive1HMIScreenDetails(string woNo, string partNo, string InnerOpNo)
        {
            List<tbllivehmiscreen> liveHMI = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT TOP 1 * from " + databaseName + ".[tbllivehmiscreen] where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by HMIID";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                //MessageBox.Show(ex.ToString());
            }
            return liveHMI;
        }

        //Added by Ashok
        //public List<tbllivehmiscreen> GetLive1HMIScreenDetailsforStart(string woNo, string partNo, string InnerOpNo)
        //{
        //    List<tbllivehmiscreen> liveHMI = new List<tbllivehmiscreen>();
        //    try
        //    {
        //        Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

        //        string qry = "SELECT TOP 1 * from "+databaseName+".[tbllivehmiscreen] where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' and isWorkInProgress=2 order by HMIID";
        //        liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
        //    }
        //    catch (Exception ex)
        //    {
        //        IntoFile(ex.ToString());
        //        //MessageBox.Show(ex.ToString());
        //    }
        //    return liveHMI;
        //}

        public tbllivehmiscreen GetLive3HMIDetails(int HMIID, string Work_Order_No, string PartNo, string OperationNo)
        {
            tbllivehmiscreen LiveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * from " + databaseName + ".[tbllivehmiscreen] WHERE HMIID !=" + HMIID + " and Work_Order_No = '" + Work_Order_No + "' and PartNo ='" + PartNo + "' and OperationNo ='" + OperationNo + "' order by PEStartTime desc";
                LiveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return LiveHMI;
        }

        public List<tbllivehmiscreen> GetList4HMIDetails(int macID)
        {
            List<tbllivehmiscreen> liveHMI = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * from " + databaseName + ".[tbllivehmiscreen] where isWorkInProgress = 0 and HMIID IN (SELECT HMIID from " + databaseName + ".[tbllivehmiscreen] where MachineID = " + macID + "  order by HMIID desc )";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public tblmachinedetail GetMacDetails(int macid)
        {
            tblmachinedetail machinelist = new tblmachinedetail();
            try
            {
                Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();

                string qry = "SELECT MachineName FROM " + databaseName + ".[tblmachinedetails] WHERE MachineID =" + macid + "";
                machinelist = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return machinelist;
        }

        public tblmachinedetail GetMac1Details(string id)
        {
            tblmachinedetail machinelist = new tblmachinedetail();
            try
            {
                Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();

                string qry = "SELECT * FROM " + databaseName + ".[tblmachinedetails] WHERE MachineName ='" + id + "'";
                machinelist = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return machinelist;
        }

        public tblmachinedetail GetMac2Details(string id)
        {
            tblmachinedetail machinelist = new tblmachinedetail();
            try
            {
                Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();

                string qry = "SELECT * FROM " + databaseName + ".[tblmachinedetails] WHERE MachineName ='" + id + "' and IsDeleted = 0";
                machinelist = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return machinelist;
        }

        public List<tblddl> Getddl1Details(string woNo, string partNo, string opNo)
        {
            List<tblddl> ddl = new List<tblddl>();
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();

                string qry = "SELECT * from " + databaseName + ".[tblddl] where WorkOrder = '" + woNo + "' and MaterialDesc = '" + partNo + "'  and OperationNo != '" + opNo + "' and IsCompleted = 0 order by OperationNo";
                ddl = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ddl;
        }
        public List<tblddl> Getddl1Details1(string woNo)
        {
            List<tblddl> ddl = new List<tblddl>();
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();

                string qry = "SELECT DISTINCT OperationNo from tblddl where WorkOrder = '" + woNo + "' and IsDeleted = 0";
                ddl = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ddl;
        }

        public tbllivehmiscreen GetListH1MIDetails(string WONo, string Part, string opNo, int machineID)
        {
            tbllivehmiscreen liveHMI = new tbllivehmiscreen();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE Work_Order_No ='" + WONo + "' and PartNo='" + Part + "' and OperationNo ='" + opNo + "' and isWorkInProgress = 2 and MachineID = " + machineID + "";
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public int UpdateLiveHMIScreen1Detail(int HMIID, int IsHold)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set IsHold = " + IsHold + " WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public tblmanuallossofentry GetManualLossofEntryDetails(int HMIID)
        {
            tblmanuallossofentry lossofEntry = new tblmanuallossofentry();
            try
            {
                Repository<tblmanuallossofentry> lista = new Repository<tblmanuallossofentry>();

                string qry = "SELECT * FROM " + databaseName + ".[tblmanuallossofentry] WHERE HMIID =" + HMIID + " order by StartDateTime desc";
                lossofEntry = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossofEntry;
        }

        public int UpdateManualLossofEntryDetails(int HMIID, int EndHMIID, DateTime EndDateTime)
        {
            int res = 0;
            try
            {
                Repository<tblmanuallossofentry> lista = new Repository<tblmanuallossofentry>();

                string qry = "UPDATE " + databaseName + ".[tblmanuallossofentry] set EndHMIID = " + EndHMIID + ", EndDateTime ='" + EndDateTime + "' WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLivH2MIDetails(int HMIID, DateTime Date, int isWorkOrder, int ProcessQty)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set Date ='" + Date + "',isWorkOrder =" + isWorkOrder + " ,ProcessQty =" + ProcessQty + " WHERE HMIID =" + HMIID;
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public List<tbllivehmiscreen> GetLiveHMIDetailsList(string woNo, string partNo, string opNo, int MachineId)
        {
            List<tbllivehmiscreen> liveHMI = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * from " + databaseName + ".[tbllivehmiscreen] where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' and MachineID = " + MachineId + " order by HMIID";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public List<tblhmiscreen> GetHisHMIDetailsList(string woNo, string partNo, string opNo, int MachineId)
        {
            List<tblhmiscreen> liveHMI = new List<tblhmiscreen>();
            try
            {
                Repository<tblhmiscreen> lista = new Repository<tblhmiscreen>();

                string qry = "SELECT * from " + databaseName + ".[tblhmiscreen] where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' and MachineID = " + MachineId + " order by HMIID";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public int UpdateLiv2HMIDetails(int HMIID, DateTime Date, int Status, int isWorkOrder)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set Date='" + Date + "',isUpdate = 1 ,Status=" + Status + ",isWorkOrder=" + isWorkOrder + " WHERE HMIID =" + HMIID;
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

            }
            return res;
        }

        public int UpdateLiveHMIDets(int HMIID, int Delivered_Qty)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set Delivered_Qty =" + Delivered_Qty + " WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLiveHMIScreeDetails(int HMIID, int Delivered_Qty, string SplitWO, int isWorkInProgress, int Status, DateTime Time)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set Delivered_Qty = " + Delivered_Qty + ",SplitWO ='" + SplitWO + "' ,isWorkInProgress=" + isWorkInProgress + " ,Status=" + Status + " ,Time ='" + Time + "' WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public List<tbllivehmiscreen> GetLiveHMIDetailsList1(int hmiid)
        {
            List<tbllivehmiscreen> liveHMI = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE HMIID =" + hmiid + "";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public List<tbllivehmiscreen> GetListHMIScreeDetails(int MachineID)
        {
            List<tbllivehmiscreen> LiveHMI = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * from " + databaseName + ".[tbllivehmiscreen] WHERE MachineID=" + MachineID + " and Status = 0";
                LiveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return LiveHMI;
        }

        public tblholdcode GetHoldLDetails(int HoldCodeID)
        {
            tblholdcode liveHMI = new tblholdcode();
            try
            {
                Repository<tblholdcode> lista = new Repository<tblholdcode>();

                string qry = "SELECT * FROM " + databaseName + ".[tblholdcodes] WHERE HoldCodeID =" + HoldCodeID + "";
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public List<tblholdcode> GetHoldDetails(int HoldCodesLevel)
        {
            List<tblholdcode> liveHMI = new List<tblholdcode>();
            try
            {
                Repository<tblholdcode> lista = new Repository<tblholdcode>();

                string qry = "SELECT * FROM " + databaseName + ".[tblholdcodes] WHERE IsDeleted = 0 and HoldCodesLevel =" + HoldCodesLevel + " and HoldCodesLevel1ID IS NULL and HoldCodesLevel2ID IS NULL";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public List<tblholdcode> GetHoldCodeDetails(int HoldCodesLevel1ID)
        {
            List<tblholdcode> liveHMI = new List<tblholdcode>();
            try
            {
                Repository<tblholdcode> lista = new Repository<tblholdcode>();

                string qry = "SELECT * FROM " + databaseName + ".[tblholdcodes] WHERE IsDeleted = 0 and HoldCodesLevel1ID =" + HoldCodesLevel1ID + " and HoldCodesLevel = 2 and HoldCodesLevel2ID IS NULL";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public List<tblholdcode> GetHoldCodeDetails1(int HoldCodesLevel2ID)
        {
            List<tblholdcode> liveHMI = new List<tblholdcode>();
            try
            {
                Repository<tblholdcode> lista = new Repository<tblholdcode>();

                string qry = "SELECT * FROM " + databaseName + ".[tblholdcodes] WHERE IsDeleted = 0 and HoldCodesLevel2ID =" + HoldCodesLevel2ID + " and HoldCodesLevel = 3";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public string GetHoldCodeDetails2(int HoldCodeID)
        {
            string its = "";
            tblholdcode liveHMI = new tblholdcode();
            try
            {
                Repository<tblholdcode> lista = new Repository<tblholdcode>();

                string qry = "SELECT HoldCode FROM " + databaseName + ".[tblholdcodes] WHERE HoldCodeID =" + HoldCodeID + "";
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
                its = liveHMI.HoldCode;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return its;
        }


        public tblholdcode GetHoldCodeDetailsList(int HoldCodeID)
        {
            string its = "";
            tblholdcode liveHMI = new tblholdcode();
            try
            {
                Repository<tblholdcode> lista = new Repository<tblholdcode>();

                string qry = "SELECT * FROM " + databaseName + ".[tblholdcodes] WHERE HoldCodeID =" + HoldCodeID + "";
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);

            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public List<tblholdcode> GetHoldCodeDetails3(int HoldCodesLevel1ID)
        {
            List<tblholdcode> liveHMI = new List<tblholdcode>();
            try
            {
                Repository<tblholdcode> lista = new Repository<tblholdcode>();

                string qry = "SELECT * FROM " + databaseName + ".[tblholdcodes] WHERE IsDeleted = 0 and HoldCodesLevel1ID =" + HoldCodesLevel1ID + " and HoldCodesLevel2ID IS NULL";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public List<tblholdcode> GetHoldCodeDetails4()
        {
            List<tblholdcode> liveHMI = new List<tblholdcode>();
            try
            {
                Repository<tblholdcode> lista = new Repository<tblholdcode>();

                string qry = "SELECT * FROM " + databaseName + ".[tblholdcodes] WHERE IsDeleted = 0 and HoldCodesLevel = 1";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }

        public int UpdateLiveHMIScreenDets(int HMIID, int IsHold, int isWorkInProgress, DateTime Time, int Status)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "update " + databaseName + ".[tbllivehmiscreen] set IsHold = " + IsHold + " , isWorkInProgress =" + isWorkInProgress + " , Time ='" + Time + "' , Status = " + Status + "WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

            }
            return res;
        }

        public int InsertManualHMIDetails(int MessageCodeID, DateTime StartDateTime, string CorrectedDate, int MachineID, string Shift, string MessageDesc, string MessageCode, int HMIID, string PartNo, int OpNo, string WONo)
        {
            int res = 0;
            try
            {
                Repository<tbllivemanuallossofentry> lista = new Repository<tbllivemanuallossofentry>();

                string qry = "INSERT INTO " + databaseName + ".[tbllivemanuallossofentry](MessageCodeID,StartDateTime,CorrectedDate,MachineID,Shift,MessageDesc,MessageCode,HMIID,PartNo,OpNo,WONo) VALUES(" + MessageCodeID + ",'" + StartDateTime + "','" + CorrectedDate + "'," + MachineID + ",'" + Shift + "','" + MessageDesc + "','" + MessageCode + "'," + HMIID + ",'" + PartNo + "','" + OpNo + "'," + WONo + ")";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLiveHMIScreen1Dets(int HMIID, int isWorkInProgress, DateTime Time, int Status)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "update " + databaseName + ".[tbllivehmiscreen] set  isWorkInProgress =" + isWorkInProgress + ", Time ='" + Time + "', Status = " + Status + "WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public List<tblbreakdown> GetBreakDownDetails5(int MachineID, string CorrectedDate)
        {
            List<tblbreakdown> lossCode = new List<tblbreakdown>();
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();

                string qry = "SELECT * FROM " + databaseName + ".[tblbreakdown] WHERE MachineID =" + MachineID + " and CorrectedDate='" + CorrectedDate + "' and EndTime IS NULL order by BreakdownID desc";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public int UpdateBreakDownDetails1(int BreakdownID, DateTime EndTime)
        {
            int res = 0;
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();

                string qry = "UPDATE " + databaseName + ".[tblbreakdown] set EndTime='" + EndTime + "' WHERE BreakdownID=" + BreakdownID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public List<tblbreakdown> GetBreakDownDetails6(int MachineID)
        {
            List<tblbreakdown> lossCode = new List<tblbreakdown>();
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();

                string qry = "SELECT * FROM " + databaseName + ".[tblbreakdown] WHERE MachineID =" + MachineID + " and EndTime IS NULL and MessageCode = 'PM'";
                return lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public List<tbllossescode> GetLossCodesDetails()
        {
            List<tbllossescode> lossCode = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE IsDeleted = 0  and MessageType = 'PM'";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public tbllossescode GetLossCode1Details(string LossCode)
        {
            tbllossescode lossCode = new tbllossescode();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE LossCode =" + LossCode + "";
                lossCode = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public List<tbllivemode> GetLiveModeListDetails(int machineid)
        {
            List<tbllivemode> lossofEntry = new List<tbllivemode>();
            try
            {
                Repository<tbllivemode> lista = new Repository<tbllivemode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivemode] WHERE MachineID =" + machineid + " and IsCompleted = 0 and IsDeleted = 0 order by StartTime desc";
                lossofEntry = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossofEntry;
        }

        public int UpdateBreakDownDetails2(int BreakdownID, DateTime EndTime, string CorrectedDate)
        {
            int res = 0;
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();

                string qry = "UPDATE " + databaseName + ".[tblbreakdown] set EndTime=" + EndTime + " ,CorrectedDate ='" + CorrectedDate + "' ,DoneWithRow = 1 WHERE BreakdownID=" + BreakdownID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public tblbreakdown GetBreakDownDetails7(int MachineID)
        {
            tblbreakdown lossCode = new tblbreakdown();
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();

                string qry = "SELECT TOP 1 * FROM " + databaseName + ".[tblbreakdown] WHERE MachineID =" + MachineID + " and DoneWithRow= 0 order by BreakdownID";
                lossCode = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public List<tbllossescode> GetLossCodeDetail10(int breakdowncode)
        {
            List<tbllossescode> lossCode = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT LossCode FROM " + databaseName + ".[tbllossescodes] WHERE LossCodeID =" + breakdowncode + "";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public List<tblbreakdown> GetBreakDownDetailsList(int MachineID)
        {
            List<tblbreakdown> lossCode = new List<tblbreakdown>();
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();

                string qry = "SELECT TOP 1 * FROM " + databaseName + ".[tblbreakdown] WHERE MachineID =" + MachineID + " and DoneWithRow= 0 order by BreakdownID";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public int UpdateBreakDownDetails3(int BreakdownID, DateTime EndTime)
        {
            int res = 0;
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();

                string qry = "UPDATE " + databaseName + ".[tblbreakdown] set EndTime='" + EndTime + "' ,DoneWithRow = 1 WHERE BreakdownID=" + BreakdownID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }


        //Monika
        public tbllivelossofentry GettbllivelossofentriesDetails(string CorrectedDate, int Machineid)
        {
            tbllivelossofentry daytimings = new tbllivelossofentry();
            Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivelossofentry WHERE CorrectedDate ='" + CorrectedDate + "' and  MachineID = " + Machineid + " order by EntryTime desc";
                daytimings = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return daytimings;
        }
        public tbllossescode GettbllossDetails(string BreakDownCode)
        {
            tbllossescode daytimings = new tbllossescode();
            Repository<tbllossescode> lista = new Repository<tbllossescode>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllossescodes WHERE LossCode = '" + BreakDownCode + "'";
                daytimings = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return daytimings;
        }
        public tbllivehmiscreen GetLiveDetails(string CorrectedDate, int opid, int MachineID)
        {
            tbllivehmiscreen daytimings = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivehmiscreen WHERE CorrectedDate = '" + CorrectedDate + "' and OperatiorID = " + opid + " and Status = 0 and MachineID = " + MachineID + "";
                daytimings = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return daytimings;
        }
        public List<string> GetMacDetails1(int macid)
        {
            List<string> ivs = new List<string>();
            List<tblmachinedetail> machinelist = new List<tblmachinedetail>();
            try
            {
                Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();

                string qry = "SELECT MachineName FROM " + databaseName + ".[tblmachinedetails] WHERE IsDeleted = 0 and MachineID =" + macid + "";
                var its = lista.GetList(qry, _connectionFactory.GetConnection);
                foreach (var item in its)
                {
                    ivs.Add(item.MachineName);
                }
                return ivs;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ivs;
        }
        public tbllivehmiscreen GetLiveHMIDetails(string CorrectedDate, int opid, int MachineID)
        {
            tbllivehmiscreen daytimings = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivehmiscreen WHERE CorrectedDate = '" + CorrectedDate + "' and OperatiorID = " + opid + " and Status = 0 and MachineID = " + MachineID + " order by HMIID desc";
                daytimings = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return daytimings;
        }

        public int UpdateLiveHMIDetails(int id)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen] SET isUpdate = 1 WHERE HMIID = " + id + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateLiveHMIDetails4(int id)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET isUpdate = 0 WHERE HMIID = " + id + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateLiveHMIDetails1(string Shift, int id)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET Shift = '" + Shift + "',isUpdate = 1 WHERE HMIID = " + id + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateLiveHMIDetails12(string Shift, int id, DateTime PEStartTime, string opdet)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET Shift = '" + Shift + "',PEStartTime = '" + PEStartTime + "',isUpdate = 1 ,OperatorDet ='" + opdet + "'WHERE HMIID = " + id + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateLiveHMIDetails1(int id)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET isUpdate = 0  WHERE HMIID = " + id + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateLiveHMIDetails2(DateTime time, int id)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET Time = '" + time + "' WHERE HMIID = " + id + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public tblmachinedetail GetMachineDet(int MachineID)
        {
            tblmachinedetail Machine = new tblmachinedetail();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblmachinedetails WHERE MachineID = " + MachineID + "";
                Machine = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return Machine;
        }
        public string GetPlantDetails(int PlantID)
        {
            string item = "";
            tblplant Plant = new tblplant();
            Repository<tblplant> lista = new Repository<tblplant>();
            try
            {
                string query = "SELECT PlantName from " + databaseName + ".tblplant WHERE PlantID = " + PlantID + "";
                Plant = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                item = Plant.PlantName;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return item;
        }
        public string GetShopDetails(int ShopID)
        {
            string item = "";
            tblshop Shop = new tblshop();
            Repository<tblshop> lista = new Repository<tblshop>();
            try
            {
                string query = "SELECT ShopName from " + databaseName + ".tblshop WHERE PlantID = " + ShopID + "";
                Shop = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                item = Shop.ShopName;

            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                return item;
            }
            return item;
        }
        public string GetCellDetails(int CellID)
        {
            string item = "";
            tblcell Cell = new tblcell();
            Repository<tblcell> lista = new Repository<tblcell>();
            try
            {
                string query = "SELECT CellName from " + databaseName + ".tblcell WHERE PlantID = " + CellID + "";
                Cell = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                item = Cell.CellName;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                return item;
            }
            return item;
        }
        public tbllivehmiscreen GetHMIDetails34(int hmiid)
        {
            tbllivehmiscreen hmidet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivehmiscreen WHERE HMIID = " + hmiid + "";
                hmidet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return hmidet;
        }
        public tbllivehmiscreen GetliveHMIScreenDetails(string WONo, string OpNo, int MachineID)
        {
            tbllivehmiscreen hmidet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivehmiscreen WHERE Work_Order_No = '" + WONo + "' and OperationNo = '" + OpNo + "' and MachineID = " + MachineID + " order by PEStartTime desc";
                hmidet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return hmidet;
        }
        public tbllivehmiscreen GetliveHMIScreenDetails(string WONo, string OpNo)
        {
            tbllivehmiscreen hmidet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivehmiscreen WHERE Work_Order_No = '" + WONo + "' and OperationNo = '" + OpNo + "' order by PEStartTime desc";
                hmidet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return hmidet;
        }
        public tblddl GetddlDetails(string WONo, string OpNo)
        {
            tblddl ddldet = new tblddl();
            Repository<tblddl> lista = new Repository<tblddl>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblddl WHERE WorkOrder = '" + WONo + "' and OperationNo = '" + OpNo + "' and IsCompleted = 0";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public tblmachinedetail GetmacDetails(string ddlWorkCenter)
        {
            tblmachinedetail ddldet = new tblmachinedetail();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblmachinedetails WHERE MachineName = '" + ddlWorkCenter + "' and IsDeleted = 0";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ddldet;
        }
        public tblmachinedetail GetmacDetails(int MachineID)
        {
            tblmachinedetail ddldet = new tblmachinedetail();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblmachinedetails WHERE MachineID = " + MachineID + " and IsDeleted = 0";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ddldet;
        }
        public int UpdateLiveHMIScreenDetails(string WorkCenter, int HMIID, int isHold, int tragetqty, string Type, int MachineID, string OpNo, string MaterialDesc, string Project, string WONo)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET DDLWokrCentre = '" + WorkCenter + "',DoneWithRow = 0,IsHold=" + isHold + ",IsMultiWO = 0,isUpdate =0,isWorkInProgress = 2,isWorkOrder = 0,Target_Qty = " + tragetqty + ",Prod_FAI = '" + Type + "',MachineID =" + MachineID + ",OperationNo ='" + OpNo + "',PartNo ='" + MaterialDesc + "',ProcessQty =0,Project ='" + Project + "',Status =0,Work_Order_No='" + WONo + "' WHERE HMIID = " + HMIID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateHMIDetails(int cjtextbox7, int cjtextbox8, int HMIID)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET Rej_Qty = " + cjtextbox7 + ",Delivered_Qty =" + cjtextbox8 + ",Status=3,isWorkInProgress = 0 WHERE HMIID = " + HMIID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateHMIDetails1(int HMIID)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET isWorkOrder = 1 WHERE HMIID = " + HMIID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public tblddl GetddlDetails1(string WONo, string OpNo)
        {
            tblddl ddldet = new tblddl();
            Repository<tblddl> lista = new Repository<tblddl>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblddl WHERE WorkOrder = '" + WONo + "' and OperationNo = '" + OpNo + "' and IsCompleted = 1";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public int UpdateLiveHMIScreenDetails1(string DDLWokrCentre, int HMIID, int isHold, int Target_Qty, string Prod_FAI, int MachineID, string OpNo, string PartNo, string Project, string WONo)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET DDLWokrCentre = '" + DDLWokrCentre + "',DoneWithRow = 0,IsHold=" + isHold + ",IsMultiWO = 0,isUpdate =0,isWorkInProgress = 2,isWorkOrder = 0,Target_Qty = " + Target_Qty + ",Prod_FAI ='" + Prod_FAI + "',MachineID =" + MachineID + ",OperationNo ='" + OpNo + "',PartNo ='" + PartNo + "',ProcessQty =0,Project ='" + Project + "',Status =0,Work_Order_No='" + WONo + "' WHERE HMIID = " + HMIID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateLiveHMIScreenDetails2(int HMIID, string OpNo, string WONo)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET OperationNo ='" + OpNo + "',PartNo =null,Work_Order_No='" + WONo + "' WHERE HMIID = " + HMIID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public tblddl GetddlDetails(int DDLID)
        {
            tblddl ddldet = new tblddl();
            Repository<tblddl> lista = new Repository<tblddl>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblddl WHERE DDLID = " + DDLID + " and IsCompleted = 0";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public List<tbllivehmiscreen> GethmiList(string WONo, string Part, string Operation)
        {
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            List<tbllivehmiscreen> det = new List<tbllivehmiscreen>();
            try
            {
                string query = "SELECT TOP 1 * From " + databaseName + ".tbllivehmiscreen WHERE Work_Order_No='" + WONo + "' and OperationNo ='" + Operation + "' and PartNo ='" + Part + "' and isWorkInProgress !=2";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tbllivehmiscreen> GettbllivehmiscreensList(int id, string CorrectedDate)
        {
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            List<tbllivehmiscreen> det = new List<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tbllivehmiscreen HMI " + " left outer join " + databaseName + ".tblmachinedetail mac on HMI.MachineID = mac.MachineID WHERE MachineID = " + id + " and CorrectedDate ='" + CorrectedDate + "' and isWorkInProgress = 0";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tbllivedailyprodstatu> GettbllivedailyprodstatusList(string CorrectedDate, int Machineid)
        {
            Repository<tbllivedailyprodstatu> lista = new Repository<tbllivedailyprodstatu>();
            List<tbllivedailyprodstatu> det = new List<tbllivedailyprodstatu>();
            try
            {
                string query = "SELECT TOP 1 * From " + databaseName + ".tbllivedailyprodstatu WHERE CorrectedDate ='" + CorrectedDate + "' and  MachineID = " + Machineid + " order by StartTime desc";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tbldailyprodstatu> GettbllivedailyprodstatusList1(int Machineid)
        {
            Repository<tbldailyprodstatu> lista = new Repository<tbldailyprodstatu>();
            List<tbldailyprodstatu> det = new List<tbldailyprodstatu>();
            try
            {
                string query = "SELECT TOP 1 * From " + databaseName + ".tbldailyprodstatu WHERE MachineID = " + Machineid + " order by StartTime desc";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tbldailyprodstatu> GettbllivedailyprodstatusList2(string CorrectedDate, DateTime? StartTime, DateTime? EndTime, int MachineID)
        {
            Repository<tbldailyprodstatu> lista = new Repository<tbldailyprodstatu>();
            List<tbldailyprodstatu> det = new List<tbldailyprodstatu>();
            try
            {
                string query = "SELECT TOP 1 * From " + databaseName + ".tbldailyprodstatu WHERE CorrectedDate = '" + CorrectedDate + "' and StartTime >= '" + StartTime + "' and EndTime <= '" + EndTime + "' and MachineID = " + MachineID + "";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public tbllivehmiscreen GetLiveHMIScreenDet(int machineId)
        {
            tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivehmiscreen WHERE MachineID =" + machineId + " and isWorkInProgress =2 order by HMIID desc";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public int deleteMultiWOSlectionDetails2(int HMIID)
        {
            int res = 0;
            try
            {
                Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();
                string query = "delete from " + databaseName + ".tbllivemultiwoselection WHERE HMIID = " + HMIID + " and IsSubmit = 0";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateLiveHMIScreenDetails3(int Hmiid, string OperationNo, string MaterialDesc, string Project, int Target_Qty, string WorkOrder)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET Target_Qty = " + Target_Qty + ",OperationNo ='" + OperationNo + "',PartNo ='" + MaterialDesc + "',IsMultiWO =0,Project ='" + Project + "',Work_Order_No='" + WorkOrder + "' WHERE HMIID = " + Hmiid + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateprodsttausDetails(int id)
        {
            int res = 0;
            try
            {
                Repository<tbldailyprodstatu> lista = new Repository<tbldailyprodstatu>();
                string query = "Update  " + databaseName + ".tbldailyprodstatu SET ColorCode = 'yellow' WHERE ID = " + id + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public List<tbllivemultiwoselection> GetMultiWOSelectionList(string WONo, string Part, string Operation)
        {
            Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();
            List<tbllivemultiwoselection> det = new List<tbllivemultiwoselection>();
            try
            {
                string query = "SELECT TOP 1 mul.* From " + databaseName + ".tbllivemultiwoselection mul " + " left outer join " + databaseName + ".tbllivehmiscreen HMI on mul.HMIID = HMI.HMIID WHERE mul.WorkOrder='" + WONo + "' and mul.OperationNo ='" + Operation + "' and mul.PartNo ='" + Part + "' and HMI.isWorkInProgress !=2 order by HMI.Time";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tbllivehmiscreen> GetHMIList(string WONo, string Part, string Operation)
        {
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            List<tbllivehmiscreen> det = new List<tbllivehmiscreen>();
            try
            {
                string query = "SELECT TOP 1 * From " + databaseName + ".tbllivehmiscreen WHERE Work_Order_No='" + WONo + "' and OperationNo ='" + Operation + "' and PartNo ='" + Part + "' and isWorkInProgress !=2 order by Time desc";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public tbllivehmiscreen GetLiveHMIScreenDetForFinish(string WONo, string Operation)
        {
            tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tbllivehmiscreen WHERE Work_Order_No='" + WONo + "' and OperationNo ='" + Operation + "' and isWorkInProgress !=2 order by Time desc";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public int InsertmultiwoselectionDetails(string WorkCenter, string Operation, string Part, int TargetQty, string WONo, int ProcessQty, int Hmiid, DateTime date)
        {
            int res = 0;
            try
            {
                Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();
                string query = "INSERT INTO  " + databaseName + ".tbllivemultiwoselection(DDLWorkCentre,OperationNo,PartNo,SplitWO,TargetQty,WorkOrder,ProcessQty,HMIID,IsCompleted,CreatedOn)" +
                                        "VALUES('" + WorkCenter + "','" + Operation + "','" + Part + "',0," + TargetQty + ",'" + WONo + "'," + ProcessQty + "," + Hmiid + ",0,'" + date + "')";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return res;
        }
        public int InsertBreakdownDetails(string WorkCenter, string Operation, string Part, int TargetQty, string WONo, int ProcessQty, int Hmiid, DateTime date)
        {
            int res = 0;
            try
            {
                Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();
                string query = "INSERT INTO  " + databaseName + ".tbllivemultiwoselection(DDLWorkCentre,OperationNo,PartNo,SplitWO,TargetQty,WorkOrder,ProcessQty,HMIID,IsCompleted,CreatedOn)" +
                                        "VALUES('" + WorkCenter + "','" + Operation + "','" + Part + "',0," + TargetQty + ",'" + WONo + "'," + ProcessQty + "," + Hmiid + ",0,'" + date + "')";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return res;
        }
        public int InserttbllivemodedbDetails(string CorrectedDate1, int InsertedBy, DateTime InsertedOn, int MachineID, DateTime StartTime1, int shift)
        {
            int res = 0;
            try
            {
                Repository<tbllivemode> lista = new Repository<tbllivemode>();
                string query = "INSERT INTO  " + databaseName + ".tbllivemode(MachineID,CorrectedDate,InsertedBy,InsertedOn,StartTime,ColorCode,IsCompleted,IsDeleted,MacMode,IsShiftEnd)" +
                                        "VALUES(" + MachineID + ",'" + CorrectedDate1 + "'," + InsertedBy + ",'" + InsertedOn + "','" + StartTime1 + "','YELLOW',0,0,'IDLE'," + shift + ")";
                //IntoFile("Inset query in maintainance:" + query);
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return res;
        }
        public int InserttbllivemodedbDetails1(int MachineID, string CorrectedDate1, DateTime InsertedOn, DateTime StartTime1)
        {
            int res = 0;
            try
            {
                Repository<tbllivemode> lista = new Repository<tbllivemode>();
                string query = "INSERT INTO  " + databaseName + ".tbllivemode(MachineID,CorrectedDate,InsertedBy,InsertedOn,StartTime,ColorCode,IsCompleted,IsDeleted,MacMode)" +
                                        "VALUES(" + MachineID + ",'" + CorrectedDate1 + "',1,'" + InsertedOn + "','" + StartTime1 + "','GREEN',0,0,'SETUP')";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return res;
        }
        public int InserttbllivemodedbDetails(int MachineID, string CorrectedDate, int InsertedBy, DateTime InsertedOn, DateTime StartTime1)
        {
            int res = 0;
            try
            {
                Repository<tbllivemode> lista = new Repository<tbllivemode>();
                string query = "INSERT INTO  " + databaseName + ".tbllivemode(MachineID,CorrectedDate,InsertedBy,InsertedOn,StartTime,ColorCode,IsCompleted,IsDeleted,MacMode)" +
                                        "VALUES(" + MachineID + ",'" + CorrectedDate + "'," + InsertedBy + ",'" + InsertedOn + "','" + StartTime1 + "','RED',0,0,'MNT')";
                //IntoFile("Inset query in maintainance:" + query);
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int InserttbllivemodedbDetails2(int MachineID, string CorrectedDate, int InsertedBy, DateTime InsertedOn, DateTime StartTime1, int shift)
        {
            int res = 0;
            try
            {
                Repository<tbllivemode> lista = new Repository<tbllivemode>();
                string query = "INSERT INTO  " + databaseName + ".tbllivemode(MachineID,CorrectedDate,InsertedBy,InsertedOn,StartTime,ColorCode,IsCompleted,IsDeleted,IsShiftEnd,MacMode)" +
                                        "VALUES(" + MachineID + ",'" + CorrectedDate + "'," + InsertedBy + ",'" + InsertedOn + "','" + StartTime1 + "','RED',0,0," + shift + ",'MNT')";
                //IntoFile("Inset query in maintainance:" + query);
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public List<tbllivehmiscreen> GetLiveHMIDetail(int MachineID, string CorrectedDate)
        {
            List<tbllivehmiscreen> liveHMI = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivehmiscreen] WHERE Status = 0 and CorrectedDate ='" + CorrectedDate + "' and MachineID = " + MachineID + " ";
                liveHMI = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }
        public int InsertlossofentryDetails(string CorrectedDate1, DateTime Time, int MachineID, string Shift, string MessageCode)
        {
            int res = 0;
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();
                string query = "INSERT INTO  " + databaseName + ".tblbreakdown(CorrectedDate,StartTime,MachineID,Shift,MessageCode,BreakDownCode,DoneWithRow,MessageDesc)" +
                                        "VALUES('" + CorrectedDate1 + "','" + Time + "'," + MachineID + ",'" + Shift + "','" + MessageCode + "',14,0,'PM')";

                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return res;
        }
        public int InsertLIVEHMIDetails(string project, string PorF, string partno, string wono, string opno, int target, int machineID, string CorrectedDate, DateTime pestarttime, string Shiftgen, int Opgid)
        {
            int res = 0;
            try
            {
                Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();
                string query = "INSERT INTO  " + databaseName + ".tbllivemultiwoselection(Project,Prod_FAI,PartNo,Work_Order_No,OperationNo,Target_Qty,MachineID,CorrectedDate,PEStartTime,Shift,Status,isWorkInProgress,OperatiorID)" + "VALUES('" + project + "','" + PorF + "','" + partno + "','" + wono + "','" + opno + "'," + target + "," + machineID + ",'" + CorrectedDate + "','" + pestarttime + "','" + Shiftgen + "'," + Opgid + ")";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return res;
        }
        public int UpdateLiveHMIScreenDetails4(string opt, string part, string MainProject, int TotalTargetQty, string MainWorkOrder, int TotalProcessQty, int HMIID, string ddlWorkCenter)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET OperationNo = '" + opt + "',PartNo ='" + part + "',Project ='" + MainProject + "',Target_Qty =" + TotalTargetQty + ",Work_Order_No ='" + MainWorkOrder + "',ProcessQty=" + TotalProcessQty + ",DDLWokrCentre='" + ddlWorkCenter + "' WHERE HMIID = " + HMIID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return res;
        }
        public int UpdatetblbreakdownDetails(string CorrectedDate, DateTime EndTime1, int id)
        {
            int res = 0;
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();
                string query = "Update  " + databaseName + ".tblbreakdown SET CorrectedDate = '" + CorrectedDate + "',EndTime ='" + EndTime1 + "',DoneWithRow =1 WHERE BreakdownID = " + id + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdatetblbreakdownDetails1(string CorrectedDate, DateTime? EndTime1, int id, int MachineID)
        {
            int res = 0;
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();
                string query = "Update  " + databaseName + ".tblbreakdown SET CorrectedDate = '" + CorrectedDate + "',EndTime ='" + EndTime1 + "',MachineID = " + MachineID + " WHERE BreakdownID = " + id + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return res;
        }

        public tblbreakdown GetBreakdownDet(int machineID)
        {
            tblbreakdown ddldet = new tblbreakdown();
            Repository<tblbreakdown> lista = new Repository<tblbreakdown>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblbreakdown WHERE MachineID =" + machineID + " and DoneWithRow =0 order by StartTime desc";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public tblbreakdown GetBreakdownDet1(int machineID)
        {
            tblbreakdown ddldet = new tblbreakdown();
            Repository<tblbreakdown> lista = new Repository<tblbreakdown>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblbreakdown WHERE MachineID =" + machineID + " and DoneWithRow =0 order by BreakdownID desc";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public tbllossescode GetLossDet(int? BreakDownCode)
        {
            tbllossescode ddldet = new tbllossescode();
            Repository<tbllossescode> lista = new Repository<tbllossescode>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllossescodes WHERE LossCodeID = " + BreakDownCode + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ddldet;
        }
        public tbllossescode GetLossDet2(int losscodeid)
        {
            tbllossescode ddldet = new tbllossescode();
            Repository<tbllossescode> lista = new Repository<tbllossescode>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllossescodes WHERE LossCodeID =" + losscodeid + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }

        public tbllivemultiwoselection GettbllivemultiwoselectionDet(int multiwoID)
        {
            tbllivemultiwoselection ddldet = new tbllivemultiwoselection();
            Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivemultiwoselection WHERE MultiWOID =" + multiwoID + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public int UpdateMultiwoselectionDetails(string SplitWO, int multiwoID)
        {
            int res = 0;
            try
            {
                Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();
                string query = "Update  " + databaseName + ".tbllivemultiwoselection SET SplitWO = '" + SplitWO + "' WHERE MultiWOID =" + multiwoID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public List<tbllivemultiwoselection> GetmultiplewoselectionList(int multiwoID)
        {
            Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();
            List<tbllivemultiwoselection> det = new List<tbllivemultiwoselection>();
            try
            {
                string query = "SELECT TOP 1 * From " + databaseName + ".tbllivemultiwoselection WHERE MultiWOID =" + multiwoID + "";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }

        public List<tbllivemode> GettbllivemodedbsList(int MachineID)
        {
            Repository<tbllivemode> lista = new Repository<tbllivemode>();
            List<tbllivemode> det = new List<tbllivemode>();
            try
            {
                string query = "SELECT * From " + databaseName + ".[tbllivemode] WHERE IsDeleted = 0 and MachineID = " + MachineID + " and IsCompleted = 0 order by StartTime desc";

                det = _connectionFactory.GetConnection.QueryAsync<tbllivemode>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public bool checkOpCancel(string workOrderNo, string operationNo)
        {
            bool check = false;
            tbl_PrevOperationCancel opCan = new tbl_PrevOperationCancel();
            try
            {
                Repository<tbl_PrevOperationCancel> lista = new Repository<tbl_PrevOperationCancel>();
                string query = "SELECT *  FROM [" + ConnectionFactory.DB + "].[dbo].[tbl_PrevOperationCancel] where ProductionOrder='" + workOrderNo + "' and Operation=" + operationNo + " and IsCancelled=2";
                opCan = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                if (opCan != null)
                {
                    check = true;
                }
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return check;
        }
        public List<tbllivemode> GettbllivemodedbsList1(int MachineID)
        {
            Repository<tbllivemode> lista = new Repository<tbllivemode>();
            List<tbllivemode> det = new List<tbllivemode>();
            try
            {
                string query = "SELECT * From " + databaseName + ".[tbllivemode] WHERE IsDeleted = 0 and MachineID = " + MachineID + " order by StartTime desc";
                det = _connectionFactory.GetConnection.QueryAsync<tbllivemode>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }


        public List<shift_master> GetShiftList(TimeSpan Tm)
        {
            Repository<shift_master> lista = new Repository<shift_master>();
            List<shift_master> det = new List<shift_master>();
            try
            {
                string query = "SELECT TOP 1 * From " + databaseName + ".shift_master WHERE StartTime <= '" + Tm + "' and EndTime >= '" + Tm + "'";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tbllivemultiwoselection> GettbllivemultiwoselectionsList(int HMIID)
        {
            Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();
            List<tbllivemultiwoselection> det = new List<tbllivemultiwoselection>();
            try
            {
                string query = "delete From " + databaseName + ".tbllivemultiwoselection WHERE HMIID = " + HMIID + " ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public tbllivehmiscreen GetLiveHMIDet(int HMIID)
        {
            tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivehmiscreen WHERE HMIID =" + HMIID + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public tblgenericworkentry GetgenericworkDetails(int MacID)
        {
            tblgenericworkentry liveHMI = new tblgenericworkentry();
            try
            {
                Repository<tblgenericworkentry> lista = new Repository<tblgenericworkentry>();

                string qry = "SELECT * FROM " + databaseName + ".[tblgenericworkentry] WHERE MachineID =" + MacID + " order by GWEntryID desc";
                liveHMI = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return liveHMI;
        }
        public int UpdatetblgenericworkentryDetails(int GWEntryID, DateTime EndDateTime)
        {
            int res = 0;
            try
            {
                Repository<tblgenericworkentry> lista = new Repository<tblgenericworkentry>();
                string query = "Update  " + databaseName + ".tblgenericworkentry SET DoneWithRow = 1, EndDateTime = '" + EndDateTime + "' WHERE GWEntryID =" + GWEntryID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateLiveHMIDetails(string SplitWO, int HMIID)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET SplitWO = '" + SplitWO + "' WHERE HMIID =" + HMIID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int InsertbookDetails(string BookName)
        {
            int res = 0;
            try
            {
                Repository<book> lista = new Repository<book>();

                string qry = "INSERT into " + databaseName + ".[books] (BookName) VALUES('" + BookName + "')";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int InsertgenericworkDetails(int HiddenID, string CorrectedDate, int DoneWithRow, int MachineID, string msgCode, string msgDesc, string Shift, DateTime Date)
        {
            int res = 0;
            try
            {
                Repository<tblgenericworkentry> lista = new Repository<tblgenericworkentry>();

                string qry = "INSERT INTO " + databaseName + ".[tblgenericworkentry](GWCodeID,CorrectedDate,DoneWithRow,MachineID,GWCode,GWCodeDesc,Shift,StartDateTime) VALUES(" + HiddenID + ",'" + CorrectedDate + "',0," + MachineID + ",'" + msgCode + "','" + msgDesc + "','" + Shift + "','" + Date + "')";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdatetblmodedataDetails(DateTime EndTime, int id)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivemode SET EndTime = '" + EndTime + "',IsCompleted = 1 WHERE ModeID =" + id + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return res;
        }
        public int UpdatetblmodedataDetails1(DateTime EndTime, int id)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivemode SET EndTime = '" + EndTime + "' WHERE ModeID =" + id + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateMultiwoselectionDetails1(int multiwoID, int DelQty, string SplitWO)
        {
            int res = 0;
            try
            {
                Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();
                string query = "Update  " + databaseName + ".tbllivemultiwoselection SET IsCompleted =0,DeliveredQty = " + DelQty + " SplitWO = '" + SplitWO + "' WHERE MultiWOID =" + multiwoID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return res;
        }
        public int UpdateHMIDetails1(int DelQty, int hmiID)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update " + databaseName + ".tbllivehmiscreen SET Delivered_Qty = " + DelQty + " WHERE MultiWOID =" + hmiID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                /// MessageBox.Show(ex.ToString());
            }
            return res;
        }
        public string GetMachineDet1(int macID)
        {
            string det = "";
            tblmachinedetail ddldet = new tblmachinedetail();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblmachinedetails WHERE IsDeleted = 0 and MachineID = " + macID + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                det = ddldet.MachineDisplayName;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public tblmachinedetail GetOneMachineDet(int macID)
        {
            tblmachinedetail ddldet = new tblmachinedetail();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblmachinedetails WHERE IsDeleted = 0 and MachineID = " + macID + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ddldet;
        }
        public List<tblmachinedetail> GetCellMachineList1(int cellid)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineName from " + databaseName + ".tblmachinedetails WHERE IsDeleted = 0 and CellID = " + cellid + " and ManualWCID is null and IPAddress!=''";
                det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tblbreakdown> GetBreakdownList(int id, string CorrectedDate)
        {
            Repository<tblbreakdown> lista = new Repository<tblbreakdown>();
            List<tblbreakdown> det = new List<tblbreakdown>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblbreakdown WHERE MachineID = " + id + " and CorrectedDate = '" + CorrectedDate + "' and EndTime IS NULL order by BreakdownID desc";
                det = _connectionFactory.GetConnection.QueryAsync<tblbreakdown>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tblmachinedetail> GetCellMachineList2(int cellid)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineName from " + databaseName + ".tblmachinedetails WHERE IsDeleted = 0 and CellID = " + cellid + "";
                det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                //return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tblmachinedetail> GetMachineList2(int MacId)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineName from " + databaseName + ".tblmachinedetails WHERE IsDeleted = 0 and MachineID = " + MacId + "";
                det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                //return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblmachinedetail> GetShopMachineList1(int cellid, int shopid)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineName from " + databaseName + ".tblmachinedetails WHERE ShopID = " + shopid + " and IsDeleted = 0 and CellID != " + cellid + " and !ManualWCID.HasValue";
                query = "SELECT MachineName from " + databaseName + ".tblmachinedetails WHERE ShopID = " + shopid + " and IsDeleted = 0 and CellID != " + cellid + " and ManualWCID IS NULL";
                query = "SELECT MachineName from " + databaseName + ".tblmachinedetails where ShopID = " + shopid + " and CellID != " + cellid + " and IsDeleted = 0  and ManualWCID is null and IPAddress !=''";
                det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                //return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }

        public List<tblmachinedetail> GetShopMachineListDLL(int cellid, int shopid)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineName from " + databaseName + ".tblmachinedetails WHERE ShopID = " + shopid + " and IsDeleted = 0 and CellID != " + cellid + " and !ManualWCID.HasValue";
                query = "SELECT MachineName from " + databaseName + ".tblmachinedetails WHERE ShopID = " + shopid + " and IsDeleted = 0 and CellID != " + cellid + " and ManualWCID IS NULL";
                query = "SELECT MachineName from " + databaseName + ".tblmachinedetails where ShopID = " + shopid + " and CellID != " + cellid + " and IsDeleted = 0  and ManualWCID is null and IPAddress =''";
                det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                //return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblmachinedetail> GetShopMachineList2(int cellid, int shopid)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineName from " + databaseName + ".tblmachinedetails WHERE ShopID = " + shopid + " and IsDeleted = 0 and CellID != " + cellid + "";
                det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                //return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblddl> GetDDLList1(string machineInvNo, string ExceptionDDLsArray)
        {
            Repository<tblddl> lista = new Repository<tblddl>();
            List<tblddl> det = new List<tblddl>();
            try
            {
                string query = "select * " + "from tblddl WHERE WorkCenter = '" + machineInvNo + "' AND IsCompleted = 0  AND DDLID NOT IN (" + ExceptionDDLsArray + ")" + " order by DaysAgeing = '', Convert(DaysAgeing , SIGNED INTEGER) asc ,FlagRushInd = '',FlagRushInd = 0 ,Convert(FlagRushInd , SIGNED INTEGER) asc  , MADDateInd = '' , MADDateInd asc , MADDate asc";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tblmachinedetail> GetMachineList1(int macID)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tblmachinedetails WHERE IsDeleted = 0 and MachineID = " + macID + "";
                det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                //return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tbllivemultiwoselection> GetmultiplewoselectionList1(int id)
        {
            Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();
            List<tbllivemultiwoselection> det = new List<tbllivemultiwoselection>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tbllivemultiwoselection WHERE HMIID =" + id + "";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tblddl> GetDDLList2(string machineInvNo)
        {
            Repository<tblddl> lista = new Repository<tblddl>();
            List<tblddl> det = new List<tblddl>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tblddl WHERE WorkCenter = '" + machineInvNo + "' and IsCompleted = 0";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tbllivehmiscreen> GetDDLList3(int machineId)
        {
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            List<tbllivehmiscreen> det = new List<tbllivehmiscreen>();
            try
            {
                string query = "SELECT* from " + databaseName + ".tbllivehmiscreen where isWorkInProgress = 0 and HMIID IN(SELECT HMIID from tbllivehmiscreen as h where h.MachineID = " + machineId + ") order by Date";
                //query = "SELECT* from "+databaseName+".tbllivehmiscreen where isWorkInProgress = 0 and HMIID IN(SELECT HMIID from tbllivehmiscreen as h where h.MachineID = " + machineId + " )order by Date";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tbllivehmiscreen> GetHMIList3(int machineId)
        {
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            List<tbllivehmiscreen> det = new List<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from tbllivehmiscreen where isWorkInProgress = 0 and HMIID IN ( SELECT HMIID from tbllivehmiscreen where MachineID = " + machineId + ")order by HMIID desc ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tbllivehmiscreen> GetHMIList4(int MacID)
        {
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            List<tbllivehmiscreen> det = new List<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from tbllivehmiscreen where isWorkInProgress = 0 and HMIID IN ( SELECT HMIID from tbllivehmiscreen as h where h.MachineID = " + MacID + " order by h.Date)";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public int InsertLiveHMIScreenDetails4(string WorkCenter, int isHold, int OperatiorID, string OperatorDet, int isUpdate, int targetqty, string Type, int MachineID, string OpNo, string MaterialDesc, string Project, string WONo, int processqty)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string qry = "INSERT into " + databaseName + ".[tbllivehmiscreen] (DDLWokrCentre,DoneWithRow,IsHold,IsMultiWO,OperatiorID, OperatorDet,isUpdate,isWorkInProgress,isWorkOrder,Target_Qty,Prod_FAI,MachineID,OperationNo,PartNo,ProcessQty,Delivered_Qty,Project,Status,Work_Order_No) VALUES ('" + WorkCenter + "',0," + isHold + ",0," + OperatiorID + ",'" + OperatorDet + "'," + isUpdate + ",2,0," + targetqty + ",'" + Type + "'," + MachineID + ",'" + OpNo + "','" + MaterialDesc + "'," + processqty + ",0,'" + Project + "',0,'" + WONo + "')";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int InsertHMI4(int id, int optId, string shift, string CorrectedDate, string opno, DateTime PEStartTime)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string qry = "INSERT into " + databaseName + ".[tbllivehmiscreen] (MachineID,OperatiorID,Shift,Date,Time,Project,PartNo,OperationNo,Rej_Qty,Work_Order_No,Target_Qty,Delivered_Qty,Status,CorrectedDate,Prod_FAI,isUpdate,DoneWithRow,isWorkInProgress,isWorkOrder,OperatorDet,PEStartTime, ProcessQty,DDLWokrCentre,IsMultiWO,IsHold,SplitWO,batchCount,batchNo) VALUES (" + id + "," + optId + ",'" + shift + "',null,null,null,null,null,null,null,null,null,0,'" + CorrectedDate + "',null,0,0,2,0,'" + opno + "','" + PEStartTime + "',0,null,0,0,null,0,0)";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int InsertLiveHMIScreenDetail(string OperationNo, string PartNo, DateTime PEStartTime, string CorrectedDate, string Shift, int OperatiorID, string OperatorDet, string Project, int Target_Qty, string Work_Order_No, int ProcessQty, string DDLWokrCentre, int MachineID)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string qry = "INSERT into " + databaseName + ".[tbllivehmiscreen] (OperationNo,PartNo,PEStartTime,CorrectedDate,Shift, OperatiorID,OperatorDet,Project,Target_Qty,Work_Order_No,ProcessQty,Delivered_Qty,DDLWokrCentre,MachineID,IsMultiWO,isWorkInProgress,Status) VALUES ('" + OperationNo + "','" + PartNo + "','" + PEStartTime + "','" + CorrectedDate + "','" + Shift + "'," + OperatiorID + "','" + OperatorDet + "','" + Project + "'," + Target_Qty + ",'" + Work_Order_No + "'," + ProcessQty + ",0,'" + DDLWokrCentre + "'," + MachineID + ",0,2,0)";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int InsertLiveHMIScreenDetail1(string OperationNo, string PartNo, DateTime PEStartTime, string CorrectedDate, string Shift, int OperatiorID, string Project, int Target_Qty, string Work_Order_No, int ProcessQty, string DDLWokrCentre, int MachineID)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string qry = "INSERT into " + databaseName + ".[tbllivehmiscreen] (OperationNo,PartNo,PEStartTime,CorrectedDate,Shift, OperatiorID,Project,Target_Qty,Work_Order_No,ProcessQty,Delivered_Qty,DDLWokrCentre,MachineID,IsMultiWO,isWorkInProgress,Status) VALUES ('" + OperationNo + "','" + PartNo + "','" + PEStartTime + "','" + CorrectedDate + "','" + Shift + "'," + OperatiorID + "','" + Project + "'," + Target_Qty + ",'" + Work_Order_No + "'," + ProcessQty + ",0,'" + DDLWokrCentre + "'," + MachineID + ",0,2,0)";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

            }
            return res;
        }
        public int InsertLiveHMIScreenDetail2(string OperationNo, int isHold, string PartNo, DateTime PEStartTime, string CorrectedDate, string Shift, int OperatiorID, string OperatorDet, string Project, int Target_Qty, string Work_Order_No, int isWorkOrder, int ProcessQty, string DDLWokrCentre, int MachineID)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string qry = "INSERT into " + databaseName + ".[tbllivehmiscreen] (OperationNo,isHold,PartNo,PEStartTime,CorrectedDate,Shift, OperatiorID,OperatorDet,Project,Target_Qty,Work_Order_No,isWorkOrder,ProcessQty,Delivered_Qty,DDLWokrCentre,MachineID,IsMultiWO,isWorkInProgress,Status) VALUES ('" + OperationNo + "'," + isHold + ",'" + PartNo + "','" + PEStartTime + "','" + CorrectedDate + "','" + Shift + "'," + OperatiorID + ",'" + OperatorDet + "','" + Project + "'," + Target_Qty + ",'" + Work_Order_No + "'," + isWorkOrder + "," + ProcessQty + ",0,'" + DDLWokrCentre + "'," + MachineID + ",0,2,0)";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public List<tbllivehmiscreen> GetHMIList5(string hmiids)
        {
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            List<tbllivehmiscreen> det = new List<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from tbllivehmiscreen where HMIID IN ( " + hmiids + " ) order by Work_Order_No,PartNo,OperationNo ; ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tblmachinedetail> GetPlantList3(int plantid)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineName from " + databaseName + ".tblmachinedetails where PlantID = " + plantid + " and IsDeleted = 0 and ShopID IS NULL and CellID IS NULL and ManualWCID IS NOT NULL";
                det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<string> GetCellList3(int cellid)
        {
            List<string> ivs = new List<string>();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineName from " + databaseName + ".tblmachinedetails where CellID =" + cellid + " and IsDeleted = 0 and ManualWCID IS NULL";
                det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();

                foreach (var item in det)
                {
                    ivs.Add(item.MachineName);
                }
                return ivs;
                //return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return ivs;
        }

        public List<string> GetShopList3(int cellid, int shopid)
        {
            List<string> ivs = new List<string>();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineName from " + databaseName + ".tblmachinedetails where ShopID =" + shopid + " and CellID !=" + cellid + " and IsDeleted = 0 and ManualWCID IS NULL";

                det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                foreach (var item in det)
                {
                    ivs.Add(item.MachineName);
                }
                return ivs;
                //return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return ivs;
        }
        public List<tblmachinedetail> GetShopList41(int shopid)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineName from " + databaseName + ".[tblmachinedetails] where ShopID =" + shopid + " and CellID IS NULL and IsDeleted = 0 and ManualWCID IS NULL";
                det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                //return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblmachinedetail> GetShopList42(int plantid)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineName from " + databaseName + ".tblmachinedetails where PlantID = " + plantid + " and ShopID IS NULL and CellID IS NULL and IsDeleted = 0 and ManualWCID IS NULL";
                det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                //return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblmachinedetail> GetPlantList4(int plantid)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineName from " + databaseName + ".tblmachinedetails where PlantID = " + plantid + " and IsDeleted = 0 and ShopID IS NULL and CellID IS NULL";
                det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                //return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblmachinedetail> GetShopList3(int shopid)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineName from " + databaseName + ".tblmachinedetails where IsDeleted = 0 and ShopID = " + shopid + " and CellID IS NULL and ManualWCID is not null";
                det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                //return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tblmachinedetail> GetShopList4(int shopid)
        {
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            List<tblmachinedetail> det = new List<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineName from " + databaseName + ".tblmachinedetails where IsDeleted = 0 and ShopID = " + shopid + " and CellID IS NULL";
                det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                //return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public tblmachinedetail GetMachineDet2(int Machineid)
        {
            tblmachinedetail ddldet = new tblmachinedetail();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblmachinedetails WHERE MachineID = " + Machineid + " and IsDeleted = 0";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ddldet;
        }

        public tblmachinedetail GetMachineDetDDLManual(int Machineid)
        {
            tblmachinedetail ddldet = new tblmachinedetail();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblmachinedetails WHERE MachineID = " + Machineid;
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ddldet;
        }

        public tblmachinedetail GetMachineDet1(string ID)
        {
            tblmachinedetail ddldet = new tblmachinedetail();
            Repository<tblmachinedetail> lista = new Repository<tblmachinedetail>();
            try
            {
                string query = "SELECT MachineDispName from " + databaseName + ".tblmachinedetails WHERE MachineName = " + ID + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ddldet;
        }
        public tbllossescode GetLossDet1(int ID)
        {
            tbllossescode ddldet = new tbllossescode();
            Repository<tbllossescode> lista = new Repository<tbllossescode>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllossescodes WHERE IsDeleted = 0 and LossCodeID = " + ID + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public List<tblddl> Getddl1Details1(string woNo, string partNo, string opNo)
        {
            List<tblddl> ddl = new List<tblddl>();
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();

                string qry = "SELECT * from " + databaseName + ".[tblddl] where WorkOrder = '" + woNo + "' and MaterialDesc = '" + partNo + "'  and OperationNo != '" + opNo + "' and IsCompleted = 0 order by OperationNo,WorkOrder,MaterialDesc";
                ddl = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return ddl;
        }
        public List<tbllossescode> GetLosscode1Det1(int ID)
        {
            Repository<tbllossescode> lista = new Repository<tbllossescode>();
            List<tbllossescode> det = new List<tbllossescode>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllossescodes WHERE IsDeleted = 0 and LossCodesLevel1ID = " + ID + "";
                det = _connectionFactory.GetConnection.QueryAsync<tbllossescode>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)

            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public List<tbllossescode> GetLosscode2Det1(int ID)
        {
            Repository<tbllossescode> lista = new Repository<tbllossescode>();
            List<tbllossescode> det = new List<tbllossescode>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllossescodes WHERE IsDeleted == 0 and LossCodesLevel2ID = " + ID + "";
                det = _connectionFactory.GetConnection.QueryAsync<tbllossescode>(query).Result.ToList();
                //return lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public tblddl GetDDLDet1(string wono, string partno, string opno)
        {
            tblddl ddldet = new tblddl();
            Repository<tblddl> lista = new Repository<tblddl>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblddl WHERE WorkOrder = '" + wono + "' and MaterialDesc = '" + partno + "' and OperationNo = '" + opno + "' and IsCompleted = 0";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public tblddl GetDDLDet1ForScan(string wono, string opno)
        {
            tblddl ddldet = new tblddl();
            Repository<tblddl> lista = new Repository<tblddl>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblddl WHERE WorkOrder = '" + wono + "' and  OperationNo = '" + opno + "' and IsCompleted = 0";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public int UpdateLiveHMIScreenDetailsD1(DateTime? Time, int Status, int isWorkInProgress, int HMIID)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set Status =" + Status + ",isWorkInProgress =" + isWorkInProgress + ",Time='" + Time + "'WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

            }
            return res;
        }
        public List<tblddl> GetDDLDet2(string machineInvNo, string ExceptionDDLsArray)
        {
            Repository<tblddl> lista = new Repository<tblddl>();
            List<tblddl> det = new List<tblddl>();
            try
            {
                //string query = "select * " +
                //             "from "+databaseName+".[tblddl] WHERE WorkCenter = '" + machineInvNo + "' AND IsCompleted = 0 AND DDLID NOT IN (" + ExceptionDDLsArray + ")" +
                //             " order by DaysAgeing = '', Convert(DaysAgeing , SIGNED INTEGER) asc ,FlagRushInd = '',FlagRushInd = 0 ,Convert(FlagRushInd , SIGNED INTEGER) asc  , MADDateInd = '' , MADDateInd asc , MADDate asc";
                string query = "select * from" + databaseName + ".[tblddl] WHERE WorkCenter ='" + machineInvNo + "' AND IsCompleted = 0 AND DDLID NOT IN(0) order by DaysAgeing asc, FlagRushInd asc  , MADDateInd asc, MADDate asc";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tblmultipleworkorder> GettblmultipleworkordersList1(int value)
        {
            Repository<tblmultipleworkorder> lista = new Repository<tblmultipleworkorder>();
            List<tblmultipleworkorder> det = new List<tblmultipleworkorder>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tblmultipleworkorder WHERE IsDeleted = 0 and WCID = " + value + "";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tblmultipleworkorder> GettblmultipleworkordersList2(int value)
        {
            Repository<tblmultipleworkorder> lista = new Repository<tblmultipleworkorder>();
            List<tblmultipleworkorder> det = new List<tblmultipleworkorder>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tblmultipleworkorder WHERE IsDeleted = 0 and WCID = " + null + " and CellID = " + value + "";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tblmultipleworkorder> GettblmultipleworkordersList3(int value)
        {
            Repository<tblmultipleworkorder> lista = new Repository<tblmultipleworkorder>();
            List<tblmultipleworkorder> det = new List<tblmultipleworkorder>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tblmultipleworkorder WHERE IsDeleted = 0 and WCID = " + null + " and CellID = " + null + " and ShopID =" + value + "";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tblmultipleworkorder> GettblmultipleworkordersList4(int value)
        {
            Repository<tblmultipleworkorder> lista = new Repository<tblmultipleworkorder>();
            List<tblmultipleworkorder> det = new List<tblmultipleworkorder>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tblmultipleworkorder WHERE IsDeleted = 0 and WCID = " + null + " and CellID = " + null + " and ShopID =" + null + " and PlantID =" + value + "";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public tbllivehmiscreen GetLiveHMIDet1(int ID)
        {
            tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivehmiscreen WHERE HMIID = " + ID + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public tbllivehmiscreen GetLiveHMIDet2(string wono, string partno, int InnerOpNo)
        {
            tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT TOP 1 * from " + databaseName + ".[tbllivehmiscreen] where Work_Order_No = '" + wono + "' and PartNo = '" + partno + "'  and OperationNo = " + InnerOpNo + " and isWorkInProgress = 1 ";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }

        public tblgenericworkentry GetgenericworkentryDet(int HiddenID, int MachineID)
        {
            tblgenericworkentry ddldet = new tblgenericworkentry();
            Repository<tblgenericworkentry> lista = new Repository<tblgenericworkentry>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblgenericworkentry WHERE GWCodeID =" + HiddenID + " and MachineID =" + MachineID + " and DoneWithRow = 0 order by GWCodeID desc";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public tblgenericworkentry GetgenericworkentryDet1(int MachineID)
        {
            tblgenericworkentry ddldet = new tblgenericworkentry();
            Repository<tblgenericworkentry> lista = new Repository<tblgenericworkentry>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblgenericworkentry WHERE MachineID =" + MachineID + " order by GWEntryID desc";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public tbllivelossofentry GetLossOfEntryDetails4(int macid, int HiddenID)
        {
            tbllivelossofentry lossofEntry = new tbllivelossofentry();
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllivelossofentry] WHERE MessageCodeID =" + HiddenID + " and  MachineID =" + macid + " and DoneWithRow = 0 order by LossID desc";
                lossofEntry = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossofEntry;
        }
        public tblddl GetLiveHMIDet2(string wono)
        {
            tblddl ddldet = new tblddl();
            Repository<tblddl> lista = new Repository<tblddl>();
            try
            {
                string query = "SELECT MaterialDesc from " + databaseName + ".tblddl WHERE WorkOrder = '" + wono + "'";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public int UpdateLiveHMIScreenDeta(int HMIID)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "update " + databaseName + ".[tbllivehmiscreen] set isUpdate=0 WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

            }
            return res;
        }
        public tblgenericworkcode GettblgenericworkcodeDet2(int HiddenID)
        {
            tblgenericworkcode ddldet = new tblgenericworkcode();
            Repository<tblgenericworkcode> lista = new Repository<tblgenericworkcode>();
            try
            {
                string query = "SELECT * from " + databaseName + ".[tblgenericworkcodes] WHERE IsDeleted = 0 and GenericWorkID =" + HiddenID + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public tblgenericworkcode GettblgenericworkcodeDet4(int HiddenID)
        {
            tblgenericworkcode ddldet = new tblgenericworkcode();
            Repository<tblgenericworkcode> lista = new Repository<tblgenericworkcode>();
            try
            {
                string query = "SELECT * from " + databaseName + ".[tblgenericworkcodes] WHERE GenericWorkID =" + HiddenID + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public List<tblgenericworkcode> GettblgenericworkcodeDet3(int HiddenID)
        {
            List<tblgenericworkcode> lossCode = new List<tblgenericworkcode>();
            try
            {
                Repository<tblgenericworkcode> lista = new Repository<tblgenericworkcode>();

                string qry = "SELECT * from " + databaseName + ".[tblgenericworkcodes] WHERE IsDeleted = 0 and GenericWorkID =" + HiddenID + "";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }
        public List<tblgenericworkcode> GettblgenericworkcodeDet14(int Bid)
        {
            List<tblgenericworkcode> lossCode = new List<tblgenericworkcode>();
            try
            {
                Repository<tblgenericworkcode> lista = new Repository<tblgenericworkcode>();

                string qry = "SELECT * from " + databaseName + ".[tblgenericworkcodes] WHERE IsDeleted = 0 and GWCodesLevel1ID = " + Bid + " and GWCodesLevel = 2 and GWCodesLevel2ID IS NULL";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }
        public List<tblgenericworkcode> GettblgenericworkcodeDet16(int Bid)
        {
            List<tblgenericworkcode> lossCode = new List<tblgenericworkcode>();
            try
            {
                Repository<tblgenericworkcode> lista = new Repository<tblgenericworkcode>();

                string qry = "SELECT * from " + databaseName + ".[tblgenericworkcode] WHERE IsDeleted = 0 and GWCodesLevel2ID = " + Bid + " and GWCodesLevel = 3";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }
        public List<tblgenericworkcode> GettblgenericworkcodeDet17()
        {
            List<tblgenericworkcode> lossCode = new List<tblgenericworkcode>();
            try
            {
                Repository<tblgenericworkcode> lista = new Repository<tblgenericworkcode>();

                string qry = "SELECT * from " + databaseName + ".[tblgenericworkcodes] WHERE IsDeleted = 0 and GWCodesLevel = 1";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }
        public List<tblgenericworkcode> GettblgenericworkcodeDet15(int level)
        {
            List<tblgenericworkcode> lossCode = new List<tblgenericworkcode>();
            try
            {
                Repository<tblgenericworkcode> lista = new Repository<tblgenericworkcode>();

                string qry = "SELECT * from " + databaseName + ".[tblgenericworkcodes] WHERE IsDeleted = 0 and GWCodesLevel =" + level + " and GWCodesLevel1ID IS NULL and  GWCodesLevel2ID IS NULL";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }
        public tblgenericworkcode GetgenericCodeDetails4(int prevLevelId)
        {
            tblgenericworkcode lossCode = new tblgenericworkcode();
            try
            {
                Repository<tblgenericworkcode> lista = new Repository<tblgenericworkcode>();

                string qry = "SELECT GenericWorkCode FROM " + databaseName + ".[tblgenericworkcodes] WHERE GenericWorkID =" + prevLevelId + " and IsDeleted = 0";
                lossCode = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }
        public List<tblgenericworkcode> GetgenericCodeDetails5(int prevLevelId)
        {
            List<tblgenericworkcode> lossCode = new List<tblgenericworkcode>();
            try
            {
                Repository<tblgenericworkcode> lista = new Repository<tblgenericworkcode>();

                string qry = "SELECT * FROM " + databaseName + ".[tblgenericworkcodes] WHERE IsDeleted = 0 and GWCodesLevel1ID =" + prevLevelId + " and GWCodesLevel2ID IS NULL";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }
        public int deleteMultiWOSlectionDetails3(int HMIID)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "delete from " + databaseName + ".tbllivehmiscreen WHERE HMIID = " + HMIID + "";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public tbllivelossofentry GettbllivelossofentryDet(int macID)
        {
            tbllivelossofentry ddldet = new tbllivelossofentry();
            Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();
            try
            {
                string query = "SELECT * from " + databaseName + ".[tbllivelossofentry] WHERE MachineID = " + macID + " order by StartDateTime desc";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public List<tbllossescode> GetLossCodeDetails11(int Bid, int losscodelevel)
        {
            List<tbllossescode> lossCode = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE IsDeleted = 0 and LossCodesLevel1ID =" + Bid + " and LossCodesLevel =" + losscodelevel + "  and LossCodesLevel2ID IS NULL and MessageType != 'MNT'";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }
        public List<tbllossescode> GetLossCodeDetails13(int Bid, int losscodelevel)
        {
            List<tbllossescode> lossCode = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE IsDeleted = 0 and LossCodesLevel2ID =" + Bid + " and LossCodesLevel =" + losscodelevel + "  and MessageType != 'MNT'";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }
        public List<tbllossescode> GetLossCodeDetails14(int Bid, int losscodelevel)
        {
            List<tbllossescode> lossCode = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE IsDeleted = 0 and LossCodesLevel2ID =" + Bid + " and LossCodesLevel =" + losscodelevel + "";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }
        public List<tbllossescode> GetLossCodeDetails12(int level)
        {
            List<tbllossescode> lossCode = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE IsDeleted = 0 and LossCodesLevel =" + level + " and LossCodesLevel1ID =null and LossCodesLevel2ID =null and MessageType != 'NoCode' and MessageType != 'MNT' and MessageType != 'PM'";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public List<tbllossescode> GetBreakdowndetails(int level)
        {
            List<tbllossescode> lossCode = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE IsDeleted = 0 and LossCodesLevel =" + level + " and LossCodesLevel1ID =null and LossCodesLevel2ID =null and MessageType != 'NoCode' and MessageType != 'IDLE' and MessageType != 'Setup'";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }

        public List<tbllossescode> GetLossCodeDetails15()
        {
            List<tbllossescode> lossCode = new List<tbllossescode>();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE IsDeleted = 0 and LossCodesLevel =1 and MessageType != 'NoCode' and MessageType != 'MNT' and MessageType != 'PM'";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return lossCode;
        }
        public tblbreakdown GetbreakdownDet(int brekdnID)
        {
            tblbreakdown ddldet = new tblbreakdown();
            Repository<tblbreakdown> lista = new Repository<tblbreakdown>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblbreakdown WHERE BreakdownID = " + brekdnID + " ";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public int InsertLiveHMIDetails1(int MachineID, int OperatiorID, string Shift, DateTime Date, DateTime Time, string Project, string PartNo, string OperationNo, int? Rej_Qty, string Work_Order_No, int Target_Qty, int? Status, string CorrectedDate, string Prod_FAI, int isWorkOrder, string OperatorDet, DateTime PEStartTime, int ProcessQty, string DDLWokrCentre)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "INSERT INTO " + databaseName + ".[tbllivehmiscreen](MachineID,OperatiorID,Shift,Date,Time,Project,PartNo,OperationNo,Rej_Qty,Work_Order_No,Target_Qty,Delivered_Qty,Status,CorrectedDate,Prod_FAI,isUpdate,DoneWithRow,isWorkInProgress,isWorkOrder,OperatorDet,PEStartTime,ProcessQty,DDLWokrCentre,IsMultiWO,IsHold,SplitWO) VALUES(' " + MachineID + "," + OperatiorID + ",'" + Shift + "','" + Date + "','" + Time + "','" + Project + "','" + PartNo + "','" + OperationNo + "'," + Rej_Qty + ",'" + Work_Order_No + "'," + Target_Qty + ",0," + Status + ",'" + CorrectedDate + "','" + Prod_FAI + "',1,1,1," + isWorkOrder + ",'" + OperatorDet + "','" + PEStartTime + "'," + ProcessQty + ",'" + DDLWokrCentre + "',0,1,'No')";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

            }
            return res;
        }
        public int InsertLiveHMIDetails2(DateTime PEStartTime, string OperationNo, string MaterialDesc, string Project, string CorrectedDate, string Shift, int OperatiorID, string OpName, int TargetQty, string WorkOrder, int isWorkOrder, int TotalProcessQty, int ishold, string WorkCenter, int machineId)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "INSERT INTO " + databaseName + ".[tbllivehmiscreen](MachineID,OperatiorID,Shift,Project,PartNo,OperationNo,Work_Order_No,Target_Qty,Status,CorrectedDate,isWorkOrder,OperatorDet,PEStartTime,ProcessQty,DDLWokrCentre,IsHold,Prod_FAI) VALUES(" + machineId + "," + OperatiorID + ",'" + Shift + "','" + Project + "','" + MaterialDesc + "','" + OperationNo + "','" + WorkOrder + "'," + TargetQty + ",0,'" + CorrectedDate + "'," + isWorkOrder + ",'" + OpName + "','" + PEStartTime + "'," + TotalProcessQty + ",'" + WorkCenter + "'," + ishold + ",'Production')";
                res = lista.Insert(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

            }
            return res;
        }

        public List<tbllivelossofentry> GettbllivelossofentriesList4(int value)
        {
            Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();
            List<tbllivelossofentry> det = new List<tbllivelossofentry>();
            try
            {
                string query = "SELECT TOP 1 * From " + databaseName + ".tbllivelossofentry WHERE MachineID =" + value + " and (IsScreen = 1 or IsStart = 1) order by StartDateTime desc";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tbllossescode> GettbllossescodeList()
        {
            Repository<tbllossescode> lista = new Repository<tbllossescode>();
            List<tbllossescode> det = new List<tbllossescode>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tbllossescodes WHERE IsDeleted =0 and MessageType = 'PM'";
                det = _connectionFactory.GetConnection.QueryAsync<tbllossescode>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tbllossescode> GettbllossescodeList1()
        {
            Repository<tbllossescode> lista = new Repository<tbllossescode>();
            List<tbllossescode> det = new List<tbllossescode>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tbllossescodes WHERE IsDeleted =0 and MessageType = 'SETUP'";
                det = _connectionFactory.GetConnection.QueryAsync<tbllossescode>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return det;
        }
        public int UpdateLossofentriesDetails(int LossID)
        {
            int res = 0;
            try
            {
                Repository<tbllivelossofentry> lista = new Repository<tbllivelossofentry>();
                //string query = "Update  "+databaseName+".tbllivelossofentry SET ForRefresh = 1 WHERE MachineID =" + machineID + " ";
                string query = "Update  " + databaseName + ".tbllivelossofentry SET ForRefresh = 1 WHERE LossID =" + LossID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateLossofentriesDetails1(int hmiid, DateTime Time)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET Status = 2,isWorkInProgress = 1,SplitWO = 'No',Time='" + Time + "' WHERE HMIID =" + hmiid + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdatetblddlDetails1(int DDLID)
        {
            int res = 0;
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();
                string query = "Update  " + databaseName + ".[tblddl] SET IsCompleted = 1 WHERE DDLID =" + DDLID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdatetblddlDetails2(int ddlID)
        {
            int res = 0;
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();
                string query = "Update  " + databaseName + ".tblddl SET IsCompleted = 1 WHERE DDLID =" + ddlID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }

        public int UpdateBreakdownDetails(DateTime time, int currentId)
        {
            int res = 0;
            try
            {
                Repository<tblbreakdown> lista = new Repository<tblbreakdown>();
                string query = "Update  " + databaseName + ".tblbreakdown SET EndTime = '" + time + "' WHERE BreakdownID =" + currentId + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateLossofentriesDetails1(int machineID)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivelossofentry SET ForRefresh = 2 WHERE MachineID =" + machineID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return res;
        }
        public int UpdateLossofentriesDetails2(int id, DateTime Time)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivelossofentry SET EndDateTime ='" + Time + "',DoneWithRow = 1,IsUpdate = 1,IsScreen = 0,IsStart = 0,ForRefresh = 0 WHERE LossID =" + id + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return res;
        }
        public List<tblddl> GettblddlList(string WONo, string Part, string Operation)
        {
            Repository<tblddl> lista = new Repository<tblddl>();
            List<tblddl> det = new List<tblddl>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tblddl WHERE IsCompleted = 0 and WorkOrder = '" + WONo + "' and MaterialDesc ='" + Part + "' and OperationNo != '" + Operation + "' order by WorkOrder,MaterialDesc,OperationNo";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tblddl> GettblddlList1(string WONo, string Part, string Operation)
        {
            Repository<tblddl> lista = new Repository<tblddl>();
            List<tblddl> det = new List<tblddl>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblddl where WorkOrder = '" + WONo + "' and MaterialDesc = '" + Part + "'  and OperationNo != '" + Operation + "' and IsCompleted = 0 order by OperationNo ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tblbreakdown> GetbreakdownList(int id)
        {
            Repository<tblbreakdown> lista = new Repository<tblbreakdown>();
            List<tblbreakdown> det = new List<tblbreakdown>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tblbreakdown WHERE MachineID = " + id + " and (EndTime IS NULL and MessageCode ='PM')";
                det = _connectionFactory.GetConnection.QueryAsync<tblbreakdown>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tblbreakdown> GetbreakdownList1(int id, string mcode)
        {
            Repository<tblbreakdown> lista = new Repository<tblbreakdown>();
            List<tblbreakdown> det = new List<tblbreakdown>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tblbreakdown WHERE MachineID = " + id + " and EndTime IS NULL and MessageCode ='" + mcode + "'";
                det = _connectionFactory.GetConnection.QueryAsync<tblbreakdown>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public tbllivehmiscreen GettbllivehmiscreensDet(string WONo, string Part, string Operation)
        {
            tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tbllivehmiscreen WHERE IsCompleted = 0 and WorkOrder = '" + WONo + "' and MaterialDesc ='" + Part + "' and OperationNo != '" + Operation + "' order by WorkOrder,MaterialDesc,OperationNo";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public message_code_master Getmessage_code_masterDet(int? BreakDownCode)
        {
            message_code_master ddldet = new message_code_master();
            Repository<message_code_master> lista = new Repository<message_code_master>();
            try
            {
                string query = "SELECT * From " + databaseName + ".message_code_master WHERE BreakDownCode = " + BreakDownCode + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public message_code_master Getmessage_code_masterDet1(int? MessageCodeID)
        {
            message_code_master ddldet = new message_code_master();
            Repository<message_code_master> lista = new Repository<message_code_master>();
            try
            {
                string query = "SELECT * From " + databaseName + ".message_code_master WHERE MessageCodeID = " + MessageCodeID + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public List<tbllivemultiwoselection> GettbllivemultiwoselectionList(string WONo, string Part, int localOPNo)
        {
            Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();
            List<tbllivemultiwoselection> det = new List<tbllivemultiwoselection>();
            try
            {
                string query = "SELECT TOP 1 * from " + databaseName + ".tbllivemultiwoselection where WorkOrder = '" + WONo + "' and PartNo = '" + Part + "' and OperationNo = " + localOPNo + " order by MultiWOID";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tblddl> GettblddlWOList(string WONo, string localOPNo, string Part)
        {
            Repository<tblddl> lista = new Repository<tblddl>();
            List<tblddl> det = new List<tblddl>();
            try
            {
                string query = "SELECT  * from " + databaseName + ".tblddl where WorkOrder = '" + WONo + "' and OperationNo < " + Convert.ToInt32(localOPNo) + " and IsCompleted=0 order by OperationNo";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public tbllivehmiscreen GettbllivehmiscreensDet(string woNo, string partNo, string opNo, int Hmiid)
        {
            tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivehmiscreen WHERE Work_Order_No = '" + woNo + "' and PartNo ='" + partNo + "' and OperationNo ='" + opNo + "' and HMIID !=" + Hmiid + " and isWorkInProgress !=2 order by Time desc";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }



        public tbllivehmiscreen GetliveHMIScreenDetails1(int hmiidiInner, string WONo, string partNo, int hmiidi)
        {
            tbllivehmiscreen hmidet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivehmiscreen WHERE HMIID = " + hmiidiInner + " and Work_Order_No ='" + WONo + "' and PartNo ='" + partNo + "' and HMIID != " + hmiidi + "";
                hmidet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return hmidet;
        }
        public tbllivehmiscreen GettbllivehmiscreensDet1(string woNo, string partNo, string opNo, int MachineID)
        {
            tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivehmiscreen WHERE Work_Order_No = '" + woNo + "' and PartNo ='" + partNo + "' and OperationNo ='" + opNo + "' and MachineID !=" + MachineID + " and isWorkInProgress =2 order by Time desc";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public tbllivehmiscreen GettbllivehmiscreensDetforhold(string woNo, string opNo, int MachineID)
        {
            tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivehmiscreen WHERE Work_Order_No = '" + woNo + "' and OperationNo ='" + opNo + "' and MachineID !=" + MachineID + " and Date is not null and Time is null";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public tbllivehmiscreen GettbllivehmiscreensDet1(int machineid)
        {
            tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivehmiscreen WHERE MachineID = " + machineid + " and isWorkInProgress = 2 order by HMIID desc";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }

        public tbllivehmiscreen validatelivehmiscreensDet(string WoNo, string PartNo, string OpNo)
        {
            tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivehmiscreen WHERE Work_Order_No = '" + WoNo + "' and PartNo = '" + PartNo + "' and OperationNo = '" + OpNo + "' order by HMIID desc";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public tblhmiscreen validateHisthmiscreensDet(string WoNo, string PartNo, string OpNo)
        {
            tblhmiscreen ddldet = new tblhmiscreen();
            Repository<tblhmiscreen> lista = new Repository<tblhmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblhmiscreen WHERE Work_Order_No = '" + WoNo + "' and PartNo = '" + PartNo + "' and OperationNo = '" + OpNo + "'";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public List<tbllivehmiscreen> ValidateLiveHMIData(string WoNo, string OpNo, string PartNo)
        {
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            List<tbllivehmiscreen> det = new List<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tbllivehmiscreen where Work_Order_No = '" + WoNo + "' and PartNo = '" + PartNo + "' and OperationNo < " + OpNo + " order by OperationNo";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tbllivehmiscreen> ValidateHistHMIData(string WoNo, string OpNo, string PartNo)
        {
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            List<tbllivehmiscreen> det = new List<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblhmiscreen where Work_Order_No = '" + WoNo + "' and PartNo = '" + PartNo + "' and OperationNo < " + OpNo + " order by OperationNo";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public tblddl GetDDLsDet(int DDLID)
        {
            tblddl ddldet = new tblddl();
            Repository<tblddl> lista = new Repository<tblddl>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblddl WHERE DDLID = " + DDLID + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public tblddl GetDDLsDet1(string MainPartNo, string MainOpearationNo, string MainWorkOrder)
        {
            tblddl ddldet = new tblddl();
            Repository<tblddl> lista = new Repository<tblddl>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblddl WHERE MaterialDesc = '" + MainPartNo + "' and OperationNo = '" + MainOpearationNo + "' and WorkOrder = '" + MainWorkOrder + "' and IsCompleted = 1";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public int UpdateLiveHMI1(int Hmiid)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET Prod_FAI =null,Target_Qty=null,OperationNo =null,PartNo=null,Work_Order_No =null,Project =null,Date =null,DDLWokrCentre =null,ProcessQty =0,Delivered_Qty =0 WHERE HMIID =" + Hmiid + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return res;
        }
        public int UpdateIsworkorder(int Hmiid)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET isWorkOrder=0  WHERE HMIID =" + Hmiid + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return res;
        }
        public int UpdateLiveHMI3(int Hmiid)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET Target_Qty=null,OperationNo =null,PartNo=null,Work_Order_No =null,ProcessQty =0,Delivered_Qty =0,DDLWokrCentre IS NULL,IsMultiWO = 0 WHERE HMIID =" + Hmiid + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateLiveHMI4(int Hmiid)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET Target_Qty=null,OperationNo =null,PartNo=null,Project IS NULL,Work_Order_No =null,ProcessQty =0,DDLWokrCentre IS NULL,IsMultiWO = 0 WHERE HMIID =" + Hmiid + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return res;
        }
        public int UpdateLiveHMI2(int Hmiid)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET ProcessQty =0,Delivered_Qty =0,Date IS NULL WHERE HMIID =" + Hmiid + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateLiveHMIScreenDetails5(int Hmiid, string OperationNo, string MaterialDesc, string WorkCenter, string Project, int Target_Qty, string WorkOrder, int TotalProcessQty)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET OperationNo ='" + OperationNo + "',PartNo ='" + MaterialDesc + "',DDLWokrCentre ='" + WorkCenter + "', Project ='" + Project + "',Target_Qty = " + Target_Qty + ",Work_Order_No='" + WorkOrder + "',ProcessQty =0, Delivered_Qty = 0,IsMultiWO = 0 WHERE HMIID = " + Hmiid + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public List<tblddl> GetddlList(string WONoInner, string PartInner, string OperationInner)
        {
            Repository<tblddl> lista = new Repository<tblddl>();
            List<tblddl> det = new List<tblddl>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tblddl WHERE IsCompleted = 0 and WorkOrder = '" + WONoInner + "' and MaterialDesc = '" + PartInner + "' and OperationNo != '" + OperationInner + "' and IsCompleted = 0 order by WorkOrder,MaterialDesc,OperationNo";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public tbllivehmiscreen GettbllivehmiscreensDet1(string WONoInner, string PartInner, string localOPNoString)
        {
            tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tbllivehmiscreen WHERE Work_Order_No = '" + WONoInner + "' and PartNo = '" + PartInner + "' and OperationNo != '" + localOPNoString + "'";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public tbllivehmiscreen GettbllivehmiscreensDet2(string WONoInner, string PartInner, string OperationInner, int MachineID)
        {
            tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * From " + databaseName + ".tbllivehmiscreen WHERE Work_Order_No ='" + WONoInner + "' and PartNo ='" + PartInner + "' and OperationNo ='" + OperationInner + "' and isWorkInProgress = 2 and MachineID =" + MachineID + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public List<tbllivemultiwoselection> GettbllivemultiwoselectionList1(string WONoInner, string PartInner, int localOPNo)
        {
            Repository<tbllivemultiwoselection> lista = new Repository<tbllivemultiwoselection>();
            List<tbllivemultiwoselection> det = new List<tbllivemultiwoselection>();
            try
            {
                string query = "SELECT TOP 1* from " + databaseName + ".tbllivemultiwoselection where WorkOrder = '" + WONoInner + "' and PartNo = '" + PartInner + "' and OperationNo = '" + localOPNo + "' order by MultiWOID";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public int UpdateLiveHMIScreenDetails5(string opt, string part, string MainProject, int TotalTargetQty, int TotalProcessQty, string woorder, int HMIID, string ddlWorkCenter)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET OperationNo = '" + opt + "',PartNo ='" + part + "',Project ='" + MainProject + "',Target_Qty =" + TotalTargetQty + ",Work_Order_No ='" + woorder + "',SplitWO = 0,ProcessQty=" + TotalProcessQty + ",Delivered_Qty = 0,DDLWokrCentre='" + ddlWorkCenter + "',IsMultiWO = 1 WHERE HMIID = " + HMIID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateLiveHMIScreenListNo(int HMIID, string value)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "update " + databaseName + ".[tbllivehmiscreen] set SheetNoList = '" + value + "' WHERE HMIID =" + HMIID + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateLiveHMIScreenDetails6(string opt, string part, string MainProject, int TotalTargetQty, int TotalProcessQty, string woorder, string Type, int HMIID, string ddlWorkCenter)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET OperationNo = '" + opt + "',PartNo ='" + part + "',Project ='" + MainProject + "',SplitWO = 0,Target_Qty =" + TotalTargetQty + ",Work_Order_No ='" + woorder + "',Prod_FAI='" + Type + "',ProcessQty=" + TotalProcessQty + ",Delivered_Qty = 0,DDLWokrCentre='" + ddlWorkCenter + "',IsMultiWO = 0 WHERE HMIID = " + HMIID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public List<tblddl> GetddlDetails2(int DDLID)
        {
            Repository<tblddl> lista = new Repository<tblddl>();
            List<tblddl> det = new List<tblddl>();
            try
            {
                string query = "SELECT * from " + databaseName + ".tblddl WHERE DDLID = " + DDLID + "";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<tbllivehmiscreen> GetLiveHMIDetails1(string WONo, string Part, string Operation)
        {
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            List<tbllivehmiscreen> det = new List<tbllivehmiscreen>();
            try
            {
                string query = "SELECT TOP 1* from " + databaseName + ".tbllivehmiscreen WHERE Work_Order_No = '" + WONo + "' and PartNo = '" + Part + "' and OperationNo = '" + Operation + "' and isWorkInProgress != 2 order by HMIID desc";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public List<int> GetListingno()
        {
            List<int> det1 = new List<int>();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            List<tbllivehmiscreen> det = new List<tbllivehmiscreen>();
            try
            {
                string query = "Select * from " + databaseName + ".tbloperatordetails where isDeleted = 0";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
                foreach (var row in det)
                {
                    //det1 = row.SheetNoList;
                }
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det1;
        }

        public List<tblddl> GetLiveHMIDetails1(string DDLIDString)
        {
            Repository<tblddl> lista = new Repository<tblddl>();
            List<tblddl> det = new List<tblddl>();
            try
            {
                string query = "SELECT * from tblddl where  DDLID IN ( " + DDLIDString + " ) order by WorkOrder,MaterialDesc,OperationNo ";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public int UpdateDDLDetails(int DDLID)
        {
            int res = 0;
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();
                string query = "Update  " + databaseName + ".tblddl SET IsCompleted =0 WHERE DDLID = " + DDLID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public int UpdateDDLDetails1(int DDLID)
        {
            int res = 0;
            try
            {
                Repository<tblddl> lista = new Repository<tblddl>();
                string query = "Update  " + databaseName + ".tblddl SET IsCompleted =1 WHERE DDLID = " + DDLID + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public tbllivehmiscreen GettbllivehmiscreensDet4(int machineid)
        {
            tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT Shift From " + databaseName + ".tbllivehmiscreen WHERE MachineID = " + machineid + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public int UpdateLiveHMIScreenDetails6(string OperationNo, string MaterialDesc, string Project, int Target_Qty, string WorkOrder, int TotalProcessQty, string WorkCenter, int Hmiid)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string query = "Update  " + databaseName + ".tbllivehmiscreen SET OperationNo ='" + OperationNo + "',PartNo ='" + MaterialDesc + "',DDLWokrCentre ='" + WorkCenter + "', Project ='" + Project + "',Target_Qty = " + Target_Qty + ",Work_Order_No='" + WorkOrder + "',ProcessQty =0, DDLWokrCentre = '" + WorkCenter + "',IsMultiWO = 0 WHERE HMIID = " + Hmiid + " ";
                res = _connectionFactory.GetConnection.ExecuteAsync(query).Result;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return res;
        }
        public tbllivehmiscreen GettbllivehmiscreensDet5(int hmid)
        {
            tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * From " + databaseName + ".[tbllivehmiscreen] where HMIID =" + hmid + " and Work_Order_No is not null";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);

            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public tbllivehmiscreen GettbllivehmiscreensDet51(int MachineId)
        {
            tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * From " + databaseName + ".[tbllivehmiscreen] where MachineID =" + MachineId + " and Work_Order_No is null";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);

            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public List<tbllivehmiscreen> GetLiveHMIDetails145(int MachineId, string CorrectedDate)
        {
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            List<tbllivehmiscreen> det = new List<tbllivehmiscreen>();
            try
            {
                string query = "SELECT * From " + databaseName + ".[tbllivehmiscreen] where MachineID =" + MachineId + " and (Work_Order_No is null or Work_Order_No is not null) and Date is null and CorrectedDate='" + CorrectedDate + "'";
                //det = _connectionFactory.GetConnection.QueryAsync<tblmachinedetail>(query).Result.ToList();
                det = lista.GetList(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }
        public int GetMaxbatchNoDet4()
        {
            int ddldet = 0;
            //tbllivehmiscreen ddldet = new tbllivehmiscreen();
            Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
            try
            {
                string query = "SELECT MAX(batchNo) as batchNo From " + databaseName + ".[tbllivehmiscreen]";
                var sa = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
                ddldet = Convert.ToInt32(sa.batchNo);

            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }
        public int UpdateBatchNo(int hmid, int? maxBatchNo)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set batchNo = " + maxBatchNo + ",isUpdate = 1 WHERE HMIID =" + hmid + " and Date IS NULL and Work_Order_No IS NOT NULL";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

            }
            return res;
        }
        public int GetHMIDet5(int hmid, int? maxBatchNo)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set batchNo = " + maxBatchNo + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

            }
            return res;
        }
        public int GetHMIDet5(int hmid, int FinalProcessed)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set ProcessQty = " + FinalProcessed + " WHERE HMIID =" + hmid + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

            }
            return res;
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
        public tbllossescode GetLossCode1Details132(string Losscode)
        {
            tbllossescode lossCode = new tbllossescode();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE LossCode ='" + Losscode + "'";
                lossCode = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                //MessageBox.Show(ex.ToString());
            }
            return lossCode;
        }
        public tbllossescode GetLossDet21(int losscodeid)
        {
            tbllossescode ddldet = new tbllossescode();
            Repository<tbllossescode> lista = new Repository<tbllossescode>();
            try
            {
                string query = "SELECT MessageType from " + databaseName + ".tbllossescodes WHERE LossCodeID =" + losscodeid + "";
                ddldet = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                // MessageBox.Show(ex.ToString());
            }
            return ddldet;
        }

        public tbllossescode GetLossCode1Details(int BID)
        {
            tbllossescode lossCode = new tbllossescode();
            try
            {
                Repository<tbllossescode> lista = new Repository<tbllossescode>();

                string qry = "SELECT * FROM " + databaseName + ".[tbllossescodes] WHERE LossCodeID =" + BID + "";
                lossCode = lista.GetFirstOrDefault(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

                //MessageBox.Show(ex.ToString());
            }
            return lossCode;
        }

        public int GetHMIDet5(int hmid, int? maxBatchNo, DateTime st, int isUpdate, int Status, int isWorkOrder)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "UPDATE " + databaseName + ".[tbllivehmiscreen] set batchNo = " + maxBatchNo + ",Date='" + st + "',isUpdate=1,Status=0,isWorkOrder=0 where HMIID=" + hmid + "";
                res = lista.update(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

            }
            return res;
        }

        public int GetHMIDetDel(int hmid)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string qry = "Delete FROM " + databaseName + ".[tbllivehmiscreen] where HMIID=" + hmid + "";
                res = lista.delete(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

            }
            return res;
        }
        public int deleteHMi(int Machineid)
        {
            int res = 0;
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();
                string qry = "Delete FROM " + databaseName + ".[tbllivehmiscreen] where MachineID=" + Machineid + " and Date is null and Time is null and Project is null and PartNo is null and OperationNo is null and Work_Order_No is null and Target_Qty is null and Delivered_Qty is null and Prod_FAI is null and DDLWokrCentre is null and batchNo is null";
                res = lista.delete(qry, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());

            }
            return res;
        }

        public object Operatoridvalid(int v, object roleid)
        {
            throw new NotImplementedException();
        }

        public int GetHMIDetCount(int Machineid, string correctedDate)
        {
            int item = 0;
            List<tbllivehmiscreen> lossCode = new List<tbllivehmiscreen>();
            try
            {
                Repository<tbllivehmiscreen> lista = new Repository<tbllivehmiscreen>();

                string qry = "SELECT HMIID FROM " + databaseName + ".[tbllivehmiscreen] WHERE MachineID =" + Machineid + " and CorrectedDate='"+correctedDate+"' and date is not null and Time is not null ";
                lossCode = lista.GetList(qry, _connectionFactory.GetConnection);
                item = lossCode.Count;
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return Convert.ToInt32(item);
        }

        public tbllivemode GetLatestMode(int MachineID)
        {
            
           tbllivemode det = new tbllivemode();
            try
            {
                Repository<tbllivemode> lista = new Repository<tbllivemode>();
                string query = "SELECT * From " + databaseName + ".[tbllivemode] WHERE IsCompleted = 0 and MachineID = " + MachineID + " and  ColorCode='RED' order by Modeid desc";
                //det = _connectionFactory.GetConnection.QueryAsync<tbllivemode>(query).Result.FirstOrDefault();
                det = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
            return det;
        }

        //public bool checkOpCancel(string workOrderNo, string operationNo)
        //{
        //    bool check = false;
        //    tbl_PrevOperationCancel opCan = new tbl_PrevOperationCancel();
        //    try
        //    {
        //        Repository<tbl_PrevOperationCancel> lista = new Repository<tbl_PrevOperationCancel>();
        //        string query = "SELECT *  FROM [" + ConnectionFactory.DB + "].[" + ConnectionFactory.Schema + "].[tbl_PrevOperationCancel] where ProductionOrder='" + workOrderNo + "' and Operation=" + operationNo + " and IsCancelled=2";
        //        opCan = lista.GetFirstOrDefault(query, _connectionFactory.GetConnection);
        //        if (opCan != null)
        //        {
        //            check = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        IntoFile(ex.ToString());
        //    }
        //    return check;
        //}

    }
}
