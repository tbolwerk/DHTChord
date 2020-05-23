using System.Threading.Tasks;

namespace DHT
{
    public interface IDistributedHashtable
    {
        Task Join(int id, string ipAddress, int port);
        void Create();
    }
}