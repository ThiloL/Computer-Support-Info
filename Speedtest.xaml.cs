using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
//using System.Windows.Shapes;

namespace Computer_Support_Info
{
    /// <summary>
    /// Interaktionslogik für Speedtest.xaml
    /// </summary>
    public partial class Speedtest : Window, INotifyPropertyChanged
    {
        BackgroundWorker worker = null;
        bool isBusy;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                OnPropertyChanged();
            }
        }


        public Speedtest()
        {
            InitializeComponent();

            worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            this.Title += " (" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ") - Internetgeschwindigkeit";

            Rect workArea = SystemParameters.WorkArea;

            this.Height = workArea.Height / 2;
            this.Width = workArea.Width / 2.5;

            this.Left = (workArea.Width - this.Width) / 2 + workArea.Left;
            this.Top = (workArea.Height - this.Height) / 2 + workArea.Top;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            TextOut.Text = string.Empty;
            TextOut.Text = e.Result.ToString();
            BtnStart.IsEnabled = true;
            BtnExit.IsEnabled = true;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (worker.CancellationPending)
            {
                e.Cancel = true;
                IsBusy = false;
                return;
            }

            IsBusy = true;

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
                    StandardOutputEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage),
                    CreateNoWindow = true
                };

                Process p = new Process() { StartInfo = psi };

                p.Start();
                p.WaitForExit(120000);

                //ProcessStartInfo psi = new ProcessStartInfo()
                //{
                //    FileName = "C:\\Windows\\System32\\ping.exe",
                //    UseShellExecute = false,
                //    LoadUserProfile = false,
                //    Arguments = "-n 4 8.8.8.8",
                //    RedirectStandardOutput = true,
                //    StandardOutputEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage),
                //    CreateNoWindow = true
                //};

                //Process p = new Process() { StartInfo = psi };

                //p.Start();
                //p.WaitForExit(120000);


                e.Result = p.StandardOutput.ReadToEnd();

                //Speedtest SpeedtestWindow = new Speedtest();
                //SpeedtestWindow.TextOut.Text = output;
                //SpeedtestWindow.ShowDialog();
            }
            catch
            {
                MessageBox.Show("Fehler beim Speedtest!", "Speedtest", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            TextOut.Text = string.Empty;
            BtnStart.IsEnabled = false;
            BtnExit.IsEnabled = false;
            IsBusy = true;
            worker.RunWorkerAsync();
            return;
        }
    }
}
