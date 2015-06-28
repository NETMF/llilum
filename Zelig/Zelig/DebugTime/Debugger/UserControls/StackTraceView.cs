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
    using Hst = Microsoft.Zelig.Emulation.Hosting;


    public partial class StackTraceView : UserControl
    {
        //
        // State
        //

        DebuggerMainForm m_owner;
        int              m_version;

        Icon             m_icon_CurrentStatement = Properties.Resources.CurrentStatement;
        Icon             m_icon_StackFrame       = Properties.Resources.StackFrame;
        Icon             m_icon_EmptyIcon        = Properties.Resources.EmptyIcon;

        //
        // Constructor Methods
        //

        public StackTraceView()
        {
            InitializeComponent();
        }

        //--//

        public void Link( DebuggerMainForm owner )
        {
            m_owner = owner;

            m_owner.HostingSite.NotifyOnExecutionStateChange += delegate( Hst.Forms.HostingSite                host     ,
                                                                          Hst.Forms.HostingSite.ExecutionState oldState ,
                                                                          Hst.Forms.HostingSite.ExecutionState newState )
            {
                UpdateUI();

                return Hst.Forms.HostingSite.NotificationResponse.DoNothing;
            };

            m_owner.HostingSite.NotifyOnVisualizationEvent += delegate( Hst.Forms.HostingSite                    host ,
                                                                        Hst.Forms.HostingSite.VisualizationEvent e    )
            {
                if(e == Hst.Forms.HostingSite.VisualizationEvent.NewStackFrame)
                {
                    UpdateUI();
                }

                return Hst.Forms.HostingSite.NotificationResponse.DoNothing;
            };
        }

        private void UpdateUI()
        {
            StackFrame currentStackFrame = m_owner.SelectedStackFrame;

            //--//

            dataGridView_StackTrace.SuspendLayout();

            DataGridViewRowCollection rows = dataGridView_StackTrace.Rows;

            dataGridView_StackTrace.Enabled = (currentStackFrame != null);

            if(currentStackFrame == null || m_owner.IsIdle == false)
            {
                m_version = -1;

                DebuggerMainForm.GrayOutRowsInDataGridView( rows );
            }
            else
            {
                bool fRebuild = (m_version != m_owner.VersionStackTrace);

                if(fRebuild)
                {
                    m_version = m_owner.VersionStackTrace;

                    rows.Clear();
                }

                foreach(StackFrame sf in m_owner.SelectedThread.StackTrace)
                {
                    Icon icon;

                    if(sf.Depth == 0)
                    {
                        icon = m_icon_CurrentStatement;
                    }
                    else if(sf == m_owner.SelectedStackFrame)
                    {
                        icon = m_icon_StackFrame;
                    }
                    else
                    {
                        icon = m_icon_EmptyIcon;
                    }

                    if(fRebuild)
                    {
                        var cm = sf.CodeMapOfTarget;

                        if(cm != null)
                        {
                            if(0 != ( cm.Target.BuildTimeFlags & TS.MethodRepresentation.BuildTimeAttributes.Imported ))
                            {
                                string method = sf.DebugInfo.MethodName;

                                if(string.IsNullOrEmpty( method ))
                                {
                                    method = cm.Target.ToShortString();
                                }

                                rows.Add( icon, method, sf.Depth + 1 );
                            }
                            else
                            {
                                rows.Add( icon, cm.Target.ToShortString(), sf.Depth + 1 );
                            }
                        }
                        else
                        {
                            rows.Add( icon, string.Format( "0x{0:X8}", sf.ProgramCounter ), sf.Depth + 1 );
                        }
                    }
                    else
                    {
                        rows[sf.Depth].Cells[0].Value = icon;
                    }
                }
            }

            dataGridView_StackTrace.ResumeLayout();
        }

        //
        // Access Methods
        //

        //
        // Event Methods
        //

        private void dataGridView_StackTrace_CellContentClick( object sender, DataGridViewCellEventArgs e )
        {
            m_owner.Action_SelectStackFrame( e.RowIndex );
        }
    }
}
