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
    using InstructionSet     = Microsoft.Zelig.TargetModel.ArmProcessor.InstructionSet;
    using IR                 = Microsoft.Zelig.CodeGeneration.IR;
    using RT                 = Microsoft.Zelig.Runtime;
    using TS                 = Microsoft.Zelig.Runtime.TypeSystem;
    using Hst                = Microsoft.Zelig.Emulation.Hosting;


    public partial class LocalsView : UserControl
    {
        //
        // State
        //

        DebuggerMainForm m_owner;
        WatchHelper      m_wh;

        //
        // Constructor Methods
        //

        public LocalsView()
        {
            InitializeComponent();

            WatchHelper.SetColumns( treeBasedGridView_Locals );
        }

        //
        // Helper Methods
        //

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
            ImageInformation imageInformation  = m_owner.ImageInformation;
            StackFrame       currentStackFrame = m_owner.SelectedStackFrame;
            
            //--//

            WatchHelper.Synchronize( ref m_wh, m_owner.MemoryDelta, treeBasedGridView_Locals.RootNode, true, true );

            if(m_wh != null)
            {
                m_wh.HexadecimalDisplay = toolStripMenuItem_HexDisplay.Checked;
            }

            //--//

            treeBasedGridView_Locals.StartTreeUpdate();

            treeBasedGridView_Locals.Enabled = currentStackFrame != null;

            if(currentStackFrame == null || m_owner.IsIdle == false)
            {
                //DebuggerMainForm.GrayOutRowsInDataGridView( treeBasedGridView_Locals.Rows );
            }
            else
            {
                IR.LowLevelVariableExpression[] array       = imageInformation.AliveVariables( currentStackFrame.Region, currentStackFrame.RegionOffset );
                int                             lenArray    = array.Length;
                var                             valArray    = new AbstractValueHandle[lenArray];
                var                             nameArray   = new string             [lenArray];
                var                             peripherals = imageInformation.TypeSystem.MemoryMappedPeripherals;

                for(int i = 0; i < lenArray; i++)
                {
                    IR.LowLevelVariableExpression var = array[i];

                    IR.VariableExpression varSrc = var.SourceVariable;
                    if(varSrc != null && varSrc.DebugName != null)
                    {
                        nameArray[i] = varSrc.DebugName.Name;
                    }

                    if(var is IR.PhysicalRegisterExpression)
                    {
                        var varReg  = var as IR.PhysicalRegisterExpression;
                        var regDesc = varReg.RegisterDescriptor;

                        valArray[i] = new RegisterValueHandle( currentStackFrame.RegisterContext.GetValue( regDesc ), varReg.Type, true );

                        if(nameArray[i] == null)
                        {
                            nameArray[i] = string.Format( "$Reg( {0} )", regDesc.Mnemonic );
                        }
                    }
                    else if(var is IR.StackLocationExpression)
                    {
                        var  varStack = var as IR.StackLocationExpression;
                        uint address  = 0;

                        switch(varStack.StackPlacement)
                        {
                            case IR.StackLocationExpression.Placement.In:
                                {
                                    StackFrame sf = m_owner.SelectedThread.PreviousStackFrame;

                                    if(sf == null)
                                    {
                                        sf = currentStackFrame;
                                    }

                                    address = sf.StackPointer + (uint)varStack.Number * sizeof(uint);
                                }
                                break;

                            case IR.StackLocationExpression.Placement.Local:
                            case IR.StackLocationExpression.Placement.Out:
                                {
                                    address = currentStackFrame.StackPointer + varStack.AllocationOffset;
                                }
                                break;
                        }

                        valArray[i] = new MemoryValueHandle( varStack.Type, peripherals.GetValue( varStack.Type ), null, true, m_wh.MemoryDelta, address );

                        if(nameArray[i] == null)
                        {
                            nameArray[i] = string.Format( "$Stack(0x{0:X8})", varStack.AllocationOffset );
                        }
                    }
                    else
                    {
                        array[i] = null;
                    }
                }

                var lst = new List< WatchHelper.ItemDescriptor >();

                for(int i = 0; i < lenArray; i++)
                {
                    IR.LowLevelVariableExpression var = array[i];

                    if(var != null)
                    {
                        IR.VariableExpression varSrc = var.SourceVariable;
                        string                name   = nameArray[i];
                        AbstractValueHandle   valCtx = valArray[i];
                        TS.TypeRepresentation td;

                        if(varSrc != null)
                        {
                            td = varSrc.Type;
                        }
                        else
                        {
                            td = var.Type;
                        }

                        if(td is TS.ValueTypeRepresentation)
                        {
                            if(varSrc != null)
                            {
                                var subLocations = BuildSubLocations( array, valArray, varSrc );
                                if(subLocations != null)
                                {
                                    valCtx = new CompoundValueHandle( td, true, subLocations );
                                }
                            }
                        }

                        var item = new WatchHelper.ItemDescriptor( m_wh, name, td, valCtx );

                        lst.Add( item );
                    }
                }

                m_wh.Update( lst, true );
            }

            treeBasedGridView_Locals.EndTreeUpdate();
        }

        private static CompoundValueHandle.Fragment[] BuildSubLocations( IR.LowLevelVariableExpression[] array    ,
                                                                         AbstractValueHandle[]           valArray ,
                                                                         IR.VariableExpression           varSrc   )
        {
            CompoundValueHandle.Fragment[] res = null;

            for(int i = 0; i < array.Length; i++)
            {
                var ex = array[i];

                if(ex != null && ex.SourceVariable == varSrc)
                {
                    res = ArrayUtility.AppendToArray( res, new CompoundValueHandle.Fragment( valArray[i], (int)ex.SourceOffset ) );

                    array[i] = null;
                }
            }

            return res;
        }

        //
        // Access Methods
        //

        private void treeBasedGridView_Locals_CellMouseClick( object                               sender ,
                                                              TreeBasedGridView.NodeMouseEventArgs e      )
        {
            if(e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show( treeBasedGridView_Locals, e.Location );
            }
        }

        private void toolStripMenuItem_HexDisplay_CheckedChanged( object    sender ,
                                                                  EventArgs e      )
        {
            if(treeBasedGridView_Locals.Enabled)
            {
                m_wh.HexadecimalDisplay = toolStripMenuItem_HexDisplay.Checked;
            }
        }

        private void toolStripMenuItem_HexDisplay_Click( object    sender ,
                                                         EventArgs e      )
        {
            toolStripMenuItem_HexDisplay.Checked = !toolStripMenuItem_HexDisplay.Checked;
        }

        //
        // Event Methods
        //
    }
}
