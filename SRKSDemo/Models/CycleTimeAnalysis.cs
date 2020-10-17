using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo.Models
{
    public class CycleTimeAnalysis
    {
        public int Sl_No { get; set; }
        public string Machine_Name { get; set; }
        public string Part_Number { get; set; }
        public string Date { get; set; }
        public int Parts_Count { get; set; }
        public decimal Std_CycleTime_In_Minutes { get; set; }
        // public string Std_CycleTime_Unit { get; set; }
        public decimal Operating_Time_In_Minutes { get; set; }
        //public string Operating_Time_Unit { get; set; }
        public decimal Avg_Operating_Time_In_Minutes { get; set; }
        // public string Avg_Operating_Time_Unit { get; set; }

        public decimal std_LoadTime_In_Minutes { get; set; }
        // public string Std_LoadTime_Unit { get; set; }
        public decimal Total_LoadTime_In_Minutes { get; set; }
        // public string Total_LoadTime_Unit { get; set; }

        public decimal Avg_LoadTime_In_Minutes { get; set; }
        // public string Avg_LoadTime_Unit { get; set; }
    }
}