using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SRKSDemo.Server_Model;

namespace SRKSDemo.Models
{
    public class Reject
    {
        public string Shifttime { get; set; }
        public bool isEnable { get; set; }
        public List<Rejectdata> RList { get; set; }
    }

    public class Rejectdata
    {
        public int RejectReasonID { get; set; }
        public string RejectReason { get; set; }
        public int? RejectVal { get; set; }
    }

    public class IdlePopupMachine
    {
        public string machinename { get; set; }
        public int MachineID { get; set; }
        public string starttimeidle { get; set; }
        public int IdelNo { get; set; }
        public int cellidleno { get; set; }
        public List<LossCode> LLoss { get; set; }
    }

    public class LossCode
    {
        public int losscodeid { get; set; }
        public string losscode { get; set; }
        public int losslevel { get; set; }
    }

    //public class Rejectdataprint
    //{
    //    public string RejectReason { get; set; }
    //    public string RejectVal { get; set; }
    //}

    //public class RejectPrint
    //{
    //    public string Shifttime { get; set; }
    //    public bool isEnable { get; set; }
    //    public List<Rejectdataprint> RDPList { get; set; }
    //}
}