namespace SRKSDemo.Models
{
    public class CTAModel
    {
        public int CTAID { get; set; }
        public int MachineID { get; set; }
        public string CorrectedDate { get; set; }
        public int PartsCount { get; set; }
        public decimal Std_CycleTime { get; set; }
        public decimal AvgLoadTimeinMinutes { get; set; }
        public decimal CuttingTime { get; set; }
        public decimal AvgCuttingTime{ get; set; }
        public string PartNum { get; set; }
        // public string Std_CycleTimeUnit { get; set; }
        public decimal OperatingTime { get; set; }
        //   public string OperatingTimeUnit { get; set; }
        public decimal AvgOperatingTime { get; set; }
        //  public string AvgOperatingTimeUnit { get; set; }
        public decimal Std_LoadTime { get; set; }
        //  public string Std_LoadTimeUnit { get; set; }
        public decimal TotalLoadTime { get; set; }
        // public string TotalLoadTimeUnit { get; set; }
        public decimal LossTime { get; set; }
        //  public string LossTimeUnit { get; set; }
    }
}