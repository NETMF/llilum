//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class GarbageCollectionExtensionHandler
    {
        //
        // Helper Methods.
        //

        public virtual void Initialize()
        {
        }

        public abstract void StartOfMarkPhase( GarbageCollectionManager gc );

        public abstract void Mark( GarbageCollectionManager gc     ,
                                   object                   target );

        public abstract void EndOfMarkPhase( GarbageCollectionManager gc );

        public abstract void StartOfSweepPhase( GarbageCollectionManager gc );

        public abstract void Sweep( GarbageCollectionManager gc     ,
                                    object                   target );

        public abstract void EndOfSweepPhase( GarbageCollectionManager gc );

        public virtual void RestartExecution()
        {
        }

        //
        // Access Methods
        //
    }
}
