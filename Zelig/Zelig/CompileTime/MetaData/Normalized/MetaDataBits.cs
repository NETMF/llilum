//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData
{
    using System;

    // MetaDataBits
    //
    // definitions for various bit-level flags, masks
    // derived from //urtdist/builds/src/2727/Lightning/Src/inc/CorHdr.h

    public enum NativeTypes : byte
    {
        END             = 0x00,    //DEPRECATED
        VOID            = 0x01,    //DEPRECATED
        BOOLEAN         = 0x02,    // (4 byte boolean value: TRUE = non-zero, FALSE = 0)
        I1              = 0x03,
        U1              = 0x04,
        I2              = 0x05,
        U2              = 0x06,
        I4              = 0x07,
        U4              = 0x08,
        I8              = 0x09,
        U8              = 0x0A,
        R4              = 0x0B,
        R8              = 0x0C,
        SYSCHAR         = 0x0D,   //DEPRECATED
        VARIANT         = 0x0E,   //DEPRECATED
        CURRENCY        = 0x0F,
        PTR             = 0x10,   //DEPRECATED
        DECIMAL         = 0x11,   //DEPRECATED
        DATE            = 0x12,   //DEPRECATED
        BSTR            = 0x13,
        LPSTR           = 0x14,
        LPWSTR          = 0x15,
        LPTSTR          = 0x16,
        FIXEDSYSSTRING  = 0x17,
        OBJECTREF       = 0x18,   //DEPRECATED
        IUNKNOWN        = 0x19,
        IDISPATCH       = 0x1A,
        STRUCT          = 0x1B,
        INTF            = 0x1C,
        SAFEARRAY       = 0x1D,
        FIXEDARRAY      = 0x1E,
        INT             = 0x1F,
        UINT            = 0x20,
        //////////////////////////@todo: sync up the spec
        NESTEDSTRUCT    = 0x21, //DEPRECATED (use STRUCT)
        BYVALSTR        = 0x22,
        ANSIBSTR        = 0x23,
        TBSTR           = 0x24,   // select BSTR or ANSIBSTR depending on platform
        VARIANTBOOL     = 0x25,   // (2-byte boolean value: TRUE = -1, FALSE = 0)
        FUNC            = 0x26,
        ASANY           = 0x28,
        ARRAY           = 0x2A,
        LPSTRUCT        = 0x2B,
        CUSTOMMARSHALER = 0x2C, // Custom marshaler native type. This must be followed
        ////////////////////////// by a string of the following format:
        ////////////////////////// "Native type name/0Custom marshaler type name/0Optional cookie/0"
        ////////////////////////// Or
        ////////////////////////// "{Native type GUID}/0Custom marshaler type name/0Optional cookie/0"
        ERROR           = 0x2D,   // This native type coupled with ELEMENT_TYPE_I4 will map to VT_HRESULT
        MAX             = 0x50    // first invalid element type
    }

    public enum VariantTypes
    {
        VT_EMPTY           = 0,
        VT_NULL            = 1,
        VT_I2              = 2,
        VT_I4              = 3,
        VT_R4              = 4,
        VT_R8              = 5,
        VT_CY              = 6,
        VT_DATE            = 7,
        VT_BSTR            = 8,
        VT_DISPATCH        = 9,
        VT_ERROR           = 10,
        VT_BOOL            = 11,
        VT_VARIANT         = 12,
        VT_UNKNOWN         = 13,
        VT_DECIMAL         = 14,
        VT_I1              = 16,
        VT_UI1             = 17,
        VT_UI2             = 18,
        VT_UI4             = 19,
        VT_I8              = 20,
        VT_UI8             = 21,
        VT_INT             = 22,
        VT_UINT            = 23,
        VT_VOID            = 24,
        VT_HRESULT         = 25,
        VT_PTR             = 26,
        VT_SAFEARRAY       = 27,
        VT_CARRAY          = 28,
        VT_USERDEFINED     = 29,
        VT_LPSTR           = 30,
        VT_LPWSTR          = 31,
        VT_RECORD          = 36,
        VT_FILETIME        = 64,
        VT_BLOB            = 65,
        VT_STREAM          = 66,
        VT_STORAGE         = 67,
        VT_STREAMED_OBJECT = 68,
        VT_STORED_OBJECT   = 69,
        VT_BLOB_OBJECT     = 70,
        VT_CF              = 71,
        VT_CLSID           = 72,
        VT_BSTR_BLOB       = 0x0FFF,
        VT_VECTOR          = 0x1000,
        VT_ARRAY           = 0x2000,
        VT_BYREF           = 0x4000,
        VT_RESERVED        = 0x8000,
        VT_ILLEGAL         = 0xFFFF,
        VT_ILLEGALMASKED   = 0x0FFF,
        VT_TYPEMASK        = 0x0FFF
    };

    public enum TokenType : byte
    {
        Module                 = 0x00,
        TypeRef                = 0x01,
        TypeDef                = 0x02,
        FieldPtr               = 0x03, // Not Supported
        Field                  = 0x04,
        MethodPtr              = 0x05, // Not Supported
        Method                 = 0x06,
        ParamPtr               = 0x07, // Not Supported
        Param                  = 0x08,
        InterfaceImpl          = 0x09,
        MemberRef              = 0x0A,
        Constant               = 0x0B,
        CustomAttribute        = 0x0C,
        FieldMarshal           = 0x0D,
        DeclSecurity           = 0x0E,
        ClassLayout            = 0x0F,
        FieldLayout            = 0x10,
        StandAloneSig          = 0x11,
        EventMap               = 0x12,
        EventPtr               = 0x13, // Not Supported
        Event                  = 0x14,
        PropertyMap            = 0x15,
        PropertyPtr            = 0x16, // Not Supported
        Property               = 0x17,
        MethodSemantics        = 0x18,
        MethodImpl             = 0x19,
        ModuleRef              = 0x1A,
        TypeSpec               = 0x1B,
        ImplMap                = 0x1C,
        FieldRVA               = 0x1D,
        ENCLog                 = 0x1E, // Not Supported
        ENCMap                 = 0x1F, // Not Supported
        Assembly               = 0x20,
        AssemblyProcessor      = 0x21, // Ignored
        AssemblyOS             = 0x22, // Ignored
        AssemblyRef            = 0x23,
        AssemblyRefProcessor   = 0x24, // Ignored
        AssemblyRefOS          = 0x25, // Ignored
        File                   = 0x26,
        ExportedType           = 0x27, // Not Supported
        ManifestResource       = 0x28,
        NestedClass            = 0x29,
        GenericParam           = 0x2A,
        MethodSpec             = 0x2B,
        GenericParamConstraint = 0x2C,

        Count                  = GenericParamConstraint + 1,

        // Common synonyms.
        FieldDef               = Field,
        MethodDef              = Method,
        ParamDef               = Param,
        Permission             = DeclSecurity,
        Signature              = StandAloneSig,

        String                 = 0x70,
        Illegal                = 0xFF,
    }

    //--//

    //
    // Section 23.1.1 of ECMA spec, Partition II
    //
    public enum HashAlgorithmID
    {
        None = 0x0000,
        MD5  = 0x8003,
        SHA1 = 0x8004
    }

    //
    // Section 23.1.2 of ECMA spec, Partition II
    //
    [Flags]
    public enum AssemblyFlags
    {
        PublicKey                  = 0x0001, // The assembly ref holds the full (unhashed) public key.
        CompatibilityMask          = 0x0070,
        SideBySideCompatible       = 0x0000, // The assembly is side by side compatible.
        NonSideBySideAppDomain     = 0x0010, // The assembly cannot execute with other versions if they are executing in the same application domain.
        NonSideBySideProcess       = 0x0020, // The assembly cannot execute with other versions if they are executing in the same process.
        NonSideBySideMachine       = 0x0030, // The assembly cannot execute with other versions if they are executing on the same machine.
        EnableJITcompileTracking   = 0x8000, // From "DebuggableAttribute".
        DisableJITcompileOptimizer = 0x4000  // From "DebuggableAttribute".
    }

    //
    // Section 23.1.4 of ECMA spec, Partition II
    //
    [Flags]
    public enum EventAttributes : ushort
    {
        SpecialName   = 0x0200, // Event is special.  Name describes how.
        ReservedMask  = 0x0400, // Reserved flags for Runtime use only.
        RTSpecialName = 0x0400  // Runtime (metadata internal APIs) should check name encoding.
    }

    //
    // Section 23.1.5 of ECMA spec, Partition II
    //
    [Flags]
    public enum FieldAttributes : ushort
    {
        FieldAccessMask = 0x0007, // member access mask - Use this mask to retrieve accessibility information.
        PrivateScope    = 0x0000, // Member not referenceable.
        Private         = 0x0001, // Accessible only by the parent type.
        FamANDAssem     = 0x0002, // Accessible by sub-types only in this Assembly.
        Assembly        = 0x0003, // Accessibly by anyone in the Assembly.
        Family          = 0x0004, // Accessible only by type and sub-types.
        FamORAssem      = 0x0005, // Accessibly by sub-types anywhere, plus anyone in assembly.
        Public          = 0x0006, // Accessibly by anyone who has visibility to this scope.
                                  // end member access mask

                                  // field contract attributes.
        Static          = 0x0010, // Defined on type, else per instance.
        InitOnly        = 0x0020, // Field may only be initialized, not written to after init.
        Literal         = 0x0040, // Value is compile time constant.
        NotSerialized   = 0x0080, // Field does not have to be serialized when type is remoted.
        SpecialName     = 0x0200, // field is special.  Name describes how.

                                  // interop attributes
        PinvokeImpl     = 0x2000, // Implementation is forwarded through pinvoke.

                                  // Reserved flags for runtime use only.
        ReservedMask    = 0x9500,
        RTSpecialName   = 0x0400, // Runtime(metadata internal APIs) should check name encoding.
        HasFieldMarshal = 0x1000, // Field has marshalling information.
        HasDefault      = 0x8000, // Field has default.
        HasFieldRVA     = 0x0100  // Field has RVA.
    }

    //
    // Section 23.1.6 of ECMA spec, Partition II
    //
    [Flags]
    public enum FileAttributes
    {
        ContainsMetaData   = 0x0000, // This is not a resource file,
        ContainsNoMetaData = 0x0001  // This is a resource file or other non-metadata-containing file
    }

    //
    // Section 23.1.7 of ECMA spec, Partition II
    //
    [Flags]
    public enum GenericParameterAttributes : ushort
    {
        VarianceMask                   = 0x0003,
        NonVariant                     = 0x0000, // The generic parameter is non-variant
        Covariant                      = 0x0001, // The generic parameter is covariant
        Contravariant                  = 0x0002, // The generic parameter is contravariant

        SpecialConstraintMask          = 0x001C,
        ReferenceTypeConstraint        = 0x0004, // The generic parameter has the class special constraint
        NotNullableValueTypeConstraint = 0x0008, // The generic parameter has the valuetype special constraint
        DefaultConstructorConstraint   = 0x0010, // The generic parameter has the .ctor special constraint
    }

    //
    // Section 23.1.8 of ECMA spec, Partition II
    //
    [Flags]
    public enum ImplementationMapAttributes : ushort
    {
        NoMangle                    = 0x0001, // Pinvoke is to use the member name as specified.

                                              // Character set flags
        CharSetMask                 = 0x0006, // Use this mask to retrieve the CharSet information.
        CharSetNotSpec              = 0x0000,
        CharSetAnsi                 = 0x0002,
        CharSetUnicode              = 0x0004,
        CharSetAuto                 = 0x0006,

        SupportsLastError           = 0x0040, // Information about target function. Not relevant for fields.

                                              // None of the calling convention flags is relevant for fields.
        CallConvMask                = 0x0700,
        CallConvWinapi              = 0x0100, // Pinvoke will use native callconv appropriate to target windows platform.
        CallConvCdecl               = 0x0200,
        CallConvStdcall             = 0x0300,
        CallConvThiscall            = 0x0400, // In M9, pinvoke will raise exception.
        CallConvFastcall            = 0x0500
    }

    //
    // Section 23.1.9 of ECMA spec, Partition II
    //
    [Flags]
    public enum ManifestResourceAttributes
    {
        VisibilityMask = 0x0007,
        Public         = 0x0001, // The Resource is exported from the Assembly.
        Private        = 0x0002  // The Resource is private to the Assembly.
    }

    //
    // Section 23.1.10 of ECMA spec, Partition II
    //
    [Flags]
    public enum MethodAttributes : ushort
    {
                                              // member access attributes
        MemberAccessMask            = 0x0007, // Use this mask to retrieve accessibility information.
        PrivateScope                = 0x0000, // Member not referenceable.
        Private                     = 0x0001, // Accessible only by the parent type.
        FamANDAssem                 = 0x0002, // Accessible by sub-types only in this Assembly.
        Assem                       = 0x0003, // Accessibly by anyone in the Assembly.
        Family                      = 0x0004, // Accessible only by type and sub-types.
        FamORAssem                  = 0x0005, // Accessibly by sub-types anywhere, plus anyone in assembly.
        Public                      = 0x0006, // Accessibly by anyone who has visibility to this scope.

                                              // method contract attributes.
        Static                      = 0x0010, // Defined on type, else per instance.
        Final                       = 0x0020, // Method may not be overridden.
        Virtual                     = 0x0040, // Method virtual.
        HideBySig                   = 0x0080, // Method hides by name+sig, else just by name.

                                              // vtable layout mask - Use this mask to retrieve vtable attributes.
        VtableLayoutMask            = 0x0100,
        ReuseSlot                   = 0x0000, // The default.
        NewSlot                     = 0x0100, // Method always gets a new slot in the vtable.

        Strict                      = 0x0200,

                                              // method implementation attributes.
        Abstract                    = 0x0400, // Method does not provide an implementation.
        SpecialName                 = 0x0800, // Method is special.  Name describes how.

                                              // interop attributes
        PinvokeImpl                 = 0x2000, // Implementation is forwarded through pinvoke.
        UnmanagedExport             = 0x0008, // Managed method exported via thunk to unmanaged code.

                                              // Reserved flags for runtime use only.
        ReservedMask                = 0xD000,
        RTSpecialName               = 0x1000, // Runtime should check name encoding.
        HasSecurity                 = 0x4000, // Method has security associate with it.
        RequireSecObject            = 0x8000  // Method calls another method containing security code.
    }

    //
    // Section 23.1.11 of ECMA spec, Partition II
    //
    [Flags]
    public enum MethodImplAttributes : ushort
    {
                                              // code impl mask
        CodeTypeMask                = 0x0003, // Flags about code type.
        IL                          = 0x0000, // Method impl is IL.
        Native                      = 0x0001, // Method impl is native.
        OPTIL                       = 0x0002, // Method impl is OPTIL
        Runtime                     = 0x0003, // Method impl is provided by the runtime.

                                              // managed mask
        ManagedMask                 = 0x0004, // Flags specifying whether the code is managed or unmanaged.
        Unmanaged                   = 0x0004, // Method impl is unmanaged, otherwise managed.
        Managed                     = 0x0000, // Method impl is managed.

                                              // implementation info and interop
        ForwardRef                  = 0x0010, // Indicates method is defined; used primarily in merge scenarios.
        PreserveSig                 = 0x0080, // Indicates method sig is not to be mangled to do HRESULT conversion.
        InternalCall                = 0x1000, // Reserved for internal use.
        Synchronized                = 0x0020, // Method is single threaded through the body.
        NoInlining                  = 0x0008, // Method may not be inlined.
        MaxMethodImplVal            = 0xFFFF  // Range check value
    }

    //
    // Section 23.1.12 of ECMA spec, Partition II
    //
    [Flags]
    public enum MethodSemanticAttributes : ushort
    {
        Setter   = 0x0001, // Setter for property
        Getter   = 0x0002, // Getter for property
        Other    = 0x0004, // other method for property or event
        AddOn    = 0x0008, // AddOn method for event
        RemoveOn = 0x0010, // RemoveOn method for event
        Fire     = 0x0020  // Fire method for event
    }

    //
    // Section 23.1.13 of ECMA spec, Partition II
    //
    [Flags]
    public enum ParamAttributes : ushort
    {
        In              = 0x0001, // Param is [In]
        Out             = 0x0002, // Param is [out]
        Optional        = 0x0010, // Param is optional

                                  // Reserved flags for Runtime use only.
        ReservedMask    = 0xF000,
        HasDefault      = 0x1000, // Param has default value.
        HasFieldMarshal = 0x2000, // Param has FieldMarshal.
        Unused          = 0xCFE0
    }

    //
    // Section 23.1.14 of ECMA spec, Partition II
    //
    [Flags]
    public enum PropertyAttributes : ushort
    {
        SpecialName   = 0x0200, // property is special.  Name describes how.

                                // Reserved flags for Runtime use only.
        ReservedMask  = 0xF400,
        RTSpecialName = 0x0400, // Runtime(metadata internal APIs) should check name encoding.
        HasDefault    = 0x1000, // Property has default
        Unused        = 0xE9FF
    }

    //
    // Section 23.1.15 of ECMA spec, Partition II
    //
    [Flags]
    public enum TypeAttributes
    {
        VisibilityMask     = 0x00000007,
        NotPublic          = 0x00000000, // Class is not public scope.
        Public             = 0x00000001, // Class is public scope.
        NestedPublic       = 0x00000002, // Class is nested with public visibility.
        NestedPrivate      = 0x00000003, // Class is nested with private visibility.
        NestedFamily       = 0x00000004, // Class is nested with family visibility.
        NestedAssembly     = 0x00000005, // Class is nested with assembly visibility.
        NestedFamANDAssem  = 0x00000006, // Class is nested with family and assembly visibility.
        NestedFamORAssem   = 0x00000007, // Class is nested with family or assembly visibility.

        // Use this mask to retrieve class layout informaiton
        // 0 is AutoLayout, 0x2 is SequentialLayout, 4 is ExplicitLayout
        LayoutMask         = 0x00000018,
        AutoLayout         = 0x00000000, // Class fields are auto-laid out
        SequentialLayout   = 0x00000008, // Class fields are laid out sequentially
        ExplicitLayout     = 0x00000010, // Layout is supplied explicitly
        // end layout mask

        // Use this mask to distinguish a type declaration as a Class, ValueType or Interface
        ClassSemanticsMask = 0x00000020,
        Class              = 0x00000000, // Type is a class.
        Interface          = 0x00000020, // Type is an interface.

        // Special semantics in addition to class semantics.
        Abstract           = 0x00000080, // Class is abstract
        Sealed             = 0x00000100, // Class is concrete and may not be extended
        SpecialName        = 0x00000400, // Class name is special.  Name describes how.

        // Implementation attributes.
        Import             = 0x00001000, // Class / interface is imported
        Serializable       = 0x00002000, // The class is Serializable.

        // Use tdStringFormatMask to retrieve string information for native interop
        StringFormatMask   = 0x00030000,
        AnsiClass          = 0x00000000, // LPTSTR is interpreted as ANSI in this class
        UnicodeClass       = 0x00010000, // LPTSTR is interpreted as UNICODE
        AutoClass          = 0x00020000, // LPTSTR is interpreted automatically
        CustomFormatClass  = 0x00030000, // A non-standard encoding specified by CustomFormatMask
        CustomFormatMask   = 0x00C00000, // Use this mask to retrieve non-standard encoding information for native interop. The meaning of the values of these 2 bits is unspecified.

        // end string format mask

        BeforeFieldInit    = 0x00100000, // Initialize the class any time before first static field access.

        // Flags reserved for runtime use.
        ReservedMask       = 0x00040800,
        RTSpecialName      = 0x00000800, // Runtime should check name encoding.
        HasSecurity        = 0x00040000, // Class has security associate with it.
    }

    //
    // Section 23.1.16 of ECMA spec, Partition II
    //
    public enum ElementTypes : byte
    {
        END         = 0x00,
        VOID        = 0x01,
        BOOLEAN     = 0x02,
        CHAR        = 0x03,
        I1          = 0x04,
        U1          = 0x05,
        I2          = 0x06,
        U2          = 0x07,
        I4          = 0x08,
        U4          = 0x09,
        I8          = 0x0A,
        U8          = 0x0B,
        R4          = 0x0C,
        R8          = 0x0D,
        STRING      = 0x0E,
        ///////////////////////// every type above PTR will be simple type
        PTR         = 0x0F,    // PTR <type>
        BYREF       = 0x10,    // BYREF <type>
        ///////////////////////// Please use VALUETYPE. VALUECLASS is deprecated.
        VALUETYPE   = 0x11,    // VALUETYPE <class Token>
        CLASS       = 0x12,    // CLASS <class Token>
        VAR         = 0x13,    // a class type variable VAR <U1>
        ARRAY       = 0x14,    // MDARRAY <type> <rank> <bcount> <bound1> ... <lbcount> <lb1> ...
        GENERICINST = 0x15,    // instantiated type
        TYPEDBYREF  = 0x16,    // This is a simple type.
        I           = 0x18,    // native integer size
        U           = 0x19,    // native unsigned integer size
        FNPTR       = 0x1B,    // FNPTR <complete sig for the function including calling convention>
        OBJECT      = 0x1C,    // Shortcut for System.Object
        SZARRAY     = 0x1D,    // Shortcut for single dimension zero lower bound array
        ///////////////////////// SZARRAY <type>
        ///////////////////////// This is only for binding
        MVAR        = 0x1E,    // a method type variable MVAR <U1>
        CMOD_REQD   = 0x1F,    // required C modifier : E_T_CMOD_REQD <mdTypeRef/mdTypeDef>
        CMOD_OPT    = 0x20,    // optional C modifier : E_T_CMOD_OPT <mdTypeRef/mdTypeDef>
        ///////////////////////// This is for signatures generated internally (which will not be persisted in any way).
        INTERNAL    = 0x21,    // INTERNAL <typehandle>
        ///////////////////////// Note that this is the max of base type excluding modifiers
        MAX         = 0x22,    // first invalid element type
        MODIFIER    = 0x40,
        SENTINEL    = 0x01 | MODIFIER, // sentinel for varargs
        PINNED      = 0x05 | MODIFIER
    }

    public enum SerializationTypes
    {
        BOOLEAN       = ElementTypes.BOOLEAN,
        CHAR          = ElementTypes.CHAR,
        I1            = ElementTypes.I1,
        U1            = ElementTypes.U1,
        I2            = ElementTypes.I2,
        U2            = ElementTypes.U2,
        I4            = ElementTypes.I4,
        U4            = ElementTypes.U4,
        I8            = ElementTypes.I8,
        U8            = ElementTypes.U8,
        R4            = ElementTypes.R4,
        R8            = ElementTypes.R8,
        STRING        = ElementTypes.STRING,
        OBJECT        = ElementTypes.OBJECT,
        SZARRAY       = ElementTypes.SZARRAY, // Shortcut for single dimension zero lower bound array
        TYPE          = 0x50,
        TAGGED_OBJECT = 0x51,
        FIELD         = 0x53,
        PROPERTY      = 0x54,
        ENUM          = 0x55
    }
}
