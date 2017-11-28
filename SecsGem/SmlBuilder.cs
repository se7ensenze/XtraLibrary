using System;
using System.Collections.Generic;

using System.Text;
using System.IO;

namespace XtraLibrary.SecsGem
{
    public class SmlBuilder
    {
        private SmlBuilder() { }

        #region "Convert SecsMessageBase to SML"

        public static string ToSmlString(SecsMessageBase msg)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("S{0}F{1} {2} [TID:={3}]", msg.Stream, msg.Function,(msg.NeedReply ? " W" : ""), msg.TransactionId));

            foreach (SecsItem item in msg.Items)
            {
                GetItemValueAsString(sb, item, 1);
            }

            string ret = sb.ToString();
            sb.Remove(0, sb.Length);

            return ret;
        }

        public static string GetItemFormatString(FormatCode fc)
        {
            string ret = "";

            switch (fc)
            { 
                case FormatCode.ASCII:
                    ret = "A";
                    break;
                case FormatCode.Binary :
                    ret = "B";
                    break;
                case FormatCode.Boolean:
                    ret = "TF";
                    break;
                case FormatCode.LIST:
                    ret = "L";
                    break;
                case FormatCode.MC:
                    ret = "C2";
                    break;
                case FormatCode.F4:
                case FormatCode.F8 :
                case FormatCode.I1:
                case FormatCode.I2 :
                case FormatCode.I4:
                case FormatCode.I8:
                case FormatCode.JIS8:
                case FormatCode.U1:
                case FormatCode.U2:
                case FormatCode.U4:
                case FormatCode.U8 :
                    ret = fc.ToString();
                    break;
            }

            return ret;
        }

        public static string GetSecsItemValue(SecsItem item)
        {
            StringBuilder sb = new StringBuilder();

            if ((item is SecsItemAscii) || (item is SecsItemMultiByteChar))
            {
                sb.Append("\"" + (string)item.Value + "\"");
            }
            else if (item is SecsItemBoolean)
            {
                GetItem_Boolean_ValueAsString(sb, (SecsItemBoolean)item);
            }
            else if (item is SecsItemBinary)
            {
                GetItem_Binary_ValueAsString(sb, (SecsItemBinary)item);
            }
            else if (item is SecsItemList)
            {                
                //nothing to get
            }
            else if (item is SecsItemU1)
            {
                GetItem_U1_ValueAsString(sb, (SecsItemU1)item);
            }
            else if (item is SecsItemU2)
            {
                GetItem_U2_ValueAsString(sb, (SecsItemU2)item);
            }
            else if (item is SecsItemU4)
            {
                GetItem_U4_ValueAsString(sb, (SecsItemU4)item);
            }
            else if (item is SecsItemU8)
            {
                GetItem_U8_ValueAsString(sb, (SecsItemU8)item);
            }
            else if (item is SecsItemI1)
            {
                GetItem_I1_ValueAsString(sb, (SecsItemI1)item);
            }
            else if (item is SecsItemI2)
            {
                GetItem_I2_ValueAsString(sb, (SecsItemI2)item);
            }
            else if (item is SecsItemI4)
            {
                GetItem_I4_ValueAsString(sb, (SecsItemI4)item);
            }
            else if (item is SecsItemI8)
            {
                GetItem_I8_ValueAsString(sb, (SecsItemI8)item);
            }
            else if (item is SecsItemF4)
            {
                GetItem_F4_ValueAsString(sb, (SecsItemF4)item);
            }
            else if (item is SecsItemF8)
            {
                GetItem_F8_ValueAsString(sb, (SecsItemF8)item);
            }

            return sb.ToString();
        }

        private static int GetItemLength(SecsItem item)
        {
            int ret = 0;

            if (item == null)
            {
                ret = 0;
            }
            else if (item.Value == null)
            {
                ret = 0;
            }
            else
            {
                Type t = item.Value.GetType();
                if (t.IsArray)
                {
                    ret = ((Array)(item.Value)).Length;
                }
                else if (item is SecsItemList)
                {
                    ret = ((SecsItemList)item).Value.Count;
                }
                else if (item.Value is string)
                {
                    ret = ((string)(item.Value)).Length;
                }
            }

            return ret;
        }

        private static void GetItemValueAsString(StringBuilder sb, SecsItem item, int tabCount)
        {
            sb.Append(Environment.NewLine);
            if (tabCount > 0)
            {
                sb.Append('\t', tabCount);
            }

            sb.Append(string.Format("< {0} [{1}] ", GetItemFormatString(item.Format), GetItemLength(item)));

            if ((item is SecsItemAscii) || (item is SecsItemMultiByteChar))
            {
                sb.Append("\"" + (string)item.Value + "\"");
            }
            else if (item is SecsItemBoolean)
            {
                GetItem_Boolean_ValueAsString(sb, (SecsItemBoolean)item);
            }
            else if (item is SecsItemBinary)
            {
                GetItem_Binary_ValueAsString(sb, (SecsItemBinary)item);
            }
            else if (item is SecsItemList)
            {
                SecsItemList list = (SecsItemList)item;

                GetItem_List_ValueAsString(sb, list, tabCount + 1);

                if (list.Value.Count > 0)
                {
                    sb.Append(Environment.NewLine);

                    if (tabCount > 0)
                    {
                        sb.Append('\t', tabCount);
                    }
                }

            }
            else if (item is SecsItemU1)
            {
                GetItem_U1_ValueAsString(sb, (SecsItemU1)item);
            }
            else if (item is SecsItemU2)
            {
                GetItem_U2_ValueAsString(sb, (SecsItemU2)item);
            }
            else if (item is SecsItemU4)
            {
                GetItem_U4_ValueAsString(sb, (SecsItemU4)item);
            }
            else if (item is SecsItemU8)
            {
                GetItem_U8_ValueAsString(sb, (SecsItemU8)item);
            }
            else if (item is SecsItemI1)
            {
                GetItem_I1_ValueAsString(sb, (SecsItemI1)item);
            }
            else if (item is SecsItemI2)
            {
                GetItem_I2_ValueAsString(sb, (SecsItemI2)item);
            }
            else if (item is SecsItemI4)
            {
                GetItem_I4_ValueAsString(sb, (SecsItemI4)item);
            }
            else if (item is SecsItemI8)
            {
                GetItem_I8_ValueAsString(sb, (SecsItemI8)item);
            }
            else if (item is SecsItemF4)
            {
                GetItem_F4_ValueAsString(sb, (SecsItemF4)item);
            }
            else if (item is SecsItemF8)
            {
                GetItem_F8_ValueAsString(sb, (SecsItemF8)item);
            }

            if (!(item is SecsItemList))
            {
                sb.Append(" ");
            }
            sb.Append(">");

        }

        private static void GetItem_List_ValueAsString(StringBuilder sb, SecsItemList item, int tabCount)
        {
            foreach (SecsItem subItem in item.Value)
            {
                GetItemValueAsString(sb, subItem, tabCount);
            }
        }

        private static void GetItem_Boolean_ValueAsString(StringBuilder sb, SecsItemBoolean item)
        {
            if (item.Value != null)
            {
                bool val;
                for (int i = 0; i < item.Value.Length; i++)
                {
                    val = item.Value[i];
                    if (i == 0)
                    {
                        sb.Append(val.ToString());
                    }
                    else
                    {
                        sb.Append(" " + val.ToString());
                    }
                }
            }
        }

        private static void GetItem_Binary_ValueAsString(StringBuilder sb, SecsItemBinary item)
        {
            if (item.Value != null)
            {
                byte val;
                for (int i = 0; i < item.Value.Length; i++)
                {
                    val = item.Value[i];
                    if (i == 0)
                    {
                        sb.Append(val.ToString("X2"));
                    }
                    else
                    {
                        sb.Append(" " + val.ToString("X2"));
                    }
                }
            }
        }

        private static void GetItem_U1_ValueAsString(StringBuilder sb, SecsItemU1 item)
        {
            if (item.Value != null)
            {
                byte val;
                for (int i = 0; i < item.Value.Length; i++)
                {
                    val = item.Value[i];
                    if (i == 0)
                    {
                        sb.Append(val.ToString());
                    }
                    else
                    {
                        sb.Append(" " + val.ToString());
                    }
                }
            }
        }

        private static void GetItem_U2_ValueAsString(StringBuilder sb, SecsItemU2 item)
        {
            if (item.Value != null)
            {
                ushort val;
                for (int i = 0; i < item.Value.Length; i++)
                {
                    val = item.Value[i];
                    if (i == 0)
                    {
                        sb.Append(val.ToString());
                    }
                    else
                    {
                        sb.Append(" " + val.ToString());
                    }
                }
            }
        }

        private static void GetItem_U4_ValueAsString(StringBuilder sb, SecsItemU4 item)
        {
            if (item.Value != null)
            {
                uint val;
                for (int i = 0; i < item.Value.Length; i++)
                {
                    val = item.Value[i];
                    if (i == 0)
                    {
                        sb.Append(val.ToString());
                    }
                    else
                    {
                        sb.Append(" " + val.ToString());
                    }
                }
            }
        }

        private static void GetItem_U8_ValueAsString(StringBuilder sb, SecsItemU8 item)
        {
            if (item.Value != null)
            {
                ulong val;
                for (int i = 0; i < item.Value.Length; i++)
                {
                    val = item.Value[i];
                    if (i == 0)
                    {
                        sb.Append(val.ToString());
                    }
                    else
                    {
                        sb.Append(" " + val.ToString());
                    }
                }
            }
        }

        private static void GetItem_I1_ValueAsString(StringBuilder sb, SecsItemI1 item)
        {
            if (item.Value != null)
            {
                sbyte val;
                for (int i = 0; i < item.Value.Length; i++)
                {
                    val = item.Value[i];
                    if (i == 0)
                    {
                        sb.Append(val.ToString());
                    }
                    else
                    {
                        sb.Append(" " + val.ToString());
                    }
                }
            }
        }

        private static void GetItem_I2_ValueAsString(StringBuilder sb, SecsItemI2 item)
        {
            if (item.Value != null)
            {
                short val;
                for (int i = 0; i < item.Value.Length; i++)
                {
                    val = item.Value[i];
                    if (i == 0)
                    {
                        sb.Append(val.ToString());
                    }
                    else
                    {
                        sb.Append(" " + val.ToString());
                    }
                }
            }
        }

        private static void GetItem_I4_ValueAsString(StringBuilder sb, SecsItemI4 item)
        {
            if (item.Value != null)
            {
                int val;
                for (int i = 0; i < item.Value.Length; i++)
                {
                    val = item.Value[i];
                    if (i == 0)
                    {
                        sb.Append(val.ToString());
                    }
                    else
                    {
                        sb.Append(" " + val.ToString());
                    }
                }
            }
        }

        private static void GetItem_I8_ValueAsString(StringBuilder sb, SecsItemI8 item)
        {
            if (item.Value != null)
            {
                long val;
                for (int i = 0; i < item.Value.Length; i++)
                {
                    val = item.Value[i];
                    if (i == 0)
                    {
                        sb.Append(val.ToString());
                    }
                    else
                    {
                        sb.Append(" " + val.ToString());
                    }
                }
            }
        }

        private static void GetItem_F4_ValueAsString(StringBuilder sb, SecsItemF4 item)
        {
            if (item.Value != null)
            {
                float val;
                for (int i = 0; i < item.Value.Length; i++)
                {
                    val = item.Value[i];
                    if (i == 0)
                    {
                        sb.Append(val.ToString());
                    }
                    else
                    {
                        sb.Append(" " + val.ToString());
                    }
                }
            }
        }

        private static void GetItem_F8_ValueAsString(StringBuilder sb, SecsItemF8 item)
        {
            if (item.Value != null)
            {
                double val;
                for (int i = 0; i < item.Value.Length; i++)
                {
                    val = item.Value[i];
                    if (i == 0)
                    {
                        sb.Append(val.ToString());
                    }
                    else
                    {
                        sb.Append(" " + val.ToString());
                    }
                }
            }
        }

        #endregion

        //not finished yet
        #region "Convert SML to SecsMessageBase"

        //*******************************
        //******** 2015-05-29 ***********
        //*******************************
        //  S1F14  [TID:=2]
        //  < L [2] 
        //      < B [1] 00 >
        //      < L [2] 
        //          < A [9] "LT-VISION" >
        //          < A [10] "3.05.08.66" >
        //      >
        //  >
        //*******************************

        //private SecsMessageBase ToSecsMessageBase(string sml)
        //{
        //    SecsMessageBase msg = null;
        //    StringReader reader = new StringReader(sml);
        //    char[] buff = new char[1];
        //    char c;
        //    SecsItem currentItem = null;
        //    string str = "";

        //    while (reader.Peek() > -1)
        //    {
        //        reader.Read(buff, 0, 1);
        //        c = buff[0];
        //        switch (c)
        //        { 
        //            case '<':
        //                currentItem = null;
        //                break;
        //            case '>':
        //                break;
        //            case ' ':
        //                break;
        //            case '\t':
        //                break;
        //            case '[':
        //                break;
        //            default:
        //                str = str + c;
        //                break;
        //        }
        //    }  
        //    return msg;
        //}

        //private void GetItem_LIST_AsSecsItem(StringReader reader, SecsItemList currentItem)
        //{ 
            
        //}

        private FormatCode GetItemFormatCode(string strFc)
        {
            FormatCode ret = FormatCode.LIST;

            switch (strFc)
            {
                case "A":
                    ret = FormatCode.ASCII;
                    break;
                case "B":
                    ret = FormatCode.Binary;
                    break;
                case "TF":
                    ret = FormatCode.Boolean;
                    break;
                case "L":
                    ret = FormatCode.LIST;
                    break;
                case "C2":
                    ret = FormatCode.MC;
                    break;
                case "F4":                    
                case "F8":
                case "I1":
                case "I2":
                case "I4":
                case "I8":
                case "JIS8":
                case "U1":
                case "U2":
                case "U4":
                case "U8":
                    ret = (FormatCode)Enum.Parse(typeof(FormatCode), strFc);
                    break;
            }

            return ret;
        }

        #endregion
    }
}
