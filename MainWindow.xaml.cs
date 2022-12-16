using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Windows;
using System.Windows.Input;
//using System.Management.Automation;

namespace ScrollWheelReverseForWin10
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Mouse _selectedMouse;

        public MainWindow()
        {
            InitializeComponent();

            Dictionary<string, Mouse> deviceDriverInfo = GetMice();
            var sortedDeviceDriverInfo = deviceDriverInfo.OrderBy(d => d.Value.Name.StartsWith("HID")).Select(d => d.Value).ToList();
            foreach (var item in sortedDeviceDriverInfo)
                deviceList.Items.Add(item);
            UpdateOptions();
        }

        Dictionary<string, Mouse> GetMice()
        {
            var drivers = new Dictionary<string, Mouse>();

            ManagementObjectSearcher objSearcher = new ManagementObjectSearcher(
                "select * from Win32_PnPEntity where PNPClass = 'Mouse'"
            );
            ManagementObjectCollection objCollection = objSearcher.Get();

            foreach (ManagementObject obj in objCollection.Cast<ManagementObject>())
            {
                var mouse = new Mouse(obj);
                drivers.Add(mouse.DeviceID, mouse);
            }

            return drivers;
        }

        private void Apply()
        {
            if (_selectedMouse == null)
                return;

            if (_selectedMouse.FlipFlopWheel != _selectedMouse.FlipFlopWheelOriginal)
            {
                _selectedMouse.SetDeviceParameter(Mouse.FLIP_FLOP_WHEEL, _selectedMouse.FlipFlopWheel);
            }
        }

        private void RestartMachine()
        {
            var ps = PowerShell.Create();
            ps.AddCommand("Restart-Computer");
            ps.Invoke();
        }

        void UpdateOptions()
        {
            optionScrollUpUp.IsEnabled = (_selectedMouse != null);
            optionScrollUpDown.IsEnabled = (_selectedMouse != null);

            btnApply.IsEnabled = (_selectedMouse != null && _selectedMouse.FlipFlopWheel != _selectedMouse.FlipFlopWheelOriginal);
            btnApplyAndRestart.IsEnabled = (_selectedMouse != null && _selectedMouse.FlipFlopWheel != _selectedMouse.FlipFlopWheelOriginal);

            if (_selectedMouse != null)
            {
                optionScrollUpUp.IsChecked = (_selectedMouse.FlipFlopWheel == 0);
                optionScrollUpDown.IsChecked = (_selectedMouse.FlipFlopWheel == 1);
            }

        }

        private void deviceList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selectedMouse = e.AddedItems.Count > 0 ? (Mouse)e.AddedItems[0] : null;
            UpdateOptions();
        }

        private void optionScrollUpUp_Checked(object sender, RoutedEventArgs e)
        {
            if (_selectedMouse != null)
                _selectedMouse.FlipFlopWheel = 0;
            UpdateOptions();
        }

        private void optionScrollUpDown_Checked(object sender, RoutedEventArgs e)
        {
            if (_selectedMouse != null)
                _selectedMouse.FlipFlopWheel = 1;
            UpdateOptions();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            Apply();
            UpdateOptions();
        }

        void ApplyAndRestart_Click(object sender, RoutedEventArgs e)
        {
            Apply();
            UpdateOptions();
            RestartMachine();
        }
    }
}
