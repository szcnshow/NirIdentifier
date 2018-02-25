using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using NirIdentifier.Common;

namespace NirIdentifier.SystemSetup
{
    /// <summary>
    /// ScanParameterEditor.xaml 的交互逻辑
    /// </summary>
    public partial class ScanParameterEditor : UserControl
    {
        /// <summary>
        /// 属性变更消息
        /// </summary>
        SettingFile.scanParameter curpara = new SettingFile.scanParameter();

        public ScanParameterEditor()
        {
            InitializeComponent();
            checkAutoScan.IsChecked = curpara.autoScan;
            //checkAutoScan_Checked(null, null);
        }

        public void SetScanParameter(SettingFile.scanParameter newpara)
        {
            if (newpara != null)
                curpara = newpara.Clone();
            else
                curpara = new SettingFile.scanParameter();      //设置为初始化数据

            if (checkAutoScan.IsChecked == curpara.autoScan)    //AutoScan相同，需要强制刷新
                checkAutoScan_Checked(null, null);
            else    //将会自动刷新
                checkAutoScan.IsChecked = curpara.autoScan;
        }

        public SettingFile.scanParameter GetScanParameter()
        {
            try
            {
                curpara.autoScan = checkAutoScan.IsChecked == true;
                curpara.autoScanTime = float.Parse(txtScanTime.Text);
                curpara.integrateTime = curpara.autoScanTime;
                curpara.scanCount = int.Parse(txtScanCount.Text);
                curpara.autoScanFirstX = float.Parse(txtFirstX.Text);
                curpara.autoScanLastX = float.Parse(txtLastX.Text);

                return curpara.Clone();     //返回一个新值，防止引用错误
            }
            catch (Exception ex)
            {
                CommonMethod.ErrorMsgBox(ex.Message);
                return null;
            }
        }

        private void checkAutoScan_Checked(object sender, RoutedEventArgs e)
        {
            Visibility vis = (checkAutoScan.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;
            labelScanCount.Visibility = vis;
            txtScanCount.Visibility = vis;

            vis = (checkAutoScan.IsChecked == true) ? Visibility.Visible : Visibility.Collapsed;
            labelFirstX.Visibility = vis;
            txtFirstX.Visibility = vis;
            labelLastX.Visibility = vis;
            txtLastX.Visibility = vis;

            txtScanTime.Text = (checkAutoScan.IsChecked == true) ? curpara.autoScanTime.ToString() : curpara.integrateTime.ToString();
            txtScanCount.Text = curpara.scanCount.ToString();
            txtFirstX.Text = curpara.autoScanFirstX.ToString();
            txtLastX.Text = curpara.autoScanLastX.ToString();
        }

        public void EnableEditor(bool enabled)
        {
            txtScanTime.IsReadOnly = !enabled;
            txtScanCount.IsReadOnly = !enabled;
            txtFirstX.IsReadOnly = !enabled;
            txtLastX.IsReadOnly = !enabled;
            checkAutoScan.IsEnabled = enabled;
        }

    }
}
