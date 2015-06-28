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


    public sealed class MemoryValueHandle : AbstractValueHandle
    {
        //
        // State
        //

        public  readonly MemoryDelta MemoryDelta;
        private readonly uint        m_address;

        //
        // Contructor Methods
        //

        public MemoryValueHandle( ImageInformation.PointerContext pc                 ,
                                  bool                            fAsHoldingVariable ) : this( pc.Type, null, null, fAsHoldingVariable, pc.MemoryDelta, pc.Address )
        {
        }

        public MemoryValueHandle( TS.TypeRepresentation            type                     ,
                                  TS.CustomAttributeRepresentation caMemoryMappedPeripheral ,
                                  TS.CustomAttributeRepresentation caMemoryMappedRegister   ,
                                  bool                             fAsHoldingVariable       ,
                                  MemoryDelta                      memoryDelta              ,
                                  uint                             address                  ) : base( type, caMemoryMappedPeripheral, caMemoryMappedRegister, fAsHoldingVariable )
        {
            this.MemoryDelta = memoryDelta;
            this.m_address   = address;
        }

        //
        // Helper Methods
        //

        public override bool IsEquivalent( AbstractValueHandle abstractValueHandle )
        {
            var other = abstractValueHandle as MemoryValueHandle;

            if(other != null)
            {
                if(this.m_address == other.m_address)
                {
                    return true;
                }
            }

            return false;
        }

        public override Emulation.Hosting.BinaryBlob Read(     int  offset   ,
                                                               int  count    ,
                                                           out bool fChanged )
        {
            var  memoryDelta = this.MemoryDelta;
            uint address     = m_address + (uint)offset;

            switch(count)
            {
                case 1:
                    {
                        byte result;

                        if(memoryDelta.GetUInt8( address, true, out result, out fChanged ))
                        {
                            return Emulation.Hosting.BinaryBlob.Wrap( result );
                        }
                    }
                    break;

                case 2:
                    {
                        ushort result;

                        if(memoryDelta.GetUInt16( address, true, out result, out fChanged ))
                        {
                            return Emulation.Hosting.BinaryBlob.Wrap( result );
                        }
                    }
                    break;

                case 4:
                    {
                        uint result;

                        if(memoryDelta.GetUInt32( address, true, out result, out fChanged ))
                        {
                            return Emulation.Hosting.BinaryBlob.Wrap( result );
                        }
                    }
                    break;

                case 8:
                    {
                        ulong result;

                        if(memoryDelta.GetUInt64( address, true, out result, out fChanged ))
                        {
                            return Emulation.Hosting.BinaryBlob.Wrap( result );
                        }
                    }
                    break;

                default:
                    {
                        byte[] result;

                        if(memoryDelta.GetBlock( address, count, true, out result, out fChanged ))
                        {
                            return new Emulation.Hosting.BinaryBlob( result );
                        }
                    }
                    break;
            }

            fChanged = false;

            return null;
        }

        public override bool Write( Microsoft.Zelig.Emulation.Hosting.BinaryBlob bb     ,
                                    int                                          offset ,
                                    int                                          count  )
        {
            var  memoryDelta = this.MemoryDelta;
            uint address     = m_address + (uint)offset;

            switch(count)
            {
                case 1:
                    {
                        byte result = bb.ReadUInt8( 0 );

                        return memoryDelta.SetUInt8( address, result );
                    }

                case 2:
                    {
                        ushort result = bb.ReadUInt16( 0 );

                        return memoryDelta.SetUInt16( address, result );
                    }

                case 4:
                    {
                        uint result = bb.ReadUInt32( 0 );

                        return memoryDelta.SetUInt32( address, result );
                    }

                case 8:
                    {
                        ulong result = bb.ReadUInt64( 0 );

                        return memoryDelta.SetUInt64( address, result );
                    }

                default:
                    {
                        byte[] result = bb.ReadBlock( 0, count );

                        return memoryDelta.SetBlock( address, result );
                    }
            }
        }

        //
        // Access Methods
        //

        public override bool CanUpdate
        {
            get
            {
                foreach(var block in this.MemoryDelta.ImageInformation.ImageBuilder.MemoryBlocks)
                {
                    if(block.Contains( this.Address ))
                    {
                        switch(block.Attributes & RT.MemoryAttributes.LocationMask)
                        {
                            case RT.MemoryAttributes.FLASH:
                            case RT.MemoryAttributes.ROM:
                                return false;
                        }
                    }
                }

                return true;
            }
        }

        public override bool HasAddress
        {
            get
            {
                return true;
            }
        }

        public override uint Address
        {
            get
            {
                return m_address;
            }
        }
    }
}