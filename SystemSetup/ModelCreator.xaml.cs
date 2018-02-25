using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NirIdentifier.Common;
using System.Collections.ObjectModel;

namespace NirIdentifier.SystemSetup
{
    /// <summary>
    /// ModelCreator.xaml 的交互逻辑
    /// </summary>
    public partial class ModelCreator : UserControl
    {
        ModelInfo curModel = null;

        ObservableCollection<Ai.Hong.CommonLibrary.spectrumDisplayInfo> allFiles = new ObservableCollection<Ai.Hong.CommonLibrary.spectrumDisplayInfo>();
        public ModelCreator()
        {
            InitializeComponent();
            listModels.SetEditable(true);

            listFiles.SetGraphicChart(graphicChart);
            listFiles.SetGridData(allFiles);

            listModels_ModelSelected(null, null);
        }

        private void btnModelNew_Click(object sender, RoutedEventArgs e)
        {
            Common.InputText dlg = new InputText("请输入药品注册码", "国药准字");
            dlg.Owner = App.Current.MainWindow;
            if (dlg.ShowDialog() == true)
            {
                DrugInfo info = SettingData.dataBase.GetDrugInfoFromLicense(dlg.InputedText);
                if (info == null)
                {
                    CommonMethod.ErrorMsgBox("您输入的药品注册码：" + dlg.InputedText + " 不存在");
                    return;
                }

                ModelInfo newmodel = new ModelInfo(info);
                SettingData.settingData.models.Add(newmodel);
            }            
        }

        private void listModels_ModelSelected(object sender, RoutedEventArgs e)
        {
            Ai.Hong.CommonLibrary.SelectChangedArgs args;
            if (e == null)
                curModel = null;
            else
            {
                args = e as Ai.Hong.CommonLibrary.SelectChangedArgs;
                curModel = args.item as ModelInfo;
            }

            if(curModel == null)
            {
                listFiles.SetGridData(allFiles);
                groupFiles.IsEnabled = false;

                listRegions.ItemsSource = null;
                groupRegions.IsEnabled = false;
            }
            else
            {
                //listFiles.SetGridData(curModel.files);
                groupFiles.IsEnabled = true;
                foreach (var item in curModel.files)
                {
                    if (item.fileData == null)
                    {
                        item.fileData = new Ai.Hong.CommonLibrary.SpecFileFormatDouble();
                        item.fileData.ReadFile(item.filename);
                    }
                    item.color = Ai.Hong.CommonLibrary.spectrumDisplayInfo.GetDisplayColor(curModel.files.IndexOf(item));
                }

                listFiles.SetGridData(curModel.files);
                
                groupRegions.IsEnabled = true;
                listRegions.ItemsSource = curModel.regions;

            }
        }

        private void btnFileAdd_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Title = "请选择建模光谱";
            dlg.Filter = "光谱文件|*.spc";
            dlg.Multiselect = true;

            if (dlg.ShowDialog() == true)
            {
                foreach (string file in dlg.FileNames)
                {
                    Ai.Hong.CommonLibrary.spectrumDisplayInfo item = new Ai.Hong.CommonLibrary.spectrumDisplayInfo(file, Ai.Hong.CommonLibrary.spectrumDisplayInfo.GetDisplayColor(curModel.files.Count));
                    curModel.files.Add(item);
                }
            }
        }

        private void btnFileRemove_Click(object sender, RoutedEventArgs e)
        {
            if (curModel == null)
                return;
            listFiles.RemoveSelected();
        }

        private void btnRegionAdd_Click(object sender, RoutedEventArgs e)
        {
            if (curModel == null)
                return;

            curModel.regions.Add(new ModelInfo.region());
        }

        private void btnRegionDelete_Click(object sender, RoutedEventArgs e)
        {
            if (curModel == null)
                return;

            CommonMethod.RemoveDataGridItems(curModel.regions, listRegions.SelectedItems);
        }

        /// <summary>
        /// 交互设置Region
        /// </summary>
        private void btnRegionBrowse_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            ModelInfo.region region = btn.Tag as ModelInfo.region;

            if (curModel.files.Count > 0)
            {
                SpectrumRegionSelector dlg = new SpectrumRegionSelector(curModel.files[0].filename, region.firstX, region.lastX);
                dlg.Owner = App.Current.MainWindow;
                if (dlg.ShowDialog() == true)
                {
                    region.firstX = dlg.firstX;
                    region.lastX = dlg.lastX;
                }
            }
        }

        /// <summary>
        /// 删除模型
        /// </summary>
        private void btnModelDelete_Click(object sender, RoutedEventArgs e)
        {
            listModels.DeleteSelectedItems();
        }
    }
}
