using System;
using System.Collections.Generic;
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

namespace XDOErrorDetectorUI
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        postgreSQL sql;
        public MainWindow()
        {
            InitializeComponent();
            sql = new postgreSQL();
            label.Content = sql.baseURL;
            
        }

        private void button_update_Click(object sender, RoutedEventArgs e)
        {
            sql.update();
        }

        private void button_check_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<String, DBItem> dic = sql.check();

            foreach (KeyValuePair<String, DBItem> key in dic)
            {
                listView1.Items.Add(new myItem { File = key.Key, Success = key.Value.status_correct, Warning = key.Value.status_warning, Error = key.Value.status_error });
            }
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public class myItem
        {
            public string File { get; set; }
            public int Success { get; set; }
            public int Warning { get; set; }
            public int Error { get; set; }
        }
    }
}
