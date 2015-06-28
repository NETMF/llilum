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

    public class PdbWriter
    {
        //
        // State
        //

        private ArrayWriter m_writer;
        private int         m_pageSize;
        private int         m_usedBytes;

        //
        // Constructor Methods
        //

        public PdbWriter( ArrayWriter writer   ,
                          int         pageSize )
        {
            m_writer    = writer;
            m_pageSize  = pageSize;
            m_usedBytes = pageSize * 3;
        }

        //
        // Helper Methods
        //

        public void Emit( MsfDirectory dir )
        {
            PdbFileHeader head = new PdbFileHeader( m_pageSize );

            dir.Emit( this, out head.directoryRoot, out head.directorySize );

            WriteFreeMap();

            head.freePageMap = 2;
            head.pagesUsed   = m_usedBytes / m_pageSize;

            Seek( 0, 0 );

            head.Emit( m_writer );
        }

        private void WriteDirectory(     DataStream[] streams       ,
                                     out int          directoryRoot ,
                                     out int          directorySize )
        {
            int pages = 0;

            foreach(DataStream stream in streams)
            {
                if(stream.Length > 0)
                {
                    pages += stream.NumOfPages;
                }
            }

            ArrayWriter writer = new ArrayWriter();

            writer.Write( streams.Length );

            foreach(DataStream stream in streams)
            {
                writer.Write( stream.Length );
            }

            foreach(DataStream stream in streams)
            {
                if(stream.Length > 0)
                {
                    writer.Write( stream.Pages );
                }
            }

            DataStream directory = new DataStream();

            directory.Write( this, writer );

            directorySize = directory.Length;

            //--//

            ArrayWriter writer2 = new ArrayWriter();

            writer2.Write( directory.Pages );

            DataStream ddir = new DataStream();

            ddir.Write( this, writer2 );

            directoryRoot = ddir.Pages[0];
        }

        private void WriteFreeMap()
        {
            byte[] buffer = new byte[m_pageSize];

            // We configure the old free map with only the first 3 pages allocated.
            buffer[0] = 0xF8;
            for(int i = 1; i < m_pageSize; i++)
            {
                buffer[i] = 0xff;
            }

            Seek ( 1, 0                  );
            Write( buffer, 0, m_pageSize );

            // We configure the new free map with all of the used pages gone.
            int count = m_usedBytes / m_pageSize;
            int full  = count / 8;
            for(int i = 0; i < full; i++)
            {
                buffer[i] = 0;
            }
            int rema = count % 8;
            buffer[full] = (byte)(0xff << rema);

            Seek ( 2     , 0             );
            Write( buffer, 0, m_pageSize );
        }

        internal int AllocatePages( int count )
        {
            int begin = m_usedBytes;

            m_usedBytes += count * m_pageSize;

            m_writer.SetLength( m_usedBytes );

            if(m_usedBytes > m_pageSize * m_pageSize * 8)
            {
                throw new Exception( "PdbWriter does not support multiple free maps." );
            }

            return begin / m_pageSize;
        }

        internal void Seek( int page   ,
                            int offset )
        {
            m_writer.Seek( page * m_pageSize + offset );
        }

        internal void Write( byte[] bytes  ,
                             int    offset ,
                             int    count  )
        {
            m_writer.Write( bytes, offset, count );
        }

        //
        // Access Methods
        //

        public int PageSize
        {
            get
            {
                return m_pageSize;
            }
        }
    }
}
