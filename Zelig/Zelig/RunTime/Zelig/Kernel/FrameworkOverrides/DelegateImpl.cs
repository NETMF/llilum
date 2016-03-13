//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.Delegate))]
    public class DelegateImpl
    {
        //
        // State
        //

        // m_target is the object we will invoke on
        [TS.WellKnownField( "DelegateImpl_m_target" )]
        internal Object                  m_target;

        // m_codePtr is a pointer to the method we will invoke
        [TS.WellKnownField( "DelegateImpl_m_codePtr" )]
        internal TS.CodePointer          m_codePtr;

        //
        // Constructor Methods
        //

        [MergeWithTargetImplementation]
        internal DelegateImpl( Object         target  ,
                               TS.CodePointer codePtr )
        {
            m_target  = target;
            m_codePtr = codePtr;
        }

        //--//

        public override bool Equals( Object obj )
        {
            if(obj == null || !InternalEqualTypes( this, obj ))
            {
                return false;
            }

            DelegateImpl d = obj as DelegateImpl;
            if(d == null)
            {
                return false;
            }

            return (m_target == d.m_target && m_codePtr.Target == d.m_codePtr.Target);
        }

        public override int GetHashCode()
        {
            return m_codePtr.Target.GetHashCode();
        }

        //--//

        public static bool operator ==( DelegateImpl d1 ,
                                        DelegateImpl d2 )
        {
            if((Object)d1 == null)
            {
                return (Object)d2 == null;
            }

            return d1.Equals( d2 );
        }

        public static bool operator !=( DelegateImpl d1 ,
                                        DelegateImpl d2 )
        {
            if((Object)d1 == null)
            {
                return (Object)d2 != null;
            }

            return !d1.Equals( d2 );
        }

        public static DelegateImpl Combine( DelegateImpl a, DelegateImpl b )
        {
            // boundary conditions -- if either (or both) delegates is null return the other.
            if((Object)a == null) // cast to object for a more efficient test
            {
                return b;
            }

            if((Object)b == null) // cast to object for a more efficient test
            {
                return a;
            }

            if(!InternalEqualTypes( a, b ))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( "Arg_DlgtTypeMis" );
#else
                throw new ArgumentException();
#endif
            }

            return a.CombineImpl( b );
        }

        public static DelegateImpl Remove( DelegateImpl source, DelegateImpl value )
        {
            if(source == null)
            {
                return null;
            }

            if(value == null)
            {
                return source;
            }

            if(!InternalEqualTypes( source, value ))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( "Arg_DlgtTypeMis" );
#else
                throw new ArgumentException();
#endif
            }

            return source.RemoveImpl( value );
        }

        //--//

        protected virtual Object GetTarget()
        {
            return m_target;
        }

        protected virtual System.Reflection.MethodInfo GetMethodImpl()
        {
            return TypeSystemManager.CodePointerToMethodInfo( m_codePtr );
        }

        protected virtual DelegateImpl CombineImpl( DelegateImpl d )
        {
#if EXCEPTION_STRINGS
            throw new MulticastNotSupportedException( "Multicast_Combine" );
#else
            throw new MulticastNotSupportedException();
#endif
        }

        protected virtual DelegateImpl RemoveImpl( DelegateImpl d )
        {
            return d.Equals( this ) ? null : this;
        }

        //--//

        public TS.CodePointer InnerGetCodePointer()
        {
            return m_codePtr;
        }

        internal System.Reflection.MethodInfo InnerGetMethod()
        {
            return TypeSystemManager.CodePointerToMethodInfo( m_codePtr );
        }

        [Inline]
        protected static bool InternalEqualTypes( object a ,
                                                  object b )
        {
            return TS.VTable.SameType( a, b );
        }
    }
}
