//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.GC), NoConstructors=true)]
    public class GCImpl
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        //--//

        //
        // Helper Methods
        //

        [NoInline]
        public static void KeepAlive( Object obj )
        {
            //
            // This is just a method that cannot be inlined,
            // so you can guarantee that your reference is kept alive up to this method call.
            //
        }

        [NoInline]
        public static void Collect()
        {
            GarbageCollectionManager.Instance.Collect();
        }

        [NoInline]
        public static long GetTotalMemory( bool forceFullCollection )
        {
            if(forceFullCollection)
            {
                Collect();
            }

            return GarbageCollectionManager.Instance.GetTotalMemory();
        }

        [NoInline]
        public static int CollectionCount( int generation )
        {
            // BUGBUG: Add implementation!!!
            return 0;
        }

        public static void WaitForPendingFinalizers()
        {
            Finalizer.WaitForPendingFinalizers();
        }

        public static void SuppressFinalize( Object obj )
        {
            Finalizer.SuppressFinalize( obj );
        }

        public static void ReRegisterForFinalize( Object obj )
        {
            Finalizer.ReRegisterForFinalize( obj );
        }

        //
        // Access Methods
        //

    }
}
