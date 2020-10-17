using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo.OperatorEntryModelClass
{
    public class MainDetails
    {
        public string MachineName { get; set; }
        public string WONumber { get; set; }
        public string PartNumber { get; set; }
        public int PartsCountActual { get; set; }
        public int ShiftCountAtcual { get; set; }
        public int PartsCountTarget { get; set; }
        public int ShiftCountTarget { get; set; }
        public string MachineStatusColor { get; set; }
        public int MachineId { get; set; }
        //public string Shift { get; set; }
        public string WOStartTime { get; set; }
        public string OperationNo { get; set; }
        public int WOQty { get; set; }
    }

    public class MachineStatusData
    {
        public int MachineID { get; set; }
        public int TotalDuration { get; set; }
        public int IdleDuration { get; set; }
        public int ProductionDuration { get; set; }
        public int BreakdownDuration { get; set; }
        public int PoweroffDuration { get; set; }
    }   

    public class ReworkBreakDown
    {
        public int MachineID { get; set; }
        public string ReworkStartTime { get; set; }
        public int ReworkStart { get; set; }
        public string BreakDownStartTime { get; set; }
        public string ContentName { get; set; }
        public int BreakDownStart { get; set; }
    }

    public class MachRea
    {
        public string Content { get; set; }
        public int Machineid { get; set; }
    }

    public class PartsCountDet
    {
        public int MachineID { get; set; }
        public int PartsCountActual { get; set; }
        public int ShiftCountAtcual { get; set; }
        public int PartsCountTarget { get; set; }
        public int ShiftCountTarget { get; set; }
    }
    public class MacintanceAccp
    {
        public string MachineName { get; set; }
        public string Reason { get; set; }
        public string DateTimeDis { get; set; }
        public string Operatorname { get; set; }
        public string MaintName { get; set; }
        public string result { get; set; }
        public string AcceptTime { get; set; }
        public string finish { get; set; }
    }
    public class ProuctioneAccp
    {
        public string MachineName { get; set; }
        public string Reason { get; set; }
        public string DateTimeDis { get; set; }
        public string Operatorname { get; set; }
        public string MaintName { get; set; }
        public string result { get; set; }
        public string AcceptTime { get; set; }
        public string finish { get; set; }
    }

    public class MachNameCol
    {
        public string MchineName { get; set; }
        public int MachineID { get; set; }
        public string Colour { get; set; }
    }

    public class UtilSummerized
    {
        public string PlantName { get; set; }
        public string ShopName { get; set; }
        public string CellName { get; set; }
        public string MachineName { get; set; }
        public string DateTime { get; set; }
        public List<ShiftValue> ShiftDoubleVal { get; set; }
    }

    public class ShiftValue
    {
        public double OPTime { get; set; }
        public double CTTime { get; set; }
        public double POTime { get; set; }
        public int MachineID { get; set; }
    }

    public class IdlePopupMachine
    {
        public string machinename { get; set; }
        public int MachineID { get; set; }
        public string starttimeidle { get; set; }
        public int IdelNo { get; set; }
        public int cellidleno { get; set; }
        public List<LossCode> LLoss { get; set; }
    }

    public class LossCode
    {
        public int losscodeid { get; set; }
        public string losscode { get; set; }
        public int losslevel { get; set; }
    }

    public class WOEntry
    {
        public string ShiftID { get; set; }
        public string PartNo { get; set; }
        public string WONO { get; set; }
        public string OperationNo { get; set; }
        public int WOQTY { get; set; }
        public int PartPerCycle { get; set; }
    }

    public class MachineTrue
    {
        public string TrueorFalse { get; set; }
        public int MachineID { get; set; }
    }

    public class MachineStatus
    {
        public string ColourCode { get; set; }
        public int MachineID { get; set; }
        public int DurationInMinutes { get; set; }
        public string ModeType { get; set; }
    }

    public class StartButton
    {
        public string Shift { get; set; }
        public string WONumber { get; set; }
        public string PartNumber { get; set; }
        public int WOQty { get; set; }
    }
}