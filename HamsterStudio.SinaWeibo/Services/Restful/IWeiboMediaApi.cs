using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.SinaWeibo.Services.Restful;

[Headers("User-Agent:Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36 Edg/136.0.0.0",
    "Referer:https://weibo.com/")]
public interface IWeiboMediaApi
{
    [Get("/large/{filename}")]
    Task<Stream> GetFile(string filename);

}
