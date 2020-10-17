using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo
{
    public class RoleModuleModel
    {
        public tblrolemodulelink RoleModule { get; set; }

        public List<tblrolemodulelink> RoleModuleList { get; set; }
    }
}