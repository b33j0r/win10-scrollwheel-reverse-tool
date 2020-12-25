using System;
using System.Collections.Generic;
using System.Management;
using System.Management.Automation;
using System.Windows;
using System.Windows.Input;
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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Mouse selectedMouse;

        public MainWindow()
        {
            InitializeComponent();

            Dictionary<string, Mouse> deviceDriverInfo = GetMice();
            foreach (Mouse mouse in deviceDriverInfo.Values)
            {
                deviceList.Items.Add(mouse);
            }

            _update_options();
        }

        Dictionary<string, Mouse> GetMice()
        {
            var drivers = new Dictionary<string, Mouse>();

            ManagementObjectSearcher objSearcher = new ManagementObjectSearcher(
                "select * from Win32_PnPEntity where PNPClass = 'Mouse'"
            );
            ManagementObjectCollection objCollection = objSearcher.Get();

            foreach (ManagementObject obj in objCollection)
            {
                var mouse = new Mouse(obj);
                drivers.Add(mouse.DeviceID, mouse);
            }

            return drivers;
        }

        private void _apply()
        {
            if (selectedMouse == null)
                return;

            if (selectedMouse.FlipFlopWheel != selectedMouse.FlipFlopWheelOriginal)
            {
                selectedMouse.SetDeviceParameter(Mouse.FLIP_FLOP_WHEEL, selectedMouse.FlipFlopWheel);
            }
        }

        private void _restart()
        {
            var ps = PowerShell.Create();
            ps.AddCommand("Restart-Computer");
            ps.Invoke();
        }

        void _update_options()
        {
            optionScrollUpUp.IsEnabled = (selectedMouse != null);
            optionScrollUpDown.IsEnabled = (selectedMouse != null);

            btnApply.IsEnabled = (selectedMouse != null && selectedMouse.FlipFlopWheel != selectedMouse.FlipFlopWheelOriginal);
            btnApplyAndRestart.IsEnabled = (selectedMouse != null && selectedMouse.FlipFlopWheel != selectedMouse.FlipFlopWheelOriginal);

            if (selectedMouse != null)
            {
                optionScrollUpUp.IsChecked = (selectedMouse.FlipFlopWheel == 0);
                optionScrollUpDown.IsChecked = (selectedMouse.FlipFlopWheel == 1);
            }

        }

        private void deviceList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedMouse = e.AddedItems.Count > 0 ? (Mouse)e.AddedItems[0] : null;
            _update_options();
        }

        private void optionScrollUpUp_Checked(object sender, RoutedEventArgs e)
        {
            if (selectedMouse != null)
            {
                selectedMouse.FlipFlopWheel = 0;
                _update_options();
            }
        }

        private void optionScrollUpDown_Checked(object sender, RoutedEventArgs e)
        {
            if (selectedMouse != null)
            {
                selectedMouse.FlipFlopWheel = 1;
                _update_options();
            }
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            _apply();
            _update_options();
        }

        void btnApplyAndRestart_Click(object sender, RoutedEventArgs e)
        {
            _apply();
            _update_options();
            _restart();
        }
    }
}
