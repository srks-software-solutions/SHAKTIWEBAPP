
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo.Models
{
    public class MachineStatus
    {
        public int MachineID { get; set; }
        public int MachineOffTime { get; set; }
        public int OperatingTime { get; set; }
        public int IdleTime { get; set; }
        public int BreakdownTime { get; set; }
        public int Utilization { get; set; }
        public int TotalTime { get; set; }
        public int PrevUtilization { get; set; }
        public int PrevOperatingTime { get; set; }
        public int PrevTotalTime { get; set; }
        public string CellName { get; set; }

        public virtual tblmachinedetail machinedet { get; set; }
    }

    public class GetOEE {

        public int Availability { get; set; }
        public int Quality { get; set; }
        public int Performance { get; set; }
        public int OEE { get; set; }

        public int RunningTime { get; set; }
        public int IDLETIME { get; set; }
        public int BreakdownTime { get; set; }
        public int PowerOffTime { get; set; }
        public List<IDLELosses> TopIDLELosses { get; set; }
        public List<BreakdownLosses> TopbrkdwnLosses { get; set; }
        public List<Losses> TopLosses { get; set; }
        
        public MachineTime Machinetimes { get; set; }
    }

    public class ViewData
    {
        public string type { get; set; }
        public string color { get; set; }
        public string lineColor { get; set; }
        //public string showInLegend { get; set; }
      
        //public string name { get; set; }
        public List<dataPoints> dataPoints { get; set; }


    }

    public class dataPoints
    {
        public string label { get; set; }
        public decimal y { get; set; }
    }

    public class IDLELosses {
        public int ID { get; set; }
        public int? LossID { get; set; }
        public string LossName { get; set; }
        public double LossPercent { get; set; }

    }

    public class BreakdownLosses
    {
        public int ID { get; set; }
        public int LossID { get; set; }
        public string LossName { get; set; }
        public double LossPercent { get; set; }

    }

    public class MachineTime {
        public decimal RunningTime { get; set; }
        public decimal IDLETime { get; set; }
        public decimal BreakDownTime { get; set; }
        public decimal PowerON { get; set; }

        public decimal RunningTimePerc { get; set; }
        public decimal IDLETimePerc { get; set; }
        public decimal BreakDownTimePerc { get; set; }
        public decimal PowerONPerc { get; set; }
    }

    public class Losses {
        public int ID { get; set; }
        public int LossID { get; set; }
        public string LossName { get; set; }
        public double Duration { get; set; }
    }

}
