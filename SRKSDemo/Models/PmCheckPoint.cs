using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo
{
    public class PmCheckPoint
    {
        public configuration_tblpmcheckpoint pmCheckPoint { get; set; }
        public IEnumerable<configuration_tblpmcheckpoint> pmCheckPointlist { get; set; }
    }
}