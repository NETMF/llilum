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

    public class PdbFile
    {
        //
        // State
        //

        const int c_Index_PdbStream = 1;
        const int c_Index_DbiStream = 3;

        private PdbFileHeader                   m_head;
        private MsfDirectory                    m_dir;
        private PdbStream                       m_pdbStream;

        private PdbFunction[]                   m_funcs;
        private Dictionary< uint, PdbFunction > m_lookupToken;

        //
        // Constructor Methods
        //

        public PdbFile( ArrayReader image )
        {
            PdbReader pdbReader;

            m_head      = new PdbFileHeader( image                                              );
            pdbReader   = new PdbReader    ( image    , m_head.pageSize                         );
            m_dir       = new MsfDirectory ( pdbReader, m_head                                  );
            m_pdbStream = new PdbStream    ( m_dir.GetArrayReaderForStream( c_Index_PdbStream ) );

            int nameStream  = m_pdbStream.GetStreamNumber( "/names" );
            if(nameStream <= 0)
            {
                throw new PdbException( "No name stream" );
            }

            ushort                    tokenRidMapStream;
            uint[]                    tokenRidMap = null;
            Dictionary< int, string > names   = LoadNameStream( m_dir.GetArrayReaderForStream( nameStream        )                        );
            List< DbiModuleInfo >     modules = LoadDbiStream ( m_dir.GetArrayReaderForStream( c_Index_DbiStream ), out tokenRidMapStream );

            if(tokenRidMapStream != 0xFFFF)
            {
                tokenRidMap = LoadTokenRidMap( m_dir.GetArrayReaderForStream( tokenRidMapStream ) );
            }

            List< PdbFunction > funcList = new List< PdbFunction >();

            foreach(DbiModuleInfo module in modules)
            {
                if(module.stream > 0)
                {
                    LoadFuncsFromDbiModule( m_dir.GetArrayReaderForStream( module.stream ), module, names, funcList );
                }
            }

            m_funcs       = funcList.ToArray();
            m_lookupToken = new Dictionary< uint, PdbFunction >();

            //
            // Remap function tokens.
            //
            foreach(PdbFunction func in m_funcs)
            {
                uint token = func.Token;

                if(tokenRidMap != null)
                {
                    TokenType tt = MetaData.UnpackTokenAsType( (int)token );
                    if(tt == TokenType.Method)
                    {
                        int index = MetaData.UnpackTokenAsIndex( (int)token );
                        if(index >= 0 && index < tokenRidMap.Length)
                        {
                            token = (uint)MetaData.PackToken( TokenType.Method, (int)tokenRidMap[index] );

                            func.Token = token;
                        }
                    }
                }

                m_lookupToken[token] = func;
            }

            Array.Sort( m_funcs, PdbFunction.byAddress );
        }

        public byte[] Emit()
        {
            ArrayWriter writer = new ArrayWriter();

            int pageSize = m_head.pageSize;

            while(true)
            {
                int totalNumberOfPages = 3;

                for(int i = 0; i < m_dir.Streams.Length; i++)
                {
                    int size = m_dir.Streams[i].Length;

                    totalNumberOfPages += (size + pageSize - 1) / pageSize;
                }

                if(totalNumberOfPages < pageSize * 8)
                {
                    break;
                }

                pageSize *= 2;
            }

            PdbWriter pdbWriter = new PdbWriter( writer, pageSize );

            MsfDirectory newDir = new MsfDirectory();

            for(int i = 0; i < m_dir.Streams.Length; i++)
            {
                DataStream src = m_dir .Streams[i];
                DataStream dst = newDir.CreateNewStream( i );

                dst.Copy( pdbWriter, src );
            }

            pdbWriter.Emit( newDir );

            return writer.ToArray();
        }

        public DataStream GetStream( string name )
        {
            int idx = m_pdbStream.GetStreamNumber( name );

            if(idx == -1)
            {
                return null;
            }

            return m_dir.Streams[ idx ];
        }

        public DataStream CreateNewStream( string name )
        {
            DataStream res = GetStream( name );

            if(res == null)
            {
                int idx;

                res = m_dir.CreateNewStream( out idx );

                m_pdbStream.SetStreamNumber( name, idx );

                DataStream  stream = m_dir.CreateNewStream( c_Index_PdbStream );
                ArrayWriter writer = new ArrayWriter();

                m_pdbStream.Emit( writer );

                stream.Payload = writer.ToArray();
            }

            return res;
        }

        public PdbFunction[] Functions
        {
            get
            {
                return m_funcs;
            }
        }

        public PdbFunction FindFunction( uint token )
        {
            PdbFunction res;

            if(m_lookupToken.TryGetValue( token, out res ))
            {
                return res;
            }

            return null;
        }

        //--//

        static Dictionary< int, string > LoadNameStream( ArrayReader bits )
        {
            Dictionary< int, string > ht = new Dictionary< int, string >();

            uint sig = bits.ReadUInt32();   //  0..3  Signature
            int  ver = bits.ReadInt32();    //  4..7  Version

            if(sig != 0xEFFEEFFE || ver != 1)
            {
                throw new PdbException( "Unsupported Name Stream version" );
            }

            // Read (or skip) string buffer.
            int         stringBufLen = bits.ReadInt32();    // 8..11 Bytes of Strings
            ArrayReader stringBits   = bits.CreateSubsetAndAdvance( stringBufLen );

            // Read hash table.
            int siz = bits.ReadInt32();    // n+0..3 Number of hash buckets.

            ArrayReader subBits = new ArrayReader( bits, 0 );

            for(int i = 0; i < siz; i++)
            {
                int ni = subBits.ReadInt32();

                if(ni != 0)
                {
                    stringBits.Position = ni;

                    string name = stringBits.ReadZeroTerminatedUTF8String();

                    ht.Add( ni, name );
                }
            }

            return ht;
        }

        private static PdbFunction FindFunction( PdbFunction[] funcs ,
                                                 ushort        sec   ,
                                                 uint          off   )
        {
            PdbFunction match = new PdbFunction();

            match.Segment = sec;
            match.Address = off;

            int item = Array.BinarySearch( funcs, match, PdbFunction.byAddress );
            if(item >= 0)
            {
                return funcs[item];
            }

            return null;
        }

        static void LoadManagedLines( PdbFunction[]          funcs ,
                                      Dictionary<int,string> names ,
                                      ArrayReader            bits  ,
                                      uint                   limit )
        {
            Dictionary<int,PdbSource> checks = new Dictionary<int,PdbSource>();

            Array.Sort( funcs, PdbFunction.byAddress );

            // Read the files first
            ArrayReader subBits = bits.CreateSubset( (int)(limit - bits.Position) );

            while(subBits.IsEOF == false)
            {
                DEBUG_S_SUBSECTION sig = (DEBUG_S_SUBSECTION)subBits.ReadInt32();
                int                siz =                     subBits.ReadInt32();

                ArrayReader subsectionBits = subBits.CreateSubsetAndAdvance( siz );

                switch(sig)
                {
                    case DEBUG_S_SUBSECTION.FILECHKSMS:
                        while(subsectionBits.IsEOF == false)
                        {
                            CV_FileCheckSum chk;

                            int ni = subsectionBits.Position;

                            chk.name = subsectionBits.ReadUInt32();
                            chk.len  = subsectionBits.ReadUInt8 ();
                            chk.type = subsectionBits.ReadUInt8 ();

                            string name = names[(int)chk.name];

                            PdbSource src = new PdbSource( (uint)ni, name );

                            checks.Add( ni, src );

                            subsectionBits.Seek( chk.len );

                            subsectionBits.AlignAbsolute( 4 );
                        }
                        break;
                }
            }

            // Read the lines next.
            subBits.Rewind();

            while(subBits.IsEOF == false)
            {
                DEBUG_S_SUBSECTION sig = (DEBUG_S_SUBSECTION)subBits.ReadInt32();
                int                siz =                     subBits.ReadInt32();

                ArrayReader subsectionBits = subBits.CreateSubsetAndAdvance( siz );

                switch(sig)
                {
                    case DEBUG_S_SUBSECTION.LINES:
                        {
                            CV_LineSection sec;

                            sec.off   = subsectionBits.ReadUInt32();
                            sec.sec   = subsectionBits.ReadUInt16();
                            sec.flags = subsectionBits.ReadUInt16();
                            sec.cod   = subsectionBits.ReadUInt32();

                            PdbFunction func = FindFunction( funcs, sec.sec, sec.off );

                            // Count the line blocks.
                            List<PdbLines> blocks = new List<PdbLines>();

                            while(subsectionBits.IsEOF == false)
                            {
                                CV_SourceFile file;

                                file.index  = subsectionBits.ReadUInt32();
                                file.count  = subsectionBits.ReadUInt32();
                                file.linsiz = subsectionBits.ReadUInt32();   // Size of payload.

                                //
                                // Apparently, file.linsiz overestimates the size of the payload...
                                //
                                int linsiz = (int)file.count * (8 + ((sec.flags & 1) != 0 ? 4 : 0));

                                ArrayReader payloadBits = subsectionBits.CreateSubsetAndAdvance( linsiz );

                                PdbSource src = checks[(int)file.index];

                                PdbLines tmp = new PdbLines( src, file.count );

                                blocks.Add( tmp );

                                PdbLine[] lines = tmp.Lines;

                                ArrayReader plinBits = payloadBits.CreateSubsetAndAdvance( 8 * (int)file.count );

                                for(int i = 0; i < file.count; i++)
                                {
                                    CV_Line   line;
                                    CV_Column column = new CV_Column();

                                    line.offset = plinBits.ReadUInt32();
                                    line.flags  = plinBits.ReadUInt32();

                                    uint delta     =  (line.flags & 0x7f000000) >> 24;
                                    bool statement = ((line.flags & 0x80000000) == 0);
                                    uint lineNo    =   line.flags & 0x00FFFFFF;

                                    if((sec.flags & 1) != 0)
                                    {
                                        column.offColumnStart = payloadBits.ReadUInt16();
                                        column.offColumnEnd   = payloadBits.ReadUInt16();
                                    }

                                    lines[i] = new PdbLine( line.offset, lineNo, lineNo + delta, column.offColumnStart, column.offColumnEnd );
                                }
                            }

                            func.LineBlocks = blocks.ToArray();
                            break;
                        }
                }
            }
        }

        static PdbFunction[] LoadManagedFunctions( string      module ,
                                                   ArrayReader bits   ,
                                                   uint        limit  )
        {
            List<PdbFunction> funcList = new List<PdbFunction>();

            while(bits.Position < limit)
            {
                ushort siz = bits.ReadUInt16();

                ArrayReader subBits = bits.CreateSubsetAndAdvance( siz );

                SYM rec = (SYM)subBits.ReadUInt16();
                switch(rec)
                {
                    case SYM.S_GMANPROC:
                    case SYM.S_LMANPROC:
                        {
                            ManProcSym proc = new ManProcSym();

                            proc.parent   = subBits.ReadUInt32();
                            proc.end      = subBits.ReadUInt32();
                            proc.next     = subBits.ReadUInt32();
                            proc.len      = subBits.ReadUInt32();
                            proc.dbgStart = subBits.ReadUInt32();
                            proc.dbgEnd   = subBits.ReadUInt32();
                            proc.token    = subBits.ReadUInt32();
                            proc.off      = subBits.ReadUInt32();
                            proc.seg      = subBits.ReadUInt16();
                            proc.flags    = subBits.ReadUInt8 ();
                            proc.retReg   = subBits.ReadUInt16();
                            proc.name     = subBits.ReadZeroTerminatedUTF8String();


                            funcList.Add( new PdbFunction( module, proc, bits ) );
                        }
                        break;

                    default:
                        throw new PdbException( "Unknown SYMREC {0}", rec );
                }
            }

            return funcList.ToArray();
        }

        static void LoadFuncsFromDbiModule( ArrayReader            bits     ,
                                            DbiModuleInfo          info     ,
                                            Dictionary<int,string> names    ,
                                            List<PdbFunction>      funcList )
        {
            bits.Rewind();

            int sig = bits.ReadInt32();
            if(sig != 4)
            {
                throw new PdbException( "Invalid signature." );
            }

            PdbFunction[] funcs = LoadManagedFunctions( info.moduleName, bits, (uint)info.cbSyms );
            if(funcs != null)
            {
                bits.Position = info.cbSyms + info.cbOldLines;

                LoadManagedLines( funcs, names, bits, (uint)(info.cbSyms + info.cbOldLines + info.cbLines) );

                for(int i = 0; i < funcs.Length; i++)
                {
                    funcList.Add( funcs[i] );
                }
            }
        }

        static List<DbiModuleInfo> LoadDbiStream(     ArrayReader bits              ,
                                                  out ushort      tokenRidMapStream )
        {
            DbiHeader dh = new DbiHeader( bits );

            if(dh.sig != -1 || dh.ver != 19990903)
            {
                throw new PdbException( "Unsupported DBI Stream version" );
            }

            ArrayReader subBits;

            // Read gpmod section.

            List<DbiModuleInfo> modList = new List<DbiModuleInfo>();

            subBits = bits.CreateSubsetAndAdvance( dh.gpmodiSize );

            while(subBits.IsEOF == false)
            {
                modList.Add( new DbiModuleInfo( subBits ) );
            }

            // Skip the Section Contribution substream.

            bits.Seek( dh.secconSize );

            // Skip the Section Map substream.

            bits.Seek( dh.secmapSize );

            // Skip the File Info substream.

            bits.Seek( dh.filinfSize );

            // Skip the TSM substream.

            bits.Seek( dh.tsmapSize );

            // Skip the EC substream.

            bits.Seek( dh.ecinfoSize );

            // Read the optional header.

            subBits = bits.CreateSubsetAndAdvance( dh.dbghdrSize );

            DbgType idx = DbgType.dbgtypeFirst;

            tokenRidMapStream = 0xFFFF;

            while(subBits.IsEOF == false)
            {
                ushort stream = subBits.ReadUInt16();

                if(idx == DbgType.dbgtypeTokenRidMap)
                {
                    tokenRidMapStream = stream;
                    break;
                }

                idx++;
            }

            return modList;
        }

        static uint[] LoadTokenRidMap( ArrayReader bits )
        {
            return bits.ReadUInt32Array( bits.Length / 4 );
        }
    }
}
