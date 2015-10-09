//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [TS.AllowCompileTimeIntrospection]
    [ExtendClass(typeof(System.RuntimeTypeHandle), NoConstructors=true)]
    public unsafe struct RuntimeTypeHandleImpl
    {
        //
        // State
        //

        [TS.WellKnownField( "RuntimeTypeHandleImpl_m_value" )] 
        internal TS.VTable m_value;

        //
        // Constructor Methods
        //

        internal RuntimeTypeHandleImpl( TS.VTable value )
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
            if(!(obj is RuntimeTypeHandleImpl))
            {
                return false;
            }
    
            return Equals( (RuntimeTypeHandleImpl)obj );
        }

        public bool Equals( RuntimeTypeHandleImpl handle )
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

        //
        // Debug Methods
        //

        public override string ToString()
        {
            return m_value.ToString();
        }
    }
}
