using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Computer_Support_Info
{
    public class ViewModel : INotifyPropertyChanged
    {
        //public CollectionViewSource ViewSource1 { get; set; }
        //public CollectionViewSource ViewSource2 { get; set; }
        //public CollectionViewSource ViewSource3 { get; set; }

        //public ObservableCollection<SupportInfoElement> SupportInfoData
        //{
        //    get ;
        //    private set ;
        //}

        private string _ComputerName;
        public string ComputerName
        {
            get { 
                return _ComputerName;
            }
            set
            {
                _ComputerName = value;
                RaisePropertyChanged();
            }
        }

        private string _InternetConnection;
        public string InternetConnection
        {
            get
            {
                return _InternetConnection;
            }
            set
            {
                _InternetConnection = value;
                RaisePropertyChanged();
            }
        }



        public ViewModel()
        {
            //this.SupportInfoData = new ObservableCollection<SupportInfoElement>();
            
            //this.ViewSource1 = new CollectionViewSource();
            
            //ViewSource1.Source = this.SupportInfoData;
            
            //ViewSource1.Filter += (sender, e) => {
            //    SupportInfoElement sie = e.Item as SupportInfoElement;
            //    e.Accepted = sie.Column.Equals(1);
            //};

            //ViewSource1.SortDescriptions.Add(new SortDescription("Number", ListSortDirection.Ascending));
            //ViewSource1.SortDescriptions.Add(new SortDescription("SubNumber", ListSortDirection.Ascending));


            //this.ViewSource2 = new CollectionViewSource();

            //ViewSource2.Source = this.SupportInfoData;

            //ViewSource2.Filter += (sender, e) => {
            //    SupportInfoElement sie = e.Item as SupportInfoElement;
            //    e.Accepted = sie.Column.Equals(2);
            //};

            //ViewSource2.SortDescriptions.Add(new SortDescription("Number", ListSortDirection.Ascending));
            //ViewSource2.SortDescriptions.Add(new SortDescription("SubNumber", ListSortDirection.Ascending));

            //this.ViewSource3 = new CollectionViewSource();

            //ViewSource3.Source = this.SupportInfoData;

            //ViewSource3.Filter += (sender, e) => {
            //    SupportInfoElement sie = e.Item as SupportInfoElement;
            //    e.Accepted = sie.Column.Equals(3);
            //};

            //ViewSource3.SortDescriptions.Add(new SortDescription("Number", ListSortDirection.Ascending));
            //ViewSource3.SortDescriptions.Add(new SortDescription("SubNumber", ListSortDirection.Ascending));


        }


        public void AddItem(SupportInfoElement Item)
        {
            //SupportInfoData.Add(Item);
            //RaisePropertyChanged("SupportInfoData");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler == null) return;

            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
