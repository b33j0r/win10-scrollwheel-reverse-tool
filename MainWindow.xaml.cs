using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Windows;
using System.Windows.Input;

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

        public bool IsModified
        {
            get
            {
                foreach (Mouse mouse in deviceList.Items)
                {
                    if (mouse.FlipFlopWheel != mouse.FlipFlopWheelOriginal)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private void Apply()
        {
            if (!IsModified)
                return;

            foreach (Mouse mouse in deviceList.Items)
            {
                if (mouse.FlipFlopWheel != mouse.FlipFlopWheelOriginal)
                {
                    mouse.SetDeviceParameter(Mouse.FLIP_FLOP_WHEEL, mouse.FlipFlopWheel);
                }
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
            optionScrollUpUp.IsEnabled = true;
            optionScrollUpDown.IsEnabled = true;
            if (_selectedMouse != null)
            {
                optionScrollUpUp.IsChecked = (_selectedMouse.FlipFlopWheel == 0);
                optionScrollUpUp.FontWeight = (_selectedMouse.FlipFlopWheelOriginal == 0)
                    ? FontWeights.Bold : FontWeights.Normal;

                optionScrollUpDown.IsChecked = (_selectedMouse.FlipFlopWheel == 1);
                optionScrollUpDown.FontWeight = (_selectedMouse.FlipFlopWheelOriginal == 1)
                    ? FontWeights.Bold : FontWeights.Normal;
            }

            btnApply.FontWeight = IsModified ? FontWeights.Bold : FontWeights.Normal;
            btnApplyAndRestart.FontWeight = IsModified ? FontWeights.Bold : FontWeights.Normal;
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
            else foreach (Mouse mouse in deviceList.Items)
                {
                    mouse.FlipFlopWheel = 0;
                }
            UpdateOptions();
        }

        private void optionScrollUpDown_Checked(object sender, RoutedEventArgs e)
        {
            if (_selectedMouse != null)
                _selectedMouse.FlipFlopWheel = 1;
            else foreach (Mouse mouse in deviceList.Items)
                {
                    mouse.FlipFlopWheel = 1;
                }
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

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
