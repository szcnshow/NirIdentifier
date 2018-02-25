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

namespace NirIdentifier
{
    /// <summary>
    /// MainSelect.xaml 的交互逻辑
    /// </summary>
    public partial class MainSelect : UserControl
    {
        public MainSelect()
        {
            InitializeComponent();
        }

        private void btnCalibration_Click(object sender, RoutedEventArgs e)
        {
            OperatorWindowChangeScreen("Correction");
        }

        public void OperatorWindowChangeScreen(string dstControl)
        {
            NirIdentifier.MainWindow temp = App.Current.MainWindow as NirIdentifier.MainWindow;
            if (temp != null)
            {
                temp.MainSelectChange(dstControl);
            }
        }

        private void btnDetect_Click(object sender, RoutedEventArgs e)
        {
            OperatorWindowChangeScreen("DetectPanel");
        }

        private void btnSetup_Click(object sender, RoutedEventArgs e)
        {
            OperatorWindowChangeScreen("SystemSetupPanel");
        }

        //离线分析
        private void btnOffline_Click(object sender, RoutedEventArgs e)
        {
            OperatorWindowChangeScreen("OfflinePanel");
        }

        //常规扫描界面
        private void btnNormalScan_Click(object sender, RoutedEventArgs e)
        {
            OperatorWindowChangeScreen("NormalScanPanel");
        }
    }
}
