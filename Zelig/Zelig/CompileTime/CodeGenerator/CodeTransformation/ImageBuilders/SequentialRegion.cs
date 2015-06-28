//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.ImageBuilders
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.TargetModel.ArmProcessor;


    public class SequentialRegion
    {
        public class Section
        {
            //
            // State
            //

            private SequentialRegion m_context;
            private uint             m_start;
            private uint             m_end;
            private uint             m_offset;

            //
            // Constructor Methods
            //

            internal Section( SequentialRegion context ,
                              uint             start   )
            {
                m_context = context;
                m_start   = start;
                m_end     = uint.MaxValue;
                m_offset  = 0;
            }

            internal Section( SequentialRegion context ,
                              uint             start   ,
                              uint             size    )
            {
                m_context = context;
                m_start   = start;
                m_end     = start + size;
                m_offset  = 0;
            }

            //
            // Helper Methods
            //

            public Section GetSubSection( uint size )
            {
                uint start = m_start + m_offset;

                m_offset += size;

                CheckPosition();

                return new Section( m_context, start, size );
            }

            public void AlignGeneric( uint alignment )
            {
                uint off = m_offset % alignment;

                if(off != 0)
                {
                    m_offset += alignment - off;

                    CheckPosition();
                }
            }

            public void AlignToHalfWord()
            {
                m_offset = (m_offset + 1u) & ~1u;

                CheckPosition();
            }

            public void AlignToWord()
            {
                m_offset = (m_offset + 3u) & ~3u;

                CheckPosition();
            }

            public void Skip( uint size )
            {
                m_offset += size;

                CheckPosition();
            }

            //--//

            public void Write( byte val )
            {
                uint pos = Expand( sizeof(byte) );

                m_context.Write( pos, val );
            }

            public void Write( ushort val )
            {
                uint pos = Expand( sizeof(ushort) );

                m_context.Write( pos, val );
            }

            public void Write( uint val )
            {
                uint pos = Expand( sizeof(uint) );

                m_context.Write( pos, val );
            }

            public void WriteNullPointer()
            {
                Expand( sizeof(uint) );
            }

            public ImageAnnotation WritePointerToDataDescriptor( DataManager.DataDescriptor dd )
            {
                CHECKS.ASSERT( dd != null, "Cannot write null pointer" );

                uint pos = Expand( sizeof(uint) );

                var reloc = new DataRelocation( m_context, pos, dd );

                m_context.m_owner.AddObject( dd );

                return reloc;
            }

            public ImageAnnotation WritePointerToBasicBlock( BasicBlock bb )
            {
                CHECKS.ASSERT( bb != null, "Cannot write null pointer" );

                uint pos = Expand( sizeof(uint) );

                var reloc = new DataRelocation( m_context, pos, bb );

                m_context.m_owner.CompileBasicBlock( bb );

                return reloc;
            }

            //--//

            public void Write( bool val )
            {
                Write( val ? (byte)1 : (byte)0 );
            }

            public void Write( ulong val )
            {
                uint pos = Expand( sizeof(ulong) );

                m_context.Write( pos               , (uint)(val      ) );
                m_context.Write( pos + sizeof(uint), (uint)(val >> 32) );
            }

            public void Write( sbyte val )
            {
                uint pos = Expand( sizeof(sbyte) );

                m_context.Write( pos, (byte)val );
            }

            public void Write( short val )
            {
                uint pos = Expand( sizeof(short) );

                m_context.Write( pos, (ushort)val );
            }

            public void Write( int val )
            {
                uint pos = Expand( sizeof(int) );

                m_context.Write( pos, (uint)val );
            }

            public void Write( long val )
            {
                uint pos = Expand( sizeof(long) );

                m_context.Write( pos               , (uint)(val      ) );
                m_context.Write( pos + sizeof(uint), (uint)(val >> 32) );
            }

            public void Write( char val )
            {
                uint pos = Expand( sizeof(char) );

                m_context.Write( pos, (ushort)val );
            }

            public void Write( float val )
            {
                uint pos = Expand( sizeof(float) );

                m_context.Write( pos, DataConversion.GetFloatAsBytes( val ) );
            }

            public void Write( double val )
            {
                uint pos = Expand( sizeof(double) );

                ulong val2 = DataConversion.GetDoubleAsBytes( val );

                m_context.Write( pos               , (uint)(val2      ) );
                m_context.Write( pos + sizeof(uint), (uint)(val2 >> 32) );
            }

            public void Write( UIntPtr val )
            {
                uint pos = Expand( sizeof(uint) );

                m_context.Write( pos, val.ToUInt32() );
            }

            public void Write( IntPtr val )
            {
                uint pos = Expand( sizeof(uint) );

                m_context.Write( pos, (uint)val.ToInt32() );
            }

            //--//

            public void Write( byte[] valArray )
            {
                uint pos = Expand( (uint)(valArray.Length * sizeof(byte)) );

                m_context.Write( pos, valArray );
            }

            public void Write( char[] valArray )
            {
                uint pos = Expand( (uint)(valArray.Length * sizeof(char)) );

                m_context.Write( pos, valArray );
            }

            public void Write( int[] valArray )
            {
                uint pos = Expand( (uint)(valArray.Length * sizeof(int)) );

                m_context.Write( pos, valArray );
            }

            public void Write( uint[] valArray )
            {
                uint pos = Expand( (uint)(valArray.Length * sizeof(uint)) );

                m_context.Write( pos, valArray );
            }

            //--//

            public bool WriteGeneric( object val )
            {
                if(val != null)
                {
                    Type t = val.GetType();

                    if(t.IsEnum)
                    {
                        t = Enum.GetUnderlyingType( t );
                    }

                    if(t == typeof(bool   )) { Write( (bool   )val ); return true; }
                    if(t == typeof(byte   )) { Write( (byte   )val ); return true; }
                    if(t == typeof(ushort )) { Write( (ushort )val ); return true; }
                    if(t == typeof(uint   )) { Write( (uint   )val ); return true; }
                    if(t == typeof(ulong  )) { Write( (ulong  )val ); return true; }
                    if(t == typeof(sbyte  )) { Write( (sbyte  )val ); return true; }
                    if(t == typeof(short  )) { Write( (short  )val ); return true; }
                    if(t == typeof(int    )) { Write( (int    )val ); return true; }
                    if(t == typeof(long   )) { Write( (long   )val ); return true; }
                    if(t == typeof(char   )) { Write( (char   )val ); return true; }
                    if(t == typeof(float  )) { Write( (float  )val ); return true; }
                    if(t == typeof(double )) { Write( (double )val ); return true; }
                    if(t == typeof(UIntPtr)) { Write( (UIntPtr)val ); return true; }
                    if(t == typeof(IntPtr )) { Write( (IntPtr )val ); return true; }
                    if(t == typeof(byte[] )) { Write( (byte[] )val ); return true; }
                    if(t == typeof(char[] )) { Write( (char[] )val ); return true; }
                    if(t == typeof(int [] )) { Write( (int [] )val ); return true; }
                    if(t == typeof(uint[] )) { Write( (uint[] )val ); return true; }
                }

                return false;
            }

            //--//

            public ImageAnnotation AddImageAnnotation( uint   size ,
                                                       object val  )
            {
                return new GenericImageAnnotation( m_context, m_start + m_offset, size, val );
            }

            //--//

            private void CheckPosition()
            {
                if(m_end == uint.MaxValue)
                {
                    m_context.EnsureSize( m_start + m_offset );
                }
                else if(m_offset > this.Size)
                {
                    throw new IndexOutOfRangeException();
                }
            }

            private uint ExpandAndAnnotate( uint   size ,
                                            object val  )
            {
                if(val != null)
                {
                    AddImageAnnotation( size, val );
                }

                return Expand( size );
            }

            private uint Expand( uint size )
            {
                uint pos = m_start + m_offset;

                m_offset += size;

                CheckPosition();

                return pos;
            }

            //
            // Access Methods
            //

            public SequentialRegion Context
            {
                get
                {
                    return m_context;
                }
            }

            public uint Position
            {
                get
                {
                    return m_start + m_offset;
                }
            }

            public uint Offset
            {
                get
                {
                    return m_offset;
                }

                set
                {
                    m_offset = value;

                    CheckPosition();
                }
            }

            public uint Size
            {
                get
                {
                    return m_end - m_start;
                }
            }
        }

        //--//

        const uint AddressNotAssigned = 0xFFFFFFFFu;

        //
        // State
        //

        private Core                               m_owner;
        private object                             m_context;
        private Abstractions.PlacementRequirements m_placementRequirements;
        private UIntPtr                            m_baseAddress;
        private uint                               m_pointerOffset;
                                        
        private uint                               m_position;
        private uint                               m_size;
        private byte[]                             m_payload;
        private uint                               m_payloadCutoff;
        private List< ImageAnnotation >            m_annotationList;
                                        
        private PrettyDumper                       m_dumper;

        //
        // Constructor Methods
        //

        private SequentialRegion() // Default constructor required by TypeSystemSerializer.
        {
            m_annotationList = new List< ImageAnnotation >();
        }

        internal SequentialRegion( Core                               owner   ,
                                   object                             context ,
                                   Abstractions.PlacementRequirements pr      ) : this()
        {
            m_owner                 = owner;
            m_context               = context;
            m_placementRequirements = pr;
            m_pointerOffset         = 0;

            m_position              = 0;
            m_size                  = 0;
            m_payloadCutoff         = uint.MaxValue;
            m_payload               = null;

            InvalidateBaseAddress();
        }

        //
        // Helper Methods
        //

        public void ApplyTransformation( TransformationContextForCodeTransformation context )
        {
            context.Push( this );

            context.Transform( ref m_owner                 );
            context.Transform( ref m_context               );
            context.Transform( ref m_placementRequirements );
            context.Transform( ref m_baseAddress           );
            context.Transform( ref m_pointerOffset         );
                                                     
            context.Transform( ref m_position              );
            context.Transform( ref m_size                  );
            context.Transform( ref m_payloadCutoff         );
            context.Transform( ref m_payload               );
            context.Transform( ref m_annotationList        );

            context.Pop();
        }

        //--//

        public void InvalidateBaseAddress()
        {
            m_baseAddress = new UIntPtr( AddressNotAssigned );
        }

        public void Clear()
        {
            m_pointerOffset = 0;

            m_position      = 0;
            m_size          = 0;
            m_payload       = null;
            m_annotationList.Clear();
        }

        //--//

        public uint AdjustSizeForAlignment( uint size )
        {
            return AddressMath.AlignToBoundary( size, m_owner.TypeSystem.PlatformAbstraction.MemoryAlignment );
        }

        public uint GetAbsoluteAddress( uint offset )
        {
            return this.ExternalAddress + offset;
        }

        public Section GetSectionOfVariableSize( uint alignment )
        {
            Section sec = new Section( this, m_position );

            return sec;
        }

        public Section GetSectionOfFixedSize( uint size )
        {
            size = AdjustSizeForAlignment( size );

            Section sec = new Section( this, m_position, size );

            Skip( size );

            return sec;
        }

        public void Skip( uint size )
        {
            m_position += size;

            EnsureSize( m_position );
        }

        //--//

        public byte ReadByte( uint offset )
        {
            return m_payload[offset];
        }

        public ushort ReadUShort( uint offset )
        {
            return (ushort)(((uint)m_payload[offset  ]     ) | 
                            ((uint)m_payload[offset+1] << 8) );
        }

        public uint ReadUInt( uint offset )
        {
            return (((uint)m_payload[offset  ]      ) | 
                    ((uint)m_payload[offset+1] <<  8) | 
                    ((uint)m_payload[offset+2] << 16) | 
                    ((uint)m_payload[offset+3] << 24) );
        }

        //--//

        public void Write( uint offset ,
                           byte val    )
        {
            m_payload[offset] = val;
        }

        public void Write( uint   offset ,
                           ushort val    )
        {
            m_payload[offset  ] = (byte)(val     );
            m_payload[offset+1] = (byte)(val >> 8);
        }

        public void Write( uint offset ,
                           uint val    )
        {
            m_payload[offset  ] = (byte)(val      );
            m_payload[offset+1] = (byte)(val >>  8);
            m_payload[offset+2] = (byte)(val >> 16);
            m_payload[offset+3] = (byte)(val >> 24);
        }

        //--//

        public void Write( uint   offset   ,
                           byte[] valArray )
        {
            Buffer.BlockCopy( valArray, 0, m_payload, (int)offset, valArray.Length );
        }

        public void Write( uint   offset   ,
                           char[] valArray )
        {
            Buffer.BlockCopy( valArray, 0, m_payload, (int)offset, valArray.Length * sizeof(char) );
        }

        public void Write( uint  offset   ,
                           int[] valArray )
        {
            Buffer.BlockCopy( valArray, 0, m_payload, (int)offset, valArray.Length * sizeof(int) );
        }

        public void Write( uint   offset   ,
                           uint[] valArray )
        {
            Buffer.BlockCopy( valArray, 0, m_payload, (int)offset, valArray.Length * sizeof(uint) );
        }

        //--//

        public void Remove( uint offset ,
                            uint len    )
        {
            if(offset < m_size)
            {
                uint end = offset + len;

                if(end < m_size)
                {
                    Array.Copy( m_payload, end, m_payload, offset, m_size - end );

                    m_size -= len;
                }
                else
                {
                    m_size = offset;
                }

                //
                // Adjust relocation info.
                //
                foreach(ImageAnnotation an in m_annotationList)
                {
                    if(an.Offset >= offset)
                    {
                        if(an.Offset >= end)
                        {
                            an.Offset -= len;
                        }

                        else
                        {
                            an.Offset = offset;
                        }
                    }
                }
            }
        }

        //--//

        internal void AddImageAnnotation( ImageAnnotation an )
        {
            //
            // Insert the annotation in 'Offset' order.
            //
            int pos = m_annotationList.Count;

            while(--pos >= 0)
            {
                ImageAnnotation anOld = m_annotationList[pos];

                if(anOld.Offset <= an.Offset)
                {
                    break;
                }
            }

            m_annotationList.Insert( pos + 1, an );
        }

        //--//

        internal void AssignAbsoluteAddress()
        {
            if(this.IsBaseAddressAssigned == false)
            {
                UIntPtr address;

                if(m_owner.ReserveRangeOfMemory( m_size, m_pointerOffset, m_placementRequirements, out address ) == false)
                {
                    throw TypeConsistencyErrorException.Create( "Cannot allocate memory for data: {0}", m_context );
                }

                this.BaseAddress = address;
            }
        }

        internal bool ApplyRelocation()
        {
            bool fRes = true;

            foreach(ImageAnnotation an in m_annotationList)
            {
                fRes &= an.ApplyRelocation();
            }

            return fRes;
        }

        //--//

        public static int ComputeSize( object val )
        {
            if(val is bool  ) return sizeof(bool  );
            if(val is byte  ) return sizeof(byte  );
            if(val is ushort) return sizeof(ushort);
            if(val is uint  ) return sizeof(uint  );
            if(val is ulong ) return sizeof(ulong );
            if(val is sbyte ) return sizeof(sbyte );
            if(val is short ) return sizeof(short );
            if(val is int   ) return sizeof(int   );
            if(val is long  ) return sizeof(long  );
            if(val is char  ) return sizeof(char  );
            if(val is float ) return sizeof(float );
            if(val is double) return sizeof(double);

            return -1;
        }

        //--//

        void EnsureSize( uint size )
        {
            if(m_payload == null)
            {
                m_payload = new byte[128];
            }

            if(m_size < size)
            {
                uint newSize = Math.Max( m_size + 128, size + 64 );

                m_payload = ArrayUtility.EnsureSizeOfNotNullArray( m_payload, (int)newSize );
            }

            m_size = size;
        }

        //--//

        public byte[] ToArray()
        {
            if(m_payload == null)
            {
                return new byte[0];
            }

            return ArrayUtility.ExtractSliceFromNotNullArray( m_payload, 0, (int)this.PayloadCutoff );
        }

        //--//

        //
        // Access Methods
        //

        public Core Owner
        {
            get
            {
                return m_owner;
            }
        }

        public object Context
        {
            get
            {
                return m_context;
            }
        }

        public Abstractions.PlacementRequirements PlacementRequirements
        {
            get
            {
                return m_placementRequirements;
            }
        }

        public UIntPtr BaseAddress
        {
            get
            {
                return m_baseAddress;
            }

            set
            {
                m_baseAddress = value;
            }
        }

        public UIntPtr EndAddress
        {
            get
            {
                return new UIntPtr( m_baseAddress.ToUInt32() + m_size );
            }
        }

        public UIntPtr PayloadEndAddress
        {
            get
            {
                return new UIntPtr( m_baseAddress.ToUInt32() + this.PayloadCutoff );
            }
        }

        public uint PointerOffset
        {
            get
            {
                return m_pointerOffset;
            }

            set
            {
                m_pointerOffset = value;
            }
        }

        public uint PayloadCutoff
        {
            get
            {
                return Math.Min( m_size, m_payloadCutoff );
            }

            set
            {
                m_payloadCutoff = value;
            }
        }

        public uint ExternalAddress
        {
            get
            {
                CHECKS.ASSERT( this.IsBaseAddressAssigned, "Cannot access ExternalAddress before it's been assigned to {0}", this );

                return m_baseAddress.ToUInt32() + m_pointerOffset;
            }
        }

        //--//

        public uint Position
        {
            get
            {
                return m_position;
            }

            set
            {
                if(value > m_size)
                {
                    throw new IndexOutOfRangeException();
                }

                m_position = value;
            }
        }

        public uint Size
        {
            get
            {
                return m_size;
            }
        }

        public bool IsBaseAddressAssigned
        {
            get
            {
                return m_baseAddress.ToUInt32() != AddressNotAssigned;
            }
        }

        public List< ImageAnnotation > AnnotationList
        {
            get
            {
                return m_annotationList;
            }
        }

        //
        // Debug Methods
        //

        internal IIntermediateRepresentationDumper Dumper
        {
            get
            {
                if(m_dumper == null)
                {
                    m_dumper = new PrettyDumper();
                }

                return m_dumper;
            }
        }

        private static string s_indent1 = new string( ' ', 12 );
        private static string s_indent2 = new string( ' ', 13 );

        private void DumpActivationRecordEvents( System.IO.TextWriter textWriter ,
                                                 uint                 offset     )
        {
            bool fGot = false;

            foreach(ImageAnnotation an in m_annotationList)
            {
                if(an.Offset == offset)
                {
                    if(an.Target is Runtime.ActivationRecordEvents)
                    {
                        if(fGot == false)
                        {
                            fGot = true;

                            textWriter.WriteLine( "{0};;;;;;;;;;", s_indent1 );
                        }

                        textWriter.Write( "{0};;;;;;;;;;{1}ACTIVATION RECORD >>>> ", s_indent1, s_indent2 ); an.Dump( textWriter );
                        textWriter.WriteLine();
                    }
                }
            }

            if(fGot)
            {
                textWriter.WriteLine( "{0};;;;;;;;;;", s_indent1 );
            }
        }

        private void DumpPointers( System.IO.TextWriter textWriter ,
                                   uint                 offset     )
        {
            bool fGot = false;

            foreach(ImageAnnotation an in m_annotationList)
            {
                if(an.Offset == offset)
                {
                    if(an is TrackVariableLifetime)
                    {
                        TrackVariableLifetime tvl = (TrackVariableLifetime)an;

                        if(offset == 0 && tvl.IsAlive == false)
                        {
                            //
                            // Don't dump dead variables at the entrance of the basic block.
                            //
                            continue;
                        }

                        if(fGot == false)
                        {
                            fGot = true;

                            textWriter.WriteLine( "{0};;;;;;;;;;", s_indent1 );
                        }

                        textWriter.Write( "{0};;;;;;;;;;{1}VAR >>>> ", s_indent1, s_indent2 ); an.Dump( textWriter );
                        textWriter.WriteLine();
                    }
                }
            }

            if(fGot)
            {
                textWriter.WriteLine( "{0};;;;;;;;;;", s_indent1 );
            }
        }

        private void DumpOperators( System.IO.TextWriter textWriter ,
                                    uint                 offset     )
        {
            foreach(ImageAnnotation an in m_annotationList)
            {
                if(an.Offset == offset)
                {
                    Operator op = an.Target as Operator;
                    if(op != null)
                    {
                        bool fGot = false;

                        m_owner.SourceCodeTracker.Print( op.DebugInfo, delegate ( string format, object[] args )
                        {
                            textWriter.Write( "{0};;;;;;;;;;{1}", s_indent1, s_indent2 );
                            textWriter.WriteLine( format, args );

                            fGot = true;
                        } );

                        if(fGot)
                        {
                            textWriter.WriteLine( "{0};;;;;;;;;;", s_indent1 );
                        }

                        textWriter.WriteLine( "{0};;;;;;;;;;{1}IR >>>> {2}", s_indent1, s_indent2, op.FormatOutput( this.Dumper ) );
                        textWriter.WriteLine( "{0};;;;;;;;;;", s_indent1 );
                    }
                }
            }
        }

        private void DumpOther( System.IO.TextWriter textWriter ,
                                uint                 offset     )
        {
            bool fFirst = true;

            foreach(ImageAnnotation an in m_annotationList)
            {
                if(an.Offset == offset)
                {
                    if(an is TrackVariableLifetime)
                    {
                        continue;
                    }

                    if(an.IsScalar)
                    {
                        continue;
                    }

                    if(an.Target is Operator)
                    {
                        continue;
                    }

                    if(fFirst)
                    {
                        fFirst = false;

                        textWriter.Write( "  ;;; " );
                    }
                    else
                    {
                        textWriter.Write( ", " );
                    }

                    an.Dump( textWriter );
                }
            }
        }

        public void Dump( System.IO.TextWriter textWriter )
        {
            if(m_context is BasicBlock)
            {
                BasicBlock bb = (BasicBlock)m_context;

                textWriter.WriteLine( "    Opcodes for {0} for {1} [{2}]", this.Dumper.CreateLabel( bb ), bb.Owner, bb.Annotation );

                InstructionSet encoder = this.Owner.GetInstructionSetProvider();

                uint address = this.IsBaseAddressAssigned ? this.BaseAddress.ToUInt32() : 0;

                m_owner.SourceCodeTracker.ResetContext();
                m_owner.SourceCodeTracker.ExtraLinesToOutput = 3;


                for(uint offset = 0; offset < m_size; offset += sizeof(uint))
                {
                    uint   target;
                    bool   targetIsCode;
                    uint   opcode = ReadUInt( offset );
                    string res    = encoder.DecodeAndPrint( address + offset, opcode, out target, out targetIsCode );

                    //
                    // Dump activation record events.
                    //
                    DumpActivationRecordEvents( textWriter, offset );

                    //
                    // Dump liveness status of variables.
                    //
                    DumpPointers( textWriter, offset );

                    //
                    // Dump IR operators associated with the instruction.
                    //
                    DumpOperators( textWriter, offset );

                    //
                    // Dump ARM opcode.
                    //

                    textWriter.Write( "{0}0x{1:X8}:  {2:X8}  {3}", s_indent1, address + offset, opcode, res );

                    //
                    // Dump all the other annotations.
                    //
                    DumpOther( textWriter, offset );

                    textWriter.WriteLine();
                }

                DumpActivationRecordEvents( textWriter, m_size );

                DumpPointers( textWriter, m_size );

                textWriter.WriteLine();
            }
            else
            {
                if(m_size > 0)
                {
                    textWriter.WriteLine( "    Data for {0}", m_context );

                    uint address  = this.IsBaseAddressAssigned ? this.BaseAddress.ToUInt32() : 0;
                    uint pos      = 0;
                    bool fNewLine = false;

                    for(uint offset = 0; offset < this.PayloadCutoff; offset++)
                    {
                        ImageAnnotation gotAnnotation = null;

                        foreach(ImageAnnotation an in m_annotationList)
                        {
                            if(an.Offset == offset)
                            {
                                gotAnnotation = an;
                                break;
                            }
                        }

                        if(gotAnnotation != null && pos != 0)
                        {
                            fNewLine = true;
                        }

                        if(fNewLine)
                        {
                            fNewLine = false;

                            pos = 0;

                            textWriter.WriteLine();
                        }

                        textWriter.Write( pos == 0 ? "            0x{0:X8}:  " : ", " , address + offset );

                        if(gotAnnotation == null)
                        {
                            textWriter.Write( "0x{0:X2}", ReadByte( offset ) );

                            pos++;

                            if(pos >= 32)
                            {
                                fNewLine = true;
                            }
                        }
                        else
                        {
                            switch(gotAnnotation.Size)
                            {
                                case 1:
                                    textWriter.Write( "0x{0:X2}", ReadByte( offset ) );
                                    break;

                                case 2:
                                    textWriter.Write( "0x{0:X4}", ReadUShort( offset ) );
                                    offset += 2 - 1;
                                    break;

                                default:
                                case 4:
                                    textWriter.Write( "0x{0:X8}", ReadUInt( offset ) );
                                    offset += 4 - 1;
                                    break;

                                case 8:
                                    textWriter.Write( "0x{0:X8}{1:X8}", ReadUInt( offset + 4 ), ReadUInt( offset ) );
                                    offset += 8 - 1;
                                    break;
                            }

                            bool fFirst = true;

                            foreach(ImageAnnotation an in m_annotationList)
                            {
                                if(an.Offset == gotAnnotation.Offset)
                                {
                                    if(an.IsScalar == false || an.Target is char)
                                    {
                                        if(fFirst)
                                        {
                                            fFirst = false;

                                            textWriter.Write( "  ;;; " );
                                        }
                                        else
                                        {
                                            textWriter.Write( ", " );
                                        }

                                        an.Dump( textWriter );
                                    }
                                }
                            }

                            fNewLine = true;
                        }
                    }

                    if(fNewLine || pos > 0)
                    {
                        textWriter.WriteLine();
                    }

                    if(this.PayloadCutoff < this.Size)
                    {
                        textWriter.WriteLine( "            0x{0:X8}:  <skipping {1} (0x{1:X8}) bytes>" , address + this.PayloadCutoff, this.Size - this.PayloadCutoff );
                    }

                    textWriter.WriteLine();
                }
            }
        }

        //
        // Debug Methods
        //

        public override string ToString()
        {
            if(this.IsBaseAddressAssigned)
            {
                return string.Format( "Region at 0x{0:X8} for {1}", this.ExternalAddress, m_context );
            }
            else
            {
                return string.Format( "Region for {0}", m_context );
            }
        }
    }
}
