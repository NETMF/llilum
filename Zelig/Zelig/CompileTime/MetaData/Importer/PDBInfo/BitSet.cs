//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
//  Originally from the Microsoft Research Singularity code base.
//
namespace Microsoft.Zelig.MetaData.Importer.PdbInfo.Features
{
    public class BitSet
    {
        //
        // State
        //

        private int    m_size;
        private uint[] m_words;

        //
        // Constructor Methods
        //

        public BitSet( ArrayReader bits )
        {
            m_size  = bits.ReadInt32      (        );    // 0..3 : Number of words
            m_words = bits.ReadUInt32Array( m_size );
        }

        public BitSet( int size )
        {
            m_size  = size;
            m_words = new uint[size];
        }

        //
        // Helper Methods
        //

        public void Emit( ArrayWriter writer )
        {
            writer.Write( m_size  );
            writer.Write( m_words );
        }

        //--//

        public bool IsSet( int index )
        {
            int word = index / 32;

            if(word >= 0 && word < m_words.Length)
            {
                return ((m_words[word] & GetBit( index )) != 0);
            }

            return false;
        }

        public void Set( int index )
        {
            int word = index / 32;

            EnsureSize( word );

            m_words[word] |= GetBit( index );
        }

        public void Clear( int index )
        {
            int word = index / 32;

            EnsureSize( word );

            m_words[word] &= ~GetBit( index );
        }

        private void EnsureSize( int word )
        {
            m_words = ArrayUtility.EnsureSizeOfNotNullArray( m_words, word + 1 );
            m_size  = m_words.Length;
        }

        private static uint GetBit( int index )
        {
            return ((uint)1 << (index % 32));
        }

        //
        // Access Methods
        //

        public bool IsEmpty
        {
            get
            {
                return m_size == 0;
            }
        }
    }
}
