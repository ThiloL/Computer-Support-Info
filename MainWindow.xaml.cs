using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Path = System.IO.Path;

using AForge.Video.DirectShow;

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
                

                ManagementClass cs = new ManagementClass("win32_baseboard");
                ManagementObjectCollection moc = cs.GetInstances();
                if (moc.Count != 0)
                {
                    foreach (ManagementObject MO in cs.GetInstances())
                    {
                        manufacturer = MO.Properties["Manufacturer"].Value.ToString();
                        model = MO.Properties["Product"].Value.ToString();
                        serial = MO.Properties["SerialNumber"].Value.ToString();
                    }
                }
            }
            catch { }

            SupportInfoList.Add(new SupportInfoElement() { Name = "Gerät", Value = $"{manufacturer} {model}, Seriennr.: {serial}" });

            // CPU

            string cpu = string.Empty;

            try
            {


                ManagementClass cs = new ManagementClass("win32_processor");
                ManagementObjectCollection moc = cs.GetInstances();
                if (moc.Count != 0)
                {
                    foreach (ManagementObject MO in cs.GetInstances())
                    {
                        cpu = MO.Properties["Name"].Value.ToString();
                   }
                }
            }
            catch { }

            SupportInfoList.Add(new SupportInfoElement() { Name = "CPU", Value = cpu });

            // Firmware

            string bios_manufacturer = string.Empty;
            string bios_version = string.Empty;
            DateTime bios_datetime = DateTime.MinValue;

            try
            {


                ManagementClass cs = new ManagementClass("win32_bios");
                ManagementObjectCollection moc = cs.GetInstances();
                if (moc.Count != 0)
                {
                    foreach (ManagementObject MO in cs.GetInstances())
                    {
                        bios_manufacturer = MO.Properties["Manufacturer"].Value.ToString();
                        bios_version = MO.Properties["SMBIOSBIOSVersion"].Value.ToString();
                        bios_datetime = ManagementDateTimeConverter.ToDateTime(MO.Properties["ReleaseDate"].Value.ToString()); 
                    }
                }
            }
            catch { }

            SupportInfoList.Add(new SupportInfoElement() { Name = "Firmware", Value = $"{bios_manufacturer} {bios_version}, {bios_datetime.ToString("dd.MM.yyyy")}" });

            // RAM

            double ram_gb = 0L;

            try
            {


                ManagementClass cs = new ManagementClass("Win32_OperatingSystem");
                ManagementObjectCollection moc = cs.GetInstances();
                if (moc.Count != 0)
                {
                    foreach (ManagementObject MO in cs.GetInstances())
                    {
                        ram_gb = double.Parse(MO.Properties["TotalVisibleMemorySize"].Value.ToString()) / 1024.0 / 1024.0;
                    }
                }
            }
            catch { }

            SupportInfoList.Add(new SupportInfoElement() { Name = "RAM", Value = ram_gb.ToString("#.0") + " GB" });

            // Webcam

            string WebCamInfo = string.Empty;

            FilterInfoCollection videoInputCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if ((videoInputCollection != null) && (videoInputCollection.Count > 0))
            {
                foreach (FilterInfo videoDevice in videoInputCollection)
                {
                    WebCamInfo += $"Webcam: {videoDevice.Name}\n";
                }

                SupportInfoList.Add(new SupportInfoElement() { Name = "Webcam", Value = WebCamInfo });
            }

            // Grafik

            string name = string.Empty;
            string driver = string.Empty;

            string videoInfo = string.Empty;

            try
            {


                ManagementClass cs = new ManagementClass("win32_videocontroller");
                ManagementObjectCollection moc = cs.GetInstances();
                if (moc.Count != 0)
                {
                    foreach (ManagementObject MO in cs.GetInstances())
                    {
                        name = MO.Properties["Name"].Value.ToString();
                        driver = MO.Properties["DriverVersion"].Value.ToString();

                        videoInfo += $"{name}, Treiber-Version: {driver}\n";
                    }

                    

                }

                if (!string.IsNullOrWhiteSpace(videoInfo)) SupportInfoList.Add(new SupportInfoElement() { Name = "Grafikkarte", Value = videoInfo });
            }
            catch { }

            


            // Bitlocker

            var bitLocker = string.Empty;

            IShellProperty prop = ShellObject.FromParsingName("C:").Properties.GetProperty("System.Volume.BitLockerProtection");
            int? bitLockerProtectionStatus = (prop as ShellProperty<int?>).Value;

            if (bitLockerProtectionStatus.HasValue && (bitLockerProtectionStatus == 1 || bitLockerProtectionStatus == 3 || bitLockerProtectionStatus == 5))
                bitLocker = "Ein";
            else
                bitLocker = "Aus";

            if (!string.IsNullOrWhiteSpace(bitLocker)) SupportInfoList.Add(new SupportInfoElement() { Name = "Bitlocker (C:)", Value = bitLocker });


            


            (sender as DataGrid).ItemsSource = SupportInfoList;
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Speedtest

            try
            {
                var full_exe_path = System.Reflection.Assembly.GetEntryAssembly().Location;
                var folder = Path.GetDirectoryName(full_exe_path);
                var full_speed_test_name = Path.Combine(folder, Properties.Settings.Default.speedtest_exe_name);

                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    FileName = full_speed_test_name,
                    UseShellExecute = false,
                    LoadUserProfile = false,
                    Arguments = Properties.Settings.Default.speedtest_parameters,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                Process p = new Process() { StartInfo = psi };

                Mouse.OverrideCursor = Cursors.Wait;

                p.Start();
                p.WaitForExit(30000);

                Mouse.OverrideCursor = null;

                string output = p.StandardOutput.ReadToEnd();

                MessageBox.Show(output, "Speedtest", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Fehler beim Speedtest!", "Speedtest", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
