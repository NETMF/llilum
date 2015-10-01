//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv4
{
    using System;

    using TS     = Microsoft.Zelig.Runtime.TypeSystem;
    using EncDef = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition;


    public static class Coprocessor14
    {
        [Inline]
        public static void WriteDebugCommData( uint value )
        {
            ProcessorARMv4.MoveToCoprocessor( 14, 0, 1, 0, 0, value );
        }

        [Inline]
        public static uint ReadDebugCommData()
        {
            return ProcessorARMv4.MoveFromCoprocessor( 14, 0, 1, 0, 0 );
        }

        [Inline]
        public static uint ReadDebugCommStatus()
        {
            return ProcessorARMv4.MoveFromCoprocessor( 14, 0, 0, 0, 0 );
        }

        //--//

        [Inline]
        public static bool CanWriteDCC()
        {
            return (ReadDebugCommStatus() & 2) == 0;
        }

        [Inline]
        public static bool CanReadDCC()
        {
            return (ReadDebugCommStatus() & 1) != 0;
        }

        public static void WriteDCC( uint value )
        {
            while(!CanWriteDCC())
            {
            }

            WriteDebugCommData( value );
        }

        public static uint ReadDCC()
        {
            while(!CanReadDCC())
            {
            }

            return ReadDebugCommData();
        }

        //--//

        public static bool WriteDCCWithTimeout( uint value   ,
                                                int  timeout )
        {
            while((ReadDebugCommStatus() & 2) != 0)
            {
                if(--timeout < 0)
                {
                    return false;
                }
            }

            WriteDebugCommData( value );

            return true;
        }

        public static bool ReadDCCWithTimeout( out uint value   ,
                                                   int  timeout )
        {
            while((ReadDebugCommStatus() & 1) != 0)
            {
                if(--timeout < 0)
                {
                    value = 0;
                    return false;
                }
            }

            value = ReadDebugCommData();

            return true;
        }
    }
}
