
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SRKSDemo.Models
{
    public class pms
    {
        public List<PreventiveMaintainanceScheduling> pmsList { get; set; }
    }


    public class PreventiveMaintainanceScheduling
    {
        public virtual List<configuration_tblprimitivemaintainancescheduling> primitivemaintainancescheduling { get; set; }
       
        
    }
 public class preventiveList
    {
        public int pmid { get; set; }
        public int MachineID { get; set; }
        public int Month { get; set; }
        public int Week { get; set; }
        public string MachineName { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int Isdeleted { get; set; }
        public string PlantName { get; set; }
        public string shopName { get; set; }
        public int plantID { get; set; }
        public int ShopID { get; set; }
        public int cellID { get; set; }
        public string cellName { get; set; }

        public List<preventiveList> preventList { get; set; }


    }
}