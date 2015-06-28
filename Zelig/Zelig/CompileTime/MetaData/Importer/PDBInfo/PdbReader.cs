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

    public class PdbReader
    {
        //
        // State
        //

        private readonly ArrayReader m_image;
        private readonly int         m_pageSize;

        //
        // Constructor Methods
        //

        public PdbReader( ArrayReader image    ,
                          int         pageSize )
        {
            m_image    = image;
            m_pageSize = pageSize;
        }

        //
        // Helper Methods
        //

        public void Seek( int page   ,
                          int offset )
        {
            m_image.SeekAbsolute( page * m_pageSize + offset );
        }

        public void Read( byte[] bytes  ,
                          int    offset ,
                          int    count  )
        {
            m_image.CopyIntoArray( bytes, offset, count );
        }

        public int PagesFromSize( int size )
        {
            return (size + m_pageSize - 1) / (m_pageSize);
        }

        public ArrayReader Image
        {
            get
            {
                return m_image;
            }
        }

        public int PageSize
        {
            get
            {
                return m_pageSize;
            }
        }
    }
}
