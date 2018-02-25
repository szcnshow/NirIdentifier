using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Threading;
using NirIdentifier.Common;

namespace NirIdentifier.Offline
{
    /// <summary>
    /// SpecFileIdentify.xaml 的交互逻辑
    /// </summary>
    public partial class SpecFileIdentify : Window
    {
        public List<DrugInfo> IdentSampleList = null;
        public string ErrorString { get; set; }     //分析中出现的错误

        public SpecFileIdentify()
        {
            InitializeComponent();
            ErrorString = null;
        }

        public SpecFileIdentify(List<DrugInfo> fileList)
        {
            InitializeComponent();

            IdentSampleList = fileList;
            AddToList();
        }

        public SpecFileIdentify(DrugInfo sampleData)
        {
            InitializeComponent();

            IdentSampleList = new List<DrugInfo>();
            IdentSampleList.Add(sampleData);
            AddToList();
        }

        //将要分析的光谱文件增加到列表中
        private void AddToList()
        {
            if (IdentSampleList == null)
                return;

            int currow = 0;
            foreach (DrugInfo data in IdentSampleList)
            {
                //新增一行
                RowDefinition row = new RowDefinition();
                row.Height = GridLength.Auto;
                gridFileLsit.RowDefinitions.Add(row);

                //加入图标
                Image img = new Image();
                img.Stretch = Stretch.None;
                img.BeginInit();
                img.Source = new BitmapImage(new Uri(@"pack://application:,,/images/NA.png"));     //初始状态设置为NA
                img.EndInit();
                img.VerticalAlignment = VerticalAlignment.Center;
                img.HorizontalAlignment = HorizontalAlignment.Center;
                img.Stretch = Stretch.None;
                Grid.SetRow(img, currow);
                Grid.SetColumn(img, 0);
                gridFileLsit.Children.Add(img);

                //加入文件名
                TextBlock txt = new TextBlock();
                txt.Text = System.IO.Path.GetFileName(data.filename);
                txt.FontSize = 16;
                txt.VerticalAlignment = VerticalAlignment.Center;
                txt.Margin = new Thickness(5, 10, 0, 0);
                Grid.SetRow(txt, currow);
                Grid.SetColumn(txt, 1);
                gridFileLsit.Children.Add(txt);

                currow++;
            }
        }

        private delegate void ShowProgressTextDelegate(string msg);
        private void ShowProgressText(string msg)
        {
            txtProgress.Text = msg;
        }

        private delegate void SetSuccessedDelegate(int index);
        private void SetSuccessed(int index)
        {
            //找到需要改变的图标
            foreach (UIElement item in gridFileLsit.Children)
            {
                if(Grid.GetColumn(item) == 0 && Grid.GetRow(item) == index)
                {
                    Image img = item as Image;
                    img.BeginInit();
                    img.Source = img.Source = new BitmapImage(new Uri(@"pack://application:,,/images/OK_16.png"));     //初始状态设置为NA
                    img.EndInit();
                }
            }
        }

        public void IdentifyFile()      //分析线程
        {
        }

        DispatcherTimer timer = null;                 //检查分析是否完成的Timer
        Thread identThread = null;                  //分析的线程

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NirIdentifier.Common.CommonMethod.HideWindowSystemButton(this);
            ErrorString = null;

            identThread = new Thread(new ThreadStart(IdentifyFile));
            identThread.IsBackground = true;
            identThread.Start();

            timer = new DispatcherTimer();
            timer.Interval = new System.TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();        
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (identThread== null || identThread.IsAlive == false)
            {
                timer.Stop();

                if (ErrorString != null)
                    DialogResult = false;
                else
                    DialogResult = true;

                this.Close();
            }
        }

    }
}
