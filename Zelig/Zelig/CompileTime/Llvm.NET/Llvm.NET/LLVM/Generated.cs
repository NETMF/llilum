// This file was generated from LLVM 3.6.1 llvm-c API using ClangSharp
// it was further modified in the following ways:
//  - removed most uses of the partial keyword except for LLVMNative static class and LLVMBool
//  - added warning disable to avoid benign compiler warnings about fields only set by native code
//  - modified all elements to be internal instead of public
//  - modified PInvoke attributes to use fully qualified name for CallingConvention to avoid conflicts
//  - Added Missing LLVMLinkerMode enumeration
//  - Fixed signature of LLVMLinkModules to use new enum
//  - modified all LLVMNative.* functions returning a string to return IntPtr
//    as using string with default CLR marshaling was causing run-time
//    Heap Corruption. [Reason unknown] Calling these functions requires calling
//    Marshal.PtrToAnsi( x ) manually, which is handled by the OO wrappers. 
//  - made Value field of LLVMBool private to help prevent confusion on use the Value. For status
//    code values the Succes/Failure properties (added to partial implementation) and implicit
//    casts to/from bool for actual boolean. Unfortunately nothing in LLVM or the value of a LLVMBool
//    can help in identifying which form one should use. The only way to know at this point is to
//    read the LLVM-C API documentation. In the Future it may be good to Create an LLVMStatusResult
//    type for those function returning a status and use standard marshalling techniques for actual
//    boolean values...
//  - manually updated to 3.7.0 apis
 
using System;
using System.Runtime.InteropServices;

//warning CS0649: Field 'xxx' is never assigned to, and will always have its default value 0
#pragma warning disable 649

namespace Llvm.NET
{
    internal struct LLVMOpaqueMemoryBuffer
    {
    }

    internal struct LLVMOpaqueContext
    {
    }

    internal struct LLVMOpaqueModule
    {
    }

    internal struct LLVMOpaqueType
    {
    }

    internal struct LLVMOpaqueValue
    {
    }

    internal struct LLVMOpaqueBasicBlock
    {
    }

    internal struct LLVMOpaqueBuilder
    {
    }

    internal struct LLVMOpaqueModuleProvider
    {
    }

    internal struct LLVMOpaquePassManager
    {
    }

    internal struct LLVMOpaquePassRegistry
    {
    }

    internal struct LLVMOpaqueUse
    {
    }

    internal struct LLVMOpaqueDiagnosticInfo
    {
    }

    internal struct LLVMOpInfoSymbol1
    {
        public int @Present;
        [MarshalAs(UnmanagedType.LPStr)] public string @Name;
        public int @Value;
    }

    internal struct LLVMOpInfo1
    {
        public LLVMOpInfoSymbol1 @AddSymbol;
        public LLVMOpInfoSymbol1 @SubtractSymbol;
        public int @Value;
        public int @VariantKind;
    }

    internal struct LLVMOpaqueTargetData
    {
    }

    internal struct LLVMOpaqueTargetLibraryInfotData
    {
    }

    internal struct LLVMOpaqueTargetMachine
    {
    }

    internal struct LLVMTarget
    {
    }

    internal struct LLVMOpaqueGenericValue
    {
    }

    internal struct LLVMOpaqueExecutionEngine
    {
    }

    internal struct LLVMOpaqueMCJITMemoryManager
    {
    }

    internal struct LLVMMCJITCompilerOptions
    {
        public uint @OptLevel;
        public LLVMCodeModel @CodeModel;
        public int @NoFramePointerElim;
        public int @EnableFastISel;
        public IntPtr @MCJMM;
    }

    internal struct LLVMOpaqueLTOModule
    {
    }

    internal struct LLVMOpaqueLTOCodeGenerator
    {
    }

    internal struct LLVMOpaqueObjectFile
    {
    }

    internal struct LLVMOpaqueSectionIterator
    {
    }

    internal struct LLVMOpaqueSymbolIterator
    {
    }

    internal struct LLVMOpaqueRelocationIterator
    {
    }

    internal struct LLVMOpaquePassManagerBuilder
    {
    }

    internal partial struct LLVMBool
    {
        public LLVMBool(int value)
        {
            this.Value = value;
        }

        internal int Value;
    }

    internal struct LLVMMemoryBufferRef
    {
        public LLVMMemoryBufferRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMContextRef
    {
        public LLVMContextRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMModuleRef
    {
        public LLVMModuleRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMTypeRef
    {
        public LLVMTypeRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMValueRef
    {
        public LLVMValueRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer { get; }

        public readonly static LLVMValueRef Zero = new LLVMValueRef( IntPtr.Zero );
    }

    internal struct LLVMBasicBlockRef
    {
        public LLVMBasicBlockRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMBuilderRef
    {
        public LLVMBuilderRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMModuleProviderRef
    {
        public LLVMModuleProviderRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMPassManagerRef
    {
        public LLVMPassManagerRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMPassRegistryRef
    {
        public LLVMPassRegistryRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMUseRef
    {
        public LLVMUseRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMDiagnosticInfoRef
    {
        public LLVMDiagnosticInfoRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    internal delegate void LLVMFatalErrorHandler([MarshalAs(UnmanagedType.LPStr)] string @Reason);

    [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    internal delegate void LLVMDiagnosticHandler(out LLVMOpaqueDiagnosticInfo @param0, IntPtr @param1);

    [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    internal delegate void LLVMYieldCallback(out LLVMOpaqueContext @param0, IntPtr @param1);

    internal struct LLVMDisasmContextRef
    {
        public LLVMDisasmContextRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    internal delegate int LLVMOpInfoCallback(IntPtr @DisInfo, int @PC, int @Offset, int @Size, int @TagType, IntPtr @TagBuf);

    [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    internal delegate string LLVMSymbolLookupCallback(IntPtr @DisInfo, int @ReferenceValue, out int @ReferenceType, int @ReferencePC, out IntPtr @ReferenceName);

    internal struct LLVMTargetDataRef
    {
        public LLVMTargetDataRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMTargetLibraryInfoRef
    {
        public LLVMTargetLibraryInfoRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMTargetMachineRef
    {
        public LLVMTargetMachineRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMTargetRef
    {
        public LLVMTargetRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMGenericValueRef
    {
        public LLVMGenericValueRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMExecutionEngineRef
    {
        public LLVMExecutionEngineRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMMCJITMemoryManagerRef
    {
        public LLVMMCJITMemoryManagerRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    internal delegate IntPtr LLVMMemoryManagerAllocateCodeSectionCallback(IntPtr @Opaque, int @Size, uint @Alignment, uint @SectionID, [MarshalAs(UnmanagedType.LPStr)] string @SectionName);

    [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    internal delegate IntPtr LLVMMemoryManagerAllocateDataSectionCallback(IntPtr @Opaque, int @Size, uint @Alignment, uint @SectionID, [MarshalAs(UnmanagedType.LPStr)] string @SectionName, int @IsReadOnly);

    [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    internal delegate int LLVMMemoryManagerFinalizeMemoryCallback(IntPtr @Opaque, out IntPtr @ErrMsg);

    [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    internal delegate void LLVMMemoryManagerDestroyCallback(IntPtr @Opaque);

    internal struct llvm_lto_t
    {
        public llvm_lto_t(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct lto_bool_t
    {
        public lto_bool_t(bool value)
        {
            this.Value = value;
        }

        public bool Value;
    }

    internal struct lto_module_t
    {
        public lto_module_t(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct lto_code_gen_t
    {
        public lto_code_gen_t(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    internal delegate void lto_diagnostic_handler_t(lto_codegen_diagnostic_severity_t @severity, [MarshalAs(UnmanagedType.LPStr)] string @diag, IntPtr @ctxt);

    internal struct LLVMObjectFileRef
    {
        public LLVMObjectFileRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMSectionIteratorRef
    {
        public LLVMSectionIteratorRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMSymbolIteratorRef
    {
        public LLVMSymbolIteratorRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMRelocationIteratorRef
    {
        public LLVMRelocationIteratorRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal struct LLVMPassManagerBuilderRef
    {
        public LLVMPassManagerBuilderRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        public IntPtr Pointer;
    }

    internal enum LLVMAttribute : int
    {
        @LLVMZExtAttribute = 1,
        @LLVMSExtAttribute = 2,
        @LLVMNoReturnAttribute = 4,
        @LLVMInRegAttribute = 8,
        @LLVMStructRetAttribute = 16,
        @LLVMNoUnwindAttribute = 32,
        @LLVMNoAliasAttribute = 64,
        @LLVMByValAttribute = 128,
        @LLVMNestAttribute = 256,
        @LLVMReadNoneAttribute = 512,
        @LLVMReadOnlyAttribute = 1024,
        @LLVMNoInlineAttribute = 2048,
        @LLVMAlwaysInlineAttribute = 4096,
        @LLVMOptimizeForSizeAttribute = 8192,
        @LLVMStackProtectAttribute = 16384,
        @LLVMStackProtectReqAttribute = 32768,
        @LLVMAlignment = 2031616,
        @LLVMNoCaptureAttribute = 2097152,
        @LLVMNoRedZoneAttribute = 4194304,
        @LLVMNoImplicitFloatAttribute = 8388608,
        @LLVMNakedAttribute = 16777216,
        @LLVMInlineHintAttribute = 33554432,
        @LLVMStackAlignment = 469762048,
        @LLVMReturnsTwice = 536870912,
        @LLVMUWTable = 1073741824,
        @LLVMNonLazyBind = -2147483648,
    }

    internal enum LLVMOpcode : uint
    {
        @LLVMRet = 1,
        @LLVMBr = 2,
        @LLVMSwitch = 3,
        @LLVMIndirectBr = 4,
        @LLVMInvoke = 5,
        @LLVMUnreachable = 7,
        @LLVMAdd = 8,
        @LLVMFAdd = 9,
        @LLVMSub = 10,
        @LLVMFSub = 11,
        @LLVMMul = 12,
        @LLVMFMul = 13,
        @LLVMUDiv = 14,
        @LLVMSDiv = 15,
        @LLVMFDiv = 16,
        @LLVMURem = 17,
        @LLVMSRem = 18,
        @LLVMFRem = 19,
        @LLVMShl = 20,
        @LLVMLShr = 21,
        @LLVMAShr = 22,
        @LLVMAnd = 23,
        @LLVMOr = 24,
        @LLVMXor = 25,
        @LLVMAlloca = 26,
        @LLVMLoad = 27,
        @LLVMStore = 28,
        @LLVMGetElementPtr = 29,
        @LLVMTrunc = 30,
        @LLVMZExt = 31,
        @LLVMSExt = 32,
        @LLVMFPToUI = 33,
        @LLVMFPToSI = 34,
        @LLVMUIToFP = 35,
        @LLVMSIToFP = 36,
        @LLVMFPTrunc = 37,
        @LLVMFPExt = 38,
        @LLVMPtrToInt = 39,
        @LLVMIntToPtr = 40,
        @LLVMBitCast = 41,
        @LLVMAddrSpaceCast = 60,
        @LLVMICmp = 42,
        @LLVMFCmp = 43,
        @LLVMPHI = 44,
        @LLVMCall = 45,
        @LLVMSelect = 46,
        @LLVMUserOp1 = 47,
        @LLVMUserOp2 = 48,
        @LLVMVAArg = 49,
        @LLVMExtractElement = 50,
        @LLVMInsertElement = 51,
        @LLVMShuffleVector = 52,
        @LLVMExtractValue = 53,
        @LLVMInsertValue = 54,
        @LLVMFence = 55,
        @LLVMAtomicCmpXchg = 56,
        @LLVMAtomicRMW = 57,
        @LLVMResume = 58,
        @LLVMLandingPad = 59,
    }

    internal enum LLVMTypeKind : uint
    {
        @LLVMVoidTypeKind = 0,
        @LLVMHalfTypeKind = 1,
        @LLVMFloatTypeKind = 2,
        @LLVMDoubleTypeKind = 3,
        @LLVMX86_FP80TypeKind = 4,
        @LLVMFP128TypeKind = 5,
        @LLVMPPC_FP128TypeKind = 6,
        @LLVMLabelTypeKind = 7,
        @LLVMIntegerTypeKind = 8,
        @LLVMFunctionTypeKind = 9,
        @LLVMStructTypeKind = 10,
        @LLVMArrayTypeKind = 11,
        @LLVMPointerTypeKind = 12,
        @LLVMVectorTypeKind = 13,
        @LLVMMetadataTypeKind = 14,
        @LLVMX86_MMXTypeKind = 15,
    }

    internal enum LLVMLinkage : uint
    {
        @LLVMExternalLinkage = 0,
        @LLVMAvailableExternallyLinkage = 1,
        @LLVMLinkOnceAnyLinkage = 2,
        @LLVMLinkOnceODRLinkage = 3,
        @LLVMLinkOnceODRAutoHideLinkage = 4,
        @LLVMWeakAnyLinkage = 5,
        @LLVMWeakODRLinkage = 6,
        @LLVMAppendingLinkage = 7,
        @LLVMInternalLinkage = 8,
        @LLVMPrivateLinkage = 9,
        @LLVMDLLImportLinkage = 10,
        @LLVMDLLExportLinkage = 11,
        @LLVMExternalWeakLinkage = 12,
        @LLVMGhostLinkage = 13,
        @LLVMCommonLinkage = 14,
        @LLVMLinkerPrivateLinkage = 15,
        @LLVMLinkerPrivateWeakLinkage = 16,
    }

    internal enum LLVMVisibility : uint
    {
        @LLVMDefaultVisibility = 0,
        @LLVMHiddenVisibility = 1,
        @LLVMProtectedVisibility = 2,
    }

    internal enum LLVMDLLStorageClass : uint
    {
        @LLVMDefaultStorageClass = 0,
        @LLVMDLLImportStorageClass = 1,
        @LLVMDLLExportStorageClass = 2,
    }

    internal enum LLVMCallConv : uint
    {
        @LLVMCCallConv = 0,
        @LLVMFastCallConv = 8,
        @LLVMColdCallConv = 9,
        @LLVMWebKitJSCallConv = 12,
        @LLVMAnyRegCallConv = 13,
        @LLVMX86StdcallCallConv = 64,
        @LLVMX86FastcallCallConv = 65,
    }

    internal enum LLVMIntPredicate : uint
    {
        @LLVMIntEQ = 32,
        @LLVMIntNE = 33,
        @LLVMIntUGT = 34,
        @LLVMIntUGE = 35,
        @LLVMIntULT = 36,
        @LLVMIntULE = 37,
        @LLVMIntSGT = 38,
        @LLVMIntSGE = 39,
        @LLVMIntSLT = 40,
        @LLVMIntSLE = 41,
    }

    internal enum LLVMRealPredicate : uint
    {
        @LLVMRealPredicateFalse = 0,
        @LLVMRealOEQ = 1,
        @LLVMRealOGT = 2,
        @LLVMRealOGE = 3,
        @LLVMRealOLT = 4,
        @LLVMRealOLE = 5,
        @LLVMRealONE = 6,
        @LLVMRealORD = 7,
        @LLVMRealUNO = 8,
        @LLVMRealUEQ = 9,
        @LLVMRealUGT = 10,
        @LLVMRealUGE = 11,
        @LLVMRealULT = 12,
        @LLVMRealULE = 13,
        @LLVMRealUNE = 14,
        @LLVMRealPredicateTrue = 15,
    }

    internal enum LLVMLandingPadClauseTy : uint
    {
        @LLVMLandingPadCatch = 0,
        @LLVMLandingPadFilter = 1,
    }

    internal enum LLVMThreadLocalMode : uint
    {
        @LLVMNotThreadLocal = 0,
        @LLVMGeneralDynamicTLSModel = 1,
        @LLVMLocalDynamicTLSModel = 2,
        @LLVMInitialExecTLSModel = 3,
        @LLVMLocalExecTLSModel = 4,
    }

    internal enum LLVMAtomicOrdering : uint
    {
        @LLVMAtomicOrderingNotAtomic = 0,
        @LLVMAtomicOrderingUnordered = 1,
        @LLVMAtomicOrderingMonotonic = 2,
        @LLVMAtomicOrderingAcquire = 4,
        @LLVMAtomicOrderingRelease = 5,
        @LLVMAtomicOrderingAcquireRelease = 6,
        @LLVMAtomicOrderingSequentiallyConsistent = 7,
    }

    internal enum LLVMAtomicRMWBinOp : uint
    {
        @LLVMAtomicRMWBinOpXchg = 0,
        @LLVMAtomicRMWBinOpAdd = 1,
        @LLVMAtomicRMWBinOpSub = 2,
        @LLVMAtomicRMWBinOpAnd = 3,
        @LLVMAtomicRMWBinOpNand = 4,
        @LLVMAtomicRMWBinOpOr = 5,
        @LLVMAtomicRMWBinOpXor = 6,
        @LLVMAtomicRMWBinOpMax = 7,
        @LLVMAtomicRMWBinOpMin = 8,
        @LLVMAtomicRMWBinOpUMax = 9,
        @LLVMAtomicRMWBinOpUMin = 10,
    }

    internal enum LLVMDiagnosticSeverity : uint
    {
        @LLVMDSError = 0,
        @LLVMDSWarning = 1,
        @LLVMDSRemark = 2,
        @LLVMDSNote = 3,
    }

    internal enum LLVMVerifierFailureAction : uint
    {
        @LLVMAbortProcessAction = 0,
        @LLVMPrintMessageAction = 1,
        @LLVMReturnStatusAction = 2,
    }

    internal enum LLVMByteOrdering : uint
    {
        @LLVMBigEndian = 0,
        @LLVMLittleEndian = 1,
    }

    internal enum LLVMCodeGenOptLevel : uint
    {
        @LLVMCodeGenLevelNone = 0,
        @LLVMCodeGenLevelLess = 1,
        @LLVMCodeGenLevelDefault = 2,
        @LLVMCodeGenLevelAggressive = 3,
    }

    internal enum LLVMRelocMode : uint
    {
        @LLVMRelocDefault = 0,
        @LLVMRelocStatic = 1,
        @LLVMRelocPIC = 2,
        @LLVMRelocDynamicNoPic = 3,
    }

    internal enum LLVMCodeModel : uint
    {
        @LLVMCodeModelDefault = 0,
        @LLVMCodeModelJITDefault = 1,
        @LLVMCodeModelSmall = 2,
        @LLVMCodeModelKernel = 3,
        @LLVMCodeModelMedium = 4,
        @LLVMCodeModelLarge = 5,
    }

    internal enum LLVMCodeGenFileType : uint
    {
        @LLVMAssemblyFile = 0,
        @LLVMObjectFile = 1,
    }

    internal enum llvm_lto_status : uint
    {
        @LLVM_LTO_UNKNOWN = 0,
        @LLVM_LTO_OPT_SUCCESS = 1,
        @LLVM_LTO_READ_SUCCESS = 2,
        @LLVM_LTO_READ_FAILURE = 3,
        @LLVM_LTO_WRITE_FAILURE = 4,
        @LLVM_LTO_NO_TARGET = 5,
        @LLVM_LTO_NO_WORK = 6,
        @LLVM_LTO_MODULE_MERGE_FAILURE = 7,
        @LLVM_LTO_ASM_FAILURE = 8,
        @LLVM_LTO_NULL_OBJECT = 9,
    }

    internal enum lto_symbol_attributes : uint
    {
        @LTO_SYMBOL_ALIGNMENT_MASK = 31,
        @LTO_SYMBOL_PERMISSIONS_MASK = 224,
        @LTO_SYMBOL_PERMISSIONS_CODE = 160,
        @LTO_SYMBOL_PERMISSIONS_DATA = 192,
        @LTO_SYMBOL_PERMISSIONS_RODATA = 128,
        @LTO_SYMBOL_DEFINITION_MASK = 1792,
        @LTO_SYMBOL_DEFINITION_REGULAR = 256,
        @LTO_SYMBOL_DEFINITION_TENTATIVE = 512,
        @LTO_SYMBOL_DEFINITION_WEAK = 768,
        @LTO_SYMBOL_DEFINITION_UNDEFINED = 1024,
        @LTO_SYMBOL_DEFINITION_WEAKUNDEF = 1280,
        @LTO_SYMBOL_SCOPE_MASK = 14336,
        @LTO_SYMBOL_SCOPE_INTERNAL = 2048,
        @LTO_SYMBOL_SCOPE_HIDDEN = 4096,
        @LTO_SYMBOL_SCOPE_PROTECTED = 8192,
        @LTO_SYMBOL_SCOPE_DEFAULT = 6144,
        @LTO_SYMBOL_SCOPE_DEFAULT_CAN_BE_HIDDEN = 10240,
    }

    internal enum lto_debug_model : uint
    {
        @LTO_DEBUG_MODEL_NONE = 0,
        @LTO_DEBUG_MODEL_DWARF = 1,
    }

    internal enum lto_codegen_model : uint
    {
        @LTO_CODEGEN_PIC_MODEL_STATIC = 0,
        @LTO_CODEGEN_PIC_MODEL_DYNAMIC = 1,
        @LTO_CODEGEN_PIC_MODEL_DYNAMIC_NO_PIC = 2,
        @LTO_CODEGEN_PIC_MODEL_DEFAULT = 3,
    }

    internal enum lto_codegen_diagnostic_severity_t : uint
    {
        @LTO_DS_ERROR = 0,
        @LTO_DS_WARNING = 1,
        @LTO_DS_REMARK = 3,
        @LTO_DS_NOTE = 2,
    }

    internal enum LLVMLinkerMode : uint
    {
        @LLVMLinkerDestroySource = 0, /* Allow source module to be destroyed. */
        @LLVMLinkerPreserveSource = 1 /* Preserve the source module. */
    }

    internal static partial class NativeMethods
    {
        private const string libraryPath = "LibLlvm.dll";

        [DllImport(libraryPath, EntryPoint = "LLVMLoadLibraryPermanently", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true )]
        internal static extern LLVMBool LoadLibraryPermanently([MarshalAs(UnmanagedType.LPStr)] string @Filename);

        [DllImport(libraryPath, EntryPoint = "LLVMParseCommandLineOptions", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void ParseCommandLineOptions(int @argc, string[] @argv, [MarshalAs(UnmanagedType.LPStr)] string @Overview);

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeCore", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeCore(LLVMPassRegistryRef @R);

        [DllImport(libraryPath, EntryPoint = "LLVMShutdown", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void Shutdown();

        [DllImport(libraryPath, EntryPoint = "LLVMCreateMessage", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr CreateMessage([MarshalAs(UnmanagedType.LPStr)] string @Message);

        [DllImport(libraryPath, EntryPoint = "LLVMDisposeMessage", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DisposeMessage(IntPtr @Message);

        [DllImport(libraryPath, EntryPoint = "LLVMInstallFatalErrorHandler", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InstallFatalErrorHandler(LLVMFatalErrorHandler @Handler);

        [DllImport(libraryPath, EntryPoint = "LLVMResetFatalErrorHandler", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void ResetFatalErrorHandler();

        [DllImport(libraryPath, EntryPoint = "LLVMEnablePrettyStackTrace", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void EnablePrettyStackTrace();

        [DllImport(libraryPath, EntryPoint = "LLVMContextCreate", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMContextRef ContextCreate();

        [DllImport(libraryPath, EntryPoint = "LLVMGetGlobalContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMContextRef GetGlobalContext();

        [DllImport(libraryPath, EntryPoint = "LLVMContextSetDiagnosticHandler", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void ContextSetDiagnosticHandler(LLVMContextRef @C, LLVMDiagnosticHandler @Handler, IntPtr @DiagnosticContext);

        [DllImport(libraryPath, EntryPoint = "LLVMContextSetYieldCallback", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void ContextSetYieldCallback(LLVMContextRef @C, LLVMYieldCallback @Callback, IntPtr @OpaqueHandle);

        [DllImport(libraryPath, EntryPoint = "LLVMContextDispose", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void ContextDispose(LLVMContextRef @C);

        [DllImport(libraryPath, EntryPoint = "LLVMGetDiagInfoDescription", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetDiagInfoDescription(LLVMDiagnosticInfoRef @DI);

        [DllImport(libraryPath, EntryPoint = "LLVMGetDiagInfoSeverity", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMDiagnosticSeverity GetDiagInfoSeverity(LLVMDiagnosticInfoRef @DI);

        [DllImport(libraryPath, EntryPoint = "LLVMGetMDKindIDInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint GetMDKindIDInContext(LLVMContextRef @C, [MarshalAs(UnmanagedType.LPStr)] string @Name, uint @SLen);

        [DllImport(libraryPath, EntryPoint = "LLVMGetMDKindID", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint GetMDKindID([MarshalAs(UnmanagedType.LPStr)] string @Name, uint @SLen);

        [DllImport(libraryPath, EntryPoint = "LLVMModuleCreateWithName", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMModuleRef ModuleCreateWithName([MarshalAs(UnmanagedType.LPStr)] string @ModuleID);

        [DllImport(libraryPath, EntryPoint = "LLVMModuleCreateWithNameInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMModuleRef ModuleCreateWithNameInContext([MarshalAs(UnmanagedType.LPStr)] string @ModuleID, LLVMContextRef @C);

        [DllImport(libraryPath, EntryPoint = "LLVMCloneModule", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMModuleRef CloneModule(LLVMModuleRef @M);

        [DllImport(libraryPath, EntryPoint = "LLVMDisposeModule", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DisposeModule(LLVMModuleRef @M);

        [DllImport(libraryPath, EntryPoint = "LLVMGetDataLayout", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetDataLayout(LLVMModuleRef @M);

        [DllImport(libraryPath, EntryPoint = "LLVMSetDataLayout", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetDataLayout(LLVMModuleRef @M, [MarshalAs(UnmanagedType.LPStr)] string @Triple);

        [DllImport(libraryPath, EntryPoint = "LLVMGetTarget", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetTarget(LLVMModuleRef @M);

        [DllImport(libraryPath, EntryPoint = "LLVMSetTarget", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetTarget(LLVMModuleRef @M, [MarshalAs(UnmanagedType.LPStr)] string @Triple);

        [DllImport(libraryPath, EntryPoint = "LLVMDumpModule", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DumpModule(LLVMModuleRef @M);

        [DllImport(libraryPath, EntryPoint = "LLVMPrintModuleToFile", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool PrintModuleToFile(LLVMModuleRef @M, [MarshalAs(UnmanagedType.LPStr)] string @Filename, out IntPtr @ErrorMessage);

        [DllImport(libraryPath, EntryPoint = "LLVMPrintModuleToString", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr PrintModuleToString(LLVMModuleRef @M);

        [DllImport(libraryPath, EntryPoint = "LLVMSetModuleInlineAsm", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetModuleInlineAsm(LLVMModuleRef @M, [MarshalAs(UnmanagedType.LPStr)] string @Asm);

        [DllImport(libraryPath, EntryPoint = "LLVMGetModuleContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMContextRef GetModuleContext(LLVMModuleRef @M);

        [DllImport(libraryPath, EntryPoint = "LLVMGetTypeByName", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef GetTypeByName(LLVMModuleRef @M, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMGetNamedMetadataNumOperands", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint GetNamedMetadataNumOperands(LLVMModuleRef @M, [MarshalAs(UnmanagedType.LPStr)] string @name);

        [DllImport(libraryPath, EntryPoint = "LLVMGetNamedMetadataOperands", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void GetNamedMetadataOperands(LLVMModuleRef @M, [MarshalAs(UnmanagedType.LPStr)] string @name, out LLVMValueRef @Dest);

        [DllImport(libraryPath, EntryPoint = "LLVMAddNamedMetadataOperand", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddNamedMetadataOperand(LLVMModuleRef @M, [MarshalAs(UnmanagedType.LPStr)] string @name, LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMAddFunction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef AddFunction(LLVMModuleRef @M, [MarshalAs(UnmanagedType.LPStr)] string @Name, LLVMTypeRef @FunctionTy);

        [DllImport(libraryPath, EntryPoint = "LLVMGetNamedFunction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetNamedFunction(LLVMModuleRef @M, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMGetFirstFunction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetFirstFunction(LLVMModuleRef @M);

        [DllImport(libraryPath, EntryPoint = "LLVMGetLastFunction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetLastFunction(LLVMModuleRef @M);

        [DllImport(libraryPath, EntryPoint = "LLVMGetNextFunction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetNextFunction(LLVMValueRef @Fn);

        [DllImport(libraryPath, EntryPoint = "LLVMGetPreviousFunction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetPreviousFunction(LLVMValueRef @Fn);

        [DllImport(libraryPath, EntryPoint = "LLVMGetTypeKind", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeKind GetTypeKind(LLVMTypeRef @Ty);

        [DllImport(libraryPath, EntryPoint = "LLVMTypeIsSized", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool TypeIsSized(LLVMTypeRef @Ty);

        [DllImport(libraryPath, EntryPoint = "LLVMGetTypeContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMContextRef GetTypeContext(LLVMTypeRef @Ty);

        [DllImport(libraryPath, EntryPoint = "LLVMDumpType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DumpType(LLVMTypeRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMPrintTypeToString", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr PrintTypeToString(LLVMTypeRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMInt1TypeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef Int1TypeInContext(LLVMContextRef @C);

        [DllImport(libraryPath, EntryPoint = "LLVMInt8TypeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef Int8TypeInContext(LLVMContextRef @C);

        [DllImport(libraryPath, EntryPoint = "LLVMInt16TypeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef Int16TypeInContext(LLVMContextRef @C);

        [DllImport(libraryPath, EntryPoint = "LLVMInt32TypeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef Int32TypeInContext(LLVMContextRef @C);

        [DllImport(libraryPath, EntryPoint = "LLVMInt64TypeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef Int64TypeInContext(LLVMContextRef @C);

        [DllImport(libraryPath, EntryPoint = "LLVMIntTypeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef IntTypeInContext(LLVMContextRef @C, uint @NumBits);

        [DllImport(libraryPath, EntryPoint = "LLVMInt1Type", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef Int1Type();

        [DllImport(libraryPath, EntryPoint = "LLVMInt8Type", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef Int8Type();

        [DllImport(libraryPath, EntryPoint = "LLVMInt16Type", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef Int16Type();

        [DllImport(libraryPath, EntryPoint = "LLVMInt32Type", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef Int32Type();

        [DllImport(libraryPath, EntryPoint = "LLVMInt64Type", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef Int64Type();

        [DllImport(libraryPath, EntryPoint = "LLVMIntType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef IntType(uint @NumBits);

        [DllImport(libraryPath, EntryPoint = "LLVMGetIntTypeWidth", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint GetIntTypeWidth(LLVMTypeRef @IntegerTy);

        [DllImport(libraryPath, EntryPoint = "LLVMHalfTypeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef HalfTypeInContext(LLVMContextRef @C);

        [DllImport(libraryPath, EntryPoint = "LLVMFloatTypeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef FloatTypeInContext(LLVMContextRef @C);

        [DllImport(libraryPath, EntryPoint = "LLVMDoubleTypeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef DoubleTypeInContext(LLVMContextRef @C);

        [DllImport(libraryPath, EntryPoint = "LLVMX86FP80TypeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef X86FP80TypeInContext(LLVMContextRef @C);

        [DllImport(libraryPath, EntryPoint = "LLVMFP128TypeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef FP128TypeInContext(LLVMContextRef @C);

        [DllImport(libraryPath, EntryPoint = "LLVMPPCFP128TypeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef PPCFP128TypeInContext(LLVMContextRef @C);

        [DllImport(libraryPath, EntryPoint = "LLVMHalfType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef HalfType();

        [DllImport(libraryPath, EntryPoint = "LLVMFloatType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef FloatType();

        [DllImport(libraryPath, EntryPoint = "LLVMDoubleType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef DoubleType();

        [DllImport(libraryPath, EntryPoint = "LLVMX86FP80Type", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef X86FP80Type();

        [DllImport(libraryPath, EntryPoint = "LLVMFP128Type", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef FP128Type();

        [DllImport(libraryPath, EntryPoint = "LLVMPPCFP128Type", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef PPCFP128Type();

        [DllImport(libraryPath, EntryPoint = "LLVMFunctionType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef FunctionType(LLVMTypeRef @ReturnType, out LLVMTypeRef @ParamTypes, uint @ParamCount, LLVMBool @IsVarArg);

        [DllImport(libraryPath, EntryPoint = "LLVMIsFunctionVarArg", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool IsFunctionVarArg(LLVMTypeRef @FunctionTy);

        [DllImport(libraryPath, EntryPoint = "LLVMGetReturnType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef GetReturnType(LLVMTypeRef @FunctionTy);

        [DllImport(libraryPath, EntryPoint = "LLVMCountParamTypes", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint CountParamTypes(LLVMTypeRef @FunctionTy);

        [DllImport(libraryPath, EntryPoint = "LLVMGetParamTypes", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void GetParamTypes(LLVMTypeRef @FunctionTy, out LLVMTypeRef @Dest);

        [DllImport(libraryPath, EntryPoint = "LLVMStructTypeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef StructTypeInContext(LLVMContextRef @C, out LLVMTypeRef @ElementTypes, uint @ElementCount, LLVMBool @Packed);

        [DllImport(libraryPath, EntryPoint = "LLVMStructType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef StructType(out LLVMTypeRef @ElementTypes, uint @ElementCount, LLVMBool @Packed);

        [DllImport(libraryPath, EntryPoint = "LLVMStructCreateNamed", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef StructCreateNamed(LLVMContextRef @C, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMGetStructName", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetStructName(LLVMTypeRef @Ty);

        [DllImport(libraryPath, EntryPoint = "LLVMStructSetBody", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void StructSetBody(LLVMTypeRef @StructTy, out LLVMTypeRef @ElementTypes, uint @ElementCount, LLVMBool @Packed);

        [DllImport(libraryPath, EntryPoint = "LLVMCountStructElementTypes", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint CountStructElementTypes(LLVMTypeRef @StructTy);

        [DllImport(libraryPath, EntryPoint = "LLVMGetStructElementTypes", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void GetStructElementTypes(LLVMTypeRef @StructTy, out LLVMTypeRef @Dest);

        [DllImport(libraryPath, EntryPoint = "LLVMIsPackedStruct", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool IsPackedStruct(LLVMTypeRef @StructTy);

        [DllImport(libraryPath, EntryPoint = "LLVMIsOpaqueStruct", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool IsOpaqueStruct(LLVMTypeRef @StructTy);

        [DllImport(libraryPath, EntryPoint = "LLVMGetElementType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef GetElementType(LLVMTypeRef @Ty);

        [DllImport(libraryPath, EntryPoint = "LLVMArrayType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef ArrayType(LLVMTypeRef @ElementType, uint @ElementCount);

        [DllImport(libraryPath, EntryPoint = "LLVMGetArrayLength", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint GetArrayLength(LLVMTypeRef @ArrayTy);

        [DllImport(libraryPath, EntryPoint = "LLVMPointerType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef PointerType(LLVMTypeRef @ElementType, uint @AddressSpace);

        [DllImport(libraryPath, EntryPoint = "LLVMGetPointerAddressSpace", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint GetPointerAddressSpace(LLVMTypeRef @PointerTy);

        [DllImport(libraryPath, EntryPoint = "LLVMVectorType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef VectorType(LLVMTypeRef @ElementType, uint @ElementCount);

        [DllImport(libraryPath, EntryPoint = "LLVMGetVectorSize", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint GetVectorSize(LLVMTypeRef @VectorTy);

        [DllImport(libraryPath, EntryPoint = "LLVMVoidTypeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef VoidTypeInContext(LLVMContextRef @C);

        [DllImport(libraryPath, EntryPoint = "LLVMLabelTypeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef LabelTypeInContext(LLVMContextRef @C);

        [DllImport(libraryPath, EntryPoint = "LLVMX86MMXTypeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef X86MMXTypeInContext(LLVMContextRef @C);

        [DllImport(libraryPath, EntryPoint = "LLVMVoidType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef VoidType();

        [DllImport(libraryPath, EntryPoint = "LLVMLabelType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef LabelType();

        [DllImport(libraryPath, EntryPoint = "LLVMX86MMXType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef X86MMXType();

        [DllImport(libraryPath, EntryPoint = "LLVMTypeOf", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef TypeOf(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMGetValueName", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetValueName(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMSetValueName", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetValueName(LLVMValueRef @Val, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMDumpValue", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DumpValue(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMPrintValueToString", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr PrintValueToString(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMReplaceAllUsesWith", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void ReplaceAllUsesWith(LLVMValueRef @OldVal, LLVMValueRef @NewVal);

        [DllImport(libraryPath, EntryPoint = "LLVMIsConstant", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool IsConstant(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsUndef", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool IsUndef(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAArgument", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAArgument(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsABasicBlock", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsABasicBlock(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAInlineAsm", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAInlineAsm(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAUser", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAUser(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAConstant", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAConstant(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsABlockAddress", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsABlockAddress(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAConstantAggregateZero", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAConstantAggregateZero(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAConstantArray", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAConstantArray(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAConstantDataSequential", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAConstantDataSequential(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAConstantDataArray", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAConstantDataArray(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAConstantDataVector", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAConstantDataVector(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAConstantExpr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAConstantExpr(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAConstantFP", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAConstantFP(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAConstantInt", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAConstantInt(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAConstantPointerNull", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAConstantPointerNull(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAConstantStruct", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAConstantStruct(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAConstantVector", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAConstantVector(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAGlobalValue", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAGlobalValue(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAGlobalAlias", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAGlobalAlias(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAGlobalObject", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAGlobalObject(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAFunction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAFunction(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAGlobalVariable", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAGlobalVariable(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAUndefValue", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAUndefValue(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAInstruction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAInstruction(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsABinaryOperator", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsABinaryOperator(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsACallInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsACallInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAIntrinsicInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAIntrinsicInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsADbgInfoIntrinsic", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsADbgInfoIntrinsic(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsADbgDeclareInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsADbgDeclareInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAMemIntrinsic", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAMemIntrinsic(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAMemCpyInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAMemCpyInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAMemMoveInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAMemMoveInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAMemSetInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAMemSetInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsACmpInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsACmpInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAFCmpInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAFCmpInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAICmpInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAICmpInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAExtractElementInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAExtractElementInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAGetElementPtrInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAGetElementPtrInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAInsertElementInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAInsertElementInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAInsertValueInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAInsertValueInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsALandingPadInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsALandingPadInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAPHINode", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAPHINode(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsASelectInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsASelectInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAShuffleVectorInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAShuffleVectorInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAStoreInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAStoreInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsATerminatorInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsATerminatorInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsABranchInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsABranchInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAIndirectBrInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAIndirectBrInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAInvokeInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAInvokeInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAReturnInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAReturnInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsASwitchInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsASwitchInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAUnreachableInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAUnreachableInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAResumeInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAResumeInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAUnaryInstruction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAUnaryInstruction(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAAllocaInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAAllocaInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsACastInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsACastInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAAddrSpaceCastInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAAddrSpaceCastInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsABitCastInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsABitCastInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAFPExtInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAFPExtInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAFPToSIInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAFPToSIInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAFPToUIInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAFPToUIInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAFPTruncInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAFPTruncInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAIntToPtrInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAIntToPtrInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAPtrToIntInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAPtrToIntInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsASExtInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsASExtInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsASIToFPInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsASIToFPInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsATruncInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsATruncInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAUIToFPInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAUIToFPInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAZExtInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAZExtInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAExtractValueInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAExtractValueInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsALoadInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsALoadInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAVAArgInst", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAVAArgInst(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAMDNode", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAMDNode(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMIsAMDString", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef IsAMDString(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMGetFirstUse", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMUseRef GetFirstUse(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMGetNextUse", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMUseRef GetNextUse(LLVMUseRef @U);

        [DllImport(libraryPath, EntryPoint = "LLVMGetUser", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetUser(LLVMUseRef @U);

        [DllImport(libraryPath, EntryPoint = "LLVMGetUsedValue", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetUsedValue(LLVMUseRef @U);

        [DllImport(libraryPath, EntryPoint = "LLVMGetOperand", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetOperand(LLVMValueRef @Val, uint @Index);

        [DllImport(libraryPath, EntryPoint = "LLVMGetOperandUse", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMUseRef GetOperandUse(LLVMValueRef @Val, uint @Index);

        [DllImport(libraryPath, EntryPoint = "LLVMSetOperand", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetOperand(LLVMValueRef @User, uint @Index, LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMGetNumOperands", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int GetNumOperands(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMConstNull", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstNull(LLVMTypeRef @Ty);

        [DllImport(libraryPath, EntryPoint = "LLVMConstAllOnes", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstAllOnes(LLVMTypeRef @Ty);

        [DllImport(libraryPath, EntryPoint = "LLVMGetUndef", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetUndef(LLVMTypeRef @Ty);

        [DllImport(libraryPath, EntryPoint = "LLVMIsNull", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool IsNull(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMConstPointerNull", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstPointerNull(LLVMTypeRef @Ty);

        [DllImport(libraryPath, EntryPoint = "LLVMConstInt", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstInt(LLVMTypeRef @IntTy, ulong @N, LLVMBool @SignExtend);

        [DllImport(libraryPath, EntryPoint = "LLVMConstIntOfArbitraryPrecision", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstIntOfArbitraryPrecision(LLVMTypeRef @IntTy, uint @NumWords, int[] @Words);

        [DllImport(libraryPath, EntryPoint = "LLVMConstIntOfString", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstIntOfString(LLVMTypeRef @IntTy, [MarshalAs(UnmanagedType.LPStr)] string @Text, char @Radix);

        [DllImport(libraryPath, EntryPoint = "LLVMConstIntOfStringAndSize", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstIntOfStringAndSize(LLVMTypeRef @IntTy, [MarshalAs(UnmanagedType.LPStr)] string @Text, uint @SLen, char @Radix);

        [DllImport(libraryPath, EntryPoint = "LLVMConstReal", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstReal(LLVMTypeRef @RealTy, double @N);

        [DllImport(libraryPath, EntryPoint = "LLVMConstRealOfString", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstRealOfString(LLVMTypeRef @RealTy, [MarshalAs(UnmanagedType.LPStr)] string @Text);

        [DllImport(libraryPath, EntryPoint = "LLVMConstRealOfStringAndSize", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstRealOfStringAndSize(LLVMTypeRef @RealTy, [MarshalAs(UnmanagedType.LPStr)] string @Text, uint @SLen);

        [DllImport(libraryPath, EntryPoint = "LLVMConstIntGetZExtValue", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern ulong ConstIntGetZExtValue(LLVMValueRef @ConstantVal);

        [DllImport(libraryPath, EntryPoint = "LLVMConstIntGetSExtValue", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern long ConstIntGetSExtValue(LLVMValueRef @ConstantVal);

        [DllImport(libraryPath, EntryPoint = "LLVMConstRealGetDouble", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern double ConstRealGetDouble(LLVMValueRef @ConstantVal, out LLVMBool @losesInfo);

        [DllImport(libraryPath, EntryPoint = "LLVMConstStringInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstStringInContext(LLVMContextRef @C, [MarshalAs(UnmanagedType.LPStr)] string @Str, uint @Length, LLVMBool @DontNullTerminate);

        [DllImport(libraryPath, EntryPoint = "LLVMConstString", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstString([MarshalAs(UnmanagedType.LPStr)] string @Str, uint @Length, LLVMBool @DontNullTerminate);

        [DllImport(libraryPath, EntryPoint = "LLVMIsConstantString", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool IsConstantString(LLVMValueRef @c);

        [DllImport(libraryPath, EntryPoint = "LLVMGetAsString", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetAsString(LLVMValueRef @c, out int @out);

        [DllImport(libraryPath, EntryPoint = "LLVMConstStructInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstStructInContext(LLVMContextRef @C, out LLVMValueRef @ConstantVals, uint @Count, LLVMBool @Packed);

        [DllImport(libraryPath, EntryPoint = "LLVMConstStruct", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstStruct(out LLVMValueRef @ConstantVals, uint @Count, LLVMBool @Packed);

        [DllImport(libraryPath, EntryPoint = "LLVMConstArray", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstArray(LLVMTypeRef @ElementTy, out LLVMValueRef @ConstantVals, uint @Length);

        [DllImport(libraryPath, EntryPoint = "LLVMConstNamedStruct", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstNamedStruct(LLVMTypeRef @StructTy, out LLVMValueRef @ConstantVals, uint @Count);

        [DllImport(libraryPath, EntryPoint = "LLVMGetElementAsConstant", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetElementAsConstant(LLVMValueRef @c, uint @idx);

        [DllImport(libraryPath, EntryPoint = "LLVMConstVector", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstVector(out LLVMValueRef @ScalarConstantVals, uint @Size);

        [DllImport(libraryPath, EntryPoint = "LLVMGetConstOpcode", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMOpcode GetConstOpcode(LLVMValueRef @ConstantVal);

        [DllImport(libraryPath, EntryPoint = "LLVMAlignOf", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef AlignOf(LLVMTypeRef @Ty);

        [DllImport(libraryPath, EntryPoint = "LLVMSizeOf", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef SizeOf(LLVMTypeRef @Ty);

        [DllImport(libraryPath, EntryPoint = "LLVMConstNeg", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstNeg(LLVMValueRef @ConstantVal);

        [DllImport(libraryPath, EntryPoint = "LLVMConstNSWNeg", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstNSWNeg(LLVMValueRef @ConstantVal);

        [DllImport(libraryPath, EntryPoint = "LLVMConstNUWNeg", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstNUWNeg(LLVMValueRef @ConstantVal);

        [DllImport(libraryPath, EntryPoint = "LLVMConstFNeg", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstFNeg(LLVMValueRef @ConstantVal);

        [DllImport(libraryPath, EntryPoint = "LLVMConstNot", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstNot(LLVMValueRef @ConstantVal);

        [DllImport(libraryPath, EntryPoint = "LLVMConstAdd", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstAdd(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstNSWAdd", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstNSWAdd(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstNUWAdd", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstNUWAdd(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstFAdd", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstFAdd(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstSub", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstSub(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstNSWSub", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstNSWSub(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstNUWSub", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstNUWSub(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstFSub", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstFSub(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstMul", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstMul(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstNSWMul", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstNSWMul(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstNUWMul", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstNUWMul(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstFMul", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstFMul(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstUDiv", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstUDiv(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstSDiv", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstSDiv(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstExactSDiv", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstExactSDiv(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstFDiv", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstFDiv(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstURem", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstURem(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstSRem", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstSRem(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstFRem", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstFRem(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstAnd", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstAnd(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstOr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstOr(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstXor", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstXor(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstICmp", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstICmp(LLVMIntPredicate @Predicate, LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstFCmp", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstFCmp(LLVMRealPredicate @Predicate, LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstShl", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstShl(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstLShr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstLShr(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstAShr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstAShr(LLVMValueRef @LHSConstant, LLVMValueRef @RHSConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstGEP", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstGEP(LLVMValueRef @ConstantVal, out LLVMValueRef @ConstantIndices, uint @NumIndices);

        [DllImport(libraryPath, EntryPoint = "LLVMConstInBoundsGEP", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstInBoundsGEP(LLVMValueRef @ConstantVal, out LLVMValueRef @ConstantIndices, uint @NumIndices);

        [DllImport(libraryPath, EntryPoint = "LLVMConstTrunc", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstTrunc(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType);

        [DllImport(libraryPath, EntryPoint = "LLVMConstSExt", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstSExt(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType);

        [DllImport(libraryPath, EntryPoint = "LLVMConstZExt", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstZExt(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType);

        [DllImport(libraryPath, EntryPoint = "LLVMConstFPTrunc", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstFPTrunc(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType);

        [DllImport(libraryPath, EntryPoint = "LLVMConstFPExt", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstFPExt(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType);

        [DllImport(libraryPath, EntryPoint = "LLVMConstUIToFP", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstUIToFP(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType);

        [DllImport(libraryPath, EntryPoint = "LLVMConstSIToFP", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstSIToFP(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType);

        [DllImport(libraryPath, EntryPoint = "LLVMConstFPToUI", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstFPToUI(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType);

        [DllImport(libraryPath, EntryPoint = "LLVMConstFPToSI", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstFPToSI(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType);

        [DllImport(libraryPath, EntryPoint = "LLVMConstPtrToInt", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstPtrToInt(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType);

        [DllImport(libraryPath, EntryPoint = "LLVMConstIntToPtr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstIntToPtr(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType);

        [DllImport(libraryPath, EntryPoint = "LLVMConstBitCast", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstBitCast(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType);

        [DllImport(libraryPath, EntryPoint = "LLVMConstAddrSpaceCast", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstAddrSpaceCast(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType);

        [DllImport(libraryPath, EntryPoint = "LLVMConstZExtOrBitCast", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstZExtOrBitCast(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType);

        [DllImport(libraryPath, EntryPoint = "LLVMConstSExtOrBitCast", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstSExtOrBitCast(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType);

        [DllImport(libraryPath, EntryPoint = "LLVMConstTruncOrBitCast", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstTruncOrBitCast(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType);

        [DllImport(libraryPath, EntryPoint = "LLVMConstPointerCast", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstPointerCast(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType);

        [DllImport(libraryPath, EntryPoint = "LLVMConstIntCast", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstIntCast(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType, LLVMBool @isSigned);

        [DllImport(libraryPath, EntryPoint = "LLVMConstFPCast", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstFPCast(LLVMValueRef @ConstantVal, LLVMTypeRef @ToType);

        [DllImport(libraryPath, EntryPoint = "LLVMConstSelect", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstSelect(LLVMValueRef @ConstantCondition, LLVMValueRef @ConstantIfTrue, LLVMValueRef @ConstantIfFalse);

        [DllImport(libraryPath, EntryPoint = "LLVMConstExtractElement", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstExtractElement(LLVMValueRef @VectorConstant, LLVMValueRef @IndexConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstInsertElement", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstInsertElement(LLVMValueRef @VectorConstant, LLVMValueRef @ElementValueConstant, LLVMValueRef @IndexConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstShuffleVector", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstShuffleVector(LLVMValueRef @VectorAConstant, LLVMValueRef @VectorBConstant, LLVMValueRef @MaskConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMConstExtractValue", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstExtractValue(LLVMValueRef @AggConstant, out uint @IdxList, uint @NumIdx);

        [DllImport(libraryPath, EntryPoint = "LLVMConstInsertValue", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstInsertValue(LLVMValueRef @AggConstant, LLVMValueRef @ElementValueConstant, out uint @IdxList, uint @NumIdx);

        [DllImport(libraryPath, EntryPoint = "LLVMConstInlineAsm", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef ConstInlineAsm(LLVMTypeRef @Ty, [MarshalAs(UnmanagedType.LPStr)] string @AsmString, [MarshalAs(UnmanagedType.LPStr)] string @Constraints, LLVMBool @HasSideEffects, LLVMBool @IsAlignStack);

        [DllImport(libraryPath, EntryPoint = "LLVMBlockAddress", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BlockAddress(LLVMValueRef @F, LLVMBasicBlockRef @BB);

        [DllImport(libraryPath, EntryPoint = "LLVMGetGlobalParent", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMModuleRef GetGlobalParent(LLVMValueRef @Global);

        [DllImport(libraryPath, EntryPoint = "LLVMIsDeclaration", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool IsDeclaration(LLVMValueRef @Global);

        [DllImport(libraryPath, EntryPoint = "LLVMGetLinkage", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMLinkage GetLinkage(LLVMValueRef @Global);

        [DllImport(libraryPath, EntryPoint = "LLVMSetLinkage", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetLinkage(LLVMValueRef @Global, LLVMLinkage @Linkage);

        [DllImport(libraryPath, EntryPoint = "LLVMGetSection", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetSection(LLVMValueRef @Global);

        [DllImport(libraryPath, EntryPoint = "LLVMSetSection", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetSection(LLVMValueRef @Global, [MarshalAs(UnmanagedType.LPStr)] string @Section);

        [DllImport(libraryPath, EntryPoint = "LLVMGetVisibility", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMVisibility GetVisibility(LLVMValueRef @Global);

        [DllImport(libraryPath, EntryPoint = "LLVMSetVisibility", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetVisibility(LLVMValueRef @Global, LLVMVisibility @Viz);

        [DllImport(libraryPath, EntryPoint = "LLVMGetDLLStorageClass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMDLLStorageClass GetDLLStorageClass(LLVMValueRef @Global);

        [DllImport(libraryPath, EntryPoint = "LLVMSetDLLStorageClass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetDLLStorageClass(LLVMValueRef @Global, LLVMDLLStorageClass @Class);

        [DllImport(libraryPath, EntryPoint = "LLVMHasUnnamedAddr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool HasUnnamedAddr(LLVMValueRef @Global);

        [DllImport(libraryPath, EntryPoint = "LLVMSetUnnamedAddr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetUnnamedAddr(LLVMValueRef @Global, LLVMBool @HasUnnamedAddr);

        [DllImport(libraryPath, EntryPoint = "LLVMGetAlignment", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint GetAlignment(LLVMValueRef @V);

        [DllImport(libraryPath, EntryPoint = "LLVMSetAlignment", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetAlignment(LLVMValueRef @V, uint @Bytes);

        [DllImport(libraryPath, EntryPoint = "LLVMAddGlobal", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef AddGlobal(LLVMModuleRef @M, LLVMTypeRef @Ty, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMAddGlobalInAddressSpace", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef AddGlobalInAddressSpace(LLVMModuleRef @M, LLVMTypeRef @Ty, [MarshalAs(UnmanagedType.LPStr)] string @Name, uint @AddressSpace);

        [DllImport(libraryPath, EntryPoint = "LLVMGetNamedGlobal", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetNamedGlobal(LLVMModuleRef @M, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMGetFirstGlobal", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetFirstGlobal(LLVMModuleRef @M);

        [DllImport(libraryPath, EntryPoint = "LLVMGetLastGlobal", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetLastGlobal(LLVMModuleRef @M);

        [DllImport(libraryPath, EntryPoint = "LLVMGetNextGlobal", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetNextGlobal(LLVMValueRef @GlobalVar);

        [DllImport(libraryPath, EntryPoint = "LLVMGetPreviousGlobal", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetPreviousGlobal(LLVMValueRef @GlobalVar);

        [DllImport(libraryPath, EntryPoint = "LLVMDeleteGlobal", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DeleteGlobal(LLVMValueRef @GlobalVar);

        [DllImport(libraryPath, EntryPoint = "LLVMGetInitializer", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetInitializer(LLVMValueRef @GlobalVar);

        [DllImport(libraryPath, EntryPoint = "LLVMSetInitializer", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetInitializer(LLVMValueRef @GlobalVar, LLVMValueRef @ConstantVal);

        [DllImport(libraryPath, EntryPoint = "LLVMIsThreadLocal", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool IsThreadLocal(LLVMValueRef @GlobalVar);

        [DllImport(libraryPath, EntryPoint = "LLVMSetThreadLocal", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetThreadLocal(LLVMValueRef @GlobalVar, LLVMBool @IsThreadLocal);

        [DllImport(libraryPath, EntryPoint = "LLVMIsGlobalConstant", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool IsGlobalConstant(LLVMValueRef @GlobalVar);

        [DllImport(libraryPath, EntryPoint = "LLVMSetGlobalConstant", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetGlobalConstant(LLVMValueRef @GlobalVar, LLVMBool @IsConstant);

        [DllImport(libraryPath, EntryPoint = "LLVMGetThreadLocalMode", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMThreadLocalMode GetThreadLocalMode(LLVMValueRef @GlobalVar);

        [DllImport(libraryPath, EntryPoint = "LLVMSetThreadLocalMode", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetThreadLocalMode(LLVMValueRef @GlobalVar, LLVMThreadLocalMode @Mode);

        [DllImport(libraryPath, EntryPoint = "LLVMIsExternallyInitialized", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool IsExternallyInitialized(LLVMValueRef @GlobalVar);

        [DllImport(libraryPath, EntryPoint = "LLVMSetExternallyInitialized", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetExternallyInitialized(LLVMValueRef @GlobalVar, LLVMBool @IsExtInit);

        [DllImport(libraryPath, EntryPoint = "LLVMAddAlias", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef AddAlias(LLVMModuleRef @M, LLVMTypeRef @Ty, LLVMValueRef @Aliasee, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMDeleteFunction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DeleteFunction(LLVMValueRef @Fn);

        [DllImport(libraryPath, EntryPoint = "LLVMGetIntrinsicID", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint GetIntrinsicID(LLVMValueRef @Fn);

        [DllImport(libraryPath, EntryPoint = "LLVMGetFunctionCallConv", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint GetFunctionCallConv(LLVMValueRef @Fn);

        [DllImport(libraryPath, EntryPoint = "LLVMSetFunctionCallConv", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetFunctionCallConv(LLVMValueRef @Fn, uint @CC);

        [DllImport(libraryPath, EntryPoint = "LLVMGetGC", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetGC(LLVMValueRef @Fn);

        [DllImport(libraryPath, EntryPoint = "LLVMSetGC", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetGC(LLVMValueRef @Fn, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        //[DllImport(libraryPath, EntryPoint = "LLVMAddFunctionAttr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        //internal static extern void AddFunctionAttr(LLVMValueRef @Fn, LLVMAttribute @PA);

        //[DllImport(libraryPath, EntryPoint = "LLVMAddTargetDependentFunctionAttr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        //internal static extern void AddTargetDependentFunctionAttr(LLVMValueRef @Fn, [MarshalAs(UnmanagedType.LPStr)] string @A, [MarshalAs(UnmanagedType.LPStr)] string @V);

        //[DllImport(libraryPath, EntryPoint = "LLVMGetFunctionAttr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        //internal static extern LLVMAttribute GetFunctionAttr(LLVMValueRef @Fn);

        //[DllImport(libraryPath, EntryPoint = "LLVMRemoveFunctionAttr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        //internal static extern void RemoveFunctionAttr(LLVMValueRef @Fn, LLVMAttribute @PA);

        [DllImport(libraryPath, EntryPoint = "LLVMCountParams", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint CountParams(LLVMValueRef @Fn);

        [DllImport(libraryPath, EntryPoint = "LLVMGetParams", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void GetParams(LLVMValueRef @Fn, out LLVMValueRef @Params);

        [DllImport(libraryPath, EntryPoint = "LLVMGetParam", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetParam(LLVMValueRef @Fn, uint @Index);

        [DllImport(libraryPath, EntryPoint = "LLVMGetParamParent", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetParamParent(LLVMValueRef @Inst);

        [DllImport(libraryPath, EntryPoint = "LLVMGetFirstParam", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetFirstParam(LLVMValueRef @Fn);

        [DllImport(libraryPath, EntryPoint = "LLVMGetLastParam", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetLastParam(LLVMValueRef @Fn);

        [DllImport(libraryPath, EntryPoint = "LLVMGetNextParam", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetNextParam(LLVMValueRef @Arg);

        [DllImport(libraryPath, EntryPoint = "LLVMGetPreviousParam", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetPreviousParam(LLVMValueRef @Arg);

        //[DllImport(libraryPath, EntryPoint = "LLVMAddAttribute", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        //internal static extern void AddAttribute(LLVMValueRef @Arg, LLVMAttribute @PA);

        //[DllImport(libraryPath, EntryPoint = "LLVMRemoveAttribute", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        //internal static extern void RemoveAttribute(LLVMValueRef @Arg, LLVMAttribute @PA);

        //[DllImport(libraryPath, EntryPoint = "LLVMGetAttribute", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        //internal static extern LLVMAttribute GetAttribute(LLVMValueRef @Arg);

        [DllImport(libraryPath, EntryPoint = "LLVMSetParamAlignment", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetParamAlignment(LLVMValueRef @Arg, uint @align);

        [DllImport(libraryPath, EntryPoint = "LLVMMDStringInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef MDStringInContext(LLVMContextRef @C, [MarshalAs(UnmanagedType.LPStr)] string @Str, uint @SLen);

        [DllImport(libraryPath, EntryPoint = "LLVMMDString", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef MDString([MarshalAs(UnmanagedType.LPStr)] string @Str, uint @SLen);

        [DllImport(libraryPath, EntryPoint = "LLVMMDNodeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef MDNodeInContext(LLVMContextRef @C, out LLVMValueRef @Vals, uint @Count);

        [DllImport(libraryPath, EntryPoint = "LLVMMDNode", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef MDNode(out LLVMValueRef @Vals, uint @Count);

        [DllImport(libraryPath, EntryPoint = "LLVMGetMDString", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetMDString(LLVMValueRef @V, out uint @Len);

        [DllImport(libraryPath, EntryPoint = "LLVMGetMDNodeNumOperands", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint GetMDNodeNumOperands(LLVMValueRef @V);

        [DllImport(libraryPath, EntryPoint = "LLVMGetMDNodeOperands", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void GetMDNodeOperands(LLVMValueRef @V, out LLVMValueRef @Dest);

        [DllImport(libraryPath, EntryPoint = "LLVMBasicBlockAsValue", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BasicBlockAsValue(LLVMBasicBlockRef @BB);

        [DllImport(libraryPath, EntryPoint = "LLVMValueIsBasicBlock", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool ValueIsBasicBlock(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMValueAsBasicBlock", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBasicBlockRef ValueAsBasicBlock(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMGetBasicBlockParent", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetBasicBlockParent(LLVMBasicBlockRef @BB);

        [DllImport(libraryPath, EntryPoint = "LLVMGetBasicBlockTerminator", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetBasicBlockTerminator(LLVMBasicBlockRef @BB);

        [DllImport(libraryPath, EntryPoint = "LLVMCountBasicBlocks", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint CountBasicBlocks(LLVMValueRef @Fn);

        [DllImport(libraryPath, EntryPoint = "LLVMGetBasicBlocks", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void GetBasicBlocks(LLVMValueRef @Fn, out LLVMBasicBlockRef @BasicBlocks);

        [DllImport(libraryPath, EntryPoint = "LLVMGetFirstBasicBlock", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBasicBlockRef GetFirstBasicBlock(LLVMValueRef @Fn);

        [DllImport(libraryPath, EntryPoint = "LLVMGetLastBasicBlock", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBasicBlockRef GetLastBasicBlock(LLVMValueRef @Fn);

        [DllImport(libraryPath, EntryPoint = "LLVMGetNextBasicBlock", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBasicBlockRef GetNextBasicBlock(LLVMBasicBlockRef @BB);

        [DllImport(libraryPath, EntryPoint = "LLVMGetPreviousBasicBlock", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBasicBlockRef GetPreviousBasicBlock(LLVMBasicBlockRef @BB);

        [DllImport(libraryPath, EntryPoint = "LLVMGetEntryBasicBlock", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBasicBlockRef GetEntryBasicBlock(LLVMValueRef @Fn);

        [DllImport(libraryPath, EntryPoint = "LLVMAppendBasicBlockInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBasicBlockRef AppendBasicBlockInContext(LLVMContextRef @C, LLVMValueRef @Fn, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMAppendBasicBlock", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBasicBlockRef AppendBasicBlock(LLVMValueRef @Fn, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMInsertBasicBlockInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBasicBlockRef InsertBasicBlockInContext(LLVMContextRef @C, LLVMBasicBlockRef @BB, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMInsertBasicBlock", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBasicBlockRef InsertBasicBlock(LLVMBasicBlockRef @InsertBeforeBB, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMDeleteBasicBlock", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DeleteBasicBlock(LLVMBasicBlockRef @BB);

        [DllImport(libraryPath, EntryPoint = "LLVMRemoveBasicBlockFromParent", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void RemoveBasicBlockFromParent(LLVMBasicBlockRef @BB);

        [DllImport(libraryPath, EntryPoint = "LLVMMoveBasicBlockBefore", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void MoveBasicBlockBefore(LLVMBasicBlockRef @BB, LLVMBasicBlockRef @MovePos);

        [DllImport(libraryPath, EntryPoint = "LLVMMoveBasicBlockAfter", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void MoveBasicBlockAfter(LLVMBasicBlockRef @BB, LLVMBasicBlockRef @MovePos);

        [DllImport(libraryPath, EntryPoint = "LLVMGetFirstInstruction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetFirstInstruction(LLVMBasicBlockRef @BB);

        [DllImport(libraryPath, EntryPoint = "LLVMGetLastInstruction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetLastInstruction(LLVMBasicBlockRef @BB);

        [DllImport(libraryPath, EntryPoint = "LLVMHasMetadata", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int HasMetadata(LLVMValueRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMGetMetadata", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetMetadata(LLVMValueRef @Val, uint @KindID);

        [DllImport(libraryPath, EntryPoint = "LLVMSetMetadata", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetMetadata(LLVMValueRef @Val, uint @KindID, LLVMValueRef @Node);

        [DllImport(libraryPath, EntryPoint = "LLVMGetInstructionParent", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBasicBlockRef GetInstructionParent(LLVMValueRef @Inst);

        [DllImport(libraryPath, EntryPoint = "LLVMGetNextInstruction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetNextInstruction(LLVMValueRef @Inst);

        [DllImport(libraryPath, EntryPoint = "LLVMGetPreviousInstruction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetPreviousInstruction(LLVMValueRef @Inst);

        [DllImport(libraryPath, EntryPoint = "LLVMInstructionEraseFromParent", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InstructionEraseFromParent(LLVMValueRef @Inst);

        [DllImport(libraryPath, EntryPoint = "LLVMGetInstructionOpcode", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMOpcode GetInstructionOpcode(LLVMValueRef @Inst);

        [DllImport(libraryPath, EntryPoint = "LLVMGetICmpPredicate", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMIntPredicate GetICmpPredicate(LLVMValueRef @Inst);

        [DllImport(libraryPath, EntryPoint = "LLVMGetFCmpPredicate", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMRealPredicate GetFCmpPredicate(LLVMValueRef @Inst);

        [DllImport(libraryPath, EntryPoint = "LLVMInstructionClone", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef InstructionClone(LLVMValueRef @Inst);

        [DllImport(libraryPath, EntryPoint = "LLVMSetInstructionCallConv", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetInstructionCallConv(LLVMValueRef @Instr, uint @CC);

        [DllImport(libraryPath, EntryPoint = "LLVMGetInstructionCallConv", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint GetInstructionCallConv(LLVMValueRef @Instr);

        //[DllImport(libraryPath, EntryPoint = "LLVMAddInstrAttribute", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        //internal static extern void AddInstrAttribute(LLVMValueRef @Instr, uint @index, LLVMAttribute @param2);

        //[DllImport(libraryPath, EntryPoint = "LLVMRemoveInstrAttribute", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        //internal static extern void RemoveInstrAttribute(LLVMValueRef @Instr, uint @index, LLVMAttribute @param2);

        [DllImport(libraryPath, EntryPoint = "LLVMSetInstrParamAlignment", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetInstrParamAlignment(LLVMValueRef @Instr, uint @index, uint @align);

        [DllImport(libraryPath, EntryPoint = "LLVMIsTailCall", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool IsTailCall(LLVMValueRef @CallInst);

        [DllImport(libraryPath, EntryPoint = "LLVMSetTailCall", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetTailCall(LLVMValueRef @CallInst, LLVMBool @IsTailCall);

        [DllImport(libraryPath, EntryPoint = "LLVMGetNumSuccessors", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint GetNumSuccessors(LLVMValueRef @Term);

        [DllImport(libraryPath, EntryPoint = "LLVMGetSuccessor", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBasicBlockRef GetSuccessor(LLVMValueRef @Term, uint @i);

        [DllImport(libraryPath, EntryPoint = "LLVMSetSuccessor", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetSuccessor(LLVMValueRef @Term, uint @i, LLVMBasicBlockRef @block);

        [DllImport(libraryPath, EntryPoint = "LLVMIsConditional", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool IsConditional(LLVMValueRef @Branch);

        [DllImport(libraryPath, EntryPoint = "LLVMGetCondition", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetCondition(LLVMValueRef @Branch);

        [DllImport(libraryPath, EntryPoint = "LLVMSetCondition", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetCondition(LLVMValueRef @Branch, LLVMValueRef @Cond);

        [DllImport(libraryPath, EntryPoint = "LLVMGetSwitchDefaultDest", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBasicBlockRef GetSwitchDefaultDest(LLVMValueRef @SwitchInstr);

        [DllImport(libraryPath, EntryPoint = "LLVMAddIncoming", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddIncoming(LLVMValueRef @PhiNode, out LLVMValueRef @IncomingValues, out LLVMBasicBlockRef @IncomingBlocks, uint @Count);

        [DllImport(libraryPath, EntryPoint = "LLVMCountIncoming", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint CountIncoming(LLVMValueRef @PhiNode);

        [DllImport(libraryPath, EntryPoint = "LLVMGetIncomingValue", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetIncomingValue(LLVMValueRef @PhiNode, uint @Index);

        [DllImport(libraryPath, EntryPoint = "LLVMGetIncomingBlock", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBasicBlockRef GetIncomingBlock(LLVMValueRef @PhiNode, uint @Index);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateBuilderInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBuilderRef CreateBuilderInContext(LLVMContextRef @C);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateBuilder", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBuilderRef CreateBuilder();

        [DllImport(libraryPath, EntryPoint = "LLVMPositionBuilder", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void PositionBuilder(LLVMBuilderRef @Builder, LLVMBasicBlockRef @Block, LLVMValueRef @Instr);

        [DllImport(libraryPath, EntryPoint = "LLVMPositionBuilderBefore", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void PositionBuilderBefore(LLVMBuilderRef @Builder, LLVMValueRef @Instr);

        [DllImport(libraryPath, EntryPoint = "LLVMPositionBuilderAtEnd", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void PositionBuilderAtEnd(LLVMBuilderRef @Builder, LLVMBasicBlockRef @Block);

        [DllImport(libraryPath, EntryPoint = "LLVMGetInsertBlock", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBasicBlockRef GetInsertBlock(LLVMBuilderRef @Builder);

        [DllImport(libraryPath, EntryPoint = "LLVMClearInsertionPosition", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void ClearInsertionPosition(LLVMBuilderRef @Builder);

        [DllImport(libraryPath, EntryPoint = "LLVMInsertIntoBuilder", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InsertIntoBuilder(LLVMBuilderRef @Builder, LLVMValueRef @Instr);

        [DllImport(libraryPath, EntryPoint = "LLVMInsertIntoBuilderWithName", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InsertIntoBuilderWithName(LLVMBuilderRef @Builder, LLVMValueRef @Instr, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMDisposeBuilder", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DisposeBuilder(LLVMBuilderRef @Builder);

        [DllImport(libraryPath, EntryPoint = "LLVMSetCurrentDebugLocation", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetCurrentDebugLocation(LLVMBuilderRef @Builder, LLVMValueRef @L);

        [DllImport(libraryPath, EntryPoint = "LLVMGetCurrentDebugLocation", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef GetCurrentDebugLocation(LLVMBuilderRef @Builder);

        [DllImport(libraryPath, EntryPoint = "LLVMSetInstDebugLocation", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetInstDebugLocation(LLVMBuilderRef @Builder, LLVMValueRef @Inst);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildRetVoid", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildRetVoid(LLVMBuilderRef @param0);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildRet", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildRet(LLVMBuilderRef @param0, LLVMValueRef @V);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildAggregateRet", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildAggregateRet(LLVMBuilderRef @param0, out LLVMValueRef @RetVals, uint @N);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildBr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildBr(LLVMBuilderRef @param0, LLVMBasicBlockRef @Dest);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildCondBr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildCondBr(LLVMBuilderRef @param0, LLVMValueRef @If, LLVMBasicBlockRef @Then, LLVMBasicBlockRef @Else);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildSwitch", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildSwitch(LLVMBuilderRef @param0, LLVMValueRef @V, LLVMBasicBlockRef @Else, uint @NumCases);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildIndirectBr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildIndirectBr(LLVMBuilderRef @B, LLVMValueRef @Addr, uint @NumDests);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildInvoke", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildInvoke(LLVMBuilderRef @param0, LLVMValueRef @Fn, out LLVMValueRef @Args, uint @NumArgs, LLVMBasicBlockRef @Then, LLVMBasicBlockRef @Catch, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildLandingPad", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildLandingPad(LLVMBuilderRef @B, LLVMTypeRef @Ty, LLVMValueRef @PersFn, uint @NumClauses, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildResume", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildResume(LLVMBuilderRef @B, LLVMValueRef @Exn);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildUnreachable", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildUnreachable(LLVMBuilderRef @param0);

        [DllImport(libraryPath, EntryPoint = "LLVMAddCase", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddCase(LLVMValueRef @Switch, LLVMValueRef @OnVal, LLVMBasicBlockRef @Dest);

        [DllImport(libraryPath, EntryPoint = "LLVMAddDestination", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddDestination(LLVMValueRef @IndirectBr, LLVMBasicBlockRef @Dest);

        [DllImport(libraryPath, EntryPoint = "LLVMAddClause", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddClause(LLVMValueRef @LandingPad, LLVMValueRef @ClauseVal);

        [DllImport(libraryPath, EntryPoint = "LLVMSetCleanup", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetCleanup(LLVMValueRef @LandingPad, LLVMBool @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildAdd", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildAdd(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildNSWAdd", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildNSWAdd(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildNUWAdd", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildNUWAdd(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildFAdd", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildFAdd(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildSub", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildSub(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildNSWSub", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildNSWSub(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildNUWSub", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildNUWSub(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildFSub", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildFSub(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildMul", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildMul(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildNSWMul", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildNSWMul(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildNUWMul", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildNUWMul(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildFMul", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildFMul(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildUDiv", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildUDiv(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildSDiv", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildSDiv(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildExactSDiv", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildExactSDiv(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildFDiv", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildFDiv(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildURem", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildURem(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildSRem", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildSRem(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildFRem", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildFRem(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildShl", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildShl(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildLShr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildLShr(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildAShr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildAShr(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildAnd", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildAnd(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildOr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildOr(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildXor", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildXor(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildBinOp", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildBinOp(LLVMBuilderRef @B, LLVMOpcode @Op, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildNeg", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildNeg(LLVMBuilderRef @param0, LLVMValueRef @V, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildNSWNeg", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildNSWNeg(LLVMBuilderRef @B, LLVMValueRef @V, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildNUWNeg", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildNUWNeg(LLVMBuilderRef @B, LLVMValueRef @V, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildFNeg", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildFNeg(LLVMBuilderRef @param0, LLVMValueRef @V, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildNot", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildNot(LLVMBuilderRef @param0, LLVMValueRef @V, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildMalloc", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildMalloc(LLVMBuilderRef @param0, LLVMTypeRef @Ty, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildArrayMalloc", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildArrayMalloc(LLVMBuilderRef @param0, LLVMTypeRef @Ty, LLVMValueRef @Val, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildAlloca", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildAlloca(LLVMBuilderRef @param0, LLVMTypeRef @Ty, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildArrayAlloca", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildArrayAlloca(LLVMBuilderRef @param0, LLVMTypeRef @Ty, LLVMValueRef @Val, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildFree", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildFree(LLVMBuilderRef @param0, LLVMValueRef @PointerVal);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildLoad", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildLoad(LLVMBuilderRef @param0, LLVMValueRef @PointerVal, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildStore", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildStore(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMValueRef @Ptr);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildGEP", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildGEP(LLVMBuilderRef @B, LLVMValueRef @Pointer, out LLVMValueRef @Indices, uint @NumIndices, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildInBoundsGEP", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildInBoundsGEP(LLVMBuilderRef @B, LLVMValueRef @Pointer, out LLVMValueRef @Indices, uint @NumIndices, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildStructGEP", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildStructGEP(LLVMBuilderRef @B, LLVMValueRef @Pointer, uint @Idx, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildGlobalString", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildGlobalString(LLVMBuilderRef @B, [MarshalAs(UnmanagedType.LPStr)] string @Str, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildGlobalStringPtr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildGlobalStringPtr(LLVMBuilderRef @B, [MarshalAs(UnmanagedType.LPStr)] string @Str, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMGetVolatile", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool GetVolatile(LLVMValueRef @MemoryAccessInst);

        [DllImport(libraryPath, EntryPoint = "LLVMSetVolatile", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetVolatile(LLVMValueRef @MemoryAccessInst, LLVMBool @IsVolatile);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildTrunc", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildTrunc(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildZExt", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildZExt(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildSExt", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildSExt(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildFPToUI", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildFPToUI(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildFPToSI", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildFPToSI(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildUIToFP", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildUIToFP(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildSIToFP", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildSIToFP(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildFPTrunc", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildFPTrunc(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildFPExt", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildFPExt(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildPtrToInt", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildPtrToInt(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildIntToPtr", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildIntToPtr(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildBitCast", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildBitCast(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildAddrSpaceCast", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildAddrSpaceCast(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildZExtOrBitCast", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildZExtOrBitCast(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildSExtOrBitCast", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildSExtOrBitCast(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildTruncOrBitCast", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildTruncOrBitCast(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildCast", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildCast(LLVMBuilderRef @B, LLVMOpcode @Op, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildPointerCast", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildPointerCast(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildIntCast", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildIntCast(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildFPCast", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildFPCast(LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildICmp", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildICmp(LLVMBuilderRef @param0, LLVMIntPredicate @Op, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildFCmp", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildFCmp(LLVMBuilderRef @param0, LLVMRealPredicate @Op, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildPhi", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildPhi(LLVMBuilderRef @param0, LLVMTypeRef @Ty, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildCall", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildCall(LLVMBuilderRef @param0, LLVMValueRef @Fn, out LLVMValueRef @Args, uint @NumArgs, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildSelect", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildSelect(LLVMBuilderRef @param0, LLVMValueRef @If, LLVMValueRef @Then, LLVMValueRef @Else, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildVAArg", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildVAArg(LLVMBuilderRef @param0, LLVMValueRef @List, LLVMTypeRef @Ty, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildExtractElement", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildExtractElement(LLVMBuilderRef @param0, LLVMValueRef @VecVal, LLVMValueRef @Index, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildInsertElement", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildInsertElement(LLVMBuilderRef @param0, LLVMValueRef @VecVal, LLVMValueRef @EltVal, LLVMValueRef @Index, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildShuffleVector", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildShuffleVector(LLVMBuilderRef @param0, LLVMValueRef @V1, LLVMValueRef @V2, LLVMValueRef @Mask, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildExtractValue", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildExtractValue(LLVMBuilderRef @param0, LLVMValueRef @AggVal, uint @Index, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildInsertValue", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildInsertValue(LLVMBuilderRef @param0, LLVMValueRef @AggVal, LLVMValueRef @EltVal, uint @Index, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildIsNull", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildIsNull(LLVMBuilderRef @param0, LLVMValueRef @Val, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildIsNotNull", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildIsNotNull(LLVMBuilderRef @param0, LLVMValueRef @Val, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildPtrDiff", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildPtrDiff(LLVMBuilderRef @param0, LLVMValueRef @LHS, LLVMValueRef @RHS, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildFence", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildFence(LLVMBuilderRef @B, LLVMAtomicOrdering @ordering, LLVMBool @singleThread, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMBuildAtomicRMW", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMValueRef BuildAtomicRMW(LLVMBuilderRef @B, LLVMAtomicRMWBinOp @op, LLVMValueRef @PTR, LLVMValueRef @Val, LLVMAtomicOrdering @ordering, LLVMBool @singleThread);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateModuleProviderForExistingModule", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMModuleProviderRef CreateModuleProviderForExistingModule(LLVMModuleRef @M);

        [DllImport(libraryPath, EntryPoint = "LLVMDisposeModuleProvider", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DisposeModuleProvider(LLVMModuleProviderRef @M);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateMemoryBufferWithContentsOfFile", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool CreateMemoryBufferWithContentsOfFile([MarshalAs(UnmanagedType.LPStr)] string @Path, out LLVMMemoryBufferRef @OutMemBuf, out IntPtr @OutMessage);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateMemoryBufferWithSTDIN", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool CreateMemoryBufferWithSTDIN(out LLVMMemoryBufferRef @OutMemBuf, out IntPtr @OutMessage);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateMemoryBufferWithMemoryRange", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMMemoryBufferRef CreateMemoryBufferWithMemoryRange([MarshalAs(UnmanagedType.LPStr)] string @InputData, int @InputDataLength, [MarshalAs(UnmanagedType.LPStr)] string @BufferName, LLVMBool @RequiresNullTerminator);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateMemoryBufferWithMemoryRangeCopy", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMMemoryBufferRef CreateMemoryBufferWithMemoryRangeCopy([MarshalAs(UnmanagedType.LPStr)] string @InputData, int @InputDataLength, [MarshalAs(UnmanagedType.LPStr)] string @BufferName);

        [DllImport(libraryPath, EntryPoint = "LLVMGetBufferStart", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetBufferStart(LLVMMemoryBufferRef @MemBuf);

        [DllImport(libraryPath, EntryPoint = "LLVMGetBufferSize", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int GetBufferSize(LLVMMemoryBufferRef @MemBuf);

        [DllImport(libraryPath, EntryPoint = "LLVMDisposeMemoryBuffer", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DisposeMemoryBuffer(LLVMMemoryBufferRef @MemBuf);

        [DllImport(libraryPath, EntryPoint = "LLVMGetGlobalPassRegistry", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMPassRegistryRef GetGlobalPassRegistry();

        [DllImport(libraryPath, EntryPoint = "LLVMCreatePassManager", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMPassManagerRef CreatePassManager();

        [DllImport(libraryPath, EntryPoint = "LLVMCreateFunctionPassManagerForModule", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMPassManagerRef CreateFunctionPassManagerForModule(LLVMModuleRef @M);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateFunctionPassManager", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMPassManagerRef CreateFunctionPassManager(LLVMModuleProviderRef @MP);

        [DllImport(libraryPath, EntryPoint = "LLVMRunPassManager", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool RunPassManager(LLVMPassManagerRef @PM, LLVMModuleRef @M);

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeFunctionPassManager", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool InitializeFunctionPassManager(LLVMPassManagerRef @FPM);

        [DllImport(libraryPath, EntryPoint = "LLVMRunFunctionPassManager", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool RunFunctionPassManager(LLVMPassManagerRef @FPM, LLVMValueRef @F);

        [DllImport(libraryPath, EntryPoint = "LLVMFinalizeFunctionPassManager", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool FinalizeFunctionPassManager(LLVMPassManagerRef @FPM);

        [DllImport(libraryPath, EntryPoint = "LLVMDisposePassManager", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DisposePassManager(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMStartMultithreaded", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool StartMultithreaded();

        [DllImport(libraryPath, EntryPoint = "LLVMStopMultithreaded", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void StopMultithreaded();

        [DllImport(libraryPath, EntryPoint = "LLVMIsMultithreaded", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool IsMultithreaded();

        [DllImport(libraryPath, EntryPoint = "LLVMVerifyModule", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool VerifyModule(LLVMModuleRef @M, LLVMVerifierFailureAction @Action, out IntPtr @OutMessage);

        [DllImport(libraryPath, EntryPoint = "LLVMVerifyFunction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool VerifyFunction(LLVMValueRef @Fn, LLVMVerifierFailureAction @Action);

        [DllImport(libraryPath, EntryPoint = "LLVMViewFunctionCFG", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void ViewFunctionCFG(LLVMValueRef @Fn);

        [DllImport(libraryPath, EntryPoint = "LLVMViewFunctionCFGOnly", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void ViewFunctionCFGOnly(LLVMValueRef @Fn);

        [DllImport(libraryPath, EntryPoint = "LLVMParseBitcode", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool ParseBitcode(LLVMMemoryBufferRef @MemBuf, out LLVMModuleRef @OutModule, out IntPtr @OutMessage);

        [DllImport(libraryPath, EntryPoint = "LLVMParseBitcodeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool ParseBitcodeInContext(LLVMContextRef @ContextRef, LLVMMemoryBufferRef @MemBuf, out LLVMModuleRef @OutModule, out IntPtr @OutMessage);

        [DllImport(libraryPath, EntryPoint = "LLVMGetBitcodeModuleInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool GetBitcodeModuleInContext(LLVMContextRef @ContextRef, LLVMMemoryBufferRef @MemBuf, out LLVMModuleRef @OutM, out IntPtr @OutMessage);

        [DllImport(libraryPath, EntryPoint = "LLVMGetBitcodeModule", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool GetBitcodeModule(LLVMMemoryBufferRef @MemBuf, out LLVMModuleRef @OutM, out IntPtr @OutMessage);

        [DllImport(libraryPath, EntryPoint = "LLVMGetBitcodeModuleProviderInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool GetBitcodeModuleProviderInContext(LLVMContextRef @ContextRef, LLVMMemoryBufferRef @MemBuf, out LLVMModuleProviderRef @OutMP, out IntPtr @OutMessage);

        [DllImport(libraryPath, EntryPoint = "LLVMGetBitcodeModuleProvider", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool GetBitcodeModuleProvider(LLVMMemoryBufferRef @MemBuf, out LLVMModuleProviderRef @OutMP, out IntPtr @OutMessage);

        [DllImport(libraryPath, EntryPoint = "LLVMWriteBitcodeToFile", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int WriteBitcodeToFile(LLVMModuleRef @M, [MarshalAs(UnmanagedType.LPStr)] string @Path);

        [DllImport(libraryPath, EntryPoint = "LLVMWriteBitcodeToFD", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int WriteBitcodeToFD(LLVMModuleRef @M, int @FD, int @ShouldClose, int @Unbuffered);

        [DllImport(libraryPath, EntryPoint = "LLVMWriteBitcodeToFileHandle", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int WriteBitcodeToFileHandle(LLVMModuleRef @M, int @Handle);

        [DllImport(libraryPath, EntryPoint = "LLVMWriteBitcodeToMemoryBuffer", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMMemoryBufferRef WriteBitcodeToMemoryBuffer(LLVMModuleRef @M);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateDisasm", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMDisasmContextRef CreateDisasm([MarshalAs(UnmanagedType.LPStr)] string @TripleName, IntPtr @DisInfo, int @TagType, LLVMOpInfoCallback @GetOpInfo, LLVMSymbolLookupCallback @SymbolLookUp);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateDisasmCPU", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMDisasmContextRef CreateDisasmCPU([MarshalAs(UnmanagedType.LPStr)] string @Triple, [MarshalAs(UnmanagedType.LPStr)] string @CPU, IntPtr @DisInfo, int @TagType, LLVMOpInfoCallback @GetOpInfo, LLVMSymbolLookupCallback @SymbolLookUp);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateDisasmCPUFeatures", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMDisasmContextRef CreateDisasmCPUFeatures([MarshalAs(UnmanagedType.LPStr)] string @Triple, [MarshalAs(UnmanagedType.LPStr)] string @CPU, [MarshalAs(UnmanagedType.LPStr)] string @Features, IntPtr @DisInfo, int @TagType, LLVMOpInfoCallback @GetOpInfo, LLVMSymbolLookupCallback @SymbolLookUp);

        [DllImport(libraryPath, EntryPoint = "LLVMSetDisasmOptions", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int SetDisasmOptions(LLVMDisasmContextRef @DC, int @Options);

        [DllImport(libraryPath, EntryPoint = "LLVMDisasmDispose", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DisasmDispose(LLVMDisasmContextRef @DC);

        [DllImport(libraryPath, EntryPoint = "LLVMDisasmInstruction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int DisasmInstruction(LLVMDisasmContextRef @DC, out char @Bytes, int @BytesSize, int @PC, IntPtr @OutString, int @OutStringSize);

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeAMDGPUTargetInfo", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeAMDGPUTargetInfo();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeSystemZTargetInfo", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeSystemZTargetInfo();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeHexagonTargetInfo", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeHexagonTargetInfo();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeNVPTXTargetInfo", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeNVPTXTargetInfo();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeCppBackendTargetInfo", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeCppBackendTargetInfo();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeMSP430TargetInfo", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeMSP430TargetInfo();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeXCoreTargetInfo", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeXCoreTargetInfo();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeMipsTargetInfo", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeMipsTargetInfo();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeAArch64TargetInfo", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeAArch64TargetInfo();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeARMTargetInfo", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeARMTargetInfo();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializePowerPCTargetInfo", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializePowerPCTargetInfo();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeSparcTargetInfo", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeSparcTargetInfo();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeX86TargetInfo", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeX86TargetInfo();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeAMDGPUTarget", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeAMDGPUTarget();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeSystemZTarget", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeSystemZTarget();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeHexagonTarget", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeHexagonTarget();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeNVPTXTarget", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeNVPTXTarget();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeCppBackendTarget", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeCppBackendTarget();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeMSP430Target", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeMSP430Target();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeXCoreTarget", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeXCoreTarget();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeMipsTarget", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeMipsTarget();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeAArch64Target", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeAArch64Target();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeARMTarget", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeARMTarget();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializePowerPCTarget", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializePowerPCTarget();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeSparcTarget", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeSparcTarget();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeX86Target", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeX86Target();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeAMDGPUTargetMC", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeAMDGPUTargetMC();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeSystemZTargetMC", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeSystemZTargetMC();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeHexagonTargetMC", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeHexagonTargetMC();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeNVPTXTargetMC", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeNVPTXTargetMC();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeCppBackendTargetMC", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeCppBackendTargetMC();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeMSP430TargetMC", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeMSP430TargetMC();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeXCoreTargetMC", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeXCoreTargetMC();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeMipsTargetMC", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeMipsTargetMC();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeAArch64TargetMC", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeAArch64TargetMC();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeARMTargetMC", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeARMTargetMC();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializePowerPCTargetMC", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializePowerPCTargetMC();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeSparcTargetMC", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeSparcTargetMC();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeX86TargetMC", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeX86TargetMC();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeAMDGPUAsmPrinter", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeAMDGPUAsmPrinter();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeSystemZAsmPrinter", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeSystemZAsmPrinter();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeHexagonAsmPrinter", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeHexagonAsmPrinter();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeNVPTXAsmPrinter", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeNVPTXAsmPrinter();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeMSP430AsmPrinter", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeMSP430AsmPrinter();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeXCoreAsmPrinter", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeXCoreAsmPrinter();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeMipsAsmPrinter", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeMipsAsmPrinter();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeAArch64AsmPrinter", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeAArch64AsmPrinter();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeARMAsmPrinter", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeARMAsmPrinter();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializePowerPCAsmPrinter", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializePowerPCAsmPrinter();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeSparcAsmPrinter", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeSparcAsmPrinter();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeX86AsmPrinter", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeX86AsmPrinter();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeAMDGPUAsmParser", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeAMDGPUAsmParser();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeSystemZAsmParser", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeSystemZAsmParser();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeMipsAsmParser", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeMipsAsmParser();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeAArch64AsmParser", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeAArch64AsmParser();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeARMAsmParser", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeARMAsmParser();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializePowerPCAsmParser", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializePowerPCAsmParser();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeSparcAsmParser", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeSparcAsmParser();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeX86AsmParser", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeX86AsmParser();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeSystemZDisassembler", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeSystemZDisassembler();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeHexagonDisassembler", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeHexagonDisassembler();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeXCoreDisassembler", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeXCoreDisassembler();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeMipsDisassembler", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeMipsDisassembler();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeAArch64Disassembler", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeAArch64Disassembler();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeARMDisassembler", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeARMDisassembler();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializePowerPCDisassembler", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializePowerPCDisassembler();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeSparcDisassembler", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeSparcDisassembler();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeX86Disassembler", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeX86Disassembler();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeAllTargetInfos", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeAllTargetInfos();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeAllTargets", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeAllTargets();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeAllTargetMCs", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeAllTargetMCs();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeAllAsmPrinters", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeAllAsmPrinters();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeAllAsmParsers", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeAllAsmParsers();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeAllDisassemblers", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeAllDisassemblers();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeNativeTarget", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool InitializeNativeTarget();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeNativeAsmParser", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool InitializeNativeAsmParser();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeNativeAsmPrinter", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool InitializeNativeAsmPrinter();

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeNativeDisassembler", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool InitializeNativeDisassembler();

        [DllImport(libraryPath, EntryPoint = "LLVMCreateTargetData", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTargetDataRef CreateTargetData([MarshalAs(UnmanagedType.LPStr)] string @StringRep);

        [DllImport(libraryPath, EntryPoint = "LLVMAddTargetData", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddTargetData(LLVMTargetDataRef @TD, LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddTargetLibraryInfo", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddTargetLibraryInfo(LLVMTargetLibraryInfoRef @TLI, LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMCopyStringRepOfTargetData", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr CopyStringRepOfTargetData(LLVMTargetDataRef @TD);

        [DllImport(libraryPath, EntryPoint = "LLVMByteOrder", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMByteOrdering ByteOrder(LLVMTargetDataRef @TD);

        [DllImport(libraryPath, EntryPoint = "LLVMPointerSize", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint PointerSize(LLVMTargetDataRef @TD);

        [DllImport(libraryPath, EntryPoint = "LLVMPointerSizeForAS", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint PointerSizeForAS(LLVMTargetDataRef @TD, uint @AS);

        [DllImport(libraryPath, EntryPoint = "LLVMIntPtrType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef IntPtrType(LLVMTargetDataRef @TD);

        [DllImport(libraryPath, EntryPoint = "LLVMIntPtrTypeForAS", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef IntPtrTypeForAS(LLVMTargetDataRef @TD, uint @AS);

        [DllImport(libraryPath, EntryPoint = "LLVMIntPtrTypeInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef IntPtrTypeInContext(LLVMContextRef @C, LLVMTargetDataRef @TD);

        [DllImport(libraryPath, EntryPoint = "LLVMIntPtrTypeForASInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTypeRef IntPtrTypeForASInContext(LLVMContextRef @C, LLVMTargetDataRef @TD, uint @AS);

        [DllImport(libraryPath, EntryPoint = "LLVMSizeOfTypeInBits", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern ulong SizeOfTypeInBits(LLVMTargetDataRef @TD, LLVMTypeRef @Ty);

        [DllImport(libraryPath, EntryPoint = "LLVMStoreSizeOfType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern ulong StoreSizeOfType(LLVMTargetDataRef @TD, LLVMTypeRef @Ty);

        [DllImport(libraryPath, EntryPoint = "LLVMABISizeOfType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern ulong ABISizeOfType(LLVMTargetDataRef @TD, LLVMTypeRef @Ty);

        [DllImport(libraryPath, EntryPoint = "LLVMABIAlignmentOfType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint ABIAlignmentOfType(LLVMTargetDataRef @TD, LLVMTypeRef @Ty);

        [DllImport(libraryPath, EntryPoint = "LLVMCallFrameAlignmentOfType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint CallFrameAlignmentOfType(LLVMTargetDataRef @TD, LLVMTypeRef @Ty);

        [DllImport(libraryPath, EntryPoint = "LLVMPreferredAlignmentOfType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint PreferredAlignmentOfType(LLVMTargetDataRef @TD, LLVMTypeRef @Ty);

        [DllImport(libraryPath, EntryPoint = "LLVMPreferredAlignmentOfGlobal", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint PreferredAlignmentOfGlobal(LLVMTargetDataRef @TD, LLVMValueRef @GlobalVar);

        [DllImport(libraryPath, EntryPoint = "LLVMElementAtOffset", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint ElementAtOffset(LLVMTargetDataRef @TD, LLVMTypeRef @StructTy, ulong @Offset);

        [DllImport(libraryPath, EntryPoint = "LLVMOffsetOfElement", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern ulong OffsetOfElement(LLVMTargetDataRef @TD, LLVMTypeRef @StructTy, uint @Element);

        [DllImport(libraryPath, EntryPoint = "LLVMDisposeTargetData", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DisposeTargetData(LLVMTargetDataRef @TD);

        [DllImport(libraryPath, EntryPoint = "LLVMGetFirstTarget", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTargetRef GetFirstTarget();

        [DllImport(libraryPath, EntryPoint = "LLVMGetNextTarget", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTargetRef GetNextTarget(LLVMTargetRef @T);

        [DllImport(libraryPath, EntryPoint = "LLVMGetTargetFromName", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTargetRef GetTargetFromName([MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMGetTargetFromTriple", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool GetTargetFromTriple([MarshalAs(UnmanagedType.LPStr)] string @Triple, out LLVMTargetRef @T, out IntPtr @ErrorMessage);

        [DllImport(libraryPath, EntryPoint = "LLVMGetTargetName", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetTargetName(LLVMTargetRef @T);

        [DllImport(libraryPath, EntryPoint = "LLVMGetTargetDescription", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetTargetDescription(LLVMTargetRef @T);

        [DllImport(libraryPath, EntryPoint = "LLVMTargetHasJIT", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool TargetHasJIT(LLVMTargetRef @T);

        [DllImport(libraryPath, EntryPoint = "LLVMTargetHasTargetMachine", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool TargetHasTargetMachine(LLVMTargetRef @T);

        [DllImport(libraryPath, EntryPoint = "LLVMTargetHasAsmBackend", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool TargetHasAsmBackend(LLVMTargetRef @T);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateTargetMachine", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTargetMachineRef CreateTargetMachine(LLVMTargetRef @T, [MarshalAs(UnmanagedType.LPStr)] string @Triple, [MarshalAs(UnmanagedType.LPStr)] string @CPU, [MarshalAs(UnmanagedType.LPStr)] string @Features, LLVMCodeGenOptLevel @Level, LLVMRelocMode @Reloc, LLVMCodeModel @CodeModel);

        [DllImport(libraryPath, EntryPoint = "LLVMDisposeTargetMachine", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DisposeTargetMachine(LLVMTargetMachineRef @T);

        [DllImport(libraryPath, EntryPoint = "LLVMGetTargetMachineTarget", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTargetRef GetTargetMachineTarget(LLVMTargetMachineRef @T);

        [DllImport(libraryPath, EntryPoint = "LLVMGetTargetMachineTriple", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetTargetMachineTriple(LLVMTargetMachineRef @T);

        [DllImport(libraryPath, EntryPoint = "LLVMGetTargetMachineCPU", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetTargetMachineCPU(LLVMTargetMachineRef @T);

        [DllImport(libraryPath, EntryPoint = "LLVMGetTargetMachineFeatureString", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetTargetMachineFeatureString(LLVMTargetMachineRef @T);

        [DllImport(libraryPath, EntryPoint = "LLVMGetTargetMachineData", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTargetDataRef GetTargetMachineData(LLVMTargetMachineRef @T);

        [DllImport(libraryPath, EntryPoint = "LLVMSetTargetMachineAsmVerbosity", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void SetTargetMachineAsmVerbosity(LLVMTargetMachineRef @T, LLVMBool @VerboseAsm);

        [DllImport(libraryPath, EntryPoint = "LLVMTargetMachineEmitToFile", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool TargetMachineEmitToFile(LLVMTargetMachineRef @T, LLVMModuleRef @M, [MarshalAs(UnmanagedType.LPStr)] string @Filename, LLVMCodeGenFileType @codegen, out IntPtr @ErrorMessage);

        [DllImport(libraryPath, EntryPoint = "LLVMTargetMachineEmitToMemoryBuffer", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool TargetMachineEmitToMemoryBuffer(LLVMTargetMachineRef @T, LLVMModuleRef @M, LLVMCodeGenFileType @codegen, out IntPtr @ErrorMessage, out LLVMMemoryBufferRef @OutMemBuf);

        [DllImport(libraryPath, EntryPoint = "LLVMGetDefaultTargetTriple", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetDefaultTargetTriple();

        [DllImport(libraryPath, EntryPoint = "LLVMAddAnalysisPasses", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddAnalysisPasses(LLVMTargetMachineRef @T, LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMLinkInMCJIT", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void LinkInMCJIT();

        [DllImport(libraryPath, EntryPoint = "LLVMLinkInInterpreter", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void LinkInInterpreter();

        [DllImport(libraryPath, EntryPoint = "LLVMCreateGenericValueOfInt", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMGenericValueRef CreateGenericValueOfInt(LLVMTypeRef @Ty, ulong @N, LLVMBool @IsSigned);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateGenericValueOfPointer", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMGenericValueRef CreateGenericValueOfPointer(IntPtr @P);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateGenericValueOfFloat", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMGenericValueRef CreateGenericValueOfFloat(LLVMTypeRef @Ty, double @N);

        [DllImport(libraryPath, EntryPoint = "LLVMGenericValueIntWidth", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint GenericValueIntWidth(LLVMGenericValueRef @GenValRef);

        [DllImport(libraryPath, EntryPoint = "LLVMGenericValueToInt", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern ulong GenericValueToInt(LLVMGenericValueRef @GenVal, LLVMBool @IsSigned);

        [DllImport(libraryPath, EntryPoint = "LLVMGenericValueToPointer", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GenericValueToPointer(LLVMGenericValueRef @GenVal);

        [DllImport(libraryPath, EntryPoint = "LLVMGenericValueToFloat", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern double GenericValueToFloat(LLVMTypeRef @TyRef, LLVMGenericValueRef @GenVal);

        [DllImport(libraryPath, EntryPoint = "LLVMDisposeGenericValue", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DisposeGenericValue(LLVMGenericValueRef @GenVal);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateExecutionEngineForModule", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool CreateExecutionEngineForModule(out LLVMExecutionEngineRef @OutEE, LLVMModuleRef @M, out IntPtr @OutError);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateInterpreterForModule", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool CreateInterpreterForModule(out LLVMExecutionEngineRef @OutInterp, LLVMModuleRef @M, out IntPtr @OutError);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateJITCompilerForModule", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool CreateJITCompilerForModule(out LLVMExecutionEngineRef @OutJIT, LLVMModuleRef @M, uint @OptLevel, out IntPtr @OutError);

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeMCJITCompilerOptions", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeMCJITCompilerOptions(out LLVMMCJITCompilerOptions @Options, int @SizeOfOptions);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateMCJITCompilerForModule", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool CreateMCJITCompilerForModule(out LLVMExecutionEngineRef @OutJIT, LLVMModuleRef @M, out LLVMMCJITCompilerOptions @Options, int @SizeOfOptions, out IntPtr @OutError);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateExecutionEngine", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool CreateExecutionEngine(out LLVMExecutionEngineRef @OutEE, LLVMModuleProviderRef @MP, out IntPtr @OutError);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateInterpreter", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool CreateInterpreter(out LLVMExecutionEngineRef @OutInterp, LLVMModuleProviderRef @MP, out IntPtr @OutError);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateJITCompiler", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool CreateJITCompiler(out LLVMExecutionEngineRef @OutJIT, LLVMModuleProviderRef @MP, uint @OptLevel, out IntPtr @OutError);

        [DllImport(libraryPath, EntryPoint = "LLVMDisposeExecutionEngine", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DisposeExecutionEngine(LLVMExecutionEngineRef @EE);

        [DllImport(libraryPath, EntryPoint = "LLVMRunStaticConstructors", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void RunStaticConstructors(LLVMExecutionEngineRef @EE);

        [DllImport(libraryPath, EntryPoint = "LLVMRunStaticDestructors", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void RunStaticDestructors(LLVMExecutionEngineRef @EE);

        [DllImport(libraryPath, EntryPoint = "LLVMRunFunctionAsMain", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int RunFunctionAsMain(LLVMExecutionEngineRef @EE, LLVMValueRef @F, uint @ArgC, string[] @ArgV, string[] @EnvP);

        [DllImport(libraryPath, EntryPoint = "LLVMRunFunction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMGenericValueRef RunFunction(LLVMExecutionEngineRef @EE, LLVMValueRef @F, uint @NumArgs, out LLVMGenericValueRef @Args);

        [DllImport(libraryPath, EntryPoint = "LLVMFreeMachineCodeForFunction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void FreeMachineCodeForFunction(LLVMExecutionEngineRef @EE, LLVMValueRef @F);

        [DllImport(libraryPath, EntryPoint = "LLVMAddModule", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddModule(LLVMExecutionEngineRef @EE, LLVMModuleRef @M);

        [DllImport(libraryPath, EntryPoint = "LLVMAddModuleProvider", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddModuleProvider(LLVMExecutionEngineRef @EE, LLVMModuleProviderRef @MP);

        [DllImport(libraryPath, EntryPoint = "LLVMRemoveModule", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool RemoveModule(LLVMExecutionEngineRef @EE, LLVMModuleRef @M, out LLVMModuleRef @OutMod, out IntPtr @OutError);

        [DllImport(libraryPath, EntryPoint = "LLVMRemoveModuleProvider", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool RemoveModuleProvider(LLVMExecutionEngineRef @EE, LLVMModuleProviderRef @MP, out LLVMModuleRef @OutMod, out IntPtr @OutError);

        [DllImport(libraryPath, EntryPoint = "LLVMFindFunction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool FindFunction(LLVMExecutionEngineRef @EE, [MarshalAs(UnmanagedType.LPStr)] string @Name, out LLVMValueRef @OutFn);

        [DllImport(libraryPath, EntryPoint = "LLVMRecompileAndRelinkFunction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr RecompileAndRelinkFunction(LLVMExecutionEngineRef @EE, LLVMValueRef @Fn);

        [DllImport(libraryPath, EntryPoint = "LLVMGetExecutionEngineTargetData", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTargetDataRef GetExecutionEngineTargetData(LLVMExecutionEngineRef @EE);

        [DllImport(libraryPath, EntryPoint = "LLVMGetExecutionEngineTargetMachine", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMTargetMachineRef GetExecutionEngineTargetMachine(LLVMExecutionEngineRef @EE);

        [DllImport(libraryPath, EntryPoint = "LLVMAddGlobalMapping", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddGlobalMapping(LLVMExecutionEngineRef @EE, LLVMValueRef @Global, IntPtr @Addr);

        [DllImport(libraryPath, EntryPoint = "LLVMGetPointerToGlobal", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetPointerToGlobal(LLVMExecutionEngineRef @EE, LLVMValueRef @Global);

        [DllImport(libraryPath, EntryPoint = "LLVMGetGlobalValueAddress", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int GetGlobalValueAddress(LLVMExecutionEngineRef @EE, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMGetFunctionAddress", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int GetFunctionAddress(LLVMExecutionEngineRef @EE, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMCreateSimpleMCJITMemoryManager", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMMCJITMemoryManagerRef CreateSimpleMCJITMemoryManager(IntPtr @Opaque, LLVMMemoryManagerAllocateCodeSectionCallback @AllocateCodeSection, LLVMMemoryManagerAllocateDataSectionCallback @AllocateDataSection, LLVMMemoryManagerFinalizeMemoryCallback @FinalizeMemory, LLVMMemoryManagerDestroyCallback @Destroy);

        [DllImport(libraryPath, EntryPoint = "LLVMDisposeMCJITMemoryManager", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DisposeMCJITMemoryManager(LLVMMCJITMemoryManagerRef @MM);

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeTransformUtils", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeTransformUtils(LLVMPassRegistryRef @R);

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeScalarOpts", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeScalarOpts(LLVMPassRegistryRef @R);

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeObjCARCOpts", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeObjCARCOpts(LLVMPassRegistryRef @R);

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeVectorization", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeVectorization(LLVMPassRegistryRef @R);

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeInstCombine", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeInstCombine(LLVMPassRegistryRef @R);

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeIPO", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeIPO(LLVMPassRegistryRef @R);

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeInstrumentation", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeInstrumentation(LLVMPassRegistryRef @R);

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeAnalysis", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeAnalysis(LLVMPassRegistryRef @R);

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeIPA", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeIPA(LLVMPassRegistryRef @R);

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeCodeGen", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeCodeGen(LLVMPassRegistryRef @R);

        [DllImport(libraryPath, EntryPoint = "LLVMInitializeTarget", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void InitializeTarget(LLVMPassRegistryRef @R);

        [DllImport(libraryPath, EntryPoint = "LLVMParseIRInContext", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool ParseIRInContext(LLVMContextRef @ContextRef, LLVMMemoryBufferRef @MemBuf, out LLVMModuleRef @OutM, out IntPtr @OutMessage);

        [DllImport(libraryPath, EntryPoint = "LLVMLinkModules", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool LinkModules(LLVMModuleRef @Dest, LLVMModuleRef @Src, LLVMLinkerMode @Mode, out IntPtr @OutMessage);

#if INCLUDE_LTO_SUPPORT
        [DllImport(libraryPath, EntryPoint = "llvm_create_optimizer", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern llvm_lto_t llvm_create_optimizer();

        [DllImport(libraryPath, EntryPoint = "llvm_destroy_optimizer", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void llvm_destroy_optimizer(llvm_lto_t @lto);

        [DllImport(libraryPath, EntryPoint = "llvm_read_object_file", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern llvm_lto_status llvm_read_object_file(llvm_lto_t @lto, [MarshalAs(UnmanagedType.LPStr)] string @input_filename);

        [DllImport(libraryPath, EntryPoint = "llvm_optimize_modules", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern llvm_lto_status llvm_optimize_modules(llvm_lto_t @lto, [MarshalAs(UnmanagedType.LPStr)] string @output_filename);

        [DllImport(libraryPath, EntryPoint = "lto_get_version", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr lto_get_version();

        [DllImport(libraryPath, EntryPoint = "lto_get_error_message", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr lto_get_error_message();

        [DllImport(libraryPath, EntryPoint = "lto_module_is_object_file", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_bool_t lto_module_is_object_file([MarshalAs(UnmanagedType.LPStr)] string @path);

        [DllImport(libraryPath, EntryPoint = "lto_module_is_object_file_for_target", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_bool_t lto_module_is_object_file_for_target([MarshalAs(UnmanagedType.LPStr)] string @path, [MarshalAs(UnmanagedType.LPStr)] string @target_triple_prefix);

        [DllImport(libraryPath, EntryPoint = "lto_module_is_object_file_in_memory", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_bool_t lto_module_is_object_file_in_memory(IntPtr @mem, int @length);

        [DllImport(libraryPath, EntryPoint = "lto_module_is_object_file_in_memory_for_target", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_bool_t lto_module_is_object_file_in_memory_for_target(IntPtr @mem, int @length, [MarshalAs(UnmanagedType.LPStr)] string @target_triple_prefix);

        [DllImport(libraryPath, EntryPoint = "lto_module_create", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_module_t lto_module_create([MarshalAs(UnmanagedType.LPStr)] string @path);

        [DllImport(libraryPath, EntryPoint = "lto_module_create_from_memory", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_module_t lto_module_create_from_memory(IntPtr @mem, int @length);

        [DllImport(libraryPath, EntryPoint = "lto_module_create_from_memory_with_path", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_module_t lto_module_create_from_memory_with_path(IntPtr @mem, int @length, [MarshalAs(UnmanagedType.LPStr)] string @path);

        [DllImport(libraryPath, EntryPoint = "lto_module_create_in_local_context", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_module_t lto_module_create_in_local_context(IntPtr @mem, int @length, [MarshalAs(UnmanagedType.LPStr)] string @path);

        [DllImport(libraryPath, EntryPoint = "lto_module_create_in_codegen_context", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_module_t lto_module_create_in_codegen_context(IntPtr @mem, int @length, [MarshalAs(UnmanagedType.LPStr)] string @path, lto_code_gen_t @cg);

        [DllImport(libraryPath, EntryPoint = "lto_module_create_from_fd", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_module_t lto_module_create_from_fd(int @fd, [MarshalAs(UnmanagedType.LPStr)] string @path, int @file_size);

        [DllImport(libraryPath, EntryPoint = "lto_module_create_from_fd_at_offset", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_module_t lto_module_create_from_fd_at_offset(int @fd, [MarshalAs(UnmanagedType.LPStr)] string @path, int @file_size, int @map_size, int @offset);

        [DllImport(libraryPath, EntryPoint = "lto_module_dispose", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void lto_module_dispose(lto_module_t @mod);

        [DllImport(libraryPath, EntryPoint = "lto_module_get_target_triple", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr lto_module_get_target_triple(lto_module_t @mod);

        [DllImport(libraryPath, EntryPoint = "lto_module_set_target_triple", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void lto_module_set_target_triple(lto_module_t @mod, [MarshalAs(UnmanagedType.LPStr)] string @triple);

        [DllImport(libraryPath, EntryPoint = "lto_module_get_num_symbols", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint lto_module_get_num_symbols(lto_module_t @mod);

        [DllImport(libraryPath, EntryPoint = "lto_module_get_symbol_name", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr lto_module_get_symbol_name(lto_module_t @mod, uint @index);

        [DllImport(libraryPath, EntryPoint = "lto_module_get_symbol_attribute", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_symbol_attributes lto_module_get_symbol_attribute(lto_module_t @mod, uint @index);

        [DllImport(libraryPath, EntryPoint = "lto_module_get_num_deplibs", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint lto_module_get_num_deplibs(lto_module_t @mod);

        [DllImport(libraryPath, EntryPoint = "lto_module_get_deplib", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr lto_module_get_deplib(lto_module_t @mod, uint @index);

        [DllImport(libraryPath, EntryPoint = "lto_module_get_num_linkeropts", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern uint lto_module_get_num_linkeropts(lto_module_t @mod);

        [DllImport(libraryPath, EntryPoint = "lto_module_get_linkeropt", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr lto_module_get_linkeropt(lto_module_t @mod, uint @index);

        [DllImport(libraryPath, EntryPoint = "lto_codegen_set_diagnostic_handler", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void lto_codegen_set_diagnostic_handler(lto_code_gen_t @param0, lto_diagnostic_handler_t @param1, IntPtr @param2);

        [DllImport(libraryPath, EntryPoint = "lto_codegen_create", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_code_gen_t lto_codegen_create();

        [DllImport(libraryPath, EntryPoint = "lto_codegen_create_in_local_context", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_code_gen_t lto_codegen_create_in_local_context();

        [DllImport(libraryPath, EntryPoint = "lto_codegen_dispose", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void lto_codegen_dispose(lto_code_gen_t @param0);

        [DllImport(libraryPath, EntryPoint = "lto_codegen_add_module", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_bool_t lto_codegen_add_module(lto_code_gen_t @cg, lto_module_t @mod);

        [DllImport(libraryPath, EntryPoint = "lto_codegen_set_debug_model", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_bool_t lto_codegen_set_debug_model(lto_code_gen_t @cg, lto_debug_model @param1);

        [DllImport(libraryPath, EntryPoint = "lto_codegen_set_pic_model", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_bool_t lto_codegen_set_pic_model(lto_code_gen_t @cg, lto_codegen_model @param1);

        [DllImport(libraryPath, EntryPoint = "lto_codegen_set_cpu", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void lto_codegen_set_cpu(lto_code_gen_t @cg, [MarshalAs(UnmanagedType.LPStr)] string @cpu);

        [DllImport(libraryPath, EntryPoint = "lto_codegen_set_assembler_path", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void lto_codegen_set_assembler_path(lto_code_gen_t @cg, [MarshalAs(UnmanagedType.LPStr)] string @path);

        [DllImport(libraryPath, EntryPoint = "lto_codegen_set_assembler_args", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void lto_codegen_set_assembler_args(lto_code_gen_t @cg, out IntPtr @args, int @nargs);

        [DllImport(libraryPath, EntryPoint = "lto_codegen_add_must_preserve_symbol", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void lto_codegen_add_must_preserve_symbol(lto_code_gen_t @cg, [MarshalAs(UnmanagedType.LPStr)] string @symbol);

        [DllImport(libraryPath, EntryPoint = "lto_codegen_write_merged_modules", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_bool_t lto_codegen_write_merged_modules(lto_code_gen_t @cg, [MarshalAs(UnmanagedType.LPStr)] string @path);

        [DllImport(libraryPath, EntryPoint = "lto_codegen_compile", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr lto_codegen_compile(lto_code_gen_t @cg, out int @length);

        [DllImport(libraryPath, EntryPoint = "lto_codegen_compile_to_file", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern lto_bool_t lto_codegen_compile_to_file(lto_code_gen_t @cg, out IntPtr @name);

        [DllImport(libraryPath, EntryPoint = "lto_codegen_debug_options", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void lto_codegen_debug_options(lto_code_gen_t @cg, [MarshalAs(UnmanagedType.LPStr)] string @param1);

        [DllImport(libraryPath, EntryPoint = "lto_initialize_disassembler", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void lto_initialize_disassembler();
#endif
        [DllImport(libraryPath, EntryPoint = "LLVMCreateObjectFile", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMObjectFileRef CreateObjectFile(LLVMMemoryBufferRef @MemBuf);

        [DllImport(libraryPath, EntryPoint = "LLVMDisposeObjectFile", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DisposeObjectFile(LLVMObjectFileRef @ObjectFile);

        [DllImport(libraryPath, EntryPoint = "LLVMGetSections", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMSectionIteratorRef GetSections(LLVMObjectFileRef @ObjectFile);

        [DllImport(libraryPath, EntryPoint = "LLVMDisposeSectionIterator", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DisposeSectionIterator(LLVMSectionIteratorRef @SI);

        [DllImport(libraryPath, EntryPoint = "LLVMIsSectionIteratorAtEnd", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool IsSectionIteratorAtEnd(LLVMObjectFileRef @ObjectFile, LLVMSectionIteratorRef @SI);

        [DllImport(libraryPath, EntryPoint = "LLVMMoveToNextSection", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void MoveToNextSection(LLVMSectionIteratorRef @SI);

        [DllImport(libraryPath, EntryPoint = "LLVMMoveToContainingSection", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void MoveToContainingSection(LLVMSectionIteratorRef @Sect, LLVMSymbolIteratorRef @Sym);

        [DllImport(libraryPath, EntryPoint = "LLVMGetSymbols", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMSymbolIteratorRef GetSymbols(LLVMObjectFileRef @ObjectFile);

        [DllImport(libraryPath, EntryPoint = "LLVMDisposeSymbolIterator", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DisposeSymbolIterator(LLVMSymbolIteratorRef @SI);

        [DllImport(libraryPath, EntryPoint = "LLVMIsSymbolIteratorAtEnd", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool IsSymbolIteratorAtEnd(LLVMObjectFileRef @ObjectFile, LLVMSymbolIteratorRef @SI);

        [DllImport(libraryPath, EntryPoint = "LLVMMoveToNextSymbol", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void MoveToNextSymbol(LLVMSymbolIteratorRef @SI);

        [DllImport(libraryPath, EntryPoint = "LLVMGetSectionName", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetSectionName(LLVMSectionIteratorRef @SI);

        [DllImport(libraryPath, EntryPoint = "LLVMGetSectionSize", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int GetSectionSize(LLVMSectionIteratorRef @SI);

        [DllImport(libraryPath, EntryPoint = "LLVMGetSectionContents", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetSectionContents(LLVMSectionIteratorRef @SI);

        [DllImport(libraryPath, EntryPoint = "LLVMGetSectionAddress", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int GetSectionAddress(LLVMSectionIteratorRef @SI);

        [DllImport(libraryPath, EntryPoint = "LLVMGetSectionContainsSymbol", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool GetSectionContainsSymbol(LLVMSectionIteratorRef @SI, LLVMSymbolIteratorRef @Sym);

        [DllImport(libraryPath, EntryPoint = "LLVMGetRelocations", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMRelocationIteratorRef GetRelocations(LLVMSectionIteratorRef @Section);

        [DllImport(libraryPath, EntryPoint = "LLVMDisposeRelocationIterator", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void DisposeRelocationIterator(LLVMRelocationIteratorRef @RI);

        [DllImport(libraryPath, EntryPoint = "LLVMIsRelocationIteratorAtEnd", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMBool IsRelocationIteratorAtEnd(LLVMSectionIteratorRef @Section, LLVMRelocationIteratorRef @RI);

        [DllImport(libraryPath, EntryPoint = "LLVMMoveToNextRelocation", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void MoveToNextRelocation(LLVMRelocationIteratorRef @RI);

        [DllImport(libraryPath, EntryPoint = "LLVMGetSymbolName", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetSymbolName(LLVMSymbolIteratorRef @SI);

        [DllImport(libraryPath, EntryPoint = "LLVMGetSymbolAddress", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int GetSymbolAddress(LLVMSymbolIteratorRef @SI);

        [DllImport(libraryPath, EntryPoint = "LLVMGetSymbolSize", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int GetSymbolSize(LLVMSymbolIteratorRef @SI);

        [DllImport(libraryPath, EntryPoint = "LLVMGetRelocationOffset", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int GetRelocationOffset(LLVMRelocationIteratorRef @RI);

        [DllImport(libraryPath, EntryPoint = "LLVMGetRelocationSymbol", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMSymbolIteratorRef GetRelocationSymbol(LLVMRelocationIteratorRef @RI);

        [DllImport(libraryPath, EntryPoint = "LLVMGetRelocationType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int GetRelocationType(LLVMRelocationIteratorRef @RI);

        [DllImport(libraryPath, EntryPoint = "LLVMGetRelocationTypeName", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetRelocationTypeName(LLVMRelocationIteratorRef @RI);

        [DllImport(libraryPath, EntryPoint = "LLVMGetRelocationValueString", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetRelocationValueString(LLVMRelocationIteratorRef @RI);

        [DllImport(libraryPath, EntryPoint = "LLVMAddArgumentPromotionPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddArgumentPromotionPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddConstantMergePass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddConstantMergePass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddDeadArgEliminationPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddDeadArgEliminationPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddFunctionAttrsPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddFunctionAttrsPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddFunctionInliningPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddFunctionInliningPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddAlwaysInlinerPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddAlwaysInlinerPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddGlobalDCEPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddGlobalDCEPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddGlobalOptimizerPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddGlobalOptimizerPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddIPConstantPropagationPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddIPConstantPropagationPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddPruneEHPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddPruneEHPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddIPSCCPPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddIPSCCPPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddInternalizePass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddInternalizePass(LLVMPassManagerRef @param0, uint @AllButMain);

        [DllImport(libraryPath, EntryPoint = "LLVMAddStripDeadPrototypesPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddStripDeadPrototypesPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddStripSymbolsPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddStripSymbolsPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMPassManagerBuilderCreate", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern LLVMPassManagerBuilderRef PassManagerBuilderCreate();

        [DllImport(libraryPath, EntryPoint = "LLVMPassManagerBuilderDispose", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void PassManagerBuilderDispose(LLVMPassManagerBuilderRef @PMB);

        [DllImport(libraryPath, EntryPoint = "LLVMPassManagerBuilderSetOptLevel", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void PassManagerBuilderSetOptLevel(LLVMPassManagerBuilderRef @PMB, uint @OptLevel);

        [DllImport(libraryPath, EntryPoint = "LLVMPassManagerBuilderSetSizeLevel", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void PassManagerBuilderSetSizeLevel(LLVMPassManagerBuilderRef @PMB, uint @SizeLevel);

        [DllImport(libraryPath, EntryPoint = "LLVMPassManagerBuilderSetDisableUnitAtATime", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void PassManagerBuilderSetDisableUnitAtATime(LLVMPassManagerBuilderRef @PMB, LLVMBool @Value);

        [DllImport(libraryPath, EntryPoint = "LLVMPassManagerBuilderSetDisableUnrollLoops", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void PassManagerBuilderSetDisableUnrollLoops(LLVMPassManagerBuilderRef @PMB, LLVMBool @Value);

        [DllImport(libraryPath, EntryPoint = "LLVMPassManagerBuilderSetDisableSimplifyLibCalls", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void PassManagerBuilderSetDisableSimplifyLibCalls(LLVMPassManagerBuilderRef @PMB, LLVMBool @Value);

        [DllImport(libraryPath, EntryPoint = "LLVMPassManagerBuilderUseInlinerWithThreshold", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void PassManagerBuilderUseInlinerWithThreshold(LLVMPassManagerBuilderRef @PMB, uint @Threshold);

        [DllImport(libraryPath, EntryPoint = "LLVMPassManagerBuilderPopulateFunctionPassManager", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void PassManagerBuilderPopulateFunctionPassManager(LLVMPassManagerBuilderRef @PMB, LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMPassManagerBuilderPopulateModulePassManager", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void PassManagerBuilderPopulateModulePassManager(LLVMPassManagerBuilderRef @PMB, LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMPassManagerBuilderPopulateLTOPassManager", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void PassManagerBuilderPopulateLTOPassManager(LLVMPassManagerBuilderRef @PMB, LLVMPassManagerRef @PM, LLVMBool @Internalize, LLVMBool @RunInliner);

        [DllImport(libraryPath, EntryPoint = "LLVMAddAggressiveDCEPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddAggressiveDCEPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddAlignmentFromAssumptionsPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddAlignmentFromAssumptionsPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddCFGSimplificationPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddCFGSimplificationPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddDeadStoreEliminationPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddDeadStoreEliminationPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddScalarizerPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddScalarizerPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddMergedLoadStoreMotionPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddMergedLoadStoreMotionPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddGVNPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddGVNPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddIndVarSimplifyPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddIndVarSimplifyPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddInstructionCombiningPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddInstructionCombiningPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddJumpThreadingPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddJumpThreadingPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddLICMPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddLICMPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddLoopDeletionPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddLoopDeletionPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddLoopIdiomPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddLoopIdiomPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddLoopRotatePass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddLoopRotatePass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddLoopRerollPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddLoopRerollPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddLoopUnrollPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddLoopUnrollPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddLoopUnswitchPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddLoopUnswitchPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddMemCpyOptPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddMemCpyOptPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddPartiallyInlineLibCallsPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddPartiallyInlineLibCallsPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddLowerSwitchPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddLowerSwitchPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddPromoteMemoryToRegisterPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddPromoteMemoryToRegisterPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddReassociatePass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddReassociatePass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddSCCPPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddSCCPPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddScalarReplAggregatesPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddScalarReplAggregatesPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddScalarReplAggregatesPassSSA", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddScalarReplAggregatesPassSSA(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddScalarReplAggregatesPassWithThreshold", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddScalarReplAggregatesPassWithThreshold(LLVMPassManagerRef @PM, int @Threshold);

        [DllImport(libraryPath, EntryPoint = "LLVMAddSimplifyLibCallsPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddSimplifyLibCallsPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddTailCallEliminationPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddTailCallEliminationPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddConstantPropagationPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddConstantPropagationPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddDemoteMemoryToRegisterPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddDemoteMemoryToRegisterPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddVerifierPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddVerifierPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddCorrelatedValuePropagationPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddCorrelatedValuePropagationPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddEarlyCSEPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddEarlyCSEPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddLowerExpectIntrinsicPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddLowerExpectIntrinsicPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddTypeBasedAliasAnalysisPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddTypeBasedAliasAnalysisPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddScopedNoAliasAAPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddScopedNoAliasAAPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddBasicAliasAnalysisPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddBasicAliasAnalysisPass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddBBVectorizePass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddBBVectorizePass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddLoopVectorizePass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddLoopVectorizePass(LLVMPassManagerRef @PM);

        [DllImport(libraryPath, EntryPoint = "LLVMAddSLPVectorizePass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void AddSLPVectorizePass(LLVMPassManagerRef @PM);
    }
}

