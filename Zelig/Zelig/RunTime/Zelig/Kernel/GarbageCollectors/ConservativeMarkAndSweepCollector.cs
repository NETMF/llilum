//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;


    public abstract class ConservativeMarkAndSweepCollector : MarkAndSweepCollector
    {
        class ConservativeStackWalker : MarkAndSweepStackWalker
        {
            ConservativeMarkAndSweepCollector m_owner;

            internal ConservativeStackWalker( ConservativeMarkAndSweepCollector owner )
            {
                m_owner = owner;
            }

            public unsafe void Process( Processor.Context ctx )
            {
                // Go through each entry in the stack
                UIntPtr* end = (UIntPtr*)ctx.BaseStackPointer.ToUInt32( );
                UIntPtr* current = (UIntPtr*)ctx.StackPointer.ToUInt32( );

                while(current < end)
                {
                    m_owner.VisitInternalPointer( *current );
                    current++;
                }
            }
        }

        //--//

        protected override int MarkStackForObjectsSize
        {
            get { return 256; }
        }
        protected override int MarkStackForArraysSize
        {
            get { return 32; }
        }

        //--//
        protected override MarkAndSweepStackWalker CreateStackWalker( )
        {
            return new ConservativeStackWalker( this );
        }

        //--//

        protected override bool IsThisAGoodPlaceToStopTheWorld( )
        {
            return true;
        }
    }
}
