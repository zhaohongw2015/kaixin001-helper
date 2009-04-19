
namespace SNSHelper_Win_Parker.Entity
{
    public class AdvanceSettings
    {
        private bool autoUpdateFreePark = false;
        /// <summary>
        /// 是否自动升级免费车位
        /// </summary>
        public bool AutoUpdateFreePark
        {
            get
            {
                return autoUpdateFreePark;
            }
            set
            {
                autoUpdateFreePark = value;
            }
        }

        private bool autoBuyCar = false;
        /// <summary>
        /// 是否自动买车
        /// </summary>
        public bool AutoBuyCar
        {
            get
            {
                return autoBuyCar;
            }
            set
            {
                autoBuyCar = value;
            }
        }

        private int maxCarNo = 0;
        /// <summary>
        /// 最大车数
        /// </summary>
        public int MaxCarNo
        {
            get
            {
                return maxCarNo;
            }
            set
            {
                maxCarNo = value;
            }
        }

        private bool autoUpdateCar = false;
        /// <summary>
        /// 是否自动升级车
        /// </summary>
        public bool AutoUpdateCar
        {
            get
            {
                return autoUpdateCar;
            }
            set
            {
                autoUpdateCar = value;
            }
        }

        private string autoUpdateCarType = "先升级贵的";
        /// <summary>
        /// 自动升级车的类型
        /// </summary>
        public string AutoUpdateCarType
        {
            get
            {
                return autoUpdateCarType;
            }
            set
            {
                autoUpdateCarType = value;
            }
        }

        private int color = 0;
        /// <summary>
        /// 颜色
        /// </summary>
        public int Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }
    }
}
