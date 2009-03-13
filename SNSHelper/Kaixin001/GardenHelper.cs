using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SNSHelper.Common;
using SNSHelper.Kaixin001.Entity;
using SNSHelper.Kaixin001.Entity.Garden;

namespace SNSHelper.Kaixin001
{
    public class GardenHelper : AppHelperBase
    {
        private HttpHelper httpHelper;
        private string verifyCode = string.Empty;
        private static string SUCC = "succ";

        #region Urls

        private string houseIndexUrl = "http://www.kaixin001.com/app/app.php?aid=1062&url=index.php";
        private string houseFriendUrl = "http://www.kaixin001.com/house/mystay_dialog.php?verify=";

        private string gardenIndexUrl = "http://www.kaixin001.com/app/app.php?aid=1062&url=garden/index.php";
        private string gardenDetailsUrl = "http://www.kaixin001.com/house/garden/getconf.php?verify={0}&fuid={1}&r=0.5507916617207229";
        private string seedListUrl = "http://www.kaixin001.com/house/garden/seedlist.php?verify={0}&r=0.4371907408349216";
        private string waterUrl = "http://www.kaixin001.com/house/garden/water.php?verify={0}&seedid={1}&fuid={2}&farmnum={3}";
        private string buySeedUrl = "http://www.kaixin001.com/house/garden/buyseed.php?verify={0}&num={1}&seedid={2}";
        private string getMySeedsUrl = "http://www.kaixin001.com/house/garden/myseedlist.php?verify={0}&page={1}&r=0.357555715367198";
        private string farmSeedUrl = "http://www.kaixin001.com/house/garden/farmseed.php?verify={0}&seedid={1}&fuid={2}&farmnum={3}";
        private string ploughUrl = "http://www.kaixin001.com/house/garden/plough.php?verify={0}&seedid={1}&fuid={2}&farmnum={3}";
        private string getMyGranaryUrl = "http://www.kaixin001.com/house/garden/mygranary.php?verify={0}&r=0.36221551802009344";
        private string submitSettingUrl = "http://www.kaixin001.com/house/garden/setting_submit.php?verify={0}&vermin={1}&farm={2}&steal={3}&water={4}";
        private string havestUrl = "http://www.kaixin001.com/house/garden/havest.php?verify={0}&seedid={1}&fuid={2}&farmnum={3}";
        private string sellFruitUrl = "http://www.kaixin001.com/house/garden/sellfruit.php?verify={0}&all={1}&seedid={2}&num={3}";
        private string antiVerminUrl = "http://www.kaixin001.com/house/garden/antivermin.php?verify={0}&seedid={1}&fuid={2}&farmnum={3}";
        private string antiGrassUrl = "http://www.kaixin001.com/house/garden/antigrass.php";

        #endregion

        Utility _utility;

        public GardenHelper(Utility utility, int delay)
        {
            AppID = "1062";
            _utility = utility;
            httpHelper = new HttpHelper(_utility.Cookies);
            httpHelper.NetworkDelay = delay;
        }

        #region Go one's garden

        /// <summary>
        /// 去自己的花园
        /// </summary>
        public void GotoMyGarden()
        {
            verifyCode = ContentHelper.GetMidString(httpHelper.GetHtml(gardenIndexUrl), "var g_verify = \"", "\";");
        }

        /// <summary>
        /// 去好友的花园
        /// </summary>
        /// <param name="fuid">好友编号</param>
        /// <returns>1: 不是好友;2: 网络请求失败</returns>
        public string GotoFriendGarden(string fuid)
        {
            string html = httpHelper.GetHtml(gardenIndexUrl + "&fuid=" + fuid);
            if (string.IsNullOrEmpty(html))
            {
                return "2";
            }
            else
            {
                if (html.Contains("你不是她好友，没有权限查看此内容！"))
                {
                    return "1";
                }
            }

            verifyCode = ContentHelper.GetMidString(html, "var g_verify = \"", "\";");

            return string.Empty;
        }

        #endregion

        /// <summary>
        /// 获取花园好友
        /// </summary>
        /// <returns>fuid, name</returns>
        public Dictionary<string, string> GetGardenFriend()
        {
            string html = httpHelper.GetHtml(houseIndexUrl);

            string vc = ContentHelper.GetMidString(html, "g_verify = \"", "\";");

            html = httpHelper.GetHtml(houseFriendUrl + vc);

            Dictionary<string, string> friends = new Dictionary<string, string>();

            string reg = "<div class=\"l\" style=\"width:8em;\"><a href=\"javascript:gotoUser(?<fuid>[^<]+);\" class=\"sl\">(?<name>[^<]+)</a></div>";
            Regex re = new Regex(reg);
            MatchCollection mc = re.Matches(html);
            foreach (Match match in mc)
            {
                if (match.Success)
                {
                    friends.Add(match.Groups["fuid"].Value.Replace("(", "").Replace(")", ""), match.Groups["name"].Value);
                }
            }

            return friends;
        }

        /// <summary>
        /// 获取花园信息
        /// </summary>
        /// <returns></returns>
        public GardenDetails GetGardenDetails(string fuid)
        {
            if (!_utility.IsLogin)
            {
                throw new Exception("请先登录！");
            }

            return new GardenDetails(httpHelper.GetHtml(string.Format(gardenDetailsUrl, 
                                                                    verifyCode, 
                                                                    string.IsNullOrEmpty(fuid) ? "0" : fuid)));
        }

        /// <summary>
        /// 获取种子信息
        /// </summary>
        /// <returns></returns>
        public SeedData GetSeedData()
        {
            if (!_utility.IsLogin)
            {
                throw new Exception("请先登录！");
            }

            return new SeedData(httpHelper.GetHtml(string.Format(seedListUrl, verifyCode)));
        }

        /// <summary>
        /// 获取仓库信息
        /// </summary>
        /// <returns></returns>
        public Granary GetMyGranary()
        {
            if (!_utility.IsLogin)
            {
                throw new Exception("请先登录！");
            }

            return new Granary(httpHelper.GetHtml(string.Format(getMyGranaryUrl, verifyCode)));
        }

        /// <summary>
        /// 浇水
        /// </summary>
        /// <param name="farmNum">土地编号（必须）</param>
        /// <param name="fuid">好友编号（可无）</param>
        /// <param name="seedId">种子编号（可无）</param>
        /// <returns></returns>
        public bool WaterFarm(string farmNum, string fuid, string seedId)
        {
            string result = httpHelper.GetHtml(string.Format(waterUrl,
                                                            verifyCode,
                                                            string.IsNullOrEmpty(seedId) ? "0" : seedId,
                                                            string.IsNullOrEmpty(fuid) ? "0" : fuid,
                                                            string.IsNullOrEmpty(farmNum) ? "0" : farmNum));

            return result.Contains(SUCC);
        }

        /// <summary>
        /// 买种子
        /// </summary>
        /// <param name="seedNum">购买数量</param>
        /// <param name="seedId">种子编号</param>
        /// <returns></returns>
        public bool BuySeed(int seedNum, string seedId)
        {
            string result = httpHelper.GetHtml(string.Format(buySeedUrl, verifyCode, seedNum, seedId));

            return result.Contains(SUCC);
        }

        /// <summary>
        /// 获取我的种子
        /// </summary>
        /// <param name="page">页数</param>
        /// <returns></returns>
        public MySeeds GetMySeeds(int page)
        {
            return new MySeeds(httpHelper.GetHtml(string.Format(getMySeedsUrl, verifyCode, page)));
        }


        /// <summary>
        /// 播种
        /// </summary>
        /// <param name="farmNum">土地编号（必须）</param>
        /// <param name="fuid">好友编号（可无）</param>
        /// <param name="seedId">种子编号（可无）</param>
        /// <returns></returns>
        public FarmResult FarmSeed(string farmNum, string fuid, string seedId)
        {
            string result = httpHelper.GetHtml(string.Format(farmSeedUrl, verifyCode, seedId, fuid, farmNum));

            return new FarmResult(result);
        }

        /// <summary>
        /// 犁地
        /// </summary>
        /// <param name="farmNum">土地编号（必须）</param>
        /// <param name="fuid">好友编号（可无）</param>
        /// <param name="seedId">种子编号（可无）</param>
        /// <returns></returns>
        public bool Plough(string farmNum, string fuid, string seedId)
        {
            string result = httpHelper.GetHtml(string.Format(ploughUrl, 
                                                            verifyCode,
                                                            string.IsNullOrEmpty(seedId) ? "0" : seedId,
                                                            string.IsNullOrEmpty(fuid) ? "0" : fuid,
                                                            farmNum));

            return result.Contains(SUCC);
        }

        /// <summary>
        /// 收获/偷果实
        /// </summary>
        /// <param name="farmNum">土地编号（必须）</param>
        /// <param name="fuid">好友编号（可无）</param>
        /// <param name="seedId">种子编号（可无）</param>
        /// <returns></returns><data><ret>fail</ret><reason>今天不能再偷了</reason></data>
        public HavestResult Havest(string farmNum, string fuid, string seedId)
        {
            string result = httpHelper.GetHtml(string.Format(havestUrl,
                                                            verifyCode,
                                                            string.IsNullOrEmpty(seedId) ? "0" : seedId,
                                                            string.IsNullOrEmpty(fuid) ? "0" : fuid,
                                                            farmNum));

            return new HavestResult(result);
        }

        /// <summary>
        /// 出售果实
        /// </summary>
        /// <param name="isAll">是否全部出售</param>
        /// <param name="num">出售数量</param>
        /// <param name="seedId">种子编号</param>
        /// <returns></returns>
        public SellResult SellFruit(bool isAll, string num, string seedId)
        {
            string result = httpHelper.GetHtml(string.Format(sellFruitUrl, 
                                                            verifyCode, 
                                                            Convert.ToInt32(isAll),
                                                            string.IsNullOrEmpty(seedId) ? "0" : seedId,
                                                            string.IsNullOrEmpty(num) ? "0" : num));
            return new SellResult(result);
        }

        /// <summary>
        /// 捉虫
        /// </summary>
        /// <param name="farmNum">土地编号（必须）</param>
        /// <param name="fuid">好友编号（可无）</param>
        /// <param name="seedId">种子编号（可无）</param>
        /// <returns></returns>
        public bool AntiVermin(string farmNum, string fuid, string seedId)
        {
            string antiVerminUrl = "http://www.kaixin001.com/house/garden/antivermin.php?verify={0}&seedid={1}&fuid={2}&farmnum={3}";
            string result = httpHelper.GetHtml(string.Format(antiVerminUrl,
                                                            verifyCode,
                                                            string.IsNullOrEmpty(seedId) ? "0" : seedId,
                                                            string.IsNullOrEmpty(fuid) ? "0" : fuid,
                                                            farmNum));

            return result.Contains(SUCC);
        }

        /// <summary>
        /// 锄草
        /// </summary>
        /// <param name="farmNum">土地编号（必须）</param>
        /// <param name="fuid">好友编号（可无）</param>
        /// <param name="seedId">种子编号（可无）</param>
        /// <returns></returns>
        public bool AntiGrass(string farmNum, string fuid, string seedId)
        {
            string postData = string.Format("verify={0}&seedid={1}&fuid={2}&farmnum={3}&r=0.3884165622293949",
                                            verifyCode,
                                            string.IsNullOrEmpty(seedId) ? "0" : seedId,
                                            string.IsNullOrEmpty(fuid) ? "0" : fuid,
                                            farmNum);

            string result = httpHelper.GetHtml(antiGrassUrl, postData, true);

            return result.Contains(SUCC);
        }

        /// <summary>
        /// 修改设置
        /// </summary>
        /// <param name="water">好友帮我浇水时，我会说：</param>
        /// <param name="vemin">好友帮我来捉虫时，我会说：</param>
        /// <param name="steal">好友来偷我果实时，我会说：</param>
        /// <param name="farm">好友来我家种地时，我会说：</param>
        /// <returns></returns>
        public bool ChangeSettings(string water, string vemin, string steal, string farm)
        {
            string result = httpHelper.GetHtml(string.Format(submitSettingUrl, 
                                                            verifyCode, 
                                                            water, 
                                                            vemin, 
                                                            steal, 
                                                            farm));

            return result.Contains(SUCC);
        }
    }
}
