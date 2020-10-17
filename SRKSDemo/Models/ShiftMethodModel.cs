using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SRKSDemo.Server_Model;
namespace SRKSDemo
{
    public class ShiftMethodModel
    {
        public tblshiftmethod ShiftMethod { get; set; }

        public IEnumerable<tblshiftmethod> ShiftMethodList { get; set; }

    }
}