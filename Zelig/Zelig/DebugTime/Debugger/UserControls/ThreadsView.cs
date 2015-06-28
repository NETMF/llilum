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

    using EncDef             = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition;
    using IR                 = Microsoft.Zelig.CodeGeneration.IR;
    using RT                 = Microsoft.Zelig.Runtime;
    using TS                 = Microsoft.Zelig.Runtime.TypeSystem;
    using Hst                = Microsoft.Zelig.Emulation.Hosting;


    public partial class ThreadsView : UserControl
    {
        //
        // State
        //

        DebuggerMainForm m_owner;

        Icon             m_icon_CurrentStatement = Properties.Resources.CurrentStatement;
        Icon             m_icon_StackFrame       = Properties.Resources.StackFrame;
        Icon             m_icon_EmptyIcon        = Properties.Resources.EmptyIcon;

        //
        // Constructor Methods
        //

        public ThreadsView()
        {
            InitializeComponent();
        }

        //
        // Helper Methods
        //

        public void Link( DebuggerMainForm owner )
        {
            m_owner = owner;

            m_owner.HostingSite.NotifyOnExecutionStateChange += ExecutionStateChangeNotification;
        }

        public Hst.Forms.HostingSite.NotificationResponse ExecutionStateChangeNotification( Hst.Forms.HostingSite                host     ,
                                                                                            Hst.Forms.HostingSite.ExecutionState oldState ,
                                                                                            Hst.Forms.HostingSite.ExecutionState newState )
        {
            dataGridView_Threads.SuspendLayout();

            DataGridViewRowCollection rows = dataGridView_Threads.Rows;

            dataGridView_Threads.Enabled = (m_owner.ActiveThread != null);

            if(m_owner.ActiveThread == null || host.IsIdle == false)
            {
                DebuggerMainForm.GrayOutRowsInDataGridView( rows );
            }
            else
            {
                rows.Clear();

                ulong total = 0;

                foreach(ThreadStatus ts in m_owner.Threads)
                {
                    total += ts.ActiveTime.TotalTime;
                }

                if(total == 0)
                {
                    total = 1;
                }

                foreach(ThreadStatus ts in m_owner.Threads)
                {
                    Icon   icon;
                    string kind;
                    string id;

                    if(ts == m_owner.SelectedThread)
                    {
                        icon = m_icon_CurrentStatement;
                    }
                    else if(ts == m_owner.ActiveThread)
                    {
                        icon = m_icon_StackFrame;
                    }
                    else
                    {
                        icon = m_icon_EmptyIcon;
                    }

                    if(ts.ThreadObject != null)
                    {
                        id = string.Format( ts.ManagedThreadId != -1 ? "{0} [0x{1:X8}]" : "0x{1:X8}", ts.ManagedThreadId, ts.ThreadObject.Address );
                    }
                    else
                    {
                        id = string.Format( "{0}", ts.ManagedThreadId );
                    }

                    switch(ts.ThreadKind)
                    {
                        case ThreadStatus.Kind.Bootstrap          : kind = "<Bootstrap> "; break;
                        case ThreadStatus.Kind.InterruptThread    : kind = "<IRQ> "      ; break;
                        case ThreadStatus.Kind.FastInterruptThread: kind = "<FIQ> "      ; break;
                        case ThreadStatus.Kind.IdleThread         : kind = "<Idle> "     ; break;
                        default                                   : kind = ""            ; break;
                    }

                    if(ts.TopMethod != null)
                    {
                        kind += ts.TopMethod.ToShortString();
                    }

                    double cpuTime = 100.0 * ts.ActiveTime.TotalTime / total;

                    int rowNum = rows.Add( icon, id, cpuTime.ToString( "###.##" ), ts.ActiveTime.Hits.ToString(), kind );

                    var row = rows[rowNum];

                    row.Tag                  = ts;
                    row.Cells[4].ToolTipText = ts.ToString();
                }
            }

            dataGridView_Threads.ResumeLayout();

            return Hst.Forms.HostingSite.NotificationResponse.DoNothing;
        }

        //
        // Access Methods
        //

        //
        // Event Methods
        //

        private void dataGridView_Threads_CellContentClick( object                    sender ,
                                                            DataGridViewCellEventArgs e      )
        {
            var rows = dataGridView_Threads.Rows;

            if(e.RowIndex >= 0 && e.RowIndex < rows.Count)
            {
                var ts = rows[e.RowIndex].Tag as ThreadStatus;
                if(ts != null)
                {
                    m_owner.Action_SelectThread( m_owner.Threads.IndexOf( ts ) );
                }
            }
        }
    }
}
