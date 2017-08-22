// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
using System.Runtime.InteropServices;

namespace System.Security.AccessControl
{
    [Flags]
    public enum InheritanceFlags
    {
        None                     = 0x00,
        ContainerInherit         = 0x01,
        ObjectInherit            = 0x02,
    }

    [Flags]
    public enum PropagationFlags
    {
        None                     = 0x00,
        NoPropagateInherit       = 0x01,
        InheritOnly              = 0x02,
    }

    [Flags]
    public enum AuditFlags
    {
        None                    = 0x00,
        Success                 = 0x01,
        Failure                 = 0x02,
    }

    [Flags]
    public enum SecurityInfos
    {
        Owner                  = 0x00000001,
        Group                  = 0x00000002,
        DiscretionaryAcl       = 0x00000004,
        SystemAcl              = 0x00000008,

    }


    public enum ResourceType
    {
        Unknown                = 0x00,
        FileObject             = 0x01,
        Service                = 0x02,
        Printer                = 0x03,
        RegistryKey            = 0x04,
        LMShare                = 0x05,
        KernelObject           = 0x06,
        WindowObject           = 0x07,
        DSObject               = 0x08,
        DSObjectAll            = 0x09,
        ProviderDefined        = 0x0A,
        WmiGuidObject          = 0x0B,
        RegistryWow6432Key     = 0x0C,
    }

    [Flags]
    public enum AccessControlSections {
        None = 0,
        Audit = 0x1,
        Access = 0x2,
        Owner = 0x4,
        Group = 0x8,
        All = 0xF
    }

    /// <summary>
    /// 指定对可保护对象允许的操作。
    /// </summary>
    [Flags]
    public enum AccessControlActions {
#if FEATURE_MACL
        /// <summary>
        /// 指定无访问权限。
        /// </summary>
        None = 0,
        /// <summary>
        /// 指定只读访问权限。
        /// </summary>
        View = 1,
        /// <summary>
        /// 指定只写访问权限
        /// </summary>
        Change = 2
#else
        None = 0
#endif
    }
}
