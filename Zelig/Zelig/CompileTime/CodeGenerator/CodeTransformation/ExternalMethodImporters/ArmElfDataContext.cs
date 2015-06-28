//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.ExternalMethodImporters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.binutils.elflib;
    using Microsoft.Zelig.CodeGeneration.IR.ImageBuilders;
    using Microsoft.Zelig.Runtime.TypeSystem;


    public class ArmElfExternalDataContext : ArmElfContext, ExternalDataDescriptor.IExternalDataContext
    {
        ElfSection m_dataSec;
        ArmElfExternalCallContext m_owner;
        
        public ArmElfExternalDataContext( ElfSection dataSec, ArmElfExternalCallContext owner )
        {
            m_dataSec = dataSec;
            m_owner = owner;
        }

        public byte[] RawData
        {
            get { return m_dataSec.Raw; }
        }

        public object DataSection
        {
            get { return m_dataSec; }
        }

        internal ElfSection Section
        {
            get { return m_dataSec; }
        }

        public void WriteData( ImageBuilders.SequentialRegion region )
        {
            uint len = (uint)m_dataSec.Raw.Length;

            ImageBuilders.SequentialRegion.Section sec = region.GetSectionOfFixedSize( len );

            uint offset = region.PointerOffset;

            sec.Write( m_dataSec.Raw );

            foreach(SectionReference sr in m_dataSec.References)
            {
                foreach(SectionReference.SymbolReference symRef in sr.CallOffsets)
                {
                    uint callOffset = symRef.Offset;
                    SymbolType st = symRef.RelocationRef.ReferencedSymbol.Type;
                    SymbolBinding sb = symRef.RelocationRef.ReferencedSymbol.Binding;
                    string symName = symRef.RelocationRef.ReferencedSymbol.Name;

                    uint opcode = BitConverter.ToUInt32( m_dataSec.Raw, (int)symRef.Offset );

                    if(st == SymbolType.STT_FUNC || st == SymbolType.STT_NOTYPE)
                    {
                        uint dataOffset;
                        object target = ArmElfExternalCallContext.GetCodeForMethod( symName, out dataOffset );

                        if(target != null)
                        {
                            new ExternalPointerRelocation( region, offset + callOffset, dataOffset, target );
                        }
                        else
                        {
                            Console.WriteLine( "UNABLE TO FIND EXTERNAL REFERENCE " + symName );
                        }
                    }
                    else if(st == SymbolType.STT_OBJECT || st == SymbolType.STT_SECTION)
                    {
                        uint dataOffset;

                        ExternalDataDescriptor.IExternalDataContext ctx = GlobalContext.GetExternalDataContext( symName, symRef, opcode, out dataOffset );

                        if(ctx != null)
                        {
                            new ExternalPointerRelocation( region, offset + callOffset, dataOffset, ctx );
                        }
                        else
                        {
                            Console.WriteLine( "ERROR: Unable to find symbol: " + symName );
                        }
                    }
                    else
                    {
                        Console.WriteLine( "ERROR: UNKNOWN DATA REFERENCE TYPE: " + st.ToString() );
                    }
                }
            }

        }
    }
}
