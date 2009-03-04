using System;
using System.Collections.Generic;
using System.Text;

namespace SNSHelper.Kaixin001.Entity.Garden
{
    public class FruitItem
    {
        private string name = string.Empty;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        private string fruitPic = string.Empty;
        /// <summary>
        /// 图片路径
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

        private string seedId = string.Empty;
        /// <summary>
        /// 种子编号
        /// </summary>
        public string SeedID
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

        private string num = string.Empty;
        /// <summary>
        /// 果实数
        /// </summary>
        public string Num
        {
            get
            {
                return num;
            }
            set
            {
                num = value;
            }
        }
    }
}
