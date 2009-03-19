
// 感谢网友“我爱我家”提供加解密算法
namespace SNSHelper.Common
{
    public class EncryptHelper
    {
        static int axexso = 0;
        static int loco21 = 0;
        static int idkz29 = 0;
        static int polkc1 = 0;
        static string yie291 = "";
        static string dkow1 = "";
        static string cncmdo = "";
        static string cnmwol = "";
        static string aqqown = "";
        static string ghkod = "";
        static string[] okcoa = new string[98];

        static string key = "abc";// 密钥

        static EncryptHelper()
        {
            int W;
            int X;

            yie291 = @"PQRjrAC0-.#/\!@bEF9t;:, 124$<>&uvOWD38xcdfpGs7XYZ`^|[+~?=Nklmnoq%wyzHIJKL5Bea6M]{}STUVghi'*()_";

            yie291 = yie291 + "\t\v" + "\"";

            W = 1;
            idkz29 = yie291.Length;
            okcoa[1] = yie291;

            for (X = 2; X <= idkz29; X++)
            {
                dkow1 = okcoa[W].Substring(0, 1);
                cncmdo = okcoa[W].Substring(1);
                okcoa[X] = cncmdo + dkow1;
                W += 1;
            }
        }

        public static string Encrypt(string input)
        {
            int X;
            int Y;
            int z;
            string idkw;
            string lsowc;

            if (input.Trim().Length == 0) input = key;

            lsowc = input;
            loco21 = input.Length;
            axexso = key.Length;
            ghkod = "";
            aqqown = "";

            Y = 0;
            for (X = 0; X < loco21; X++)
            {
                idkw = lsowc.Substring(X, 1);
                polkc1 = yie291.IndexOf(idkw);
                cnmwol = key.Substring(Y, 1);
                for (z = 1; z <= idkz29; z++)
                {
                    if (okcoa[z].Substring(polkc1, 1) == cnmwol)
                    {
                        ghkod = okcoa[z].Substring(0, 1);
                        aqqown = aqqown + ghkod;
                        continue;
                    }
                }
                Y += 1;
                if (Y >= axexso) Y = 0;
            }

            return aqqown;
        }
    }
}
