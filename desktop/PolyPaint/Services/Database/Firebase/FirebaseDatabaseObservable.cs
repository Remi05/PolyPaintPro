using Slofth.Firebase.Database;

namespace PolyPaint.Services.Database
{
    public class FirebaseSubscription : ISubscription
    {
        private bool IsStopped { get; set; }
        private Subscription Subscription { get; }

        public FirebaseSubscription(Subscription subscription)
        {
            Subscription = subscription;
        }

        public void Stop()
        {
            if (IsStopped)
                return;

            Subscription.Stop();
            IsStopped = true;
        }

        ~FirebaseSubscription()
        {
            Stop();
        }
    }
}