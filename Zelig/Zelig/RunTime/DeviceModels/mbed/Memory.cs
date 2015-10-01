//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Support.mbed
{
    using System.Runtime.InteropServices;

    //--//

    public static class Memory
    {
        public static unsafe byte* RequestMemoryPool( uint* size )
        {
            byte* mem = null;

            //
            // never try and allocate more than than half of the address space
            //
            int requestSize = (int)System.Math.Min(*size, System.UInt32.MaxValue / 2);
            do
            {
                mem = malloc( (uint)requestSize ); 

                if(mem == null)
                {
                    requestSize -= requestSize / 10;
                }
            } while(mem == null && requestSize > 0);

            *size = (uint)requestSize;

            return mem;
        }

        
        public static unsafe void FreeMemoryPool( byte* mem )
        {
            if(mem != null)
            {
                free( mem );
            }
        }

        [DllImport("C")]
        public static unsafe extern byte* malloc( uint size );

        [DllImport("C")]
        public static unsafe extern byte* free( byte* mem );
    }


}
