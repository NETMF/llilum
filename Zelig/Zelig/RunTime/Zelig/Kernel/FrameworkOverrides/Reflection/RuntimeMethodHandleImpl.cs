//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [TS.AllowCompileTimeIntrospection]
    [ExtendClass(typeof(System.RuntimeMethodHandle), NoConstructors=true)]
    public unsafe struct RuntimeMethodHandleImpl
    {
        //
        // State
        //

        [TS.WellKnownField( "RuntimeMethodHandleImpl_m_value" )] 
        internal TS.MethodRepresentation m_value;

        //
        // Constructor Methods
        //
    
        internal RuntimeMethodHandleImpl( TS.MethodRepresentation value )
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
            if(!(obj is RuntimeMethodHandleImpl))
            {
                return false;
            }
    
            return Equals( (RuntimeMethodHandleImpl)obj );
        }

        public bool Equals( RuntimeMethodHandleImpl handle )
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
