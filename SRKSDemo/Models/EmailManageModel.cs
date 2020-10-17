using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo
{
    public class EmailManageModel
    {
        public tblmailid Email { get; set; }

        public IEnumerable<tblmailid> EmailList { get; set; }
    }
}