using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo.SmartContract.Framework.Services.Neo;

namespace PolyNFTLegacy
{
    public partial class PolyNFT
    {
        /// <summary>
        /// 断言
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                Runtime.Notify("Fault:" + message);
                throw new Exception(message);
            }
        }

        private static bool IsAddress(byte[] address)
        {
            return address != null && address.Length == 20;
        }
    }
}
