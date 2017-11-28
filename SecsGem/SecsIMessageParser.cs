using System;
using System.Collections.Generic;

using System.Text;
using System.IO;

namespace XtraLibrary.SecsGem
{
    public class SecsIMessageParser
        :SecsMessageParserBase
    {
        public override SecsMessageBase ToSecsMessage(byte[] data)
        {
            byte[] header = new byte[10];
            Array.Copy(data, 0, header, 0, header.Length);
            //header
            //[0][1][2][3][4][5][6][7][8][9] ...
            //[0][1]
            byte[] tmp2bytes = new byte[2];
            Array.Copy(header, 0, tmp2bytes, 0, tmp2bytes.Length);
            Array.Reverse(tmp2bytes);
            ushort deviceId = BitConverter.ToUInt16(tmp2bytes, 0);
            //       [2]
            byte stream = (byte)(header[2] & 0x7F); //0111 1111
            bool needReply = (0x80 == (byte)(header[2] & 0x80));
            //          [3]
            byte function = header[3];
            bool isPrimary = (function % 2 == 1);
            //             [4][5]
            Array.Copy(header, 4, tmp2bytes, 0, tmp2bytes.Length);
            bool isLastBlock = (0x80 == (tmp2bytes[0] & 0x80));                       
            Array.Reverse(tmp2bytes); //reverse for convert by BitConvertor
            tmp2bytes[0] = (byte)(tmp2bytes[0] & 0x7F);
            ushort blockNo = BitConverter.ToUInt16(tmp2bytes, 0);
            //                   [6][7][8][9]
            byte[] temp4Bytes = new byte[4];
            Array.Copy(header, 6, temp4Bytes, 0, temp4Bytes.Length);
            Array.Reverse(temp4Bytes);
            uint transId = BitConverter.ToUInt32(temp4Bytes, 0);  
            
            SecsMessageBase msg = GetSecsMessageInstance(stream, function, needReply);
                        
            if (data != null && data.Length > 10)
            {
                using (MemoryStream reader = new MemoryStream(data))
                {
                    reader.Position = header.Length;
                    msg.ReadItems(reader);
                }
            }

            msg.TransactionId = transId;
            msg.DeviceId = deviceId;

            return msg;
        }

        public override byte[] GetBytes(SecsMessageBase message)
        {
            byte[] headerBytes = new byte[10];
            //device id ** let system manage
            byte[] deviceIdBytes = BitConverter.GetBytes(message.DeviceId);
            Array.Reverse(deviceIdBytes);

            headerBytes[0] = deviceIdBytes[0];
            headerBytes[1] = deviceIdBytes[1];
            //stream and w-bit
            if (message.NeedReply)
            {
                headerBytes[2] = (byte)(message.Stream | 0x80);
            }
            else
            {
                headerBytes[2] = (byte)(message.Stream & 0x7F);
            }
            //function
            headerBytes[3] = message.Function;
            //BLOCK NO
            headerBytes[4] = 0;
            headerBytes[5] = 0; 
            //transaction id ** let system manage

            byte[] transactionIdBytes = BitConverter.GetBytes(message.TransactionId);
            Array.Reverse(transactionIdBytes);

            headerBytes[6] = transactionIdBytes[0];
            headerBytes[7] = transactionIdBytes[1];
            headerBytes[8] = transactionIdBytes[2];
            headerBytes[9] = transactionIdBytes[3];

            List<byte[]> itemArray = new List<byte[]>();

            int itemByteCount = 0;

            byte[] tmp1;
            foreach (SecsItem item in message.Items)
            {
                tmp1 = item.ToBytes();
                itemArray.Add(tmp1);
                itemByteCount += tmp1.Length;
            }

            byte[] allBytes = new byte[headerBytes.Length + itemByteCount];

            int index = 0;

            Array.Copy(headerBytes, 0, allBytes, 0, headerBytes.Length);
            index += headerBytes.Length;

            byte[] tmp2;
            for (int i = 0; i < itemArray.Count; i++)
            {
                tmp2 = itemArray[i];
                Array.Copy(tmp2, 0, allBytes, index, tmp2.Length);
                index += tmp2.Length;
            }

            itemArray.Clear();

            return allBytes;
        }
    }
}
