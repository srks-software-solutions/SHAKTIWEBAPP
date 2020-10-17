
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SRKSDemo.Server_Model;
namespace SRKSDemo
{
    public class UserModel
    {
        public tbluser Users { get; set; }

        public IEnumerable<tbluser> UsersList { get; set; }
    }
}