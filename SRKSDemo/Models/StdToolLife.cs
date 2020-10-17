using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SRKSDemo.Server_Model;
namespace SRKSDemo.Models
{
    public class StdToolLife
    {
        public tblStdToolLife tblStdToolLife { get; set; }
        public IEnumerable<tblStdToolLife> tblStdToolLifeList { get; set; }
    }
}