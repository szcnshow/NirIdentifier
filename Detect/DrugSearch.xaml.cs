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
using mshtml;
using System.Windows.Threading;
//using System.Drawing;
using System.Runtime.InteropServices;

namespace NirIdentifier.Detect
{
    /// <summary>
    /// DrugSearch.xaml 的交互逻辑
    /// </summary>
    public partial class DrugSearch : Window
    {
        //网络查询信息格式：string postFormat = "captchaUUID=&code=81061110048489991589&contactNo=028-86518886&systemId=drug-web&checkcode1=8106&checkcode2=1110&checkcode3=0484&checkcode4=8999&checkcode5=1589&areaNo=028&contactNo1=86518886&captcha=2312&searchthrough=3&Submit.x=19&Submit.y=7";
        DispatcherTimer deviceTimer = null;

        const string UriHeader = "Content-Type: application/x-www-form-urlencoded\r\n";
        const string UriAddress = "http://220.181.27.147/ivr/code/codeQuery.jhtml";
        string SearchCode = null;

        public Common.DrugInfo SearchedInfo = null;
        public string ErrorString = null;

        public DrugSearch()
        {
            InitializeComponent();
            webBrowser1.LoadCompleted += new System.Windows.Navigation.LoadCompletedEventHandler(webBrowser1_LoadCompleted);
        }

        public DrugSearch(string searchCode)
        {
            InitializeComponent();
            webBrowser1.LoadCompleted += new System.Windows.Navigation.LoadCompletedEventHandler(webBrowser1_LoadCompleted);
            this.SearchCode = searchCode;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SearchCode == null)
                    throw new Exception("请先设置查找的药监码");

                Common.CommonMethod.HideWindowSystemButton(this);

                deviceTimer = new DispatcherTimer();
                deviceTimer.Interval = new TimeSpan(0, 0, 1);
                deviceTimer.Tick += new EventHandler(deviceTimer_Tick);
                deviceTimer.Start();

                progressSearch.Maximum = 20;    //最多等20秒
                progressSearch.Value = 0;

                SearchOneDrug(SearchCode);
            }
            catch (Exception ex)
            {
                ErrorString = ex.Message;
                if (deviceTimer != null)
                    deviceTimer.Stop();
                DialogResult = false;
                this.Close();
            }
        }

        public void SearchOneDrug(string barCode)
        {
            SearchedInfo = null;

            string datastr = "captchaUUID=&code=" + barCode;
            datastr += "&contactNo=028-86518886&systemId=drug-web& ";
            datastr += "checkcode1=" + barCode.Substring(0, 4) + " &";
            datastr += "checkcode2=" + barCode.Substring(4, 4) + "&";
            datastr += "checkcode3=" + barCode.Substring(8, 4) + "&";
            datastr += "checkcode4=" + barCode.Substring(12, 4) + "&";
            datastr += "checkcode5=" + barCode.Substring(16) + "&";
            datastr += "areaNo=028&contactNo1=86518886&captcha=1235&searchthrough=3&Submit.x=19&Submit.y=7";

            byte[] data = Encoding.ASCII.GetBytes(datastr);
            webBrowser1.Navigate(new Uri(UriAddress), null, data, UriHeader);
        }

        private string GetOneInfo(string totalInfo, string infoName)
        {
            int beginpos = totalInfo.IndexOf(infoName);     //先找到【药品通用名】
            if (beginpos > 0)
            {
                beginpos += infoName.Length;
                int endpos = totalInfo.IndexOf("\r", beginpos);     //找到回车符号，中间的就是内容了
                if (endpos > 0)
                    return totalInfo.Substring(beginpos, endpos - beginpos).Trim() ;
            }

            return null;
        }

        private void webBrowser1_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            WebBrowser browser = sender as WebBrowser;
            try
            {
                if (browser == null || browser.Document == null)
                    throw new Exception("网络超时，请检查网络连接");

                HTMLDocument doc = (HTMLDocument)browser.Document;
                string infoStr = doc.documentElement.innerText;

                if (infoStr.IndexOf(SearchCode)<0)
                    throw new Exception("网络超时，请检查网络连接");
                
                SearchedInfo = new Common.DrugInfo();
                SearchedInfo.chemicalName = GetOneInfo(infoStr, "【药品通用名】");
                if(SearchedInfo.chemicalName == null)
                    throw new Exception("药品电子监管码输入错误");

                //SearchedInfo.specification = GetOneInfo(infoStr, "【制剂规格】");       //暂时不用制剂规格
                SearchedInfo.form = GetOneInfo(infoStr, "【剂型】");
                SearchedInfo.productUnit = GetOneInfo(infoStr, "【生产企业】");

                string tempstr = GetOneInfo(infoStr, "【生产日期】");
                DateTime temptime;
                DateTime.TryParse(tempstr, out temptime);
                SearchedInfo.productTime = temptime;

                SearchedInfo.batchNumber = GetOneInfo(infoStr, "【产品批号】");
                tempstr = GetOneInfo(infoStr, "【有效期至】");
                DateTime.TryParse(tempstr, out temptime);

                SearchedInfo.licenseCode = GetOneInfo(infoStr, "【批准文号】");
                DialogResult = true;
            }
            catch (Exception ex)
            {
                ErrorString = ex.Message;
                DialogResult = false;
            }
            finally
            {
                deviceTimer.Stop();
                this.Close();
            }

        }

        void deviceTimer_Tick(object sender, EventArgs e)
        {
            if (progressSearch.Value >= progressSearch.Maximum - 1)
            {
                ErrorString = "网络超时，请检查网络连接";
                deviceTimer.Stop();
                DialogResult = false;
                Close();
            }
            else
                progressSearch.Value++;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            if (deviceTimer != null)
                deviceTimer.Stop();
            this.Close();
        }

    }

    /* 识别验证码  捕捉webBrowser弹出的新窗口
     * 捕捉webBrowser弹出的新窗口
        IHTMLImgElement VerifyImage = null;

        //显示网页中的图片
        private void ShowImage()
        {
            HTMLDocument doc = (HTMLDocument)webBrowser1.Document;
            HTMLBody body = (mshtml.HTMLBody)doc.body;
            IHTMLControlRange rang = (IHTMLControlRange)body.createControlRange();
            IHTMLControlElement Img = (IHTMLControlElement)doc.getElementById("captchaImage");
            rang.add(Img);
            rang.execCommand("copy", false, null);
            Img = null;
            rang = null;
            if (Clipboard.ContainsImage())
            {
                imgCode.Stretch = Stretch.None;
                imgCode.BeginInit();
                //得到BITmap文件
                System.Drawing.Bitmap s = (System.Drawing.Bitmap)Clipboard.GetDataObject().GetData("System.Drawing.Bitmap");
                //将文件写入到内存流
                System.IO.MemoryStream meroryStream = new System.IO.MemoryStream();
                s.Save(meroryStream, System.Drawing.Imaging.ImageFormat.Png);
                //新建轉換類型
                ImageSourceConverter imgSourceConvert = new ImageSourceConverter();
                //幫定值 顯示修改後的結果
                imgCode.Source = (ImageSource)imgSourceConvert.ConvertFrom(meroryStream);
                imgCode.EndInit();
                Clipboard.Clear();
            }
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (webBrowser1.Document == null)
                return;

            HTMLDocument doc = (HTMLDocument)webBrowser1.Document;
            HTMLInputTextElement elInput;
            for (int i = 1; i <= 5; i++)
            {
                elInput = (HTMLInputTextElement)doc.getElementById("piatscode" + i);
                elInput.innerText = i.ToString() + i + i+i;
            }
            elInput = (HTMLInputTextElement)doc.getElementById("areaNo");
            elInput.innerText = "028";

            elInput = (HTMLInputTextElement)doc.getElementById("contactNo1");
            elInput.innerText = "86518886";

            HTMLButtonElement btn = (HTMLButtonElement)doc.getElementById("Submit");
            btn.click();

        }
      
       [ComImport]
        [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface UCOMIServiceProvider
        {
            [return: MarshalAs(UnmanagedType.IUnknown)]
            object QueryService(ref Guid guidService, ref Guid riid);
        }


        private void webBrowser1_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            WebBrowser browser = sender as WebBrowser;
            Guid SID_SWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
            try
            {
                //this is very important because we need a reference on Document not null!
                UCOMIServiceProvider serviceProvider = (UCOMIServiceProvider)browser.Document;
                Guid serviceGuid = SID_SWebBrowserApp;
                Guid iid = typeof(SHDocVw.IWebBrowser2).GUID;
                SHDocVw.IWebBrowser2 myWebBrowser2 = (SHDocVw.IWebBrowser2)serviceProvider.QueryService(ref serviceGuid, ref iid);
                SHDocVw.DWebBrowserEvents_Event wbEvents = (SHDocVw.DWebBrowserEvents_Event)myWebBrowser2;
                //this is the real event handling done
                wbEvents.NewWindow += new SHDocVw.DWebBrowserEvents_NewWindowEventHandler(wbEvents_NewWindow);
                
                HTMLDocument doc = null;
                if (browser.Document != null && (doc = (HTMLDocument)browser.Document) != null)
                {
                    VerifyImage = (IHTMLImgElement)doc.getElementById("captchaImage");
                }
            }
            catch (System.Exception)
            {
            }
        }

        void wbEvents_NewWindow(string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Processed)
        {
            try
            {
                // Set Processed to cancel opening of the new window.
                Processed = true;
                webBrowser2.LoadCompleted += new System.Windows.Navigation.LoadCompletedEventHandler(webBrowser2_LoadCompleted);
                string temp = null;
                byte[] data = (byte[])PostData;
                for (int i = 0; i < data.Length; i++)
                    temp += (char)data[i];
                //Data 格式：captchaUUID=&code=81061110048489991589&contactNo=028-86518886&systemId=drug-web&checkcode1=8106&checkcode2=1110&checkcode3=0484&checkcode4=8999&checkcode5=1589&areaNo=028&contactNo1=86518886&captcha=2312&searchthrough=3&Submit.x=19&Submit.y=7
                webBrowser2.Navigate(new Uri(URL), null, (byte[])PostData, Headers);
            }
            catch (Exception)
            {                
                throw;
            }
        }

        void webBrowser2_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            webBrowser1.Visibility = Visibility.Collapsed;
        }
     
    class UnCodebase
    {
        public Bitmap bmpobj;
        public UnCodebase(Bitmap pic)
        {
            bmpobj = new Bitmap(pic);    //转换为Format32bppRgb
        }

        /// <summary>
        /// 根据RGB，计算灰度值
        /// </summary>
        /// <param name="posClr">Color值</param>
        /// <returns>灰度值，整型</returns>
        private int GetGrayNumColor(System.Drawing.Color posClr)
        {
            return (posClr.R * 19595 + posClr.G * 38469 + posClr.B * 7472) >> 16;
        }

        /// <summary>
        /// 灰度转换,逐点方式
        /// </summary>
        public void GrayByPixels()
        {
            for (int i = 0; i < bmpobj.Height; i++)
            {
                for (int j = 0; j < bmpobj.Width; j++)
                {
                    int tmpValue = GetGrayNumColor(bmpobj.GetPixel(j, i));
                    bmpobj.SetPixel(j, i, Color.FromArgb(tmpValue, tmpValue, tmpValue));
                }
            }
        }

        /// <summary>
        /// 去图形边框
        /// </summary>
        /// <param name="borderWidth"></param>
        public void ClearPicBorder(int borderWidth)
        {
            for (int i = 0; i < bmpobj.Height; i++)
            {
                for (int j = 0; j < bmpobj.Width; j++)
                {
                    if (i < borderWidth || j < borderWidth || j > bmpobj.Width - 1 - borderWidth || i > bmpobj.Height - 1 - borderWidth)
                        bmpobj.SetPixel(j, i, Color.FromArgb(255, 255, 255));
                }
            }
        }

        /// <summary>
        /// 灰度转换,逐行方式
        /// </summary>
        public void GrayByLine()
        {
            Rectangle rec = new Rectangle(0, 0, bmpobj.Width, bmpobj.Height);
            BitmapData bmpData = bmpobj.LockBits(rec, ImageLockMode.ReadWrite, bmpobj.PixelFormat);// PixelFormat.Format32bppPArgb);
            //    bmpData.PixelFormat = PixelFormat.Format24bppRgb;
            IntPtr scan0 = bmpData.Scan0;
            int len = bmpobj.Width * bmpobj.Height;
            int[] pixels = new int[len];
            Marshal.Copy(scan0, pixels, 0, len);

            //对图片进行处理
            int GrayValue = 0;
            for (int i = 0; i < len; i++)
            {
                GrayValue = GetGrayNumColor(Color.FromArgb(pixels[i]));
                pixels[i] = (byte)(Color.FromArgb(GrayValue, GrayValue, GrayValue)).ToArgb();      //Color转byte
            }

            bmpobj.UnlockBits(bmpData);
        }

        /// <summary>
        /// 得到有效图形并调整为可平均分割的大小
        /// </summary>
        /// <param name="dgGrayValue">灰度背景分界值</param>
        /// <param name="CharsCount">有效字符数</param>
        /// <returns></returns>
        public void GetPicValidByValue(int dgGrayValue, int CharsCount)
        {
            int posx1 = bmpobj.Width; int posy1 = bmpobj.Height;
            int posx2 = 0; int posy2 = 0;
            for (int i = 0; i < bmpobj.Height; i++)      //找有效区
            {
                for (int j = 0; j < bmpobj.Width; j++)
                {
                    int pixelValue = bmpobj.GetPixel(j, i).R;
                    if (pixelValue < dgGrayValue)     //根据灰度值
                    {
                        if (posx1 > j) posx1 = j;
                        if (posy1 > i) posy1 = i;

                        if (posx2 < j) posx2 = j;
                        if (posy2 < i) posy2 = i;
                    };
                };
            };
            // 确保能整除
            int Span = CharsCount - (posx2 - posx1 + 1) % CharsCount;   //可整除的差额数
            if (Span < CharsCount)
            {
                int leftSpan = Span / 2;    //分配到左边的空列 ，如span为单数,则右边比左边大1
                if (posx1 > leftSpan)
                    posx1 = posx1 - leftSpan;
                if (posx2 + Span - leftSpan < bmpobj.Width)
                    posx2 = posx2 + Span - leftSpan;
            }
            //复制新图
            Rectangle cloneRect = new Rectangle(posx1, posy1, posx2 - posx1 + 1, posy2 - posy1 + 1);
            bmpobj = bmpobj.Clone(cloneRect, bmpobj.PixelFormat);
        }

        /// <summary>
        /// 得到有效图形,图形为类变量
        /// </summary>
        /// <param name="dgGrayValue">灰度背景分界值</param>
        /// <param name="CharsCount">有效字符数</param>
        /// <returns></returns>
        public void GetPicValidByValue(int dgGrayValue)
        {
            int posx1 = bmpobj.Width; int posy1 = bmpobj.Height;
            int posx2 = 0; int posy2 = 0;
            for (int i = 0; i < bmpobj.Height; i++)      //找有效区
            {
                for (int j = 0; j < bmpobj.Width; j++)
                {
                    int pixelValue = bmpobj.GetPixel(j, i).R;
                    if (pixelValue < dgGrayValue)     //根据灰度值
                    {
                        if (posx1 > j) posx1 = j;
                        if (posy1 > i) posy1 = i;

                        if (posx2 < j) posx2 = j;
                        if (posy2 < i) posy2 = i;
                    };
                };
            };
            //复制新图
            Rectangle cloneRect = new Rectangle(posx1, posy1, posx2 - posx1 + 1, posy2 - posy1 + 1);
            bmpobj = bmpobj.Clone(cloneRect, bmpobj.PixelFormat);
        }

        /// <summary>
        /// 得到有效图形,图形由外面传入
        /// </summary>
        /// <param name="dgGrayValue">灰度背景分界值</param>
        /// <param name="CharsCount">有效字符数</param>
        /// <returns></returns>
        public Bitmap GetPicValidByValue(Bitmap singlepic, int dgGrayValue)
        {
            int posx1 = singlepic.Width; int posy1 = singlepic.Height;
            int posx2 = 0; int posy2 = 0;
            for (int i = 0; i < singlepic.Height; i++)      //找有效区
            {
                for (int j = 0; j < singlepic.Width; j++)
                {
                    int pixelValue = singlepic.GetPixel(j, i).R;
                    if (pixelValue < dgGrayValue)     //根据灰度值
                    {
                        if (posx1 > j) posx1 = j;
                        if (posy1 > i) posy1 = i;

                        if (posx2 < j) posx2 = j;
                        if (posy2 < i) posy2 = i;
                    };
                };
            };
            //复制新图
            Rectangle cloneRect = new Rectangle(posx1, posy1, posx2 - posx1 + 1, posy2 - posy1 + 1);
            return singlepic.Clone(cloneRect, singlepic.PixelFormat);
        }

        /// <summary>
        /// 平均分割图片
        /// </summary>
        /// <param name="RowNum">水平上分割数</param>
        /// <param name="ColNum">垂直上分割数</param>
        /// <returns>分割好的图片数组</returns>
        public Bitmap[] GetSplitPics(int RowNum, int ColNum)
        {
            if (RowNum == 0 || ColNum == 0)
                return null;
            int singW = bmpobj.Width / RowNum;
            int singH = bmpobj.Height / ColNum;
            Bitmap[] PicArray = new Bitmap[RowNum * ColNum];

            Rectangle cloneRect;
            for (int i = 0; i < ColNum; i++)      //找有效区
            {
                for (int j = 0; j < RowNum; j++)
                {
                    cloneRect = new Rectangle(j * singW, i * singH, singW, singH);
                    PicArray[i * RowNum + j] = bmpobj.Clone(cloneRect, bmpobj.PixelFormat);//复制小块图
                }
            }
            return PicArray;
        }

        /// <summary>
        /// 返回灰度图片的点阵描述字串，1表示灰点，0表示背景
        /// </summary>
        /// <param name="singlepic">灰度图</param>
        /// <param name="dgGrayValue">背前景灰色界限</param>
        /// <returns></returns>
        public string GetSingleBmpCode(Bitmap singlepic, int dgGrayValue)
        {
            Color piexl;
            string code = "";
            for (int posy = 0; posy < singlepic.Height; posy++)
                for (int posx = 0; posx < singlepic.Width; posx++)
                {
                    piexl = singlepic.GetPixel(posx, posy);
                    if (piexl.R < dgGrayValue)    // Color.Black )
                        code = code + "1";
                    else
                        code = code + "0";
                }
            return code;
        }
    }
     * */
}
