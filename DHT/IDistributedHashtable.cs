namespace DHT
{
    public interface IDistributedHashtable
    {
        string Get(string key);
        void Put(string key, string value);
        void Run(string[] args);
    }
}