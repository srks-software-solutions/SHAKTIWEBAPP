using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo.Models
{
    public class PMSEntry
    {
        public int checkpointid { get; set; }
        public string checkpointname { get; set; }
        public List<pmchecklistdata> checklistdata { get; set; }
    }
    public class pmchecklistdata
    {
        public int checklistid { get; set; }
        public string checklistname { get; set; }
        public string Frequency { get; set; }
        public string Value { get; set; }
        public string How { get; set; }
    }
    public class pmshistdata
    {
        public int work { get; set; }
        public string remarks { get; set; }
        public List<pmshistdata> pmsdet { get; set; }
    }
}