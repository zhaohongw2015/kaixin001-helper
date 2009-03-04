using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using SNSHelper.Common;
using System.Configuration;

namespace SNSHelper_UnitTest
{
    
    
    /// <summary>
    ///这是 HttpHelperTest 的测试类，旨在
    ///包含所有 HttpHelperTest 单元测试
    ///</summary>
    [TestClass()]
    public class HttpHelperTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 附加测试属性
        // 
        //编写测试时，还可使用以下属性:
        //
        //使用 ClassInitialize 在运行类中的第一个测试前先运行代码
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //使用 ClassCleanup 在运行完类中的所有测试后再运行代码
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //使用 TestInitialize 在运行每个测试前先运行代码
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //使用 TestCleanup 在运行完每个测试后运行代码
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///GetHtml 的测试
        ///</summary>
        [TestMethod()]
        public void GetHtmlTest()
        {
            HttpHelper target = new HttpHelper();
            string url = "http://www.kaixin001.com/login/login.php";
            string postData = string.Format("url=/home/&invisible_mode=0&email={0}.com&password={1}",UnitTestHelper.LoginEmail, UnitTestHelper.LoginPassword);
            bool isPost = true;
            CookieContainer cookieContainer = new CookieContainer();
            string expected = string.Empty;
            string actual;
            actual = target.GetHtml(url, postData, isPost, cookieContainer);
        }
    }
}
