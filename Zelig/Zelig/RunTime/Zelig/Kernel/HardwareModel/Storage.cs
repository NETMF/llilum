//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ImplicitInstance]
    [ForceDevirtualization]
    public abstract class Storage
    {
        //
        // State
        //

        //
        // Helper Methods
        //

        public abstract void InitializeStorage();

        //--//

        public abstract bool EraseSectors( UIntPtr addressStart ,
                                           UIntPtr addressEnd   );

        public abstract bool WriteByte ( UIntPtr address, byte   val );
        public abstract bool WriteShort( UIntPtr address, ushort val );
        public abstract bool WriteWord ( UIntPtr address, uint   val );

        public abstract bool Write( UIntPtr address  ,
                                    byte[]  buffer   ,
                                    uint    offset   ,
                                    uint    numBytes );

        //--//

        public abstract byte   ReadByte ( UIntPtr address );
        public abstract ushort ReadShort( UIntPtr address );
        public abstract uint   ReadWord ( UIntPtr address );

        public abstract void Read( UIntPtr address  ,
                                   byte[]  buffer   ,
                                   uint    offset   ,
                                   uint    numBytes );

        public abstract void SubstituteFirmware( UIntPtr addressDestination ,
                                                 UIntPtr addressSource      ,
                                                 uint    numBytes           );

        public abstract void RebootDevice();

        //--//

        //
        // Access Methods
        //

        public static extern Storage Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}
