
namespace SNSHelper_Win_Garden.Entity
{
    class GlobalSetting
    {
        private int workingInterval = 0;
        /// <summary>
        /// 工作周期
        /// </summary>
        public int WorkingInterval
        {
            get
            {
                return workingInterval;
            }
            set
            {
                workingInterval = value;
            }
        }

        private int networkDelay = 0;
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
