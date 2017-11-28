using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary.SecsGem
{
    public class GemOption
    {
        private ushort m_DeviceId;
        private GemProtocol m_Protocol;
        private SecsIParameters m_SecsIParameters;
        private HsmsParameters m_HsmsParameters;

        public GemOption() {
            m_DeviceId = 0;
            m_Protocol = GemProtocol.HSMS;
        }

        public ushort DeviceId
        {
            get {
                return m_DeviceId;
            }
            set
            {
                m_DeviceId = value;
            }
        }

        public GemProtocol Protocol
        {
            get
            {
                return m_Protocol;
            }
            set {
                m_Protocol = value;
            }
        }

        public SecsIParameters SecsIParameters
        {
            get
            {
                return m_SecsIParameters;
            }
            set {
                m_SecsIParameters = value;
            }
        }

        public HsmsParameters HsmsParameters
        {
            get {
                return m_HsmsParameters;
            }
            set {
                m_HsmsParameters = value;
            }
        }
    }
}
