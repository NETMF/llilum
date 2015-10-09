//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.WeakReference))]
    public class WeakReferenceImpl
    {
        [TS.GarbageCollectionExtension(typeof(WeakReferenceImpl))]
        class Handler : GarbageCollectionExtensionHandler
        {
            [TS.SkipDuringGarbageCollection] WeakReferenceImpl m_head;

            public override void StartOfMarkPhase( GarbageCollectionManager gc )
            {
                m_head = null;
            }

            public override void Mark( GarbageCollectionManager gc     ,
                                       object                   target )
            {
                var obj = (WeakReferenceImpl)target;

                if(gc.IsMarked( obj.Target ))
                {
                    return;
                }

                obj.m_next = m_head;

                m_head = obj;
            }

            public override void EndOfMarkPhase( GarbageCollectionManager gc )
            {
            }

            public override void StartOfSweepPhase( GarbageCollectionManager gc )
            {
                for(var ptr = m_head; ptr != null; ptr = ptr.m_next)
                {
                    if(!gc.IsMarked( ptr.m_target ))
                    {
                        ptr.m_target = null;
                    }
                }
            }

            public override void Sweep( GarbageCollectionManager gc     ,
                                        object                   target )
            {
            }

            public override void EndOfSweepPhase( GarbageCollectionManager gc )
            {
            }
        }

        //
        // State
        //

        [TS.SkipDuringGarbageCollection] object            m_target;
        [TS.SkipDuringGarbageCollection] WeakReferenceImpl m_next;

        //
        // Constructor Methods
        //

        [DiscardTargetImplementation]
        public WeakReferenceImpl( Object target            ,
                                  bool   trackResurrection )
        {
            m_target = target;
        }

        //
        // Access Methods
        //

        //Determines whether or not this instance of WeakReference still refers to an object
        //that has not been collected.
        //
        public virtual bool IsAlive
        {
            get
            {
                return m_target != null;
            }
        }
    
        //Gets the Object stored in the handle if it's accessible.
        // Or sets it.
        //
        public virtual Object Target
        {
            get
            {
                return m_target;
            }

            set
            {
                m_target = value;
            }
        }
    }
}
