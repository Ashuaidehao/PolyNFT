using Neo.SmartContract.Framework;
using System.ComponentModel;
using System.Numerics;
using Neo;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;

namespace PolyNFT
{
    [DisplayName("PolyNFT")]
    [ManifestExtra("Author", "NEO")]
    [ManifestExtra("Email", "developer@neo.org")]
    [ManifestExtra("Description", "This is a Neo3 Contract")]
    [ContractPermission("*", "onNEP11Payment")]
    public partial class PolyNFT : Nep11Token<TokenState>
    {
        //[DisplayName("Transfer")]
        //public static event OnTransferDelegate OnTransfer;
        //public delegate void OnTransferDelegate(UInt160 from, UInt160 to, BigInteger amount, ByteString tokenId);

        /// <summary>
        /// params: message, extend data
        /// </summary>
        [DisplayName("Fault")]
        public static event FaultEvent onFault;
        public delegate void FaultEvent(string message, params object[] paras);



        public override string Symbol() => "Poly";

        private const string UpperLimit = nameof(UpperLimit);
        private const string LowerLimit = nameof(LowerLimit);
        private const string Deadline = nameof(Deadline);


        /// <summary>
        /// 获取upperLimit
        /// </summary>
        /// <returns></returns>
        public static BigInteger GetUpperLimit()
        {
            var value = StorageGet(UpperLimit);
            return value == null ? 0 : (BigInteger)value;
        }

        /// <summary>
        /// 设置upperLimit
        /// </summary>
        /// <param name="upperLimit"></param>
        /// <returns></returns>
        public static bool SetUpperLimit(BigInteger upperLimit)
        {
            Assert(Verify(), "Forbidden");
            StoragePut(UpperLimit, upperLimit);
            return true;
        }

        /// <summary>
        /// 获取 lowerLimit
        /// </summary>
        /// <returns></returns>
        public static BigInteger GetLowerLimit()
        {
            var value = StorageGet(LowerLimit);
            return value == null ? 0 : (BigInteger)value;
        }

        /// <summary>
        /// 设置 lowerLimit
        /// </summary>
        /// <param name="lowerLimit"></param>
        /// <returns></returns>
        public static bool SetLowerLimit(BigInteger lowerLimit)
        {
            Assert(Verify(), "Forbidden");
            StoragePut(LowerLimit, lowerLimit);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lowerLimit"></param>
        /// <param name="upperLimit"></param>
        public static void SetClaimRange(BigInteger lowerLimit, BigInteger upperLimit)
        {
            Assert(Verify(), "Forbidden");
            StoragePut(LowerLimit, lowerLimit);
            StoragePut(UpperLimit, upperLimit);
        }

        /// <summary>
        /// 获取 Deadline
        /// </summary>
        /// <returns></returns>
        public static BigInteger GetDeadline()
        {
            var value = StorageGet(Deadline);
            return value == null ? 0 : (BigInteger)value;
        }

        /// <summary>
        /// 设置 Deadline
        /// </summary>
        /// <param name="deadline"></param>
        /// <returns></returns>
        public static bool SetDeadline(BigInteger deadline)
        {
            Assert(Verify(), "Forbidden");
            StoragePut(Deadline, deadline);
            return true;
        }


        /// <summary>
        /// 合约初始化
        /// </summary>
        /// <param name="data"></param>
        /// <param name="update"></param>
        public static void _deploy(object data, bool update)
        {
            var paras = (BigInteger[])data;
            if (paras.Length == 3)
            {
                SetLowerLimit(paras[0]);
                SetUpperLimit(paras[1]);
                SetDeadline(paras[2]);
            }
        }

        public static void MintWithURI(UInt160 to, BigInteger tokenId, string uri)
        {
            Assert(Verify(), "Forbidden");
            SafeMint(to, tokenId);
            SetTokenURI(tokenId, uri);
        }

        public static void Claim(UInt160 account, BigInteger tokenId, string uri)
        {
            Assert(Runtime.CheckWitness(account), "Forbidden");
            Assert(OwnerOf((ByteString)tokenId.ToByteArray()) == account, "Invalid owner");
            CheckDeadline();
            CheckRange(tokenId);
            SafeMint(account, tokenId);
            SetTokenURI(tokenId, uri);
        }


        public static string GetTokenURI(BigInteger tokenId)
        {
            return TokenURIs[tokenId.ToByteArray()];
        }

        public static bool Exist(BigInteger tokenId)
        {
            var owner = OwnerOf((ByteString)tokenId.ToByteArray());
            return owner != null;
        }


        private static void CheckRange(BigInteger tokenId)
        {
            var lowerLimit = GetLowerLimit();
            var upperLimit = GetUpperLimit();
            Assert(tokenId >= lowerLimit && tokenId <= upperLimit, "Invalid token id");
        }

        private static void CheckDeadline()
        {
            var deadline = GetDeadline();
            Assert(deadline == 0 || Runtime.Time <= deadline, "Claim entrance is closed!");
        }
        
        private static void SetTokenURI(BigInteger tokenId, string tokenURI)
        {
            Assert(Exist(tokenId), "URI set of nonexistent token");
            TokenURIs[tokenId.ToByteArray()] = tokenURI;
        }

        private static void SafeMint(UInt160 owner, BigInteger tokenId)
        {
            var token = new TokenState();
            token.Owner = owner;
            token.TokenId = (ByteString)tokenId.ToByteArray();
            Mint(token.TokenId, token);
        }

  

    }
}
