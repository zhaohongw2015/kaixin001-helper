
namespace SNSHelper_Win.Entity
{
    public class FriendSetting
    {
        public FriendSetting()
        {
        }

        public FriendSetting(string nickName, string uid)
        {
            this.nickName = nickName;
            this.uid = uid;
            allowedPark = true;
            allowedPost = true;
            parkPriority = int.MaxValue;
        }

        string uid = string.Empty;
        public string UID
        {
            get
            {
                return uid;
            }
            set
            {
                uid = value;
            }
        }

        string nickName = string.Empty;
       /// <summary>
       /// 好友昵称
       /// </summary>
        public string NickName
        {
            get
            {
                return nickName;
            }
            set
            {
                nickName = value;
            }
        }

        bool allowedPark = false;
        /// <summary>
        /// 是否把车停在好友车位上
        /// </summary>
        public bool AllowedPark
        {
            get
            {
                return allowedPark;
            }
            set
            {
                allowedPark = value;
            }
        }

        bool allowedPost = false;
        /// <summary>
        /// 对好友停在车位上的车是否贴条
        /// </summary>
        public bool AllowedPost
        {
            get
            {
                return allowedPost;
            }
            set
            {
                allowedPost = value;
            }
        }

        int parkPriority = int.MaxValue;
        /// <summary>
        /// 停车优先级
        /// </summary>
        public int ParkPriority
        {
            get
            {
                return parkPriority;
            }
            set
            {
                parkPriority = value;
            }
        }

        bool isFull = false;
        public bool IsFull
        {
            get
            {
                return isFull;
            }
            set
            {
                isFull = value;
            }
        }
    }
}
