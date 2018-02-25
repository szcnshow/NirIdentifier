using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;
using NirIdentifier.Common;
using System.Collections.Generic;
using System.IO;

namespace NirIdentifier.Detect
{
    /// <summary>
    /// ScanWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ScanWindow : Window
    {
        DispatcherTimer checkTimer;

        private Common.SettingFile.scanParameter method;
        public string ErrorString = null;
        private System.Threading.Thread measureTread = null;
        private int scanStartTime = 0;

        public ScanWindow(Common.SettingFile.scanParameter scanMethod)
        {
            method = scanMethod;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Common.CommonMethod.HideWindowSystemButton(this);

            progressScan.Maximum = method.autoScan? method.autoScanTime : method.integrateTime * method.scanCount;
            progressScan.Value = 0;

            //启动扫描线程
            measureTread = new Thread(new ThreadStart(ScanAndDetectThread));
            measureTread.IsBackground = true;
            measureTread.Start();

            //检查扫描是否完成
            checkTimer = new DispatcherTimer();
            checkTimer.Interval = new TimeSpan(0, 0, 1);
            checkTimer.Tick += new EventHandler(checkTimer_Tick);
            checkTimer.Start();

            scanStartTime = Environment.TickCount;
        }

        void checkTimer_Tick(object sender, EventArgs e)
        {
            if (measureTread.IsAlive)
            {
                if (Environment.TickCount - scanStartTime > progressScan.Maximum * 2*1000+20*1000)      //超过规定扫描时间两倍了,20*1000=预计分析的时间
                {
                    checkTimer.Stop();
                    measureTread.Abort();
                    Thread.Sleep(100);      //等待线程终止
                    this.Close();
                }
                else
                    progressScan.Value++;
            }
            else
            {
                checkTimer.Stop();
                this.Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (measureTread.IsAlive)
            {
                e.Cancel = false;
                return;
            }

            if (ErrorString != null)
                DialogResult = false;
            else
                DialogResult = true;
        }

        private void ScanAndDetectThread()
        {
            try
            {
            }
            catch (Exception ex)
            {
                ErrorString = ex.Message;
            }
        }

        private delegate void ShowProgressTextDelegate(string msg);
        private void ShowProgressText(string msg)
        {
            txtTitle.Text = msg;
        }

        public bool IdentifyFile()      //分析线程
        {


            return true;
        }
    }
}
