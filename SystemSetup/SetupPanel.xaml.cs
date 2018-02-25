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

namespace NirIdentifier.SystemSetup
{
    /// <summary>
    /// SetupPanel.xaml 的交互逻辑
    /// </summary>
    public partial class SetupPanel : UserControl
    {
        public SetupPanel()
        {
            InitializeComponent();
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            OperatorWindowChangeScreen("MainSelectPanel");
        }

        public void OperatorWindowChangeScreen(string dstControl)
        {
            NirIdentifier.MainWindow temp = App.Current.MainWindow as NirIdentifier.MainWindow;
            if (temp != null)
                temp.ChangeScreen(dstControl);
        }
    }
}
