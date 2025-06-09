using HamsterStudio.DeepSeek.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.DeepSeek.Services.Restful;

public interface IDeepSeekApi
{
    [Post("/chat/completions")]
    Task<ChatResponse> GetResponseAsync([Body] ChatRequestRoot chat);

    [Post("/chat/completions")]
    Task<Stream> GetStreamingResponseAsync([Body] ChatRequestRoot chat);

    [Get("/models")]
    Task<DeepSeekModels> GetModels();
}

public interface IDeepSeekBetaApi
{
    [Post("/completions")]
    [Headers("Content-Type:application/json", "Accept: application/json")]
    Task<Stream> QuestFillInTheMiddle([Body] FimChatRequest request);

}
