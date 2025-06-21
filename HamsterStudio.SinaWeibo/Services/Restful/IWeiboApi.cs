using HamsterStudio.SinaWeibo.Models;
using Refit;

namespace HamsterStudio.SinaWeibo.Services.Restful;

[Headers("User-Agent:Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36 Edg/136.0.0.0",
    "Cookie:SINAGLOBAL=193023600802.39334.1719727949337; SUB=_2AkMQrvI_f8NxqwFRmfwXymLraolywgvEieKm8gPkJRMxHRl-yT8XqhxStRB6Oy7c0DXXtG_Nevnl7VijOyLFQN7rBFAr; SUBP=0033WrSXqPxfM72-Ws9jqgMF55529P9D9WW86Cs5Xy-S3SIf_vpJ2P.e; ULV=1748181709433:16:5:1:3843642033749.0386.1748181709431:1748021354206; WBPSESS=Wk6CxkYDejV3DDBcnx2LORJZd1BTx3BthNo3-yq5XMJO50aX9L3iwYOxGA1tjs5HGRjz24phWfR7xoC9w5xtd2ASMNbl_5_tGUuTQd7XjN_pke2MmaRUzGDClQbJhOzJ; PC_TOKEN=6830111bc9; XSRF-TOKEN=U-aOBuzF4S2ewulXuClQbQGY")]
public interface IWeiboApi
{
    [Get("/ajax/statuses/show")]
    Task<ShowDataModel> GetShowInfo(string id, 
                             string locale = "zh-CN",
                             bool isGetLongText = true);

}
