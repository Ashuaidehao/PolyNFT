using System.Numerics;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services;

namespace PolyNFT
{
    public partial class PolyNFT
    {
        private static readonly byte[] Prefix_Storage = new byte[] { 0xff };
        private const byte Prefix_TokenURI = 0xfe;


        private static ByteString StorageGet(ByteString key)
        {
            return Storage.Get(Storage.CurrentContext, Prefix_Storage.Concat(key));
        }

        private static ByteString StorageGet(byte[] key)
        {
            return Storage.Get(Storage.CurrentContext, Prefix_Storage.Concat(key));
        }



        private static void StoragePut(ByteString key, ByteString value)
        {
            Storage.Put(Storage.CurrentContext, Prefix_Storage.Concat(key), value);
        }

        private static void StoragePut(ByteString key, BigInteger value)
        {
            Storage.Put(Storage.CurrentContext, Prefix_Storage.Concat(key), value);
        }

        private static void StoragePut(ByteString key, byte[] value)
        {
            Storage.Put(Storage.CurrentContext, Prefix_Storage.Concat(key), (ByteString)value);
        }


        private static StorageMap GetTokenMap()
        {
            return new StorageMap(Storage.CurrentContext, Prefix_TokenURI);
        }

        private static StorageMap TokenURIs => GetTokenMap();
    }
}
