using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.ComponentModel;
using System.Xml.Serialization;

namespace RedryingOnline.Common
{
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
        private static string DBPassword = "RFDI@mtg-tech.com.cn";     //最后需要设置Password: RFDI@mtg-tech.com.cn
        public enum TDBableType { AnalysisResult, DICTIONARY, EachPackResult};
        private static string[] DBTableName = { "检测结果表", "字典信息表", "每箱检测结果表"};    //数据库中存在的表

        public DBConnection()
        {
            try
            {
                ErrorString = null;

                string DBFilename = Common.SettingData.settingData.runing_para.database;
                //检查有没有路径, 如果没有，表示文件在setting目录下面
                if (DBFilename.IndexOf("\\") < 0 && DBFilename.IndexOf("/") < 0)
                    DBFilename = System.IO.Path.Combine(Common.SettingData.SettingPath, DBFilename);

                myAccessConn = new OleDbConnection();
                if (System.IO.Path.GetFileName(DBFilename).IndexOf("RFDI_", 0, StringComparison.OrdinalIgnoreCase) != 0)
                    DBPassword = "";
                myAccessConn.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Jet OLEDB:Database Password=" + DBPassword + ";Data Source=" + DBFilename;
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
        /// 根据条件获取ID
        /// </summary>
        /// <param name="tableType">表名</param>
        /// <param name="condition">条件</param>
        /// <returns>ID号，-1表示错误</returns>
        public int GetTableID(TDBableType tableType, string condition)
        {
            try
            {
                string sqlstr = "select ID from " + DBTableName[(int)tableType];
                if (condition != null)
                    sqlstr += condition;

                OleDbCommand myAccessCommand = new OleDbCommand(sqlstr, myAccessConn);
                OleDbDataReader myDataReader = myAccessCommand.ExecuteReader();

                if (!myDataReader.Read())
                    return -1;

                int id = GetData<int>(myDataReader, "ID");

                myDataReader.Close();

                return id;
            }
            catch (Exception ex)
            {
                ErrorString = ex.Message;
                return -1;
            }
        }

        #endregion  基本操作

        #region 检测信息表操作
        /// <summary>
        /// 查询检测结果信息，参数可以为空
        /// </summary>
        /// <param name="isPackData">是否查询一箱的平均结果</param>
        /// <returns>查询检测结果信息</returns>
        public List<ResultDisplayInfo> GetAnalysisResult(productInfo info, DateTime beginDate, DateTime endDate, bool isPackData=false)
        {
            try
            {
                //构建查询语句
                TDBableType datatype = isPackData == false ? TDBableType.AnalysisResult : TDBableType.EachPackResult;
                string sqlstr = "select * from " + DBTableName[(int)datatype] + " where ID>=1 "; //添加一个ID>=1防止后面的and出现错误
                if (((DateTime)beginDate).ToString(Common.SettingData.ShortDateTimeString).Equals(((DateTime)endDate).ToString(Common.SettingData.ShortDateTimeString)))
                {
                    string[] tt = ((DateTime)beginDate).ToString(Common.SettingData.ShortDateTimeString).Split(' ');
                    sqlstr += " and 时间 >=#" + tt[0]+" 0:0:0#";
                    sqlstr += " and 时间 <=#" + tt[0]+ " 23:59:59#";
                }
                else
                {
                    sqlstr += " and 时间 >=#" + ((DateTime)beginDate).ToString(Common.SettingData.ShortDateTimeString) + "#";
                    sqlstr += " and 时间 <=#" + ((DateTime)endDate).ToString(Common.SettingData.ShortDateTimeString) + "#";
                }
                if (info.custom != "")
                    sqlstr += " and 客户='" + info.custom.Trim() + "'";
                if (info.grade != "")
                    sqlstr += " and 等级='" + info.grade.Trim() + "'";
                if (info.worker != "")
                    sqlstr += " and 班组='" + info.worker.Trim() + "'";
                if (info.place != "")
                    sqlstr += " and 产地='" + info.place.Trim() + "'";
                if (info.year != "" && info.year != "0")
                {
                    sqlstr += " and 年份=" + info.year.ToString();
                }
                OleDbCommand myAccessCommand = new OleDbCommand(sqlstr, myAccessConn);
                OleDbDataReader myDataReader = myAccessCommand.ExecuteReader();

                List<ResultDisplayInfo> analysisList = new List<ResultDisplayInfo>();

                while (myDataReader.Read())
                {
                    ResultDisplayInfo data = new ResultDisplayInfo();

                    data.id = GetData<int>(myDataReader, "ID");
                    data.time = GetData<DateTime>(myDataReader, "时间");
                    data.custom = GetData<string>(myDataReader, "客户");
                    data.grade = GetData<string>(myDataReader, "等级");
                    data.worker = GetData<string>(myDataReader, "班组");
                    data.place = GetData<string>(myDataReader, "产地");
                    data.year = GetData<int>(myDataReader, "年份");
                    data.output = GetData<int>(myDataReader, "流量");
                    data.filename = GetData<string>(myDataReader, "光谱名称");
                    data.values = new double[SettingData.settingData.programParamerter.dbComponentNames.Count];

                    //读取组分值
                    for (int i = 0; i < SettingData.settingData.programParamerter.dbComponentNames.Count; i++)
                    {
                        data.values[i] = GetData<double>(myDataReader, SettingData.settingData.programParamerter.dbComponentNames[i]);
                    }
                    analysisList.Add(data);
                }
                myDataReader.Close();

                for (int i = 0; i < analysisList.Count; i++)
                {
                    for (int j = i; j < analysisList.Count; j++)
                    {
                        if (analysisList[i].time > analysisList[j].time)
                        {
                            ResultDisplayInfo temp = null;
                            temp = analysisList[i];
                            analysisList[i] = analysisList[j];
                            analysisList[j] = temp;
                        }

                    }
                }
                return analysisList;
            }
            catch (Exception ex)
            {
                ErrorString = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 新增检测结果信息
        /// </summary>
        /// <param name="data">检测结果信息</param>
        /// <param name="isPackData">是否新增一箱的平均结果</param>
        /// <returns>操作结果</returns>
        public bool InsertAnalysisResult(ResultDisplayInfo data, bool isPackData = false)
        {
            TDBableType datatype = isPackData == false ? TDBableType.AnalysisResult : TDBableType.EachPackResult;
            string sqlstr = "insert into " + DBTableName[(int)datatype];
            sqlstr += "(时间, 客户, 等级, 班组, 产地, 年份, 流量, 光谱名称";

            //组分名称
            foreach (string name in SettingData.settingData.programParamerter.dbComponentNames)
                sqlstr += ", " + name;
            sqlstr += ") values(";
            //sqlstr += "convert(DateTime,'"  + data.time.ToString(Common.SettingData.LongDateTimeString) + "', 20) ,";
            sqlstr += "#"  + data.time.ToString(Common.SettingData.LongDateTimeString) + "# ,";
            sqlstr += "'" + data.custom + "',";
            sqlstr += "'" + data.grade + "',";
            sqlstr += "'" + data.worker + "',";
            sqlstr += "'" + data.place + "',";
            sqlstr += data.year + ",";
            sqlstr += data.output + ",";
            sqlstr += "'" + data.filename + "'";

            //组分值
            foreach (double value in data.values)
                sqlstr += ", "+ value.ToString();
            sqlstr += ")";

            if (ExcuteSqlCommand(sqlstr) == false)
                return false;

            if ((data.id = GetMaxID(datatype)) < 0)
                return false;

            return true;
        }

        /// <summary>
        /// 修改检测结果信息
        /// </summary>
        /// <param name="data">检测结果信息</param>
        /// <param name="isPackData">是否修改一箱的平均结果</param>
        /// <returns>操作结果</returns>
        public bool UpdateAnalysisResult(ResultDisplayInfo data, bool isPackData = false)
        {
            TDBableType datatype = isPackData == false ? TDBableType.AnalysisResult : TDBableType.EachPackResult;
            string sqlstr = "update " + DBTableName[(int)datatype] + " set ";
            sqlstr += " 时间 = convert(DateTime, "  + data.time.ToString(Common.SettingData.LongDateTimeString) + ", 20) ,";
            sqlstr += " 客户='" + data.custom + "',";
            sqlstr += " 等级='" + data.grade + "',";
            sqlstr += " 班组='" + data.worker + "',";
            sqlstr += " 产地='" + data.place + "',";
            sqlstr += " 年份=" + data.year;
            sqlstr += " 流量=" + data.output;
            sqlstr += " 光谱名称='" + data.filename + "'";
            //组分名称和值
            for (int i = 0; i < data.values.Length; i++ )
            {
                sqlstr += "," + SettingData.settingData.programParamerter.dbComponentNames[i] + "=" + data.values[i].ToString();
            }
            sqlstr += " where ID=" + data.id;

            if (ExcuteSqlCommand(sqlstr) == false)
                return false;

            return true;
        }
        #endregion

        #region 数据字典表操作
        /// <summary>
        /// 根据关键字获取内容列表, 如果getKey=true, 忽略keyWord
        /// </summary>
        /// <param name="getKey">是否获取数据字典的KEY</param>
        /// <param name="keyWord">获取数据字典dictKey所代表的内容</param>
        /// <returns>内容列表</returns>
        public List<string> GetDictInfo(bool getKey, string keyWord)
        {
            try
            {
                List<string> contents = new List<string>();

                string sqlstr;
                if (getKey)
                    sqlstr = "select DISTINCT 关键字 as aa from " + DBTableName[(int)TDBableType.DICTIONARY];
                else
                    sqlstr = "select DISTINCT 内容 as aa from " + DBTableName[(int)TDBableType.DICTIONARY] + " where 关键字='" + keyWord + "'";

                OleDbCommand myAccessCommand = new OleDbCommand(sqlstr, myAccessConn);
                OleDbDataReader myDataReader = myAccessCommand.ExecuteReader();

                while (myDataReader.Read())
                {
                    contents.Add(GetData<string>(myDataReader, "aa"));
                }
                myDataReader.Close();

           //     contents.RemoveAt(5);
                return contents;
            }
            catch (Exception ex)
            {
                ErrorString = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 获取所有的数据字典信息
        /// </summary>
        /// <returns>数据字典列表</returns>
        public List<DictionaryInfo> GetDictInfo()
        {
            try
            {
                List<DictionaryInfo> components = new List<DictionaryInfo>();

                string sqlstr = "select * from " + DBTableName[(int)TDBableType.DICTIONARY];
                OleDbCommand myAccessCommand = new OleDbCommand(sqlstr, myAccessConn);
                OleDbDataReader myDataReader = myAccessCommand.ExecuteReader();

                while (myDataReader.Read())
                {
                    DictionaryInfo info = new DictionaryInfo();
                    info.id = GetData<int>(myDataReader, "ID");
                    info.keyWord = GetData<string>(myDataReader, "关键字");
                    info.keyContent = GetData<string>(myDataReader, "内容");

                    components.Add(info);
                }
                myDataReader.Close();

                return components;
            }
            catch (Exception ex)
            {
                ErrorString = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 查找数据字典中是否存在指定条目
        /// </summary>
        /// <param name="keyWord">关键字</param>
        /// <param name="content">内容</param>
        /// <returns>该条目的ID号, -1标示没有找到条目</returns>
        public int GetDictItemID(string keyWord, string content)
        {
            string sqlstr = "select ID from " + DBTableName[(int)TDBableType.DICTIONARY] + " where 关键字='" + keyWord + "' and 内容='" + content + "'";

            OleDbCommand myAccessCommand = new OleDbCommand(sqlstr, myAccessConn);
            OleDbDataReader myDataReader = myAccessCommand.ExecuteReader();
            int id = -1;
            while (myDataReader.Read())
            {
                id = GetData<int>(myDataReader, "ID");
                break;
            }
            myDataReader.Close();

            return id;
        }

        /// <summary>
        /// 插入新的数据字典内容
        /// </summary>
        /// <param name="KeyWord">关键字</methodname>
        /// <param name="content">内容</param>
        /// <returns>插入数据的ID号</returns>
        public int InsertDictInfo(string KeyWord, string content)
        {
            if (KeyWord == null || KeyWord.Trim() == "" || content == null || content.Trim() == "")
                return -1;

            int id = GetDictItemID(KeyWord, content);
            if (id != -1)
                return id;

            string sqlstr = "insert into " + DBTableName[(int)TDBableType.DICTIONARY] + "(关键字, 内容) values('" + KeyWord + "','" + content + "')";
            if (ExcuteSqlCommand(sqlstr))
                return GetMaxID(TDBableType.DICTIONARY);
            else
                return -1;
        }

        /// <summary>
        /// 更新数据字典
        /// </summary>
        /// <param name="dictID">要更新数据的ID号</methodname>
        /// <param name="KeyWord">关键字</methodname>
        /// <param name="content">内容</param>
        /// <returns>操作结果</returns>
        public bool UpdateDictInfo(int dictID, string KeyWord, string content)
        {
            string sqlstr = "update " + DBTableName[(int)TDBableType.DICTIONARY] + " set 关键字='" + KeyWord + "', 内容='" + content + "' where ID=" + dictID;
            return ExcuteSqlCommand(sqlstr);
        }

        public bool UpdateDictInfo(string KeyWord, string oldContent, string newContent)
        {
            string sqlstr = "update " + DBTableName[(int)TDBableType.DICTIONARY] + " set 内容='" + newContent +
                "' where 关键字='" + KeyWord + "' and 内容='" + oldContent + "'";
            return ExcuteSqlCommand(sqlstr);
        }

        /// <summary>
        /// 根据关键字删除内容
        /// </summary>
        /// <param name="KeyWord">关键字</param>
        /// <param name="content">内容</param>
        /// <returns>操作结果</returns>
        public bool DeleteDictConent(string KeyWord, string content)
        {
            string sqlstr = "delete from " + DBTableName[(int)TDBableType.DICTIONARY] + " where 关键字='" + KeyWord + "' and 内容='" + content + "'";
            return ExcuteSqlCommand(sqlstr);
        }

        /// <summary>
        /// 删除关键字及其所包含的内容
        /// </summary>
        /// <param name="KeyWord">关键字</param>
        /// <returns>操作结果</returns>
        public bool DeleteDictKey(string KeyWord)
        {
            string sqlstr = "delete from " + DBTableName[(int)TDBableType.DICTIONARY] + " where 关键字='" + KeyWord + "'";
            return ExcuteSqlCommand(sqlstr);
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
    }

}
