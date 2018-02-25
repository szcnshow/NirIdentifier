using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Threading;

namespace NirIdentifier.Common
{
    /// <summary>
    /// ProcessWaitDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessWaitDialog : Window
    {
        public delegate void SetProcessAndMsgDeletage(string displayMsg, int maxProcessValue, int curProcessValue, out bool abortTask);
        public delegate bool ProcessTask(SetProcessAndMsgDeletage callBack);
        
        ProcessTask curTask = null;
        Thread taskThread = null;
        bool userAbortTask = false;

        /// <summary>
        /// 操作等待界面
        /// </summary>
        /// <param name="task"></param>
        /// <param name="buttonName">取消按钮的内容，如果为NULL，表示隐藏取消按钮</param>
        public ProcessWaitDialog(ProcessTask task, string buttonName)
        {
            InitializeComponent();   
            curTask = task;

            if (buttonName == null)
                btnCancel.Visibility = System.Windows.Visibility.Hidden;
            else
                btnCancel.Text = buttonName;
        }

        /// <summary>
        /// 设置进度条信息
        /// </summary>
        public void SetProcessAndMsg(string displayMsg, int maxProcessValue, int curProcessValue, out bool abortTask)
        {
            abortTask = userAbortTask;

            if(abortTask == false)
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new SetMessageDelegate(SetMsg), displayMsg, maxProcessValue, curProcessValue);
        }
        public delegate void SetMessageDelegate(string displayMsg, int maxProcessValue, int curProcessValue);
        private void SetMsg(string displayMsg, int maxProcessValue, int curProcessValue)
        {
            if (txtProcess.Text != displayMsg)
                txtProcess.Text = displayMsg;
            if (barProcess.Maximum != maxProcessValue)
                barProcess.Maximum = maxProcessValue;
            if (barProcess.Value != curProcessValue)
                barProcess.Value = curProcessValue;
        }

        /// <summary>
        /// 运行任务
        /// </summary>
        private void RunTask()
        {
            taskSucessed = curTask(SetProcessAndMsg);
        }

        DispatcherTimer checkTimer = null;
        bool taskSucessed = false;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Common.CommonMethod.HideWindowSystemButton(this);

            //启动扫描线程
            taskThread = new Thread(new ThreadStart(RunTask));
            taskThread.IsBackground = false;
            taskThread.Start();

            //检查扫描是否完成
            checkTimer = new DispatcherTimer();
            checkTimer.Interval = new TimeSpan(0, 0, 1);
            checkTimer.Tick += new EventHandler(checkTimer_Tick);
            checkTimer.Start();
        }

        /// <summary>
        /// 检查任务是否完成, 完成后自动关闭
        /// </summary>
        void checkTimer_Tick(object sender, EventArgs e)
        {
            if (taskThread == null || taskThread.IsAlive == false)
            {
                DialogResult = taskSucessed;
                checkTimer.Stop();
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            userAbortTask = true;
            btnCancel.IsEnabled = false;

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new SetMessageDelegate(SetMsg), "正在取消......", barProcess.Maximum, barProcess.Value);
        }
    }
}
