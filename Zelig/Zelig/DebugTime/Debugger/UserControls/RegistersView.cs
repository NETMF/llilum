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


    public partial class RegistersView : UserControl
    {
        //
        // State
        //

        DebuggerMainForm m_owner;
        WatchHelper      m_wh;

        //
        // Constructor Methods
        //

        public RegistersView()
        {
            InitializeComponent();

            WatchHelper.SetColumns( treeBasedGridView_Registers );
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

            WatchHelper.Synchronize( ref m_wh, m_owner.MemoryDelta, treeBasedGridView_Registers.RootNode, false, false );

            if(m_wh != null)
            {
                m_wh.HexadecimalDisplay = toolStripMenuItem_HexDisplay.Checked;
            }

            //--//

            treeBasedGridView_Registers.StartTreeUpdate();

            treeBasedGridView_Registers.Enabled = currentStackFrame != null;

            if(currentStackFrame == null || m_owner.IsIdle == false)
            {
////            DebuggerMainForm.GrayOutRowsInDataGridView( treeBasedGridView_Registers.Rows );
            }
            else
            {
                IR.LowLevelVariableExpression[] array = imageInformation.AliveVariables( currentStackFrame.Region, currentStackFrame.RegionOffset );
                var                             ht    = HashTableFactory.NewWithReferenceEquality< IR.Abstractions.RegisterDescriptor, IR.PhysicalRegisterExpression >();

                foreach(IR.LowLevelVariableExpression var in array)
                {
                    IR.PhysicalRegisterExpression varReg = var as IR.PhysicalRegisterExpression;

                    if(varReg != null)
                    {
                        ht[varReg.RegisterDescriptor] = varReg;
                    }
                }

                var regCtx = currentStackFrame.RegisterContext;
                var lst    = new List< WatchHelper.ItemDescriptor >();

                foreach(IR.Abstractions.RegisterDescriptor regDesc in imageInformation.TypeSystem.PlatformAbstraction.GetRegisters())
                {
                    TS.TypeRepresentation registerType   = null;
                    string                typeDescriptor = null;

                    //--//

                    IR.PhysicalRegisterExpression varReg;

                    if(ht.TryGetValue( regDesc, out varReg ))
                    {
                        var varType = varReg.Type;

                        registerType = varType;

                        string                typeName = varType.FullNameWithAbbreviation;
                        IR.VariableExpression varSrc   = varReg.SourceVariable;

                        if(varSrc != null && varSrc.DebugName != null)
                        {
                            typeDescriptor = string.Format( "{0} {1}", typeName, varSrc.DebugName.Name );
                        }
                        else
                        {
                            typeDescriptor = string.Format( "{0}", typeName );
                        }
                    }
                    else
                    {
                        var wkt = imageInformation.TypeSystem.WellKnownTypes;

                        if(regDesc.InIntegerRegisterFile)
                        {
                            switch(regDesc.PhysicalStorageSize)
                            {
                                case 1:
                                    registerType = wkt.System_UInt32;
                                    break;

                                case 2:
                                    registerType = wkt.System_UInt64;
                                    break;
                            }
                        }
                        else if(regDesc.InFloatingPointRegisterFile)
                        {
                            switch(regDesc.PhysicalStorageSize)
                            {
                                case 1:
                                    registerType = wkt.System_Single;
                                    break;

                                case 2:
                                    registerType = wkt.System_Double;
                                    break;
                            }
                        }

                        if(registerType == null)
                        {
                            registerType = wkt.System_UInt32;
                        }
                    }

                    //--//

                    var valueHandle = new RegisterValueHandle( regCtx.GetValue( regDesc ), registerType, true );
                    var item        = new WatchHelper.ItemDescriptor( m_wh, regDesc.Mnemonic, registerType, valueHandle, typeDescriptor );

                    lst.Add( item );
                }

                m_wh.Update( lst, false );
            }

            treeBasedGridView_Registers.EndTreeUpdate();
        }

        //
        // Event Methods
        //

        private void treeBasedGridView_Registers_CellMouseClick( object                               sender ,
                                                                 TreeBasedGridView.NodeMouseEventArgs e      )
        {
            if(e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show( treeBasedGridView_Registers, e.Location );
            }
        }

        private void toolStripMenuItem_HexDisplay_CheckedChanged( object    sender ,
                                                                  EventArgs e      )
        {
            if(treeBasedGridView_Registers.Enabled)
            {
                if(m_wh != null)
                {
                    m_wh.HexadecimalDisplay = toolStripMenuItem_HexDisplay.Checked;
                }
            }
        }

        private void toolStripMenuItem_HexDisplay_Click( object    sender ,
                                                         EventArgs e      )
        {
            toolStripMenuItem_HexDisplay.Checked = !toolStripMenuItem_HexDisplay.Checked;
        }
    }
}
