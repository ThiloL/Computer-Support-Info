using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Computer_Support_Info
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<SupportInfoElement> SupportInfoList;

        public MainWindow()
        {
            InitializeComponent();
            SupportInfoList = new List<SupportInfoElement>();
        }

        private void SupportInfosGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var user = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

            SupportInfoList = new List<SupportInfoElement>();

            SupportInfoList.Add(new SupportInfoElement() { Name = "Benutzername", Value = user });
            SupportInfoList.Add(new SupportInfoElement() { Name = "Computername", Value = Environment.MachineName });

            var versionString = (string)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("productName");
            var releaseID = (string)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("ReleaseID");
            var currentBuildNumber = (string)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("currentBuildNumber");
            var ubr = (int)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("UBR");

            var x64 = Environment.Is64BitOperatingSystem ? "x64" : "x86";

            SupportInfoList.Add(new SupportInfoElement() { Name = "Windows", Value = $"{versionString}, Build: {currentBuildNumber}, Version: {releaseID}, Patch-Level: {ubr}, Architektur: {x64}" });

            string drive_string = string.Empty;

            try
            {
                foreach(var d in DriveInfo.GetDrives())
                {
                    if (!d.DriveType.Equals(DriveType.Fixed)) continue;

                    var fds = ((double)d.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0).ToString("F");

                    drive_string += $"Laufwerk: {d.Name}, Speicherplatz (frei): {fds} GB\n";
                }

                if (!string.IsNullOrEmpty(drive_string))
                {
                    SupportInfoList.Add(new SupportInfoElement() { Name = "Laufwerke", Value = drive_string });

                }
            }
            catch { }

            DriveInfo[] allDrives = DriveInfo.GetDrives();


            string net_info = string.Empty;

            try 
            {
                var ip = string.Empty;

                foreach (var n in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (n.OperationalStatus.Equals(OperationalStatus.Down)) continue;
                    if (n.NetworkInterfaceType.Equals(NetworkInterfaceType.Loopback)) continue;

                    IPInterfaceProperties ipip = n.GetIPProperties();

                    if (ipip.GatewayAddresses == null) continue;

                    if (ipip.GatewayAddresses.Count.Equals(0)) continue;



                    foreach(var u in ipip.UnicastAddresses)
                    {
                        if (!u.Address.AddressFamily.Equals(System.Net.Sockets.AddressFamily.InterNetwork)) continue;

                        ip += $"{n.Description}, IP: {u.Address}\n";
                    }


                }

                if (!string.IsNullOrEmpty(ip)) SupportInfoList.Add(new SupportInfoElement() { Name = "Netzwerk", Value = ip });
            }
            catch { }

            // Ping

            Ping ping = new Ping();

            try
            {
                PingReply pr = ping.Send(new System.Net.IPAddress(new byte[] { 8, 8, 8, 8 }), 1000);

                var ping_result = pr.Status.ToString();
                var ping_ms = pr.RoundtripTime.ToString();

                SupportInfoList.Add(new SupportInfoElement() { Name = "Ping (8.8.8.8)", Value = $"Ergebnis: {ping_result}, {ping_ms} ms" });

            }
            catch { }

            // HW

            string manufacturer = string.Empty;
            string model = string.Empty;
            string serial = string.Empty;

            try
            {
                

                ManagementClass cs = new ManagementClass("win32_computersystemproduct");
                ManagementObjectCollection moc = cs.GetInstances();
                if (moc.Count != 0)
                {
                    foreach (ManagementObject MO in cs.GetInstances())
                    {
                        manufacturer = MO.Properties["Vendor"].Value.ToString();
                        model = MO.Properties["Name"].Value.ToString();
                        serial = MO.Properties["IdentifyingNumber"].Value.ToString();
                    }
                }
            }
            catch { }

            SupportInfoList.Add(new SupportInfoElement() { Name = "Gerät", Value = $"{manufacturer} {model}, Seriennr.: {serial}" });






            (sender as DataGrid).ItemsSource = SupportInfoList;
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

    }
}
