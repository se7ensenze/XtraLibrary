using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary.SecsGem
{
    public abstract class SecsMessageParserBase
    {
        public abstract SecsMessageBase ToSecsMessage(byte[] data);
        public abstract byte[] GetBytes(SecsMessageBase msg);

        private Dictionary<byte, Dictionary<byte, Type>> m_RegisterCustomTypeDict;

        protected SecsMessageParserBase() {
            m_RegisterCustomTypeDict = new Dictionary<byte, Dictionary<byte, Type>>();
        }

        protected SecsMessageBase GetSecsMessageInstance(byte stream, byte function, bool needReply)
        {
            Dictionary<byte, Type> dict;
            Type msgType;
            SecsMessageBase msg = null;
            if (m_RegisterCustomTypeDict.TryGetValue(stream, out dict)
                && dict.TryGetValue(function, out msgType))
            {
                msg = (SecsMessageBase)Activator.CreateInstance(msgType);
            }
            else
            {
                msg = new SecsMessage(stream, function, needReply);
            }

            return msg;
        }

        /// <summary>
        /// In case of UserDefined class that inherit SecsMessageBase 
        ///  and want to Parser to convert to UserDefined type instead of SecsMessage
        /// </summary>
        /// <param name="messageType">UserDefined type that inherited from SecsMessageBase</param>
        public void RegisterCustomSecsMessage(Type messageType)
        {

            try
            {


                if (messageType.IsSubclassOf(typeof(SecsMessageBase)) &&
                    !messageType.IsAbstract)
                {




                    SecsMessageBase tmp = (SecsMessageBase)Activator.CreateInstance(messageType);
                    Dictionary<byte, Type> dict;

                    if (!m_RegisterCustomTypeDict.TryGetValue(tmp.Stream, out dict))
                    {
                        dict = new Dictionary<byte, Type>();
                        dict.Add(tmp.Function, messageType);
                        m_RegisterCustomTypeDict.Add(tmp.Stream, dict);
                    }
                    else
                    {
                        if (dict.ContainsKey(tmp.Function))
                        {
                            //replace the same function
                            dict.Remove(tmp.Function);
                        }
                        dict.Add(tmp.Function, messageType);
                    }

                }
            
                                
            
            }
            catch
            {
                throw new Exception("RegisterCustomSecsMessage Error");      // 160715 \783 Catch RegiserSecsCustom
            
            }

           

         
        
        
        
        
        
        
        
        }
      
        
       

        }

}
