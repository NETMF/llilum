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
    using Hst = Microsoft.Zelig.Emulation.Hosting;
    using Microsoft.Zelig.TargetModel.ArmProcessor;
    using Microsoft.Zelig.CodeGeneration.IR.Abstractions;


    public partial class DebuggerMainForm : Form
    {
        //
        // Helper Methods
        //

        private void InnerAction_StopExecution()
        {
            Emulation.Hosting.ProcessorControl svcPC;

            if(m_processorHost.GetHostingService( out svcPC ))
            {
                svcPC.StopExecution = true;
            }
        }

        //--//

        private void InnerAction_SaveSession( string file )
        {
            m_currentSession.Save( file, true );
        }

        private void InnerAction_LoadSession( string file )
        {
            m_currentSession = m_sessionManagerForm.LoadSession( file );

            InnerAction_LoadImage();
        }

        //--//

        private bool InnerAction_LoadImage()
        {
            if(m_currentSession == null)
            {
                return false;
            }

            if(string.IsNullOrEmpty( m_currentSession.ImageToLoad ))
            {
                return false;
            }

            InnerAction_SynchronousStopExecution();

            string file = m_currentSession.ImageToLoad;

            ExecuteInWorkerThread( Hst.Forms.HostingSite.ExecutionState.Loading, delegate()
            {
                try
                {
                    using(m_processorHost.SuspendMemoryDeltaUpdates())
                    {
                        Emulation.Hosting.Breakpoint[] oldBreakpoints = null;

                        if(m_imageInformation != null)
                        {
                            if(file == m_imageInformation.ImageFile)
                            {
                                oldBreakpoints = m_processorHost.Breakpoints;
                            }
                            else
                            {
                                m_imageInformation = null;
                            }
                        }

                        if(m_imageInformation == null)
                        {
                            if(file.EndsWith( ".axfdump" ))
                            {
                                m_imageInformation = ImageInformation.LoadAdsImage( file );
                            }
                            else if(file.EndsWith( ".hex" ))
                            {
                                m_imageInformation = ImageInformation.LoadHexImage( file );
                            }
                            else
                            {
                                m_imageInformation = ImageInformation.LoadZeligImage( file, delegate( string format, object[] args )
                                {
                                    ExecuteInFormThread( delegate()
                                    {
                                        toolStripStatus_ExecutionState.Text = string.Format( format, args );
                                    } );
                                } );
                            }
                        }

                        m_processorHost.RegisterService( typeof(ImageInformation), m_imageInformation );

                        //--//

                        ExecuteInFormThread( delegate()
                        {
                            toolStripStatus_ExecutionState.Text = "Connecting to target...";
                        } );

                        InstructionSet iset = m_imageInformation.ImageBuilder.TypeSystem.PlatformAbstraction.GetInstructionSetProvider();

                        m_processorHost.SelectEngine(m_currentSession.SelectedEngine, iset);

                        m_imageInformation.ApplyToProcessorHost( m_processorHost, m_currentSession.SelectedProduct );

                        ExecuteInFormThread( Hst.Forms.HostingSite.ExecutionState.Deploying, null );

                        m_imageInformation.DeployImage( m_processorHost, delegate( string format, object[] args )
                        {
                            ExecuteInFormThread( delegate()
                            {
                                toolStripStatus_ExecutionState.Text = string.Format( format, args );
                            } );
                        } );

                        m_imageInformation.InitializePlugIns( m_processorHost, m_arguments.m_handlers );

                        if(DebugGC)
                        {
                            m_debugGC = new DebugGarbageColllection( new MemoryDelta( m_imageInformation, m_processorHost ), DebugGCVerbose );
                        }

                        ExecuteInFormThread( Hst.Forms.HostingSite.ExecutionState.Loaded, delegate()
                        {
                            if(oldBreakpoints != null)
                            {
                                foreach(Emulation.Hosting.Breakpoint bp in oldBreakpoints)
                                {
                                    m_processorHost.RestoreBreakpoint( bp );
                                }
                            }

                            if(m_imageInformation.TypeSystem != null)
                            {
                                //AddBreakpointOnDebugMethod( "TypeSystemManager_Throw"                 , null );
                                AddBreakpointOnDebugMethod( "TypeSystemManager_Rethrow__Exception"    , null );
                                AddBreakpointOnDebugMethod( "ThreadImpl_ThrowNullException"           , null );
                                AddBreakpointOnDebugMethod( "ThreadImpl_ThrowIndexOutOfRangeException", null );
                                AddBreakpointOnDebugMethod( "ThreadImpl_ThrowOverflowException"       , null );
                                AddBreakpointOnDebugMethod( "ThreadImpl_ThrowNotImplementedException" , null );
                                AddBreakpointOnDebugMethod( "BugCheck_Raise"                          , null );

                                //
                                // To properly set soft breakpoints, we need to wait until all the code has been relocated to its final location.
                                // We set a temporary breakpoint on this method because it's the first program point where relocation is guaranteed to have occurred.
                                //
                                {
                                    var bp = AddBreakpointOnDebugMethod( "Bootstrap_Initialization", delegate( Emulation.Hosting.Breakpoint bpHit )
                                                                                                     {
                                                                                                         return Emulation.Hosting.Breakpoint.Response.NextInstruction;
                                                                                                     } );

                                    if(bp != null)
                                    {
                                        bp.ShouldImplementInHardware = true;
                                        bp.IsOptional                = false;
                                        bp.IsTemporary               = true;
                                    }
                                }

                                {
                                    var memDelta = new MemoryDelta  ( m_imageInformation, m_processorHost );
                                    var ih       = new InteropHelper( m_imageInformation, m_processorHost );

                                    ih.SetInteropOnWellKnownMethod( "BugCheck_WriteLine", false, delegate()
                                    {
                                        uint textAddress = ih.GetRegisterUInt32( Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition.c_register_r1 );

                                        memDelta.FlushCache();

                                        var text = m_imageInformation.GetStringFromMemory( memDelta, textAddress );
                                        if(text != null)
                                        {
                                            Hst.OutputSink svc;

                                            if(m_processorHost.GetHostingService( out svc ))
                                            {
                                                svc.OutputLine( text );
                                            }
                                        }

                                        return ih.SkipCall();
                                    } );
                                }
                            }

                            m_baseSample_clockTicks  = 0;
                            m_baseSample_nanoseconds = 0;
                            m_lastSample_clockTicks     = m_baseSample_clockTicks;
                            m_lastSample_nanoseconds    = m_baseSample_nanoseconds;

                            ExitingState_Running( null );
                        } );
                    }
                }
                catch(Emulation.Hosting.AbstractEngineException ex)
                {
                    ExecuteInFormThread( Hst.Forms.HostingSite.ExecutionState.Idle, delegate()
                    {
                        Hst.OutputSink svc;

                        if(m_processorHost.GetHostingService( out svc ))
                        {
                            svc.OutputLine( "Caught exception while loading {0}: {1}", file, ex );
                        }

                        MessageBox.Show( string.Format( "Exception caught while loading {0}:\r\n\r\nPhase:{1}\r\n\r\n{2}", file, ex.Reason, ex.Message ), "Image Load Error", MessageBoxButtons.OK );
                    } );
                }
                catch(Exception ex)
                {
                    ExecuteInFormThread( Hst.Forms.HostingSite.ExecutionState.Idle, delegate()
                    {
                        Hst.OutputSink svc;

                        if(m_processorHost.GetHostingService( out svc ))
                        {
                            svc.OutputLine( "Caught exception while loading {0}: {1}", file, ex );
                        }

                        MessageBox.Show( string.Format( "Exception caught while loading {0}:\r\n\r\n{1}", file, ex ), "Image Load Error", MessageBoxButtons.OK );
                    } );
                }
            } );

            return true;
        }

        //--//

        private void InnerAction_Execute( StackFrame startStackFrame      ,
                                          bool       fAssemblyGranularity ,
                                          bool       fStep                ,
                                          bool       fSkipMethodCalls     )
        {
            Emulation.Hosting.ProcessorControl svcPC; m_processorHost.GetHostingService( out svcPC );

            MemoryDelta memDelta = new MemoryDelta( m_imageInformation, m_processorHost );

            try
            {
                using(m_processorHost.SuspendMemoryDeltaUpdates())
                {
                    Debugging.DebugInfo diTarget = null;

                    if(fStep)
                    {
                        if(fAssemblyGranularity)
                        {
                            m_processorHost.ExecuteStep( m_imageInformation, m_currentSession.SelectedProduct );

                            if(fSkipMethodCalls)
                            {
                                memDelta.FlushCache();

                                ThreadStatus ts = ThreadStatus.GetCurrent( memDelta );

                                //
                                // Did we enter a new method? Then create a temporary breakpoint and run until we hit it.
                                //
                                if(DidWeFollowAMethodCall( startStackFrame, ts.StackTrace ))
                                {
                                    RunToAddress( ts.StackTrace[1].ProgramCounter );

                                    m_processorHost.Execute( m_imageInformation, m_currentSession.SelectedProduct );
                                }
                            }
                        }
                        else
                        {
                            bool fSingleStep    = true;
                            bool fHitBreakpoint = false;

                            while(true)
                            {
                                if(fSingleStep)
                                {
                                    m_processorHost.ExecuteStep( m_imageInformation, m_currentSession.SelectedProduct );
                                }
                                else
                                {
                                    m_processorHost.Execute( m_imageInformation, m_currentSession.SelectedProduct );
                                }

                                if(svcPC.StopExecution)
                                {
                                    if(fHitBreakpoint)
                                    {
                                        fHitBreakpoint      = false;
                                        svcPC.StopExecution = false;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                memDelta.FlushCache();

                                ThreadStatus ts = ThreadStatus.GetCurrent( memDelta );

                                if(ts.StackTrace.Count == 0)
                                {
                                    break;
                                }

                                StackFrame topStackFrame = ts.StackTrace[0];

                                if(startStackFrame != null)
                                {
                                    if(fSkipMethodCalls)
                                    {
                                        bool fMovedOneOpcode = (startStackFrame.ProgramCounter + sizeof(uint) == topStackFrame.ProgramCounter);

                                        if(fMovedOneOpcode == false && DidWeFollowAMethodCall( startStackFrame, ts.StackTrace ))
                                        {
                                            RunToAddress( ts.StackTrace[1].ProgramCounter, null, delegate( Emulation.Hosting.Breakpoint bp )
                                            {
                                                fHitBreakpoint = true;

                                                return Emulation.Hosting.Breakpoint.Response.StopExecution;
                                            } );

                                            fSingleStep = false;
                                            continue;
                                        }
                                    }

                                    if(startStackFrame.DebugInfo != topStackFrame.DebugInfo)
                                    {
                                        diTarget = topStackFrame.DebugInfo;
                                        break;
                                    }

                                    //
                                    // If the top stack frame has multiple DebugInfo associated with its current program counter,
                                    // pretend to execute the step and just update the UI.
                                    //
                                    if(topStackFrame.AdvanceToNextDebugInfo())
                                    {
                                        if(startStackFrame.DebugInfo != topStackFrame.DebugInfo)
                                        {
                                            diTarget = topStackFrame.DebugInfo;
                                            break;
                                        }
                                    }
                                }

                                startStackFrame = topStackFrame;
                                fSingleStep     = true;
                            }
                        }
                    }
                    else
                    {
                        m_processorHost.Execute( m_imageInformation, m_currentSession.SelectedProduct );
                    }

                    ExecuteInFormThread( Hst.Forms.HostingSite.ExecutionState.Paused, delegate()
                    {
                        ExitingState_Running( diTarget );
                    } );
                }
            }
            catch(Exception ex)
            {
                ExecuteInFormThread( Hst.Forms.HostingSite.ExecutionState.Paused, delegate()
                {
                    Hst.OutputSink svc;

                    if(m_processorHost.GetHostingService( out svc ))
                    {
                        svc.OutputLine( "Caught exception while executing code: {0}", ex );
                    }

                    //MessageBox.Show( string.Format( "Exception caught while executing code:\r\n{0}", ex ), "Image Load Error", MessageBoxButtons.OK );

                    ExitingState_Running( null );
                } );
            }

            svcPC.StopExecution = false;
        }

        private bool DidWeFollowAMethodCall( StackFrame         sf         ,
                                             List< StackFrame > stackTrace )
        {
            if(stackTrace.Count > 1)
            {
                StackFrame previousStackFrame = stackTrace[1];

                if(sf.CodeMapOfTarget == previousStackFrame.CodeMapOfTarget &&
                   sf.StackPointer    == previousStackFrame.StackPointer     )
                {
                    return true;
                }
            }

            return false;
        }

        private void RunToAddress( uint address )
        {
            RunToAddress( address, null, delegate( Emulation.Hosting.Breakpoint bp )
            {
                return Emulation.Hosting.Breakpoint.Response.StopExecution;
            } );
        }

        private Emulation.Hosting.Breakpoint RunToAddress( uint                                  address  ,
                                                           Debugging.DebugInfo                   di       ,
                                                           Emulation.Hosting.Breakpoint.Callback callback )
        {
            var bp = Action_SetBreakpoint( address, di, false, false, callback );

            bp.IsTemporary = true;

            return bp;
        }

        private Emulation.Hosting.Breakpoint AddBreakpointOnDebugMethod( string                                name   ,
                                                                         Emulation.Hosting.Breakpoint.Callback target )
        {
            TS.MethodRepresentation md = m_imageInformation.TypeSystem.GetWellKnownMethodNoThrow( name );
            if(md != null)
            {
                IR.ImageBuilders.SequentialRegion reg;

                reg = m_imageInformation.ResolveMethodToRegion( md );
                if(reg != null)
                {
                    return Action_SetBreakpoint( reg.ExternalAddress, null, false, true, target );
                }
            }

            return null;
        }

        //--//

        private void InnerAction_SynchronizeWithWorker()
        {
            bool fAcknowledged = false;

            ExecuteInWorkerThread( Hst.Forms.HostingSite.ExecutionState.Invalid, delegate()
            {
                ExecuteInFormThread( Hst.Forms.HostingSite.ExecutionState.Invalid, delegate()
                {
                    fAcknowledged = true;
                } );
            } );

            while(!fAcknowledged)
            {
                Application.DoEvents();

                Thread.Sleep( 10 );
            }
        }

        private void InnerAction_SynchronousStopExecution()
        {
            Emulation.Hosting.ProcessorControl svcPC;

            if(m_processorHost.GetHostingService( out svcPC ) && svcPC.StopExecution == false)
            {
                InnerAction_StopExecution();

                InnerAction_SynchronizeWithWorker();

                svcPC.StopExecution = false;
            }
        }

        //--//

        public DialogResult Action_EnsureConfiguration()
        {
            if(m_currentSession == null)
            {
                return DialogResult.Cancel;
            }

            m_environmentForm.SelectedEngine  = m_currentSession.SelectedEngine;
            m_environmentForm.SelectedProduct = m_currentSession.SelectedProduct;

            if(m_environmentForm.IsConfigured)
            {
                return DialogResult.OK;
            }

            return Action_EditConfiguration();
        }

        public DialogResult Action_EditConfiguration()
        {
            if(m_currentSession == null)
            {
                return DialogResult.Cancel;
            }

            return Action_EditConfiguration( m_currentSession );
        }

        public DialogResult Action_EditConfiguration( Session session )
        {
            m_environmentForm.SelectedEngine  = session.SelectedEngine;
            m_environmentForm.SelectedProduct = session.SelectedProduct;

            DialogResult res = m_environmentForm.ShowDialog();
            if(res == DialogResult.OK)
            {
                session.SelectedEngine  = m_environmentForm.SelectedEngine;
                session.SelectedProduct = m_environmentForm.SelectedProduct;
            }

            return res;
        }

        //--//

        public string Action_SelectSessionToLoad( string file )
        {
            if(file != null)
            {
                sessionOpenFileDialog.FileName = file;
            }

            if(sessionOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                return sessionOpenFileDialog.FileName;
            }

            return null;
        }

        public void Action_LoadSession()
        {
            string file = Action_SelectSessionToLoad( null );
            if(file != null)
            {
                try
                {
                    InnerAction_LoadSession( file );
                }
                catch(Exception ex)
                {
                    MessageBox.Show( string.Format( "Exception caught while loading {0}:\r\n{1}", file, ex ), "Session Load Error", MessageBoxButtons.OK );
                }
            }
        }

        public string Action_SelectSessionToSave( string file )
        {
            if(file != null)
            {
                sessionSaveFileDialog.FileName = file;
            }

            if(sessionSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                return sessionSaveFileDialog.FileName;
            }

            return null;
        }

        public void Action_SaveSession()
        {
            string file = Action_SelectSessionToSave( m_currentSession.SettingsFile );
            if(file != null)
            {
                Action_SaveSession( file );
            }
        }

        public void Action_SaveSession( string file )
        {
            try
            {
                InnerAction_SaveSession( file );
            }
            catch(Exception ex)
            {
                MessageBox.Show( string.Format( "Exception caught while saving {0}:\r\n{1}", file, ex ), "Session Save Error", MessageBoxButtons.OK );
            }
        }

        //--//

        public void Action_ResetAbsoluteTime()
        {
            if(m_processorHost.GetAbsoluteTime( out m_baseSample_clockTicks, out m_baseSample_nanoseconds ))
            {
                m_currentSample_clockTicks  = m_baseSample_clockTicks;
                m_currentSample_nanoseconds = m_baseSample_nanoseconds;
                m_lastSample_clockTicks     = m_baseSample_clockTicks;
                m_lastSample_nanoseconds    = m_baseSample_nanoseconds;
            }
        }

        public void Action_LoadImage()
        {
            if(Action_EnsureConfiguration() == DialogResult.OK)
            {
                if(imageOpenFileDialog.ShowDialog() == DialogResult.OK)
                {
                    m_currentSession.ImageToLoad = imageOpenFileDialog.FileName;

                    InnerAction_LoadImage();
                }
            }
        }

        public void Action_Run()
        {
            if(this.IsIdle)
            {
                StackFrame currentStackFrame = m_activeThread.StackFrame;

                if(ShouldExecuteForReal( currentStackFrame ))
                {
                    ExecuteInWorkerThread( Hst.Forms.HostingSite.ExecutionState.Running, delegate()
                    {
                        InnerAction_Execute( null, false, false, false );
                    } );
                }
            }
        }

        public void Action_Start()
        {
            if(this.IsIdle)
            {
////            InnerAction_SynchronousStopExecution();

                Action_Run();
            }
        }

        public void Action_BreakAll()
        {
            if(!this.IsIdle)
            {
                InnerAction_SynchronousStopExecution();
            }
        }

        public void Action_StopDebugging()
        {
            InnerAction_LoadImage();
        }

        public void Action_Restart()
        {
            InnerAction_LoadImage();

            InnerAction_SynchronizeWithWorker();

            Action_Run();
        }

        public void Action_StepInto()
        {
            if(this.IsIdle)
            {
                InnerAction_SynchronousStopExecution();

                bool       fUseAssemblyGranularity = m_currentSession.DisplayDisassembly;
                StackFrame currentStackFrame       = m_activeThread.StackFrame;

                if(ShouldStepForReal( currentStackFrame ))
                {
                    ExecuteInWorkerThread( Hst.Forms.HostingSite.ExecutionState.Running, delegate()
                    {
                        InnerAction_Execute( currentStackFrame, fUseAssemblyGranularity, true, false );
                    } );
                }
            }
        }

        public void Action_StepOver()
        {
            if(this.IsIdle)
            {
                InnerAction_SynchronousStopExecution();

                bool       fUseAssemblyGranularity = m_currentSession.DisplayDisassembly;
                StackFrame currentStackFrame       = m_activeThread.StackFrame;

                ///
                /// TODO: allow step over to step over inlined methods
                ///

                if(ShouldStepForReal( currentStackFrame ))
                {
                    ExecuteInWorkerThread( Hst.Forms.HostingSite.ExecutionState.Running, delegate()
                    {
                        InnerAction_Execute( currentStackFrame, fUseAssemblyGranularity, true, true );
                    } );
                }
            }
        }

        public void Action_StepOut()
        {
            if(this.IsIdle)
            {
                InnerAction_SynchronousStopExecution();

                if(m_activeThread.StackTrace.Count > 1)
                {
                    StackFrame sf = m_activeThread.StackTrace[1];

                    RunToAddress( sf.ProgramCounter );

                    Action_Run();
                }
            }
        }

        private bool ShouldExecuteForReal( StackFrame currentStackFrame )
        {
            //
            // If the top stack frame has multiple DebugInfo associated with its current program counter,
            // pretend to execute the step and just update the UI.
            //
            if(currentStackFrame != null)
            {
                bool fGot = false;
                uint pc   = currentStackFrame.ProgramCounter;

                foreach(var bp in m_processorHost.Breakpoints)
                {
                    if(bp.IsActive && bp.Address == pc)
                    {
                        if(currentStackFrame.MoveForwardToDebugInfo( bp.DebugInfo ))
                        {
                            fGot = true;

                            bp.Hit();
                        }
                    }
                }

                if(fGot)
                {
                    UpdateDisplay( false );

                    UpdateExecutionState( Hst.Forms.HostingSite.ExecutionState.Paused );

                    return false;
                }
            }

            return true;
        }

        private bool ShouldStepForReal( StackFrame currentStackFrame )
        {
            //
            // If the top stack frame has multiple DebugInfo associated with its current program counter,
            // pretend to execute the step and just update the UI.
            //
            if(currentStackFrame != null && currentStackFrame.AdvanceToNextDebugInfo())
            {
                UpdateDisplay( false );

                UpdateExecutionState( Hst.Forms.HostingSite.ExecutionState.Paused );

                return false;
            }

            return true;
        }

        //--//

        public void Action_SetBreakpoint( uint                pc ,
                                          Debugging.DebugInfo di )
        {
            Action_SetBreakpoint( pc, di, true );
        }

        public void Action_SetBreakpoint( uint                pc       ,
                                          Debugging.DebugInfo di       ,
                                          bool                fVisible )
        {
            Action_SetBreakpoint( pc, di, fVisible, false, null );
        }

        public Emulation.Hosting.Breakpoint Action_SetBreakpoint( uint                                  pc          ,
                                                                  Debugging.DebugInfo                   di          ,
                                                                  bool                                  fVisible    ,
                                                                  bool                                  fIsOptional ,
                                                                  Emulation.Hosting.Breakpoint.Callback target      )
        {
            if(target == null)
            {
                target = delegate( Emulation.Hosting.Breakpoint bpHit )
                {
                    return Emulation.Hosting.Breakpoint.Response.StopExecution;
                };
            }

            var bp = m_processorHost.CreateBreakpoint( pc, di, target );

            m_versionBreakpoints++;

            bp.IsOptional = fIsOptional;
            bp.ShowInUI   = fVisible;

            Action_ActivateBreakpoint( bp );

            return bp;
        }

        public void Action_ActivateBreakpoint( Emulation.Hosting.Breakpoint bp )
        {
            if(bp.IsActive == false)
            {
                bp.IsActive = true;

                SynchronizeBreakpointsUI();
            }
        }

        public void Action_RemoveBreakpoint( Emulation.Hosting.Breakpoint bp )
        {
            m_processorHost.RemoveBreakpoint( bp );

            m_versionBreakpoints++;

            SynchronizeBreakpointsUI();
        }

        public void Action_RefreshBreakpoints()
        {
            SynchronizeBreakpointsUI();
        }

        public void Action_DeleteAllBreakpoints()
        {
            foreach(Emulation.Hosting.Breakpoint bp in m_processorHost.Breakpoints)
            {
                if(bp.ShowInUI)
                {
                    m_processorHost.RemoveBreakpoint( bp );
                }
            }

            m_versionBreakpoints++;

            SynchronizeBreakpointsUI();
        }

        //--//

        public void Action_ToggleDisassembly()
        {
            if(this.IsIdle)
            {
                m_currentSession.DisplayDisassembly = !m_currentSession.DisplayDisassembly;

                if(this.SelectedStackFrame != null)
                {
                    VisualizeStackFrame( this.SelectedStackFrame );
                }
            }
        }

        public void Action_ToggleWrapper()
        {
            if(this.IsIdle)
            {
                m_imageInformation.DisplayWrapper = !m_imageInformation.DisplayWrapper;

                Debugging.DebugInfo diTarget = null;

                if(m_activeThread != null)
                {
                    if(m_activeThread.TopStackFrame != null)
                    {
                        diTarget = m_activeThread.TopStackFrame.DebugInfo;
                    }
                }

                ExitingState_Running( diTarget );
            }
        }

        //--//

        public void Action_MoveInTheStackTrace( int direction )
        {
            if(this.IsIdle)
            {
                if(this.SelectedStackFrame != null)
                {
                    int pos = this.SelectedThread.StackTrace.IndexOf( this.SelectedStackFrame );
                    if(pos >= 0)
                    {
                        Action_SelectStackFrame( pos + direction );
                    }
                }
            }
        }

        public void Action_SelectStackFrame( int depth )
        {
            foreach(StackFrame sf in this.SelectedThread.StackTrace)
            {
                if(sf.Depth == depth)
                {
                    CHECKS.ASSERT( m_imageInformation != null, "Missing ImageInformation" );

                    VisualizeStackFrame( sf );
                    break;
                }
            }
        }

        //--//

        public void Action_MoveInTheThreadList( int direction )
        {
            if(this.IsIdle)
            {
                if(this.SelectedThread != null)
                {
                    int pos = this.Threads.IndexOf( this.SelectedThread );
                    if(pos >= 0)
                    {
                        Action_SelectThread( pos + direction );
                    }
                }
            }
        }

        public void Action_SelectThread( int idx )
        {
            if(idx >= 0 && idx < m_threads.Count)
            {
                m_selectedThread = m_threads[idx];

                UpdateDisplay( false );
            }
        }
    }
}