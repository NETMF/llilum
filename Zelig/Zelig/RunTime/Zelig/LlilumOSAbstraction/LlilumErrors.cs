//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction
{
    using System;

    using RT = Microsoft.Zelig.Runtime;


    public static class LlilumErrors
    {
        public const uint S_OK                  = 0;
        public const uint S_FALSE               = 1;
        public const uint E_NOTIMPL             = 0xCA000000;
        public const uint E_IO                  = 0xCF000000;
        public const uint E_INVALID_OPERATION   = 0xD6000000;
        public const uint E_OUT_OF_MEMORY       = 0xFA000000;
        public const uint E_NOT_SUPPORTED       = 0xF7000000;
        public const uint E_INVALID_PARAMETER   = 0xFD000000;
        public const uint E_TIMEOUT             = 0xFE000000;
        public const uint E_FAIL                = 0xFF000000;

        public static bool Succeeded( uint result )
        {
            return ( result & 0x80000000UL ) == 0;
        }

        public static bool Failed( uint result )
        {
            return ( result & 0x80000000UL ) != 0;
        }

        public static void ThrowOnError(uint result, bool ignoreNotSupportedOrImplemented)
        {
            if( Failed( result ) )
            {
                switch( result )
                {
                    case E_NOTIMPL:
                        if(!ignoreNotSupportedOrImplemented)
                        {
                            throw new NotImplementedException( );
                        }
                        break;
                    case E_INVALID_OPERATION:
                        throw new InvalidOperationException( );
                    case E_OUT_OF_MEMORY:
                        throw new OutOfMemoryException( );
                    case E_NOT_SUPPORTED:
                        if(!ignoreNotSupportedOrImplemented)
                        {
                            throw new NotSupportedException( );
                        }
                        break;
                    case E_INVALID_PARAMETER:
                        throw new ArgumentException( );
                    case E_TIMEOUT:
                        throw new TimeoutException( );
                    case E_IO:
                    case E_FAIL:
                    default:
                        throw new Exception( );
                }
            }
        }

        [RT.ExportedMethod]
        public static void LLOS_Die()
        {
            RT.BugCheck.Raise( Runtime.BugCheck.StopCode.Fault_Unknown ); 
        }
    }
}
