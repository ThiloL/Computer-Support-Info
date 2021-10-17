using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Computer_Support_Info
{
    public class ViewModel : INotifyPropertyChanged
    {
        public CollectionViewSource ViewSource1 { get; set; }
        public CollectionViewSource ViewSource2 { get; set; }

        public ObservableCollection<SupportInfoElement> SupportInfoData
        {
            get ;
            private set ;
        }

        public ViewModel()
        {
            this.SupportInfoData = new ObservableCollection<SupportInfoElement>();
            
            this.ViewSource1 = new CollectionViewSource();
            
            ViewSource1.Source = this.SupportInfoData;
            
            ViewSource1.Filter += (sender, e) => {
                SupportInfoElement sie = e.Item as SupportInfoElement;
                e.Accepted = sie.Column.Equals(1);
            };

            ViewSource1.SortDescriptions.Add(new SortDescription("Number", ListSortDirection.Ascending));


            this.ViewSource2 = new CollectionViewSource();

            ViewSource2.Source = this.SupportInfoData;

            ViewSource2.Filter += (sender, e) => {
                SupportInfoElement sie = e.Item as SupportInfoElement;
                e.Accepted = sie.Column.Equals(2);
            };

            ViewSource2.SortDescriptions.Add(new SortDescription("Number", ListSortDirection.Ascending));


        }


        public void AddItem(SupportInfoElement Item)
        {
            SupportInfoData.Add(Item);
            RaisePropertyChanged("SupportInfoData");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler == null) return;

            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
