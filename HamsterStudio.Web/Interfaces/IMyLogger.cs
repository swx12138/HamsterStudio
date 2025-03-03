//using Microsoft.Extensions.Logging;

using OpenCvSharp;

namespace HamsterStudio.Web.Interfaces
{
    public interface IMyLogger
    {
        void Critical(string message);
        void Critical(Exception ex);
        void Error(string message);
        void Flush();
        void Information(string message);
        void Log(string message, LogLevel loggingLevel);
        void Trace(string message);
        void Warning(string message);
    }
}
