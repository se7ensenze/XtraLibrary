using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary.SecsGem
{
    public class SecsItemFactory
    {
        public static SecsItem Create(string name, FormatCode format)
        {
            SecsItem item = null;

            switch (format)
            {
                case FormatCode.ASCII:
                    item = new SecsItemAscii(name);
                    break;
                case FormatCode.Binary:
                    item = new SecsItemBinary(name);
                    break;
                case FormatCode.Boolean:
                    item = new SecsItemBoolean(name);
                    break;
                case FormatCode.MC:
                    break;
                case FormatCode.F4:
                    item = new SecsItemF4(name);
                    break;
                case FormatCode.F8:
                    item = new SecsItemF8(name);
                    break;
                case FormatCode.I1:
                    item = new SecsItemI1(name);
                    break;
                case FormatCode.I2:
                    item = new SecsItemI2(name);
                    break;
                case FormatCode.I4:
                    item = new SecsItemI4(name);
                    break;
                case FormatCode.I8:
                    item = new SecsItemI8(name);
                    break;
                case FormatCode.JIS8:
                    break;
                case FormatCode.LIST:
                    item = new SecsItemList(name);
                    break;
                case FormatCode.U1:
                    item = new SecsItemU1(name);
                    break;
                case FormatCode.U2:
                    item = new SecsItemU2(name);
                    break;
                case FormatCode.U4:
                    item = new SecsItemU4(name);
                    break;
                case FormatCode.U8:
                    item = new SecsItemU8(name);
                    break;
                default :
                    throw new Exception("Not support item format code");
            }

            return item;
        }

        /// <summary>
        /// Create SecsItem by value typs { U1, U2, U4, U8, I1, I2, I4, I8, F4, F8 }
        /// </summary>
        /// <param name="name">SecsItem name</param>
        /// <param name="value">value</param>
        /// <returns>SecsItem</returns>
        public static SecsItem Create(string name, object numericValue)
        {
            Type valueType = numericValue.GetType();

            return Create(name, valueType, numericValue);
        }

        public static SecsItem Create(string name, Type valueType)
        {
            return Create(name, valueType, Activator.CreateInstance(valueType));
        }

        public static SecsItem Create(string name, Type valueType, object numericValue)
        {
            SecsItem item = null;

            if (valueType.Equals(typeof(byte)))
            {
                item = new SecsItemU1(name, new byte[] { (byte)numericValue });
            }
            else if (valueType.Equals(typeof(byte[])))
            {
                item = new SecsItemU1(name, (byte[])numericValue);
            }
            else if (valueType.Equals(typeof(ushort)))
            {
                item = new SecsItemU2(name, new ushort[] { (ushort)numericValue });
            }
            else if (valueType.Equals(typeof(ushort[])))
            {
                item = new SecsItemU2(name, (ushort[])numericValue);
            }
            else if (valueType.Equals(typeof(uint)))
            {
                item = new SecsItemU4(name, new uint[] { (uint)numericValue });
            }
            else if (valueType.Equals(typeof(uint[])))
            {
                item = new SecsItemU4(name, (uint[])numericValue);
            }
            else if (valueType.Equals(typeof(ulong)))
            {
                item = new SecsItemU8(name, new ulong[] { (ulong)numericValue });
            }
            else if (valueType.Equals(typeof(ulong[])))
            {
                item = new SecsItemU8(name, (ulong[])numericValue);
            }
            else if (valueType.Equals(typeof(sbyte)))
            {
                item = new SecsItemI1(name, new sbyte[] { (sbyte)numericValue });
            }
            else if (valueType.Equals(typeof(sbyte[])))
            {
                item = new SecsItemI1(name, (sbyte[])numericValue);
            }
            else if (valueType.Equals(typeof(short)))
            {
                item = new SecsItemI2(name, new short[] { (short)numericValue });
            }
            else if (valueType.Equals(typeof(short[])))
            {
                item = new SecsItemI2(name, (short[])numericValue);
            }
            else if (valueType.Equals(typeof(int)))
            {
                item = new SecsItemI4(name, new int[] { (int)numericValue });
            }
            else if (valueType.Equals(typeof(int[])))
            {
                item = new SecsItemI4(name, (int[])numericValue);
            }
            else if (valueType.Equals(typeof(long)))
            {
                item = new SecsItemI8(name, new long[] { (long)numericValue });
            }
            else if (valueType.Equals(typeof(long[])))
            {
                item = new SecsItemI8(name, (long[])numericValue);
            }
            else if (valueType.Equals(typeof(float)))
            {
                item = new SecsItemF4(name, new float[] { (float)numericValue });
            }
            else if (valueType.Equals(typeof(float[])))
            {
                item = new SecsItemF4(name, (float[])numericValue);
            }
            else if (valueType.Equals(typeof(double)))
            {
                item = new SecsItemF8(name, new double[] { (double)numericValue });
            }
            else if (valueType.Equals(typeof(double[])))
            {
                item = new SecsItemF8(name, (double[])numericValue);
            }
            else
            {
                throw new Exception("Not supported value type :" + valueType.Name);
            }
            return item;
        }

        private SecsItemFactory() { 
        }
    }
}
