//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;

    public class WellKnownFields
    {
        //
        // State
        //

        public readonly FieldRepresentation ArrayImpl_m_numElements;

        public readonly FieldRepresentation ThreadImpl_s_currentThread;
        public readonly FieldRepresentation ThreadImpl_m_currentException;

        public readonly FieldRepresentation DelegateImpl_m_target;
        public readonly FieldRepresentation DelegateImpl_m_codePtr;

        public readonly FieldRepresentation MulticastDelegateImpl_m_invocationList;

        public readonly FieldRepresentation StringImpl_ArrayLength;
        public readonly FieldRepresentation StringImpl_StringLength;
        public readonly FieldRepresentation StringImpl_FirstChar;

        //--//

        public readonly FieldRepresentation ObjectHeader_MultiUseWord;
        public readonly FieldRepresentation ObjectHeader_VirtualTable;

        public readonly FieldRepresentation CodePointer_Target;

        public readonly FieldRepresentation CodeMap_LookupAddress;
        public readonly FieldRepresentation CodeMap_Target;
        public readonly FieldRepresentation CodeMap_Ranges;
        public readonly FieldRepresentation CodeMap_ExceptionMap;

        public readonly FieldRepresentation ExceptionMap_Ranges;

        public readonly FieldRepresentation VTable_BaseSize;
        public readonly FieldRepresentation VTable_ElementSize;
        public readonly FieldRepresentation VTable_TypeInfo;
        public readonly FieldRepresentation VTable_GCInfo;
        public readonly FieldRepresentation VTable_Type;
        public readonly FieldRepresentation VTable_ShapeCategory;
        public readonly FieldRepresentation VTable_MethodPointers;
        public readonly FieldRepresentation VTable_InterfaceMap;

        public readonly FieldRepresentation RuntimeTypeImpl_m_handle;

        public readonly FieldRepresentation TypeRepresentation_InterfaceMethodTables;
        public readonly FieldRepresentation TypeRepresentation_MethodTable;

        public readonly FieldRepresentation RuntimeTypeHandleImpl_m_value;
        public readonly FieldRepresentation RuntimeFieldHandleImpl_m_value;
        public readonly FieldRepresentation RuntimeMethodHandleImpl_m_value;

        public readonly FieldRepresentation ResourceManagerImpl_s_resources;

        //--//

        public readonly FieldRepresentation Memory_m_availableMemory;
        public readonly FieldRepresentation Memory_m_relocationInfo;
        public readonly FieldRepresentation RelocationInfo_m_data;

        public readonly FieldRepresentation GarbageCollectionManager_m_extensionTargets;
        public readonly FieldRepresentation GarbageCollectionManager_m_extensionHandlers;

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
