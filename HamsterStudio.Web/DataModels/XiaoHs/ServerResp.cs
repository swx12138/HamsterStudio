using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HamsterStudio.Web.DataModels.XiaoHs
{
    public class ServerRespData
    {
        [JsonPropertyName("作品标题")]
        public string Title { get; set; }

        [JsonPropertyName("作品描述")]
        public string Description { get; set; }

        [JsonPropertyName("作者昵称")]
        public string AuthorNickName { get; set; }

        [JsonPropertyName("static_files")]
        public string[] StaticFiles { get; set; }

    }

    public class ServerResp
    {
        [JsonPropertyName("messge")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public ServerRespData Data { get; set; }


    }

}
