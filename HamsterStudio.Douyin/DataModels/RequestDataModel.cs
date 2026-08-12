namespace HamsterStudio.Douyin.DataModels;

public class RequestDataModel
{
    public string PostId { init; get; } = string.Empty;
    public string UserName { init; get; } = string.Empty;
    public string Title { init; get; } = string.Empty;
    public string Description { init; get; } = string.Empty;
    public string CoverUrl { init; get; } = string.Empty;
    public List<string> ResourceUrls { init; get; } = new List<string>();
    public RequestDataModel() { }
    public RequestDataModel(WebAwemePostModel postModel)
    {
        throw new NotImplementedException();
    }
}
