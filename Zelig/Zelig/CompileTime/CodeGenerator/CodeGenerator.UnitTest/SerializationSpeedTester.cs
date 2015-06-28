//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.UnitTest
{
    using System;
    using System.Collections.Generic;

    using IR                     = Microsoft.Zelig.CodeGeneration.IR;
    using RT                     = Microsoft.Zelig.Runtime;
    using TS                     = Microsoft.Zelig.Runtime.TypeSystem;

    class SerializationSpeedTester
    {
        private class TypeSystemForExecution : IR.TypeSystemForCodeTransformation
        {
            internal TypeSystemForExecution() : base( null )
            {
            }
        }

        public static void Run( string[] args )
        {
////        string file = @"S:\enlistments\VOX\main\ZeligUnitTestResults\Microsoft.NohauLPC3180Loader.ZeligImage";
////        string file = @"S:\enlistments\VOX\main\ZeligUnitTestResults\mscorlib_UnitTest_FF.ZeligImage";
            string file = @"S:\enlistments\VOX\main\ZeligUnitTestResults\soloTest_NXP.ZeligImage";

            IR.TypeSystemForCodeTransformation ts;

            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();

            st.Start();
            using(System.IO.Stream serializedStream = new System.IO.FileStream( file, System.IO.FileMode.Open ))
            {
                IR.TypeSystemSerializer.CreateInstance dlgCreateInstance = delegate( Type t )
                {
                    if(t == typeof(IR.TypeSystemForCodeTransformation))
                    {
                        return new TypeSystemForExecution();
                    }

                    return null;
                };

                IR.TypeSystemSerializer.ProgressCallback dlgProgress = delegate( long pos, long total )
                {
                };

                ts = (IR.TypeSystemForCodeTransformation)IR.TypeSystemSerializer.Deserialize( serializedStream, dlgCreateInstance, dlgProgress, 10000 );
            }
            st.Stop();
            long val1 = st.ElapsedMilliseconds;

            st.Reset();
        }
    }
}
