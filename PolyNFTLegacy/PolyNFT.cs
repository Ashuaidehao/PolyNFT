using System;
using System.ComponentModel;
using System.Numerics;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;

namespace PolyNFTLegacy
{
    [DisplayName("PolyNFT Contract")]
    public partial class PolyNFT : SmartContract
    {

  

        [DisplayName("decimals")]
        public static byte Decimals() => 0;

        [DisplayName("name")]
        public static string Name() => "PolyNFT"; //name of the token

        [DisplayName("symbol")]
        public static string Symbol() => "Poly"; //symbol of the token

        public static object Main(string method, object[] args)
        {
            if (Runtime.Trigger == TriggerType.Verification)
            {
                return Runtime.CheckWitness(GetAdmin());
            }
            if (Runtime.Trigger == TriggerType.Application)
            {
                //合约调用时，等价以太坊的msg.sender
                //直接调用时，此处为 tx.Script.ToScriptHash();
                var msgSender = ExecutionEngine.CallingScriptHash;


                //if (method == "mint") return Mint(msgSender, (byte[])args[0]);//msgSender应当为router

                if (method == "transfer") return Transfer((byte[])args[0], (byte[])args[1], (object)args[2], msgSender);

                if (method == "balanceOf") return BalanceOf((byte[])args[0]);

                if (method == "ownerOf") return OwnerOf((byte[])args[0]);

                if (method == "tokens") return Tokens();

                if (method == "tokensOf") return TokensOf((byte[])args[0]);

                if (method == "decimals") return Decimals();

                if (method == "name") return Name();

                if (method == "symbol") return Symbol();

                if (method == "totalSupply") return TotalSupply();



                if (method == "getDeadline") return GetDeadline();

                if (method == "setDeadline") return SetDeadline((BigInteger)args[0]);

                if (method == "getUpperLimit") return GetUpperLimit();

                if (method == "setUpperLimit") return SetUpperLimit((BigInteger)args[0]);

                if (method == "getLowerLimit") return GetLowerLimit();

                if (method == "setLowerLimit") return SetLowerLimit((BigInteger)args[0]);

                if (method == "setClaimRange") return SetClaimRange((BigInteger)args[0], (BigInteger)args[1]);

                if (method == "mintWithURI") return MintWithURI((byte[])args[0], (BigInteger)args[1], (string)args[2]);

                if (method == "claim") return Claim((byte[])args[0], (BigInteger)args[1], (string)args[2]);

                if (method == "getTokenURI") return GetTokenURI((BigInteger)args[0]);

                if (method == "exist") return Exist((BigInteger)args[0]);

                if (method == "getAdmin") return GetAdmin();

                if (method == "setAdmin") return SetAdmin((byte[])args[0]);

                if (method == "upgrade")
                {
                    Assert(args.Length == 9, "upgrade: args.Length != 9");
                    byte[] script = (byte[])args[0];
                    byte[] plist = (byte[])args[1];
                    byte rtype = (byte)args[2];
                    ContractPropertyState cps = (ContractPropertyState)args[3];
                    string name = (string)args[4];
                    string version = (string)args[5];
                    string author = (string)args[6];
                    string email = (string)args[7];
                    string description = (string)args[8];
                    return Upgrade(script, plist, rtype, cps, name, version, author, email, description);
                }

            }
            return false;
        }


        /// <summary>
        /// 获取upperLimit
        /// </summary>
        /// <returns></returns>
        public static BigInteger GetUpperLimit()
        {
            var value = Storage.Get(UpperLimit);
            return value == null ? 0 : value.ToBigInteger();
        }

        /// <summary>
        /// 设置upperLimit
        /// </summary>
        /// <param name="upperLimit"></param>
        /// <returns></returns>
        public static bool SetUpperLimit(BigInteger upperLimit)
        {
            Assert(Verify(), "Forbidden");
            Storage.Put(UpperLimit, upperLimit);
            return true;
        }

        /// <summary>
        /// 获取 lowerLimit
        /// </summary>
        /// <returns></returns>
        public static BigInteger GetLowerLimit()
        {
            var value = Storage.Get(LowerLimit);
            return value == null ? 0 : value.ToBigInteger();
        }

        /// <summary>
        /// 设置 lowerLimit
        /// </summary>
        /// <param name="lowerLimit"></param>
        /// <returns></returns>
        public static bool SetLowerLimit(BigInteger lowerLimit)
        {
            Assert(Verify(), "Forbidden");
            Storage.Put(LowerLimit, lowerLimit);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lowerLimit"></param>
        /// <param name="upperLimit"></param>
        public static bool SetClaimRange(BigInteger lowerLimit, BigInteger upperLimit)
        {
            Assert(Verify(), "Forbidden");
            Storage.Put(LowerLimit, lowerLimit);
            Storage.Put(UpperLimit, upperLimit);
            return true;
        }

        /// <summary>
        /// 获取 Deadline
        /// </summary>
        /// <returns></returns>
        public static BigInteger GetDeadline()
        {
            var value = Storage.Get(Deadline);
            return value == null ? 0 : value.ToBigInteger();
        }

        /// <summary>
        /// 设置 Deadline
        /// </summary>
        /// <param name="deadline"></param>
        /// <returns></returns>
        public static bool SetDeadline(BigInteger deadline)
        {
            Assert(Verify(), "Forbidden");
            Storage.Put(Deadline, deadline);
            return true;
        }


        public static bool MintWithURI(byte[] to, BigInteger tokenId, string uri)
        {
            Assert(Verify(), "Forbidden");
            SafeMint(to, tokenId);
            SetTokenURI(tokenId, uri);
            return true;
        }

        public static bool Claim(byte[] account, BigInteger tokenId, string uri)
        {
            Assert(Runtime.CheckWitness(account), "Forbidden");
            Assert(OwnerOf(tokenId.ToByteArray()) == account, "Invalid owner");
            CheckDeadline();
            CheckRange(tokenId);
            SafeMint(account, tokenId);
            SetTokenURI(tokenId, uri);
            return true;
        }

        public static bool Exist(BigInteger tokenId)
        {
            var owner = OwnerOf(tokenId.ToByteArray());
            return owner != null;
        }

        public static byte[] GetTokenURI(BigInteger tokenId)
        {
            return TokenURIs.Get(tokenId.ToByteArray());
        }

        private static void SetTokenURI(BigInteger tokenId, string tokenURI)
        {
            Assert(Exist(tokenId), "URI set of nonexistent token");
            TokenURIs.Put(tokenId.ToByteArray(), tokenURI);
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

  
        private static void SafeMint(byte[] owner, BigInteger tokenId)
        {
            var token = new TokenState();
            token.Owner = owner;
            token.TokenId = tokenId.ToByteArray();
            Mint(token.TokenId, token);
        }

        private static StorageMap TokenURIs => Storage.CurrentContext.CreateMap(TokenURI);
        

    }
}
