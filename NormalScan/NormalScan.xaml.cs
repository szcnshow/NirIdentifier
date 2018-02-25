using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using NirIdentifier.Common;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Ai.Hong.CommonLibrary;

namespace NirIdentifier.NormalScan
{
    /// <summary>
    /// NormalScan.xaml 的交互逻辑
    /// </summary>
    public partial class NormalScan : UserControl
    {
        private SettingFile.scanParameter curScanPara =new SettingFile.scanParameter ();// SettingData.settingData.runing_para.scanPara.Clone();
        private string backFile = "";
        public NormalScan()
        {
            InitializeComponent();
            listFiles.SetGraphicChart(graphicChart);
            listFiles.SetGridData(scanedFiles);

            
        }

        /// <summary>
        /// 光谱列表
        /// </summary>
        static ObservableCollection<spectrumDisplayInfo> scanedFiles = new ObservableCollection<spectrumDisplayInfo>();

        /// <summary>
        /// 加载光谱
        /// </summary>
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "仪器光谱(*.SPC)|*.SPC|其它光谱(*.*)|*.*";
            dlg.Multiselect = true;
            dlg.Title = "加载光谱文件";

            if (dlg.ShowDialog() == true)
            {
                foreach (string file in dlg.FileNames)
                {
                    spectrumDisplayInfo newdata = new spectrumDisplayInfo(file, spectrumDisplayInfo.GetDisplayColor(scanedFiles.Count));
                    scanedFiles.Add(newdata);
                }
            }
        }

        /// <summary>
        /// 移除光谱
        /// </summary>
        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            listFiles.RemoveSelected();
        }

        /// <summary>
        /// 当前扫描信息
        /// </summary>
        private ScanTaskInfo curScanTaskInfo = null;

        /// <summary>
        ///  光谱扫描回调函数，在ProcessWaitDialog中调用
        /// </summary>
        private bool ScanSampleTask(ProcessWaitDialog.SetProcessAndMsgDeletage callBack)
        {
            if (curScanTaskInfo == null)
                return false;

            //创建扫描线程
            System.Threading.Thread scanThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(curScanTaskInfo.DoScan));
            scanThread.IsBackground = true;
            scanThread.Start(curScanPara.scanCount);
            int curTime = Environment.TickCount;
            string msgStr = "正在扫描" + (curScanTaskInfo.isBackground ? "背景" : "样品") + "......";

            //等待扫描完成
            while (scanThread.IsAlive)
            {
                //假定一次扫描需要2秒
                if (Environment.TickCount - curTime > curScanTaskInfo.scanPara.scanCount * 2000)
                    return false;

                System.Threading.Thread.Sleep(1000);
                bool abortTask = false;
                if (Environment.TickCount - curTime < (int)curScanTaskInfo.scanPara.scanCount * 2000)
                    callBack(msgStr, (int)curScanTaskInfo.scanPara.scanCount * 2000, Environment.TickCount - curTime, out abortTask);

                //用户取消扫描, 需要根据实际调整
                if (abortTask == true)
                {
                    scanThread.Abort();
                    curScanTaskInfo.scanSuccessed = false;
                    break;
                }
            }
            if (!curScanTaskInfo.scanSuccessed)
            {
                //  MessageBox.Show(curScanTaskInfo.ErrorString);
            }

            return curScanTaskInfo.scanSuccessed;
        }


        private string ScanSpectrum(bool isBackground)
        {
            if (isBackground&&!(bool)VspecInstrument.IsIntegratingSphere)
            {
                if (Common.CommonMethod.QuestionMsgBox("请确认背景测量标样放置到测量台上") == false)
                    return null;
            }
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "仪器光谱(*.SPC)|*.SPC";
            dlg.Title = "保存光谱文件";
            if (dlg.ShowDialog() == true)
            {
                //创建扫描任务并显示扫描等待窗口，开始扫描
                curScanTaskInfo = new ScanTaskInfo(curScanPara, isBackground, dlg.FileName);

                ProcessWaitDialog waitDlg = new ProcessWaitDialog(ScanSampleTask, "取消扫描");
                waitDlg.Owner = App.Current.MainWindow;
                // bool successed = (bool)
                waitDlg.ShowDialog();
                if (isBackground)
                {
                    MainWindow mainWnd = App.Current.MainWindow as MainWindow;
                    mainWnd.BackgroundScaned = curScanTaskInfo.scanSuccessed;
                }

                if (!curScanTaskInfo.scanSuccessed)
                {
                    Common.CommonMethod.ErrorMsgBox(curScanTaskInfo.ErrorString);
                    return null;
                }
                else
                {
                    if (isBackground)
                    {
                        backFile = dlg.FileName;
                    }
                    if (File.Exists(backFile) && !isBackground)
                    {

                        Ai.Hong.CommonLibrary.SpecFileFormatDouble sampleData1 = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
                        Ai.Hong.CommonLibrary.SpecFileFormatDouble sampleData2 = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();

                        if (sampleData1.ReadFile(backFile) == false)
                        {
                            MessageBox.Show("读取光谱错误:" + backFile + " " + sampleData1.ErrorString);
                            return null;
                        }
                        if (sampleData2.ReadFile(dlg.FileName) == false)
                        {
                            MessageBox.Show("读取光谱错误:" + dlg.FileName + " " + sampleData2.ErrorString);
                            return null;
                        }
                        if (sampleData1.YDatas.Length != sampleData2.YDatas.Length)
                        {
                            MessageBox.Show("样品和背景不一致，请重新扫描背景！");
                            return null;
                        }
                        //计算样品1和样品2比值,得到100%线
                        for (int index = 0; index < sampleData1.Parameter.dataCount; index++)
                        {
                            if (sampleData2.YDatas[index] == 0)
                                sampleData2.YDatas[index] = 1;
                            else
                                sampleData2.YDatas[index] = Math.Abs(sampleData2.YDatas[index] / sampleData1.YDatas[index]);
                        }

                        float[] trData = new float[sampleData2.YDatas.Length];
                        for (int index = 0; index < trData.Length; index++)
                        {
                            trData[index] = (float)Math.Log10(1 / sampleData2.YDatas[index]);
                        }
                        Ai.Hong.CommonLibrary.SPCFile.SaveFile(dlg.FileName, trData, sampleData2.Parameter);
                    }
                    return dlg.FileName;
                }
            }
            else
                return null;
        }

        /// <summary>
        /// 扫描背景
        /// </summary>
        private void btnScanBackground_Click(object sender, RoutedEventArgs e)
        {
            if (!System.IO.File.Exists(curScanPara.scanSettingFile))
            {
                MessageBox.Show("请加载参数文件(*.vspec_nir_ini)");
                return;
            }
            string backFile = ScanSpectrum(true);
            if (backFile != null)
            {
                if (!File.Exists(backFile))
                    backFile = backFile.Replace(".spc", "_rsb.spc");
                spectrumDisplayInfo newdata = new spectrumDisplayInfo(backFile, spectrumDisplayInfo.GetDisplayColor(scanedFiles.Count)) { isChecked = true };
                newdata.isChecked = true;
                scanedFiles.Add(newdata);
            }
            else
            {
                // MessageBox.Show(curScanTaskInfo.ErrorString);
            }
        }

        /// <summary>
        /// 扫描样品
        /// </summary>
        private void btnStartScan_Click(object sender, RoutedEventArgs e)
        {
            string scanfile = ScanSpectrum(false);
            if (scanfile != null)
            {
                if (!File.Exists(scanfile))
                    scanfile = scanfile.Replace(".spc", "_abs.spc");
                spectrumDisplayInfo newdata = new spectrumDisplayInfo(scanfile, spectrumDisplayInfo.GetDisplayColor(scanedFiles.Count)) { isChecked = true };
                if (newdata.fileData.XDatas != null && newdata.fileData.YDatas != null)
                    scanedFiles.Add(newdata);
            }
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        private void btnSettingBrowse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "配置文件(*.vspec_nir_ini)|*.vspec_nir_ini";
            dlg.Multiselect = false;
            dlg.Title = "加载配置文件";

            if (dlg.ShowDialog() == true)
            {
                curScanPara.scanSettingFile = dlg.FileName;
                gridPara.DataContext = null;
                gridPara.DataContext = curScanPara;
                iniFile.Text = System.IO.Path.GetFileName(dlg.FileName);

            }
        }

        private static string text = "";
        /// <summary>
        /// 预处理输入字符是否为数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void boxInt_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox put = sender as TextBox;
            if (!isNumberic(e.Text, put))
            {
                MessageBox.Show("非法字符！");
                e.Handled = true;
                while (true)
                {
                    if (!isNumberic(put.Text, put))
                    {
                        if (string.IsNullOrEmpty(put.Text))
                            break;
                        put.Text = put.Text.Substring(0, put.Text.Length - 1);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                e.Handled = false;
                text = put.Text;
            }
        }
        //isDigit是否是数字
        private static bool isNumberic(string _string, TextBox put)
        {
            if (string.IsNullOrEmpty(_string))
                return false;

            //if (put.Text != "")
            //{
            //    if (put.Text.IndexOf('.') != -1 && _string == ".")
            //    {
            //        MessageBox.Show("非法字符！");
            //        return false;
            //    }
            //}
            foreach (char c in _string)
            {
                if (!char.IsDigit(c))
                {
                    //if (c != '.')
                    //{
                   
                   // put.Text = text;
                    return false;
                    //}
                }

                //if(c<'0' c="">'9')//最好的方法,在下面测试数据中再加一个0，然后这种方法效率会搞10毫秒左右
            }
            return true;
        }

        /// <summary>
        /// 判断剪贴板数据是否为数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Paste_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            try
            {
                Convert.ToDouble(System.Windows.Clipboard.GetText());
            }
            catch
            {
                e.Handled = true;
                MessageBox.Show("粘贴失败！", "提示");
            }
        }

      

    }
}
