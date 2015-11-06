//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Microsoft.Zelig.MetaData.Importer;
    using Microsoft.Zelig.MetaData.Importer.PdbInfo;
    using Microsoft.Zelig.Test;

    class PDBFileTester : TestBase
    {
        override public TestResult Run( string[] args )
        {
            base.Run(args);

            string currentDir = System.Environment.CurrentDirectory;

            LoadSymbols( Expand( currentDir + @"\Test\TestPayload__CLR1_1__VanillaSingleClass.pdb" ) );

            return TestResult.Pass;
        }

        static string Expand( string file )
        {
            return Environment.ExpandEnvironmentVariables( file );
        }

        static PdbFunction[] LoadSymbols( string file )
        {
            byte[] image = System.IO.File.ReadAllBytes( file );

            PdbFile pdbFile = new PdbFile( new ArrayReader( image ) );

            PdbFunction[] funcs = pdbFile.Functions;

            for(int f = 0; f < funcs.Length; f++)
            {
                Dump( funcs[f], 0 );
            }

            return funcs;
        }

        //--//

        public static void Dump( PdbFunction f      ,
                                 int         indent )
        {
            string pad = new String( ' ', indent );

            Console.WriteLine( "            {0}Func: [{1}] addr={2:x4}:{3:x8} len={4:x4} token={5:x8}",
                              pad,
                              f.Name, f.Segment, f.Address, f.Length, f.Token );

            if(f.Metadata != null)
            {
                Console.Write( "            {0} Meta: [", pad );
                for(int i = 0; i < f.Metadata.Length; i++)
                {
                    Console.Write( "{0:x2}", f.Metadata[i] );
                }
                Console.WriteLine( "]" );
            }

            if(f.Scopes != null)
            {
                for(int i = 0; i < f.Scopes.Length; i++)
                {
                    Dump( f.Scopes[i], indent + 1 );
                }
            }

            if(f.LineBlocks != null)
            {
                for(int i = 0; i < f.LineBlocks.Length; i++)
                {
                    Dump( f.LineBlocks[i], indent + 1 );
                }
            }
        }

        public static void Dump( PdbScope s      ,
                                 int      indent )
        {
            string pad = new String( ' ', indent );

            Console.WriteLine( "            {0}Scope: addr={1:x4}:{2:x8} len={3:x4}",
                              pad, s.Segment, s.Address, s.Length );

            if(s.Slots != null)
            {
                for(int i = 0; i < s.Slots.Length; i++)
                {
                    Dump( s.Slots[i], indent + 1 );
                }
            }

            if(s.Scopes != null)
            {
                for(int i = 0; i < s.Scopes.Length; i++)
                {
                    Dump( s.Scopes[i], indent + 1 );
                }
            }
        }

        public static void Dump( PdbSlot s      ,
                                 int     indent )
        {
            string pad = new String( ' ', indent );

            Console.WriteLine( "            {0}Slot: {1,2} [{2}] addr={3:x4}:{4:x8}",
                              pad, s.Slot, s.Name, s.Segment, s.Address );
        }


        public static void Dump( PdbSource s      ,
                                 int       indent )
        {
            string pad = new String( ' ', indent );

            Console.WriteLine( "            {0}[{1}] : {2}", pad, s.Name, s.Index );
        }

        public static void Dump( PdbLines s      ,
                                 int      indent )
        {
            string pad = new String( ' ', indent );

            Console.WriteLine( "            {0}[{1}]", pad, s.File.Name );

            for(int i = 0; i < s.Lines.Length; i++)
            {
                Dump( s.Lines[i], indent + 1 );
            }
        }

        public static void Dump( PdbLine s      ,
                                 int     indent )
        {
            string pad = new String( ' ', indent );

            if(s.LineBegin == 0xFEEFEE && s.ColumnBegin == 0 && s.ColumnEnd == 0)
            {
                Console.WriteLine( "            {0}off={1:x8} #---------",
                                  pad, s.Offset, s.LineBegin, s.ColumnBegin );
            }
            else
            {
                Console.WriteLine( "            {0}off={1:x8} #{2,6},{3,2}",
                                  pad, s.Offset, s.LineBegin, s.ColumnBegin );
            }
        }
    }
}
