using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo.Models
{
    public class OEEModel
    {
        public double [] data { get; set; }
        public string CellName { get; set; }
        public int CellID { get; set; }
        public int Target { get; set; }
        public int Actual { get; set; }
        public string[] backgroundColor { get; set; }
        public string[] borderColor { get; set; }
    }

    public class TopContributingFactors
    {
        
        public double[] data { get; set; }
        public string CellName { get; set; }
        public string[] backgroundColor { get; set; }
        public string[] borderColor { get; set; }
        public string[] LossName { get; set; }
        public string indexLabel { get; set; }
    }


}