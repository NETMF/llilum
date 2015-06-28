//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    using System;
    using System.Collections.Generic;

    using IR = Microsoft.Zelig.CodeGeneration.IR;
    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    internal sealed class BitFieldValueHandle : AbstractValueHandle
    {
        //
        // State
        //

        private readonly AbstractValueHandle   m_value;
        private readonly TS.TypeRepresentation m_ownerType;
        private readonly IR.BitFieldDefinition m_bfDef;

        //
        // Contructor Methods
        //

        internal BitFieldValueHandle( TS.TypeRepresentation            type                     ,
                                      TS.CustomAttributeRepresentation caMemoryMappedPeripheral ,
                                      TS.CustomAttributeRepresentation caMemoryMappedRegister   ,
                                      AbstractValueHandle              value                    ,
                                      TS.TypeRepresentation            ownerType                ,
                                      IR.BitFieldDefinition            bfDef                    ) : base( type, caMemoryMappedPeripheral, caMemoryMappedRegister, true )
        {
            m_value     = value;
            m_ownerType = ownerType;
            m_bfDef     = bfDef;
        }

        //
        // Helper Methods
        //

        public override bool IsEquivalent( AbstractValueHandle abstractValueHandle )
        {
            var other = abstractValueHandle as BitFieldValueHandle;

            if(other != null)
            {
                if(this.m_ownerType == other.m_ownerType &&
                   this.m_bfDef     == other.m_bfDef      )
                {
                    return this.m_value.IsEquivalent( other.m_value );
                }
            }

            return false;
        }

        public override Emulation.Hosting.BinaryBlob Read(     int  offset   ,
                                                               int  count    ,
                                                           out bool fChanged )
        {
            ulong rebuiltValue;

            if(ExtractValue( out rebuiltValue, out fChanged ) == false)
            {
                return null;
            }

            var bbRebuiltValue = new Emulation.Hosting.BinaryBlob( (int)m_ownerType.Size );

            bbRebuiltValue.WriteUInt64( rebuiltValue, 0 );

            return bbRebuiltValue.Extract( offset, count );
        }

        public override bool Write( Emulation.Hosting.BinaryBlob bb     ,
                                    int                          offset ,
                                    int                          count  )
        {
            ulong rebuiltValue;
            bool  fChanged;

            if(ExtractValue( out rebuiltValue, out fChanged ) == false)
            {
                return false;
            }

            var bbRebuiltValue = new Emulation.Hosting.BinaryBlob( (int)m_ownerType.Size );

            bbRebuiltValue.WriteUInt64( rebuiltValue, 0 );

            bbRebuiltValue.Insert( bb, offset, count );

            ulong rebuildValueNew = bbRebuiltValue.ReadUInt64( 0 );

            return InsertValue( rebuildValueNew );
        }

        //--//

        private bool ExtractValue( out ulong value    ,
                                   out bool  fChanged )
        {
            value = 0;

            var bbOwner = m_value.Read( out fChanged );
            if(bbOwner == null)
            {
                return false;
            }

            ulong rawValue     = bbOwner.ReadUInt64( 0 );
            ulong rebuiltValue = 0;

            foreach(var section in m_bfDef.Sections)
            {
                rebuiltValue |= ((rawValue >> (int)section.Position) & ((1ul << (int)section.Size) - 1)) << (int)section.Offset;
            }

            if(this.Type.IsSigned)
            {
                int  unusedBits  = (64 - (int)m_bfDef.TotalSize);
                long signedValue = (long)(rebuiltValue << unusedBits);

                rebuiltValue = (ulong)(signedValue >> unusedBits);
            }

            value = rebuiltValue;
            return true;
        }

        private bool InsertValue( ulong value )
        {
            bool fChanged;
    
            var bbOwner = m_value.Read( out fChanged );
            if(bbOwner == null)
            {
                return false;
            }
    
            ulong rawValue = bbOwner.ReadUInt64( 0 );
    
            foreach(var section in m_bfDef.Sections)
            {
                ulong mask = ((1ul << (int)section.Size) - 1);

                rawValue &= ~(                                 mask  << (int)section.Position);
                rawValue |=  ((value >> (int)section.Offset) & mask) << (int)section.Position ;
            }
    
            bbOwner.WriteUInt64( rawValue, 0 );

            return m_value.Write( bbOwner );
        }

        //
        // Access Methods
        //

        public override bool CanUpdate
        {
            get
            {
                return m_value.CanUpdate;
            }
        }

        public override bool HasAddress
        {
            get
            {
                return m_value.HasAddress;
            }
        }

        public override uint Address
        {
            get
            {
                return m_value.Address;
            }
        }
    }
}