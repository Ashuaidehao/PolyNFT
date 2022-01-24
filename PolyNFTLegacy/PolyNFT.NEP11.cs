using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;

namespace PolyNFTLegacy
{
    public partial class PolyNFT
    {
        public delegate void OnTransferDelegate(byte[] from, byte[] to, BigInteger amount, byte[] tokenId);

        [DisplayName("Transfer")]
        public static event OnTransferDelegate OnTransfer;


        public static BigInteger TotalSupply() => Storage.Get(Storage.CurrentContext, Prefix_TotalSupply).ToBigInteger();

        public static BigInteger BalanceOf(byte[] owner)
        {
            Assert(IsAddress(owner), "The argument \"owner\" is invalid.");

            StorageMap balanceMap = Storage.CurrentContext.CreateMap(Prefix_Balance);
            return balanceMap.Get(owner).ToBigInteger();
        }

        public static byte[] OwnerOf(byte[] tokenId)
        {
            StorageMap tokenMap = Storage.CurrentContext.CreateMap(Prefix_Token);
            TokenState token = (TokenState)tokenMap.Get(tokenId).Deserialize();
            return token.Owner;
        }

        public virtual Map<string, object> Properties(byte[] tokenId)
        {
            StorageMap tokenMap = Storage.CurrentContext.CreateMap(Prefix_Token);
            TokenState token = (TokenState)tokenMap.Get(tokenId).Deserialize();
            Map<string, object> map = new Map<string, object>();
            map["tokenId"] = token.TokenId;
            return map;
        }

        public static Iterator<string, byte[]> Tokens()
        {
            return Storage.Find(Prefix_Token);
        }

        public static Iterator<string, byte[]> TokensOf(byte[] owner)
        {
            Assert(IsAddress(owner), "The argument \"owner\" is invalid");
            return Storage.Find(Prefix_AccountToken);
            //return accountMap.Find(owner, FindOptions.KeysOnly | FindOptions.RemovePrefix);
        }

        public static bool Transfer(byte[] to, byte[] tokenId, object data, byte[] caller)
        {
            Assert(IsAddress(to), "The argument \"to\" is invalid");

            StorageMap tokenMap = Storage.CurrentContext.CreateMap(Prefix_Token);
            TokenState token = (TokenState)tokenMap.Get(tokenId).Deserialize();
            var from = token.Owner;
            if (from != caller && !Runtime.CheckWitness(from)) return false;
            if (from != to)
            {
                token.Owner = to;
                tokenMap.Put(tokenId, token.Serialize());
                UpdateBalance(from, tokenId, -1);
                UpdateBalance(to, tokenId, +1);
            }
            PostTransfer(from, to, tokenId, data);
            return true;
        }


        private static void UpdateBalance(byte[] owner, byte[] tokenId, int increment)
        {
            UpdateBalance(owner, increment);
            StorageMap accountMap = Storage.CurrentContext.CreateMap(Prefix_AccountToken);
            var key = owner.Concat(tokenId);
            if (increment > 0)
                accountMap.Put(key, 0);
            else
                accountMap.Delete(key);
        }

        private static void PostTransfer(byte[] from, byte[] to, byte[] tokenId, object data)
        {
            OnTransfer(from, to, 1, tokenId);
            if (to != null && Blockchain.GetContract(to) != null)
                ((Action<string, object[]>)to.ToDelegate())("onNEP11Payment", new object[] { @from, 1, tokenId, data });

            //Contract.Call(to, "onNEP11Payment", CallFlags.All, from, 1, tokenId, data);
        }


        private protected static bool UpdateBalance(byte[] owner, BigInteger increment)
        {
            StorageMap balanceMap = Storage.CurrentContext.CreateMap(Prefix_Balance);
            BigInteger balance = balanceMap.Get(owner).ToBigInteger();
            balance += increment;
            if (balance < 0) return false;
            if (balance == 0)
                balanceMap.Delete(owner);
            else
                balanceMap.Put(owner, balance);
            return true;
        }


        protected static void Mint(byte[] tokenId, TokenState token)
        {
            StorageMap tokenMap = Storage.CurrentContext.CreateMap(Prefix_Token);
            tokenMap.Put(tokenId, token.Serialize());
            UpdateBalance(token.Owner, tokenId, +1);
            UpdateTotalSupply(+1);
            PostTransfer(null, token.Owner, tokenId, null);
        }



        private protected static void UpdateTotalSupply(BigInteger increment)
        {
            StorageContext context = Storage.CurrentContext;
            BigInteger totalSupply = Storage.Get(context, Prefix_TotalSupply).ToBigInteger();
            totalSupply += increment;
            Storage.Put(context, Prefix_TotalSupply, totalSupply);
        }

    }
}
