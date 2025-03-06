using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Toolkits.Logging
{
    public class Logger
    {
        public static Logger Shared { get; } = new Logger();

        private readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private Logger() {

        }

        public bool AddTarget(Target target)
        {
            var asyncTarget = new NLog.Targets.Wrappers.AsyncTargetWrapper(target);
            var config = new LoggingConfiguration();
            config.AddTarget(target.Name, asyncTarget);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, asyncTarget);
            LogManager.Configuration = config;
            return true;
        }

        public void Critical(Exception ex) => logger.Error(ex.Message + "\n" + ex.StackTrace);

        public void Error(string message) => logger.Error(message);

        public void Information(string message) => logger.Info(message);

        public void Trace(string message) => logger.Trace(message);

        public void Warning(string message) => logger.Warn(message);

        public void Debug(string message) => logger.Debug(message);

    }

}
