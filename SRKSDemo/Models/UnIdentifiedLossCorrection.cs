using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo.Models
{
    public class UnIdentifiedLossCorrection
    {
        public int LossID { get; set; }
        public string MachineName { get; set; }
        public int Level1 { get; set; }
        public int Level2 { get; set; }
        public int Level3 { get; set; }
        public DateTime SDateTime { get; set; }
        public DateTime EDateTime { get; set; }

    }
}