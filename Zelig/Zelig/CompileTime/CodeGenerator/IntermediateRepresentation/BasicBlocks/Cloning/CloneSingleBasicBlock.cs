//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class CloneSingleBasicBlock : CloningContext
    {
        //
        // State
        //

        private readonly BasicBlock m_singleBasicBlockToClone;

        //
        // Constructor Methods
        //

        private CloneSingleBasicBlock( BasicBlock basicBlock ) : base( basicBlock.Owner, basicBlock.Owner, null )
        {
            m_singleBasicBlockToClone = basicBlock;
        }

        //--//

        public static BasicBlock Execute( BasicBlock block )
        {
            CloningContext context = new CloneSingleBasicBlock( block );

            return context.Clone( block );
        }

        //--//

        protected override bool FoundInCache(     object from ,
                                              out object to   )
        {
            if(base.FoundInCache( from, out to )) return true;

            if(from is BasicBlock && from != m_singleBasicBlockToClone)
            {
                to = from;
                return true;
            }

            if(from is Expression)
            {
                to = from;
                return true;
            }

            return false;
        }
    }
}
