using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using NirIdentifier.Common;
using System.Collections.ObjectModel;
using System.IO;
using Ai.Hong.CommonLibrary;

namespace NirIdentifier.Offline
{
    /// <summary>
    /// SampleResult.xaml 的交互逻辑
    /// </summary>
    public partial class SampleResult : UserControl
    {
        /// <summary>
        /// 文件列表
        /// </summary>
        private ObservableCollection<Ai.Hong.CommonLibrary.spectrumDisplayInfo> dataList = new ObservableCollection<Ai.Hong.CommonLibrary.spectrumDisplayInfo>();

        public SampleResult()
        {
            InitializeComponent();
            SetButtonState(false);
            listFiles.SetGraphicChart(graphicChart);
            listFiles.SetGridData(dataList);
            SetResultListHeaders();

            btnPrint.IsEnabled = false;
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog<FileFormat.FileOpenDlg>();
            dlg.Filter = "样品光谱|*.spc";
            dlg.Multiselect = true;
            dlg.Title = "Open File";
            dlg.FileDlgStartLocation = AddonWindowLocation.Right;
            dlg.FileDlgDefaultViewMode = NativeMethods.FolderViewMode.Tiles;
            dlg.FileDlgOkCaption = "&Open";
            dlg.FileDlgEnableOkBtn = true;
            dlg.SetPlaces(new object[] {(int)Places.History, (int)Places.MyComputer, (int)Places.Desktop, 
                (int)Places.MyDocuments, (int)Places.Favorites});

            if ((bool)dlg.ShowDialog() == true)
            {
                foreach (string file in dlg.FileNames)
                {
                    DrugInfo newDrug = Common.CommonMethod.ReadDrugInfo(file);
                    if (newDrug == null)
                    {
                        newDrug = new DrugInfo();
                        newDrug.filename = file;
                        newDrug.fileData = new SpecFileFormatDouble();
                        newDrug.fileData.ReadFile(newDrug.filename);
                        newDrug.identResult = EnumIdentResult.UNKNOWN;
                    }

                    dataList.Add(newDrug);
                }
            }
        }

        //设置按钮状态
        private void SetButtonState(bool enabled)
        {
            btnUnload.IsEnabled = enabled;
            btnPrint.IsEnabled = enabled;
            btnReIdentify.IsEnabled = enabled;
        }

        private void btnUnload_Click(object sender, RoutedEventArgs e)
        {
            listFiles.RemoveSelected();
        }

        private string ErrorString;
        /// <summary>
        ///  光谱分析回调函数，在ProcessWaitDialog中调用
        /// </summary>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public bool ProcessTask(ProcessWaitDialog.SetProcessAndMsgDeletage callBack)
        {
            foreach (spectrumDisplayInfo item in reIdentifyItems)
            {
                DrugInfo drug = item as DrugInfo;

                double cc = Common.DrugAnalyte.CorCoeffAnalyte(curModel, drug.filename);
                if (cc > 0)
                {
                    drug.identModel = curModel.licenseCode;
                    drug.identValue = cc;
                    drug.identResult = cc >= curModel.thresold ? EnumIdentResult.OK : EnumIdentResult.FAULT;
                    drug.identTime = DateTime.Now;

                    Common.CommonMethod.WriteDrugInfo(drug);
                }
                else
                {
                    ErrorString = Common.DrugAnalyte.ErrorString;
                    return false;
                }

                bool abortTask = false;
                callBack("正在分析:" + drug.filename, reIdentifyItems.Count, reIdentifyItems.IndexOf(item), out abortTask);
            }
            return true;
        }

        /// <summary>
        /// 需要重新分析的光谱列表
        /// </summary>
        List<spectrumDisplayInfo> reIdentifyItems = null;

        /// <summary>
        /// 重新分析光谱所使用的模型
        /// </summary>
        ModelInfo curModel = null;

        /// <summary>
        /// 重新分析光谱
        /// </summary>
        private void btnReIdentify_Click(object sender, RoutedEventArgs e)
        {
            ModelSelector modelDlg = new ModelSelector();
            modelDlg.Owner = App.Current.MainWindow;
            if (modelDlg.ShowDialog() == true)
            {
                //获得当前选择的模型
                curModel = modelDlg.GetSelectedModel();

                //获取需要重新分析的光谱
                reIdentifyItems = listFiles.GetSelectedItems();
                if (reIdentifyItems.Count > 0)
                {
                    //调用等待窗口
                    Common.ProcessWaitDialog waitdlg = new ProcessWaitDialog(ProcessTask,null);
                    waitdlg.Owner = App.Current.MainWindow;
                    if (waitdlg.ShowDialog() == false)
                        Common.CommonMethod.ErrorMsgBox(ErrorString);
                }
            }
        }

        /// <summary>
        /// 打印报告
        /// </summary>
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            List<spectrumDisplayInfo> selItems = listFiles.GetSelectedItems();
            foreach (spectrumDisplayInfo item in dataList)
            {
                DrugInfo drug = item as DrugInfo;

                drug.tempuse = selItems.IndexOf(item) >=0 ? 1 : 0;
            }

            List<DrugInfo> drugList = new List<DrugInfo>();
            foreach (spectrumDisplayInfo item in dataList)
                drugList.Add(item as DrugInfo);

            NirIdentifier.Detect.ReportPrint dlg = new Detect.ReportPrint(drugList);
            dlg.ShowDialog();
        }

        /// <summary>
        /// 列表选择信息
        /// </summary>
        private void listFiles_ItemSelected(object sender, RoutedEventArgs e)
        {
            Ai.Hong.CommonLibrary.SelectChangedArgs arg = e as Ai.Hong.CommonLibrary.SelectChangedArgs;

            if (arg == null)
            {
                gridDrugInfo.DataContext = null;
                SetButtonState(false);
            }
            else
            {
                DrugInfo drug = arg.item as DrugInfo;
                gridDrugInfo.DataContext = drug;

                SetButtonState(drug != null);
            }
        }

        /// <summary>
        /// 设置列表显示数据
        /// </summary>
        private void SetResultListHeaders()
        {
            //添加一些列
            string[] titles = new string[] { "药品名", "生产厂家","规格","批准文号", "模型名称", "阈值", "检测值", "检测时间", "检测员", "检测单位", "备注" };
            string[] values = new string[] { "chemicalName", "productUnit", "specification", "licenseCode", "identModel", 
                        "identThresold", "identValue",  "identTime",  "identOperator",  "identUnit", "memo"};
            listFiles.AddNewDataBinding(titles, values);

            //插入结果列
            DataGridTemplateColumn col = new DataGridTemplateColumn();
            col.Header = "结果";

            //加载在XAML中定义的显示模板，绑定在模板中
            DataTemplate temp = this.FindResource("imageCell") as DataTemplate;
            col.CellTemplate = temp;

            DataGrid grid = listFiles.GetDataGrid();
            grid.Columns.Insert(1, col);
        }
    }
}
