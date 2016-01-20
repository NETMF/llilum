//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.CmsisRtos
{
    using System;
    using Microsoft.Zelig.Runtime;

    using RT = Microsoft.Zelig.Runtime;


    internal class CmsisObject : IDisposable
    {

        //
        // State
        //

        private static RT.KernelList<CmsisObject> s_objects = new RT.KernelList<CmsisObject>();

        //--//

        private readonly RT.KernelNode<CmsisObject> m_registrationLink;

        //--//

        //
        // Contructors
        //

        protected CmsisObject()
        {
            RT.BugCheck.Assert( RT.TargetPlatform.ARMv7.ProcessorARMv7M.IsAnyExceptionActive() == false, BugCheck.StopCode.IllegalMode );

            m_registrationLink = new RT.KernelNode<CmsisObject>( this );

            Register( this );
        }

        internal static void Register( CmsisObject obj )
        {
            using(RT.SmartHandles.InterruptState.DisableAll())
            {
                s_objects.InsertAtTail( obj.RegistrationLink );
            }
        }

        internal static void Deregister( CmsisObject obj )
        {
            using(RT.SmartHandles.InterruptState.DisableAll())
            {
                obj.RegistrationLink.RemoveFromList( ); 
            }
        }

        protected virtual void Dispose( bool disposing )
        {
            if(disposing)
            {
                Deregister( this );
            }
        }

        ~CmsisObject( )
        {
            Dispose( false );
        }
        
        public void Dispose( )
        { 
            Dispose( true );

            GC.SuppressFinalize(this);
        }

        //
        // Helper methods 
        // 

        public static CmsisObject GetObject( UIntPtr ptr )
        {
            using(RT.SmartHandles.InterruptState.DisableAll( ))
            {
                RT.KernelNode< CmsisObject > node = s_objects.StartOfForwardWalk;

                while(node.IsValidForForwardMove)
                {
                    var node2 = (ObjectImpl)(object)node.Target;

                    if(node2.ToPointer( ) == ptr)
                    {
                        break;
                    }

                    node = node.Next;
                }

                if(node.IsValidForForwardMove)
                {
                    return node.Target;
                }
            }

            return null;
        }

        public static CmsisObject FindObject<T>( object cmp ) where T: CmsisObject
        {
            using(RT.SmartHandles.InterruptState.DisableAll( ))
            {
                RT.KernelNode< CmsisObject > node = s_objects.StartOfForwardWalk;

                while(node.IsValidForForwardMove)
                {
                    CmsisObject obj = (CmsisObject)node.Target;

                    if(obj is T)
                    {
                        if(( (T)obj ).SameObject( cmp ))
                        {
                            return obj;
                        }
                    }

                    node = node.Next;
                }
            }

            return null;
        }

        //
        // Access Methods
        // 

        protected RT.KernelNode<CmsisObject> RegistrationLink
        {
            get
            {
                return m_registrationLink;
            }
        }

        protected virtual bool SameObject( object cmp )
        {
            return this == cmp;
        }
    }
}
