using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary.SecsGem
{
    /// <summary>
    /// Common SecsMessage : this class can not be inherite
    /// </summary>
    public sealed class SecsMessage :SecsMessageBase 
    {
        public SecsMessage(byte s, byte f, bool w)
            :base(s, f, w)
        {             
        }
    }
}
