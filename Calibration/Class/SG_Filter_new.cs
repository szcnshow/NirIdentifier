using System;

namespace NirLib.BasicAlgorithm
{

    public class SG_Filter
    {
        static string errorstr = null;

        public string GetError()
        {
            return errorstr;
        }

        /// <summary>
        /// 获取Savitzky-Golay滤波系数
        /// </summary>
        /// <param name="nl">左边取样点数</param>
        /// <param name="nr">右边边取样点数</param>
        /// <param name="ld">导数的阶数, 0=平滑</param>
        /// <param name="m">拟合方程的阶数,2阶，4阶</param>
        /// <returns>SG滤波的系数</returns>
        private static double[] sgcoeff(int nl, int nr, int ld, int m)
        {
            int j, k, imj, ipj, kk, mm;
            double d, fac, sum;
            int np = nl + nr + 1;
            double[] c = new double[np+1];

            if (nl < 0 || nr < 0 || ld > m || nl + nr < m)
            {
                errorstr = "bad args in savgol";
                return null;
            }

            int[] indx = new int[m + 1];
            double[,] a = new double[m + 1, m + 1];
            double[] b = new double[m + 1];
            for (ipj = 0; ipj <= (m << 1); ipj++)       //设置所求最小二乘法拟合的正规方程
            {
                sum = (ipj != 0 ? 0.0 : 1.0);
                for (k = 1; k <= nr; k++)
                    sum += System.Math.Pow(k, ipj);
                for (k = 1; k <= nl; k++)
                    sum += System.Math.Pow(-k, ipj);
                mm = Math.Min(ipj, 2 * m - ipj);
                for (imj = -mm; imj <= mm; imj += 2)
                    a[(ipj + imj) / 2, (ipj - imj) / 2] = sum;
            }
            ludcmp(a, indx, out d);         //LU分解
            for (j = 0; j < m + 1; j++)
                b[j] = 0.0;
            b[ld] = 1.0;

            //右端向量为单位向量，依赖于所求导数
            lubksb(a, indx, ref b);         //获取逆矩阵的一行
            for (kk = 0; kk < np; kk++)     //0化输出数组，可能大于实际需要的长度
                c[kk] = 0.0;
            for (k = -nl; k <= nr; k++)
            {
                sum = b[0];                 //每个S_G系数都是整数幂与逆矩阵行的点乘
                fac = 1.0;
                for (mm = 1; mm <= m; mm++)
                    sum += b[mm] * (fac *= k);
                kk = (np - k) % np;         //环绕存储的位置
                c[kk] = sum;
            }

            return c;
        }

        /// <summary>
        /// LU分解的Crout算法（将A分解为两个矩阵的乘积，A = L x U，L为下三角矩阵，U为上三角矩阵)
        /// </summary>
        /// <param name="a">n x n阶矩阵</param>
        /// <param name="indx">输出：n阶向量，用来记录因部分主元法而改变了的行排列次序</param>
        /// <param name="d">输出：+1=行交互次数为偶数，-1=奇数</param>
        private static void ludcmp(double[,] a, int[] indx, out double d)
        {
            const double TINY = 1.0e-20;
            int i, imax = 0, j, k;
            double big, dum, sum, temp;

            int n = a.GetLength(0);
            double[] vv = new double[n];
            d = 1.0;
            for (i = 0; i < n; i++)
            {
                big = 0.0;
                for (j = 0; j < n; j++)
                {
                    if ((temp = System.Math.Abs(a[i, j])) > big)
                        big = temp;
                }
                if (big == 0.0)
                    errorstr = "Singular matrix in routine ludcmp";
                vv[i] = 1.0 / big;
            }
            for (j = 0; j < n; j++)
            {
                for (i = 0; i < j; i++)
                {
                    sum = a[i, j];
                    for (k = 0; k < i; k++)
                        sum -= a[i, k] * a[k, j];
                    a[i, j] = sum;
                }
                big = 0.0;
                for (i = j; i < n; i++)
                {
                    sum = a[i, j];
                    for (k = 0; k < j; k++)
                        sum -= a[i, k] * a[k, j];
                    a[i, j] = sum;
                    if ((dum = vv[i] * System.Math.Abs(sum)) >= big)
                    {
                        big = dum;
                        imax = i;
                    }
                }
                if (j != imax)
                {
                    for (k = 0; k < n; k++)
                    {
                        dum = a[imax, k];
                        a[imax, k] = a[j, k];
                        a[j, k] = dum;
                    }
                    d = -d;
                    vv[imax] = vv[j];
                }
                indx[j] = imax;
                if (a[j, j] == 0.0)
                    a[j, j] = TINY;
                if (j != n - 1)
                {
                    dum = 1.0 / (a[j, j]);
                    for (i = j + 1; i < n; i++)
                        a[i, j] *= dum;
                }
            }
        }

        /// <summary>
        /// 交换数据
        /// </summary>
        private static void SWAP<T>(T a, T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        /// <summary>
        /// 快速傅里叶变换
        /// </summary>
        /// <param name="data">数据，长度为2的n次幂</param>
        /// <param name="isign">1:傅里叶变换，-1:逆变换</param>
        public static void four1(double[] data,int isign)
        {
	        uint n,mmax,m,j,istep,i;
	        double wtemp,wr,wpr,wpi,wi,theta;
	        double tempr,tempi;

            uint nn = (uint)data.Length/2;
	        n= nn<<1;
	        j= 1;
	        for(i= 1;i<n;i+= 2)
            {
		        if(j> i)        //位序颠倒
                {
                    SWAP(data[j], data[i]);            //交换两个复数
			        SWAP(data[j+1],data[i+1]);
		        }
		        m= n>>1;
		        while(m>=2&&j> m)
                {
			        j-= m;
			        m>>= 1;
		        }
		        j+= m;
	        }

            //Danielson-Lanczos算法部分
	        mmax= 2;
	        while(n> mmax)      //执行log2nn次外循环
            {
		        istep= mmax<<1;
		        theta= isign*(6.28318530717959/mmax);       //三角递归的初始赋值
		        wtemp= Math.Sin(0.5*theta);
		        wpr= -2.0*wtemp*wtemp;
                wpi = Math.Sin(theta);
		        wr= 1.0;
		        wi= 0.0;
		        for(m= 1;m<mmax;m+= 2)      //两个嵌套内循环
                {
			        for(i= m;i<=n;i+= istep)    
                    {
				        j= i+mmax;
                        tempr = wr * data[j] - wi * data[j + 1];     //Danielson-Lanczos公式
                        tempi = wr * data[j + 1] + wi * data[j];
				        data[j]= data[i]-tempr;
				        data[j+1]= data[i+1]-tempi;
				        data[i]+= tempr;
				        data[i+1]+= tempi;
			        }
			        wr= (wtemp= wr)*wpr-wi*wpi+wr;      //三角递归
			        wi= wi*wpr+wtemp*wpi+wi;
		        }
		        mmax= istep;
	        }
        }

        /// <summary>
        /// 利用变换Fn的对称性对两个实函数同时做FFT变换。
        /// </summary>
        /// <param name="data1">实函数，长度为2的n次幂</param>
        /// <param name="data2">实函数，长度为2的n次幂</param>
        /// <param name="fft1">输出复型结果，长度为2*n</param>
        /// <param name="fft2">输出复型结果，长度为2*n</param>
        public static void twofft(double[] data1,double[] data2, out double[] fft1, out double[] fft2)
        {
            int nn3, nn2, jj, j;
	        double rep,rem,aip,aim;

            int n = data1.Length;
            fft1 = new double[2 * n];
            fft2 = new double[2 * n];

	        nn3= 1+(nn2= 2+n+n);
	        for(j= 1,jj= 2;j<=n;j++,jj+= 2)     //将两个实型数组组合成一个复型数组
            {
		        fft1[jj-1]= data1[j];
		        fft1[jj]= data2[j];
	        }
	        four1(fft1,1);      //求复型数组的变换

	        fft2[1]= fft1[2];
	        fft1[2]= fft2[2]= 0.0;
	        for(j= 3;j<=n+1;j+= 2){
		        rep= 0.5*(fft1[j]+fft1[nn2-j]);     //用对称性分离两个变换
		        rem= 0.5*(fft1[j]-fft1[nn2-j]);
		        aip= 0.5*(fft1[j+1]+fft1[nn3-j]);
		        aim= 0.5*(fft1[j+1]-fft1[nn3-j]);

		        fft1[j]= rep;                       //将它们按复型数组输出来
		        fft1[j+1]= aim;
		        fft1[nn2-j]= rep;
		        fft1[nn3-j]= -aim;
		        fft2[j]= aip;
		        fft2[j+1]= -rem;
		        fft2[nn2-j]= aip;
		        fft2[nn3-j]= rem;
	        }
        }

        /// <summary>
        /// 单个实函数的FFT，计算一组n个实值数据点的傅里叶变换，用复傅里叶变换的正半频率替换这些数据
        /// 复变换的第一个和最后一个分量的实数值分别返回单元data[0]和data[1]中
        /// </summary>
        /// <param name="data">n个实值数据点，n必须是2的整数次幂</param>
        /// <param name="isign">True=傅里叶变换，false=复型数据数组的逆变换，在这种情况下，其结果必须乘以2/n</param>
        private static void realft(double[] data, int isign)
        {
            int i,i1,i2,i3,i4,n=data.Length;
            double c1=0.5,c2,h1r,h1i,h2r,h2i,wr,wi,wpr,wpi,wtemp;
            double theta=3.141592653589793238/(double)(n>>1);   //递归的初始值

            if (isign == 1) 
            {
                c2 = -0.5;
                four1(data,1); //正向变换
            } 
            else 
            {
                c2=0.5;     //逆向变换
                theta = -theta; 
            }

            wtemp=Math.Sin(0.5*theta);
            wpr = -2.0*wtemp*wtemp;
            wpi=Math.Sin(theta);
            wr=1.0+wpr;
            wi=wpi;
            for (i=1;i<(n>>2);i++)  //i=0时以下分别完成
            { 
                i2=1+(i1=i+i);
                i4=1+(i3=n-i1);
                h1r=c1*(data[i1]+data[i3]);     //两个分变换从Data中分离出来
                h1i = c1*(data[i2]-data[i4]); 
                h2r= -c2*(data[i2]+data[i4]);
                h2i=c2*(data[i1]-data[i3]);
                data[i1]=h1r+wr*h2r-wi*h2i;     //次处重新组合以形成原始数据的真实变换
                data[i2]=h1i+wr*h2i+wi*h2r;
                data[i3]=h1r-wr*h2r+wi*h2i;
                data[i4]= -h1i+wr*h2i+wi*h2r;
                wr=(wtemp=wr)*wpr-wi*wpi+wr;    //递归式
                wi=wi*wpr+wtemp*wpi+wi;
            }
            if (isign == 1) 
            {
                data[0] = (h1r=data[0])+data[1]; //同时挤压第一个和最后一个数据使它们都在原始数组中
                data[1] = h1r-data[1];
            } 
            else 
            {
                data[0]=c1*((h1r=data[0])+data[1]);
                data[1]=c1*(h1r-data[1]);
                four1(data,-1);                  //iSign=-1的逆变换
            } 
        }

        private static void lubksb(double[,] a, int[] indx, ref double[] b)
        {
            int i, ii = 0, ip, j;
            double sum;

            int n = a.GetLength(0);
            for (i = 0; i < n; i++)
            {
                ip = indx[i];
                sum = b[ip];
                b[ip] = b[i];
                if (ii != 0)
                {
                    for (j = ii - 1; j < i; j++)
                        sum -= a[i, j] * b[j];
                }
                else if (sum != 0.0)
                    ii = i + 1;
                b[i] = sum;
            }
            for (i = n - 1; i >= 0; i--)
            {
                sum = b[i];
                for (j = i + 1; j < n; j++)
                    sum -= a[i, j] * b[j];
                b[i] = sum / a[i, i];
            }
        }

        /// <summary>
        /// 卷积和解卷积
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="respns">响应函数, 长度m为奇数并且不能大于data的长度n，必须环绕顺序存储，前一半是正时间脉冲响应函数，后一半是负时间脉冲响应函数，计算从最高元素respns[m-1]开始</param>
        /// <param name="isign">1：卷积，-1:解卷积</param>
        public static double[] convlv(double[] data, double[] respns, int isign) 
        {
            int i, no2, n = data.Length, m = respns.Length;
            double mag2,tmp;
            double[] temp = new double[n];
            double[] ans = new double[data.Length*2];     //返回数组

            temp[0]=respns[0];
            for (i=1;i<(m+1)/2;i++)             //把respns变为长度为n的数组
            { 
                temp[i]=respns[i];
                temp[n-i]=respns[m-i];
            }
            for (i=(m+1)/2;i<n-(m-1)/2;i++)     //n长度的temp数组中间填充0
                temp[i]=0.0;
            for (i=0;i<n;i++)
                ans[i]=data[i];

            realft(ans,1);                  //对两个函数进行FFT
            realft(temp,1);
            no2=n>>1;
            if (isign == 1)                 //卷积
            {
                for (i=2;i<n;i+=2)          //FFT相乘以求卷积
                {
                    tmp=ans[i];
                    ans[i]=(ans[i]*temp[i]-ans[i+1]*temp[i+1])/no2;
                    ans[i+1]=(ans[i+1]*temp[i]+tmp*temp[i+1])/no2;
                }
                ans[0]=ans[0]*temp[0]/no2;
                ans[1]=ans[1]*temp[1]/no2;
            } 
            else if (isign == -1) 
            {
                for (i=2;i<n;i+=2)          //FFT相除解卷积
                {
                    if ((mag2=Math.Sqrt(temp[i])+Math.Sqrt(temp[i+1])) == 0.0)
                    {
                        errorstr = "Deconvolving at response zero in convlv";
                        return null;
                    }
                    tmp=ans[i];
                    ans[i]=(ans[i]*temp[i]+ans[i+1]*temp[i+1])/mag2/no2;
                    ans[i+1]=(ans[i+1]*temp[i]-tmp*temp[i+1])/mag2/no2;
                }
                if (temp[0] == 0.0 || temp[1] == 0.0)
                {
                    errorstr = "Deconvolving at response zero in convlv";
                    return null;
                }
                ans[0]=ans[0]/temp[0]/no2;
                ans[1]=ans[1]/temp[1]/no2;
            } 
            else
            {
                errorstr = "No meaning for isign in convlv";
                return null;
            }
            realft(ans,-1); //求逆变换返回时域

            return ans;

        }

        /// <summary>
        /// Savitzy-Golay滤波(double)
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="nl">左边取样点数</param>
        /// <param name="nr">右边取样点数</param>
        /// <param name="ld">导数阶数,0=平滑</param>
        /// <param name="m">拟合方程阶数</param>
        /// <param name="useConvolv">是否使用NR的卷积算法</param>
        /// <returns>处理后的数据</returns>
        public static double[] sgfilter(double[] data, double xstep, int nl,int nr,int ld,int m, bool useConvolv)
        {
	        int np= nl+1+nr;
	        double[] c;
	        int j;
	        int k;
            int mm = data.Length;
            double[] retdata;

	        c = sgcoeff(nl,nr,ld,m);
            if (c == null)
                return null;
            
            //0阶（平滑）、1阶2次的系数与标准系数相同
            if (ld == 1 && m > 2)    //1阶3次，4次的系数 x -1 = 标准系数
            {
                for (int i = 0; i < c.Length; i++)
                    c[i] *= -1;
            }
            else if (ld == 2 && m<=2)       //2阶2次的系数 x 2 = 标准系数
            {
                for (int i = 0; i < c.Length; i++)
                    c[i] *= 2;
            }
            else if (ld == 2 && m > 2) //2阶3次，4次的系数 x 24 = 标准系数(不可靠)
            {
                for (int i = 0; i < c.Length; i++)
                    c[i] *= 24;
            }

            if (useConvolv)
            {
                retdata = convlv(data, c, 1);
            }
            else
            {
                retdata = new double[data.Length];
                for (k = nl; k < retdata.Length-nr; k++)
                {
                    for (j = -nl; j <= nr; j++)
                    {
                        retdata[k] += c[(j <= 0 ? -j : nl + nr + 1 - j)] * data[k + j];
                    }
                }
                for (k = 0; k < nl; k++)    //开始的nl个数据
                    retdata[k] = retdata[nl];
                for (k = retdata.Length - nr; k < retdata.Length; k++)  //结束的nr个数据
                    retdata[k] = retdata[retdata.Length - nr-1];

                //还要除去xstep，实现导数(1阶导数: 1/xstep, 2阶导数: 1/(xstep*xstep)
                if (ld > 0)
                {
                    double rate = (ld == 1 ? 1.0/xstep : 1.0/(xstep*xstep));
                    for (k = 0; k < retdata.Length; k++)
                        retdata[k] *= rate;
                }
            }

            return retdata;
        }

    }

    public static class SpectrumAlgorithm
    {
        /// <summary>
        /// 是否在区间内
        /// </summary>
        /// <param name="value">要查找的值</param>
        /// <param name="startValue">区间起始值</param>
        /// <param name="endValue">区间结束值</param>
        /// <returns>是否在区间内</returns>
        private static bool ValueInside(double value, double startValue, double endValue)
        {
            if (startValue < endValue)
            {
                if (value >= startValue && value <= endValue)
                    return true;
                else
                    return false;
            }
            else
            {
                if (value >= endValue && value <= startValue)
                    return true;
                else
                    return false;
            }
        }

        //比较大小，如果begin>end，交换
        private static void SortInOrder(ref int beginvalue, ref int endvalue)
        {
            if (beginvalue > endvalue)
            {
                int temp = endvalue;
                endvalue = beginvalue;
                beginvalue = temp;
            }
        }

        /// <summary>
        /// 线性插值
        /// </summary>
        private static double LinearInterpolation(double interpX, double x1, double y1, double x2, double y2)
        {
            double k = (y2 - y1) / (x2 - x1);
            double b = y2 - k * x2;

            return interpX * k + b;
        }

        /// <summary>
        /// 计算光谱积分，积分方式与OPUS B 类型相同
        /// </summary>
        /// <param name="xData">X轴数据</param>
        /// <param name="yData">Y轴数据</param>
        /// <param name="freqStart">积分起始X值</param>
        /// <param name="freqEnd">积分结束X值</param>
        /// <returns>积分结果</returns>
        public static double Integrate(double[] xData, double[] yData, double freqStart, double freqEnd)
        {
            int beginIndex = FindNearestPosition(xData, 0, xData.Length - 1, freqStart);
            int endIndex = FindNearestPosition(xData, 0, xData.Length - 1, freqEnd);

            //xData范围内没有找到freqStart或freqEnd
            if (beginIndex == -1 || endIndex == -1)
                return 0;

            SortInOrder(ref beginIndex, ref endIndex);

            double[] x = new double[endIndex - beginIndex + 1];
            double[] y = new double[endIndex - beginIndex + 1];

            for (int i = beginIndex; i <= endIndex; i++)
            {
                x[i - beginIndex] = xData[i];
                y[i - beginIndex] = yData[i];
            }

            alglib.spline1dinterpolant c;
            alglib.spline1dbuildcubic(x, y, out c);

            double value = alglib.spline1dintegrate(c, x[x.Length - 1]);

            //计算基线下面的面积
            double beginyvalue = alglib.spline1dcalc(c, freqStart);
            double endyvalue = alglib.spline1dcalc(c, freqEnd);
            double basevalue = Math.Abs((freqEnd - freqStart) * (endyvalue + beginyvalue) / 2);

            return value - basevalue;
        }

        /// <summary>
        /// 查找valueToFind在X轴数据中的位置
        /// </summary>
        /// <param name="xData">X轴数据</param>
        /// <param name="beginx">查找起始点</param>
        /// <param name="endx">查找结束点</param>
        /// <param name="valueToFind">要查找的值</param>
        /// <returns>返回valueToFind的位置</returns>
        public static int FindNearestPosition(double[] xData, int beginx, int endx, double valueToFind)
        {
            if (xData == null || xData.Length == 0 || !ValueInside(valueToFind, xData[beginx], xData[endx]))
                return -1;

            int foundPos = -1;
            if (xData[beginx] == valueToFind)
                foundPos = beginx;
            else if (xData[endx] == valueToFind)
                foundPos = endx;
            else if (beginx == endx || beginx == endx - 1)      //只相差1个，不用再分了
            {
                if (Math.Abs(xData[endx] - valueToFind) > Math.Abs(xData[beginx] - valueToFind))
                    foundPos = beginx;
                else
                    foundPos = endx;
            }

            if (foundPos != -1)  // 找到了,还要比较左右看看哪个更接近
            {
                double x1 = Math.Abs(xData[foundPos] - valueToFind);
                double x2 = (foundPos - 1>0) ? Math.Abs(xData[foundPos - 1] - valueToFind) : x1;
                double x3 = (foundPos+1)<xData.Length ? Math.Abs(xData[foundPos + 1] - valueToFind):x1;

                if (x1 <= x2 && x1 <= x3)
                    return foundPos;
                else if (x2 < x1 && x2 < x3)
                    return foundPos - 1;
                else
                    return foundPos + 1;
            }

            int midPos = beginx + (endx - beginx) / 2;      //中间点
            if (xData[0] < xData[xData.Length - 1])      //递增序列
            {
                if (xData[midPos] < valueToFind)      //中间点小于valutToFind，在后面查找
                {
                    return FindNearestPosition(xData, midPos, endx, valueToFind);
                }
                else //中间点大于valutToFind，在前面查找
                {
                    return FindNearestPosition(xData, beginx, midPos, valueToFind);
                }
            }
            else        //递减序列
            {
                if (xData[midPos] < valueToFind)      //中间点小于valutToFind，在前面查找
                {
                    return FindNearestPosition(xData, beginx, midPos, valueToFind);
                }
                else //中间点大于valutToFind，在后面查找
                {
                    return FindNearestPosition(xData, midPos, endx, valueToFind);
                }
            }
        }

        /// <summary>
        /// 计算光谱的RMS
        /// </summary>
        /// <param name="xData">X轴数据</param>
        /// <param name="yData">Y轴数据</param>
        /// <param name="freqStart">RMS起始X值</param>
        /// <param name="freqEnd">RMS结束X值</param>
        /// <returns>RMS值</returns>
        public static double CalculateRMS(double[] xData, double[] yData, double freqStart, double freqEnd)
        {
            int beginIndex = FindNearestPosition(xData, 0, xData.Length - 1, freqStart);
            int endIndex = FindNearestPosition(xData, 0, xData.Length - 1, freqEnd);

            //xData范围内没有找到freqStart或freqEnd
            if (beginIndex == -1 || endIndex == -1)
                return 0;

            SortInOrder(ref beginIndex, ref endIndex);

            double[] x = new double[endIndex - beginIndex];
            double[] y = new double[endIndex - beginIndex];
            for (int i = beginIndex; i < endIndex; i++)
            {
                x[i - beginIndex] = xData[i];
                y[i - beginIndex] = yData[i];
            }

            int info;
            alglib.spline1dinterpolant s;
            alglib.spline1dfitreport rep;
            double rho = 5;

            alglib.spline1dfitpenalized(x, y, 50, rho, out info, out s, out rep);

            //计算平均值
            double argY = 0;
            for (int i = 0; i < y.Length; i++)
            {
                double newy = alglib.spline1dcalc(s, x[i]);
                y[i] -= newy;
                argY += Math.Abs(y[i]);
            }
            argY = argY / y.Length;

            //计算平方和
            double sumY = 0;
            for (int i = 0; i < y.Length; i++)
                sumY += (y[i] - argY) * (y[i] - argY);

            double rms = Math.Sqrt(sumY / y.Length);

            return rms;
        }


        /// <summary>
        /// 计算光谱的RMS
        /// </summary>
        /// <param name="xData">X轴数据</param>
        /// <param name="yData">Y轴数据</param>
        /// <param name="freqStart">RMS起始X值</param>
        /// <param name="freqEnd">RMS结束X值</param>
        /// <returns>RMS值</returns>
        public static double CalculateSpectrumRMS(double[] xData, double[] yData, double freqStart, double freqEnd)
        {
            int beginIndex = FindNearestPosition(xData, 0, xData.Length - 1, freqStart);
            int endIndex = FindNearestPosition(xData, 0, xData.Length - 1, freqEnd);

            //xData范围内没有找到freqStart或freqEnd
            if (beginIndex == -1 || endIndex == -1)
                return 0;

            SortInOrder(ref beginIndex, ref endIndex);

            double[] x = new double[endIndex - beginIndex];
            double[] y = new double[endIndex - beginIndex];
            for (int i = beginIndex; i < endIndex; i++)
            {
                x[i - beginIndex] = xData[i];
                y[i - beginIndex] = yData[i];
            }

            int info;
            alglib.spline1dinterpolant s;
            alglib.spline1dfitreport rep;
            double rho = 5;

            int points = (endIndex - beginIndex) < 100 ? endIndex - beginIndex : 100;
            alglib.spline1dfitpenalized(x, y, points, rho, out info, out s, out rep);

            //计算平方和
            double sumY = 0;
            for (int i = 0; i < y.Length; i++)
            {
                double newy = alglib.spline1dcalc(s, x[i]);     //拟合值
                sumY += (y[i]-newy) * (y[i]-newy);
            }

            double rms = Math.Sqrt(sumY / y.Length);

            return rms;
        }

        /// <summary>
        /// 标定峰位
        /// </summary>
        /// <param name="xData">X轴数据</param>
        /// <param name="yData">Y轴数据</param>
        /// <param name="peakValue">要标定的峰位</param>
        /// <param name="pointsToCal">计算的点</param>
        /// <param name="newyvalue">返回峰位的峰高</param>
        /// <param name="isUpPeak">是否向上的峰</param>
        /// <returns>找到的峰位</returns>
        public static double PickPeak(double[] xData, double[] yData, double peakValue, int pointsToCal, bool isUpPeak, out double newyvalue)
        {
            //找到要标记的峰在xData中的最近位置
            newyvalue = 0;
            int foundpos = FindNearestPosition(xData, 0, xData.Length - 1, peakValue);
            if (foundpos == -1)  //没找到
                return -1;

            if (isUpPeak)    //正峰（峰在上面)
            {
                //查找该位置最近的峰
                double maxValue = yData[foundpos];
                if (foundpos > 0)
                {
                    if (yData[foundpos - 1] >= maxValue)    //下降曲线，峰在左边
                    {
                        while (foundpos > 0 && yData[foundpos - 1] > yData[foundpos])
                            foundpos--;
                    }
                    else    //上升曲线，峰在右边
                    {
                        while (foundpos < yData.Length - 1 && yData[foundpos + 1] > yData[foundpos])
                            foundpos++;
                    }
                }

                //foundpos是当前的峰位
                //现在要找峰谷
                int leftpos = foundpos, rightpos = foundpos;
                for (int i = foundpos; i > 1; i--)
                {
                    if (yData[i] < yData[i - 1])
                        break;
                    leftpos--;
                }
                for (int i = foundpos; i < yData.Length - 1; i++)
                {
                    if (yData[i] < yData[i + 1])
                        break;
                    rightpos++;
                }

                //调整左右必须在光谱区间内
                if (foundpos - leftpos > pointsToCal)
                    leftpos = foundpos - pointsToCal;
                if (leftpos < 0)
                    leftpos = 0;

                if (rightpos - foundpos > pointsToCal)
                    rightpos = foundpos + pointsToCal;
                if (rightpos > yData.Length - 1)
                    rightpos = yData.Length - 1;

                //取左右各pointsToCal个点来做曲线拟合
                double[] x = new double[rightpos - leftpos + 1];
                double[] y = new double[rightpos - leftpos + 1];
                for (int i = leftpos; i <= rightpos; i++)
                {
                    x[i - leftpos] = xData[i];
                    y[i - leftpos] = yData[i];
                }

                alglib.spline1dinterpolant c;
                alglib.spline1dbuildcubic(x, y, out c);

                //计算拟合后的Y值
                for (int i = 0; i < x.Length; i++)
                {
                    y[i] = alglib.spline1dcalc(c, x[i]);
                }

                //查找Y的最大值
                foundpos = 0;
                double maxy = y[0];
                for (int i = 1; i < y.Length; i++)
                {
                    if (y[i] > maxy)
                    {
                        foundpos = i;
                        maxy = y[i];
                    }
                }

                double stepx = (x[1] - x[0]) / 100;   //按照X最小间隔的1/100来逐步逼近
                maxy = y[foundpos];
                double maxx = x[foundpos];

                //先查左边
                double cury = alglib.spline1dcalc(c, x[foundpos] - stepx);
                if (cury > maxy)   //左边的Y值大于最大Y值，因此最大Y值还在左边
                    stepx *= -1;     //实际上每次计算要减小stepx

                for (int i = 1; i < 100; i++)
                {
                    cury = alglib.spline1dcalc(c, x[foundpos] + i * stepx);
                    if (cury > maxy)   //找到更大的Y值了
                    {
                        maxx = x[foundpos] + i * stepx;
                        maxy = cury;
                    }
                    else
                        break;
                }
                newyvalue = maxy;
                return maxx;
            }
            else    //倒峰(峰在下面)
            {
                //查找该位置最近的峰
                double maxValue = yData[foundpos];
                if (foundpos > 0)
                {
                    if (yData[foundpos - 1] <= maxValue)    //上升曲线，峰在左边
                    {
                        while (foundpos > 0 && yData[foundpos - 1] < yData[foundpos])
                            foundpos--;
                    }
                    else    //下降曲线，峰在右边
                    {
                        while (foundpos < yData.Length - 1 && yData[foundpos + 1] < yData[foundpos])
                            foundpos++;
                    }
                }

                //foundpos是当前的峰位
                //现在要找两边的峰顶
                int leftpos = foundpos, rightpos = foundpos;
                for (int i = foundpos; i > 1; i--)   //找左边的峰顶
                {
                    if (yData[i - 1] < yData[i])
                        break;
                    leftpos--;
                }
                for (int i = foundpos; i < yData.Length - 1; i++)   //找右边的峰顶
                {
                    if (yData[i + 1] < yData[i])
                        break;
                    rightpos++;
                }

                //调整左右必须在光谱区间内
                if (foundpos - leftpos > pointsToCal)
                    leftpos = foundpos - pointsToCal;
                if (leftpos < 0)
                    leftpos = 0;

                if (rightpos - foundpos > pointsToCal)
                    rightpos = foundpos + pointsToCal;
                if (rightpos > yData.Length - 1)
                    rightpos = yData.Length - 1;

                //取左右各pointsToCal个点来做曲线拟合
                double[] x = new double[rightpos - leftpos + 1];
                double[] y = new double[rightpos - leftpos + 1];
                for (int i = leftpos; i <= rightpos; i++)
                {
                    x[i - leftpos] = xData[i];
                    y[i - leftpos] = yData[i];
                }

                alglib.spline1dinterpolant c;
                alglib.spline1dbuildcubic(x, y, out c);

                //计算拟合后的Y值
                for (int i = 0; i < x.Length; i++)
                {
                    y[i] = alglib.spline1dcalc(c, x[i]);
                }

                //查找Y的最小值
                foundpos = 0;
                double maxy = y[0];
                for (int i = 1; i < y.Length; i++)
                {
                    if (y[i] < maxy)
                    {
                        foundpos = i;
                        maxy = y[i];
                    }
                }

                double stepx = (x[1] - x[0]) / 100;   //按照X最小间隔的1/100来逐步逼近
                maxy = y[foundpos];
                double maxx = x[foundpos];

                //先查左边, 如果最小值在左边，stepx为负数
                double cury = alglib.spline1dcalc(c, x[foundpos] - stepx);
                if (cury < maxy)   //左边的Y值小于最小Y值，因此最小Y值还在左边
                    stepx *= -1;     //实际上每次计算要减小stepx

                for (int i = 1; i < 100; i++)
                {
                    cury = alglib.spline1dcalc(c, x[foundpos] + i * stepx);
                    if (cury < maxy)   //找到更小的Y值了
                    {
                        maxx = x[foundpos] + i * stepx;
                        maxy = cury;
                    }
                    else
                        break;
                }
                newyvalue = maxy;
                return maxx;
            }
        }

        public static void GetRangeData(double[] inXData, double[] inYData, double firstX, double lastX, out double[] outXData, out double[] outYData)
        {
            int beginIndex = FindNearestPosition(inXData, 0, inXData.Length - 1, firstX);
            int endIndex = FindNearestPosition(inXData, 0, inXData.Length - 1, lastX);

            //xData范围内没有找到freqStart或freqEnd
            if (beginIndex == -1 || endIndex == -1)
            {
                outXData = null;
                outYData = null;
                return;
            }
            SortInOrder(ref beginIndex, ref endIndex);

            outXData = new double[endIndex - beginIndex];
            outYData = new double[endIndex - beginIndex];
            for (int i = beginIndex; i < endIndex; i++)
            {
                outXData[i - beginIndex] = inXData[i];
                outYData[i - beginIndex] = inYData[i];
            }            
        }

        #region 二元多次线性方程拟合曲线

        public static double PickPeakUseGussan(double[] xData, double[] yData, double peakValue, int pointsToCal, out double newyvalue)
        {
            //找到要标记的峰在xData中的最近位置
            int foundpos = FindNearestPosition(xData, 0, xData.Length - 1, peakValue);

            //查找该位置最近的峰
            double maxValue = yData[foundpos];
            if (yData[foundpos - 1] >= maxValue)    //下降曲线，峰在左边
            {
                while (foundpos > 0 && yData[foundpos - 1] > yData[foundpos])
                    foundpos--;
            }
            else    //上升曲线，峰在右边
            {
                while (foundpos < yData.Length - 1 && yData[foundpos + 1] > yData[foundpos])
                    foundpos++;
            }

            //取左右各pointsToCal个点来做曲线拟合
            double[] x = new double[pointsToCal * 2 + 1];
            double[] y = new double[pointsToCal * 2 + 1];
            for (int i = foundpos - pointsToCal; i <= foundpos + pointsToCal; i++)
            {
                x[i - (foundpos - pointsToCal)] = xData[i];
                y[i - (foundpos - pointsToCal)] = yData[i];
            }

            //计算曲线拟合参数
            double[] c = MultiLine(x, y, x.Length, 2);

            y = new double[pointsToCal * 2 + 1];
            //计算拟合后的Y值
            for (int i = 0; i < y.Length; i++)
            {
                y[i] = c[0];
                for (int j = 1; j < c.Length; j++)
                    y[i] += c[j] * Math.Pow(x[i], j);
            }

            //查找Y的最大值
            foundpos = 0;
            double maxy = y[0];
            for (int i = 1; i < y.Length; i++)
            {
                if (y[i] > maxy)
                {
                    foundpos = i;
                    maxy = y[i];
                }
            }

            double stepx = (x[1] - x[0]) / 100;   //按照X最小间隔的1/100来逐步逼近
            maxy = y[foundpos];
            double maxx = x[foundpos];

            //先查左边
            double cury = c[0];
            for (int j = 1; j < c.Length; j++)
                cury += c[j] * Math.Pow(x[foundpos] - stepx, j);

            if (cury > maxy)   //左边的Y值大于最大Y值，因此最大Y值还在左边
                stepx *= -1;     //实际上每次计算要减小stepx

            for (int i = 1; i < 100; i++)
            {
                //根据最小二乘法参数计算Y值
                cury = c[0];
                for (int j = 1; j < c.Length; j++)
                    cury += c[j] * Math.Pow(x[foundpos] + i * stepx, j);

                if (cury > maxy)   //找到更大的Y值了
                {
                    maxx = x[foundpos] + i * stepx;
                    maxy = cury;
                }
                else
                    break;
            }
            newyvalue = maxy;

            return maxx;
        }

        public static double IntegrateUseGussan(float[] xData, float[] yData, float freqStart, float freqEnd)
        {
            if (xData == null || yData == null || xData.Length == 0 || yData.Length == 0 || freqStart == freqEnd)
                return 0;

            float firstX = xData[0], lastX = xData[xData.Length - 1];

            //xData是否是递增的
            bool increase = lastX > firstX ? true : false;

            //起始和结束的顺序与xData相同
            if ((increase && freqStart > freqEnd) ||
                 (!increase && freqStart < freqEnd))
            {
                float temp = freqEnd;
                freqEnd = freqStart;
                freqStart = temp;
            }

            //判断积分起止位置是否在X数据范围内
            if (increase && (freqStart < firstX || freqEnd > lastX) ||
                !increase && (freqStart > firstX || freqEnd < lastX))
                return 0;

            //找到xData中的实际积分区间
            int beginIndex = 0, endIndex = 0;

            //查找freqStart
            for (int i = 0; i < xData.Length; i++)
            {
                if (increase && xData[i] > freqStart ||
                    !increase && xData[i] < freqStart)
                {
                    beginIndex = i;
                    break;
                }
            }

            //查找freqEnd
            for (int i = 0; i < xData.Length - 1; i++)
            {
                if (increase && xData[i + 1] > freqEnd ||
                    !increase && xData[i + 1] < freqEnd)
                {
                    endIndex = i;
                    break;
                }
            }

            //xData范围内没有找到freqStart或freqEnd
            if (beginIndex == 0 || endIndex == 0)
                return 0;

            //beginIndex和endIndex区间小于freqStart和freqEnd区间
            //计算freqStart位置的Y值
            float yStart = yData[beginIndex - 1] + (yData[beginIndex] - yData[beginIndex - 1]) *
                ((freqStart - xData[beginIndex - 1]) / (xData[beginIndex] - xData[beginIndex - 1]));

            //计算freqEnd位置的Y值
            float yEnd = yData[endIndex + 1] + (yData[endIndex] - yData[endIndex + 1]) *
                ((freqEnd - xData[endIndex + 1]) / (xData[endIndex] - xData[endIndex + 1]));

            //beginIndex到endIndex之间的面积
            double retvalue = 0;
            for (int i = beginIndex + 1; i <= endIndex; i++)
            {
                //计算xData[i]位置基线的Y值
                //float cury = yStart + (yEnd - yStart) * ((xData[i] - freqStart) / (freqEnd - freqStart));
                //if (yData[i] > cury)    //Y值在基线上方
                //    retvalue += (xData[i] - xData[i - 1]) * (yData[i] + yData[i - 1]) / 2;
                //else    //Y值在基线下方, 模拟反转到基线上方
                //    retvalue += (xData[i] - xData[i - 1]) * (cury + (cury - yData[i]) + cury + (cury - yData[i - 1])) / 2;

                retvalue += (xData[i] - xData[i - 1]) * (yData[i] + yData[i - 1]) / 2;

            }

            //计算beginIndex与freqStart之间的面积
            retvalue += Math.Abs((xData[beginIndex] - freqStart) * (yData[beginIndex] + yStart) / 2);

            //计算endIndex与freqEnd之间的面积
            retvalue += Math.Abs((freqEnd - xData[endIndex]) * (yEnd + yData[endIndex]) / 2);

            //扣除freqStart到freqEnd连接直线下方的面积
            retvalue -= Math.Abs((freqEnd - freqStart) * (yEnd + yStart) / 2);

            return retvalue;
        }


        ///<summary>
        ///用最小二乘法拟合二元多次曲线
        ///</summary>
        ///<param name="arrX">已知点的x坐标集合</param>
        ///<param name="arrY">已知点的y坐标集合</param>
        ///<param name="length">已知点的个数</param>
        ///<param name="dimension">方程的最高次数</param>
        public static double[] MultiLine(double[] arrX, double[] arrY, int length, int dimension)//二元多次线性方程拟合曲线
        {
            int n = dimension + 1;                  //dimension次方程需要求 dimension+1个 系数
            double[,] Guass = new double[n, n + 1];      //高斯矩阵 例如：y=a0+a1*x+a2*x*x
            for (int i = 0; i < n; i++)
            {
                int j;
                for (j = 0; j < n; j++)
                {
                    Guass[i, j] = SumArr(arrX, j + i, length);
                }
                Guass[i, j] = SumArr(arrX, i, arrY, 1, length);
            }
            return ComputGauss(Guass, n);

        }
        public static double SumArr(double[] arr, int n, int length) //求数组的元素的n次方的和
        {
            double s = 0;
            for (int i = 0; i < length; i++)
            {
                if (arr[i] != 0 || n != 0)
                    s = s + Math.Pow(arr[i], n);
                else
                    s = s + 1;
            }
            return s;
        }
        public static double SumArr(double[] arr1, int n1, double[] arr2, int n2, int length)
        {
            double s = 0;
            for (int i = 0; i < length; i++)
            {
                if ((arr1[i] != 0 || n1 != 0) && (arr2[i] != 0 || n2 != 0))
                    s = s + Math.Pow(arr1[i], n1) * Math.Pow(arr2[i], n2);
                else
                    s = s + 1;
            }
            return s;

        }
        //求解高斯方程
        public static double[] ComputGauss(double[,] Guass, int n)
        {
            int i, j;
            int k, m;
            double temp;
            double max;
            double s;
            double[] x = new double[n];

            for (i = 0; i < n; i++)
                x[i] = 0.0;//初始化
            for (j = 0; j < n; j++)
            {
                max = 0;    /*max不能为零*/

                k = j;      /*默认gauss[k=j][j]在地j列中最大,用k标记首个最大行*/
                for (i = j; i < n; i++)
                {
                    if (Math.Abs(Guass[i, j]) > max)
                    {
                        max = Guass[i, j];
                        k = i;
                    }
                }

                if (k != j) /*调换*/
                {
                    for (m = j; m < n + 1; m++)
                    {
                        temp = Guass[j, m];
                        Guass[j, m] = Guass[k, m];
                        Guass[k, m] = temp;

                    }
                }

                if (0 == max)
                {
                    // "此线性方程为奇异线性方程" 

                    return x;
                }


                for (i = j + 1; i < n; i++) /*对j+1行到第n行进行消元*/
                {

                    s = Guass[i, j];
                    for (m = j; m < n + 1; m++)
                    {
                        Guass[i, m] = Guass[i, m] - Guass[j, m] * s / (Guass[j, j]);

                    }
                }
            }//结束for (j=0;j<n;j++)

            for (i = n - 1; i >= 0; i--)    /*回代*/
            {
                s = 0;
                for (j = i + 1; j < n; j++)
                {
                    s = s + Guass[i, j] * x[j];
                }

                x[i] = (Guass[i, n] - s) / Guass[i, i];

            }

            return x;
        }//返回值是函数的系数
        #endregion

    }

}