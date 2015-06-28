//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.IO;
    using System.Windows.Forms;
    using System.Threading;

    using IR  = Microsoft.Zelig.CodeGeneration.IR;
    using RT  = Microsoft.Zelig.Runtime;
    using TS  = Microsoft.Zelig.Runtime.TypeSystem;
    using Cfg = Microsoft.Zelig.Configuration.Environment;

    
    public partial class SessionManagerForm : Form
    {
        const string c_registry_RootKey     = "SessionManager";
        const string c_registry_LastSession = "LastSession";

        const string c_file_Sessions        = "Sessions";
        const string c_file_Extension       = "zeligsession";
        const string c_file_SearchPattern   = "*." + c_file_Extension;

        //
        // State
        //

        DebuggerMainForm m_owner;
        List< Session >  m_sessions;
        Session          m_defaultSession;
        Session          m_selectedSession;

        //
        // Constructor Methods
        //

        public SessionManagerForm( DebuggerMainForm owner )
        {
            m_owner = owner;

            InitializeComponent();

            //--//

            m_sessions = new List< Session >();

            LoadSessions();
        }

        //
        // Helper Methods
        //

        private DirectoryInfo GetDataPath()
        {
            return new DirectoryInfo( Path.Combine( Application.UserAppDataPath, c_file_Sessions ) );
        }

        public void SaveSessions()
        {
            Win32.RegistryKey rootKey = Application.UserAppDataRegistry;

            Win32.RegistryKey nodeKey = rootKey.CreateSubKey( c_registry_RootKey );
            if(nodeKey != null)
            {
                if(m_defaultSession != null && m_defaultSession.IsTemporary == false)
                {
                    nodeKey.SetValue( c_registry_LastSession, m_defaultSession.Id.ToString(), Microsoft.Win32.RegistryValueKind.String );
                }

                nodeKey.Close();
            }

            var di = GetDataPath();
            if(!di.Exists)
            {
                di.Create();
            }

            foreach(Session session in m_sessions)
            {
                if(session.IsTemporary == false)
                {
                    if(session.Dirty)
                    {
                        if(session.SettingsFile != null)
                        {
                            if(MessageBox.Show( string.Format( "Save session '{0}'?", session.SettingsFile ), "Workplace", MessageBoxButtons.YesNo ) == DialogResult.Yes)
                            {
                                try
                                {
                                    session.Save( session.SettingsFile, false );
                                }
                                catch
                                {
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                string file = Path.Combine( di.FullName, string.Format( "{0}.{1}", session.Id, c_file_Extension ) );

                                session.Save( file, false );
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
        }

        private void LoadSessions()
        {
            string defaultSessionId = null;

            Win32.RegistryKey rootKey = Application.UserAppDataRegistry;
            Win32.RegistryKey nodeKey = rootKey.OpenSubKey( c_registry_RootKey );
            if(nodeKey != null)
            {
                defaultSessionId = nodeKey.GetValue( c_registry_LastSession, null, Microsoft.Win32.RegistryValueOptions.DoNotExpandEnvironmentNames ) as string;

                nodeKey.Close();
            }

            var di = GetDataPath();
            if(di.Exists)
            {
                foreach(var fi in di.GetFiles( c_file_SearchPattern ))
                {
                    try
                    {
                        AddSession( Session.Load( fi.FullName ), defaultSessionId );
                    }
                    catch
                    {
                    }
                }
            }
        }

        public Session LoadSession( string file )
        {
            Session session = Session.LoadAndSetOrigin( file );

            InsertUniqueSession( session );

            return session;
        }

        public void SelectSession( Session session )
        {
            m_selectedSession = session;

            UpdateList();
        }

        public Session SelectSession( bool fSetDefault )
        {
            if(m_sessions.Count > 0)
            {
                if(fSetDefault)
                {
                    SelectSession( m_defaultSession );
                }

                if(this.ShowDialog() != DialogResult.OK)
                {
                    return null;
                }
            }
            else
            {
                Session session = new Session();

                InputForm form = new InputForm( "Enter Name For New Session", session.DisplayName );

                if(form.ShowDialog() == DialogResult.OK)
                {
                    session.DisplayName = form.Result;

                    if(m_owner.Action_EditConfiguration( session ) == DialogResult.OK)
                    {
                        InsertUniqueSession( session );

                        SelectSession( session );
                    }
                }
            }

            m_defaultSession = m_selectedSession;

            return m_defaultSession;
        }

        public Session FindSession( string name )
        {
            foreach(Session session in m_sessions)
            {
                if(session.DisplayName == name)
                {
                    return session;
                }
            }

            return null;
        }

        public Session LoadSession( string file       ,
                                    bool   fTemporary )
        {
            Session session = Session.LoadAndSetOrigin( file );

            session.IsTemporary = fTemporary;

            InsertUniqueSession( session );

            return session;
        }

        //--//

        private void AddSession( Session session          ,
                                 string  defaultSessionId )
        {
            InsertUniqueSession( session );

            if(session.Id.ToString() == defaultSessionId)
            {
                m_defaultSession = session;
            }
        }

        private void InsertUniqueSession( Session newSession )
        {
            for(int i = 0; i < m_sessions.Count; i++)
            {
                Session session = m_sessions[i];

                if(session.Id == newSession.Id)
                {
                    m_sessions[i] = newSession;
                    return;
                }
            }

            m_sessions.Add( newSession );
        }

        private void UpdateList()
        {
            ListView.ListViewItemCollection items        = listView1.Items;
            ListViewItem                    itemToSelect = null;

            items.Clear();

            foreach(Session session in m_sessions)
            {
                string name = session.DisplayName;

                if(string.IsNullOrEmpty( name ))
                {
                    name = "<unnamed>";
                }

                var item = new ListViewItem( name );
    
                item.Tag = session;

                item.SubItems.Add( session.SelectedEngine .ToString() );
                item.SubItems.Add( session.SelectedProduct.ToString() );
                item.SubItems.Add( session.LastModified   .ToString() );
                item.SubItems.Add( session.ImageToLoad                );
    
                items.Add( item );

                if(m_selectedSession == session)
                {
                    itemToSelect = item;
                }
            }

            if(itemToSelect != null)
            {
                itemToSelect.Selected = true;
                listView1.Focus();
            }

            UpdateButtons();
        }

        private void UpdateButtons()
        {
            bool fEnable = (m_selectedSession != null);

            buttonSelect.Enabled = fEnable;
            buttonClone .Enabled = fEnable;
            buttonRemove.Enabled = fEnable;
            buttonSave  .Enabled = fEnable;
        }

        //
        // Access Methods
        //

        //
        // Event Methods
        //

        private void SessionManagerForm_Load( object    sender ,
                                              EventArgs e      )
        {
            UpdateList();
        }

        private void buttonSelect_Click( object    sender ,
                                         EventArgs e      )
        {
            this.DialogResult = DialogResult.OK;

            this.Close();
        }

        private void buttonNew_Click( object    sender ,
                                      EventArgs e      )
        {
            Session session = new Session();

            InputForm form = new InputForm( "Enter Name For Session", session.DisplayName );

            if(form.ShowDialog() == DialogResult.OK)
            {
                session.DisplayName = form.Result;

                if(m_owner.Action_EditConfiguration( session ) == DialogResult.OK)
                {
                    InsertUniqueSession( session );

                    SelectSession( session );
                }
            }
        }


        private void buttonRename_Click( object    sender ,
                                         EventArgs e      )
        {
            InputForm form = new InputForm( "Enter Name For Session", m_selectedSession.DisplayName );

            if(form.ShowDialog() == DialogResult.OK)
            {
                m_selectedSession.DisplayName = form.Result;

                UpdateList();
            }
        }

        private void buttonClone_Click( object    sender ,
                                        EventArgs e      )
        {
            Session session = new Session( m_selectedSession );

            InsertUniqueSession( session );

            SelectSession( session );
        }

        private void buttonRemove_Click( object    sender ,
                                         EventArgs e      )
        {
            var session = m_selectedSession;

            if(session != null)
            {
                if(session.SettingsFile != null)
                {
                    if(MessageBox.Show( string.Format( "Delete session file '{0}'?", session.SettingsFile ), "Workplace", MessageBoxButtons.YesNo ) == DialogResult.Yes)
                    {
                        try
                        {
                            File.Delete( session.SettingsFile );
                        }
                        catch
                        {
                        }
                    }
                }

                try
                {
                    var di = GetDataPath();
                    if(di.Exists)
                    {
                        string file = Path.Combine( di.FullName, string.Format( "{0}.{1}", session.Id, c_file_Extension ) );

                        File.Delete( file );
                    }
                }
                catch
                {
                }

                //--//

                m_sessions.Remove( session );
                m_selectedSession = null;
            }

            UpdateList();
        }

        private void buttonSave_Click( object    sender ,
                                       EventArgs e      )
        {
            if(m_selectedSession.Dirty)
            {
                string file = m_owner.Action_SelectSessionToSave( m_selectedSession.SettingsFile );
                if(file != null)
                {
                    m_selectedSession.Save( file, false );

                    UpdateList();
                }
            }
        }

        private void buttonExport_Click( object sender, EventArgs e )
        {
            if(m_selectedSession != null)
            {
                string file = m_owner.Action_SelectSessionToSave( null );
                if(file != null)
                {
                    m_selectedSession.Save( file, false );

                    UpdateList();
                }
            }
        }

        private void buttonBrowse_Click( object    sender ,
                                         EventArgs e      )
        {
            string file = m_owner.Action_SelectSessionToLoad( null );
            if(file != null)
            {
                SelectSession( LoadSession( file ) );
            }
        }

        private void buttonCancel_Click( object    sender ,
                                         EventArgs e      )
        {
            this.DialogResult = DialogResult.Cancel;

            this.Close();
        }

        //--//

        private void listView1_ItemSelectionChanged( object                                sender ,
                                                     ListViewItemSelectionChangedEventArgs e      )
        {
            if(e.IsSelected)
            {
                m_selectedSession = (Session)e.Item.Tag;
            }
            else
            {
                m_selectedSession = null;
            }

            UpdateButtons();
        }

        private void listView1_DoubleClick( object    sender ,
                                            EventArgs e      )
        {
            if(m_selectedSession != null)
            {
                this.DialogResult = DialogResult.OK;

                this.Close();
            }
        }
    }
}