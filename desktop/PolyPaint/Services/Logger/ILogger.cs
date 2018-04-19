using System;

namespace PolyPaint.Services.Logger
{
    public interface ILogger
    {
        void Debug(object message);
        void Debug(object message, Exception exception);
        void Error(object message);
        void Error(object message, Exception exception);
        void Fatal(object message);
        void Fatal(object message, Exception exception);
        void Info(object message);
        void Info(object message, Exception exception);
        void Warn(object message);
        void Warn(object message, Exception exception);
    }
}