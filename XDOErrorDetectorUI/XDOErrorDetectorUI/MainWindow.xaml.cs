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

            textBox_host.Text = "localhost";
            textBox_username.Text = "postgres";
            textBox_password.Text = "root";
            textBox_database.Text = "mydata";
            textBox_table.Text = "xdo2";
        }

        private void button_update_Click(object sender, RoutedEventArgs e)
        {
            sql.update();
            label1.Content = "DB가 업데이트 되었습니다.";
        }

        private void button_check_Click(object sender, RoutedEventArgs e)
        {
            listView1.Items.Clear();
            Dictionary<String, DBItem> dic = sql.check();
            label1.Content = "데이터 개수: " + dic.Count;
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

        private void button_CreateTable_Click(object sender, RoutedEventArgs e)
        {
            sql.createTable();
            label1.Content = "Table을 생성하였습니다.";
        }

        private void button_Connect_Click(object sender, RoutedEventArgs e)
        {
            sql.info = new XDOErrorDetectorUI.DB(textBox_host.Text, textBox_username.Text, textBox_password.Text, textBox_database.Text);
            label1.Content = sql.connect();
            btn_createtable.IsEnabled = true;
            btn_deletetable.IsEnabled = true;
            btn_cleartable.IsEnabled = true;
            textBox_table.IsEnabled = true;

            btn_connection.IsEnabled = false;
            textBox_host.IsEnabled = false;
            textBox_username.IsEnabled = false;
            textBox_password.IsEnabled = false;
            textBox_database.IsEnabled = false;
        }
    }
}
