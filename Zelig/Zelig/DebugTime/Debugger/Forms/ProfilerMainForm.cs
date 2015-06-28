//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;

    using IR  = Microsoft.Zelig.CodeGeneration.IR;
    using RT  = Microsoft.Zelig.Runtime;
    using TS  = Microsoft.Zelig.Runtime.TypeSystem;
    using Hst = Microsoft.Zelig.Emulation.Hosting;


    public partial class ProfilerMainForm : Emulation.Hosting.Forms.BaseDebuggerForm
    {
        //
        // State
        //

        private Profiler                                   m_profiler;
        private Profiler.Callers                           m_profiler_Callers;
        private Profiler.Callees                           m_profiler_Callees;
        private Profiler.CallsByMethodAndType              m_profiler_CallsByMethodAndType;
        private Emulation.Hosting.DeviceClockTicksTracking m_svcTiming;
        private TS.MethodRepresentation                    m_selectedMethod;

        private int                                        m_verProfiler = 1;
        private int                                        m_verCallTree;
        private int                                        m_verFunctions;
        private int                                        m_verClasses;
        private int                                        m_verAllocations;

        private TreeBasedGridView                          m_treeBasedGridView_CallTree;
        private TreeBasedGridView                          m_treeBasedGridView_Functions;
        private TreeBasedGridView                          m_treeBasedGridView_Classes;
        private TS.MethodRepresentation                    m_treeBasedGridView_CallerCallee_Method;
        private SplitContainer                             m_treeBasedGridView_CallerCallee_Top;
        private SplitContainer                             m_treeBasedGridView_CallerCallee_Bottom;
        private TreeBasedGridView                          m_treeBasedGridView_CallerCallee_Callers;
        private TreeBasedGridView                          m_treeBasedGridView_CallerCallee;
        private TreeBasedGridView                          m_treeBasedGridView_CallerCallee_Callees;
        private TreeBasedGridView                          m_treeBasedGridView_Allocations;

        //
        // Constructor Methods
        //

        public ProfilerMainForm( Emulation.Hosting.Forms.HostingSite site ) : base( site )
        {
            InitializeComponent();

            //--//

            UpdateButtonState();

            toolStripComboBox_Mode.SelectedIndex = 0;
            toolStripComboBox_View.SelectedIndex = 0;

            //--//

            site.RegisterView( this, Emulation.Hosting.Forms.HostingSite.PublicationMode.Tools );


            site.NotifyOnExecutionStateChange += delegate( Hst.Forms.HostingSite                host     ,
                                                           Hst.Forms.HostingSite.ExecutionState oldState ,
                                                           Hst.Forms.HostingSite.ExecutionState newState )
            {
                if(newState == Hst.Forms.HostingSite.ExecutionState.Loaded)
                {
                    DeallocateProfiler();
                }

                UpdateButtonState();

                return Hst.Forms.HostingSite.NotificationResponse.DoNothing;
            };
        }

        //
        // Helper Methods
        //

        private void AllocateProfiler()
        {
            if(m_profiler == null)
            {
                ImageInformation               imageInformation; this.Host.GetHostingService( out imageInformation );
                Emulation.Hosting.AbstractHost host;             this.Host.GetHostingService( out host             );

                MemoryDelta memDelta = new MemoryDelta( imageInformation, host );

                memDelta.RegisterForNotification( false, true );

                m_profiler = new Profiler( memDelta );
                
                host.GetHostingService( out m_svcTiming );
            }
        }

        private void DeallocateProfiler()
        {
            if(m_profiler != null)
            {
                m_profiler.Detach();

                m_profiler = null;

                UpdateUI();
            }
        }

        private void UpdateButtonState()
        {
            if(this.Host.IsIdle)
            {
                if(m_profiler != null && m_profiler.IsActive)
                {
                    toolStripButton_New  .Enabled = false;
                    toolStripButton_Start.Enabled = false;
                    toolStripButton_Stop .Enabled = true;
                }
                else
                {
                    toolStripButton_New  .Enabled = true;
                    toolStripButton_Start.Enabled = true;
                    toolStripButton_Stop .Enabled = false;
                }
            }
            else
            {
                toolStripButton_New  .Enabled = false;
                toolStripButton_Start.Enabled = false;
                toolStripButton_Stop .Enabled = false;
            }
        }

        private void SwitchToViewMode( Control ctrl )
        {
            this.panelBottom.SuspendLayout();

            this.panelBottom.Controls.Clear();

            if(ctrl != null)
            {
                this.panelBottom.Controls.Add( ctrl );
            }

            this.panelBottom.ResumeLayout();
        }

        private void UpdateUI()
        {
            if(m_profiler == null)
            {
                SwitchToViewMode( null );
            }
            else
            {
                switch(toolStripComboBox_View.SelectedIndex)
                {
////                Call Tree
                    case 0:
                        BuildUI_CallTree();
                        break;

////                Functions
                    case 1:
                        BuildUI_Functions();
                        break;

////                Classes
                    case 2:
                        BuildUI_Classes();
                        break;

////                Caller / Callee
                    case 3:
                        BuildUI_CallerCallee();
                        break;

////                Memory Allocations
                    case 4:
                        BuildUI_Allocations();
                        break;
                }
            }
        }

        //--//

        private void BuildUI_CallTree()
        {
            if(m_treeBasedGridView_CallTree == null)
            {
                var tree = BuildTree_Call();

                m_treeBasedGridView_CallTree = tree;

                tree.NodeMouseClick       += treeBasedGridView_NodeMouseClick;
                tree.NodeMouseDoubleClick += treeBasedGridView_NodeMouseDoubleClick;
            }

            if(m_verCallTree != m_verProfiler)
            {
                m_verCallTree = m_verProfiler;

                var root = m_treeBasedGridView_CallTree.RootNode;

                root.Clear();

                long inclusiveCycles = ComputeTotalInclusiveCycles();

                foreach(var tc in m_profiler.Threads)
                {
                    var en = tc.TopLevel;

                    if(en.Children != null)
                    {
                        string fmt;

                        switch(tc.ThreadKind)
                        {
                            case ThreadStatus.Kind.Bootstrap          : fmt = "Bootstrap"  ; break;
                            case ThreadStatus.Kind.IdleThread         : fmt = "Idle Thread"; break;
                            case ThreadStatus.Kind.InterruptThread    : fmt = "IRQ Thread" ; break;
                            case ThreadStatus.Kind.FastInterruptThread: fmt = "FIQ Thread" ; break;
                            default                                   : fmt = "Thread {0}" ; break;
                        }

                        var node = root.AddChild( string.Format( fmt, tc.ManagedThreadId ), en.InclusiveAllocatedBytes.ToString(),
                                                  PrintNumber( en.InclusiveClockCycles ), PrintTime( en.InclusiveClockCycles ), PrintPercentage( en.InclusiveClockCycles, inclusiveCycles ) );

                        node.Tag            = tc;
                        node.ExpandCallback = ExpandNode_CallTree;
                    }
                }
            }

            SwitchToViewMode( m_treeBasedGridView_CallTree );
        }

        private void ExpandNode_CallTree( TreeBasedGridView.GridNode node )
        {
            Profiler.CallEntry en;

            if(node.Tag is Profiler.ThreadContext)
            {
                var tc = (Profiler.ThreadContext)node.Tag;
                
                en = tc.TopLevel;
            }
            else
            {
                en = (Profiler.CallEntry)node.Tag;
            }

            if(en.Children != null)
            {
                ExpandNode_CallTree( node, en, 0, en.Children.Count );
            }
        }

        private void ExpandNode_CallTree( TreeBasedGridView.GridNode node       ,
                                          Profiler.CallEntry         en         ,
                                          int                        startIndex ,
                                          int                        endIndex   )
        {
            const int MaxArrayDisplay = 100;

            int length = endIndex - startIndex;

            if(length > MaxArrayDisplay)
            {
                int scale = MaxArrayDisplay;

                while(scale * MaxArrayDisplay < length)
                {
                    scale *= MaxArrayDisplay;
                }

                for(int pos = startIndex; pos < endIndex; pos += scale)
                {
                    int newStartIndex = pos;
                    int newEndIndex   = Math.Min( pos + scale, endIndex );

                    long inclusiveCycles         = 0;
                    long inclusiveAllocatedBytes = 0;

                    for(int pos2 = newStartIndex; pos2 < newEndIndex; pos2++)
                    {
                        var subEn = en.Children[pos2];

                        inclusiveCycles         += subEn.InclusiveClockCycles;
                        inclusiveAllocatedBytes += subEn.InclusiveAllocatedBytes;
                    }

                    var newNode = node.AddChild( string.Format( "[Calls {0}-{1}]", newStartIndex + 1, newEndIndex ), inclusiveAllocatedBytes.ToString(),
                                                 PrintNumber( inclusiveCycles ), PrintTime( inclusiveCycles ), PrintPercentage( inclusiveCycles, en.Owner.TopLevel.InclusiveClockCycles ) );

                    newNode.ExpandCallback = delegate( TreeBasedGridView.GridNode nodeSub )
                    {
                        ExpandNode_CallTree( nodeSub, en, newStartIndex, newEndIndex );
                    };
                }
            }
            else
            {
                for(int pos = startIndex; pos < endIndex; pos++)
                {
                    var subEn = en.Children[pos];

                    AddEntry_CallTree( node, subEn );
                }
            }
        }

        private void AddEntry_CallTree( TreeBasedGridView.GridNode node ,
                                        Profiler.CallEntry         en   )
        {
            var subNode = node.AddChild( en.Method.ToShortStringNoReturnValue(), en.InclusiveAllocatedBytes.ToString(),
                                         PrintNumber( en.InclusiveClockCycles ), PrintTime( en.InclusiveClockCycles ), PrintPercentage( en.InclusiveClockCycles, en.Owner.TopLevel.InclusiveClockCycles ),
                                         PrintNumber( en.ExclusiveClockCycles ), PrintTime( en.ExclusiveClockCycles ), PrintPercentage( en.ExclusiveClockCycles, en.Owner.TopLevel.InclusiveClockCycles ) );

            subNode.Tag            = en;
            subNode.ExpandCallback = ExpandNode_CallTree;
        }

        //--//

        private void BuildUI_Functions()
        {
            if(m_treeBasedGridView_Functions == null)
            {
                var tree = BuildTree_Function();

                m_treeBasedGridView_Functions = tree;

                tree.NodeMouseClick       += treeBasedGridView_NodeMouseClick;
                tree.NodeMouseDoubleClick += treeBasedGridView_NodeMouseDoubleClick;
            }

            if(m_verFunctions != m_verProfiler)
            {
                m_verFunctions = m_verProfiler;

                var coll = CacheGetCallsByMethodAndType();

                var root = m_treeBasedGridView_Functions.RootNode;

                root.Clear();

                long totalCycles = ComputeTotalInclusiveCycles();

                var array = coll.ToArray();

                Array.Sort( array, (x, y) =>
                    {
                        return y.Value.ExclusiveClockCycles.CompareTo( x.Value.ExclusiveClockCycles );
                    }
                );

                foreach(var pair in array)
                {
                    var md                     = pair.Key;
                    var collByMethod           = pair.Value;
                    var callsByMethod          = collByMethod[md.OwnerType].Count;
                    var clockCyclesByMethod    = collByMethod.ExclusiveClockCycles;
                    var allocatedBytesByMethod = collByMethod.AllocatedBytes;

                    var nodeByMethod = root.AddChild( md.ToShortStringNoReturnValue(), allocatedBytesByMethod.ToString(),
                                                      callsByMethod.ToString(),
                                                      PrintNumber    ( clockCyclesByMethod ), PrintFraction( clockCyclesByMethod, callsByMethod ),
                                                      PrintTime      ( clockCyclesByMethod ), PrintTime   ( clockCyclesByMethod, callsByMethod ),
                                                      PrintPercentage( clockCyclesByMethod, totalCycles ) );

                    nodeByMethod.Tag = md;
////                nodeByMethod.ExpandCallback = delegate( TreeBasedGridView.GridNode nodeSub )
////                {
////                    foreach(var td in collByMethod.Keys)
////                    {
////                        var collByType           = collByMethod[td];
////                        var clockCyclesByType    = collByType.ClockCycles;
////                        var allocatedBytesByType = collByType.AllocatedBytes;
////
////                        var nodeByType = nodeByMethod.AddChild( td.FullNameWithAbbreviation, allocatedBytesByType.ToString(),
////                                                                PrintNumber( clockCyclesByType ), PrintTime( clockCyclesByType ), PrintPercentage( clockCyclesByType, clockCyclesByMethod ) );
////
////                    }
////                };
                }
            }

            SwitchToViewMode( m_treeBasedGridView_Functions );
        }

        //--//

        private long ComputeTotalInclusiveCycles()
        {
            long totalCycles = 0;
    
            foreach(var tc in m_profiler.Threads)
            {
                var en = tc.TopLevel;
    
                totalCycles += en.InclusiveClockCycles;
            }

            return totalCycles;
        }

        private Profiler.CallsByMethodAndType CacheGetCallsByMethodAndType()
        {
            if(m_profiler_CallsByMethodAndType == null)
            {
                m_profiler_CallsByMethodAndType = m_profiler.GetCallsByMethod();
            }

            return m_profiler_CallsByMethodAndType;
        }

        private Profiler.Callers CacheGetCallers()
        {
            if(m_profiler_Callers == null)
            {
                m_profiler.GetCallersAndCallees( out m_profiler_Callers, out m_profiler_Callees );
            }

            return m_profiler_Callers;
        }

        private Profiler.Callees CacheGetCallees()
        {
            if(m_profiler_Callees == null)
            {
                m_profiler.GetCallersAndCallees( out m_profiler_Callers, out m_profiler_Callees );
            }

            return m_profiler_Callees;
        }

        //--//

        private void BuildUI_Classes()
        {
            if(m_treeBasedGridView_Classes == null)
            {
                var tree = BuildTree();

                m_treeBasedGridView_Classes = tree;

                tree.NodeMouseClick       += treeBasedGridView_NodeMouseClick;
                tree.NodeMouseDoubleClick += treeBasedGridView_NodeMouseDoubleClick;

                tree.SetColumns( new TreeBasedGridView.GridColumnDefinition( "Class"           , DataGridViewContentAlignment.MiddleLeft , false, false, false ),
                                 new TreeBasedGridView.GridColumnDefinition( "Allocated Bytes" , DataGridViewContentAlignment.MiddleRight, false, false, false ),
                                 new TreeBasedGridView.GridColumnDefinition( "Exclusive Cycles", DataGridViewContentAlignment.MiddleRight, false, false, false ),
                                 new TreeBasedGridView.GridColumnDefinition( "Exclusive Time"  , DataGridViewContentAlignment.MiddleRight, false, false, false ),
                                 new TreeBasedGridView.GridColumnDefinition( "Exclusive CPU%"  , DataGridViewContentAlignment.MiddleRight, false, false, false )  );
            }

            if(m_verClasses != m_verProfiler)
            {
                m_verClasses = m_verProfiler;

                var coll = m_profiler.GetCallsByType();

                var root = m_treeBasedGridView_Classes.RootNode;

                root.Clear();

                long totalCycles = ComputeTotalInclusiveCycles();

                var array = coll.ToArray();

                Array.Sort( array, (x, y) =>
                    {
                        return y.Value.ExclusiveClockCycles.CompareTo( x.Value.ExclusiveClockCycles );
                    }
                );

                foreach(var pair in array)
                {
                    var td                   = pair.Key;
                    var collByType           = pair.Value;
                    var clockCyclesByType    = collByType.ExclusiveClockCycles;
                    var allocatedBytesByType = collByType.AllocatedBytes;

                    var nodeByType = root.AddChild( td.FullNameWithAbbreviation, allocatedBytesByType.ToString(),
                                                    PrintNumber( clockCyclesByType ), PrintTime( clockCyclesByType ), PrintPercentage( clockCyclesByType, totalCycles ) );

                    nodeByType.ExpandCallback = delegate( TreeBasedGridView.GridNode nodeSub )
                    {
                        foreach(var md in collByType.Keys)
                        {
                            var collByMethod           = collByType[md];
                            var clockCyclesByMethod    = collByMethod.ExclusiveClockCycles;
                            var allocatedBytesByMethod = collByMethod.AllocatedBytes;

                            var nodeByMethod = nodeByType.AddChild( md.ToShortStringNoReturnValue(), allocatedBytesByMethod.ToString(),
                                                                    PrintNumber( clockCyclesByMethod ), PrintTime( clockCyclesByMethod ), PrintPercentage( clockCyclesByMethod, clockCyclesByType ) );

                            nodeByMethod.Tag = md;
                        }
                    };
                }
            }

            SwitchToViewMode( m_treeBasedGridView_Classes );
        }

        //--//

        private void BuildUI_CallerCallee()
        {
            if(m_treeBasedGridView_CallerCallee == null)
            {
                m_treeBasedGridView_CallerCallee_Callers = BuildTree_FunctionGeneric( "Callers", "# of Calls to Target"   );
                m_treeBasedGridView_CallerCallee         = BuildTree_FunctionGeneric( "Target" , "# of Calls to Target"   );
                m_treeBasedGridView_CallerCallee_Callees = BuildTree_FunctionGeneric( "Callees", "# of Calls from Target" );

                m_treeBasedGridView_CallerCallee_Top     = new SplitContainer
                                                           {
                                                               Orientation      = Orientation.Horizontal,
                                                               Dock             = DockStyle.Fill,
                                                               Size             = new System.Drawing.Size( 668, 440 ),
                                                               SplitterDistance = 220,
                                                               FixedPanel       = FixedPanel.Panel1,
                                                           };

                m_treeBasedGridView_CallerCallee_Bottom  = new SplitContainer
                                                           {
                                                               Orientation      = Orientation.Horizontal,
                                                               Dock             = DockStyle.Fill,
                                                               Size             = new System.Drawing.Size( 668, 220 ),
                                                               SplitterDistance = 70,
                                                               FixedPanel       = FixedPanel.Panel1,
                                                           };

                m_treeBasedGridView_CallerCallee_Top   .Panel1.Controls.Add( m_treeBasedGridView_CallerCallee_Callers );
                m_treeBasedGridView_CallerCallee_Top   .Panel2.Controls.Add( m_treeBasedGridView_CallerCallee_Bottom  );
                m_treeBasedGridView_CallerCallee_Bottom.Panel1.Controls.Add( m_treeBasedGridView_CallerCallee         );
                m_treeBasedGridView_CallerCallee_Bottom.Panel2.Controls.Add( m_treeBasedGridView_CallerCallee_Callees );

                TreeBasedGridView.NodeMouseEventHandler callback = delegate( object                               sender ,
                                                                             TreeBasedGridView.NodeMouseEventArgs e      )
                {
                    var md2 = GetSelectedMethod( e );
                    if(md2 != null)
                    {
                        m_selectedMethod = md2;
                        BuildUI_CallerCallee();
                    }
                };

                m_treeBasedGridView_CallerCallee_Callers.NodeMouseDoubleClick += callback;
                m_treeBasedGridView_CallerCallee        .NodeMouseDoubleClick += treeBasedGridView_NodeMouseDoubleClick;
                m_treeBasedGridView_CallerCallee_Callees.NodeMouseDoubleClick += callback;
            }

            var md = m_selectedMethod;

            if(md != null && m_treeBasedGridView_CallerCallee_Method != md)
            {
                m_treeBasedGridView_CallerCallee_Method = md;

                long totalCycles = ComputeTotalInclusiveCycles();

                {
                    var root = m_treeBasedGridView_CallerCallee_Callers.RootNode;

                    root.Clear();

                    var                    coll = CacheGetCallers();
                    Profiler.CallsByMethod calls;

                    if(coll.TryGetValue( md, out calls ))
                    {
                        var array = calls.ToArray();

                        Array.Sort( array, (x, y) =>
                            {
                                return y.Value.ComputeExactInclusiveClockCycles().CompareTo( x.Value.ComputeExactInclusiveClockCycles() );
                            }
                        );

                        foreach(var pair in array)
                        {
                            var md2                    = pair.Key;
                            var collByMethod           = pair.Value;
                            var callsByMethod          = collByMethod.Count;
                            var clockCyclesByMethod    = collByMethod.ComputeExactInclusiveClockCycles();
                            var allocatedBytesByMethod = collByMethod.ComputeExactAllocatedBytes      ();

                            var nodeByMethod = root.AddChild( md2.ToShortStringNoReturnValue(), allocatedBytesByMethod.ToString(),
                                                              callsByMethod.ToString(),
                                                              PrintNumber    ( clockCyclesByMethod ), PrintFraction( clockCyclesByMethod, callsByMethod ),
                                                              PrintTime      ( clockCyclesByMethod ), PrintTime   ( clockCyclesByMethod, callsByMethod ),
                                                              PrintPercentage( clockCyclesByMethod, totalCycles ) );

                            nodeByMethod.Tag = md2;
                        }
                    }
                }

                {
                    var root = m_treeBasedGridView_CallerCallee.RootNode;

                    root.Clear();

                    long callsByMethod;
                    long clockCyclesByMethod;
                    long allocatedBytesByMethod;

                    {
                        var                    coll = CacheGetCallers();
                        Profiler.CallsByMethod calls;
                        
                        if(coll.TryGetValue( md, out calls ))
                        {
                            callsByMethod = calls.TotalCalls;
                        }
                        else
                        {
                            callsByMethod = 1;
                        }
                    }

                    {
                        var                  coll = CacheGetCallsByMethodAndType();
                        Profiler.CallsByType calls;

                        if(coll.TryGetValue( md, out calls ))
                        {
                            clockCyclesByMethod    = calls.InclusiveClockCycles;
                            allocatedBytesByMethod = calls.AllocatedBytes;
                        }
                        else
                        {
                            clockCyclesByMethod    = 0;
                            allocatedBytesByMethod = 0;
                        }
                    }


                    var nodeByMethod = root.AddChild( md.ToShortStringNoReturnValue(), allocatedBytesByMethod.ToString(),
                                                      callsByMethod.ToString(),
                                                      PrintNumber    ( clockCyclesByMethod ), PrintFraction( clockCyclesByMethod, callsByMethod ),
                                                      PrintTime      ( clockCyclesByMethod ), PrintTime    ( clockCyclesByMethod, callsByMethod ),
                                                      PrintPercentage( clockCyclesByMethod, totalCycles ) );

                    nodeByMethod.Tag = md;
                }

                {
                    var root = m_treeBasedGridView_CallerCallee_Callees.RootNode;

                    root.Clear();

                    var                    coll = CacheGetCallees();
                    Profiler.CallsByMethod calls;

                    if(coll.TryGetValue( md, out calls ))
                    {
                        var array = calls.ToArray();

                        Array.Sort( array, (x, y) =>
                            {
                                return y.Value.InclusiveClockCycles.CompareTo( x.Value.InclusiveClockCycles );
                            }
                        );

                        foreach(var pair in array)
                        {
                            var md2                    = pair.Key;
                            var collByMethod           = pair.Value;
                            var callsByMethod          = collByMethod.Count;
                            var clockCyclesByMethod    = collByMethod.ComputeExactInclusiveClockCycles();
                            var allocatedBytesByMethod = collByMethod.ComputeExactAllocatedBytes      ();

                            var nodeByMethod = root.AddChild( md2.ToShortStringNoReturnValue(), allocatedBytesByMethod.ToString(),
                                                              callsByMethod.ToString(),
                                                              PrintNumber    ( clockCyclesByMethod ), PrintFraction( clockCyclesByMethod, callsByMethod ),
                                                              PrintTime      ( clockCyclesByMethod ), PrintTime   ( clockCyclesByMethod, callsByMethod ),
                                                              PrintPercentage( clockCyclesByMethod, totalCycles ) );

                            nodeByMethod.Tag = md2;
                        }
                    }
                }
            }

            SwitchToViewMode( m_treeBasedGridView_CallerCallee_Top );
        }

        //--//

        private void BuildUI_Allocations()
        {
            if(m_treeBasedGridView_Allocations == null)
            {
                var tree = BuildTree();

                m_treeBasedGridView_Allocations = tree;

                tree.NodeMouseDoubleClick += treeBasedGridView_NodeMouseDoubleClick;

                tree.SetColumns( new TreeBasedGridView.GridColumnDefinition( "Type"                 , DataGridViewContentAlignment.MiddleLeft , false, false, false ),
                                 new TreeBasedGridView.GridColumnDefinition( "Allocated Bytes"      , DataGridViewContentAlignment.MiddleRight, false, false, false ),
                                 new TreeBasedGridView.GridColumnDefinition( "Allocated Bytes %"    , DataGridViewContentAlignment.MiddleRight, false, false, false ),
                                 new TreeBasedGridView.GridColumnDefinition( "Allocated Instances"  , DataGridViewContentAlignment.MiddleRight, false, false, false ),
                                 new TreeBasedGridView.GridColumnDefinition( "Allocated Instances %", DataGridViewContentAlignment.MiddleRight, false, false, false )  );
            }

            if(m_verAllocations != m_verProfiler)
            {
                m_verAllocations = m_verProfiler;

                var coll = m_profiler.GetAllocationsByTypeAndMethod();

                var root = m_treeBasedGridView_Allocations.RootNode;

                root.Clear();

                long totalAllocatedBytes     = coll.AllocatedBytes;
                long totalAllocatedInstances = coll.AllocatedInstances;
        
                var array = coll.ToArray();

                Array.Sort( array, (x, y) =>
                    {
                        return y.Value.AllocatedBytes.CompareTo( x.Value.AllocatedBytes );
                    }
                );

                foreach(var pair in array)
                {
                    var td                       = pair.Key;
                    var collByType               = pair.Value;
                    var allocatedBytesByType     = collByType.AllocatedBytes;
                    var allocatedInstancesByType = collByType.AllocatedInstances;

                    var nodeByType = root.AddChild( td.FullNameWithAbbreviation,
                                                    PrintNumber( allocatedBytesByType     ), PrintPercentage( allocatedBytesByType    , totalAllocatedBytes     ),
                                                    PrintNumber( allocatedInstancesByType ), PrintPercentage( allocatedInstancesByType, totalAllocatedInstances )  );

                    nodeByType.ExpandCallback = delegate( TreeBasedGridView.GridNode nodeSub )
                    {
                        foreach(var md in collByType.Keys)
                        {
                            var collByMethod               = collByType[md];
                            var allocatedBytesByMethod     = collByMethod.AllocatedBytes;
                            var allocatedInstancesByMethod = collByMethod.AllocatedInstances;

                            var nodeByMethod = nodeByType.AddChild( md.ToShortStringNoReturnValue(),
                                                                    PrintNumber( allocatedBytesByMethod     ), PrintPercentage( allocatedBytesByMethod    , allocatedBytesByType     ),
                                                                    PrintNumber( allocatedInstancesByMethod ), PrintPercentage( allocatedInstancesByMethod, allocatedInstancesByType )  );

                            nodeByMethod.Tag = md;
                        }
                    };
                }
            }

            SwitchToViewMode( m_treeBasedGridView_Allocations );
        }

        //--//

        private string PrintTime( long cycles )
        {
            if(m_svcTiming != null)
            {
                var time = m_svcTiming.ClockTicksToTime( cycles );

                //
                // \u00b5 = Greek micro
                //
                return string.Format( "{0:F3}\u00B5Sec", time.TotalSeconds * 1E6 );
            }
            else
            {
                return "n/a";
            }
        }

        private string PrintTime( long cycles  ,
                                  long divider )
        {
            if(m_svcTiming != null && divider != 0)
            {
                var time = m_svcTiming.ClockTicksToTime( cycles );

                //
                // \u00b5 = Greek micro
                //
                return string.Format( "{0:F3}\u00B5Sec", time.TotalSeconds * 1E6 / divider );
            }
            else
            {
                return "n/a";
            }
        }

        private string PrintNumber( long val )
        {
            return string.Format( "{0}", val );
        }

        private string PrintPercentage( long val   ,
                                        long total )
        {
            if(total != 0)
            {
                return string.Format( "{0:F2}%", 100.00 * val / total );
            }
            else
            {
                return "n/a";
            }
        }

        private string PrintFraction( long val   ,
                                      long total )
        {
            if(total != 0)
            {
                return string.Format( "{0:F2}", (double)val / total );
            }
            else
            {
                return "n/a";
            }
        }

        //--//

        private static TreeBasedGridView BuildTree()
        {
            var tree = new TreeBasedGridView
            {
                BorderStyle = BorderStyle.Fixed3D,
                Dock        = DockStyle.Fill,
                Location    = new Point( 0, 0 ),
                TabIndex    = 0,
            };

            return tree;
        }

        private static TreeBasedGridView BuildTree_Call()
        {
            var tree = BuildTree();

            tree.SetColumns( new TreeBasedGridView.GridColumnDefinition( "Entry"           , DataGridViewContentAlignment.MiddleLeft , false, false, false ),
                             new TreeBasedGridView.GridColumnDefinition( "Allocated Bytes" , DataGridViewContentAlignment.MiddleRight, false, false, false ),
                             new TreeBasedGridView.GridColumnDefinition( "Inclusive Cycles", DataGridViewContentAlignment.MiddleRight, false, false, false ),
                             new TreeBasedGridView.GridColumnDefinition( "Inclusive Time"  , DataGridViewContentAlignment.MiddleRight, false, false, false ),
                             new TreeBasedGridView.GridColumnDefinition( "Inclusive CPU%"  , DataGridViewContentAlignment.MiddleRight, false, false, false ),
                             new TreeBasedGridView.GridColumnDefinition( "Exclusive Cycles", DataGridViewContentAlignment.MiddleRight, false, false, false ),
                             new TreeBasedGridView.GridColumnDefinition( "Exclusive Time"  , DataGridViewContentAlignment.MiddleRight, false, false, false ),
                             new TreeBasedGridView.GridColumnDefinition( "Exclusive CPU%"  , DataGridViewContentAlignment.MiddleRight, false, false, false )  );

            return tree;
        }

        private static TreeBasedGridView BuildTree_Function()
        {
            return BuildTree_FunctionGeneric( "Function", "# of Calls" );
        }

        private static TreeBasedGridView BuildTree_FunctionGeneric( string function      ,
                                                                    string numberOfCalls )
        {
            var tree = BuildTree();

            tree.SetColumns( new TreeBasedGridView.GridColumnDefinition( function          , DataGridViewContentAlignment.MiddleLeft , false, false, false ),
                             new TreeBasedGridView.GridColumnDefinition( "Allocated Bytes" , DataGridViewContentAlignment.MiddleRight, false, false, false ),
                             new TreeBasedGridView.GridColumnDefinition( numberOfCalls     , DataGridViewContentAlignment.MiddleRight, false, false, false ),
                             new TreeBasedGridView.GridColumnDefinition( "Cycles"          , DataGridViewContentAlignment.MiddleRight, false, false, false ),
                             new TreeBasedGridView.GridColumnDefinition( "Cycles/Call"     , DataGridViewContentAlignment.MiddleRight, false, false, false ),
                             new TreeBasedGridView.GridColumnDefinition( "Time"            , DataGridViewContentAlignment.MiddleRight, false, false, false ),
                             new TreeBasedGridView.GridColumnDefinition( "Time/Call"       , DataGridViewContentAlignment.MiddleRight, false, false, false ),
                             new TreeBasedGridView.GridColumnDefinition( "CPU%"            , DataGridViewContentAlignment.MiddleRight, false, false, false )  );

            return tree;
        }

        //--//

        private void ShowMethod( TS.MethodRepresentation md )
        {
            var cfg = IR.TypeSystemForCodeTransformation.GetCodeForMethod( md );
            if(cfg != null)
            {
                ImageInformation imageInformation; this.Host.GetHostingService( out imageInformation );

                IR.ImageBuilders.SequentialRegion reg = imageInformation.ImageBuilder.GetAssociatedRegion( cfg );
                if(reg != null)
                {
                    Debugging.DebugInfo di;

                    if(imageInformation.LocateFirstSourceCode( reg, 0, out di ))
                    {
                        this.Host.VisualizeDebugInfo( di );
                    }
                }
            }
        }

        private static TS.MethodRepresentation GetSelectedMethod( TreeBasedGridView.NodeMouseEventArgs e )
        {
            if(e.SelectedColumn == 0)
            {
                var node = e.SelectedNode;
                if(node != null)
                {
                    var tag = node.Tag;

                    if(tag is Profiler.CallEntry)
                    {
                        Profiler.CallEntry en = (Profiler.CallEntry)tag;

                        return en.Method;
                    }
                    else if(tag is TS.MethodRepresentation)
                    {
                        TS.MethodRepresentation md = (TS.MethodRepresentation)tag;

                        return md;
                    }
                }
            }

            return null;
        }

        //
        // Access Methods
        //

        public override string ViewTitle
        {
            get
            {
                return "&Profiler";
            }
        }

        //
        // Event Methods
        //

        private void toolStripButton_New_Click( object    sender ,
                                                EventArgs e      )
        {
            DeallocateProfiler();

            UpdateButtonState();
        }

        private void toolStripButton_Start_Click( object    sender ,
                                                  EventArgs e      )
        {
            AllocateProfiler();

            m_profiler.CollectAllocationData = (toolStripComboBox_Mode.SelectedIndex == 1);

            m_profiler.Attach();

            UpdateButtonState();
        }

        private void toolStripButton_Stop_Click( object    sender ,
                                                 EventArgs e      )
        {
            if(m_profiler != null)
            {
                m_profiler.Detach();
                m_verProfiler++;

                m_profiler_CallsByMethodAndType = null;
                m_profiler_Callers              = null;
                m_profiler_Callees              = null;

                UpdateUI();
            }

            UpdateButtonState();
        }

        private void toolStripComboBox_View_SelectedIndexChanged( object    sender ,
                                                                  EventArgs e      )
        {
            if(m_profiler != null && m_profiler.IsActive == false)
            {
                UpdateUI();
            }
        }
 
        void treeBasedGridView_NodeMouseClick( object                               sender ,
                                               TreeBasedGridView.NodeMouseEventArgs e      )
        {
            var md = GetSelectedMethod( e );
            if(md != null)
            {
                m_selectedMethod = md;
            }
        }
 
        void treeBasedGridView_NodeMouseDoubleClick( object                               sender ,
                                                     TreeBasedGridView.NodeMouseEventArgs e      )
        {
            var md = GetSelectedMethod( e );
            if(md != null)
            {
                ShowMethod( md );
            }
        }
   }
}