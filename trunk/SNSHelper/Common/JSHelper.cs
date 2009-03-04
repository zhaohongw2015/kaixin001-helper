
namespace SNSHelper.Common
{
    public class JSHelper
    {
        /// <summary>
        /// 执行JScript脚本
        /// </summary>
        /// <param name="js">JScript脚本</param>
        /// <returns>JScript脚本执行结果</returns>
        public static string EvalJavascript(string js)
        {
            Microsoft.JScript.Vsa.VsaEngine ve = Microsoft.JScript.Vsa.VsaEngine.CreateEngine();

            return Microsoft.JScript.Eval.JScriptEvaluate(js, ve).ToString();
        }

    }
}
