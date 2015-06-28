//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
//  Originally from the Microsoft Research Singularity code base.
//
namespace Microsoft.Zelig.MetaData.Importer.PdbInfo
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Microsoft.Zelig.MetaData.Importer.PdbInfo.CodeView;
    using Microsoft.Zelig.MetaData.Importer.PdbInfo.Features;

    public class PdbFunction
    {
        internal static readonly Guid                   msilMetaData = new Guid( 0xC6EA3FC9, 0x59B3, 0x49D6, 0xBC, 0x25, 0x09, 0x02,0xBB, 0xAB, 0xB4, 0x60 );
        internal static readonly IComparer<PdbFunction> byAddress    = new PdbFunctionsByAddress();

        //
        // State
        //

        public uint       Token;
        public uint       SlotToken;
        public string     Name;
        public string     Module;
        public ushort     Flags;

        public uint       Segment;
        public uint       Address;
        public uint       Length;

        public byte[]     Metadata;
        public PdbScope[] Scopes;
        public PdbLines[] LineBlocks;

        //
        // Constructor Methods
        //

        internal PdbFunction()
        {
        }

        internal PdbFunction( string      module ,
                              ManProcSym  proc   ,
                              ArrayReader bits   )
        {
            this.Token     = proc.token;
            this.Module    = module;
            this.Name      = proc.name;
            this.Flags     = proc.flags;
            this.Segment   = proc.seg;
            this.Address   = proc.off;
            this.Length    = proc.len;
            this.SlotToken = 0;

            this.LineBlocks = new PdbLines[0];

            if(proc.seg != 1)
            {
                throw new PdbException( "Segment is {0}, not 1.", proc.seg );
            }

            if(proc.parent != 0 || proc.next != 0)
            {
                throw new PdbException( "Warning parent={0}, next={1}", proc.parent, proc.next );
            }

            if(proc.dbgStart != 0 || proc.dbgEnd != 0)
            {
                throw new PdbException( "Warning DBG start={0}, end={1}", proc.dbgStart, proc.dbgEnd );
            }

            int scopeCount;
            int slotCount;

            CountScopesAndSlots( bits, proc.end, out scopeCount, out slotCount );

            Scopes = new PdbScope[scopeCount];
            int scope = 0;

            while(bits.Position < proc.end)
            {
                ushort siz = bits.ReadUInt16();

                ArrayReader subBits = bits.CreateSubsetAndAdvance( siz );

                SYM rec = (SYM)subBits.ReadUInt16();
                switch(rec)
                {
                    case SYM.S_OEM:
                        {          // 0x0404
                            OemSymbol oem;

                            oem.idOem  = subBits.ReadGuid  ();
                            oem.typind = subBits.ReadUInt32();
                            // public byte[]   rgl;        // user data, force 4-byte alignment

                            if(oem.idOem == msilMetaData)
                            {
                                Metadata = subBits.ReadUInt8Array( subBits.Length - subBits.Position );
                            }
                            else
                            {
                                throw new PdbException( "OEM section: guid={0} ti={1}", oem.idOem, oem.typind );
                                // bits.Position = stop;
                            }
                        }
                        break;

                    case SYM.S_BLOCK32:
                        {
                            BlockSym32 block = new BlockSym32();

                            block.parent = subBits.ReadUInt32();
                            block.end    = subBits.ReadUInt32();
                            block.len    = subBits.ReadUInt32();
                            block.off    = subBits.ReadUInt32();
                            block.seg    = subBits.ReadUInt16();
                            block.name   = subBits.ReadZeroTerminatedUTF8String();

                            Scopes[scope++] = new PdbScope( block, bits, out SlotToken );

                            bits.Position = (int)block.end;
                        }
                        break;

                    case SYM.S_UNAMESPACE:
                        break;

                    case SYM.S_END:
                        break;

                    default:
                        throw new PdbException( "Unknown SYM: {0}", rec );
                }
            }

            if(bits.Position != proc.end)
            {
                throw new PdbException( "Not at S_END" );
            }

            ushort esiz = bits.ReadUInt16();
            ushort erec = bits.ReadUInt16();

            if(erec != (ushort)SYM.S_END)
            {
                throw new PdbException( "Missing S_END" );
            }
        }

        public bool FindOffset(     uint      offset  ,
                                out PdbSource resFile ,
                                out PdbLine   resLine )
        {
            foreach(PdbLines lineBlock in LineBlocks)
            {
                foreach(PdbLine line in lineBlock.Lines)
                {
                    if(line.Offset == offset)
                    {
                        resFile = lineBlock.File;
                        resLine = line;
                        return true;
                    }
                }
            }

            resFile = null;
            resLine = new PdbLine();
            return false;
        }

        public void CollectSlots( List<PdbSlot> list )
        {
            foreach(PdbScope scope in Scopes)
            {
                scope.CollectSlots( list );
            }
        }

        //--//

        internal static void CountScopesAndSlots(     ArrayReader bits   ,
                                                      uint        limit  ,
                                                  out int         scopes ,
                                                  out int         slots  )
        {
            BlockSym32 block;

            slots  = 0;
            scopes = 0;

            int pos = bits.Position;

            while(bits.Position < limit)
            {
                ushort siz = bits.ReadUInt16();

                ArrayReader subBits = bits.CreateSubsetAndAdvance( siz );

                SYM rec = (SYM)subBits.ReadUInt16();
                switch(rec)
                {
                    case SYM.S_BLOCK32:
                        {
                            block.parent = subBits.ReadUInt32();
                            block.end    = subBits.ReadUInt32();

                            scopes++;

                            bits.Position = (int)block.end;
                        }
                        break;

                    case SYM.S_MANSLOT:
                        slots++;
                        break;
                }
            }

            bits.Position = pos;
        }

        //--//

        internal class PdbFunctionsByAddress : IComparer<PdbFunction>
        {
            public int Compare( PdbFunction fx ,
                                PdbFunction fy )
            {
                if(fx.Segment < fy.Segment)
                {
                    return -1;
                }
                else if(fx.Segment > fy.Segment)
                {
                    return 1;
                }
                else if(fx.Address < fy.Address)
                {
                    return -1;
                }
                else if(fx.Address > fy.Address)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        internal class PdbFunctionsByToken : IComparer<PdbFunction>
        {
            public int Compare( PdbFunction fx ,
                                PdbFunction fy )
            {
                if(fx.Token < fy.Token)
                {
                    return -1;
                }
                else if(fx.Token > fy.Token)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
