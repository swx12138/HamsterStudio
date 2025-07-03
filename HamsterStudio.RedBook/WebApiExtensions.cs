using HamsterStudio.RedBook.Interfaces;
using HamsterStudio.RedBook.Services;
using HamsterStudio.RedBook.Services.Parsing;
using HamsterStudio.RedBook.Services.XhsRestful;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace HamsterStudio.RedBook;

public static class WebApiExtensions
{
    public static IServiceCollection AddRedBookWebApiServices(this IServiceCollection services)
    {
        services.AddSingleton<IRedBookParser, RedBookNoteParser>();
        services.AddSingleton<NoteDownloadService>();
        services.AddSingleton<FileMgmt>();
        
        return services;
    }
}
