using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo
{
    public class PartsManagement
    {

        public tblpart MasterParts { get; set; }
        public IEnumerable<tblpart> MasterPartsList { get; set; }
    }
}