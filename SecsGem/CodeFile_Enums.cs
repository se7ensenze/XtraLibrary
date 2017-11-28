namespace XtraLibrary.SecsGem
{
    public enum GemProtocol
    {
        SECS_I = 0,
        HSMS = 1
    }

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

    public enum SType : byte
    {
        Message = 0,
        SelectReq = 1,
        SelectResp = 2,
        LinktestReq = 5,
        LinktestResp = 6,
        SeparateReq = 9
    }

    /// <summary>
    /// Bit 876543 of format code byte
    /// </summary>
    public enum FormatCode : byte
    {
        /// <summary>
        /// LIST (length in elements)
        /// </summary>
        LIST = 0,

        /// <summary>
        /// Binary
        /// </summary>
        Binary = 8,

        /// <summary>
        /// Boolean
        /// </summary>
        Boolean = 9,

        /// <summary>
        /// ASCII : Non-printing characters are equipment-specific
        /// </summary>
        ASCII = 16,

        /// <summary>
        /// JIS-8
        /// </summary>
        JIS8 = 17,

        /// <summary>
        /// Multi-byte charactor
        /// </summary>
        MC = 18,

        /// <summary>
        /// 8-byte integer (signed)
        /// </summary>
        I8 = 24,

        /// <summary>
        /// 1-byte integer (signed)
        /// </summary>
        I1 = 25,

        /// <summary>
        /// 2-byte integer (signed)
        /// </summary>
        I2 = 26,

        /// <summary>
        /// 4-byte integer (signed)
        /// </summary>
        I4 = 28,

        /// <summary>
        /// 8-byte floating poing
        /// </summary>
        F8 = 32,

        /// <summary>
        /// 4-byte floating poing
        /// </summary>
        F4 = 36,

        /// <summary>
        /// 8-byte integer (unsigned)
        /// </summary>
        U8 = 40,

        /// <summary>
        /// 1-byte integer (unsigned)
        /// </summary>
        U1 = 41,

        /// <summary>
        /// 2-byte integer (unsigned)
        /// </summary>
        U2 = 42,

        /// <summary>
        /// 4-byte integer (unsigned)
        /// </summary>
        U4 = 44
    }

    public enum CategoryOfMessage :byte
    { 
        NotUse = 0,
        EquipmentStatus = 1,
        EquipmentControlAndDiagnostics = 2,
        MaterialsStatus = 3,
        MaterialControl = 4,
        ExceptionHandling = 5,
        DataCollection = 6,
        ProcessProgramManagement = 7,
        ControlProgramTransfer = 8,
        SystemError = 9,
        TerminalService = 10,
        HostFileService = 11,
        WaferMapping = 12,
        DataSetTransafer = 13,
        ObjectServices = 14,
        RecipeManagement = 15,
        ProcessingManagement = 16,
        EquipmentControlAndDiagnostics_2 = 17,
        SubSystemControlAndData = 18,
        RecipeAndParameterManagement = 19

    }
    
    public enum DirectionType
    {
        Sent = 0,
        Recv = 1
    }
}