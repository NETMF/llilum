//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;

    public class WellKnownMethods
    {
        //
        // State
        //

        public readonly MethodRepresentation MemoryManager_Allocate;
        public readonly MethodRepresentation MemoryManager_Release;

        public readonly MethodRepresentation TypeSystemManager_InvokeStaticConstructors;
        public readonly MethodRepresentation TypeSystemManager_AllocateObject;
        public readonly MethodRepresentation TypeSystemManager_AllocateObjectWithExtensions;
        public readonly MethodRepresentation TypeSystemManager_AllocateArray;
        public readonly MethodRepresentation TypeSystemManager_AllocateArrayNoClear;
        public readonly MethodRepresentation TypeSystemManager_AllocateString;
        public readonly MethodRepresentation TypeSystemManager_CastToType;
        public readonly MethodRepresentation TypeSystemManager_CastToTypeNoThrow;
        public readonly MethodRepresentation TypeSystemManager_CastToSealedType;
        public readonly MethodRepresentation TypeSystemManager_CastToSealedTypeNoThrow;
        public readonly MethodRepresentation TypeSystemManager_CastToInterface;
        public readonly MethodRepresentation TypeSystemManager_CastToInterfaceNoThrow;
        public readonly MethodRepresentation TypeSystemManager_Throw;
        public readonly MethodRepresentation TypeSystemManager_Rethrow;
        public readonly MethodRepresentation TypeSystemManager_Rethrow__Exception;
        public readonly MethodRepresentation TypeSystemManager_get_Instance;

        public readonly MethodRepresentation VTable_Get;
        public readonly MethodRepresentation VTable_GetInterface;

        public readonly MethodRepresentation ArrayImpl_get_Length;

        public readonly MethodRepresentation StringImpl_ctor_charArray_int_int;
        public readonly MethodRepresentation StringImpl_ctor_charArray;
        public readonly MethodRepresentation StringImpl_ctor_char_int;
        public readonly MethodRepresentation StringImpl_FastAllocateString;

        public readonly MethodRepresentation MulticastDelegateImpl_MulticastDelegateImpl;

        public readonly MethodRepresentation ThreadImpl_get_CurrentThread;
        public readonly MethodRepresentation ThreadImpl_GetCurrentException;
        public readonly MethodRepresentation ThreadImpl_ThrowNullException;
        public readonly MethodRepresentation ThreadImpl_ThrowIndexOutOfRangeException;
        public readonly MethodRepresentation ThreadImpl_ThrowOverflowException;
        public readonly MethodRepresentation ThreadImpl_ThrowNotImplementedException;

        public readonly MethodRepresentation RuntimeHelpers_InitializeArray2;

        public readonly MethodRepresentation TypeImpl_GetTypeFromHandle;

        //--//

        public readonly MethodRepresentation Object_Equals;
        public readonly MethodRepresentation Object_GetHashCode;

        public readonly MethodRepresentation System_Buffer_InternalMemoryCopy;
        public readonly MethodRepresentation System_Buffer_InternalBackwardMemoryCopy;

        public readonly MethodRepresentation Helpers_BinaryOperations_IntDiv;
        public readonly MethodRepresentation Helpers_BinaryOperations_IntRem;

        public readonly MethodRepresentation Helpers_BinaryOperations_UintDiv;
        public readonly MethodRepresentation Helpers_BinaryOperations_UintRem;

        public readonly MethodRepresentation Helpers_BinaryOperations_LongMul;
        public readonly MethodRepresentation Helpers_BinaryOperations_LongDiv;
        public readonly MethodRepresentation Helpers_BinaryOperations_LongRem;
        public readonly MethodRepresentation Helpers_BinaryOperations_LongShl;
        public readonly MethodRepresentation Helpers_BinaryOperations_LongShr;

        public readonly MethodRepresentation Helpers_BinaryOperations_UlongMul;
        public readonly MethodRepresentation Helpers_BinaryOperations_UlongDiv;
        public readonly MethodRepresentation Helpers_BinaryOperations_UlongRem;
        public readonly MethodRepresentation Helpers_BinaryOperations_UlongShl;
        public readonly MethodRepresentation Helpers_BinaryOperations_UlongShr;

        public readonly MethodRepresentation Finalizer_Allocate;

        //--//

        public readonly MethodRepresentation Microsoft_Zelig_Runtime_AbstractMethodWrapper_AddActivationRecordEvent;
        public readonly MethodRepresentation Microsoft_Zelig_Runtime_AbstractMethodWrapper_Prologue;
        public readonly MethodRepresentation Microsoft_Zelig_Runtime_AbstractMethodWrapper_Prologue2;
        public readonly MethodRepresentation Microsoft_Zelig_Runtime_AbstractMethodWrapper_Epilogue;
        public readonly MethodRepresentation Microsoft_Zelig_Runtime_AbstractMethodWrapper_Epilogue2;

        //--//
        
        //
        // Helper Methods
        //

        public void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            context.TransformFields( this );

            context.Pop();
        }
    }
}
