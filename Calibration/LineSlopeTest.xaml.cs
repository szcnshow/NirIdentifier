using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NirIdentifier.Calibration
{
    /// <summary>
    /// EnergyDistribution.xaml 的交互逻辑
    /// </summary>
    public partial class LineSlopeTest : UserControl
    {
       double calThresold;

        /// <summary>
        /// 是否显示谱图消息
        /// </summary>
        public static readonly RoutedEvent DisplayCheckClickedEvent = EventManager.RegisterRoutedEvent("DisplayCheckClicked",
                RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LineSlopeTest));
        public event RoutedEventHandler DisplayCheckClicked
        {
            add { AddHandler(DisplayCheckClickedEvent, value); }
            remove { RemoveHandler(DisplayCheckClickedEvent, value); }
        }

        public LineSlopeTest()
        {
            InitializeComponent();

        }


        /// <summary>
        /// 初始化面板
        /// </summary>
        /// <param name="calName">校验的名称</param>
        /// <param name="thresold">设定的阈值</param>
        public void InitPanel(string calName, double thresold)
        {
            txtCalibrateName.Text = calName;
            calThresold = thresold;
         //   txtThresold.Text = thresold.ToString();
        }

        /// <summary>
        /// 显示光谱选择框，发送命令到外面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkShowSpectrum_Checked(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs args = new RoutedEventArgs(DisplayCheckClickedEvent, checkShowSpectrum.IsChecked);
            RaiseEvent(args);
        }

        /// <summary>
        /// 是否要显示光谱
        /// </summary>
        public bool NeedShowSpectrum()
        {
            return checkShowSpectrum.IsChecked == true;
        }

        /// <summary>
        /// 设置实际测量的值
        /// </summary>
        public void SetRealCalibrateValue(bool greatThan)
        {
           // txtRealValue.Text = realValue.ToString();
           // if (greatThan)
                Common.CommonMethod.SetImageSource(imageResult, greatThan ? "OK_32.png" : "error_32.png");
                scanProgress.Visibility = Visibility.Collapsed;
               
            //else
            //    Common.CommonMethod.SetImageSource(imageResult, realValue <= calThresold ? "OK_32.png" : "error_32.png");
        }

        /// <summary>
        /// 设置进度条的状态
        /// </summary>
        /// <param name="maxValue">最大值</param>
        /// <param name="curValue">当前值</param>
        public void SetProgressBar(double maxValue, double curValue)
        {
            scanProgress.Maximum = maxValue;
            scanProgress.Value = curValue;
        }

        private void checkShowSpectrum_UnChecked(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs args = new RoutedEventArgs(DisplayCheckClickedEvent, checkShowSpectrum.IsChecked);
            RaiseEvent(args);
        }
    }
}
