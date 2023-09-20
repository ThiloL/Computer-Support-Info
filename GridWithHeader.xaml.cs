using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Computer_Support_Info
{
    /// <summary>
    /// Interaktionslogik für GridWithHeader.xaml
    /// </summary>
    public partial class GridWithHeader : UserControl, INotifyPropertyChanged
    {
        public GridWithHeader()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        private void UserGrid_Loaded(object sender, RoutedEventArgs e)
        {
            SetColumnWidths();
        }

        private void UserGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetColumnWidths();
        }

        private void SetColumnWidths()
        {
            //this.UserGrid.Columns[0].Width = new DataGridLength(20, DataGridLengthUnitType.Star);
            //this.UserGrid.Columns[1].Width = new DataGridLength(80, DataGridLengthUnitType.Star);
            //this.UserGrid.Columns[2].Width = new DataGridLength(0, DataGridLengthUnitType.Star);
        }


    }
}
