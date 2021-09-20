using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Principal;
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

using Humanizer;
using AForge.Video.DirectShow;
using NAudio.Wave;
using System.ComponentModel;
using System.Reflection;

namespace Computer_Support_Info
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker background_worker = new BackgroundWorker();
        public ViewModel vm;
        int ItemNumber = 1;

        public MainWindow()
        {
            InitializeComponent();

            this.Title += " (" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ")";

            vm = new ViewModel();
            DataContext = vm;

            background_worker.WorkerReportsProgress = false;
            background_worker.DoWork += Background_worker_DoWork;
            background_worker.RunWorkerCompleted += Background_worker_RunWorkerCompleted;

            Mouse.OverrideCursor = Cursors.Wait;

            background_worker.RunWorkerAsync();
        }

        private void Background_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MainGrid.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFD8E8E5"));
            Mouse.OverrideCursor = null;

            Rect workArea = SystemParameters.WorkArea;
            this.Left = (workArea.Width - this.Width) / 2 + workArea.Left;
            this.Top = (workArea.Height - this.Height) / 2 + workArea.Top;
        }

        private void Background_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadData();
        }

        private void AddGridItem(SupportInfoElement Item)
        {
            Item.Number = ItemNumber++;

            Application.Current.Dispatcher.Invoke
            (
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() =>
                {
                    vm.AddItem(Item);
                }
                )
            );
        }

        private void LoadData()
        {
            var user = WindowsIdentity.GetCurrent().Name;
            var current_user_sid = WindowsIdentity.GetCurrent().User.Value;

            AddGridItem(new SupportInfoElement() { Name = "Benutzername", Value = user });

            bool CurrentUserIsmemberOfAdminGroup = false;
            try
            {
                var administrator_group_sid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);

                using (PrincipalContext pc = new PrincipalContext(ContextType.Machine,Environment.MachineName))
                {
                    GroupPrincipal gp = GroupPrincipal.FindByIdentity(pc, IdentityType.Sid, administrator_group_sid.Value);
                    if (gp != null)
                    {
                        PrincipalCollection members = gp.Members;
                        if (members != null)
                        {
                            foreach (Principal p in members)
                            {
                                if (p.Sid != null)
                                {
                                    if (p.Sid.Value.Equals(current_user_sid)) CurrentUserIsmemberOfAdminGroup = true;
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            //SupportInfoList = new List<SupportInfoElement>();



            if (CurrentUserIsmemberOfAdminGroup) AddGridItem(new SupportInfoElement() { Name = "Administrative Rechte", Value = "JA", MakeBold = true });

            AddGridItem(new SupportInfoElement() { Name = "Computername", Value = Environment.MachineName });

            // windows version

            var versionString = (string)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("productName");
            var releaseID = (string)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("ReleaseID");
            var currentBuildNumber = (string)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("currentBuildNumber");
            var ubr = (int)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("UBR");

            var x64 = Environment.Is64BitOperatingSystem ? "x64" : "x86";

            AddGridItem(new SupportInfoElement() { Name = "Windows", Value = $"{versionString}, Build: {currentBuildNumber}, Version: {releaseID}, Patch-Level: {ubr}, Architektur: {x64}" });

            // HW

            string manufacturer = string.Empty;
            string model = string.Empty;
            string serial = string.Empty;
            string serial_bios = string.Empty;

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

                ManagementClass cs2 = new ManagementClass("win32_bios");
                ManagementObjectCollection moc2 = cs.GetInstances();
                if (moc2.Count != 0)
                {
                    foreach (ManagementObject MO in cs2.GetInstances())
                    {
                        serial_bios = MO.Properties["SerialNumber"].Value.ToString();
                    }
                }

            }
            catch { }

            AddGridItem(new SupportInfoElement() { Name = "Gerät", Value = $"{manufacturer} {model}" });
            AddGridItem(new SupportInfoElement() { Name = "Serien-Nummer (Baseboard)", Value = $"{serial}" });
            AddGridItem(new SupportInfoElement() { Name = "Serien-Nummer (BIOS)", Value = $"{serial_bios}" });

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

            AddGridItem(new SupportInfoElement() { Name = "CPU", Value = cpu });

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

            AddGridItem(new SupportInfoElement() { Name = "Firmware", Value = $"{bios_manufacturer} {bios_version}, {bios_datetime.ToString("dd.MM.yyyy")}" });

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

            AddGridItem(new SupportInfoElement() { Name = "RAM", Value = ram_gb.ToString("#.0") + " GB" });


            // physical disk drives

            List<DiskDrive> DiskDrives = new List<DiskDrive>();

            string disk_info = string.Empty;

            try
            {
                string caption = string.Empty;
                string size_text = string.Empty;

                ManagementClass cs = new ManagementClass("win32_diskdrive");
                ManagementObjectCollection moc = cs.GetInstances();
                if (moc.Count != 0)
                {
                    foreach (ManagementObject MO in cs.GetInstances())
                    {
                        var type = MO.Properties["MediaType"]?.Value?.ToString();

                        if (type == null) continue;
                        if (!type.Equals("fixed hard disk media", StringComparison.InvariantCultureIgnoreCase)) continue;

                        DiskDrives.Add(new DiskDrive()
                        {
                            Caption = MO.Properties["Caption"].Value.ToString(),
                            Size = Convert.ToInt64(MO.Properties["Size"].Value),
                            SerialNumber = MO.Properties["SerialNumber"].Value.ToString(),
                            Index = Convert.ToInt32(MO.Properties["Index"].Value)
                        });

                        //caption = MO.Properties["Caption"].Value.ToString();
                        //size = Convert.ToInt64(MO.Properties["Size"].Value);
                        //size_text = (size / 1024.0 / 1024.0 / 1024.0).ToString("F");

                        //disk_info += $"{caption}, {size_text} GB\n";
                    }

                    // sortieren
                    DiskDrives = DiskDrives.OrderBy(x => x.Index).ToList();

                    foreach(DiskDrive d in DiskDrives)
                    {
                        disk_info += string.Format("{0}  |  {1}  |  Serien-Nr.: {2}  |  Größe: {3}\n", d.Index, d.Caption, d.SerialNumber, d.DiskSizeText);
                    }
                }
            }
            catch { }

            AddGridItem(new SupportInfoElement() { Name = "Laufwerke (physikalisch)", Value = $"{disk_info}" });

            // logical drives

            string drive_string = string.Empty;

            try
            {
                foreach(var d in DriveInfo.GetDrives())
                {
                    if (!d.DriveType.Equals(DriveType.Fixed)) continue;

                    var fds = (d.AvailableFreeSpace).Bytes().Humanize("#.#");

                    drive_string += $"Laufwerk: {d.Name}  |  Speicherplatz (frei): {fds}\n";
                }

                if (!string.IsNullOrEmpty(drive_string))
                {
                    AddGridItem(new SupportInfoElement() { Name = "Laufwerke (logisch)", Value = drive_string });

                }
            }
            catch { }

            DriveInfo[] allDrives = DriveInfo.GetDrives();

            // netzwerk info

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

                if (!string.IsNullOrEmpty(ip)) AddGridItem(new SupportInfoElement() { Name = "Netzwerk", Value = ip });
            }
            catch { }

            // Ping

            Ping ping = new Ping();

            try
            {
                PingReply pr = ping.Send(new System.Net.IPAddress(new byte[] { 8, 8, 8, 8 }), 1000);

                var ping_result = pr.Status.ToString();
                var ping_ms = pr.RoundtripTime.ToString();

                AddGridItem(new SupportInfoElement() { Name = "Ping (8.8.8.8)", Value = $"Ergebnis: {ping_result}, {ping_ms} ms" });

            }
            catch { }


            // Webcam

            string WebCamInfo = string.Empty;

            FilterInfoCollection videoInputCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if ((videoInputCollection != null) && (videoInputCollection.Count > 0))
            {
                foreach (FilterInfo videoDevice in videoInputCollection)
                {
                    WebCamInfo += $"Webcam: {videoDevice.Name}\n";
                }

                AddGridItem(new SupportInfoElement() { Name = "Webcam", Value = WebCamInfo });
            }

            // Audio

            string AudioOutInfo = string.Empty;


            try
            {
                for (int i = -1; i < WaveOut.DeviceCount; i++)
                {
                    var c = WaveOut.GetCapabilities(i);
                    AudioOutInfo += c.ProductName + "\n";
                }

                AddGridItem(new SupportInfoElement() { Name = "Audio (Out)", Value = AudioOutInfo });
            } 
            catch { }

            string AudioInInfo = string.Empty;

            try
            {
                for (int i = -1; i < WaveIn.DeviceCount; i++)
                {
                    var c = WaveIn.GetCapabilities(i);
                    AudioInInfo += c.ProductName + "\n";
                }

                AddGridItem(new SupportInfoElement() { Name = "Audio (In)", Value = AudioInInfo });
            }
            catch { }



            //FilterInfoCollection audioCollection = new FilterInfoCollection(FilterCategory.AudioInputDevice);

            //if ((audioCollection != null) && (audioCollection.Count > 0))
            //{
            //    foreach (FilterInfo audioDevice in audioCollection)
            //    {
            //        AudioInfo += $"Audio: {audioDevice.Name}\n";
            //    }

            //    SupportInfoList.Add(new SupportInfoElement() { Name = "Audio", Value = AudioInfo });
            //}

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

                if (!string.IsNullOrWhiteSpace(videoInfo)) AddGridItem(new SupportInfoElement() { Name = "Grafikkarte", Value = videoInfo });
            }
            catch { }

            // Monitor


            string monitorInfo = string.Empty;

            try
            {


                ManagementClass cs = new ManagementClass("win32_desktopmonitor");
                ManagementObjectCollection moc = cs.GetInstances();
                if (moc.Count != 0)
                {
                    foreach (ManagementObject MO in cs.GetInstances())
                    {
                        string monitor_manufacturer = string.Empty;
                        string monitor_type = string.Empty;

                        monitor_manufacturer = MO.Properties["MonitorManufacturer"].Value != null ? MO.Properties["MonitorManufacturer"].Value.ToString() : string.Empty;
                        monitor_type = MO.Properties["MonitorType"].Value != null ? MO.Properties["MonitorType"].Value.ToString() : string.Empty;

                        monitorInfo += $"{monitor_manufacturer} {monitor_type}\n";
                    }



                }

                if (!string.IsNullOrWhiteSpace(monitorInfo)) AddGridItem(new SupportInfoElement() { Name = "Monitor", Value = monitorInfo });
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

            if (!string.IsNullOrWhiteSpace(bitLocker)) AddGridItem(new SupportInfoElement() { Name = "Bitlocker (C:)", Value = bitLocker });


        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            background_worker.CancelAsync();
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
                p.WaitForExit(120000);

                Mouse.OverrideCursor = null;

                string output = p.StandardOutput.ReadToEnd();

                MessageBox.Show(output, "Speedtest", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Fehler beim Speedtest!", "Speedtest", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            WindowsUpdates WindowsUpdatesWindow = new WindowsUpdates();
            WindowsUpdatesWindow.ShowDialog();
        }
    }
}
