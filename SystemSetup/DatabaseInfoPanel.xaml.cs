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
using System.IO;

namespace NirIdentifier.SystemSetup
{
    /// <summary>
    /// DatabaseInfoPanel.xaml 的交互逻辑
    /// </summary>
    public partial class DatabaseInfoPanel : UserControl
    {
        MethodTreeNode SelectedNode = null;
        private int newNodeType;    //0:Same Level, 1:Sub Level, 2:MethodInfo, 3:edit item name, 4:edit method info
        private MethodInfo newMethodInfo = null;

        public DatabaseInfoPanel()
        {
            InitializeComponent();
            treeMethod.HideMethodTextBox(false);
            NodeSelectChanged();
        }

        //当前选择节点改变时引发的相关处理
        private void NodeSelectChanged()
        {
            btnAddNode.Visibility = (SelectedNode != null && SelectedNode.Method == null) ? Visibility.Visible : Visibility.Collapsed;
            btnAddMethod.Visibility = (SelectedNode != null && SelectedNode.Method == null) ? Visibility.Visible : Visibility.Collapsed;
            btnEditNode.Visibility = (SelectedNode != null && SelectedNode != SettingData.rootMethodTreeNode) ? Visibility.Visible : Visibility.Collapsed;
            btnDeleteNode.Visibility = (SelectedNode != null) ? Visibility.Visible : Visibility.Collapsed;
            gridItemName.Visibility = (SelectedNode != null && SelectedNode.Method == null) ? Visibility.Visible : Visibility.Collapsed;
            methodPanel.Visibility = (SelectedNode != null && SelectedNode.Method != null) ? Visibility.Visible : Visibility.Collapsed;
            gridOkCancel.Visibility = Visibility.Collapsed;
            gridNewEditDelete.Visibility = Visibility.Visible;
            treeMethod.IsEnabled = true;

            methodPanel.EnableEditor(false);
            txtItemName.IsReadOnly = true;

            txtItemName.Text = SelectedNode == null ? null : SelectedNode.Name;
            if (SelectedNode != null && SelectedNode.Method != null)
                methodPanel.SetData(SelectedNode.Method);
        }

        //添加节点
        private void btnAddNode_Click(object sender, RoutedEventArgs e)
        {
            newNodeType = 0;
            gridItemName.Visibility = Visibility.Visible;
            txtItemName.IsReadOnly = false;
            txtItemName.Text = null;
            txtItemName.Focus();

            methodPanel.Visibility = Visibility.Collapsed;
            gridNewEditDelete.Visibility = Visibility.Collapsed;
            treeMethod.IsEnabled = false;
            gridOkCancel.Visibility = Visibility.Visible;
        }

        //添加模型
        private void btnAddMethod_Click(object sender, RoutedEventArgs e)
        {
            newNodeType = 2;
            gridItemName.Visibility = Visibility.Collapsed;
            methodPanel.Visibility = Visibility.Visible;

            newMethodInfo = new MethodInfo();      //新建一个MethodInfo，防止传入的改变
            methodPanel.SetData(newMethodInfo);
            methodPanel.EnableEditor(true);

            gridNewEditDelete.Visibility = Visibility.Collapsed;
            gridOkCancel.Visibility = Visibility.Visible;
            treeMethod.IsEnabled = false;
        }

        //编辑节点或者模型信息
        private void btnEditNode_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedNode == null)
                return;

            gridNewEditDelete.Visibility = Visibility.Collapsed;
            if (SelectedNode.Method == null)    //编辑节点
            {
                txtItemName.IsReadOnly = false;
                txtItemName.Focus();
                newNodeType = 3;
            }
            else        //编辑模型信息
            {
                newMethodInfo = SelectedNode.Method.Clone();      //新建一个MethodInfo，防止传入的改变
                methodPanel.SetData(newMethodInfo);
                methodPanel.EnableEditor(true);
                newNodeType = 4;
            }
            gridOkCancel.Visibility = Visibility.Visible;
            treeMethod.IsEnabled = false;
        }

        private void btnDeleteNode_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedNode == null)
                return;

            if (SelectedNode == SettingData.rootMethodTreeNode)
            {
                if (CommonMethod.QuestionMsgBox("删除所有节点和模型信息？") == true)
                {
                    while (SelectedNode.Children.Count > 0)
                    {
                        treeMethod.DeleteNode(SelectedNode.Children[0]);
                    }
                }
            }
            else
            {
                if (CommonMethod.QuestionMsgBox("删除模型节点：\"" + SelectedNode.Name + "\"及其子节点以及包含的模型信息？") == true)
                    treeMethod.DeleteNode(SelectedNode);
            }
        }

        //模型树选择新模型或者节点的消息
        private void treeMethod_MethodSelect(object sender, RoutedEventArgs e)
        {
            SelectedNode = treeMethod.SelectedNode;
            NodeSelectChanged();
        }

        //放弃改变
        private void gridOkCancel_CancelClicked(object sender, RoutedEventArgs e)
        {
            treeMethod.IsEnabled = true;
            SelectedNode = treeMethod.SelectedNode;
            NodeSelectChanged();
        }

        //编辑时的确认按钮
        private void gridOkCancel_OKClicked(object sender, RoutedEventArgs e)
        {
            MethodTreeNode newNode = null;
            switch (newNodeType)
            {
                case 0:     //新增同级目录
                case 1:     //新增子目录
                    if (txtItemName.Text == null || txtItemName.Text.Trim() == "")
                    {
                        CommonMethod.ErrorMsgBox("请输入类型节点的名称");
                        return;
                    }

                    newNode = new MethodTreeNode();
                    newNode.Name = txtItemName.Text;
                    treeMethod.AddNode(newNode, false);     //永远是增加下一层节点
                    break;
                case 2:   //新增模型信息 
                    if (!methodPanel.UpdateData())
                        return;

                    newNode = new MethodTreeNode();
                    newNode.Method = newMethodInfo;
                    treeMethod.AddNode(newNode, false);    //永远是增加下一层节点
                    break;
                case 3:     //修改当前目录
                    SelectedNode.Name = txtItemName.Text;
                    treeMethod.ModifyNode(SelectedNode);
                    break;
                case 4:     //修改当前模型信息
                    methodPanel.UpdateData();
                    SelectedNode.Method = newMethodInfo;
                    treeMethod.ModifyNode(SelectedNode);
                    break;
            }
            NodeSelectChanged();
        }

        private bool CopyMethodFiles(MethodInfo method, string parentDirectory)
        {
            try
            {
                eftir_cls_ident_method cls_method = eftir_cls_ident_method.Deserialize(method.methodFile);
                if (cls_method == null)
                    return false;

                //拷贝引用的光谱文件
                //会将模型中的数状结构变成平面结构，因此要保证模型中所引用的文件名不相同
                foreach (eftir_cls_ident_method.analyte item in cls_method.analytes)
                {
                    string filename = Path.Combine(parentDirectory, Path.GetFileName(item.filename));
                    File.Copy(item.filename, filename, true);       
                    item.filename = filename;   //修改引用文件路径到当前路径
                }
                foreach (eftir_cls_ident_method.interferent item in cls_method.interferents)
                {
                    string filename = Path.Combine(parentDirectory, Path.GetFileName(item.filename));
                    File.Copy(item.filename, filename, true);
                    item.filename = filename;
                }

                //序列化模型文件到当前文件夹
                string clsfile = Path.Combine(parentDirectory, Path.GetFileName(method.methodFile));
                if (!cls_method.Serialize(clsfile))
                    return false;

                //序列化MethodInfo到当前文件夹
                clsfile = Path.Combine(parentDirectory, "MethodInfo_Temp.methodinfo");      //MethodInfo的内容
                return method.Serialize(clsfile);
            }
            catch (Exception ex)
            {
                CommonMethod.ErrorMsgBox(ex.Message);
                return false;
            }

        }

        //导出模型树
        private bool CreateZipFileTree(MethodTreeNode currentNode, string parentDirectory)
        {
            if (currentNode == null)
                return true;

            //创建当前节点的目录, 从1开始顺序增加
            string nodename = (currentNode.Name == null) ? "TREEVIEW_ROOT" : currentNode.Name;
            int index = 1;
            
            //去除文件名中非法字符，防止错误
            char[] badchar = new char[]{'〃', '／', ':','?', '〈', '〉', '*', '\\'};
            foreach(char st in badchar)
                nodename = nodename.Replace(st, '-');

            string nodedir = Path.Combine(parentDirectory, nodename);
            while (Directory.Exists(nodedir))   //为了防止目录名称重复，在重复的目录后面增加序号
            {
                nodedir = Path.Combine(parentDirectory, nodename + index);
                index++;
            }
            Directory.CreateDirectory(nodedir);

            //在当前节点目录下用methodnode.name记录本节点的名称
            string namefile = Path.Combine(nodedir, "methodnode.name");
            StreamWriter writer = new StreamWriter(namefile, false, Encoding.GetEncoding(SettingData.UTF8));
            writer.WriteLine(nodename);
            writer.Close();

            //是模型信息, 需要拷贝相应的模型文件和引用光谱文件，并做相应处理
            if (currentNode.Method != null)
            {
                if (CopyMethodFiles(currentNode.Method, nodedir) == false)
                    return false;
            }

            //是模型节点，继续处理下一级节点
            if (currentNode.Children != null)
            {
                foreach (MethodTreeNode node in currentNode.Children)
                {
                    if (CreateZipFileTree(node, nodedir) == false)
                        return false;
                }
            }

            return true;
        }

        private string filter = "模型包(*.raman_package)|*.raman_package";
        private void ExportMethodNode(MethodTreeNode rootNode)
        {
            if (rootNode == null)
            {
                CommonMethod.ErrorMsgBox("没有选择需要输出的模型节点");
                return;
            }

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = filter;
            if (dlg.ShowDialog() == true)
            {
                //在temp目录中创建rfdi目录
                string rfdiDir = Path.Combine(Path.GetTempPath(), "rfdi_nodeExport");
                if (!Directory.Exists(rfdiDir))
                    Directory.CreateDirectory(rfdiDir);

                CreateZipFileTree(rootNode, rfdiDir);

                if (File.Exists(dlg.FileName))      //如果压缩文件存在，删除该文件
                    File.Delete(dlg.FileName);

                using(Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(dlg.FileName, Encoding.GetEncoding(SettingData.UTF8)))
                {
                    zip.AddDirectory(rfdiDir);
                    zip.Save();
                }

                Directory.Delete(rfdiDir, true);
            }
        }

        //导出所选择的节点
        private void btnExportOne_Click(object sender, RoutedEventArgs e)
        {
            ExportMethodNode(treeMethod.SelectedNode);
        }

        private MethodTreeNode CreateMethodNodeFromDirectory(string currentPath)
        {
            try
            {
                //先查找当前node的名称, 从文件methodnode.name中读取
                string namefile = Path.Combine(currentPath, "methodnode.name");
                if (!File.Exists(namefile))
                    throw new Exception("文件格式不正确或者被损坏");
                MethodTreeNode curnode = new MethodTreeNode();
                StreamReader reader = new StreamReader(namefile, Encoding.GetEncoding(SettingData.UTF8));
                curnode.Name = reader.ReadLine();
                reader.Close();
                File.Delete(namefile);      //删除临时的methodnode.name

                string tempmethodfile = Path.Combine(currentPath, "MethodInfo_Temp.methodinfo");
                if (File.Exists(tempmethodfile))      //查看是否包含MethodInfo信息
                {
                    //加载MethodInfo信息
                    curnode.Method = MethodInfo.Deserialize(tempmethodfile);
                    File.Delete(tempmethodfile);    //删除临时的MethodInfo_Temp.methodinfo

                    //更改引用拉曼方法的路径为当前路径
                    curnode.Method.methodFile = Path.Combine(currentPath, Path.GetFileName(curnode.Method.methodFile));

                    //处理拉曼方法
                    var files = Directory.EnumerateFiles(currentPath, Path.GetFileName(curnode.Method.methodFile));
                    foreach (string file in files)
                    {
                        eftir_cls_ident_method method = eftir_cls_ident_method.Deserialize(Path.Combine(currentPath, file));
                        if (method == null)
                            throw new Exception("文件格式不正确或者被损坏");

                        //更改引用光谱的路径为当前路径
                        foreach (eftir_cls_ident_method.analyte item in method.analytes)
                            item.filename = Path.Combine(currentPath, Path.GetFileName(item.filename));
                        foreach (eftir_cls_ident_method.interferent item in method.interferents)
                            item.filename = Path.Combine(currentPath, Path.GetFileName(item.filename));

                        //保存修改路径后的方法
                        method.Serialize(Path.Combine(currentPath, file));
                    }
                }

                //处理子目录, 也就是子节点
                var subdirs = Directory.GetDirectories(currentPath);
                foreach (string dir in subdirs)
                {
                    MethodTreeNode subnode = CreateMethodNodeFromDirectory(dir);
                    if (subnode == null)
                        return null;
                    curnode.Children.Add(subnode);
                }

                return curnode;
            }
            catch (Exception ex)
            {
                CommonMethod.ErrorMsgBox(ex.Message);
                return null;
            }
        }

        private void DirectoryCopy(string SourcePath, string DestinationPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));

            //Copy all the files
            foreach (string newPath in Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
        }

        //加载打包文件
        private MethodTreeNode ImportMethodPackageFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = filter;
            dlg.Title = "请选择要导入的模型包";
            try
            {
                if (dlg.ShowDialog() != true)
                    return null;

                System.Windows.Forms.FolderBrowserDialog pathdlg = new System.Windows.Forms.FolderBrowserDialog();
                pathdlg.Description = "请选择模型包解压缩的目录";
                if (pathdlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return null;
                string savepath = pathdlg.SelectedPath;

                //在temp目录中创建rfdi目录
                string rfdiDir = Path.Combine(Path.GetTempPath(), "rfdi_nodeExport");
                if (Directory.Exists(rfdiDir))  //如果存在，删除当前文件夹
                    Directory.Delete(rfdiDir, true);
                Directory.CreateDirectory(rfdiDir);

                using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(dlg.FileName))
                {
                    zip.ExtractAll(rfdiDir);
                }
                string[] subdirs = Directory.GetDirectories(rfdiDir);

                string extractdir = Path.Combine(savepath, Path.GetFileName(subdirs[0]));
                int index = 1;
                while (Directory.Exists(extractdir))
                {
                    extractdir = Path.Combine(savepath, Path.GetFileName(subdirs[0]) + index);
                    index++;
                }
                Directory.CreateDirectory(extractdir);
                DirectoryCopy(subdirs[0], extractdir);
                Directory.Delete(rfdiDir, true);

                return CreateMethodNodeFromDirectory(extractdir);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            if (treeMethod.SelectedNode == null || treeMethod.SelectedNode.Method != null)
            {
                CommonMethod.ErrorMsgBox("请选择类型节点后再导入模型");
                return;
            }
            MethodTreeNode importNode = ImportMethodPackageFile();
            if (importNode == null)
                return;

            treeMethod.AddNodeFromPackage(treeMethod.SelectedNode, importNode);
        }


    }
}
