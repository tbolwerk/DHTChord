using System.Threading.Tasks;

namespace DHT
{
    public interface IDistributedHashtable
    {
        void Join(int id, string ipAddress, int port);
        void Create();
        void Start();
    }
}