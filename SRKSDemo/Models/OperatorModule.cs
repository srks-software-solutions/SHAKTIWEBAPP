using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using i_facility.Models;
using SRKSDemo.Server_Model;

namespace i_facility.Models
{
    public class OperatorModule
    {
        public tbloperatordetail OPDetails { get; set; }
        public IEnumerable<tbloperatordetail> OPdetailsList { get; set; }
    }
}