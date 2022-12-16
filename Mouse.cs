using System.Management;
//using System.Management.Automation;

namespace ScrollWheelReverseForWin10
{
    public class Mouse
    {
        public const string FLIP_FLOP_WHEEL = "FlipFlopWheel";

        ManagementObject managementObject;

        public Mouse(ManagementObject managementObject)
        {
            this.managementObject = managementObject;
            this.FlipFlopWheel = FlipFlopWheelOriginal;
        }

        public string Description => this.managementObject["Description"].ToString();
        public string Name => this.managementObject["Name"].ToString();
        public string Manufacturer => this.managementObject["Manufacturer"].ToString();
        public string DeviceID => this.managementObject["DeviceID"].ToString();
        public string PNPDeviceID => this.managementObject["PNPDeviceID"].ToString();
        public string Status => this.managementObject["Status"].ToString();
        
        public string RegistryKey => "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Enum\\" + DeviceID + "\\Device Parameters";

        public int FlipFlopWheel { set; get; }
        public int FlipFlopWheelOriginal => GetDeviceParameter(FLIP_FLOP_WHEEL);

        public int GetDeviceParameter(string name)
        {
            return (int)Microsoft.Win32.Registry.GetValue(RegistryKey, name, 0);
        }

        public void SetDeviceParameter(string name, object value)
        {
            Microsoft.Win32.Registry.SetValue(RegistryKey, name, value);
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, DeviceID);
        }
    }
}
