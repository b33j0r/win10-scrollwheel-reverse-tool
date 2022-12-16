using System;
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
        Mouse selectedMouse;

        public MainWindow()
        {
            InitializeComponent();

            Dictionary<string, Mouse> deviceDriverInfo = GetMice();
            foreach (Mouse mouse in deviceDriverInfo.Values)
            {
                deviceList.Items.Add(mouse);
            }

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
            if (selectedMouse == null)
                return;

            if (selectedMouse.FlipFlopWheel != selectedMouse.FlipFlopWheelOriginal)
            {
                selectedMouse.SetDeviceParameter(Mouse.FLIP_FLOP_WHEEL, selectedMouse.FlipFlopWheel);
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
            UpdateOptions();
        }

        private void optionScrollUpUp_Checked(object sender, RoutedEventArgs e)
        {
            if (selectedMouse != null)
            {
                selectedMouse.FlipFlopWheel = 0;
                UpdateOptions();
            }
        }

        private void optionScrollUpDown_Checked(object sender, RoutedEventArgs e)
        {
            if (selectedMouse != null)
            {
                selectedMouse.FlipFlopWheel = 1;
                UpdateOptions();
            }
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            Apply();
            UpdateOptions();
        }

        void btnApplyAndRestart_Click(object sender, RoutedEventArgs e)
        {
            Apply();
            UpdateOptions();
            RestartMachine();
        }
    }
}
