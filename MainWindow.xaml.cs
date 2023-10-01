using AForge.Video.DirectShow;
using Computer_Support_Info.Classes;
using Humanizer;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Vanara.PInvoke;
using static Vanara.PInvoke.NetApi32;
using WindowsDisplayAPI;
using Path = System.IO.Path;
using Vanara.Extensions;
using System.Runtime.InteropServices;

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

        GridWithHeaderData Session = new GridWithHeaderData("Sitzung");
        GridWithHeaderData User = new GridWithHeaderData("Benutzer/Entra ID");
        GridWithHeaderData Os = new GridWithHeaderData("Betriebssystem");
        GridWithHeaderData Computer = new GridWithHeaderData("Computer");
        GridWithHeaderData Network = new GridWithHeaderData("Netzwerk");
        GridWithHeaderData DiskP = new GridWithHeaderData("Laufwerke (physisch)");
        GridWithHeaderData DiskL = new GridWithHeaderData("Laufwerke (logisch)");
        GridWithHeaderData AV = new GridWithHeaderData("Geräte");
        GridWithHeaderData Webcam = new GridWithHeaderData("Webcam");

        public MainWindow()
        {


            InitializeComponent();

            SESSION.DataContext = Session;
            USER.DataContext = User;
            OS.DataContext = Os;
            COMPUTER.DataContext = Computer;
            DISK_PHYSICAL.DataContext = DiskP;
            DISK_LOGICAL.DataContext = DiskL;
            NETWORK.DataContext = Network;
            AV_DEVICES.DataContext = AV;
            WEBCAM.DataContext = Webcam;

            cts = new CancellationTokenSource();
            po = new ParallelOptions() { CancellationToken = cts.Token, MaxDegreeOfParallelism = System.Environment.ProcessorCount };

            background_worker = new BackgroundWorker() { WorkerSupportsCancellation = true };

            this.Title += " (" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ")";

            vm = new ViewModel();
            DataContext = vm;

            // check internet connection
            WinINet.INTERNET_CONNECTION ic;
            IsConnectedToInternet = WinINet.InternetGetConnectedState(out ic, 0);

            vm.InternetConnection = (IsConnectedToInternet == true) ? "\u2705" : "\u274E";

            background_worker.WorkerReportsProgress = false;
            background_worker.DoWork += Background_worker_DoWork;
            background_worker.RunWorkerCompleted += Background_worker_RunWorkerCompleted;

            Mouse.OverrideCursor = Cursors.Wait;
            background_worker.RunWorkerAsync();
        }

        private void Background_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //SupportInfosGrid1.Items.Refresh();
            //SupportInfosGrid2.Items.Refresh();
            //SupportInfosGrid3.Items.Refresh();

            this.MainMenu.IsEnabled = true;

            this.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFD8E8E5"));
            Mouse.OverrideCursor = null;

            RepositionWindows();

        }

        private void RepositionWindows()
        {
            Rect workArea = SystemParameters.WorkArea;
            
            this.Height = workArea.Height - (workArea.Height * 0.1);
            this.Width = workArea.Width - (workArea.Width * 0.1);

            this.Left = (workArea.Width - this.Width) / 2 + workArea.Left;
            this.Top = (workArea.Height - this.Height) / 2 + workArea.Top;
            
        }

        private void Load2()
        {

        }

        public class Taskhelper
        {
            public bool IsPlainText{ get; set;} 

            public string Plaintext { get; set;}

            public SupportInfotype support_info_type { get; set; }
            public int number { get; set; }

            public string number_prefix { get; set; }
            public int col { get; set; }

            public GridWithHeaderData gwhd { get; set; }
        }

        private int Add10(ref int number)
        {
            number += 10;
            return number;
        }

        private void Background_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int no = 1;

            List<Taskhelper> th = new List<Taskhelper>();

            th.Add(new Taskhelper() { support_info_type = SupportInfotype.BootAndSessionInfo, number = Add10(ref no), col = 1, gwhd = Session });

            th.Add(new Taskhelper() { support_info_type = SupportInfotype.UserName, number = Add10(ref no), col = 1, gwhd = User, IsPlainText = true });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.IsAdmin, number = Add10(ref no), col = 1, gwhd = User });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.AadInfo, number = Add10(ref no), col = 1, gwhd = User });

            th.Add(new Taskhelper() { support_info_type = SupportInfotype.ComputerName, number = no++, col = 1, IsPlainText = true });
            
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.OperatingSystem, number = Add10(ref no), col = 1, gwhd = Os });
            
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.ComputerManufacturer, number = Add10(ref no), col = 1, gwhd = Computer });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.ComputerModel, number = Add10(ref no), col = 1, gwhd = Computer });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.Firmware, number = Add10(ref no), col = 1, gwhd = Computer });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.SerialBaseboard, number = Add10(ref no), col = 1, gwhd = Computer });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.SerialBios, number = Add10(ref no), col = 1, gwhd = Computer });

            th.Add(new Taskhelper() { support_info_type = SupportInfotype.CPU, number = Add10(ref no), col = 1, gwhd = Computer });
            
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.Memory, number = Add10(ref no), col = 1, gwhd = Computer });
            
            
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.Network, number = Add10(ref no), col = 1, gwhd = Network });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.Ping, number = Add10(ref no), col = 1, gwhd = Network });

            //th.Add(new Taskhelper() { support_info_type = SupportInfotype.Bitlocker, number = no++, col = 1 });

            th.Add(new Taskhelper() { support_info_type = SupportInfotype.GraphicsCard, number = Add10(ref no), col = 2, gwhd = AV });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.Display, number = Add10(ref no), col = 2, gwhd = AV });

            th.Add(new Taskhelper() { support_info_type = SupportInfotype.PhysicalDrives, number = Add10(ref no), col = 2 , gwhd = DiskP });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.LogicalDrives, number = Add10(ref no), col = 2, gwhd = DiskL });

            th.Add(new Taskhelper() { support_info_type = SupportInfotype.Webcam, number =Add10(ref no), col = 2, gwhd = Webcam });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.AudioOutDevices, number = Add10(ref no), col = 2, gwhd = AV });
            th.Add(new Taskhelper() { support_info_type = SupportInfotype.AudioInDevices, number = Add10(ref no), col = 2, gwhd = AV });

            th.Add(new Taskhelper() { support_info_type = SupportInfotype.WebcamDetails, number_prefix = "WC", number = Add10(ref no), col = 3, gwhd = Webcam });

            try
            {

                Parallel.ForEach(th, po, one =>
                {
                    // List<SupportInfoElement> Results = LoadData(one.support_info_type, one.number, one.col);
                    List<NameAndValue> Results = LoadData(one.support_info_type, one.number, one.col);

                    if (Results == null) return;

                    if (one.IsPlainText)
                    {
                        if (one.support_info_type == SupportInfotype.ComputerName) vm.ComputerName = Results[0].Value;
                        if (one.support_info_type == SupportInfotype.UserName) vm.UserName = Results[0].Value;
                    }
                    else
                        // foreach (SupportInfoElement sie in Results) AddGridItem(sie);
                        foreach (NameAndValue nal in Results)
                            AddData(nal, one.gwhd);
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

        private void AddData(NameAndValue nav, GridWithHeaderData gwhd)
        {
            if (System.Windows.Application.Current == null) return;
            System.Windows.Application.Current.Dispatcher.Invoke
            (
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() =>
                {
                    gwhd.AddListItem(nav);
                })
            );
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
                    //vm.AddItem(Item);
                    //vm.ViewSource1.View.Refresh();
                    //vm.ViewSource2.View.Refresh();
                    //vm.ViewSource3.View.Refresh();
                    RepositionWindows();
                })
            );
        }


        private List<NameAndValue> LoadData(SupportInfotype sit, int number, int col, string number_prefix = "")
        {
            if (sit == SupportInfotype.BootAndSessionInfo)
            {
                List<NameAndValue> L = new List<NameAndValue>();

                var bootTime = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem").Get()
                    .OfType<ManagementObject>()
                    .First()
                    .Properties["LastBootUpTime"].Value;

                DateTime LastBootTime = ManagementDateTimeConverter.ToDateTime(bootTime.ToString());

                string strLastBootTime = LastBootTime.ToString("dd.MM.yyyy HH:mm");

                TimeSpan DurationSinceLastBoot = DateTime.Now - LastBootTime;

                L.Add(new NameAndValue()
                {
                    Name = "Computerstart vor",
                    Value = $"{DurationSinceLastBoot.Humanize()} [{strLastBootTime}]",
                    Order = number++
                });

                var x = new ManagementObjectSearcher("SELECT * FROM Win32_LogonSession WHERE LogonType = 2").Get()
                    .OfType<ManagementObject>();


                var id = System.Diagnostics.Process.GetCurrentProcess().SessionId;



                return L;
            }


            if (sit == SupportInfotype.AadInfo)
            {
                List<NameAndValue> L = new List<NameAndValue>();

                string workgroup_name = string.Empty;
                NETSETUP_JOIN_STATUS nsjs = new NETSETUP_JOIN_STATUS();

                Win32Error x = NetGetJoinInformation(null, out workgroup_name, out nsjs);
                if (x == Win32Error.NERR_Success)
                {
                    //
                    if (nsjs == NETSETUP_JOIN_STATUS.NetSetupWorkgroupName)
                    {
                        L.Add(new NameAndValue()
                        {
                            Name = "Workgroup",
                            Value = workgroup_name,
                            Order = number++
                        });
                    }
                }



                DSREG_JOIN_INFO join_info = new DSREG_JOIN_INFO();

                try
                {
                    var r = NetGetAadJoinInformation(null, out join_info);
                    if (r == HRESULT.S_OK)
                    {
                        

                        L.Add(new NameAndValue()
                        {
                            Name = "Tenant | -ID",
                            Value = $"{join_info.pszTenantDisplayName} | {join_info.pszTenantId}",
                            Order = number++
                        });

                        L.Add(new NameAndValue()
                        {
                            Name = "User E-Mail",
                            Value = $"{join_info.pszJoinUserEmail}",
                            Order = number++
                        });

                        return L;
                    }
                } 
                catch
                {
                }

                

                return null;

                

                


            }



            if (sit == SupportInfotype.UserName)
            {
                var user = WindowsIdentity.GetCurrent().Name;

                StringBuilder sbu = new StringBuilder(1024);
                uint sbu_size = (uint)sbu.Capacity;

                var fn = string.Empty;

                //fn = System.DirectoryServices.AccountManagement.UserPrincipal.Current.GivenName;

                return new List<NameAndValue>
                {
                    new NameAndValue()
                    {
                        Name = "Benutzername",
                        Value = $"{user} {fn}",
                        Order = number
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
                {
                    return new List<NameAndValue> {
                        new NameAndValue()
                        {
                            Name = "Administrative Rechte",
                            Value = "JA",
                            Order = number
                        }
                    };

                }
                else

                    return new List<NameAndValue> {
                        new NameAndValue()
                        {
                            Name = "Administrative Rechte",
                            Value = "NEIN"
                        }
                    };

            }

            if (sit == SupportInfotype.ComputerName)
            {
                return new List<NameAndValue> { new NameAndValue() { Value = Environment.MachineName } };
            }


            // === OS ===

            if (sit == SupportInfotype.OperatingSystem)
            {
                var versionString = string.Empty;

                ManagementClass cs = new ManagementClass("Win32_OperatingSystem");
                ManagementObjectCollection moc = cs.GetInstances();
                if (moc.Count != 0)
                {
                    foreach (ManagementObject MO in cs.GetInstances())
                    {
                        versionString = MO.Properties["Caption"].Value.ToString();
                    }
                }

                var releaseID = (string)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("DisplayVersion");
                var x64 = Environment.Is64BitOperatingSystem ? "x64" : "x86";

                List<NameAndValue> O = new List<NameAndValue>();

                int N = 1;

                O.Add(new NameAndValue()
                {
                    Name = "Betriebssystem",
                    Value = versionString,
                    Order = N++
                });

                O.Add(new NameAndValue()
                {
                    Name = "-Release",
                    Value = releaseID,
                    Order = N++
                });

                O.Add(new NameAndValue()
                {
                    Name = "-Architektur",
                    Value = x64,
                    Order = N++
                });

                var os_major = (int)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("CurrentMajorVersionNumber");
                var os_minor = (int)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("CurrentMinorVersionNumber");
                var currentBuildNumber = (string)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("currentBuildNumber");
                var ubr = (int)Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion")?.GetValue("UBR");

                O.Add(new NameAndValue()
                {
                    Name = "-Version",
                    Value = $"{os_major}.{os_minor}.{currentBuildNumber}.{ubr}",
                    Order = N++
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

                return new List<NameAndValue> {
                        new NameAndValue()
                        {
                            Name = "Hersteller",
                            Value = manufacturer,
                            Order = number
                        }
                    };


            }

            if (sit == SupportInfotype.ComputerModel)
            {
                string model = string.Empty;
                string model2 = string.Empty;

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

                    ManagementClass cs2 = new ManagementClass("win32_computersystem");
                    ManagementObjectCollection moc2 = cs.GetInstances();
                    if (moc2.Count != 0)
                    {
                        foreach (ManagementObject MO in cs2.GetInstances())
                        {
                            model2 = MO.Properties["Model"].Value.ToString();
                        }
                    }

                }
                catch { }

                return new List<NameAndValue> {
                        new NameAndValue()
                        {
                            Name = "Modell",
                            Value = string.Format("{0} | {1}", model, model2),
                            Order = number
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

                return new List<NameAndValue> {
                        new NameAndValue()
                        {
                            Name = "Serien-Nummer (Baseboard)",
                            Value = serial,
                            Order = number
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

                return new List<NameAndValue> {
                        new NameAndValue()
                        {
                            Name = "Serien-Nummer (BIOS)",
                            Value = serial,
                            Order = number
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

                return new List<NameAndValue> {
                        new NameAndValue()
                        {
                            Name = "CPU",
                            Value = cpu,
                            Order = number
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

                List<NameAndValue> F = new List<NameAndValue>();

                F.Add(new NameAndValue()
                        {
                            Name = "Firmware-Hersteller",
                            Value = bios_manufacturer,
                            Order = number++
                        });

                F.Add(new NameAndValue()
                {
                    Name = "-Version",
                    Value = bios_version,
                    Order = number++
                });

                F.Add(new NameAndValue()
                {
                    Name = "-Datum",
                    Value = bios_datetime.ToString("dd.MM.yyyy"),
                    Order = number++
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

                return new List<NameAndValue> {
                        new NameAndValue()
                        {
                            Name = "RAM",
                            Value = ram,
                            Order = number
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

                    //ManagementScope scope = new ManagementScope("\\\\.\\ROOT\\Microsoft\\Windows\\Storage");
                    //ObjectQuery query = new ObjectQuery("SELECT * FROM MSFT_Disk");

                    //ManagementObjectSearcher searcher =
                    //        new ManagementObjectSearcher(scope, query);

                    //ManagementObjectCollection queryCollection = searcher.Get();


                    ManagementClass cs = new ManagementClass("win32_diskdrive");
                    ManagementObjectCollection moc = cs.GetInstances();
                    if (moc.Count != 0)
                    {
                        foreach (ManagementObject MO in cs.GetInstances())
                        {
                            var type = MO.Properties["MediaType"]?.Value?.ToString();

                            if (type == null) continue;
                            
                            if (!type.Equals("fixed hard disk media", StringComparison.InvariantCultureIgnoreCase)
                                &&
                                !type.Equals("external hard disk media", StringComparison.InvariantCultureIgnoreCase)
                                ) continue;

                            DiskDrives.Add(new DiskDrive()
                            {
                                Caption = MO.Properties["Caption"].Value.ToString(),
                                Size = Convert.ToInt64(MO.Properties["Size"].Value),
                                SerialNumber = MO.Properties["SerialNumber"].Value.ToString(),
                                Firmware = MO.Properties["FirmwareRevision"].Value.ToString(),
                                Index = Convert.ToInt32(MO.Properties["Index"].Value)
                            });
                        }

                        // sortieren
                        DiskDrives = DiskDrives.OrderBy(x => x.Index).ToList();
                    }
                }
                catch { }

                List<NameAndValue> L = new List<NameAndValue>();

                DiskDrives.ForEach(x => L.Add(new NameAndValue()
                {
                    Name = x.Index.ToString(),
                    Value = x.ToString(),
                    Order = number++
                }));

                return L;

                //return new List<SupportInfoElement> {
                //    new SupportInfoElement() {
                //        Name = "Laufwerk (phys.)",
                //        Value = string.Join("\n", DiskDrives.Select(x => x.ToString())),
                //        Number = number,
                //        Column = col
                //    }
                //};
            }

            if (sit == SupportInfotype.LogicalDrives)
            {
                // logical drives

                List<LogicalVolume> L = new List<LogicalVolume>();

                List<NameAndValue> nv = new List<NameAndValue>();

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

                L.ForEach(x => nv.Add(new NameAndValue()
                {
                    Name = x.Name,
                    Value = x.ToString(),
                    Order = number++
                }));

                return nv;
            }

            if (sit == SupportInfotype.Network)
            {
                // netzwerk info

                int N = 1;

                List<NameAndValue> C = new List<NameAndValue>();

                List<NetworkInfo> NetworkAdapter = new List<NetworkInfo>();

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

                C.Add(new NameAndValue()
                {
                    Name = "Adapter",
                    Value = string.Join("\n", NetworkAdapter.Select(x => x.ToString())),
                    Order = number++
                });

                if (!string.IsNullOrEmpty(ip))
                    C.Add(new NameAndValue()
                    {
                        Name = "IP-Adresse (privat)",
                        Value = ip,
                        Order = number++
                    });

                IPAddress publicIp = null;

                try
                {
                    publicIp = Ipify.Ipify.GetPublicIPAddress();
                }
                catch { }

                if (publicIp != null)
                {
                    C.Add(new NameAndValue()
                    {
                        Name = "IP-Adresse  (öffentlich)",
                        Value = publicIp.ToString(),
                        Order = number++
                    });

                    try
                    {

                        HttpClient client = new HttpClient();
                        HttpResponseMessage response = client.GetAsync($"https://api.ipgeolocation.io/ipgeo?apiKey=0b3a13f962a5480c88a81a14bf1cb9eb&ip={publicIp.ToString()}&lang=de").Result;
                        response.EnsureSuccessStatusCode();
                        string responseBody = response.Content.ReadAsStringAsync().Result;

                        IpLocation ip_location = JsonSerializer.Deserialize<IpLocation>(responseBody);

                        if (ip_location != null)
                        {
                            C.Add(new NameAndValue()
                            {
                                Name = "Standort/Provider",
                                Value = ip_location.ToString(),
                                Order = number++
                            });
                        }

                    }
                    catch { }


                }

                return C;


            }

            if (sit == SupportInfotype.Ping)
            {
                // Ping

                if (!IsConnectedToInternet)
                {
                    MenuInternetSpeed.IsEnabled = false;

                    return new List<NameAndValue> {
                        new NameAndValue() {
                            Name = "Ping (8.8.8.8)",
                            Value = "Keine Internetverbindung",
                            Order = number
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

                return new List<NameAndValue> {
                    new NameAndValue() {
                        Name = "Ping (8.8.8.8)",
                        Value = ping_info,
                        Order = number
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

                return new List<NameAndValue> {
                    new NameAndValue() {
                        Name = "Webcam",
                        Value = string.Join("\n", W.Select(x => x.ToString())),
                        Order = number
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
                                R.Add(new ResolutionAndFramerate()
                                {
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

                List<NameAndValue> L = new List<NameAndValue>();

                foreach (WebcamInfo wci in W)
                {
                    L.Add(new NameAndValue()
                    {
                        Name = wci.Name,
                        Value = wci.ResultionText,
                        MakeBold = false,
                        Order = number++
                    });
                }

                return L;
            }

            if (sit == SupportInfotype.AudioOutDevices)
            {
                //var x2 = Vanara.PInvoke.WinMm.waveOutGetNumDevs();

                //WinMm.WAVEINCAPS caps = new WinMm.WAVEINCAPS();

                //var dev = Vanara.PInvoke.WinMm.waveInGetDevCaps(0, out caps, (uint)Marshal.SizeOf(typeof(WinMm.WAVEINCAPS)));


                MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                



                // Audio (out)

                List<AudioDevice> audio_out = new List<AudioDevice>();

                var out_dev = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

                try
                {
                    foreach(MMDevice d in out_dev)
                    {



                        audio_out.Add(new AudioDevice()
                        {
                            Name  = d.FriendlyName,
                            VolumeLevel = Convert.ToInt32(d.AudioEndpointVolume.MasterVolumeLevelScalar * 100)
                        });
                    }

                    //for (int i = -1; i < WaveOut.DeviceCount; i++)
                    //{
                    //    var c = WaveOut.GetCapabilities(i);
                    //    audio_out.Add(c.ProductName);
                    //}
                }
                catch { }

                if (audio_out.Count > 0)
                    return new List<NameAndValue> {
                        new NameAndValue() {
                            Name = "Audio (Out)",
                            Value = string.Join("\n", audio_out.Select(x => x.ToString())),
                            Order = number
                        }
                    };
            }

            if (sit == SupportInfotype.AudioInDevices)
            {

                MMDeviceEnumerator enumerator = new MMDeviceEnumerator();

                // Audio (in)

                List<AudioDevice> audio_in = new List<AudioDevice>();

                var out_dev = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);


                try
                {
                    foreach (MMDevice d in out_dev)
                    {
                        audio_in.Add(new AudioDevice()
                        {
                            Name = d.FriendlyName,
                            VolumeLevel = Convert.ToInt32(d.AudioEndpointVolume.MasterVolumeLevelScalar * 100)
                        });
                    }

                    //for (int i = -1; i < WaveIn.DeviceCount; i++)
                    //{
                    //    var c = WaveIn.GetCapabilities(i);
                    //    audio_in.Add(c.ProductName);
                    //}

                }
                catch { }

                if (audio_in.Count > 0)
                    return new List<NameAndValue> {
                    new NameAndValue() {
                        Name = "Audio (In)",
                        Value = string.Join("\n", audio_in.Select(x => x.ToString())),
                        Order = number
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
                }
                catch { }


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

                return new List<NameAndValue> {
                    new NameAndValue() {
                        Name = "Grafikkarte",
                        Value = string.Join("\n", G.Select(x => x.ToString())),
                        Order = number
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
                    foreach (Display d in all_displays)
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







                // display devices structure
               // DISPLAY_DEVICE dd = new DISPLAY_DEVICE();
               // dd.cb = (uint)Marshal.SizeOf(dd);

               // for (uint id = 0; Vanara.PInvoke.User32.EnumDisplayDevices(null, id, ref dd, 0); id++)
               // {
               //     if (dd.StateFlags.HasFlag(DISPLAY_DEVICE_FLAGS.DISPLAY_DEVICE_ACTIVE))
               //     {
               //         string did = dd.DeviceID;
               //         string name = dd.DeviceName;
               //         string displaystring = dd.DeviceString;

               //         Vanara.PInvoke.User32.EnumDisplayDevices(dd.DeviceName, 0, ref dd, 0);

               //         string monitor = dd.DeviceString;

               //         DisplayDevices.Add(new DisplayDevice()
               //         {
               //             ID = did,
               //             Name = name,
               //             DisplayString = displaystring,
               //             Monitor = monitor
               //         });
               //     }
               // }

               // device mode structure
               //DEVMODE dm = new DEVMODE();

               // foreach (DisplayDevice DispDev in DisplayDevices)
               // {

               //     bool ret = Vanara.PInvoke.User32.EnumDisplaySettings(DispDev.Name, Vanara.PInvoke.User32.ENUM_CURRENT_SETTINGS, ref dm);

               //     if (ret)
               //     {
               //         DisplayDevices.First(x => x.ID.Equals(DispDev.ID)).Info.BitsPerPel = (int)dm.dmBitsPerPel;
               //         DisplayDevices.First(x => x.ID.Equals(DispDev.ID)).Info.DisplayFrequency = (int)dm.dmDisplayFrequency;
               //         DisplayDevices.First(x => x.ID.Equals(DispDev.ID)).Info.PelsWidth = (int)dm.dmPelsWidth;
               //         DisplayDevices.First(x => x.ID.Equals(DispDev.ID)).Info.PelsHeight = (int)dm.dmPelsHeight;
               //     }
               // }

               // string display_info = string.Join("\n", DisplayDevices.Select(x => x.ToString()));

               // try
               // {
               //     ManagementClass cs = new ManagementClass("win32_desktopmonitor");
               //     ManagementObjectCollection moc = cs.GetInstances();
               //     if (moc.Count != 0)
               //     {
               //         foreach (ManagementObject MO in cs.GetInstances())
               //         {
               //             D.Add(new DisplayInfo()
               //             {
               //                 Manufacturer = MO.Properties["MonitorManufacturer"].Value != null ? MO.Properties["MonitorManufacturer"].Value.ToString() : string.Empty,
               //                 Name = MO.Properties["MonitorType"].Value != null ? MO.Properties["MonitorType"].Value.ToString() : string.Empty
               //             });
               //         }
               //     }
               // }
               // catch { }

                return new List<NameAndValue> {
                    new NameAndValue() {
                        Name = "Monitor",
                        Value = string.Join("\n", D.Select(x => x.ToString())),
                        Order = number
                    }
                };
            }

            //if (sit == SupportInfotype.Bitlocker)
            //{
            //    // Bitlocker

            //    var bitLocker = string.Empty;

            //    IShellProperty prop = ShellObject.FromParsingName("C:").Properties.GetProperty("System.Volume.BitLockerProtection");
            //    int? bitLockerProtectionStatus = (prop as ShellProperty<int?>).Value;

            //    if (bitLockerProtectionStatus.HasValue && (bitLockerProtectionStatus == 1 || bitLockerProtectionStatus == 3 || bitLockerProtectionStatus == 5))
            //        bitLocker = "Ein";
            //    else
            //        bitLocker = "Aus";

            //    return new List<SupportInfoElement> {
            //        new SupportInfoElement() {
            //            Name = "Bitlocker (C:)",
            //            Value = bitLocker,
            //            Number = number,
            //            Column = col
            //        }
            //    };
            //}

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
                    StandardOutputEncoding = Encoding.UTF8,
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

        private void MenuInstalledApps(object sender, RoutedEventArgs e)
        {
            InstalledApps InstalledAppsWindow = new InstalledApps();
            InstalledAppsWindow.ShowDialog();
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
