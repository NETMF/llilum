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


    public partial class DebuggerMainForm : Form
    {
        private delegate void WorkerWorkItem();

        private delegate void WorkerWorkItemReply( Emulation.Hosting.Forms.HostingSite.ExecutionState state, WorkerWorkItem msg );

        //
        // Helper Methods
        //

        private void ExecuteInWorkerThread( Emulation.Hosting.Forms.HostingSite.ExecutionState state ,
                                            WorkerWorkItem                                     msg   )
        {
            if(state == Hst.Forms.HostingSite.ExecutionState.Running)
            {
                EnteringState_Running();
            }

            if(state != Hst.Forms.HostingSite.ExecutionState.Invalid)
            {
                UpdateExecutionState( state );
            }
            
            lock(m_processorWorkerRequests)
            {
                m_processorWorkerRequests.Enqueue( msg );
                m_processorWorkerSignal.Set();
            }
        }

        private void ExecuteInFormThread( WorkerWorkItem msg )
        {
            ExecuteInFormThread( Hst.Forms.HostingSite.ExecutionState.Invalid, msg );
        }

        private void ExecuteInFormThread( Emulation.Hosting.Forms.HostingSite.ExecutionState state ,
                                          WorkerWorkItem                                     msg   )
        {
            if(this.InvokeRequired)
            {
                WorkerWorkItemReply dlg = ReplyFromWorker;

                this.BeginInvoke( dlg, state, msg );
            }
            else
            {
                ReplyFromWorker( state, msg );
            }
        }

        private void ReplyFromWorker( Emulation.Hosting.Forms.HostingSite.ExecutionState state ,
                                      WorkerWorkItem                                     msg   )
        {
            if(msg != null)
            {
                msg();
            }

            if(state != Hst.Forms.HostingSite.ExecutionState.Invalid)
            {
                UpdateExecutionState( state );
            }

            if(m_fFullyInitialized == false)
            {
                m_fFullyInitialized = true;

                FullyInitialized();
            }
        }

        private void ProcessorWorker()
        {
            ExecuteInFormThread( Hst.Forms.HostingSite.ExecutionState.Idle, null );

            while(true)
            {
                m_processorWorkerSignal.WaitOne();

                while(true)
                {
                    WorkerWorkItem msg;

                    lock(this)
                    {
                        if(m_processorWorkerRequests.Count == 0)
                        {
                            break;
                        }

                        msg = m_processorWorkerRequests.Dequeue();
                    }

                    msg();

                    if(m_processorWorkerExit)
                    {
                        try
                        {
                            Emulation.Hosting.ProcessorControl svcPC;

                            if(m_processorHost.GetHostingService( out svcPC ))
                            {
                                svcPC.Shutdown();
                            }
                        }
                        catch
                        {
                        }

                        return;
                    }
                }
            }
        }
    }
}