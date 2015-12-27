//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.CmsisRtos
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.Zelig.Runtime;
    
    using RT   = Microsoft.Zelig.Runtime;
    using TS   = Microsoft.Zelig.Runtime.TypeSystem;
    using LLOS = Microsoft.Zelig.LlilumOSAbstraction;


    internal class CmsisObject : IDisposable
    {

        //
        // State
        //

        private static RT.KernelList<CmsisObject> s_objects = new RT.KernelList<CmsisObject>();
        private static object                     s_sync    = new object();

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
            lock(s_sync)
            {
                s_objects.InsertAtTail( obj.RegistrationLink );
            }
        }

        internal static void Deregister( CmsisObject obj )
        {
            lock(s_sync)
            {
                obj.RegistrationLink.RemoveFromList( ); 
            }
        }

        public void Dispose( )
        {
            Deregister( this );
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
    }
}
