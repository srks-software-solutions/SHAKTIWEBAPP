using System;

namespace SRKSDemo.Models
{
    public class LossDetails
    {
        public int CellID { get; set; }
        public int? LossID { get; set; }
        public string LossCodeDescription { get; set; }
        public DateTime LossStartTime { get; set; }
        public DateTime LossEndTime { get; set; }
        public double DurationinMin { get; set; }
    }
}
