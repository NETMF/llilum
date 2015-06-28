//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.Zelig.Emulation.Hosting.Forms
{
    public partial class BaseDebuggerForm : Form
    {
        //
        // State
        //

        private HostingSite m_site;

        //
        // Constructor Methods
        //

        private BaseDebuggerForm() // Unused contructor required by Visual Studio Designer
        {
        }

        public BaseDebuggerForm( HostingSite site )
        {
            m_site = site;

            InitializeComponent();
        }

        //
        // Helper Methods
        //

        protected virtual void NotifyChangeInVisibility( bool fVisible )
        {
        }

        protected virtual bool ProcessKeyDown( KeyEventArgs e )
        {
            return false;
        }

        protected virtual bool ProcessKeyUp( KeyEventArgs e )
        {
            return false;
        }

        //
        // Access Methods
        //

        protected HostingSite Host
        {
            get
            {
                return m_site;
            }
        }

        public virtual Image ViewImage
        {
            get
            {
                return null;
            }
        }

        public virtual string ViewTitle
        {
            get
            {
                return null;
            }
        }

        //
        // Event Methods
        //

        private void BaseDebuggerForm_FormClosing( object               sender ,
                                                   FormClosingEventArgs e      )
        {
            if(e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;

                this.Hide();
            }
        }

        private void BaseDebuggerForm_VisibleChanged( object    sender ,
                                                      EventArgs e      )
        {
            bool fVisible = this.Visible;

            NotifyChangeInVisibility( fVisible );

            m_site.ReportFormStatus( this, fVisible );
        }

        private void BaseDebuggerForm_KeyDown( object       sender ,
                                               KeyEventArgs e      )
        {
            if(ProcessKeyDown( e ) == false)
            {
                m_site.ProcessKeyDownEvent( e );
            }
        }

        private void BaseDebuggerForm_KeyUp( object       sender ,
                                             KeyEventArgs e      )
        {
            if(ProcessKeyUp( e ) == false)
            {
                m_site.ProcessKeyUpEvent( e );
            }
        }
    }
}
