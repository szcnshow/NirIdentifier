using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using NirIdentifier.Common;
using System.Collections.ObjectModel;

namespace NirIdentifier.SystemSetup
{
    /// <summary>
    /// ProgramInfoPanel.xaml 的交互逻辑
    /// </summary>
    public partial class ProgramInfoPanel : UserControl
    {
        string[] dictKey = new string[] { "测样方式", "剂型", "注册码类型", "操作人员" };
        public ProgramInfoPanel()
        {
            InitializeComponent();

            if (SettingData.settingData != null && SettingData.settingData.runing_para!=null)
            {
                stackInstrumentInfo.DataContext = SettingData.settingData.runing_para;
                gridSaveInfo.DataContext = SettingData.settingData.runing_para;
                listSaveType.SelectedIndex = (int)SettingData.settingData.runing_para.savePathType;     //特别处理一下，没有自动刷新
                gridScanParameterInfo.DataContext = SettingData.settingData.runing_para.scanPara;
                InitDictList();
            }
        }

        //初始化数据字典
        private void InitDictList()
        {
            listDictKeyword.ItemsSource = dictKey;
            listDictKeyword.SelectedIndex = 0;
        }

        private void btnSavePath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();

            dlg.SelectedPath = Common.SettingData.settingData.runing_para.savePath;

            System.Windows.Forms.DialogResult ret = dlg.ShowDialog();
            if (ret == System.Windows.Forms.DialogResult.OK)
            {
                txtSavepath.Text = dlg.SelectedPath;
                Common.SettingData.settingData.runing_para.savePath = dlg.SelectedPath;
            }
        }

        //保存设置
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if ((bool)System.ComponentModel.DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue == false)    //设计模式不可用
            {
                SettingData.settingData.Serialize(SettingData.settingData.filename);
            }
        }

        private void listDictKeyword_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnDictAdd.IsEnabled = listDictKeyword.SelectedIndex != -1;
            if (listDictKeyword.SelectedIndex == -1)
            {
                listDictContent.SelectedIndex = -1;
                return;
            }

            switch (dictKey[listDictKeyword.SelectedIndex])
            {
                case "测样方式":
                    listDictContent.ItemsSource = SettingData.settingData.dictionary.scanTypes;
                    break;
                case "剂型":
                    listDictContent.ItemsSource = SettingData.settingData.dictionary.forms;
                    break;
                case "注册码类型":
                    listDictContent.ItemsSource = SettingData.settingData.dictionary.licenseTypes;
                    break;
                case "操作人员":
                    listDictContent.ItemsSource = SettingData.settingData.dictionary.operators;
                    break;
            }
            listDictContent.SelectedIndex = -1;
            btnDictEdit.IsEnabled = false;
            btnDictDelete.IsEnabled = false;
        }

        private void listDictContent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnDictEdit.IsEnabled = listDictContent.SelectedIndex != -1;
            btnDictDelete.IsEnabled = listDictContent.SelectedIndex != -1;
        }

        private void btnDictAdd_Click(object sender, RoutedEventArgs e)
        {
            Common.InputText dlg = new InputText("请输入内容", null);
            dlg.Owner = App.Current.MainWindow;
            if (dlg.ShowDialog() == true)
            {
                List<string> selList = listDictContent.ItemsSource as List<string>;
                selList.Add(dlg.InputedText);
                listDictContent.Items.Refresh();
            }
        }

        private void btnDictEdit_Click(object sender, RoutedEventArgs e)
        {
            if (listDictContent.SelectedIndex >= 0)
            {
                Common.InputText dlg = new InputText("请输入内容", listDictContent.SelectedItem as string);
                dlg.Owner = App.Current.MainWindow;
                if (dlg.ShowDialog() == true)
                {
                    List<string> selList = listDictContent.ItemsSource as List<string>;
                    selList[listDictContent.SelectedIndex] = dlg.InputedText;
                    listDictContent.Items.Refresh();
                }
            }
        }

        private void btnDictDelete_Click(object sender, RoutedEventArgs e)
        {
            if (listDictContent.SelectedIndex >= 0)
            {
                List<string> selList = listDictContent.ItemsSource as List<string>;
                selList.RemoveAt(listDictContent.SelectedIndex);
                listDictContent.Items.Refresh();
            }
        }

        /// <summary>
        /// 设置仪器扫描配置文件
        /// </summary>
        private void btnSettingFilePath_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Title = "打开配置文件";
            dlg.Filter = "*.vspec_nir_ini|*.vspec_nir_ini";
            if (SettingData.settingData.runing_para.scanPara.scanSettingFile != null)
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(SettingData.settingData.runing_para.scanPara.scanSettingFile);
            if (dlg.ShowDialog() == true)
            {
                txtSettingFile.Text = dlg.FileName;
                SettingData.settingData.runing_para.scanPara.scanSettingFile = dlg.FileName;
            }
        }
    }
}
