using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo
{
    public class PlantModel
    {
        public tblplant Plant { get; set; }

        public IEnumerable<tblplant> PlantList { get; set; }
    }
}