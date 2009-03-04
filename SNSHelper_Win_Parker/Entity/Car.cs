
namespace SNSHelper_Win_Garden.Entity
{
    public class Car
    {
        private string id = string.Empty;
        /// <summary>
        /// 编号
        /// </summary>
        public string ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

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

        private int price = 0;
        /// <summary>
        /// 单价
        /// </summary>
        public int Price
        {
            get
            {
                return price;
            }
            set
            {
                price = value;
            }
        }
    }
}
