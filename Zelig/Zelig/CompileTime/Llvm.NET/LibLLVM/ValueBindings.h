#ifndef _VALUE_BINDINGS_H_
#define _VALUE_BINDINGS_H_

#include "llvm-c/Core.h"

#ifdef __cplusplus
extern "C" {
#endif

    enum LLVMValueKind
    {
        LLVMValueKindArgumentVal,              // This is an instance of Argument
        LLVMValueKindBasicBlockVal,            // This is an instance of BasicBlock
        LLVMValueKindFunctionVal,              // This is an instance of Function
        LLVMValueKindGlobalAliasVal,           // This is an instance of GlobalAlias
        LLVMValueKindGlobalVariableVal,        // This is an instance of GlobalVariable
        LLVMValueKindUndefValueVal,            // This is an instance of UndefValue
        LLVMValueKindBlockAddressVal,          // This is an instance of BlockAddress
        LLVMValueKindConstantExprVal,          // This is an instance of ConstantExpr
        LLVMValueKindConstantAggregateZeroVal, // This is an instance of ConstantAggregateZero
        LLVMValueKindConstantDataArrayVal,     // This is an instance of ConstantDataArray
        LLVMValueKindConstantDataVectorVal,    // This is an instance of ConstantDataVector
        LLVMValueKindConstantIntVal,           // This is an instance of ConstantInt
        LLVMValueKindConstantFPVal,            // This is an instance of ConstantFP
        LLVMValueKindConstantArrayVal,         // This is an instance of ConstantArray
        LLVMValueKindConstantStructVal,        // This is an instance of ConstantStruct
        LLVMValueKindConstantVectorVal,        // This is an instance of ConstantVector
        LLVMValueKindConstantPointerNullVal,   // This is an instance of ConstantPointerNull
        LLVMValueKindMetadataAsValueVal,       // This is an instance of MetadataAsValue
        LLVMValueKindInlineAsmVal,             // This is an instance of InlineAsm
        LLVMValueKindInstructionVal,           // This is an instance of Instruction
                                               // Enum values starting at InstructionVal are used for Instructions;

                                               // Markers:
        LLVMValueKindConstantFirstVal = LLVMValueKindFunctionVal,
        LLVMValueKindConstantLastVal = LLVMValueKindConstantPointerNullVal
    };


    LLVMBool LLVMIsConstantZeroValue( LLVMValueRef valueRef );
    void LLVMRemoveGlobalFromParent( LLVMValueRef valueRef );
    
    LLVMValueRef LLVMBuildIntCast2( LLVMBuilderRef B, LLVMValueRef Val, LLVMTypeRef DestTy, LLVMBool isSigned, const char *Name );
    int LLVMGetValueID( LLVMValueRef valueRef);

#ifdef __cplusplus
}
#endif

#endif