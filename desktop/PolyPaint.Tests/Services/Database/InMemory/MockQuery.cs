using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PolyPaint.Services.Database;

namespace PolyPaint.Tests.Services.Database
{
    class MockQuery : IQuery
    {
        private JObject Parent { get; set; }
        private Dictionary<string, MockChildQuery> Children { get; set; }
        private JToken Value
        {
            get => Parent[Key];
            set => Parent[Key] = value;
        }

        public string Key { get; private set; }

        private event Action<JToken> ChildAdded;
        private event Action<JToken> ChildChanged;
        private event Action<JToken> ChildRemoved;
        private event Action<JToken> ValueChanged;

        private event Action<JToken> ValueAdded;
        private event Action<JToken> ValueRemoved;

        public MockQuery(string key, JObject parent)
        {
            Children = new Dictionary<string, MockChildQuery>();
            Key = key;
            Parent = parent;
        }

        public IChildQuery Child(string name)
        {
            if (Children.ContainsKey(name)) return Children[name];

            var child = new MockChildQuery(name, Value as JObject);
            Children[name] = child;
            if (Value[name] == null) { Value[name] = new JObject(); }

            child.ValueAdded += (value) => { ChildAdded?.Invoke(Value[name]); ValueChanged?.Invoke(Value); };
            child.ValueRemoved += (value) => { ChildRemoved?.Invoke(Value[name]); ValueChanged?.Invoke(Value); };
            child.ValueChanged += (value) => { ChildChanged?.Invoke(Value[name]); ValueChanged?.Invoke(Value); };

            return child;
        }

        public Task<T> Once<T>()
        {
            return Task.FromResult(IsEmpty() ? default(T) : Value.ToObject<T>());
        }

        public Task<IChildQuery> Push()
        {
            return Task.FromResult(Child(Guid.NewGuid().ToString()));
        }

        public Task Remove()
        {
            throw new NotImplementedException();
        }

        public Task Set<T>(T value)
        {
            if (value == null)
            {
                ValueRemoved?.Invoke(Value);
                Value.Replace(null);
            }
            else
            {
                var isNewValue = IsEmpty();
                Value.Replace(JToken.FromObject(value));
                if (isNewValue)
                {
                    ValueAdded?.Invoke(Value);
                }
                else
                {
                    ValueChanged?.Invoke(Value);
                }
            }

            return Task.CompletedTask;
        }

        public Task Update<T>(T value)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public ISubscription OnChildAdded<T>(Action<T> callback)
        {
            var sub = new MockSubscription<T>(callback);
            ChildAdded += sub.Callback;
            return sub;
        }

        public ISubscription OnChildRemoved<T>(Action<T> callback)
        {
            var sub = new MockSubscription<T>(callback);
            ChildRemoved += sub.Callback;
            return sub;
        }

        public ISubscription OnChildChanged<T>(Action<T> callback)
        {
            var sub = new MockSubscription<T>(callback);
            ChildChanged += sub.Callback;
            return sub;
        }

        public ISubscription OnValue<T>(Action<T> callback)
        {
            var sub = new MockSubscription<T>(callback);
            ValueChanged += sub.Callback;
            return sub;
        }

        private bool IsEmpty()
        {
            var value = Value as JValue;
            return !Value.HasValues && value?.Value == null;
        }
    }
}
