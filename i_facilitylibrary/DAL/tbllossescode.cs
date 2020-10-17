using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tbllossescode
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbllossescode()
        {
            this.tblbreakdowns = new HashSet<tblbreakdown>();
            this.tblemailescalations = new HashSet<tblemailescalation>();
            this.tblemailescalations1 = new HashSet<tblemailescalation>();
            this.tblemailescalations2 = new HashSet<tblemailescalation>();
            //this.tblescalationlogs = new HashSet<tblescalationlog>();
            this.tbllivelossofentries = new HashSet<tbllivelossofentry>();
            this.tbllivelossofentryreps = new HashSet<tbllivelossofentryrep>();
            this.tbllivemanuallossofentries = new HashSet<tbllivemanuallossofentry>();
            this.tbllivemanuallossofentryreps = new HashSet<tbllivemanuallossofentryrep>();
            this.tbllossofentries = new HashSet<tbllossofentry>();
            this.tblmanuallossofentries = new HashSet<tblmanuallossofentry>();
        }

        public int LossCodeID { get; set; }
        public string LossCode { get; set; }
        public string LossCodeDesc { get; set; }
        public string MessageType { get; set; }
        public int LossCodesLevel { get; set; }
        public Nullable<int> LossCodesLevel1ID { get; set; }
        public Nullable<int> LossCodesLevel2ID { get; set; }
        public string ContributeTo { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> EndCode { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblbreakdown> tblbreakdowns { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblemailescalation> tblemailescalations { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblemailescalation> tblemailescalations1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblemailescalation> tblemailescalations2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        // public virtual ICollection<tblescalationlog> tblescalationlogs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbllivelossofentry> tbllivelossofentries { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbllivelossofentryrep> tbllivelossofentryreps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbllivemanuallossofentry> tbllivemanuallossofentries { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbllivemanuallossofentryrep> tbllivemanuallossofentryreps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbllossofentry> tbllossofentries { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblmanuallossofentry> tblmanuallossofentries { get; set; }

    }
}
