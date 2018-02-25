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
using System.Windows.Shapes;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;
using System.Windows.Xps.Serialization;
using System.IO.Packaging;
using System.IO;
using NirIdentifier.Common;
using Microsoft.Office.Interop;
using System.Runtime.InteropServices;
using System.Windows.Markup;
using System.ComponentModel;

namespace NirIdentifier.Detect
{
    /// <summary>
    /// ReportPrint.xaml 的交互逻辑
    /// </summary>
    public partial class ReportPrint : Window
    {
        //fromGasValue True:气体定量分析，需要显示趋势图, False:定性检测，不显示趋势图
        public List<DrugInfo> printData = null;     //样品打印数据
        public string reportTitle = null;                   //输出到Excel文件的标题
        public bool IsPpm = true;           //针对气体定量：PPM单位还是mg/m3
        public bool IsDry = false;          //针对气体定量：干基还是湿基

        public ReportPrint(List<DrugInfo> inputData)
        {
            printData = inputData;
            InitializeComponent();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (reportTitle == null)
                reportTitle = "样品检测结果";

            if (radioToPrinter.IsChecked == true)   //打印
            {
                PrintToPrinter();
            }
            else if(radioToXPS.IsChecked == true)   //存为XPS
            {
                SaveToXpsFile();
            }
            else    //输出到Excel
            {
                SaveToExcel();
            }
            this.Close();
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);  

        const int WM_QUIT = 0x0012;

        //保存检测结果到Excel文件，第一行：文件标题，第二行：内容表头，以下为内容
        //定性和定量的表格格式不相同
        private bool SaveToExcel()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "Excel文档(*.xls)|*.xls";
            if (dlg.ShowDialog() != true)
                return false;

            Microsoft.Office.Interop.Excel.Application _app = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook _workBook = _app.Workbooks.Add(Microsoft.Office.Interop.Excel.XlSheetType.xlWorksheet);
            Microsoft.Office.Interop.Excel.Worksheet _workSheet = (Microsoft.Office.Interop.Excel.Worksheet)_workBook.ActiveSheet;

            string[] titles = new string[] { "样品编号", "检测时间", "检测人员", "检测模型", "判定阈值", "检测值", "检测结果", "光谱文件", 
                "批准文号", "药品名称", "商品名称", "生产厂家", "剂型", "规格", "生产批号", "生产日期", "有效期(月)", "备注"};

            //取得需要输出的数据
            List<DrugInfo> curPrintData = new List<DrugInfo>();
            curPrintData.AddRange(printData);
            curPrintData.RemoveAll(data => radioSelected.IsChecked == true);

                //获取Excel多个单元格区域，设置表格名称
            int colindex = 1;
            //设置表头
            for(int i=0; i<titles.Length; i++)
                _workSheet.Cells[colindex, i + 1] = titles[i];      //Excel列从1开始，因此要i+1

            //填充数据
            foreach (DrugInfo data in curPrintData)
            {
                colindex++;
                //填写检测信息
                int rowindex=1;
                _workSheet.Cells[colindex, rowindex++] = data.sampleNumber;       //样品编号
                _workSheet.Cells[colindex, rowindex++] = data.identTime.ToString(SettingData.LongDateTimeString);     //检测时间
                _workSheet.Cells[colindex, rowindex++] = data.identOperator;        //检测人员
                _workSheet.Cells[colindex, rowindex++] = data.identModel;           //检测模型
                _workSheet.Cells[colindex, rowindex++] = data.identThresold;        //判定阈值
                _workSheet.Cells[colindex, rowindex++] = data.identValue;           //检测值
                _workSheet.Cells[colindex, rowindex++] = data.identResult;          //检测结果
                _workSheet.Cells[colindex, rowindex++] = data.filename;             //光谱文件

                //填写药品信息
                _workSheet.Cells[colindex, rowindex++] = data.licenseCode;          //批准文号
                _workSheet.Cells[colindex, rowindex++] = data.chemicalName;         //药品名称
                _workSheet.Cells[colindex, rowindex++] = data.commercialName;       //商品名称
                _workSheet.Cells[colindex, rowindex++] = data.productUnit;          //生产厂家
                _workSheet.Cells[colindex, rowindex++] = data.form;                 //剂型
                _workSheet.Cells[colindex, rowindex++] = data.specification;        //规格
                _workSheet.Cells[colindex, rowindex++] = data.batchNumber;          //生产批号
                _workSheet.Cells[colindex, rowindex++] = data.productTime.ToString(SettingData.ShortDateTimeString);                         //生产日期
                _workSheet.Cells[colindex, rowindex++] = data.validMonth;           //有效期
                _workSheet.Cells[colindex, rowindex++] = data.memo;                 //备注
            }


            _workSheet.SaveAs(dlg.FileName);
            _workBook.Close();
            _app.Quit();

            //关闭Excel
            try
            {
                if (_app != null)
                {
                    int lpdwProcessId;
                    GetWindowThreadProcessId(new IntPtr(_app.Hwnd), out lpdwProcessId);
                    System.Diagnostics.Process.GetProcessById(lpdwProcessId).Kill();
                }

                System.Diagnostics.Process.Start(dlg.FileName);    //打开Excel文件
            }
            catch (Exception ex)
            {
                Console.WriteLine("Delete Excel Process Error:" + ex.Message);
            }
            return true;
        }

        //通过写入和读出Xaml方式克隆一个Object
        public T CloneObject<T>(T obj)
        {
            string gridXaml = XamlWriter.Save(obj);
            MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(gridXaml));
            Object clone = XamlReader.Load(stream);
            return (T)clone;
        }

        private void FillTextData(Border rootBorder, string IdName, string content)
        {
            object el = rootBorder.FindName(IdName);
            if (el == null)
                return;

            if(el is TextBlock)
            {
                TextBlock txt = rootBorder.FindName(IdName) as TextBlock;
                if (txt != null)
                    txt.Text = content;
            }
        }

        //填写样品固定的信息, headBorder:报表的外框
        private void FillDocumentFixedInfo(DrugInfo data, Border headBorder)
        {
            //填写样品检测信息
            Run typerun = headBorder.FindName("IDUnit") as Run;     //单位
            typerun.Text = SettingData.settingData.runing_para.unitName;

            FillTextData(headBorder, "txtsampleNumber", data.sampleNumber);     //检测时间   
            FillTextData(headBorder, "txtidentTime", data.identTime.ToString(SettingData.LongDateTimeString));                               //检测地点
            FillTextData(headBorder, "txtidentModel", data.identModel);                 //取样日期
            FillTextData(headBorder, "txtscanType", data.scanType);              //取样地点
            FillTextData(headBorder, "txtidentThresold", data.identThresold.ToString());                                //数据文件名
            FillTextData(headBorder, "txtidentValue", data.identValue.ToString());               //操作员
            FillTextData(headBorder, "txtfilename", data.filename);               //操作员

            //填写样品药品信息
            FillTextData(headBorder, "txtlicenseCode", data.licenseCode);                                      //备注
            FillTextData(headBorder, "txtproductUnit", data.productUnit);             //名称
            FillTextData(headBorder, "txtchemicalName", data.chemicalName);               //厂家
            FillTextData(headBorder, "txtcommercialName", data.commercialName);               //厂家
            FillTextData(headBorder, "txtform", data.form);               //厂家
            FillTextData(headBorder, "txtspecification", data.specification);               //厂家
            FillTextData(headBorder, "txtbatchNumber", data.batchNumber);               //厂家
            FillTextData(headBorder, "txtproductTime", data.productTime.ToString(SettingData.ShortDateTimeString));               //厂家
            FillTextData(headBorder, "txtvalidMonth", data.validMonth+"月");               //厂家
            //还需要输入剂型

            FillTextData(headBorder, "txtmemo", data.memo);                             //剂型
            FillTextData(headBorder, "txtidentOperator",data.identOperator);               //文号
        }

        //填写检出组分信息
        private void FillComponentsData(Border rootBorder, string componentsBorderName, DrugInfo data)
        {
        }

        private void FillFinalResult(Border rootBorder, DrugInfo data)
        {
            bool isOk = data.identResult == EnumIdentResult.OK;
            FillTextData(rootBorder, "txtAllResult", isOk? "检测通过":"检测未通过");     //检测结果   

            Image imgresult = rootBorder.FindName("imgAllResult") as Image;
            if (imgresult != null)
            {
                string imgfile=isOk ? "IdentYes_128.png":"IdentNo_128.png";
                Common.CommonMethod.SetImageSource(imgresult, imgfile);
            }
        }

        //创建定性检测打印文档
        private FixedDocument CreateIdentifyDocument(List<DrugInfo> curPrintData)
        {
            FixedDocument fixedDoc = new FixedDocument();

            Stream xamlStream = null;

            try
            {
                xamlStream = Common.CommonMethod.StreamFromResource("IdentifyReport");
                if (xamlStream == null)
                    return null;

                FlowDocument flowDoc = XamlReader.Load(xamlStream) as FlowDocument;

                BlockUIContainer block = flowDoc.FindName("rootBlock") as BlockUIContainer;     //根内容
                Border rootBorder = block.Child as Border;
                block.Child = null;

                foreach (DrugInfo data in curPrintData) 
                {
                    PageContent pageContent = new PageContent();
                    FixedPage page = new FixedPage();
                    page.Width = 21 * 96 / 2.54;        //A4 Paper: 21cm x 29.7cm
                    page.Height = 29.7 * 96 / 2.54;

                    Border tempborder = CloneObject(rootBorder);
                    tempborder.DataContext = data;

                    FillDocumentFixedInfo(data, tempborder);    //填写样品固定的信息

                    //填写光谱图形
                    ReportTemplate.ShowSpectrumGraphic(tempborder, "spectrumBorder", data.filename, 18, double.MaxValue, 200);

                    //检出组分信息
                    FillComponentsData(tempborder, "borderComponents", data);

                    //最后检测结果
                    FillFinalResult(tempborder, data);

                    page.Children.Add(tempborder);
                    ((IAddChild)pageContent).AddChild(page);
                    fixedDoc.Pages.Add(pageContent);
                    
                }
            }
            catch (System.Exception ex)
            {
                CommonMethod.ErrorMsgBox(ex.Message);
                fixedDoc = null;
            }
            finally
            {
                if (xamlStream != null)
                    xamlStream.Close();
            }

            return fixedDoc;
        }


        //创建一个带边框，填充文字的Border，并加入到Grid中
        private void NewBorderToGrid(string textstr, Grid parentGrid, int row, int column, string borderLine)
        {
            if (parentGrid == null)
                return;

            TextBlock txt = new TextBlock();
            txt.Text = textstr;
            txt.TextAlignment = TextAlignment.Center;
            txt.VerticalAlignment = VerticalAlignment.Center;

            Border border = new Border();
            border.Child = txt;
            border.BorderBrush = Brushes.Black;

            switch (borderLine)
            {
                case "RB":  //右下有线
                    border.BorderThickness = new Thickness(0, 0, 1, 1);
                    break;
                case "R":   //右边有线
                    border.BorderThickness = new Thickness(0, 0, 1, 0);
                    break;
                case "B":   //下边有线
                    border.BorderThickness = new Thickness(0, 0, 0, 1);
                    break;
            }
            Grid.SetColumn(border, column);
            Grid.SetRow(border, row);

            parentGrid.Children.Add(border);
        }

        private FixedDocument CreateValueDocument()
        {
            return null;
        }

        //打印到打印机
        private void PrintToPrinter()
        {
            //创建XPS文件内容
            FixedDocument fixedDoc = null;
            List<DrugInfo> curPrintData = new List<DrugInfo>();
            curPrintData.AddRange(printData);
            curPrintData.RemoveAll(data => radioSelected.IsChecked == true && data.tempuse == 0);
            fixedDoc = CreateIdentifyDocument(curPrintData);

            if (fixedDoc == null)
                return;

            PrintPreView printdlg = new PrintPreView(fixedDoc);
            printdlg.ShowDialog();
        }

        //保存到XPS格式文档
        private void SaveToXpsFile()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "XPS文档(*.xps)|*.xps";
            if (dlg.ShowDialog() != true)
                return;
            try
            {
                //创建XPS文件内容
                FixedDocument fixedDoc = null;
                List<DrugInfo> curPrintData = new List<DrugInfo>();
                curPrintData.AddRange(printData);
                curPrintData.RemoveAll(data => radioSelected.IsChecked == true && data.tempuse == 0);
                fixedDoc = CreateIdentifyDocument(curPrintData);
                if (fixedDoc == null)
                    return;

                //保存到XPS文件
                DocumentPaginator paginator = fixedDoc.DocumentPaginator;
                XpsDocument xpsDocument = new XpsDocument(dlg.FileName, FileAccess.Write);
                System.Windows.Xps.XpsDocumentWriter documentWriter = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                documentWriter.Write(paginator);
                xpsDocument.Close();

                System.Diagnostics.Process.Start(dlg.FileName);
            }
            catch (System.Exception ex)
            {
                CommonMethod.ErrorMsgBox(ex.Message);
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CommonMethod.HideWindowSystemButton(this);
            radioSelected_Checked(null, null);
        }

        //输出选择数据, 设置按钮状态
        private void radioSelected_Checked(object sender, RoutedEventArgs e)
        {
            if (btnPrint == null)
                return;

            btnPrint.IsEnabled = false;

            if (printData == null && printData.Count == 0)
                return;
            
            //再查看是否有选择项
            foreach (DrugInfo data in printData)
            {
                if(data.tempuse == 1)
                {
                    btnPrint.IsEnabled = true;
                    break;
                }
            }
        }

        //输出全部数据, 设置按钮状态
        private void radioAll_Checked(object sender, RoutedEventArgs e)
        {
            if (btnPrint == null)
                return;
            btnPrint.IsEnabled = (printData != null && printData.Count != 0);
        }

    }
}
