using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using DHT.ConsistentHash;
using Microsoft.Extensions.Options;

namespace DHT
{
    public class KeyGenerator : IGenerateKey
    {
        private readonly IHash _hash;
        private readonly uint _maxKeySpace;
        
        public List<string?> bootstrappingNodes { get; }

        public KeyGenerator(IOptions<DhtSettings> options,IHash hash)
        {
            _hash = hash;
            var optionsKeySpace = options.Value.KeySpace;
            if (optionsKeySpace != _maxKeySpace  && optionsKeySpace > 1)
            {
                _maxKeySpace = options.Value.KeySpace;
            }
            else
            {
                _maxKeySpace = uint.MaxValue;
            }

            bootstrappingNodes = options.Value.BootstrapUrls;
        }

        public BigInteger Generate(string input)
        {
            Console.WriteLine(input);
            if (bootstrappingNodes.Exists(x=>x.Equals(input)))
            {
                return 0;
            }

            return _hash.Hash(Encoding.ASCII.GetBytes(input.ToString())) % _maxKeySpace;
        }
    }
}