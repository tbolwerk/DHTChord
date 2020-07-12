using System.Numerics;

namespace DHT
{
    public interface IGenerateKey
    {
        BigInteger Generate(string input);
    }
}