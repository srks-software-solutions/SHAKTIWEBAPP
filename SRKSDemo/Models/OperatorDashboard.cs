using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo.Models
{
    public class OperatorDashboard
    {
        public tblOperatorDashboard OpDashboard { get; set; }

        public IEnumerable<tblOperatorDashboard> OpDashboardList { get; set; }
    }
}