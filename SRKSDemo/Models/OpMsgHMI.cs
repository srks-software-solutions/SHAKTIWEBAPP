using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo.Models
{
    public class OpMsgHMI
    {
        public string MsgNo { set; get; }
        public string MsgDesc { set; get; }
        public string OccurredOn { set; get; }

        public IEnumerable<OpMsgHMI> OpMsgList { get; set; }
    }
}