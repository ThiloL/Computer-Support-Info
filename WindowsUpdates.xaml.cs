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

using WUApiLib;

namespace Computer_Support_Info
{
    /// <summary>
    /// Interaktionslogik für WindowsUpdates.xaml
    /// </summary>
    public partial class WindowsUpdates : Window
    {
        List<WindowsUpdatesElement> WindowsUpdatesData = new List<WindowsUpdatesElement>();

        public WindowsUpdates()
        {
            InitializeComponent();



            Load();


            this.Title += " (" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ") - Installierte Updates";


            WindowsUpdatesGrid.ItemsSource = WindowsUpdatesData;


            Rect workArea = SystemParameters.WorkArea;

            this.Height = workArea.Height / 1.25;
            this.Width = workArea.Width / 1.25;

            this.Left = (workArea.Width - this.Width) / 2 + workArea.Left;
            this.Top = (workArea.Height - this.Height) / 2 + workArea.Top;

            MainGrid.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFD8E8E5"));
        }

        private void Load()
        {
            List<string> categoriesToExclude = Properties.Settings.Default.update_categories_to_exclude.Cast<string>().ToList().ConvertAll(x => x.ToLower().Trim());

            UpdateSession session = new UpdateSession();
            IUpdateSearcher searcher = session.CreateUpdateSearcher();
            searcher.Online = false;

            //try
            //{
            //    ISearchResult result = searcher.Search("IsInstalled=1");

            //    foreach (IUpdate u in result.Updates)
            //    {
            //        var title = u.Title;
            //        var kb = u.KBArticleIDs.Cast<string>().ToList().Aggregate( (x,y) => $"{x},{y}");
            //        var date = u.LastDeploymentChangeTime;

            //        WindowsUpdatesData.Add(new WindowsUpdatesElement()
            //        {
            //            Date = date.ToString(),
            //            Kb = kb,
            //            Title = title
            //        });
            //    }
            //}
            //catch { }


            var count = searcher.GetTotalHistoryCount();
            if (count == 0) return;

            var history = searcher.QueryHistory(0, count);

            for (int i = 0; i < count; i++)
            {
                IUpdateHistoryEntry2 e = (IUpdateHistoryEntry2)history[i];

                var category = "n/a";

                ICategoryCollection categories = e.Categories;
                foreach(ICategory oc in categories)
                {
                    category = oc.Name;
                    break;
                }

                // check for excluded catgory
                if (categoriesToExclude.Contains(category.ToLower().Trim())) continue;

                var title = history[i].Title;

                IUpdateIdentity ident = history[i].UpdateIdentity;

                var id = ident.UpdateID;

                WindowsUpdatesData.Add(new WindowsUpdatesElement() { 
                    Kb = id, 
                    Title = title, 
                    Date = history[i].Date.ToString(), 
                    Result = history[i].ResultCode.ToString().Replace("orc",string.Empty),
                    Category = category
                });
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
