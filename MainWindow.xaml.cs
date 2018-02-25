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
using System.Windows.Threading;
using System.Threading;

namespace NirIdentifier
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        UserControl curOperatePanel = null;
        public MainWindow()
        {
            InitializeComponent();
            btnTest.Visibility = Visibility.Collapsed;
            InstrumentState(double.MaxValue, null);
            ChangeScreen("MainWindow", "MainSelectPanel");
        }

        //页面之间的切换
        public void ChangeScreen(string srcCtrlName, string dstCtrlName)
        {
            if (curOperatePanel != null)
                curOperatePanel.Visibility = Visibility.Hidden;
            switch (dstCtrlName)
            {
                case "MainSelectPanel":
                    curOperatePanel = new MainSelect();
                    break;
                case "DetectPanel":
                    curOperatePanel = new NirIdentifier.Detect.DetectPanel();
                    break;
                case "SystemSetupPanel":
                    curOperatePanel = new NirIdentifier.SystemSetup.SetupPanel();
                    break;
                case "Correction":
                    //curOperatePanel = new NirIdentifier.Calibration.Correction();
                    break;
                case "NormalScan":
                    curOperatePanel = new NirIdentifier.NormalScan.NormalScan();
                    break;
            }
            GridScreenContainer.Children.Clear();
            GridScreenContainer.Children.Add(curOperatePanel);
            curOperatePanel.Width = GridScreenContainer.Width;
            curOperatePanel.Height = GridScreenContainer.Height;
            curOperatePanel.Visibility = Visibility.Visible;
        }

        #region Test Butotn
        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
        }

        private void TestXCalibration()
        {
            //double[] x = new double[]{110.1313, 126.1111, 273.7677, 368.2525, 424.9091, 473.2121, 555.1919, 1229.0505, 1354.7071,1403.7879,1413.9495};
            //double[] y = new double[]{384.1,426.3,801.3,1028.3,1157.6,1266.4,1444.4,2664.4,2852.9,2923.8,2938.3};
            double[] pixelX = new double[] { 127.0909, 278.6465, 374.9293, 482.7273, 566.8081, 1387.9293 };
            double[] waveX = new double[] { 426.3, 801.3, 1028.3, 1266.4, 1444.4, 2852.9 };

            int info;
            alglib.barycentricinterpolant p;
            alglib.polynomialfitreport rep;

            //三阶多项式拟合,得到拟合系数
            alglib.polynomialfit(pixelX, waveX, 4, out info, out p, out rep);

            //计算起始点和终止点波数
            double firstx = alglib.barycentriccalc(p, 0);
            double lastx = alglib.barycentriccalc(p, 1649);

            //按照1.3波数插值，得到X轴数量
            int count = (int)((lastx - firstx) / 1.3);
            double[] newx = new double[count];
            double stepx = (lastx - firstx) / (double)count;
            //得到X轴波数值
            for (int i = 0; i < count; i++)
                 newx[i] = i*stepx + firstx;

            //读入像素谱中的Y值
            double[] orgyData = new double[1650];
            System.IO.StreamReader reader = new System.IO.StreamReader("f:\\temp\\ydata.csv", Encoding.GetEncoding("gb2312"));
            string readstr = reader.ReadLine();
            int index = 0;
            while (readstr != null && readstr.Trim() != "" && index<1650)
            {
                orgyData[index] = float.Parse(readstr);
                index++;

                readstr = reader.ReadLine();
            }

            //按照像素谱得到对应的X值(1650个）
            double[] tempx = new double[orgyData.Length];
            for(int i=0; i<tempx.Length; i++)
                tempx[i] = alglib.barycentriccalc(p, (double)i);

            //对像素谱Y值和像素谱对应的波数值进行三次样条曲线拟合
            alglib.spline1dinterpolant c;
            alglib.spline1dbuildcubic(tempx, orgyData, out c);

            //对X轴波数插值后的值计算Y值
            double[] newyData = new double[newx.Length];
            for (int i = 0; i < newx.Length; i++)
                newyData[i] = alglib.spline1dcalc(c, newx[i]);

            double b = newx[0];
        }

        private void TestYCalibration()
        {
            //计算NistPoly
            System.IO.StreamReader reader = new System.IO.StreamReader("f:\\temp\\系数X.txt", Encoding.GetEncoding("gb2312"));
            string readstr = reader.ReadLine();
            double[] conX = new double[3000];
            int index = 0;
            while (readstr != null && readstr.Trim() != "" && index<conX.Length)
            {
                conX[index] = double.Parse(readstr);
                index++;
                readstr = reader.ReadLine();
            }
            reader.Close();

            double[] coe = new double[]{9.71937E-02, 2.28325E-04, -5.86762E-08, 2.16023E-10, -9.77171E-14, 1.15596E-17};
            double[] coeY = new double[index];
            for(int i=0; i<coeY.Length; i++)
                coeY[i] = coe[0] + coe[1] * conX[i] + coe[2] * Math.Pow(conX[i],2) + coe[3] * Math.Pow(conX[i],3) + coe[4] * Math.Pow(conX[i],4) + coe[5] * Math.Pow(conX[i],5);
            /*
            reader = new System.IO.StreamReader("f:\\temp\\nistR-Y.txt", Encoding.GetEncoding("gb2312"));
            index = 0;
            readstr = reader.ReadLine();
            while (readstr != null && readstr.Trim() != "" && index < conX.Length)
            {
                conX[index] = double.Parse(readstr);
                index++;
                readstr = reader.ReadLine();
            }
            reader.Close();
            */
            index = index + 1;
        }

        #endregion

        #region 仪器状态检测和信息

        System.Threading.Thread checkTread = null;

        public bool InstrumentConnected
        {
            get { return (bool)GetValue(InstrumentConnectedProperty); }
            set { SetValue(InstrumentConnectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InstrumentConnected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InstrumentConnectedProperty =
            DependencyProperty.Register("InstrumentConnected", typeof(bool), typeof(MainWindow), new UIPropertyMetadata(false));

        private delegate void InstrumentStateDelegate(double temperature, string errorMsg);
        private void InstrumentState(double temperature, string errorMsg)
        {
            if (temperature == double.MinValue)      //仪器连接失败
            {
                txtCCDStatus.Text = null;
                if(errorMsg != null)
                    txtCCDStatus.Inlines.Add(new Run(errorMsg));
                else
                    txtCCDStatus.Inlines.Add(new Run("不能连接仪器或超时，"));
                Run tempRun = new Run("重新连接......");
                tempRun.Cursor = Cursors.Hand;
                tempRun.ForceCursor = true;
                tempRun.Foreground = Brushes.Blue;
                tempRun.FontStyle = FontStyles.Normal;
                tempRun.MouseLeftButtonDown += new MouseButtonEventHandler(ReConnectText_MouseLeftButtonDown);
                Underline tempUnder = new Underline(tempRun);
                txtCCDStatus.Inlines.Add(tempUnder);

                //txtCCDStatus.Background = Brushes.Yellow;
                Common.CommonMethod.SetImageSource(imgCCDStatus, "Error_16.png");
                imgCCDStatus.Visibility = Visibility.Visible;
                InstrumentConnected = false;
            }
            else if (temperature == double.MaxValue)     //仪器未知状态
            {
                txtCCDStatus.Inlines.Clear();
                Common.CommonMethod.SetImageSource(imgCCDStatus, "Warning_16.png");
                txtCCDStatus.Background = null;
                txtCCDStatus.Text = "正在连接仪器......";
                InstrumentConnected = false;
            }
            else                //仪器已经连接
            {
                txtCCDStatus.Inlines.Clear();
                //仪器需要自检
                if ((DateTime.Now - SettingData.settingData.runing_para.lastCalibartionTime).Hours > SettingData.settingData.runing_para.calibartionTime)
                {
                    txtCCDStatus.Background = Brushes.Yellow;
                    txtCCDStatus.Text = "CCD温度: " + temperature.ToString() + "℃(仪器需要自检)";
                }
                else
                {
                    txtCCDStatus.Background = null;
                    txtCCDStatus.Text = "CCD温度: " + temperature.ToString() + "℃";
                }
                InstrumentConnected = (temperature <= -50);     //小于等于-50度才能操作
                imgCCDStatus.Visibility = Visibility.Visible;
                Common.CommonMethod.SetImageSource(imgCCDStatus, "OK_16.png");
            }
        }

        //重新连接仪器按钮的消息
        void ReConnectText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartConnectInstrument();
        }

        //检测仪器状态的线程
        private void CheckInstument()
        {
            double temperature;
            int count = 0;

            if (CheckRamanInstrument() == false)        //没有检查到仪器
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new InstrumentStateDelegate(InstrumentState), double.MinValue, null);
                return;
            }

            if (RamanInstrument.Reconnect() == false)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new InstrumentStateDelegate(InstrumentState), double.MinValue, RamanInstrument.ErrorString);
                return;
            }

            while (true)
            {
                temperature = RamanInstrument.GetTemperature();
                if (temperature == double.MaxValue)
                {
                    RamanInstrument.Connect();
                    temperature = RamanInstrument.GetTemperature();
                }

                if (temperature == double.MaxValue)         //仪器没连接上
                {
                    if (count < 2)       //重复3次，如果还是没有取到温度，则表示仪器连接错误了
                        count++;
                    else
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, new InstrumentStateDelegate(InstrumentState), double.MinValue, null);
                        break;
                    }
                }
                else
                    count = 0;

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new InstrumentStateDelegate(InstrumentState), temperature, null);

                System.Threading.Thread.Sleep(1000);
                if (Environment.TickCount - SettingData.lastOperateTime > SettingData.settingData.runing_para.InstrumentTimeOut*60*1000)      //超过设定时间没有操作了，需要重新连接
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new InstrumentStateDelegate(InstrumentState), double.MinValue, null);
                    RamanInstrument.DisConnect();

                    break;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string temptxt = SettingData.settingData.runing_para.savePath;
            Title = "拉曼药品快速检测(NirIdentifier Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()+")";
            StartConnectInstrument();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CommonMethod.QuestionMsgBox("确认退出NirIdentifier软件?") == false)
            {
                e.Cancel = true;
                return;
            }

            if (checkTread != null && checkTread.IsAlive)
                checkTread.Abort();

            RamanInstrument.DisConnect();
        }
        #endregion

        private void menuCalibrate_Click(object sender, RoutedEventArgs e)
        {
            ChangeScreen(null, "MainSelectPanel");
        }

        private void menuDetect_Click(object sender, RoutedEventArgs e)
        {
            ChangeScreen(null, "DetectPanel");
        }

        private void menuSetup_Click(object sender, RoutedEventArgs e)
        {
            ChangeScreen(null, "SystemSetupPanel");
        }

        //点击
        private void txtCCDStatus_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (txtCCDStatus.Text == "不能连接仪器，离线分析状态")   //只有在脱机状态下点击才有效
                StartConnectInstrument();
        }

        private void StartConnectInstrument()
        {
            if(checkTread==null || checkTread.IsAlive == false)
            {
                InstrumentState(double.MaxValue, null);   //设置为正在连接仪器

                if (checkTread != null)
                    checkTread.Abort();

                checkTread = new Thread(new ThreadStart(CheckInstument));
                checkTread.IsBackground = true;
                checkTread.Start();
            }
        }

        //通过系统设备表查询是否连接了拉曼光谱仪器
        private bool CheckRamanInstrument()
        {
            System.Management.ManagementObjectSearcher wmiSearcher = new System.Management.ManagementObjectSearcher("Select * From WIN32_USBControllerDevice");
            foreach (System.Management.ManagementObject wmi_USB in wmiSearcher.Get())
            {

                string strPath = wmi_USB.Path.RelativePath;
                if (strPath.Contains(@"VID_136E&PID_000D"))     //这是EnwaveRaman仪器的Device ID(硬件管理器-详细信息-硬件ID
                {
                    return true;
                }
            }

            return false;
        }

        private void btnToolButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if(btn == btnHomePage)
                ChangeScreen("MainWindow", "MainSelectPanel");
            else if(btn == btnVerify)
                ChangeScreen("MainWindow", "Correction");
            else if (btn == btnNormalScan)
                ChangeScreen("MainWindow", "NormalScan");
            else if (btn == btnSampleDetect)
                ChangeScreen("MainWindow", "DetectPanel");
            else if (btn == btnSetup)
                ChangeScreen("MainWindow", "SystemSetupPanel");
        }
    }
}
