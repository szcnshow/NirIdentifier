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

namespace RFDI.Calibration
{
    /// <summary>
    /// Cal_prompt.xaml 的交互逻辑
    /// </summary>
    public partial class Cal_prompt : Window
    {
        public Cal_prompt(bool laserMax = true)
        {
            InitializeComponent();

            string imgfile = (laserMax == true) ? "LaserPower_max.jpg" : "LaserPower_min.jpg";
            imgfile = Common.SettingData.PackgeImagePath + imgfile;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new System.Uri(imgfile);
            bi.EndInit();
            imgLaser.Source = bi;
            txtLaser.Text = (laserMax == true) ? "请将仪器激光功率旋钮旋调到最大" : "请将仪器激光功率旋钮旋调到最小";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Common.CommonMethod.HideWindowSystemButton(this);
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

    }
}
