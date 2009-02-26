using System.Collections.Generic;

namespace SNSHelper_Win.Entity
{
    public class AccountSetting
    {
        string loginEmail = string.Empty;
        /// <summary>
        /// 登陆Email
        /// </summary>
        public string LoginEmail
        {
            get
            {
                return loginEmail;
            }
            set
            {
                loginEmail = value;
            }
        }

        string loginPwd = string.Empty;
        /// <summary>
        /// 登录密码
        /// </summary>
        public string LoginPwd
        {
            get
            {
                return loginPwd;
            }
            set
            {
                loginPwd = value;
            }
        }

        bool isOperation = false;
        /// <summary>
        /// 是否操作
        /// </summary>
        public bool IsOperation
        {
            get
            {
                return isOperation;
            }
            set
            {
                isOperation = value;
            }
        }

        string masterAccount = string.Empty;
        /// <summary>
        /// 主号
        /// </summary>
        public string MasterAccount
        {
            get
            {
                return masterAccount;
            }
            set
            {
                masterAccount = value;
            }
        }

        int minPark = 0;
        /// <summary>
        /// 最低停车额度
        /// </summary>
        public int MinPark
        {
            get
            {
                return minPark;
            }
            set
            {
                minPark = value;
            }
        }

        int minPost = 0;
        /// <summary>
        /// 最低贴条额度
        /// </summary>
        public int MinPost
        {
            get
            {
                return minPost;
            }
            set
            {
                minPost = value;
            }
        }

        List<FriendSetting> friendSettingList = new List<FriendSetting>();
        /// <summary>
        /// 好友设置
        /// </summary>
        public List<FriendSetting> FriendSettings
        {
            get
            {
                return friendSettingList;
            }
            set
            {
                friendSettingList = value;
            }
        }

        List<FriendSetting> canParkFriendList;
        /// <summary>
        /// 能停车的好友名
        /// </summary>
        public List<FriendSetting> CanParkFriendList
        {
            get
            {
                if (canParkFriendList == null)
                {
                    canParkFriendList = new List<FriendSetting>();
                    for (int i = 0; i < friendSettingList.Count; i++)
                    {
                        if (friendSettingList[i].AllowedPark)
                        {
                            canParkFriendList.Add(friendSettingList[i]);
                        }
                    }
                }

                return canParkFriendList;
            }
            set
            {
                canParkFriendList = value;
            }
        }

        /// <summary>
        /// 获取好友
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public FriendSetting GetFriendSetting(string uid)
        {
            for (int i = 0; i < friendSettingList.Count; i++)
            {
                if (friendSettingList[i].UID.Equals(uid))
                {
                    return friendSettingList[i];
                }
            }

            return null;
        }
    }
}
