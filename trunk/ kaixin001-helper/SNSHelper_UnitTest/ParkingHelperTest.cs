using SNSHelper.Kaixin001;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SNSHelper.Kaixin001.Entity;
using System.Collections.Generic;

namespace SNSHelper_UnitTest
{


    /// <summary>
    ///这是 ParkingHelperTest 的测试类，旨在
    ///包含所有 ParkingHelperTest 单元测试
    ///</summary>
    [TestClass()]
    public class ParkingHelperTest
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
        ///GetFirendParkingInfo 的测试
        ///</summary>
        [TestMethod()]
        public void GetFirendParkingInfoTest()
        {
            string loginEmail = UnitTestHelper.LoginEmail;
            string loginPassword = UnitTestHelper.LoginPassword;

            Utility.Login(loginEmail, loginPassword);

            ParkingHelper target = new ParkingHelper();

            string friendUId = "12580789";
            List<ParkingInfo> actual;
            actual = target.GetFirendParkingInfo(friendUId);
        }

        /// <summary>
        ///ParkOneCar 的测试（在该测试中，将把玩家的第N辆车停到第M个好友的第X个私家车库中）
        ///</summary>
        [TestMethod()]
        public void ParkOneCarTest()
        {
            string loginEmail = UnitTestHelper.LoginEmail;
            string loginPassword = UnitTestHelper.LoginPassword;

            Utility.Login(loginEmail, loginPassword);

            ParkingHelper helper = new ParkingHelper();

            CarInfo carInfo = helper.CarList[2]; // 第N辆车
            ParkerFriendInfo parkerFriendInfo = helper.ParkerFriendList[5]; // 第M个好友

            List<ParkingInfo> parkingInfoList = helper.GetFirendParkingInfo(parkerFriendInfo.UId);
            ParkingInfo parkingInfo = parkingInfoList[0];   // 第X个私家车位

            ParkResult result = helper.ParkOneCar(parkerFriendInfo, carInfo, parkingInfo);
            Console.WriteLine(result.Error);
            Assert.AreEqual<string>("0", result.ErrNo);
        }

        /// <summary>
        ///PostOneCar 的测试(对第N个车位进行贴条操作)
        ///</summary>
        [TestMethod()]
        public void PostOneCarTest()
        {
            string loginEmail = UnitTestHelper.LoginEmail;
            string loginPassword = UnitTestHelper.LoginPassword;

            Utility.Login(loginEmail, loginPassword);

            ParkingHelper helper = new ParkingHelper();

            PostResult result = helper.PostOneCar(helper.ParkingList[3]);   // 对第N个车位进行贴条操作
            Assert.AreEqual<string>("0", result.ErrNo);
            Console.WriteLine(result.Error);
        }

        /// <summary>
        ///GetParkerDetails 的测试
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SNSHelper.dll")]
        public void GetParkerDetailsTest()
        {
            ParkingHelper_Accessor target = new ParkingHelper_Accessor(); // TODO: 初始化为适当的值

            string loginEmail = UnitTestHelper.LoginEmail;
            string loginPassword = UnitTestHelper.LoginPassword;

            Utility.Login(loginEmail, loginPassword);

            target.AppID = "1040";
            target.GetParkerDetails();

        }
    }
}
