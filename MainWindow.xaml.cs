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
using Vanara.PInvoke;
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
            SupportInfosGrid1.Items.Refresh();
            SupportInfosGrid2.Items.Refresh();

            MainGrid.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFD8E8E5"));
            Mouse.OverrideCursor = null;

            RepositionWindows();

        }

        private void RepositionWindows()
        {
            Rect workArea = SystemParameters.WorkArea;
            this.Left = (workArea.Width - this.Width) / 2 + workArea.Left;
            this.Top = (workArea.Height - this.Height) / 2 + workArea.Top;
        }

        private void Load2()
        {

        }

        public class Taskhelper
        {
            public SupportInfotype support_info_type { get; set; }
            public int number { get; set; }
            public int col { get; set; }
        }

        private void Background_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int no = 1;

            List<Taskhelper> th = new List<Taskhelper>();
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.UserName, number = no++, col = 1 });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.IsAdmin, number = no++, col = 1 });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.ComputerName, number = no++, col = 1 });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.OperatingSystem, number = no++, col = 1 });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.WindowsVersionInfo, number = no++, col = 1 });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.ComputerManufacturer, number = no++, col = 1 });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.ComputerModel, number = no++, col = 1 });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.Firmware, number = no++, col = 1 });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.SerialBaseboard, number = no++, col = 1 });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.SerialBios, number = no++, col = 1 });

            th.Add(new Taskhelper() { support_info_type = SupportInfotype.CPU, number = no++, col = 1 });
            
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.Memory, number = no++, col = 1 });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.Network, number = no++, col = 1 });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.Ping, number = no++, col = 1 });
            
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.Bitlocker, number = no++, col = 1 });

            th.Add(new Taskhelper() { support_info_type = SupportInfotype.GraphicsCard, number = no++, col = 2 });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.Display, number = no++, col = 2 });

            th.Add(new Taskhelper() { support_info_type = SupportInfotype.PhysicalDrives, number = no++, col = 2 });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.LogicalDrives, number = no++, col = 2 });

            th.Add(new Taskhelper() { support_info_type = SupportInfotype.Webcam, number = no++, col = 2 });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.AudioOutDevices, number = no++, col = 2 });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.AudioInDevices, number = no++, col = 2 });

            Parallel.ForEach(th, one  =>
            {
                SupportInfoElement r = LoadData(one.support_info_type, one.number, one.col);
                AddGridItem(r);
            });
        }

        private void AddGridItem(SupportInfoElement Item)
        {
            //Item.Number = ItemNumber++;

            Application.Current.Dispatcher.Invoke
            (
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() =>
                {
                    vm.AddItem(Item);
                    vm.ViewSource1.View.Refresh();
                    vm.ViewSource2.View.Refresh();
                    RepositionWindows();
                }
                )
            );
        }

        private SupportInfoElement LoadData(SupportInfotype sit, int number, int col)
        {
            if (sit == SupportInfotype.UserName)
            {
                var user = WindowsIdentity.GetCurrent().Name;

                StringBuilder sbu = new StringBuilder(1024);
                uint sbu_size = (uint)sbu.Capacity;

                var fn = string.Empty;

                //fn = System.DirectoryServices.AccountManagement.UserPrincipal.Current.GivenName;

                               

                return new SupportInfoElement() { Name = "Benutzername", Value = $"{user} {fn}", Number = number, Column = col};
            }

            if (sit == SupportInfotype.IsAdmin)
            {
                var current_user_sid = WindowsIdentity.GetCurrent().User.Value;

                bool CurrentUserIsmemberOfAdminGroup = false;
                try
                {
                    var administrator_group_sid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);

                    using (PrincipalContext pc = new PrincipalContext(ContextType.Machine, Environment.MachineName))
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

                if (CurrentUserIsmemberOfAdminGroup)
                    return new SupportInfoElement() { Name = "Administrative Rechte", Value = "JA", MakeBold = true, Number = number, Column = col };
                else
                    return new SupportInfoElement() { Name = "Administrative Rechte", Value = "NEIN", Number = number, Column = col };
            }

            if (sit == SupportInfotype.ComputerName)
            {
                return new SupportInfoElement() { Name = "Computername", Value = Environment.MachineName, Number = number, Column = col };
            }

            if (sit == SupportInfotype.OperatingSystem)
            {
                var versionString = (string)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("productName");
                var releaseID = (string)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("ReleaseID");
                var x64 = Environment.Is64BitOperatingSystem ? "x64" : "x86";

                return new SupportInfoElement() { Name = "Betriebssystem-Edition | -Release | -Architektur", Value = $"{versionString}  |  {releaseID}  |  {x64}", Number = number, Column = col };
            }

            if (sit == SupportInfotype.WindowsVersionInfo)
            {
                var os_major = (int)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("CurrentMajorVersionNumber");
                var os_minor = (int)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("CurrentMinorVersionNumber");
                var currentBuildNumber = (string)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("currentBuildNumber");
                var ubr = (int)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("UBR");

                return new SupportInfoElement() { Name = "Betriebssystem-Version", Value = $"{os_major}.{os_minor}.{currentBuildNumber}.{ubr}", Number = number, Column = col };
            }

            if (sit == SupportInfotype.ComputerManufacturer)
            {
                string manufacturer = string.Empty;

                try
                {


                    ManagementClass cs = new ManagementClass("win32_baseboard");
                    ManagementObjectCollection moc = cs.GetInstances();
                    if (moc.Count != 0)
                    {
                        foreach (ManagementObject MO in cs.GetInstances())
                        {
                            manufacturer = MO.Properties["Manufacturer"].Value.ToString();
                        }
                    }

                }
                catch { }

                return new SupportInfoElement() { Name = "Hersteller", Value = manufacturer, Number = number, Column = col };
                
            }

            if (sit == SupportInfotype.ComputerModel)
            {
                string model = string.Empty;

                try
                {


                    ManagementClass cs = new ManagementClass("win32_baseboard");
                    ManagementObjectCollection moc = cs.GetInstances();
                    if (moc.Count != 0)
                    {
                        foreach (ManagementObject MO in cs.GetInstances())
                        {
                            model = MO.Properties["Product"].Value.ToString();
                        }
                    }

                }
                catch { }

                return new SupportInfoElement() { Name = "Modell", Value =model, Number = number, Column = col };

            }

            if (sit == SupportInfotype.SerialBaseboard)
            {
                string serial = string.Empty;

                try
                {
                    ManagementClass cs = new ManagementClass("win32_baseboard");
                    ManagementObjectCollection moc = cs.GetInstances();
                    if (moc.Count != 0)
                    {
                        foreach (ManagementObject MO in cs.GetInstances())
                        {
                            serial = MO.Properties["SerialNumber"].Value.ToString();
                        }
                    }
                }
                catch { }

                return new SupportInfoElement() { Name = "Serien-Nummer (Baseboard)", Value = $"{serial}", Number = number, Column = col };
            }

            if (sit == SupportInfotype.SerialBios)
            {
                string serial = string.Empty;

                try
                {
                    ManagementClass cs = new ManagementClass("win32_bios");
                    ManagementObjectCollection moc = cs.GetInstances();
                    if (moc.Count != 0)
                    {
                        foreach (ManagementObject MO in cs.GetInstances())
                        {
                            serial = MO.Properties["SerialNumber"].Value.ToString();
                        }
                    }
                }
                catch { }

                return new SupportInfoElement() { Name = "Serien-Nummer (BIOS)", Value = $"{serial}", Number = number, Column = col };
            }

            if (sit == SupportInfotype.CPU)
            {
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
                return new SupportInfoElement() { Name = "CPU", Value = cpu, Number = number, Column = col };
            }

            if (sit == SupportInfotype.Firmware)
            {
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

                return new SupportInfoElement() { Name = "Firmware-Hersteller | -Version | -Datum", Value = $"{bios_manufacturer}  |  {bios_version}  | {bios_datetime.ToString("dd.MM.yyyy")}", Number = number, Column = col };
            }

            if (sit == SupportInfotype.Memory)
            {
                // RAM

                string ram = string.Empty;

                try
                {


                    ManagementClass cs = new ManagementClass("Win32_OperatingSystem");
                    ManagementObjectCollection moc = cs.GetInstances();
                    if (moc.Count != 0)
                    {
                        foreach (ManagementObject MO in cs.GetInstances())
                        {
                            ram = Convert.ToInt64(MO.Properties["TotalVisibleMemorySize"].Value).Kilobytes().Humanize("#.#");
                        }
                    }
                }
                catch { }

                return new SupportInfoElement() { Name = "RAM", Value = ram, Number = number, Column = col };
            }

            if (sit == SupportInfotype.PhysicalDrives)
            {
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
                        }

                        // sortieren
                        DiskDrives = DiskDrives.OrderBy(x => x.Index).ToList();
                    }
                }
                catch { }

                return new SupportInfoElement() { 
                    Name = "Laufwerke (physikalisch)", 
                    Value = string.Join("\n", DiskDrives.Select(x => x.ToString())), 
                    Number = number, 
                    Column = col };
            }

            if (sit == SupportInfotype.LogicalDrives)
            {
                // logical drives

                List<LogicalVolume> L = new List<LogicalVolume>();

                string drive_string = string.Empty;

                try
                {
                    foreach (var d in DriveInfo.GetDrives())
                    {
                        if (!d.DriveType.Equals(DriveType.Fixed)) continue;

                        L.Add(new LogicalVolume()
                        {
                            Name = d.Name,
                            TotalSpace = d.TotalSize,
                            FreeSpace = d.AvailableFreeSpace
                        });

                    }
                }
                catch { }

                return new SupportInfoElement() { 
                    Name = "Laufwerke (logisch)", 
                    Value = string.Join("\n", L.Select(x => x.ToString())), 
                    Number = number, 
                    Column = col };
            }

            if (sit == SupportInfotype.Network)
            {
                // netzwerk info

                List<NetworkInfo> NetworkAdapter = new List<NetworkInfo>();
                string net_info = string.Empty;

                try
                {
                    

                    foreach (var n in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (n.NetworkInterfaceType.Equals(NetworkInterfaceType.Loopback)) continue;

                        
                        //if (n.OperationalStatus.Equals(OperationalStatus.Down)) continue;
                        

                        IPInterfaceProperties ipip = n.GetIPProperties();

                        if ((ipip.GatewayAddresses == null) || (ipip.GatewayAddresses.Count.Equals(0)))
                        {
                            NetworkAdapter.Add(new NetworkInfo() { AdapterName = n.Description, Speed = n.Speed });
                            continue;
                        };

                        foreach (var u in ipip.UnicastAddresses)
                        {
                            if (!u.Address.AddressFamily.Equals(System.Net.Sockets.AddressFamily.InterNetwork)) continue;

                            NetworkAdapter.Add(new NetworkInfo() { AdapterName = n.Description, Speed = n.Speed, IP = u.Address.ToString() });
                        }


                    }

                    
                }
                catch { }

                return new SupportInfoElement() { 
                    Name = "Netzwerk", 
                    Value = string.Join("\n", NetworkAdapter.Select(x => x.ToString())), 
                    Number = number, 
                    Column = col };
            }

            if (sit == SupportInfotype.Ping)
            {
                // Ping

                string ping_info = string.Empty;

                Ping ping = new Ping();

                try
                {
                    PingReply pr = ping.Send(new System.Net.IPAddress(new byte[] { 8, 8, 8, 8 }), 1000);

                    var ping_result = pr.Status.ToString();
                    var ping_ms = pr.RoundtripTime.ToString();

                    ping_info = $"Ergebnis: {ping_result}, {ping_ms} ms";

                }
                catch { }

                return new SupportInfoElement() { Name = "Ping (8.8.8.8)", Value = ping_info, Number = number, Column = col };
            }

            if (sit == SupportInfotype.Webcam)
            {
                // Webcam

                string WebCamInfo = string.Empty;

                FilterInfoCollection videoInputCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if ((videoInputCollection != null) && (videoInputCollection.Count > 0))
                {
                    foreach (FilterInfo videoDevice in videoInputCollection)
                    {
                        WebCamInfo += $"{videoDevice.Name}\n";
                    }

                }

                return new SupportInfoElement() { Name = "Webcam", Value = WebCamInfo, Number = number, Column = col };
            }

            if (sit == SupportInfotype.AudioOutDevices)
            {
                // Audio

                string AudioOutInfo = string.Empty;


                try
                {
                    for (int i = -1; i < WaveOut.DeviceCount; i++)
                    {
                        var c = WaveOut.GetCapabilities(i);
                        AudioOutInfo += c.ProductName + "\n";
                    }

                    
                }
                catch { }

                return new SupportInfoElement() { Name = "Audio (Out)", Value = AudioOutInfo, Number = number, Column = col };
            }

            if (sit == SupportInfotype.AudioInDevices)
            {
                string AudioInInfo = string.Empty;

                try
                {
                    for (int i = -1; i < WaveIn.DeviceCount; i++)
                    {
                        var c = WaveIn.GetCapabilities(i);
                        AudioInInfo += c.ProductName + "\n";
                    }
                   
                }
                catch { }

                return new SupportInfoElement() { Name = "Audio (In)", Value = AudioInInfo, Number = number, Column = col };
            }

            if (sit == SupportInfotype.GraphicsCard)
            {
                // Grafik

                List<GraphicsAdapter> G = new List<GraphicsAdapter>();

                try
                {
                    ManagementClass cs = new ManagementClass("win32_videocontroller");
                    ManagementObjectCollection moc = cs.GetInstances();
                    if (moc.Count != 0)
                    {
                        foreach (ManagementObject MO in cs.GetInstances())
                        {
                            G.Add(new GraphicsAdapter()
                            {
                                Name = MO.Properties["Name"].Value.ToString(),
                                DriverVersion = MO.Properties["DriverVersion"].Value.ToString()
                            });
                        }
                    }
                }
                catch { }

                return new SupportInfoElement() { Name = "Grafikkarte", Value = string.Join("\n", G.Select(x => x.ToString())), Number = number, Column = col };
            }

            if (sit == SupportInfotype.Display)
            {
                List<DisplayInfo> D = new List<DisplayInfo>();

                try
                {
                    ManagementClass cs = new ManagementClass("win32_desktopmonitor");
                    ManagementObjectCollection moc = cs.GetInstances();
                    if (moc.Count != 0)
                    {
                        foreach (ManagementObject MO in cs.GetInstances())
                        {
                            D.Add(new DisplayInfo()
                            {
                                Manufacturer = MO.Properties["MonitorManufacturer"].Value != null ? MO.Properties["MonitorManufacturer"].Value.ToString() : string.Empty,
                                Name = MO.Properties["MonitorType"].Value != null ? MO.Properties["MonitorType"].Value.ToString() : string.Empty
                            });
                        }
                    }
                }
                catch { }

                return new SupportInfoElement() { 
                    Name = "Monitor", 
                    Value = string.Join("\n", D.Select(x => x.ToString())), 
                    Number = number, 
                    Column = col };
            }

            if (sit == SupportInfotype.Bitlocker)
            {
                // Bitlocker

                var bitLocker = string.Empty;

                IShellProperty prop = ShellObject.FromParsingName("C:").Properties.GetProperty("System.Volume.BitLockerProtection");
                int? bitLockerProtectionStatus = (prop as ShellProperty<int?>).Value;

                if (bitLockerProtectionStatus.HasValue && (bitLockerProtectionStatus == 1 || bitLockerProtectionStatus == 3 || bitLockerProtectionStatus == 5))
                    bitLocker = "Ein";
                else
                    bitLocker = "Aus";

                return new SupportInfoElement() { Name = "Bitlocker (C:)", Value = bitLocker, Number = number, Column = col };
            }

            return null;
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
