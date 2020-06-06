using System.Threading.Tasks;

namespace DHT
{
    public interface IDistributedHashtable
    {
        void Join(uint id, string ipAddress, int port);
        string Get(string key);
        void Put(string key, string value);
        void Create();
        void Run(string[] args);
    }
}