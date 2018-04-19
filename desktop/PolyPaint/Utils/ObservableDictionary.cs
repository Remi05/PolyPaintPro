using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace PolyPaint.Utils
{
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged
    {
        private Dictionary<TKey, TValue> Dictionary { get; set; } = new Dictionary<TKey, TValue>();

        public ICollection<TKey> Keys => Dictionary.Keys;

        public ICollection<TValue> Values => Dictionary.Values;

        public int Count => Dictionary.Count;

        public bool IsReadOnly => false;

        public TValue this[TKey key]
        {
            get => Dictionary[key];
            set
            {
                var oldItem = ContainsKey(key) ? Dictionary[key] : default(TValue);

                Dictionary[key] = value;
                if (oldItem == null)
                {
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
                }
                else
                {
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem));
                }
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public bool ContainsKey(TKey key)
        {
            return Dictionary.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            Dictionary.Add(key, value);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
        }

        public bool Remove(TKey key)
        {
            var oldItem = ContainsKey(key) ? Dictionary[key] : default(TValue);

            if (!Dictionary.Remove(key))
                return false;

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem));
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            Dictionary.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return Dictionary.ContainsKey(item.Key) && Dictionary[item.Key].Equals(item.Value);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }
    }
}
