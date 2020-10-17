using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblmachinedetailsnew
    {
        public int MachineID { get; set; }
        public string MachineInvNo { get; set; }
        public string IPAddress { get; set; }
        public string MachineType { get; set; }
        public string ControllerType { get; set; }
        public string InsertedOn { get; set; }
        public int InsertedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> IsDeleted { get; set; }
        public string MachineModel { get; set; }
        public string MachineMake { get; set; }
        public string ModelType { get; set; }
        public string MachineDispName { get; set; }
        public int IsParameters { get; set; }
        public string ShopNo { get; set; }
        public Nullable<int> IsPCB { get; set; }
        public Nullable<int> IsLevel { get; set; }
        public Nullable<int> PlantID { get; set; }
        public Nullable<int> ShopID { get; set; }
        public Nullable<int> CellID { get; set; }

        public virtual tblcell tblcell { get; set; }
        public virtual tblplant tblplant { get; set; }
        public virtual tblshop tblshop { get; set; }
    }
}
