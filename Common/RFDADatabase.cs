using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace NirIdentifier.Common
{
    //药品信息表
    public enum EnumLicenseType 
    { 
        [Description("批准文号")]
        Approve,

        [Description("注册证号")]
        Registe
    }

    public enum EnumIdentifyMethod
    {
        [Description("相关系数")]
        Correlation,

        [Description("一致性")]
        Identity
    }

    public enum EnumIdentResult
    {
        [Description("通过")]
        OK,

        [Description("未通过")]
        FAULT,

        [Description("未知")]
        UNKNOWN
    }

    public enum EnumSavePathType
    {
        [Description("检测时间")]
        ScanDate,

        [Description("药品名称")]
        DrugName,

        [Description("生产厂家")]
        ProductUnit,

        [Description("药品注册码")]
        LicenseCode
    }

    /// <summary>
    /// 药品信息
    /// </summary>
    public class DrugInfo:Ai.Hong.CommonLibrary.spectrumDisplayInfo
    {
        /*
        public event PropertyChangedEventHandler PropertyChanged;
        private void DoProperChange(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        */
        private string _sampleNumber;
        /// <summary>
        /// 样品编号
        /// </summary>
        [XmlElement]
        public string sampleNumber
        {
            get { return _sampleNumber; }
            set { _sampleNumber = value; DoProperChange("sampleNumber"); }
        }

        private string _licenseType;
        /// <summary>
        /// 批准文号, 注册证号
        /// </summary>
        [XmlElement]
        public string licenseType
        {
            get { return _licenseType; }
            set { _licenseType = value; DoProperChange("licenseType"); }
        }

        private string _licenseCode;
        /// <summary>
        /// 批准文号 = passBatch(唯一值)
        /// </summary>
        [XmlElement]
        public string licenseCode
        {
            get { return _licenseCode; }
            set { _licenseCode = value; DoProperChange("licenseCode"); }
        }

        private string _commercialName;
        /// <summary>
        /// 通用名 = tradeCName
        /// </summary>
        [XmlElement]
        public string commercialName
        {
            get { return _commercialName; }
            set { _commercialName = value; DoProperChange("commercialName"); }
        }

        private string _chemicalName;
        /// <summary>
        /// 化学名称/药品名 = phyName
        /// </summary>
        [XmlElement]
        public string chemicalName
        {
            get { return _chemicalName; }
            set { _chemicalName = value; DoProperChange("chemicalName"); }
        }

        private string _form;
        /// <summary>
        /// 剂型 = doseType
        /// </summary>
        [XmlElement]
        public string form
        {
            get { return _form; }
            set { _form = value; DoProperChange("form"); }
        }

        private string _specification;
        /// <summary>
        /// 规格 = Specs
        /// </summary>
        [XmlElement]
        public string specification
        {
            get { return _specification; }
            set { _specification = value; DoProperChange("specification"); }
        }

        private string _productUnit;
        /// <summary>
        /// 生产单位 = nowProUnit
        /// </summary>
        [XmlElement]
        public string productUnit
        {
            get { return _productUnit; }
            set { _productUnit = value; DoProperChange("productUnit"); }
        }

        private DateTime _productTime;
        /// <summary>
        /// 生产日期
        /// </summary>
        [XmlElement]
        public DateTime productTime
        {
            get { return _productTime; }
            set { _productTime = value; DoProperChange("productTime"); }
        }

        private uint _validMonth;
        /// <summary>
        /// 有效期(月)
        /// </summary>
        [XmlElement]
        public uint validMonth
        {
            get { return _validMonth; }
            set { _validMonth = value; DoProperChange("validMonth"); }
        }

        private string _batchNumber;
        /// <summary>
        /// 产品批号
        /// </summary>
        [XmlElement]
        public string batchNumber
        {
            get { return _batchNumber; }
            set { _batchNumber = value; DoProperChange("batchNumber"); }
        }

        private string _scanType;
        /// <summary>
        /// 测样方式
        /// </summary>
        [XmlElement]
        public string scanType
        {
            get { return _scanType; }
            set { _scanType = value; DoProperChange("scanType"); }
        }

        private string _memo;
        /// <summary>
        /// 备注
        /// </summary>
        [XmlElement]
        public string memo
        {
            get { return _memo; }
            set { _memo = value; DoProperChange("memo"); }
        }

        private string _identUnit;
        /// <summary>
        /// 测量单位
        /// </summary>
        [XmlElement]
        public string identUnit
        {
            get { return _identUnit; }
            set { _identUnit = value; DoProperChange("identUnit"); }
        }

        private string _identOperator;
        /// <summary>
        /// 操作人员
        /// </summary>
        [XmlElement]
        public string identOperator
        {
            get { return _identOperator; }
            set { _identOperator = value; DoProperChange("identOperator"); }
        }

        private string _identInstrumentID;
        /// <summary>
        /// 分析仪器编号
        /// </summary>
        [XmlElement]
        public string identInstrumentID
        {
            get { return _identInstrumentID; }
            set { _identInstrumentID = value; DoProperChange("identInstrumentID"); }
        }

        private EnumIdentifyMethod _identMethod;
        /// <summary>
        /// 分析方法的类型（相似系数法、一致性验证法等
        /// </summary>
        [XmlElement]
        public EnumIdentifyMethod identMethod
        {
            get { return _identMethod; }
            set { _identMethod = value; DoProperChange("_identMethod"); }
        }

        private EnumIdentResult _identResult;
        /// <summary>
        /// 检测结果:OK, FAULT, UNKNOWN
        /// </summary>
        [XmlElement]
        public EnumIdentResult identResult
        {
            get { return _identResult; }
            set { _identResult = value; DoProperChange("identResult"); }
        }

        private double _identValue;
        /// <summary>
        /// 分析结果值
        /// </summary>
        [XmlElement]
        public double identValue
        {
            get { return _identValue; }
            set { _identValue = value; DoProperChange("identValue"); }
        }

        private double _identThresold;
        /// <summary>
        /// 阈值
        /// </summary>
        [XmlElement]
        public double identThresold
        {
            get { return _identThresold; }
            set { _identThresold = value; DoProperChange("identThresold"); }
        }

        private DateTime _identTime;
        /// <summary>
        /// 检测时间
        /// </summary>
        [XmlElement]
        public DateTime identTime
        {
            get { return _identTime; }
            set { _identTime = value; DoProperChange("identTime"); }
        }

        public string _identModel;
        /// <summary>
        /// 分析模型
        /// </summary>
        [XmlElement]
        public string identModel
        {
            get { return _identModel; }
            set { _identModel = value; DoProperChange("identModel"); }
        }

        /// <summary>
        /// 临时使用变量，不写入Xml
        /// </summary>
        [XmlIgnore]
        public int tempuse { get; set; }

        public DrugInfo()
        {
            if(SettingData.settingData.dictionary.licenseTypes != null && SettingData.settingData.dictionary.licenseTypes.Count>0)
               licenseType = SettingData.settingData.dictionary.licenseTypes[0];
            if (SettingData.settingData.dictionary.operators != null && SettingData.settingData.dictionary.operators.Count > 0)
                identOperator = SettingData.settingData.dictionary.operators[0];
            if (SettingData.settingData.dictionary.forms != null && SettingData.settingData.dictionary.forms.Count > 0)
                form = SettingData.settingData.dictionary.forms[0];
            if (SettingData.settingData.dictionary.scanTypes != null && SettingData.settingData.dictionary.scanTypes.Count > 0)
                scanType = SettingData.settingData.dictionary.scanTypes[0];

            identThresold = 0.97;
            identResult = EnumIdentResult.UNKNOWN;
        }

        public DrugInfo Clone()
        {
            return MemberwiseClone() as DrugInfo;
        }
    }

    /// <summary>
    /// 模型信息
    /// </summary>
    public class ModelInfo:INotifyPropertyChanged
    {
        public class region : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            private void DoProperChange(string name)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

            private double _firstX;
            [XmlElement("firstX")]
            public double firstX
            {
                get { return _firstX; }
                set { _firstX = value; DoProperChange("firstX"); }
            }

            private double _lastX;
            [XmlElement("lastX")]
            public double lastX
            {
                get { return _lastX; }
                set { _lastX = value; DoProperChange("lastX"); }
            }

            [XmlIgnore]
            public region thisObject { get; set; }

            public region()
            {
                firstX = 4000;
                lastX = 10000;
                thisObject = this;
            }

            public region(double firstX, double lastX)
            {
                this.firstX = firstX;
                this.lastX = lastX;
                thisObject = this;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void DoProperChange(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// 保存路径
        /// </summary>
        [XmlElement]
        public string licenseCode { get; set; }

        /// <summary>
        /// 化学名称/药品名 = phyName
        /// </summary>
        [XmlElement]
        public string chemicalName { get; set; }

        /// <summary>
        /// 剂型 = doseType
        /// </summary>
        [XmlElement]
        public string form { get; set; }

        /// <summary>
        /// 规格 = Specs
        /// </summary>
        [XmlElement]
        public string specification { get; set; }

        /// <summary>
        /// 生产单位 = nowProUnit
        /// </summary>
        [XmlElement]
        public string productUnit { get; set; }

        /// <summary>
        /// 阈值
        /// </summary>
        [XmlElement]
        public double thresold { get; set; }

        /// <summary>
        /// 指向本Class实例，操作过程中使用
        /// </summary>
        [XmlIgnore]
        public ModelInfo thisObject { get; set; }

        /// <summary>
        /// 光谱文件列表
        /// </summary>
        [XmlArray(ElementName = "files")]
        [XmlArrayItem("file")]
        public ObservableCollection<Ai.Hong.CommonLibrary.spectrumDisplayInfo> files { get; set; }

        [XmlArray(ElementName = "regions")]     //Regions下面包含很多region
        [XmlArrayItem("region")]
        public ObservableCollection<region> regions { get; set; }

        public ModelInfo()
        {
            thresold = 0.97;
            thisObject = this;
            files = new ObservableCollection<Ai.Hong.CommonLibrary.spectrumDisplayInfo>();
            regions = new ObservableCollection<region>();
        }

        public ModelInfo(DrugInfo drug)
        {
            thresold = 0.97;
            licenseCode = drug.licenseCode;
            productUnit = drug.productUnit;
            form = drug.form;
            specification = drug.specification;
            chemicalName = drug.chemicalName;
            thisObject = this;
            files = new ObservableCollection<Ai.Hong.CommonLibrary.spectrumDisplayInfo>();
            regions = new ObservableCollection<region>();
        }
    }

    //数据字典
    public class DictionaryInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public int id;
        /// <summary>
        /// 关键字
        /// </summary>
        public string keyWord;
        /// <summary>
        /// 词典内容
        /// </summary>
        public string keyContent;
    }

    //数据库连接
    public class DBConnection:IDisposable
    {
        static OleDbConnection myAccessConn = null;
        public static string ErrorString;
        private static string DBPassword = "wubkHuZDUgq6gekglVv5Og==";        //!**((AI.HONG@VSPEC))!
        public enum TDBableType { DRUG, METHODINFO, DICTIONARY, COMPONENTS, METHODNODE};
        private static string[] DBTableName = { "简化药品信息", "模型信息表", "数据字典", "药品组分信息表", "模型树形列表" };    //数据库中存在的表

        public DBConnection()
        {
            try
            {
                string detemp = Ai.Hong.CommonMethod.GetDatabaseTemplate(DBPassword, false);

                ErrorString = null;

                string DBFilename = Common.SettingData.settingData.runing_para.database;
                myAccessConn = new OleDbConnection();
                //detemp = "";
                myAccessConn.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Jet OLEDB:Database Password=" + detemp + ";Data Source=" + DBFilename;
                myAccessConn.Open();
            }
            catch (Exception ex)
            {
                ErrorString = ex.Message;
            }
        }

        #region 基本操作
        private static T GetData<T>(OleDbDataReader reader, string colName)
        {
            object value = reader.GetValue(reader.GetOrdinal(colName));
            if (!DBNull.Value.Equals(value))
                return (T)value;
            else
                return default(T);
        }

        public string GetTableName(TDBableType tableType)
        {
            return DBTableName[(int)tableType];
        }

        /// <summary>
        /// 获取最大的ID号，主要用于新增数据
        /// </summary>
        /// <param name="tableType">数据表格类型</param>
        /// <param name="IDName">id字段的名称</param>
        /// <returns>最新的ID号</returns>
        public int GetMaxID(TDBableType tableType, string IDName = "ID")
        {
            //获取最大的ID号，也就是新增加的ID
            OleDbCommand myAccessCommand = null;
            OleDbDataReader vOleDbDataReader = null;
            try
            {
                myAccessCommand = new OleDbCommand("select max(" + IDName + ") as maxid from " + DBTableName[(int)tableType], myAccessConn);
                vOleDbDataReader = myAccessCommand.ExecuteReader();
                vOleDbDataReader.Read();
                int maxid = (int)vOleDbDataReader["maxid"];

                return maxid;
            }
            catch (Exception ex)
            {
                ErrorString = ex.Message;
                return -1;
            }
            finally
            {
                if (vOleDbDataReader != null)
                {
                    vOleDbDataReader.Close();
                    vOleDbDataReader.Dispose();
                }
                if (myAccessCommand != null)
                    myAccessCommand.Dispose();
            }
        }

        /// <summary>
        /// 执行SQL命令
        /// </summary>
        /// <param name="sqlstr">Sql命令</param>
        /// <returns>执行结果</returns>
        public bool ExcuteSqlCommand(string sqlstr)
        {
            try
            {
                if (sqlstr == null)
                    throw new Exception("参数错误");

                using (OleDbCommand myAccessCommand = new OleDbCommand(sqlstr, myAccessConn))
                {
                    myAccessCommand.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorString = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 通用的数据删除过程
        /// </summary>
        /// <param name="tableType">数据表类型（名称）</param>
        /// <param name="conditionstr">删除条件（不带where)</param>
        /// <returns>操作结果</returns>
        public bool DeleteData(TDBableType tableType, string conditionstr)
        {
            return ExcuteSqlCommand("delete from " + DBTableName[(int)tableType] + " where " + conditionstr);
        }

        public bool ClearTable(TDBableType tableType)
        {
            return ExcuteSqlCommand("delete from " + DBTableName[(int)tableType]);
        }

        /// <summary>
        /// 通用的数据删除过程, 通常情况下是安装ID字段来删除
        /// </summary>
        /// <param name="tableType">数据表类型（名称）</param>
        /// <param name="id">需要删除的数据的ID号</param>
        /// <returns></returns>
        public bool DeleteData(TDBableType tableType, int id)
        {
            return DeleteData(tableType, "ID=" + id);
        }

        /// <summary>
        /// 返回指定表的字段名称
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public List<string> GetTableColumn(string tableName)
        {
            //string connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + @"C:\信息技术考试.mdb";
            //OleDbConnection conn = new OleDbConnection(connString);
            List<string> list = new List<string>();
            DataTable dt = new DataTable();
            try
            {

                dt=myAccessConn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, tableName, null });
                int n = dt.Rows.Count;
                int m = dt.Columns.IndexOf("COLUMN_NAME");
                foreach(DataRow dr in dt.Rows)
                {
                    list.Add( dr.ItemArray.GetValue(m).ToString());
                }
                list.Remove("测试时间");
                return list;
            }
            catch (Exception ex)
            {
                ErrorString = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 获取指定表指定列在指定时间段内的数据
        /// </summary>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <param name="list"></param>
        /// <param name="listPqOq"></param>
        /// <returns></returns>
        public List<string> GetColumnsInf(DateTime beginDate, DateTime endDate, string list, string listPqOq)
        {
            List<string> res = new List<string>();
            string strsql = "select * from " + list + "结果 where ";
            if (((DateTime)beginDate).ToString(Common.SettingData.ShortDateTimeString).Equals(((DateTime)endDate).ToString(Common.SettingData.ShortDateTimeString)))
            {
                string[] tt = ((DateTime)beginDate).ToString(Common.SettingData.ShortDateTimeString).Split(' ');
                strsql += " 测试时间 >=#" + tt[0] + " 0:0:0#";
                strsql += " and 测试时间 <=#" + tt[0] + " 23:59:59#";
            }
            else
            {
                strsql += " 测试时间 >=#" + ((DateTime)beginDate).ToString(Common.SettingData.ShortDateTimeString) + "#";
                strsql += " and 测试时间 <=#" + ((DateTime)endDate).ToString(Common.SettingData.ShortDateTimeString) + "#";
            }
            OleDbCommand myAccessCommand = new OleDbCommand(strsql, myAccessConn);
            OleDbDataReader myDataReader = myAccessCommand.ExecuteReader();
            while (myDataReader.Read())
            {
                if (listPqOq == "100%线斜率测试结果")
                    res.Add(GetData<string>(myDataReader, listPqOq));
                else
                    res.Add(GetData<double>(myDataReader, listPqOq).ToString());
            }
            myDataReader.Close();
            return res;
        }

        public List<string> GetComponentNameList()
        {
            try
            {
                List<string> methods = new List<string>();

                string sqlstr = "select DISTINCT 组分名称 from " + DBTableName[(int)TDBableType.COMPONENTS];
                OleDbCommand myAccessCommand = new OleDbCommand(sqlstr, myAccessConn);
                OleDbDataReader myDataReader = myAccessCommand.ExecuteReader();

                while (myDataReader.Read())
                {
                    methods.Add(GetData<string>(myDataReader, "组分名称"));
                }
                myDataReader.Close();

                return methods;
            }
            catch (Exception ex)
            {
                ErrorString = ex.Message;
                return null;
            }
        }
        #endregion  基本操作

        #region 药品检测信息表操作
        /// <summary>
        /// 通过药品注册码获取药品信息
        /// </summary>
        /// <returns>药品检测信息</returns>
        public DrugInfo GetDrugInfoFromLicense(string licenseCode)
        {
            try
            {
                List<DrugInfo> drugs = new List<DrugInfo>();

                string sqlstr = "select * from " + DBTableName[(int)TDBableType.DRUG] + " where passBatch='" + licenseCode+"'";
                OleDbCommand myAccessCommand = new OleDbCommand(sqlstr, myAccessConn);
                OleDbDataReader myDataReader = myAccessCommand.ExecuteReader();

                DrugInfo retDrugInfo = null;
                if (myDataReader.Read())
                {
                    retDrugInfo = new DrugInfo();
                    retDrugInfo.chemicalName = GetData<string>(myDataReader, "phyName");
                    retDrugInfo.commercialName = GetData<string>(myDataReader, "tradeCName");
                    retDrugInfo.productUnit = GetData<string>(myDataReader, "nowProUnit");
                    retDrugInfo.form = GetData<string>(myDataReader, "doseType");
                    retDrugInfo.specification = GetData<string>(myDataReader, "Specs");
                    retDrugInfo.licenseCode = GetData<string>(myDataReader, "passBatch");
                }
                myDataReader.Close();

                return retDrugInfo;
            }
            catch (Exception ex)
            {
                ErrorString = ex.Message;
                return null;
            }
        }

        //分析药品的规格中的含量
        private float AnalysisDrugSpecs(string specsStr, string firstStr)
        {
            int index = specsStr.IndexOf(firstStr, StringComparison.OrdinalIgnoreCase);
            if (index <= 0)
                return float.MinValue;

            int beginIndex = index;
            for (int i = index - 1; i >= 0; i--)
            {
                if ((specsStr[i] < '0' || specsStr[i] > '9') && specsStr[i] != '.')
                    break;
                beginIndex--;
            }
            string valueStr = specsStr.Substring(beginIndex, index - beginIndex);

            float retValue = 0;
            if (float.TryParse(valueStr, out retValue) == false)
                return float.MinValue;

            return retValue;
        }

        /// <summary>
        /// 新增药品检测信息
        /// </summary>
        /// <param name="drugInfo">药品检测信息</param>
        /// <returns>操作结果</returns>
        public bool InsertDrugInfo(DrugInfo drugInfo)
        {
            string sqlstr = "insert into " + DBTableName[(int)TDBableType.DRUG];
            sqlstr += "(phyName, nowProUnit, doseType, Specs, packSpecs, passBatch) values(";
            sqlstr += "'" + drugInfo.chemicalName + "',";
            sqlstr += "'" + drugInfo.productUnit + "',";
            sqlstr += "'" + drugInfo.form + "',";
            sqlstr += "'" + drugInfo.specification + "',";
            sqlstr += "'" + drugInfo.licenseCode + "')";

            if (ExcuteSqlCommand(sqlstr) == false)
                return false;

            return true;
        }

        public bool UpdateDrugInfo(DrugInfo drugInfo)
        {
            string sqlstr = "update " + DBTableName[(int)TDBableType.DRUG] + " set ";
            sqlstr += " phyName='" + drugInfo.chemicalName + "',";
            sqlstr += " nowProUnit='" + drugInfo.productUnit + "',";
            sqlstr += " doseType='" + drugInfo.form + "',";
            sqlstr += " Specs='" + drugInfo.specification;
            sqlstr += " passBatch='" + drugInfo.licenseCode + "'";

            if (ExcuteSqlCommand(sqlstr) == false)
                return false;

            return true;
        }
        #endregion

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    try
                    {
                        if (myAccessConn != null)
                            myAccessConn.Close();

                        myAccessConn = null;
                        disposed = true;
                    }
                    catch (Exception ex)
                    {
                        ErrorString = ex.Message;
                    }
                }
            }
        }

        ~DBConnection()
        {
            Dispose(false);
        }

   
        /// <summary>
        /// Data为PQ或者OQ对象
        /// </summary>
        /// <param name="Data"></param>
        public bool InsertPQOQ(object Data)
        {
            PQData pData = Data as PQData;
            string sqlstr;
            if (pData != null)
            {
                sqlstr = "insert into PQ测试结果(测试时间,[100%线噪声测试结果],[100%线偏差测试结果],波数精度测试结果,吸收重复性测试结果) values(";
                sqlstr += "#" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "# ,";
                sqlstr += pData.lineNoise + ",";
                sqlstr += pData.lineDev + ",";
                sqlstr += pData.waveNum + ",";
                sqlstr += pData.yaxisRep+")";
            }
            else
            {
                OQData oData = Data as OQData;
                sqlstr = "insert into OQ测试结果(测试时间,分辨率测试结果,[100%线噪声测试结果],能量分布测试结果,[100%线斜率测试结果],透射重复性测试结果,波数精度测试结果,波数重复性测试结果) values(";
                sqlstr += "#" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "# ,";
                sqlstr += oData.resolution + ",";
                sqlstr += oData.lineNoise + ",";
                sqlstr += oData.energyDis + ",";
                sqlstr +="'"+ oData.lineSlope+"',";
                sqlstr += oData.transRep + ",";
                sqlstr += oData.waveNum + ",";
                sqlstr += oData.waveNumRep+")";
            }
            try
            {
                ExcuteSqlCommand(sqlstr);
            }
            catch (Exception ex)
            {
                ErrorString = ex.ToString();
                return false;
            }
            return true;
        }
    }

    public class PQData
    {
        /// <summary>
        /// PQ 100%线噪声测试结果
        /// </summary>
        public double lineNoise { get; set; }

        /// <summary>
        /// PQ 100%线偏差测试结果
        /// </summary>
        public double lineDev { get; set; }

        /// <summary>
        /// PQ 波数精度测试结果
        /// </summary>
        public double waveNum { get; set; }

        /// <summary>
        /// PQ 吸收重复性测试结果
        /// </summary>
        public double yaxisRep { get; set; }

        

        public PQData(double lineNoise, double lineDev, double waveNum, double yaxisRep)
        {
            this.lineNoise = lineNoise;
            this.lineDev = lineDev;
            this.waveNum = waveNum;
            this.yaxisRep = yaxisRep;
        }
        public PQData() { }
    }
    public class OQData
    {
        /// <summary>
        /// OQ 分辨率测试结果
        /// </summary>
        public double resolution { get; set; }

        /// <summary>
        /// OQ 100%线噪声测试结果
        /// </summary>
        public double lineNoise { get; set; }

        /// <summary>
        /// OQ 能量分布测试结果
        /// </summary>
        public double energyDis { get; set; }

        /// <summary>
        /// OQ 100%线斜率测试结果
        /// </summary>
        public string lineSlope { get; set; }

        /// <summary>
        /// OQ 透射重复性测试
        /// </summary>
        public double transRep { get; set; }

        /// <summary>
        /// OQ 波数精度测试
        /// </summary>
        public double waveNum { get; set; }

        /// <summary>
        /// OQ 波数重复性测试
        /// </summary>
        public double waveNumRep { get; set; }

        public OQData(double resolution, double lineNoise, double energyDis, string lineSlope, double transRep, double waveNum, double waveNumRep)
        {
            this.resolution = resolution;
            this.lineNoise = lineNoise;
            this.energyDis = energyDis;
            this.lineSlope = lineSlope;
            this.transRep = transRep;
            this.waveNum = waveNum;
            this.waveNumRep = waveNumRep;
        }
        public OQData() { }
    }
}
