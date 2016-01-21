//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    [TS.DisableAutomaticReferenceCounting]
    public abstract class DefaultTypeSystemManager : TypeSystemManager
    {
        [NoInline]
        public override Object AllocateObject( TS.VTable vTable )
        {
            uint size = ComputeObjectSize( vTable );
            UIntPtr ptr;
            object obj;

            using(SmartHandles.YieldLockHolder hnd = new SmartHandles.YieldLockHolder( MemoryManager.Lock ))
            {
                ptr = AllocateInner( vTable, size );
                obj = InitializeObject( ptr, vTable, referenceCounting: false );
            }

            if(MemoryManager.Configuration.TrashFreeMemory)
            {
                Memory.Zero( ( (ObjectImpl)obj ).GetFieldPointer( ), AddressMath.Increment( ptr, size ) );
            }

            return obj;
        }

        [NoInline]
        public override Object AllocateReferenceCountingObject( TS.VTable vTable )
        {
            uint size = ComputeObjectSize( vTable );
            UIntPtr ptr;
            object obj;

            using(SmartHandles.YieldLockHolder hnd = new SmartHandles.YieldLockHolder( MemoryManager.Lock ))
            {
                ptr = AllocateInner( vTable, size );
                obj = InitializeObject( ptr, vTable, referenceCounting: true );
            }

            if(MemoryManager.Configuration.TrashFreeMemory)
            {
                Memory.Zero( ( (ObjectImpl)obj ).GetFieldPointer( ), AddressMath.Increment( ptr, size ) );
            }

            return obj;
        }

        [NoInline]
        public override Object AllocateObjectWithExtensions( TS.VTable vTable )
        {
            uint size = ComputeObjectSize( vTable );
            UIntPtr ptr;
            object obj;

            using(SmartHandles.YieldLockHolder hnd = new SmartHandles.YieldLockHolder( MemoryManager.Lock ))
            {
                ptr = AllocateInner( vTable, size );
                obj = InitializeObjectWithExtensions( ptr, vTable );
            }

            if(MemoryManager.Configuration.TrashFreeMemory)
            {
                Memory.Zero( ( (ObjectImpl)obj ).GetFieldPointer( ), AddressMath.Increment( ptr, size ) );
            }

            return obj;
        }

        [NoInline]
        public override Array AllocateArray( TS.VTable vTable ,
                                             uint      length )
        {
            uint size = ComputeArraySize( vTable, length );
            UIntPtr ptr;
            Array array;

            using(SmartHandles.YieldLockHolder hnd = new SmartHandles.YieldLockHolder( MemoryManager.Lock ))
            {
                ptr = AllocateInner( vTable, size );
                array = InitializeArray( ptr, vTable, length, referenceCounting: false );
            }

            if(MemoryManager.Configuration.TrashFreeMemory)
            {
                unsafe
                {
                    Memory.Zero(
                        (UIntPtr)ArrayImpl.CastAsArray( array ).GetDataPointer( ),
                        AddressMath.Increment( ptr, size ) );
                }
            }

            return array;
        }

        [NoInline]
        public override Array AllocateReferenceCountingArray( TS.VTable vTable,
                                                              uint      length )
        {
            uint size = ComputeArraySize( vTable, length );
            UIntPtr ptr;
            Array array;

            using(SmartHandles.YieldLockHolder hnd = new SmartHandles.YieldLockHolder( MemoryManager.Lock ))
            {
                ptr = AllocateInner( vTable, size );
                array = InitializeArray( ptr, vTable, length, referenceCounting: true );
            }

            if(MemoryManager.Configuration.TrashFreeMemory)
            {
                unsafe
                {
                    Memory.Zero(
                        (UIntPtr)ArrayImpl.CastAsArray( array ).GetDataPointer( ),
                        AddressMath.Increment( ptr, size ) );
                }
            }

            return array;
        }

        [NoInline]
        public override Array AllocateArrayNoClear( TS.VTable vTable ,
                                                    uint      length )
        {
            uint size = ComputeArraySize( vTable, length );
            UIntPtr ptr;
            Array array;

            using(SmartHandles.YieldLockHolder hnd = new SmartHandles.YieldLockHolder( MemoryManager.Lock ))
            {
                ptr = AllocateInner( vTable, size );
                array = InitializeArray( ptr, vTable, length, referenceCounting: false );
            }

            return array;
        }

        [NoInline]
        public override String AllocateString( TS.VTable vTable ,
                                               int       length )
        {
            uint size = ComputeArraySize( vTable, (uint)length );
            UIntPtr ptr;
            String str;

            using(SmartHandles.YieldLockHolder hnd = new SmartHandles.YieldLockHolder( MemoryManager.Lock ))
            {
                ptr = AllocateInner( vTable, size );
                str = InitializeString( ptr, vTable, length, referenceCounting: false );
            }

            if(MemoryManager.Configuration.TrashFreeMemory)
            {
                unsafe
                {
                    Memory.Zero(
                        (UIntPtr)StringImpl.CastAsString( str ).GetDataPointer( ),
                        AddressMath.Increment( ptr, size ) );
                }
            }

            return str;
        }

        [NoInline]
        public override String AllocateReferenceCountingString( TS.VTable vTable ,
                                                                int       length )
        {
            uint size = ComputeArraySize( vTable, (uint)length );
            UIntPtr ptr;
            String str;

            using(SmartHandles.YieldLockHolder hnd = new SmartHandles.YieldLockHolder( MemoryManager.Lock ))
            {
                ptr = AllocateInner( vTable, size );
                str = InitializeString( ptr, vTable, length, referenceCounting: true );
            }

            if(MemoryManager.Configuration.TrashFreeMemory)
            {
                unsafe
                {
                    Memory.Zero(
                        (UIntPtr)StringImpl.CastAsString( str ).GetDataPointer( ),
                        AddressMath.Increment( ptr, size ) );
                }
            }

            return str;
        }

        //--//

        public static uint ComputeObjectSize(TS.VTable vTable)
        {
            return ObjectHeader.HeaderSize + ObjectHeader.ComputeObjectSize(vTable, arrayLength: 0);
        }

        public static uint ComputeArraySize(TS.VTable vTable, uint length)
        {
            return ObjectHeader.HeaderSize + ObjectHeader.ComputeObjectSize(vTable, length);
        }

        //--//

        [TS.WellKnownMethod("DebugGC_DefaultTypeSystemManager_AllocateInner")]
        private UIntPtr AllocateInner( TS.VTable vTable ,
                                       uint      size   )
        {
            UIntPtr ptr = MemoryManager.Instance.Allocate( size );

            if(ptr == UIntPtr.Zero)
            {
                GarbageCollectionManager.Instance.Collect();

                ptr = MemoryManager.Instance.Allocate( size );
                if(ptr == UIntPtr.Zero)
                {
                    GarbageCollectionManager.Instance.ThrowOutOfMemory( vTable );
                }
            }

            return ptr;
        }
    }
}
