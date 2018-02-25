using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using NirIdentifier.Common;
using System.IO;
using System.Runtime.InteropServices;

namespace NirIdentifier.SystemSetup
{
    /// <summary>
    /// DrugInfoPanel.xaml 的交互逻辑
    /// </summary>
    public partial class DrugInfoPanel : UserControl
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        const int WM_QUIT = 0x0012;
        ObservableCollection<DrugInfo> DrugList = new ObservableCollection<DrugInfo>();

        public DrugInfoPanel()
        {
            InitializeComponent();
            viewDrugInfo.ItemsSource = DrugList;
        }

        private void btnImportData_Click(object sender, RoutedEventArgs e)
        {

        }

        private void InitDrugList()
        {
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitDrugList();
        }
    }
}
