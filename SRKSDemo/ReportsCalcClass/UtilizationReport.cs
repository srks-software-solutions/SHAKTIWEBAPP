using System;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Data.SqlClient;
using SRKSDemo.Server_Model;
using SRKSDemo;

namespace SRKSDemo.ReportsCalcClass
{
    public class UtilizationReport
    {
      i_facility_shaktiEntities1 Serverdb = new i_facility_shaktiEntities1();

        public UtilizationReport()
        {

        }

        public void CalculateUtilization(int PlantID, int ShopID, int CellID, int MachineID, DateTime FromDate, DateTime Enddate)
        {
            var getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

            if (MachineID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).ToList();
            }
            else if (CellID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
            }
            else if (ShopID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
            }

            int dateDifference = Enddate.Subtract(FromDate).Days;
            MsqlConnection mc = new MsqlConnection();
            for (int i = 0; i <= dateDifference; i++)
            {
                DateTime QueryDate = FromDate.AddDays(i);

                foreach (var Machine in getMachineList)
                {
                    var GetUtilList = Serverdb.tbl_UtilReport.Where(m => m.MachineID == Machine.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
                    if (GetUtilList.Count == 0)
                    {
                        using (SqlCommand cmd = new SqlCommand("[dbo].SP_UtilData", mc.msqlConnection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Cdate", QueryDate.Date);
                            cmd.Parameters.AddWithValue("@MachineID", Machine.MachineID);
                            mc.open();
                            cmd.ExecuteNonQuery();
                            mc.close();
                        }
                        //Serverdb.Database.
                        //Serverdb.SP_UtilData(QueryDate.Date, Machine.MachineID);
                    }
                }
            }

        }
    }
}