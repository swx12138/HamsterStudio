using HamsterStudio.Web.DataModels.Kuro;
using Refit;

namespace HamsterStudio.Web.Requests.Kuro
{
    internal interface ILauncher
    {
        [Get("/pcstarter/prod/starter/10003_Y8xXrXk65DqFHEDgApn3cpK5lfczpFx5/G152/index.json")]
        Task<StarterInfo> GetStarterInfo();
        //Task<StarterInfo> GetStarterInfo();

    }
}
