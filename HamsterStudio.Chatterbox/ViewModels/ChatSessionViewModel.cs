using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Chatterbox.Models;
using HandyControl.Data;
using LiteObservableCollections;
using Microsoft.Extensions.Logging;

namespace HamsterStudio.Chatterbox.ViewModels;

public partial class ChatSessionViewModel(ILogger<ChatSessionViewModel> logger) : KnownViewModel(logger)
{
    [ObservableProperty]
    private ObservableCollection<ChatInfoModel> chatInfos = [
            new ChatInfoModel(){Role = ChatRoleType.Sender, Message = "Hello"},
            new ChatInfoModel(){Role = ChatRoleType.Receiver, Message = "World!"},
            new ChatInfoModel(){Role = ChatRoleType.Sender, Message = "🙂"}
    ];



}
