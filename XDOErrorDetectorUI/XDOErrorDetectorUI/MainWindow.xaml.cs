﻿using System;
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
using System.IO;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;
namespace XDOErrorDetectorUI
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        postgreSQL sql;
        bool clickSearch = false, clickCheckVersion = false;
        Stopwatch watch = new Stopwatch();
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

            this.progressBarWorker();
        }
        private void button_CreateTable_Click(object sender, RoutedEventArgs e)
        {
            setTableName(sql, textBox_table.Text);

            var worker = new BackgroundWorker();
            worker.DoWork += delegate (object o, DoWorkEventArgs args)
            {
                watch.Restart();
                args.Result = sql.createTable();
            };

            worker.RunWorkerCompleted += delegate (object o, RunWorkerCompletedEventArgs args)
            {
                watch.Stop();
                label1.Content = args.Result;
                label1.Content += ms(watch);
            };
            worker.RunWorkerAsync();
        }

        private void button_Connect_Click(object sender, RoutedEventArgs e)
        {
            sql.info = new XDOErrorDetectorUI.DB(textBox_host.Text, textBox_username.Text, textBox_password.Text, textBox_database.Text, textBox_port.Text);

            var worker = new BackgroundWorker();

            worker.DoWork += delegate (object s, DoWorkEventArgs args)
            {
                watch.Restart();
                args.Result = sql.connect();
            };

            worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                label1.Content = args.Result;
                btn_createtable.IsEnabled = true;
                btn_deletetable.IsEnabled = true;
                btn_cleartable.IsEnabled = true;
                btn_searchtable.IsEnabled = true;
                textBox_table.IsEnabled = true;
                btn_load.IsEnabled = true;
                btn_toGLTF.IsEnabled = true;

                btn_connection.IsEnabled = false;
                textBox_host.IsEnabled = false;
                textBox_username.IsEnabled = false;
                textBox_password.IsEnabled = false;
                textBox_database.IsEnabled = false;
                textBox_port.IsEnabled = false;
                watch.Stop();
                label1.Content += ms(watch);
            };

            worker.RunWorkerAsync();
            }

        private void setTableName(postgreSQL sql, string table)
        {
            sql.table_dat = table + "_dat";
            sql.table_dat_log = sql.table_dat + "_log";
            sql.table_xdo = table + "_xdo";
            sql.table_xdo_log = sql.table_xdo + "_log";
        }
        private void button_ClearTable_Click(object sender, RoutedEventArgs e)
        {
            setTableName(sql, textBox_table.Text);

            var worker = new BackgroundWorker();
            worker.DoWork += delegate (object o, DoWorkEventArgs args)
            {
                watch.Restart();
                args.Result = sql.clearTable();
            };

            worker.RunWorkerCompleted += delegate (object o, RunWorkerCompletedEventArgs args)
            {
                watch.Stop();
                label1.Content = args.Result;
                label1.Content += ms(watch);
            };
            worker.RunWorkerAsync();
        }
        private string ms(Stopwatch sw)
        {
            return "(" + sw.ElapsedMilliseconds.ToString() + " ms)";
        }
        private void button_DeleteTable_Click(object sender, RoutedEventArgs e)
        {
            setTableName(sql, textBox_table.Text);

            var worker = new BackgroundWorker();
            worker.DoWork += delegate (object o, DoWorkEventArgs args)
            {
                watch.Restart();
                args.Result = sql.deleteTable();
            };

            worker.RunWorkerCompleted += delegate (object o, RunWorkerCompletedEventArgs args)
            {
                watch.Stop();
                label1.Content = args.Result;
                label1.Content += ms(watch);
            };
            worker.RunWorkerAsync();
        }
        
        private void button_search_Click(object sender, RoutedEventArgs e)
        {
            int min, max;
            setTableName(sql, textBox_table.Text);
            var folderPathText = folder_path.Text;
            if(!int.TryParse(textBox_Min.Text, out min))
            {
                System.Windows.Forms.MessageBox.Show("Min level 값을 정수형으로 변환할 수 없습니다. 0으로 검색합니다.");
                min = 0;
            }
            if(!int.TryParse(textBox_Max.Text, out max))
            {
                System.Windows.Forms.MessageBox.Show("Max level 값을 정수형으로 변환할 수 없습니다. 15로 검색합니다.");
                max = 15;
            }
            if (checkbox_tableClear.IsChecked == true)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    this.button_ClearTable_Click(sender, e);
                }));
            }
            var worker = new BackgroundWorker();
            worker.DoWork += delegate (Object s, DoWorkEventArgs args)
            {
                watch.Restart();
                args.Result = sql.search(folderPathText, min, max);
            };
            worker.RunWorkerCompleted += delegate (Object s, RunWorkerCompletedEventArgs args)
            {
                watch.Stop();
                label1.Content = args.Result;
                label1.Content += ms(watch);
                clickSearch = true;

                if (clickSearch && clickCheckVersion)
                    btn_repair.IsEnabled = true;
                btn_check_version_error.IsEnabled = true;
            };
            worker.RunWorkerAsync();
        }
        private void progressBarWorker()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                while (true)
                {
                    (sender as BackgroundWorker).ReportProgress(new Random().Next(1, 100));
                    Thread.Sleep(100);
                }
            };
            worker.ProgressChanged += delegate (object sender, ProgressChangedEventArgs e)
            {
                pbStatus.Value = e.ProgressPercentage;
            };

            worker.RunWorkerAsync();
        }
        
        private void button_changefolder(object sender, RoutedEventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            folderDialog.ShowDialog();
            folder_path.Text = folderDialog.SelectedPath;
        }

        private void button_load_Click(object sender, RoutedEventArgs e)
        {
            setTableName(sql, textBox_table.Text);

            listView1.Items.Clear();
            listView_Log.Items.Clear();
            dat_info_listview.Items.Clear();
            dat_log_listview.Items.Clear();
            
            var worker = new BackgroundWorker();
            worker.DoWork += delegate (object o, DoWorkEventArgs args)
            {
                watch.Restart();

                var returnObject = new BackgroundWorkerReturnClass();
                returnObject.xdoList = sql.loadXDOTable();
                returnObject.xdoLogList = sql.loadXDOLogTable();
                returnObject.datList = sql.loadDATTable();
                returnObject.datLogList = sql.loadDATLogTable();
                args.Result = returnObject;
            };

            worker.RunWorkerCompleted += delegate (object o, RunWorkerCompletedEventArgs args)
            {
                var result = (BackgroundWorkerReturnClass)args.Result;

                foreach(var key in result.xdoList)
                {
                    listView1.Items.Add(new myXDOItem
                    {
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

                foreach (var item in result.xdoLogList)
                {
                    listView_Log.Items.Add(item);
                }

                foreach (var item in result.datList)
                {
                    dat_info_listview.Items.Add(new myDATItem
                    {
                        FileName = item.datFileName,
                        Level = item.level,
                        IDX = item.idx.ToString(),
                        IDY = item.idy.ToString(),
                        ObjCount = item.objCount,
                        Key = String.Join(", ", item.key.ToArray()),
                        Altitude = String.Join(", ", item.altitude.ToArray()),
                        XDO = String.Join(", ", item.dataFile.ToArray()),
                        Version = String.Join(", ", item.version.ToArray()),
                        imgFileName = String.Join(", ", item.imgFileName.ToArray()),
                        ImgLevel = String.Join(", ", item.imgLevel.ToArray()),
                        minX = String.Join(", ", item.minX.ToArray()),
                        minY = String.Join(", ", item.minY.ToArray()),
                        minZ = String.Join(", ", item.minZ.ToArray()),
                        maxX = String.Join(", ", item.maxX.ToArray()),
                        maxY = String.Join(", ", item.maxY.ToArray()),
                        maxZ = String.Join(", ", item.maxZ.ToArray()),
                        CenterPos_X = String.Join(", ", item.centerPos_X.ToArray()),
                        CenterPos_Y = String.Join(", ", item.centerPos_Y.ToArray())
                    });
                }

                foreach (var item in result.datLogList)
                {
                    dat_log_listview.Items.Add(item);
                }

                watch.Stop();
                label1.Content = "데이터 개수 " + result.datList.Count + "/" + result.datLogList.Count + "/" + result.xdoList.Count + "/" + result.xdoLogList.Count + "개를 불러왔습니다.";
                label1.Content += ms(watch);
            };
            worker.RunWorkerAsync();
        }

        private void button_DatXdo_check_Click(object sender, RoutedEventArgs e)
        {
            listView_version.Items.Clear();
            setTableName(sql, textBox_table.Text);

            int min = int.Parse(textBox_Min.Text);
            int max = int.Parse(textBox_Max.Text);

            var worker = new BackgroundWorker();
            worker.DoWork += delegate (object s, DoWorkEventArgs args)
            {
                watch.Restart();
                var list = sql.checkVersion(folder_path.Text, min, max);
                foreach (var item in list)
                {
                    listView_version.Items.Add(item);
                }
                args.Result = list.Count + "개가 확인되었습니다.";
            };
            worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                clickCheckVersion = true;
                if (clickSearch && clickCheckVersion)
                    btn_repair.IsEnabled = true;
                watch.Stop();
                label1.Content = args.Result + ms(watch);
            };
            worker.RunWorkerAsync();
        }

        private void button_repair_Click(object sender, RoutedEventArgs e)
        {
            setTableName(sql, textBox_table.Text);

            var worker = new BackgroundWorker();
            worker.DoWork += delegate (object s, DoWorkEventArgs args)
            {
                watch.Restart();
                args.Result = sql.repair();
            };
            worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                watch.Stop();
                label1.Content = (int)args.Result + "개의 DAT 파일이 고쳐졌습니다. 원본 파일은 .backup으로 백업되었습니다.";
                label1.Content += ms(watch);
            };
            worker.RunWorkerAsync();
        }

        private void button_reset_click(object sender, RoutedEventArgs e)
        {

            var worker = new BackgroundWorker();
            worker.DoWork += delegate (object unused, DoWorkEventArgs args)
            {
                watch.Restart();
                var set = new HashSet<string>();
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                    repairBackup(set, folder_path.Text);
                })); 
                foreach (var s in set)
                {
                    Console.WriteLine(s);
                    var withoutbackup = s.Split(new string[] { ".backup" }, StringSplitOptions.None);
                    if (File.Exists(withoutbackup[0]))
                    {
                        File.Delete(withoutbackup[0]);
                    }
                    File.Move(s, withoutbackup[0]);
                }
            };
            worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                watch.Stop();
                label1.Content = ms(watch);
            };
            worker.RunWorkerAsync();
                
        }

        private void button_GLTF_GLB(object sender, RoutedEventArgs e)
        {
            setTableName(sql, textBox_table.Text);
            var worker = new BackgroundWorker();
            worker.DoWork += delegate (object s, DoWorkEventArgs args)
            {
                watch.Restart();
                sql.makeGLTF(folder_path.Text, int.Parse(textBox_Min.Text), int.Parse(textBox_Max.Text));
                watch.Stop();
                label1.Content = ms(watch);
            };
            worker.RunWorkerAsync();
        }


        private void repairBackup(HashSet<string> set, string path)
        {
            try
            {
                foreach (var tempDirectoryName in Directory.GetDirectories(path))
                {
                    var k = Directory.GetFiles(tempDirectoryName, "*.backup", SearchOption.TopDirectoryOnly);
                    if (k.Length > 0)
                    {
                        foreach (var file in k)
                        {
                            if(!file.Contains(".dat"))
                                set.Add(file);
                        }
                    }
                    repairBackup(set, tempDirectoryName);
                }
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
            }
        }
    }

    public class myXDOItem
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
    public class myDATItem
    {
        public string FileName { get; set; }
        public int Level { get; set; }
        public string IDX { get; set; }
        public string IDY { get; set; }
        public int ObjCount { get; set; }
        public string ImgLevel { get; set; }
        public string Key { get; set; }
        public string Version { get; set; }
        public string CenterPos_X { get; set; }
        public string CenterPos_Y { get; set; }
        public string Altitude { get; set; }
        public string XDO { get; set; }
        public string imgFileName { get; set; }
        public string minX { get; set; }
        public string minY { get; set; }
        public string minZ { get; set; }
        public string maxX { get; set; }
        public string maxY { get; set; }
        public string maxZ { get; set; }
    }

    public class BackgroundWorkerReturnClass
    {
        public List<XDODBItem> xdoList;
        public List<XDOLogItem> xdoLogList;
        public List<DATDBItem> datList;
        public List<DATLogItem> datLogList;
    }
}
