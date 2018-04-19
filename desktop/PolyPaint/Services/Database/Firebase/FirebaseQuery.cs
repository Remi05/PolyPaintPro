using System;
using System.Threading.Tasks;
using Slofth.Firebase.Database;

namespace PolyPaint.Services.Database
{
    public class FirebaseQuery : IQuery
    {
        private Query Query { get; }
        public string Key => Query?.Key;

        public FirebaseQuery(Query query)
        {
            Query = query;
        }

        public ISubscription OnChildAdded<T>(Action<T> callback)
        {
            return new FirebaseSubscription(Query.OnChildAdded(callback));
        }

        public ISubscription OnChildRemoved<T>(Action<T> callback)
        {
            return new FirebaseSubscription(Query.OnChildRemoved(callback));
        }

        public ISubscription OnChildChanged<T>(Action<T> callback)
        {
            return new FirebaseSubscription(Query.OnChildChanged(callback));
        }

        public ISubscription OnValue<T>(Action<T> callback)
        {
            return new FirebaseSubscription(Query.OnValue(callback));
        }

        public Task<T> Once<T>()
        {
            return Query.Once<T>();
        }

        public async Task<IChildQuery> Push()
        {
            return new FirebaseChildQuery(await Query.Push());
        }

        public async Task Update<T>(T value)
        {
            await Query.Update(value);
        }

        public async Task Remove()
        {
            await Query.Remove();
        }

        public async Task Set<T>(T value)
        {
            await Query.Set(value);
        }

        public IChildQuery Child(string name)
        {
            return new FirebaseChildQuery(Query.Child(name));
        }
    }
}