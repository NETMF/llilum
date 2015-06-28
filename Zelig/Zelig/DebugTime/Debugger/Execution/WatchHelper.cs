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

    using EncDef             = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_ARM;
    using InstructionSet     = Microsoft.Zelig.TargetModel.ArmProcessor.InstructionSet;
    using IR                 = Microsoft.Zelig.CodeGeneration.IR;
    using RT                 = Microsoft.Zelig.Runtime;
    using TS                 = Microsoft.Zelig.Runtime.TypeSystem;


    public class WatchHelper
    {
        public class PointerContext
        {
            //
            // State
            //

            private uint                  m_address;
            private TS.TypeRepresentation m_type;
            private bool                  m_fCheckVirtualTable;

            //
            // Constructor Methods
            //

            public PointerContext( uint                  address           ,
                                   TS.TypeRepresentation td                ,
                                   bool                  checkVirtualTable )
            {
                m_address            = address;
                m_type               = td;
                m_fCheckVirtualTable = checkVirtualTable;
            }

            //
            // Helper Methods
            //

            public PointerContext FixType( MemoryDelta memoryDelta )
            {
                uint                  address  = m_address;
                TS.TypeRepresentation td       = m_type ?? memoryDelta.LocateType( ref address );

                if(m_fCheckVirtualTable)
                {
                    if(td is TS.ReferenceTypeRepresentation)
                    {
                        TS.TypeRepresentation tdFromVtable = memoryDelta.LookForVirtualTable( address );

                        if(tdFromVtable != null)
                        {
                            td = tdFromVtable;
                        }
                    }
                }

                if(td == null)
                {
                    td = memoryDelta.LookForVirtualTable( address );
                }

                if(td == m_type && address == m_address)
                {
                    return this;
                }

                return new PointerContext( address, td, false );
            }

            //
            // Access Methods
            //

            public uint Address
            {
                get
                {
                    return m_address;
                }

                set
                {
                    m_address = value;
                }
            }
    
            public TS.TypeRepresentation Type
            {
                get
                {
                    return m_type;
                }
            }
    
            public bool CheckVirtualTable
            {
                get
                {
                    return m_fCheckVirtualTable;
                }
            }
        }

        //--//

        public sealed class ItemDescriptor
        {
            //
            // State
            //

            public  readonly WatchHelper                Owner;
            public  readonly string                     Name;
            public  readonly TS.TypeRepresentation      Type;
            public           AbstractValueHandle        Location;

            private          string                     m_valueText;
            private          string                     m_typeDescriptor;
                                                
            private          AbstractValueHandle        m_subLocation;
            private          bool                       m_fTopView;

            private          TreeBasedGridView.GridNode m_node;
                                       

            //
            // Constructor Methods
            //

            public ItemDescriptor( WatchHelper           owner    ,
                                   string                name     ,
                                   TS.TypeRepresentation type     ,
                                   AbstractValueHandle   location ) : this( owner, name, type, location, type.FullNameWithAbbreviation )
            {
            }

            public ItemDescriptor( WatchHelper           owner          ,
                                   string                name           ,
                                   TS.TypeRepresentation type           ,
                                   AbstractValueHandle   location       ,
                                   string                typeDescriptor )
            {
                this.Owner    = owner;
                this.Name     = name;
                this.Type     = type;
                this.Location = location;

                m_typeDescriptor = typeDescriptor;
                m_fTopView       = true;
            }

            //
            // Helper Methods
            //

            internal void AttachToView( TreeBasedGridView.GridNode                   parentNode ,
                                        LinkedListNode< TreeBasedGridView.GridNode > anchor     ,
                                        Icon                                         icon       )
            {
                bool fChanged;

                this.ConvertToText( out fChanged );

                TreeBasedGridView.GridNode newNode;
                
                if(anchor != null)
                {
                    newNode = anchor.Value.AddBefore( Name, m_valueText, m_typeDescriptor );
                }
                else
                {
                    newNode = parentNode.AddChild( Name, m_valueText, m_typeDescriptor );
                }

                m_node             = newNode;
                newNode.Tag        = this;
                newNode.Icon       = icon;
                newNode.HasChanged = fChanged;

                if(m_subLocation != null)
                {
                    newNode.ExpandCallback = this.Owner.ExpandValue;
                }
                
                if(CanDisplayAsSingleItem( this.Type ))
                {
                    var loc = this.Location;

                    if(loc != null && loc.CanUpdate)
                    {
                        newNode.UpdateCallback = this.Owner.UpdateValue;
                    }
                }
            }

            internal bool IsEquivalent( ItemDescriptor item )
            {
                if(this.Name == item.Name &&
                   this.Type == item.Type  )
                {
                    if(this.Location.IsEquivalent( item.Location ))
                    {
                        return true;
                    }
                }

                return false;
            }

            internal void UpdateText()
            {
                bool fChanged;

                this.ConvertToText( out fChanged );

                if(m_node != null)
                {
                    m_node[1]         = m_valueText;
                    m_node.HasChanged = fChanged;
                }
            }

            internal void CollapseIfChanged()
            {
                if(m_node != null)
                {
                    bool fChanged;

                    var previousText = m_valueText;

                    this.ConvertToText( out fChanged );

                    if(fChanged || previousText != m_valueText)
                    {
                        m_node.Invalidate();

                        m_node[1] = m_valueText;

                        if(m_subLocation != null)
                        {
                            m_node.ExpandCallback = this.Owner.ExpandValue;
                        }
                        else
                        {
                            m_node.ExpandCallback = null;
                        }
                    }

                    m_node.HasChanged = fChanged;
                }
            }

            //--//

            public void ConvertToText( out bool fChanged )
            {
                m_subLocation = null;

                if(CanDisplayAsSingleItem( this.Type ))
                {
                    var locReg = this.Location as RegisterValueHandle;
                    if(locReg != null)
                    {
                        if((locReg.RegisterValue.Register.PhysicalClass & IR.Abstractions.RegisterClass.StatusRegister) != 0)
                        {
                            var bb = locReg.Read( out fChanged );

                            m_valueText = (bb == null) ? "<scratched>" : DecodePSR( bb.ReadUInt32( 0 ) );
                            return;
                        }
                    }

                    if(FormatItem( out fChanged ) == false)
                    {
                        if(locReg != null)
                        {
                            m_valueText = "<scratched>";
                        }
                        else
                        {
                            m_valueText = "<unable to access>";
                        }
                    }
                    return;
                }

                fChanged      = false;
                m_subLocation = this.Location;

                return;
            }

            public bool ConvertFromText( string value )
            {
                if(CanDisplayAsSingleItem( this.Type ))
                {
                    var locReg = this.Location as RegisterValueHandle;
                    if(locReg != null)
                    {
                        if((locReg.RegisterValue.Register.PhysicalClass & IR.Abstractions.RegisterClass.StatusRegister) != 0)
                        {
                            return false;
                        }
                    }

                    if(ParseItem( value ))
                    {
                        return true;
                    }
                }

                return false;
            }

            //--//

            private static string DecodePSR( uint psr )
            {
                var sb = new StringBuilder();

                sb.AppendFormat( "<{0}>", InstructionSet.DumpMode( psr ) );

                if((psr & EncDef.c_psr_N) != 0) sb.Append( " N"         );
                if((psr & EncDef.c_psr_Z) != 0) sb.Append( " Z"         );
                if((psr & EncDef.c_psr_C) != 0) sb.Append( " C"         );
                if((psr & EncDef.c_psr_V) != 0) sb.Append( " V"         );
                if((psr & EncDef.c_psr_T) != 0) sb.Append( " <Thumb>"   );
                if((psr & EncDef.c_psr_F) != 0) sb.Append( " <FIQ off>" );
                if((psr & EncDef.c_psr_I) != 0) sb.Append( " <IRQ off>" );

                return sb.ToString();
            }

            //--//

            private static bool CanDisplayAsSingleItem( TS.TypeRepresentation td )
            {
                if(td.IsNumeric || td is TS.EnumerationTypeRepresentation)
                {
                    return true;
                }

                if(td is TS.PointerTypeRepresentation    ||
                   td is TS.BoxedValueTypeRepresentation  )
                {
                    return true;
                }

                if(td is TS.ValueTypeRepresentation)
                {
                    return false;
                }

                if(td is TS.ReferenceTypeRepresentation)
                {
                    return true;
                }

                return false;
            }

            private bool ParseItem( string value )
            {
                var td = this.Type;

                if(td is TS.EnumerationTypeRepresentation)
                {
                    var tdEnum = (TS.EnumerationTypeRepresentation)td;
                    var tdVal  = tdEnum.UnderlyingType;

                    if(!this.ShowHex)
                    {
////                    var bb = this.Location.Read( out fChanged );
////                    if(bb == null)
////                    {
////                        return false;
////                    }
////
////                    switch(tdVal.BuiltInType)
////                    {
////                        case TS.TypeRepresentation.BuiltInTypes.CHAR: FormatEnum( tdEnum, (char )bb.ReadUInt16( 0 ) ); return true;
////                        case TS.TypeRepresentation.BuiltInTypes.I1  : FormatEnum( tdEnum, (sbyte)bb.ReadUInt8 ( 0 ) ); return true;
////                        case TS.TypeRepresentation.BuiltInTypes.U1  : FormatEnum( tdEnum,        bb.ReadUInt8 ( 0 ) ); return true;
////                        case TS.TypeRepresentation.BuiltInTypes.I2  : FormatEnum( tdEnum, (short)bb.ReadUInt16( 0 ) ); return true;
////                        case TS.TypeRepresentation.BuiltInTypes.U2  : FormatEnum( tdEnum,        bb.ReadUInt16( 0 ) ); return true;
////                        case TS.TypeRepresentation.BuiltInTypes.I4  : FormatEnum( tdEnum, (int  )bb.ReadUInt32( 0 ) ); return true;
////                        case TS.TypeRepresentation.BuiltInTypes.U4  : FormatEnum( tdEnum,        bb.ReadUInt32( 0 ) ); return true;
////                        case TS.TypeRepresentation.BuiltInTypes.I8  : FormatEnum( tdEnum, (long )bb.ReadUInt64( 0 ) ); return true;
////                        case TS.TypeRepresentation.BuiltInTypes.U8  : FormatEnum( tdEnum,        bb.ReadUInt64( 0 ) ); return true;
////                    }
                        return false;
                    }

                    //
                    // Fallback to scalar view.
                    //
                    td = tdVal;
                }

                value = value.Trim();

                if(value.ToLower().StartsWith( "0x" ))
                {
                    value = value.Substring( 2 );

                    if(td.IsNumeric)
                    {
                        switch(td.BuiltInType)
                        {
                            case TS.TypeRepresentation.BuiltInTypes.BOOLEAN:
                                {
                                    uint val;

                                    if(uint.TryParse( value, System.Globalization.NumberStyles.HexNumber, null, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.CHAR:
                                {
                                    ushort val;

                                    if(ushort.TryParse( value, System.Globalization.NumberStyles.HexNumber, null, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.I1:
                                {
                                    sbyte val;

                                    if(sbyte.TryParse( value, System.Globalization.NumberStyles.HexNumber, null, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.U1:
                                {
                                    byte val;

                                    if(byte.TryParse( value, System.Globalization.NumberStyles.HexNumber, null, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.I2:
                                {
                                    short val;

                                    if(short.TryParse( value, System.Globalization.NumberStyles.HexNumber, null, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.U2:
                                {
                                    ushort val;

                                    if(ushort.TryParse( value, System.Globalization.NumberStyles.HexNumber, null, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.I4:
                                {
                                    int val;

                                    if(int.TryParse( value, System.Globalization.NumberStyles.HexNumber, null, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.U4:
                                {
                                    uint val;

                                    if(uint.TryParse( value, System.Globalization.NumberStyles.HexNumber, null, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.I8:
                                {
                                    long val;

                                    if(long.TryParse( value, System.Globalization.NumberStyles.HexNumber, null, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.U8:
                                {
                                    ulong val;

                                    if(ulong.TryParse( value, System.Globalization.NumberStyles.HexNumber, null, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.R4:
                                {
                                    uint val;

                                    if(uint.TryParse( value, System.Globalization.NumberStyles.HexNumber, null, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.R8:
                                {
                                    ulong val;

                                    if(ulong.TryParse( value, System.Globalization.NumberStyles.HexNumber, null, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.I:
                            case TS.TypeRepresentation.BuiltInTypes.U:
                                {
                                    uint val;

                                    if(uint.TryParse( value, System.Globalization.NumberStyles.HexNumber, null, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;
                        }

                        return false;
                    }

                    if(td is TS.PointerTypeRepresentation    ||
                       td is TS.BoxedValueTypeRepresentation ||
                       td is TS.ReferenceTypeRepresentation   )
                    {
                        uint val;

                        if(uint.TryParse( value, System.Globalization.NumberStyles.HexNumber, null, out val ))
                        {
                            return WriteDirect( val );
                        }

                        return false;
                    }
                }
                else
                {
                    if(td.IsNumeric)
                    {
                        switch(td.BuiltInType)
                        {
                            case TS.TypeRepresentation.BuiltInTypes.BOOLEAN:
                                {
                                    bool val;

                                    if(bool.TryParse( value, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.CHAR:
                                {
                                    char val;

                                    if(char.TryParse( value, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.I1:
                                {
                                    sbyte val;

                                    if(sbyte.TryParse( value, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.U1:
                                {
                                    byte val;

                                    if(byte.TryParse( value, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.I2:
                                {
                                    short val;

                                    if(short.TryParse( value, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.U2:
                                {
                                    ushort val;

                                    if(ushort.TryParse( value, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.I4:
                                {
                                    int val;

                                    if(int.TryParse( value, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.U4:
                                {
                                    uint val;

                                    if(uint.TryParse( value, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.I8:
                                {
                                    long val;

                                    if(long.TryParse( value, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.U8:
                                {
                                    ulong val;

                                    if(ulong.TryParse( value, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.R4:
                                {
                                    float val;

                                    if(float.TryParse( value, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;

                            case TS.TypeRepresentation.BuiltInTypes.R8:
                                {
                                    double val;

                                    if(double.TryParse( value, out val ))
                                    {
                                        return WriteDirect( val );
                                    }
                                }
                                break;
                        }
                    }
                }

                return false;
            }

            private bool WriteDirect( object val )
            {
                return this.Location.Write( Emulation.Hosting.BinaryBlob.Wrap( val ) );
            }

            private bool FormatItem( out bool fChanged )
            {
                var td  = this.Type;
                var loc = this.Location;

                if(td is TS.EnumerationTypeRepresentation)
                {
                    var tdEnum = (TS.EnumerationTypeRepresentation)td;
                    var tdVal  = tdEnum.UnderlyingType;

                    if(!this.ShowHex)
                    {
                        var bb = loc.Read( out fChanged );
                        if(bb == null)
                        {
                            return false;
                        }

                        switch(tdVal.BuiltInType)
                        {
                            case TS.TypeRepresentation.BuiltInTypes.CHAR: FormatEnum( tdEnum, (char )bb.ReadUInt16( 0 ) ); return true;
                            case TS.TypeRepresentation.BuiltInTypes.I1  : FormatEnum( tdEnum, (sbyte)bb.ReadUInt8 ( 0 ) ); return true;
                            case TS.TypeRepresentation.BuiltInTypes.U1  : FormatEnum( tdEnum,        bb.ReadUInt8 ( 0 ) ); return true;
                            case TS.TypeRepresentation.BuiltInTypes.I2  : FormatEnum( tdEnum, (short)bb.ReadUInt16( 0 ) ); return true;
                            case TS.TypeRepresentation.BuiltInTypes.U2  : FormatEnum( tdEnum,        bb.ReadUInt16( 0 ) ); return true;
                            case TS.TypeRepresentation.BuiltInTypes.I4  : FormatEnum( tdEnum, (int  )bb.ReadUInt32( 0 ) ); return true;
                            case TS.TypeRepresentation.BuiltInTypes.U4  : FormatEnum( tdEnum,        bb.ReadUInt32( 0 ) ); return true;
                            case TS.TypeRepresentation.BuiltInTypes.I8  : FormatEnum( tdEnum, (long )bb.ReadUInt64( 0 ) ); return true;
                            case TS.TypeRepresentation.BuiltInTypes.U8  : FormatEnum( tdEnum,        bb.ReadUInt64( 0 ) ); return true;
                        }
                    }

                    //
                    // Fallback to scalar view.
                    //
                    td = tdVal;
                }

                if(td.IsNumeric)
                {
                    var bb = loc.Read( out fChanged );
                    if(bb == null)
                    {
                        return false;
                    }

                    switch(td.BuiltInType)
                    {
                        case TS.TypeRepresentation.BuiltInTypes.BOOLEAN:
                            {
                                byte val = bb.ReadUInt8( 0 );

                                m_valueText = string.Format( this.ShowHex ? "0x{1:X2}" : "{0}", val != 0, val );
                                return true;
                            }

                        case TS.TypeRepresentation.BuiltInTypes.CHAR:
                            {
                                ushort val = bb.ReadUInt16( 0 );

                                m_valueText = string.Format( this.ShowHex ? "0x{1:X4}" : "'{0}' 0x{1:X4}", (char)val, val );
                                return true;
                            }

                        case TS.TypeRepresentation.BuiltInTypes.I1:
                            {
                                byte val = bb.ReadUInt8( 0 );

                                m_valueText = string.Format( this.ShowHex ? "0x{1:X2}" : "{0}", (sbyte)val, val );
                                return true;
                            }

                        case TS.TypeRepresentation.BuiltInTypes.U1:
                            {
                                byte val = bb.ReadUInt8( 0 );

                                m_valueText = string.Format( this.ShowHex ? "0x{1:X2}" : "{0}", val, val );
                                return true;
                            }

                        case TS.TypeRepresentation.BuiltInTypes.I2:
                            {
                                ushort val = bb.ReadUInt16( 0 );

                                m_valueText = string.Format( this.ShowHex ? "0x{1:X4}" : "{0}", (short)val, val );
                                return true;
                            }

                        case TS.TypeRepresentation.BuiltInTypes.U2:
                            {
                                ushort val = bb.ReadUInt16( 0 );

                                m_valueText = string.Format( this.ShowHex ? "0x{1:X4}" : "{0}", val, val );
                                return true;
                            }

                        case TS.TypeRepresentation.BuiltInTypes.I4:
                            {
                                uint val = bb.ReadUInt32( 0 );

                                m_valueText = string.Format( this.ShowHex ? "0x{1:X8}" : "{0}", (int)val, val );
                                return true;
                            }

                        case TS.TypeRepresentation.BuiltInTypes.U4:
                            {
                                uint val = bb.ReadUInt32( 0 );

                                m_valueText = string.Format( this.ShowHex ? "0x{1:X8}" : "{0}", val, val );
                                return true;
                            }

                        case TS.TypeRepresentation.BuiltInTypes.I8:
                            {
                                ulong val = bb.ReadUInt64( 0 );

                                m_valueText = string.Format( this.ShowHex ? "0x{1:X16}" : "{0}", (long)val, val );
                                return true;
                            }

                        case TS.TypeRepresentation.BuiltInTypes.U8:
                            {
                                ulong val = bb.ReadUInt64( 0 );

                                m_valueText = string.Format( this.ShowHex ? "0x{1:X16}" : "{0}", val, val );
                                return true;
                            }

                        case TS.TypeRepresentation.BuiltInTypes.R4:
                            {
                                uint val = bb.ReadUInt32( 0 );

                                m_valueText = string.Format( this.ShowHex ? "0x{1:X4}" : "{0} (0x{1:X8})", DataConversion.GetFloatFromBytes( val ), val );
                                return true;
                            }

                        case TS.TypeRepresentation.BuiltInTypes.R8:
                            {
                                ulong val = bb.ReadUInt64( 0 );

                                m_valueText = string.Format( this.ShowHex ? "0x{1:X16}" : "{0} (0x{1:X16})", DataConversion.GetDoubleFromBytes( val ), val );
                                return true;
                            }

                        case TS.TypeRepresentation.BuiltInTypes.I:
                        case TS.TypeRepresentation.BuiltInTypes.U:
                            {
                                uint val = bb.ReadUInt32( 0 );

                                m_valueText = string.Format( "0x{0:X8}", val );
                                return true;
                            }
                    }

                    m_valueText = "<unable to access>";
                    return false;
                }

                if(td is TS.PointerTypeRepresentation    ||
                   td is TS.BoxedValueTypeRepresentation  )
                {
                    var bb = loc.Read( out fChanged );
                    if(bb == null)
                    {
                        return false;
                    }

                    uint address = bb.ReadUInt32( 0 );

                    if(address == 0)
                    {
                        m_valueText = "<null>";
                        return true;
                    }

                    m_valueText   = string.Format( "0x{0:X8}", address );
                    m_subLocation = new MemoryValueHandle( td.UnderlyingType, null, null, false, this.Owner.MemoryDelta, address ); 
                    return true;
                }

                if(td is TS.ValueTypeRepresentation)
                {
                    m_valueText = "";
                    m_subLocation = loc;
                    fChanged = false;
                    return true;
                }

                if(td is TS.ReferenceTypeRepresentation)
                {
                    if(loc.MemoryMappedRegister != null)
                    {
                        m_subLocation = loc;
                        fChanged = false;
                        return true;
                    }
                    else
                    {
                        var bb = loc.Read( out fChanged );
                        if(bb == null)
                        {
                            return false;
                        }

                        uint address = bb.ReadUInt32( 0 );

                        if(address == 0 || address == 0xdeadbeef)
                        {
                            m_valueText = "<null>";
                            return true;
                        }

                        td = FixType( td, ref address );

                        m_subLocation = new MemoryValueHandle( td, null, null, false, this.Owner.MemoryDelta, address );

                        if(td == this.Owner.ImageInformation.TypeSystem.WellKnownTypes.System_String)
                        {
                            m_valueText = this.Owner.MemoryDelta.ExtractString( address );

                            if(m_valueText != null)
                            {
                                m_valueText = '"' + m_valueText + '"';
                                return true;
                            }
                        }

                        m_valueText = string.Format( "0x{0:X8}", address );
                        return true;
                    }
                }

                m_valueText = "<unable to access>";
                fChanged = false;
                return true;
            }

            public TS.TypeRepresentation FixType(     TS.TypeRepresentation td      ,
                                                  ref uint                  address )
            {
                var memoryDelta = this.Owner.MemoryDelta;

                if(td == null)
                {
                    td = memoryDelta.LocateType( ref address );
                }

                if(td is TS.ReferenceTypeRepresentation && 0 == (td.ExpandedBuildTimeFlags & TS.TypeRepresentation.BuildTimeAttributes.NoVTable) )
                {
                    TS.TypeRepresentation tdFromVtable = memoryDelta.LookForVirtualTable( address );

                    if(tdFromVtable != null)
                    {
                        td = tdFromVtable;
                    }
                }

                if(td == null)
                {
                    td = memoryDelta.LookForVirtualTable( address );
                }

                return td;
            }

            private void FormatEnum( TS.EnumerationTypeRepresentation td  ,
                                     object                           val )
            {
                m_valueText = td.FormatValue( val );
            }

            //
            // Access Methods
            //

            public string ValueText
            {
                get
                {
                    return m_valueText;
                }
            }

            public string TypeDescriptor
            {
                get
                {
                    return m_typeDescriptor;
                }

                set
                {
                    m_typeDescriptor = value;
                }
            }

            public TreeBasedGridView.GridNode Node
            {
                get
                {
                    return m_node;
                }
            }

            public AbstractValueHandle SubLocation
            {
                get
                {
                    return m_subLocation;
                }

                set
                {
                    m_subLocation = value;
                }
            }

            public bool IsTopView
            {
                get
                {
                    return m_fTopView;
                }

                set
                {
                    m_fTopView = value;
                }
            }

            public bool ShowHex
            {
                get
                {
                    return this.Owner.HexadecimalDisplay;
                }
            }
        }
        
        //
        // State 
        //

        const int MaxArrayDisplay = 32;

        public  readonly ImageInformation           ImageInformation;
        public  readonly MemoryDelta                MemoryDelta;
        public  readonly TreeBasedGridView.GridNode RootNode;

        private          bool                       m_fHexadecimalDisplay;
        private          TreeBasedGridView.GridNode m_staticNode;
        private          TreeBasedGridView.GridNode m_singletonNode;

        //
        // Constructor Methods
        //

        private WatchHelper( MemoryDelta                memoryDelta ,
                             TreeBasedGridView.GridNode rootNode    )
        {
            this.ImageInformation   = memoryDelta.ImageInformation;
            this.MemoryDelta        = memoryDelta;
            this.RootNode           = rootNode;
        }

        //
        // Helper Methods
        //

        public static void Synchronize( ref WatchHelper                wh                  ,
                                            MemoryDelta                memoryDelta         ,
                                            TreeBasedGridView.GridNode rootNode            ,
                                            bool                       fAddStaticFields    ,
                                            bool                       fAddSingletonFields )
        {
            var imageInformation = memoryDelta.ImageInformation;

            if(imageInformation == null)
            {
                rootNode.Clear();
                wh = null;
                return;
            }

            if(wh == null || wh.ImageInformation != imageInformation || wh.RootNode != rootNode)
            {
                rootNode.Clear();

                wh = new WatchHelper( memoryDelta, rootNode );

                if(fAddStaticFields)
                {
                    wh.AddStaticFields( wh.RootNode );
                }

                if(fAddSingletonFields)
                {
                    wh.AddSingletonFields( wh.RootNode );
                }
            }
        }

        //--//

        public static void SetColumns( TreeBasedGridView treeBasedGridView )
        {
            treeBasedGridView.SetColumns( new TreeBasedGridView.GridColumnDefinition( "Name" , DataGridViewContentAlignment.MiddleLeft, true , false, false ) ,
                                          new TreeBasedGridView.GridColumnDefinition( "Value", DataGridViewContentAlignment.MiddleLeft, false, true , true  ) ,
                                          new TreeBasedGridView.GridColumnDefinition( "Type" , DataGridViewContentAlignment.MiddleLeft, false, false, false ) );
        }

        static bool UpdateText( TreeBasedGridView.GridNode node )
        {
            var item = node.Tag as ItemDescriptor;

            if(item != null)
            {
                item.UpdateText();
            }

            return true;
        }

        static bool CollapseIfChanged( TreeBasedGridView.GridNode node )
        {
            var item = node.Tag as ItemDescriptor;

            if(item != null)
            {
                item.CollapseIfChanged();
            }

            return true;
        }

        //--//

        public void Update( List< ItemDescriptor > lst            ,
                            bool                   fTryToPreserve )
        {
            foreach(var node in this.RootNode.ArrayOfChildNodesNoPopulate)
            {
                if(node == m_staticNode   ) continue;
                if(node == m_singletonNode) continue;

                bool fRemove = true;

                if(fTryToPreserve)
                {
                    var item = node.Tag as ItemDescriptor;
                    if(item != null)
                    {
                        foreach(var itemNew in lst)
                        {
                            if(item.IsEquivalent( itemNew ))
                            {
                                item.Location = itemNew.Location;

                                lst.Remove( itemNew );
                                fRemove = false;
                                break;
                            }
                        }
                    }
                }

                if(fRemove)
                {
                    node.Remove();
                }
            }

            var anchor = this.RootNode.ChildNodesNoPopulate.First;

            foreach(var item in lst)
            {
                item.AttachToView( this.RootNode, anchor, Properties.Resources.LocalVariable );
            }

            //--//

            var ctrl = this.RootNode.Owner;

            ctrl.StartTreeUpdate();

            ctrl.EnumerateNodesPreOrder( CollapseIfChanged );

            ctrl.EndTreeUpdate();
        }

        //--//

        private void AddStaticFields( TreeBasedGridView.GridNode node )
        {
            var exGlobalRoot = this.ImageInformation.TypeSystem.GlobalRoot;
            var pc           = this.ImageInformation.ResolveCostantExpression( this.MemoryDelta, exGlobalRoot );

            if(pc != null)
            {
                var set = SetFactory.NewWithReferenceEquality< TS.TypeRepresentation >();

                foreach(TS.InstanceFieldRepresentation fd in exGlobalRoot.Type.Fields)
                {
                    var fd2 = fd.ImplementationOf;
                    if(fd2 != null)
                    {
                        set.Insert( fd2.OwnerType );
                    }
                }

                if(set.Count > 0)
                {
                    var valHandleGlobalRoot = new MemoryValueHandle( pc, false );

                    m_staticNode = node.AddChild( "Static Fields", "", "" );

                    m_staticNode.Icon = Properties.Resources.StaticMembers;

                    foreach(var td in set)
                    {
                        TreeBasedGridView.GridNode staticNode2 = m_staticNode.AddChild( td.FullNameWithAbbreviation, "", "" );

                        staticNode2.Icon = Properties.Resources.StaticMembers;

                        foreach(TS.InstanceFieldRepresentation fd in exGlobalRoot.Type.Fields)
                        {
                            var fd2 = fd.ImplementationOf;
                            if(fd2 != null && fd2.OwnerType == td)
                            {
                                AddField( staticNode2, valHandleGlobalRoot, fd, fd2.Name );
                            }
                        }
                    }
                }
            }
        }

        private void AddSingletonFields( TreeBasedGridView.GridNode node )
        {
            var exGlobalRoot = this.ImageInformation.TypeSystem.GlobalRoot;
            var pc           = this.ImageInformation.ResolveCostantExpression( this.MemoryDelta, exGlobalRoot );

            if(pc != null)
            {
                var valHandleGlobalRoot = new MemoryValueHandle( pc, false );

                foreach(TS.InstanceFieldRepresentation fd in exGlobalRoot.Type.Fields)
                {
                    if(fd.ImplementationOf == null)
                    {
                        if(m_singletonNode == null)
                        {
                            m_singletonNode      = node.AddChild( "Singletons", "", "" );
                            m_singletonNode.Icon = Properties.Resources.StaticMembers;
                        }

                        AddField( m_singletonNode, valHandleGlobalRoot, fd, fd.FieldType.FullNameWithAbbreviation );
                    }
                }
            }
        }

        //--//

        private bool UpdateValue( TreeBasedGridView.GridNode node          ,
                                  string                     proposedValue ,
                                  int                        index         )
        {
            var item = (ItemDescriptor)node.Tag;

            if(index == 1)
            {
                if(item.ConvertFromText( proposedValue ))
                {
                    item.CollapseIfChanged();
                }
            }

            return false;
        }

        private void ExpandValue( TreeBasedGridView.GridNode node )
        {
            var item = (ItemDescriptor)node.Tag;

            var loc = item.SubLocation;

            if(loc == null)
            {
                return;
            }

            TS.TypeRepresentation td = loc.Type;

            if(td is TS.ArrayReferenceTypeRepresentation)
            {
                if(loc.MemoryMappedRegister != null)
                {
                    ExpandArray( node, item, td.ContainedType, 0, (uint)loc.MemoryMappedRegister.GetNamedArg< int >( "Instances" ) );
                }
                else
                {
                    bool fChanged;
                    var  bb = loc.Read( 0, sizeof(uint), out fChanged );

                    if(bb != null)
                    {
                        uint length = bb.ReadUInt32( 0 );

                        ExpandArray( node, item, td.ContainedType, 0, length );
                    }
                }
            }
            else
            {
                var exGlobalRoot = this.ImageInformation.TypeSystem.GlobalRoot;

                if(loc.HasAddress)
                {
                    uint address = loc.Address;

                    if(item.IsTopView)
                    {
                        uint addressFixed = address;
                        var tdFixed = item.FixType( td, ref addressFixed );

                        if(tdFixed != td)
                        {
                            CreateSubView( node, string.Format( "[{0}]", tdFixed.FullNameWithAbbreviation ), addressFixed, tdFixed, loc.MemoryMappedPeripheral, loc.MemoryMappedRegister );
                        }
                    }

                    TS.TypeRepresentation tdExtends = td.Extends;
                    if(tdExtends != null)
                    {
                        if(HasFieldsToDisplay( tdExtends, exGlobalRoot ))
                        {
                            CreateSubView( node, "Base Members", address, tdExtends, loc.MemoryMappedPeripheral, loc.MemoryMappedRegister );
                        }
                    }
                }

                var pc = this.ImageInformation.ResolveCostantExpression( this.MemoryDelta, exGlobalRoot );
                if(pc != null)
                {
                    var                        valHandleGlobalRoot = new MemoryValueHandle( pc, false );
                    TreeBasedGridView.GridNode staticNode          = null;

                    foreach(TS.InstanceFieldRepresentation fd in exGlobalRoot.Type.Fields)
                    {
                        var fd2 = fd.ImplementationOf;
                        if(fd2 != null &&
                           fd2.OwnerType == td)
                        {
                            if(staticNode == null)
                            {
                                staticNode = node.AddChild( "Static Members", "", "" );

                                staticNode.Icon = Properties.Resources.StaticMembers;
                            }

                            AddField( staticNode, valHandleGlobalRoot, fd, fd2.Name );
                        }
                    }
                }

                bool fAddRawView = false;

                foreach(TS.FieldRepresentation fd in td.Fields)
                {
                    var fd2 = fd as TS.InstanceFieldRepresentation;

                    if(fd2 != null)
                    {
                        bool fIsBitField;

                        AddField( node, loc, fd2, fd2.Name, out fIsBitField );

                        fAddRawView |= fIsBitField;
                    }
                }

                if(fAddRawView)
                {
                    var                   wkt = this.ImageInformation.TypeSystem.WellKnownTypes;
                    TS.TypeRepresentation rawTd;

                    switch(loc.Size)
                    {
                        case 1:
                            rawTd = wkt.System_Byte;
                            break;

                        case 2:
                            rawTd = wkt.System_UInt16;
                            break;

                        case 4:
                            rawTd = wkt.System_UInt32;
                            break;

                        case 8:
                            rawTd = wkt.System_UInt64;
                            break;

                        default:
                            rawTd = null;
                            break;
                    }

                    if(rawTd != null)
                    {
                        var rawLoc = new CompoundValueHandle( rawTd, false, new CompoundValueHandle.Fragment( loc, 0 ) );

                        var subItem = new ItemDescriptor( this, "[Raw Value]", rawLoc.Type, rawLoc );

                        subItem.AttachToView( node, null, Properties.Resources.StaticMembers );
                    }
                }
            }
        }

        private void CreateSubView( TreeBasedGridView.GridNode       node                     ,
                                    string                           text                     ,
                                    uint                             address                  ,
                                    TS.TypeRepresentation            td                       ,
                                    TS.CustomAttributeRepresentation caMemoryMappedPeripheral ,
                                    TS.CustomAttributeRepresentation caMemoryMappedRegister   )
        {
            var subNode = node.AddChild( text, "", td.FullNameWithAbbreviation );

            subNode.Icon           = Properties.Resources.LocalVariable;
            subNode.ExpandCallback = this.ExpandValue;

            var valueHandle = new MemoryValueHandle( td, caMemoryMappedPeripheral, caMemoryMappedRegister, false, this.MemoryDelta, address );
            var item        = new ItemDescriptor( this, null, td, valueHandle );

            item   .SubLocation = valueHandle;
            item   .IsTopView   = false;
            subNode.Tag         = item;
        }

        private void ExpandArray( TreeBasedGridView.GridNode node       ,
                                  ItemDescriptor             item       ,
                                  TS.TypeRepresentation      td         ,
                                  uint                       startIndex ,
                                  uint                       endIndex   )
        {
            uint length = endIndex - startIndex;

            //
            // Protect against overflows.
            //
            length = Math.Min( length, uint.MaxValue / MaxArrayDisplay / 2 );

            if(length > MaxArrayDisplay)
            {
                uint scale = MaxArrayDisplay;

                while(scale * MaxArrayDisplay < length)
                {
                    scale *= MaxArrayDisplay;
                }

                for(uint pos = startIndex; pos < endIndex; pos += scale)
                {
                    uint newStartIndex = pos;
                    uint newEndIndex   = Math.Min( pos + scale, endIndex );

                    var newNode = node.AddChild( string.Format( "[{0}-{1}]", newStartIndex, newEndIndex - 1 ), "...", "" );

                    newNode.Tag = item;
                    newNode.ExpandCallback = (nodeSub => this.ExpandArray( nodeSub, item, td, newStartIndex, newEndIndex ));
                }
            }
            else
            {
                var loc = item.SubLocation;

                bool fAsHoldingVariable = (loc.MemoryMappedRegister == null);
                uint address            = loc.Address;
                uint step;

                if(fAsHoldingVariable)
                {
                    var fd = this.ImageInformation.TypeSystem.WellKnownFields.ArrayImpl_m_numElements;

                    address += (uint)fd.Offset + fd.FieldType.SizeOfHoldingVariable;
                    step     = td.SizeOfHoldingVariable;
                }
                else
                {
                    step = td.Size;
                }

                for(uint pos = startIndex; pos < endIndex; pos++)
                {
                    var name        = string.Format( "[{0}]", pos );
                    var valueHandle = new MemoryValueHandle( td, null, null, fAsHoldingVariable, this.MemoryDelta, address + pos * step );
                    var itemSub     = new ItemDescriptor( this, name, td, valueHandle );

                    itemSub.AttachToView( node, null, Properties.Resources.LocalVariable );
                }
            }
        }

        private static bool HasFieldsToDisplay( TS.TypeRepresentation td           ,
                                                IR.ConstantExpression exGlobalRoot )
        {
            foreach(TS.InstanceFieldRepresentation fd in exGlobalRoot.Type.Fields)
            {
                var fd2 = fd.ImplementationOf;
                if(fd2 != null && fd2.OwnerType == td)
                {
                    return true;
                }
            }

            foreach(TS.FieldRepresentation fd in td.Fields)
            {
                if(fd is TS.InstanceFieldRepresentation)
                {
                    return true;
                }
            }

            TS.TypeRepresentation tdExtends = td.Extends;

            if(tdExtends != null)
            {
                if(HasFieldsToDisplay( tdExtends, exGlobalRoot ))
                {
                    return true;
                }
            }

            return false;
        }

        private void AddField(     TreeBasedGridView.GridNode     node        ,
                                   AbstractValueHandle            valHandle   ,
                                   TS.InstanceFieldRepresentation fd          ,
                                   string                         name        )
        {
            bool fIsBitField;

            AddField( node, valHandle, fd, name, out fIsBitField );
        }

        private void AddField(     TreeBasedGridView.GridNode     node        ,
                                   AbstractValueHandle            valHandle   ,
                                   TS.InstanceFieldRepresentation fd          ,
                                   string                         name        ,
                               out bool                           fIsBitField )
        {
            AbstractValueHandle   subVal;
            Icon                  icon;
            IR.BitFieldDefinition bfDef;
            var                   ts = this.ImageInformation.TypeSystem;

            var caTd = ts.MemoryMappedPeripherals.GetValue( fd.OwnerType );
            var caFd = ts.RegisterAttributes     .GetValue( fd           );

            switch(fd.Flags & TS.FieldRepresentation.Attributes.FieldAccessMask)
            {
                case TS.FieldRepresentation.Attributes.Public:
                    icon = Properties.Resources.PublicMember;
                    break;

                default:
                    icon = Properties.Resources.PrivateMember;
                    break;
            }

            fIsBitField = ts.BitFieldRegisterAttributes.TryGetValue( fd, out bfDef );
            if(fIsBitField)
            {
                subVal = valHandle.AccessBitField( fd, caTd, caFd, bfDef );
            }
            else
            {
                subVal = valHandle.AccessField( fd, caTd, caFd );
            }


            //--//

            var item = new ItemDescriptor( this, name, subVal.Type, subVal );

            item.AttachToView( node, null, icon );
        }

        //
        // Access Methods
        //

        public bool HexadecimalDisplay
        {
            get
            {
                return m_fHexadecimalDisplay;
            }

            set
            {
                if(m_fHexadecimalDisplay != value)
                {
                    m_fHexadecimalDisplay = value;

                    var ctrl = this.RootNode.Owner;

                    ctrl.StartTreeUpdate();

                    ctrl.EnumerateNodesPreOrder( UpdateText );

                    ctrl.EndTreeUpdate();
                }
            }
        }
    }
}
