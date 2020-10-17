using i_facilitylibrary.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblplant
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblplant()
        {
            this.tbl_autoreportsetting = new HashSet<tbl_autoreportsetting>();
            this.tblcells = new HashSet<tblcell>();
            this.tblemailescalations = new HashSet<tblemailescalation>();
            this.tblmachinedetails = new HashSet<tblmachinedetail>();
            this.tblmachinedetailsnews = new HashSet<tblmachinedetailsnew>();
            this.tblmultipleworkorders = new HashSet<tblmultipleworkorder>();
            this.tblshops = new HashSet<tblshop>();
            this.tblshiftplanners = new HashSet<tblshiftplanner>();
        }

        public int PlantID { get; set; }
        public string PlantName { get; set; }
        public string PlantDesc { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_autoreportsetting> tbl_autoreportsetting { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblcell> tblcells { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblemailescalation> tblemailescalations { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblmachinedetail> tblmachinedetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblmachinedetailsnew> tblmachinedetailsnews { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblmultipleworkorder> tblmultipleworkorders { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblshop> tblshops { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblshiftplanner> tblshiftplanners { get; set; }
    }
}
