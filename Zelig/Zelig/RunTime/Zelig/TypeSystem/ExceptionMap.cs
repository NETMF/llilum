//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [TS.AllowCompileTimeIntrospection]
    public class ExceptionMap
    {
        [TS.AllowCompileTimeIntrospection]
        public struct Handler
        {
            public static readonly Handler[] SharedEmptyArray = new Handler[0];

            //
            // State
            //

            public VTable      Filter;
            public CodePointer HandlerCode;

            //
            // Equality Methods
            //

            public bool SameContents( ref Handler other )
            {
                if(this.Filter == other.Filter)
                {
                    return this.HandlerCode.SameContents( ref other.HandlerCode );
                }

                return false;
            }
        }

        [TS.AllowCompileTimeIntrospection]
        public struct Range
        {
            public static readonly Range[] SharedEmptyArray = new Range[0];

            //
            // State
            //

            public UIntPtr   Start;
            public UIntPtr   End;
            public Handler[] Handlers;

            //
            // Equality Methods
            //

            public bool SameContents( ref Range other )
            {
                if(this.Start == other.Start &&
                   this.End   == other.End    )
                {
                    Handler[] thisHandlers  = this .Handlers;
                    Handler[] otherHandlers = other.Handlers;
                    int       thisLen       = thisHandlers  != null ? thisHandlers .Length : 0;
                    int       otherLen      = otherHandlers != null ? otherHandlers.Length : 0;

                    if(thisLen == otherLen)
                    {
                        for(int i = 0; i < thisLen; i++)
                        {
                            if(thisHandlers[i].SameContents( ref otherHandlers[i] ) == false)
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                }

                return false;
            }

            public bool Contains( UIntPtr address )
            {
                return AddressMath.IsInRange( address, this.Start, this.End );
            }

            public CodePointer Match( UIntPtr address   ,
                                      VTable  exception )
            {
                if(Contains( address ))
                {
                    Handler[] handlers = this.Handlers;

                    for(int i = 0; i < handlers.Length; i++)
                    {
                        VTable filter = handlers[i].Filter;

                        if(filter == null || filter.CanBeAssignedFrom( exception ))
                        {
                            return handlers[i].HandlerCode;
                        }
                    }
                }

                return new CodePointer();
            }
        }

        //--//

        //
        // State
        //

        [WellKnownField( "ExceptionMap_Ranges" )] public Range[] Ranges;

        //
        // Equality Methods
        //

        public static bool SameContents( ExceptionMap left  ,
                                         ExceptionMap right )
        {
            if(Object.ReferenceEquals( left, right ))
            {
                return true;
            }

            if(left != null && right != null)
            {
                return left.SameContents( right );
            }

            return false;
        }

        public bool SameContents( ExceptionMap other )
        {
            Range[] thisRanges  = this .Ranges;
            Range[] otherRanges = other.Ranges;
            int     thisLen     = thisRanges  != null ? thisRanges .Length : 0;
            int     otherLen    = otherRanges != null ? otherRanges.Length : 0;

            if(thisLen == otherLen)
            {
                for(int i = 0; i < thisLen; i++)
                {
                    if(thisRanges[i].SameContents( ref otherRanges[i] ) == false)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        [NoInline]
        public CodePointer ResolveAddressToHandler( UIntPtr address   ,
                                                    VTable  exception )
        {
            Range[] ranges = this.Ranges;

            if(ranges != null)
            {
                for(int i = 0; i < ranges.Length; i++)
                {
                    CodePointer res = ranges[i].Match( address, exception );

                    if(res.IsValid)
                    {
                        return res;
                    }
                }
            }

            return new CodePointer();
        }
    }
}
