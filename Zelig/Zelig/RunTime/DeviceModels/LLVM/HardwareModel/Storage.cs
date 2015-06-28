//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.LLVMHosted
{
    using System;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    public sealed class Storage : RT.Storage
    {
        public override void InitializeStorage( )
        {

        }

        //--//

        public override bool EraseSectors( UIntPtr addressStart,
                                           UIntPtr addressEnd )
        {
            throw new Exception( "EraseSectors not implemented" );
        }

        public override bool WriteByte( UIntPtr address, byte val )
        {
            throw new Exception( "WriteByte not implemented" );
        }
        public override bool WriteShort( UIntPtr address, ushort val )
        {
            throw new Exception( "WriteShort not implemented" );
        }
        public override bool WriteWord( UIntPtr address, uint val )
        {
            throw new Exception( "WriteWord not implemented" );
        }

        public override bool Write( UIntPtr address,
                                    byte[] buffer,
                                    uint offset,
                                    uint numBytes )
        {
            throw new Exception( "Write not implemented" );
        }

        //--//

        public override byte ReadByte( UIntPtr address )
        {
            throw new Exception( "ReadByte not implemented" );
        }
        public override ushort ReadShort( UIntPtr address )
        {
            throw new Exception( "ReadShort not implemented" );
        }
        public override uint ReadWord( UIntPtr address )
        {
            throw new Exception( "ReadWord not implemented" );
        }

        public override void Read( UIntPtr address,
                                   byte[] buffer,
                                   uint offset,
                                   uint numBytes )
        {
            throw new Exception( "Read not implemented" );
        }

        public override void SubstituteFirmware( UIntPtr addressDestination,
                                                 UIntPtr addressSource,
                                                 uint numBytes )
        {
            throw new Exception( "SubstituteFirmware not implemented" );
        }

        public override void RebootDevice( )
        {

        }

    }
}
