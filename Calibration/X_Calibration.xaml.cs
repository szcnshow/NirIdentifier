using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.IO;

namespace RFDI.Calibration
{
    /// <summary>
    /// Calibration.xaml 的交互逻辑
    /// </summary>
    public partial class X_Calibration : UserControl
    {
        class XPeakDataInfo
        {
            public int index { get; set; }
            public int pixel { get; set; }
            public int wavenumber { get; set; }

            public XPeakDataInfo(int inIndex, int inPixel, int inWavenumber)
            {
                index = inIndex;
                pixel = inPixel;
                wavenumber = inWavenumber;
            }
        }
        ObservableCollection<XPeakDataInfo> listPeak = new ObservableCollection<XPeakDataInfo>();

        //应用按钮的通知消息
        public static readonly RoutedEvent CmdApplyEvent = EventManager.RegisterRoutedEvent("CmdApply",
                RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(X_Calibration));

        public event RoutedEventHandler CmdApply
        {
            add { AddHandler(CmdApplyEvent, value); }
            remove { RemoveHandler(CmdApplyEvent, value); }
        }

        public X_Calibration()
        {
            InitializeComponent();

            for (int i = 0; i < 10; i++)
            {
                XPeakDataInfo info = new XPeakDataInfo(i, i*10, i*10);
                listPeak.Add(info);
            }
            viewFileInfo.ItemsSource = listPeak;
            viewFileInfo.Items.Refresh();

        }

        
        private void btnXCalculate_Click(object sender, RoutedEventArgs e)
        {
            int i = listPeak[2].index;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            string imgpath = System.IO.Path.Combine(Common.SettingData.SettingPath, "ramantest.bmp");
            if (File.Exists(imgpath))
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(imgpath, UriKind.Absolute);
                bi.EndInit();
                imgXCal.Stretch = Stretch.Uniform;
                imgXCal.Source = bi;
            }
        }

        //应用当前参数，并通知上级可以开始下一步了
        private void btnXApply_Click(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = CmdApplyEvent;
            args.Source = this;
            RaiseEvent(args);
        }

    }
}
