using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tbloeedashboardvariable
    {
        public int OEEVariablesID { get; set; }
        public Nullable<int> PlantID { get; set; }
        public Nullable<int> ShopID { get; set; }
        public Nullable<int> CellID { get; set; }
        public Nullable<int> WCID { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<double> MinorLosses { get; set; }
        public Nullable<double> Blue { get; set; }
        public Nullable<double> Green { get; set; }
        public Nullable<double> SettingTime { get; set; }
        public Nullable<double> ROALossess { get; set; }
        public Nullable<double> DownTimeBreakdown { get; set; }
        public Nullable<double> SummationOfSCTvsPP { get; set; }
        public Nullable<double> ScrapQtyTime { get; set; }
        public Nullable<double> ReWOTime { get; set; }
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
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public int IsDeleted { get; set; }
    }
}
