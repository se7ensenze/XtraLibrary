using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary.SecsGem
{
    public class FormatCodeHelper
    {
        private FormatCodeHelper() { }
   
        public static FormatCode GetFormatCode(byte formatCodeByte, out byte lengthByteCount){
            
            //bit            : 8765 4321
            //formatCodeByte : 0000 00
            //lenght byte    :        00

            byte formatCodeVal = (byte)(formatCodeByte >> 2);                        

            lengthByteCount = (byte)(formatCodeByte & 0x03); //0000 0011 make bit "8764 43" to 0

            if (!Enum.IsDefined(typeof(FormatCode), formatCodeVal))
            {
                throw new Exception("Unknow format code :" + formatCodeVal.ToString());
            }

            return (FormatCode)formatCodeVal;

        }

    }
}
