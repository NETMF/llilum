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

    public class PdbStream
    {
        //
        // State
        //

        private int                       m_ver;
        private int                       m_sig;
        private int                       m_age;
        private Guid                      m_guid;
        private Dictionary< string, int > m_lookupStream = new Dictionary< string, int >();


        //
        // Constructor Methods
        //

        public PdbStream( ArrayReader bits )
        {
            m_ver  = bits.ReadInt32();    //  0..3  Version
            m_sig  = bits.ReadInt32();    //  4..7  Signature
            m_age  = bits.ReadInt32();    //  8..11 Age
            m_guid = bits.ReadGuid();     // 12..27 GUID

            if(m_ver != 20000404)
            {
                throw new PdbException( "Unsupported PDB Stream version: {0}", m_ver );
            }

            // Read string buffer.
            int         stringBufLen = bits.ReadInt32();    // 28..31 Bytes of Strings
            ArrayReader stringBits   = bits.CreateSubsetAndAdvance( stringBufLen );

            // Read map index.
            int cnt = bits.ReadInt32(); // n+0..3 hash size.
            int max = bits.ReadInt32(); // n+4..7 maximum ni.

            BitSet present = new BitSet( bits );
            BitSet deleted = new BitSet( bits );

            if(!deleted.IsEmpty)
            {
                throw new PdbException( "Unsupported PDB deleted bitset is not empty." );
            }

            int j = 0;

            for(int i = 0; i < max; i++)
            {
                if(present.IsSet( i ))
                {
                    int ns = bits.ReadInt32();
                    int ni = bits.ReadInt32();

                    stringBits.Position = ns;

                    string name = stringBits.ReadZeroTerminatedUTF8String();

                    SetStreamNumber( name, ni );

                    j++;
                }
            }

            if(j != cnt)
            {
                throw new PdbException( "Stream count mismatch" );
            }
        }

        //
        // Helper Methods
        //

        public int GetStreamNumber( string name )
        {
            int res;

            if(m_lookupStream.TryGetValue( name, out res ) == false)
            {
                return -1;
            }

            return res;
        }

        public void SetStreamNumber( string name ,
                                     int    idx  )
        {
            m_lookupStream[name] = idx;
        }

        public void Emit( ArrayWriter writer )
        {
            writer.Write( m_ver  ); //  0..3  Version
            writer.Write( m_sig  ); //  4..7  Signature
            writer.Write( m_age  ); //  8..11 Age
            writer.Write( m_guid ); // 12..27 GUID

            var    lookupOffset = new Dictionary< string, int >();
            var    subWriter    = new ArrayWriter();
            BitSet present      = new BitSet( 0 );
            BitSet deleted      = new BitSet( 0 );
            int    cnt          = 0;
            int    max          = 0;


            //
            // Streams should be sorted by index.
            //
            var lst = new List< KeyValuePair< string, int > >( m_lookupStream );

            lst.Sort( delegate( KeyValuePair< string, int > x, KeyValuePair< string, int > y ) { return x.Value.CompareTo( y.Value ); } );

            foreach(var pair in lst)
            {
                var name = pair.Key;

                if(lookupOffset.ContainsKey( name ) == false)
                {
                    lookupOffset[name] = subWriter.Length;

                    subWriter.WriteZeroTerminatedUTF8String( name );
                }

                present.Set( cnt++ );

                max = Math.Max( max, m_lookupStream[name] );
            }

            // Write string buffer.
            writer.Write( subWriter.Length );    // 28..31 Bytes of Strings
            writer.Write( subWriter        );

            // Write map index.
            writer.Write( cnt ); // n+0..3 hash size.
            writer.Write( max ); // n+4..7 maximum ni.

            present.Emit( writer );
            deleted.Emit( writer );

            foreach(var pair in lst)
            {
                writer.Write( lookupOffset[pair.Key  ] );
                writer.Write(              pair.Value  );
            }

            writer.Write( (int)0 );
        }
    }
}
