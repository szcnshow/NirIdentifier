using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Ai.Hong.CommonLibrary;

namespace NirIdentifier.FileFormat
{
    /// <summary>
    /// FileOpenDlg.xaml 的交互逻辑
    /// </summary>
    public partial class FileOpenDlg : WindowAddOnBase 
    {
        public FileOpenDlg()
        {
            InitializeComponent();
            this.Background = SystemColors.ControlBrush;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ParentDlg.EventFileNameChanged += new PathChangedEventHandler(ParentDlg_EventFileNameChanged);
            ParentDlg.EventFolderNameChanged += new PathChangedEventHandler(ParentDlg_EventFolderNameChanged);
            ParentDlg.EventFilterChanged += new FilterChangedEventHandler(ParentDlg_EventFilterChanged);
        }

        void ParentDlg_EventFilterChanged(IFileDlgExt sender, int index)
        {
            ShowFileInfo(null);
        }

        void ParentDlg_EventFolderNameChanged(IFileDlgExt sender, string filePath)
        {
            ShowFileInfo(filePath);
        }

        void ParentDlg_EventFileNameChanged(IFileDlgExt sender, string filePath)
        {
            ShowFileInfo(filePath);
        }

        void fd_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void ShowFileInfo(string filename)
        {
            Common.DrugInfo drugInfo = Common.CommonMethod.ReadDrugInfo(filename);
            gridDrugInfo.DataContext = drugInfo;
        }
    }

}
