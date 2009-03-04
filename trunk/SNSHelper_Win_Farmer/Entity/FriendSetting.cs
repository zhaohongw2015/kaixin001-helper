
namespace SNSHelper_Win_Garden.Entity
{
    class FriendSetting
    {
        private string uid = string.Empty;
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

        private string name = string.Empty;
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

        private bool plough = false;
        public bool Plough
        {
            get
            {
                return plough;
            }
            set
            {
                plough = value;
            }
        }

        private bool farm = false;
        public bool Farm
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

        private bool water = false;
        public bool Water
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

        private bool vermin = false;
        public bool Vermin
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

        private bool steal = false;
        public bool Steal
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

        private bool grass = false;
        public bool Grass
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
    }
}
