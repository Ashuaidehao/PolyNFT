using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo.SmartContract.Framework;

namespace PolyNFT
{
    public class TokenState:Nep11TokenState
    {
        public ByteString TokenId;
    }
}
