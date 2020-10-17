using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo.Models
{
    public class MachineConnectivityStatusModel
    {

        public string plantName { get; set; }
        public string shopName { get; set; }
        public string cellName { get; set; }
        public List<Machines> machines { get; set; }
        public int MachineID { get; set; }
        public string MachineName { get; set; }
        public string PowerOnTime { get; set; }
        public string RunningTime { get; set; }
        public string CuttingTime { get; set; }
        public string CurrentStatus { get; set; }
        public string ExeProgramName { get; set; }
        public int? PartsCount { get; set; }
        public string Color { get; set; }
        public string IdleTime { get; set; }
        public string CycleTime { get; set; }

        public string CuttingRatio{get;set;}

    }


    public class GetMachines {
        public string plantName { get; set; }
        public string shopName { get; set; }
        public string cellName { get; set; }
        public List<Machines> machines { get; set; }
        public List<MachineConnectivityStatusModel> machineModel { get; set; }
    }

    public class Machines
    {
        public int MachineID { get; set; }
        public string MachineName { get; set; }
        public string PowerOnTime { get; set; }
        public string RunningTime { get; set; }
        public string CuttingTime { get; set; }
        public string CurrentStatus { get; set; }
        public string ExeProgramName { get; set; }
        public int? PartsCount { get; set; }
        public string Color { get; set; }
        public string IdleTime { get; set; }
        public string CycleTime { get; set; }
        public string Time { get; set; }
    }
}