using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo
{
    public class PmCheckList
    {
        public configuration_tblpmchecklist pmchecklist { get; set; }
        public IEnumerable<configuration_tblpmchecklist> pmchecklistlist { get; set; }
    }
}