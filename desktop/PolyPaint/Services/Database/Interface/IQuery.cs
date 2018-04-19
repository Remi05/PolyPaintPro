using System;
using System.Threading.Tasks;

namespace PolyPaint.Services.Database
{
    public interface IQuery
    {
        string Key { get; }

        IChildQuery Child(string name);
        ISubscription OnChildAdded<T>(Action<T> callback);
        ISubscription OnChildRemoved<T>(Action<T> callback);
        ISubscription OnChildChanged<T>(Action<T> callback);
        ISubscription OnValue<T>(Action<T> callback);
        Task<T> Once<T>();
        Task<IChildQuery> Push();
        Task Remove();
        Task Set<T>(T value);
        Task Update<T>(T value);
    }
}