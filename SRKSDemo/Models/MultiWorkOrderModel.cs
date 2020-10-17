using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo
{
    public class MultiWorkOrderModel
    {
        public tblmultipleworkorder Multiworkorder { get; set; }
        public IEnumerable<tblmultipleworkorder> MultiOwrkOrderList { get; set; }
    }
}