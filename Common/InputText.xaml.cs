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

namespace NirIdentifier.Common
{
    /// <summary>
    /// InputText.xaml 的交互逻辑
    /// </summary>
    public partial class InputText : Window
    {
        public InputText()
        {
            InitializeComponent();
        }

        public string InputedText = null;

        /// <summary>
        /// 输入内容
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="content">当前内容</param>
        public InputText(string title, string content)
        {
            InitializeComponent();
            txtTitle.Text = title;
            txtInput.Text = content;
        }

        private void BtnOkCancel_CancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void BtnOkCancel_OKClicked(object sender, RoutedEventArgs e)
        {
            if (txtInput.Text == null || txtInput.Text.Trim() == "")
            {
                Common.CommonMethod.ErrorMsgBox("请输入内容");
                return;
            }

            InputedText = txtInput.Text;

            DialogResult = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Common.CommonMethod.HideWindowSystemButton(this);
        }
    }
}
