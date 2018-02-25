using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NirIdentifier.Common;

namespace NirIdentifier.Common
{
    /// <summary>
    /// ModelInfoPanel.xaml 的交互逻辑
    /// </summary>
    public partial class ModelInfoPanel : UserControl
    {
        /// <summary>
        /// 模型选择消息
        /// </summary>
        public static readonly RoutedEvent ModelSelectedEvent = EventManager.RegisterRoutedEvent("ModelSelected",
                RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainWindow));
        public event RoutedEventHandler ModelSelected
        {
            add { AddHandler(ModelSelectedEvent, value); }
            remove { RemoveHandler(ModelSelectedEvent, value); }
        }

        //国药准字H20030863
        public ModelInfoPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 删除模型
        /// </summary>
        public void DeleteSelectedItems()
        {
            CommonMethod.RemoveDataGridItems(SettingData.settingData.models, gridModel.SelectedItems);
        }

        public void SetEditable(bool editable)
        {
            if (editable)
            {
                gridModel.IsReadOnly = false;
            }
            else
            {
                gridModel.IsReadOnly = true;
                gridModel.Columns[gridModel.Columns.Count - 1].Visibility = Visibility.Collapsed;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if ((bool)System.ComponentModel.DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue == false)    //设计模式不可用
            {
                if(SettingData.settingData != null && SettingData.settingData.models != null)
                    gridModel.ItemsSource = SettingData.settingData.models;
            }
        }

        private void gridModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Ai.Hong.CommonLibrary.SelectChangedArgs arg = new Ai.Hong.CommonLibrary.SelectChangedArgs(ModelSelectedEvent, gridModel.SelectedItem);
            RaiseEvent(arg);
        }
    }
}
