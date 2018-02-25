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
    /// PPDeviation.xaml 的交互逻辑
    /// </summary>
    public partial class PPDeviation : UserControl
    {
        double calThresold;
        double devThresold;
        double ipaThresold;
        double engThresold;

        /// <summary>
        /// 是否显示谱图消息
        /// </summary>
        public static readonly RoutedEvent DisplayCheckClickedEvent = EventManager.RegisterRoutedEvent("DisplayCheckClicked",
                RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PPDeviation));
        public event RoutedEventHandler DisplayCheckClicked
        {
            add { AddHandler(DisplayCheckClickedEvent, value); }
            remove { RemoveHandler(DisplayCheckClickedEvent, value); }
        }

        public PPDeviation()
        {
            InitializeComponent();

        }

        /// <summary>
        /// 初始化面板
        /// </summary>
        /// <param name="calName">校验的名称</param>
        /// <param name="thresold">100%线噪比阈值</param>
        /// <param name="devThresold">偏差阈值</param>
        public void InitPanel(string calName, double thresold,double devThresold,double ipaThresold,double engThresold)
        {
            txtCalibrateName.Text = calName;
            calThresold = thresold;
            this.devThresold = devThresold;
            txtLineNioseThresold.Text = thresold.ToString();
            txtLineNioseThresold.TextAlignment = TextAlignment.Center;
            this.ipaThresold = ipaThresold;
            this.engThresold = engThresold;
            txtIpa.Text = ipaThresold.ToString() + "%";
            txtEng.Text = engThresold.ToString() + "%";
            txtDevThresold.Text = devThresold.ToString();
            txtDevThresold.TextAlignment = TextAlignment.Center;
        }

        /// <summary>
        /// 显示光谱选择框，发送命令到外面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkShowSpectrum_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox check = sender as CheckBox;
            if (check == null)
                return;
            RoutedEventArgs args = new RoutedEventArgs(DisplayCheckClickedEvent,check);// checkShowSpectrum.IsChecked);
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
        /// <param name="realValue">100%线噪比测量值</param>
        /// <param name="realDev">偏差测量值</param>
        /// </summary>
        public void SetRealCalibrateValue(double ppValue,double realDev,double ipaValue,double engValue)
        {
            txtRealLineNoiseValue.Text = ppValue.ToString();
            txtRealLineNoiseValue.TextAlignment = TextAlignment.Center;
            txtRealDevValue.Text = realDev.ToString();
            txtRealDevValue.TextAlignment = TextAlignment.Center;

            txtRealIpa.Text = ipaValue.ToString() + "%";
            txtRealEng.Text = engValue.ToString() + "%";
            //if (greatThan)
            //{
            //if (Math.Abs(realDev) > devThresold)
            //{
            //    Common.CommonMethod.SetImageSource(imageResult, "error_32.png");
            //    txtRealDevValue.Background = Brushes.Red;
            //}
            //else
                Common.CommonMethod.SetImageSource(imageResult, ppValue <= calThresold && realDev <= devThresold&&ipaValue>=ipaThresold&&engValue<=engThresold ? "OK_32.png" : "error_32.png");
                scanProgress.Visibility = Visibility.Collapsed;
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
            CheckBox check = sender as CheckBox;
            if (check == null)
                return;
            RoutedEventArgs args = new RoutedEventArgs(DisplayCheckClickedEvent, check);
            RaiseEvent(args);
        }
    }
}
