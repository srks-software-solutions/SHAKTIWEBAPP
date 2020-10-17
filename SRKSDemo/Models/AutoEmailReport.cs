using SRKSDemo.Server_Model;
using System.Collections.Generic;

namespace SRKSDemo
{
    public class AutoEmailReport
    {
        public tbl_autoreportsetting Autoemailreport { get; set; }
        public IEnumerable<tbl_autoreportsetting> Autoreportemaillist { get; set; }
    }
}