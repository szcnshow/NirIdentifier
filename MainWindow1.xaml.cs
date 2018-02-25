using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
using System.Windows.Input;
//using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Shapes;
//using System.Windows.Controls.Primitives;
using NirIdentifier.Common;
using System.Threading;
using System.Windows.Threading;

namespace NirIdentifier
{
    /// <summary>
    /// MainWindow1.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        class ButtonInfo
        {
            public string text { get; set; }
            public BitmapImage selImage { get; set; }
            public BitmapImage unselImage { get; set; }
            public RadioButton button { get; set; }

            public ButtonInfo(RadioButton button, string text, string selImage, string unselImage)
            {
                this.button = button;
                this.text = text;
                this.selImage = Common.CommonMethod.LoadImage(selImage);
                this.unselImage = Common.CommonMethod.LoadImage(unselImage);
                button.DataContext = this;
            }
        }

        /// <summary>
        /// 依赖属性：仪器是否连接
        /// </summary>
        public static readonly DependencyProperty InstrumentConnectedProperty =
            DependencyProperty.Register("InstrumentConnected", typeof(bool), typeof(MainWindow), new UIPropertyMetadata(false));
        public bool InstrumentConnected
        {
            get { return (bool)GetValue(InstrumentConnectedProperty); }
            set { SetValue(InstrumentConnectedProperty, value); }
        }

        /// <summary>
        /// 依赖属性：是否扫描背景
        /// </summary>
        public static readonly DependencyProperty BackgroundScanedProperty =
            DependencyProperty.Register("BackgroundScaned", typeof(bool), typeof(MainWindow), new UIPropertyMetadata(false));
        public bool BackgroundScaned
        {
            get { return (bool)GetValue(BackgroundScanedProperty); }
            set { SetValue(BackgroundScanedProperty, value); }
        }

        List<ButtonInfo> mainButtons = new List<ButtonInfo>();
        UserControl curOperatePanel = null;

        public MainWindow()
        {
            SettingData.dataBase = new DBConnection();
            InitializeComponent();

            mainButtons.Add(new ButtonInfo(btnHomePage, "主界面", "HomePageSel.png", "HomePageUnsel.png"));
            //联机检测
            mainButtons.Add(new ButtonInfo(btnDetect, "联机检测", "DrugDetectSel.png", "DrugDetectUnsel.png"));
            //离线分析
            mainButtons.Add(new ButtonInfo(btnOffline, "离线分析", "OfflineSel.png", "OfflineUnsel.png"));
            //仪器校验
            mainButtons.Add(new ButtonInfo(btnCalibrate, "仪器校验", "CalibrateSel.png", "CalibrateUnsel.png"));
            //常规扫描
            mainButtons.Add(new ButtonInfo(btnNormalScan, "常规扫描", "NormalScanSel.png", "NormalScanUnsel.png"));
            mainButtons.Add(new ButtonInfo(btnSystemSetup, "系统设置", "SystemSetupSel.png", "SystemSetupUnsel.png"));

            btnHomePage.IsChecked = true;
            //SettingData.settingData.runing_para.isSimulator = true;
            if (SettingData.settingData.runing_para.isSimulator)
            {
                InstrumentConnected = true;
                BackgroundScaned = true;
            }
           
            //string path=@"E:\Project\NirIdentifier\bin\Release\Data\Calibration\PQ\Accuracy\PQ2015_04_03\1.spc";
            //Ai.Hong.CommonLibrary.SpecFileFormatDouble sample = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
            //sample.ReadFile(path);
            //float[] tr = new float[sample.YDatas.Count()];
            //for (int i = 0; i < sample.YDatas.Count(); i++)
            //{
            //    tr[i] = (float)sample.YDatas[i];
            //}
            //sample.Parameter.resolution = "8";
            //Ai.Hong.CommonLibrary.SPCFile.SaveFile(path, tr,sample.Parameter);

            //Ai.Hong.CommonLibrary.SpecFileFormatDouble sam = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
            //sam.ReadFile(path);
            // int j=sam.XDatas.Count();

            //string ss = Ai.Hong.CommonMethod.ReadIniFile(@"E:\Project\NirIdentifier\bin\Release\IniFile\Laser.vspec_nir_ini", "Collection", "resolution").ToString();
            //MessageBox.Show(ss);
        }

        NirIdentifier.Calibration.NirCalibration correction = null;
        NirIdentifier.NormalScan.NormalScan normalScan = null;
        NirIdentifier.Detect.NewSample newSample = null;
        NirIdentifier.SystemSetup.SetupPanel setupPanel = null;
        NirIdentifier.Offline.SampleResult sampleResult = null;

        //页面之间的切换
        public void ChangeScreen(string dstCtrlName)
        {
            if (curOperatePanel != null)
                curOperatePanel.Visibility = Visibility.Hidden;
            switch (dstCtrlName)
            {
                case "MainSelectPanel":
                    curOperatePanel = new MainSelect();
                    break;
                case "DetectPanel":
                    if (newSample == null)
                    {
                        newSample = new NirIdentifier.Detect.NewSample();
                        curOperatePanel = newSample;
                    }
                    curOperatePanel = newSample;
                    break;
                case "SystemSetupPanel":
                    if (setupPanel == null)
                    {
                        setupPanel = new NirIdentifier.SystemSetup.SetupPanel();
                        curOperatePanel = setupPanel;
                    }
                    curOperatePanel = setupPanel;
                    break;
                case "Correction":
                    if (correction == null)
                    {
                        correction = new NirIdentifier.Calibration.NirCalibration(SettingData.settingData);
                        curOperatePanel = correction;
                    }
                    curOperatePanel = correction;
                    break;
                case "OfflinePanel":
                    if (sampleResult == null)
                    {
                        sampleResult = new NirIdentifier.Offline.SampleResult();
                        curOperatePanel = sampleResult;
                    }
                    curOperatePanel = sampleResult;
                    break;
                case "NormalScanPanel":
                    if (normalScan == null)
                    {
                        normalScan = new NirIdentifier.NormalScan.NormalScan();
                        curOperatePanel = normalScan;
                    }
                    curOperatePanel = normalScan;
                    break;
            }
            GridScreenContainer.Children.Clear();
            GridScreenContainer.Children.Add(curOperatePanel);
            curOperatePanel.Width = GridScreenContainer.Width;
            curOperatePanel.Height = GridScreenContainer.Height;
            curOperatePanel.Visibility = Visibility.Visible;
        }
        private void btnHomePage_Checked(object sender, RoutedEventArgs e)
        {
            ChangeScreen("MainSelectPanel");
        }

        private void btnCalibrate_Checked(object sender, RoutedEventArgs e)
        {
            ChangeScreen("Correction");
        }

        private void btnDetect_Checked(object sender, RoutedEventArgs e)
        {
            ChangeScreen("DetectPanel");
        }

        private void btnOffine_Checked(object sender, RoutedEventArgs e)
        {
            ChangeScreen("OfflinePanel");
        }

        private void btnNormalScan_Checked(object sender, RoutedEventArgs e)
        {
            ChangeScreen("NormalScanPanel");
        }

        private void btnSystemSetup_Checked(object sender, RoutedEventArgs e)
        {
            ChangeScreen("SystemSetupPanel");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //如果从Setup面板中直接退出，要保存设置后的数据
            if(curOperatePanel is NirIdentifier.SystemSetup.SetupPanel)
                SettingData.settingData.Serialize(SettingData.settingData.filename);

            //终止状态检查线程
            if (instrumentTread != null && instrumentTread.IsAlive)
                instrumentTread.Abort();

            //断开仪器连接
            VspecInstrument.Disconnect();
        }

        public void MainSelectChange(string dstCtrlName)
        {
            switch (dstCtrlName)
            {
                case "DetectPanel":
                    btnDetect.IsChecked = true;
                    break;
                case "SystemSetupPanel":
                    btnSystemSetup.IsChecked = true;
                    break;
                case "Correction":
                    btnCalibrate.IsChecked = true;
                    break;
                case "OfflinePanel":
                    btnOffline.IsChecked = true;
                    break;
                case "NormalScanPanel":
                    btnNormalScan.IsChecked = true;
                    break;
            }
        }

        /// <summary>
        /// 仪器状态线程
        /// </summary>
        Thread instrumentTread = null;

        private delegate void UpdateInstrumentDeletage(bool connected);
        private void UpdateInstrument(bool connected)
        {
            if (SettingData.settingData.runing_para.isSimulator)
                connected = true;
            if (correction != null)
            {
                //更新状态
                correction.IsConnect(connected);
            }
            InstrumentConnected = connected;
            if (connected)
            {
                Common.CommonMethod.SetImageSource(imgState, "OK_16.png");
                txtStatus.Text = "联机";
            }
            else
            {
                Common.CommonMethod.SetImageSource(imgState, "Error_16.png");
                txtStatus.Text = "脱机";
            }
        }


        /// <summary>
        /// 通过USB端口检查仪器状态的Thread
        /// </summary>
        private void CheckInstrumentState()
        {
            bool oldUsbState =false;
            while (true)
            {
                bool curUsbState = false;

                System.Management.ManagementObjectSearcher wmiSearcher = new System.Management.ManagementObjectSearcher("Select * From WIN32_USBControllerDevice");
                foreach (System.Management.ManagementObject wmi_USB in wmiSearcher.Get())
                {
                    string strPath = wmi_USB.Path.RelativePath;
                    if (strPath.Contains(SettingData.settingData.runing_para.usbDeviceID))     //这是VSPEC仪器的Device ID(硬件管理器-详细信息-硬件ID
                    {
                        curUsbState = true;
                        break;
                    }
                }

                //没有检测到USB设备，表示仪器没有连接上
                if (curUsbState == false)
                {
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new UpdateInstrumentDeletage(UpdateInstrument), false);
                    oldUsbState = false;
                }

                else if (oldUsbState == false)   //如果上次已经连接了，不用再连接
                {
                    oldUsbState = Common.VspecInstrument.Connect();
                    //读取Json string
                    string paraString = VspecInstrument.GetParametersTable();
                    //读取SystemType
                    string[] systemTypeString = paraString.Split(',')[0].Split(':');
                    string systemType = systemTypeString[1].ToString();
                    switch(systemType)
                    {
                        case "1": Common.SettingData.systemType = Common.SystemTypeEnum.Fibre; break;
                        case "2": Common.SettingData.systemType = Common.SystemTypeEnum.IntegrateSphere; break;
                        default: return;

                    }
                    //Common.SettingData.systemType = systemType == "1" ? Common.SystemTypeEnum.Fibre : SystemTypeEnum.IntegrateSphere;
                   
                    //读取仪器序列号
                    string para = VspecInstrument.GetParametersTable();
                    string temp=para.Split(',')[1].Split(':')[1].Replace('\"', ' ');
                   // temp=temp.Substring(1,temp.Length-2);
                    SettingData.settingData.runing_para.serialNo =System.Text.RegularExpressions.Regex.Replace(temp, @"\s", "");// para == null ? "读取仪器序列号失败！" : temp;
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new DisplayModelDelegate(DisplayModel), SettingData.settingData.runing_para.serialNo);
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new UpdateInstrumentDeletage(UpdateInstrument), oldUsbState);
                }
                else
                {
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new UpdateInstrumentDeletage(UpdateInstrument), true);

                }
                Thread.Sleep(3*1000);      //5秒检查一次
            }
        }

        public delegate void DisplayModelDelegate(string model);
        public void  DisplayModel(string model)
        {
            txtLaser.Text = model;

        }
        //通过系统设备表查询是否连接了拉曼光谱仪器
        private bool CheckRamanInstrument()
        {

            return false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //启动仪器连接检查线程
            instrumentTread = new Thread(new ThreadStart(CheckInstrumentState));
            instrumentTread.IsBackground = true;
            instrumentTread.Start();
        }
    }
}
