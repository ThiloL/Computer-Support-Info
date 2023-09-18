using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Computer_Support_Info
{
    /// <summary>
    /// Interaktionslogik für Speedtest.xaml
    /// </summary>
    public partial class Speedtest : Window
    {
        public Speedtest()
        {
            InitializeComponent();

            this.Title += " (" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ") - Speedtest";

            Rect workArea = SystemParameters.WorkArea;

            this.Height = workArea.Height / 2;
            this.Width = workArea.Width / 2;

            this.Left = (workArea.Width - this.Width) / 2 + workArea.Left;
            this.Top = (workArea.Height - this.Height) / 2 + workArea.Top;

            this.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFD8E8E5"));
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
