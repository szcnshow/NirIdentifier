using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace NirIdentifier.Common
{
    /// <summary>
    /// ClusterProcessDlg.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessDlg : Window
    {
        public ProcessDlg()
        {
            InitializeComponent();
        }

        public void SetProcessAndMsg(string displayMsg, int maxProcessValue, int curProcessValue)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new SetMessageDelegate(SetMsg), displayMsg, maxProcessValue, curProcessValue);
        }
        public delegate void SetMessageDelegate(string displayMsg, int maxProcessValue, int curProcessValue);
        private void SetMsg(string displayMsg, int maxProcessValue, int curProcessValue)
        {
            if (txtProcess.Text != displayMsg)
                txtProcess.Text = displayMsg;
            if (clusterProcess.Maximum != maxProcessValue)
                clusterProcess.Maximum = maxProcessValue;
            if (clusterProcess.Value != curProcessValue)
                clusterProcess.Value = curProcessValue;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CommonMethod.HideWindowSystemButton(this);
        }
    }
}
