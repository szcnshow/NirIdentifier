using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using NirIdentifier.Common;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.ObjectModel;

//20131014:增加了txtIdentNumber作为检品号，将其放在IdentifyInfo中，修改了文件名命名规则:药品名_厂家_规格_批号_检品号(没有输入则不加)_时间

namespace NirIdentifier.Detect
{
    /// <summary>
    /// NewSample.xaml 的交互逻辑
    /// </summary>
    public partial class NewSample : UserControl
    {
        /// <summary>
        /// 退出消息
        /// </summary>
        public static readonly RoutedEvent NewPanelExitEvent = EventManager.RegisterRoutedEvent("NewPanelExit",
                RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NewSample));
        public event RoutedEventHandler NewPanelExit
        {
            add { AddHandler(NewPanelExitEvent, value); }
            remove { RemoveHandler(NewPanelExitEvent, value); }
        }

        /// <summary>
        /// 新样品消息
        /// </summary>
        public static readonly RoutedEvent NewSampleDataEvent = EventManager.RegisterRoutedEvent("NewSampleData",
                RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NewSample));
        public event RoutedEventHandler NewSampleData
        {
            add { AddHandler(NewSampleDataEvent, value); }
            remove { RemoveHandler(NewSampleDataEvent, value); }
        }

        private class CtrlToFieldMap
        {
            public UIElement ctrlElement;
            public string displayName;

            public CtrlToFieldMap(UIElement ctrl, string display)
            {
                ctrlElement = ctrl;
                displayName = display;
            }
        }
        CtrlToFieldMap[] CtrlToFields = null;

        DrugInfo drugInfo = null;
        ModelInfo curAnalyteModel = null;

        /// <summary>
        /// 扫描样品列表
        /// </summary>
        private ObservableCollection<Ai.Hong.CommonLibrary.spectrumDisplayInfo> resultDatas = new ObservableCollection<Ai.Hong.CommonLibrary.spectrumDisplayInfo>();

        public NewSample()
        {
            InitializeComponent();

            //if(SettingData.settingData.runing_para.isSimulator)
            //    txtBarCode.Text = "81057270061546826041";

            CtrlToFields = new CtrlToFieldMap[] {
                new CtrlToFieldMap(txtSampleNumber, "样品编号"),
                new CtrlToFieldMap(listLicenseType, "批准类型"),
                new CtrlToFieldMap(txtLicenseCode, "批准文号"),
                new CtrlToFieldMap(listChemicalName, "药品名"),
                new CtrlToFieldMap(txtCommercialName, "商品名"),
                new CtrlToFieldMap(listForm, "剂型"),
                new CtrlToFieldMap(txtSpecification, "规格"),
                new CtrlToFieldMap(listProductUnit, "生产厂家"),
                new CtrlToFieldMap(txtProductTime, "生产日期"),
                new CtrlToFieldMap(txtBatchNumber, "生产批号"),
                new CtrlToFieldMap(txtValidMonth, "有效期"),
                new CtrlToFieldMap(txtIdentThresold, "阈值"),
                new CtrlToFieldMap(listScanType, "测样方式"),
                new CtrlToFieldMap(listIdentOperator, "检测人员"),
                new CtrlToFieldMap(txtMemo, "备注"),
                new CtrlToFieldMap(btnSeachInfo, "检索")
            };

            gridDrugInfo.DataContext = drugInfo;
            //InitComboBox();

            SetDrugInfoCtrlEnable(false);

            listSampleFiles.SetGraphicChart(graphicChart);
            listSampleFiles.SetGridData(resultDatas);
            SetResultListHeaders();
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = NewPanelExitEvent;
            args.Source = this;
            RaiseEvent(args);
        }

        //初始化所有列表
        private void InitComboBox()
        {
            listLicenseType.ItemsSource = SettingData.settingData.dictionary.licenseTypes;
            listForm.ItemsSource = SettingData.settingData.dictionary.forms;
            listScanType.ItemsSource = SettingData.settingData.dictionary.scanTypes;
            listIdentOperator.ItemsSource = SettingData.settingData.dictionary.operators;
        }

        //81061110048489991589
        private void btnSeachInfo_Click(object sender, RoutedEventArgs e)
        {
            if (txtLicenseCode.Text == null || txtLicenseCode.Text.Trim() == "")
            {
                CommonMethod.ErrorMsgBox("请输入药品注册码后再搜索");
                return;
            }

            DrugInfo newdrugInfo = null;
            newdrugInfo = SettingData.dataBase.GetDrugInfoFromLicense(txtLicenseCode.Text.Trim().ToUpper());
            if (newdrugInfo == null)
            {
                CommonMethod.ErrorMsgBox("找不到药品信息");
            }
            else
            {
                drugInfo.chemicalName = newdrugInfo.chemicalName;
                drugInfo.commercialName = newdrugInfo.commercialName;
                drugInfo.form = newdrugInfo.form;
                drugInfo.specification = newdrugInfo.specification;
                drugInfo.productUnit = newdrugInfo.productUnit;
            } 
        }

        /// <summary>
        /// 获取最大数量的字符串
        /// </summary>
        private string MaxString(string srcStr, int length)
        {
            if (srcStr.Length <= length)
                return srcStr;
            else
                return srcStr.Substring(0, length);
        }

        private void btnDetect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnDetect.Text == "新增样品")
                {
                    SetDrugInfoCtrlEnable(true);
                    btnDetect.Text = "开始检定";

                    /*
                    DrugInfo newdrug = new DrugInfo();
                    newdrug.licenseCode = "国药准字H20030863";
                    newdrug.identOperator = drugInfo.identOperator;
                    newdrug.scanType = drugInfo.scanType;
                    newdrug.licenseType = drugInfo.licenseType;
                    newdrug.identThresold = drugInfo.identThresold;
                    newdrug.validMonth = drugInfo.validMonth;
                    newdrug.sampleNumber = drugInfo.sampleNumber;
                    newdrug.batchNumber = drugInfo.batchNumber;
                    newdrug.productTime = drugInfo.productTime;

                    if (curAnalyteModel != null)
                        newdrug.identThresold = curAnalyteModel.thresold;
                    */
                    DrugInfo newdrug = null;
                    if (drugInfo != null)
                        newdrug = drugInfo.Clone();
                    else
                    {
                        newdrug = new DrugInfo();
                        newdrug.licenseCode = "国药准字";
                        newdrug.productTime = new DateTime(2008, 1, 1);
                        newdrug.identUnit = SettingData.settingData.runing_para.unitName;
                        newdrug.validMonth = 48;
                        newdrug.identThresold = 0.97;
                        if(SettingData.settingData.dictionary.scanTypes.Count > 0)
                           newdrug.scanType = SettingData.settingData.dictionary.scanTypes[0];
                        if (SettingData.settingData.dictionary.operators.Count > 0)
                            newdrug.scanType = SettingData.settingData.dictionary.operators[0];
                        if (SettingData.settingData.dictionary.licenseTypes.Count > 0)
                            newdrug.scanType = SettingData.settingData.dictionary.licenseTypes[0];
                        if (SettingData.settingData.dictionary.forms.Count > 0)
                            newdrug.scanType = SettingData.settingData.dictionary.forms[0];
                    }
                    drugInfo = newdrug;
                    gridDrugInfo.DataContext = drugInfo;
                }
                else
                {
                    //检查信息是否有填充
                    for (int i = 0; i < CtrlToFields.Length; i++)
                    {
                        if (CtrlToFields[i].displayName != "备注" && (CtrlToFields[i].ctrlElement is TextBox || CtrlToFields[i].ctrlElement is ComboBox))
                        {
                            bool isempty = false;
                            if (CtrlToFields[i].ctrlElement is TextBox)
                                isempty = CommonMethod.IsEmpty((CtrlToFields[i].ctrlElement as TextBox).Text);
                            else if (CtrlToFields[i].ctrlElement is ComboBox)
                                isempty = CommonMethod.IsEmpty((CtrlToFields[i].ctrlElement as ComboBox).Text);

                            if (isempty)
                            {
                                CtrlToFields[i].ctrlElement.Focus();
                                throw new Exception("请输入："+CtrlToFields[i].displayName);
                            }
                        }
                    }

                    if (drugInfo.productTime > DateTime.Now || (DateTime.Now - drugInfo.productTime).Days > 360 * 20)
                    {
                        txtProductTime.Focus();
                        throw new Exception("请输入正确的生产日期");
                    }
                    if (drugInfo.validMonth <= 0)
                    {
                        txtValidMonth.Focus();
                        throw new Exception("请输入正确的有效期");
                    }
                    if (drugInfo.identThresold <= 0)
                    {
                        txtIdentThresold.Focus();
                        throw new Exception("请输入正确的阈值");
                    }
                    if (checkNotAnalyze.IsChecked != true && curAnalyteModel == null)
                        throw new Exception("请选择分析模型");

                    string sampleFile = null;

                    //使用模拟方式，从文件中调入光谱
                    if (SettingData.settingData.runing_para.isSimulator)
                    {
                        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                        dlg.Filter = "光谱文件|*.spc";
                        dlg.Multiselect = false;
                        if (dlg.ShowDialog() == true)
                        {
                            sampleFile = dlg.FileName;
                        }
                    }
                    else
                    {
                        if(!System.IO.Directory.Exists(SettingData.settingData.runing_para.savePath))
                            throw new Exception("找不到文件保存路径:"+SettingData.settingData.runing_para.savePath+", 请在系统设置里面重新指定");

                        /*
                        //样品编号_通用名_商品名_剂型_规格_厂商_批号_生产日期_有效期_测量方式_仪器编号_检测单位_检测时间
                        sampleFile = MaxString(drugInfo.sampleNumber, 10) + "_" + drugInfo.chemicalName + "_" + drugInfo.commercialName + "_" +
                            drugInfo.form + "_" + drugInfo.specification + "_" + MaxString(drugInfo.productUnit, 10) + "_" +
                            MaxString(drugInfo.batchNumber, 10) + "_" + drugInfo.productTime.ToString("yyyyMMdd") + "_" + drugInfo.validMonth +
                            "_" + drugInfo.scanType + "_" + drugInfo.identInstrumentID + "_" + MaxString(drugInfo.identUnit, 10) +
                            "_" + drugInfo.identTime.ToString("yyyyMMddHHmmss") + ".SPC";
                        */
                        sampleFile = MaxString(drugInfo.sampleNumber, 10) + "_" + drugInfo.chemicalName + "_" + MaxString(drugInfo.productUnit, 10) + "_" +
                            drugInfo.licenseCode + "_" + drugInfo.identTime.ToString("yyyyMMddHHmmss");

                        sampleFile = Common.CommonMethod.GetValidFilename(sampleFile, "");
                        sampleFile = sampleFile.Replace('.', '-') + ".SPC";
                        string pathstr = GetFileSavePath();
                        if (pathstr == null)
                            throw new Exception("不能创建目录:" + pathstr);
                        sampleFile = System.IO.Path.Combine(pathstr, sampleFile);

                        //创建扫描任务并显示扫描等待窗口，开始扫描
                        curScanTaskInfo = new ScanTaskInfo(SettingData.settingData.runing_para.scanPara, false, sampleFile);

                        ProcessWaitDialog waitDlg = new ProcessWaitDialog(ScanSampleTask, "取消样品扫描");
                        waitDlg.Owner = App.Current.MainWindow;
                        if (waitDlg.ShowDialog() == false)
                            throw new Exception(curScanTaskInfo.ErrorString);
                    }

                    if (checkNotAnalyze.IsChecked != true)
                    {
                        //用相关系数法分析光谱
                        drugInfo.identValue = DrugAnalyte.CorCoeffAnalyte(curAnalyteModel, sampleFile);
                        //保留小数点后5位
                        drugInfo.identValue = Math.Round(drugInfo.identValue, 5);
                        if (drugInfo.identValue < 0)
                            throw new Exception(DrugAnalyte.ErrorString);

                        //填写检测信息
                        drugInfo.filename = sampleFile;
                        drugInfo.identTime = DateTime.Now;
                        drugInfo.identUnit = SettingData.settingData.runing_para.unitName;
                        drugInfo.identInstrumentID = SettingData.settingData.runing_para.serialNo;
                        //listSampleFiles.InitDisplayData(drugInfo);
                        drugInfo.isChecked = true;  // 显示光谱图像
                        drugInfo.identResult = drugInfo.identValue < curAnalyteModel.thresold ? EnumIdentResult.FAULT : EnumIdentResult.OK;
                        drugInfo.identModel = curAnalyteModel.licenseCode;
                        drugInfo.identMethod = EnumIdentifyMethod.Correlation;

                        //保存检测结果
                        Common.CommonMethod.WriteDrugInfo(drugInfo);
                    }
                    else
                    {
                        //填写检测信息
                        drugInfo.filename = sampleFile;
                        drugInfo.identTime = DateTime.Now;
                        drugInfo.identUnit = SettingData.settingData.runing_para.unitName;
                        drugInfo.identInstrumentID = SettingData.settingData.runing_para.serialNo;
                        //listSampleFiles.InitDisplayData(drugInfo);
                        drugInfo.isChecked = true;  // 显示光谱图像
                        drugInfo.identValue = 0;
                        drugInfo.identResult = EnumIdentResult.UNKNOWN;
                        drugInfo.identModel = null;
                        drugInfo.identMethod = EnumIdentifyMethod.Correlation;
                    }

                    //加入到结果列表
                    //读取光谱数据
                    if (drugInfo.fileData == null)
                    {
                        drugInfo.fileData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
                        drugInfo.fileData.ReadFile(drugInfo.filename);
                        drugInfo.color = Ai.Hong.CommonLibrary.spectrumDisplayInfo.GetDisplayColor(resultDatas.Count);
                    }

                    resultDatas.Add(drugInfo);
                    listSampleFiles.SelectItem(drugInfo);

                    //准备输入新的药品信息
                    SetDrugInfoCtrlEnable(false);
                    btnDetect.Text = "新增样品";
                }
            }
            catch (Exception ex)
            {
                CommonMethod.ErrorMsgBox(ex.Message);
            }

        }

        private void txtTextFiled_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            txt.SelectAll();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if ((bool)System.ComponentModel.DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue == false)    //设计模式不可用
            {
                InitComboBox();
                listMethods.SetEditable(false);
            }
        }

        /// <summary>
        /// 设置样品信息填写控件的状态
        /// </summary>
        /// <param name="enabled"></param>
        private void SetDrugInfoCtrlEnable(bool enabled)
        {
            foreach (CtrlToFieldMap item in CtrlToFields)
            {
                item.ctrlElement.IsEnabled = enabled;
            }
        }

        /// <summary>
        /// 模型列表选择消息
        /// </summary>
        private void listMethods_ModelSelected(object sender, RoutedEventArgs e)
        {
            Ai.Hong.CommonLibrary.SelectChangedArgs arg = e as Ai.Hong.CommonLibrary.SelectChangedArgs;
            if(arg == null || arg.item == null)
                return;

            curAnalyteModel = arg.item as ModelInfo;

            //如果正在新建样品，改变当前阈值，否则不改变
            if (txtIdentThresold.IsEnabled == true && btnDetect.Text == "开始检定")
                drugInfo.identThresold = curAnalyteModel.thresold;
        }

        /// <summary>
        /// 检测结果列表选择消息
        /// </summary>
        private void listSampleFiles_ItemSelected(object sender, RoutedEventArgs e)
        {
            Ai.Hong.CommonLibrary.SelectChangedArgs arg = e as Ai.Hong.CommonLibrary.SelectChangedArgs;
            if (arg == null)
                return;

            DrugInfo info = arg.item as DrugInfo;
            if (info != null)
            {
                SetDrugInfoCtrlEnable(false);
                btnDetect.Text = "新增样品";

                drugInfo = info;
                gridDrugInfo.DataContext = drugInfo;
            }
        }

        /// <summary>
        /// 设置检测结果列表显示项
        /// </summary>
        private void SetResultListHeaders()
        {
            //添加一些列
            string[] titles = new string[] { "阈值", "检测值", "模型名称" };
            string[] values = new string[] { "identThresold", "identValue", "identModel" };
            listSampleFiles.AddNewDataBinding(titles, values);

            //插入结果列
            DataGridTemplateColumn col = new DataGridTemplateColumn();
            col.Header = "结果";

            //加载在XAML中定义的显示模板，绑定在模板中
            DataTemplate temp = this.FindResource("imageCell") as DataTemplate;
            col.CellTemplate = temp;

            DataGrid grid = listSampleFiles.GetDataGrid();
            grid.Columns.Insert(1, col);
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
            if(curScanTaskInfo == null)
                return false;

            //创建扫描线程
            System.Threading.Thread scanThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(curScanTaskInfo.DoScan));
            scanThread.IsBackground = true;
            scanThread.Start(curScanTaskInfo.scanPara.scanCount);
            int curTime = Environment.TickCount;
            string msgStr = "正在扫描" + (curScanTaskInfo.isBackground ? "背景" : "样品") + "......";

            //等待扫描完成
            while (scanThread.IsAlive)
            {
                //假定一次扫描需要2秒
                if (Environment.TickCount - curTime > curScanTaskInfo.scanPara.scanCount * 4000)
                    return false;

                System.Threading.Thread.Sleep(1000);

                bool abortTask = false;
                callBack(msgStr, (int)curScanTaskInfo.scanPara.scanCount * 4000, Environment.TickCount - curTime, out abortTask);

                if (abortTask)
                {
                    curScanTaskInfo.scanSuccessed = false;
                    break;
                }
            }

            return curScanTaskInfo.scanSuccessed;
        }

        /// <summary>
        /// 扫描背景
        /// </summary>
        private void btnBackground_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Common.CommonMethod.QuestionMsgBox("请确认背景测量标样放置到测量台上") == false)
                    return;

                if(!Directory.Exists(SettingData.settingData.runing_para.savePath))
                    throw new Exception("找不到文件保存路径:"+SettingData.settingData.runing_para.savePath+", 请在系统设置里面重新指定");

                string backFile = Path.Combine(SettingData.settingData.runing_para.savePath, "Background", DateTime.Now.ToString("yyyy-MM-dd"));
                if(!Directory.Exists(backFile))
                    Directory.CreateDirectory(backFile);

                backFile = Path.Combine(backFile, "Background" + DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss") + ".spc");
            
                //创建扫描任务并显示扫描等待窗口，开始扫描
                curScanTaskInfo = new ScanTaskInfo(SettingData.settingData.runing_para.scanPara, true, backFile);

                MainWindow mainWnd = App.Current.MainWindow as MainWindow;
                ProcessWaitDialog waitDlg = new ProcessWaitDialog(ScanSampleTask, "取消背景扫描");
                waitDlg.Owner = App.Current.MainWindow;
                if (waitDlg.ShowDialog() == false)
                {
                    mainWnd.BackgroundScaned = false;
                    Common.CommonMethod.ErrorMsgBox(curScanTaskInfo.ErrorString);
                }
                else
                    mainWnd.BackgroundScaned = true;
            }
            catch(Exception ex)
            {
                Common.CommonMethod.ErrorMsgBox(ex.Message);
            }
        }

        /// <summary>
        /// 获取扫描光谱保存路径
        /// </summary>
        /// <returns></returns>
        private string GetFileSavePath()
        {
            try
            {
                string curPath = null;
                switch (SettingData.settingData.runing_para.savePathType)
                {
                    case EnumSavePathType.ScanDate:
                        curPath = DateTime.Now.ToString(SettingData.ShortDateTimeString);
                        break;
                    case EnumSavePathType.DrugName:
                        curPath = drugInfo.chemicalName;
                        break;
                    case EnumSavePathType.ProductUnit:
                        curPath = drugInfo.productUnit;
                        break;
                    case EnumSavePathType.LicenseCode:
                        curPath = drugInfo.licenseCode;
                        break;
                    default:
                        curPath = "未知类型";
                        break;
                }

                curPath = Common.CommonMethod.GetValidFilename(curPath, "");
                curPath = System.IO.Path.Combine(SettingData.settingData.runing_para.savePath, curPath);
                if (!System.IO.Directory.Exists(curPath))
                    System.IO.Directory.CreateDirectory(curPath);

                return curPath;
            }
            catch (Exception ex)
            {
                Common.CommonMethod.ErrorMsgBox(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 移除样品光谱列表中的选择文件
        /// </summary>
        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            listSampleFiles.RemoveSelected();
        }
    }
}
