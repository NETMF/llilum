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

    public class DataStream
    {
        //
        // State
        //

        private int    m_contentSize;
        private int[]  m_pages;
        private byte[] m_payload;

        //
        // Constructor Methods
        //

        public DataStream()
        {
            m_contentSize = 0;
            m_pages       = new int[0];
            m_payload     = new byte[0];
        }

        internal DataStream( int         contentSize ,
                             PdbReader   reader      ,
                             ArrayReader bits        )
        {
            m_contentSize = contentSize;

            int pageCount = reader.PagesFromSize( contentSize );
            if(pageCount > 0)
            {
                m_pages = bits.ReadInt32Array( pageCount );
            }
            else
            {
                m_pages = new int[0];
            }

            m_payload = new byte[contentSize];

            int pageSize   = reader.PageSize;
            int pageNumber = 0;
            int offset     = 0;

            while(contentSize > 0)
            {
                int pageAvailable = Math.Min( pageSize, contentSize );

                reader.Seek( m_pages[pageNumber], 0 );
                reader.Read( m_payload, offset, pageAvailable );

                pageNumber  += 1;
                offset      += pageAvailable;
                contentSize -= pageAvailable;
            }
        }

        //
        // Helper Methods
        //

        public ArrayReader GetReader()
        {
            return new ArrayReader( m_payload );
        }

        public void Copy( PdbWriter  pdbWriter ,
                          DataStream src       )
        {
            if(src.Length > 0)
            {
                byte[] data = src.Payload;

                Write( pdbWriter, data, data.Length );
            }
        }

        internal void Write( PdbWriter   writer ,
                             ArrayWriter buffer )
        {
            byte[] bytes = buffer.ToArray();
            int    data  = bytes.Length;

            Write( writer, bytes, data );
        }

        internal void Write( PdbWriter writer ,
                             byte[]    bytes  ,
                             int       data   )
        {
            if(bytes == null || data == 0)
            {
                return;
            }
    
            int left = data;
            int used = 0;
            int rema = m_contentSize % writer.PageSize;

            if(rema != 0)
            {
                int todo = writer.PageSize - rema;
                if(todo > left)
                {
                    todo = left;
                }
    
                int lastPage = m_pages[m_pages.Length - 1];

                writer.Seek( lastPage, rema );
                writer.Write( bytes, used, todo );

                used += todo;
                left -= todo;
            }
    
            if(left > 0)
            {
                int count = (left + writer.PageSize - 1) / writer.PageSize;
                int page0 = writer.AllocatePages( count );
    
                writer.Seek( page0, 0 );
                writer.Write( bytes, used, left );
    
                AddPages( page0, count );
            }
    
            m_contentSize += data;
        }
    
        private void AddPages( int page0 ,
                               int count )
        {
            int used = m_pages.Length;

            m_pages = ArrayUtility.EnsureSizeOfNotNullArray( m_pages, used + count );

            for(int i = 0; i < count; i++)
            {
                m_pages[used + i] = page0 + i;
            }
        }

        public int GetPage( int index )
        {
            return m_pages[index];
        }

        //
        // Access Methods
        //

        public int[] Pages
        {
            get
            {
                return m_pages;
            }
        }

        public int NumOfPages
        {
            get
            {
                return m_pages.Length;
            }
        }

        public int Length
        {
            get
            {
                return m_contentSize;
            }
        }

        public byte[] Payload
        {
            get
            {
                return m_payload;
            }

            set
            {
                m_contentSize = value.Length;
                m_payload     = value;
                m_pages       = null;
            }
        }
    }
}
