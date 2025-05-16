using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet.Logging
{
    public class Logger
    {
        public static Logger Shared { get; } = new Logger();

        private readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private Logger()
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
            config.AddRule(min, max?? LogLevel.Fatal, asyncTarget);
            LogManager.Configuration = config;
            return true;
        }

        public void Critical(Exception ex) => logger.Error(ex.Message + "\n" + ex.StackTrace);

        public void Error(string message) => logger.Error(message);

        public void Information(string message) => logger.Info(message);

        public void Trace(string message) => logger.Trace(message);

        public void Warning(string message) => logger.Warn(message);

        public void Debug(string message) => logger.Debug(message);
        public void Debug(Exception ex) => logger.Debug(ex.Message + "\n" + ex.StackTrace);
        public void Debug(string source,Exception ex) => logger.Debug($"[{source}]"+ ex.Message + "\n" + ex.StackTrace);

    }

}
