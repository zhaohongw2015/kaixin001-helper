using System;
using System.Collections.Generic;
using System.Text;

namespace SNSHelper_Win_Garden.Entity
{
    public class SeedMVP
    {
        private string rank;
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

        private string seedName;
        public string SeedName
        {
            get
            {
                return seedName;
            }
            set
            {
                seedName = value;
            }
        }
    }
}
