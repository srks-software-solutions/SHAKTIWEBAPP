using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo
{
    public class ProductionLossModel
    {
        public tbllossescode ProductionLoss { get; set; }
        public IEnumerable<tbllossescode> ProductionLossList { get; set; }
    }
}