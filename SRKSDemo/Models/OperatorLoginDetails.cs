using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo.Models
{
    public class OperatorLoginDetails
    {
        public int operatorLoginId { get; set; }
        public string operatorUserName { get; set; }
        public string operatorPwd { get; set; }
        public int NumOfMachines { get; set; }
        public string operatorMobileNo { get; set; }
        public string operatorEmailId { get; set; }
        public int roleId { get; set; }
        public int isDeleted { get; set; }
        public string machineids { get; set; }
        public int operatorId { get; set; }
        public string operatorName { get; set; }
    }
}