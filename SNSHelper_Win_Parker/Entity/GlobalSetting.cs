
namespace SNSHelper_Win_Garden.Entity
{
    public class GlobalSetting
    {
        int parkInterval = 0;
        /// <summary>
        /// 停车周期
        /// </summary>
        public int ParkInterval
        {
            get
            {
                return parkInterval;
            }
            set
            {
                parkInterval = value;
            }
        }

        int networkDelay = 0;
        /// <summary>
        /// 网络延迟
        /// </summary>
        public int NetworkDelay
        {
            get
            {
                return networkDelay;
            }
            set
            {
                networkDelay = value;
            }
        }
    }
}
