using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SNSHelper_UnitTest
{
    class UnitTestHelper
    {
        public static string LoginEmail = ConfigurationManager.AppSettings["LoginEmail"];
        public static string LoginPassword = ConfigurationManager.AppSettings["LoginPassword"];
    }
}
