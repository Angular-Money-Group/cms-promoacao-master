using Bitzar.CMS.Extension.CMS;
using System.Collections.Concurrent;
using System.Linq;

namespace Bitzar.CMS.Extension.Classes
{
    /// <summary>
    /// Dictionary Cache implementation
    /// </summary>
    public class DictionaryCache : IDictionaryCache
    {
        private ConcurrentDictionary<string, object> Storage { get; set; } = new ConcurrentDictionary<string, object>();

        // Note: The methods Contains, Retrieve, Store (and Remove) must exactly look like the following:

        public bool Contains(string key)
        {
            return Storage.ContainsKey(key);
        }

        public T Retrieve<T>(string key)
        {
            if (!Storage.TryGetValue(key, out var value))
                return default;

            return (T)value;
        }

        public void Store(string key, object data)
        {
            Storage[key] = data;
        }

        // Remove is needed for writeable properties which must invalidate the Cache
        // You can skip this method but then only readonly properties are supported
        public void Remove(string key)
        {
            Storage.TryRemove(key, out _);
        }

        public string[] AllKeys
        {
            get => Storage.Select(d => d.Key).ToArray();
        }

        public void Clear()
        {
            Storage.Clear();
        }
    }
}
