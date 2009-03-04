using System;
using System.Collections.Generic;
using System.Text;

namespace SNSHelper.Common
{
    public class JsonHelper
    {
        public static string GetJsonStringValue(Newtonsoft.Json.JavaScriptObject jso, string key)
        {
            try
            {
                return jso[key] == null ? string.Empty : jso[key].ToString();
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        public static string InitJsonString(string input)
        {
            return input.Replace("\\/", "/").Replace("\\u", "\\\\u");
        }
    }
}
