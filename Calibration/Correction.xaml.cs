using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NirIdentifier.Common;
using System.Runtime.InteropServices;

namespace NirIdentifier.Calibration
{
    /// <summary>
    /// Correction.xaml 的交互逻辑
    /// </summary>
    public partial class Correction : UserControl
    {
        Button curSelButton;
        string fileSavePath;
        string ramanSettingFile;

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        private static extern int WritePrivateProfileString(string section, string key, string writeVal, string filePath);

        Ai.Hong.CommonLibrary.SpecFileFormat curScanedFile = null;

        public Correction()
        {
            InitializeComponent();

            //校正文件保存路径
            DateTime date = DateTime.Now;
            fileSavePath = System.IO.Path.Combine(SettingData.settingData.runing_para.savePath, "calibration", date.ToString("yyyy"), date.ToString("MM"), date.ToString("dd"));
            if (!System.IO.Directory.Exists(fileSavePath))
                System.IO.Directory.CreateDirectory(fileSavePath);

            //拉曼配置文件
            ramanSettingFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            ramanSettingFile = System.IO.Path.Combine(ramanSettingFile, "EFTIR\\metage_raman", "metageRamanSettings.ini");

            btnCancel_Click(null, null);    //显示元素界面

        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            NirIdentifier.MainWindow temp = App.Current.MainWindow as NirIdentifier.MainWindow;
            if (temp != null)
                temp.ChangeScreen("Correction", "MainSelectPanel");
        }

        //Offset校正
        private void btnOffsetCorrect_Click(object sender, RoutedEventArgs e)
        {
            btnProcess.Text = "扫描光谱";
            curSelButton = sender as Button;
            stackTotalButton.Visibility = Visibility.Collapsed;
            stackProcessButton.Visibility = Visibility.Visible;
            stackXCalButton.Visibility = Visibility.Collapsed;

            mainDescription.Visibility = Visibility.Collapsed;
            graphicChart.Visibility = Visibility.Collapsed;
            imgPrompt.Visibility = Visibility.Visible;

            Common.CommonMethod.SetImageSource(imgPrompt, "OffsetCalPrompt.png");
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            stackTotalButton.Visibility = Visibility.Visible;
            stackProcessButton.Visibility = Visibility.Collapsed;
            stackXCalButton.Visibility = Visibility.Collapsed;

            mainDescription.Visibility = Visibility.Visible;
            graphicChart.Visibility = Visibility.Collapsed;
            imgPrompt.Visibility = Visibility.Collapsed;
            xCalgraphicChart.Visibility = Visibility.Collapsed;
            imgXCalStandard.Visibility = Visibility.Collapsed;
        }

        //仪器验证
        private void btnAllCal_Click(object sender, RoutedEventArgs e)
        {
            btnProcess.Text = "开始验证";
            curSelButton = sender as Button;
            stackTotalButton.Visibility = Visibility.Collapsed;
            stackProcessButton.Visibility = Visibility.Visible;
            stackXCalButton.Visibility = Visibility.Collapsed;

            mainDescription.Visibility = Visibility.Collapsed;
            graphicChart.Visibility = Visibility.Collapsed;
            imgPrompt.Visibility = Visibility.Visible;

            Common.CommonMethod.SetImageSource(imgPrompt, "XCalPrompt.png");
        }

        private void btnSuccess_Click(object sender, RoutedEventArgs e)
        {
            stackTotalButton.Visibility = Visibility.Visible;
            stackProcessButton.Visibility = Visibility.Collapsed;
            stackXCalButton.Visibility = Visibility.Collapsed;

            mainDescription.Visibility = Visibility.Visible;
            graphicChart.Visibility = Visibility.Collapsed;
            imgPrompt.Visibility = Visibility.Collapsed;
        }

        //X轴校正
        private void btnXCorrect_Click(object sender, RoutedEventArgs e)
        {
            //Raman峰位数量和Pixel峰位数量必须相等并且要大于等于4
            if (SettingData.settingData.x_Correction.ramanPoint.Count != SettingData.settingData.x_Correction.pixelPoint.Count ||
                SettingData.settingData.x_Correction.ramanPoint.Count < 4)
            {
                CommonMethod.ErrorMsgBox("X校正系统参数设置错误，请检查后再操作");
                return;
            }
            btnProcess.Text = "扫描光谱";
            curSelButton = sender as Button;
            stackTotalButton.Visibility = Visibility.Collapsed;
            stackProcessButton.Visibility = Visibility.Visible;

            mainDescription.Visibility = Visibility.Collapsed;
            graphicChart.Visibility = Visibility.Collapsed;
            imgPrompt.Visibility = Visibility.Visible;
            Common.CommonMethod.SetImageSource(imgPrompt, "XCalPrompt.png");
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            if (curSelButton == btnOffsetCorrect || curSelButton == btnXCorrect)
            {
                SampleResultData data = new SampleResultData();
                Common.SettingFile.scanParameter para;
                if (curSelButton == btnOffsetCorrect)
                {
                    data.file = System.IO.Path.Combine(fileSavePath, "Offset" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".spc");
                    para = Common.SettingData.settingData.offset_Correction.scanPara;
                }
                else
                {
                    data.file = System.IO.Path.Combine(fileSavePath, "xAxis" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".spc");
                    para = Common.SettingData.settingData.x_Correction.scanPara;
                }

                para.saveRamanData = false;     //设置为Pixel扫描
                data.drugInfo = null;           //设置为NULL，以便在扫描时不用分析
                Detect.ScanWindow dlg = new Detect.ScanWindow(data, para);
                dlg.Owner = App.Current.MainWindow;
                if (dlg.ShowDialog() == false)
                {
                    CommonMethod.ErrorMsgBox(dlg.ErrorString);
                    return;
                }

                //显示光谱(扫描得到的是_counts.spc)
                curScanedFile = new Ai.Hong.CommonLibrary.SpecFileFormat();
                if (curScanedFile.ReadFile(data.file) == false)
                {
                    CommonMethod.ErrorMsgBox("不能识别的文件格式：" + data.file);
                    return;
                }

                imgPrompt.Visibility = Visibility.Collapsed;

                if (curSelButton == btnOffsetCorrect)   //Offset校正
                {
                    graphicChart.Visibility = Visibility.Visible;
                    graphicChart.DrawGraphic(curScanedFile, System.Drawing.Color.Red);
                }
                else    //X轴校正
                {
                    imgXCalStandard.Visibility = Visibility.Visible;

                    //显示峰位图形
                    xCalgraphicChart.Visibility = Visibility.Visible;
                    xCalgraphicChart.DrawGraphic(curScanedFile, System.Drawing.Color.Red);

                    System.Windows.Point[] pt = new System.Windows.Point[SettingData.settingData.x_Correction.pixelPoint.Count];
                    for (int i = 0; i < SettingData.settingData.x_Correction.pixelPoint.Count; i++)
                    {
                        double newy;
                        pt[i].X = Common.SpectrumAlgorithm.PickPeak(curScanedFile.XDatas, curScanedFile.YDatas, SettingData.settingData.x_Correction.pixelPoint[i], 3, out newy);
                        pt[i].Y = newy;
                    }

                    xCalgraphicChart.DrawPeaks(pt, null, PickOnePeak);
                }
                stackXCalButton.Visibility = Visibility.Visible;
                stackProcessButton.Visibility = Visibility.Collapsed;
            }
            else if (curSelButton == btnAllCal)
            {
                Calibration calDlg = new Calibration();
                calDlg.Owner = App.Current.MainWindow;
                calDlg.ShowDialog();
                btnCancel_Click(null, null);
            }
        }

        //回调函数,由ChartPickPeak调用，标出x位置的峰位
        public System.Windows.Point PickOnePeak(double x)
        {
            System.Windows.Point pt = new System.Windows.Point(0, 0);
            if (curScanedFile != null)
            {
                double newy;
                pt.X = Common.SpectrumAlgorithm.PickPeak(curScanedFile.XDatas, curScanedFile.YDatas, (float)x, 3, out newy);
                pt.Y = newy;
            }

            return pt;
        }

        private string ReadRamanSetting(string section, string key)
        {
            StringBuilder retstr = new StringBuilder(256);

            GetPrivateProfileString(section, key, "0", retstr, 256, ramanSettingFile);
            return retstr.ToString().Trim();
        }

        private void WriteRamanSetting(string section, string key, string writestr)
        {
            WritePrivateProfileString(section, key, writestr, ramanSettingFile);
        }

        //确认X轴校正结果
        private void btnConfirmXCal_Click(object sender, RoutedEventArgs e)
        {
            if (curSelButton == btnOffsetCorrect)   //Offset校正
            {
                float curoffset = curScanedFile.YDatas.Average();
                CommonMethod.InfoMsgBox("偏移量校正完毕\r\n" + "Offset=" + curoffset);
                float oldoffset = float.Parse(ReadRamanSetting("OffsetCalibration", "dcoffset"));
                curoffset += oldoffset;
                WriteRamanSetting("OffsetCalibration", "dcoffset", curoffset.ToString());   //当前offset值
                WriteRamanSetting("OffsetCalibration", "done", "1");    //已做offset校正

                Common.SettingData.settingData.offset_Correction.Done = true;
                Common.SettingData.settingData.offset_Correction.Distance = curoffset;
            }
            else    //X轴校正
            {
                System.Windows.Point[] peaks = xCalgraphicChart.GetPeaks();
                if (peaks.Length != SettingData.settingData.x_Correction.pixelPoint.Count)      //没有标出规定的峰位
                {
                    Common.CommonMethod.ErrorMsgBox("峰位数量错误,峰位数量应该为：" + SettingData.settingData.x_Correction.pixelPoint.Count);
                    return;
                }

                double[] piexlPoints = new double[peaks.Length];
                for (int i = 0; i < peaks.Length; i++)
                    piexlPoints[i] = peaks[i].X;

                int info;
                alglib.barycentricinterpolant p;
                alglib.polynomialfitreport rep;

                //环己烷的理论峰位
                double[] ramanPoints = new double[SettingData.settingData.x_Correction.ramanPoint.Count];
                for (int i = 0; i < SettingData.settingData.x_Correction.ramanPoint.Count; i++)
                    ramanPoints[i] = SettingData.settingData.x_Correction.ramanPoint[i];

                //三阶多项式拟合,得到拟合系数
                alglib.polynomialfit(piexlPoints, ramanPoints, 4, out info, out p, out rep);

                //计算多项式系数,多项式公式：y=c0+c1*x+c2*x^2+c3*x^3,分别带入0,1,2,3得到4个方程式
                double y0 = alglib.barycentriccalc(p, 0);   //y0=a+0b+0c+0d
                double y1 = alglib.barycentriccalc(p, 1);   //y1=a+1b+1c+1d
                double y2 = alglib.barycentriccalc(p, 2);   //y2=a+2b+4c+8d
                double y3 = alglib.barycentriccalc(p, 3);   //y3=a+3b+9c+27d

                //解四元一次方程
                //创建系数矩阵
                double[,] factor = new double[4, 4] { { 1, 0, 0, 0 }, { 1, 1, 1, 1 }, { 1, 2, 4, 8 }, { 1, 3, 9, 27 } };       //系数矩阵

                //求系数矩阵的逆矩阵(A-1)
                alglib.matinvreport factorRep;
                alglib.rmatrixinverse(ref factor, out info, out factorRep);

                //创建结果矩阵
                double[] result = new double[4] { y0, y1, y2, y3 };     //结果矩阵
                double[] multRet = new double[4];

                //系数矩阵的逆矩阵乘上结果矩阵，得到变量矩阵的值
                alglib.rmatrixmv(4, 4, factor, 0, 0, 0, result, 0, ref multRet, 0);

                //记录X校正结果
                SettingData.settingData.x_Correction.Done = true;
                SettingData.settingData.x_Correction.C0 = multRet[0];
                SettingData.settingData.x_Correction.C1 = multRet[1];
                SettingData.settingData.x_Correction.C2 = multRet[2];
                SettingData.settingData.x_Correction.C3 = multRet[3];

                //进行Y轴校正
                if (YAxisCorrect() == false)
                {
                    btnCancel_Click(null, null);
                    return;
                }

                //写入Metage Raman配置文件
                WriteRamanSetting("XCalibration", "done", "1");
                WriteRamanSetting("XCalibration", "c0", multRet[0].ToString());
                WriteRamanSetting("XCalibration", "c1", multRet[1].ToString());
                WriteRamanSetting("XCalibration", "c2", multRet[2].ToString());
                WriteRamanSetting("XCalibration", "c3", multRet[3].ToString("e"));

                string msg = "X轴校正完毕\r\n";
                msg += "C0=" + multRet[0].ToString()+"\r\n";
                msg += "C1=" + multRet[1].ToString()+"\r\n";
                msg += "C2=" + multRet[2].ToString()+"\r\n";
                msg += "C3=" + multRet[3].ToString();

                CommonMethod.InfoMsgBox( msg );

            }

            SettingData.settingData.Serialize(SettingData.IniFileName);     //将结果保存到配置文件

            btnCancel_Click(null, null);

            RamanInstrument.ramanObject = new MetageRamanTypeLib.MetageRamanObject();
        }

        private bool YAxisCorrect()
        {
            try
            {
                //找到共享文档下的Y轴文件，Win7中是：C:\Users\Public\Documents\EFTIR\metage_raman\nistR.spc
                SettingFile.Y_Correction_Parameter yPara = Common.SettingData.settingData.y_Correction;
                string nistRFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
                if(SettingData.settingData.y_Correction.isStandard)     //使用标准物质的校正像素光谱
                    nistRFile = System.IO.Path.Combine(nistRFile, "eftir", "metage_raman", yPara.nistR_file);
                else        //用户指定校正物质的像素光谱（经过CorrectParameterPanel处理过了）
                    nistRFile = System.IO.Path.Combine(nistRFile, "eftir", "metage_raman", yPara.user_nistR_file);

                if (!System.IO.File.Exists(nistRFile))
                    throw new Exception("找不到Y轴校正文件：" + nistRFile+", 请在系统设置中重新设置校正参数");

                /*测试用
                nistRFile = @"F:\软件开发\RFDA\往来资料\校正研究\20120911广东省所\nistR_Pixel.spc";
                SettingData.settingData.x_Correction.C0 = 88.6993612527;
                SettingData.settingData.x_Correction.C1 = 2.74298980447;
                SettingData.settingData.x_Correction.C2 = -0.000672483023999;
                SettingData.settingData.x_Correction.C3 = 9.51962220393e-08;
                */

                Ai.Hong.CommonLibrary.SpecFileFormat nirstRData = new Ai.Hong.CommonLibrary.SpecFileFormat();
                if (nirstRData.ReadFile(nistRFile) == false || nirstRData.XDatas.Length != nirstRData.YDatas.Length)   //确保X和Y的数量相同
                    throw new Exception("不能识别的文件格式：" + nistRFile);
                if (nirstRData.XDatas.Length >= 2000 || nirstRData.XDatas[0] != 0)
                    throw new Exception("不是像素光谱文件：" + nistRFile);

                int xLen = nirstRData.XDatas.Length;
                //先使用X校正后的系数将nirstRData中的X轴转换到波数
                double[] nirstRX = new double[xLen];
                double[] nirstRY = new double[xLen];
                for (int i = 0; i < xLen; i++)
                {
                    nirstRX[i] = ConvertRamanX(nirstRData.XDatas[i]);
                    nirstRY[i] = nirstRData.YDatas[i];
                }

                //对转换为波数的nirstR.spc进行三次样条曲线拟合
                alglib.spline1dinterpolant c;
                alglib.spline1dbuildcubic(nirstRX, nirstRY, out c);

                //对X轴进行插值
                double stepX = nirstRX[xLen - 1] - nirstRX[xLen - 2];    //波数间隔
                int ramanPoints = (int)Math.Ceiling((nirstRX[xLen - 1] - nirstRX[0]) / stepX) + 1;     //Raman谱的X轴点数

                //按照当前X轴校正后的坐标对Y轴重新插值
                double[] newNirstRX = new double[ramanPoints];
                double[] newNirstRY = new double[ramanPoints];
                for (int i = 0; i < newNirstRX.Length; i++)
                {
                    newNirstRX[i] = nirstRX[0] + i * stepX;
                    newNirstRY[i] = alglib.spline1dcalc(c, newNirstRX[i]);
                }

                //使用NIST2241证书中的多项式计算ycal = nistPoly/nistR, X轴的值为新计算得到的newNirstRX
                //double[] coe = new double[] { 9.71937E-02, 2.28325E-04, -5.86762E-08, 2.16023E-10, -9.77171E-14, 1.15596E-17 };
                double[] coe = yPara.coefficient;
                if (coe.Length != 6)
                    throw new Exception("Y轴校正coefficient参数设置错误");

                //计算多项式结果并除以当前Y值, 结果保存在newNirstRY中
                for (int i = 0; i < newNirstRY.Length; i++)
                {
                    newNirstRY[i] = (coe[0] + coe[1] * newNirstRX[i] + coe[2] * Math.Pow(newNirstRX[i], 2) +
                        coe[3] * Math.Pow(newNirstRX[i], 3) + coe[4] * Math.Pow(newNirstRX[i], 4) + coe[5] * Math.Pow(newNirstRX[i], 5)) / newNirstRY[i];
                }

                //取ycal中y轴的最小值，将全部Y轴除以最小值，得到新的ycal光谱
                double minNirstRY = newNirstRY.Min();
                for (int i = 0; i < newNirstRY.Length; i++)
                    newNirstRY[i] = newNirstRY[i] / minNirstRY;

                //将计算得到的newNirstRX和newNirstRY写入光谱YCal.SPC
                nirstRData.XDatas = new float[newNirstRY.Length];
                nirstRData.YDatas = new float[newNirstRY.Length];
                for (int i = 0; i < newNirstRY.Length; i++)
                {
                    nirstRData.XDatas[i] = (float)newNirstRX[i];
                    nirstRData.YDatas[i] = (float)newNirstRY[i];
                }
                nirstRData.Parameter.firstX = newNirstRX[0];
                nirstRData.Parameter.lastX = newNirstRX[newNirstRX.Length - 1];
                nirstRData.Parameter.maxYValue = (float)newNirstRY.Max();
                nirstRData.Parameter.minYValue = (float)newNirstRY.Min();
                nirstRData.Parameter.dataCount = (uint)newNirstRX.Length;
                nirstRData.Parameter.time = DateTime.Now;

                //将结果保存到共享文档下，Win7中是：C:\Users\Public\Documents\EFTIR\metage_raman\ycal.spc
                string yCalFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
                yCalFile = System.IO.Path.Combine(yCalFile, "eftir", "metage_raman", yPara.ycal_file);
                //yCalFile = "f:\\temp\\ycal.spc";
                if (Ai.Hong.CommonLibrary.SPCFile.SaveFile(yCalFile, nirstRData.YDatas, nirstRData.Parameter) == false)
                    throw new Exception("保存文件出现错误：" + yCalFile);

                WriteRamanSetting("YCalibration", "done", "1");
                yPara.Done = true;

                return true;

            }
            catch (Exception ex)
            {
                CommonMethod.ErrorMsgBox(ex.Message);
                return false;
            }
        }

        //通过三次方程将像素坐标转换到拉曼坐标
        private double ConvertRamanX(double x)
        {
            SettingFile.X_Correction_Parameter xPara = SettingData.settingData.x_Correction;
            /*
            xPara.C0 = 88.6993612527;
            xPara.C1 = 2.74298980447;
            xPara.C2 = -0.000672483023999;
            xPara.C3 = 9.51962220393e-08;
             * */
            return xPara.C0 + xPara.C1 * x + xPara.C2 * x * x + xPara.C3 * x * x * x;
        }

    }
}
