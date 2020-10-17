using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SRKSDemo.Server_Model;
namespace SRKSDemo
{
    public class ShiftPlannerModel
    {
        public tblshiftplanner ShiftPlanner { get; set; }

        public IEnumerable<tblshiftplanner> ShiftPlannerList { get; set; }
    }

    public class ShiftPlan
    {
        public string SPlannerName { get; set; }

        public string SPlannerDesc { get; set; }

        public int Sethod { get; set; }

        public int plantid { get; set; }

        public int department { get; set; }

        public int machinecate { get; set; }

        public int machineid { get; set; }

        public string startdate { get; set; }

        public string enddate { get; set; }

    }
}