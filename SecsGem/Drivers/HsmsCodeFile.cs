namespace XtraLibrary.SecsGem.Drivers
{
    public enum HsmsConnectProcedure
    { 
        PASSIVE = 0,
        ACTIVE = 1
    }

    public enum HsmsState
    { 
        NOT_CONNECTED = 0,
        NOT_SELECTED = 1,
        SELECTED = 2
    }

    public enum SType :byte
    { 
        Message = 0,
        SelectReq = 1,
        SelectResp = 2,
        LinktestReq = 5,
        LinktestResp = 6,
        SeparateReq = 9
    }

    public enum SecsIContentionType
    { 
        Slave = 0,
        Master = 1
    }
}