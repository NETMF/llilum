//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.VoxSoloFormFactorLoader
{
    using System;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class Storage : RT.Storage
    {
        //
        // Helper Methods
        //

        public override void InitializeStorage()
        {
        }

        //--//

        public override bool EraseSectors( UIntPtr addressStart ,
                                           UIntPtr addressEnd   )
        {
            return false;
        }

        public override bool WriteByte( UIntPtr address ,
                                        byte    val     )
        {
            return false;
        }

        public override bool WriteShort( UIntPtr address ,
                                         ushort  val     )
        {
            return false;
        }

        public override bool WriteWord( UIntPtr address ,
                                        uint    val     )
        {
            return false;
        }

        public override bool Write( UIntPtr address  ,
                                    byte[]  buffer   ,
                                    uint    offset   ,
                                    uint    numBytes )
        {
            return false;
        }

        //--//

        public override unsafe byte ReadByte( UIntPtr address )
        {
            return 0;
        }

        public override unsafe ushort ReadShort( UIntPtr address )
        {
            return 0;
        }

        public override unsafe uint ReadWord( UIntPtr address )
        {
            return 0;
        }

        public override void Read( UIntPtr address  ,
                                   byte[]  buffer   ,
                                   uint    offset   ,
                                   uint    numBytes )
        {
        }

        public override void SubstituteFirmware( UIntPtr addressDestination ,
                                                 UIntPtr addressSource      ,
                                                 uint    numBytes           )
        {
        }

        public override void RebootDevice()
        {
            throw new System.NotImplementedException();
        }
    }
}
