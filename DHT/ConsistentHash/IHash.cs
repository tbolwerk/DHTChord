using System.Numerics;

namespace DHT.ConsistentHash
{
    public interface IHash
    {
        public BigInteger Hash(byte[] data);
    }
}