using System.Net;
using SNSHelper.Common;

namespace SNSHelper.Kaixin001
{
    public static class Utility
    {
        private static HttpHelper httpHelper = new HttpHelper();
        private static CookieContainer cookieContainer = new CookieContainer();
        /// <summary>
        /// Cookie
        /// </summary>
        public static CookieContainer Cookies
        {
            get
            {
                return cookieContainer;
            }
            set
            {
                cookieContainer = value;
            }
        }

        private static bool isLogin = false;
        /// <summary>
        /// 用户是否登录
        /// </summary>
        public static bool IsLogin
        {
            get
            {
                return isLogin;
            }
        }

        public static int NetworkDelay
        {
            get
            {
                return httpHelper.NetworkDelay;
            }
            set
            {
                httpHelper.NetworkDelay = value;
            }
        }

        /// <summary>
        /// 登录开心网
        /// </summary>
        /// <param name="loginEmail">Email</param>
        /// <param name="loginPassword">密码</param>
        /// <returns></returns>
        public static bool Login(string loginEmail, string loginPassword)
        {
            string loginUrl = "http://www.kaixin001.com/login/login.php";
            string postData = string.Format("url=/home/&invisible_mode=0&email={0}&password={1}", loginEmail, loginPassword);
            string result = httpHelper.GetHtml(loginUrl, postData, true, cookieContainer);

            return isLogin = result.Contains("我的首页");
        }

        public static void Logout()
        {
            isLogin = false;
            string logoutUrl = "http://www.kaixin001.com/login/logout.php";
            httpHelper.GetHtml(logoutUrl);
        }
    }
}
