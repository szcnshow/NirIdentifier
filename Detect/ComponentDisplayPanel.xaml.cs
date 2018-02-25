using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NirIdentifier.Common;
using System.Collections.ObjectModel;

namespace NirIdentifier.Detect
{
    /// <summary>
    /// ComponentDisplayPanel.xaml 的交互逻辑
    /// </summary>
    public partial class ComponentDisplayPanel : UserControl
    {
        private class ComponentDisplayInfo      //暂时没用上
        {
            public string name { get; set; }
            public string imageName { get; set; }
            public float identValue { get; set; }
            public float downThreshold { get; set; }
            public float upThreshold { get; set; }
            public string errorName { get; set; }

            private ComponentInfo _data;
            public ComponentInfo ComponentData
            {
                get { return _data; }
                set
                {
                    _data = value;
                    name = _data.name;
                    imageName = ComponentInfo.GetResultImageFile(_data.identResult);
                    identValue = _data.identValue;
                    downThreshold = _data.downThreshold;
                    upThreshold = _data.upThreshold;
                    errorName = ComponentInfo.GetResultName(_data.identResult);
                }
            }

            public ComponentDisplayInfo(ComponentInfo data)
            {
                ComponentData = data;
            }
        }

        /// <summary>
        /// Binding到viewComponents，显示检出组分信息
        /// </summary>
        ObservableCollection<ComponentInfo> ComponentDatas = new ObservableCollection<ComponentInfo>();     //暂时没用上

        private SampleResultData _data = null;
        public SampleResultData SampleData
        {
            get { return _data; }
            set
            {
                _data = value;
                if (_data == null)
                    gridResult.Visibility = Visibility.Collapsed;
                else
                    gridResult.Visibility = Visibility.Visible;

                //临时修改：20130326
                txtQuantResult.Visibility = Visibility.Collapsed;
                imgQuantResult.Visibility = Visibility.Collapsed;

                if (_data != null)
                {
                    if (imgResult == null)      //有可能还没有初始化控件
                        return;

                    panelGraphic.DrawGraphic(_data.file, System.Drawing.Color.Red);
                    if (_data.identInfo.identResult == IdentResultEnum.UNKNOWN)  //没有检测的数据
                    {
                        CommonMethod.SetImageSource(imgResult, "Unknown_128.png");
                        imgQuantResult.Source = null;
                        imgIdentifyResult.Source = null;
                        txtAddinResult.Text = null;
                        txtInvalidResult.Text = null;
                    }
                    else
                    {
                        //总的检测结果
                        bool isok = _data.identInfo.identResult == IdentResultEnum.OK;
                        CommonMethod.SetImageSource(imgResult, isok ? "IdentYes_128.png" : "IdentNo_128.png");

                        txtTotalResult.Text = isok ? "检测通过" : "检测未通过";
                        imgQuantResult.Source = null;
                        imgIdentifyResult.Source = null;
                        txtAddinResult.Text = null;
                        txtInvalidResult.Text = null;

                        //主料定量分析结果
                        isok = true;
                        if (_data.drugInfo.ramanMethod.components.Find(item => item.isAPI && (item.type == ComponentType.Quant || item.type == ComponentType.QuantAndIdent)) != null)
                            isok = (_data.drugInfo.ramanMethod.components.Find(item => item.isAPI && item.identResult == IdentResultEnum.VALUEERROR) == null);   //有定量分析错误
                        CommonMethod.SetImageSource(imgQuantResult, isok ? "OK.png" : "Error.png");

                        //主料定性分析结果
                        isok = true;
                        if (_data.drugInfo.ramanMethod.components.Find(item => item.isAPI && (item.type == ComponentType.IdentExist || item.type == ComponentType.QuantAndIdent)) != null)
                            isok = _data.drugInfo.ramanMethod.components.Find(item => item.isAPI && item.identResult == IdentResultEnum.NOTFOUND) == null;   //有定性分析错误
                        CommonMethod.SetImageSource(imgIdentifyResult, isok ? "OK.png" : "Error.png");

                        //定量定性结果
                        txtAddinResult.Text = "";
                        List<ComponentInfo> comps = _data.drugInfo.ramanMethod.components.FindAll(item => !item.isAPI && item.type==ComponentType.IdentExist && item.identResult == IdentResultEnum.OK);
                        if(comps != null)
                        {
                            foreach (ComponentInfo comp in comps)
                                txtAddinResult.Text += comp.name + ",";
                            if (txtAddinResult.Text.EndsWith(","))
                                txtAddinResult.Text = txtAddinResult.Text.Remove(txtAddinResult.Text.Length - 1);
                        }

                        //非法添加结果
                        txtInvalidResult.Text = "";
                        comps = _data.drugInfo.ramanMethod.components.FindAll(item => !item.isAPI && item.type== ComponentType.IdentNotExis && item.identResult == IdentResultEnum.NOEXIST);   //非法检出辅料
                        if (comps != null)
                        {
                            foreach (ComponentInfo comp in comps)
                                txtInvalidResult.Text += comp.name + ",";
                            if (txtInvalidResult.Text.EndsWith(","))
                                txtInvalidResult.Text = txtInvalidResult.Text.Remove(txtInvalidResult.Text.Length - 1);
                        }
                    }
                }
            }
        }

        public Visibility OpticalVisible
        {
            get { return (Visibility)GetValue(OpticalVisibleProperty); }
            set { SetValue(OpticalVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OpticalVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpticalVisibleProperty =
            DependencyProperty.Register("OpticalVisible", typeof(Visibility), typeof(ComponentDisplayPanel), new UIPropertyMetadata(Visibility.Visible));

        public ComponentDisplayPanel()
        {
            InitializeComponent();
            SampleData = null;
        }

        //隐藏拉曼光谱面板（主要用于打印结果）
        public void HideSpectrumGraphic()
        {
            tabOptical.Visibility = Visibility.Collapsed;
        }

        //隐藏详细结果，用于OpenFileDialog
        public void HideTotalResultImage()
        {
            detailsCol.Width = new GridLength(0, GridUnitType.Pixel);
        }
    }
}
