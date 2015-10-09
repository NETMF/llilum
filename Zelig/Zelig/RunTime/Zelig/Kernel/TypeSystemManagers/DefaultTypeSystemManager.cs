//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class DefaultTypeSystemManager : TypeSystemManager
    {
        [NoInline]
        public override Object AllocateObject( TS.VTable vTable )
        {
            uint   size = ComputeObjectSize( vTable       );
            UIntPtr ptr = AllocateInner    ( vTable, size );

            if(MemoryManager.Configuration.TrashFreeMemory)
            {
                Memory.Zero( ptr, AddressMath.Increment( ptr, size ) ); 
            }

            return InitializeObject( ptr, vTable );
        }

        [NoInline]
        public override Object AllocateObjectWithExtensions( TS.VTable vTable )
        {
            uint   size = ComputeObjectSize( vTable       );
            UIntPtr ptr = AllocateInner    ( vTable, size );

            if(MemoryManager.Configuration.TrashFreeMemory)
            {
                Memory.Zero( ptr, AddressMath.Increment( ptr, size ) ); 
            }

            return InitializeObjectWithExtensions( ptr, vTable );
        }

        [NoInline]
        public override Array AllocateArray( TS.VTable vTable ,
                                             uint      length )
        {
            uint    size = ComputeArraySize( vTable, length );
            UIntPtr ptr  = AllocateInner   ( vTable, size   );

            if(MemoryManager.Configuration.TrashFreeMemory)
            {
                Memory.Zero( ptr, AddressMath.Increment( ptr, size ) ); 
            }

            return InitializeArray( ptr, vTable, length );
        }

        [NoInline]
        public override Array AllocateArrayNoClear( TS.VTable vTable ,
                                                    uint      length )
        {
            uint    size = ComputeArraySize( vTable, length );
            UIntPtr ptr  = AllocateInner   ( vTable, size   );

            return InitializeArray( ptr, vTable, length );
        }

        [NoInline]
        public override String AllocateString( TS.VTable vTable ,
                                               int       length )
        {
            uint    size = ComputeArraySize( vTable, (uint)length );
            UIntPtr ptr  = AllocateInner   ( vTable,       size   );

            if(MemoryManager.Configuration.TrashFreeMemory)
            {
                Memory.Zero( ptr, AddressMath.Increment( ptr, size ) ); 
            }

            return InitializeString( ptr, vTable, length );
        }

        //--//

        public static uint ComputeObjectSize( TS.VTable vTable )
        {
            uint size = (uint)System.Runtime.InteropServices.Marshal.SizeOf( typeof(ObjectHeader) );

            size += vTable.BaseSize;

            //
            // Align to word boundary.
            //
            size = (size + (sizeof(uint)-1)) & ~(uint)(sizeof(uint)-1);

            return size;
        }

        public static uint ComputeArraySize( TS.VTable vTable ,
                                             uint      length )
        {
            uint size = (uint)System.Runtime.InteropServices.Marshal.SizeOf( typeof(ObjectHeader) );

            size += vTable.BaseSize;
            size += vTable.ElementSize * length;

            return Microsoft.Zelig.AddressMath.AlignToWordBoundary( size );
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

            GarbageCollectionManager.Instance.NotifyNewObject( ptr, size );

            return ptr;
        }
    }
}
