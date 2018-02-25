using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;
using System.Windows.Xps.Serialization;
using System.Windows.Documents;
using System.IO.Packaging;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Serialization;
using Ai.Hong.CommonLibrary;

namespace NirIdentifier.Common
{
    [XmlRoot("SettingFile")]
    public class SettingFile:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void DoProperChange(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        //扫描参数
        public class scanParameter : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            private void DoProperChange(string name)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

            public string _scanSettingFile;
            /// <summary>
            /// 通用扫描参数文件
            /// </summary>
            [XmlElement("scanSettingFile")]
            public string scanSettingFile
            {
                get { return _scanSettingFile; }
                set
                {
                    if (!File.Exists(value)) return;
                    _scanSettingFile = value;
                    _scanCount=GetScanCountFromIni(value, "Collection", "sampleScans");
                    _resolution = GetScanCountFromIni(value,  "Collection", "resolution");
                    _firstX = GetScanCountFromIni(value,  "Process", "firstX");
                    _lastX = GetScanCountFromIni(value,  "Process", "lastX");
                }
            }

            public int _scanCount;
            /// <summary>
            /// 扫描次数
            /// </summary>
            [XmlElement("scanCount")]
            public int scanCount
            {
                get { return _scanCount; }
                set
                {
                    _scanCount = value;
                    if (File.Exists(_scanSettingFile))
                    {
                        Ai.Hong.CommonMethod.WriteIniFile(_scanSettingFile, "Collection", "backgroundScans", value.ToString());
                        Ai.Hong.CommonMethod.WriteIniFile(_scanSettingFile, "Collection", "sampleScans", value.ToString());
                    }
                    DoProperChange("scanCount");
                }
            }

            public double _resolution;
            /// <summary>
            /// 分辨率
            /// </summary>
            public double resolution
            { get { return _resolution; }
                set 
                { 
                    _resolution = value;
                    if (File.Exists(_scanSettingFile))
                        Ai.Hong.CommonMethod.WriteIniFile(_scanSettingFile, "Collection", "resolution", value.ToString());
                    DoProperChange("resolution");

                }
            }

            public double _firstX;
            public double firstX
            {
                get { return _firstX; }
                set
                {
                    _firstX = value;
                    if (File.Exists(_scanSettingFile))
                        Ai.Hong.CommonMethod.WriteIniFile(_scanSettingFile, "Process", "firstX", value.ToString());
                    DoProperChange("firstX");
                }
            }
            public double _lastX;
            public double lastX
            {
                get { return _lastX; }
                set
                {
                    _lastX = value;
                    if (File.Exists(_scanSettingFile))
                        Ai.Hong.CommonMethod.WriteIniFile(_scanSettingFile, "Process", "lastX", value.ToString());
                    DoProperChange("lastX");
                }
            }

            public scanParameter()
            {
            }

            public scanParameter(string file, int count)
            {
                scanSettingFile = file;
                scanCount = count;
            }

            public scanParameter Clone()
            {
                return (scanParameter)MemberwiseClone();
            }

            /// <summary>
            /// 从配置文件中获取扫描次数
            /// </summary>
            public static int GetScanCountFromIni(string iniFile,string header,string readString)
            {
                string inistring;
                //if (isBackground)
                //    inistring = Ai.Hong.CommonMethod.ReadIniFile(iniFile, "Collection", "backgroundScans");
                //else
                    inistring = Ai.Hong.CommonMethod.ReadIniFile(iniFile,header , readString);
                    try
                    {
                        return (int)Convert.ToDouble(inistring);
                    }
                    catch
                    {
                        MessageBox.Show("ini文件错误！");
                        return -1;
                    }
            }
        }

        //运行参数
        public class Runing_para:INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            private void DoProperChange(string name)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

            /// <summary>
            /// 是否模拟扫描
            /// </summary>
            [XmlAttribute("isSimulator")]
            public bool isSimulator { get; set; }

            /// <summary>
            /// 数据保存目录
            /// </summary>
            [XmlElement("savePath")]
            public string savePath { get; set; }

            /// <summary>
            /// 数据目录创建方式
            /// </summary>
            [XmlElement("savePathType")]
            public EnumSavePathType savePathType { get; set; }

            /// <summary>
            /// 启动调入的数据库
            /// </summary>
            [XmlElement("database")]
            public string database { get; set; }

            /// <summary>
            /// 需要自检
            /// </summary>
            [XmlAttribute("needCalibration")]
            public bool needCalibration { get; set; }

            /// <summary>
            /// 自检间隔
            /// </summary>
            [XmlAttribute("calibartionTime")]
            public int calibartionTime { get; set; }

            /// <summary>
            /// 上次自检时间
            /// </summary>
            [XmlAttribute("lastCalibartionTime")]
            public DateTime lastCalibartionTime { get; set; }

            /// <summary>
            /// 检测单位
            /// </summary>
            [XmlAttribute("Unit")]
            public string unitName { get; set; }

            /// <summary>
            /// 操作员
            /// </summary>
            [XmlAttribute("Operator")]
            public string operatorName { get; set; }

            /// <summary>
            /// 仪器序列号
            /// </summary>
            [XmlAttribute("SerialNo")]
            public string serialNo { get; set; }

            /// <summary>
            /// 仪器超时设定(分钟)
            /// </summary>
            [XmlAttribute("InstrumentTimeOut")]
            public int InstrumentTimeOut { get; set; }

            /// <summary>
            /// 仪器硬件USB设备ID号
            /// </summary>
            [XmlElement("usbDeviceID")]
            public string usbDeviceID { get; set; }

            /// <summary>
            /// 是否调试状态
            /// </summary>
            [XmlAttribute("isDebug")]
            public bool isDebug { get; set; }

            /// <summary>
            /// 扫描参数
            /// </summary>
            [XmlElement("scanParameter")]
            public scanParameter scanPara { get; set; }

            //初始化属性
            public Runing_para()
            {
                savePath = "d:\\RFDI_DATA";
                database = "NirIdentifier_DB.mdb";// "RFDI_DB.mdb";
                needCalibration = true;
                calibartionTime = 24;
                unitName = "未知单位";
                operatorName = "未知";
                serialNo = "";
                isSimulator = false;
                scanPara = new scanParameter();
            }
        }

        public class Dictionary
        {
            /// <summary>
            /// 测样方式
            /// </summary>
            [XmlArray(ElementName = "scanTypes")]                  //scanTypes下面包含很多scanType
            [XmlArrayItem("scanType")]
            public List<string> scanTypes { get; set; }

            /// <summary>
            /// 操作人员
            /// </summary>
            [XmlArray(ElementName = "operators")]
            [XmlArrayItem("operator")]
            public List<string> operators { get; set; }

            /// <summary>
            /// 剂型
            /// </summary>
            [XmlArray(ElementName = "forms")]
            [XmlArrayItem("form")]
            public List<string> forms { get; set; }

            /// <summary>
            /// 注册码类型
            /// </summary>
            [XmlArray(ElementName = "licenseTypes")]
            [XmlArrayItem("licenseType")]
            public List<string> licenseTypes { get; set; }
        }

        /// <summary>
        /// 仪器校正参数
        /// </summary>
        public class Calibrate_Parameter : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            private void DoProperChange(string name)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

            /// <summary>
            /// 获取峰位的参数
            /// </summary>
            public class Peak_Parameter
            {
                /// <summary>
                /// 是否测试
                /// </summary>
                public bool IsTest { get; set; }
                [XmlIgnore]
                public double TestResult { get; set; }

                [XmlIgnore]
                public double PolyTestResult { get; set; }

                ///// <summary>
                ///// 聚苯乙烯测试起始波数
                ///// </summary>
                //[XmlAttribute("PloyFirstX")]
                //public double PloyFirstX { get; set; }

                ///// <summary>
                ///// 聚苯乙烯测试结束波数
                ///// </summary>
                //[XmlAttribute("PloyLastX")]
                //public double PloyLastX { get; set; }

                /// <summary>
                /// 聚苯乙烯目标峰位
                /// </summary>
                [XmlAttribute("PloyTargetX")]
                public double PolyTargetX { get; set; }

                /// <summary>
                /// 聚苯乙烯阈值
                /// </summary>
                [XmlAttribute("PloyThresold")]
                public double PolyThresold { get; set; }

                /// <summary>
                /// 实际谱带
                /// </summary>
                [XmlAttribute("ActualBand")]
                public double ActualBand { get; set; }

                /// <summary>
                /// 水蒸气实际谱带
                /// </summary>
                [XmlAttribute("PloyActualBand")]
                public double PolyActualBand { get; set; }

                /// <summary>
                /// 与7181.68的最大偏差
                /// </summary>
                //[XmlAttribute("DevMax")]
                //public double DevMax { get; set; }

                /// <summary>
                /// 实际偏差
                /// </summary>
                //[XmlAttribute("MinDev")]
                //public double MinDev { get; set; }

                /// <summary>
                /// 待检测的峰位(水蒸气吸收峰, 7181.68)
                /// </summary>
                [XmlAttribute("targetPeak")]
                public double targetPeak { get; set; }

                /// <summary>
                /// 验证峰位1(7232.29)
                /// </summary>
                [XmlAttribute("verfyPeak1")]
                public double verfyPeak1 { get; set; }

                /// <summary>
                /// 验证峰位2(7242.77)
                /// </summary>
                [XmlAttribute("verfyPeak2")]
                public double verfyPeak2 { get; set; }

                /// <summary>
                /// 验证峰位区域(7230)
                /// </summary>
                [XmlAttribute("firstX")]
                public double firstX { get; set; }

                /// <summary>
                /// 验证峰位区域(7245)
                /// </summary>
                [XmlAttribute("lastX")]
                public double lastX { get; set; }

                /// <summary>
                /// 需要校正的阈值
                /// </summary>
                [XmlAttribute("thresold")]
                public double thresold { get; set; }

                /// <summary>
                /// 重复测量次数
                /// </summary>
                [XmlAttribute("repeatCount")]
                public int repeatCount { get; set; }

                /// <summary>
                /// 扫描参数文件
                /// </summary>
                [XmlElement("iniFile")]
                public string iniFile { get; set; }

                /// <summary>
                /// 存储波数精度水峰测试光谱
                /// </summary>
                [XmlIgnore]
                public string SpectrumPath { get; set; }

                /// <summary>
                /// 存储激光波数校正光谱
                /// </summary>
                [XmlIgnore]
                public string LeaserSpectrum { get; set; }

                /// <summary>
                /// 存储波数精度聚苯乙烯测试光谱
                /// </summary>
                [XmlElement]
                public string PolySpectrumPath { get; set; }
            }

            public class OQTest
            {
                public class ResolutionTest
                {
                    /// <summary>
                    /// 是否测试
                    /// </summary>
                    public bool IsTest { get; set; }
                    [XmlIgnore]
                    public double TestResult { get; set; }
                    /// <summary>
                    /// 分辨率阈值
                    /// </summary>
                    [XmlAttribute("ResolutionThresold")]
                    public double ResolutionThresold { get; set; }

                    /// <summary>
                    /// 分辨率测试的目标峰位：7181.68)
                    /// </summary>
                    [XmlAttribute("targetPeak")]
                    public double targetPeak { get; set; }

                    /// <summary>
                    /// 分辨率光谱路径
                    /// </summary>
                    [XmlElement("SpectrumPath")]
                    public string SpectrumPath { get; set; }

                    /// <summary>
                    /// 扫描参数文件
                    /// </summary>
                    [XmlElement("iniFile")]
                    public string iniFile { get; set; }

                   
                }

                public class LineNoiseTest
                {
                    /// <summary>
                    /// 是否测试
                    /// </summary>
                    public bool IsTest { get; set; }
                    [XmlIgnore]
                    public double TestResult { get; set; }

                    /// <summary>
                    /// PP信噪比区域起始波数
                    /// </summary>
                    [XmlAttribute("firstX")]
                    public double firstX { get; set; }

                    /// <summary>
                    /// PP信噪比区域结束波数
                    /// </summary>
                    [XmlAttribute("lastX")]
                    public double lastX { get; set; }                    

                    /// <summary>
                    /// 重复次数
                    /// </summary>
                    [XmlAttribute("repeatCount")]
                    public int repeatCount { get; set; }

                    
                    /// <summary>
                    /// 信噪比PP阈值
                    /// </summary>
                    [XmlAttribute("PPThresold")]
                    public double PPThresold { get; set; }                

                    /// <summary>
                    /// 扫描参数文件
                    /// </summary>
                    [XmlElement("iniFile")]
                    public string iniFile { get; set; }

                    /// <summary>
                    /// 光谱路径
                    /// </summary>
                    [XmlElement("SpectrumFile")]
                    public List<string> SpectrumFile { get; set; }
                }

                public class LineSlopeTest
                {
                    /// <summary>
                    /// 是否测试
                    /// </summary>
                    public bool IsTest { get; set; }
                    public class data
                    {
                        /// <summary>
                        /// 斜率
                        /// </summary>
                        [XmlIgnore]
                        public double Slope { get; set; }

                        /// <summary>
                        /// 波数区间
                        /// </summary>
                        [XmlElement("WaveNumRange")]
                        public string WaveNumRange { get; set; }

                        /// <summary>
                        /// 区域最大最小值限制
                        /// </summary>
                        [XmlElement("lineLimit")]
                        public string lineLimit { get; set; }


                        /// <summary>
                        /// 测量值最小值
                        /// </summary>
                        [XmlIgnore]
                        public double meaMinValue { get; set; }

                        /// <summary>
                        /// 测量值最大值
                        /// </summary>
                        [XmlIgnore]
                        public double meaMaxValue { get; set; }


                        /// <summary>
                        /// 区域内限定最小值
                        /// </summary>
                        [XmlIgnore]
                        public double minValue { get; set; }

                        /// <summary>
                        /// 区域内限定最大值
                        /// </summary>
                        [XmlIgnore]
                        public double maxValue { get; set; }
                    }

                    public List<data> data1 { get; set; }

                    /// <summary>
                    /// 分辨率光谱路径
                    /// </summary>
                    [XmlElement("SpectrumPath")]
                    public string SpectrumPath { get; set; }

                    /// <summary>
                    /// 扫描参数文件
                    /// </summary>
                    [XmlElement("iniFile")]
                    public string iniFile { get; set; }

                    ///// <summary>
                    ///// 波数区间
                    ///// </summary>
                    //[XmlElement("WaveNumRange")]
                    //public List<string> WaveNumRange { get; set; }

                    ///// <summary>
                    ///// 区域最大最小值限制
                    ///// </summary>
                    //[XmlElement("lineLimit")]
                    //public List<string> lineLimit { get; set; }


                    ///// <summary>
                    ///// 区域内测量最大值
                    ///// </summary>
                    //[XmlElement("maxValue")]
                    //public List<double> maxValue { get; set; }

                    ///// <summary>
                    ///// 测量值最大值
                    ///// </summary>
                    //[XmlIgnore]
                    //public List<double> meaMaxValue { get; set; }

                    ///// <summary>
                    ///// 测量值最小值
                    ///// </summary>
                    //[XmlIgnore]
                    //public List<double> meaMinValue { get; set; }

                    ///// <summary>
                    ///// 区域内测量最小值
                    ///// </summary>
                    //[XmlElement("minValue")]
                    //public List<double> minValue { get; set; }
                }
                public class TransRepTest
                {
                    /// <summary>
                    /// 是否测试
                    /// </summary>
                    public bool IsTest { get; set; }
                    [XmlIgnore]
                    public double TestResult { get; set; }

                    /// <summary>
                    /// 区间起始值
                    /// </summary>
                    [XmlAttribute("firstX")]
                    public double firstX { get; set; }

                    /// <summary>
                    /// 区间结束值
                    /// </summary>
                    [XmlAttribute("lastX")]
                    public double lastX { get; set; }

                    /// <summary>
                    /// 阈值
                    /// </summary>
                    [XmlAttribute("minValue")]
                    public double transRepThresold { get; set; }

                    /// <summary>
                    /// 重复测量次数
                    /// </summary>
                    [XmlAttribute("repeatCount")]
                    public int repeatCount { get; set; }


                    /// <summary>
                    /// 分辨率光谱路径
                    /// </summary>
                    [XmlElement("SpectrumPath")]
                    public List<string> SpectrumPath { get; set; }

                    /// <summary>
                    /// 扫描参数文件
                    /// </summary>
                    [XmlElement("iniFile")]
                    public string iniFile { get; set; }                 

                }

                public class AccuracyTestOQ
                {
                    /// <summary>
                    /// 是否测试
                    /// </summary>
                    public bool IsTest { get; set; }
                    [XmlIgnore]
                    public double TestResult { get; set; }

                    [XmlIgnore]
                    public double PolyTestResult { get; set; }

                    ///// <summary>
                    ///// 聚苯乙烯测试起始波数
                    ///// </summary>
                    //[XmlAttribute("PloyFirstX")]
                    //public double PloyFirstX { get; set; }

                    ///// <summary>
                    ///// 聚苯乙烯测试结束波数
                    ///// </summary>
                    //[XmlAttribute("PloyLastX")]
                    //public double PloyLastX { get; set; }

                    /// <summary>
                    /// 聚苯乙烯目标峰位
                    /// </summary>
                    [XmlAttribute("PloyTargetX")]
                    public double PolyTargetX { get; set; }

                    /// <summary>
                    /// 聚苯乙烯阈值
                    /// </summary>
                    [XmlAttribute("PloyThresold")]
                    public double PolyThresold { get; set; }

                    /// <summary>
                    /// 预期谱带
                    /// </summary>
                    [XmlAttribute("ExpectedBand")]
                    public double ExpectedBand { get; set; }

                    /// <summary>
                    /// 实际谱带
                    /// </summary>
                    [XmlAttribute("ActualBand")]
                    public double ActualBand { get; set; }

                    /// <summary>
                    /// 聚苯乙烯实际谱带
                    /// </summary>
                    [XmlAttribute("PloyActualBand")]
                    public double PolyActualBand { get; set; }

                    /// <summary>
                    /// 与7181.68的最大偏差
                    /// </summary>
                    [XmlAttribute("DevMax")]
                    public double DevMax { get; set; }

                    /// <summary>
                    /// 实际偏差
                    /// </summary>
                    [XmlAttribute("MinDev")]
                    public double MinDev { get; set; }

                    /// <summary>
                    /// 待检测的峰位(水蒸气吸收峰, 7181.68)
                    /// </summary>
                    [XmlAttribute("targetPeak")]
                    public double targetPeak { get; set; }

                    /// <summary>
                    /// 验证峰位1(7232.29)
                    /// </summary>
                    [XmlAttribute("verfyPeak1")]
                    public double verfyPeak1 { get; set; }

                    /// <summary>
                    /// 验证峰位2(7242.77)
                    /// </summary>
                    [XmlAttribute("verfyPeak2")]
                    public double verfyPeak2 { get; set; }

                    /// <summary>
                    /// 验证峰位区域(7230)
                    /// </summary>
                    [XmlAttribute("firstX")]
                    public double firstX { get; set; }

                    /// <summary>
                    /// 验证峰位区域(7245)
                    /// </summary>
                    [XmlAttribute("lastX")]
                    public double lastX { get; set; }

                    /// <summary>
                    /// 需要校正的阈值
                    /// </summary>
                    [XmlAttribute("thresold")]
                    public double thresold { get; set; }

                    /// <summary>
                    /// 重复测量次数
                    /// </summary>
                    [XmlAttribute("repeatCount")]
                    public int repeatCount { get; set; }

                    /// <summary>
                    /// 扫描参数文件
                    /// </summary>
                    [XmlElement("iniFile")]
                    public string iniFile { get; set; }

                    /// <summary>
                    /// 存储水峰波数精度测试光谱
                    /// </summary>
                    [XmlIgnore]
                    public string SpectrumPath { get; set; }

                    /// <summary>
                    /// 存储聚苯乙烯波数精度测试光谱
                    /// </summary>
                    [XmlIgnore]
                    public string PolySpectrumPath { get; set; }
                }

                public class WaveNumRepTest
                {
                    /// <summary>
                    /// 是否测试
                    /// </summary>
                    public bool IsTest { get; set; }
                    [XmlIgnore]
                    public double TestResult { get; set; }

                    ///// <summary>
                    ///// 预期谱带
                    ///// </summary>
                    //[XmlAttribute("ExpectedBand")]
                    //public double ExpectedBand { get; set; }

                    ///// <summary>
                    ///// 实际谱带
                    ///// </summary>
                    //[XmlAttribute("ActualBand")]
                    //public double ActualBand { get; set; }

                    /// <summary>
                    /// 与7181.68的最大偏差
                    /// </summary>
                    [XmlAttribute("DevMax")]
                    public double DevMax { get; set; }

                    /// <summary>
                    /// 实际偏差
                    /// </summary>
                    [XmlAttribute("MinDev")]
                    public double MinDev { get; set; }

                    /// <summary>
                    /// 标定的峰位 7181.68
                    /// </summary>
                    [XmlAttribute("TargetPeak")]
                    public double TargetPeak { get; set; }

                    /// <summary>
                    /// 重复测量次数
                    /// </summary>
                    [XmlAttribute("repeatCount")]
                    public int repeatCount { get; set; }

                    ///// <summary>
                    ///// 区间起始值
                    ///// </summary>
                    //[XmlAttribute("firstX")]
                    //public double firstX { get; set; }

                    ///// <summary>
                    ///// 区间结束值
                    ///// </summary>
                    //[XmlAttribute("lastX")]
                    //public double lastX { get; set; }


                    /// <summary>
                    /// 阈值
                    /// </summary>
                    [XmlAttribute("transRepThresold")]
                    public double transRepThresold { get; set; }

                    /// <summary>
                    /// 分辨率光谱路径
                    /// </summary>
                    [XmlElement("SpectrumPath")]
                    public List<string> SpectrumPath { get; set; }

                    /// <summary>
                    /// 扫描参数文件
                    /// </summary>
                    [XmlElement("iniFile")]
                    public string iniFile { get; set; }              
                }

                public class EnergyDistributionTest
                {
                    /// <summary>
                    /// 是否测试
                    /// </summary>
                    public bool IsTest { get; set; }
                    [XmlIgnore]
                    public double TestResult { get; set; }

                    /// <summary>
                    /// 目标位置
                    /// </summary>
                    [XmlAttribute("TargetX")]
                    public double TargetX { get; set; }

                    /// <summary>
                    /// 能量分布测试阈值
                    /// </summary>
                    [XmlAttribute("engerDisThresold")]
                    public double engerDisThresold { get; set; }

                    /// <summary>
                    /// 能量分布光谱路径
                    /// </summary>
                    [XmlElement("SpectrumPath")]
                    public string SpectrumPath { get; set; }                    

                    /// <summary>
                    /// 扫描参数文件
                    /// </summary>
                    [XmlElement("iniFile")]
                    public string iniFile { get; set; }                   
                }
            }

            public class YaxisRep_Parameter
            {
                /// <summary>
                /// 是否测试
                /// </summary>
                public bool IsTest { get; set; }
                /// <summary>
                /// 保存测试结果
                /// </summary>
                [XmlIgnore]
                public double TestResult { get; set; }
                /// <summary>
                /// Y轴再现性测试区域起始波数
                /// </summary>
                [XmlAttribute("firstX")]
                public double firstX { get; set; }

                /// <summary>
                /// Y轴再相信测试区域结束波数
                /// </summary>
                [XmlAttribute("lastX")]
                public double lastX { get; set; }


                /// <summary>
                /// 信噪比PP阈值
                /// </summary>
                [XmlAttribute("YaxisRepThresold")]
                public double YaxisRepThresold { get; set; }

                /// <summary>
                /// 扫描参数文件
                /// </summary>
                [XmlElement("iniFile")]
                public string iniFile { get; set; }

                /// <summary>
                /// 保存Y轴重复性测试透射谱
                /// </summary>
                [XmlElement("SpectrumFile")]
                public string SpectrumFile { get; set; }

            }

            /// <summary>
            /// 获取信噪比的参数
            /// </summary>
            public class SNR_Parameter
            {
                public SNR_Parameter()
                {
                    SpectrumFile = new List<string>();
                    IpaSpectrumFile = new List<string>();
                    EngSpectrumFile = new List<string>();
                }
                /// <summary>
                /// 是否测试
                /// </summary>
                public bool IsTest { get; set; }
                /// <summary>
                /// 偏差测量结果
                /// </summary>
                [XmlIgnore]
                public double TestResult { get; set; }

                /// <summary>
                /// 线噪比测量结果
                /// </summary>
                [XmlIgnore]
                public double LineNoiseResult { get; set; }

                /// <summary>
                /// 参考光谱的最大偏差
                /// </summary>
                [XmlElement("MaxRef")]
                public double MaxRef { get; set; }

                /// <summary>
                /// PP信噪比区域起始波数
                /// </summary>
                [XmlAttribute("firstX")]
                public double firstX { get; set; }

                /// <summary>
                /// PP信噪比区域结束波数
                /// </summary>
                [XmlAttribute("lastX")]
                public double lastX { get; set; }

                /// <summary>
                /// 偏差起始区域
                /// </summary>
                [XmlAttribute("DevfirstX")]
                public double DevfirstX { get; set; }

                /// <summary>
                /// 偏差结束区域
                /// </summary>
                [XmlAttribute("DevlastX")]
                public double DevlastX { get; set; }

                /// <summary>
                /// 偏差阈值
                /// </summary>
                [XmlAttribute("DevThresold")]
                public double DevThresold { get; set; }

                /// <summary>
                /// 重复次数
                /// </summary>
                [XmlAttribute("repeatCount")]
                public int repeatCount { get; set; }

                /// <summary>
                /// 信噪比RMS阈值
                /// </summary>
                [XmlAttribute("RMSThresold")]
                public double RMSThresold { get; set; }

                /// <summary>
                /// 信噪比PP阈值
                /// </summary>
                [XmlAttribute("PPThresold")]
                public double PPThresold { get; set; }

                ///// <summary>
                ///// OQ峰峰噪声测试光谱
                ///// </summary>
                //public string lineNoiseSpecPath { get; set; }

                /// <summary>
                /// 扫描参数文件
                /// </summary>
                [XmlElement("iniFile")]
                public string iniFile { get; set; }

                /// <summary>
                /// 存储测试光谱文件
                /// </summary>
                [XmlElement("SpectrumFile")]
                public List<string> SpectrumFile { get; set; }

                /// <summary>
                /// 干涉峰值阈值
                /// </summary>
                [XmlAttribute("IpaThresold")]
                public double IpaThresold { get; set; }

                /// <summary>
                /// 干涉峰值阈值
                /// </summary>
                [XmlAttribute("IpaTestResult")]
                public double IpaTestResult { get; set; }



                /// <summary>
                /// 能量值阈值
                /// </summary>
                [XmlAttribute("EngThresold")]
                public double EngThresold { get; set; }

                /// <summary>
                /// 能量值阈值
                /// </summary>
                [XmlAttribute("EngTestResult")]
                public double EngTestResult { get; set; }

                /// <summary>
                /// 干涉峰测试光谱文件
                /// </summary>
                [XmlElement("IpaSpectrumFile")]
                public List<string> IpaSpectrumFile { get; set; }

                /// <summary>
                /// 干涉峰测试光谱文件
                /// </summary>
                [XmlElement("EngSpectrumFile")]
                public List<string> EngSpectrumFile { get; set; }

                /// <summary>
                /// 能量测试起始区间
                /// </summary>
                [XmlAttribute("engFirstX")]
                public double engFirstX { get; set; }

                /// <summary>
                /// 能量测试结束区间
                /// </summary>
                [XmlAttribute("engLastX")]
                public double engLastX { get; set; }
            }

            /// <summary>
            /// 激光波数校正参数
            /// </summary>
            [XmlElement("laserPara")]
            public Peak_Parameter laserPara { get; set; }

            /// <summary>
            /// 波数准确度验证参数
            /// </summary>
            [XmlElement("accuracyPara")]
            public Peak_Parameter accuracyPara { get; set; }

            /// <summary>
            /// 分辨率测试参数
            /// </summary>
            [XmlElement("resolutionTestPara")]
            public OQTest.ResolutionTest resolutionTestPara { get; set; }

            /// <summary>
            /// OQ线噪比测试
            /// </summary>
            [XmlElement("lineNoiseTest")]
            public OQTest.LineNoiseTest lineNoiseTest { get; set; }

            /// <summary>
            /// 100%线斜率测试
            /// </summary>
            [XmlElement("lineSlopeTestPara")]
            public OQTest.LineSlopeTest lineSlopeTestPara { get; set; }

            /// <summary>
            /// 吸收重复性测试参数
            /// </summary>
            [XmlElement("yaxisRepPara")]
            public YaxisRep_Parameter yaxisRepPara { get; set; }

            /// <summary>
            /// OQ波数精度测试
            /// </summary>
            [XmlElement("accuracyTestOQ")]
            public OQTest.AccuracyTestOQ accuracyTestOQ { get; set; }

            /// <summary>
            /// 波数重复性测试
            /// </summary>
            [XmlElement("waveNumRepTestPara")]
            public OQTest.WaveNumRepTest waveNumRepTestPara { get; set; }

            /// <summary>
            /// 能量分布测试
            /// </summary>
            [XmlElement("energyDisPara")]
            public OQTest.EnergyDistributionTest energyDisPara { get; set; }


            /// <summary>
            /// 信噪比验证参数
            /// </summary>
            [XmlElement("snrPara")]
            // public Peak_Parameter snrPara { get; set; }
            public SNR_Parameter snrPara { get; set; }

            /// <summary>
            /// 透射重复性测试
            /// </summary>
            [XmlElement("transRepTest")]
            public OQTest.TransRepTest transRepTest { get; set; }
        }

        /// <summary>
        /// 当前配置文件名, 不写入配置文件
        /// </summary>
        [XmlIgnore]
        public string filename { get; set; }
        
        /// <summary>
        /// 运行参数
        /// </summary>
        [XmlElement("runing_parameter")]
        public Runing_para runing_para { get; set; }

        /// <summary>
        /// 仪器验证参数
        /// </summary>
        [XmlElement("calibratePara")]
        public Calibrate_Parameter calibratePara { get; set; } 

        /// <summary>
        /// 常用数据字典
        /// </summary>
        [XmlElement("dictionary")]
        public Dictionary dictionary { get; set; }

        /// <summary>
        /// 模型列表
        /// </summary>
        [XmlArray(ElementName = "models")]                  //models下面包含很多model
        [XmlArrayItem("model")]
        public ObservableCollection<ModelInfo> models { get; set; }

        //如果文件在setting目录下，没有包含路径，需要增加setting路径后返回
        private static string AddSettingPath(string filename)
        {
            if (filename.IndexOf("\\") >= 0)
                return Path.GetFileName(filename);
            else
                return Path.Combine(SettingData.settingFolder, filename);
        }

        //写入信息到文件中
        public  bool Serialize(string filename)
        {
            try
            {
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(SettingFile));
                StringWriter textWriter = new StringWriter();
                xs.Serialize(textWriter, this);

                //如果文件在setting目录下，只保存文件名，不保存路径
                string repstr = SettingData.settingFolder.EndsWith("\\") ? SettingData.settingFolder : SettingData.settingFolder + "\\";
                string writeStr = textWriter.ToString().Replace(repstr, "");

                StreamWriter writer = new StreamWriter(filename, false, Encoding.GetEncoding(SettingData.UTF8));
                writer.Write(writeStr);
                writer.Close();

                return true;
            }
            catch (Exception ex)
            {
                CommonMethod.ErrorMsgBox(ex.Message);
                return false;
            }
        }

        //从文件中读取信息
        public static SettingFile Deserialize(string filename)
        {
            try
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(filename, Encoding.GetEncoding(SettingData.UTF8));
                string fileContent = sr.ReadToEnd();
                sr.Close();

                StringReader textReader = new StringReader(fileContent);

                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(SettingFile));
                SettingFile retData = xs.Deserialize(textReader) as SettingFile;

                //如果文件名中没有路径，表示文件在setting目录下，需要补充完整的路径
                retData.runing_para.database = AddSettingPath(retData.runing_para.database);
                retData.filename = filename;

                return retData;
            }
            catch (Exception ex)
            {
                CommonMethod.ErrorMsgBox(ex.Message);
                return null;
            }
        }
    }

    public static class SettingData
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        private static extern int WritePrivateProfileString(string section, string key, string writeVal, string filePath);

        /// <summary>
        /// 数据保存的根目录
        /// </summary>
        public static string rootPath;
        public static string IniFileName;       //配置文件名
        public const string GBCode2312="GB2312";
        public const string UTF8 = "utf-8";

        /// <summary>
        /// yyyy-MM-dd HH:mm:ss
        /// </summary>
        public const string LongDateTimeString = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// yyyy-MM-dd
        /// </summary>
        public const string ShortDateTimeString = "yyyy-MM-dd";

        //预置的目录
        public static string settingFolder;
        public static string iniFolder;
        public static string reportFolder;
        public static string dataFolder;
        public static string logFile;

        public const int CurrentVersion = 100;                    //当前运行的版本号
        public const uint IdentFileMark = 0x4E445149;           //Nir Drug Quick Identify标志
        public const string PackgeImagePath = @"pack://application:,,/Images/";     //资源文件中的图像文件路径
        public static SettingFile settingData = null;
        public static int lastOperateTime;                         //上一次对仪器操作的时间，超过1小时没操作，需要重新连接
        public static DBConnection dataBase = null;
        public static SystemTypeEnum systemType;

        static SettingData()
        {
           // string tempstr = Ai.Hong.CommonMethod.GetDatabaseTemplate("AI.HONG@VSPEC", true);
            //tempstr += "";

          //  rootPath = Ai.Hong.CommonMethod.GetProgramDataPath();
            rootPath = Environment.CurrentDirectory;// Path.Combine(Environment.CurrentDirectory, "setting");

            settingFolder = Path.Combine(rootPath, Properties.Settings.Default.Properties["settingFolder"].DefaultValue.ToString());
            iniFolder = Path.Combine(rootPath, Properties.Settings.Default.Properties["iniFolder"].DefaultValue.ToString());
            reportFolder = Path.Combine(rootPath, Properties.Settings.Default.Properties["reportFolder"].DefaultValue.ToString());
            dataFolder = Path.Combine(rootPath, Properties.Settings.Default.Properties["dataFolder"].DefaultValue.ToString());
            logFile = Path.Combine(rootPath, "logfile.txt");

            if ((bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue == false)    //设计模式不可用
            {
                //系统设置中的配置文件名
                string settingname = Properties.Settings.Default.Properties["SettingFileName"].DefaultValue.ToString() ;

                //读入Ini设置数据
                IniFileName = Path.Combine(settingFolder, settingname);
                settingData = SettingFile.Deserialize(IniFileName);
                //settingData.calibratePara.yaxisRepPara = new SettingFile.Calibrate_Parameter.YaxisRep_Parameter();
                //settingData.calibratePara.resolutionTestPara = new SettingFile.Calibrate_Parameter.OQTest.ResolutionTest_Parameter();
                //settingData.calibratePara.energyDisPara = new SettingFile.Calibrate_Parameter.OQTest.EnergyDistributionTest();
                //settingData.calibratePara.lineSlopeTestPara = new SettingFile.Calibrate_Parameter.OQTest.LineSlopeTest();
                //settingData.calibratePara.resolutionTestPara = new SettingFile.Calibrate_Parameter.OQTest.ResolutionTest_Parameter();
                //settingData.calibratePara.transRepTest = new SettingFile.Calibrate_Parameter.OQTest.TransRepTest();
                //settingData.calibratePara.waveNumRepTestPara = new SettingFile.Calibrate_Parameter.OQTest.WaveNumRepTest();
                dataBase = new DBConnection();
                
                if (!Directory.Exists(settingData.runing_para.savePath))
                {
                    CommonMethod.ErrorMsgBox("数据保存路径不存在：" + settingData.runing_para.savePath + ",需要重新指定。");
                    System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        SettingData.settingData.runing_para.savePath = dlg.SelectedPath;
                        SettingData.settingData.Serialize(SettingData.settingData.filename);
                    }
                    else
                    {
                        CommonMethod.InfoMsgBox("请在系统设置中指定数据保存路径。");
                    }
                }                
            }
        }

        public static string ReadIniString(string iniFile, string section, string key)
        {
            StringBuilder retstr = new StringBuilder(256);

            GetPrivateProfileString(section, key, "", retstr, 256, iniFile);
            return retstr.ToString().Trim();      //转为大写，好比较
        }

        public static string ReadIniString(string section, string key)
        {
            StringBuilder retstr = new StringBuilder(256);

            GetPrivateProfileString(section, key, "", retstr, 256, IniFileName);
            return retstr.ToString().Trim();      //转为大写，好比较
        }

        public static void WriteIniString(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, IniFileName);
        }

        //将分析后的原始结果写入当前运行目录下的IdentifyResult.csv文件中,CSV文件以','分隔
        public static void AddToResultFile(string writeStr)
        {
            StreamWriter stream = null;
            try
            {
                string logdir = Path.Combine(settingData.runing_para.savePath, "logfile");
                if (!Directory.Exists(logdir))
                    Directory.CreateDirectory(logdir);
                
                string csvfile = Path.Combine(logdir, "Result" + DateTime.Now.ToString("yyyyMMdd") + ".csv");
                stream = new StreamWriter(csvfile, true, Encoding.GetEncoding(GBCode2312));
                stream.WriteLine(writeStr);
            }
            catch (System.Exception ex)
            {
                CommonMethod.ErrorMsgBox(ex.Message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }

        //专门针对定性检测结果
        public static void AddToResultFile(string filename, string aditionalInfo)
        {
            StreamWriter stream = null;
            try
            {
                string logdir = Path.Combine(settingData.runing_para.savePath, "logfile");
                if (!Directory.Exists(logdir))
                    Directory.CreateDirectory(logdir);

                string csvfile = Path.Combine(logdir, "Result" + DateTime.Now.ToString("yyyyMMdd") + ".csv");
                bool needHead = !File.Exists(csvfile) ? true : false;
                stream = new StreamWriter(csvfile, true, Encoding.GetEncoding(GBCode2312));
                if (needHead)        //需要创建文件头
                {
                    stream.WriteLine("文件名,组分名,浓度,相关系数,CLS系数,信噪比,检测时间");
                }
                stream.Write(filename+",");
                if(aditionalInfo != null)
                    stream.Write(aditionalInfo + ",");

                stream.WriteLine(DateTime.Now.ToString(LongDateTimeString));
            }
            catch (System.Exception ex)
            {
                CommonMethod.ErrorMsgBox(ex.Message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }

        /*
        public static RamanScanMethodInfo GetScanMethod(string name)
        {
            name = name.ToUpper();
            return RamanScanMethods.Find(item=>item.name == name);
        }
        */

        /// <summary>
        /// 数据字典对应的关键字名称，必须要一一对应
        /// </summary>
        public enum DICTKEYWORD { DRUGNAME, FACTROY, SPECIFACTION, PACAKGE, OPERATOR, IDENTADDRESS, PICKADDRESS };
        private static string[] DictKeyWordName = new string[] { "药品名", "生产厂家", "规格", "包装", "操作员", "检测地点", "取样地点" };

        public static string GetKeyName(DICTKEYWORD keyType)
        {
            return DictKeyWordName[(int)keyType];
        }
    }

    public static class CommonMethod
    {
        private const int GWL_STYLE = -16; private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public static void HideWindowSystemButton(Window dstWnd)
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper(dstWnd).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        public static void ErrorMsgBox(string errstr)
        {
            MessageBox.Show(errstr, "错误信息", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void InfoMsgBox(string infostr)
        {
            MessageBox.Show(infostr, "提示信息", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static bool QuestionMsgBox(string questionStr)
        {
            if (MessageBox.Show(questionStr, "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                return true;

            return false;
        }

        //从资源文件中创建Stream
        public static Stream StreamFromResource(string filename)
        {
            Assembly assemb = Assembly.GetExecutingAssembly();

            if (assemb == null)
                return null;

            foreach (string resname in assemb.GetManifestResourceNames())
            {
                if (resname.IndexOf(filename) > 0)
                {
                    return assemb.GetManifestResourceStream(resname);
                }
            }
            return null;
        }

        public static bool ConvertToUTF_8(string methodFile)
        {
            try
            {
                StreamReader reader = new StreamReader(methodFile, Encoding.GetEncoding(SettingData.GBCode2312));
                string fileContent = reader.ReadToEnd();
                reader.Close();

                StreamWriter writer = new StreamWriter(methodFile, false, Encoding.GetEncoding(SettingData.UTF8));
                writer.Write(fileContent);
                writer.Close();

                return true;
            }
            catch (Exception ex)
            {
                ErrorMsgBox(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 设置Image Control的图像
        /// </summary>
        /// <param name="imgCtrl">Image Control</param>
        /// <param name="imgfile">图形文件名(在Images目录下)</param>
        public static void SetImageSource(Image imgCtrl, string imgfile)
        {
            string tempimgfile = SettingData.PackgeImagePath + imgfile;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new System.Uri(tempimgfile);
            bi.EndInit();
            imgCtrl.Source = bi;
        }

        /// <summary>
        /// 加载Image
        /// </summary>
        public static BitmapImage LoadImage(string imgfile)
        {
            string tempimgfile = SettingData.PackgeImagePath + imgfile;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new System.Uri(tempimgfile);
            bi.EndInit();

            return bi;
        }

        /// <summary>
        /// 显示光谱图形
        /// </summary>
        /// <param name="chart">显示的Chart</param>
        /// <param name="ca">属于Chart,显示的ChartArea，用于设置坐标轴的属性</param>
        /// <param name="spectrumFile">光谱文件名</param>
        /// <param name="color">显示的颜色</param>
        public static void DrawSpectrumGraphic(Chart chart, ChartArea ca, string spectrumFile, System.Drawing.Color color)
        {
            SpecFileFormat fileData = new SpecFileFormat();
            if (fileData.ReadFile(spectrumFile) == false)
                return;

            Series sr = new Series();            
            sr.XValueType = ChartValueType.Int32;
            sr.Color = color;
            sr.ChartType = SeriesChartType.Line;
            for (int j = 0; j < fileData.XDatas.Length; j++)
            {
                sr.Points.AddXY(fileData.XDatas[j], fileData.YDatas[j]);
            }
            chart.Series.Add(sr);

            double maxXValue = double.MinValue, minXValue = double.MaxValue;
            double minYValue = double.MaxValue, maxYValue = double.MinValue;

            minXValue = fileData.Parameter.firstX < fileData.Parameter.lastX ? fileData.Parameter.firstX : fileData.Parameter.lastX;
            maxXValue = fileData.Parameter.firstX > fileData.Parameter.lastX ? fileData.Parameter.firstX : fileData.Parameter.lastX;
            maxYValue = fileData.Parameter.maxYValue;
            minYValue = fileData.Parameter.minYValue;

            //设置最大/最小X轴的值
            ca.AxisX.Maximum = Math.Ceiling(maxXValue / 50) * 50;
            ca.AxisX.Minimum = Math.Round(minXValue / 50, MidpointRounding.AwayFromZero) * 50;

            //将Y轴刻度显示在左边
            ca.AxisX.IsReversed = true;
            ca.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            ca.AxisX2.IntervalOffset = 10;
            ca.AxisY2.Enabled = AxisEnabled.True;
            ca.AxisY2.LabelStyle.Enabled = true;
            ca.AxisY2.IsLabelAutoFit = false;   // gives me smaller labels which I like.
            ca.AxisY2.LineColor = ca.AxisX.LineColor;
            ca.AxisY2.IntervalAutoMode = IntervalAutoMode.VariableCount;

            ca.AxisY.Title = string.Empty;
            ca.AxisY.IsMarksNextToAxis = false;
            ca.AxisY.LabelStyle.Enabled = false;
            ca.AxisY.IsLabelAutoFit = false;

            //不显示网格线
            ca.AxisX.MajorGrid.Enabled = false;
            ca.AxisY2.MajorGrid.Enabled = false;

            //光谱显示不要在0位，让0位稍微往上提一点
            double temp = Math.Log10(maxYValue - minYValue);
            temp = Math.Round(temp) - 1;
            temp = Math.Pow(10, temp);
            ca.AxisY2.Minimum = -temp / 10;

            //第二Y轴(左边的）显示
            foreach (Series series in chart.Series)
            {
                series.YAxisType = AxisType.Secondary;
            }
        }

        public static bool IsEmpty(string str)
        {
            return str == null || str.Trim() == "";
        }

        public static string GetEnumDescription(Enum enumObj)
        {
            FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj.ToString());

            object[] attribArray = fieldInfo.GetCustomAttributes(false);

            if (attribArray.Length == 0)
            {
                return enumObj.ToString();
            }
            else
            {
                DescriptionAttribute attrib = attribArray[0] as DescriptionAttribute;
                return attrib.Description;
            }
        }

        public static string GetEnumFromDescription(System.Type enumtype, string description)
        {
            FieldInfo[] fieldInfos = enumtype.GetFields();

            foreach (FieldInfo info in fieldInfos)
            {
                object[] attribArray = info.GetCustomAttributes(false);
                if (attribArray.Length > 0)
                {
                    DescriptionAttribute attrib = attribArray[0] as DescriptionAttribute;
                    if (attrib.Description == description)
                        return info.Name;
                }
            }
            return null;
        }

        public static System.Collections.IList EnumDescriptionToList(this Type type)
        {
            System.Collections.ArrayList list = new System.Collections.ArrayList();
            Array enumValues = Enum.GetValues(type);

            foreach (Enum value in enumValues)
            {
                list.Add(CommonMethod.GetEnumDescription(value));
            }

            return list;
        }

        /// <summary>
        /// 拷贝文件夹中所有文件到新文件夹
        /// </summary>
        public static void DirectoryCopy(string SourcePath, string DestinationPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));

            //Copy all the files
            foreach (string newPath in Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
        }

        public static void AddToLogFile(string msg)
        {
            try
            {
                StreamWriter writer = new StreamWriter(Path.Combine(SettingData.settingFolder, "logfile.txt"), true, System.Text.Encoding.GetEncoding(SettingData.GBCode2312));
                writer.WriteLine(DateTime.Now.ToString(SettingData.LongDateTimeString) + msg);
                writer.Close();
            }
            catch (Exception)
            {
            }
        }


        /// <summary>
        /// 移除DataGrid中的选定项
        /// </summary>
        /// <param name="allItems"></param>
        /// <param name="removeItems"></param>
        public static void RemoveDataGridItems(System.Collections.IList allItems, System.Collections.IList removeItems)
        {
            List<object> tempList = new List<object>();
            foreach (object item in removeItems)
                tempList.Add(item);

            foreach (object item in tempList)
            {
                allItems.Remove(item);
            }
        }

        /// <summary>
        /// 读取药品检测信息
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static DrugInfo ReadDrugInfo(string filename)
        {
            if (filename == null)
                return null;

            //string dataStr = Ai.Hong.CommonLibrary.IdentifyFileBlock.ReadData(filename, SettingData.IdentFileMark, SettingData.CurrentVersion);
            //if (dataStr == null)
            //    return null;

            byte[] databytes = Ai.Hong.CommonLibrary.SPCFile.ReadIdentifyBlock(filename);
            if (databytes == null)
                return null;

            string dataStr = Encoding.UTF8.GetString(databytes);
            if (dataStr == null)
                return null;

            DrugInfo retData = Ai.Hong.CommonMethod.Deserialize<DrugInfo>(dataStr);

            //将原来的文件名变成现在的文件名
            if (retData != null)
                retData.filename = filename;

            return retData;
        }

        /// <summary>
        /// 将药品检测结果保存到文件中
        /// </summary>
        public static bool WriteDrugInfo(DrugInfo data)
        {
            string dataStr = Ai.Hong.CommonMethod.Serialize<DrugInfo>(data);
            if (dataStr == null)
                return false;

            //if (Ai.Hong.CommonLibrary.IdentifyFileBlock.WriteData(data.filename, SettingData.IdentFileMark, SettingData.CurrentVersion, dataStr) == false)
            //    return false;

            byte[] writebytes = Encoding.UTF8.GetBytes(dataStr);
            return Ai.Hong.CommonLibrary.SPCFile.WriteIdentifyBlock(data.filename, writebytes);
        }

        /// <summary>
        /// 将文件名中的非法字符转换为指定字符
        /// </summary>
        public static string GetValidFilename(string sourceName, string replaceChar="")
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars());
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(string.Format("[{0}]", System.Text.RegularExpressions.Regex.Escape(regexSearch)));
            return r.Replace(sourceName, replaceChar);
        }

    }

    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class ImageConvert : IValueConverter     //文件名转换为图像类型
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            string filename = (string)value;
            return new BitmapImage(new Uri(filename));
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            BitmapImage img = value as BitmapImage;
            return Path.GetFileName(img.UriSource.AbsoluteUri);
        }
    }

    [ValueConversion(typeof(string), typeof(EnumIdentResult))]
    public class IdentResultImageConvert : IValueConverter     //文件名转换为图像类型
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            EnumIdentResult result = (EnumIdentResult)value;
            switch (result)
            {
                case EnumIdentResult.OK:
                    return CommonMethod.LoadImage("IdentYes_128.png");
                case EnumIdentResult.FAULT:
                    return CommonMethod.LoadImage("IdentNo_128.png");
                default:
                    return CommonMethod.LoadImage("Unknown_128.png");
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// EnumLicenseType与描述之间的相互转换
    /// </summary>
    [ValueConversion(typeof(List<string>), typeof(EnumLicenseType))]
    public class ComponentTypeDescriptoinConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<string> result = new List<string>();
            foreach (var item in Enum.GetValues(typeof(EnumLicenseType)))
            {
                result.Add(CommonMethod.GetEnumDescription((Enum)item));
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// EnumLicenseType与描述之间的相互转换
    /// </summary>
    [ValueConversion(typeof(List<string>), typeof(EnumSavePathType))]
    public class SavePathTypeConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<string> result = new List<string>();
            foreach (var item in Enum.GetValues(typeof(EnumSavePathType)))
            {
                result.Add(CommonMethod.GetEnumDescription((Enum)item));
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            foreach (var item in Enum.GetValues(typeof(EnumSavePathType)))
            {
                if (CommonMethod.GetEnumDescription((Enum)item) == (string)value)
                    return item;
            }
            return null;
        }
    }

    [ValueConversion(typeof(DateTime), typeof(String))]
    public class DateTimeConverter : IValueConverter      //将时间转换到字符串yyyy-MM-dd HH:mm:ss
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime date = (DateTime)value;
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = value as string;
            DateTime resultDateTime;
            if (DateTime.TryParse(strValue, out resultDateTime))
            {
                return resultDateTime;
            }
            return DependencyProperty.UnsetValue;
        }
    }

    [ValueConversion(typeof(Visibility), typeof(bool))]
    public class CheckedVisibilityConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((bool?)value==true) ? Visibility.Visible :Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible;
        }
    }

    /// <summary>
    /// 多个Bool为True时，返回True，否则返回False
    /// </summary>
    public class MultiAndBoolConvert : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            foreach (object item in values)
            {
                if (!(item is bool))
                    return false;

                bool isvalue = (bool)item;
                if(isvalue == false)
                    return false;
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack should never be called");
        }

    }
    //在OperaterWindow的Statusbar中显示当前日期和时间，用于Binding
    public class TimeTicker : INotifyPropertyChanged
    {
        public TimeTicker()
        {
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new System.TimeSpan(0, 0, 1);     //1秒钟一次
            timer.Tick += timer_Elapsed;
            timer.Start();
        }
        public DateTime Now
        {
            get { return DateTime.Now; }
        }
        void timer_Elapsed(object sender, EventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Now"));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ValueRoutedEventArgs : RoutedEventArgs     //带自定义值的消息参数
    {
        public enum EventTypeEnum { BOOL, INT, STRING, SAMPLEDATA, METRIALDATA }; //消息的类型, string, SampleResultData, MetiralInfo
        public ValueRoutedEventArgs(EventTypeEnum type, Object value)
        {
            EventType = type;
            EventValue = value;
        }
        public EventTypeEnum EventType { get; set; }    //消息种类
        public Object EventValue { get; set; }      //消息参数
    }

    public static class SpectrumAlgorithm
    {
        /// <summary>
        /// 是否在区间内
        /// </summary>
        /// <param name="value">要查找的值</param>
        /// <param name="startValue">区间起始值</param>
        /// <param name="endValue">区间结束值</param>
        /// <returns>是否在区间内</returns>
        private static bool ValueInside(double value, double startValue, double endValue)
        {
            if (startValue < endValue)
            {
                if (value >= startValue && value <= endValue)
                    return true;
                else
                    return false;
            }
            else
            {
                if (value >= endValue && value <= startValue)
                    return true;
                else
                    return false;
            }
        }

        //比较大小，如果begin>end，交换
        private static void SortInOrder(ref int beginvalue, ref int endvalue)
        {
            if (beginvalue > endvalue)
            {
                int temp = endvalue;
                endvalue = beginvalue;
                beginvalue = temp;
            }
        }

        /// <summary>
        /// 线性插值
        /// </summary>
        private static double LinearInterpolation(double interpX, double x1, double y1, double x2, double y2)
        {
            double k = (y2 - y1) / (x2 - x1);
            double b = y2 - k * x2;

            return interpX * k + b;
        }

        /// <summary>
        /// 计算光谱积分，积分方式与OPUS B 类型相同
        /// </summary>
        /// <param name="xData">X轴数据</param>
        /// <param name="yData">Y轴数据</param>
        /// <param name="freqStart">积分起始X值</param>
        /// <param name="freqEnd">积分结束X值</param>
        /// <returns>积分结果</returns>
        public static double Integrate(float[] xData, float[] yData, float freqStart, float freqEnd)
        {
            int beginIndex = FindNearestPosition(xData, 0, xData.Length - 1, freqStart);
            int endIndex = FindNearestPosition(xData, 0, xData.Length - 1, freqEnd);

            //xData范围内没有找到freqStart或freqEnd
            if (beginIndex == -1 || endIndex == -1)
                return 0;

            SortInOrder(ref beginIndex, ref endIndex);

            double[] x = new double[endIndex - beginIndex + 1];
            double[] y = new double[endIndex - beginIndex + 1];

            for (int i = beginIndex; i <= endIndex; i++)
            {
                x[i - beginIndex] = xData[i];
                y[i - beginIndex] = yData[i];
            }

            alglib.spline1dinterpolant c;
            alglib.spline1dbuildcubic(x, y, out c);

            double value = alglib.spline1dintegrate(c, x[x.Length - 1]);

            //计算基线下面的面积
            double beginyvalue = alglib.spline1dcalc(c, freqStart);
            double endyvalue = alglib.spline1dcalc(c, freqEnd);
            double basevalue = Math.Abs((freqEnd-freqStart)*(endyvalue+beginyvalue)/2);

            return value-basevalue;
        }

        /// <summary>
        /// 查找valueToFind在X轴数据中的位置
        /// </summary>
        /// <param name="xData">X轴数据</param>
        /// <param name="beginx">查找起始点</param>
        /// <param name="endx">查找结束点</param>
        /// <param name="valueToFind">要查找的值</param>
        /// <returns>返回valueToFind的位置</returns>
        public static int FindNearestPosition(float[] xData, int beginx, int endx, float valueToFind)
        {
            if (xData == null || xData.Length == 0 || !ValueInside(valueToFind, xData[beginx], xData[endx]))
                return -1;

            int foundPos = -1;
            if (xData[beginx] == valueToFind)
                foundPos = beginx;
            else if (xData[endx] == valueToFind)
                foundPos = endx;
            else if (beginx == endx || beginx == endx - 1)      //只相差1个，不用再分了
            {
                if(Math.Abs(xData[endx] - valueToFind) > Math.Abs(xData[beginx] - valueToFind))
                    foundPos = beginx;
                else
                    foundPos = endx;
            }

            if (foundPos != -1)  // 找到了,还要比较左右看看哪个更接近
            {
                double x1 = Math.Abs(xData[foundPos] - valueToFind);
                double x2 = Math.Abs(xData[foundPos-1] - valueToFind);
                double x3 = Math.Abs(xData[foundPos+1] - valueToFind);

                if (x1 < x2 && x1 < x3)
                    return foundPos;
                else if (x2 < x1 && x2 < x3)
                    return foundPos - 1;
                else
                    return foundPos + 1;
            }

            int midPos = beginx + (endx - beginx) / 2;      //中间点
            if (xData[0] < xData[xData.Length-1])      //递增序列
            {
                if (xData[midPos] < valueToFind)      //中间点小于valutToFind，在后面查找
                {
                    return FindNearestPosition(xData, midPos, endx, valueToFind);
                }
                else //中间点大于valutToFind，在前面查找
                {
                    return FindNearestPosition(xData, beginx, midPos, valueToFind);
                }
            }
            else        //递减序列
            {
                if (xData[midPos] < valueToFind)      //中间点小于valutToFind，在前面查找
                {
                    return FindNearestPosition(xData, beginx, midPos, valueToFind);
                }
                else //中间点大于valutToFind，在后面查找
                {
                    return FindNearestPosition(xData, midPos, endx, valueToFind);
                }
            }
        }

        /// <summary>
        /// 计算光谱的RMS
        /// </summary>
        /// <param name="xData">X轴数据</param>
        /// <param name="yData">Y轴数据</param>
        /// <param name="freqStart">RMS起始X值</param>
        /// <param name="freqEnd">RMS结束X值</param>
        /// <returns>RMS值</returns>
        public static double CalculateRMS(float[] xData, float[] yData, float freqStart, float freqEnd)
        {
            int beginIndex = FindNearestPosition(xData, 0, xData.Length - 1, freqStart);
            int endIndex = FindNearestPosition(xData, 0, xData.Length - 1, freqEnd);

            //xData范围内没有找到freqStart或freqEnd
            if (beginIndex == -1 || endIndex == -1)
                return 0;

            SortInOrder(ref beginIndex, ref endIndex);

            double[] x = new double[endIndex - beginIndex];
            double[] y = new double[endIndex - beginIndex];
            for (int i = beginIndex; i < endIndex; i++)
            {
                x[i - beginIndex] = xData[i];
                y[i - beginIndex] = yData[i];
            }

            int info;
            alglib.spline1dinterpolant s;
            alglib.spline1dfitreport rep;
            double rho = 5;

            alglib.spline1dfitpenalized(x, y, 50, rho, out info, out s, out rep);

            //计算平均值
            double argY = 0;
            for (int i = 0; i < y.Length; i++)
            {
                double newy = alglib.spline1dcalc(s, x[i]);
                y[i] -= newy;
                argY += Math.Abs(y[i]);
            }
            argY = argY / y.Length;

            //计算平方和
            double sumY = 0;
            for (int i = 0; i < y.Length; i++)
                sumY += (y[i] - argY) * (y[i] - argY);

            double rms = Math.Sqrt(sumY / y.Length);

            return rms;
        }

        /// <summary>
        /// 标定峰位
        /// </summary>
        /// <param name="xData">X轴数据</param>
        /// <param name="yData">Y轴数据</param>
        /// <param name="peakValue">要标定的峰位</param>
        /// <param name="pointsToCal">计算的点</param>
        /// <param name="newyvalue">返回峰位的峰高</param>
        /// <returns>找到的峰位</returns>
        public static double PickPeak(float[] xData, float[] yData, float peakValue, int pointsToCal, out double newyvalue)
        {
            //找到要标记的峰在xData中的最近位置
            int foundpos = FindNearestPosition(xData, 0, xData.Length - 1, peakValue);

            //查找该位置最近的峰
            double maxValue = yData[foundpos];
            if (yData[foundpos - 1] >= maxValue)    //下降曲线，峰在左边
            {
                while (foundpos > 0 && yData[foundpos - 1] > yData[foundpos])
                    foundpos--;
            }
            else    //上升曲线，峰在右边
            {
                while (foundpos < yData.Length - 1 && yData[foundpos + 1] > yData[foundpos])
                    foundpos++;
            }

            pointsToCal = 4;
            //取左右各pointsToCal个点来做曲线拟合
            double[] x = new double[pointsToCal*2+1];
            double[] y = new double[pointsToCal*2+1];
            for (int i = foundpos - pointsToCal; i <= foundpos+pointsToCal; i++)
            {
                x[i-(foundpos - pointsToCal)] = xData[i];
                y[i-(foundpos - pointsToCal)] = yData[i];
            }

            alglib.spline1dinterpolant c;
            alglib.spline1dbuildcubic(x, y, out c);

            //计算拟合后的Y值
            for (int i = 0; i < x.Length; i++)
            {
                y[i] = alglib.spline1dcalc(c, x[i]);
            }

            //查找Y的最大值
            foundpos = 0;
            double maxy = y[0];
            for (int i = 1; i < y.Length; i++)
            {
                if (y[i] > maxy)
                {
                    foundpos = i;
                    maxy = y[i];
                }
            }

            double stepx = (x[1] - x[0]) / 100;   //按照X最小间隔的1/100来逐步逼近
            maxy = y[foundpos];
            double maxx = x[foundpos];

            //先查左边
            double cury = alglib.spline1dcalc(c, x[foundpos] - stepx);
            if (cury > maxy)   //左边的Y值大于最大Y值，因此最大Y值还在左边
                stepx *= -1;     //实际上每次计算要减小stepx

            for (int i = 1; i < 100; i++)
            {
                cury = alglib.spline1dcalc(c, x[foundpos] + i*stepx);
                if (cury > maxy)   //找到更大的Y值了
                {
                    maxx = x[foundpos] + i * stepx;
                    maxy = cury;
                }
                else
                    break;
            }
            newyvalue = maxy;

            return maxx;
        }

        #region 二元多次线性方程拟合曲线

        public static double PickPeakUseGussan(float[] xData, float[] yData, float peakValue, int pointsToCal, out double newyvalue)
        {
            //找到要标记的峰在xData中的最近位置
            int foundpos = FindNearestPosition(xData, 0, xData.Length - 1, peakValue);

            //查找该位置最近的峰
            double maxValue = yData[foundpos];
            if (yData[foundpos - 1] >= maxValue)    //下降曲线，峰在左边
            {
                while (foundpos > 0 && yData[foundpos - 1] > yData[foundpos])
                    foundpos--;
            }
            else    //上升曲线，峰在右边
            {
                while (foundpos < yData.Length - 1 && yData[foundpos + 1] > yData[foundpos])
                    foundpos++;
            }

            //取左右各pointsToCal个点来做曲线拟合
            double[] x = new double[pointsToCal * 2 + 1];
            double[] y = new double[pointsToCal * 2 + 1];
            for (int i = foundpos - pointsToCal; i <= foundpos + pointsToCal; i++)
            {
                x[i - (foundpos - pointsToCal)] = xData[i];
                y[i - (foundpos - pointsToCal)] = yData[i];
            }

            //计算曲线拟合参数
            double[] c = MultiLine(x, y, x.Length, 2);

            y = new double[pointsToCal * 2 + 1];
            //计算拟合后的Y值
            for (int i = 0; i < y.Length; i++)
            {
                y[i] = c[0];
                for (int j = 1; j < c.Length; j++)
                    y[i] += c[j] * Math.Pow(x[i], j);
            }

            //查找Y的最大值
            foundpos = 0;
            double maxy = y[0];
            for (int i = 1; i < y.Length; i++)
            {
                if (y[i] > maxy)
                {
                    foundpos = i;
                    maxy = y[i];
                }
            }

            double stepx = (x[1] - x[0]) / 100;   //按照X最小间隔的1/100来逐步逼近
            maxy = y[foundpos];
            double maxx = x[foundpos];

            //先查左边
            double cury = c[0];
            for (int j = 1; j < c.Length; j++)
                cury += c[j] * Math.Pow(x[foundpos] - stepx, j);

            if (cury > maxy)   //左边的Y值大于最大Y值，因此最大Y值还在左边
                stepx *= -1;     //实际上每次计算要减小stepx

            for (int i = 1; i < 100; i++)
            {
                //根据最小二乘法参数计算Y值
                cury = c[0];
                for (int j = 1; j < c.Length; j++)
                    cury += c[j] * Math.Pow(x[foundpos] + i * stepx, j);

                if (cury > maxy)   //找到更大的Y值了
                {
                    maxx = x[foundpos] + i * stepx;
                    maxy = cury;
                }
                else
                    break;
            }
            newyvalue = maxy;

            return maxx;
        }

        public static double IntegrateUseGussan(float[] xData, float[] yData, float freqStart, float freqEnd)
        {
            if (xData == null || yData == null || xData.Length == 0 || yData.Length == 0 || freqStart == freqEnd)
                return 0;

            float firstX = xData[0], lastX = xData[xData.Length - 1];

            //xData是否是递增的
            bool increase = lastX > firstX ? true : false;

            //起始和结束的顺序与xData相同
            if ((increase && freqStart > freqEnd) ||
                 (!increase && freqStart < freqEnd))
            {
                float temp = freqEnd;
                freqEnd = freqStart;
                freqStart = temp;
            }

            //判断积分起止位置是否在X数据范围内
            if (increase && (freqStart < firstX || freqEnd > lastX) ||
                !increase && (freqStart > firstX || freqEnd < lastX))
                return 0;

            //找到xData中的实际积分区间
            int beginIndex = 0, endIndex = 0;

            //查找freqStart
            for (int i = 0; i < xData.Length; i++)
            {
                if (increase && xData[i] > freqStart ||
                    !increase && xData[i] < freqStart)
                {
                    beginIndex = i;
                    break;
                }
            }

            //查找freqEnd
            for (int i = 0; i < xData.Length - 1; i++)
            {
                if (increase && xData[i + 1] > freqEnd ||
                    !increase && xData[i + 1] < freqEnd)
                {
                    endIndex = i;
                    break;
                }
            }

            //xData范围内没有找到freqStart或freqEnd
            if (beginIndex == 0 || endIndex == 0)
                return 0;

            //beginIndex和endIndex区间小于freqStart和freqEnd区间
            //计算freqStart位置的Y值
            float yStart = yData[beginIndex - 1] + (yData[beginIndex] - yData[beginIndex - 1]) *
                ((freqStart - xData[beginIndex - 1]) / (xData[beginIndex] - xData[beginIndex - 1]));

            //计算freqEnd位置的Y值
            float yEnd = yData[endIndex + 1] + (yData[endIndex] - yData[endIndex + 1]) *
                ((freqEnd - xData[endIndex + 1]) / (xData[endIndex] - xData[endIndex + 1]));

            //beginIndex到endIndex之间的面积
            double retvalue = 0;
            for (int i = beginIndex + 1; i <= endIndex; i++)
            {
                //计算xData[i]位置基线的Y值
                //float cury = yStart + (yEnd - yStart) * ((xData[i] - freqStart) / (freqEnd - freqStart));
                //if (yData[i] > cury)    //Y值在基线上方
                //    retvalue += (xData[i] - xData[i - 1]) * (yData[i] + yData[i - 1]) / 2;
                //else    //Y值在基线下方, 模拟反转到基线上方
                //    retvalue += (xData[i] - xData[i - 1]) * (cury + (cury - yData[i]) + cury + (cury - yData[i - 1])) / 2;

                retvalue += (xData[i] - xData[i - 1]) * (yData[i] + yData[i - 1]) / 2;

            }

            //计算beginIndex与freqStart之间的面积
            retvalue += Math.Abs((xData[beginIndex] - freqStart) * (yData[beginIndex] + yStart) / 2);

            //计算endIndex与freqEnd之间的面积
            retvalue += Math.Abs((freqEnd - xData[endIndex]) * (yEnd + yData[endIndex]) / 2);

            //扣除freqStart到freqEnd连接直线下方的面积
            retvalue -= Math.Abs((freqEnd - freqStart) * (yEnd + yStart) / 2);

            return retvalue;
        }


        ///<summary>
        ///用最小二乘法拟合二元多次曲线
        ///</summary>
        ///<param name="arrX">已知点的x坐标集合</param>
        ///<param name="arrY">已知点的y坐标集合</param>
        ///<param name="length">已知点的个数</param>
        ///<param name="dimension">方程的最高次数</param>
        public static double[] MultiLine(double[] arrX, double[] arrY, int length, int dimension)//二元多次线性方程拟合曲线
        {
            int n = dimension + 1;                  //dimension次方程需要求 dimension+1个 系数
            double[,] Guass = new double[n, n + 1];      //高斯矩阵 例如：y=a0+a1*x+a2*x*x
            for (int i = 0; i < n; i++)
            {
                int j;
                for (j = 0; j < n; j++)
                {
                    Guass[i, j] = SumArr(arrX, j + i, length);
                }
                Guass[i, j] = SumArr(arrX, i, arrY, 1, length);
            }
            return ComputGauss(Guass, n);

        }
        public static double SumArr(double[] arr, int n, int length) //求数组的元素的n次方的和
        {
            double s = 0;
            for (int i = 0; i < length; i++)
            {
                if (arr[i] != 0 || n != 0)
                    s = s + Math.Pow(arr[i], n);
                else
                    s = s + 1;
            }
            return s;
        }
        public static double SumArr(double[] arr1, int n1, double[] arr2, int n2, int length)
        {
            double s = 0;
            for (int i = 0; i < length; i++)
            {
                if ((arr1[i] != 0 || n1 != 0) && (arr2[i] != 0 || n2 != 0))
                    s = s + Math.Pow(arr1[i], n1) * Math.Pow(arr2[i], n2);
                else
                    s = s + 1;
            }
            return s;

        }
        //求解高斯方程
        public static double[] ComputGauss(double[,] Guass, int n)
        {
            int i, j;
            int k, m;
            double temp;
            double max;
            double s;
            double[] x = new double[n];

            for (i = 0; i < n; i++)
                x[i] = 0.0;//初始化
            for (j = 0; j < n; j++)
            {
                max = 0;    /*max不能为零*/

                k = j;      /*默认gauss[k=j][j]在地j列中最大,用k标记首个最大行*/
                for (i = j; i < n; i++)
                {
                    if (Math.Abs(Guass[i, j]) > max)
                    {
                        max = Guass[i, j];
                        k = i;
                    }
                }

                if (k != j) /*调换*/
                {
                    for (m = j; m < n + 1; m++)
                    {
                        temp = Guass[j, m];
                        Guass[j, m] = Guass[k, m];
                        Guass[k, m] = temp;

                    }
                }

                if (0 == max)
                {
                    // "此线性方程为奇异线性方程" 

                    return x;
                }


                for (i = j + 1; i < n; i++) /*对j+1行到第n行进行消元*/
                {

                    s = Guass[i, j];
                    for (m = j; m < n + 1; m++)
                    {
                        Guass[i, m] = Guass[i, m] - Guass[j, m] * s / (Guass[j, j]);

                    }
                }
            }//结束for (j=0;j<n;j++)

            for (i = n - 1; i >= 0; i--)    /*回代*/
            {
                s = 0;
                for (j = i + 1; j < n; j++)
                {
                    s = s + Guass[i, j] * x[j];
                }

                x[i] = (Guass[i, n] - s) / Guass[i, i];

            }

            return x;
        }//返回值是函数的系数
        #endregion

    }

    public static class ReportTemplate
    {
        public static string ErrorString = null;

        //保存到XPS格式文档
        public static bool SaveToXpsFile(string xpsFilename, FixedDocument fixedDoc)
        {
            try
            {
                //保存到XPS文件
                DocumentPaginator paginator = fixedDoc.DocumentPaginator;
                XpsDocument xpsDocument = new XpsDocument(xpsFilename, FileAccess.Write);
                System.Windows.Xps.XpsDocumentWriter documentWriter = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                documentWriter.Write(paginator);
                xpsDocument.Close();

                return true;
            }
            catch (Exception ex)
            {
                ErrorString = ex.Message;
                return false;
            }
        }

        //通过写入和读出Xaml方式克隆一个Object
        public static T CloneObject<T>(T obj)
        {
            string gridXaml = System.Windows.Markup.XamlWriter.Save(obj);
            MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(gridXaml));
            Object clone = System.Windows.Markup.XamlReader.Load(stream);
            return (T)clone;
        }

        public static void FillTextData(FrameworkElement rootEl, string IdName, string content)
        {
            object el = rootEl.FindName(IdName);
            if (el == null)
                return;

            if (el is TextBlock)
            {
                (el as TextBlock).Text = content;
            }
        }

        /// <summary>
        /// 加载打印模版
        /// </summary>
        /// <param name="templateName">模版文件名</param>
        /// <returns>加载后的FlowDocument</returns>
        public static FlowDocument LoadDocumentTemplate(string templateName)
        {
            Stream xamlStream = null;

            try
            {
                xamlStream = CommonMethod.StreamFromResource(templateName);
                if (xamlStream == null)
                    return null;

                return System.Windows.Markup.XamlReader.Load(xamlStream) as FlowDocument;
            }
            catch (System.Exception ex)
            {
                ErrorString =  ex.Message;
                return null;
            }
            finally
            {
                if (xamlStream != null)
                    xamlStream.Close();
            }
        }

        /// <summary>
        /// 显示光谱图形
        /// </summary>
        /// <param name="rootBorder">控件树的根</param>
        /// <param name="graphicBorderName">图像控件名</param>
        /// <param name="graphicFile">光谱文件</param>
        /// <param name="graphicWidth">图像的宽度</param>
        /// <param name="graphicHeight">图像的高度</param>
        /// <param name="DPI">图像分辨率</param>
        public static void ShowSpectrumGraphic(Border rootBorder, string graphicBorderName, string graphicFile, double graphicWidth, double graphicHeight = double.MaxValue, double DPI = double.MaxValue)
        {
            Border graphicBorder = rootBorder.FindName(graphicBorderName) as Border;
            if (graphicBorder != null)
            {
                System.Windows.Forms.DataVisualization.Charting.Chart graphicChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
                System.Windows.Forms.DataVisualization.Charting.ChartArea ca = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
                graphicChart.ChartAreas.Add(ca);
                Common.CommonMethod.DrawSpectrumGraphic(graphicChart, ca, graphicFile, System.Drawing.Color.Black);

                DPI = (DPI == double.MaxValue) ? 96 : DPI;
                graphicHeight = (graphicHeight == double.MaxValue) ? graphicBorder.Height * DPI / 96 : graphicHeight * DPI / 2.54;

                graphicChart.Width = (int)(graphicWidth * DPI / 2.54);        //1cm = 2.54inch = 96dpi
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
                img.Stretch = System.Windows.Media.Stretch.Uniform;
            }
        }
    }

    [XmlRoot("eftir_cls_ident_method")]
    public class eftir_cls_ident_method
    {
        public class region
        {
            [XmlElement("firstX")]
            public double firstX { get; set; }
            [XmlElement("status")]
            public string status { get; set; }
            [XmlElement("dof")]
            public int dof { get; set; }
            [XmlElement("lastX")]
            public double lastX { get; set; }
        }

        //分析组分信息
        public class analyte
        {
            /// <summary>
            /// status(include, exclude)
            /// </summary>
            [XmlElement("status")]
            public statusEnum status { get; set; }

            [XmlElement("name")]
            public string name { get; set; }

            /// <summary>
            /// 定义不明
            /// </summary>
            [XmlElement("cc")]
            public string cc { get; set; }

            [XmlElement("filename")]
            public string filename { get; set; }
            
            /// <summary>
            /// concentration factor(贡献值,0-1)
            /// </summary>
            [XmlElement("concentration")]
            public double concentration { get; set; }

            /// <summary>
            /// 定义不明
            /// </summary>
            [XmlElement("subfile")]
            public int subfile { get; set; }

            [XmlArray(ElementName = "regions")]     //Regions下面包含很多region
            [XmlArrayItem("region")]
            public List<region> regions { get; set; }
        }

        //参考光谱信息
        public class interferent
        {
            /// <summary>
            /// include, exclude
            /// </summary>
            [XmlElement("status")]
            public statusEnum status { get; set; }
            /// <summary>
            /// 辅料名
            /// </summary>
            [XmlElement("name")]
            public string name { get; set; }
            /// <summary>
            /// 通常为standard
            /// </summary>
            [XmlElement("cc")]
            public string cc { get; set; }
            /// <summary>
            /// 辅料光谱文件名
            /// </summary>
            [XmlElement("filename")]
            public string filename { get; set; }
            /// <summary>
            /// 通常为1
            /// </summary>
            [XmlElement("concentration")]
            public double concentration { get; set; }
            /// <summary>
            /// 通常为0
            /// </summary>
            [XmlElement("subfile")]
            public int subfile { get; set; }
        }

        /// <summary>
        /// Smoothing Method(光谱平滑方法)
        /// </summary>
        [XmlElement("derivativeMethod")]
        public int derivativeMethod { get; set; }

        /// <summary>
        /// Derivative Order(导数阶数)
        /// </summary>
        [XmlElement("derivativeOrder")]
        public int derivativeOrder { get; set; }

        /// <summary>
        /// Correlation Method(系数方法)
        /// </summary>
        [XmlElement("cc")]
        public string cc { get; set; }

        /// <summary>
        /// SNR thresold(信噪比阈值，只显示大于本值的结果)
        /// </summary>
        [XmlElement("snrThreshold")]
        public double snrThreshold { get; set; }

        /// <summary>
        /// Smoothing Method(平滑点数)
        /// </summary>
        [XmlElement("derivativePoints")]
        public int derivativePoints { get; set; }

        /// <summary>
        /// 噪声起始波数
        /// </summary>
        [XmlElement("noiseStart")]
        public double noiseStart { get; set; }

        /// <summary>
        /// Derivative Tail Handling(边缘数据处理方法)
        /// </summary>
        [XmlElement("derivativeTailHandling")]
        public int derivativeTailHandling { get; set; }

        /// <summary>
        /// 噪声结束波数
        /// </summary>
        [XmlElement("noiseEnd")]
        public double noiseEnd { get; set; }

        /// <summary>
        /// CC thresold(相关系数阈值，只显示大于本值的结果)
        /// </summary>
        [XmlElement("ccThreshold")]
        public double ccThreshold { get; set; }

        /// <summary>
        /// 文件版本
        /// </summary>
        [XmlElement("version")]
        public double version { get; set; }

        /// <summary>
        /// Baseline Corretion(基线校正方式)
        /// </summary>
        [XmlElement("baselineOrder")]
        public int baselineOrder { get; set; }

        /// <summary>
        /// analyte 检出组分，直接的列表，没有上某个Element下面
        /// </summary>
        [XmlElement("analyte")]
        public List<analyte> analytes { get; set; }

        /// <summary>
        /// interferent参考光谱，直接的列表，没有上某个Element下面
        /// </summary>
        [XmlElement("interferent")]
        public List<interferent> interferents { get; set; }

        public string ErrorString = null;
        public eftir_cls_ident_method()
        {
        }

        public void Clone(eftir_cls_ident_method method)
        {
            derivativeMethod = method.derivativeMethod;
            derivativeOrder = method.derivativeOrder;
            cc = method.cc;
            snrThreshold = method.snrThreshold;
            derivativePoints = method.derivativePoints;
            noiseStart = method.noiseStart;
            derivativeTailHandling = method.derivativeTailHandling;
            noiseEnd = method.noiseEnd;
            ccThreshold = method.ccThreshold;
            version = method.version;
            baselineOrder = method.baselineOrder;
            analytes = method.analytes;
            interferents = method.interferents;
        }

        public static eftir_cls_ident_method Deserialize(string methodFile)
        {
            try
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(methodFile, true);
                string fileContent = sr.ReadToEnd();
                sr.Close();
                
                fileContent = fileContent.Replace("\"", "");  //替换所有的"号
                StringReader textReader = new StringReader(fileContent);

                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(eftir_cls_ident_method));
                eftir_cls_ident_method curmethod = xs.Deserialize(textReader) as eftir_cls_ident_method;

                return curmethod;
            }
            catch (Exception)
            {
                return null;
            }
        }

        //序列化到文件
        public bool Serialize(string methodFile)
        {
            try
            {
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(eftir_cls_ident_method));
                StringWriter textWriter = new StringWriter();
                xs.Serialize(textWriter, this);

                StreamWriter stream = new StreamWriter(methodFile, false, Encoding.GetEncoding(SettingData.UTF8));
                string tempstr = textWriter.ToString();
                int index = tempstr.IndexOf("\r\n");
                index += 2;
                index = tempstr.IndexOf("\r\n", index) + 2;
                tempstr = "<eftir_cls_ident_method>\r\n" + tempstr.Substring(index);
                stream.WriteLine(tempstr);
                stream.Close();

                return true;
            }
            catch (Exception ex)
            {
                CommonMethod.ErrorMsgBox(ex.Message);
                return false;
            }

        }

    }

    /// <summary>
    /// 模型文件状态选项
    /// </summary>
    public enum statusEnum { include, exclude };

    /// <summary>
    /// 仪器类型 积分球、光纤 Fibre:光纤，IntegrateSphere：积分球
    /// </summary>
    public enum SystemTypeEnum { Fibre, IntegrateSphere };

     

    public class ModelSelRoutedEventArgs : RoutedEventArgs
    {
        public ModelInfo selModel { get; set; }

        public ModelSelRoutedEventArgs(ModelInfo info)
        {
            selModel = info;
        }
    }

}
