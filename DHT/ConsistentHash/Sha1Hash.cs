using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace DHT.ConsistentHash
{
    public class Sha1Hash : IHash
    {
        public BigInteger Hash(byte[] data)
        {
            var hash = new SHA1Managed().ComputeHash(data);
            return BigInteger.Parse(string.Concat(hash.Select(b => b.ToString())));
        }
    }
}