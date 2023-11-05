using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Computer_Support_Info
{
    /// <summary>
    /// Interaktionslogik für InstalledApps.xaml
    /// </summary>
    public partial class InstalledApps : Window
    {
        List<InstalledApp> InstalledAppList = new List<InstalledApp>();

        public InstalledApps()
        {
            InitializeComponent();

            Load();

            DataContext = InstalledAppList;

            this.Title += " (" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ") - Installierte Apps";

            Rect workArea = SystemParameters.WorkArea;

            this.Height = workArea.Height / 1.25;
            this.Width = workArea.Width / 2;

            this.Left = (workArea.Width - this.Width) / 2 + workArea.Left;
            this.Top = (workArea.Height - this.Height) / 2 + workArea.Top;

            this.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFD8E8E5"));
        }

        private void Load()
        {
            string displayName;
            string displayVersion;
            RegistryKey key;

            key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            foreach (String keyName in key.GetSubKeyNames())
            {
                RegistryKey subkey = key.OpenSubKey(keyName);
                displayName = subkey.GetValue("DisplayName") as string;
                displayVersion = subkey.GetValue("DisplayVersion") as string;

                if (string.IsNullOrEmpty(displayName)) continue;
                if ((subkey.GetValue("SystemComponent") as int?) == 1) continue;

                InstalledAppList.Add(new InstalledApp()
                {
                    Caption = displayName,
                    Version = displayVersion,
                    ComputerOrUser = "Benutzer"
                });
            }

            // search in: LocalMachine_32
            key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            foreach (String keyName in key.GetSubKeyNames())
            {
                RegistryKey subkey = key.OpenSubKey(keyName);
                displayName = subkey.GetValue("DisplayName") as string;
                displayVersion = subkey.GetValue("DisplayVersion") as string;

                if (string.IsNullOrEmpty(displayName)) continue;
                if ((subkey.GetValue("SystemComponent") as int?) == 1) continue;

                InstalledAppList.Add(new InstalledApp()
                {
                    Caption = displayName,
                    Version = displayVersion,

                    ComputerOrUser = "Computer (x64)"
                });
            }

            // search in: LocalMachine_64
            key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            foreach (String keyName in key.GetSubKeyNames())
            {
                RegistryKey subkey = key.OpenSubKey(keyName);
                displayName = subkey.GetValue("DisplayName") as string;
                displayVersion = subkey.GetValue("DisplayVersion") as string;

                if (string.IsNullOrEmpty(displayName)) continue;
                if ((subkey.GetValue("SystemComponent") as int?) == 1) continue;

                InstalledAppList.Add(new InstalledApp()
                {
                    Caption = displayName,
                    Version = displayVersion,
                    ComputerOrUser = "Computer (x86)"
                });
            }

            return;


            ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT Name FROM win32_product");
            //ManagementClass cs = new ManagementClass("Win32reg_addremoveprograms");
            ManagementObjectCollection moc = mos.Get();
            if (moc.Count != 0)
            {
                foreach (ManagementObject MO in moc)
                {
                    var caption = MO.Properties["Caption"]?.Value?.ToString();

                    //var type = MO.Properties["MediaType"]?.Value?.ToString();

                    //if (type == null) continue;
                    //if (!type.Equals("fixed hard disk media", StringComparison.InvariantCultureIgnoreCase)) continue;

                    //DiskDrives.Add(new DiskDrive()
                    //{
                    //    Caption = MO.Properties["Caption"].Value.ToString(),
                    //    Size = Convert.ToInt64(MO.Properties["Size"].Value),
                    //    SerialNumber = MO.Properties["SerialNumber"].Value.ToString(),
                    //    Firmware = MO.Properties["FirmwareRevision"].Value.ToString(),
                    //    Index = Convert.ToInt32(MO.Properties["Index"].Value)
                    //});
                }

                // sortieren
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
