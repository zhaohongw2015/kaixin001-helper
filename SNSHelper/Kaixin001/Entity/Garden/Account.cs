using System;
using System.Collections.Generic;
using System.Text;

namespace SNSHelper.Kaixin001.Entity.Garden
{
    public class Account
    {
        private string rank = string.Empty;
        /// <summary>
        /// 技能等级
        /// </summary>
        public string Rank
        {
            get
            {
                return rank;
            }
            set
            {
                rank = value;
            }
        }

        private string rankTip = string.Empty;
        /// <summary>
        /// 技能等级
        /// </summary>
        public string RankTip
        {
            get
            {
                return rankTip;
            }
            set
            {
                rankTip = value;
            }
        }

        private string cash = string.Empty;
        /// <summary>
        /// 现金
        /// </summary>
        public string Cash
        {
            get
            {
                return cash;
            }
            set
            {
                cash = value;
            }
        }

        private string cashTip = string.Empty;
        /// <summary>
        /// 现金描述
        /// </summary>
        public string CashTip
        {
            get
            {
                return cashTip;
            }
            set
            {
                cashTip = value;
            }
        }

        private string name = string.Empty;
        /// <summary>
        /// 姓名
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

        private string logo50 = string.Empty;
        /// <summary>
        /// 头像路径
        /// </summary>
        public string Logo50
        {
            get
            {
                return logo50;
            }
            set
            {
                logo50 = value;
            }
        }

        private string bkswf = string.Empty;
        /// <summary>
        /// 花园背景Flash路径
        /// </summary>
        public string BkSwf
        {
            get
            {
                return bkswf;
            }
            set
            {
                bkswf = value;
            }
        }

        private string bFirst = string.Empty;
        /// <summary>
        /// 未知属性
        /// </summary>
        public string BFirst
        {
            get
            {
                return bFirst;
            }
            set
            {
                bFirst = value;
            }
        }

        private string tcharms = string.Empty;
        /// <summary>
        /// 魅力值
        /// </summary>
        public string TCharms
        {
            get
            {
                return tcharms;
            }
            set
            {
                tcharms = value;
            }
        }

        private string careUrl = string.Empty;
        public string CareUrl
        {
            get
            {
                return careUrl;
            }
            set
            {
                careUrl = value;
            }
        }

        private AccountSetting setting = new AccountSetting();
        /// <summary>
        /// 设置
        /// </summary>
        public AccountSetting Setting
        {
            get
            {
                return setting;
            }
            set
            {
                setting = value;
            }
        }
    }
}
