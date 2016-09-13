//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define LLVM

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.Buffer), NoConstructors=true, ProcessAfter=typeof(ArrayImpl))]
    public static class BufferImpl
    {
        //
        // Helper Methods
        //

        public unsafe static void BlockCopy( ArrayImpl src       ,
                                             int       srcOffset ,
                                             ArrayImpl dst       ,
                                             int       dstOffset ,
                                             int       count     )
        {
            TS.VTable vtSrc = TS.VTable.Get( src );
            TS.VTable vtDst = TS.VTable.Get( dst );

            if((vtSrc.TypeInfo.ContainedType is TS.ScalarTypeRepresentation) == false)
            {
#if EXCEPTION_STRINGS
                throw new NotSupportedException( "Not a scalar" );
#else
                throw new NotSupportedException();
#endif
            }

            if((vtDst.TypeInfo.ContainedType is TS.ScalarTypeRepresentation) == false)
            {
#if EXCEPTION_STRINGS
                throw new NotSupportedException( "Not a scalar" );
#else
                throw new NotSupportedException();
#endif
            }

            if(count < 0)
            {
#if EXCEPTION_STRINGS
                throw new IndexOutOfRangeException( "count" );
#else
                throw new IndexOutOfRangeException();
#endif
            }

            if(srcOffset < 0 || (uint)(srcOffset + count) > src.Size)
            {
#if EXCEPTION_STRINGS
                throw new IndexOutOfRangeException( "src" );
#else
                throw new IndexOutOfRangeException();
#endif
            }

            if(dstOffset < 0 || (uint)(dstOffset + count) > dst.Size)
            {
#if EXCEPTION_STRINGS
                throw new IndexOutOfRangeException( "dst" );
#else
                throw new IndexOutOfRangeException();
#endif
            }

            byte* srcPtr = (byte*)src.GetDataPointer();
            byte* dstPtr = (byte*)dst.GetDataPointer();

            InternalMemoryMove( &srcPtr[srcOffset], &dstPtr[dstOffset], count );
        }

        internal unsafe static void InternalBlockCopy( ArrayImpl src       ,
                                                       int       srcOffset ,
                                                       ArrayImpl dst       ,
                                                       int       dstOffset ,
                                                       int       count     )
        {
            BugCheck.Assert( TS.VTable.Get( src ) == TS.VTable.Get( dst ), BugCheck.StopCode.IncorrectArgument );

            if(count < 0)
            {
#if EXCEPTION_STRINGS
                throw new IndexOutOfRangeException( "count" );
#else
                throw new IndexOutOfRangeException();
#endif
            }

            if(srcOffset < 0 || (uint)(srcOffset + count) > src.Size)
            {
#if EXCEPTION_STRINGS
                throw new IndexOutOfRangeException( "src" );
#else
                throw new IndexOutOfRangeException();
#endif
            }

            if(dstOffset < 0 || (uint)(dstOffset + count) > dst.Size)
            {
#if EXCEPTION_STRINGS
                throw new IndexOutOfRangeException( "dst" );
#else
                throw new IndexOutOfRangeException();
#endif
            }

            byte* srcPtr = (byte*)src.GetDataPointer();
            byte* dstPtr = (byte*)dst.GetDataPointer();

            InternalMemoryMove( &srcPtr[srcOffset], &dstPtr[dstOffset], count );
        }

        //--//--//

        [DisableNullChecks]
#if LLVM
        [NoInline] // Disable inlining so we always have a chance to replace the method.
#endif // LLVM
        [TS.WellKnownMethod("System_Buffer_InternalMemoryCopy")]
        internal unsafe static void InternalMemoryCopy( byte* src   ,
                                                        byte* dst   ,
                                                        int   count )
        {
            BugCheck.Assert( count >= 0, BugCheck.StopCode.NegativeIndex );

            if(AddressMath.IsAlignedTo32bits( src ) &&
               AddressMath.IsAlignedTo32bits( dst )  )
            {
                int count32 = count / 4;

                InternalMemoryCopy( (uint*)src, (uint*)dst, count32 );

                int count2 = count32 * 4;

                src   += count2;
                dst   += count2;
                count -= count2;
            }
            else if(AddressMath.IsAlignedTo16bits( src ) &&
                    AddressMath.IsAlignedTo16bits( dst )  )
            {
                int count16 = count / 2;

                InternalMemoryCopy( (ushort*)src, (ushort*)dst, count16 );

                int count2 = count16 * 2;

                src   += count2;
                dst   += count2;
                count -= count2;
            }

            if(count > 0)
            {
                while(count >= 4)
                {
                    var v0 = src[0];
                    var v1 = src[1];
                    var v2 = src[2];
                    var v3 = src[3];

                    dst[0] = v0;
                    dst[1] = v1;
                    dst[2] = v2;
                    dst[3] = v3;

                    dst   += 4;
                    src   += 4;
                    count -= 4;
                }

                if((count & 2) != 0)
                {
                    var v0 = src[0];
                    var v1 = src[1];

                    dst[0] = v0;
                    dst[1] = v1;

                    dst += 2;
                    src += 2;
                }

                if((count & 1) != 0)
                {
                    dst[0] = src[0];
                }
            }
        }

        [Inline]
        internal unsafe static void InternalMemoryCopy( sbyte* src   ,
                                                        sbyte* dst   ,
                                                        int    count )
        {
            InternalMemoryCopy( (byte*)src, (byte*)dst, count );
        }

        //--//

        [DisableNullChecks]
        internal unsafe static void InternalMemoryCopy( ushort* src   ,
                                                        ushort* dst   ,
                                                        int     count )
        {
#if LLVM
            InternalMemoryCopy((byte*)src, (byte*)dst, count * sizeof(ushort));
#else // LLVM
            BugCheck.Assert( count >= 0, BugCheck.StopCode.NegativeIndex );

            if(AddressMath.IsAlignedTo32bits( src ) &&
               AddressMath.IsAlignedTo32bits( dst )  )
            {
                int count32 = count / 2;

                InternalMemoryCopy( (uint*)src, (uint*)dst, count32 );

                int count2 = count32 * 2;

                src   += count2;
                dst   += count2;
                count -= count2;
            }

            if(count > 0)
            {
                while(count >= 4)
                {
                    var v0 = src[0];
                    var v1 = src[1];
                    var v2 = src[2];
                    var v3 = src[3];

                    dst[0] = v0;
                    dst[1] = v1;
                    dst[2] = v2;
                    dst[3] = v3;

                    dst   += 4;
                    src   += 4;
                    count -= 4;
                }

                if((count & 2) != 0)
                {
                    var v0 = src[0];
                    var v1 = src[1];

                    dst[0] = v0;
                    dst[1] = v1;

                    dst += 2;
                    src += 2;
                }

                if((count & 1) != 0)
                {
                    dst[0] = src[0];
                }
            }
#endif // LLVM
        }

        [Inline]
        internal unsafe static void InternalMemoryCopy( short* src   ,
                                                        short* dst   ,
                                                        int    count )
        {
            InternalMemoryCopy( (ushort*)src, (ushort*)dst, count );
        }

        [Inline]
        internal unsafe static void InternalMemoryCopy( char* src   ,
                                                        char* dst   ,
                                                        int   count )
        {
            InternalMemoryCopy( (ushort*)src, (ushort*)dst, count );
        }

        //--//

        [DisableNullChecks]
        internal unsafe static void InternalMemoryCopy( uint* src   ,
                                                        uint* dst   ,
                                                        int   count )
        {
#if LLVM
            InternalMemoryCopy((byte*)src, (byte*)dst, count * sizeof(uint));
#else // LLVM
            BugCheck.Assert( count >= 0, BugCheck.StopCode.NegativeIndex );

            if(count > 0)
            {
                while(count >= 4)
                {
                    var v0 = src[0];
                    var v1 = src[1];
                    var v2 = src[2];
                    var v3 = src[3];

                    dst[0] = v0;
                    dst[1] = v1;
                    dst[2] = v2;
                    dst[3] = v3;

                    dst   += 4;
                    src   += 4;
                    count -= 4;
                }

                if((count & 2) != 0)
                {
                    var v0 = src[0];
                    var v1 = src[1];

                    dst[0] = v0;
                    dst[1] = v1;

                    dst += 2;
                    src += 2;
                }

                if((count & 1) != 0)
                {
                    dst[0] = src[0];
                }
            }
#endif // LLVM
        }

        [Inline]
        internal unsafe static void InternalMemoryCopy( int* src   ,
                                                        int* dst   ,
                                                        int  count )
        {
            InternalMemoryCopy( (uint*)src, (uint*)dst, count );
        }

        //--//--//

        [DisableNullChecks]
#if LLVM
        [NoInline] // Disable inlining so we always have a chance to replace the method.
#endif // LLVM
        [TS.WellKnownMethod( "System_Buffer_InternalBackwardMemoryCopy" )]
        internal unsafe static void InternalBackwardMemoryCopy( byte* src   ,
                                                                byte* dst   ,
                                                                int   count )
        {
            BugCheck.Assert( count >= 0, BugCheck.StopCode.NegativeIndex );

            src += count;
            dst += count;

            if(AddressMath.IsAlignedTo32bits( src ) &&
               AddressMath.IsAlignedTo32bits( dst )  )
            {
                uint* src2    = (uint*)src;
                uint* dst2    = (uint*)dst;
                int   count32 =        count / 4;

                src2 -= count32;
                dst2 -= count32;

                InternalBackwardMemoryCopy( src2, dst2, count32 );

                int count2 = count32 * 4;

                src    = (byte*)src2;
                dst    = (byte*)dst2;
                count -= count2;
            }
            else if(AddressMath.IsAlignedTo16bits( src ) &&
                    AddressMath.IsAlignedTo16bits( dst )  )
            {
                ushort* src2    = (ushort*)src;
                ushort* dst2    = (ushort*)dst;
                int     count16 =          count / 2;

                src2 -= count16;
                dst2 -= count16;

                InternalBackwardMemoryCopy( src2, dst2, count16 );

                int count2 = count16 * 2;

                src    = (byte*)src2;
                dst    = (byte*)dst2;
                count -= count2;
            }

            if(count > 0)
            {
                while(count >= 4)
                {
                    dst   -= 4;
                    src   -= 4;
                    count -= 4;

                    var v0 = src[0];
                    var v1 = src[1];
                    var v2 = src[2];
                    var v3 = src[3];

                    dst[0] = v0;
                    dst[1] = v1;
                    dst[2] = v2;
                    dst[3] = v3;
                }

                if((count & 2) != 0)
                {
                    dst -= 2;
                    src -= 2;

                    var v0 = src[0];
                    var v1 = src[1];

                    dst[0] = v0;
                    dst[1] = v1;
                }

                if((count & 1) != 0)
                {
                    dst -= 1;
                    src -= 1;

                    dst[0] = src[0];
                }
            }
        }

        [Inline]
        internal unsafe static void InternalBackwardMemoryCopy( sbyte* src   ,
                                                                sbyte* dst   ,
                                                                int    count )
        {
            InternalBackwardMemoryCopy( (byte*)src, (byte*)dst, count );
        }

        //--//

        [DisableNullChecks]
        internal unsafe static void InternalBackwardMemoryCopy( ushort* src   ,
                                                                ushort* dst   ,
                                                                int     count )
        {
#if LLVM
            InternalBackwardMemoryCopy( (byte*)src, (byte*)dst, count * sizeof( ushort ) );
#else // LLVM
            BugCheck.Assert( count >= 0, BugCheck.StopCode.NegativeIndex );

            src += count;
            dst += count;

            if(AddressMath.IsAlignedTo32bits( src ) &&
               AddressMath.IsAlignedTo32bits( dst )  )
            {
                uint* src2    = (uint*)src;
                uint* dst2    = (uint*)dst;
                int   count32 =        count / 2;

                src2 -= count32;
                dst2 -= count32;

                InternalBackwardMemoryCopy( src2, dst2, count32 );

                int count2 = count32 * 2;

                src    = (ushort*)src2;
                dst    = (ushort*)dst2;
                count -= count2;
            }

            if(count > 0)
            {
                while(count >= 4)
                {
                    dst   -= 4;
                    src   -= 4;
                    count -= 4;

                    var v0 = src[0];
                    var v1 = src[1];
                    var v2 = src[2];
                    var v3 = src[3];

                    dst[0] = v0;
                    dst[1] = v1;
                    dst[2] = v2;
                    dst[3] = v3;
                }

                if((count & 2) != 0)
                {
                    dst -= 2;
                    src -= 2;

                    var v0 = src[0];
                    var v1 = src[1];

                    dst[0] = v0;
                    dst[1] = v1;
                }

                if((count & 1) != 0)
                {
                    dst -= 1;
                    src -= 1;

                    dst[0] = src[0];
                }
            }
#endif // LLVM
        }

        [Inline]
        internal unsafe static void InternalBackwardMemoryCopy( short* src   ,
                                                                short* dst   ,
                                                                int    count )
        {
            InternalBackwardMemoryCopy( (ushort*)src, (ushort*)dst, count );
        }

        [Inline]
        internal unsafe static void InternalBackwardMemoryCopy( char* src   ,
                                                                char* dst   ,
                                                                int   count )
        {
            InternalBackwardMemoryCopy( (ushort*)src, (ushort*)dst, count );
        }

        //--//

        [DisableNullChecks]
        internal unsafe static void InternalBackwardMemoryCopy( uint* src   ,
                                                                uint* dst   ,
                                                                int   count )
        {
#if LLVM
            InternalBackwardMemoryCopy( (byte*)src, (byte*)dst, count * sizeof( uint ) );
#else // LLVM
            BugCheck.Assert( count >= 0, BugCheck.StopCode.NegativeIndex );

            if(count > 0)
            {
                src += count;
                dst += count;

                while(count >= 4)
                {
                    dst   -= 4;
                    src   -= 4;
                    count -= 4;

                    var v0 = src[0];
                    var v1 = src[1];
                    var v2 = src[2];
                    var v3 = src[3];

                    dst[0] = v0;
                    dst[1] = v1;
                    dst[2] = v2;
                    dst[3] = v3;
                }

                if((count & 2) != 0)
                {
                    dst -= 2;
                    src -= 2;

                    var v0 = src[0];
                    var v1 = src[1];

                    dst[0] = v0;
                    dst[1] = v1;
                }

                if((count & 1) != 0)
                {
                    dst -= 1;
                    src -= 1;

                    dst[0] = src[0];
                }
            }
#endif // LLVM
        }

        [Inline]
        internal unsafe static void InternalBackwardMemoryCopy( int* src   ,
                                                                int* dst   ,
                                                                int  count )
        {
            InternalBackwardMemoryCopy( (uint*)src, (uint*)dst, count );
        }

        //--//--//

        internal unsafe static void InternalMemoryMove( byte* src   ,
                                                        byte* dst   ,
                                                        int   count )
        {
            BugCheck.Assert( count >= 0, BugCheck.StopCode.NegativeIndex );

            if(src <= dst && dst < &src[count])
            {
                InternalBackwardMemoryCopy( src, dst, count );
            }
            else
            {
                InternalMemoryCopy( src, dst, count );
            }
        }

        [Inline]
        internal unsafe static void InternalMemoryMove( sbyte* src   ,
                                                        sbyte* dst   ,
                                                        int    count )
        {
            InternalMemoryMove( (byte*)src, (byte*)dst, count );
        }

        //--//

        internal unsafe static void InternalMemoryMove( ushort* src   ,
                                                                ushort* dst   ,
                                                                int     count )
        {
            BugCheck.Assert( count >= 0, BugCheck.StopCode.NegativeIndex );

            if(src <= dst && dst < &src[count])
            {
                InternalBackwardMemoryCopy( src, dst, count );
            }
            else
            {
                InternalMemoryCopy( src, dst, count );
            }
        }

        [Inline]
        internal unsafe static void InternalMemoryMove( short* src   ,
                                                        short* dst   ,
                                                        int    count )
        {
            InternalMemoryMove( (ushort*)src, (ushort*)dst, count );
        }

        [Inline]
        internal unsafe static void InternalMemoryMove( char* src   ,
                                                        char* dst   ,
                                                        int   count )
        {
            InternalMemoryMove( (ushort*)src, (ushort*)dst, count );
        }

        //--//

        internal unsafe static void InternalMemoryMove( uint* src   ,
                                                        uint* dst   ,
                                                        int   count )
        {
            BugCheck.Assert( count >= 0, BugCheck.StopCode.NegativeIndex );

            if(src <= dst && dst < &src[count])
            {
                InternalBackwardMemoryCopy( src, dst, count );
            }
            else
            {
                InternalMemoryCopy( src, dst, count );
            }
        }

        [Inline]
        internal unsafe static void InternalMemoryMove( int* src   ,
                                                        int* dst   ,
                                                        int  count )
        {
            InternalMemoryMove( (uint*)src, (uint*)dst, count );
        }
    }
}
