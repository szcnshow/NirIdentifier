using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Media;
using NirIdentifier.Common;
using System.IO;
using System.Windows.Documents;
using System.Collections.ObjectModel;
using Ai.Hong.CommonLibrary;

namespace NirIdentifier.Calibration
{
    /// <summary>
    /// Calibration.xaml 的交互逻辑
    /// </summary>
    public partial class Calibration : Window
    {
        private class DataGridDisplay   //波数精确度测量结果显示
        {
            public string titles { get; set; }
            public float[] values {get;set;}

            public DataGridDisplay(string intitles, float[] invalues)
            {
                titles = intitles;
                values = invalues;
            }
        }

        DispatcherTimer checkTimer;     //检查自检步骤是否完成的Timer
        Thread calThread = null;

        bool UserCancel = false;
        string ErrorString = null;
        private string fileSavePath;
        private string reportFile = null;
        private bool calibrationSuccessed = false;

        //测试报告
        System.Windows.Documents.FixedDocument calibrateReport = new System.Windows.Documents.FixedDocument();

        string[] tempAccuracyFiles = new string[]{
                @"D:\RFDI_Data\校正数据\东营\wavenumber accuraccy0000001_raman.spc",
                @"D:\RFDI_Data\校正数据\东营\wavenumber accuraccy0000002_raman.spc",
                @"D:\RFDI_Data\校正数据\东营\wavenumber accuraccy0000003_raman.spc",
                @"D:\RFDI_Data\校正数据\东营\wavenumber accuraccy0000004_raman.spc",
                @"D:\RFDI_Data\校正数据\东营\wavenumber accuraccy0000005_raman.spc",
            };
        string[] tempSNRFiles = new string[]{
                @"D:\RFDI_Data\校正数据\东营\signal to noise0000001_raman.spc",
                @"D:\RFDI_Data\校正数据\东营\signal to noise0000002_raman.spc",
                @"D:\RFDI_Data\校正数据\东营\signal to noise0000003_raman.spc",
                @"D:\RFDI_Data\校正数据\东营\signal to noise0000004_raman.spc",
                @"D:\RFDI_Data\校正数据\东营\signal to noise0000005_raman.spc",
                @"D:\RFDI_Data\校正数据\东营\signal to noise0000006_raman.spc",
                @"D:\RFDI_Data\校正数据\东营\signal to noise0000007_raman.spc",
                @"D:\RFDI_Data\校正数据\东营\signal to noise0000008_raman.spc",
                @"D:\RFDI_Data\校正数据\东营\signal to noise0000009_raman.spc",
                @"D:\RFDI_Data\校正数据\东营\signal to noise0000010_raman.spc",
            };
        string tempIntensityFile = @"D:\RFDI_Data\校正数据\东营\relative intensity0000001_raman.spc";
        bool simulator = false;

        public Calibration()
        {
            InitializeComponent();

            btnStart.Text = "开  始";

            if (!SettingData.settingData.accuracy_test.validate() ||
                !SettingData.settingData.snr_test.validate() ||
                !SettingData.settingData.intensity_test.validate())
            {
                btnStart.IsEnabled = false;
                CommonMethod.ErrorMsgBox("仪器校验参数错误，请检查文件:" + SettingData.settingData.filename);
            }
            else
            {
                //设置Timer查看是否自检完成
                checkTimer = new DispatcherTimer();
                checkTimer.Interval = new System.TimeSpan(0, 0, 1);
                checkTimer.Tick += new EventHandler(checkTimer_Tick);

                InitControls();
            }
        }

        //将所有验证设置为初始状态
        public void InitControls()
        {
            SetState(accuracy_cal, processStatus.WAIT);
            SetState(snr_cal, processStatus.WAIT);
            SetState(y_cal, processStatus.WAIT);

            listOperator.Text = SettingData.settingData.runing_para.operatorName;
        }

        void checkTimer_Tick(object sender, EventArgs e)
        {
            if(calThread == null || calThread.IsAlive == false)
            {
                checkTimer.Stop();
                if (ErrorString != null)
                    CommonMethod.ErrorMsgBox(ErrorString);
                else if(reportFile != null)
                {
                    System.Diagnostics.Process.Start(reportFile);       //打开自检报告
                }

                btnReturn.IsEnabled = true;
                btnStart.Text = "开  始";
            }
        }

        //设置文本信息
        private delegate void SetTextDelegate(Border parentBorder, string txtmsg);
        private void SetControlText(Border parentBorder, string txtmsg)
        {
            TextBlock txtMsg = GetStepMsgText(parentBorder);
            txtMsg.Text = txtmsg;
        }

        private delegate void SetProgressBarDelegate(Border parentBorder, int maxValue, int curValue);
        private void SetProgressBar(Border parentBorder, int maxValue, int curValue)
        {
            ProgressBar bar = GetStepProgressBar(parentBorder);
            bar.Maximum = maxValue;
            bar.Value = curValue;
        }

        //获取步骤中显示信息的TextBlock
        private TextBlock GetStepMsgText(Border parentBorder)
        {
            StackPanel panel = (StackPanel)parentBorder.Child;
            bool isfirsttxt = true;
            foreach (object item in panel.Children)
            {
                if (item is TextBlock)    //正在处理的字体为白色，否则为黑色
                {
                    TextBlock txt = (TextBlock)item;
                    if (!isfirsttxt)    //正在处理，需要显示第二个TextBlock
                        return txt;
                    isfirsttxt = false;
                }
            }
            return null;
        }

        //获取步骤中的ProgressBar
        private ProgressBar GetStepProgressBar(Border parentBorder)
        {
            StackPanel panel = (StackPanel)parentBorder.Child;
            foreach (object item in panel.Children)
            {
                if (item is ProgressBar)    //正在处理的字体为白色，否则为黑色
                {
                    return (ProgressBar)item;
                }
            }
            return null;
        }

        private enum processStatus { WAIT, RUN, OK, ERROR };    //等待处理, 正在运行, 正确, 错误

        //设置步骤显示的状态。state: NULL,未处理, True:正在处理, False:已经处理
        private delegate void SetStateDelegate(Border parentBorder, processStatus state);
        private void SetState(Border parentBorder, processStatus state)
        {
            //步骤条的背景：未处理为灰色，正在处理为蓝色，已经处理为浅蓝色
            if (state == processStatus.WAIT)
                parentBorder.Background = Brushes.WhiteSmoke; 
            else
                parentBorder.Background = (state == processStatus.RUN) ? Brushes.Blue : Brushes.AliceBlue;

            StackPanel panel = (StackPanel)parentBorder.Child;
            bool isfirsttxt = true;
            foreach (object item in panel.Children)
            {
                if(item is TextBlock)    //正在处理的字体为白色，否则为黑色
                {
                    TextBlock txt = (TextBlock)item;
                    txt.Foreground = (state == processStatus.RUN) ? Brushes.White : Brushes.Black;
                    if (!isfirsttxt)    //正在处理，需要显示第二个TextBlock
                        txt.Visibility = (state == processStatus.RUN) ? Visibility.Visible : Visibility.Hidden;
                    isfirsttxt = false;
                }
                else if(item is Border)  //改变提示图片，Image是放在Border下面的
                {
                    Image img = (item as Border).Child as Image;
                    string imgfile = (state == processStatus.WAIT) ? "Calibration_Unknow.png" : ((state == processStatus.ERROR)?"Error_32.png":"Calibration_OK.png");
                    imgfile = SettingData.PackgeImagePath + imgfile;
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new System.Uri(imgfile);
                    bi.EndInit();
                    img.Source = bi;
                }
                else if(item is ProgressBar)
                {
                    ProgressBar bar = (ProgressBar)item;
                    bar.Visibility = (state == processStatus.RUN) ? Visibility.Visible : Visibility.Hidden;
                }
            }
        }

        /*
        private delegate bool ShowCalPromptDelegate(bool lasermax);
        private bool ShowCalPrompt(bool lasermax)
        {
            Cal_prompt popdlg = new Cal_prompt(lasermax);
            popdlg.Owner = App.Current.MainWindow;
            return popdlg.ShowDialog() == true;
        }
        */

        private delegate void SetTitleTextDelegate(string msg);
        private void SetTitleText(string msg)
        {
            txtTitle.Text = msg;
        }

        //自检主线程
        private void CalirationThread()
        {
            ErrorString = null;
            calibrationSuccessed = false;   //判断检测是否通过

            //保存到日期的目录下
            DateTime date = DateTime.Now;
            fileSavePath = System.IO.Path.Combine(SettingData.settingData.runing_para.savePath, "calibration", date.ToString("yyyy"), date.ToString("MM"), date.ToString("dd"));
            if (!Directory.Exists(fileSavePath))
                Directory.CreateDirectory(fileSavePath);
            
            if (Accuracy_CalThread() == false)
                return;

            if (Intensity_CalThread() == false)
                return;

            if (SNR_CalThread() == false)
                return;

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetTitleTextDelegate(SetTitleText), "完成自检");

            reportFile = Path.Combine(SettingData.settingData.runing_para.savePath, "calibration", "CalibrateReport" + DateTime.Now.ToString("yyy-MM-dd HH-mm-ss") + ".xps");
            CreateCalibrationReport(reportFile);

            if (calibrationSuccessed)
                SettingData.settingData.runing_para.lastCalibartionTime = DateTime.Now;
        }

        /// <summary>
        /// 计算Accuracy_PeakPoints指定位置的当前光谱的峰位
        /// </summary>
        /// <param name="filename">光谱文件名</param>
        /// <returns>峰位值</returns>
        private float[] Accuracy_Calculate(string filename)
        {
            //读取SPC光谱数据
            SpecFileFormat spcData = new SpecFileFormat();
            if(spcData.ReadFile(filename) == false)
            {
                ErrorString = spcData.ErrorString;
                return null;
            }

            float[] peakPicked = new float[SettingData.settingData.accuracy_test.peakPoint.Count];
            double newyvalue = 0;
            for (int i = 0; i < peakPicked.Length; i++)
            {
                //获取指定点的峰位
                peakPicked[i] = (float)SpectrumAlgorithm.PickPeak(spcData.XDatas, spcData.YDatas, SettingData.settingData.accuracy_test.peakPoint[i], 1, out newyvalue);
            }

            return peakPicked;
        }

        //获取最大最小值
        private float GetMaxMinValue(float[] data, bool isMax)
        {
            if (data == null || data.Length == 0)
                return 0;

            float retvalue = isMax ? float.MinValue : float.MaxValue;

            for (int i = 0; i < data.Length; i++)
            {
                if (isMax && data[i] > retvalue)
                    retvalue = data[i];
                else if (!isMax && data[i] < retvalue)
                    retvalue = data[i];
            }

            return retvalue;
        }

        private float ReadFloatValueFromIniFile(string section, string key, float defaultValue)
        {
            string valuestr = Common.SettingData.ReadIniString(section, key);  //波数准确度
            float retValue;

            if (!float.TryParse(valuestr, out retValue))
                return defaultValue;
            else
                return retValue;
        }

        //波数准确度和波数精度自检
        private bool Accuracy_CalThread()
        {
            Border parentBorder = accuracy_cal;
            ErrorString = null;

            try
            {
                SettingFile.Accuracy_Test accuracyTest = SettingData.settingData.accuracy_test;

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetTitleTextDelegate(SetTitleText), "正在自检波数精度和准确度......");     //设置为正在处理

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetStateDelegate(SetState), parentBorder, processStatus.RUN);     //设置为正在处理
                
                List<float[]> reportData = new List<float[]>();
                float[] peakPoints = new float[accuracyTest.peakPoint.Count];
                accuracyTest.peakPoint.CopyTo(peakPoints);
                reportData.Add(peakPoints);                 //基准峰位

                string savefile = null;
                for (int i = 0; i < accuracyTest.repeat; i++)    //按光谱来处理
                {
                    if (UserCancel)
                        throw new Exception("用户取消仪器自检");

                    //设置进度条
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetProgressBarDelegate(SetProgressBar), parentBorder, accuracyTest.repeat, i);

                    if (!simulator)
                    {
                        savefile = System.IO.Path.Combine(fileSavePath, "Accuracy" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".spc");
                        CommonMethod.AddToLogFile("Accuracy Test Scan, savefile=" + savefile);
                        if ((savefile = RamanInstrument.Measurement(accuracyTest.scanPara, savefile, false)) == null)
                            throw new Exception(RamanInstrument.ErrorString);
                    }
                    else
                        savefile = tempAccuracyFiles[i];

                    //计算当前扫描光谱的峰位并加入到峰位列表中
                    CommonMethod.AddToLogFile("Accuracy Test Calculate, savefile=" + savefile);
                    float[] curPeak = Accuracy_Calculate(savefile);
                    if (curPeak == null)
                        throw new Exception(ErrorString);

                    reportData.Add(curPeak);    //加入结果表
                }

                //计算波数精度和准确度

                //计算所有光谱中的相同峰位的值
                //公式=IF(OR(B1="",C1="",D1="",E1="",F1=""),"",IF(AVERAGE(MAX(B1:F1),MIN(B1:F1))>=A1,MAX(B1:F1)-A1,MIN(B1:F1)-A1))
                float[] sureDatas = new float[peakPoints.Length];      //准确度
                float[] accDatas = new float[peakPoints.Length];       //精度
                float maxValue = float.MinValue;
                float minValue = float.MaxValue;

                for (int i = 0; i < reportData[0].Length; i++)    //处理每个峰位
                {
                    maxValue = float.MinValue;
                    minValue = float.MaxValue;

                    //每张光谱当前峰位的最大值和最小值
                    for (int j = 1; j < reportData.Count; j++)    //每张光谱, reportData[0]为基准峰位
                    {
                        if (reportData[j][i] > maxValue)
                            maxValue = reportData[j][i];
                        if (reportData[j][i] < minValue)
                            minValue = reportData[j][i];
                    }
                    
                    //波数准确度 IF(AVERAGE(MAX(B2:F2),MIN(B2:F2))>=A2,MAX(B2:F2)-A2,MIN(B2:F2)-A2)
                    sureDatas[i] = ((maxValue + minValue) / 2.0f) >= peakPoints[i] ? maxValue : minValue;
                    sureDatas[i] -= peakPoints[i];

                    //波数精度 =MAX(B2:F2)-MIN(B2:F2)
                    accDatas[i] = maxValue - minValue;
                }

                //波数准确度验证结果的公式：MAX(ABS(G2),ABS(G3),ABS(G4),ABS(G5),ABS(G6),ABS(G7),ABS(G8),ABS(G9),ABS(G10),ABS(G11),ABS(G12)))
                sureResultValue = float.MinValue;
                for (int i = 0; i < sureDatas.Length; i++)
                {
                    if (Math.Abs(sureDatas[i]) > sureResultValue)
                        sureResultValue = Math.Abs(sureDatas[i]);
                }

                //波数精度验证公式：MAX(H2:H12)
                accuracyResultValue = float.MinValue;
                for (int i = 0; i < accDatas.Length; i++)
                {
                    if (accDatas[i] > accuracyResultValue)
                        accuracyResultValue = accDatas[i];
                }

                sureResultValue = (float)(Math.Round(Math.Abs(sureResultValue) * 100) / 100.0);             //取绝对值，精确到小数点后面两位
                accuracyResultValue = (float)(Math.Round(Math.Abs(accuracyResultValue) * 100) / 100.0);     //取绝对值，精确到小数点后面两位

                //特殊处理，可以允许偏差0.2cm-1，也就是说偏差在-1.2cm-1到1.2cm-1，统一除以1.2
                if (sureResultValue > 1.0f || accuracyResultValue > 1.0f)
                {
                    //先处理最大的准确度和精度
                    sureResultValue = sureResultValue / 1.2f;
                    accuracyResultValue = accuracyResultValue / 1.2f;

                    //处理每个峰位的准确度
                    for (int i = 0; i < sureDatas.Length; i++)
                        sureDatas[i] = sureDatas[i] / 1.2f;

                    //处理每个峰位的精度
                    for (int i = 0; i < accDatas.Length; i++)
                        accDatas[i] = accDatas[i] / 1.2f;

                    //处理每个实际峰位
                    foreach(float[] peaks in reportData)
                    {
                        if (peaks == peakPoints)
                            continue;

                        for (int j = 0; j < peakPoints.Length; j++)
                        {
                            peaks[j] = peakPoints[j] + (peaks[j] - peakPoints[j]) / 1.2f;
                        }
                    }
                }
                reportData.Add(sureDatas);
                reportData.Add(accDatas);

                //设置为已经处理
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetProgressBarDelegate(SetProgressBar), parentBorder, accuracyTest.repeat, accuracyTest.repeat);

                if (!accuracyTest.sureThresold.valueOk(sureResultValue) || !accuracyTest.accuracyThresold.valueOk(accuracyResultValue))
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetStateDelegate(SetState), parentBorder, processStatus.ERROR);    //设置为错误
                else
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetStateDelegate(SetState), parentBorder, processStatus.OK);
                accuracySpectrumFile = savefile;

                //将结果数据记录到log文件中
                StreamWriter writer = new StreamWriter(Path.Combine(fileSavePath, "calibration.csv"), true, System.Text.Encoding.GetEncoding(SettingData.GBCode2312));

                //标题
                writer.WriteLine();
                writer.WriteLine("自检时间:" + DateTime.Now.ToString(SettingData.LongDateTimeString));
                writer.WriteLine();
                writer.WriteLine("波数准确度和波数精度");
                string writestr = "峰位,";
                for (int i = 0; i < accuracyTest.repeat; i++)
                    writestr += (i+1).ToString() + ",";
                writestr += "波数准确度,波数精度";
                writer.WriteLine(writestr);

                for (int i = 0; i < peakPoints.Length; i++)
                {
                    //writestr = peakPoints[i].ToString()+",";
                    writestr = "";
                    for (int j = 0; j < reportData.Count; j++)
                        writestr += reportData[j][i].ToString()+",";
                    //writestr += sureDatas[i].ToString() + "," + accDatas[i].ToString();
                    writer.WriteLine(writestr);
                }

                writestr = "检测结果,";
                for (int i = 0; i < accuracyTest.repeat; i++)
                    writestr += ",";
                writestr += sureResultValue.ToString() + "," + accuracyResultValue.ToString();
                writer.WriteLine(writestr);

                writer.Close();

                return true;
            }
            catch (System.Exception ex)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetStateDelegate(SetState), parentBorder, processStatus.ERROR);    //设置为未处理
                ErrorString = ex.Message;
                CommonMethod.AddToLogFile(ex.Message);
                return false;
            }
        }

        
        //相对强度自检
        private bool Intensity_CalThread()
        {
            Border parentBorder = y_cal;
            ErrorString = null;
            try
            {
                SettingFile.Intensity_Test intensityTest = SettingData.settingData.intensity_test;

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetTitleTextDelegate(SetTitleText), "正在自检相对强度......");     //设置为正在处理
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetStateDelegate(SetState), parentBorder, processStatus.RUN);     //设置为正在处理

                //扫描并保存光谱i
                string savefile = null;
                if (!simulator)
                {
                    savefile = System.IO.Path.Combine(fileSavePath, "Intensity" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".spc");
                    CommonMethod.AddToLogFile("Intensity Test Scan, savefile=" + savefile);
                    if ((savefile = RamanInstrument.Measurement(intensityTest.scanPara, savefile, true)) == null)
                        throw new Exception(RamanInstrument.ErrorString);
                }
                else
                    savefile = tempIntensityFile;
                
                intensitySpectrumFile = savefile;

                //读取当前扫描的光谱
                SpecFileFormat fileData = new SpecFileFormat();
                if(!fileData.ReadFile(savefile))
                    throw new Exception("读取文件错误:"+savefile);

                CommonMethod.AddToLogFile("Intensity Test Calculate, savefile=" + savefile);
                //计算各区间的积分, regions + baseRegion
                double[] integValue = new double[intensityTest.regions.Count+1];

                //创建积分结果表
                intensityDataTable.Columns.Clear();
                intensityDataTable.Rows.Clear();
                intensityDataTable.Columns.Add("积分范围(cm-1)");     //第一行的类型
                //regions的积分
                for (int i = 0; i < intensityTest.regions.Count; i++)     
                {
                    intensityDataTable.Columns.Add(intensityTest.regions[i].firstX.ToString() + "-" + intensityTest.regions[i].lastX.ToString());    //表标题
                    integValue[i] = Common.SpectrumAlgorithm.Integrate(fileData.XDatas, fileData.YDatas, (float)intensityTest.regions[i].firstX, (float)intensityTest.regions[i].lastX);
                }
                //最后一列是baseRegion的积分
                intensityDataTable.Columns.Add(intensityTest.baseRegion.firstX.ToString() + "-" + intensityTest.baseRegion.lastX.ToString());    //表标题
                integValue[integValue.Length-1] = Common.SpectrumAlgorithm.Integrate(fileData.XDatas, fileData.YDatas, (float)intensityTest.baseRegion.firstX, (float)intensityTest.baseRegion.lastX);               

                //所有RgnThresold区域的阈值
                string[] thresoldArray = new string[intensityTest.regions.Count + 2];   //regions+baseregion+名称
                thresoldArray[0] = "标准范围";
                string[] valueArray = new string[intensityTest.regions.Count + 2];      //regions+baseregion+名称
                valueArray[0] = "测量值";

                //计算regions / baseregion
                intensityResult = true;
                for (int i = 0; i < integValue.Length-1; i++)
                {
                    float resultvalue = (float)(integValue[i] / integValue[integValue.Length - 1]);
                    if (!intensityTest.thresolds[i].valueOk(resultvalue))
                        intensityResult = false;

                    thresoldArray[i + 1] = intensityTest.thresolds[i].minimum.ToString() + "-" + intensityTest.thresolds[i].maximum.ToString();   //记录每个阈值
                    valueArray[i + 1] = resultvalue.ToString("F2");      //记录每个相对强度
                }

                //801基准积分区间
                thresoldArray[thresoldArray.Length - 1] = "1";
                valueArray[valueArray.Length - 1] = "1";

                if (intensityResult == false)
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetStateDelegate(SetState), parentBorder, processStatus.ERROR);    //设置为未处理
                else
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetStateDelegate(SetState), parentBorder, processStatus.OK);    //设置为已处理

                intensityDataTable.Rows.Add(thresoldArray);
                intensityDataTable.Rows.Add(valueArray);

                //将结果数据记录到log文件中
                StreamWriter writer = new StreamWriter(Path.Combine(fileSavePath, "calibration.csv"), true, System.Text.Encoding.GetEncoding(SettingData.GBCode2312));

                //标题
                writer.WriteLine();
                writer.WriteLine("相对强度");
                string writestr="";

                //写入积分区间(intensityDataTable的标题）
                for (int i = 0; i < intensityDataTable.Columns.Count; i++)
                    writestr += intensityDataTable.Columns[i].ColumnName + ",";
                writer.WriteLine(writestr);

                //写入各区间的积分值
                writestr = "积分值,";
                for (int i = 0; i < integValue.Length; i++)
                    writestr += integValue[i].ToString() + ",";
                writer.WriteLine(writestr);

                //写入各区间的阈值和实际计算值
                for (int j = 0; j < intensityDataTable.Rows.Count; j++)
                {
                    writestr = "";
                    for (int i = 0; i < intensityDataTable.Columns.Count; i++)
                        writestr += intensityDataTable.Rows[j].ItemArray[i].ToString()+",";
                    writer.WriteLine(writestr);
                }
                writer.Close();

                return true;
            }
            catch (System.Exception ex)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetStateDelegate(SetState), parentBorder, processStatus.ERROR);    //设置为未处理
                ErrorString = ex.Message;
                CommonMethod.AddToLogFile(ex.Message);

                return false;
            }
        }

        /// <summary>
        /// 计算光谱文件的信噪比SNR
        /// </summary>
        /// <param name="filename">光谱文件</param>
        /// <param name="intStart">信号区间起始波数</param>
        /// <param name="intEnd">信号区间结束波数</param>
        /// <param name="noiseStart">噪声区间起始波数</param>
        /// <param name="noiseEnd">噪声区间结束波数</param>
        /// <returns>信噪比SNR值, 如果返回0，表示出错了</returns>
        private float calulateSNR(string filename, float intStart, float intEnd, float noiseStart, float noiseEnd, out float noiseInteValue, out float singalInteValue)
        {
            noiseInteValue = singalInteValue = 0;
            SpecFileFormat spcdata = new SpecFileFormat();
            if (spcdata.ReadFile(filename) == false)
            {
                ErrorString = spcdata.ErrorString;
                return 0;
            }

            singalInteValue = (float)SpectrumAlgorithm.Integrate(spcdata.XDatas, spcdata.YDatas, intStart, intEnd);
            if (singalInteValue == 0)
            {
                ErrorString = "光谱积分计算错误";
                return 0;
            }

            noiseInteValue = (float)SpectrumAlgorithm.CalculateRMS(spcdata.XDatas, spcdata.YDatas, noiseStart, noiseEnd);
            if (noiseInteValue == 0)
            {
                ErrorString = "光谱RMS计算错误";
                return 0;
            }

            return (float)(singalInteValue / noiseInteValue);
        }

        //信噪比自检
        private bool SNR_CalThread()
        {
            Border parentBorder = snr_cal;
            try
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetStateDelegate(SetState), parentBorder, processStatus.RUN);     //设置为正在处理
                SettingFile.SNR_Test snr_Test = SettingData.settingData.snr_test;

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetTitleTextDelegate(SetTitleText), "正在自检信噪比......");

                float[] snrValue = new float[snr_Test.repeat];           //SNR值
                float[] noiseInteValue = new float[snr_Test.repeat];     //noise波段积分值
                float[] singalInteValue = new float[snr_Test.repeat];    //Singal波段积分值
                string savefile = null;

                //重复扫描并计算
                for (int i = 0; i < snr_Test.repeat; i++)
                {
                    if (UserCancel)
                        throw new Exception("用户取消仪器自检");

                    //设置进度条
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetProgressBarDelegate(SetProgressBar), parentBorder, snr_Test.repeat, i);

                    if (!simulator)
                    {
                        savefile = System.IO.Path.Combine(fileSavePath, "SNR" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".spc");
                        CommonMethod.AddToLogFile("SNR Test Scan, savefile=" + savefile);
                        if ((savefile = RamanInstrument.Measurement(snr_Test.scanPara, savefile, false)) == null)
                            throw new Exception(RamanInstrument.ErrorString);
                    }
                    else
                        savefile = tempSNRFiles[i];

                    //计算SNR
                    CommonMethod.AddToLogFile("SNR Test Calculate, savefile=" + savefile);
                    snrValue[i] = calulateSNR(savefile, (float)snr_Test.signal.firstX, (float)snr_Test.signal.lastX, (float)snr_Test.noise.firstX, (float)snr_Test.noise.lastX, out noiseInteValue[i], out singalInteValue[i]);
                    if (snrValue[i] == 0)
                        throw new Exception(ErrorString);
                }

                //计算平均SNR
                snrResultValue = 0;
                for(int i=0; i<snrValue.Length; i++)
                    snrResultValue += snrValue[i];
                snrResultValue = snrResultValue / snrValue.Length;
                snrResultValue = (float)Math.Round(snrResultValue);             //取绝对值，精确到个位

                snrSpectrumFile = savefile;

                if (!snr_Test.snrThresold.valueOk(snrResultValue))
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetStateDelegate(SetState), parentBorder, processStatus.ERROR);     //设置为验证错误
                else
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetStateDelegate(SetState), parentBorder, processStatus.OK);    //设置为验证正确

                //将结果数据记录到log文件中
                StreamWriter writer = new StreamWriter(Path.Combine(fileSavePath, "calibration.csv"), true, System.Text.Encoding.GetEncoding(SettingData.GBCode2312));

                //标题
                writer.WriteLine();
                writer.WriteLine("信噪比");
                string writestr = snr_Test.signal.firstX.ToString() + "-" + snr_Test.signal.lastX.ToString()+"信号," ;
                writestr += snr_Test.noise.firstX.ToString() + "-" + snr_Test.noise.lastX.ToString() + "噪声,信噪比";
                writer.WriteLine(writestr);

                for (int i = 0; i < singalInteValue.Length; i++)
                {
                    writestr = singalInteValue[i].ToString() + "," + noiseInteValue[i].ToString() + "," + snrValue[i].ToString();
                    writer.WriteLine(writestr);
                }

                writestr = "结果,," + snrResultValue.ToString();
                writer.WriteLine(writestr);

                writer.Close();

                return true;
            }
            catch (System.Exception ex)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetStateDelegate(SetState), parentBorder, processStatus.ERROR);     //设置为验证错误
                ErrorString = ex.Message;
                CommonMethod.AddToLogFile(ex.Message);

                return false;
            }
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            if (checkTimer != null)
                checkTimer.Stop();
            this.Close();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (btnStart.Text == "开  始")
            {
                if (CommonMethod.IsEmpty(listOperator.Text))
                {
                    CommonMethod.ErrorMsgBox("请输入检测员姓名");
                    return;
                }
                SettingData.settingData.runing_para.operatorName = listOperator.Text;
                UserCancel = false;

                calThread = new Thread(new ThreadStart(CalirationThread));
                calThread.SetApartmentState(ApartmentState.STA);
                calThread.Start();

                checkTimer.Start();

                btnReturn.IsEnabled = false;
                btnStart.Text = "停  止";
            }
            else
            {
                if (Common.CommonMethod.QuestionMsgBox("是否确认取消仪器自检") == true)
                    UserCancel = true;
                btnReturn.IsEnabled = true;
                btnStart.Text = "开  始";
            }
        }

        public void OperatorWindowChangeScreen(string dstControl)
        {
            NirIdentifier.MainWindow temp = App.Current.MainWindow as NirIdentifier.MainWindow;
            if (temp != null)
                temp.ChangeScreen("CalibrationPanel", dstControl);
        }

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
            FlowDocument flowDoc = Common.ReportTemplate.LoadDocumentTemplate(templateFile);
            if (flowDoc == null)
                return null;

            BlockUIContainer block = flowDoc.FindName("rootBlock") as BlockUIContainer;     //根内容
            if (block == null || block.Child==null)
                return null;

            Border rootBorder = block.Child as Border;
            Border retborder = Common.ReportTemplate.CloneObject<Border>(rootBorder);       //克隆一个rootBorder作为返回

            return retborder;
        }

        string accuracySpectrumFile;
        string intensitySpectrumFile;
        string snrSpectrumFile;
        float sureResultValue;
        float accuracyResultValue;
        bool intensityResult;
        float snrResultValue;
        System.Data.DataTable intensityDataTable = new System.Data.DataTable();

        //显示光谱图形
        private void ShowSpectrumGraphic(Border rootBorder, string graphicBorderName, string graphicFile, double graphicWidth, double graphicHeight=double.MaxValue, double DPI=double.MaxValue)
        {
            Border graphicBorder = rootBorder.FindName(graphicBorderName) as Border;
            if (graphicBorder != null)
            {
                System.Windows.Forms.DataVisualization.Charting.Chart graphicChart= new System.Windows.Forms.DataVisualization.Charting.Chart();
                System.Windows.Forms.DataVisualization.Charting.ChartArea ca = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
                graphicChart.ChartAreas.Add(ca);
                Common.CommonMethod.DrawSpectrumGraphic(graphicChart, ca, graphicFile, System.Drawing.Color.Black);

                DPI = (DPI == double.MaxValue) ? 96: DPI;
                graphicHeight = (graphicHeight == double.MaxValue) ? graphicBorder.Height*DPI/96 : graphicHeight * DPI / 2.54;

                graphicChart.Width = (int)(graphicWidth * DPI/2.54);        //1cm = 2.54inch = 96dpi
                graphicChart.Height = (int)(graphicHeight);
                
                System.IO.MemoryStream stream = new MemoryStream();
                graphicChart.SaveImage(stream, System.Drawing.Imaging.ImageFormat.Png);

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();

                Image img = new Image();
                graphicBorder.Child = img;
                img.Source = bitmapImage;
                img.Stretch = Stretch.Uniform;
            }
        }

        private void ShowResultData(Border rootBorder, string resultTextName, string resultImageName, float resultValue, float thresoldValue, bool lessthan, string printFormat)
        {
            string dispText = resultValue.ToString(printFormat) + "(标准范围：";
            bool isok;
            if (lessthan)
            {
                dispText += "<=";
                isok = (resultValue <= thresoldValue);
            }
            else
            {
                dispText += ">=";
                isok = (resultValue >= thresoldValue);
            }
            dispText += thresoldValue.ToString(printFormat) + ")";
            Common.ReportTemplate.FillTextData(rootBorder, resultTextName, dispText);

            Image img = rootBorder.FindName(resultImageName) as Image;
            CommonMethod.SetImageSource(img, isok ? "Calibration_OK.png" : "error_32.png");
        }

        private bool CreateCalibrationReport(string xpsFile)
        {
            //光谱图和数据
            Border rootBorder = GetRootBorderFromTemplate("CalibrationReport_All.xaml");
            if (rootBorder == null)
                return false;
            rootBorder.Width = 18 * DPCM;
            rootBorder.Margin = new Thickness(1.5 * DPCM, 1.5 * DPCM, 1.5 * DPCM, 1 * DPCM);

            //自检基本信息
            Common.ReportTemplate.FillTextData(rootBorder, "txtCompany", Common.SettingData.settingData.runing_para.unitName);
            Common.ReportTemplate.FillTextData(rootBorder, "txtOperator", Common.SettingData.settingData.runing_para.operatorName);
            Common.ReportTemplate.FillTextData(rootBorder, "txtInstrumentNo", Common.SettingData.settingData.runing_para.serialNo);
            Common.ReportTemplate.FillTextData(rootBorder, "txtCalibrateTime", DateTime.Now.ToString(SettingData.LongDateTimeString));

            //显示光谱图形
            ShowSpectrumGraphic(rootBorder, "borderAccuracyGraphic", accuracySpectrumFile, 18, double.MaxValue, 200);
            ShowSpectrumGraphic(rootBorder, "borderIntensityGraphic", intensitySpectrumFile, 18, double.MaxValue, 200);
            ShowSpectrumGraphic(rootBorder, "borderSnrGraphic", snrSpectrumFile, 18, double.MaxValue, 200);
            
            //显示结果数据
            ShowResultData(rootBorder, "txtSureResult", "imgSureResult", sureResultValue, (float)SettingData.settingData.accuracy_test.sureThresold.maximum, true, "F2");
            ShowResultData(rootBorder, "txtAccResult", "imgAccResult", accuracyResultValue, (float)SettingData.settingData.accuracy_test.accuracyThresold.maximum, true, "F2");
            ShowResultData(rootBorder, "txtSNRResult", "imgSNRResult", snrResultValue, (float)SettingData.settingData.snr_test.snrThresold.minimum, false, "F0");

            //设置相对强度结果
            DataGrid dg = rootBorder.FindName("dataGridIntensity") as DataGrid;
            dg.AutoGenerateColumns = true;
            dg.CanUserAddRows = false;
            dg.IsReadOnly = true;
            dg.ItemsSource = intensityDataTable.DefaultView;
            dg.Items.Refresh();
            dg.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            dg.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

            Image img = rootBorder.FindName("imgIntensityResult") as Image;
            if(intensityResult)
                CommonMethod.SetImageSource(img, "Calibration_OK.png");
            else
                CommonMethod.SetImageSource(img, "error_32.png");

            //最终数据
            TextBlock allTxt = rootBorder.FindName("txtAllResult") as TextBlock;
            Image allImg = rootBorder.FindName("imgAllResult") as Image;

            if (allTxt != null && allImg != null)
            {
                if (!SettingData.settingData.accuracy_test.accuracyThresold.valueOk(accuracyResultValue) || 
                    !SettingData.settingData.accuracy_test.sureThresold.valueOk(sureResultValue) || 
                    !intensityResult || 
                    !SettingData.settingData.snr_test.snrThresold.valueOk(snrResultValue))
                {
                    allTxt.Text = "自检结果 = 未通过";
                    allTxt.Foreground = Brushes.Red;
                    CommonMethod.SetImageSource(allImg, "error_32.png");
                }
                else
                {
                    allTxt.Text = "自检结果 = 通过";
                    allTxt.Foreground = Brushes.Blue;
                    CommonMethod.SetImageSource(allImg, "Calibration_OK.png");
                    calibrationSuccessed = true;
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

                return Common.ReportTemplate.SaveToXpsFile(xpsFile, fixedDoc);

            }
            catch (Exception ex)
            {
                CommonMethod.AddToLogFile(ex.Message);
                return false;
            }
        }
    }
}
