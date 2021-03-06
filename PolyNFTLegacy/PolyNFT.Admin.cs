using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;

namespace PolyNFTLegacy
{
    public partial class PolyNFT
    {
        static readonly byte[] superAdmin = "AVB7PZUpfZShoP8ih4krcCV5Z1SdpxQX3B".ToScriptHash();


        public static bool Verify() => Runtime.CheckWitness(GetAdmin());


        /// <summary>
        /// 获取合约管理员
        /// </summary>
        /// <returns></returns>
        public static byte[] GetAdmin()
        {
            var admin = Storage.Get(AdminKey);
            return admin.Length == 20 ? admin : superAdmin;
        }

        /// <summary>
        /// 设置合约管理员
        /// </summary>
        /// <param name="admin"></param>
        /// <returns></returns>
        public static bool SetAdmin(byte[] admin)
        {
            Assert(admin.Length == 20, "NewAdmin Invalid");
            Assert(Runtime.CheckWitness(GetAdmin()), "Forbidden");
            Storage.Put(AdminKey, admin);
            return true;
        }

        #region Upgrade

        public static byte[] Upgrade(byte[] newScript, byte[] paramList, byte returnType, ContractPropertyState cps, string name, string version, string author, string email, string description)
        {
            Assert(Runtime.CheckWitness(GetAdmin()), "upgrade: CheckWitness failed!");

            //var me = ExecutionEngine.ExecutingScriptHash;
            byte[] newContractHash = Hash160(newScript);
            Assert(Blockchain.GetContract(newContractHash).Serialize().Equals(new byte[] { 0x00, 0x00 }), "upgrade: The contract already exists");

            Contract newContract = Contract.Migrate(newScript, paramList, returnType, cps, name, version, author, email, description);
            Runtime.Notify("upgrade", ExecutionEngine.ExecutingScriptHash, newContractHash);
            return newContractHash;
        }


        #endregion
    }
}
