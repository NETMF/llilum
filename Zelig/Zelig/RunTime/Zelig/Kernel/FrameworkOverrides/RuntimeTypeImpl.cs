//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [TS.WellKnownType( "Microsoft_Zelig_Runtime_RuntimeTypeImpl" )]
    [ExtendClass("System_RuntimeType", NoConstructors=true, ProcessAfter=typeof(RuntimeTypeHandleImpl))]
    public class RuntimeTypeImpl : TypeImpl
    {
        //
        // State
        //

#pragma warning disable 649
        [TS.WellKnownField( "RuntimeTypeImpl_m_handle" )] [AliasForBaseField] internal RuntimeTypeHandleImpl m_handle;
#pragma warning restore 649

        //
        // Constructor Methods
        //

        //--//

        //
        // Helper Methods
        //

        //--//

        //
        // Access Methods
        //

        public override System.Reflection.Assembly Assembly
        {
            get
            {
                return null;
            }
        }

        public override Type BaseType
        {
            get
            {
                TS.TypeRepresentation baseTd = m_handle.m_value.TypeInfo.Extends;

                return (baseTd != null) ? baseTd.VirtualTable.Type : null;
            }
        }

        public override String Name
        {
            get
            {
                TS.TypeRepresentation td = m_handle.m_value.TypeInfo;

                return td.FullName;
            }
        }

        public override String FullName
        {
            get
            {
                TS.TypeRepresentation td = m_handle.m_value.TypeInfo;

                return td.FullName;
            }
        }

        public override String AssemblyQualifiedName
        {
            get
            {
////            if(!IsGenericTypeDefinition && ContainsGenericParameters)
////            {
////                return null;
////            }
    
////            return System.Reflection.Assembly.CreateQualifiedName( this.Assembly.FullName, this.FullName );
                return this.FullName;
            }
        }
    
        public override String Namespace
        {
            get
            {
                TS.TypeRepresentation td = m_handle.m_value.TypeInfo;

                return td.Namespace;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return (Type)(object)this;
            }
        }

        //--//

        protected override bool IsValueTypeInner
        {
            get
            {
                return m_handle.m_value.TypeInfo is TS.ValueTypeRepresentation;
            }
        }
    }
}
