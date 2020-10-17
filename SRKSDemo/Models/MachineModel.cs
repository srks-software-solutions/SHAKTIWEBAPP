
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo
{
    public class MachineModel
    {
        public tblmachinedetail Machine { get; set; }
        public IEnumerable<tblmachinedetail> MachineList { get; set; }
    }
}