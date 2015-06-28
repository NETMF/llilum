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


    public sealed class CompoundValueHandle : AbstractValueHandle
    {
        public struct Fragment
        {
            //
            // State
            //

            public AbstractValueHandle Component;
            public int                 Offset;

            //
            // Contructor Methods
            //

            public Fragment( AbstractValueHandle component ,
                             int                 offset    )
            {
                this.Component = component;
                this.Offset    = offset;
            }

            //
            // Helper Methods
            //

            internal bool IsEquivalent( ref Fragment other )
            {
                if(this.Offset == other.Offset)
                {
                    if(this.Component.IsEquivalent( other.Component ))
                    {
                        return true;
                    }
                }

                return false;
            }

            internal bool Read(     Emulation.Hosting.BinaryBlob bb       ,
                                    int                          offset   ,
                                    int                          count    ,
                                out bool                         fChanged )
            {
                if(this.Component == null)
                {
                    fChanged = false;
                    return false;
                }

                int start = Math.Max( offset        , this.Offset                       ); 
                int end   = Math.Min( offset + count, this.Offset + this.Component.Size );

                if(start >= end)
                {
                    //
                    // Non-overlapping regions.
                    //
                    fChanged = false;
                    return true;
                }

                var bbSub = this.Component.Read( start - this.Offset, end - start, out fChanged );
 
                if(bbSub != null)
                {
                    bb.Insert( bbSub, start - offset );
 
                    return true;
                }

                return false;
            }

            internal bool Write( Emulation.Hosting.BinaryBlob bb     ,
                                 int                          offset ,
                                 int                          count  )
            {
                if(this.Component == null)
                {
                    return false;
                }

                int start = Math.Max( offset        , this.Offset                       ); 
                int end   = Math.Min( offset + count, this.Offset + this.Component.Size );

                if(start >= end)
                {
                    //
                    // Non-overlapping regions.
                    //
                    return true;
                }

                var bbSub = bb.Extract( start - offset, end - start );
 
                return this.Component.Write( bbSub, start - this.Offset, end - start );
            }
        }

        //
        // State
        //

        public readonly Fragment[] Fragments;

        //
        // Contructor Methods
        //

        public CompoundValueHandle(        TS.TypeRepresentation type               ,
                                           bool                  fAsHoldingVariable ,
                                    params Fragment[]            fragments          ) : base( type, null, null, fAsHoldingVariable )
        {
            this.Fragments = fragments;
        }

        //
        // Helper Methods
        //

        public override bool IsEquivalent( AbstractValueHandle abstractValueHandle )
        {
            var other = abstractValueHandle as CompoundValueHandle;

            if(other != null)
            {
                if(this.Fragments.Length == other.Fragments.Length)
                {
                    for(int i = 0; i < this.Fragments.Length; i++)
                    {
                        if(this.Fragments[i].IsEquivalent( ref other.Fragments[i] ) == false)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public override Emulation.Hosting.BinaryBlob Read(     int  offset   ,
                                                               int  count    ,
                                                           out bool fChanged )
        {
            var bb = new Emulation.Hosting.BinaryBlob( count );

            fChanged = false;

            foreach(var fragment in this.Fragments)
            {
                bool fChangedFragment;

                if(fragment.Read( bb, offset, count, out fChangedFragment ) == false)
                {
                    return null;
                }

                fChanged |= fChangedFragment;
            }

            return bb;
        }

        public override bool Write( Emulation.Hosting.BinaryBlob bb     ,
                                    int                          offset ,
                                    int                          count  )
        {
            foreach(var fragment in this.Fragments)
            {
                if(fragment.Write( bb, offset, count ) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public override AbstractValueHandle AccessField( TS.InstanceFieldRepresentation   fd                       ,
                                                         TS.CustomAttributeRepresentation caMemoryMappedPeripheral ,
                                                         TS.CustomAttributeRepresentation caMemoryMappedRegister   )
        {
            int offset = fd.Offset;
            int size   = (int)fd.FieldType.SizeOfHoldingVariable;

            foreach(var fragment in this.Fragments)
            {
                if(fragment.Offset == offset)
                {
                    var subLoc = fragment.Component;
                
                    if(subLoc != null && subLoc.Size == size)
                    {
                        return subLoc;
                    }
                }
            }

            return base.AccessField( fd, caMemoryMappedPeripheral, caMemoryMappedRegister );
        }

        //
        // Access Methods
        //

        public override bool CanUpdate
        {
            get
            {
                foreach(var fragment in this.Fragments)
                {
                    if(fragment.Component == null)
                    {
                        return false;
                    }

                    if(fragment.Component.CanUpdate == false)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public override bool HasAddress
        {
            get
            {
                return false;
            }
        }

        public override uint Address
        {
            get
            {
                return 0;
            }
        }
    }
}