using SNSHelper.Kaixin001;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SNSHelper.Common;
namespace SNSHelper_UnitTest
{
    
    
    /// <summary>
    ///这是 UtilityTest 的测试类，旨在
    ///包含所有 UtilityTest 单元测试
    ///</summary>
    [TestClass()]
    public class UtilityTest
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
        ///Login 的测试
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SNSHelper.dll")]
        public void LoginTest()
        {
            string loginEmail = UnitTestHelper.LoginEmail;
            string loginPassword = UnitTestHelper.LoginPassword;
            bool expected = true;
            bool actual;
            actual = Utility.Login(loginEmail, loginPassword);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Unicode2Character 的测试
        ///</summary>
        [TestMethod()]
        public void Unicode2CharacterTest()
        {
            string str = "4F60";
            string expected = "你";
            string actual;
            actual = ContentHelper.Unicode2Character(str);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///RunJS 的测试
        ///</summary>
        [TestMethod()]
        public void EvalJavascriptTest()
        {
            string js = "var gb58b = \"c48429010dbb\"; var acc3 = \"cbaktr0f1\"; function acc() { var acc = daa49.charCodeAt(1) + gb58b.length; return acc; } var daa49 = \"da5af2af139b\"; acc();";
            string expected = "109";
            string actual;
            actual = JSHelper.EvalJavascript(js);
            Assert.AreEqual(expected, actual);
        }
    }
}
