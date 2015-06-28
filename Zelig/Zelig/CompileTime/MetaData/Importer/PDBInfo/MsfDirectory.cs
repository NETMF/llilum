//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
//  Originally from the Microsoft Research Singularity code base.
//
namespace Microsoft.Zelig.MetaData.Importer.PdbInfo.Features
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Microsoft.Zelig.MetaData.Importer.PdbInfo.CodeView;

    public class MsfDirectory
    {
        //
        // State
        //

        private DataStream[] m_streams;

        //
        // Constructor Methods
        //

        public MsfDirectory()
        {
            m_streams = new DataStream[0];
        }

        public MsfDirectory( PdbReader     reader ,
                             PdbFileHeader head   )
        {
            // 0..n in page of directory pages.
            reader.Seek( head.directoryRoot, 0 );

            DataStream stream = new DataStream( head.directorySize, reader, reader.Image );

            ArrayReader bits = stream.GetReader();

            // 0..3 in directory pages
            int count = bits.ReadInt32();

            // 4..n
            int[] sizes = bits.ReadInt32Array( count );

            // n..m
            m_streams = new DataStream[count];
            for(int i = 0; i < count; i++)
            {
                if(sizes[i] <= 0)
                {
                    m_streams[i] = new DataStream();
                }
                else
                {
                    m_streams[i] = new DataStream( sizes[i], reader, bits );
                }
            }
        }

        //
        // Helper Methods
        //

        public ArrayReader GetArrayReaderForStream( int idx )
        {
            return m_streams[idx].GetReader();
        }

        public DataStream CreateNewStream()
        {
            int idx;

            return CreateNewStream( out idx );
        }

        public DataStream CreateNewStream( out int idx )
        {
            idx = m_streams.Length;

            return CreateNewStream( idx );
        }

        public DataStream CreateNewStream( int idx )
        {
            m_streams = ArrayUtility.EnsureSizeOfNotNullArray( m_streams, idx + 1 );

            DataStream res = new DataStream();

            m_streams[idx] = res;

            return res;
        }

        //--//

        public void Emit(     PdbWriter pdbWriter     ,
                          out int       directoryRoot ,
                          out int       directorySize )
        {
            int pages = 0;

            foreach(DataStream stream in m_streams)
            {
                if(stream.Length > 0)
                {
                    pages += stream.NumOfPages;
                }
            }

            ArrayWriter writer = new ArrayWriter();

            writer.Write( m_streams.Length );

            foreach(DataStream stream in m_streams)
            {
                writer.Write( stream.Length );
            }

            foreach(DataStream stream in m_streams)
            {
                if(stream.Length > 0)
                {
                    writer.Write( stream.Pages );
                }
            }

            DataStream directory = new DataStream();

            directory.Write( pdbWriter, writer );

            directorySize = directory.Length;

            //--//

            ArrayWriter writer2 = new ArrayWriter();

            writer2.Write( directory.Pages );

            DataStream ddir = new DataStream();

            ddir.Write( pdbWriter, writer2 );

            directoryRoot = ddir.Pages[0];
        }

        //
        // Access Methods
        //

        public DataStream[] Streams
        {
            get
            {
                return m_streams;
            }
        }
    }
}
