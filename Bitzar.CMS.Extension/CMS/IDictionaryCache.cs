namespace Bitzar.CMS.Extension.CMS
{
    public interface IDictionaryCache
    {
        string[] AllKeys { get; }

        void Clear();
        bool Contains(string key);
        void Remove(string key);
        T Retrieve<T>(string key);
        void Store(string key, object data);
    }
}