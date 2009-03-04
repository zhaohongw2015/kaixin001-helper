
namespace SNSHelper.Kaixin001.Entity
{
    public class AppHelperBase
    {
        /// <summary>
        /// 应用程序编号
        /// </summary>
        public string AppID;

        /// <summary>
        /// 应用程序URL
        /// </summary>
        public string AppUrl;

        public AppHelperBase()
        {
            AppID = string.Empty;
            AppUrl = "http://www.kaixin001.com/app/app.php?aid=";
        }
    }
}
