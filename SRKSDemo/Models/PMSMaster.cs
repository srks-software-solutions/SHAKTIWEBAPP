using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRKSDemo.Models
{
    public class PMSMaster
    {
        public configuration_TblPMSNotification_Master pMSNotification_Master { get; set; }
        public IEnumerable<configuration_TblPMSNotification_Master> pMSNotification_Masters { get; set; }
    
    }
}
