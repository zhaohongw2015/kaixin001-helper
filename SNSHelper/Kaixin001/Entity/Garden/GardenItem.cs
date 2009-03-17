using System;
using System.Collections.Generic;
using System.Text;

namespace SNSHelper.Kaixin001.Entity.Garden
{
    public class GardenItem
    {
        private int water = 0;
        /// <summary>
        /// 浇水情况
        /// </summary>
        public int Water
        {
            get
            {
                return water;
            }
            set
            {
                water = value;
            }
        }

        private string farmNum = string.Empty;
        /// <summary>
        /// 编号
        /// </summary>
        public string FarmNum
        {
            get
            {
                return farmNum;
            }
            set
            {
                farmNum = value;
            }
        }

        private int vermin = 0;
        /// <summary>
        /// 害虫数
        /// </summary>
        public int Vermin
        {
            get
            {
                return vermin;
            }
            set
            {
                vermin = value;
            }
        }

        private string cropsId = string.Empty;
        /// <summary>
        /// 农作物编号
        /// </summary>
        public string CropsId
        {
            get
            {
                return cropsId;
            }
            set
            {
                cropsId = value;
            }
        }

        private string fuId = string.Empty;
        /// <summary>
        /// 好友编号
        /// </summary>
        public string FUId
        {
            get
            {
                return fuId;
            }
            set
            {
                fuId = value;
            }
        }

        private string status = string.Empty;
        /// <summary>
        /// 状态（0：可种；1：不可种）（不太确定）
        /// </summary>
        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
            }
        }

        private string shared = string.Empty;
        /// <summary>
        /// 好友种植状态（0：私家地；1：爱心地；2：爱心地已成熟？）
        /// </summary>
        public string Shared
        {
            get
            {
                return shared;
            }
            set
            {
                shared = value;
            }
        }

        private string grass = string.Empty;
        public string Grass
        {
            get
            {
                return grass;
            }
            set
            {
                grass = value;
            }
        }
        #region 以下属性仅当种有农作物时有效

        private string pic = string.Empty;
        /// <summary>
        /// 未知属性
        /// </summary>
        public string Pic
        {
            get
            {
                return pic;
            }
            set
            {
                pic = value;
            }
        }

        private string fruitPic = string.Empty;
        /// <summary>
        /// 果实Flash路径
        /// </summary>
        public string FruitPic
        {
            get
            {
                return fruitPic;
            }
            set
            {
                fruitPic = value;
            }
        }

        private string picWidth = string.Empty;
        /// <summary>
        /// 图像宽度
        /// </summary>
        public string PicWidth
        {
            get
            {
                return picWidth;
            }
            set
            {
                picWidth = value;
            }
        }

        private string picHeight = string.Empty;
        /// <summary>
        /// 图像高度
        /// </summary>
        public string PicHeight
        {
            get
            {
                return picHeight;
            }
            set
            {
                picHeight = value;
            }
        }

        private string cropsStatus = string.Empty;
        /// <summary>
        /// 农作物状态（1：生长中；2：成熟；3：已收获）
        /// </summary>
        public string CropsStatus
        {
            get
            {
                return cropsStatus;
            }
            set
            {
                cropsStatus = value;
            }
        }

        private string grow = string.Empty;
        /// <summary>
        /// 生长程度（待定）
        /// </summary>
        public string Grow
        {
            get
            {
                return grow;
            }
            set
            {
                grow = value;
            }
        }

        private string totalGrow = string.Empty;
        /// <summary>
        /// 未知属性
        /// </summary>
        public string TotalGrow
        {
            get
            {
                return totalGrow;
            }
            set
            {
                totalGrow = value;
            }
        }

        private string fruitNum = string.Empty;
        /// <summary>
        /// 果实数量
        /// </summary>
        public string FruitNum
        {
            get
            {
                return fruitNum;
            }
            set
            {
                fruitNum = value;
            }
        }

        private string seedId = string.Empty;
        /// <summary>
        /// 果实编号
        /// </summary>
        public string SeedId
        {
            get
            {
                return seedId;
            }
            set
            {
                seedId = value;
            }
        }

        private string crops = string.Empty;
        /// <summary>
        /// 农作物情况
        /// </summary>
        public string Crops
        {
            get
            {
                return crops;
            }
            set
            {
                crops = value;
            }
        }

        private string farm = string.Empty;
        /// <summary>
        /// 其他
        /// </summary>
        public string Farm
        {
            get
            {
                return farm;
            }
            set
            {
                farm = value;
            }
        }
        #endregion
    }
}
