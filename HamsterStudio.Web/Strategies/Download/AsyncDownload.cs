using HamsterStudio.Web.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Web.Strategies.Download;

internal class AsyncDownloadStrategy : IDownloadStrategy
{
    public Task<DownloadResult> DownloadAsync(DownloadRequest request)
    {
        throw new NotImplementedException();
    }

}
