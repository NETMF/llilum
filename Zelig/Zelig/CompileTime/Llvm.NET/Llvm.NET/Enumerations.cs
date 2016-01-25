// This file maps the lower level internal LLVM enumeration names to something
// more compatible with the styles, patterns and conventions familiar to .NET Developers
// and also keeping the lower level interop namespace internal to prevent mis-use or
// violations of uniqueness rules

namespace Llvm.NET
{
    /// See Module::ModFlagBehavior
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1028:EnumStorageShouldBeInt32" )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag" )]
    public enum ModuleFlagBehavior : uint
    {
        Invalid = 0,
        Error = LLVMModFlagBehavior.Error,
        Warning = LLVMModFlagBehavior.Warning,
        Require = LLVMModFlagBehavior.Require,
        Override = LLVMModFlagBehavior.Override,
        Append = LLVMModFlagBehavior.Append,
        AppendUnique = LLVMModFlagBehavior.AppendUnique
    };

    /// <summary>LLVM Instruction opcodes</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1028:EnumStorageShouldBeInt32" )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1027:MarkEnumsWithFlags", Justification = "Not actually flags" )]
    public enum OpCode : uint
    {
        Invalid = 0,
        /* Terminator Instructions */
        Return = LLVMOpcode.LLVMRet,
        Branch = LLVMOpcode.LLVMBr,
        Switch = LLVMOpcode.LLVMSwitch,
        IndirectBranch = LLVMOpcode.LLVMIndirectBr,
        Invoke = LLVMOpcode.LLVMInvoke,
        Unreachable = LLVMOpcode.LLVMUnreachable,

        /* Standard Binary Operators */
        Add = LLVMOpcode.LLVMAdd,
        FAdd = LLVMOpcode.LLVMFAdd,
        Sub = LLVMOpcode.LLVMSub,
        FSub = LLVMOpcode.LLVMFSub,
        Mul = LLVMOpcode.LLVMMul,
        FMul = LLVMOpcode.LLVMFMul,
        UDiv = LLVMOpcode.LLVMUDiv,
        SDiv = LLVMOpcode.LLVMSDiv,
        FDiv = LLVMOpcode.LLVMFDiv,
        URem = LLVMOpcode.LLVMURem,
        SRem = LLVMOpcode.LLVMSRem,
        FRem = LLVMOpcode.LLVMFRem,

        /* Logical Operators */
        Shl = LLVMOpcode.LLVMShl,
        LShr = LLVMOpcode.LLVMLShr,
        AShr = LLVMOpcode.LLVMAShr,
        And = LLVMOpcode.LLVMAnd,
        Or = LLVMOpcode.LLVMOr,
        Xor = LLVMOpcode.LLVMXor,

        /* Memory Operators */
        Alloca = LLVMOpcode.LLVMAlloca,
        Load = LLVMOpcode.LLVMLoad,
        Store = LLVMOpcode.LLVMStore,
        GetElementPtr = LLVMOpcode.LLVMGetElementPtr,

        /* Cast Operators */
        Trunc = LLVMOpcode.LLVMTrunc,
        ZeroExtend = LLVMOpcode.LLVMZExt,
        SignExtend = LLVMOpcode.LLVMSExt,
        FPToUI = LLVMOpcode.LLVMFPToUI,
        FPToSI = LLVMOpcode.LLVMFPToSI,
        UIToFP = LLVMOpcode.LLVMUIToFP,
        SIToFP = LLVMOpcode.LLVMSIToFP,
        FPTrunc = LLVMOpcode.LLVMFPTrunc,
        FPExt = LLVMOpcode.LLVMFPExt,
        PtrToInt = LLVMOpcode.LLVMPtrToInt,
        IntToPtr = LLVMOpcode.LLVMIntToPtr,
        BitCast = LLVMOpcode.LLVMBitCast,
        AddrSpaceCast = LLVMOpcode.LLVMAddrSpaceCast,

        /* Other Operators */
        ICmp = LLVMOpcode.LLVMICmp,
        FCmp = LLVMOpcode.LLVMFCmp,
        Phi = LLVMOpcode.LLVMPHI,
        Call = LLVMOpcode.LLVMCall,
        Select = LLVMOpcode.LLVMSelect,
        UserOp1 = LLVMOpcode.LLVMUserOp1,
        UserOp2 = LLVMOpcode.LLVMUserOp2,
        VaArg = LLVMOpcode.LLVMVAArg,
        ExtractElement = LLVMOpcode.LLVMExtractElement,
        InsertElement = LLVMOpcode.LLVMInsertElement,
        ShuffleVector = LLVMOpcode.LLVMShuffleVector,
        ExtractValue = LLVMOpcode.LLVMExtractValue,
        InsertValue = LLVMOpcode.LLVMInsertValue,

        /* Atomic operators */
        Fence = LLVMOpcode.LLVMFence,
        AtomicCmpXchg = LLVMOpcode.LLVMAtomicCmpXchg,
        AtomicRMW = LLVMOpcode.LLVMAtomicRMW,

        /* Exception Handling Operators */
        Resume = LLVMOpcode.LLVMResume,
        LandingPad = LLVMOpcode.LLVMLandingPad
    }

    /// <summary>Basic kind of a type</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1028:EnumStorageShouldBeInt32" )]
    public enum TypeKind : uint
    {
        Void = LLVMTypeKind.LLVMVoidTypeKind,           // type with no size
        Float16 = LLVMTypeKind.LLVMHalfTypeKind,        // 16 bit floating point type
        Float32 = LLVMTypeKind.LLVMFloatTypeKind,       // 32 bit floating point type
        Float64 = LLVMTypeKind.LLVMDoubleTypeKind,      // 64 bit floating point type
        X86Float80 = LLVMTypeKind.LLVMX86_FP80TypeKind, // 80 bit floating point type (X87)
        Float128m112 = LLVMTypeKind.LLVMFP128TypeKind,  // 128 bit floating point type (112-bit mantissa)
        Float128 = LLVMTypeKind.LLVMPPC_FP128TypeKind,  // 128 bit floating point type (two 64-bits)
        Label = LLVMTypeKind.LLVMLabelTypeKind,         // Labels
        Integer = LLVMTypeKind.LLVMIntegerTypeKind,     // Arbitrary bit width integers
        Function = LLVMTypeKind.LLVMFunctionTypeKind,   // Functions
        Struct = LLVMTypeKind.LLVMStructTypeKind,       // Structures
        Array = LLVMTypeKind.LLVMArrayTypeKind,         // Arrays
        Pointer = LLVMTypeKind.LLVMPointerTypeKind,     // Pointers
        Vector = LLVMTypeKind.LLVMVectorTypeKind,       // SIMD 'packed' format, or other vector type
        Metadata = LLVMTypeKind.LLVMMetadataTypeKind,   // Metadata
        X86MMX = LLVMTypeKind.LLVMX86_MMXTypeKind       // X86 MMX
    }

    /// <summary>Calling Convention for functions</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1028:EnumStorageShouldBeInt32" )]
    public enum CallingConvention : uint
    {
        C = LLVMCallConv.LLVMCCallConv,
        FastCall = LLVMCallConv.LLVMFastCallConv,
        ColdCall = LLVMCallConv.LLVMColdCallConv,
        WebKitJS = LLVMCallConv.LLVMWebKitJSCallConv,
        AnyReg = LLVMCallConv.LLVMAnyRegCallConv,
        x86StdCall = LLVMCallConv.LLVMX86StdcallCallConv,
        x86FastCall = LLVMCallConv.LLVMX86FastcallCallConv
    }

    /// <summary>Linkage specification for functions and globals</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1028:EnumStorageShouldBeInt32" )]
    public enum Linkage : uint
    {
        External = LLVMLinkage.LLVMExternalLinkage,    /*< Externally visible function */
        AvailableExternally = LLVMLinkage.LLVMAvailableExternallyLinkage,
        LinkOnceAny = LLVMLinkage.LLVMLinkOnceAnyLinkage, /*< Keep one copy of function when linking (inline)*/
        LinkOnceODR = LLVMLinkage.LLVMLinkOnceODRLinkage, /*< Same, but only replaced by something equivalent. */
        //LLVMLinkage.LLVMLinkOnceODRAutoHideLinkage, /**< Obsolete */
        Weak = LLVMLinkage.LLVMWeakAnyLinkage,     /*< Keep one copy of function when linking (weak) */
        WeakODR = LLVMLinkage.LLVMWeakODRLinkage,     /*< Same, but only replaced by something equivalent. */
        Append = LLVMLinkage.LLVMAppendingLinkage,   /*< Special purpose, only applies to global arrays */
        Internal = LLVMLinkage.LLVMInternalLinkage,    /*< Rename collisions when linking (static functions) */
        Private = LLVMLinkage.LLVMPrivateLinkage,     /*< Like Internal, but omit from symbol table */
        DllImport = LLVMLinkage.LLVMDLLImportLinkage,   /*< Function to be imported from DLL */
        DllExport = LLVMLinkage.LLVMDLLExportLinkage,   /*< Function to be accessible from DLL */
        ExternalWeak = LLVMLinkage.LLVMExternalWeakLinkage,/*< ExternalWeak linkage description */
        //LLVMLinkage.LLVMGhostLinkage,       /*< Obsolete */
        Common = LLVMLinkage.LLVMCommonLinkage,      /*< Tentative definitions */
        LinkerPrivate = LLVMLinkage.LLVMLinkerPrivateLinkage, /*< Like Private, but linker removes. */
        LinkerPrivateWeak = LLVMLinkage.LLVMLinkerPrivateWeakLinkage /*< Like LinkerPrivate, but is weak. */
    }

    ///<summary>Enumeration for the visibility of a global value</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1028:EnumStorageShouldBeInt32" )]
    public enum Visibility : uint
    {
        Default = LLVMVisibility.LLVMDefaultVisibility,  /*< The GV is visible */
        Hidden = LLVMVisibility.LLVMHiddenVisibility,   /*< The GV is hidden */
        Protected = LLVMVisibility.LLVMProtectedVisibility /*< The GV is protected */
    }

    /// <summary>Unified predicate enumeration</summary>
    /// <remarks>
    /// Underneath the C API this is what LLVM uses. For some reason the C API
    /// split it into the integer and float predicate enumerations.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1028:EnumStorageShouldBeInt32" )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1027:MarkEnumsWithFlags" )]
    public enum Predicate : uint
    {
        False = LLVMRealPredicate.LLVMRealPredicateFalse,
        OrderedAndEqual = LLVMRealPredicate.LLVMRealOEQ,
        OrderedAndGreaterThan = LLVMRealPredicate.LLVMRealOGT,
        OrderedAndGreaterThanOrEqual = LLVMRealPredicate.LLVMRealOGE,
        OrderedAndLessThan = LLVMRealPredicate.LLVMRealOLT,
        OrderedAndLessThanOrEqual = LLVMRealPredicate.LLVMRealOLE,
        OrderedAndNotEqual = LLVMRealPredicate.LLVMRealONE,
        Ordered = LLVMRealPredicate.LLVMRealORD,
        Unordered = LLVMRealPredicate.LLVMRealUNO,
        UnorderedAndEqual = LLVMRealPredicate.LLVMRealUEQ,
        UnorderedOrGreaterThan = LLVMRealPredicate.LLVMRealUGT,
        UnorderedOrGreaterThanOrEqual = LLVMRealPredicate.LLVMRealUGE,
        UnorderedOrLessThan = LLVMRealPredicate.LLVMRealULT,
        UnorderedOrLessThanOrEqual = LLVMRealPredicate.LLVMRealULE,
        UnorderedOrNotEqual = LLVMRealPredicate.LLVMRealUNE,
        True = LLVMRealPredicate.LLVMRealPredicateTrue,
        FirstFcmpPredicate = False,
        LastFcmpPredicate = True,
        /// <summary>Any value Greater than or equal to this is not valid for Fcmp operations</summary>
        BadFcmpPredicate = LastFcmpPredicate + 1,

        Equal = LLVMIntPredicate.LLVMIntEQ,
        NotEqual = LLVMIntPredicate.LLVMIntNE,
        UnsignedGreater = LLVMIntPredicate.LLVMIntUGT,
        UnsignedGreaterOrEqual = LLVMIntPredicate.LLVMIntUGE,
        UnsignedLess = LLVMIntPredicate.LLVMIntULT,
        UnsignedLessOrEqual = LLVMIntPredicate.LLVMIntULE,
        SignedGreater = LLVMIntPredicate.LLVMIntSGT,
        SignedGreaterOrEqual = LLVMIntPredicate.LLVMIntSGE,
        SignedLess = LLVMIntPredicate.LLVMIntSLT,
        SignedLessOrEqual = LLVMIntPredicate.LLVMIntSLE,
        FirstIcmpPredicate = Equal,
        LastIcmpPredicate = SignedLessOrEqual,
        /// <summary>Any value Greater than or equal to this is not valid for Icmp operations</summary>
        BadIcmpPredicate = LastIcmpPredicate + 1
    }

    ///<summary>Predicate enumeration for integer comparison</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1028:EnumStorageShouldBeInt32" )]
    public enum IntPredicate : uint
    {
        False = LLVMRealPredicate.LLVMRealPredicateFalse,
        Equal = LLVMIntPredicate.LLVMIntEQ,
        NotEqual = LLVMIntPredicate.LLVMIntNE,
        UnsignedGreater = LLVMIntPredicate.LLVMIntUGT,
        UnsignedGreaterOrEqual = LLVMIntPredicate.LLVMIntUGE,
        UnsignedLess = LLVMIntPredicate.LLVMIntULT,
        UnsignedLessOrEqual = LLVMIntPredicate.LLVMIntULE,
        SignedGreater = LLVMIntPredicate.LLVMIntSGT,
        SignedGreaterOrEqual = LLVMIntPredicate.LLVMIntSGE,
        SignedLess = LLVMIntPredicate.LLVMIntSLT,
        SignedLessOrEqual = LLVMIntPredicate.LLVMIntSLE
    }

    ///<summary>Predicate enumeration for integer comparison</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1028:EnumStorageShouldBeInt32" )]
    public enum RealPredicate : uint
    {
        False = LLVMRealPredicate.LLVMRealPredicateFalse,
        OrderedAndEqual = LLVMRealPredicate.LLVMRealOEQ,
        OrderedAndGreaterThan = LLVMRealPredicate.LLVMRealOGT,
        OrderedAndGreaterThanOrEqual = LLVMRealPredicate.LLVMRealOGE,
        OrderedAndLessThan = LLVMRealPredicate.LLVMRealOLT,
        OrderedAndLessThanOrEqual = LLVMRealPredicate.LLVMRealOLE,
        OrderedAndNotEqual = LLVMRealPredicate.LLVMRealONE,
        Ordered = LLVMRealPredicate.LLVMRealORD,
        Unordered = LLVMRealPredicate.LLVMRealUNO,
        UnorderedAndEqual = LLVMRealPredicate.LLVMRealUEQ,
        UnorderedOrGreaterThan = LLVMRealPredicate.LLVMRealUGT,
        UnorderedOrGreaterThanOrEqual = LLVMRealPredicate.LLVMRealUGE,
        UnorderedOrLessThan = LLVMRealPredicate.LLVMRealULT,
        UnorderedOrLessThanOrEqual = LLVMRealPredicate.LLVMRealULE,
        UnorderedOrNotEqual = LLVMRealPredicate.LLVMRealUNE,
        True = LLVMRealPredicate.LLVMRealPredicateTrue,
    }

    /// <summary>Optimization level for target code generation</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1028:EnumStorageShouldBeInt32" )]
    public enum CodeGenOpt : uint
    {
        None = LLVMCodeGenOptLevel.LLVMCodeGenLevelNone,
        Less = LLVMCodeGenOptLevel.LLVMCodeGenLevelLess,
        Default = LLVMCodeGenOptLevel.LLVMCodeGenLevelDefault,
        Aggressive = LLVMCodeGenOptLevel.LLVMCodeGenLevelAggressive
    }

    /// <summary>Relocation type for target code generation</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1028:EnumStorageShouldBeInt32" )]
    public enum Reloc : uint
    {
        Default = LLVMRelocMode.LLVMRelocDefault,
        Static = LLVMRelocMode.LLVMRelocStatic,
        PositionIndependent = LLVMRelocMode.LLVMRelocPIC,
        Dynamic = LLVMRelocMode.LLVMRelocDynamicNoPic
    }

    /// <summary>Code model to use for target code generation</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1028:EnumStorageShouldBeInt32" )]
    public enum CodeModel : uint
    {
        Default = LLVMCodeModel.LLVMCodeModelDefault,
        JitDefault = LLVMCodeModel.LLVMCodeModelJITDefault,
        Small = LLVMCodeModel.LLVMCodeModelSmall,
        Kernel = LLVMCodeModel.LLVMCodeModelKernel,
        Medium = LLVMCodeModel.LLVMCodeModelMedium,
        Large = LLVMCodeModel.LLVMCodeModelLarge
    }

    /// <summary>Output file type for target code generation</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1028:EnumStorageShouldBeInt32" )]
    public enum CodeGenFileType : uint
    {
        AssemblySource = LLVMCodeGenFileType.LLVMAssemblyFile,
        ObjectFile = LLVMCodeGenFileType.LLVMObjectFile
    }

    /// <summary>Byte ordering for target code generation and data type layout</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1028:EnumStorageShouldBeInt32" )]
    public enum ByteOrdering : uint
    {
        LittleEndian = LLVMByteOrdering.LLVMLittleEndian,
        BigEndian = LLVMByteOrdering.LLVMBigEndian
    }

    /// <summary>LLVM module linker mode</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1028:EnumStorageShouldBeInt32" )]
    public enum LinkerMode : uint
    {
        DestroySource = LLVMLinkerMode.LLVMLinkerDestroySource,
        PreserveSource = LLVMLinkerMode.LLVMLinkerPreserveSource
    }

    internal enum ValueKind : uint
    {
        Argument              = LLVMValueKind.LLVMValueKindArgumentVal,             // This is an instance of Argument
        BasicBlock            = LLVMValueKind.LLVMValueKindBasicBlockVal,           // This is an instance of BasicBlock
        Function              = LLVMValueKind.LLVMValueKindFunctionVal,             // This is an instance of Function
        GlobalAlias           = LLVMValueKind.LLVMValueKindGlobalAliasVal,          // This is an instance of GlobalAlias
        GlobalVariable        = LLVMValueKind.LLVMValueKindGlobalVariableVal,       // This is an instance of GlobalVariable
        UndefValue            = LLVMValueKind.LLVMValueKindUndefValueVal,           // This is an instance of UndefValue
        BlockAddress          = LLVMValueKind.LLVMValueKindBlockAddressVal,         // This is an instance of BlockAddress
        ConstantExpr          = LLVMValueKind.LLVMValueKindConstantExprVal,         // This is an instance of ConstantExpr
        ConstantAggregateZero = LLVMValueKind.LLVMValueKindConstantAggregateZeroVal,// This is an instance of ConstantAggregateZero
        ConstantDataArray     = LLVMValueKind.LLVMValueKindConstantDataArrayVal,    // This is an instance of ConstantDataArray
        ConstantDataVector    = LLVMValueKind.LLVMValueKindConstantDataVectorVal,   // This is an instance of ConstantDataVector
        ConstantInt           = LLVMValueKind.LLVMValueKindConstantIntVal,          // This is an instance of ConstantInt
        ConstantFP            = LLVMValueKind.LLVMValueKindConstantFPVal,           // This is an instance of ConstantFP
        ConstantArray         = LLVMValueKind.LLVMValueKindConstantArrayVal,        // This is an instance of ConstantArray
        ConstantStruct        = LLVMValueKind.LLVMValueKindConstantStructVal,       // This is an instance of ConstantStruct
        ConstantVector        = LLVMValueKind.LLVMValueKindConstantVectorVal,       // This is an instance of ConstantVector
        ConstantPointerNull   = LLVMValueKind.LLVMValueKindConstantPointerNullVal,  // This is an instance of ConstantPointerNull
        MetadataAsValue       = LLVMValueKind.LLVMValueKindMetadataAsValueVal,      // This is an instance of MetadataAsValue
        InlineAsm             = LLVMValueKind.LLVMValueKindInlineAsmVal,            // This is an instance of InlineAsm
        Instruction           = LLVMValueKind.LLVMValueKindInstructionVal,          // This is an instance of Instruction
                                                                                    // Enum values starting at InstructionVal are used for Instructions;

        // instruction values come directly from LLVM Instruction.def which is different from the "stable"
        // LLVM-C API, therefore they are less "stable" and bound to the C++ implementation version and
        // subject to change from version to version.
        Return         = 1 + Instruction,
        Branch         = 2 + Instruction,
        Switch         = 3 + Instruction,
        IndirectBranch = 4 + Instruction,
        Invoke         = 5 + Instruction,
        Resume         = 6 + Instruction,
        Unreachable    = 7 + Instruction,

        Add            = 8 + Instruction,
        FAdd           = 9 + Instruction,
        Sub            = 10 + Instruction,
        FSub           = 11 + Instruction,
        Mul            = 12 + Instruction,
        FMul           = 13 + Instruction,
        UDiv           = 14 + Instruction,
        SDiv           = 15 + Instruction,
        FDiv           = 16 + Instruction,
        URem           = 17 + Instruction,
        SRem           = 18 + Instruction,
        FRem           = 19 + Instruction,

        Shl            = 20 + Instruction,
        LShr           = 21 + Instruction,
        AShr           = 22 + Instruction,
        And            = 23 + Instruction,
        Or             = 24 + Instruction,
        Xor            = 25 + Instruction,

        Alloca         = 26 + Instruction,
        Load           = 27 + Instruction,
        Store          = 28 + Instruction,
        GetElementPtr  = 29 + Instruction,
        Fence          = 30 + Instruction,
        AtomicCmpXchg  = 31 + Instruction,
        AtomicRMW      = 32 + Instruction,

        Trunc          = 33 + Instruction,
        ZeroExtend     = 34 + Instruction,
        SignExtend     = 35 + Instruction,
        FPToUI         = 36 + Instruction,
        FPToSI         = 37 + Instruction,
        UIToFP         = 38 + Instruction,
        SIToFP         = 39 + Instruction,
        FPTrunc        = 40 + Instruction,
        FPExt          = 41 + Instruction,
        PtrToInt       = 42 + Instruction,
        IntToPtr       = 43 + Instruction,
        BitCast        = 44 + Instruction,
        AddrSpaceCast  = 45 + Instruction,

        ICmp           = 46 + Instruction,
        FCmp           = 47 + Instruction,
        Phi            = 48 + Instruction,
        Call           = 49 + Instruction,
        Select         = 50 + Instruction,
        UserOp1        = 51 + Instruction,
        UserOp2        = 52 + Instruction,
        VaArg          = 53 + Instruction,
        ExtractElement = 54 + Instruction,
        InsertElement  = 55 + Instruction,
        ShuffleVector  = 56 + Instruction,
        ExtractValue   = 57 + Instruction,
        InsertValue    = 58 + Instruction,
        LandingPad     = 59 + Instruction,

        // Markers:
        ConstantFirstVal = Function,
        ConstantLastVal = ConstantPointerNull
    }

    public enum AttributeKind
    {
        // IR-Level Attributes
        None,                  // No attributes have been set
        Alignment,             // Alignment of parameter (5 bits)
                               // stored as log2 of alignment with +1 bias
                               // 0 means unaligned (different from align(1))
        AlwaysInline,          // inline=always
        Builtin,               // Callee is recognized as a builtin, despite
                               // nobuiltin attribute on its declaration.
        ByVal,                 // Pass structure by value
        InAlloca,              // Pass structure in an alloca
        Cold,                  // Marks function as being in a cold path.
        Convergent,            // Can only be moved to control-equivalent blocks
        InlineHint,            // Source said inlining was desirable
        InReg,                 // Force argument to be passed in register
        JumpTable,             // Build jump-instruction tables and replace refs.
        MinSize,               // Function must be optimized for size first
        Naked,                 // Naked function
        Nest,                  // Nested function static chain
        NoAlias,               // Considered to not alias after call
        NoBuiltin,             // Callee isn't recognized as a builtin
        NoCapture,             // Function creates no aliases of pointer
        NoDuplicate,           // Call cannot be duplicated
        NoImplicitFloat,       // Disable implicit floating point insts
        NoInline,              // inline=never
        NonLazyBind,           // Function is called early and/or
                               // often, so lazy binding isn't worthwhile
        NonNull,               // Pointer is known to be not null
        Dereferenceable,       // Pointer is known to be dereferenceable
        DereferenceableOrNull, // Pointer is either null or dereferenceable
        NoRedZone,             // Disable redzone
        NoReturn,              // Mark the function as not returning
        NoUnwind,              // Function doesn't unwind stack
        OptimizeForSize,       // opt_size
        OptimizeNone,          // Function must not be optimized.
        ReadNone,              // Function does not access memory
        ReadOnly,              // Function only reads from memory
        ArgMemOnly,            // Funciton can access memory only using pointers
                               // based on its arguments.
        Returned,              // Return value is always equal to this argument
        ReturnsTwice,          // Function can return twice
        SExt,                  // Sign extended before/after call
        StackAlignment,        // Alignment of stack for function (3 bits)
                               // stored as log2 of alignment with +1 bias 0
                               // means unaligned (different from
                               // alignstack=(1))
        StackProtect,          // Stack protection.
        StackProtectReq,       // Stack protection required.
        StackProtectStrong,    // Strong Stack protection.
        SafeStack,             // Safe Stack protection.
        StructRet,             // Hidden pointer to structure to return
        SanitizeAddress,       // AddressSanitizer is on.
        SanitizeThread,        // ThreadSanitizer is on.
        SanitizeMemory,        // MemorySanitizer is on.
        UWTable,               // Function must be in a unwind table
        ZExt,                  // Zero extended before/after call

        EndAttrKinds           // Sentinal value useful for loops
    };

    /// <summary>Function index for attributes</summary>
    /// <remarks>
    /// Attributes on functions apply to the function itself, the return type
    /// or one of the function's parameters. This enumeration is used to 
    /// identify where the attribute applies.
    /// </remarks>
    public enum FunctionAttributeIndex
    {
        /// <summary>The attribute applies to the function itself</summary>
        Function = -1,
        /// <summary>The attribute applies to the return type of the function</summary>
        ReturnType = 0,
        /// <summary>The attribute applies to the first parameter of the function</summary>
        /// <remarks>
        /// Additional parameters can identified by simply adding an integer value to
        /// this value. (i.e. FunctionAttributeIndex.Parameter0 + 1 )
        /// </remarks>
        Parameter0 = 1 
    }

}