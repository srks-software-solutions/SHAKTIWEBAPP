//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SRKSDemo.Server_Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbloeedashboardfinalvariable
    {
        public int OEEDashboardID { get; set; }
        public Nullable<int> PlantID { get; set; }
        public Nullable<int> ShopID { get; set; }
        public Nullable<int> CellID { get; set; }
        public Nullable<int> WCID { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<decimal> OEE { get; set; }
        public Nullable<decimal> Availability { get; set; }
        public Nullable<decimal> Performance { get; set; }
        public Nullable<decimal> Quality { get; set; }
        public Nullable<int> IsOverallPlantWise { get; set; }
        public Nullable<int> IsOverallShopWise { get; set; }
        public Nullable<int> IsOverallWCWise { get; set; }
        public string Loss1Name { get; set; }
        public Nullable<int> Loss1Value { get; set; }
        public string Loss2Name { get; set; }
        public Nullable<int> Loss2Value { get; set; }
        public string Loss3Name { get; set; }
        public Nullable<int> Loss3Value { get; set; }
        public string Loss4Name { get; set; }
        public Nullable<int> Loss4Value { get; set; }
        public string Loss5Name { get; set; }
        public Nullable<int> Loss5Value { get; set; }
        public string IPAddress { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public int IsDeleted { get; set; }
        public int IsOverallCellWise { get; set; }
        public int IsToday { get; set; }
    }
}
