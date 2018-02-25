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
using NirIdentifier.Common;

namespace NirIdentifier.SystemSetup
{
    /// <summary>
    /// CorrectParameterPanel.xaml 的交互逻辑
    /// </summary>
    public partial class CorrectParameterPanel : UserControl
    {
        SettingFile.Y_Correction_Parameter yPara = null;
        List<TextBlock> paraNames = new List<TextBlock>();
        List<TextBox> paraValues = new List<TextBox>();

        public CorrectParameterPanel()
        {
            InitializeComponent();

            for (int i = 3; i < 7; i++)
                listOrders.Items.Add(i.ToString());

            paraNames.Add(namePara0);
            paraNames.Add(namePara1);
            paraNames.Add(namePara2);
            paraNames.Add(namePara3);
            paraNames.Add(namePara4);
            paraNames.Add(namePara5);
            paraNames.Add(namePara6);

            paraValues.Add(txtPara0);
            paraValues.Add(txtPara1);
            paraValues.Add(txtPara2);
            paraValues.Add(txtPara3);
            paraValues.Add(txtPara4);
            paraValues.Add(txtPara5);
            paraValues.Add(txtPara6);

        }

        /// <summary>
        /// 显示/隐藏参数控件
        /// </summary>
        /// <param name="order">拟合的阶数</param>
        private void VisibleParameterField(int order)
        {
            for (int i = 0; i < paraNames.Count; i++)
            {
                if (i <= order)
                {
                    paraNames[i].Visibility = Visibility.Visible;
                    paraValues[i].Visibility = Visibility.Visible;
                }
                else
                {
                    paraNames[i].Visibility = Visibility.Collapsed;
                    paraValues[i].Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// 填充当前系数值
        /// </summary>
        /// <param name="values">系数</param>
        private void FillParameterField(int order, double[] values)
        {
            listOrders.Text = order.ToString();
            for (int i = 0; i < values.Length; i++)
            {
                if(i < paraValues.Count)
                    paraValues[i].Text = values[i].ToString();
            }
        }

        private void radioStandard_Checked(object sender, RoutedEventArgs e)
        {
            //VisibleParameterField(yPara.standard_order);
            //FillParameterField(yPara.standard_order, yPara.coefficient);
        }

        private void radioExternal_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void listOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VisibleParameterField(int.Parse(listOrders.Items[listOrders.SelectedIndex].ToString()));
        }
        
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //使用标准的校正文件
                if (radioStandard.IsChecked == true)
                {
                    yPara.isStandard = true;
                    SettingData.settingData.Serialize(SettingData.IniFileName);
                    CommonMethod.InfoMsgBox("参数保存完毕");
                    return;
                }
                
                //使用用户指定的校正文件
                if (CommonMethod.IsEmpty(txtFilename.Text))
                    throw new Exception("请指定Y轴校正光谱");

                yPara.user_order = int.Parse(listOrders.Text);
                yPara.user_coefficient = new double[yPara.user_order+1];    //+1是因为还有C0系数
                for (int i = 0; i <= yPara.user_order; i++)
                {
                    double coe = 0;
                    if (double.TryParse(paraValues[i].Text, out coe) == false)
                        throw new Exception("参数" + i.ToString() + " :设置错误");
                    yPara.user_coefficient[i] = coe;
                }
                
                Ai.Hong.CommonLibrary.SpecFileFormat data = new Ai.Hong.CommonLibrary.SpecFileFormat();
                if (data.ReadFile(txtFilename.Text) == false)
                    throw new Exception(txtFilename.Text + " :读取文件错误");
                if (data.Parameter.dataCount >= 2000 || data.XDatas[0] != 0)
                    throw new Exception(txtFilename.Text + " :不是Y轴校正的像素光谱");

                for (int i = 0; i < data.Parameter.dataCount; i++)
                {
                    double coe = yPara.user_coefficient[0];
                    for (int j = 1; j <= yPara.user_order; j++)
                        coe += yPara.user_coefficient[j] * Math.Pow(data.XDatas[i], j);
                    data.YDatas[i] = (float)(data.YDatas[i]* coe);
                }

                //将结果保存到共享文档下，Win7中是：C:\Users\Public\Documents\EFTIR\metage_raman\user_nistR_RFDI.spc
                string nistfile = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
                nistfile = System.IO.Path.Combine(nistfile, "eftir", "metage_raman", yPara.user_nistR_file);
                if (Ai.Hong.CommonLibrary.SPCFile.SaveFile(nistfile, data.YDatas, data.Parameter) == false)
                    throw new Exception("保存文件出现错误：" + nistfile);

                yPara.isStandard = false;   //用户自定义的校正文件
                //SettingData.settingData.y_Correction = yPara;
                SettingData.settingData.Serialize(SettingData.IniFileName);

                CommonMethod.InfoMsgBox("参数保存完毕");
            }
            catch (Exception ex)
            {
                CommonMethod.ErrorMsgBox(ex.Message);
            }
    
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "*.spc|*.spc";
            dlg.Title = "Y轴校正文件";
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == true)
            {
                txtFilename.Text = dlg.FileName;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
