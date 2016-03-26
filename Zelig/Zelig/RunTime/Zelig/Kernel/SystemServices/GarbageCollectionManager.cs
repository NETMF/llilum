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
    [TS.WellKnownType("Microsoft_Zelig_Runtime_GarbageCollectionManager")]
    public abstract class GarbageCollectionManager
    {
        public static class Configuration
        {
            public static bool CollectMinimalPerformanceStatistics
            {
                [ConfigurationOption("GarbageCollectionManager__CollectMinimalPerformanceStatistics")]
                get
                {
                    return false;
                }
            }

            public static bool CollectPerformanceStatistics
            {
                [ConfigurationOption("GarbageCollectionManager__CollectPerformanceStatistics")]
                get
                {
                    return false;
                }
            }

            public static bool TraceFreeBlocks
            {
                [ConfigurationOption("GarbageCollectionManager__TraceFreeBlocks")]
                get
                {
                    return false;
                }
            }

            public static bool ValidateHeap
            {
                [ConfigurationOption("GarbageCollectionManager__ValidateHeap")]
                get
                {
                    return false;
                }
            }
        }

        class EmptyManager : GarbageCollectionManager
        {
            public override void InitializeGarbageCollectionManager()
            {
            }

            public override void NotifyNewObject( UIntPtr ptr  ,
                                                  uint    size )
            {
            }

            public override UIntPtr FindObject( UIntPtr interiorPtr )
            {
                return UIntPtr.Zero;
            }

            public override uint Collect()
            {
                return 0;
            }

            public override long GetTotalMemory()
            {
                return 0;
            }

            public override void ThrowOutOfMemory( TS.VTable vTable )
            {
                BugCheck.Raise( BugCheck.StopCode.NoMemory );
            }

            public override bool IsMarked( object target )
            {
                return true;
            }

            public override void ExtendMarking( object target )
            {
            }
        }

        //
        // State
        //

#pragma warning disable 649 // These fields are populated at code generation.
        [TS.WellKnownField( "GarbageCollectionManager_m_extensionTargets"  )] private readonly TS.VTable[]                         m_extensionTargets;
        [TS.WellKnownField( "GarbageCollectionManager_m_extensionHandlers" )] private readonly GarbageCollectionExtensionHandler[] m_extensionHandlers;
#pragma warning restore 649

        //
        // Helper Methods
        //

        public abstract void InitializeGarbageCollectionManager();

        public abstract void NotifyNewObject( UIntPtr ptr  ,
                                              uint    size );

        public abstract UIntPtr FindObject( UIntPtr interiorPtr );

        public abstract uint Collect();

        public abstract long GetTotalMemory();

        public abstract void ThrowOutOfMemory( TS.VTable vTable );

        //--//

        public abstract bool IsMarked     ( object target );

        public abstract void ExtendMarking( object target );
        
        [Inline]
        public GarbageCollectionExtensionHandler FindExtensionHandler( TS.VTable vTable )
        {
            for(int i = 0; i < m_extensionTargets.Length; i++)
            {
                if(m_extensionTargets[i] == vTable)
                {
                    return m_extensionHandlers[i];
                }
            }

            return null;
        }

        //
        // Access Methods
        //

        public static extern GarbageCollectionManager Instance
        {
            [SingletonFactory(Fallback=typeof(EmptyManager))]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }

        public GarbageCollectionExtensionHandler[] ExtensionHandlers
        {
            get
            {
                return m_extensionHandlers;
            }
        }
    }
}
