using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NirLib.BasicAlgorithm;
//using Ai.Hong.SpectrumAlgorithm;
using NirIdentifier.Common;
using VspecNIRTypeLib;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;

namespace NirIdentifier.Common
{
    /// <summary>
    /// 相似系数分析
    /// </summary>
    public static class DrugAnalyte
    {
        public static string ErrorString = null;

        public static void LoadingCallBack(string displayMsg, int maxProcessValue, int curProcessValue)
        {
        }

        /// <summary>
        /// 使用相似系数分析光谱
        /// </summary>
        /// <param name="analyteModel">分析模型</param>
        /// <param name="filename">待分析的光谱</param>
        /// <returns>相似系数值，如果返回值小于0，表示错误</returns>
        public static double CorCoeffAnalyte(ModelInfo analyteModel, string filename)
        {
            try 
	        {	        
                if(analyteModel == null || analyteModel.files == null || analyteModel.files.Count == 0)
                    throw new Exception("分析模型设置错误");
                if(filename == null)
                    throw new Exception("光谱文件错误");

                //将模型文件和光谱文件添加到一个列表中
                List<string> allFiles = new List<string>();
                var temp = from item in analyteModel.files select item.filename;
                allFiles.AddRange(temp);
                allFiles.Add(filename);

                //光谱有效区域
                NirLib.BasicAlgorithm.FileRegion[] regions = null;
                if(analyteModel.regions.Count>0)
                {
                    regions = new FileRegion[analyteModel.regions.Count];
                    for(int i=0; i<regions.Length; i++)
                    {
                        regions[i] = new FileRegion(analyteModel.regions[i].firstX, analyteModel.regions[i].lastX);
                    }
                }

                double xstep = 1;
                //Ai.Hong.NirAlgorithm.NormalAlgorithm.
                List<double[]> allDatas = Ai.Hong.NirAlgorithm.NormalAlgorithm.LoadFiles(allFiles, regions, LoadingCallBack, out xstep);
                if (allDatas == null)
                    throw new Exception(Ai.Hong.NirAlgorithm.NormalAlgorithm.ErrorString);

                //最后一个数据为待测光谱
                double[] spectrumData = allDatas[allDatas.Count - 1];

                //计算平均光谱
                allDatas.RemoveAt(allDatas.Count - 1);
                double[] AverageData = NirLib.BasicAlgorithm.BasicClusterAlgorithm.CreateAverageSpectrum(allDatas);

                //预处理光谱
                //矢量归一化
                AverageData = NirLib.BasicAlgorithm.BasicClusterAlgorithm.VertorNormalize(AverageData);
                spectrumData = NirLib.BasicAlgorithm.BasicClusterAlgorithm.VertorNormalize(spectrumData);

                //一阶导数,17个平滑点
                AverageData = NirLib.BasicAlgorithm.BasicClusterAlgorithm.SG_FirstDerivative(AverageData, xstep, 17);
                spectrumData = NirLib.BasicAlgorithm.BasicClusterAlgorithm.SG_FirstDerivative(spectrumData, xstep, 17);

                //相似系数
                double cc = Ai.Hong.NirAlgorithm.CorCoeffDistance.Distance(AverageData, spectrumData);

                return cc;
            }
	        catch (Exception ex)
	        {
                ErrorString = ex.Message;
		        return -1;
	        }
        }
    }

    /// <summary>
    /// 仪器接口
    /// </summary>
    public static class VspecInstrument
    {
        /// <summary>
        /// 仪器接口
        /// </summary>
        static VspecNIRTypeLib.VspecNIRObject instrumentObject = null;

        /// <summary>
        /// 是否是积分球仪器
        /// </summary>
        public static bool? IsIntegratingSphere;

        /// <summary>
        /// 错误代码
        /// </summary>
        private static int ErrorCode;

        /// <summary>
        /// 仪器是否联机
        /// </summary>
        private static bool isConnected;

        //获取错误信息
        public static string GetError()
        {
            switch (ErrorCode)
            {
                case -2:
                    return "没有找到加密卡";
                case -3:
                    return "加载扫描配置文件错误";
                case -4:
                    return "COM接口错误";
                case -5:
                    return "仪器扫描光谱出现错误";
                case -6:
                    return "光谱文件保存错误";
                case -10:   //这是我加的
                    return "创建接口对象错误";
                case -11:   //这是我加的
                    return "没有连接仪器";
                case -12:
                    return "没有生成AB光谱,请检查扫描配置文件";
                case -13:
                    return "重命名光谱文件错误";
                case -14:
                    return "LOCK FAULT";
                default:
                    return "未知错误 from scanner";
            }
        }

        /// <summary>
        /// 互斥对象，防止同时操作
        /// </summary>
        private static object thisLock = new object();
        /// <summary>
        /// 连接光谱仪
        /// </summary>
        public static bool Connect()
        {
            lock(thisLock)
            {
                if (instrumentObject == null)
                    instrumentObject = new VspecNIRObject();

                if (instrumentObject == null)
                {
                    ErrorCode = -10;
                    return false;
                }
                ErrorCode = instrumentObject.Connect();
                isConnected = (ErrorCode == 0);
                //判断仪器是否是积分球类型
                string jsonString = GetParametersTable();
                if (jsonString != null)
                {
                    JsonString.ParametersTable par = JsonString.JsonToObj<JsonString.ParametersTable>(jsonString);
                    IsIntegratingSphere = par.systemType == 2;
                }
                return isConnected;
            }
        }

        /// <summary>
        /// 断开仪器连接
        /// </summary>
        public static bool Disconnect()
        {
            lock(thisLock)
            {
                if (instrumentObject == null)
                    return true;

                if (!isConnected)
                    return true;

                ErrorCode = instrumentObject.Disconnect();
                
                GC.Collect();
                instrumentObject = null;
                isConnected = false;

                return ErrorCode == 0;
            }
        }

        /// <summary>
        /// 把激光波数写入仪器
        /// </summary>
        /// <param name="waveNum"></param>
        /// <returns></returns>
        //public static bool Executed(string waveNum)
        //{
        //    lock (thisLock)
        //    {
        //        if (!isConnected)
        //        {
        //            ErrorCode = -11;
        //            return false;
        //        }
        //        ErrorCode = instrumentObject.Execute(waveNum.ToString());
        //        return ErrorCode == 0;
        //    }
        //}

        /// <summary>
        /// 把激光波数写入仪器
        /// </summary>
        /// <param name="waveNum"></param>
        /// <returns></returns>
        public static bool SetLaserWavelength(string laserWavelength)
        {
            lock (thisLock)
            {
                if (!isConnected)
                {
                    ErrorCode = -11;
                    return false;
                }
                ErrorCode = instrumentObject.SetLaserWavelength(laserWavelength);
                return ErrorCode == 0;
            }
        }
        /// <summary>
        /// 获得仪器参数 格式:{"systemType":1,"serialNum":"VS1003","firmwareVer":3,"laserWavelen":"637.947265","velocities":[15],"resolutions":[32,16,8,4],"retVal":0}
        /// </summary>
        /// <returns></returns>
        public static string GetParametersTable()
        {
            lock(thisLock)
            {
                if(!isConnected)
                {
                    ErrorCode = -1;
                    return null;
                }
                string para="";
                ErrorCode = instrumentObject.GetParametersTable(ref para);
                return ErrorCode == 0 ? para : null;
            }
        }

        /// <summary>
        /// 灵敏度测试  格式：{"sensors":[{"id":1,"val":20.00000},{"id":2,"val":22.00000},{"id":3,"val":55.00000},{"id":4,"val":15.10000},{"id":5,"val":-40.00000}],"retVal":0}，ID2是温度
        /// </summary>
        /// <returns></returns>
        public static string ReadSensors()
        {
            lock(thisLock)
            {
                if(!isConnected)
                {
                    ErrorCode = -1;
                    return null;
                }
                string para = "";
                ErrorCode = instrumentObject.ReadSensors(ref para);
                return ErrorCode == 0 ? para : null;
            }
        }

        /// <summary>
        /// 移动转轮
        /// </summary>
        /// <param name="position">0 = 打开, 1 = NG4滤光玻璃, 2 = 聚苯乙烯, 3 = 过滤网</param>
        /// <returns></returns>
        public static bool MoveWheel(int position)
        {
            lock (thisLock)
            {
                if (!isConnected)
                {
                    ErrorCode = -11;
                    return false;
                }
                ErrorCode = instrumentObject.MoveWheel(position);
                return ErrorCode == 0;
            }
        }

        /// <summary>
        /// 是否是积分球类型仪器
        /// </summary>
        /// <returns></returns>
        public static bool IsMoveFlagBack()
        {
            return SettingData.systemType == Common.SystemTypeEnum.IntegrateSphere;
        }

        /// <summary>
        /// 移动积分球位置
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private static bool MoveFlag(int position)
        {
            lock (thisLock)
            {
                if (!isConnected)
                {
                    ErrorCode = -11;
                    return false;
                }

                ErrorCode = instrumentObject.MoveFlag(position);
                return ErrorCode==0;
            }
        }

        /// <summary>
        /// 扫描背景
        /// </summary>
        /// <param name="scanMethodFile">扫描配置文件</param>
        /// <param name="scanCount">扫描次数</param>
        /// <param name="backgroundFile">背景保存文件</param>
        public static string ScanBackground(string scanMethodFile, int scanCount, string backgroundFile,bool IsOVP)
        {
            lock(thisLock)
            {
                //移动积分球
                if (IsOVP)
                {
                    MoveFlag(1);
                }
                //加载扫描配置
                ErrorCode = instrumentObject.LoadSettings(scanMethodFile);
                if (ErrorCode != 0)
                    return null;

                ErrorCode = instrumentObject.CollectBackground(scanCount, backgroundFile);
                if (ErrorCode != 0)
                    return null;

                //背景文件名后面加了后缀_rsb
                string retFile = backgroundFile.Substring(0, backgroundFile.Length - 4) + "_rsb.spc";
                if (!File.Exists(retFile))
                {
                    ErrorCode = -12;
                    return null;
                }
                return retFile;
            }
        }

        /// <summary>
        /// 扫描样品
        /// </summary>
        /// <param name="scanMethodFile">扫描配置文件</param>
        /// <param name="scanCount">扫描次数</param>
        /// <param name="backgroundFile">样品保存文件</param>
        public static string ScanSample(string scanMethodFile, int scanCount, string sampleFile, bool? IsOVP)
        {
            lock (thisLock)
            {
                //移动积分球
                if (IsOVP.HasValue)
                {
                    if ((bool)IsOVP)
                    {
                        MoveFlag(1);
                    }
                    else
                    {
                        MoveFlag(0);
                    }
                    if (IsIntegratingSphere.HasValue)
                    {
                        if ((bool)IsIntegratingSphere)
                            instrumentObject.SampleSpinner(1);
                    }
                }
                //加载扫描配置
                ErrorCode = instrumentObject.LoadSettings(scanMethodFile);
                if (ErrorCode != 0)
                    return null;

                ErrorCode = instrumentObject.CollectSpectrum(scanCount, sampleFile);
                if (ErrorCode != 0)
                    return null;

                //扫描后的文件中有_abs,需要去掉_abs
                string scanedfile = sampleFile.Insert(sampleFile.Length - ".spc".Length, "_sbm");
                //Common.CommonMethod.ErrorMsgBox(scanedfile);
                if (!File.Exists(scanedfile))
                {
                    ErrorCode = -12;
                    return null;
                }

                //try
                //{
                //    File.Move(scanedfile, sampleFile);
                //}
                //catch (Exception)
                //{
                //    ErrorCode = -13;
                //}

                return scanedfile;// ErrorCode == 0;
            }
        }

    }

    /// <summary>
    /// JsonString 操作
    /// </summary>
    public class JsonString
    {
        public static string ErrorString;

        /// <summary>
        /// 序列化对象的到Json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetJsonString(object obj)
        {
            try
            {
                return new JavaScriptSerializer().Serialize(obj);
            }
            catch (Exception ex)
            {
                ErrorString = ex.ToString();
                return null;
            }
        }

        /// <summary>
        /// 反序列化得到json字符串对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T JsonToObj<T>(string json) where T : class
        {
            try
            {
                T obj = Activator.CreateInstance<T>();
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    DataContractJsonSerializer serializer;
                    serializer = new DataContractJsonSerializer(obj.GetType());
                    return (T)serializer.ReadObject(ms);
                }
            }
            catch (Exception ex)
            {
                ErrorString = ex.ToString();
                return null;
            }
        }

        /// <summary>
        /// 反序列化Json到List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="JsonStr"></param>
        /// <returns></returns>
        public static List<T> JsonToList<T>(string JsonStr)
        {
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            List<T> objs;
            try
            {
                objs = Serializer.Deserialize<List<T>>(JsonStr);
                return objs;
            }
            catch (Exception ex)
            {
                ErrorString = ex.ToString();
                return null;
            }
        }
        /// <summary>
        /// 仪器类型、序列号等
        /// </summary>
        public class ParametersTable
        {
            /// <summary>
            /// 仪器类型 1：光纤 2：积分球  3：积分球+透射 4：光纤+积分球+透射
            /// </summary>
            public int systemType { get; set; }

            /// <summary>
            /// 仪器型号
            /// </summary>
            public string serialNum { get; set; }

            /// <summary>
            /// 防火墙版本
            /// </summary>
            public int firmwareVer { get; set; }

            /// <summary>
            /// 激光波数(635 ~ 645)
            /// </summary>
            public double laserWavelen { get; set; }

            /// <summary>
            /// 扫描速度
            /// </summary>
            public int[] velocities { get; set; }

            /// <summary>
            /// 分辨率
            /// </summary>
            public int[] resolutions { get; set; }

            public int retVal { get; set; }
        }

        /// <summary>
        /// 仪器温度等
        /// </summary>
        public class Sensors
        {
            public int id { get; set; }

            public double val { get; set; }
        }

        /// <summary>
        /// ReadSensos To Obj
        /// </summary>
        public class GetSensors
        {
            public List<Sensors> sensors { get; set; }

            public int retVal { get; set; }

            public GetSensors()
            {
                sensors = new List<Sensors>();
            }
        }
    }

    /// <summary>
    /// 光谱扫描
    /// </summary>
    public class ScanTaskInfo
    {
        /// <summary>
        /// 扫描光谱文件
        /// </summary>
        private string spectrumFile { get; set; }

        /// <summary>
        /// 是否扫描成功
        /// </summary>
        public bool scanSuccessed { get; set; }

        /// <summary>
        /// 扫描背景还是样品
        /// </summary>
        public bool isBackground { get; set; }

        /// <summary>
        /// 扫描配置文件
        /// </summary>
        public SettingFile.scanParameter scanPara { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorString { get; set; }

        public ScanTaskInfo(SettingFile.scanParameter para, bool isBackground, string spectrumFile)
        {
            this.spectrumFile = spectrumFile;
            scanPara = para.Clone();
            //检查是否没有指定路径，如果没有，表示该文件在Setting目录下
            if (scanPara.scanSettingFile.IndexOf("\\") < 0 && scanPara.scanSettingFile.IndexOf("/") < 0)
                scanPara.scanSettingFile = System.IO.Path.Combine(SettingData.settingFolder, scanPara.scanSettingFile);

            this.isBackground = isBackground;
            this.scanSuccessed = false;
        }

        private void DeleteExtentedFile(string dstFile, string ext)
        {
            try
            {
                string tempstr = dstFile.ToLower().Replace(".spc", ext);
                if (File.Exists(tempstr))
                    File.Delete(tempstr);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 实际驱动仪器进行扫描
        /// </summary>
        public void DoScan(object t)
        {
            int scanCount = (int)t;
            ErrorString = null;

            if (spectrumFile == null || scanPara == null || scanPara.scanCount < 0 || !System.IO.File.Exists(scanPara.scanSettingFile))
            {
                scanSuccessed = false;
                ErrorString = "调用参数错误";
                return;
            }
            string temp = "";
            if (isBackground)
            {
                temp = VspecInstrument.ScanBackground(scanPara.scanSettingFile, scanCount, spectrumFile, Common.VspecInstrument.IsMoveFlagBack());
                scanSuccessed = temp == null ? false : true;
                if (temp == null)
                {
                    ErrorString = VspecInstrument.GetError();
                    scanSuccessed = false;
                    return;
                }
                //Delete _rif.spc File
                DeleteExtentedFile(spectrumFile, "_rif.spc");

                //Rename _rsb.spc to .spc file
                string tempstr = spectrumFile.ToLower().Replace(".spc", "_rsb.spc");
                if (File.Exists(spectrumFile))
                    File.Delete(spectrumFile);
                if (File.Exists(tempstr))
                    File.Move(tempstr, spectrumFile);
                else
                {
                    ErrorString = VspecInstrument.GetError();
                }

            }
            else
            {
                bool? IsMoveFlagTemp = null;
                if (Common.VspecInstrument.IsMoveFlagBack())
                {
                    IsMoveFlagTemp = false;
                }
                else
                {
                    IsMoveFlagTemp = null;
                }
                temp = VspecInstrument.ScanSample(scanPara.scanSettingFile, scanCount, spectrumFile, IsMoveFlagTemp);
                scanSuccessed = temp == null ? false : true;
                if (temp == null)
                {
                    ErrorString = VspecInstrument.GetError();
                    scanSuccessed = false;
                    return;
                }
                //Delete _rif.spc File
                DeleteExtentedFile(spectrumFile, "_ifg.spc");
                DeleteExtentedFile(spectrumFile, "_trn.spc");
                // DeleteExtentedFile(spectrumFile, "_sbm.spc");

                //Rename _rsb.spc to .spc file
                string tempstr = spectrumFile.ToLower().Replace(".spc", "_sbm.spc");

                if (File.Exists(spectrumFile))
                    File.Delete(spectrumFile);

                if (File.Exists(tempstr))
                    File.Move(tempstr, spectrumFile);
                else
                    ErrorString = VspecInstrument.GetError();
            }
        }
    }

}
