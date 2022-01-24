using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services;

namespace PolyNFT
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
                onFault(message);
                ExecutionEngine.Assert(false);
            }
        }

        /// <summary>
        /// Is Valid and not Zero address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool IsAddress(UInt160 address)
        {
            return address.IsValid && !address.IsZero;
        }


     
    }
}
