using HamsterStudio.SinaWeibo.Models;
using HamsterStudio.Web.DataModels;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.SinaWeibo.Services.Restful;

public interface IWeiboClient
{
    [Post("/weibo/download")]
    public Task<ServerRespModel> PostDownloadData(DownloadPostModel post);

}
