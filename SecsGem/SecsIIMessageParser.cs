using System;
using System.Collections.Generic;

using System.Text;
using System.IO;

namespace XtraLibrary.SecsGem
{
    public class SecsIIMessageParser
        :SecsMessageParserBase
    {

        public SecsIIMessageParser()
            : base()
        {
        }

        public override SecsMessageBase ToSecsMessage(byte[] data)
        {
            using (MemoryStream reader = new MemoryStream(data))
            {
                reader.Position = 0;

                byte[] lengthBytes = new byte[4];

                //get length byte
                reader.Read(lengthBytes, 0, lengthBytes.Length);
                Array.Reverse(lengthBytes);

                int dataLength = BitConverter.ToInt32(lengthBytes, 0);
                if (data.Length != dataLength + 4)
                {
                    //invalid data length
                    throw new Exception("Invalid data lenght");
                }

                //get header
                byte[] header = new byte[10];
                reader.Read(header, 0, header.Length);

                //get device id from header
                byte[] deviceIdBytes = new byte[2];
                Array.Copy(header, 0, deviceIdBytes, 0, 2);
                Array.Reverse(deviceIdBytes);
                ushort deviceId = BitConverter.ToUInt16(deviceIdBytes, 0);

                //get stream
                byte stream = (byte)(header[2] & 0x7F);
                byte function = header[3];

                bool needReply = ((header[2] & 0x80) == 0x80);

                //transactionId.
                byte[] transactionIdBytes = new byte[4];
                Array.Copy(header, 6, transactionIdBytes, 0, transactionIdBytes.Length);
                Array.Reverse(transactionIdBytes);
                uint transactionId = BitConverter.ToUInt32(transactionIdBytes, 0);

                SecsMessageBase msg = GetSecsMessageInstance(stream, function, needReply);

                msg.ReadItems(reader);                          //Sec2 data read
                msg.NeedReply = needReply;
                msg.TransactionId = transactionId;
                msg.DeviceId = deviceId;

                return msg;
            }
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
            //ptype
            headerBytes[4] = 0;
            //stype
            headerBytes[5] = 0; //0 - mean to "SescSessionType.Message"
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

            byte[] lengthBytes = BitConverter.GetBytes(headerBytes.Length + itemByteCount); //4-byte integer

            Array.Reverse(lengthBytes);

            byte[] allBytes = new byte[lengthBytes.Length + headerBytes.Length + itemByteCount];

            int index = 0;

            Array.Copy(lengthBytes, 0, allBytes, 0, lengthBytes.Length);
            index += lengthBytes.Length;

            Array.Copy(headerBytes, 0, allBytes, lengthBytes.Length, headerBytes.Length);
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
