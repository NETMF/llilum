//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [TS.WellKnownType( "Microsoft_Zelig_Runtime_TypeImpl" )]
    [ExtendClass(typeof(System.Type), NoConstructors=true, ProcessAfter=typeof(RuntimeTypeHandleImpl))]
    public abstract class TypeImpl : MemberInfoImpl
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        //--//

        //
        // Helper Methods
        //

        //[Inline]
        [TS.WellKnownMethod( "TypeImpl_GetTypeFromHandle" )]
        public static Type GetTypeFromHandle( RuntimeTypeHandleImpl handle )
        {
            TS.VTable vt = handle.m_value;

            return (vt != null) ? vt.Type : null;
        }

        //
        // Access Methods
        //

        public abstract System.Reflection.Assembly Assembly
        {
            get;
        }

        public abstract Type BaseType
        {
            get;
        }

        public abstract String FullName
        {
            get;
        }

        public abstract String Namespace
        {
            get;
        }
    
        public abstract String AssemblyQualifiedName
        {
            get;
        }
    
        public bool IsValueType
        {
            get
            {
                return IsValueTypeInner;
            }
        }

        //--//

        protected abstract bool IsValueTypeInner
        {
            get;
        }
    }
}
