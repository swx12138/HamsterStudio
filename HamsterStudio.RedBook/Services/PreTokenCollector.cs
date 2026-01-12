using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Barefeet.Services;
using Microsoft.Extensions.Logging;

namespace HamsterStudio.RedBook.Services;

public class PreTokenCollector : BindableBase
{
    private HashSet<string> _tokens;

    public event EventHandler? TokenListChanged;

    public PreTokenCollector(NoteDownloadService service, DataStorageMgmt dataStorageMgmt, ILogger<PreTokenCollector> logger) : base(logger)
    {
        service.OnImageTokenDetected += Service_OnImageTokenDetected;
        dataStorageMgmt.BeforePersist += (object? sender, EventArgs e) =>
        {
            dataStorageMgmt.Set(nameof(PreTokenCollector), _tokens);
        };
        _tokens = dataStorageMgmt.Get<HashSet<string>>(nameof(PreTokenCollector)) ?? [];
    }

    private void Service_OnImageTokenDetected(string token)
    {
        int index = token.LastIndexOf('/');
        if (index != -1)
        {
            string preToken = token.Substring(0, index + 1);
            AddToken(preToken);
            return;
        }
    }

    public void SetTokens(IReadOnlyCollection<string> tokens)
    {
        _tokens = [.. _tokens.Concat(tokens)];
    }

    public IReadOnlyCollection<string> GetTokens()
    {
        return _tokens;
    }

    public void AddToken(string token)
    {
        lock (_tokens)
        {
            if (_tokens.Add(token))
            {
#if DEBUG
                logger.LogInformation($"已添加前缀《{token}》");
#endif
                TokenListChanged?.Invoke(this, new EventArgs());
            }
        }
    }

}
