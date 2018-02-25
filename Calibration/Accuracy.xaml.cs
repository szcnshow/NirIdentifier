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
    /// Accuracy.xaml 的交互逻辑
    /// </summary>
    public partial class Accuracy : UserControl
    {
        double accThresold;
        double polyThresold;
        /// <summary>
        /// 是否显示谱图消息
        /// </summary>
        public static readonly RoutedEvent DisplayCheckClickedEvent = EventManager.RegisterRoutedEvent("DisplayCheckClicked",
                RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Accuracy));
        public event RoutedEventHandler DisplayCheckClicked
        {
            add { AddHandler(DisplayCheckClickedEvent, value); }
            remove { RemoveHandler(DisplayCheckClickedEvent, value); }
        }
        public Accuracy()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 初始化面板
        /// </summary>
        /// <param name="calName">校验的名称</param>
        /// <param name="accThresold">水峰波数精度阈值</param>
        /// <param name="ployThresold">聚苯乙烯波数精度阈值</param>
        public void InitPanel(string calName, double accThresold, double ployThresold)
        {
            txtCalibrateName.Text = calName;
            this.accThresold = accThresold;
            this.polyThresold = ployThresold;
            txtAccThresold.Text = accThresold.ToString();
            txtAccThresold.TextAlignment = TextAlignment.Center;

            txtPloyThresold.Text = ployThresold.ToString();
            txtPloyThresold.TextAlignment = TextAlignment.Center;
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
        /// <param name="realValue">水峰波数精度测量值</param>
        /// <param name="realDev">聚苯乙烯波数精度测量值</param>
        /// </summary>
        public void SetRealCalibrateValue(double accValue, double polyValue)
        {
            txtRealAccValue.Text = accValue.ToString();
            txtRealAccValue.TextAlignment = TextAlignment.Center;
            if (polyValue != -1)
            {
                txtRealPloyValue.Text = polyValue.ToString();
                txtRealPloyValue.TextAlignment = TextAlignment.Center;
            }
            //if (greatThan)
            //{
            //if (Math.Abs(accValue) > accThresold)
            //{
            //    Common.CommonMethod.SetImageSource(imageResult, "error_32.png");
            //    txtRealAccValue.Background = Brushes.Red;
            //}
            //if (ployValue < ployThresold)
            //{
            //}
           // else
          //  if(ployValue<ployThresold)
            //Common.CommonMethod.SetImageSource(imageResult, accValue <= accThresold ? "OK_32.png" : "error_32.png");
            if (polyValue != -1)
            {
                Common.CommonMethod.SetImageSource(imageResult, polyValue < polyThresold && accValue <= accThresold ? "OK_32.png" : "error_32.png");
                scanProgress.Visibility = Visibility.Collapsed;
            }
            // }
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

        private void polyCheckShowSpectrum_UnChecked(object sender, RoutedEventArgs e)
        {
            //RoutedEventArgs args = new RoutedEventArgs(DisplayCheckClickedEvent, polyCheckShowSpectrum.IsChecked);
            //RaiseEvent(args);
        }

        private void polyCheckShowSpectrum_Checked(object sender, RoutedEventArgs e)
        {
            //RoutedEventArgs args = new RoutedEventArgs(DisplayCheckClickedEvent, polyCheckShowSpectrum.IsChecked);
            //RaiseEvent(args);
        }
    }
}
