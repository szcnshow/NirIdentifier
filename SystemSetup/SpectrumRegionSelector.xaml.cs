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

namespace NirIdentifier.SystemSetup
{
    /// <summary>
    /// SepctrumRegionSelector.xaml 的交互逻辑
    /// </summary>
    public partial class SpectrumRegionSelector : Window
    {
        /// <summary>
        ///  起始波数
        /// </summary>
        public double firstX;
        /// <summary>
        /// 结束波数
        /// </summary>
        public double lastX;
        /// <summary>
        /// firstX的Panel，在右边
        /// </summary>
        private StackPanel rightPanel = new StackPanel();
        /// <summary>
        /// lastX的panel，在左边
        /// </summary>
        private StackPanel leftPanel = new StackPanel();
        /// <summary>
        /// 中间的Panel
        /// </summary>
        private StackPanel middlePanel = new StackPanel();
        /// <summary>
        /// 光谱图形显示的Canvas
        /// </summary>
        private Canvas drawingPanel = null;
        private const double panelWidth = 3;

        private void Init()
        {
            drawingPanel = graphicChart.GetDrawingPannel();
            rightPanel.Width = panelWidth;
            rightPanel.Cursor = Cursors.SizeWE;
            rightPanel.MouseDown += new MouseButtonEventHandler(rightPanel_MouseDown);
            rightPanel.MouseUp += new MouseButtonEventHandler(rightPanel_MouseUp);
            rightPanel.MouseMove += new MouseEventHandler(rightPanel_MouseMove);

            leftPanel.Width = panelWidth;
            leftPanel.Cursor = Cursors.SizeWE;
            leftPanel.MouseDown += new MouseButtonEventHandler(rightPanel_MouseDown);
            leftPanel.MouseUp += new MouseButtonEventHandler(rightPanel_MouseUp);
            leftPanel.MouseMove += new MouseEventHandler(rightPanel_MouseMove);

            middlePanel.Width = panelWidth;
            rightPanel.Background = new SolidColorBrush(Color.FromArgb(200, 255, 0, 0));
            leftPanel.Background = new SolidColorBrush(Color.FromArgb(200, 255, 0, 0));
            middlePanel.Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 255));

            drawingPanel.Children.Add(rightPanel);
            drawingPanel.Children.Add(leftPanel);
            drawingPanel.Children.Add(middlePanel);

            Canvas.SetZIndex(rightPanel, 10);
            Canvas.SetZIndex(leftPanel, 11);

            Title = "波数范围选择器(" + firstX.ToString("F2") + "," + lastX.ToString("F2") + ")";
        }

        public SpectrumRegionSelector(string filename)
        {
            InitializeComponent();
            graphicChart.AddSpectrumFile(filename, Brushes.Black);

            Rect range = graphicChart.GetSpectrumChartRange();
            this.firstX = range.X;
            this.lastX = range.Right;

            Init();
        }

        public SpectrumRegionSelector(string filename, double firstX, double lastX)
        {
            InitializeComponent();
            graphicChart.AddSpectrumFile(filename, Brushes.Black);
            this.firstX = firstX < lastX ? firstX : lastX;
            this.lastX = lastX > firstX ? lastX : firstX;

            Rect range = graphicChart.GetSpectrumChartRange();
            this.firstX = (this.firstX > range.X && this.firstX < range.Right) ? this.firstX : range.X;
            this.lastX = (this.lastX > range.X && this.lastX < range.Right) ? this.lastX : range.Right;

            Init();
        }

        private void OkCancelPanel_CancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void OkCancelPanel_OKClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void graphicChart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas cav = graphicChart.GetDrawingPannel();
            rightPanel.Height = cav.ActualHeight;
            leftPanel.Height = cav.ActualHeight;
            middlePanel.Height = cav.ActualHeight;

            //右边的Panel
            Point rightpt = new Point(0, 0);
            graphicChart.GetMousePointFromValue(firstX, 0, ref rightpt);
            Canvas.SetLeft(rightPanel, rightpt.X - panelWidth);

            //左边的Panel
            Point leftpt = new Point(0, 0);
            graphicChart.GetMousePointFromValue(lastX, 0, ref leftpt);
            Canvas.SetLeft(leftPanel, leftpt.X);

            Canvas.SetLeft(middlePanel, leftpt.X + panelWidth);
            middlePanel.Width = rightpt.X - leftpt.X - panelWidth - panelWidth;
        }


        private bool MouseDowned = false;
        private Point oldMousePos;

        void rightPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!MouseDowned)
                return;

            StackPanel curPanel = sender as StackPanel;
            Point curMousePos = e.GetPosition(drawingPanel);

            if (curPanel == rightPanel)
            {
                //必须大于leftPanel，并且在drawingPanel区间内
                if (curMousePos.X > Canvas.GetLeft(leftPanel) + panelWidth && curMousePos.X + panelWidth < drawingPanel.ActualWidth)
                {
                    Canvas.SetLeft(rightPanel, curMousePos.X);
                    middlePanel.Width = curMousePos.X - (Canvas.GetLeft(leftPanel) + panelWidth);
                }
            }
            else
            {
                //必须小于rightPanel，并且在drawingPanel区间内
                if (curMousePos.X + panelWidth < Canvas.GetLeft(rightPanel) && curMousePos.X > 0)
                {
                    Canvas.SetLeft(leftPanel, curMousePos.X);

                    Canvas.SetLeft(middlePanel, curMousePos.X + panelWidth);
                    middlePanel.Width = Canvas.GetLeft(rightPanel) - (curMousePos.X + panelWidth);
                }
            }
        }

        void rightPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            StackPanel curPanel = sender as StackPanel;
            double tempy;

            if (curPanel == rightPanel)
                graphicChart.GetValueUnderMouse(new Point(Canvas.GetLeft(rightPanel), 0), out firstX, out tempy);
            else if (curPanel == leftPanel)
                graphicChart.GetValueUnderMouse(new Point(Canvas.GetLeft(leftPanel) + panelWidth, 0), out lastX, out tempy);

            Title = "波数范围选择器(" + firstX.ToString("F2") + "," + lastX.ToString("F2") + ")";

            curPanel.ReleaseMouseCapture();
            MouseDowned = false;
        }

        void rightPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StackPanel curPanel = sender as StackPanel;

            MouseDowned = true;
            curPanel.CaptureMouse();
            oldMousePos = e.GetPosition(drawingPanel);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Common.CommonMethod.HideWindowSystemButton(this);
        }
    }
}
