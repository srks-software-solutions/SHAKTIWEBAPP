using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo
{
    public class ModulesModel
    {
        public tblmodule Modules { get; set; }

        public IEnumerable<tblmodule> ModuleList { get; set; }
    }
}