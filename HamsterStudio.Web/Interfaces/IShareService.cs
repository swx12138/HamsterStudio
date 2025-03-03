using Refit;

namespace HamsterStudio.Web.Interfaces;

public interface IShareService
{
    [Get("/repos")]
    Task<IEnumerable<string>> GetReposAsync();

    [Post("/share")]
    Task<string> PostDataAsync(Dictionary<string, string> keyValues);



}
