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


    public partial class BreakpointsView : UserControl
    {
        private int c_panel_Breakpoints_Search__Collapsed = 30;
        private int c_panel_Breakpoints_Search__Expanded  = 187;

        //
        // State
        //

        DebuggerMainForm                     m_owner;
        int                                  m_version;
        List< Emulation.Hosting.Breakpoint > m_breakpoints;
        Emulation.Hosting.Breakpoint         m_selectedBreakpoint;

        Icon                                 m_icon_Breakpoint         = Properties.Resources.Breakpoint;
        Icon                                 m_icon_BreakpointDisabled = Properties.Resources.BreakpointDisabled;

        //
        // Constructor Methods
        //

        public BreakpointsView()
        {
            InitializeComponent();

            //--//

            m_breakpoints = new List< Emulation.Hosting.Breakpoint >();

            DebuggerMainForm.SetPanelHeight( panel_Breakpoints_Search, c_panel_Breakpoints_Search__Collapsed );
        }

        //
        // Access Methods
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
                if(e == Hst.Forms.HostingSite.VisualizationEvent.BreakpointsChange)
                {
                    UpdateUI();
                }

                return Hst.Forms.HostingSite.NotificationResponse.DoNothing;
            };
        }

        public void UpdateUI()
        {
            ProcessorHost svc; m_owner.Host.GetHostingService( out svc );

            ImageInformation               imageInformation  = m_owner.ImageInformation;
            StackFrame                     currentStackFrame = m_owner.SelectedStackFrame;
            Emulation.Hosting.Breakpoint[] array             = svc.Breakpoints;

            toolStripButton_DeleteBreakpoint    .Enabled = m_selectedBreakpoint != null;
            toolStripButton_DeleteAllBreakpoints.Enabled = array.Length > 0;
            toolStripButton_ToggleAllBreakpoints.Enabled = array.Length > 0;

            //--//

            dataGridView_Breakpoints.SuspendLayout();

            DataGridViewRowCollection rows = dataGridView_Breakpoints.Rows;

            dataGridView_Breakpoints.Enabled = currentStackFrame != null;

            if(currentStackFrame == null)
            {
                m_version = -1;

                DebuggerMainForm.GrayOutRowsInDataGridView( rows );
            }
            else
            {
                bool fRebuild = (m_version != m_owner.VersionBreakpoints);

                if(fRebuild)
                {
                    m_version = m_owner.VersionBreakpoints;

                    rows         .Clear();
                    m_breakpoints.Clear();
                }

                int pos = 0;

                foreach(Emulation.Hosting.Breakpoint bp in array)
                {
                    Icon icon;

                    if(bp.ShowInUI == false)
                    {
                        continue;
                    }

                    if(bp.IsActive)
                    {
                        icon = m_icon_Breakpoint;
                    }
                    else
                    {
                        icon = m_icon_BreakpointDisabled;
                    }

                    if(fRebuild)
                    {
                        IR.ImageBuilders.SequentialRegion reg;
                        uint                              offset;
                        Debugging.DebugInfo               di;
                        string                            location;

                        imageInformation.LocateFirstSourceCode( bp.Address, out reg, out offset, out di );

                        if(di != null)
                        {
                            location = string.Format( "{0}, line {1} character {2}", Path.GetFileName( di.SrcFileName ), di.BeginLineNumber, di.BeginColumn );
                        }
                        else if(reg != null && reg.Context is IR.BasicBlock)
                        {
                            location = string.Format( "{0}, offset {1}", reg.Context, offset );
                        }
                        else
                        {
                            location = string.Format( "Address 0x{0:X8}", bp.Address );
                        }

                        rows.Add( icon, location, "(no condition)", bp.HitCount );
                        m_breakpoints.Add( bp );
                    }
                    else
                    {
                        rows[pos].Cells[0].Value = icon;
                    }

                    pos++;
                }
            }

            dataGridView_Breakpoints.ResumeLayout();
        }

        public void Set( string breakpoint )
        {
            if(CreateBreakpoint( breakpoint ) == false)
            {
                ListMatches( breakpoint );
            }
        }

        //--//

        private void SelectBreakpoint( int index )
        {
            if(index >= 0 && index < m_breakpoints.Count)
            {
                m_selectedBreakpoint = m_breakpoints[index];
            }
            else
            {
                m_selectedBreakpoint = null;
            }
        }

        private void ListMatches( string txt )
        {
            listBox_Breakpoint_SearchResults.BeginUpdate();

            ListBox.ObjectCollection col = listBox_Breakpoint_SearchResults.Items;

            col.Clear();

            uint res;

            if(txt.StartsWith( "0x" ) && uint.TryParse( txt.Substring( 2 ), System.Globalization.NumberStyles.AllowHexSpecifier, null, out res ))
            {
                col.Add( txt );
            }
            else
            {
                ImageInformation imageInformation = m_owner.ImageInformation;
                txt = txt.ToUpper();

                if(imageInformation != null && txt.Length >= 2)
                {
                    foreach(string methodName in imageInformation.NameToCfg.Keys)
                    {
                        if(methodName.ToUpper().Contains( txt ))
                        {
                            col.Add( methodName );
                        }
                    }

                    foreach(string fileName in imageInformation.SourceCodeFiles)
                    {
                        if(fileName.ToUpper().Contains( txt ))
                        {
                            col.Add( fileName );
                        }
                    }
                }
            }

            if(col.Count > 0)
            {
                DebuggerMainForm.SetPanelHeight( panel_Breakpoints_Search, c_panel_Breakpoints_Search__Expanded );
            }
            else
            {
                DebuggerMainForm.SetPanelHeight( panel_Breakpoints_Search, c_panel_Breakpoints_Search__Collapsed );
            }

            listBox_Breakpoint_SearchResults.EndUpdate();
        }

        private bool CreateBreakpoint( string txt )
        {
            ImageInformation imageInformation = m_owner.ImageInformation;

            if(imageInformation != null)
            {
                if(txt != null)
                {
                    if(imageInformation.NameToCfg.ContainsKey( txt ))
                    {
                        IR.ImageBuilders.SequentialRegion selectedRegion = null;

                        foreach(IR.ControlFlowGraphStateForCodeTransformation cfg in imageInformation.NameToCfg[txt])
                        {
                            IR.ImageBuilders.SequentialRegion reg = imageInformation.ImageBuilder.GetAssociatedRegion( cfg );
                            if(reg != null)
                            {
                                if(selectedRegion != null)
                                {
                                    //
                                    // TODO: Select one among multiple options.
                                    //
                                }

                                selectedRegion = reg;
                            }
                        }

                        if(selectedRegion != null)
                        {
                            m_owner.Action_SetBreakpoint( selectedRegion.ExternalAddress, null );
                            return true;
                        }
                    }

                    uint address;

                    if(txt.StartsWith( "0x" ) && uint.TryParse( txt.Substring( 2 ), System.Globalization.NumberStyles.AllowHexSpecifier, null, out address ))
                    {
                        m_owner.Action_SetBreakpoint( address, null );
                        return true;
                    }
                }
            }

            return false;
        }

        //
        // Access Methods
        //

        //
        // Event Methods
        //

        private void textBox_Breakpoint_Search_TextChanged( object    sender ,
                                                            EventArgs e      )
        {
            ListMatches( textBox_Breakpoint_Search.Text );
        }

        private void listBox_Breakpoint_SearchResults_Click( object sender, EventArgs e )
        {
            ImageInformation imageInformation = m_owner.ImageInformation;

            if(imageInformation != null)
            {
                string txt = (string)listBox_Breakpoint_SearchResults.SelectedItem;

                if(txt != null)
                {
                    if(imageInformation.NameToCfg.ContainsKey( txt ))
                    {
                        foreach(IR.ControlFlowGraphStateForCodeTransformation cfg in imageInformation.NameToCfg[txt])
                        {
                            IR.ImageBuilders.SequentialRegion reg = imageInformation.ImageBuilder.GetAssociatedRegion( cfg );
                            if(reg != null)
                            {
                                IR.ImageBuilders.SequentialRegion reg2;
                                uint                              offset2;
                                Debugging.DebugInfo               di;

                                if(m_owner.ImageInformation.LocateFirstSourceCode( reg.ExternalAddress, out reg2, out offset2, out di ))
                                {
                                    m_owner.VisualizeDebugInfo( di );
                                    return;
                                }
                            }
                        }
                    }

                    if(imageInformation.SourceCodeFiles.Contains( txt ))
                    {
                        m_owner.VisualizeFile( txt );
                        return;
                    }
                }
            }
        }

        private void listBox_Breakpoint_SearchResults_DoubleClick( object sender, EventArgs e )
        {
            CreateBreakpoint( (string)listBox_Breakpoint_SearchResults.SelectedItem );
        }

        private void dataGridView_Breakpoints_CellContentClick( object sender, DataGridViewCellEventArgs e )
        {
            SelectBreakpoint( e.RowIndex );

            UpdateUI();
        }

        private void dataGridView_Breakpoints_CellContentDoubleClick( object sender, DataGridViewCellEventArgs e )
        {
            SelectBreakpoint( e.RowIndex );

            if(m_selectedBreakpoint != null)
            {
                if(e.ColumnIndex == 0)
                {
                    m_selectedBreakpoint.IsActive = !m_selectedBreakpoint.IsActive;
                }
                else
                {
                    IR.ImageBuilders.SequentialRegion reg;
                    uint                              offset;
                    Debugging.DebugInfo               di;

                    if(m_owner.ImageInformation.LocateFirstSourceCode( m_selectedBreakpoint.Address, out reg, out offset, out di ))
                    {
                        m_owner.VisualizeDebugInfo( di );
                    }
                }
            }

            m_owner.Action_RefreshBreakpoints();
        }

        private void toolStripButton_DeleteBreakpoint_Click( object sender, EventArgs e )
        {
            m_owner.Action_RemoveBreakpoint( m_selectedBreakpoint );
        }

        private void toolStripButton_DeleteAllBreakpoints_Click( object sender, EventArgs e )
        {
            m_owner.Action_DeleteAllBreakpoints();
        }

        private void toolStripButton_ToggleAllBreakpoints_Click( object sender, EventArgs e )
        {
            ProcessorHost svc; m_owner.Host.GetHostingService( out svc );

            foreach(Emulation.Hosting.Breakpoint bp in svc.Breakpoints)
            {
                bp.IsActive = !bp.IsActive;
            }

            m_owner.Action_RefreshBreakpoints();
        }
    }
}
