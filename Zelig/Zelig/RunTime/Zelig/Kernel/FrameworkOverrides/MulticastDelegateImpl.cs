//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    [ExtendClass(typeof(System.MulticastDelegate))]
    public class MulticastDelegateImpl : DelegateImpl
    {
        //
        // State
        //

        [TS.WellKnownField( "MulticastDelegateImpl_m_invocationList" )]
        internal DelegateImpl[] m_invocationList;

        //
        // Constructor Methods
        //

        [MergeWithTargetImplementation]
        [TS.WellKnownMethod( "MulticastDelegateImpl_MulticastDelegateImpl" )]
        internal MulticastDelegateImpl( Object         target  ,
                                        TS.CodePointer codePtr ) : base( target, codePtr )
        {
        }

        //--//

        // equals returns true IIF the delegate is not null and has the
        //    same target, method and invocation list as this object
        public override bool Equals( Object obj )
        {
            if(obj == null || !InternalEqualTypes( this, obj ))
            {
                return false;
            }

            MulticastDelegateImpl d = obj as MulticastDelegateImpl;
            if(d == null)
            {
                return false;
            }

            if(ArrayUtility.ArrayEquals( m_invocationList, d.m_invocationList ) == false)
            {
                return false;
            }

            // now we can call on the base
            return base.Equals( d );
        }

        public override int GetHashCode()
        {
            DelegateImpl[] invocationList = m_invocationList;
            if(invocationList == null)
            {
                return base.GetHashCode();
            }
            else
            {
                int hash = 0;

                foreach(DelegateImpl d in invocationList)
                {
                    hash = hash * 33 + d.GetHashCode();
                }

                return hash;
            }
        }

        //--//

        // This method will combine this delegate with the passed delegate to form a new delegate.
        protected override sealed DelegateImpl CombineImpl( DelegateImpl follow )
        {
            MulticastDelegateImpl dFollow = follow as MulticastDelegateImpl;
            if(dFollow == null)
            {
                return this;
            }

            DelegateImpl[] thisList   = this   .m_invocationList;
            DelegateImpl[] followList = dFollow.m_invocationList;
            int            thisLen    = thisList   != null ? thisList  .Length : 1;
            int            followLen  = followList != null ? followList.Length : 1;
            DelegateImpl[] res        = new DelegateImpl[thisLen + followLen];

            if(thisList != null)
            {
                Array.Copy( thisList, 0, res, 0, thisLen );
            }
            else
            {
                res[0] = this;
            }

            if(followList != null)
            {
                Array.Copy( followList, 0, res, thisLen, followLen );
            }
            else
            {
                res[thisLen] = dFollow;
            }

            return NewMulticastDelegate( res );
        }

        // This method currently looks backward on the invocation list
        //    for an element that has Delegate based equality with value.  (Doesn't
        //    look at the invocation list.)  If this is found we remove it from
        //    this list and return a new delegate.  If its not found a copy of the
        //    current list is returned.
        protected override sealed DelegateImpl RemoveImpl( DelegateImpl value )
        {
            DelegateImpl[] invocationList = m_invocationList;

            if(invocationList == null)
            {
                if(this == value)
                {
                    //
                    // Special case: multicast with only one delegate in it => result is empty;
                    //
                    return null;
                }
            }
            else
            {
                for(int i = invocationList.Length; --i >= 0; )
                {
                    if(invocationList[i].Equals( value ))
                    {
                        if(invocationList.Length == 2)
                        {
                            //
                            // Special case: multicast with only two delegates in it => result is the other.
                            //
                            return invocationList[1-i];
                        }

                        DelegateImpl[] res = ArrayUtility.RemoveAtPositionFromNotNullArray( invocationList, i );

                        return NewMulticastDelegate( res );
                    }
                }
            }

            return this;
        }

        // This method returns the Invocation list of this multicast delegate.
        public DelegateImpl[] GetInvocationList()
        {
            if(m_invocationList != null)
            {
                return ArrayUtility.CopyNotNullArray( m_invocationList );
            }
            else
            {
                return new DelegateImpl[] { this };
            }
        }


        protected override Object GetTarget()
        {
            DelegateImpl dlg;

            if(m_invocationList == null)
            {
                dlg = this;
            }
            else
            {
                DelegateImpl[] lst = m_invocationList;

                dlg = lst[lst.Length - 1];
            }

            return dlg.m_target;
        }

        protected override System.Reflection.MethodInfo GetMethodImpl()
        {
            DelegateImpl dlg;

            if(m_invocationList == null)
            {
                dlg = this;
            }
            else
            {
                DelegateImpl[] lst = m_invocationList;

                dlg = lst[lst.Length - 1];
            }

            return dlg.InnerGetMethod();
        }

        //--//

        internal MulticastDelegateImpl NewMulticastDelegate( DelegateImpl[] invocationList )
        {
            // First, allocate a new multicast delegate just like this one, i.e. same type as the this object
            MulticastDelegateImpl result = (MulticastDelegateImpl)this.MemberwiseClone();

            result.m_invocationList = invocationList;

            return result;
        }
    }
}
