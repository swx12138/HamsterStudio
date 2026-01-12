using NLog;
using NLog.Targets;

namespace HamsterStudio.Barefeet.Logging
{
    public class Logger0
    {
        public static Logger0 Shared { get; } = new Logger0();

        private readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private Logger0()
        {
            var debuggerTarget = new DebuggerTarget();
            debuggerTarget.Name = "Debugger";
            AddTarget(debuggerTarget, LogLevel.Trace);
        }

        public bool AddTarget(Target target, LogLevel min, LogLevel? max = null)
        {
            var asyncTarget = new NLog.Targets.Wrappers.AsyncTargetWrapper(target);
            var config = LogManager.Configuration ?? new();
            config.AddTarget(target.Name, asyncTarget);
            config.AddRule(min, max ?? LogLevel.Fatal, asyncTarget);
            LogManager.Configuration = config;
            return true;
        }

        public void Critical(Exception ex) => logger.Error(ex.Message + "\n" + ex.StackTrace);

        public void Error(string message) => logger.Error(message);

        public void Information(string message) => logger.Info(message);

        public void Trace(string message) => logger.Trace(message);
        public void Trace(Exception ex) => logger.Trace(ex.Message + "\n" + ex.StackTrace);

        public void Warning(string message) => logger.Warn(message);

        public void Debug(string message)
#if DEBUG
           => logger.Debug(message);
#else
        { }
#endif
        public void Debug(Exception ex) => logger.Debug(ex.Message + "\n" + ex.StackTrace);
        public void Debug(string source, Exception ex) => logger.Debug($"[{source}]" + ex.Message + "\n" + ex.StackTrace);

    }

}
