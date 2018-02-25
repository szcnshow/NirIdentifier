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
using NirIdentifier.Common;

namespace NirIdentifier.Detect
{
    /// <summary>
    /// DetectPanel.xaml 的交互逻辑
    /// </summary>
    public partial class DetectPanel : UserControl
    {
        public DetectPanel()
        {
            InitializeComponent();
        }

        public void OperatorWindowChangeScreen(string dstControl)
        {
            NirIdentifier.MainWindow temp = App.Current.MainWindow as NirIdentifier.MainWindow;
            if (temp != null)
                temp.ChangeScreen(dstControl);
        }

        private void newSamplePanel_NewPanelExit(object sender, RoutedEventArgs e)
        {
            OperatorWindowChangeScreen("MainSelectPanel");
        }

        private void resultSamplePanel_ResultPanelExit(object sender, RoutedEventArgs e)
        {
            OperatorWindowChangeScreen("MainSelectPanel");
        }

        private void newSamplePanel_NewSampleData(object sender, RoutedEventArgs e)
        {
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            OperatorWindowChangeScreen("MainSelectPanel");
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (e.AddedItems.Contains(tabExit))
            //    btnReturn_Click(null, null);
        }
    }
}
