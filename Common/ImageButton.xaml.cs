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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NirIdentifier.Common
{
    /// <summary>
    /// ImageButton.xaml 的交互逻辑
    /// </summary>
    public partial class ImageButton : Button
    {
        //图像显示方式


        public Stretch ImageStretch
        {
            get { return (Stretch)GetValue(ImageStretchProperty); }
            set { SetValue(ImageStretchProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageStretch.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageStretchProperty =
            DependencyProperty.Register("ImageStretch", typeof(Stretch), typeof(ImageButton), new UIPropertyMetadata(Stretch.Fill));

        

        //正常状态的按钮图像
        public ImageSource NormalImage
        {
            get { return (ImageSource)GetValue(NormalImageProperty); }
            set { SetValue(NormalImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NormalImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NormalImageProperty =
            DependencyProperty.Register("NormalImage", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));


        //Focused按钮图像
        public ImageSource FocusedImage
        {
            get { return (ImageSource)GetValue(FocusedImageProperty); }
            set { SetValue(FocusedImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FocusedImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusedImageProperty =
            DependencyProperty.Register("FocusedImage", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));



        //Disabled图像
        public ImageSource DisabledImage
        {
            get { return (ImageSource)GetValue(DisabledImageProperty); }
            set { SetValue(DisabledImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisabledImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisabledImageProperty =
            DependencyProperty.Register("DisabledImage", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));


        //Pressed图像
        public ImageSource PressedImage
        {
            get { return (ImageSource)GetValue(PressedImageProperty); }
            set { SetValue(PressedImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PressedImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PressedImageProperty =
            DependencyProperty.Register("PressedImage", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));

        
        public ImageButton()
        {
            InitializeComponent();
        }
    }
}
