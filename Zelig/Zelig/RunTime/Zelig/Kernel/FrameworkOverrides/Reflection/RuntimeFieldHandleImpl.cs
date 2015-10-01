//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [TS.AllowCompileTimeIntrospection]
    [ExtendClass(typeof(System.RuntimeFieldHandle), NoConstructors=true)]
    public struct RuntimeFieldHandleImpl
    {
        //
        // State
        //

        [TS.WellKnownField( "RuntimeFieldHandleImpl_m_value" )] 
        internal TS.FieldRepresentation m_value;

        //
        // Constructor Methods
        //
    
        internal RuntimeFieldHandleImpl( TS.FieldRepresentation value )
        {
            m_value = value;
        }

        //--//

        //
        // Helper Methods
        //

        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        public override bool Equals( object obj )
        {
            if(!(obj is RuntimeFieldHandleImpl))
            {
                return false;
            }
    
            return Equals( (RuntimeFieldHandleImpl)obj );
        }

        public bool Equals( RuntimeFieldHandleImpl handle )
        {
            return this.m_value == handle.m_value;
        }

        //--//

        internal bool IsNullHandle()
        {
            return m_value == null;
        }

        //--//

        //
        // Access Methods
        //
    }
}
