using Refit;

namespace HamsterStudio.Web.Services.Restful;

public interface IStaticFilesClient
{
    [Get("/static/{**fileRelaPath}")]
    Task<Stream> GetStaticFile(string fileRelaPath);
}
