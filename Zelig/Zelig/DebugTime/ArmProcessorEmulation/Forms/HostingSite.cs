//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting.Forms
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;

    using Cfg = Microsoft.Zelig.Configuration.Environment;


    public abstract class HostingSite
    {
        public enum PublicationMode
        {
            View  ,
            Window,
            Tools ,
        }

        //--//

        public enum ExecutionState
        {
            Invalid  ,
            Waiting  ,
            Loading  ,
            Deploying,
            Idle     ,
            Loaded   ,
            Running  ,
            Paused   ,
        }

        public enum NotificationResponse
        {
            DoNothing                 ,
            RemoveFromNotificationList,
        }

        public delegate NotificationResponse ExecutionStateChangeNotification( HostingSite host, ExecutionState oldState, ExecutionState newState );

        //--//

        public enum VisualizationEvent
        {
            NewStackFrame    ,
            BreakpointsChange,
        }

        public delegate NotificationResponse VisualizationEventNotification( HostingSite host, VisualizationEvent e );

        //--//

        //
        // State
        //

        protected List< ExecutionStateChangeNotification > m_notifyExecutionStateChange = new List< ExecutionStateChangeNotification >();
        protected List< VisualizationEventNotification   > m_notifyVisualizationEvent   = new List< VisualizationEventNotification   >();

        //
        // Helper Methods
        //

        public abstract void ProcessKeyDownEvent( KeyEventArgs e );

        public abstract void ProcessKeyUpEvent( KeyEventArgs e );

        public abstract void ReportFormStatus( BaseDebuggerForm form    ,
                                               bool             fOpened );

        public abstract void RegisterView( BaseDebuggerForm form ,
                                           PublicationMode  mode );

        public abstract void UnregisterView( BaseDebuggerForm form ,
                                             PublicationMode  mode );

        //--//

        public abstract void VisualizeDebugInfo( Debugging.DebugInfo di );

        //--//
        
        public bool GetHostingService< T >( out T service )
        {
            service = (T)GetHostingService( typeof(T) );

            return service != null;
        }

        public abstract object GetHostingService( Type t );

        public abstract void RegisterService( Type   type ,
                                              object impl );


        public abstract void UnregisterService( object impl );

        //--//

        public void RaiseNotification( ExecutionState oldState ,
                                       ExecutionState newState )
        {
            foreach(var dlg in m_notifyExecutionStateChange.ToArray())
            {
                switch(dlg( this, oldState, newState ))
                {
                    case NotificationResponse.RemoveFromNotificationList:
                        m_notifyExecutionStateChange.Remove( dlg );
                        break;
                }
            }
        }

        public void RaiseNotification( VisualizationEvent e )
        {
            foreach(var dlg in m_notifyVisualizationEvent.ToArray())
            {
                switch(dlg( this, e ))
                {
                    case NotificationResponse.RemoveFromNotificationList:
                        m_notifyVisualizationEvent.Remove( dlg );
                        break;
                }
            }
        }

        //
        // Access Methods
        // 

        public abstract bool IsIdle
        {
            get;
        }

        public event ExecutionStateChangeNotification NotifyOnExecutionStateChange
        {
            add
            {
                if(m_notifyExecutionStateChange.Contains( value ) == false)
                {
                    m_notifyExecutionStateChange.Add( value );
                }
            }

            remove
            {
                m_notifyExecutionStateChange.Remove( value );
            }
        }

        public event VisualizationEventNotification NotifyOnVisualizationEvent
        {
            add
            {
                if(m_notifyVisualizationEvent.Contains( value ) == false)
                {
                    m_notifyVisualizationEvent.Add( value );
                }
            }

            remove
            {
                m_notifyVisualizationEvent.Remove( value );
            }
        }
    }
}
