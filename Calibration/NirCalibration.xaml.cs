using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Threading;
using System.Windows.Threading;
using NirIdentifier.Common;
using System.Windows.Media;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Xps.Packaging;
using System.Windows.Media.Imaging;

namespace NirIdentifier.Calibration
{
    /// <summary>
    /// NirCalibration.xaml 的交互逻辑
    /// </summary>
    public partial class NirCalibration : UserControl
    {
        public NirCalibration(Common.SettingFile st)
        {
            InitializeComponent();
            //graphic.HideFileList();
            laserPanel.InitPanel("激光波数校正", SettingData.settingData.calibratePara.laserPara.thresold);

            //PQ
            snrPanel.InitPanel("峰峰值噪声(PP) 100%线偏差", SettingData.settingData.calibratePara.snrPara.PPThresold, SettingData.settingData.calibratePara.snrPara.DevThresold,SettingData.settingData.calibratePara.snrPara.IpaThresold,SettingData.settingData.calibratePara.snrPara.EngThresold);
            accuracyPanel.InitPanel("波数精度测试", SettingData.settingData.calibratePara.accuracyPara.thresold, SettingData.settingData.calibratePara.accuracyPara.PolyThresold);
            yaxisPanel.InitPanel("吸收重复性测试", SettingData.settingData.calibratePara.yaxisRepPara.YaxisRepThresold);

            //OQ
            resolutionTest.InitPanel("分辨率测试", SettingData.settingData.calibratePara.resolutionTestPara.ResolutionThresold);
            lineNoisePanel.InitPanel("峰峰值噪声测试", SettingData.settingData.calibratePara.snrPara.PPThresold);
            energyDisPanel.InitPanel("能量分布测试", SettingData.settingData.calibratePara.energyDisPara.engerDisThresold);
            tranRepPanel.InitPanel("透射重复性测试", SettingData.settingData.calibratePara.transRepTest.transRepThresold);
            waveNumAccPanel.InitPanel("波数精度测试", SettingData.settingData.calibratePara.accuracyPara.thresold, SettingData.settingData.calibratePara.accuracyPara.PolyThresold);
            waveNumRepPanel.InitPanel("波数重复性测试", SettingData.settingData.calibratePara.waveNumRepTestPara.transRepThresold);
            // snrPanel.InitPanel("信噪比检测", SettingData.settingData.calibratePara.snrPara.thresold);

            laserPanel.DisplayCheckClicked += new RoutedEventHandler(laserPanel_DisplayCheckClicked);
            accuracyPanel.DisplayCheckClicked += new RoutedEventHandler(accuracyPanel_DisplayCheckClicked);
            snrPanel.DisplayCheckClicked += new RoutedEventHandler(snrPanel_DisplayCheckClicked);
            settingData = st;
            lineSlopeTest.dataGrid.ItemsSource = settingData.calibratePara.lineSlopeTestPara.data1;
        }

        void snrPanel_DisplayCheckClicked(object sender, RoutedEventArgs e)
        {

        }

        void accuracyPanel_DisplayCheckClicked(object sender, RoutedEventArgs e)
        {

        }

        void laserPanel_DisplayCheckClicked(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 从配置文件中获取扫描次数
        /// </summary>
        private int GetScanCountFromIni(string iniFile, bool isBackground)
        {
            string inistring;
            if (isBackground)
                inistring = Ai.Hong.CommonMethod.ReadIniFile(iniFile, "Collection", "backgroundScans");
            else
                inistring = Ai.Hong.CommonMethod.ReadIniFile(iniFile, "Collection", "sampleScans");

            int inivalue;
            if (int.TryParse(inistring, out inivalue) == false)     //缺省是32次
                return 32;
            else
                return inivalue;
        }

        /// <summary>
        /// 从配置文件中读取当采谱区域
        /// </summary>
        private string GetRegionFromIni(string iniFile)
        {
            string fx = Ai.Hong.CommonMethod.ReadIniFile(iniFile, "Process", "firstX");
            string lx = Ai.Hong.CommonMethod.ReadIniFile(iniFile, "Process", "lastX");

            return fx + "+" + lx;
        }

        /// <summary>
        /// 从配置文件中读取当前激光波数
        /// </summary>
        private double GetLaserValueFromIni(string iniFile)
        {
            string inistring = Ai.Hong.CommonMethod.ReadIniFile(iniFile, "Advanced FFT", "laserFrequency");
            double inivalue;
            if (double.TryParse(inistring, out inivalue) == false)     //缺省是32次
                return 0.636;
            else
                return inivalue;
        }

        /// <summary>
        /// 将新的激光波数写入配置文件
        /// </summary>
        private void WriteLaserValueToIni(string iniFile, double value)
        {
            Ai.Hong.CommonMethod.WriteIniFile(iniFile, "Advanced FFT", "laserFrequency", value.ToString());
        }


        #region 仪器校验相关的函数
        /// <summary>
        /// 依赖属性：仪器是否连接
        /// </summary>
        public void SetData(Common.SettingFile st)
        {
            settingData = st;
        }

        string startPath = Environment.CurrentDirectory;
        Common.SettingFile settingData = null;
        bool userCancel = false;
        const string debugBgFile = "debugData/LWN_HW003 GS226824_rsb.spc";
        const string debugSampleFile = "debugData/";
        bool isConnect;
        object co = new object();
        public void IsConnect(bool con)
        {
            Monitor.Enter(co);
            isConnect = con;
            Monitor.Exit(co);
        }

        /// <summary>
        /// 图像文件内部路径
        /// </summary>
        public const string imagePath = @"pack://application:,,/Images/";

        private string logfile = null;

        /// <summary>
        /// 仪器校验线程
        /// </summary>
        Thread ITTread = null;

        /// <summary>
        /// 激光波数校正线程
        /// </summary>
        Thread LWCThread = null;

        /// <summary>
        /// PQ测试线程
        /// </summary>
        Thread PQThread = null;

        /// <summary>
        /// OQ测试线程
        /// </summary>
        Thread OQThread = null;

        /// <summary>
        /// 仪器测试错误信息
        /// </summary>
        string ITError = null;

        ///// <summary>
        ///// 波数准确度验证结果
        ///// </summary>
        //private double accuracyResult = 0;

        /// <summary>
        /// 信噪比(RMS)结果
        /// </summary>
        private double snrRMSResult = 0;

        /// <summary>
        /// 信噪比(PP)结果
        /// </summary>
        private double snrPPResult = 0;

        /// <summary>
        /// 最大偏差
        /// </summary>
        private double devMax = 0;

        /// <summary>
        /// 透射重复性测试6500  10000 两点的Y值之差
        /// </summary>
        private double devTransRepTest = 0;

        private double resultEnergyDis = 0;

        /// <summary>
        /// 波数重复性测试结果
        /// </summary>
        private double devWaveNumRepTest = 0;

        /// <summary>
        /// 波数精度光谱
        /// </summary>
        string[] accuracySpectrumFile = null;

        /// <summary>
        /// 信噪比光谱
        /// </summary>
        string[] snrSpectrumFile = null;

        /// <summary>
        ///
        /// </summary>
        string CurrentTimePath = "";

        /// <summary>
        /// 初始化控件的状态
        /// </summary>
        private void InitControlStatus()
        {
            Ai.Hong.CommonMethod.SetImageSource(laserPanel.imageResult, imagePath, "unknown_16.png");
            Ai.Hong.CommonMethod.SetImageSource(accuracyPanel.imageResult, imagePath, "unknown_16.png");
            Ai.Hong.CommonMethod.SetImageSource(snrPanel.imageResult, imagePath, "unknown_16.png");

            laserPanel.scanProgress.Visibility = Visibility.Hidden;
            laserPanel.scanProgress.IsIndeterminate = true;

            accuracyPanel.scanProgress.Visibility = Visibility.Hidden;
            accuracyPanel.scanProgress.IsIndeterminate = true;

            snrPanel.scanProgress.Visibility = Visibility.Hidden;
            snrPanel.scanProgress.IsIndeterminate = false;
            snrPanel.scanProgress.Maximum = 1;
            snrPanel.scanProgress.Value = 0;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //if (btnStart.Text == "开始校验")
            //{
            InitControlStatus();

            userCancel = false;

            SetStartButton(true);

            //启动仪器连接检查线程
            ITTread = new Thread(new ThreadStart(ITMainThread));
            ITTread.Name = "ITTread";
            ITTread.IsBackground = false;
            ITTread.SetApartmentState(ApartmentState.STA);
            //    ITTread.Start();
            //}
            //else
            //{
            //    userCancel = true;
            //}
        }


        private delegate void SetITStatusDeletage(object eachCalibratePanel, bool? IsPass, string imageFile, int processMaxValue, int processCurValue);
        /// <summary>
        /// 检测线程中设置检测项目的状态
        /// </summary>
        /// <param name="eachCalibratePanel">Panel参数</param>
        /// <param name="IsPass">true：绿色，false：红色，null：黄色</param>
        /// <param name="imageFile">Panel左上角状态图片路径</param>
        /// <param name="processMaxValue">进度条最大值</param>
        /// <param name="processCurValue">进度条当前值</param>
        private void SetITStatus(object eachCalibratePanel1, bool? IsPass, string imageFile, int processMaxValue, int processValue)
        {
            EachCalibratePanel ob = eachCalibratePanel1 as EachCalibratePanel;
            PPDeviation ob1 = eachCalibratePanel1 as PPDeviation;

            LineSlopeTest ob2 = eachCalibratePanel1 as LineSlopeTest;
            Accuracy ob3 = eachCalibratePanel1 as Accuracy;

            StackPanel borderCtrl = null;
            ProgressBar processCtrl = null;
            Image imgCtrl = null;
            if (ob != null)
            {
                borderCtrl = ob.stack;
                processCtrl = ob.scanProgress;
                imgCtrl = ob.imageResult;
            }
            else if (ob1 != null)
            {
                borderCtrl = ob1.ppStack;
                processCtrl = ob1.scanProgress;
                imgCtrl = ob1.imageResult;
            }
            else if (ob2 != null)
            {
                // borderCtrl = ob2.stack;
                processCtrl = ob2.scanProgress;
                imgCtrl = ob2.imageResult;
                lineSlopeTest.dataGrid.ItemsSource = null;
                lineSlopeTest.dataGrid.Items.Clear();
                lineSlopeTest.dataGrid.ItemsSource = settingData.calibratePara.lineSlopeTestPara.data1;
            }
            else if (ob3 != null)
            {
                processCtrl = ob3.scanProgress;
                imgCtrl = ob3.imageResult;
            }
            else
            {
                return;
            }

            //borderCtrl.Visibility = borderVisible ? Visibility.Visible : Visibility.Collapsed;
            if (IsPass.HasValue && ob2 == null)
            {
                if (borderCtrl != null)
                    borderCtrl.Background = (bool)IsPass ? Brushes.Green : Brushes.Red;// System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
            }
            if (processMaxValue < 0)
            {
                processCtrl.Visibility = Visibility.Hidden;
            }
            else
            {
                processCtrl.Visibility = Visibility.Visible;
                if (processValue == 0)
                {
                    processCtrl.IsIndeterminate = true;
                }
                if (processCtrl.IsIndeterminate == false)
                {
                    processCtrl.Maximum = processMaxValue;
                    processCtrl.Value = processValue;
                }
            }

            if (imageFile != null)
                Ai.Hong.CommonMethod.SetImageSource(imgCtrl, imagePath, imageFile);
        }

        private delegate void SetStartButtonDelegate(bool pressed);
        private void SetStartButton(bool pressed)
        {
            //  btnStart.Text = pressed?"停止测试": "开始测试";
            System.Windows.Media.Imaging.BitmapImage bi = new System.Windows.Media.Imaging.BitmapImage();
            bi.BeginInit();
            bi.UriSource = new System.Uri(imagePath + (pressed ? "Error.png" : "start.png"));
            bi.EndInit();
            snrPanel.scanProgress.Visibility = Visibility.Collapsed;
            accuracyPanel.scanProgress.Visibility = Visibility.Collapsed;
            yaxisPanel.scanProgress.Visibility = Visibility.Collapsed;
            //  btnStart.ImageFile = bi;
        }

        /// <summary>
        /// 仪器校验主线程
        /// </summary>
        public void ITMainThread()
        {
            try
            {
                logfile = Path.Combine(startPath, "logfile.txt");
                ITError = null;

                if (LWNCorrect() == false)
                    throw new Exception("激光波数校正:" + ITError);
                if (userCancel)
                    throw new Exception("用户取消");

                //能量测试
                if (LineNoiseAndDevTest() == false)
                    throw new Exception("信噪比测试:" + ITError);
                if (userCancel)
                    throw new Exception("用户取消");

                //波数精度测试
                if (AccuracyCal() == false)
                    throw new Exception("波数准确度测试:" + ITError);
                if (userCancel)
                    throw new Exception("用户取消");



                string xpsfile = Path.Combine(startPath, "Report");
                if (!Directory.Exists(xpsfile))
                    Directory.CreateDirectory(xpsfile);

                xpsfile = Path.Combine(xpsfile, "Report" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".xps");
                if (CreateCalibrationReport(xpsfile) == false)
                    throw new Exception("创建测试报告错误");

                System.Diagnostics.Process.Start(xpsfile);       //打开自检报告
            }
            catch (Exception ex)
            {
                if (!userCancel)
                    Ai.Hong.CommonMethod.ErrorMsgBox(ex.Message);
            }

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetStartButtonDelegate(SetStartButton), false);
        }

        private delegate void DelegateShowSpec(List<string> path);


        //class cout
        //{
        //    public double x;
        //    public double cha;
        //    public double cha1;
        //    public double cha2;
        //    public double sq;

        //}
        /// <summary>
        /// 激光波数校正
        /// </summary>
        /// <returns>True:正常完成，False:用户取消或者系统错误</returns>
        public bool LWNCorrect()
        {

            string lwnFile = null;

            Common.SettingFile.Calibrate_Parameter.Peak_Parameter para = settingData.calibratePara.laserPara;
            string iniFileName = Path.Combine(startPath, para.iniFile);
            //读取Json string
            string paraString = VspecInstrument.GetParametersTable();
            //读取SystemType
            //string[] systemTypeString = paraString.Split(',')[0].Split(':');
            //string systemType = systemTypeString[1].ToString();
            //Common.SettingData.systemType = systemType == "1" ? Common.SystemTypeEnum.Fibre : SystemTypeEnum.IntegrateSphere;
            CurrentTimePath = DateTime.Now.ToString("HH_mm_ss");
            if (settingData.runing_para.isDebug)
            {
                lwnFile = Path.Combine(startPath, debugBgFile);
                Thread.Sleep(1000);
            }
            else
            {
                // lwnFile = @"E:\Project\NirIdentifier\bin\Release\Data\LWN\IT11032015\lw.spc";
                // 从仪器中采集背景光谱
                string dataPath = Path.Combine(startPath, "Data", "LWN_" + SettingData.settingData.runing_para.serialNo, "IT" + DateTime.Now.ToString("yyyy_MM_dd"));
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);
                lwnFile = Path.Combine(dataPath, "LWN" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");

                Ai.Hong.CommonMethod.AddToLogFile(logfile, "start scan laser spectrum:" + lwnFile + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
                int scanCount = GetScanCountFromIni(iniFileName, true);
                //复原转轮
                if (!Common.VspecInstrument.MoveWheel(0))
                {
                    throw new Exception("复原转轮失败！");
                }
                //开始激光波数校正
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), laserPanel, null, null, int.MaxValue, 0);
                lwnFile = Common.VspecInstrument.ScanBackground(iniFileName, scanCount, lwnFile,Common.VspecInstrument.IsMoveFlagBack());
                if (lwnFile == null)
                {
                    Ai.Hong.CommonMethod.ErrorMsgBox(Common.VspecInstrument.GetError());
                    return false;
                }
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "start scan laser spectrum:" + lwnFile + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
            }
            para.LeaserSpectrum = lwnFile;
            //读取背景光谱
            Ai.Hong.CommonLibrary.SpecFileFormatDouble specData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
            if (specData.ReadFile(lwnFile) == false)
            {
                Ai.Hong.CommonMethod.ErrorMsgBox(specData.ErrorString);
                return false;
            }

            double newYValue;
            double lwnPeak = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, para.targetPeak, 4, false, out newYValue);
            double peak1 = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, para.verfyPeak1, 4, false, out newYValue);
            double peak2 = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, para.verfyPeak2, 4, false, out newYValue);

            Ai.Hong.CommonMethod.AddToLogFile(logfile, "target peak=" + lwnPeak.ToString("F4") + " verify peak1=" + peak1.ToString("F4") + " verify peak2=" + peak2.ToString("F4"));

            //to make sure the correct peak is selected, the program should find two peaks in the range of 7230-7245cm-1. 
            //One is at position about 7232.29cm-1 and the other is at 7242.77cm-1.
            //peak1和peak2都在区域内，peak1小于peak2，并且peak1和peak2的差值要在一定范围内
            if (peak1 < para.firstX || peak1 > para.lastX || peak2 < para.firstX || peak2 > para.lastX || peak2 < peak1)
            {
                ITError = para.firstX.ToString() + " - " + para.lastX.ToString() + " 范围内找不到标定峰位,请手动校正";
                throw new Exception(ITError);
                //return false;
            }
            double TestData = Math.Abs(lwnPeak - para.targetPeak);

            //更新界面测量值
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetEachValueDelegate(SetEachValue), laserPanel, Convert.ToDouble(TestData.ToString("F2")), false);


            //超出阈值，需要重置激光波数
            if (TestData > para.thresold)
            {
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "reset laserFrequency");

                double curvalue = 0;// GetLaserValueFromIni(iniFileName);
                
                if (paraString != null)
                {
                    string[] temp = paraString.Split(',');
                    string[] leasertemp = temp[3].Split(':');
                    leasertemp[1] = leasertemp[1].Replace('\"', ' ');
                    curvalue = Convert.ToDouble(leasertemp[1]) / 1000;

                    //读取仪器序列号,已在Window_Load中完成读取
                    //settingData.runing_para.serialNo = temp[1].Split(':')[1].Replace('\"', ' ');


                }
                else
                {
                    MessageBox.Show("获取仪器内部激光波数失败！,请检查仪器连接");
                    return false;
                }

                double setvalue = curvalue * lwnPeak / para.targetPeak;

                Ai.Hong.CommonMethod.AddToLogFile(logfile, "old laserFrequency =" + curvalue.ToString("F4") + " new laserFrequency =" + setvalue.ToString("F4"));

                //保留4位小数就可以了
                setvalue = double.Parse(setvalue.ToString());

                //激光波数配置文件 Leaser.vspec_nir_ini
                WriteLaserValueToIni(iniFileName, setvalue);

                //波数准确度配置文件  Accuracy.vspec_nir_ini
                iniFileName = Path.Combine(startPath, settingData.calibratePara.accuracyPara.iniFile);
                WriteLaserValueToIni(iniFileName, setvalue);

                //100%线噪比及偏差测试配置文件 ITSN.vspec_nir_ini
                iniFileName = Path.Combine(startPath, settingData.calibratePara.snrPara.iniFile);
                WriteLaserValueToIni(iniFileName, setvalue);

                //分辨率测试配置文件   Resolution.vspec_nir_ini
                iniFileName = Path.Combine(startPath, settingData.calibratePara.resolutionTestPara.iniFile);
                WriteLaserValueToIni(iniFileName, setvalue);

                //吸收重复性测试配置文件  ITGF.vspec_nir_ini
                iniFileName = Path.Combine(startPath, settingData.calibratePara.yaxisRepPara.iniFile);
                WriteLaserValueToIni(iniFileName, setvalue);

                //efitr测试配置文件
                string documentpath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
                iniFileName = Path.Combine(documentpath, "eftir", "vspec_nir_instrument.ini");
                WriteLaserValueToIni(iniFileName, setvalue);

                string strLWN=(setvalue*1000).ToString();
                double writeLWN = Convert.ToDouble(strLWN.Substring(0, strLWN.Length < 17 ? strLWN.Length : 17));
                if (writeLWN < 645 && writeLWN > 635)
                {
                    //写入激光波数到仪器
                    if (!VspecInstrument.SetLaserWavelength((writeLWN).ToString()))
                    {
                        MessageBox.Show("激光波数已存入ini，但写入仪器失败！");
                    }
                    else
                    {
                        MessageBox.Show("激光波数 " + (writeLWN).ToString() + " 已成功写入仪器!");
                        //if (VspecInstrument.Disconnect() && VspecInstrument.Connect())
                        //{
                        //    MessageBox.Show("重新连接仪器成功!");
                        //}
                        //else
                        //{
                        //    MessageBox.Show("重新连接仪器失败！" + "\r\n" + VspecInstrument.GetError());
                        //}
                    }
                }
                else
                {
                    MessageBox.Show("激光波数错误\r\n" + (writeLWN).ToString(), "提示");
                }
                MessageBox.Show("目标水峰：" + para.targetPeak.ToString() + "\r\n测量水峰：" + lwnPeak.ToString("F2") + "\r\n校正激光波数到：" + setvalue.ToString("F6") + ",请尝试再次校正！", "结果");
            }
            else
            {
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "need not reset laserFrequency");
                MessageBox.Show("目标水峰：" + para.targetPeak.ToString() + "\r\n测量水峰：" + lwnPeak.ToString("F2") + "\r\n\"无需校正！", "结果");
            }


            //激光波数校正结果
            // Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), laserPanel, true, "OK_16.png", -1, 1);
            return true;
        }

        /// <summary>
        /// 波数准确度检测
        /// </summary>
        /// <returns></returns>
        /// <returns>True:正常完成，False:用户取消或者系统错误</returns>
        public bool AccuracyCal()
        {
            string spectrumFile = null;

            Common.SettingFile.Calibrate_Parameter.Peak_Parameter para = settingData.calibratePara.accuracyPara;
            para.SpectrumPath = "";
            para.PolySpectrumPath = "";
            para.TestResult = 0;
            para.PolyTestResult = 0;
            if(CurrentTimePath=="")
                CurrentTimePath = DateTime.Now.ToString("HH_mm_ss");
            string dataPath = Path.Combine(startPath, "Data", "IT_" + SettingData.settingData.runing_para.serialNo + "\\PQ\\" + DateTime.Now.ToString("yyyy_MM_dd") + "\\" + CurrentTimePath + "\\Accuracy");
            string iniFileName = Path.Combine(startPath, para.iniFile);
            if (settingData.runing_para.isDebug)
            {
                spectrumFile = Path.Combine(startPath, debugBgFile);
                Thread.Sleep(1000);
            }
            else
            {
                //从仪器中采集背景光谱   水峰测试
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);
                spectrumFile = Path.Combine(dataPath, "Accuracy_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");

                Ai.Hong.CommonMethod.AddToLogFile(logfile, "start scan accuracy spectrum:" + spectrumFile + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
                int scanCount = GetScanCountFromIni(iniFileName, true);
                //复原转轮
                if (!Common.VspecInstrument.MoveWheel(0))
                {
                    throw new Exception("复原转轮失败！");
                }
                //开始波数准确度检测
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), accuracyPanel, null, null, 1, 0);
                
                spectrumFile = Common.VspecInstrument.ScanBackground(iniFileName, scanCount, spectrumFile,Common.VspecInstrument.IsMoveFlagBack());
                if (spectrumFile == null)
                {
                    ITError = "扫描光谱错误:" + Common.VspecInstrument.GetError();
                    return false;
                }
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "stop scan accuracy spectrum:" + spectrumFile + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
            }

            para.SpectrumPath = spectrumFile;

            //读取背景光谱
            Ai.Hong.CommonLibrary.SpecFileFormatDouble specData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
            if (specData.ReadFile(spectrumFile) == false)
            {
                ITError = "读取光谱错误:" + spectrumFile + " " + specData.ErrorString;
                return false;
            }

            double newYValue;
            double targetPeak = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, para.targetPeak, 4, false, out newYValue);
            double peak1 = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, para.verfyPeak1, 4, false, out newYValue);
            double peak2 = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, para.verfyPeak2, 4, false, out newYValue);
            Ai.Hong.CommonMethod.AddToLogFile(logfile, "target peak=" + targetPeak.ToString("F4") + " verify peak1=" + peak1.ToString("F4") + " verify peak2=" + peak2.ToString("F4"));

            para.ActualBand = targetPeak;
            //to make sure the correct peak is selected, the program should find two peaks in the range of 7230-7245cm-1. 
            //One is at position about 7232.29cm-1 and the other is at 7242.77cm-1.
            //peak1和peak2都在区域内，peak1小于peak2，并且peak1和peak2的差值要在一定范围内
            if (peak1 < para.firstX || peak1 > para.lastX || peak2 < para.firstX || peak2 > para.lastX || peak2 < peak1)
            {
                ITError = para.firstX.ToString() + " - " + para.lastX.ToString() + " 范围内找不到标定峰位";
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), 1, false, "Error_16.png", -1, 1);
                throw new Exception(ITError);
            }

            para.TestResult = Math.Abs(targetPeak - para.targetPeak);

            //更新界面测量值
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetAccuracyDelegate(SetAccuracyValue), accuracyPanel, Convert.ToDouble(para.TestResult.ToString("F2")), -1);

            //开始聚苯乙烯测试
            string  PolySamplePath = "";
            if (settingData.runing_para.isDebug)
            {
                spectrumFile = Path.Combine(startPath, debugBgFile);
                Thread.Sleep(1000);
            }
            else
            {
                //测试背景
              //  string dataPath = Path.Combine(startPath, "Data", "IT\\PQ\\Accuracy", "PQ" + DateTime.Now.ToString("yyyy_MM_dd"));
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);
                //spectrumFile = Path.Combine(dataPath, "Accuracy_Polystyrene_Background" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");
                int scanCount = GetScanCountFromIni(iniFileName, true);
                //PolyBackPath = Common.VspecInstrument.ScanBackground(iniFileName, scanCount, spectrumFile);
                //if (spectrumFile == null)
                //{
                //    ITError = "扫描光谱错误:" + Common.VspecInstrument.GetError();
                //    return false;
                //}
                //  MessageBox.Show("PQ_波数精度测试_开始转往聚苯乙烯位置！");
                //移动转动到聚苯乙烯位置
                if (!Common.VspecInstrument.MoveWheel(2))
                {
                    MessageBox.Show("PQ_波数精度测试_转往聚苯乙烯位置失败！！");
                    throw new Exception("PQ波数精度-转轮转到聚苯乙烯失败！");
                }
                //  MessageBox.Show("PQ_波数精度测试_转往聚苯乙烯完毕！");
                spectrumFile = Path.Combine(dataPath, "Accuracy_Polystyrene_Sample" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");
                scanCount = GetScanCountFromIni(iniFileName, true);
               
                PolySamplePath = Common.VspecInstrument.ScanSample(iniFileName, scanCount, spectrumFile, Common.VspecInstrument.IsMoveFlagBack());
                if (spectrumFile == null)
                {
                    ITError = "扫描光谱错误:" + Common.VspecInstrument.GetError();
                    return false;
                }
                //复原转轮
                if (!Common.VspecInstrument.MoveWheel(0))
                {
                    throw new Exception("复原转轮失败！");
                }

            }
            //读取背景光谱
            
            //Ai.Hong.CommonLibrary.SpecFileFormatDouble PolyBack = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
            Ai.Hong.CommonLibrary.SpecFileFormatDouble PolySample = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();

            //if (PolyBack.ReadFile(spectrumFile) == false)
            //{
            //    ITError = "读取光谱错误:" + spectrumFile + " " + specData.ErrorString;
            //    return false;
            //}
            if (PolySample.ReadFile(PolySamplePath) == false)
            {
                ITError = "读取光谱错误:" + spectrumFile + " " + specData.ErrorString;
                return false;
            }
            //计算样品1和样品2比值,得到100%线
            for (int index = 0; index < specData.Parameter.dataCount; index++)
            {
                if (specData.YDatas[index] == 0)
                    specData.YDatas[index] = 1;//00;
                else
                    specData.YDatas[index] = Math.Abs(PolySample.YDatas[index] / specData.YDatas[index]);// *100;
            }
            //得到吸收光谱
            for (int i = 0; i < specData.YDatas.Count(); i++)
            {
                specData.YDatas[i] = Math.Log10(1 / specData.YDatas[i]);
            }
            //保存吸收谱
            string tr = Path.Combine(dataPath, "AccuracyPQ_Polystyrene_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + "_abs.spc");
            para.PolySpectrumPath = tr;
            float[] trData = new float[specData.YDatas.Length];
            for (int index = 0; index < trData.Length; index++)
                trData[index] = (float)specData.YDatas[index];
            Ai.Hong.CommonLibrary.SPCFile.SaveFile(para.PolySpectrumPath, trData, specData.Parameter);



            double newPolyYValue;
            double PolyTargetPeak = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, para.PolyTargetX, 4, true, out newPolyYValue);
            //double peak1 = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, para.verfyPeak1, 4, false, out newYValue);
            //double peak2 = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, para.verfyPeak2, 4, false, out newYValue);
            //Ai.Hong.CommonMethod.AddToLogFile(logfile, "target peak=" + targetPeak.ToString("F4") + " verify peak1=" + peak1.ToString("F4") + " verify peak2=" + peak2.ToString("F4"));

            //计算温度偏差
            string paraString = VspecInstrument.ReadSensors();
            double devTemperature = 0;
            if (paraString != null)
            {
                string[] tempp = paraString.Split('}');
                string[] leasertemp = tempp[1].Split(':');
                //  leasertemp[1]=leasertemp[2].Replace('\"',' ');
                //读出仪器当前温度

                if (VspecInstrument.IsMoveFlagBack())
                {
                    PolyTargetPeak = (4571 * PolyTargetPeak) / (4571.575 - 0.0205 * Convert.ToDouble(leasertemp[2]));
                }
                else
                {
                    devTemperature = Convert.ToDouble(leasertemp[2]) * 0.0107 - 0.7;
                    PolyTargetPeak += devTemperature;
                }
            }
            else
            {
                MessageBox.Show("读取仪器温度失败！");
                return false;
            }
            //加上温度偏差校正
            para.PolyActualBand = PolyTargetPeak;


            //超出阈值，需要重置激光波数
            para.PolyTestResult = Math.Abs(PolyTargetPeak - para.PolyTargetX);

            //更新界面测量值
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetAccuracyDelegate(SetAccuracyValue), accuracyPanel, Convert.ToDouble(para.TestResult.ToString("F2")), Convert.ToDouble(para.PolyTestResult.ToString("F2")));


            ////波数准确度验证结果
            //if (accuracyResult < settingData.calibratePara.accuracyPara.thresold)
            //{
            //    Ai.Hong.CommonMethod.AddToLogFile(logfile, "PQ accuracy calibration ok");
            //  //  Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), accuracyPanel, true, "OK_16.png", -1, 1);
            //}
            //else
            //{
            //    Ai.Hong.CommonMethod.AddToLogFile(logfile, "PQ accuracy calibration fail");
            //  //  Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), accuracyPanel, false, "Error_16.png", -1, 1);
            //}
            return true;
        }

        /// <summary>
        /// 信噪比检测
        /// </summary>
        /// <returns></returns>
        /// <returns>True:正常完成，False:用户取消或者系统错误</returns>
        public bool LineNoiseTestOQ()
        {


            Common.SettingFile.Calibrate_Parameter.OQTest.LineNoiseTest para = settingData.calibratePara.lineNoiseTest;
            para.SpectrumFile.Clear();
            para.TestResult = 0;
            string iniFileName = Path.Combine(startPath, para.iniFile);
            if(CurrentTimePath=="")
                CurrentTimePath = DateTime.Now.ToString("HH_mm_ss");
            string dataPath = Path.Combine(startPath, "Data", "IT_" + SettingData.settingData.runing_para.serialNo + "\\OQ\\" + DateTime.Now.ToString("yyyy_MM_dd") + "\\" + CurrentTimePath + "\\LineNoiseTestOQ");
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            snrRMSResult = snrPPResult = 0;
            snrSpectrumFile = new string[settingData.calibratePara.snrPara.repeatCount];

            //复原转轮
            if (!Common.VspecInstrument.MoveWheel(0))
            {
                throw new Exception("复原转轮失败！");
            }
            //开始信噪比检测
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), lineNoisePanel, null, null, 1, 1);
            for (int i = 0; i < para.repeatCount; i++)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), lineNoisePanel, null, null, settingData.calibratePara.snrPara.repeatCount, i);

                string samplefile1 = null, samplefile2 = null;
                if (settingData.runing_para.isDebug)
                {
                    samplefile1 = Path.Combine(startPath, debugSampleFile + (i % 10).ToString() + ".spc");     //只有10个文件
                    samplefile2 = Path.Combine(startPath, debugSampleFile + ((i + 1) % 10).ToString() + ".spc");     //只有10个文件
                    Thread.Sleep(1000);
                }
                else
                {
                    //从仪器中采集样品光谱1
                    int scanCount = GetScanCountFromIni(iniFileName, true);
                    if (samplefile1 == null || samplefile2 == null)    //第一次需要采集样品光谱1，以后直接使用样品光谱2作为光谱1
                    {
                        samplefile1 = Path.Combine(dataPath, "LineNoiseTestOQ_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");
                        Ai.Hong.CommonMethod.AddToLogFile(logfile, "scan LineNoiseTestOQ spectrum:" + samplefile1 + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
                       
                        samplefile1 = Common.VspecInstrument.ScanBackground(iniFileName, scanCount, samplefile1, Common.VspecInstrument.IsMoveFlagBack());//, "_sbm");
                    }
                    else
                        samplefile1 = samplefile2;

                    //从仪器中采集样品光谱2
                    if (samplefile1 != null)
                    {
                        samplefile2 = Path.Combine(dataPath, "LineNoiseTestOQ_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");
                        Ai.Hong.CommonMethod.AddToLogFile(logfile, "scan LineNoiseTestOQ spectrum:" + samplefile2 + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
                        
                        samplefile2 = Common.VspecInstrument.ScanSample(iniFileName, scanCount, samplefile2, Common.VspecInstrument.IsMoveFlagBack());//, "_sbm");
                    }
                    if (samplefile1 == null || samplefile2 == null)
                    {
                        ITError = "扫描光谱错误:" + Common.VspecInstrument.GetError();
                        return false;
                    }
                }

                //读取样品1和样品2数据
                Ai.Hong.CommonLibrary.SpecFileFormatDouble sampleData1 = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
                Ai.Hong.CommonLibrary.SpecFileFormatDouble sampleData2 = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();

                if (sampleData1.ReadFile(samplefile1) == false)
                {
                    ITError = "读取光谱错误:" + samplefile1 + " " + sampleData1.ErrorString;
                    return false;
                }
                if (sampleData2.ReadFile(samplefile2) == false)
                {
                    ITError = "读取光谱错误:" + samplefile2 + " " + sampleData2.ErrorString;
                    return false;
                }

                //计算样品1和样品2比值,得到100%线
                for (int index = 0; index < sampleData1.Parameter.dataCount; index++)
                {
                    if (sampleData1.YDatas[index] == 0)
                        sampleData1.YDatas[index] = 100;
                    else
                        sampleData1.YDatas[index] = Math.Abs(sampleData2.YDatas[index] / sampleData1.YDatas[index]) * 100;
                }

                //保存透射光谱
                if (settingData.runing_para.isDebug)
                    para.SpectrumFile.Add(samplefile1.Replace(".spc", "_tr.spc"));
                else
                {
                    // para.SpectrumFile.Add(samplefile1.Replace("_sbm.spc", "_tr.spc"));
                    para.SpectrumFile.Add(Path.Combine(dataPath, "LineNoiseTestOQ_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + "_tr.spc"));
                }
                float[] trData = new float[sampleData1.YDatas.Length];
                for (int index = 0; index < trData.Length; index++)
                    trData[index] = (float)sampleData1.YDatas[index];

                Ai.Hong.CommonLibrary.SPCFile.SaveFile(para.SpectrumFile[i], trData, sampleData1.Parameter);

                //计算信噪比
                //double snr = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.CalculateSpectrumRMS(sampleData1.XDatas, sampleData1.YDatas, para.firstX, para.lastX);
                //snrRMSResult += 100/snr;
                //Ai.Hong.CommonMethod.AddToLogFile(logfile, "snrRMS" + i + "=" + snrRMSResult);



                //pp峰值
                double[] xdatas, ydatas;
                Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.GetRangeData(sampleData1.XDatas, sampleData1.YDatas, para.firstX, para.lastX, out xdatas, out ydatas);
                double maxy = double.MinValue;
                double miny = double.MaxValue;
                double maxyposx = 0, minyposx = 0;
                for (int index = 0; index < ydatas.Length; index++)
                {
                    if (ydatas[index] > maxy)
                    {
                        maxy = ydatas[index];
                        maxyposx = xdatas[index];
                    }
                    if (ydatas[index] < miny)
                    {
                        miny = ydatas[index];
                        minyposx = xdatas[index];
                    }
                }
                double newmaxy, newminy;
                Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(xdatas, ydatas, maxyposx, 4, true, out newmaxy);
                Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(xdatas, ydatas, minyposx, 4, false, out newminy);
                snrPPResult += Math.Abs(newmaxy - newminy);

                //偏差计算
                //devMax += CalculateDev(xdatas, ydatas, para.DevfirstX, para.DevlastX);

                Ai.Hong.CommonMethod.AddToLogFile(logfile, "LineNoiseTestOQ" + i + "=" + snrPPResult + ", maxy=" + newmaxy + ", miny=" + newminy);
            }

            // snrRMSResult = snrRMSResult / settingData.calibratePara.snrPara.repeatCount;
            snrPPResult = snrPPResult / settingData.calibratePara.snrPara.repeatCount;
            para.TestResult = snrPPResult;

            //更新界面测量值
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetEachValueDelegate(SetEachValue), lineNoisePanel, Convert.ToDouble(snrPPResult.ToString("F2")), false);
            //信噪比验证结果
            //波数准确度验证结果
            if (snrPPResult > settingData.calibratePara.snrPara.PPThresold)//snrRMSResult > settingData.calibratePara.snrPara.RMSThresold
            {
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "100% Line Noise Test ok");
                //   Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), lineNoisePanel, true, "OK_16.png", -1, 1);
            }
            else
            {
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "100% Line Noise Test Fail");
                //   Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), lineNoisePanel, false, "Error_16.png", -1, 1);
            }
            return true;
        }

        public double CalculateDev(double[] xData, double[] yData, double freqStart, double freqEnd)
        {
            int num4;
            alglib.spline1dinterpolant splinedinterpolant;
            alglib.spline1dfitreport splinedfitreport;
            int beginvalue = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.FindNearestPosition(xData, 0, xData.Length - 1, freqStart);
            int endvalue = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.FindNearestPosition(xData, 0, xData.Length - 1, freqEnd);
            if ((beginvalue == -1) || (endvalue == -1))
            {
                return 0.0;
            }
            if (beginvalue > endvalue)
            {
                int num = endvalue;
                endvalue = beginvalue;
                beginvalue = num;
            }
            double[] numArray = new double[endvalue - beginvalue];
            double[] numArray2 = new double[endvalue - beginvalue];
            for (int i = beginvalue; i < endvalue; i++)
            {
                numArray[i - beginvalue] = xData[i];
                numArray2[i - beginvalue] = yData[i];
            }
            double num5 = 5.0;
            int num6 = ((endvalue - beginvalue) < 100) ? (endvalue - beginvalue) : 100;
            alglib.spline1dfitpenalized(numArray, numArray2, num6, num5, out num4, out splinedinterpolant, out splinedfitreport);
            double num7 = 0.0;

            for (int j = 0; j < numArray2.Length; j++)
            {
                double num9 = alglib.spline1dcalc(splinedinterpolant, numArray[j]);
                double temp = Math.Abs(numArray2[j] - num9);//求偏差
                if (temp > num7)//求最大偏差
                    num7 = temp;
            }
            return num7;
        }

        #endregion  仪器校验相关的函数


        #region 生成报告
        const double DPCM = 96 / 2.54;      //1cm中的像素点数量
        private PageContent CreatePageContent(Border rootBorder)
        {
            PageContent pageContent = new PageContent();
            FixedPage page = new FixedPage();
            page.Width = 21 * DPCM;        //A4 Paper: 21cm x 29.7cm
            page.Height = 29.7 * DPCM;

            //page.ContentBox = new Rect(2 * DPCM, 2 * DPCM, (21 - 4) * DPCM, (29.7 - 4) * DPCM);
            page.Children.Add(rootBorder);
            ((System.Windows.Markup.IAddChild)pageContent).AddChild(page);
            return pageContent;
        }

        //获取打印模版中的rootBorder(位于root BlockUIContainer下面)
        private Border GetRootBorderFromTemplate(string templateFile)
        {
            //  Stream s = CommonMethod.StreamFromResource( templateFile);

            FlowDocument flowDoc = Ai.Hong.ReportTemplate.LoadDocumentTemplate(System.Reflection.Assembly.GetExecutingAssembly(), templateFile);
            string ss = Ai.Hong.ReportTemplate.ErrorString;

            if (flowDoc == null)
                return null;

            BlockUIContainer block = flowDoc.FindName("rootBlock") as BlockUIContainer;     //根内容
            if (block == null || block.Child == null)
                return null;

            Border rootBorder = block.Child as Border;
            Border retborder = Ai.Hong.ReportTemplate.CloneObject<Border>(rootBorder);       //克隆一个rootBorder作为返回

            return retborder;
        }

        System.Data.DataTable intensityDataTable = new System.Data.DataTable();

        /// <summary>
        /// 在指定外框中创建Image控件，并显示光谱
        /// </summary>
        /// <param name="rootBorder">文档根Control</param>
        /// <param name="graphicBorderName">图像外框</param>
        /// <param name="graphicFiles">多个光谱文件</param>
        /// <param name="graphicWidth">图像宽度</param>
        /// <param name="graphicHeight">图像高度</param>
        /// <param name="DPI">图像精度DPI</param>
        private void ShowSpectrumGraphic(Border rootBorder, string graphicBorderName, string[] graphicFiles, double graphicWidth, double graphicHeight = double.MaxValue, double DPI = double.MaxValue)
        {
            if (graphicFiles == null)
                return;

            Border graphicBorder = rootBorder.FindName(graphicBorderName) as Border;
            if (graphicBorder != null)
            {
                System.Windows.Forms.DataVisualization.Charting.Chart graphicChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
                System.Windows.Forms.DataVisualization.Charting.ChartArea ca = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
                graphicChart.ChartAreas.Add(ca);
                foreach (string file in graphicFiles)
                {
                    if (file != null)
                    {
                        Ai.Hong.CommonMethod.DrawSpectrumGraphic(graphicChart, ca, file, System.Drawing.Color.Black);
                    }
                }

                DPI = (DPI == double.MaxValue) ? 96 : DPI;
                graphicHeight = (graphicHeight == double.MaxValue) ? graphicBorder.Height * DPI / 96 : graphicHeight * DPI / 2.54;

                graphicChart.Width = (int)(graphicWidth * DPI / 2.54);        //1cm = 2.54inch = 96dpi
                graphicChart.Height = (int)(graphicHeight);

                System.IO.MemoryStream stream = new MemoryStream();
                graphicChart.SaveImage(stream, System.Drawing.Imaging.ImageFormat.Png);

                var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();

                Image img = new Image();
                graphicBorder.Child = img;
                img.Source = bitmapImage;
                img.Stretch = System.Windows.Media.Stretch.Uniform;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootBorder"></param>
        /// <param name="resultTextName"></param>
        /// <param name="resultImageName"></param>
        /// <param name="resultValue"></param>
        /// <param name="thresoldValue"></param>
        /// <param name="lessthan">false  测量值>=阈值  true  测量值小于等于阈值</param>
        /// <param name="printFormat"></param>
        private void ShowResultData(Border rootBorder, string thresoldTextName, string resultTextName, string resultImageName, double resultValue, double thresoldValue, bool lessthan, string printFormat,bool IsNullSpc)
        {
            string dispText = resultValue.ToString(printFormat);// +"(标准范围：";
            bool isok = false;
            if (lessthan)
            {
                if(!IsNullSpc)
                {
                    isok = (resultValue <= thresoldValue);
                }
                //if (resultTextName == "realmaxWaveDev")
                //{
                //    if (settingData.calibratePara.waveNumRepTestPara.SpectrumPath.Count != 0)
                //    {
                //        isok = (resultValue <= thresoldValue);
                //    }
                //}
                //else if (resultTextName == "realPolyDev" || resultTextName == "realAcc")
                //{
                //    if (!string.IsNullOrEmpty(settingData.calibratePara.accuracyTestOQ.SpectrumPath))
                //        isok = (resultValue <= thresoldValue) && (settingData.calibratePara.accuracyTestOQ.TestResult <= settingData.calibratePara.accuracyTestOQ.thresold);
                //}
                //else if (resultTextName == "realPolyDevPQ" || resultTextName == "realAccPQ")
                //{
                //    if (!string.IsNullOrEmpty(settingData.calibratePara.accuracyPara.PolySpectrumPath))
                //        isok = (resultValue <= thresoldValue) && (settingData.calibratePara.accuracyPara.TestResult <= settingData.calibratePara.accuracyPara.thresold);
                //}
                //else
                //{
                //    isok = (resultValue <= thresoldValue) && resultValue > 0;
                //}
            }
            else
            {
                if (!IsNullSpc)
                    isok = (resultValue >= thresoldValue);
                //isok = (resultValue >= thresoldValue) && resultValue > 0;
            }
            Ai.Hong.ReportTemplate.FillTextData(rootBorder, resultTextName, dispText);
            Ai.Hong.ReportTemplate.FillTextData(rootBorder, thresoldTextName, thresoldValue.ToString(printFormat));
            TextBlock tx = rootBorder.FindName(resultTextName) as TextBlock;
            if (!isok && tx != null)
            {
                tx.FontWeight = FontWeights.Bold;
                tx.Foreground = Brushes.Red;
            }

            Image img = rootBorder.FindName(resultImageName) as Image;
            Ai.Hong.CommonMethod.SetImageSource(img, imagePath, isok ? "Calibration_OK.png" : "error_32.png");
        }

        //private bool ShowLineSlope(Border rootBorder, string dataGridName, string resultImageName)
        //{

        //}

        /// <summary>
        /// 创建PQ结果报告
        /// </summary>
        /// <param name="xpsFile"></param>
        /// <returns></returns>
        private bool CreateCalibrationReport(string xpsFile)
        {
            Calibration.MeasureParameter mp = new MeasureParameter();

            //光谱图和数据
            //  Border rootBorder = GetRootBorderFromTemplate("CalibrationReport_All.xaml");
            Border rootBorder = GetRootBorderFromTemplate("PrintModelEditor.xaml");

            if (rootBorder == null)
                return false;
            rootBorder.Width = 18 * DPCM;
            rootBorder.Margin = new Thickness(1.5 * DPCM, 1.5 * DPCM, 1.5 * DPCM, 1 * DPCM);

            //自检基本信息
            Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtCompany", settingData.runing_para.unitName);
            Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtOperator", null);
            Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtInstrumentNo", settingData.runing_para.serialNo);
            Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtCalibrateTime", DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));

            //显示光谱图形
            ShowSpectrumGraphic(rootBorder, "borderAccuracyGraphic", accuracySpectrumFile, 18, double.MaxValue, 200);
            ShowSpectrumGraphic(rootBorder, "borderSnrGraphic", snrSpectrumFile, 18, double.MaxValue, 200);

            ////显示结果数据
            //ShowResultData(rootBorder, "txtSureResult", "imgSureResult", accuracyResult, settingData.calibratePara.accuracyPara.thresold, true, "F2");
            //ShowResultData(rootBorder, "txtSNRRMSResult", "imgSNRRMSResult", snrRMSResult, settingData.calibratePara.snrPara.RMSThresold, false, "F0");
            //ShowResultData(rootBorder, "txtSNRPPResult", "imgSNRPPResult", snrPPResult, settingData.calibratePara.snrPara.PPThresold, false, "F0");

            //最终数据
            TextBlock allTxt = rootBorder.FindName("txtAllResult") as TextBlock;
            Image allImg = rootBorder.FindName("imgAllResult") as Image;

            if (allTxt != null && allImg != null)
            {
                if (settingData.calibratePara.accuracyPara.TestResult > settingData.calibratePara.accuracyPara.thresold ||
                    settingData.calibratePara.accuracyPara.PolyThresold > settingData.calibratePara.accuracyPara.PolyThresold ||
                    snrRMSResult < settingData.calibratePara.snrPara.RMSThresold ||
                    snrPPResult < settingData.calibratePara.snrPara.PPThresold)
                {
                    allTxt.Text = "自检结果 = 未通过";
                    allTxt.Foreground = System.Windows.Media.Brushes.Red;
                    Ai.Hong.CommonMethod.SetImageSource(allImg, imagePath, "error_32.png");
                }
                else
                {
                    allTxt.Text = "自检结果 = 通过";
                    allTxt.Foreground = System.Windows.Media.Brushes.Blue;
                    Ai.Hong.CommonMethod.SetImageSource(allImg, imagePath, "Calibration_OK.png");
                }
            }

            PageContent content = CreatePageContent(rootBorder);
            return WriteXpsFile(content, xpsFile);
        }

        //实际写入XPS文件
        private bool WriteXpsFile(PageContent content, string xpsFile)
        {
            try
            {
                FixedDocument fixedDoc = new FixedDocument();
                fixedDoc.Pages.Add(content);

                return Ai.Hong.ReportTemplate.SaveToXpsFile(xpsFile, fixedDoc);

            }
            catch (Exception ex)
            {
                Ai.Hong.CommonMethod.ErrorMsgBox(ex.Message);
                return false;
            }
        }
        #endregion 生成报告


        #region 检查仪器是否联机

        /// <summary>
        /// 联机检测线程中刷新仪器状态
        /// </summary>
        /// <param name="connected"></param>
        private delegate void UpdateInstrumentDeletage(bool connected);
        private void UpdateInstrument(bool connected)
        {
            if (settingData.runing_para.isDebug)
                connected = true;

            //InstrumentConnected = connected;
            if (connected)
            {
                laserPanel.IsEnabled = true;
                accuracyPanel.IsEnabled = true;
                snrPanel.IsEnabled = true;
                //Ai.Hong.CommonMethod.SetImageSource(imgStatus, imagePath, "OK_16.png");
                //txtStatus.Text = "联机";
            }
            else
            {
                laserPanel.IsEnabled = false;
                accuracyPanel.IsEnabled = false;
                snrPanel.IsEnabled = false;
                //Ai.Hong.CommonMethod.SetImageSource(imgStatus, imagePath, "Error_16.png");
                //txtStatus.Text = "脱机";
            }
        }


        /// <summary>
        /// 仪器状态线程
        /// </summary>
        //Thread instrumentTread = null;

        /// <summary>
        /// 通过USB端口检查仪器状态的Thread
        /// </summary>
        private void CheckInstrumentState()
        {
            bool oldUsbState = false;
            while (true)
            {
                bool curUsbState = false;

                System.Management.ManagementObjectSearcher wmiSearcher = new System.Management.ManagementObjectSearcher("Select * From WIN32_USBControllerDevice");
                foreach (System.Management.ManagementObject wmi_USB in wmiSearcher.Get())
                {
                    string strPath = wmi_USB.Path.RelativePath;
                    if (strPath.Contains(settingData.runing_para.usbDeviceID))     //这是EnwaveRaman仪器的Device ID(硬件管理器-详细信息-硬件ID
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
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new UpdateInstrumentDeletage(UpdateInstrument), oldUsbState);
                }
                Thread.Sleep(10 * 1000);      //10秒检查一次
            }
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //启动仪器连接检查线程
            //instrumentTread = new Thread(new ThreadStart(CheckInstrumentState));
            //instrumentTread.IsBackground = true;
            //instrumentTread.Name = "instrumentTread";
            //instrumentTread.Start();

            // InitControlStatus();
        }

        private void LWCMainThread()
        {
            try
            {
                // logfile = Path.Combine(startPath, "logfile.txt");
                ITError = null;
                if (LWNCorrect() == false)
                    throw new Exception("激光波数校正:" + ITError);
                if (userCancel)
                    throw new Exception("用户取消");
            }
            catch (Exception ex)
            {
                //  if (!userCancel)
                //Ai.Hong.CommonMethod.ErrorMsgBox(ITError);
                Ai.Hong.CommonMethod.ErrorMsgBox(ex.Message);
            }

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetStartButtonDelegate(SetStartButton), false);
        }

        private void btnLWStart_Click(object sender, RoutedEventArgs e)
        {
           
            // SettingData.settingData.Serialize(@"E:\Project\NirIdentifier\bin\Release\setting\NirIdentifier.setting");

            if (!isConnect)
            {
                MessageBox.Show("提示", "仪器未连接！");
                return;
            }
            laser.IsEnabled = false;
            LWCThread = new Thread(new ThreadStart(LWCMainThread));
            //     ITTread.Name = "ITTread";
            LWCThread.IsBackground = false;
            LWCThread.SetApartmentState(ApartmentState.STA);
            LWCThread.Start();
            laser.IsEnabled = true;
        }

        private void btnLWPrintReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string xpsfile = Path.Combine(startPath, "Report");
                if (!Directory.Exists(xpsfile))
                    Directory.CreateDirectory(xpsfile);

                xpsfile = Path.Combine(xpsfile, "Report" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".xps");
                if (CreateCalibrationReport(xpsfile) == false)
                    throw new Exception("创建测试报告错误");

                System.Diagnostics.Process.Start(xpsfile);       //打开自检报告
            }
            catch (Exception ex)
            {
                if (!userCancel)
                    Ai.Hong.CommonMethod.ErrorMsgBox(ex.Message);
            }
        }

        private bool ScanRefYaxis(string refSpc, string iniFileName)
        {
            string backSpec = refSpc + "\\back.spc";
            int scanCount = GetScanCountFromIni(iniFileName, true);
            //复原转轮
            if (!VspecInstrument.MoveWheel(0))
            {
                throw new Exception("复原转轮失败！");
            }
            
            backSpec = Common.VspecInstrument.ScanBackground(iniFileName, scanCount, backSpec, Common.VspecInstrument.IsMoveFlagBack());//扫背景
            if (backSpec == null)
            {
                ITError = "扫描光谱错误:" + Common.VspecInstrument.GetError();
                return false;
            }

            //转动到滤光玻璃
            if (!VspecInstrument.MoveWheel(1))
            {
                throw new Exception("测量参考光谱出错！转轮转动到滤光玻璃失败！");
            }

            string sampleSpec = refSpc + "\\ref.spc";
            
            sampleSpec = Common.VspecInstrument.ScanSample(iniFileName, scanCount, sampleSpec, Common.VspecInstrument.IsMoveFlagBack());//扫样品
            if (sampleSpec == null)
            {
                ITError = "扫描光谱错误:" + Common.VspecInstrument.GetError();
                return false;
            }

            //读取样品1和样品2数据
            Ai.Hong.CommonLibrary.SpecFileFormatDouble backData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
            Ai.Hong.CommonLibrary.SpecFileFormatDouble sampleData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();

            if (backData.ReadFile(backSpec) == false)
            {
                ITError = "读取光谱错误:" + backSpec + " " + backData.ErrorString;
                return false;
            }
            if (sampleData.ReadFile(sampleSpec) == false)
            {
                ITError = "读取光谱错误:" + sampleSpec + " " + sampleData.ErrorString;
                return false;
            }

            //计算样品通道谱/背景通道谱 得到透射谱
            for (int index = 0; index < sampleData.Parameter.dataCount; index++)
            {
                if (sampleData.YDatas[index] == 0)
                    sampleData.YDatas[index] = 100;
                else
                    sampleData.YDatas[index] = Math.Abs(sampleData.YDatas[index] / backData.YDatas[index]) * 100;
            }

            ////保存透射光谱
            //if (settingData.runing_para.isDebug)
            //    para.SpectrumFile = sampleSpecFile.Replace(".spc", "_tr.spc");
            //else
            //    para.SpectrumFile = sampleSpecFile.Replace("_sbm.spc", "_tr.spc");
            float[] trData = new float[sampleData.YDatas.Length];
            for (int index = 0; index < trData.Length; index++)
            {
                // trData[index] = (float)Math.Log10(1 / sampleData.YDatas[index]);//得到透射谱
                trData[index] = (float)sampleData.YDatas[index];//得到透射谱
            }
            string resolution = ReadResolution(iniFileName);
            sampleData.Parameter.resolution = resolution;
            Ai.Hong.CommonLibrary.SPCFile.SaveFile(refSpc + "\\ref.spc", trData, sampleData.Parameter);
           // Ai.Hong.CommonLibrary.SPCFile.SaveFile(refSpc + "\\ref.spc", sampleData.XDatas, sampleData.YDatas, Convert.ToInt32(resolution), null, null, null);
                //refSpc + "\\ref.spc", trData, sampleData.Parameter);
            return true;
        }
        /// <summary>
        /// 从ini读取resolution
        /// </summary>
        /// <param name="path">ini路径</param>
        /// <returns></returns>
        private string ReadResolution(string path)
        {
            return Ai.Hong.CommonMethod.ReadIniFile(path, "Collection", "resolution").ToString();
        }
        private bool YaxisRepTest()
        {
            //开始信噪比检测
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), yaxisPanel, null, null, 1, 0);
            Common.SettingFile.Calibrate_Parameter.YaxisRep_Parameter para = settingData.calibratePara.yaxisRepPara;
            string iniFileName = Path.Combine(startPath, para.iniFile);
            if(CurrentTimePath=="")
                CurrentTimePath = DateTime.Now.ToString("HH_mm_ss");
            para.SpectrumFile = "";
            para.TestResult = 0;
            string dataPath = Path.Combine(startPath, "Data", "IT_" + SettingData.settingData.runing_para.serialNo + "\\PQ\\" + DateTime.Now.ToString("yyyy_MM_dd") + "\\" + CurrentTimePath + "\\YaxisRepTest");
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            ////测量参考光谱
            string refSpc = Path.Combine(Environment.CurrentDirectory, "Data", "IT_" + SettingData.settingData.runing_para.serialNo+"\\ReferenceSpc_" + SettingData.settingData.runing_para.serialNo + "\\YaxisRepTest");
            if (!Directory.Exists(refSpc))
            {
                Directory.CreateDirectory(refSpc);
            }
            //如果不存在参考光谱则扫描参考光谱
            if (!File.Exists(refSpc + "\\ref.spc"))
            {
                if (!ScanRefYaxis(refSpc, iniFileName))
                {
                    throw new Exception("扫描吸收重复性测试参考光谱出错！");
                }
            }

            //snrRMSResult = snrPPResult = 0;
            //snrSpectrumFile = new string[settingData.calibratePara.snrPara.repeatCount];
            string backSpecFile = null, sampleSpecFile = null;


            // string samplefile1 = null, samplefile2 = null;
            if (settingData.runing_para.isDebug)
            {
                //backSpecFile = Path.Combine(startPath, debugSampleFile + (i % 10).ToString() + ".spc");     //只有10个文件
                //sampleSpecFile = Path.Combine(startPath, debugSampleFile + ((i + 1) % 10).ToString() + ".spc");     //只有10个文件
                Thread.Sleep(1000);
            }
            else
            {
                //从仪器中采集背景光谱
                // dataPath = Path.Combine(startPath, "Data", "Calibration\\PQ", "PQ" + DateTime.Now.ToString("ddMMyyyy"));
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);
                backSpecFile = Path.Combine(dataPath, "YaxisRepBack_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");

                // Ai.Hong.CommonMethod.AddToLogFile(logfile, "start scan YaxisRepBack spectrum:" + backSpecFile + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
                int scanCount = GetScanCountFromIni(iniFileName, true);
                //复原转轮
                if (!VspecInstrument.MoveWheel(0))
                {
                    throw new Exception("复原转轮失败！");
                }
               
                backSpecFile = Common.VspecInstrument.ScanBackground(iniFileName, scanCount, backSpecFile, Common.VspecInstrument.IsMoveFlagBack());
                if (backSpecFile == null)
                {
                    ITError = "Yaxi-扫描背景光谱错误:" + Common.VspecInstrument.GetError();
                    return false;
                }
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "scan YaxisRepBack spectrum:" + backSpecFile + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));

                //从仪器中采集样品光谱
                // scanCount = GetScanCountFromIni(iniFileName, true);

                if (!VspecInstrument.MoveWheel(1))
                {
                    throw new Exception("吸收重复性测试-转换到滤光玻璃出错！");
                }

                sampleSpecFile = Path.Combine(dataPath, "YaxisRepSample_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "scan YaxisRepSample spectrum:" + sampleSpecFile + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
                
                sampleSpecFile = Common.VspecInstrument.ScanSample(iniFileName, scanCount, sampleSpecFile, Common.VspecInstrument.IsMoveFlagBack());//, "_sbm");
                if (sampleSpecFile == null)
                {
                    ITError = "Yaxi-扫描样品光谱错误:" + Common.VspecInstrument.GetError();
                    return false;
                }
                //复原转轮
                if (!VspecInstrument.MoveWheel(0))
                {
                    throw new Exception("复原转轮失败！");
                }
            }

            //读取样品1和样品2数据
            Ai.Hong.CommonLibrary.SpecFileFormatDouble backData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
            Ai.Hong.CommonLibrary.SpecFileFormatDouble sampleData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();

            if (backData.ReadFile(backSpecFile) == false)
            {
                ITError = "读取光谱错误:" + backSpecFile + " " + backData.ErrorString;
                return false;
            }
            if (sampleData.ReadFile(sampleSpecFile) == false)
            {
                ITError = "读取光谱错误:" + sampleSpecFile + " " + sampleData.ErrorString;
                return false;
            }

            //计算样品通道谱/背景通道谱 得到透射谱
            for (int index = 0; index < sampleData.Parameter.dataCount; index++)
            {
                if (sampleData.YDatas[index] == 0)
                    sampleData.YDatas[index] = 100;
                else
                    sampleData.YDatas[index] = Math.Abs(sampleData.YDatas[index] / backData.YDatas[index]) * 100;
            }

            //保存透射光谱
            if (settingData.runing_para.isDebug)
                para.SpectrumFile = sampleSpecFile.Replace(".spc", "_tr.spc");
            else
            {
                //  para.SpectrumFile = sampleSpecFile.Replace("_sbm.spc", "_tr.spc");
                para.SpectrumFile = Path.Combine(dataPath, "YaxisRepSample_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + "_tr.spc");
            }
            float[] trData = new float[sampleData.YDatas.Length];
            for (int index = 0; index < trData.Length; index++)
            {
                trData[index] = (float)sampleData.YDatas[index];
                //    sampleData.YDatas[index] = Math.Log10(1 / sampleData.YDatas[index]);
                //  trData[index] = (float)Math.Log10(1 / sampleData.YDatas[index]);//得到AB谱
            }
            sampleData.Parameter.resolution = ReadResolution(iniFileName);
            Ai.Hong.CommonLibrary.SPCFile.SaveFile(para.SpectrumFile, trData, sampleData.Parameter);

            //  sampleData.ReadFile(@"C:\Users\123\Desktop\仪器常规检测文件\20100324_110236\PQ_GFA.spc");
            //读取参考光谱
            Ai.Hong.CommonLibrary.SpecFileFormatDouble RefData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
            if (RefData.ReadFile(refSpc + "\\ref.spc") == false)//refSpc + "\\ref.spc"
            {
                ITError = "读取Yaxis参考光谱错误:" + sampleSpecFile + " " + sampleData.ErrorString;
                return false;
            }
            if(RefData.Parameter.resolution!=sampleData.Parameter.resolution)
            {
                MessageBox.Show("样品光谱与参考光谱分辨率不一致！\r\n"+"参考光谱分辨率："+RefData.Parameter.resolution+"\r\n"+"当前光谱分辨率："+sampleData.Parameter.resolution);
                return false;
            }
            int length = RefData.YDatas.Count() < sampleData.YDatas.Count() ? RefData.YDatas.Count() : sampleData.YDatas.Count() ;
            double RepResult = 0;
            int fx = NirLib.BasicAlgorithm.SpectrumAlgorithm.FindNearestPosition(RefData.XDatas, 0, length-1, para.firstX);
            int lx = NirLib.BasicAlgorithm.SpectrumAlgorithm.FindNearestPosition(RefData.XDatas, 0, length-1, para.lastX);
            for (int i = fx; i <= lx; i++)
            {
                RepResult += Math.Abs(sampleData.YDatas[i] - RefData.YDatas[i]);
            }

            RepResult /= para.lastX - para.firstX;
            //double backInteger = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.Integrate(RefData.XDatas, RefData.YDatas, para.firstX, para.lastX);
            //透射谱算积分
            //double transInteger = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.Integrate(sampleData.XDatas, sampleData.YDatas, para.firstX, para.lastX);
            //double RepResult = Math.Abs(transInteger - backInteger);
            // RepResult /= 100;
            para.TestResult = RepResult;

            //更新界面测量值
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetEachValueDelegate(SetEachValue), yaxisPanel, Convert.ToDouble(RepResult.ToString("F2")), false);

            //吸收重复性验证结果
            if (RepResult > 0 && RepResult < para.YaxisRepThresold)//snrRMSResult > settingData.calibratePara.snrPara.RMSThresold
            {
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "Y-axis Reproducibility Test ok");
                //  Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), yaxisPanel, true, "OK_16.png", -1, 1);
            }
            else
            {
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "Y-axis Reproducibility Test Fail");
                //   Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), yaxisPanel, false, "Error_16.png", -1, 1);
            }
            return true;
        }

        /// <summary>
        /// 为LineSlopeTest更新测量值
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="d"></param>
        /// <param name="ju"></param>
        private delegate void SetLineSlopeDelegate(LineSlopeTest panel, bool ju);
        private void SetLineSlopeValue(LineSlopeTest panel, bool ju)
        {
            panel.SetRealCalibrateValue(ju);
            lineSlopeTest.dataGrid.ItemsSource = null;
            lineSlopeTest.dataGrid.Items.Clear();
            lineSlopeTest.dataGrid.ItemsSource = settingData.calibratePara.lineSlopeTestPara.data1;
        }



        /// <summary>
        /// 为EachCalibratePanel更新测量值
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="d"></param>
        /// <param name="ju"></param>
        private delegate void SetEachValueDelegate(EachCalibratePanel panel, double d, bool? ju);
        private void SetEachValue(EachCalibratePanel panel, double d, bool? ju)
        {
            if (panel == null)
                return;
            panel.SetRealCalibrateValue(d, ju);
        }

        /// <summary>
        /// 为AccuracyPanel更新测量值
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="ppValue"></param>
        /// <param name="devValue"></param>
        private delegate void SetAccuracyDelegate(Accuracy panel, double accValue, double PolyValue);
        private void SetAccuracyValue(Accuracy panel, double accValue, double PolyValue)
        {
            panel.SetRealCalibrateValue(accValue, PolyValue);
        }

        /// <summary>
        /// 为PPDeviationPanel更新测量值
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="ppValue"></param>
        /// <param name="devValue"></param>
        private delegate void SetPPDevValueDelegate(PPDeviation panel, double ppValue, double devValue, double ipaValue, double engValue);
        private void SetPPDevValue(PPDeviation panel, double ppValue, double devValue,double ipaValue,double engValue)
        {
            panel.SetRealCalibrateValue(ppValue, devValue,ipaValue,engValue);
        }


        private void PQMainThread()
        {
            if (settingData.calibratePara.snrPara.IsTest)
            {
                try
                {
                    // logfile = Path.Combine(startPath, "logfile.txt");
                    ITError = null;

                    //100%线噪比及偏差测试
                    if (LineNoiseAndDevTest() == false)
                        throw new Exception("100%线噪比及偏差测试:" + ITError);
                    if (userCancel)
                        throw new Exception("用户取消");
                }
                catch (Exception ex)
                {
                    //if (!userCancel)
                    Ai.Hong.CommonMethod.ErrorMsgBox(ex.Message);
                }
            }
            if (settingData.calibratePara.accuracyPara.IsTest)
            {
                try
                {

                    //波数精度测试
                    if (AccuracyCal() == false)
                        throw new Exception("波数精度测试:" + ITError);
                    if (userCancel)
                        throw new Exception("用户取消");
                }
                catch (Exception ex)
                {
                    //if (!userCancel)
                    Ai.Hong.CommonMethod.ErrorMsgBox(ex.Message);
                }
            }
            if (settingData.calibratePara.yaxisRepPara.IsTest)
            {
                try
                {

                    //Y轴重复性测试
                    if (YaxisRepTest() == false)
                        throw new Exception("吸收重复性测试：" + ITError);
                    if (userCancel)
                        throw new Exception("用户取消");

                    //存储检测结果到数据库
                    PQData pdata = new PQData();
                    pdata.lineNoise = settingData.calibratePara.snrPara.LineNoiseResult;
                    pdata.lineDev = settingData.calibratePara.snrPara.TestResult;
                    pdata.waveNum = settingData.calibratePara.accuracyPara.TestResult;
                    pdata.yaxisRep = settingData.calibratePara.yaxisRepPara.TestResult;
                    SettingData.dataBase.InsertPQOQ(pdata);
                }
                catch (Exception ex)
                {
                    //if (!userCancel)
                    Ai.Hong.CommonMethod.ErrorMsgBox(ex.Message);
                }
            }
            btnPQPrintReport_Click(null, null);
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetStartButtonDelegate(SetStartButton), false);
        }

        private void btnPQStart_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnect)
            {
                MessageBox.Show("仪器未连接！", "提示");
                return;
            }
            pq.IsEnabled = false;
            spectrumChart.Visibility = Visibility.Visible;
            mainFGrid.Visibility = Visibility.Collapsed;
            PQThread = new Thread(new ThreadStart(PQMainThread));
            PQThread.IsBackground = false;
            PQThread.SetApartmentState(ApartmentState.STA);
            PQThread.Start();
            pq.IsEnabled = true;
        }

        private void btnPQPrintReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string xpsfile = Path.Combine(startPath, "Report", SettingData.settingData.runing_para.serialNo,"PQ");
                if (!Directory.Exists(xpsfile))
                    Directory.CreateDirectory(xpsfile);

                xpsfile = Path.Combine(xpsfile, "PQ_Report" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".xps");
                if (CreatePQReport(xpsfile, true) == false)
                    throw new Exception("创建测试报告错误");

                System.Diagnostics.Process.Start(xpsfile);       //打开自检报告
            }
            catch (Exception ex)
            {
                if (!userCancel)
                    Ai.Hong.CommonMethod.ErrorMsgBox(ex.Message);
            }
        }

        private string CalMidPoint(double firstX, double firstY, double endX, double endY, double targetX, double targetY)
        {

            if (firstY == endY)
            {
                return ((firstX + endX) / 2).ToString() + "+" + ((targetY + firstY) / 2).ToString();
            }
            else
            {

                double temp = targetX * (firstX - endX) * (firstX - endX) - (endY * targetY - firstY * targetY) * (firstX - endX) + (firstX * endY - firstY * endX) * (endY - firstY);
                //double temp = (endX - firstX) * (targetX * firstX - targetX * endX - targetY * firstY + targetY * endY) + (endY - firstY) * (endY * firstX - endX * firstY);
                double x = temp / ((endY - firstY) * (endY - firstY) + (endX - firstX) * (endX - firstX));
                double y = (firstX * x - endX * x + endX * targetX - firstX * targetX - firstY * targetY + endY * targetY) / (endY - firstY);
                return ((x + targetX) / 2).ToString() + "+" + ((y + targetY) / 2).ToString();
            }
        }
        //                  CalResolution((int)newStartX, newStartY, (int)newEndX, newEndY, (int)midPointX, midPointY, specData.XDatas, specData.YDatas);

        private double CalResolution(double firstX, double firstY, double endX, double endY, double midPointX, double midPointY, double[] DataX, double[] DataY)
        {
            double Resolution = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.Integrate(DataX, DataY, firstX, endX);
            int tar = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.FindNearestPosition(DataX, 0, DataX.Length - 1, settingData.calibratePara.resolutionTestPara.targetPeak);
            double tary;
            double tarx = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(DataX, DataY, settingData.calibratePara.resolutionTestPara.targetPeak, 4, true, out tary);
            string hi = CalMidPoint(firstX, firstY, endX, endY, tarx, tary);
            string[] tt = hi.Split('+');
            double xMid = Convert.ToDouble(tt[0]);
            double yMid = Convert.ToDouble(tt[1]);
            double tem = (xMid - tarx) * (xMid - tarx) + (yMid - tary) * (yMid - tary);


            //峰高
            double PickHeight = 2 * Math.Sqrt(tem);
            Resolution = Resolution / PickHeight;
            return Resolution;

        }

        private bool ResolutionTest()
        {


            string spectrumFile = null;

            Common.SettingFile.Calibrate_Parameter.OQTest.ResolutionTest para = settingData.calibratePara.resolutionTestPara;
            string iniFileName = Path.Combine(startPath, para.iniFile);
            para.SpectrumPath = string.Empty;
            para.TestResult = 0;
            CurrentTimePath = DateTime.Now.ToString("HH_mm_ss");
            if (settingData.runing_para.isDebug)
            {
                spectrumFile = Path.Combine(startPath, debugBgFile);
                Thread.Sleep(1000);
            }
            else
            {
                //从仪器中采集背景光谱
                string dataPath = Path.Combine(startPath, "Data", "IT_" + SettingData.settingData.runing_para.serialNo + "\\OQ\\" + DateTime.Now.ToString("yyyy_MM_dd") + "\\" + CurrentTimePath + "\\ResolutionTest");
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);
                spectrumFile = Path.Combine(dataPath, "ResolutionTest_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");

                Ai.Hong.CommonMethod.AddToLogFile(logfile, "start scan ResolutionTest spectrum:" + spectrumFile + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
                int scanCount = GetScanCountFromIni(iniFileName, true);
                //复原转轮
                if (!Common.VspecInstrument.MoveWheel(0))
                {
                    throw new Exception("复原转轮失败！");
                }
                //开始波数准确度检测
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), resolutionTest, null, null, 1, 0);
                
                spectrumFile = Common.VspecInstrument.ScanBackground(iniFileName, scanCount, spectrumFile, Common.VspecInstrument.IsMoveFlagBack());
                // spectrumFile = @"C:\Users\xu.chuan\Desktop\123\ResolutionTest_2015-05-15 15-02-10_rsb.spc";
                if (spectrumFile == null)
                {
                    ITError = "扫描光谱错误:" + Common.VspecInstrument.GetError();
                    return false;
                }
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "stop scan ResolutionTest spectrum:" + spectrumFile + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
            }

            para.SpectrumPath = spectrumFile;

            //读取背景光谱
            Ai.Hong.CommonLibrary.SpecFileFormatDouble specData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
            if (specData.ReadFile(para.SpectrumPath) == false)
            {
                ITError = "读取光谱错误:" + spectrumFile + " " + specData.ErrorString;
                return false;
            }

            float[] tr = new float[specData.YDatas.Length];
            //取负对数
            for (int i = 0; i < specData.YDatas.Length; i++)
            {
                specData.YDatas[i] = Math.Log10(1.0 / specData.YDatas[i]) + 1;
                tr[i] = (float)specData.YDatas[i];
            }

            Ai.Hong.CommonLibrary.SPCFile.SaveFile(spectrumFile, tr, specData.Parameter);

            double tteemp;
            double FindTargetPeak = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, para.targetPeak, 4, true, out tteemp);
            int x = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.FindNearestPosition(specData.XDatas, 0, specData.XDatas.Length - 1, FindTargetPeak);
            double leftY = specData.YDatas[x];
            int temp = x;
            int startX = x, endX = x;
            if (specData.YDatas[x - 1] < leftY)//x在上升沿中
            {
                //循环到峰位
                while (specData.YDatas[x + 1] > leftY)
                {
                    leftY = specData.YDatas[x + 1];
                    x += 1;
                    temp = x;
                    if (x + 1 > specData.YDatas.Length - 1)
                        break;
                }
                //找到endX
                while (specData.YDatas[x + 1] < leftY)
                {
                    // if ()
                    //   break;
                    leftY = specData.YDatas[x + 1];
                    x += 1;
                    endX = x;
                    if (x + 1 > specData.YDatas.Length - 1)
                        break;

                }

                x = temp;//恢复原x  找到startX
                double tt = specData.YDatas[x];
                //while(true)
                //{
                //    if(specData.YDatas[x-1]>tt)
                //    {
                //        x=x-1;
                //        break;
                //    }
                //    x-=1;
                //    tt = specData.YDatas[x - 1];
                //}
                while (specData.YDatas[x - 1] < tt)
                {
                    tt = specData.YDatas[x - 1];
                    startX = x - 1;
                    x -= 1;
                    if (x - 1 < 0)
                        break;
                }
            }
            else//x在下降沿中
            {
                //找到endX
                while (specData.YDatas[x + 1] < leftY)
                {
                    leftY = specData.YDatas[x + 1];
                    x += 1;
                    endX = x;
                    if (x + 1 > specData.YDatas.Length - 1)
                        break;
                }
                leftY = specData.YDatas[temp];
                x = temp;
                while (specData.YDatas[x - 1] > leftY)
                {
                    leftY = specData.YDatas[x - 1];
                    x -= 1;
                    if (x - 1 < 0)
                        break;
                }
                double tt = specData.YDatas[x];
                while (specData.YDatas[x - 1] < tt)
                {
                    tt = specData.YDatas[x - 1];
                    x -= 1;
                    startX = x;
                    if (x - 1 < 0)
                        break;
                }
                //while (specData.YDatas[x + 1] < tt)
                //{
                //    tt = specData.XDatas[x + 1];
                //    endX = x + 1;
                //    x += 1;
                //    if (x + 1 > specData.YDatas.Length - 1)
                //        break;
                //}

                //x = temp;//恢复原x  找到startX
                //double rightY = specData.YDatas[x];
                //while (specData.YDatas[x - 1] < rightY)
                //{
                //    rightY = specData.YDatas[x - 1];
                //    startX = x - 1;
                //    x -= 1;
                //    if (x - 1 < 0)
                //        break;
                //}
            }

            //积分运算
            // double integrateResult = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.Integrate(specData.XDatas, specData.YDatas, specData.XDatas[startX], specData.XDatas[endX]);

            double targetY = 0;
            //找到峰位
            for (int i = 0; i < endX - startX; i++)
            {
                if (specData.YDatas[temp - 1] < specData.YDatas[temp])//x在上升沿中
                {
                    if (specData.YDatas[temp - i] > targetY)
                    {
                        targetY = specData.YDatas[temp - i];
                    }
                    else
                    {
                        break;
                    }
                }
                else//下降沿找峰位
                {
                    if (specData.YDatas[temp + i] > targetY)
                    {
                        targetY = specData.YDatas[temp + i];
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //标定峰位
            double newYValue;
            double targetPeak = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, para.targetPeak, 4, true, out newYValue);
            double newEndY;
            double newEndX = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, specData.XDatas[endX], 4, false, out newEndY);
            double newStartY;
            double newStartX = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, specData.XDatas[startX], 4, false, out newStartY);
            //if (newStartX > newEndX)
            //{
            //    double tem = newStartX;
            //    newStartX = newEndX;
            //    newEndX = tem;

            //    tem = newStartY;
            //    newStartY = newEndY;
            //    newEndY = tem;
            //}

            string midPoint = CalMidPoint(newStartX, newStartY, newEndX, newEndY, targetPeak, newYValue);
            string[] mid = midPoint.Split('+');
            double midPointX = Convert.ToDouble(mid[0]);
            double midPointY = Convert.ToDouble(mid[1]);

            int midX = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.FindNearestPosition(specData.XDatas, 0, specData.XDatas.Length - 1, midPointX);
            //  double Resolution = CalResolution((int)newStartX, newStartY, (int)newEndX, newEndY, (int)midPointX, midPointY, specData.XDatas, specData.YDatas);
            double Resolution = CalResolution(newStartX, newStartY, newEndX, newEndY, midPointX, midPointY, specData.XDatas, specData.YDatas);
            para.TestResult = Resolution;
            //更新界面测量值
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetEachValueDelegate(SetEachValue), resolutionTest, Convert.ToDouble(Resolution.ToString("F2")), false);

            //波数准确度验证结果
            if (Resolution < settingData.calibratePara.resolutionTestPara.ResolutionThresold)
            {
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "ResolutionTest calibration ok");
                //  Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), accuracyPanel, true, "OK_16.png", -1, 1);
            }
            else
            {
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "ResolutionTest calibration fail");
                //  Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), accuracyPanel, false, "Error_16.png", -1, 1);
            }
            return true;
        }

        /// <summary>
        /// 安装好仪器后第一次采集参考光谱
        /// </summary>
        /// <param name="dataPath"></param>
        /// <param name="iniFileName"></param>
        /// <returns></returns>
        private bool ScanRef(string dataPath, string iniFileName, SettingFile.Calibrate_Parameter.SNR_Parameter para)
        {
            int scanCount = GetScanCountFromIni(iniFileName, true);
            string tempBack = Path.Combine(dataPath, "Ref_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");
           
            tempBack = Common.VspecInstrument.ScanBackground(iniFileName, scanCount, tempBack, Common.VspecInstrument.IsMoveFlagBack());//, "_sbm");

            string tempSample = Path.Combine(dataPath, "Ref_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");
           
            tempSample = Common.VspecInstrument.ScanSample(iniFileName, scanCount, tempSample, Common.VspecInstrument.IsMoveFlagBack());//, "_sbm");

            //读取样品1和样品2数据
            Ai.Hong.CommonLibrary.SpecFileFormatDouble backData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
            Ai.Hong.CommonLibrary.SpecFileFormatDouble sampleData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();

            if (backData.ReadFile(tempBack) == false)
            {
                ITError = "读取光谱错误:" + tempBack + " " + backData.ErrorString;
                return false;
            }
            if (sampleData.ReadFile(tempSample) == false)
            {
                ITError = "读取光谱错误:" + tempSample + " " + sampleData.ErrorString;
                return false;
            }

            Ai.Hong.CommonLibrary.SpecFileFormatDouble saveRef = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
            saveRef.YDatas = new double[backData.Parameter.dataCount];
            saveRef.XDatas = new double[backData.Parameter.dataCount];


            //保存透射光谱
            string savePath = tempBack.Replace("_rsb.spc", "_tr.spc");

            float[] trData = new float[backData.YDatas.Length];
            for (int index = 0; index < trData.Length; index++)
                trData[index] = (float)backData.YDatas[index];
            //计算样品1和样品2比值,得到100%线
            for (int index = 0; index < backData.Parameter.dataCount; index++)
            {
                if (backData.YDatas[index] == 0)
                {
                    saveRef.YDatas[index] = 100;
                    trData[index] = 100;
                }
                else
                {
                    saveRef.YDatas[index] = Math.Abs(sampleData.YDatas[index] / backData.YDatas[index]) * 100;
                    trData[index] = (float)saveRef.YDatas[index];
                }
            }
            Ai.Hong.CommonLibrary.SPCFile.SaveFile(savePath, trData, backData.Parameter);

            Ai.Hong.CommonLibrary.SpecFileFormatDouble readData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();

            if (readData.ReadFile(savePath) == false)
            {
                ITError = "读取光谱错误:" + tempBack + " " + backData.ErrorString;
                return false;
            }
            //取参考光谱数据
            double[] xdatasRe, ydatasRe;
            // double maxYRe = 0;
            Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.GetRangeData(readData.XDatas, readData.YDatas, para.DevfirstX, para.DevlastX, out xdatasRe, out ydatasRe);

            //偏差计算
            for (int j = 0; j < ydatasRe.Length; j++)
            {
                //找透射谱区间内最大Y值
                para.MaxRef = Math.Abs(ydatasRe[j]) / 100 > para.MaxRef ? Math.Abs(ydatasRe[j]) / 100 : para.MaxRef;
            }
            return true;
        }

        private bool LineNoiseAndDevTest()
        {
            //开始检测
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), snrPanel, null, null, 1, 0);
            Common.SettingFile.Calibrate_Parameter.SNR_Parameter para = settingData.calibratePara.snrPara;
            para.SpectrumFile.Clear();
            para.IpaSpectrumFile.Clear();
            para.EngSpectrumFile.Clear();
            para.TestResult = 0;
            para.LineNoiseResult = 0;
            para.IpaTestResult = 0;
            para.EngTestResult = 0;
            string iniFileName = Path.Combine(startPath, para.iniFile);
            CurrentTimePath = DateTime.Now.ToString("HH_mm_ss");
            string dataPath = Path.Combine(startPath, "Data", "IT_" + SettingData.settingData.runing_para.serialNo + "\\PQ\\" + DateTime.Now.ToString("yyy_MM_dd") + "\\" + CurrentTimePath + "\\LineNoiseAndDevTest");
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            //if (para.MaxRef == 0)
            //{
            //    ScanRef(dataPath, iniFileName, para);
            //}
            snrRMSResult = snrPPResult = 0;
            // snrSpectrumFile = new string[settingData.calibratePara.snrPara.repeatCount];
            //复原转轮
            if (!Common.VspecInstrument.MoveWheel(0))
            {
                throw new Exception("复原转轮失败！");
            }
            ////测量参考光谱
            string refSpc = Path.Combine(Environment.CurrentDirectory, "Data", "IT_" + SettingData.settingData.runing_para.serialNo + "\\ReferenceSpc_" + SettingData.settingData.runing_para.serialNo + "\\LineNoiseAndDevTest");
            if (!Directory.Exists(refSpc))
            {
                Directory.CreateDirectory(refSpc);
            }
            string back = Path.Combine(refSpc, "back.spc");
            refSpc = Path.Combine(refSpc, "ref.spc");
            string refIfg = refSpc.Replace(".spc", "_ifg.spc");
            string refSbm = refSpc.Replace(".spc", "_sbm.spc");
            //如果不存在参考光谱则扫描参考光谱
            if (!File.Exists(refIfg) || !File.Exists(refSbm))
            {
                int scanCount = GetScanCountFromIni(iniFileName, true);
                Common.VspecInstrument.ScanBackground(iniFileName, scanCount, back, Common.VspecInstrument.IsMoveFlagBack());
                string temp = Common.VspecInstrument.ScanSample(iniFileName, scanCount, refSpc, Common.VspecInstrument.IsMoveFlagBack());//, "_sbm");
                if (temp==null)
                {
                    throw new Exception("扫描样品单通道光谱出错！");
                }
            }

            Ai.Hong.CommonLibrary.SpecFileFormatDouble refDataSbm = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
            Ai.Hong.CommonLibrary.SpecFileFormatDouble refDataIfg = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();

            if (!refDataSbm.ReadFile(refSbm))
            {
                throw new Exception("读取参考光谱sbm出错！—能量测试");
            }

            if (!refDataIfg.ReadFile(refIfg))
            {
                throw new Exception("读取参考光谱ifg出错！—能量测试");
            }

            double interRef = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.Integrate(refDataSbm.XDatas, refDataSbm.YDatas, para.engFirstX, para.engLastX);
            // 计算干涉峰值
            double tempMaxIfgRef = (from p in refDataIfg.YDatas select p).Max();
            double tempMinIfgRef = (from p in refDataIfg.YDatas select p).Min();
            double IfgRefData = Math.Abs(tempMaxIfgRef - tempMinIfgRef);
            try
            {
                for (int i = 0; i < para.repeatCount; i++)
                {
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), snrPanel, null, null, settingData.calibratePara.snrPara.repeatCount, i);

                    string samplefile1 = null, samplefile2 = null,ifgspc=null;//,sbmspc=null;
                    if (settingData.runing_para.isDebug)
                    {
                        samplefile1 = Path.Combine(startPath, debugSampleFile + (i % 10).ToString() + ".spc");     //只有10个文件
                        samplefile2 = Path.Combine(startPath, debugSampleFile + ((i + 1) % 10).ToString() + ".spc");     //只有10个文件
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        //从仪器中采集样品光谱1
                        int scanCount = GetScanCountFromIni(iniFileName, true);
                        //if (true)//samplefile1 == null || samplefile2 == null)    //第一次需要采集样品光谱1，以后直接使用样品光谱2作为光谱1
                        //{
                        samplefile1 = Path.Combine(dataPath, "LineNoiseAndDevTest_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");
                        Ai.Hong.CommonMethod.AddToLogFile(logfile, "scan snr spectrum:" + samplefile1 + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
                        samplefile1 = Common.VspecInstrument.ScanBackground(iniFileName, scanCount, samplefile1, Common.VspecInstrument.IsMoveFlagBack());//, "_sbm");
                        //}
                        //else
                        //    samplefile1 = samplefile2;

                        //从仪器中采集样品光谱2
                        if (samplefile1 != null)
                        {
                            samplefile2 = Path.Combine(dataPath, "LineNoiseAndDevTest_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");
                            Ai.Hong.CommonMethod.AddToLogFile(logfile, "scan LineNoiseAndDevTest spectrum:" + samplefile2 + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
                            ifgspc = samplefile2.ToLower().Replace(".spc", "_ifg.spc");
                            //sbmspc = samplefile2.ToLower().Replace(".spc", "_sbm.spc");
                            samplefile2 = Common.VspecInstrument.ScanSample(iniFileName, scanCount, samplefile2, Common.VspecInstrument.IsMoveFlagBack());//, "_sbm");
                        }
                        if (samplefile1 == null || samplefile2 == null)
                        {
                            ITError = "扫描光谱错误:" + Common.VspecInstrument.GetError();
                            return false;
                        }
                    }

                    ////读取样品1和样品2数据
                    Ai.Hong.CommonLibrary.SpecFileFormatDouble sampleData1 = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
                    Ai.Hong.CommonLibrary.SpecFileFormatDouble sampleData2 = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
                    Ai.Hong.CommonLibrary.SpecFileFormatDouble ifgSpcData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
                    //Ai.Hong.CommonLibrary.SpecFileFormatDouble sbmSpcData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();

                    if (sampleData1.ReadFile(samplefile1) == false)
                    {
                        ITError = "读取光谱错误:" + samplefile1 + " " + sampleData1.ErrorString;
                        return false;
                    }
                    if (sampleData2.ReadFile(samplefile2) == false)
                    {
                        ITError = "读取光谱错误:" + samplefile2 + " " + sampleData2.ErrorString;
                        return false;
                    }

                    if (ifgSpcData.ReadFile(ifgspc) == false)
                    {
                        ITError = "读取光谱错误:" + ifgspc + " " + ifgSpcData.ErrorString;
                        return false;
                    }

                    //if (sbmSpcData.ReadFile(sbmspc) == false)
                    //{
                    //    ITError = "读取光谱错误:" + sbmspc + " " + sbmSpcData.ErrorString;
                    //    return false;
                    //}

                    // 计算干涉峰值
                    double tempMaxIfg = (from p in ifgSpcData.YDatas select p).Max();
                    double tempMinIfg = (from p in ifgSpcData.YDatas select p).Min();
                    para.IpaTestResult += Math.Abs(tempMaxIfg - tempMinIfg);

                    // 计算积分
                    double inter = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.Integrate(sampleData2.XDatas, sampleData2.YDatas, para.engFirstX, para.engLastX);
                    para.EngTestResult += Math.Abs(interRef - inter);// / Math.Abs(interRef);

                    para.IpaSpectrumFile.Add(ifgspc);
                    para.EngSpectrumFile.Add(samplefile2);

                    //计算样品1和样品2比值,得到100%线
                    for (int index = 0; index < sampleData1.Parameter.dataCount; index++)
                    {
                        if (sampleData1.YDatas[index] == 0)
                            sampleData1.YDatas[index] = 100;
                        else
                            sampleData1.YDatas[index] = Math.Abs(sampleData2.YDatas[index] / sampleData1.YDatas[index]) * 100;
                    }

                    

                    //保存透射光谱
                    if (settingData.runing_para.isDebug)
                        para.SpectrumFile.Add(samplefile1.Replace(".spc", "_tr.spc"));
                    else
                    {
                        para.SpectrumFile.Add(Path.Combine(dataPath, "LineNoiseAndDevTest_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + "_tr.spc"));
                    }
                    float[] trData = new float[sampleData1.YDatas.Length];
                    for (int index = 0; index < trData.Length; index++)
                        trData[index] = (float)sampleData1.YDatas[index];
                    Ai.Hong.CommonLibrary.SPCFile.SaveFile(para.SpectrumFile[i], trData, sampleData1.Parameter);

                    //pp峰值  取透射谱数据
                    double[] xdatas, ydatas;
                    Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.GetRangeData(sampleData1.XDatas, sampleData1.YDatas, para.firstX, para.lastX, out xdatas, out ydatas);
                    double maxy = double.MinValue;
                    double miny = double.MaxValue;
                    double maxyposx = 0, minyposx = 0;

                    for (int index = 0; index < ydatas.Length; index++)
                    {
                        if (ydatas[index] > maxy)
                        {
                            maxy = ydatas[index];
                            maxyposx = xdatas[index];
                        }
                        if (ydatas[index] < miny)
                        {
                            miny = ydatas[index];
                            minyposx = xdatas[index];
                        }
                    }
                    double newmaxy, newminy;
                    Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(xdatas, ydatas, maxyposx, 4, true, out newmaxy);
                    Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(xdatas, ydatas, minyposx, 4, false, out newminy);

                    snrPPResult += Math.Abs((newmaxy - newminy));


                    //取参考光谱数据
                    double[] xdatasRe, ydatasRe;

                    Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.GetRangeData(sampleData1.XDatas, sampleData1.YDatas, para.DevfirstX, para.DevlastX, out xdatasRe, out ydatasRe);
                    double maxY = ydatasRe[0];
                    //偏差计算
                    for (int j = 0; j < ydatasRe.Length; j++)
                    {
                        //找透射谱区间内最大Y值
                        maxY = Math.Abs(ydatasRe[j]) > maxY ? Math.Abs(ydatasRe[j]) : maxY;
                    }

                    devMax += Math.Abs(100 - maxY);// maxY / para.MaxRef;

                    Ai.Hong.CommonMethod.AddToLogFile(logfile, "LineNoiseAndDevTest" + i + "=" + snrPPResult + ", maxy=" + newmaxy + ", miny=" + newminy);
                }
                // para.SpectrumFile.RemoveAt(0);//移除第一条参考谱
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            //snrRMSResult = snrRMSResult / settingData.calibratePara.snrPara.repeatCount;
            snrPPResult = snrPPResult / settingData.calibratePara.snrPara.repeatCount;
            para.LineNoiseResult = snrPPResult;
            para.IpaTestResult /= settingData.calibratePara.snrPara.repeatCount;
            para.IpaTestResult /= IfgRefData;
            para.IpaTestResult *= 100;
            para.EngTestResult /= settingData.calibratePara.snrPara.repeatCount;
            para.EngTestResult *= 100;
            para.EngTestResult /= Math.Abs(interRef);

            devMax = devMax / settingData.calibratePara.snrPara.repeatCount;
            para.TestResult = devMax;//变成百分数
            //更新界面测量值
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetPPDevValueDelegate(SetPPDevValue), snrPanel, Convert.ToDouble(snrPPResult.ToString("F2")), Convert.ToDouble(devMax.ToString("F2")),Convert.ToDouble(para.IpaTestResult.ToString("F2")),Convert.ToDouble(para.EngTestResult.ToString("F2")));

            //信噪比验证结果
            //波数准确度验证结果
            if (devMax > 0 && devMax < settingData.calibratePara.snrPara.DevThresold && snrPPResult > settingData.calibratePara.snrPara.PPThresold)//snrRMSResult > settingData.calibratePara.snrPara.RMSThresold
            {
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "LineNoiseAndDevTest calibration ok");
                //   Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), snrPanel, true, "OK_16.png", -1, 1);
            }
            else
            {
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "LineNoiseAndDevTest calibration fail");
                //  Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), snrPanel, false, "Error_16.png", -1, 1);
            }
            return true;
        }

        private bool LineSlopeTest()
        {


            string spectrumFile1 = null, spectrumFile2 = null;
            bool jud = true;//判断测量值是否都满足限制条件

            Common.SettingFile.Calibrate_Parameter.OQTest.LineSlopeTest para = settingData.calibratePara.lineSlopeTestPara;
            if(CurrentTimePath=="")
                CurrentTimePath = DateTime.Now.ToString("HH_mm_ss");
            para.SpectrumPath = string.Empty;
            string dataPath = Path.Combine(startPath, "Data", "IT_" + SettingData.settingData.runing_para.serialNo + "\\OQ\\" + DateTime.Now.ToString("yyyy_MM_dd") + "\\" + CurrentTimePath + "\\LineSlopeTest");
            string iniFileName = Path.Combine(startPath, para.iniFile);
            if (settingData.runing_para.isDebug)
            {
                //spectrumFile = Path.Combine(startPath, debugBgFile);
                Thread.Sleep(1000);
            }
            else
            {
                //从仪器中采集背景光谱
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);
                spectrumFile1 = Path.Combine(dataPath, "LineSlopeTest_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");

                Ai.Hong.CommonMethod.AddToLogFile(logfile, "start scan LineSlopeTest spectrum:" + spectrumFile1 + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
                int scanCount = GetScanCountFromIni(iniFileName, true);
                //复原转轮
                if (!Common.VspecInstrument.MoveWheel(0))
                {
                    throw new Exception("复原转轮失败！");
                }
                //开始波数准确度检测
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), lineSlopeTest, null, null, 1, 0);
                
                spectrumFile1 = Common.VspecInstrument.ScanBackground(iniFileName, scanCount, spectrumFile1, Common.VspecInstrument.IsMoveFlagBack());
                //   spectrumFile1 = @"E:\Project\NirIdentifier\bin\Release\Data\Calibration\OQ\OQ2015_03_16\LineSlopeTest2015-03-16 16-13-16_sbm.spc";
                if (spectrumFile1 == null)
                {
                    ITError = "扫描光谱错误:" + Common.VspecInstrument.GetError();
                    return false;
                }

                //从仪器中采集样品光谱
                dataPath = Path.Combine(startPath, "Data", "IT\\OQ\\LineSlopeTest", DateTime.Now.ToString("yyyy_MM_dd"));
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);
                spectrumFile2 = Path.Combine(dataPath, "LineSlopeTest_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");

                Ai.Hong.CommonMethod.AddToLogFile(logfile, "start scan LineSlopeTest spectrum:" + spectrumFile2 + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
                scanCount = GetScanCountFromIni(iniFileName, true);
                spectrumFile2 = Common.VspecInstrument.ScanSample(iniFileName, scanCount, spectrumFile2, Common.VspecInstrument.IsMoveFlagBack());
                //  spectrumFile2 = @"E:\Project\NirIdentifier\bin\Release\Data\Calibration\OQ\OQ2015_03_16\LineSlopeTest2015-03-16 16-13-28_sbm.spc";
                if (spectrumFile2 == null)
                {
                    ITError = "扫描光谱错误:" + Common.VspecInstrument.GetError();
                    return false;
                }

                Ai.Hong.CommonMethod.AddToLogFile(logfile, "stop scan LineSlopeTest spectrum:" + spectrumFile2 + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
            }



            //读取背景光谱
            Ai.Hong.CommonLibrary.SpecFileFormatDouble specData1 = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
            Ai.Hong.CommonLibrary.SpecFileFormatDouble specData2 = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();

            if (specData1.ReadFile(spectrumFile1) == false)
            {
                ITError = "读取光谱错误:" + spectrumFile1 + " " + specData1.ErrorString;
                return false;
            }

            if (specData2.ReadFile(spectrumFile2) == false)
            {
                ITError = "读取光谱错误:" + spectrumFile2 + " " + specData2.ErrorString;
                return false;
            }


            //计算样品1和样品2比值,得到100%线
            for (int index = 0; index < specData1.Parameter.dataCount; index++)
            {
                if (specData1.YDatas[index] == 0)
                    specData1.YDatas[index] = 100;
                else
                    specData1.YDatas[index] = Math.Abs(specData2.YDatas[index] / specData1.YDatas[index]) * 100;
            }

            //保存透射光谱
            if (settingData.runing_para.isDebug)
                spectrumFile1 = spectrumFile1.Replace(".spc", "_tr.spc");
            else
            {
                // spectrumFile1 = spectrumFile1.Replace("_sbm.spc", "_tr.spc");
                spectrumFile1 = Path.Combine(dataPath, "LineSlopeTest_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + "_tr.spc");
            }
            float[] trData = new float[specData1.YDatas.Length];
            for (int index = 0; index < trData.Length; index++)
                trData[index] = (float)specData1.YDatas[index];
            para.SpectrumPath = spectrumFile1;
            Ai.Hong.CommonLibrary.SPCFile.SaveFile(spectrumFile1, trData, specData1.Parameter);

            for (int i = 0; i < para.data1.Count; i++)
            {
                string[] temp = para.data1[i].WaveNumRange.Split('-');
                double firstX = Convert.ToDouble(temp[0]);
                double lastX = Convert.ToDouble(temp[1]);
                string[] temp1 = para.data1[i].lineLimit.Split('-');
                double min = Convert.ToDouble(temp1[0]);
                double max = Convert.ToDouble(temp1[1]);
                para.data1[i].maxValue = max;
                para.data1[i].minValue = min;
                double findMax = 0, findMin = specData1.YDatas[0];
                int fx = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.FindNearestPosition(specData1.XDatas, 0, specData1.XDatas.Length - 1, firstX);
                int lx = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.FindNearestPosition(specData1.XDatas, 0, specData1.XDatas.Length - 1, lastX);

                //  para.data1[i].Slope = (specData1.YDatas[fx] - specData1.YDatas[lx]) / (specData1.XDatas[fx] - specData1.XDatas[lx]);
                for (int j = fx; j < lx; j++)
                {
                    findMax = specData1.YDatas[j] > findMax ? specData1.YDatas[j] : findMax;
                    findMin = specData1.YDatas[j] < findMin ? specData1.YDatas[j] : findMin;
                }
                para.data1[i].meaMaxValue = Convert.ToDouble(findMax.ToString("F3"));
                para.data1[i].meaMinValue = Convert.ToDouble(findMin.ToString("F3"));

                if (findMin < min)//para.data1[i].Slope < min || para.data1[i].Slope > max)
                {
                    jud = false;
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new UpdateLayoutDelegate(UpdateLayout), i, 2);//最小值不满足限制  标红
                }

                if (findMax > max)
                {
                    jud = false;
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new UpdateLayoutDelegate(UpdateLayout), i, 3);//最大值不满足限制  标红
                }

            }

            //波数准确度验证结果
            if (jud)
            {
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "LineSlopeTest calibration ok");
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetLineSlopeDelegate(SetLineSlopeValue), lineSlopeTest, true);

                //   Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), lineSlopeTest, true, "OK_16.png", -1, 1);
            }
            else
            {
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "LineSlopeTest calibration fail");
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetLineSlopeDelegate(SetLineSlopeValue), lineSlopeTest, false);

                //  Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), lineSlopeTest, true, "Error_16.png", -1, 1);
            }

            return true;
        }

        /// <summary>  
        /// 获取父可视对象中第一个指定类型的子可视对象  
        /// </summary>  
        /// <typeparam name="T">可视对象类型</typeparam>  
        /// <param name="parent">父可视对象</param>  
        /// <returns>第一个指定类型的子可视对象</returns>  
        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
        public delegate void UpdateLayoutDelegate(int i, int j);
        public void UpdateLayout(int i, int j)
        {
            DataGridRow rowContainer = (DataGridRow)lineSlopeTest.dataGrid.ItemContainerGenerator.ContainerFromIndex(i);

            if (rowContainer != null)
            {
                // rowContainer.Background = Brushes.Red;
                System.Windows.Controls.Primitives.DataGridCellsPresenter presenter = GetVisualChild<System.Windows.Controls.Primitives.DataGridCellsPresenter>(rowContainer);
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(j);
                if (cell == null)
                {
                    lineSlopeTest.dataGrid.ScrollIntoView(rowContainer, lineSlopeTest.dataGrid.Columns[j]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(j);
                }
                cell.Background = Brushes.Red;
                //int ii = 0;
                //ii *= ii;
            }
            //DataGridCell dg= (lineSlopeTest.dataGrid.Columns[j].GetCellContent(lineSlopeTest.dataGrid.Items[i]) as DataGridCell);//.Background = Brushes.Red;
            //dg.Background = Brushes.Red;
        }

        private bool TransRepTest()
        {


            Common.SettingFile.Calibrate_Parameter.OQTest.TransRepTest para = SettingData.settingData.calibratePara.transRepTest;
            para.SpectrumPath.Clear();
            string iniFileName = Path.Combine(startPath, para.iniFile);
            if(CurrentTimePath=="")
                CurrentTimePath = DateTime.Now.ToString("HH_mm_ss");

            string dataPath = Path.Combine(startPath, "Data", "IT_" + SettingData.settingData.runing_para.serialNo + "\\OQ\\" + DateTime.Now.ToString("yyyy_MM_dd") + "\\" + CurrentTimePath + "\\TransRepTest");
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            snrRMSResult = snrPPResult = 0;

            //从仪器中采集背景光谱  即参考光谱
            int scanCount = GetScanCountFromIni(iniFileName, true);
            string backSpc = null;
            backSpc = Path.Combine(dataPath, "TransRepTest_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");
            Ai.Hong.CommonMethod.AddToLogFile(logfile, "scan TransRepTest spectrum:" + backSpc + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
            //复原转轮
            if (!Common.VspecInstrument.MoveWheel(0))
            {
                throw new Exception("复原转轮失败！");
            }
            //开始检测
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), tranRepPanel, null, null, 1, 0);
           
            backSpc = Common.VspecInstrument.ScanBackground(iniFileName, scanCount, backSpc, Common.VspecInstrument.IsMoveFlagBack());//, "_sbm");
            if (backSpc == null)
            {
                ITError = "扫描光谱错误:" + Common.VspecInstrument.GetError();
                return false;
            }

            //读取背景光谱
            Ai.Hong.CommonLibrary.SpecFileFormatDouble backData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();

            if (backData.ReadFile(backSpc) == false)
            {
                ITError = "读取光谱错误:" + backSpc + " " + backData.ErrorString;
                return false;
            }

            for (int i = 0; i < para.repeatCount; i++)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), tranRepPanel, null, null, settingData.calibratePara.snrPara.repeatCount, i);

                string sampleSpc = null;
                if (settingData.runing_para.isDebug)
                {
                    //samplefile1 = Path.Combine(startPath, debugSampleFile + (i % 10).ToString() + ".spc");     //只有10个文件
                    //samplefile2 = Path.Combine(startPath, debugSampleFile + ((i + 1) % 10).ToString() + ".spc");     //只有10个文件
                    Thread.Sleep(1000);
                }
                else
                {
                    ////从仪器中采集样品光谱2
                    //if (samplefile1 != null)
                    //{
                    sampleSpc = Path.Combine(dataPath, "TransRepTest_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");
                    Ai.Hong.CommonMethod.AddToLogFile(logfile, "scan TransRepTest spectrum:" + sampleSpc + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
                   
                    sampleSpc = Common.VspecInstrument.ScanSample(iniFileName, scanCount, sampleSpc, Common.VspecInstrument.IsMoveFlagBack());//, "_sbm");
                    //}
                    if (sampleSpc == null)
                    {
                        ITError = "扫描光谱错误:" + Common.VspecInstrument.GetError();
                        return false;
                    }
                }

                //读取样品1和样品2数据
                Ai.Hong.CommonLibrary.SpecFileFormatDouble sampleData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();


                if (sampleData.ReadFile(sampleSpc) == false)
                {
                    ITError = "读取光谱错误:" + sampleSpc + " " + sampleData.ErrorString;
                    return false;
                }

                //计算样品1和样品2比值,得到100%线
                for (int index = 0; index < sampleData.Parameter.dataCount; index++)
                {
                    if (sampleData.YDatas[index] == 0)
                        sampleData.YDatas[index] = 100;
                    else
                        sampleData.YDatas[index] = Math.Abs(sampleData.YDatas[index] / backData.YDatas[index]) * 100;
                }

                //保存透射光谱
                if (settingData.runing_para.isDebug)
                    para.SpectrumPath.Add(sampleSpc.Replace(".spc", "_tr.spc"));
                else
                {
                    //para.SpectrumPath.Add(sampleSpc.Replace("_sbm.spc", "_tr.spc"));
                    para.SpectrumPath.Add(Path.Combine(dataPath, "TransRepTest_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + "_tr.spc"));
                }
                float[] trData = new float[sampleData.YDatas.Length];
                for (int index = 0; index < trData.Length; index++)
                    trData[index] = (float)sampleData.YDatas[index];
                Ai.Hong.CommonLibrary.SPCFile.SaveFile(para.SpectrumPath[i], trData, sampleData.Parameter);

                int firtX = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.FindNearestPosition(sampleData.XDatas, 0, sampleData.XDatas.Length - 1, para.firstX);
                int lastX = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.FindNearestPosition(sampleData.XDatas, 0, sampleData.XDatas.Length - 1, para.lastX);
                double temp = 0;
                for (int j = firtX; j <= lastX; j++)
                {
                    temp = Math.Abs(100 - sampleData.YDatas[j]) > temp ? Math.Abs(100 - sampleData.YDatas[j]) : temp;
                }
                // double temp = Math.Abs(sampleData.YDatas[firtX] - sampleData.YDatas[lastX]);
                devTransRepTest = temp > devTransRepTest ? temp : devTransRepTest;

                Ai.Hong.CommonMethod.AddToLogFile(logfile, "TransRepTest" + i + "=" + devTransRepTest);
            }

            para.TestResult = devTransRepTest;

            //更新界面测量值
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetEachValueDelegate(SetEachValue), tranRepPanel, Convert.ToDouble(para.TestResult.ToString("F2")), false);

            //波数准确度验证结果
            if (devTransRepTest < settingData.calibratePara.transRepTest.transRepThresold)//snrRMSResult > settingData.calibratePara.snrPara.RMSThresold
            {
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "Transmittance Reproducibility calibration ok");
                //  Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), tranRepPanel, true, "OK_16.png", -1, 1);
            }
            else
            {
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "Transmittance Reproducibility calibration fail");
                // Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), tranRepPanel, false, "Error_16.png", -1, 1);
            }
            return true;
        }

        private bool WaveNumRepTest()
        {


            Common.SettingFile.Calibrate_Parameter.OQTest.WaveNumRepTest para = SettingData.settingData.calibratePara.waveNumRepTestPara;
            para.SpectrumPath.Clear();
            para.DevMax = 0;
            para.MinDev = 0;
            string iniFileName = Path.Combine(startPath, para.iniFile);
            if(CurrentTimePath=="")
                CurrentTimePath = DateTime.Now.ToString("HH_mm_ss");

            string dataPath = Path.Combine(startPath, "Data", "IT_" + SettingData.settingData.runing_para.serialNo + "\\OQ\\" + DateTime.Now.ToString("yyyy_MM_dd") + "\\" + CurrentTimePath + "\\WaveNumRepTest");
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);
            //做六次透射性重复测试
            //for (int j = 0; j < para.repeatCount; j++)
            //{
            //做透射重复性测试

            //从仪器中采集样品光谱1

            for (int i = 0; i < para.repeatCount; i++)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), waveNumRepPanel, null, null, settingData.calibratePara.snrPara.repeatCount, i);

                //string sampleSpc = null;
                string backSpc = null;
                if (settingData.runing_para.isDebug)
                {
                    //samplefile1 = Path.Combine(startPath, debugSampleFile + (i % 10).ToString() + ".spc");     //只有10个文件
                    //samplefile2 = Path.Combine(startPath, debugSampleFile + ((i + 1) % 10).ToString() + ".spc");     //只有10个文件
                    Thread.Sleep(1000);
                }
                else
                {
                    int scanCount = GetScanCountFromIni(iniFileName, true);

                    backSpc = Path.Combine(dataPath, "WaveNumRepTest_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");
                    Ai.Hong.CommonMethod.AddToLogFile(logfile, "scan WaveNumRepTest spectrum:" + backSpc + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
                    //复原转轮
                    if (!Common.VspecInstrument.MoveWheel(0))
                    {
                        throw new Exception("复原转轮失败！");
                    }
                    //开始检测
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), waveNumRepPanel, null, null, 1, 1);
                    
                    backSpc = Common.VspecInstrument.ScanBackground(iniFileName, scanCount, backSpc, Common.VspecInstrument.IsMoveFlagBack());//, "_sbm");
                    if (backSpc == null)
                    {
                        ITError = "扫描光谱错误:" + Common.VspecInstrument.GetError();
                        return false;
                    }

                    ////从仪器中采集样品光谱2
                    //if (samplefile1 != null)
                    //{
                    //sampleSpc = Path.Combine(dataPath, "WaveNumRepTest_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");
                    //Ai.Hong.CommonMethod.AddToLogFile(logfile, "scan WaveNumRepTest spectrum:" + sampleSpc + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
                    //sampleSpc = Common.VspecInstrument.ScanSample(iniFileName, scanCount, sampleSpc);//, "_sbm");
                    ////}
                    //if (sampleSpc == null)
                    //{
                    //    ITError = "扫描光谱错误:" + Common.VspecInstrument.GetError();
                    //    return false;
                    //}
                }


                Ai.Hong.CommonLibrary.SpecFileFormatDouble backData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
                if (backData.ReadFile(backSpc) == false)
                {
                    ITError = "读取光谱错误:" + backSpc + " " + backData.ErrorString;
                    return false;
                }
                para.SpectrumPath.Add(backSpc);


                //标定峰位
                double target = 0, newyvalue;

                target = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(backData.XDatas, backData.YDatas, para.TargetPeak, 4, false, out newyvalue);

                para.DevMax = target > para.DevMax ? target : para.DevMax;
                if (para.MinDev == 0)
                    para.MinDev = target;
                para.MinDev = target < para.MinDev ? target : para.MinDev;
                // devWaveNumRepTest = Math.Abs(para.DevMax - para.MinDev);
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "WaveNumRepTest" + i + "*" + devWaveNumRepTest);
            }

            para.TestResult = Math.Abs(para.DevMax - para.MinDev);// devWaveNumRepTest;

            //更新界面测量值
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetEachValueDelegate(SetEachValue), waveNumRepPanel, Convert.ToDouble(para.TestResult.ToString("F2")), false);

            //波数准确度重复性验证结果
            if (devWaveNumRepTest < settingData.calibratePara.waveNumRepTestPara.transRepThresold)//snrRMSResult > settingData.calibratePara.snrPara.RMSThresold
            {
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "Wavenumber Reproducibility calibration ok");
                //    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), waveNumRepPanel, true, "OK_16.png", -1, 1);
            }
            else
            {
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "Wavenumber Reproducibility calibration fail");
                //    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), waveNumRepPanel, false, "Error_16.png", -1, 1);
            }
            return true;
        }

        private bool EnergyDistributionTest()
        {
            string spectrumFile = null;

            Common.SettingFile.Calibrate_Parameter.OQTest.EnergyDistributionTest para = settingData.calibratePara.energyDisPara;
            para.SpectrumPath = string.Empty;
            para.TestResult = 0;
            if(CurrentTimePath=="")
            {
                CurrentTimePath = DateTime.Now.ToString("HH_mm_ss");
            }
            string iniFileName = Path.Combine(startPath, para.iniFile);
            if (settingData.runing_para.isDebug)
            {
                //spectrumFile = Path.Combine(startPath, debugBgFile);
                Thread.Sleep(1000);
            }
            else
            {
                //从仪器中采集背景单通道光谱
                string dataPath = Path.Combine(startPath, "Data", "IT_" + SettingData.settingData.runing_para.serialNo + "\\OQ\\" + DateTime.Now.ToString("yyyy_MM_dd") + "\\" + CurrentTimePath + "\\EnergyDistributionTest");
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);
                spectrumFile = Path.Combine(dataPath, "EnergyDistributionTest_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");

                Ai.Hong.CommonMethod.AddToLogFile(logfile, "start scan EnergyDistributionTest spectrum:" + spectrumFile + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
                int scanCount = GetScanCountFromIni(iniFileName, true);
                //复原转轮
                if (!Common.VspecInstrument.MoveWheel(0))
                {
                    throw new Exception("复原转轮失败！");
                }
                //开始波数准确度检测
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), energyDisPanel, null, null, 1, 0);
                
                spectrumFile = Common.VspecInstrument.ScanBackground(iniFileName, scanCount, spectrumFile, Common.VspecInstrument.IsMoveFlagBack());
                if (spectrumFile == null)
                {
                    ITError = "扫描光谱错误:" + Common.VspecInstrument.GetError();
                    return false;
                }

                Ai.Hong.CommonMethod.AddToLogFile(logfile, "stop scan EnergyDistributionTest spectrum:" + spectrumFile + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
            }

            para.SpectrumPath = spectrumFile;

            //读取背景光谱
            Ai.Hong.CommonLibrary.SpecFileFormatDouble specData1 = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();

            if (specData1.ReadFile(spectrumFile) == false)
            {
                ITError = "读取光谱错误:" + spectrumFile + " " + specData1.ErrorString;
                return false;
            }

            //找到最大Y值
            double tempMax = 0;
            for (int i = 0; i < specData1.YDatas.Length; i++)
            {
                tempMax = specData1.YDatas[i] > tempMax ? specData1.YDatas[i] : tempMax;
            }
            int x = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.FindNearestPosition(specData1.XDatas, 0, specData1.XDatas.Length - 1, para.TargetX);

            //计算能量百分比
            resultEnergyDis = specData1.YDatas[x] / tempMax;
            para.TestResult = Convert.ToDouble(resultEnergyDis.ToString("F2"));

            //更新界面测量值
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetEachValueDelegate(SetEachValue), energyDisPanel, Convert.ToDouble(resultEnergyDis.ToString("F2")), true);

            //波数准确度验证结果
            if (resultEnergyDis > para.engerDisThresold)
            {
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "EnergyDistributionTest calibration ok");
                //  Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), energyDisPanel, true, "Error_16.png", -1, 1);
            }

            return true;
        }

        public bool AccuracyTestOQ()
        {

            string spectrumFile = null;

            Common.SettingFile.Calibrate_Parameter.OQTest.AccuracyTestOQ para = settingData.calibratePara.accuracyTestOQ;
            para.SpectrumPath = "";
            para.PolySpectrumPath = "";
            para.TestResult = 0;
            para.PolyTestResult = 0;
            string iniFileName = Path.Combine(startPath, para.iniFile);
            if(CurrentTimePath=="")
            {
                CurrentTimePath = DateTime.Now.ToString("HH_mm_ss");

            }
            string dataPath = Path.Combine(startPath, "Data", "IT_" + SettingData.settingData.runing_para.serialNo + "\\OQ\\" + DateTime.Now.ToString("yyyy_MM_dd") + "\\" + CurrentTimePath + "\\AccuracyTestOQ");
            if (settingData.runing_para.isDebug)
            {
                spectrumFile = Path.Combine(startPath, debugBgFile);
                Thread.Sleep(1000);
            }
            else
            {
                //从仪器中采集背景光谱
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);
                spectrumFile = Path.Combine(dataPath, "AccuracyTestOQ_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");

                Ai.Hong.CommonMethod.AddToLogFile(logfile, "start scan AccuracyTestOQ spectrum:" + spectrumFile + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
                int scanCount = GetScanCountFromIni(iniFileName, true);
                //复原转轮
                if (!Common.VspecInstrument.MoveWheel(0))
                {
                    throw new Exception("复原转轮失败！");
                }
                //开始波数准确度检测
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), waveNumAccPanel, null, null, 1, 0);
                
                spectrumFile = Common.VspecInstrument.ScanBackground(iniFileName, scanCount, spectrumFile, Common.VspecInstrument.IsMoveFlagBack());
                if (spectrumFile == null)
                {
                    ITError = "扫描光谱错误:" + Common.VspecInstrument.GetError();
                    return false;
                }
                Ai.Hong.CommonMethod.AddToLogFile(logfile, "stop scan AccuracyTestOQ spectrum:" + spectrumFile + " at: " + DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
            }

            para.SpectrumPath = spectrumFile;

            //读取背景光谱
            Ai.Hong.CommonLibrary.SpecFileFormatDouble specData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
            if (specData.ReadFile(spectrumFile) == false)
            {
                ITError = "读取光谱错误:" + spectrumFile + " " + specData.ErrorString;
                return false;
            }

            double newYValue;
            double targetPeak = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, para.targetPeak, 4, false, out newYValue);
            double peak1 = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, para.verfyPeak1, 4, false, out newYValue);
            double peak2 = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, para.verfyPeak2, 4, false, out newYValue);
            Ai.Hong.CommonMethod.AddToLogFile(logfile, "target peak=" + targetPeak.ToString("F4") + " verify peak1=" + peak1.ToString("F4") + " verify peak2=" + peak2.ToString("F4"));
            para.ActualBand = targetPeak;
            //to make sure the correct peak is selected, the program should find two peaks in the range of 7230-7245cm-1. 
            //One is at position about 7232.29cm-1 and the other is at 7242.77cm-1.
            //peak1和peak2都在区域内，peak1小于peak2，并且peak1和peak2的差值要在一定范围内
            if (peak1 < para.firstX || peak1 > para.lastX || peak2 < para.firstX || peak2 > para.lastX || peak2 < peak1)
            {
                ITError = para.firstX.ToString() + " - " + para.lastX.ToString() + " 范围内找不到标定峰位";
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), waveNumAccPanel, false, "Error_16.png", -1, 1);
                throw new Exception(ITError);
            }

            //超出阈值，需要重置激光波数
            para.TestResult = Math.Abs(targetPeak - para.targetPeak);

            //更新界面测量值
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetAccuracyDelegate(SetAccuracyValue), waveNumAccPanel, Convert.ToDouble(para.TestResult.ToString("F2")), -1);


            //开始聚苯乙烯测试
            string PolyBackPath1 = "", PolySamplePath = "";
            if (settingData.runing_para.isDebug)
            {
                spectrumFile = Path.Combine(startPath, debugBgFile);
                Thread.Sleep(1000);
            }
            else
            {
                //测试背景
             //   string dataPath = Path.Combine(startPath, "Data", "IT\\OQ\\Accuracy", "OQ" + DateTime.Now.ToString("yyyy_MM_dd"));
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);
                //spectrumFile = Path.Combine(dataPath, "AccuracyOQ_Polystyrene_Background" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");
                int scanCount = GetScanCountFromIni(iniFileName, true);
                //PolyBackPath = Common.VspecInstrument.ScanBackground(iniFileName, scanCount, spectrumFile);
                //if (spectrumFile == null)
                //{
                //    ITError = "扫描光谱错误:" + Common.VspecInstrument.GetError();
                //    return false;
                //}
                //转轮转动聚苯乙烯位置
                if (!Common.VspecInstrument.MoveWheel(2))
                {
                    throw new Exception("OQ波数精度-转轮转动聚苯乙烯位置失败！");
                }
                spectrumFile = Path.Combine(dataPath, "AccuracyOQ_Polystyrene_Sample" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".spc");
                scanCount = GetScanCountFromIni(iniFileName, true);
                
                PolySamplePath = Common.VspecInstrument.ScanSample(iniFileName, scanCount, spectrumFile, Common.VspecInstrument.IsMoveFlagBack());
                if (spectrumFile == null)
                {
                    ITError = "扫描光谱错误:" + Common.VspecInstrument.GetError();
                    return false;
                }

                //复原转轮
                if (!VspecInstrument.MoveWheel(0))
                {
                    throw new Exception("复原转轮失败！");
                }
            }
            //读取背景光谱
           // Ai.Hong.CommonLibrary.SpecFileFormatDouble PolyBack = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
            Ai.Hong.CommonLibrary.SpecFileFormatDouble PolySample = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();

            //if (specData.ReadFile(spectrumFile) == false)
            //{
            //    ITError = "读取光谱错误:" + spectrumFile + " " + specData.ErrorString;
            //    return false;
            //}
            if (PolySample.ReadFile(PolySamplePath) == false)
            {
                ITError = "读取光谱错误:" + spectrumFile + " " + specData.ErrorString;
                return false;
            }
            //计算样品1和样品2比值,得到100%线
            for (int index = 0; index < specData.Parameter.dataCount; index++)
            {
                if (specData.YDatas[index] == 0)
                    specData.YDatas[index] = 1;//00;
                else
                    specData.YDatas[index] = Math.Abs(PolySample.YDatas[index] / specData.YDatas[index]);// *100;
            }
            //得到吸收光谱
            for (int i = 0; i < specData.YDatas.Count(); i++)
            {
                specData.YDatas[i] = Math.Log10(1 / specData.YDatas[i]);
            }
            //保存透射光谱
            string tr = Path.Combine(dataPath, "AccuracyOQ_Polystyrene_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + "_abs.spc");
            para.PolySpectrumPath = tr;
            float[] trData = new float[specData.YDatas.Length];
            for (int index = 0; index < trData.Length; index++)
                trData[index] = (float)specData.YDatas[index];
            Ai.Hong.CommonLibrary.SPCFile.SaveFile(para.PolySpectrumPath, trData, specData.Parameter);



            double newPolyYValue;
            double PolyTargetPeak = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, para.PolyTargetX, 4, true, out newPolyYValue);
            //double peak1 = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, para.verfyPeak1, 4, false, out newYValue);
            //double peak2 = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.PickPeak(specData.XDatas, specData.YDatas, para.verfyPeak2, 4, false, out newYValue);
            //Ai.Hong.CommonMethod.AddToLogFile(logfile, "target peak=" + targetPeak.ToString("F4") + " verify peak1=" + peak1.ToString("F4") + " verify peak2=" + peak2.ToString("F4"));


            //计算温度偏差
            string paraString = VspecInstrument.ReadSensors();
            double devTemperature = 0;
            if (paraString != null)
            {
                string[] tempp = paraString.Split('}');
                string[] leasertemp = tempp[1].Split(':');
                //  leasertemp[1]=leasertemp[2].Replace('\"',' ');
                if (VspecInstrument.IsMoveFlagBack())
                {
                    PolyTargetPeak = (4571 * PolyTargetPeak) / (4571.575 - 0.0205 * Convert.ToDouble(leasertemp[2]));
                }
                else
                {
                    devTemperature = Convert.ToDouble(leasertemp[2]) * 0.0107 - 0.7;
                    //加上温度偏差校正
                    PolyTargetPeak += devTemperature;
                }
            }
            else
            {
                MessageBox.Show("读取仪器温度失败！");
                return false;
            }
            para.PolyActualBand = PolyTargetPeak;
            //超出阈值，需要重置激光波数
            para.PolyTestResult = Math.Abs(PolyTargetPeak - para.PolyTargetX);

            //更新界面测量值
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetAccuracyDelegate(SetAccuracyValue), waveNumAccPanel, Convert.ToDouble(para.TestResult.ToString("F2")), Convert.ToDouble(para.PolyTestResult.ToString("F4")));

            ////波数准确度验证结果
            //if (accuracyResult < settingData.calibratePara.accuracyPara.thresold)
            //{
            //    Ai.Hong.CommonMethod.AddToLogFile(logfile, "OQ accuracy calibration ok");
            // //   Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), waveNumAccPanel, true, "OK_16.png", -1, 1);
            //}
            //else
            //{
            //    Ai.Hong.CommonMethod.AddToLogFile(logfile, "OQ accuracy calibration fail");
            // //   Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetITStatusDeletage(SetITStatus), waveNumAccPanel, false, "Error_16.png", -1, 1);
            //}
            return true;
        }

        private void OQMainThread()
        {
            if (settingData.calibratePara.resolutionTestPara.IsTest)
            {
                try
                {
                    ITError = null;

                    //分辨率测试测试
                    if (ResolutionTest() == false)
                        throw new Exception("分辨率测试测试:" + ITError);
                    if (userCancel)
                        throw new Exception("用户取消");
                }
                catch (Exception ex)
                {
                    //if (!userCancel)
                    Ai.Hong.CommonMethod.ErrorMsgBox(ex.Message);
                }
            }
            if (settingData.calibratePara.lineNoiseTest.IsTest)
            {
                try
                {

                    //100%线噪声测试
                    if (LineNoiseTestOQ() == false)
                        throw new Exception("100%线噪声测试:" + ITError);
                    if (userCancel)
                        throw new Exception("用户取消");
                }
                catch (Exception ex)
                {
                    //if (!userCancel)
                    Ai.Hong.CommonMethod.ErrorMsgBox(ex.Message);
                }
            }
            if (settingData.calibratePara.energyDisPara.IsTest)
            {
                try
                {

                    //能量分布测试
                    if (EnergyDistributionTest() == false)
                        throw new Exception("能量分布测试:" + ITError);
                    if (userCancel)
                        throw new Exception("用户取消");
                }
                catch (Exception ex)
                {
                    //if (!userCancel)
                    Ai.Hong.CommonMethod.ErrorMsgBox(ex.Message);
                }
            }
            if (settingData.calibratePara.lineSlopeTestPara.IsTest)
            {
                try
                {

                    //100%线斜率测试
                    if (LineSlopeTest() == false)
                        throw new Exception("100%线斜率测试:" + ITError);
                    if (userCancel)
                        throw new Exception("用户取消");

                }
                catch (Exception ex)
                {
                    //if (!userCancel)
                    Ai.Hong.CommonMethod.ErrorMsgBox(ex.Message);
                }
            }
            if (settingData.calibratePara.transRepTest.IsTest)
            {
                try
                {
                    //透射重复性测试
                    if (TransRepTest() == false)
                        throw new Exception("透射重复性测试:" + ITError);
                    if (userCancel)
                        throw new Exception("用户取消");
                }
                catch (Exception ex)
                {
                    //if (!userCancel)
                    Ai.Hong.CommonMethod.ErrorMsgBox(ex.Message);
                }
            }
            if (settingData.calibratePara.accuracyTestOQ.IsTest)
            {
                try
                {

                    //波数精度测试测试
                    if (AccuracyTestOQ() == false)
                        throw new Exception("波数精度测试测试:" + ITError);
                    if (userCancel)
                        throw new Exception("用户取消");

                }
                catch (Exception ex)
                {
                    //if (!userCancel)
                    Ai.Hong.CommonMethod.ErrorMsgBox(ex.Message);
                }
            }
            if (settingData.calibratePara.waveNumRepTestPara.IsTest)
            {
                try
                {
                    //波数重复性测试
                    if (WaveNumRepTest() == false)
                        throw new Exception("波数重复性测试:" + ITError);
                    if (userCancel)
                        throw new Exception("用户取消");
                }
                catch (Exception ex)
                {
                    //if (!userCancel)
                    Ai.Hong.CommonMethod.ErrorMsgBox(ex.Message);
                }
            }
            try
            {
                //存储检测结果到数据库
                OQData pdata = new OQData();
                pdata.resolution = settingData.calibratePara.resolutionTestPara.TestResult;
                pdata.lineNoise = settingData.calibratePara.lineNoiseTest.TestResult;
                pdata.energyDis = settingData.calibratePara.energyDisPara.TestResult;
                string temp = "";
                foreach (SettingFile.Calibrate_Parameter.OQTest.LineSlopeTest.data ss in settingData.calibratePara.lineSlopeTestPara.data1)
                {
                    pdata.lineSlope += ss.meaMaxValue.ToString() + ",";
                    temp += ss.meaMinValue.ToString() + ",";
                }
                pdata.lineSlope = pdata.lineSlope.Substring(0, pdata.lineSlope.Length - 1);
                temp = temp.Substring(0, temp.Length - 1);
                pdata.lineSlope += "+" + temp;
                pdata.transRep = settingData.calibratePara.transRepTest.TestResult;
                pdata.waveNum = settingData.calibratePara.accuracyTestOQ.TestResult;
                pdata.waveNumRep = settingData.calibratePara.waveNumRepTestPara.TestResult;
                SettingData.dataBase.InsertPQOQ(pdata);
            }
            catch (Exception ex)
            {
                //if (!userCancel)
                Ai.Hong.CommonMethod.ErrorMsgBox(ex.Message);
            }
            btnOQPrintReport_Click(null, null);
        }

        private void btnOQStart_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnect)
            {
                MessageBox.Show("仪器未连接！", "提示");
                return;
            }
            oq.IsEnabled = false;
            spectrumChart.Visibility = Visibility.Visible;
            mainFGrid.Visibility = Visibility.Collapsed;
            OQThread = new Thread(new ThreadStart(OQMainThread));
            //     ITTread.Name = "ITTread";
            OQThread.IsBackground = false;
            OQThread.SetApartmentState(ApartmentState.STA);
            OQThread.Start();
            oq.IsEnabled = true;
        }
        System.Data.DataTable dt = new System.Data.DataTable();

        private void SaveToImage(FrameworkElement ui, string fileName)
        {

            System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create);
            System.Windows.Media.Imaging.RenderTargetBitmap bmp = new System.Windows.Media.Imaging.RenderTargetBitmap((int)ui.Width, (int)ui.Height, 96d, 96d,
            PixelFormats.Pbgra32);
            bmp.Render(ui);
            System.Windows.Media.Imaging.BitmapEncoder encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmp));
            encoder.Save(fs);
            fs.Close();
        }


        Stream GetImageFromControl(DataGrid gd)
        {
            MemoryStream ms = null;

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                VisualBrush brush = new VisualBrush(gd) { Stretch = Stretch.None };

                context.DrawRectangle(brush, null, new Rect(0, 0, gd.Width, gd.Height));
                context.Close();
            }

            //dpi可以自己设定   // 获取dpi方法：PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)gd.Width, (int)gd.Height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);

            PngBitmapEncoder encode = new PngBitmapEncoder();
            encode.Frames.Add(BitmapFrame.Create(bitmap));
            ms = new MemoryStream();
            encode.Save(ms);

            return ms;
        }

        private Border OQBaseBorder()
        {
            Border rootBorder = GetRootBorderFromTemplate("OQBase.xaml");

            if (rootBorder == null)
                return null;
            rootBorder.Width = 18 * DPCM;
            rootBorder.Margin = new Thickness(1.5 * DPCM, 1.5 * DPCM, 1.5 * DPCM, 1 * DPCM);

            //自检基本信息
            Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtCompany", settingData.runing_para.unitName);
            Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtOperator", null);
            Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtInstrumentNo", settingData.runing_para.serialNo);
            Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtCalibrateTime", DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
            Ai.Hong.ReportTemplate.FillTextData(rootBorder, "systemType", Common.SettingData.systemType.ToString());

            SettingFile.Calibrate_Parameter para = settingData.calibratePara;

            if(para.resolutionTestPara.IsTest||
                para.lineNoiseTest.IsTest||
                para.energyDisPara.IsTest||
                para.lineSlopeTestPara.IsTest||
                para.transRepTest.IsTest||
                para.accuracyTestOQ.IsTest||
                para.waveNumRepTestPara.IsTest)
            {
                Border result = (rootBorder.FindName("result") as Border);
                result.Visibility = Visibility.Visible;
            }
            //显示分辨率数据
            if (para.resolutionTestPara.IsTest)
            {
                Border re = (rootBorder.FindName("Resolution") as Border);
                re.Visibility = Visibility.Visible;
                Ai.Hong.ReportTemplate.FillTextData(rootBorder, "target", settingData.calibratePara.resolutionTestPara.targetPeak.ToString());
                ShowResultData(rootBorder, "maxRes", "realRes", "imgResolution",
                    para.resolutionTestPara.TestResult, 
                    para.resolutionTestPara.ResolutionThresold, true, "F2",
                    string.IsNullOrEmpty(settingData.calibratePara.resolutionTestPara.SpectrumPath));
            }
            //显示峰峰值噪声结果
            if (para.lineNoiseTest.IsTest)
            {
                Border PPNoise = (rootBorder.FindName("PPNoise") as Border);
                PPNoise.Visibility = Visibility.Visible;
                ShowResultData(rootBorder, "maxNoise", "realNoise", "imgNoise", 
                    para.lineNoiseTest.TestResult,
                    para.lineNoiseTest.PPThresold, true, "F2",
                    para.lineNoiseTest.SpectrumFile.Count==0);//显示线噪比结果
            }
            //显示能量分布结果 
            if (para.energyDisPara.IsTest)
            {
                Border energyDis = (rootBorder.FindName("EnergyDis") as Border);
                energyDis.Visibility = Visibility.Visible;
                ShowResultData(rootBorder, "minEnergy", "maxEnergy", "imgEnergy", 
                    para.energyDisPara.TestResult,
                    para.energyDisPara.engerDisThresold, false, "F2",
                    string.IsNullOrEmpty(para.energyDisPara.SpectrumPath));
            }
            //显示100%线斜率测试结果

            bool IsPassLineSlope = false;
            if (para.lineSlopeTestPara.IsTest)
            {
                IsPassLineSlope = true;
                Border lineSlope = (rootBorder.FindName("LineSlope") as Border);
                lineSlope.Visibility = Visibility.Visible;
                for (int i = 0; i < settingData.calibratePara.lineSlopeTestPara.data1.Count; i++)
                {
                    string[] temp1 = para.lineSlopeTestPara.data1[i].lineLimit.Split('-');
                    double min = Convert.ToDouble(temp1[0]);
                    double max = Convert.ToDouble(temp1[1]);
                    Ai.Hong.ReportTemplate.FillTextData(rootBorder, "waveNumRegion" + i.ToString(), settingData.calibratePara.lineSlopeTestPara.data1[i].WaveNumRange);
                    Ai.Hong.ReportTemplate.FillTextData(rootBorder, "thrsold" + i.ToString(), settingData.calibratePara.lineSlopeTestPara.data1[i].lineLimit);
                    Ai.Hong.ReportTemplate.FillTextData(rootBorder, "realMin" + i.ToString(), settingData.calibratePara.lineSlopeTestPara.data1[i].meaMinValue.ToString("F2"));
                    Ai.Hong.ReportTemplate.FillTextData(rootBorder, "realMax" + i.ToString(), settingData.calibratePara.lineSlopeTestPara.data1[i].meaMaxValue.ToString("F2"));
                    if (para.lineSlopeTestPara.data1[i].meaMinValue < min || para.lineSlopeTestPara.data1[i].meaMinValue > max)
                    {
                        TextBlock tx = rootBorder.FindName("realMin" + i.ToString()) as TextBlock;
                        tx.Foreground = Brushes.Red;// System.Drawing.Color.Yellow;
                        tx.FontWeight = FontWeights.Bold;
                        IsPassLineSlope = false;
                    }
                    if (para.lineSlopeTestPara.data1[i].meaMaxValue > max || para.lineSlopeTestPara.data1[i].meaMaxValue < min)
                    {
                        TextBlock tx = rootBorder.FindName("realMax" + i.ToString()) as TextBlock;
                        tx.Foreground = Brushes.Red;
                        tx.FontWeight = FontWeights.Bold;
                        IsPassLineSlope = false;
                    }
                }
                Image img = rootBorder.FindName("imgLineSlope") as Image;
                Ai.Hong.CommonMethod.SetImageSource(img, imagePath, IsPassLineSlope ? "Calibration_OK.png" : "error_32.png");
            }

            //显示透射重复性测试结果
            if (para.transRepTest.IsTest)
            {
                Border trans = (rootBorder.FindName("TransRep") as Border);
                trans.Visibility = Visibility.Visible;
                Ai.Hong.ReportTemplate.FillTextData(rootBorder, "testRegion", para.transRepTest.lastX.ToString() + " - " + para.transRepTest.firstX.ToString());
                ShowResultData(rootBorder, "maxDev", "realDev", "imgTreRep", 
                    para.transRepTest.TestResult, 
                    para.transRepTest.transRepThresold, true, "F2",
                    para.transRepTest.SpectrumPath.Count==0);
            }
            //显示波数精度测试结果
            if (para.accuracyTestOQ.IsTest)
            {
                Border accNum = (rootBorder.FindName("AccNum") as Border);
                accNum.Visibility = Visibility.Visible;
                //水蒸气
                Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtPeak", para.accuracyTestOQ.targetPeak.ToString());
                Ai.Hong.ReportTemplate.FillTextData(rootBorder, "realPeak", para.accuracyTestOQ.ActualBand.ToString("F2"));
                ShowResultData(rootBorder, "txtAcc", "realAcc", "imgAcc",
                    para.accuracyTestOQ.TestResult, 
                    para.accuracyTestOQ.thresold, true, "F2",
                    string.IsNullOrEmpty(para.accuracyTestOQ.SpectrumPath));
                //聚苯乙烯
                Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtPolyPeak", para.accuracyTestOQ.PolyTargetX.ToString());
                Ai.Hong.ReportTemplate.FillTextData(rootBorder, "realPoly", para.accuracyTestOQ.PolyActualBand.ToString("F2"));
                ShowResultData(rootBorder, "txtPoly", "realPolyDev", "imgAcc", 
                    para.accuracyTestOQ.PolyTestResult, 
                    para.accuracyTestOQ.PolyThresold, true, "F2",
                    string.IsNullOrEmpty(para.accuracyTestOQ.PolySpectrumPath));
            }
            //显示波数重现性测试结果
            if (para.waveNumRepTestPara.IsTest)
            {
                Border waveNum = (rootBorder.FindName("WaveNum") as Border);
                waveNum.Visibility = Visibility.Visible;
                ShowResultData(rootBorder, "maxWaveRepDev", "realmaxWaveDev", "imgWaveRep", 
                    para.waveNumRepTestPara.TestResult,
                    para.waveNumRepTestPara.transRepThresold, true, "F2",
                    para.waveNumRepTestPara.SpectrumPath.Count==0);
            }

            //最终数据
            TextBlock allTxt = rootBorder.FindName("txtAllResult") as TextBlock;
            Image allImg = rootBorder.FindName("imgAllResult") as Image;

            if (allTxt != null && allImg != null)
            {
                if ((!para.resolutionTestPara.IsTest||
                        (para.resolutionTestPara.TestResult < para.resolutionTestPara.ResolutionThresold&&!string.IsNullOrEmpty(para.resolutionTestPara.SpectrumPath)
                        )) &&      //分辨率结果判断
                    (!para.lineNoiseTest.IsTest||
                        (para.lineNoiseTest.TestResult < para.lineNoiseTest.PPThresold&&para.lineNoiseTest.SpectrumFile.Count!=0)
                        ) &&                        //线噪声结果判断
                    (!para.energyDisPara.IsTest||
                        (para.energyDisPara.TestResult > para.energyDisPara.engerDisThresold&&!string.IsNullOrEmpty(para.energyDisPara.SpectrumPath)
                        ) &&                  //能量分布测试结果判断
                    (!para.lineSlopeTestPara.IsTest||
                        (IsPassLineSlope&&!string.IsNullOrEmpty(para.lineSlopeTestPara.SpectrumPath))
                        ) &&                                                                     //斜率测试判断
                    (!para.transRepTest.IsTest||
                        (para.transRepTest.TestResult < para.transRepTest.transRepThresold&&para.transRepTest.SpectrumPath.Count!=0)
                        ) &&                        //透射重复性判断
                    (!para.accuracyTestOQ.IsTest||(
                        (para.accuracyTestOQ.TestResult < para.accuracyTestOQ.thresold&&!string.IsNullOrEmpty(para.accuracyTestOQ.SpectrumPath))
                        ) &&                           //波数精度判断
                        (para.accuracyTestOQ.PolyTestResult < para.accuracyTestOQ.PolyThresold&&!string.IsNullOrEmpty(para.accuracyTestOQ.PolySpectrumPath))
                        ) &&
                    (!para.waveNumRepTestPara.IsTest||
                        (para.waveNumRepTestPara.TestResult < para.waveNumRepTestPara.transRepThresold&&para.waveNumRepTestPara.SpectrumPath.Count!=0)
                        )           //波数重复性判断   
                    ))
                {
                    allTxt.Text = "自检结果 = 通过";
                    allTxt.Foreground = System.Windows.Media.Brushes.Blue;
                    Ai.Hong.CommonMethod.SetImageSource(allImg, imagePath, "Calibration_OK.png");
                }
                else
                {
                    allTxt.Text = "自检结果 = 未通过";
                    allTxt.Foreground = System.Windows.Media.Brushes.Red;
                    Ai.Hong.CommonMethod.SetImageSource(allImg, imagePath, "error_32.png");
                }
            }
            //填充时间
            Ai.Hong.ReportTemplate.FillTextData(rootBorder, "date", System.DateTime.Now.ToString("yyy-MM-dd HH:mm:ss"));
            return rootBorder;
        }

        public Border OQBorder(Border rootBorder, string panelName, List<string> path, string iniFile, double firstX, double lastX)
        {
            //光谱图和数据
            //  Border rootBorder = GetRootBorderFromTemplate("CalibrationReport_All.xaml");
            if (rootBorder == null)
                return null;
            rootBorder.Width = 18 * DPCM;
            rootBorder.Margin = new Thickness(1.5 * DPCM, 1.5 * DPCM, 1.5 * DPCM, 1 * DPCM);
            //显示测量参数
            FillPara(rootBorder, iniFile);

            //画出光谱图
            DrawSpcInRange(rootBorder, panelName, "borderGraphic", path, firstX, lastX, 18, double.MaxValue, 80);

            //给出光谱路径
            Border br = rootBorder.FindName("borderGraphicPath") as Border;

            System.Windows.Controls.Grid gr = new System.Windows.Controls.Grid();

            for (int i = 0; i < path.Count; i++)
            {
                RowDefinition rd = new RowDefinition();
                gr.RowDefinitions.Add(rd);
            }
            for (int i = 0; i < path.Count; i++)
            {
                TextBlock tx = new TextBlock();
                Thickness tk = new Thickness(0, 2, 0, 0);
                tx.Margin = tk;
                tx.FontSize = 11.5;
                tx.Text = path[i];
                tx.TextWrapping = TextWrapping.Wrap;
                tx.TextAlignment = TextAlignment.Left;
                gr.Children.Add(tx);
                System.Windows.Controls.Grid.SetRow(tx, i);
            }
            br.Child = gr;
            // rd.
            // gr.RowDefinitions

            return rootBorder;
        }

        /// <summary>
        /// 创建OQ报告
        /// </summary>
        /// <param name="xps"></param>
        /// <param name="isCreate"></param>
        /// <returns></returns>
        private bool CreateOQReport(string xpsFile, bool isCreateSpec)
        {
            FixedDocument fixedDoc = new FixedDocument();


            //打印基页
            Border baseBorder = OQBaseBorder();
            PageContent baseContent = CreatePageContent(baseBorder);
            fixedDoc.Pages.Add(baseContent);
            if (isCreateSpec)
            {
                Border rootBorder = GetRootBorderFromTemplate("OQCommonReport.xaml");

                Border resolution = Ai.Hong.ReportTemplate.CloneObject(rootBorder);
                Ai.Hong.ReportTemplate.FillTextData(resolution, "headerSpc", "分辨率测试 - 光谱图");
                Ai.Hong.ReportTemplate.FillTextData(resolution, "headerPara", "分辨率测试 - 测量参数");

                Border lineNoise = Ai.Hong.ReportTemplate.CloneObject(rootBorder);
                Ai.Hong.ReportTemplate.FillTextData(lineNoise, "headerSpc", "峰峰值噪声测试 - 光谱图");
                Ai.Hong.ReportTemplate.FillTextData(lineNoise, "headerPara", "峰峰值噪声测试 - 测量参数");

                Border energyDis = Ai.Hong.ReportTemplate.CloneObject(rootBorder);
                Ai.Hong.ReportTemplate.FillTextData(energyDis, "headerSpc", "能量分布测试 - 光谱图");
                Ai.Hong.ReportTemplate.FillTextData(energyDis, "headerPara", "能量分布测试 - 测量参数");

                Border lineSlope = Ai.Hong.ReportTemplate.CloneObject(rootBorder);
                Ai.Hong.ReportTemplate.FillTextData(lineSlope, "headerSpc", "100%线斜率测试 - 光谱图");
                Ai.Hong.ReportTemplate.FillTextData(lineSlope, "headerPara", "100%线斜率测试 - 测量参数");

                Border transRep = Ai.Hong.ReportTemplate.CloneObject(rootBorder);
                Ai.Hong.ReportTemplate.FillTextData(transRep, "headerSpc", "透射重复性测试 - 光谱图");
                Ai.Hong.ReportTemplate.FillTextData(transRep, "headerPara", "透射重复性测试 - 测量参数");

                Border waveNumAcc = Ai.Hong.ReportTemplate.CloneObject(rootBorder);
                Ai.Hong.ReportTemplate.FillTextData(waveNumAcc, "headerSpc", "波数精度测试 Water Vapor - 光谱图");
                Ai.Hong.ReportTemplate.FillTextData(waveNumAcc, "headerPara", "波数精度测试 Water Vapor - 测量参数");

                Border wavePolyAcc = Ai.Hong.ReportTemplate.CloneObject(rootBorder);
                Ai.Hong.ReportTemplate.FillTextData(wavePolyAcc, "headerSpc", "波数精度测试 Polystyrene - 光谱图");
                Ai.Hong.ReportTemplate.FillTextData(wavePolyAcc, "headerPara", "波数精度测试 Polystyrene - 测量参数");

                Border waveNumRep = Ai.Hong.ReportTemplate.CloneObject(rootBorder);
                Ai.Hong.ReportTemplate.FillTextData(waveNumRep, "headerSpc", "波数重复性测试 - 光谱图");
                Ai.Hong.ReportTemplate.FillTextData(waveNumRep, "headerPara", "波数重复性测试 - 测量参数");
                SettingFile.Calibrate_Parameter para = settingData.calibratePara;

                List<string> path = new List<string>();
                path.Add(para.resolutionTestPara.SpectrumPath);
                //显示分辨率测试谱图及测试参数
                resolution = OQBorder(resolution, null, path, para.resolutionTestPara.iniFile, 7244.8, 7344.8);
                //显示线噪声测试谱图及测试参数
                string[] region = GetRegionFromIni(settingData.calibratePara.snrPara.iniFile).Split('+');
                lineNoise = OQBorder(lineNoise, "tr", para.lineNoiseTest.SpectrumFile, para.lineNoiseTest.iniFile, Convert.ToDouble(region[0]), Convert.ToDouble(region[1]));

                path.Clear();
                path.Add(para.energyDisPara.SpectrumPath);
                //显示能量分布测试谱图及测试参数
                region = GetRegionFromIni(para.energyDisPara.iniFile).Split('+');
                energyDis = OQBorder(energyDis, null, path, para.energyDisPara.iniFile, Convert.ToDouble(region[0]), Convert.ToDouble(region[1]));

                path.Clear();
                path.Add(para.lineSlopeTestPara.SpectrumPath);
                //显示线斜率测试谱图及测试参数
                region = GetRegionFromIni(para.lineSlopeTestPara.iniFile).Split('+');
                lineSlope = OQBorder(lineSlope, "tr", path, para.lineSlopeTestPara.iniFile, Convert.ToDouble(region[0]), Convert.ToDouble(region[1]));
                //显示透射重复性测试谱图及测试参数
                region = GetRegionFromIni(para.transRepTest.iniFile).Split('+');
                //for (int i = 0; i < 11; i++)
                //{
                //    path.Add(@"E:\Project\NirIdentifier\bin\Release\Data\Calibration\OQ\新建文件夹\" + i.ToString() + ".spc");
                //}
                //para.transRepTest.SpectrumPath
                transRep = OQBorder(transRep, "tr", para.transRepTest.SpectrumPath, para.transRepTest.iniFile, Convert.ToDouble(region[0]), Convert.ToDouble(region[1]));// para.transRepTest.firstX, para.transRepTest.lastX);

                path.Clear();
                path.Add(para.accuracyTestOQ.SpectrumPath);
                //显示波数精度测试谱图及测试参数
                waveNumAcc = OQBorder(waveNumAcc, null, path, para.accuracyTestOQ.iniFile, 7160, 7210);// para.accuracyTestOQ.firstX, para.accuracyTestOQ.lastX);

                path.Clear();
                path.Add(para.accuracyTestOQ.PolySpectrumPath);
                //显示波数精度测试谱图及测试参数
                wavePolyAcc = OQBorder(wavePolyAcc, null, path, para.accuracyTestOQ.iniFile, 4521, 4621);// para.accuracyTestOQ.firstX, para.accuracyTestOQ.lastX);

                //显示波数重复性测试谱图及测试参数
                region = GetRegionFromIni(para.waveNumRepTestPara.iniFile).Split('+');
                waveNumRep = OQBorder(waveNumRep, null, para.waveNumRepTestPara.SpectrumPath, para.waveNumRepTestPara.iniFile, 7160, 7210);



                PageContent resolutionContent = CreatePageContent(resolution);
                PageContent lineNoiseContent = CreatePageContent(lineNoise);
                PageContent energyDisContent = CreatePageContent(energyDis);
                PageContent lineSlopeContent = CreatePageContent(lineSlope);
                PageContent transRepContent = CreatePageContent(transRep);
                PageContent waveNumAccContent = CreatePageContent(waveNumAcc);
                PageContent wavePolyAccContent = CreatePageContent(wavePolyAcc);
                PageContent waveNumRepContent = CreatePageContent(waveNumRep);

                if (para.resolutionTestPara.IsTest)
                    fixedDoc.Pages.Add(resolutionContent);
                if (para.lineNoiseTest.IsTest)
                    fixedDoc.Pages.Add(lineNoiseContent);
                if (para.energyDisPara.IsTest)
                    fixedDoc.Pages.Add(energyDisContent);
                if (para.lineSlopeTestPara.IsTest)
                    fixedDoc.Pages.Add(lineSlopeContent);
                if (para.transRepTest.IsTest)
                    fixedDoc.Pages.Add(transRepContent);
                if (para.accuracyTestOQ.IsTest)
                {
                    fixedDoc.Pages.Add(waveNumAccContent);
                    fixedDoc.Pages.Add(wavePolyAccContent);
                }
                if (para.waveNumRepTestPara.IsTest)
                    fixedDoc.Pages.Add(waveNumRepContent);
            }
            //预览报告
            //PrintPreView printdlg = new PrintPreView(fixedDoc);
            //printdlg.ShowDialog();

            try
            {
                //保存到XPS文件
                DocumentPaginator paginator = fixedDoc.DocumentPaginator;
                XpsDocument xpsDocument = new XpsDocument(xpsFile, FileAccess.Write);
                System.Windows.Xps.XpsDocumentWriter documentWriter = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                documentWriter.Write(paginator);
                xpsDocument.Close();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

        private void btnOQPrintReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string xpsfile = Path.Combine(startPath, "Report", SettingData.settingData.runing_para.serialNo, "OQ");
                if (!Directory.Exists(xpsfile))
                    Directory.CreateDirectory(xpsfile);

                xpsfile = Path.Combine(xpsfile, "OQ_Report" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".xps");
                if (CreateOQReport(xpsfile, true) == false)
                    throw new Exception("创建测试报告错误");

                System.Diagnostics.Process.Start(xpsfile);       //打开自检报告
            }
            catch (Exception ex)
            {
                // if (!userCancel)
                Ai.Hong.CommonMethod.ErrorMsgBox(ex.Message);
            }
        }

        private MeasureParameter ReadIniFile(MeasureParameter mp, string p)
        {
            string pa = Path.Combine(Environment.CurrentDirectory, p);
            mp.resolution = Ai.Hong.CommonMethod.ReadIniFile(pa, "Collection", "resolution");
            mp.gain = Ai.Hong.CommonMethod.ReadIniFile(pa, "Collection", "gain");
            mp.apodization = Ai.Hong.CommonMethod.ReadIniFile(pa, "Process", "apodization");
            mp.phaseCorrection = Ai.Hong.CommonMethod.ReadIniFile(pa, "Process", "phaseCorrection");
            mp.scans = Ai.Hong.CommonMethod.ReadIniFile(pa, "Collection", "sampleScans");
            mp.velocity = Ai.Hong.CommonMethod.ReadIniFile(pa, "Collection", "velocity");
            mp.zeroFill = Ai.Hong.CommonMethod.ReadIniFile(pa, "Process", "zeroFill");
            return mp;
        }

        private Border PQBaseBorder()
        {
            //光谱图和数据
            //  Border rootBorder = GetRootBorderFromTemplate("CalibrationReport_All.xaml");
            Border rootBorder = GetRootBorderFromTemplate("CalibrationReport_All.xaml");

            if (rootBorder == null)
                return null;
            rootBorder.Width = 18 * DPCM;
            rootBorder.Margin = new Thickness(1.5 * DPCM, 1.5 * DPCM, 1.5 * DPCM, 1 * DPCM);

            //Calibration.MeasureParameter mp = ReadIniFile(new MeasureParameter(), settingData.calibratePara.lineNoiseTest.iniFile);
            //Border bo = (rootBorder.FindName("Para") as Border);
            //bo.DataContext = mp;

            //自检基本信息
            Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtCompany", settingData.runing_para.unitName);
            Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtOperator", null);
            Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtInstrumentNo", settingData.runing_para.serialNo);
            Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtCalibrateTime", DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));
            Ai.Hong.ReportTemplate.FillTextData(rootBorder, "systemType", Common.SettingData.systemType.ToString());
            //显示光谱图形
            //ShowSpectrumGraphic(rootBorder, "borderAccuracyGraphic", accuracySpectrumFile, 18, double.MaxValue, 200);
            //ShowSpectrumGraphic(rootBorder, "borderSnrGraphic", snrSpectrumFile, 18, double.MaxValue, 200);

            SettingFile.Calibrate_Parameter para = settingData.calibratePara;
            if(para.snrPara.IsTest||
                para.accuracyPara.IsTest||
                para.yaxisRepPara.IsTest)
            {
                Border result = (rootBorder.FindName("result") as Border);
                result.Visibility = Visibility.Visible;
            }
            //显示线噪比及偏差结果数据
            if (para.snrPara.IsTest)
            {
                Border PPNoise = (rootBorder.FindName("PPNoise") as Border);
                Border PPDev = (rootBorder.FindName("PPDev") as Border);
                Border ppIpa = (rootBorder.FindName("PPIpa") as Border);
                Border ppEng = (rootBorder.FindName("PPEng") as Border);
                PPNoise.Visibility = Visibility.Visible;
                PPDev.Visibility = Visibility.Visible;
                ppIpa.Visibility = Visibility.Visible;
                ppEng.Visibility = Visibility.Visible;
                ShowResultData(rootBorder, "txtSNR", "realSNR", "imgSNR", 
                    para.snrPara.LineNoiseResult,
                    para.snrPara.PPThresold, true, "F2",
                    para.snrPara.SpectrumFile.Count==0);//显示线噪比结果
                ShowResultData(rootBorder, "txtDev", "realDev", "imgDevResult",
                    para.snrPara.TestResult,
                    para.snrPara.DevThresold, true, "F2",
                    para.snrPara.SpectrumFile.Count==0);//显示偏差结果
                ShowResultData(rootBorder, "txtIpa", "realIpa", "imgIpaResult",
                    para.snrPara.IpaTestResult,
                    para.snrPara.IpaThresold, false, "F2",
                    para.snrPara.IpaSpectrumFile.Count == 0);//显示干涉峰值结果
                ShowResultData(rootBorder, "txtEng", "realEng", "imgEngResult",
                    para.snrPara.EngTestResult,
                    para.snrPara.EngThresold, true, "F2",
                    para.snrPara.EngSpectrumFile.Count == 0);//显示干能量结果
            }
            //显示波数精度数据
            if (para.accuracyPara.IsTest)
            {
                Border acc = (rootBorder.FindName("AccNum") as Border);
                acc.Visibility = Visibility.Visible;
                //水蒸气
                Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtPeak", para.accuracyPara.targetPeak.ToString());
                Ai.Hong.ReportTemplate.FillTextData(rootBorder, "realPeak", para.accuracyPara.ActualBand.ToString("F2"));
                ShowResultData(rootBorder, "txtAcc", "realAccPQ", "imgAcc",
                    para.accuracyPara.TestResult, 
                    para.accuracyPara.thresold, true, "F2",
                    string.IsNullOrEmpty(para.accuracyPara.SpectrumPath));
                //聚苯乙烯
                Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtPolyPeak", para.accuracyPara.PolyTargetX.ToString());
                Ai.Hong.ReportTemplate.FillTextData(rootBorder, "realPoly", para.accuracyPara.PolyActualBand.ToString("F2"));
                ShowResultData(rootBorder, "txtPoly", "realPolyDevPQ", "imgAcc", 
                    para.accuracyPara.PolyTestResult,
                    para.accuracyPara.PolyThresold, true, "F2",
                    string.IsNullOrEmpty(para.accuracyPara.PolySpectrumPath));
            }
            //显示吸收重复性测试数据
            if (para.yaxisRepPara.IsTest)
            {
                Border yax = (rootBorder.FindName("Yax") as Border);
                yax.Visibility = Visibility.Visible;
                ShowResultData(rootBorder, "txtAccRepA", "realAccRepA", "imgAccRepA", 
                    para.yaxisRepPara.TestResult,
                    para.yaxisRepPara.YaxisRepThresold, true, "F2",
                    string.IsNullOrEmpty(para.yaxisRepPara.SpectrumFile));

            }//ShowResultData(rootBorder, "txtSureResult", "imgAcc", accuracyResult, settingData.calibratePara.accuracyPara.thresold, true, "F2");
            //ShowResultData(rootBorder, "txtSNRRMSResult", "imgSNRRMSResult", snrRMSResult, settingData.calibratePara.snrPara.RMSThresold, false, "F0");
            //ShowResultData(rootBorder, "txtSNRPPResult", "imgSNRPPResult", snrPPResult, settingData.calibratePara.snrPara.PPThresold, false, "F0");

            //最终数据
            TextBlock allTxt = rootBorder.FindName("txtAllResult") as TextBlock;
            Image allImg = rootBorder.FindName("imgAllResult") as Image;

            if (allTxt != null && allImg != null)
            {
                if (
                    (!para.snrPara.IsTest||(
                                      (para.snrPara.LineNoiseResult < para.snrPara.PPThresold || para.snrPara.SpectrumFile.Count!=0)&&
                                      (Math.Abs(para.snrPara.TestResult) < para.snrPara.DevThresold || para.snrPara.SpectrumFile.Count != 0)&&
                                      (para.snrPara.IpaTestResult>para.snrPara.IpaThresold||para.snrPara.IpaSpectrumFile.Count!=0)&&
                                      (para.snrPara.EngTestResult<para.snrPara.EngThresold||para.snrPara.EngSpectrumFile.Count!=0)
                                      ))&&
                    (!para.accuracyPara.IsTest||(
                                        (para.accuracyPara.TestResult < para.accuracyPara.thresold||!string.IsNullOrEmpty(para.accuracyPara.SpectrumPath)) &&
                                        (para.accuracyPara.PolyTestResult < para.accuracyPara.PolyThresold||!string.IsNullOrEmpty(para.accuracyPara.PolySpectrumPath))
                                        )) &&
                    (!para.yaxisRepPara.IsTest||
                        (para.yaxisRepPara.TestResult < para.yaxisRepPara.YaxisRepThresold||!string.IsNullOrEmpty(para.yaxisRepPara.SpectrumFile))
                        ))
                {
                    allTxt.Text = "自检结果 = 通过";
                    allTxt.Foreground = System.Windows.Media.Brushes.Blue;
                    Ai.Hong.CommonMethod.SetImageSource(allImg, imagePath, "Calibration_OK.png");
                }
                else
                {
                    allTxt.Text = "自检结果 = 未通过";
                    allTxt.Foreground = System.Windows.Media.Brushes.Red;
                    Ai.Hong.CommonMethod.SetImageSource(allImg, imagePath, "error_32.png");
                }
            }
            //填充时间
            Ai.Hong.ReportTemplate.FillTextData(rootBorder, "date", System.DateTime.Now.ToString("yyy-MM-dd HH:mm:ss"));

            return rootBorder;
        }

        private Border PQLineNoiseBorder()
        {
            //光谱图和数据
            //  Border rootBorder = GetRootBorderFromTemplate("CalibrationReport_All.xaml");
            Border rootBorder = GetRootBorderFromTemplate("LineNoise.xaml");

            if (rootBorder == null)
                return null;
            rootBorder.Width = 18 * DPCM;
            rootBorder.Margin = new Thickness(1.5 * DPCM, 1.5 * DPCM, 1.5 * DPCM, 1 * DPCM);

            Calibration.MeasureParameter mp = ReadIniFile(new MeasureParameter(), settingData.calibratePara.lineNoiseTest.iniFile);
            Border bo = (rootBorder.FindName("para") as Border);
            bo.DataContext = mp;

            //自检基本信息
            //Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtCompany", settingData.runing_para.unitName);
            //Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtOperator", null);
            //Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtInstrumentNo", settingData.runing_para.serialNo);
            //Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtCalibrateTime", DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));

            //显示光谱图形
            ShowSpectrumGraphic(rootBorder, "borderLineNoiseGraphic", settingData.calibratePara.lineNoiseTest.SpectrumFile.ToArray(), 18, double.MaxValue, 200);
            //ShowSpectrumGraphic(rootBorder, "borderSnrGraphic", snrSpectrumFile, 18, double.MaxValue, 200);

            // SettingFile.Calibrate_Parameter para = settingData.calibratePara;



            return rootBorder;
        }

        private void FillPara(Border rootBorder, string iniFile)
        {
            Calibration.MeasureParameter mp = ReadIniFile(new MeasureParameter(), iniFile);
            TextBlock resolution = (rootBorder.FindName("resolution") as TextBlock);
            TextBlock scans = (rootBorder.FindName("scans") as TextBlock);
            TextBlock gain = (rootBorder.FindName("gain") as TextBlock);
            TextBlock zeroFill = (rootBorder.FindName("zeroFill") as TextBlock);
            TextBlock phaseCorrection = (rootBorder.FindName("phaseCorrection") as TextBlock);
            TextBlock apodization = (rootBorder.FindName("apodization") as TextBlock);
            TextBlock velocity = (rootBorder.FindName("velocity") as TextBlock);

            resolution.Text = mp.resolution;
            scans.Text = mp.scans;
            gain.Text = mp.gain;
            zeroFill.Text = mp.zeroFill;
            phaseCorrection.Text = mp.phaseCorrection;
            apodization.Text = mp.apodization;
            velocity.Text = mp.velocity;
            //System.Windows.Controls.StackPanel gr = rootBorder.FindName("stack") as System.Windows.Controls.StackPanel;

            ////bo.DataContext = mp;
            //if (gr != null)
            //    bo.DataContext = mp;
        }

        private Border PQBorder(string PanelName, List<string> path, string iniFile, double firstX, double lastX)
        {
            //光谱图和数据
            //  Border rootBorder = GetRootBorderFromTemplate("CalibrationReport_All.xaml");
            Border rootBorder = GetRootBorderFromTemplate(PanelName);

            if (rootBorder == null)
                return null;
            rootBorder.Width = 18 * DPCM;
            rootBorder.Margin = new Thickness(1.5 * DPCM, 1.5 * DPCM, 1.5 * DPCM, 1 * DPCM);

            FillPara(rootBorder, iniFile);

            if (PanelName == "WaveNumAcc.xaml")
            {
                if (path.Count > 0)
                {
                    if (path[0] == settingData.calibratePara.accuracyPara.PolySpectrumPath)
                    {
                        Ai.Hong.ReportTemplate.FillTextData(rootBorder, "header", "波数精度测试 Polystyrene - 测量参数");
                        Ai.Hong.ReportTemplate.FillTextData(rootBorder, "headerSpc", "波数精度测试 Polystyrene - 光谱图");

                    }
                    else
                    {
                        Ai.Hong.ReportTemplate.FillTextData(rootBorder, "header", "波数精度测试 Water Vapor - 测量参数");
                        Ai.Hong.ReportTemplate.FillTextData(rootBorder, "headerSpc", "波数精度测试 Water Vapor - 光谱图");
                    }
                }
            }


            //自检基本信息
            //Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtCompany", settingData.runing_para.unitName);
            //Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtOperator", null);
            //Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtInstrumentNo", settingData.runing_para.serialNo);
            //Ai.Hong.ReportTemplate.FillTextData(rootBorder, "txtCalibrateTime", DateTime.Now.ToString(Ai.Hong.CommonMethod.LongDateTimeString));

            //显示光谱图形
            // ShowSpectrumGraphic(rootBorder, "borderLineNoiseGraphic", settingData.calibratePara.accuracyPara.SpectrumPath, 18, double.MaxValue, 200);
            if (PanelName == "LineNoise.xaml")
                DrawSpcInRange(rootBorder, "tr", "borderGraphic", path, firstX, lastX, 18, double.MaxValue, 100);
            else
                DrawSpcInRange(rootBorder, PanelName, "borderGraphic", path, firstX, lastX, 18, double.MaxValue, 100);

            //给出光谱路径
            Border br = rootBorder.FindName("borderGraphicPath") as Border;

            System.Windows.Controls.Grid gr = new System.Windows.Controls.Grid();

            for (int i = 0; i < path.Count; i++)
            {
                RowDefinition rd = new RowDefinition();
                gr.RowDefinitions.Add(rd);
            }
            for (int i = 0; i < path.Count; i++)
            {
                TextBlock tx = new TextBlock();
                Thickness tk = new Thickness(0, 2, 0, 0);
                tx.Margin = tk;
                tx.FontSize = 11.5;
                tx.Text = path[i];
                tx.TextWrapping = TextWrapping.Wrap;
                tx.TextAlignment = TextAlignment.Left;
                gr.Children.Add(tx);
                System.Windows.Controls.Grid.SetRow(tx, i);
            }
            br.Child = gr;
            //ShowSpectrumGraphic(rootBorder, "borderSnrGraphic", snrSpectrumFile, 18, double.MaxValue, 200);

            // SettingFile.Calibrate_Parameter para = settingData.calibratePara;
            return rootBorder;
        }
        private void DrawSpcInRange(Border rootBorder, string PanelName, string borderGraphicName, List<string> path, double firstX, double lastX, double graphicWidth, double graphicHeight = double.MaxValue, double DPI = double.MaxValue)
        {
            if (path == null)
                return;

            Border graphicBorder = rootBorder.FindName(borderGraphicName) as Border;
            if (graphicBorder != null)
            {
                System.Windows.Forms.DataVisualization.Charting.Chart graphicChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
                System.Windows.Forms.DataVisualization.Charting.ChartArea ca = new System.Windows.Forms.DataVisualization.Charting.ChartArea("LineArea");
                //      ca.AxisY.TitleForeColor = System.Drawing.Color.Blue;
                //      ca.AxisY.TextOrientation = TextOrientation.Auto;
               
                //OQ下设置Y轴标签
                if (PanelName != "ipa")
                {
                    if (PanelName == "tr")
                    {
                        //   ca.AxisY.Title = "Transmittance [%]";
                        ca.AxisY2.Title = "Transmittance [%]";

                    }
                    else
                    {
                        ca.AxisY2.Title = "Single Channel";
                    }
                }
                ca.AxisX.Title = "Wavenumber cm-1";
                ca.AxisX.TitleForeColor = System.Drawing.Color.Blue;
                //ca.AxisX.TitleFont = new System.Drawing.Font(System.Drawing.FontFamily.Families[19], 13);
                ca.AxisX.TitleFont = new System.Drawing.Font("", 13);

                ca.AxisX.IsReversed = true;
                ca.AxisY2.MajorGrid.Enabled = false;
                ca.AxisY2.TitleFont = new System.Drawing.Font("", 13);
                ca.AxisY2.TitleForeColor = System.Drawing.Color.Blue;
                ca.AxisY2.LineWidth = 2;
                ca.AxisY2.LineColor = System.Drawing.Color.Red;

                ca.AxisX.Crossing = double.MaxValue;
              
                LabelStyle ls = new LabelStyle();
                ls.Format = "F2";
                ls.Enabled = true;
                ca.AxisY2.LabelStyle = ls;
                LabelStyle lxx = new LabelStyle();
                ca.AxisX.LabelStyle = lxx;
                ca.AxisX.LabelStyle.Enabled = true;
                ca.AxisX.MinorTickMark.TickMarkStyle = TickMarkStyle.OutsideArea;
              
                ca.AxisX.MajorGrid.Enabled = false;
                ca.AxisX.LineWidth = 2;
                ca.AxisX.LineColor = System.Drawing.Color.Red;
           
                graphicChart.ChartAreas.Clear();
                graphicChart.ChartAreas.Add(ca);
                int iu = 0;
                double minY = 0, maxY = 0, minX = 0, maxX = 0;
                foreach (string ss in path)
                {
                    if (!File.Exists(ss))
                        continue;
                    iu++;
                    Series sr = new Series("PointSeries" + iu.ToString());
                    sr.MarkerSize = 24;
                    //读取光谱
                    Ai.Hong.CommonLibrary.SpecFileFormatDouble specData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
                    if (specData.ReadFile(ss) == false)
                    {
                        ITError = "读取光谱错误:" + path + " " + specData.ErrorString;
                        return;
                    }
                    int fx = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.FindNearestPosition(specData.XDatas, 0, specData.XDatas.Length - 1, firstX);
                    int lx = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.FindNearestPosition(specData.XDatas, 0, specData.XDatas.Length - 1, lastX);
                    //  int x = Ai.Hong.SpectrumAlgorithm.SpectrumAlgorithm.FindNearestPosition(specData.XDatas, 0, specData.XDatas.Length - 1, 4100);
                    if (minY == 0)
                    {
                        minY = specData.YDatas[lx];
                        maxY = specData.YDatas[lx];
                        minX = specData.XDatas[lx];
                        maxX = specData.XDatas[lx];
                    }
                    // int index = lx;
                    for (int i = fx; i <= lx; i++)
                    {
                        if (specData.XDatas[i] > 3900)
                        {
                            minY = specData.YDatas[i] < minY ? specData.YDatas[i] : minY;
                            maxY = specData.YDatas[i] > maxY ? specData.YDatas[i] : maxY;

                            minX = specData.XDatas[i] < minX ? specData.XDatas[i] : minX;
                            maxX = specData.XDatas[i] > maxX ? specData.XDatas[i] : maxX;
                        }
                        sr.Points.AddXY(specData.XDatas[i], specData.YDatas[i]); //index--;
                    }

                    //sr.MarkerStyle = MarkerStyle.Diamond;
                    //sr.Color = System.Drawing.Color.Green;
                    sr.SetCustomProperty("PixelPointWidth", "80");
                    sr.ChartArea = "LineArea";
                    sr.ChartType = SeriesChartType.Line;
                    sr.Color = System.Drawing.Color.Blue;
                  //  sr.Font = new System.Drawing.Font(System.Drawing.FontFamily.Families[0], 2);
                    //sr.IsValueShownAsLabel = true;

                    sr.XValueType = ChartValueType.Int32;// String;
                    // graphicChart.Series.Clear();
                    graphicChart.Series.Add(sr);
                    graphicChart.ChartAreas[0].AxisY.Enabled = AxisEnabled.False;
                    graphicChart.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;

                }

                //  graphicChart.ChartAreas[0].AxisX.Interval = 0.005;//设置轴间隔
                // graphicChart.ChartAreas[0].AxisY.Interval = 0.005;
                //if (PanelName == "lineNoise")
                //{
                //    graphicChart.ChartAreas[0].AxisY2.Minimum = 99;
                //    graphicChart.ChartAreas[0].AxisY2.Maximum = 103;
                //}
                //else if (PanelName == "transRep")
                //{
                //    graphicChart.ChartAreas[0].AxisY2.Minimum = 98;// 0.98;
                //    graphicChart.ChartAreas[0].AxisY2.Maximum = 102;// 1.02;
                //}
                //else if (maxY - minY <= 2)
                //{

                //    graphicChart.ChartAreas[0].AxisY2.Minimum = minY - 0.02;
                //    graphicChart.ChartAreas[0].AxisY2.Maximum = maxY + 0.02;
                //}
                //else
                //{


                //graphicChart.ChartAreas[0].AxisX.Minimum = minX;// firstX;
                //graphicChart.ChartAreas[0].AxisX.Maximum = maxX;// lastX;
                //graphicChart.ChartAreas[0].AxisY.Minimum = minY -(maxY - minY) / 5;
                //graphicChart.ChartAreas[0].AxisY.Maximum = maxY +(maxY - minY) / 5;

                graphicChart.ChartAreas[0].AxisY.Minimum = minY - (maxY - minY) / 10;
                graphicChart.ChartAreas[0].AxisY.Maximum = maxY + (maxY - minY) / 10;
                graphicChart.ChartAreas[0].AxisY2.Minimum = minY - (maxY - minY) / 10;
                graphicChart.ChartAreas[0].AxisY2.Maximum = maxY + (maxY - minY) / 10;
                //  graphicChart.ChartAreas[0].AxisX.Interval = (lastX - firstX) / 10;
                //  graphicChart.ChartAreas[0].AxisY.Interval = (maxY - minY) / 10;
                //}
                graphicChart.ChartAreas[0].AxisX.Minimum = firstX;
                graphicChart.ChartAreas[0].AxisX.Maximum = lastX;
                //else
                //{

                //    mainChart.ChartAreas[0].AxisX.Minimum = (int)min + 0.5;// min > 1 ? (int)(min - 1) : 0;//设置横纵坐标
                //    mainChart.ChartAreas[0].AxisY.Minimum = (int)min + 0.5;
                //    mainChart.ChartAreas[0].AxisX.Maximum = (int)max + 1;
                //    mainChart.ChartAreas[0].AxisY.Maximum = (int)max + 1;

                //    mainChart.ChartAreas[0].AxisX.Interval = 0.5;//设置轴间隔
                //    mainChart.ChartAreas[0].AxisY.Interval = 0.5;
                //}
                //foreach (string file in graphicFiles)
                //{
                //    if (file != null)
                //    {
                //Ai.Hong.CommonMethod.DrawSpectrumGraphic(graphicChart, ca, file, System.Drawing.Color.Black);
                //    }
                //}

                DPI = (DPI == double.MaxValue) ? 96 : DPI;
                graphicHeight = (graphicHeight == double.MaxValue) ? graphicBorder.Height * DPI / 96 : graphicHeight * DPI / 2.54;

                graphicChart.Width = (int)(graphicWidth * DPI / 2.54);        //1cm = 2.54inch = 96dpi
                graphicChart.Height = (int)(graphicHeight);

                System.IO.MemoryStream stream = new MemoryStream();
                graphicChart.SaveImage(stream, System.Drawing.Imaging.ImageFormat.Png);

                var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();

                Image img = new Image();
                graphicBorder.Child = img;
                img.Source = bitmapImage;
                img.Stretch = System.Windows.Media.Stretch.Uniform;
            }

        }

        /// <summary>
        /// 创建PQ报告
        /// </summary>
        /// <param name="xpsFile"></param>
        /// <param name="isCreateSpec"></param>
        /// <returns></returns>
        private bool CreatePQReport(string xpsFile, bool isCreateSpec)
        {
            FixedDocument fixedDoc = new FixedDocument();
            List<string> path = new List<string>();
            //path.Add(@"E:\Project\NirIdentifier\bin\Release\Data\Calibration\PQ\PQ12032015\LineNoiseAndDevTest_2015-03-12 15-33-42_tr.spc");
            //path.Add(@"E:\Project\NirIdentifier\bin\Release\Data\Calibration\PQ\PQ12032015\LineNoiseAndDevTest_2015-03-12 15-33-55_tr.spc");

            Border baseBorder = PQBaseBorder();
            string[] region = GetRegionFromIni(settingData.calibratePara.snrPara.iniFile).Split('+');
            Border lineNoiseBorder = PQBorder("LineNoise.xaml", settingData.calibratePara.snrPara.SpectrumFile, settingData.calibratePara.snrPara.iniFile, Convert.ToDouble(region[0]), Convert.ToDouble(region[1]));
            double ipaFirstX, ipaLastX;
            Ai.Hong.CommonLibrary.SpecFileFormatDouble ipaData=new Ai.Hong.CommonLibrary.SpecFileFormatDouble ();
            if(!ipaData.ReadFile(settingData.calibratePara.snrPara.IpaSpectrumFile[0]))
            {
                return false;
            }
            ipaFirstX = (from p in ipaData.XDatas select p).Min();
            ipaLastX = (from p in ipaData.XDatas select p).Max();
            Border ipaBaseBorder=GetRootBorderFromTemplate("OQCommonReport.xaml");
            //Border ipaBorder = PQBorder("OQCommonReport.xaml", settingData.calibratePara.snrPara.IpaSpectrumFile, settingData.calibratePara.snrPara.iniFile, ipaFirstX, ipaLastX);
            Ai.Hong.ReportTemplate.FillTextData(ipaBaseBorder, "headerSpc", "干涉峰值测试 - 光谱图");
            Ai.Hong.ReportTemplate.FillTextData(ipaBaseBorder, "headerPara", "干涉峰值测试 - 测量参数");
            Border ipaBorder = OQBorder(ipaBaseBorder, "ipa", settingData.calibratePara.snrPara.IpaSpectrumFile, settingData.calibratePara.snrPara.iniFile, ipaFirstX, ipaLastX);
            //Border engBorder = PQBorder("OQCommonReport.xaml", settingData.calibratePara.snrPara.EngSpectrumFile, settingData.calibratePara.snrPara.iniFile, Convert.ToDouble(region[0]), Convert.ToDouble(region[1]));
            Border engBaseBorder = GetRootBorderFromTemplate("OQCommonReport.xaml");
            Ai.Hong.ReportTemplate.FillTextData(engBaseBorder, "headerSpc", "能量测试 - 光谱图");
            Ai.Hong.ReportTemplate.FillTextData(engBaseBorder, "headerPara", "能量测试 - 测量参数");
            Border engBorder = OQBorder(engBaseBorder, null, settingData.calibratePara.snrPara.EngSpectrumFile, settingData.calibratePara.snrPara.iniFile, Convert.ToDouble(region[0]), Convert.ToDouble(region[1]));

            path.Add(settingData.calibratePara.accuracyPara.SpectrumPath);
            Border waveNumAccBorder = PQBorder("WaveNumAcc.xaml", path, settingData.calibratePara.accuracyPara.iniFile, 7160, 7210);
            path.Clear();
            path.Add(settingData.calibratePara.accuracyPara.PolySpectrumPath);
            Border wavePolyAccBorder = PQBorder("WaveNumAcc.xaml", path, settingData.calibratePara.accuracyPara.iniFile, 4521, 4621);

            path.Clear();
            path.Add(settingData.calibratePara.yaxisRepPara.SpectrumFile);
            // path.Add(@"E:\Project\NirIdentifier\bin\Release\Data\Calibration\PQ\Accuracy\PQ2015_04_03\1.spc");
            region = GetRegionFromIni(settingData.calibratePara.yaxisRepPara.iniFile).Split('+');
            Border yaxisRepABorder = PQBorder("YaxisRep.xaml", path, settingData.calibratePara.yaxisRepPara.iniFile, Convert.ToDouble(region[0]), Convert.ToDouble(region[1]));// settingData.calibratePara.yaxisRepPara.firstX , settingData.calibratePara.yaxisRepPara.lastX);

            PageContent baseContent = CreatePageContent(baseBorder);
            PageContent lineNoiseContent = CreatePageContent(lineNoiseBorder);
            PageContent ipaContent = CreatePageContent(ipaBorder);
            PageContent engContent = CreatePageContent(engBorder);
            PageContent waveNumAccContent = CreatePageContent(waveNumAccBorder);
            PageContent wavePolyAccContent = CreatePageContent(wavePolyAccBorder);
            PageContent yaxisRepAContent = CreatePageContent(yaxisRepABorder);

            fixedDoc.Pages.Add(baseContent);
            if (isCreateSpec)
            {
                if (settingData.calibratePara.snrPara.IsTest)
                {
                    fixedDoc.Pages.Add(lineNoiseContent);
                    fixedDoc.Pages.Add(ipaContent);
                    fixedDoc.Pages.Add(engContent);
                }
                if (settingData.calibratePara.accuracyPara.IsTest)
                {
                    fixedDoc.Pages.Add(waveNumAccContent);
                    fixedDoc.Pages.Add(wavePolyAccContent);
                }
                if (settingData.calibratePara.yaxisRepPara.IsTest)
                    fixedDoc.Pages.Add(yaxisRepAContent);
            }
            //预览报告
            //PrintPreView printdlg = new PrintPreView(fixedDoc);
            //printdlg.ShowDialog();

            try
            {
                //保存到XPS文件
                DocumentPaginator paginator = fixedDoc.DocumentPaginator;
                XpsDocument xpsDocument = new XpsDocument(xpsFile, FileAccess.Write);
                System.Windows.Xps.XpsDocumentWriter documentWriter = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                documentWriter.Write(paginator);
                xpsDocument.Close();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }

            // System.Diagnostics.Process.Start(dlg.FileName);
            // return WriteXpsFile(content, xpsFile);
        }


        private void ShowSpc_Laser(object sender, RoutedEventArgs e)
        {
            string path = settingData.calibratePara.laserPara.LeaserSpectrum;
            //  string path = @"E:\Project\NirIdentifier\bin\Release\Data\Calibration\ReferenceSpc\YaxisRepTest\ref.spc";
            if ((bool)e.OriginalSource)
            {

                //if (System.IO.File.Exists(path))
                //{
                spectrumChart.DrawGraphics(path, Brushes.Red, false);//(bool)e.OriginalSource
                //}
            }
            else
            {
                spectrumChart.RemoveGraphic(path);
            }
        }

        private void ShowSpc_lineNoise(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = e.OriginalSource as CheckBox;
            if (checkBox == null)
                return;
            if(string.Equals(checkBox.Tag.ToString(),"ifg"))
            {
                foreach(string p in settingData.calibratePara.snrPara.IpaSpectrumFile)
                {
                    if ((bool)checkBox.IsChecked)
                    {
                        spectrumChart.DrawGraphics(p, Brushes.Red, false);
                    }
                    else
                    { 
                        spectrumChart.RemoveGraphic(p);
                    }
                }
            }
            if (string.Equals(checkBox.Tag.ToString(), "sbm"))
            {
                foreach (string p in settingData.calibratePara.snrPara.EngSpectrumFile)
                {
                    if ((bool)checkBox.IsChecked)
                    {
                        spectrumChart.DrawGraphics(p, Brushes.Red, false);
                    }
                    else
                    {
                        spectrumChart.RemoveGraphic(p);
                    }
                }
            }
            if (string.Equals(checkBox.Tag.ToString(), "tr"))
            {
                foreach (string p in settingData.calibratePara.snrPara.SpectrumFile)
                {
                    if ((bool)checkBox.IsChecked)
                    {
                        spectrumChart.DrawGraphics(p, Brushes.Red, false);
                    }
                    else
                    {
                        spectrumChart.RemoveGraphic(p);
                    }
                }
            }
            //if ((bool)e.OriginalSource)
            //{
            //    foreach (string path in settingData.calibratePara.snrPara.SpectrumFile)
            //    {
            //        spectrumChart.DrawGraphics(path, Brushes.Red, false);
            //    }
            //}
            //else
            //{
            //    foreach (string path in settingData.calibratePara.snrPara.SpectrumFile)
            //    {
            //        spectrumChart.RemoveGraphic(path);
            //    }
            //}

        }

        private void ShowSpc_PQaccuracy(object sender, RoutedEventArgs e)
        {
            if ((bool)e.OriginalSource)
            {
                //if (File.Exists(settingData.calibratePara.accuracyPara.SpectrumPath))
                //{
                spectrumChart.DrawGraphics(settingData.calibratePara.accuracyPara.SpectrumPath, Brushes.Red, false);
                //}
                //if (File.Exists(settingData.calibratePara.accuracyPara.PolySpectrumPath))
                //{
                spectrumChart.DrawGraphics(settingData.calibratePara.accuracyPara.PolySpectrumPath, Brushes.Blue, false);
                //  }
            }
            else
            {
                spectrumChart.RemoveGraphic(settingData.calibratePara.accuracyPara.SpectrumPath);
                spectrumChart.RemoveGraphic(settingData.calibratePara.accuracyPara.PolySpectrumPath);

            }
        }

        private void ShowSpc_YaxisRep(object sender, RoutedEventArgs e)
        {
            if ((bool)e.OriginalSource)
            {
                //if (File.Exists(settingData.calibratePara.yaxisRepPara.SpectrumFile))
                //{
                spectrumChart.DrawGraphics(settingData.calibratePara.yaxisRepPara.SpectrumFile, Brushes.Red, false);
                //}
            }
            else
            {
                spectrumChart.RemoveGraphic(settingData.calibratePara.yaxisRepPara.SpectrumFile);
            }
        }

        private void ShowSpc_Resolution(object sender, RoutedEventArgs e)
        {
            if ((bool)e.OriginalSource)
            {
                //if (File.Exists(settingData.calibratePara.resolutionTestPara.SpectrumPath))
                //{
                spectrumChart.DrawGraphics(settingData.calibratePara.resolutionTestPara.SpectrumPath, Brushes.Red, false);
                //}
            }
            else
            {
                spectrumChart.RemoveGraphic(settingData.calibratePara.resolutionTestPara.SpectrumPath);
            }
        }

        private void ShowSpc_LineNoiseOQ(object sender, RoutedEventArgs e)
        {
            if ((bool)e.OriginalSource)
            {
                foreach (string path in settingData.calibratePara.lineNoiseTest.SpectrumFile)
                {
                    //if (File.Exists(path))
                    //{
                    spectrumChart.DrawGraphics(path, Brushes.Red, false);
                    //}
                }
            }
            else
            {
                foreach (string path in settingData.calibratePara.lineNoiseTest.SpectrumFile)
                {
                    spectrumChart.RemoveGraphic(path);
                }
            }
        }

        private void ShowSpc_EnergyDis(object sender, RoutedEventArgs e)
        {
            if ((bool)e.OriginalSource)
            {
                //if (File.Exists(settingData.calibratePara.energyDisPara.SpectrumPath))
                //{
                spectrumChart.DrawGraphics(settingData.calibratePara.energyDisPara.SpectrumPath, Brushes.Red, false);
                //}
            }
            else
            {
                spectrumChart.RemoveGraphic(settingData.calibratePara.energyDisPara.SpectrumPath);
            }
        }

        private void ShowSpc_LineSlope(object sender, RoutedEventArgs e)
        {
            if ((bool)e.OriginalSource)
            {
                //if (File.Exists(settingData.calibratePara.lineSlopeTestPara.SpectrumPath))
                //{
                spectrumChart.DrawGraphics(settingData.calibratePara.lineSlopeTestPara.SpectrumPath, Brushes.Red, false);
                //}
            }
            else
            {
                spectrumChart.RemoveGraphic(settingData.calibratePara.lineSlopeTestPara.SpectrumPath);
            }
        }

        private void ShowSpc_TransRep(object sender, RoutedEventArgs e)
        {
            if ((bool)e.OriginalSource)
            {
                foreach (string path in settingData.calibratePara.transRepTest.SpectrumPath)
                {
                    //if (File.Exists(path))
                    //{
                    spectrumChart.DrawGraphics(path, Brushes.Red, false);
                    //}
                }
            }
            else
            {
                foreach (string path in settingData.calibratePara.transRepTest.SpectrumPath)
                {
                    spectrumChart.RemoveGraphic(path);
                }
            }
        }

        private void ShowSpc_WaveNumAcc(object sender, RoutedEventArgs e)
        {
            if ((bool)e.OriginalSource)
            {
                //if (File.Exists(settingData.calibratePara.accuracyTestOQ.SpectrumPath))
                //{
                spectrumChart.DrawGraphics(settingData.calibratePara.accuracyTestOQ.SpectrumPath, Brushes.Red, false);
                //}
                //if (File.Exists(settingData.calibratePara.accuracyTestOQ.PolySpectrumPath))
                //{
                spectrumChart.DrawGraphics(settingData.calibratePara.accuracyTestOQ.PolySpectrumPath, Brushes.Blue, false);
                //}
            }
            else
            {
                spectrumChart.RemoveGraphic(settingData.calibratePara.accuracyTestOQ.SpectrumPath);
                spectrumChart.RemoveGraphic(settingData.calibratePara.accuracyTestOQ.PolySpectrumPath);

            }
        }

        private void ShowSpc_WaveNumRep(object sender, RoutedEventArgs e)
        {
            if ((bool)e.OriginalSource)
            {
                foreach (string path in settingData.calibratePara.waveNumRepTestPara.SpectrumPath)
                {
                    //if (File.Exists(path))
                    //{
                    spectrumChart.DrawGraphics(path, Brushes.Red, false);
                    //}
                }
            }
            else
            {
                foreach (string path in settingData.calibratePara.waveNumRepTestPara.SpectrumPath)
                {
                    spectrumChart.RemoveGraphic(path);
                }
            }
        }

        public DateTime beginDate;
        public DateTime endDate;
        List<double> Result = new List<double>();
        List<double> LineSlopeMin = new List<double>();
        List<double> LineSlopeMax = new List<double>();
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (list.SelectedItem == null || listPqOq.SelectedItem == null)
                return;
            if (beginDatePicker.SelectedDate == null)
                beginDate = new DateTime(1800, 1, 1);
            else
                beginDate = (DateTime)beginDatePicker.SelectedDate;

            if (endDatePicker.SelectedDate == null)
                endDate = new DateTime(2500, 1, 1);
            else
            {
                endDate = (DateTime)endDatePicker.SelectedDate;
                //设置到所选日期的23:59:59:999
                endDate.AddDays(1);
                endDate.AddMilliseconds(-1);
            }
            //清空暂存空间
            Result.Clear();
            LineSlopeMax.Clear();
            LineSlopeMin.Clear();
            List<string> res;
            spectrumChart.Visibility = Visibility.Collapsed;
            mainFGrid.Visibility = Visibility.Visible;
            if (listPqOq.SelectionBoxItem.ToString() != "100%线斜率测试结果")
            {
                res = SettingData.dataBase.GetColumnsInf(beginDate, endDate, list.SelectionBoxItem.ToString(), listPqOq.SelectionBoxItem.ToString());
                foreach (string rr in res)
                    Result.Add(Convert.ToDouble(rr));
            }
            else
            {
                res = SettingData.dataBase.GetColumnsInf(beginDate, endDate, list.SelectionBoxItem.ToString(), listPqOq.SelectionBoxItem.ToString());
                foreach (string rr in res)
                {
                    string[] tt = rr.Split('+');
                    string[] max = tt[0].Split(',');
                    string[] min = tt[1].Split(',');
                    foreach (string ss in max)
                    {
                        LineSlopeMax.Add(Convert.ToDouble(ss));
                    }
                    foreach (string ss in min)
                    {
                        LineSlopeMin.Add(Convert.ToDouble(ss));
                    }
                }

            }
            DisplayTrendChart();
        }

        /// <summary>
        /// 显示趋势图
        /// </summary>
        public void DisplayTrendChart()
        {
            mainChart.Controls.Clear();
            ChartArea ca = new ChartArea("LineArea");
            ca.BackColor = System.Drawing.Color.Transparent;
            ca.AxisY.Title = "Predicted";
            ca.AxisY.TitleForeColor = System.Drawing.Color.Blue;
            // ca.AxisY.TextOrientation = TextOrientation.Auto;
            ca.AxisX.Title = "Actual";
            Axis ass = new Axis();
            ca.AxisX.TitleForeColor = System.Drawing.Color.Blue;

            //ca.AxisX.TitleFont = new System.Drawing.Font(System.Drawing.FontFamily.Families[19], 16);
            //ca.AxisY.TitleFont = new System.Drawing.Font(System.Drawing.FontFamily.Families[19], 16);
            ca.AxisX.TitleFont = new System.Drawing.Font("", 16);
            ca.AxisY.TitleFont = new System.Drawing.Font("", 16);

            LabelStyle ls = new LabelStyle();
            ls.Format = "F2";
            // ca.AxisX.LabelStyle = ls;
            LabelStyle lx = new LabelStyle();
            lx.Format = "F2";
            // lx.Font = new System.Drawing.Font(System.Drawing.FontFamily.Families[0], 2);
            lx.Font = new System.Drawing.Font("", 2);
            ca.AxisY.LabelStyle = ls;
            ca.AxisX.LabelStyle = lx;
            
            ca.AxisX.LabelStyle.Enabled = true;
            ca.AxisX.MinorTickMark.TickMarkStyle = TickMarkStyle.None;
            ca.AxisY.MinorTickMark.TickMarkStyle = TickMarkStyle.None;
            ca.AxisX.MajorGrid.Enabled = false;
            ca.AxisX.LineWidth = 2;
            ca.AxisY.LineWidth = 2;
            
            ca.AxisX.LineColor = System.Drawing.Color.Red;
            ca.AxisY.LineColor = System.Drawing.Color.Red;
            ca.AxisX.ArrowStyle = AxisArrowStyle.SharpTriangle;
            ca.AxisY.ArrowStyle = AxisArrowStyle.SharpTriangle;
            ca.AxisX.LineDashStyle = ChartDashStyle.DashDotDot;
            
            ca.AxisX.MajorGrid.LineColor = System.Drawing.Color.Blue;
            ca.AxisY.MajorGrid.LineColor = System.Drawing.Color.Blue;
            mainChart.ChartAreas.Clear();
            mainChart.ChartAreas.Add(ca);
            Series sr = new Series("PointsSeries");

            sr.MarkerSize = 12;
            sr.MarkerStyle = MarkerStyle.Diamond;

            sr.Color = System.Drawing.Color.Green;// (System.Drawing.Color)ColorConverter.ConvertFromString("#F0F0F0");
            int sin = 0;
            if (listPqOq.SelectionBoxItem.ToString() != "100%线斜率测试结果")
            {
                foreach (double rsul in Result)
                {
                    sr.Points.AddY(rsul);

                    sr.Points[sin].ToolTip = "Y值( " + rsul.ToString("F4") + " )";
                    sin++;

                }
                sr.SetCustomProperty("PixelPointWidth", "40");
                sr.ChartArea = "LineArea";
                sr.ChartType = SeriesChartType.Line;
                //sr.IsValueShownAsLabel = true;

                sr.XValueType = ChartValueType.Double;// String;
                mainChart.Series.Clear();
                mainChart.Series.Add(sr);
            }
            else
            {
                //添加最小值
                foreach (double rsul in LineSlopeMin)
                {
                    sr.Points.AddY(rsul);

                    sr.Points[sin].ToolTip = "Y值( " + rsul.ToString("F4") + " )";
                    sin++;

                }
                sr.SetCustomProperty("PixelPointWidth", "40");
                sr.ChartArea = "LineArea";
                sr.ChartType = SeriesChartType.Line;
                //sr.IsValueShownAsLabel = true;

                sr.XValueType = ChartValueType.Double;// String;
                mainChart.Series.Clear();
                mainChart.Series.Add(sr);
                //添加最大值

                Series max = new Series("LineSeries");
                sin = 0;
                foreach (double rsul in LineSlopeMax)
                {
                    max.Points.AddY(rsul);

                    max.Points[sin].ToolTip = "Y值( " + rsul.ToString("F4") + " )";
                    sin++;

                }
                //line.Points.AddXY(min, min);
                //line.Points.AddXY(max, max);
                max.ChartType = SeriesChartType.Line;
                max.Color = System.Drawing.Color.Green;
                max.ChartArea = "LineArea";
                mainChart.Series.Add(max);
            }

            System.Windows.Forms.Label s1 = new System.Windows.Forms.Label();
            s1.Text = listPqOq.SelectionBoxItem.ToString();
            s1.Dock = System.Windows.Forms.DockStyle.Top;
            s1.Enabled = true;

            mainChart.Controls.Clear();
            mainChart.Controls.Add(s1);
        }

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (list.SelectedItem == null)
                return;
            listPqOq.ItemsSource = null;
            listPqOq.Items.Clear();
            if (list.SelectedIndex == 0)
                listPqOq.ItemsSource = SettingData.dataBase.GetTableColumn("PQ测试结果");
            else if (list.SelectedIndex == 1)
                listPqOq.ItemsSource = SettingData.dataBase.GetTableColumn("OQ测试结果");
        }

        private void Manual_Click(object sender, RoutedEventArgs e)
        {
            double curvalue = 0;// GetLaserValueFromIni(iniFileName);
            string paraString = VspecInstrument.GetParametersTable();
            if (paraString != null)
            {
                string[] temp = paraString.Split(',');
                string[] leasertemp = temp[3].Split(':');
                leasertemp[1] = leasertemp[1].Replace('\"', ' ');
                curvalue = Convert.ToDouble(leasertemp[1]) / 1000;
                //string temp1 = paraString.Split(',')[1].Split(':')[1].Replace('\"', ' ');
                //读取仪器序列号
                //settingData.runing_para.serialNo =System.Text.RegularExpressions.Regex.Replace(temp1, @"\s", "");// temp[1].Split(':')[1]//.Replace('\"', ' ');
            }
            else
            {
                MessageBox.Show("获取仪器内部激光波数失败！,请检查仪器连接");
                return;
            }

            double setvalue = curvalue * Convert.ToDouble(peakAuto.Text) / settingData.calibratePara.laserPara.targetPeak;
            string strLWN = (setvalue * 1000).ToString();
            double writeLWN = Convert.ToDouble(strLWN.Substring(0, strLWN.Length < 17 ? strLWN.Length : 17));
            if (writeLWN < 640 && writeLWN > 635)
            {
                //写入激光波数到仪器
                if (!VspecInstrument.SetLaserWavelength((writeLWN).ToString()))
                {
                    MessageBox.Show("激光波数已存入ini，但写入仪器失败！");
                }
                else
                {
                    MessageBox.Show("激光波数 " + (writeLWN).ToString() + " 已成功写入仪器!");
                    //if(VspecInstrument.Disconnect()&&VspecInstrument.Connect())
                    //{
                    //    MessageBox.Show("重新连接仪器成功!");
                    //}
                    //else
                    //{
                    //    MessageBox.Show("重新连接仪器失败！"+"\r\n"+VspecInstrument.GetError());
                    //}
                }
            }
            else
            {
                MessageBox.Show("激光波数错误\r\n" + (writeLWN).ToString(), "提示");
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!isNumberic(e.Text))
            {
                e.Handled = true;
            }
            else
                e.Handled = false;
        }
        //isDigit是否是数字
        private bool isNumberic(string _string)
        {
            if (string.IsNullOrEmpty(_string))
                return false;
            
            if (peakAuto.Text != "")
            {
                if (peakAuto.Text.IndexOf('.') != -1 && _string == ".")
                {
                    MessageBox.Show("非法字符！");
                    return false;
                }
            }

            foreach (char c in _string)
            {
                if (!char.IsDigit(c))
                {
                    if (c != '.')
                    {
                        MessageBox.Show("非法字符！");
                        return false;
                    }
                }
              
                //if(c<'0' c="">'9')//最好的方法,在下面测试数据中再加一个0，然后这种方法效率会搞10毫秒左右
            }
            return true;
        }

        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                Convert.ToDouble(System.Windows.Clipboard.GetText());
            }
            catch
            {
                e.Handled = true;
                MessageBox.Show("剪贴板数据格式不正确！粘贴失败！", "提示");
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            stackCal.IsEnabled = true;
        }

        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            stackCal.IsEnabled = false;
        }

        private void IsTestChecked_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox= sender as CheckBox;
            switch(checkBox.Tag.ToString())
            {
                case "PQPP": settingData.calibratePara.snrPara.IsTest = true; break;
                case "PQAC": settingData.calibratePara.accuracyPara.IsTest = true; break;
                case "PQYA": settingData.calibratePara.yaxisRepPara.IsTest = true; break;

                case "OQRE": settingData.calibratePara.resolutionTestPara.IsTest = true; break;
                case "OQPP": settingData.calibratePara.lineNoiseTest.IsTest = true; break;
                case "OQEN": settingData.calibratePara.energyDisPara.IsTest = true; break;
                case "OQLI": settingData.calibratePara.lineSlopeTestPara.IsTest = true; break;
                case "OQTR": settingData.calibratePara.transRepTest.IsTest = true; break;
                case "OQAC": settingData.calibratePara.accuracyTestOQ.IsTest = true; break;
                case "OQWA": settingData.calibratePara.waveNumRepTestPara.IsTest = true; break;
                default: return;

            }
        }

        private void IsTestUnChecked_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            switch (checkBox.Tag.ToString())
            {
                case "PQPP": settingData.calibratePara.snrPara.IsTest = false; break;
                case "PQAC": settingData.calibratePara.accuracyPara.IsTest = false; break;
                case "PQYA": settingData.calibratePara.yaxisRepPara.IsTest = false; break;

                case "OQRE": settingData.calibratePara.resolutionTestPara.IsTest = false; break;
                case "OQPP": settingData.calibratePara.lineNoiseTest.IsTest = false; break;
                case "OQEN": settingData.calibratePara.energyDisPara.IsTest = false; break;
                case "OQLI": settingData.calibratePara.lineSlopeTestPara.IsTest = false; break;
                case "OQTR": settingData.calibratePara.transRepTest.IsTest = false; break;
                case "OQAC": settingData.calibratePara.accuracyTestOQ.IsTest = false; break;
                case "OQWA": settingData.calibratePara.waveNumRepTestPara.IsTest = false; break;
                default: return;
            }
        }

        private void CheckAll_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox=e.Source as CheckBox;
            if(checkBox.Tag.ToString().Equals("PQ"))
            {
                foreach (GroupBox group in stack.Children.OfType<GroupBox>())
                {
                    CheckBox p = group.Header as CheckBox;
                    if(p.Tag.ToString()=="PQPP"||p.Tag.ToString() == "PQAC"||p.Tag.ToString() == "PQYA")
                    {                   
                        p.IsChecked = true;
                    }
                }
                 settingData.calibratePara.snrPara.IsTest = true; 
                 settingData.calibratePara.accuracyPara.IsTest = true;
                 settingData.calibratePara.yaxisRepPara.IsTest = true;
            }
            else if(checkBox .Tag.ToString().Equals("OQ"))
            {
                foreach (GroupBox group in oqStack.Children.OfType<GroupBox>())
                {
                    CheckBox p = group.Header as CheckBox;
                    if (p.Tag.ToString() == "OQRE" || p.Tag.ToString() == "OQPP" ||
                        p.Tag.ToString() == "OQEN" || p.Tag.ToString() == "OQLI" ||
                        p.Tag.ToString() == "OQTR" || p.Tag.ToString() == "OQAC" ||
                        p.Tag.ToString() == "OQWA")
                    {
                        p.IsChecked = true;
                    }
                }
                 settingData.calibratePara.resolutionTestPara.IsTest = true;
                 settingData.calibratePara.lineNoiseTest.IsTest = true;
                 settingData.calibratePara.energyDisPara.IsTest = true;
                 settingData.calibratePara.lineSlopeTestPara.IsTest = true;
                 settingData.calibratePara.transRepTest.IsTest = true;
                 settingData.calibratePara.accuracyTestOQ.IsTest = true;
                 settingData.calibratePara.waveNumRepTestPara.IsTest = true;
            }
        }

        private void CheckAll_UnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = e.Source as CheckBox;
            if (checkBox.Tag.ToString().Equals("PQ"))
            {
                foreach (GroupBox group in stack.Children.OfType<GroupBox>())
                {
                    CheckBox p = group.Header as CheckBox;
                    if (p.Tag.ToString() == "PQPP" || p.Tag.ToString() == "PQAC" || p.Tag.ToString() == "PQYA")
                    {
                        p.IsChecked = false;
                    }
                }
                settingData.calibratePara.snrPara.IsTest = false;
                settingData.calibratePara.accuracyPara.IsTest = false;
                settingData.calibratePara.yaxisRepPara.IsTest = false;
            }
            else if (checkBox.Tag.ToString().Equals("OQ"))
            {
                foreach (GroupBox group in oqStack.Children.OfType<GroupBox>())
                {
                    CheckBox p = group.Header as CheckBox;
                    if (p.Tag.ToString() == "OQRE" || p.Tag.ToString() == "OQPP" ||
                        p.Tag.ToString() == "OQEN" || p.Tag.ToString() == "OQLI" ||
                        p.Tag.ToString() == "OQTR" || p.Tag.ToString() == "OQAC" ||
                        p.Tag.ToString() == "OQWA")
                    {
                        p.IsChecked = false;
                    }
                }
                settingData.calibratePara.resolutionTestPara.IsTest = false;
                settingData.calibratePara.lineNoiseTest.IsTest = false;
                settingData.calibratePara.energyDisPara.IsTest = false;
                settingData.calibratePara.lineSlopeTestPara.IsTest = false;
                settingData.calibratePara.transRepTest.IsTest = false;
                settingData.calibratePara.accuracyTestOQ.IsTest = false;
                settingData.calibratePara.waveNumRepTestPara.IsTest = false;
            }
        }

        private void btnPQCancel_Click(object sender, RoutedEventArgs e)
        {
            if (PQThread!=null)
                PQThread.Abort();
            snrPanel.scanProgress.IsIndeterminate = false;
            snrPanel.scanProgress.Visibility = Visibility.Visible;
            accuracyPanel.scanProgress.IsIndeterminate = false;
            accuracyPanel.scanProgress.Visibility = Visibility.Visible;
            yaxisPanel.scanProgress.IsIndeterminate = false;
            yaxisPanel.scanProgress.Visibility = Visibility.Visible;
        }

        private void btnOQCancel_Click(object sender, RoutedEventArgs e)
        {
            if (OQThread != null)
                OQThread.Abort();
            resolutionTest.scanProgress.IsIndeterminate = false;
            resolutionTest.scanProgress.Visibility = Visibility.Visible;
            lineNoisePanel.scanProgress.IsIndeterminate = false;
            lineNoisePanel.scanProgress.Visibility = Visibility.Visible;
            energyDisPanel.scanProgress.IsIndeterminate = false;
            energyDisPanel.scanProgress.Visibility = Visibility.Visible;
            lineSlopeTest.scanProgress.IsIndeterminate = false;
            lineSlopeTest.scanProgress.Visibility = Visibility.Visible;
            tranRepPanel.scanProgress.IsIndeterminate = false;
            tranRepPanel.scanProgress.Visibility = Visibility.Visible;
            waveNumAccPanel.scanProgress.IsIndeterminate = false;
            waveNumAccPanel.scanProgress.Visibility = Visibility.Visible;
            waveNumRepPanel.scanProgress.IsIndeterminate = false;
            waveNumRepPanel.scanProgress.Visibility = Visibility.Visible;
        }
    }
}
