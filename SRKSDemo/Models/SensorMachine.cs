using SRKSDemo.Server_Model;
using System.Collections.Generic;

namespace SRKSDemo.Models
{
    public class SensorMachine
    {
        public int MachineID { get; set; }
        public int sid { get; set; }
        public string machineName { get; set; }
        public string SensorName { get; set; }

        public List<SensorMachine> sensorGroupsList { get; set; }
    }
    public class sensormachinemodel
    {
        public configurationtblmachinesensor machinesensor { get; set; }
        public List<configurationtblmachinesensor> machinesensorList { get; set; }
    }
}