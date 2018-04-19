namespace Slofth.Firebase.Database
{
    public class Subscription
    {
        private FirebaseObservable Observable { get; set; }

        public Subscription(FirebaseObservable observable)
        {
            Observable = observable;
        }

        public void Stop()
        {
            Observable.Stop();
        }
    }
}