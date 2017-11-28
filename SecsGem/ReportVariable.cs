using System;
using System.Collections.Generic;
using System.Text;

namespace XtraLibrary.SecsGem
{
    public class ReportVariable
    {
        protected object m_RPTID;

        public object ID
        {
            get {
                return m_RPTID;
            }
        }

        internal ReportVariable(object rptId)
        {
            m_RPTID = rptId;
        }


    }

    public sealed class ReportVariable<RPTID>
        :ReportVariable 
    {
        public new RPTID ID
        {
            get {
                return (RPTID)m_RPTID;
            }
        }
        public ReportVariable(RPTID rptId)
            : base(rptId)
        {        
        }
    }
}
