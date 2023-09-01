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
using static Vanara.PInvoke.Gdi32;
using System.Runtime.InteropServices;

using WindowsDisplayAPI;
using WindowsDisplayAPI.Native;
using WindowsDisplayAPI.Native.DisplayConfig;
using System.Threading;

namespace Computer_Support_Info
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CancellationTokenSource cts = null;
        ParallelOptions po = null;

        private BackgroundWorker background_worker = null;
        public ViewModel vm;

        bool IsConnectedToInternet = false;

        public MainWindow()
        {
            InitializeComponent();

            cts = new CancellationTokenSource();
            po = new ParallelOptions() { CancellationToken = cts.Token, MaxDegreeOfParallelism = System.Environment.ProcessorCount };

            background_worker = new BackgroundWorker() { WorkerSupportsCancellation = true };

            this.Title += " (" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ")";

            vm = new ViewModel();
            DataContext = vm;

            // check internet connection
            WinINet.INTERNET_CONNECTION ic;
            IsConnectedToInternet = WinINet.InternetGetConnectedState(out ic, 0);

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
            SupportInfosGrid3.Items.Refresh();

            MainGrid.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFD8E8E5"));
            Mouse.OverrideCursor = null;

            RepositionWindows();

        }

        private void RepositionWindows()
        {
            Rect workArea = SystemParameters.WorkArea;
            this.Height = workArea.Height / 1.25;
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

            public string number_prefix { get; set; }
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

            th.Add(new Taskhelper() { support_info_type = SupportInfotype.WebcamDetails, number_prefix = "WC", number = 1, col = 3 });

            try
            {

                Parallel.ForEach(th, po, one =>
                {
                    List<SupportInfoElement> Results = LoadData(one.support_info_type, one.number, one.col);
                    foreach (SupportInfoElement sie in Results) AddGridItem(sie);
                });

            }
            catch (OperationCanceledException ex)
            {
                this.Close();
            }
            finally
            {
                cts.Dispose();
            }
        }

        // add item(s) 
        private void AddGridItem(SupportInfoElement Item)
        {
            if (System.Windows.Application.Current == null) return;
            System.Windows.Application.Current.Dispatcher.Invoke
            (
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() =>
                {
                    vm.AddItem(Item);
                    vm.ViewSource1.View.Refresh();
                    vm.ViewSource2.View.Refresh();
                    vm.ViewSource3.View.Refresh();
                    RepositionWindows();
                })
            );
        }


        private List<SupportInfoElement> LoadData(SupportInfotype sit, int number, int col, string number_prefix = "")
        {
            
            if (sit == SupportInfotype.UserName)
            {
                var user = WindowsIdentity.GetCurrent().Name;

                StringBuilder sbu = new StringBuilder(1024);
                uint sbu_size = (uint)sbu.Capacity;

                var fn = string.Empty;

                //fn = System.DirectoryServices.AccountManagement.UserPrincipal.Current.GivenName;

                               

                return new List<SupportInfoElement> { 
                    new SupportInfoElement() { 
                        Name = "Benutzername", 
                        Value = $"{user} {fn}", 
                        Number = number, 
                        Column = col 
                    } 
                };
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
                    return new List<SupportInfoElement> {
                        new SupportInfoElement() { 
                            Name = "Administrative Rechte", 
                            Value = "JA", 
                            MakeBold = true, 
                            Number = number, 
                            Column = col
                        } 
                    };
                else
                    return new List<SupportInfoElement> {
                        new SupportInfoElement() { 
                            Name = "Administrative Rechte", 
                            Value = "NEIN", 
                            Number = number, 
                            Column = col 
                        }
                    };
            }

            if (sit == SupportInfotype.ComputerName)
            {
                return new List<SupportInfoElement> {
                    new SupportInfoElement() {
                        Name = "Computername",
                        Value = Environment.MachineName,
                        Number = number,
                        Column = col 
                    }
                };
            }

            if (sit == SupportInfotype.OperatingSystem)
            {
                var versionString = (string)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("productName");
                var releaseID = (string)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("ReleaseID");
                var x64 = Environment.Is64BitOperatingSystem ? "x64" : "x86";

                List<SupportInfoElement> O = new List<SupportInfoElement>();

                int N = 1;

                O.Add(new SupportInfoElement()
                {
                    Name = "Betriebssystem",
                    Value = versionString,
                    Number = number,
                    SubNumber = N++,
                    Column = col
                });

                O.Add(new SupportInfoElement()
                {
                    Name = "-Release",
                    Value = releaseID,
                    Number = number,
                    SubNumber = N++,
                    Column = col
                });

                O.Add(new SupportInfoElement()
                {
                    Name = "-Architektur",
                    Value = x64,
                    Number = number,
                    SubNumber = N++,
                    Column = col
                });

                var os_major = (int)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("CurrentMajorVersionNumber");
                var os_minor = (int)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("CurrentMinorVersionNumber");
                var currentBuildNumber = (string)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("currentBuildNumber");
                var ubr = (int)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("UBR");

                O.Add(new SupportInfoElement()
                {
                    Name = "-Version",
                    Value = $"{os_major}.{os_minor}.{currentBuildNumber}.{ubr}",
                    Number = number,
                    SubNumber = N++,
                    Column = col
                });

                return O;
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

                return new List<SupportInfoElement> {
                    new SupportInfoElement() {
                        Name = "Hersteller",
                        Value = manufacturer,
                        Number = number,
                        Column = col 
                    }
                };
                
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

                return new List<SupportInfoElement> {
                    new SupportInfoElement() {
                        Name = "Modell",
                        Value =model,
                        Number = number,
                        Column = col 
                    }
                };

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

                return new List<SupportInfoElement> {
                    new SupportInfoElement() {
                        Name = "Serien-Nummer (Baseboard)",
                        Value = $"{serial}",
                        Number = number,
                        Column = col 
                    }
                };
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

                return new List<SupportInfoElement> {
                    new SupportInfoElement() {
                        Name = "Serien-Nummer (BIOS)",
                        Value = $"{serial}",
                        Number = number,
                        Column = col
                    }
                };
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
                
                return new List<SupportInfoElement> {
                    new SupportInfoElement() {
                        Name = "CPU",
                        Value = cpu,
                        Number = number,
                        Column = col 
                    }
                };
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

                List<SupportInfoElement> F = new List<SupportInfoElement>();

                int N = 1;

                F.Add(new SupportInfoElement()
                {
                    Name = "Firmware-Hersteller",
                    Value = bios_manufacturer,
                    Number = number,
                    SubNumber = N++,
                    Column = col
                });

                F.Add(new SupportInfoElement()
                {
                    Name = "-Version",
                    Value = bios_version,
                    Number = number,
                    SubNumber = N++,
                    Column = col
                });

                F.Add(new SupportInfoElement()
                {
                    Name = "-Datum",
                    Value = bios_datetime.ToString("dd.MM.yyyy"),
                    Number = number,
                    SubNumber = N++,
                    Column = col
                });

                return F;

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

                return new List<SupportInfoElement> {
                    new SupportInfoElement() {
                        Name = "RAM",
                        Value = ram,
                        Number = number,
                        Column = col
                    }
                };
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

                return new List<SupportInfoElement> {
                    new SupportInfoElement() {
                        Name = "Laufwerk (phys.)",
                        Value = string.Join("\n", DiskDrives.Select(x => x.ToString())),
                        Number = number,
                        Column = col
                    }
                };
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

                return new List<SupportInfoElement> {
                    new SupportInfoElement() {
                        Name = "Laufwerk (log.)",
                        Value = string.Join("\n", L.Select(x => x.ToString())),
                        Number = number,
                        Column = col
                    }
                };
            }

            if (sit == SupportInfotype.Network)
            {
                // netzwerk info

                int N = 1;

                List<SupportInfoElement> C = new List<SupportInfoElement>();

                List<NetworkInfo> NetworkAdapter = new List<NetworkInfo>();
                //string net_info = string.Empty;

                string ip = string.Empty;

                try
                {
                    

                    foreach (var n in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (n.NetworkInterfaceType.Equals(NetworkInterfaceType.Loopback)) continue;

                        IPInterfaceProperties ipip = n.GetIPProperties();

                        if ((ipip.GatewayAddresses == null) || (ipip.GatewayAddresses.Count.Equals(0)))
                        {
                            NetworkAdapter.Add(new NetworkInfo() { AdapterName = n.Description, Speed = n.Speed });
                            continue;
                        };

                        foreach (var u in ipip.UnicastAddresses)
                        {
                            if (!u.Address.AddressFamily.Equals(System.Net.Sockets.AddressFamily.InterNetwork)) continue;

                            ip = u.Address.ToString();

                            NetworkAdapter.Add(new NetworkInfo() { AdapterName = "■ " + n.Description, Speed = n.Speed });
                        }


                    }

                    
                }
                catch { }


                C.Add(
                    new SupportInfoElement() {
                        Name = "Netzwerk",
                        Value = string.Join("\n", NetworkAdapter.Select(x => x.ToString())),
                        Number = number,
                        SubNumber = N++,
                        Column = col
                    }
                );

                if (!string.IsNullOrEmpty(ip))
                C.Add(
                    new SupportInfoElement()
                    {
                        Name = "IP-Adresse",
                        Value = ip,
                        Number = number,
                        SubNumber = N++,
                        Column = col
                    }
                );

                return C;


            }

            if (sit == SupportInfotype.Ping)
            {
                // Ping

                if (!IsConnectedToInternet)
                {
                    MenuInternetSpeed.IsEnabled = false;

                    return new List<SupportInfoElement> {
                        new SupportInfoElement() {
                            Name = "Ping (8.8.8.8)",
                            Value = "Keine Internetverbindung",
                            Number = number,
                            Column = col,
                            MakeBold = true
                        }
                    };
                }

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

                return new List<SupportInfoElement> {
                    new SupportInfoElement() {
                        Name = "Ping (8.8.8.8)",
                        Value = ping_info,
                        Number = number,
                        Column = col
                    }
                };
            }

            if (sit == SupportInfotype.Webcam)
            {
                // Webcam

                List<WebcamInfo> W = new List<WebcamInfo>();

                FilterInfoCollection videoInputCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if ((videoInputCollection != null) && (videoInputCollection.Count > 0))
                {
                    foreach (FilterInfo videoDevice in videoInputCollection)
                    {
                        List<ResolutionAndFramerate> R = new List<ResolutionAndFramerate>();
                        //VideoCaptureDevice vcd = new VideoCaptureDevice(videoDevice.MonikerString);

                        //if (vcd != null)
                        //{
                        //    foreach(VideoCapabilities vc in vcd.VideoCapabilities)
                        //    {

                        //    }
                        //}

                        W.Add(new WebcamInfo()
                        {
                            Name = videoDevice.Name,
                            Resolutions = R
                        });       
                    }

                }

                return new List<SupportInfoElement> {
                    new SupportInfoElement() {
                        Name = "Webcam",
                        Value = string.Join("\n", W.Select(x => x.ToString())),
                        Number = number,
                        Column = col
                    }
                };
            }

            if (sit == SupportInfotype.WebcamDetails)
            {
                // Webcam-Details

                List<WebcamInfo> W = new List<WebcamInfo>();

                FilterInfoCollection videoInputCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if ((videoInputCollection != null) && (videoInputCollection.Count > 0))
                {
                    foreach (FilterInfo videoDevice in videoInputCollection)
                    {
                        List<ResolutionAndFramerate> R = new List<ResolutionAndFramerate>();
                        VideoCaptureDevice vcd = new VideoCaptureDevice(videoDevice.MonikerString);

                        if (vcd != null)
                        {
                            foreach (VideoCapabilities vc in vcd.VideoCapabilities)
                            {
                                R.Add(new ResolutionAndFramerate() {
                                    Width = vc.FrameSize.Width,
                                    Height = vc.FrameSize.Height,
                                    Framerate = vc.AverageFrameRate
                                });
                            }
                        }

                        W.Add(new WebcamInfo()
                        {
                            Name = videoDevice.Name,
                            Resolutions = R
                        });
                    }

                }

                List<SupportInfoElement> L = new List<SupportInfoElement>();

                foreach(WebcamInfo wci in W)
                {
                    L.Add(new SupportInfoElement()
                    {
                        Name = wci.Name,
                        Value = wci.ResultionText,
                        Column = 3,
                        MakeBold = false,
                        Number = number++
                    });
                }

                return L;
            }

            if (sit == SupportInfotype.AudioOutDevices)
            {
                // Audio (out)

                List<string> audio_out = new List<string>();

                try
                {
                    for (int i = -1; i < WaveOut.DeviceCount; i++)
                    {
                        var c = WaveOut.GetCapabilities(i);
                        audio_out.Add(c.ProductName);
                    }
                }
                catch { }

                if (audio_out.Count > 0)
                    return new List<SupportInfoElement> {
                        new SupportInfoElement() {
                            Name = "Audio (Out)",
                            Value = audio_out.Aggregate((x,y) => x + "\n" + y).ToString(),
                            Number = number,
                            Column = col
                        }
                    };
            }

            if (sit == SupportInfotype.AudioInDevices)
            {
                // Audio (in)

                List<string> audio_in = new List<string>();

                try
                {
                    for (int i = -1; i < WaveIn.DeviceCount; i++)
                    {
                        var c = WaveIn.GetCapabilities(i);
                        audio_in.Add(c.ProductName);
                    }
                   
                }
                catch { }

                if (audio_in.Count > 0)
                return new List<SupportInfoElement> {
                    new SupportInfoElement() {
                        Name = "Audio (In)",
                        Value = audio_in.Aggregate((x,y) => x + "\n" + y).ToString(),
                        Number = number,
                        Column = col
                    }
                };
            }

            if (sit == SupportInfotype.GraphicsCard)
            {
                // Grafik

                List<GraphicsAdapter> G = new List<GraphicsAdapter>();

                try
                {
                    DisplayAdapter[] all_da = DisplayAdapter.GetDisplayAdapters().ToArray();
                    foreach (DisplayAdapter one_da in all_da)
                    {
                        var x = one_da.ToPathDisplayAdapter();
                        G.Add(new GraphicsAdapter() { Name = one_da.DeviceName });
                    }
                } catch { }


                //DisplayAdapter[] da = ;


                //try
                //{
                //    ManagementClass cs = new ManagementClass("win32_videocontroller");
                //    ManagementObjectCollection moc = cs.GetInstances();
                //    if (moc.Count != 0)
                //    {
                //        foreach (ManagementObject MO in cs.GetInstances())
                //        {
                //            G.Add(new GraphicsAdapter()
                //            {
                //                Name = MO.Properties["Name"].Value.ToString(),
                //                DriverVersion = MO.Properties["DriverVersion"].Value.ToString()
                //            });
                //        }
                //    }
                //}
                //catch { }

                return new List<SupportInfoElement> {
                    new SupportInfoElement() {
                        Name = "Grafikkarte",
                        Value = string.Join("\n", G.Select(x => x.ToString())),
                        Number = number,
                        Column = col
                    }
                };
            }

            if (sit == SupportInfotype.Display)
            {
                //foreach(System.Windows.Forms.Screen s in System.Windows.Forms.Screen.AllScreens)
                //{
                //    Debug.WriteLine(s.ToString());
                //}



                List<DisplayInfo> D = new List<DisplayInfo>();

                List<DisplayDevice> DisplayDevices = new List<DisplayDevice>();

                // new
                Display[] all_displays = Display.GetDisplays().ToArray();

                try
                {
                    foreach( Display d in all_displays)
                    {
                        var d1 = d.ToPathDisplaySource();
                        var d2 = d.ToPathDisplayTarget();

                        D.Add(new DisplayInfo()
                        {
                            Name = d2.FriendlyName,
                            ResX = d.CurrentSetting.Resolution.Width,
                            ResY = d.CurrentSetting.Resolution.Height,
                            Frequency = d.CurrentSetting.Frequency,
                            Dpi = d1.CurrentDPIScale.ToString() 
                        });

                    }
                }
                catch { }







                //// display devices structure
                //DISPLAY_DEVICE dd = new DISPLAY_DEVICE();
                //dd.cb = (uint)Marshal.SizeOf(dd);

                //for(uint id=0; Vanara.PInvoke.User32.EnumDisplayDevices(null, id, ref dd, 0 ); id++)
                //{
                //    if (dd.StateFlags.HasFlag(DISPLAY_DEVICE_FLAGS.DISPLAY_DEVICE_ACTIVE))
                //    {
                //        string did = dd.DeviceID;
                //        string name = dd.DeviceName;
                //        string displaystring = dd.DeviceString;

                //        Vanara.PInvoke.User32.EnumDisplayDevices(dd.DeviceName, 0, ref dd, 0);

                //        string monitor = dd.DeviceString;

                //        DisplayDevices.Add(new DisplayDevice()
                //        {
                //            ID = did,
                //            Name = name,
                //            DisplayString = displaystring,
                //            Monitor = monitor
                //        });
                //    }
                //}

                // device mode structure
                //DEVMODE dm = new DEVMODE();

                //foreach (DisplayDevice DispDev in DisplayDevices)
                //{

                //    bool ret = Vanara.PInvoke.User32.EnumDisplaySettings(DispDev.Name, Vanara.PInvoke.User32.ENUM_CURRENT_SETTINGS, ref dm);

                //    if (ret)
                //    {
                //        DisplayDevices.First(x => x.ID.Equals(DispDev.ID)).Info.BitsPerPel = (int)dm.dmBitsPerPel;
                //        DisplayDevices.First(x => x.ID.Equals(DispDev.ID)).Info.DisplayFrequency = (int)dm.dmDisplayFrequency;
                //        DisplayDevices.First(x => x.ID.Equals(DispDev.ID)).Info.PelsWidth = (int)dm.dmPelsWidth;
                //        DisplayDevices.First(x => x.ID.Equals(DispDev.ID)).Info.PelsHeight = (int)dm.dmPelsHeight;
                //    }
                //}

                //string display_info = string.Join("\n", DisplayDevices.Select(x => x.ToString()));

                //try
                //{
                //    ManagementClass cs = new ManagementClass("win32_desktopmonitor");
                //    ManagementObjectCollection moc = cs.GetInstances();
                //    if (moc.Count != 0)
                //    {
                //        foreach (ManagementObject MO in cs.GetInstances())
                //        {
                //            D.Add(new DisplayInfo()
                //            {
                //                Manufacturer = MO.Properties["MonitorManufacturer"].Value != null ? MO.Properties["MonitorManufacturer"].Value.ToString() : string.Empty,
                //                Name = MO.Properties["MonitorType"].Value != null ? MO.Properties["MonitorType"].Value.ToString() : string.Empty
                //            });
                //        }
                //    }
                //}
                //catch { }

                return new List<SupportInfoElement> {
                    new SupportInfoElement() {
                        Name = "Monitor",
                        Value = string.Join("\n", D.Select(x => x.ToString())),
                        Number = number,
                        Column = col
                    }
                };
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

                return new List<SupportInfoElement> {
                    new SupportInfoElement() {
                        Name = "Bitlocker (C:)",
                        Value = bitLocker,
                        Number = number,
                        Column = col
                    }
                };
            }

            return null;
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cts.Cancel();
            }
            catch { }
            
            this.Close();
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

                Speedtest SpeedtestWindow = new Speedtest();
                SpeedtestWindow.TextOut.Text = output;
                SpeedtestWindow.ShowDialog();
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                cts.Cancel();
            }
            catch { }
        }
    }
}
