using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DHT.ConsistentHash
{
    /// <summary>
    /// Each call to GetNode() costs only 1 or 2 macro-seconds. It may be the fastest consistent hashing in C#.
    ///This is a serious implementation that can work with over 10000 back-end servers, while many others
    /// cann't support more than 100 back-end servers for performance reason.
    /// </summary>
    public class MurmurHash2 : IHash
    {
        public BigInteger Hash(byte[] data) { return Hash(data, 0xc58f1a7b); }
        const UInt32 m = 0x5bd1e995;
        const Int32 r = 24;

        [StructLayout(LayoutKind.Explicit)]
        struct BytetoUInt32Converter
        {
            [FieldOffset(0)] public Byte[] Bytes;

            [FieldOffset(0)] public UInt32[] UInts;
        }

        public uint Hash(byte[] data, uint seed)
        {
            Int32 length = data.Length;
            if (length == 0)
                return 0;
            UInt32 h = seed ^ (UInt32)length;
            Int32 currentIndex = 0;
            // array will be length of Bytes but contains Uints
            // therefore the currentIndex will jump with +1 while length will jump with +4
            UInt32[] hackArray = new BytetoUInt32Converter {Bytes = data}.UInts;
            while (length >= 4)
            {
                UInt32 k = hackArray[currentIndex++];
                k *= m;
                k ^= k >> r;
                k *= m;

                h *= m;
                h ^= k;
                length -= 4;
            }

            currentIndex *= 4; // fix the length
            switch (length)
            {
                case 3:
                    h ^= (UInt16)(data[currentIndex++] | data[currentIndex++] << 8);
                    h ^= (UInt32)data[currentIndex] << 16;
                    h *= m;
                    break;
                case 2:
                    h ^= (UInt16)(data[currentIndex++] | data[currentIndex] << 8);
                    h *= m;
                    break;
                case 1:
                    h ^= data[currentIndex];
                    h *= m;
                    break;
                default:
                    break;
            }

            // Do a few final mixes of the hash to ensure the last few
            // bytes are well-incorporated.

            h ^= h >> 13;
            h *= m;
            h ^= h >> 15;

            return h;
        }
    }
}