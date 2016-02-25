//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public class WellKnownTypes
    {
        //
        // State
        //

        //
        // We need to explictly know at least WellKnownTypeAttribute, the rest will be resolved based on WellKnownType attributes on the actual types.
        //
        [WellKnownTypeLookup( "mscorlib", "Microsoft.Zelig.Internals", "WellKnownTypeAttribute" )]
        public readonly TypeRepresentation Microsoft_Zelig_Internals_WellKnownTypeAttribute;

        [WellKnownTypeLookup( "Microsoft.Zelig.Runtime.Common", "Microsoft.Zelig.Runtime.TypeSystem", "WellKnownTypeAttribute" )]
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_TypeSystem_WellKnownTypeAttribute;

        //--//

        [LinkToRuntimeType(typeof(System.Object           ))] public readonly TypeRepresentation System_Object;
        [LinkToRuntimeType(typeof(System.Array            ))] public readonly TypeRepresentation System_Array;
        [LinkToRuntimeType(typeof(System.ValueType        ))] public readonly TypeRepresentation System_ValueType;
        [LinkToRuntimeType(typeof(System.Enum             ))] public readonly TypeRepresentation System_Enum;
                                                 
        [LinkToRuntimeType(typeof(void                    ))] public readonly TypeRepresentation System_Void;
        [LinkToRuntimeType(typeof(System.Boolean          ))] public readonly TypeRepresentation System_Boolean;
        [LinkToRuntimeType(typeof(System.Char             ))] public readonly TypeRepresentation System_Char;
        [LinkToRuntimeType(typeof(System.SByte            ))] public readonly TypeRepresentation System_SByte;
        [LinkToRuntimeType(typeof(System.Byte             ))] public readonly TypeRepresentation System_Byte;
        [LinkToRuntimeType(typeof(System.Int16            ))] public readonly TypeRepresentation System_Int16;
        [LinkToRuntimeType(typeof(System.UInt16           ))] public readonly TypeRepresentation System_UInt16;
        [LinkToRuntimeType(typeof(System.Int32            ))] public readonly TypeRepresentation System_Int32;
        [LinkToRuntimeType(typeof(System.UInt32           ))] public readonly TypeRepresentation System_UInt32;
        [LinkToRuntimeType(typeof(System.Int64            ))] public readonly TypeRepresentation System_Int64;
        [LinkToRuntimeType(typeof(System.UInt64           ))] public readonly TypeRepresentation System_UInt64;
        [LinkToRuntimeType(typeof(System.Single           ))] public readonly TypeRepresentation System_Single;
        [LinkToRuntimeType(typeof(System.Double           ))] public readonly TypeRepresentation System_Double;
        [LinkToRuntimeType(typeof(System.String           ))] public readonly TypeRepresentation System_String;
        [LinkToRuntimeType(typeof(System.IntPtr           ))] public readonly TypeRepresentation System_IntPtr;
        [LinkToRuntimeType(typeof(System.UIntPtr          ))] public readonly TypeRepresentation System_UIntPtr;
        [LinkToRuntimeType(typeof(System.Nullable<>       ))] public readonly TypeRepresentation System_Nullable_of_T;
                  
        [LinkToRuntimeType(typeof(System.TypedReference   ))] public readonly TypeRepresentation System_TypedReference;
        
        [LinkToRuntimeType(typeof(System.Delegate         ))] public readonly TypeRepresentation System_Delegate;
        [LinkToRuntimeType(typeof(System.MulticastDelegate))] public readonly TypeRepresentation System_MulticastDelegate;

                                                              public readonly TypeRepresentation System_WeakReference;
        
                                                              public readonly TypeRepresentation System_Attribute;
                                                              public readonly TypeRepresentation System_RuntimeTypeHandle;
                                                              public readonly TypeRepresentation System_RuntimeFieldHandle;
                                                              public readonly TypeRepresentation System_RuntimeMethodHandle;
                                                              public readonly TypeRepresentation System_RuntimeArgumentHandle;

                                                              public readonly TypeRepresentation System_RuntimeType;
                                                                                                              
                                                              public readonly TypeRepresentation System_Threading_Thread;

                                                              public readonly TypeRepresentation System_Runtime_CompilerServices_IsVolatile;
                                                              public readonly TypeRepresentation System_FlagsAttribute;

                                                              public readonly TypeRepresentation System_Runtime_InteropServices_FieldOffsetAttribute;
        
                                                              public readonly TypeRepresentation System_IComparable_of_T;
                                                              public readonly TypeRepresentation System_Collections_Generic_Comparer_of_T;
                                                              public readonly TypeRepresentation System_Collections_Generic_GenericComparer_of_T;
                                                              public readonly TypeRepresentation System_Collections_Generic_NullableComparer_of_T;
                                                              public readonly TypeRepresentation System_Collections_Generic_ObjectComparer_of_T;

                                                              public readonly TypeRepresentation System_IEquatable_of_T;
                                                              public readonly TypeRepresentation System_Collections_Generic_EqualityComparer_of_Enum;
                                                              public readonly TypeRepresentation System_Collections_Generic_EqualityComparer_of_Enum_sbyte;
                                                              public readonly TypeRepresentation System_Collections_Generic_EqualityComparer_of_Enum_short;
                                                              public readonly TypeRepresentation System_Collections_Generic_EqualityComparer_of_Enum_long;
                                                              public readonly TypeRepresentation System_Collections_Generic_EqualityComparer_of_T;
                                                              public readonly TypeRepresentation System_Collections_Generic_GenericEqualityComparer_of_T;
                                                              public readonly TypeRepresentation System_Collections_Generic_NullableEqualityComparer_of_T;
                                                              public readonly TypeRepresentation System_Collections_Generic_ObjectEqualityComparer_of_T;

                                                              public readonly TypeRepresentation System_CurrentSystemTimeZone;
        
                                                              public readonly TypeRepresentation System_Resources_ResourceManager;

        // TypeSystem attributes
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_TypeSystem_AssumeReferencedAttribute;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_TypeSystem_DisableAutomaticReferenceCountingAttribute;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_TypeSystem_DisableReferenceCountingAttribute;

        // Runtime attributes
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_AliasForBaseFieldAttribute;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_AliasForBaseMethodAttribute;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_AliasForSuperMethodAttribute;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_AliasForTargetMethodAttribute;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_AlignmentRequirementsAttribute;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_DiscardTargetImplementationAttribute;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_ExtendClassAttribute;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_InjectAtEntryPointAttribute;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_InjectAtExitPointAttribute;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_MergeWithTargetImplementationAttribute;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_SingletonFactoryPlatformFilterAttribute;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_ProductFilterAttribute;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_CapabilitiesFilterAttribute;

        // Helper types
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_ObjectHeader;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_LandingPadResult;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_RuntimeTypeImpl;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_SZArrayHelper;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_TypeSystem_CodePointer;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_TypeSystem_VTable;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_TypeSystem_GlobalRoot;

        //--//

        public readonly TypeRepresentation Microsoft_Zelig_Runtime_ThreadManager;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_GarbageCollectionManager;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_ReferenceCountingCollector;
        public readonly TypeRepresentation Microsoft_Zelig_Runtime_StrictReferenceCountingCollector;

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
