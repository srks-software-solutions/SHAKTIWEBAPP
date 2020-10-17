using i_facilitylibrary.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblmachinedetail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblmachinedetail()
        {
            this.tbl_autoreportsetting = new HashSet<tbl_autoreportsetting>();
            this.tbldailyprodstatus = new HashSet<tbldailyprodstatu>();
            this.tblemailescalations = new HashSet<tblemailescalation>();
            this.tblhmiscreens = new HashSet<tblhmiscreen>();
            this.tbllivedailyprodstatus = new HashSet<tbllivedailyprodstatu>();
            this.tbllivehmiscreens = new HashSet<tbllivehmiscreen>();
            this.tbllivehmiscreenreps = new HashSet<tbllivehmiscreenrep>();
            this.tbllivemodes = new HashSet<tbllivemode>();
            this.tbllivemodedbs = new HashSet<tbllivemodedb>();
            this.tblmachineallocations = new HashSet<tblmachineallocation>();
            this.tblmimics = new HashSet<tblmimic>();
            this.tblshiftplanners = new HashSet<tblshiftplanner>();
            this.tblshiftdetails_machinewise = new HashSet<tblshiftdetails_machinewise>();
            this.tblmodes = new HashSet<tblmode>();
            this.tblusers = new HashSet<tbluser>();
            this.tblmultipleworkorders = new HashSet<tblmultipleworkorder>();
            this.tblpartwiseworkcenters = new HashSet<tblpartwiseworkcenter>();
        }

        public int MachineID { get; set; }
        public string InsertedOn { get; set; }
        public int InsertedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> IsDeleted { get; set; }
        public Nullable<int> PlantID { get; set; }
        public Nullable<int> ShopID { get; set; }
        public Nullable<int> CellID { get; set; }
        public string MachineName { get; set; }
        public string MachineDescription { get; set; }
        public string MachineDisplayName { get; set; }
        public Nullable<int> CellOrderNo { get; set; }
        public string IPAddress { get; set; }
        public Nullable<int> MachineType { get; set; }
        public string ControllerType { get; set; }
        public string MachineModel { get; set; }
        public string MachineMake { get; set; }
        public string ModelType { get; set; }
        public Nullable<int> IsParameters { get; set; }
        public string ShopNo { get; set; }
        public Nullable<int> IsPCB { get; set; }
        public Nullable<int> IsLevel { get; set; }
        public Nullable<int> IsNormalWC { get; set; }
        public Nullable<int> ManualWCID { get; set; }
        public Nullable<int> NoOfAxis { get; set; }
        public string MacType { get; set; }
        public Nullable<int> CurrentControlAxis { get; set; }
        public string ProgramNum { get; set; }
        public string ProgDBit { get; set; }
        public int MachineModelType { get; set; }
        public string MacConnName { get; set; }
        public string SpindleAxis { get; set; }
        public string TabIPAddress { get; set; }
        public Nullable<int> MachineLockBit { get; set; }
        public Nullable<int> MachineSetupBit { get; set; }
        public Nullable<int> MachineMaintBit { get; set; }
        public Nullable<int> MachineToolLifeBit { get; set; }
        public Nullable<int> MachineUnlockBit { get; set; }
        public Nullable<int> MachineIdleBit { get; set; }
        public Nullable<int> MachineIdleMin { get; set; }
        public int EnableLockLogic { get; set; }
        public int ServerTabFlagSync { get; set; }
        public int ServerTabCheck { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }
        public Nullable<bool> EnableToolLife { get; set; }
        public Nullable<int> IsBottelNeck { get; set; }
        public Nullable<int> IsFirstMachine { get; set; }
        public Nullable<int> IsLastMachine { get; set; }
        public Nullable<int> OperationNumber { get; set; }
        public int IsShiftWise { get; set; }
        public Nullable<int> LossFlag { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_autoreportsetting> tbl_autoreportsetting { get; set; }
        public virtual tblcell tblcell { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbldailyprodstatu> tbldailyprodstatus { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblemailescalation> tblemailescalations { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblhmiscreen> tblhmiscreens { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbllivedailyprodstatu> tbllivedailyprodstatus { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbllivehmiscreen> tbllivehmiscreens { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbllivehmiscreenrep> tbllivehmiscreenreps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbllivemode> tbllivemodes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbllivemodedb> tbllivemodedbs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblmachineallocation> tblmachineallocations { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblmimic> tblmimics { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblshiftplanner> tblshiftplanners { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblshiftdetails_machinewise> tblshiftdetails_machinewise { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblmode> tblmodes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbluser> tblusers { get; set; }
        public virtual tblplant tblplant { get; set; }
        public virtual tblshop tblshop { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblmultipleworkorder> tblmultipleworkorders { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblpartwiseworkcenter> tblpartwiseworkcenters { get; set; }
    }

}
