//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using Cfg = Microsoft.Zelig.Configuration.Environment;

    public abstract class MemoryProvider
    {
        //
        // State
        //

        AbstractHost                m_owner;
        Cfg.CacheControllerCategory m_cache;

        //
        // Constructor Methods
        //

        protected MemoryProvider( AbstractHost                owner ,
                                  Cfg.CacheControllerCategory cache )
        {
            m_cache = cache;
            m_owner = owner;

            m_owner.RegisterService( typeof(Emulation.Hosting.MemoryProvider), this );
        }

        //
        // Helper Methods
        //

        public void Dispose()
        {
            m_owner.UnregisterService( this );
        }

        public bool CanAccess( uint address ,
                               uint size    )
        {
            address = NormalizeAddress( address );

            switch(size)
            {
                case 1: return CanAccessUInt8 ( address );
                case 2: return CanAccessUInt16( address );
                case 4: return CanAccessUInt32( address );
                case 8: return CanAccessUInt64( address );
            }

            return false;
        }

        //--//

        public bool GetUInt8(     uint address ,
                              out byte result  )
        {
            address = NormalizeAddress( address );

            try
            {
                result = ReadUInt8( address );

                return true;
            }
            catch(Exception)
            {
                result = 0;

                return false;
            }
        }

        public bool GetUInt16(     uint   address ,
                               out ushort result  )
        {
            address = NormalizeAddress( address );

            try
            {
                result = ReadUInt16( address );

                return true;
            }
            catch(Exception)
            {
                result = 0;

                return false;
            }
        }

        public bool GetUInt32(     uint address ,
                               out uint result  )
        {
            address = NormalizeAddress( address );

            try
            {
                result = ReadUInt32( address );

                return true;
            }
            catch(Exception)
            {
                result = 0;

                return false;
            }
        }

        public bool GetUInt64(     uint  address ,
                               out ulong result  )
        {
            address = NormalizeAddress( address );

            try
            {
                result = ReadUInt64( address );

                return true;
            }
            catch(Exception)
            {
                result = 0;

                return false;
            }
        }

        public bool GetBlock(     uint   address   ,
                                  int    size      ,
                                  uint   alignment ,
                              out byte[] result    )
        {
            address = NormalizeAddress( address );

            try
            {
                result = ReadBlock( address, size, alignment );

                return true;
            }
            catch(Exception)
            {
                result = null;

                return false;
            }
        }

        //--//

        public bool SetUInt8( uint address ,
                              byte result  )
        {
            address = NormalizeAddress( address );

            try
            {
                WriteUInt8( address, result );

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public bool SetUInt16( uint   address ,
                               ushort result  )
        {
            address = NormalizeAddress( address );

            try
            {
                WriteUInt16( address, result );

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public bool SetUInt32( uint address ,
                               uint result  )
        {
            address = NormalizeAddress( address );

            try
            {
                WriteUInt32( address, result );

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public bool SetUInt64( uint  address ,
                               ulong result  )
        {
            address = NormalizeAddress( address );

            try
            {
                WriteUInt64( address, result );

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public bool SetBlock( uint   address   ,
                              byte[] result    ,
                              uint   alignment )
        {
            address = NormalizeAddress( address );

            try
            {
                WriteBlock( address, result, alignment );

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        //--//

        protected virtual uint NormalizeAddress( uint address )
        {
            if(m_cache != null)
            {
                address = m_cache.GetUncacheableAddress( address );
            }

            return address;
        }

        protected abstract bool   CanAccessUInt8 ( uint address );
        protected abstract bool   CanAccessUInt16( uint address );
        protected abstract bool   CanAccessUInt32( uint address );
        protected abstract bool   CanAccessUInt64( uint address );

        protected abstract byte   ReadUInt8 ( uint address                           );
        protected abstract ushort ReadUInt16( uint address                           );
        protected abstract uint   ReadUInt32( uint address                           );
        protected abstract ulong  ReadUInt64( uint address                           );
        protected abstract byte[] ReadBlock ( uint address, int size, uint alignment );

        protected abstract void   WriteUInt8 ( uint address, byte   result                 );
        protected abstract void   WriteUInt16( uint address, ushort result                 );
        protected abstract void   WriteUInt32( uint address, uint   result                 );
        protected abstract void   WriteUInt64( uint address, ulong  result                 );
        protected abstract void   WriteBlock ( uint address, byte[] result, uint alignment );
    }
}
