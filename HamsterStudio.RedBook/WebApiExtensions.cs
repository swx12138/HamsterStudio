using HamsterStudio.RedBook.Interfaces;
using HamsterStudio.RedBook.Services;
using HamsterStudio.RedBook.Services.Parsing;
using Microsoft.Extensions.DependencyInjection;

namespace HamsterStudio.RedBook;

public static class WebApiExtensions
{
    public static IServiceCollection AddRedBookWebApiServices(this IServiceCollection services)
    {
        services.AddSingleton<IRedBookParser, RedBookNoteParser>();
        services.AddSingleton<NoteDownloadService>();
        services.AddSingleton<FileMgmt>();
        services.AddSingleton<PreTokenCollector>();
        
        return services;
    }
}
