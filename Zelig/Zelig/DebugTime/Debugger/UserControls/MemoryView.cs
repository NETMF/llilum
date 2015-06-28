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


    public partial class MemoryView : UserControl
    {
        public enum ViewMode
        {
            Bytes,
            Shorts,
            Words,
            Longs,
            Chars,
            Floats,
            Doubles,
        }

        private class LayoutContext
        {
            //
            // State
            //

            MemoryView          m_control;
            MemoryDelta         m_memoryDelta;

            GraphicsContext     m_ctx;
            int                 m_width;
            int                 m_height;
            float               m_stepX;
            float               m_stepY;
            PointF              m_ptLine;
            PointF              m_ptWord;
                               
            ContainerVisualItem m_topElement;
            ContainerVisualItem m_lineItem;

            //
            // Constructor Methods
            //

            internal LayoutContext( MemoryView          control     ,
                                    MemoryDelta         memoryDelta ,
                                    ContainerVisualItem topElement  )
            {
                m_control     = control;
                m_memoryDelta = memoryDelta;

                m_ctx         = control.codeView1.CtxForLayout;
                m_width       = control.codeView1.Width;
                m_height      = control.codeView1.Height;
                m_stepX       = m_ctx.CharSize  ( null );
                m_stepY       = m_ctx.CharHeight( null );
                            
                m_topElement  = topElement;
            }

            //
            // Helper Methods
            //

            internal void Install( CodeView codeView )
            {
                VisualTreeInfo vti = new VisualTreeInfo( null, null, m_topElement );

                codeView.InstallVisualTree( vti, null );
            }

            //--//

            internal void CreateScalarView( uint     address ,
                                            ViewMode mode    )
            {
                uint baseAddress = address;

                if(CanPlaceLine( 2 ))
                {
                    ContainerVisualItem lineItemPrevious = m_lineItem;
                    bool                fHex             = m_control.checkBox_ShowAsHex.Checked;

                    AddText( "Previous", "Click Here To View More...", Brushes.DarkGreen, 2, 2 );

                    while(CanPlaceLine( 1 ))
                    {
                        AddText( null, string.Format( "0x{0:X8}:", address ), Brushes.Blue, 1, 1 );

                        while(true)
                        {
                            TextVisualItem item     = null;
                            uint           size     = 4;
                            bool           fChanged = false;

                            switch(mode)
                            {
                                case ViewMode.Bytes:
                                    {
                                        byte result;

                                        if(m_memoryDelta.GetUInt8( address, true, out result, out fChanged ))
                                        {
                                            Brush brush = fChanged ? Brushes.Red : Brushes.Black;

                                            item = AddText( null, string.Format( fHex ? "0x{0:X2}" : "{0}", result ), brush, 1, 1 );
                                            size = sizeof(byte);
                                        }
                                    }
                                    break;

                                case ViewMode.Shorts:
                                    {
                                        ushort result;

                                        if(m_memoryDelta.GetUInt16( address, true, out result, out fChanged ))
                                        {
                                            Brush brush = fChanged ? Brushes.Red : Brushes.Black;

                                            item = AddText( null, string.Format( fHex ? "0x{0:X4}" : "{0}", result ), brush, 1, 1 );
                                            size = sizeof(ushort);
                                        }
                                    }
                                    break;

                                case ViewMode.Words:
                                    {
                                        uint result;

                                        if(m_memoryDelta.GetUInt32( address, true, out result, out fChanged ))
                                        {
                                            Brush brush = fChanged ? Brushes.Red : Brushes.Black;

                                            item = AddText( new WatchHelper.PointerContext( result, null, true ), string.Format( fHex ? "0x{0:X8}" : "{0}", result ), brush, 1, 1 );
                                            size = sizeof(uint);
                                        }
                                    }
                                    break;

                                case ViewMode.Longs:
                                    {
                                        ulong result;

                                        if(m_memoryDelta.GetUInt64( address, true, out result, out fChanged ))
                                        {
                                            Brush brush = fChanged ? Brushes.Red : Brushes.Black;

                                            item = AddText( null, string.Format( fHex ? "0x{0:X16}" : "{0}", result ), brush, 1, 1 );
                                            size = sizeof(ulong);
                                        }
                                    }
                                    break;

                                case ViewMode.Floats:
                                    {
                                        uint result;

                                        if(m_memoryDelta.GetUInt32( address, true, out result, out fChanged ))
                                        {
                                            Brush brush = fChanged ? Brushes.Red : Brushes.Black;

                                            item = AddText( null, string.Format( "{0}", DataConversion.GetFloatFromBytes( result ) ), brush, 1, 1 );
                                            size = sizeof(uint);
                                        }
                                    }
                                    break;

                                case ViewMode.Doubles:
                                    {
                                        ulong result;

                                        if(m_memoryDelta.GetUInt64( address, true, out result, out fChanged ))
                                        {
                                            Brush brush = fChanged ? Brushes.Red : Brushes.Black;

                                            item = AddText( null, string.Format( "{0}", DataConversion.GetDoubleFromBytes( result ) ), brush, 1, 1 );
                                            size = sizeof(ulong);
                                        }
                                    }
                                    break;

                                case ViewMode.Chars:
                                    {
                                        ushort result;
                                        char   ch;
                                        Brush  brush;

                                        if(m_memoryDelta.GetUInt16( address, true, out result, out fChanged ))
                                        {
                                            ch = (char)result;

                                            if(ch < 20 || ch >= 128)
                                            {
                                                ch = '.';
                                            }

                                            brush = fChanged ? Brushes.Red : Brushes.Black;
                                        }
                                        else
                                        {
                                            ch     = '?';
                                            brush  = Brushes.Red;
                                        }

                                        item = AddText( null, string.Format( "{0}", ch ), brush, 0, 0.5f );
                                        size = sizeof(ushort);
                                    }
                                    break;
                            }

                            if(item == null)
                            {
                                item = AddText( null, "????????", Brushes.Red, 0, 1 );
                            }

                            if(m_ptWord.X >= m_width)
                            {
                                m_lineItem.Remove( item );
                                break;
                            }

                            address += size;
                        }
                    }

                    PlaceLine();

                    ContainerVisualItem lineItemNext = m_lineItem;

                    AddText( "Next", "Click Here To View More...", Brushes.DarkGreen, 2, 2 );

                    //--//

                    uint addressSpan     = address - baseAddress;
                    uint previousAddress = baseAddress - (addressSpan) / 2;
                    uint nextAddress     = address     + (addressSpan) / 2;

                    m_control.codeView1.FallbackHitSink = delegate( CodeView owner, VisualItem origin, PointF relPos, MouseEventArgs e, bool fDown, bool fUp )
                    {
                        if(ProcessButton( origin, e, fDown, fUp ))
                        {
                            return;
                        }

                        if(e.Delta > 0)
                        {
                            m_control.MoveToAddress( previousAddress, false );
                            return;
                        }

                        if(e.Delta < 0)
                        {
                            m_control.MoveToAddress( nextAddress, false );
                            return;
                        }
                    };

                    lineItemPrevious.HitSink = delegate( CodeView owner, VisualItem origin, PointF relPos, MouseEventArgs e, bool fDown, bool fUp )
                    {
                        if(fDown)
                        {
                            if((e.Button & MouseButtons.Left) != 0)
                            {
                                m_control.MoveToAddress( previousAddress, false );
                            }
                        }
                    };

                    lineItemNext.HitSink = delegate( CodeView owner, VisualItem origin, PointF relPos, MouseEventArgs e, bool fDown, bool fUp )
                    {
                        if(fDown)
                        {
                            if((e.Button & MouseButtons.Left) != 0)
                            {
                                m_control.MoveToAddress( nextAddress, false );
                            }
                        }
                    };
                }
            }

            //--//

            private bool ProcessButton( VisualItem     origin ,
                                        MouseEventArgs e      ,
                                        bool           fDown  ,
                                        bool           fUp    )
            {
                if(fDown)
                {
                    if((e.Button & MouseButtons.XButton1) != 0)
                    {
                        m_control.MoveThroughHistory( -1 );
                    }

                    if((e.Button & MouseButtons.XButton2) != 0)
                    {
                        m_control.MoveThroughHistory( +1 );
                    }

                    if((e.Button & MouseButtons.Left) != 0)
                    {
                        if(e.Clicks == 2 && origin != null)
                        {
                            WatchHelper.PointerContext ct = origin.Context as WatchHelper.PointerContext;
                            if(ct != null)
                            {
                                m_control.MoveToAddress( ct.Address, true );
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            //--//

            private void SkipX( float amount )
            {
                m_ptLine.X += m_stepX * amount;
            }

            private void SkipY( float amount )
            {
                m_ptLine.Y += m_stepY * amount;
            }

            private bool CanPlaceLine( int extraLines )
            {
                if(m_ptLine.Y + (m_stepY * (1 + extraLines)) >= m_height)
                {
                    return false;
                }

                PlaceLine();

                return true;
            }

            private ContainerVisualItem PlaceLine()
            {
                m_lineItem = new ContainerVisualItem( null );

                m_lineItem.RelativeOrigin = m_ptLine;

                m_ptLine.Y += m_stepY;
                m_ptWord    = new PointF();

                m_topElement.Add( m_lineItem );

                return m_lineItem;
            }

            private TextVisualItem AddText( object context ,
                                            string text    ,
                                            Brush  brush   )
            {
                return AddText( context, text, brush, 0, 0 );
            }

            private TextVisualItem AddText( object context ,
                                            string text    ,
                                            Brush  brush   ,
                                            float  preX    ,
                                            float  postX   )
            {
                TextVisualItem item = new TextVisualItem( context, text );

                item.TextBrush = brush;

                BaseTextVisualItem.PlaceInALine( m_ctx, item, ref m_ptWord, preX, postX );

                m_lineItem.Add( item );

                return item;
            }
        }

        //
        // State
        //

        DebuggerMainForm                   m_owner;
        ViewMode                           m_currentViewMode;
        WatchHelper.PointerContext         m_currentPointer;
        MemoryDelta                        m_memoryDelta;

        List< WatchHelper.PointerContext > m_history;
        int                                m_historyPosition;

        //
        // Constructor Methods
        //

        public MemoryView()
        {
            InitializeComponent();

            //--//

            m_currentViewMode = ViewMode.Words;

            m_history         = new List< WatchHelper.PointerContext >();
            m_historyPosition = -1;

            comboBox_ViewMode.SelectedIndex = (int)m_currentViewMode;
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
            GenerateView();

            return Hst.Forms.HostingSite.NotificationResponse.DoNothing;
        }

        private void GenerateView()
        {
            if(m_currentPointer != null)
            {
                m_memoryDelta = m_owner.MemoryDelta;

                LayoutContext lc = new LayoutContext( this, m_memoryDelta, new ContainerVisualItem( null ) );

                lc.CreateScalarView( m_currentPointer.Address, m_currentViewMode );

                lc.Install( codeView1 );
            }
            else
            {
                codeView1.InstallVisualTree( null, null );
            }
        }

        //--//

        public void MoveThroughHistory( int delta )
        {
            int nextHistoryPosition = m_historyPosition + delta;

            if(nextHistoryPosition >= 0 && nextHistoryPosition < m_history.Count)
            {
                m_currentPointer  = m_history[nextHistoryPosition];
                m_historyPosition = nextHistoryPosition;

                GenerateView();
            }
        }

        public void MoveToAddress( uint address        ,
                                   bool fUpdateHistory )
        {
            if(m_currentPointer         != null    &&
               m_currentPointer.Address == address  )
            {
                return;
            }

            if(fUpdateHistory)
            {
                int size    = m_history.Count;
                int newSize = m_historyPosition + 1;

                if(newSize < size)
                {
                    m_history.RemoveRange( newSize, size - newSize );
                }

                m_currentPointer = null;
            }

            if(m_currentPointer == null)
            {
                m_currentPointer = new WatchHelper.PointerContext( address, null, false );

                m_historyPosition = m_history.Count;
                m_history.Add( m_currentPointer );
            }
            else
            {
                m_currentPointer.Address = address;
            }

            GenerateView();
        }

        private void ValidateNewAddress()
        {
            uint   address;
            string text = this.textBox_Address.Text;

            if(text.ToUpper().StartsWith( "0X" ))
            {
                if(uint.TryParse( text.Substring( 2 ), System.Globalization.NumberStyles.AllowHexSpecifier, null, out address ))
                {
                    MoveToAddress( address, true );
                    return;
                }
            }

            if(uint.TryParse( text, out address ))
            {
                MoveToAddress( address, true );
                return;
            }

            //--//

            Emulation.Hosting.ProcessorStatus ps;  m_owner.Host.GetHostingService( out ps  );
            Emulation.Hosting.MemoryProvider  mem; m_owner.Host.GetHostingService( out mem );

            //--//

            StackFrame                      currentStackFrame = m_owner.SelectedStackFrame;
            IR.LowLevelVariableExpression[] array             = m_owner.ImageInformation.AliveVariables( currentStackFrame.Region, currentStackFrame.RegionOffset );

            foreach(IR.LowLevelVariableExpression var in array)
            {
                string name;
                object val;

                if(var is IR.PhysicalRegisterExpression)
                {
                    IR.PhysicalRegisterExpression varReg = var as IR.PhysicalRegisterExpression;

                    if(varReg.RegisterDescriptor.InIntegerRegisterFile == false)
                    {
                        continue;
                    }

                    val = ps.GetRegister( varReg.RegisterDescriptor );

                    IR.VariableExpression varSrc = var.SourceVariable;
                    if(varSrc != null && varSrc.DebugName != null)
                    {
                        name = varSrc.DebugName.Name;
                    }
                    else
                    {
                        name = string.Format( "$Reg( {0} )", varReg.RegisterDescriptor.Mnemonic );
                    }
                }
                else if(var is IR.StackLocationExpression)
                {
                    IR.StackLocationExpression varStack = var as IR.StackLocationExpression;
                    uint                       sp       = ps.StackPointer;

                    uint memVal;

                    mem.GetUInt32( sp + varStack.AllocationOffset, out memVal );

                    val = memVal;

                    IR.VariableExpression varSrc = var.SourceVariable;
                    if(varSrc != null && varSrc.DebugName != null)
                    {
                        name = varSrc.DebugName.Name;
                    }
                    else
                    {
                        name = string.Format( "$Stack(0x{0:X8})", varStack.AllocationOffset );
                    }
                }
                else
                {
                    continue;
                }

                if(name == text && val is uint)
                {
                    MoveToAddress( (uint)val, true );
                    return;
                }
            }

            //--//

            m_currentPointer = null;

            GenerateView();

            this.textBox_Address.Focus();
        }

        private void SwitchViewMode( ViewMode mode )
        {
            m_currentViewMode = mode;

            comboBox_ViewMode.SelectedIndex = (int)m_currentViewMode;
        }

        //
        // Access Methods
        //

        //
        // Event Methods
        //

        private void button_Refresh_Click( object sender, EventArgs e )
        {
            ValidateNewAddress();
        }

        private void codeView1_SizeChanged( object sender, EventArgs e )
        {
            GenerateView();
        }

        private void comboBox_ViewMode_SelectedIndexChanged( object sender, EventArgs e )
        {
            ViewMode newViewMode = (ViewMode)comboBox_ViewMode.SelectedIndex;

            if(m_currentViewMode != newViewMode)
            {
                m_currentViewMode = newViewMode;

                GenerateView();

                codeView1.Focus();
            }
        }

        private void textBox_Address_KeyDown( object sender, KeyEventArgs e )
        {
            if(e.KeyCode == Keys.Return)
            {
                ValidateNewAddress();
            }
        }

        private void checkBox_ShowAsHex_CheckedChanged( object sender, EventArgs e )
        {
            GenerateView();
        }
    }
}
