using System.Collections.Generic;

namespace SRKSDemo.Models
{
    public class Health
    {
        public string Time { get; set; }
        public int value { get; set; }
        public int sensorvalueid { get; set; }
    }

    public class MachineHealth {
        public decimal? LSL { get; set; }
        public decimal? USL { get; set; }
        public int min { get; set; }
        public int max { get; set; }
        public string unit { get; set; }
        public List<Health> MachineHealthdet{get;set;}
    }
}