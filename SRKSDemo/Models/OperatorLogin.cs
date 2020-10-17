using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo.Models
{
    public class OperatorLogin
    {
        public tblOperatorLoginDetail operatorLogin { get; set; }
        public IEnumerable<tblOperatorLoginDetail> operatorLoginList { get; set; }
    }
}