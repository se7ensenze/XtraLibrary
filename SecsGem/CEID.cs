using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary.SecsGem
{
    public class CEID
    {
        public void AddReport(ReportVariable report)
        { 
            
        }
    }
    public class CEID<T> 
        : CEID where T : SecsItem
    {
        //POSIBLE TYPES OF CEID are
        //ASCII, I1, I2, I4, I8, U1, U2, U4, U8
        //reference document :141433-E005-00-0712.pdf
                


    }
}
