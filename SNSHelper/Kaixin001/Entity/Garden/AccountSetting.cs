using System;
using System.Collections.Generic;
using System.Text;

namespace SNSHelper.Kaixin001.Entity.Garden
{
    public class AccountSetting
    {
        private string water = string.Empty;
        /// <summary>
        /// 好友帮我浇水时，我会说：
        /// </summary>
        public string Water
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

        private string vermin = string.Empty;
        /// <summary>
        /// 好友帮我来捉虫时，我会说：
        /// </summary>
        public string Vermin
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

        private string steal = string.Empty;
        /// <summary>
        /// 好友来偷我果实时，我会说：
        /// </summary>
        public string Steal
        {
            get
            {
                return steal;
            }
            set
            {
                steal = value;
            }
        }

        private string farm = string.Empty;
        /// <summary>
        /// 好友来我家种地时，我会说：
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
    }
}
