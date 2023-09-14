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
    public class GridWithHeaderData : INotifyPropertyChanged
    {
        public string Title { get; set; }
        
        public ObservableCollection<NameAndValue> ListOfValues { get; set; }

        public GridWithHeaderData(string Title) { 
        
            this.Title = Title;
            ListOfValues = new ObservableCollection<NameAndValue>();
        }

        public void AddListItem(NameAndValue nav)
        {
            ListOfValues.Add(nav);
            //ListOfValues = (ObservableCollection<NameAndValue>)ListOfValues.OrderBy(x => x.Order);
            OnPropertyChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") { 
        
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
    }

    public class NameAndValue
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public int Order { get; set; }

        public bool MakeBold { get; set; }
    }
}
