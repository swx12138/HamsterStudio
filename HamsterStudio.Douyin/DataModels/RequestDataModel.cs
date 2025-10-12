using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Douyin.DataModels;

public class RequestDataModel
{
    public string PostId { init; get; }
    public string UserName { init; get; }
    public string Title { init; get; }
    public string Description { init; get; }
    public string CoverUrl { init; get; }
    public List<string> ResourceUrls { init; get; } = new List<string>();
    public RequestDataModel() { }
    public RequestDataModel(WebAwemePostModel postModel)
    {
        throw new NotImplementedException();
    }
}
