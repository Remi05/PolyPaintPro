using log4net;
using System;
using System.Diagnostics;

namespace PolyPaint.Services.Logger
{
    public class Logger : ILogger
    {
        public void Fatal(object message)
        {
            GetLogger().Fatal(message);
        }

        public void Fatal(object message, Exception exception)
        {
            GetLogger().Fatal(message, exception);
        }

        public void Error(object message)
        {
            GetLogger().Error(message);
        }

        public void Error(object message, Exception exception)
        {
            GetLogger().Error(message, exception);
        }

        public void Warn(object message)
        {
            GetLogger().Warn(message);
        }

        public void Warn(object message, Exception exception)
        {
            GetLogger().Warn(message, exception);
        }

        public void Debug(object message)
        {
            GetLogger().Debug(message);
        }

        public void Debug(object message, Exception exception)
        {
            GetLogger().Debug(message, exception);
        }

        public void Info(object message)
        {
            GetLogger().Info(message);
        }

        public void Info(object message, Exception exception)
        {
            GetLogger().Info(message, exception);
        }

        private ILog GetLogger()
        {
            var trace = new StackTrace();
            return LogManager.GetLogger(trace.GetFrame(2).GetMethod().DeclaringType.Name);
        }
    }
}
