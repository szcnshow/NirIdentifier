using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using NirIdentifier.Common;
using System.Collections.ObjectModel;

namespace NirIdentifier.SystemSetup
{
    /// <summary>
    /// MethodInfoPanel.xaml 的交互逻辑
    /// </summary>
    public partial class MethodInfoPanel : UserControl
    {
        /// <summary>
        /// 组分信息，用于在DataGrid中显示
        /// </summary>
        private class ComponentDisplayInfo : INotifyPropertyChanged
        {
            /// <summary>
            /// 组分信息是否有改变
            /// </summary>
            public bool changed = false;
            /// <summary>
            /// 原来的组分名称，如果用户输入了空的组分名称，则恢复原来的名称
            /// </summary>
            public string oldname;
            private ComponentInfo _componentInfo;
            /// <summary>
            /// 组分信息
            /// </summary>
            public ComponentInfo componentInfo
            {
                get { return _componentInfo; }
                set
                {
                    _componentInfo = value;
                    QuantEditable = Visibility.Collapsed;
                    IndentEditable = Visibility.Collapsed;
                    switch (componentInfo.type)
                    {
                        case ComponentType.Quant:
                            QuantEditable = Visibility.Visible;
                            break;
                        case ComponentType.IdentExist:
                            IndentEditable = Visibility.Visible;
                            break;
                        case ComponentType.QuantAndIdent:
                            QuantEditable = Visibility.Visible;
                            IndentEditable = Visibility.Visible;
                            break;
                        case ComponentType.IdentNotExis:
                            IndentEditable = Visibility.Visible;
                            break;
                    }
                    if (PropertyChanged != null)    //属性变更通知
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("QuantEditable"));
                        PropertyChanged(this, new PropertyChangedEventArgs("IndentEditable"));
                    }
                }
            }
            /// <summary>
            /// 显示的组分名称
            /// </summary>
            public string Name
            {
                get { return componentInfo.name; }
                set
                {
                    componentInfo.name = value;
                    if (PropertyChanged != null)    //属性变更通知
                        PropertyChanged(this, new PropertyChangedEventArgs("Name"));
                }
            }
            public string componentType
            {
                get { return componentInfo.isAPI?"API":"辅料"; }
                set
                {
                    componentInfo.isAPI = value=="API"?true:false;
                    if (PropertyChanged != null)    //属性变更通知
                        PropertyChanged(this, new PropertyChangedEventArgs("componentType"));
                }
            }

            public Visibility QuantEditable { get; private set; }     //可以编辑定量数据
            public Visibility IndentEditable { get; private set; }    //可以编辑定性数据
            public ComponentType type
            {
                get { return componentInfo.type; }
                set
                {
                    componentInfo.type = value;
                    QuantEditable = Visibility.Collapsed;
                    IndentEditable = Visibility.Collapsed;
                    switch (componentInfo.type)
                    {
                        case ComponentType.Quant:
                            QuantEditable = Visibility.Visible;
                            break;
                        case ComponentType.IdentExist:
                            IndentEditable = Visibility.Visible;
                            break;
                        case ComponentType.QuantAndIdent:
                            QuantEditable = Visibility.Visible;
                            IndentEditable = Visibility.Visible;
                            break;
                        case ComponentType.IdentNotExis:
                            IndentEditable = Visibility.Visible;
                            break;
                    }
                    if (PropertyChanged != null)    //属性变更通知
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("type"));
                        PropertyChanged(this, new PropertyChangedEventArgs("QuantEditable"));
                        PropertyChanged(this, new PropertyChangedEventArgs("IndentEditable"));
                    }
                }
            }
            public float ccFactor
            {
                get { return componentInfo.ccFactor; }
                set
                {
                    componentInfo.ccFactor = value;
                    if (PropertyChanged != null)    //属性变更通知
                        PropertyChanged(this, new PropertyChangedEventArgs("ccFactor"));
                }
            }

            public float kFactor
            {
                get { return componentInfo.kFactor; }
                set
                {
                    componentInfo.kFactor = value;
                    if (PropertyChanged != null)    //属性变更通知
                        PropertyChanged(this, new PropertyChangedEventArgs("kFactor"));
                }
            }
            public float downThreshold
            {
                get { return componentInfo.downThreshold; }
                set
                {
                    componentInfo.downThreshold = value;
                    if (PropertyChanged != null)    //属性变更通知
                        PropertyChanged(this, new PropertyChangedEventArgs("downThreshold"));
                }
            }
            public float upThreshold
            {
                get { return componentInfo.upThreshold; }
                set
                {
                    componentInfo.upThreshold = value;
                    if (PropertyChanged != null)    //属性变更通知
                        PropertyChanged(this, new PropertyChangedEventArgs("upThreshold"));
                }
            }

            /*
            /// <summary>
            /// 编辑组分名称时的下拉列表，等于ComponentNameList
            /// </summary>
            public List<string> _sellist;
            public List<string> NameSelList
            {
                get { return _sellist; }
                set
                {
                    if (value != _sellist)
                    {
                        _sellist = value;
                        if (PropertyChanged != null)
                            PropertyChanged(this, new PropertyChangedEventArgs("NameSelList"));
                    }
                }
            }
            */

            public ComponentDisplayInfo(ComponentInfo info)
            {
                componentInfo = info;
                oldname = info.name;
                //NameSelList = namelist;
            }

            /// <summary>
            /// 属性变更消息
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

        }
        /// <summary>
        /// gridComponent中显示的组分列表
        /// </summary>
        ObservableCollection<ComponentDisplayInfo> componentList = new ObservableCollection<ComponentDisplayInfo>();

        private class InterferentDisplayInfo : INotifyPropertyChanged
        {
            /// <summary>
            /// 属性变更消息
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            private ComponentInfo _interInfo;       //引用的参考光谱信息

            public ComponentInfo CompInfo           //引用的参考光谱信息
            { 
                get { return _interInfo; }
                set { _interInfo = value; }
            }         

            public string name { get; set; }            //组分名
            public ComponentType identType0 
            {
                get { return _interInfo.interferents.Count>0 ? _interInfo.interferents[0].type:ComponentType.NotDeal; }
                set { UpdateIdentType(0, value); }
            }
            public ComponentType identType1                     //检测类别
            {
                get { return _interInfo.interferents.Count > 1 ? _interInfo.interferents[1].type : ComponentType.NotDeal; }
                set { UpdateIdentType(1, value); }
            }
            public ComponentType identType2                     //检测类别
            {
                get { return _interInfo.interferents.Count > 2 ? _interInfo.interferents[2].type : ComponentType.NotDeal; }
                set { UpdateIdentType(2, value); }
            }
            public ComponentType identType3                     //检测类别
            {
                get { return _interInfo.interferents.Count > 3 ? _interInfo.interferents[3].type : ComponentType.NotDeal; }
                set { UpdateIdentType(3, value); }
            }
            public ComponentType identType4                     //检测类别
            {
                get { return _interInfo.interferents.Count > 4 ? _interInfo.interferents[4].type : ComponentType.NotDeal; }
                set { UpdateIdentType(4, value); }
            }

            public float ccFactor0      //阈值
            { 
                get { return _interInfo.interferents.Count > 0 ? _interInfo.interferents[0].ccFactor : 0; }
                set { if (_interInfo.interferents.Count > 0) _interInfo.interferents[0].ccFactor = value; }
            }
            public float ccFactor1      //阈值
            {
                get { return _interInfo.interferents.Count > 1 ? _interInfo.interferents[1].ccFactor : 0; }
                set { if (_interInfo.interferents.Count > 1) _interInfo.interferents[1].ccFactor = value; }
            }
            public float ccFactor2      //阈值
            {
                get { return _interInfo.interferents.Count > 2 ? _interInfo.interferents[2].ccFactor : 0; }
                set { if (_interInfo.interferents.Count > 2) _interInfo.interferents[2].ccFactor = value; }
            }
            public float ccFactor3      //阈值
            {
                get { return _interInfo.interferents.Count > 3 ? _interInfo.interferents[3].ccFactor : 0; }
                set { if (_interInfo.interferents.Count > 3) _interInfo.interferents[3].ccFactor = value; }
            }
            public float ccFactor4      //阈值
            {
                get { return _interInfo.interferents.Count > 4 ? _interInfo.interferents[4].ccFactor : 0; }
                set { if (_interInfo.interferents.Count > 4) _interInfo.interferents[4].ccFactor = value; }
            }
            public Visibility showTextBox0 { get; set; }        //是否显示阈值编辑框
            public Visibility showTextBox1 { get; set; }        //是否显示阈值编辑框
            public Visibility showTextBox2 { get; set; }        //是否显示阈值编辑框
            public Visibility showTextBox3 { get; set; }        //是否显示阈值编辑框
            public Visibility showTextBox4 { get; set; }        //是否显示阈值编辑框

            //检测类型,转换为了String
            public void UpdateIdentType(int index, object newValue)
            {
                ComponentType curtype = (ComponentType)newValue;
                if (index < _interInfo.interferents.Count)
                    _interInfo.interferents[index].type = curtype;
                else
                    curtype = ComponentType.NotDeal;    //缺省设置为不显示

                System.Reflection.PropertyInfo pi=this.GetType().GetProperty("showTextBox"+index); 
                if(pi!=null)
                    pi.SetValue(this, (curtype == ComponentType.NotDeal) ? Visibility.Collapsed : Visibility.Visible, null);     //不需要处理时不显示编辑框
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("showTextBox" + index + ""));
            }

            public InterferentDisplayInfo(ComponentInfo compInfo)
            {
                _interInfo = compInfo;
                int count = compInfo.interferents.Count;
                if (count > 0)
                {
                    name = compInfo.name;   //组分名
                    for (int i = 0; i < 5; i++)     //最多5个组分
                    {
                        UpdateIdentType(i, (i < compInfo.interferents.Count) ? compInfo.interferents[i].type : ComponentType.NotDeal);  //超出interferents.Count,设置为不处理
                    }
                }
            }
        }
        ObservableCollection<InterferentDisplayInfo> InterferentList = new ObservableCollection<InterferentDisplayInfo>();

        MethodInfo methodInfo = null;
        public MethodInfoPanel()
        {   
            methodInfo = new MethodInfo();
            InitializeComponent();
            gridInterfentComponent.Visibility = Visibility.Collapsed;
        }

        public void SetData(MethodInfo inMethodInfo)
        {
            if (inMethodInfo != methodInfo)
            {
                methodInfo = inMethodInfo;
                InitData();
            }
        }

        private void InitData()
        {
            if (methodInfo == null)
                return;

            if(methodInfo.scanPara==null)
            {
                //缺省的扫描参数文件是INI文件中定义的文件
                methodInfo.scanPara = SettingData.settingData.runing_para.scanPara.Clone();
            }
            txtItemName.Text = methodInfo.name;
            txtMethodName.Text = methodInfo.methodFile;
            panelScanParameter.SetScanParameter(methodInfo.scanPara);

            componentList.Clear();
            InterferentList.Clear();

            foreach (ComponentInfo info in methodInfo.components)
            {
                if (info.isAPI)
                    componentList.Add(new ComponentDisplayInfo(info));          //加入主成分
                else
                    InterferentList.Add(new InterferentDisplayInfo(info));       //加入辅料
            }
            gridTargetComponent.ItemsSource = componentList;

            //针对主成分，临时屏蔽定量分析20130326
            gridTargetComponent.Columns[3].Visibility = Visibility.Collapsed;
            gridTargetComponent.Columns[4].Visibility = Visibility.Collapsed;
            gridTargetComponent.Columns[5].Visibility = Visibility.Collapsed;

            UpdateInterferentGrid();

        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "模型(*.eftir_aid)|*.eftir_aid";
            if (dlg.ShowDialog() == true)
            {
                //分析模型文件中待检测的组分
                eftir_cls_ident_method identmethod = eftir_cls_ident_method.Deserialize(dlg.FileName) ;
                if (identmethod != null)
                {
                    txtMethodName.Text = dlg.FileName;

                    componentList.Clear();
                    foreach (eftir_cls_ident_method.analyte item in identmethod.analytes)                //主料API
                    {
                        ComponentInfo info = new ComponentInfo();
                        info.name = item.name;
                        info.isAPI = true;
                        info.type = ComponentType.IdentExist;   //临时修改20130326, 以前是定量
                        componentList.Add(new ComponentDisplayInfo(info));   //加入主成分显示列表
                    }
                    gridTargetComponent.Items.Refresh();

                    InterferentList.Clear();
                    foreach (eftir_cls_ident_method.interferent item in identmethod.interferents)       //辅料
                    {
                        //如果是系统规定的不需要处理的组分，不列出来, 包括"水_"这样的组分
                        if (SettingData.settingData.runing_para.notDeal.Find(nodealItem => item.name == nodealItem || item.name.IndexOf(nodealItem + "_")==0) != null)
                            continue;

                        ComponentInfo info = new ComponentInfo();
                        info.name = item.name;
                        info.type = ComponentType.IdentExist;
                        info.isAPI = false;

                        //为辅料添加每一个targetName, 缺省设置为不检测
                        foreach (eftir_cls_ident_method.analyte apiitem in identmethod.analytes)
                        {
                            ComponentInfo.Interferent interinfo = new ComponentInfo.Interferent(apiitem.name, ComponentType.NotDeal, 0);
                            info.interferents.Add(interinfo);
                        }
                        InterferentList.Add(new InterferentDisplayInfo(info));      //加入辅料显示列表
                    }

                    UpdateInterferentGrid();
                }
                else
                {
                    CommonMethod.ErrorMsgBox("不能分析文件，错误：" + identmethod.ErrorString);
                }

            }           
        }

        //将修改后的数据保存到MethodInfo中
        public bool UpdateData()
        {
            try
            {
                if (txtItemName.Text == null || txtItemName.Text.Trim() == "" ||
                    txtMethodName.Text == null || txtMethodName.Text.Trim() == "" || panelScanParameter.GetScanParameter() == null)
                    throw new Exception("请输入完整的信息");

                methodInfo.name = txtItemName.Text;
                methodInfo.methodFile = txtMethodName.Text;
                methodInfo.scanPara = panelScanParameter.GetScanParameter();

                methodInfo.components.Clear();
                foreach (ComponentDisplayInfo dispinfo in componentList)        //主成分信息
                    methodInfo.components.Add(dispinfo.componentInfo);

                foreach (InterferentDisplayInfo dispinfo in InterferentList)    //辅料信息
                {
                    if (dispinfo.CompInfo.interferents.Find(interItem => interItem.type == ComponentType.IdentExist) != null)  //其中一个target选择了定性检测，就设定为定性检测
                        dispinfo.CompInfo.type = ComponentType.IdentExist;
                    else if (dispinfo.CompInfo.interferents.Find(interItem => interItem.type == ComponentType.IdentNotExis) != null)  //其中一个target选择了非法添加，就设定为定性检测
                        dispinfo.CompInfo.type = ComponentType.IdentNotExis;
                    else
                        dispinfo.CompInfo.type = ComponentType.NotDeal;     //否则设置为不需要处理
                    methodInfo.components.Add(dispinfo.CompInfo);
                }
                return true;
            }
            catch (Exception ex)
            {
                CommonMethod.ErrorMsgBox(ex.Message);
                return false;
            }
        }


        public void EnableEditor(bool enabled)
        {
            txtItemName.IsReadOnly = !enabled;
            txtMethodName.IsReadOnly = !enabled;
            panelScanParameter.EnableEditor(enabled);

            btnBrowse.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;

            for (int i = 0; i < gridTargetComponent.Columns.Count; i++)
                gridTargetComponent.Columns[i].IsReadOnly = !enabled;

            for (int i = 0; i < gridInterfentComponent.Columns.Count; i++)
                gridInterfentComponent.Columns[i].IsReadOnly = !enabled;
            
        }

        //设置辅料检测Grid
        private void UpdateInterferentGrid()
        {
            //没有辅料，不显示
            if (InterferentList.Count == 0)
            {
                gridInterfentComponent.Visibility = Visibility.Collapsed;
                return;
            }
            else
                gridInterfentComponent.Visibility = Visibility.Visible;

            int colindex = 1;
            //更改列的标题, 每个interferents中都包含相同的targetName，因此只处理第一个
            for (int i = 0; i < InterferentList[0].CompInfo.interferents.Count; i++)
            {
                gridInterfentComponent.Columns[i * 2 + 1].Header = InterferentList[0].CompInfo.interferents[i].targetName;
                gridInterfentComponent.Columns[i * 2 + 2].Header = "阈值";
            }
            colindex = InterferentList[0].CompInfo.interferents.Count * 2 + 1;

            //显示有内容的组分
            for (int i = 0; i < colindex; i++)
                gridInterfentComponent.Columns[i].Visibility = Visibility.Visible;
            //隐藏没有内容的组分
            for (int i = colindex; i < gridInterfentComponent.Columns.Count; i++)
                gridInterfentComponent.Columns[i].Visibility = Visibility.Collapsed;

            gridInterfentComponent.ItemsSource = InterferentList;
            gridInterfentComponent.Items.Refresh();
        }

        //针对辅料，屏蔽掉掉定量检测和定性定量检测
        public ObservableCollection<string> GetComboBoxList()
        {
            ObservableCollection<string> retList = new ObservableCollection<string>();
            retList.Add("定性检测");
            retList.Add("非法添加");
            retList.Add("不检测");

            return retList;
        }

        //针对主成分，临时屏蔽定量分析：20130326
        private void APIComboBoxComponentName_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox comlist = sender as ComboBox;
            List<string> source = comlist.ItemsSource as List<string>;
            source.Remove("定量分析");
            source.Remove("定量与定性");
            source.Remove("非法添加");
        }
    }
}
