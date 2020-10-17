using SRKSDemo.Server_Model;
using System.Collections.Generic;

namespace SRKSDemo
{
    public class CellsModel
    {
        public tblcell Cells { get; set; }
        public IEnumerable<tblcell> cellslist { get; set; }

    }
}