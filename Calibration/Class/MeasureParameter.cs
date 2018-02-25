using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NirIdentifier.Calibration
{
    class MeasureParameter
    {
        /// <summary>
        /// 分辨率
        /// </summary>
        public string resolution { get; set; }

        /// <summary>
        /// 扫描次数
        /// </summary>
        public string scans { get; set; }

        /// <summary>
        /// 增益
        /// </summary>
        public string gain { get; set; }

        /// <summary>
        /// 填零
        /// </summary>
        public string zeroFill { get; set; }

        /// <summary>
        /// 相位校正
        /// </summary>
        public string phaseCorrection { get; set; }

        /// <summary>
        /// 去趾函数
        /// </summary>
        public string apodization { get; set; }

        /// <summary>
        /// 扫描速度
        /// </summary>
        public string velocity { get; set; }
    }
}
