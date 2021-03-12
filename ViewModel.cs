using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Support_Info
{
    public class ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<SupportInfoElement> SupportInfoData
        {
            get ;
            private set ;
        }

        public ViewModel()
        {
            SupportInfoData = new ObservableCollection<SupportInfoElement>();
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
