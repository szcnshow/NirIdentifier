using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NirIdentifier.Common;
using System.ComponentModel;

namespace NirIdentifier.Detect
{
    /// <summary>
    /// DatabaseTree.xaml 的交互逻辑
    /// </summary>
    public partial class DatabaseTree : UserControl
    {
        MethodTreeNode rootItem = null;
        public MethodTreeNode SelectedNode = null;
        //public string MethodFileName = null;
        public string ErrorString = null;

        public static readonly RoutedEvent MethodSelectEvent = EventManager.RegisterRoutedEvent("MethodSelect",
                RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DatabaseTree));
        public event RoutedEventHandler MethodSelect
        {
            add { AddHandler(MethodSelectEvent, value); }
            remove { RemoveHandler(MethodSelectEvent, value); }
        }

        public DatabaseTree()
        {
            InitializeComponent();

            if ((bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue == false)    //设计模式不可用
                rootItem = SettingData.rootMethodTreeNode;
            else
                rootItem = new MethodTreeNode();
            rootItem.parentID = 0;

            //新建临时根节点tempRootItem，让rootItem成为tempRootItem的唯一子节点，在树图中显示rootItem，这样方便操作
            MethodTreeNode tempRootItem = new MethodTreeNode();
            rootItem.Name = "ROOT";
            tempRootItem.Children.Add(rootItem);
            treeDatabase.ItemsSource = tempRootItem.Children;
        }

        //获取选择内容
        public MethodInfo GetSelectMethod()
        {
            ErrorString = null;
            if (SelectedNode == null || SelectedNode.Method==null)
            {
                ErrorString = "请选择模型";
                return null;
            }

            MethodInfo retMethod = SelectedNode.Method.Clone();

            if (gridBrowseMethod.Visibility == Visibility)       //可能有用户另外定制的信息
            {
                if (txtMethodName.Text == null || txtMethodName.Text.Trim() == "" || panelScanParameter.GetScanParameter()==null)
                {
                    ErrorString = "请输入正确的扫参数";
                    return null;
                }

                if (!System.IO.File.Exists(txtMethodName.Text))
                {
                    ErrorString = "模型文件不存在:" + txtMethodName.Text;
                    return null;
                }

                retMethod.methodFile = txtMethodName.Text;
                retMethod.scanPara = panelScanParameter.GetScanParameter();
            }

            return retMethod;
        }

        private void treeDatabase_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e!=null && e.NewValue != null)
                SelectedNode = e.NewValue as MethodTreeNode;
            else
                SelectedNode = null;

            if (SelectedNode == null || SelectedNode.Method == null)
            {
                txtMethodName.Text = null;
                panelScanParameter.SetScanParameter(null);
            }
            else
            {
                txtMethodName.Text = SelectedNode.Method.methodFile;
                panelScanParameter.SetScanParameter(SelectedNode.Method.scanPara);
            }
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = MethodSelectEvent;
            args.Source = this;
            RaiseEvent(args);
        }

        private MethodTreeNode SearchNodeFromInfo(MethodTreeNode parentItem, DrugInfo infoToFind)
        {
            if (parentItem == null || infoToFind == null)
                return null;
            if (parentItem.Method == infoToFind.ramanMethod)
                return parentItem;

            foreach (MethodTreeNode subItem in parentItem.Children)
            {
                MethodTreeNode subNode = SearchNodeFromInfo(subItem, infoToFind);
                if (subNode != null)
                    return subNode;
            }
            return null;
        }

        //根据内容来选择, 药品名 - 厂家 - 包装
        public bool SelectNode(DrugInfo infoToFind)
        {
            MethodTreeNode chemicalInfo = rootItem.Children.Find(item => item.Name == infoToFind.chemicalName);
            if (chemicalInfo == null)
                return false;

            MethodTreeNode productInfo = chemicalInfo.Children.Find(item => item.Name == infoToFind.productUnit);
            if (productInfo == null)
                return false;

            MethodTreeNode packInfo = productInfo.Children.Find(item => item.Name == infoToFind.package);
            if (packInfo == null)
                return false;

            if (packInfo.Method != infoToFind.ramanMethod)
                return false;

            packInfo.IsExpanded = true;
            packInfo.IsSelected = true;

            return true;
        }

        public void SelectNode(MethodTreeNode node)
        {
            SelectedNode = node;
            node.IsExpanded = true;
            node.IsSelected = true;
        }

        //展开或者收缩一个Node
        private void ExpandTreeNode(MethodTreeNode parentData, bool expand)
        {
            if (parentData == null)
                return;

            foreach (MethodTreeNode item in parentData.Children)
            {
                item.IsExpanded = expand;
                ExpandTreeNode(item, expand);
            }
        }
        private void btnTreeExpand_Click(object sender, RoutedEventArgs e)
        {
            ExpandTreeNode(rootItem, true);
        }

        private void btnTreeClose_Click(object sender, RoutedEventArgs e)
        {
            ExpandTreeNode(rootItem, false);
        }

        private void btnLoadDatabase_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "数据库(*.mdb)|*.mdb";
            dlg.FileName = SettingData.settingData.runing_para.database;
            if (dlg.ShowDialog() == true)
            {
                SettingData.settingData.runing_para.database = dlg.FileName;
                
            }
        }

        //在当前目录及其子目录下搜索内容, useIndexOf:True=包含字符串，False=等于字符串
        private MethodTreeNode SearchCurNode(MethodTreeNode curNode, string searchStr, bool useIndexOf)
        {
            if (useIndexOf)
            {
                if (curNode.Name.IndexOf(searchStr, 0, StringComparison.OrdinalIgnoreCase) >= 0)
                    return curNode;
            }
            else
            {
                //模型名称前面有可能会包含001-这样的字符
                string realName;
                int i = curNode.Name.IndexOf('-');
                if (i < 6 && i < curNode.Name.Length - 1)
                    realName = curNode.Name.Substring(i + 1);
                else
                    realName = curNode.Name;

                if (realName.Equals(searchStr, StringComparison.OrdinalIgnoreCase) == true)
                    return curNode;
            }
            foreach (MethodTreeNode node in curNode.Children)
            {
                MethodTreeNode foundNode = SearchCurNode(node, searchStr, useIndexOf);
                if (foundNode != null)
                    return foundNode;
            }

            return null;
        }

        //搜索父目录中排列在当前目录后面的目录（index+1）
        private MethodTreeNode SearchParent(MethodTreeNode curNode, string searchStr)
        {
            if (curNode.Parent == null)
                return null;

            int index = curNode.Parent.Children.IndexOf(curNode);               //当前目录的序号
            for (int i = index + 1; i < curNode.Parent.Children.Count; i++)     //从下一个开始搜索
            {
                MethodTreeNode foundNode = SearchCurNode(curNode.Parent.Children[i], searchStr,true);
                if (foundNode != null)
                    return foundNode;
            }
            return SearchParent(curNode.Parent, searchStr);
        }

        //搜索内容
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedNode == null)
                SelectedNode = rootItem;

            if (rootItem.Children.Count == 0)
                return;

            if (txtSearch.Text == null || txtSearch.Text.Trim() == "")
            {
                CommonMethod.ErrorMsgBox("请输入要搜索的文字");
                return;
            }

            MethodTreeNode foundNode = null;
            foreach (MethodTreeNode node in SelectedNode.Children)       //首先在当前选择目录的子目录中查找
            {
                foundNode = SearchCurNode(node, txtSearch.Text, true);
                if (foundNode != null)
                    break;
            }

            if (foundNode == null)  //查找父目录
            {
                foundNode = SearchParent(SelectedNode, txtSearch.Text);
            }

            if (foundNode == null)
            {
                //从头查找一遍
                if(SelectedNode != rootItem.Children[0] && CommonMethod.QuestionMsgBox("在模型数中没找到：" + txtSearch.Text + "，是否从头查找？") == true)
                {
                    foreach (MethodTreeNode node in rootItem.Children)       //首先在当前选择目录的子目录中查找
                    {
                        foundNode = SearchCurNode(node, txtSearch.Text,true);
                        if (foundNode != null)
                            break;
                    }
                }
                if (foundNode == null)
                {
                    CommonMethod.ErrorMsgBox("在模型数中没找到：" + txtSearch.Text);
                }
                else
                {
                    SelectedNode = foundNode;
                    SelectedNode.IsExpanded = true;
                    SelectedNode.IsSelected = true;
                }
            }
            else
            {
                SelectedNode = foundNode;
                SelectedNode.IsExpanded = true;
                SelectedNode.IsSelected = true;
            }
        }

        //增加节点或者模型
        public void AddNode(MethodTreeNode newNode, bool sameLevel)
        {
            if (SelectedNode == null)
            {
                newNode.Parent = rootItem;
                newNode.parentID = 0;
                rootItem.Children.Add(newNode);
            }
            else
            {
                if (sameLevel)
                {
                    newNode.Parent = SelectedNode.Parent;
                    newNode.parentID = SelectedNode.parentID;
                    SelectedNode.Parent.Children.Add(newNode);
                }
                else
                {
                    newNode.Parent = SelectedNode;
                    newNode.parentID = SelectedNode.ID;
                    SelectedNode.Children.Add(newNode);
                }
            }

            newNode.IsExpanded = true;
            newNode.IsSelected = true;

            treeDatabase.Items.Refresh();

            //插入到数据库中
            using (DBConnection db = new DBConnection())
            {
                if (newNode.Method == null)         //插入树节点
                    db.InsertTreeNode(newNode);
                else        //插入模型信息
                {
                    newNode.Method.nodeID = newNode.parentID;
                    db.InsertMethodInfo(newNode.Method);
                }
            }
        }

        /// <summary>
        /// 将节点数添加到数据库中，同时更新ID和parentID
        /// </summary>
        /// <param name="parentNode">父节点</param>
        /// <param name="nodeToInsert">当前需要添加的节点</param>
        private void InsertNodeToDatabase(MethodTreeNode parentNode, MethodTreeNode nodeToInsert)
        {
            nodeToInsert.parentID = parentNode.ID;
            nodeToInsert.Parent = parentNode;
            using (DBConnection db = new DBConnection())
            {
                if (nodeToInsert.Method != null)            //如果包含模型信息，需要插入模型信息表
                {
                    nodeToInsert.Method.nodeID = parentNode.ID;       //指明这是属于nodetToInsert的方法信息
                    db.InsertMethodInfo(nodeToInsert.Method);
                }
                else
                    db.InsertTreeNode(nodeToInsert);        //插入节点信息表

            }

            foreach (MethodTreeNode subnode in nodeToInsert.Children)
                InsertNodeToDatabase(nodeToInsert, subnode);
        }

        /// <summary>
        /// 将nodeToAdd下面的节点加载到parentNode下
        /// </summary>
        /// <param name="parentNode">父节点</param>
        /// <param name="packageNode">打包的节点</param>
        public void AddNodeFromPackage(MethodTreeNode parentNode, MethodTreeNode packageNode)
        {
            //将packageNode下面的节点添加到数据库中，不包含packageNode本身，以防止从ROOT导出的节点不能添加到ROOT下面
            foreach(MethodTreeNode subnode in packageNode.Children)
                InsertNodeToDatabase(parentNode, subnode);

            if (packageNode.Method != null)      //只是模型节点
                parentNode.Children.Add(packageNode);
            else    //是类型节点，放到当前类型节点下面
                parentNode.Children.AddRange(packageNode.Children);

            treeDatabase.Items.Refresh();
            parentNode.IsExpanded = true;
        }

        //删除节点
        public void DeleteNode(MethodTreeNode delNode)
        {
            if (delNode == null || delNode.Parent == null || delNode == rootItem)
                return;

            //从数据库中删除
            using (DBConnection db = new DBConnection())
            {
                db.DeleteTreeNode(delNode);
            }

            delNode.Parent.Children.Remove(delNode);
            treeDatabase.Items.Refresh();
        }

        //修改节点
        public void ModifyNode(MethodTreeNode modifyNode)
        {
            if (modifyNode == rootItem)
                return;

            //从数据库中删除
            using (DBConnection db = new DBConnection())
            {
                if (modifyNode.Method == null)
                    db.UpdateTreeNode(modifyNode);
                else
                    db.UpdateMethodInfo(modifyNode.Method);
            }
        }

        //隐藏直接浏览拉曼方法的面板
        public void HideMethodTextBox(bool scanParaOnly)
        {
            if (scanParaOnly)
                panelScanParameter.Visibility = Visibility.Collapsed;
            else
                gridBrowseMethod.Visibility = Visibility.Collapsed;
        }

        //直接浏览方法
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Raman Method(*.eftir_aid)|*.eftir_aid";
            if (dlg.ShowDialog() == true)
                txtMethodName.Text = dlg.FileName;
        }

        private void txtMethodName_TextChanged(object sender, TextChangedEventArgs e)
        {
            //MethodFileName = txtMethodName.Text;
        }

        public MethodTreeNode FindNodeWithMethodInfo(MethodTreeNode parent, MethodInfo info)
        {
            if (info == null || parent == null)
                return null;

            if (parent.Method != null)
            {
                //比较Raman方法
                if (String.Compare(parent.Method.methodFile, info.methodFile, true) == 0)   
                {
                    //比较组分
                    if (parent.Method.components.Count == info.components.Count)
                    {
                        bool found = false;
                        for(int i=0; i<parent.Method.components.Count; i++)     //查看是否有不相同的组分
                        {
                            if(!parent.Method.components[i].IsSameComponent(info.components[i]))
                            {
                                found = true;
                                break;
                            }
                        }
                        if(!found)  //没找到不同，表示已经找到该方法
                            return parent;
                    }
                }
            }

            if (parent.Children != null)
            {
                foreach (MethodTreeNode childnode in parent.Children)
                {
                    MethodTreeNode foundnode = FindNodeWithMethodInfo(childnode, info);
                    if (foundnode != null)
                        return foundnode;
                }
            }
            return null;
        }

        /// <summary>
        /// 样品检测时搜索当前药品模型信息
        /// </summary>
        /// <param name="chemicalName">药品名称</param>
        /// <param name="packageName">包装名称</param>
        /// <returns></returns>
        public MethodTreeNode SearchFromNewSample(string chemicalName, string packageName)
        {
            MethodTreeNode chemidcalNode = SearchCurNode(rootItem, chemicalName,false);
            if (chemidcalNode == null)
                return null;

            MethodTreeNode packageNode = SearchCurNode(chemidcalNode, packageName,false);
            if (packageNode == null)
                return chemidcalNode;

            if (packageNode.Children != null && packageNode.Children.Count > 0)
            {
                ExpandTreeNode(packageNode, true);
                MethodTreeNode firstNode = packageNode.Children[0];
                firstNode.IsSelected = true;

                return firstNode;
            }
            else
                return null;
        }
    }


}
