using Newtonsoft.Json.Linq;
using PolyPaint.Services.Database;
using System;

namespace PolyPaint.Tests.Services.Database
{
    class MockSubscription<T> : ISubscription
    {
        private bool IsStopped { get; set; }
        private string Path { get; set; }
        public Action<JToken> Callback { get; private set; }

        public MockSubscription(Action<T> callback)
        {
            Callback = (token) =>
            {
                if (IsStopped)
                    return;

                callback(token.ToObject<T>());
            };
        }

        public void Stop()
        {
            IsStopped = true;
        }
    }
}
