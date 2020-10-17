using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo.Models
{
    public class OEECalModel
    {
        public decimal OperatingTime { get; set; }
        public decimal LossTime { get; set; }
        public decimal MinorLossTime { get; set; }
        public decimal MntTime { get; set; }
        public decimal SetupTime { get; set; }
        public decimal SetupMinorTime { get; set; }
        public decimal PowerOffTime { get; set; }
        public decimal PowerONTime { get; set; }
    }
}