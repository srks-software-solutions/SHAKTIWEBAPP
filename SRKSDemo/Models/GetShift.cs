using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Configuration;
//using MySql.Data.MySqlClient;
using SRKSDemo;
using System.Data.SqlClient;
using SRKSDemo.Server_Model;

namespace SRKSDemo
{
    public class GetShift
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        string dbName = ConfigurationManager.AppSettings["dbName"];

        public bool IsThisPlanInAction(int id)
        {
            bool status = false;
            DataTable dataHolder = new DataTable();

            string CorrectedDate = null;
            tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
            TimeSpan Start = StartTime.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }

          SRKSDemo.MsqlConnection mc = new SRKSDemo.MsqlConnection();
            mc.open();
            String sql = "SELECT * FROM "+dbName+".[tblshiftplanner] WHERE StartDate <='" + CorrectedDate + "' AND EndDate >='" + CorrectedDate + "'AND ShiftPlannerID = " + id + " ORDER BY ShiftPlannerID ASC";
            SqlDataAdapter da = new SqlDataAdapter(sql, mc.msqlConnection);
            da.Fill(dataHolder);
            mc.close();

            if (dataHolder.Rows.Count > 0)
            {
                status = true;
            }
            return status;
        }

        private class MsqlConnection
        {
        }
    }
}