//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;

    using Cfg = Microsoft.Zelig.Configuration.Environment;


    public abstract class AbstractHost
    {
        //
        // State
        //

        Dictionary< Type, object >             m_registeredServices = new Dictionary< Type, object >();
        List< AbstractHost >                   m_delegateServices   = new List< AbstractHost >();

        List< Hosting.AbstractHardwarePlugIn > m_plugIns            = new List< Hosting.AbstractHardwarePlugIn >();

        //
        // Helper Methods
        //

        public void RegisterService( Type   t    ,
                                     object impl )
        {
            m_registeredServices[t] = impl;
        }

        public void UnregisterService( object impl )
        {
            List< Type > lst = new List< Type >( m_registeredServices.Keys );

            foreach(Type t in lst)
            {
                object val;

                if(m_registeredServices.TryGetValue( t, out val ) && val == impl)
                {
                    m_registeredServices.Remove( t );
                }
            }
        }

        public bool GetHostingService< T >( out T service )
        {
            service = (T)GetHostingService( typeof(T) );

            return service != null;
        }

        public object GetHostingService( Type t )
        {
            object obj;

            if(m_registeredServices.TryGetValue( t, out obj ))
            {
                return obj;
            }

            foreach(AbstractHost host in m_delegateServices)
            {
                if(host.m_registeredServices.TryGetValue( t, out obj ))
                {
                    return obj;
                }
            }

            return null;
        }

        //--//

        protected void Link( object target )
        {
            AbstractHost host = target as AbstractHost;

            if(host != null)
            {
                if(m_delegateServices.Contains( host ) == false)
                {
                    host.m_delegateServices.Add( this );
                    this.m_delegateServices.Add( host );
                }
            }
        }

        protected void Unlink( object target )
        {
            AbstractHost host = target as AbstractHost;

            if(host != null)
            {
                host.m_delegateServices.Remove( this );
                this.m_delegateServices.Remove( host );
            }
        }

        //--//

        protected void StartPlugIns( Cfg.ProductCategory product       ,
                                     List< Type >        extraHandlers )
        {
            foreach(Type t in extraHandlers)
            {
                if(t.IsSubclassOf( typeof(Hosting.AbstractHardwarePlugIn)))
                {
                    Hosting.AbstractHardwarePlugIn plugIn = (Hosting.AbstractHardwarePlugIn)Activator.CreateInstance( t, this, product );

                    m_plugIns.Add( plugIn );

                    plugIn.Start();
                }
            }
        }

        protected void StopPlugIns()
        {
            foreach(Hosting.AbstractHardwarePlugIn plugIn in m_plugIns)
            {
                plugIn.Stop();
            }

            m_plugIns.Clear();
        }
    }
}
