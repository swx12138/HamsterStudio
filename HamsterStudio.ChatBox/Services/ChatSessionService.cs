using HamsterStudio.Barefeet.Services;
using HamsterStudio.ChatBox.Models;
using System.IO;

namespace HamsterStudio.ChatBox.Services;

/// <summary>对话导入导出服务：将对话数据经 BinaryDataSerializer 压缩后写入 .hchat 文件。</summary>
public class ChatSessionService
{
    public void Save(ChatSessionDocument document, string path)
    {
        var bytes = BinaryDataSerializer.Serialize(document);
        File.WriteAllBytes(path, bytes);
    }

    public ChatSessionDocument? Load(string path)
    {
        var bytes = File.ReadAllBytes(path);
        return BinaryDataSerializer.Deserialize<ChatSessionDocument>(bytes);
    }
}
