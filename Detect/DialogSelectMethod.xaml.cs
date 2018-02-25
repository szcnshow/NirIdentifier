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
using System.Windows.Shapes;

namespace NirIdentifier.Detect
{
    /// <summary>
    /// DialogSelectMethod.xaml 的交互逻辑
    /// </summary>
    public partial class DialogSelectMethod : Window
    {
        public float TotalValue = 0;
        public float ApiValue = 0;

        public DialogSelectMethod()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

    }
}
