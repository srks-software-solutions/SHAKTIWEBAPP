using System;
using System.Collections.Generic;

namespace SRKSDemo.Models
{
    public class ViewModel
    {
        public IEnumerable<MachineUtilizationModel> MachineUtilizationModels { get; set; }
        public IEnumerable<OEEModel> OEEModels { get; set; }
        public IEnumerable<AlarmList> AlarmLists { get; set; }
        public IEnumerable<TargetActualList> TargetActualLists { get; set; }
        public IEnumerable<ContributingFactors> ContributingFactors { get; set; }
    }

    public class TargetActualListDet {

        //public int[] data { get; set; }
        public List<int> Target { get; set; }
        public List<int> Actual { get; set; }
        public string CellName { get; set; }
        public string[] backgroundColor { get; set; }
        public string[] borderColor { get; set; }
        public string[] Timings { get; set; }
    }

    public class TargetActualList
    {
        public double Target { get; set; }
        public double Actual { get; set; }
        public string MachineName { get; set; }
    }

    public class ContributingFactors
    {
        public int cellid { get; set; }
        public string LossCodeDescription { get; set; }
        public decimal LossDurationInHours { get; set; }
        public decimal LossPercent { get; set; }
    }

    public class MachineUtilizationModel
    {
        public int MachineID { get; set; }
        public string  CellName { get; set; }
        public string MachineName { get; set; }
        public double MachineUtiization { get; set; }
        public string Color { get; set; }
        public TimeSpan CurrentTime { get; set; }
    }

    public class AlarmList
    {
        public String MachineID { get; set; }
        public string MachineName { get; set; }
        public string AlarmNumber { get; set; }
        public string AlarmDesc { get; set; }
        public string AlarmMessage { get; set; }
        public string AxisNumber { get; set; }
        public DateTime AlarmDateTime { get; set; }
    }
}