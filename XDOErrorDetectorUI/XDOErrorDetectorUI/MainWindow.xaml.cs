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
using System.Windows.Forms;


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
            textBox_host.Text = "localhost";
            textBox_username.Text = "postgres";
            textBox_password.Text = "root";
            textBox_database.Text = "mydata";
            textBox_table.Text = "xdo2";
            textBox_port.Text = "5433";
            folder_path.Text = @"C:\Users\KimDoHoon\Desktop\git\2DTile\XDOErrorDetector\data";
        }

        /* Unused code
        private void button_update_Click(object sender, RoutedEventArgs e)
        {
            sql.update();
            label1.Content = "DB가 업데이트 되었습니다.";
        }
        */

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void button_CreateTable_Click(object sender, RoutedEventArgs e)
        {
            label1.Content = sql.createTable(textBox_table.Text);
        }

        private void button_Connect_Click(object sender, RoutedEventArgs e)
        {
            sql.info = new XDOErrorDetectorUI.DB(textBox_host.Text, textBox_username.Text, textBox_password.Text, textBox_database.Text, textBox_port.Text);
            label1.Content = sql.connect();
            btn_createtable.IsEnabled = true;
            btn_deletetable.IsEnabled = true;
            btn_cleartable.IsEnabled = true;
            btn_searchtable.IsEnabled = true;
            textBox_table.IsEnabled = true;
            btn_load.IsEnabled = true;

            btn_connection.IsEnabled = false;
            textBox_host.IsEnabled = false;
            textBox_username.IsEnabled = false;
            textBox_password.IsEnabled = false;
            textBox_database.IsEnabled = false;
            textBox_port.IsEnabled = false;
        }

        private void button_ClearTable_Click(object sender, RoutedEventArgs e)
        {
            label1.Content = sql.clearTable(textBox_table.Text);
        }

        private void button_DeleteTable_Click(object sender, RoutedEventArgs e)
        {
            label1.Content = sql.deleteTable(textBox_table.Text);
        }

        private void button_view_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void button_search_Click(object sender, RoutedEventArgs e)
        {
            label1.Content = sql.search(textBox_table.Text, folder_path.Text);
        }

        private void button_changefolder(object sender, RoutedEventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            folderDialog.ShowDialog();
            folder_path.Text = folderDialog.SelectedPath;
        }

        private void button_load_Click(object sender, RoutedEventArgs e)
        {
            listView1.Items.Clear();
            listView_Log.Items.Clear();
            var list = sql.loadTable(textBox_table.Text);
            
            foreach (var key in list)
            {
                listView1.Items.Add(new myItem {
                    FileName = key.minifiedName,
                    Level = key.level,
                    X = key.X,
                    Y = key.Y,
                    ObjectID = key.ObjectID,
                    Key = key.Key,
                    minX = key.ObjBox[0],
                    minY = key.ObjBox[1],
                    minZ = key.ObjBox[2],
                    maxX = key.ObjBox[3],
                    maxY = key.ObjBox[4],
                    maxZ = key.ObjBox[5],
                    Altitude = key.Altitude,
                    faceNum = key.FaceNum,
                    XDOVersion = key.XDOVersion,
                    VertexCount = String.Join(", ", key.VertexCount.ToArray()),
                    IndexedCount = String.Join(", ", key.IndexedCount.ToArray()),
                    ImageLevel = String.Join(", ", key.ImageLevel.ToArray()),
                    ImageName = String.Join(", ", key.ImageName.ToArray())
                });
            }

            var logList = sql.loadLogTable(textBox_table.Text);
            foreach(var item in logList)
            {
                listView_Log.Items.Add(item);
            }

            label1.Content = "데이터 개수 " + list.Count + "/" + logList.Count + "개를 불러왔습니다.";
        }
        public class myItem
        {
            public string FileName { get; set; }
            public int Level { get; set; }
            public string X { get; set; }
            public string Y { get; set; }
            public int ObjectID { get; set; }
            public string Key { get; set; }
            public double minX { get; set; }
            public double minY { get; set; }
            public double minZ { get; set; }
            public double maxX { get; set; }
            public double maxY { get; set; }
            public double maxZ { get; set; }
            public float Altitude { get; set; }
            public int faceNum { get; set; }
            public int XDOVersion { get; set; }
            public string VertexCount { get; set; }
            public string IndexedCount { get; set; }
            public string ImageLevel { get; set; }
            public string ImageName { get; set; }
        }
    }
}
