// This file was generated from LLVMLib.3.6.1 Extended API using ClangSharp P/Invoke generator
// it was further modified in the following ways:
//  - removed most uses of the partial keyword except for LLVMNative static class
//  - added warning disable to avoid benign compiler warnings about fields only set by native code
//  - modified all elements to be internal instead of public
//  - modified PInvoke attributes to use fully qualified name for CallingConvention to avoid conflicts
//  - removed C++ function unwrap()
//  - removed DEFINE_ISA_CONVERSION_FUNCTIONS which was erroneously mis-identified as a function
//    declaration rather than a preprocessor macro instantiation
//  - converted several int return and param types to proper LLVMxxxRef types not handled correctly
//    by the ClangSharp code generator
//
using System;
using System.Runtime.InteropServices;

namespace Llvm.NET
{
    internal partial struct LLVMOpaqueMetadata
    {
    }

    internal partial struct LLVMOpaqueDIBuilder
    {
    }

    internal partial struct LLVMMetadataRef
    {
        internal LLVMMetadataRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        internal IntPtr Pointer;
    }

    internal partial struct LLVMDIBuilderRef
    {
        internal LLVMDIBuilderRef(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        internal IntPtr Pointer;
    }

    
    internal enum LLVMModFlagBehavior
    {
        @Error = 1,
        @Warning = 2,
        @Require = 3,
        @Override = 4,
        @Append = 5,
        @AppendUnique = 6,
        @ModFlagBehaviorFirstVal = Error,
        @ModFlagBehaviorLastVal = AppendUnique
    }

    internal enum LLVMValueKind : int
    {
        @LLVMValueKindArgumentVal,              // This is an instance of Argument
        @LLVMValueKindBasicBlockVal,            // This is an instance of BasicBlock
        @LLVMValueKindFunctionVal,              // This is an instance of Function
        @LLVMValueKindGlobalAliasVal,           // This is an instance of GlobalAlias
        @LLVMValueKindGlobalVariableVal,        // This is an instance of GlobalVariable
        @LLVMValueKindUndefValueVal,            // This is an instance of UndefValue
        @LLVMValueKindBlockAddressVal,          // This is an instance of BlockAddress
        @LLVMValueKindConstantExprVal,          // This is an instance of ConstantExpr
        @LLVMValueKindConstantAggregateZeroVal, // This is an instance of ConstantAggregateZero
        @LLVMValueKindConstantDataArrayVal,     // This is an instance of ConstantDataArray
        @LLVMValueKindConstantDataVectorVal,    // This is an instance of ConstantDataVector
        @LLVMValueKindConstantIntVal,           // This is an instance of ConstantInt
        @LLVMValueKindConstantFPVal,            // This is an instance of ConstantFP
        @LLVMValueKindConstantArrayVal,         // This is an instance of ConstantArray
        @LLVMValueKindConstantStructVal,        // This is an instance of ConstantStruct
        @LLVMValueKindConstantVectorVal,        // This is an instance of ConstantVector
        @LLVMValueKindConstantPointerNullVal,   // This is an instance of ConstantPointerNull
        @LLVMValueKindMetadataAsValueVal,       // This is an instance of MetadataAsValue
        @LLVMValueKindInlineAsmVal,             // This is an instance of InlineAsm
        @LLVMValueKindInstructionVal,           // This is an instance of Instruction
                                               // Enum values starting at InstructionVal are used for Instructions;

        // Markers:
        @LLVMValueKindConstantFirstVal = LLVMValueKindFunctionVal,
        @LLVMValueKindConstantLastVal = LLVMValueKindConstantPointerNullVal
    };


    internal static partial class LLVMNative
    {
        [DllImport( libraryPath, EntryPoint = "LLVMGetValueID", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl )]
        public static extern int GetValueID( LLVMValueRef @val );

        [ DllImport( libraryPath, EntryPoint = "LLVMBuildIntCast2", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl )]
        public static extern LLVMValueRef BuildIntCast( LLVMBuilderRef @param0, LLVMValueRef @Val, LLVMTypeRef @DestTy, LLVMBool isSigned, [MarshalAs( UnmanagedType.LPStr )] string @Name );

        [DllImport( libraryPath, EntryPoint = "LLVMSetDebugLoc", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl )]
        internal static extern void SetDebugLoc( LLVMValueRef inst, uint line, uint column, LLVMMetadataRef scope );

        [DllImport(libraryPath, EntryPoint = "LLVMVerifyFunctionEx", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMBool VerifyFunctionEx(LLVMValueRef @Fn, LLVMVerifierFailureAction @Action, out IntPtr @OutMessages);

        [DllImport( libraryPath, EntryPoint = "LLVMInitializeNativeTargetExport", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl )]
        internal static extern LLVMBool LLVMInitializeNativeTarget( );

        [DllImport( libraryPath, EntryPoint = "LLVMInitializeNativeAsmParserExport", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl )]
        internal static extern LLVMBool LLVMInitializeNativeAsmParser( );

        [DllImport( libraryPath, EntryPoint = "LLVMInitializeNativeAsmPrinterExport", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl )]
        internal static extern LLVMBool LLVMInitializeNativeAsmPrinter( );

        [DllImport( libraryPath, EntryPoint = "LLVMInitializeNativeDisassemblerExport", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl )]
        internal static extern LLVMBool LLVMInitializeNativeDisassembler( );

        [DllImport(libraryPath, EntryPoint = "LLVMAddAddressSanitizerFunctionPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern void AddAddressSanitizerFunctionPass( LLVMPassManagerRef @PM );

        [DllImport(libraryPath, EntryPoint = "LLVMAddAddressSanitizerModulePass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern void AddAddressSanitizerModulePass( LLVMPassManagerRef @PM );

        [DllImport(libraryPath, EntryPoint = "LLVMAddThreadSanitizerPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern void AddThreadSanitizerPass( LLVMPassManagerRef @PM );

        [DllImport(libraryPath, EntryPoint = "LLVMAddMemorySanitizerPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern void AddMemorySanitizerPass( LLVMPassManagerRef @PM );

        [DllImport(libraryPath, EntryPoint = "LLVMAddDataFlowSanitizerPass", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern void AddDataFlowSanitizerPass( LLVMPassManagerRef @PM, [MarshalAs(UnmanagedType.LPStr)] string @ABIListFile);

        [DllImport(libraryPath, EntryPoint = "LLVMAddModuleFlag", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern void AddModuleFlag(LLVMModuleRef @M, LLVMModFlagBehavior behavior, [MarshalAs(UnmanagedType.LPStr)] string @name, uint @value);

        [DllImport( libraryPath, EntryPoint = "LLVMGetOrInsertFunction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl )]
        internal static extern LLVMValueRef GetOrInsertFunction( LLVMModuleRef module, [MarshalAs( UnmanagedType.LPStr )] string @name, LLVMTypeRef functionType );

        [DllImport(libraryPath, EntryPoint = "LLVMAddFunctionAttr2", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern void AddFunctionAttr2(LLVMValueRef @Fn, ulong @PA);

        [DllImport(libraryPath, EntryPoint = "LLVMGetFunctionAttr2", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern ulong GetFunctionAttr2( LLVMValueRef @Fn );

        [DllImport(libraryPath, EntryPoint = "LLVMRemoveFunctionAttr2", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern void RemoveFunctionAttr2( LLVMValueRef @Fn, ulong @PA);

        [DllImport(libraryPath, EntryPoint = "LLVMIsConstantZeroValue", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMBool IsConstantZeroValue( LLVMValueRef @Val );

        [DllImport( libraryPath, EntryPoint = "LLVMRemoveGlobalFromParent", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl )]
        internal static extern void RemoveGlobalFromParent( LLVMValueRef @Val );

        [DllImport(libraryPath, EntryPoint = "LLVMConstantAsMetadata", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef ConstantAsMetadata( LLVMValueRef @Val );

        [DllImport(libraryPath, EntryPoint = "LLVMMDString2", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef MDString2(LLVMContextRef @C, [MarshalAs(UnmanagedType.LPStr)] string @Str, uint @SLen);

        [DllImport(libraryPath, EntryPoint = "LLVMMDNode2", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef MDNode2( LLVMContextRef @C, out LLVMMetadataRef @MDs, uint @Count);

        [DllImport(libraryPath, EntryPoint = "LLVMTemporaryMDNode", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef TemporaryMDNode( LLVMContextRef @C, out LLVMMetadataRef @MDs, uint @Count);

        [DllImport(libraryPath, EntryPoint = "LLVMAddNamedMetadataOperand2", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern void AddNamedMetadataOperand2( LLVMModuleRef @M, [MarshalAs(UnmanagedType.LPStr)] string @name, LLVMMetadataRef @Val);

        [DllImport(libraryPath, EntryPoint = "LLVMSetMetadata2", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern void SetMetadata2( LLVMValueRef @Inst, uint @KindID, LLVMMetadataRef @MD);

        [DllImport(libraryPath, EntryPoint = "LLVMMetadataReplaceAllUsesWith", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern void MetadataReplaceAllUsesWith(LLVMMetadataRef @MD, LLVMMetadataRef @New);

        [DllImport(libraryPath, EntryPoint = "LLVMSetCurrentDebugLocation2", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern void SetCurrentDebugLocation2( LLVMBuilderRef @Bref, uint @Line, uint @Col, LLVMMetadataRef @Scope, LLVMMetadataRef @InlinedAt);

        [DllImport(libraryPath, EntryPoint = "LLVMNewDIBuilder", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMDIBuilderRef NewDIBuilder( LLVMModuleRef @m, LLVMBool allowUnresolved );

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderDestroy", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern void DIBuilderDestroy(LLVMDIBuilderRef @d);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderFinalize", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern void DIBuilderFinalize(LLVMDIBuilderRef @d);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderCreateCompileUnit", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef DIBuilderCreateCompileUnit(LLVMDIBuilderRef @D, uint @Language, [MarshalAs(UnmanagedType.LPStr)] string @File, [MarshalAs(UnmanagedType.LPStr)] string @Dir, [MarshalAs(UnmanagedType.LPStr)] string @Producer, int @Optimized, [MarshalAs(UnmanagedType.LPStr)] string @Flags, uint @RuntimeVersion);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderCreateFile", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef DIBuilderCreateFile(LLVMDIBuilderRef @D, [MarshalAs(UnmanagedType.LPStr)] string @File, [MarshalAs(UnmanagedType.LPStr)] string @Dir);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderCreateLexicalBlock", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef DIBuilderCreateLexicalBlock(LLVMDIBuilderRef @D, LLVMMetadataRef @Scope, LLVMMetadataRef @File, uint @Line, uint @Column);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderCreateLexicalBlockFile", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef DIBuilderCreateLexicalBlockFile(LLVMDIBuilderRef @D, LLVMMetadataRef @Scope, LLVMMetadataRef @File, uint @Discriminator);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderCreateFunction", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef DIBuilderCreateFunction(LLVMDIBuilderRef @D, LLVMMetadataRef @Scope, [MarshalAs(UnmanagedType.LPStr)] string @Name, [MarshalAs(UnmanagedType.LPStr)] string @LinkageName, LLVMMetadataRef @File, uint @Line, LLVMMetadataRef @CompositeType, int @IsLocalToUnit, int @IsDefinition, uint @ScopeLine, uint @Flags, int @IsOptimized, LLVMValueRef @Function);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderCreateLocalVariable", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef DIBuilderCreateLocalVariable(LLVMDIBuilderRef @D, uint @Tag, LLVMMetadataRef @Scope, [MarshalAs(UnmanagedType.LPStr)] string @Name, LLVMMetadataRef @File, uint @Line, LLVMMetadataRef @Ty, int @AlwaysPreserve, uint @Flags, uint @ArgNo);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderCreateBasicType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef DIBuilderCreateBasicType(LLVMDIBuilderRef @D, [MarshalAs(UnmanagedType.LPStr)] string @Name, ulong @SizeInBits, ulong @AlignInBits, uint @Encoding);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderCreatePointerType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef DIBuilderCreatePointerType(LLVMDIBuilderRef @D, LLVMMetadataRef @PointeeType, ulong @SizeInBits, ulong @AlignInBits, [MarshalAs(UnmanagedType.LPStr)] string @Name);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderCreateSubroutineType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef DIBuilderCreateSubroutineType(LLVMDIBuilderRef @D, LLVMMetadataRef @File, LLVMMetadataRef @ParameterTypes);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderCreateStructType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef DIBuilderCreateStructType(LLVMDIBuilderRef @D, LLVMMetadataRef @Scope, [MarshalAs(UnmanagedType.LPStr)] string @Name, LLVMMetadataRef @File, uint @Line, ulong @SizeInBits, ulong @AlignInBits, uint @Flags, LLVMMetadataRef @DerivedFrom, LLVMMetadataRef @ElementTypes);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderCreateMemberType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef DIBuilderCreateMemberType(LLVMDIBuilderRef @D, LLVMMetadataRef @Scope, [MarshalAs(UnmanagedType.LPStr)] string @Name, LLVMMetadataRef @File, uint @Line, ulong @SizeInBits, ulong @AlignInBits, ulong @OffsetInBits, uint @Flags, LLVMMetadataRef @Ty);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderCreateArrayType", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef DIBuilderCreateArrayType(LLVMDIBuilderRef @D, ulong @SizeInBits, ulong @AlignInBits, LLVMMetadataRef @ElementType, LLVMMetadataRef @Subscripts);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderCreateTypedef", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef DIBuilderCreateTypedef(LLVMDIBuilderRef @D, LLVMMetadataRef @Ty, [MarshalAs(UnmanagedType.LPStr)] string @Name, LLVMMetadataRef @File, uint @Line, LLVMMetadataRef @Context);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderGetOrCreateSubrange", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef DIBuilderGetOrCreateSubrange(LLVMDIBuilderRef @D, long @Lo, long @Count);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderGetOrCreateArray", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef DIBuilderGetOrCreateArray(LLVMDIBuilderRef @D, out LLVMMetadataRef @Data, ulong @Length);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderGetOrCreateTypeArray", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef DIBuilderGetOrCreateTypeArray(LLVMDIBuilderRef @D, out LLVMMetadataRef @Data, ulong @Length);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderCreateExpression", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern LLVMMetadataRef DIBuilderCreateExpression(LLVMDIBuilderRef @Dref, out long @Addr, ulong @Length);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderInsertDeclareAtEnd", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern int DIBuilderInsertDeclareAtEnd(LLVMDIBuilderRef @D, int @Storage, LLVMMetadataRef @VarInfo, LLVMMetadataRef @Expr, int @Block);

        [DllImport(libraryPath, EntryPoint = "LLVMDIBuilderInsertValueAtEnd", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal static extern int DIBuilderInsertValueAtEnd(LLVMDIBuilderRef @D, int @Val, ulong @Offset, LLVMMetadataRef @VarInfo, LLVMMetadataRef @Expr, int @Block);
    }
}
