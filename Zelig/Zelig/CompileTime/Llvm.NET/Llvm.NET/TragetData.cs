using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Llvm.NET
{
    /// <summary>Provides access to LLVM target data layout information</summary>
    public class TargetData
        : IDisposable
    {
        public ByteOrdering Endianess => (ByteOrdering)LLVMNative.ByteOrder( OpaqueHandle );
        public uint PointerSize() => LLVMNative.PointerSize( OpaqueHandle );
        public uint PointerSize( uint addressSpace ) => LLVMNative.PointerSizeForAS( OpaqueHandle, addressSpace );

        public TypeRef IntPtrType() => TypeRef.FromHandle( LLVMNative.IntPtrType( OpaqueHandle ) );
        public TypeRef IntPtrType( uint addressSpace )
        {
            var typeHandle = LLVMNative.IntPtrTypeForAS( OpaqueHandle, addressSpace );
            return TypeRef.FromHandle( typeHandle );
        }

        public TypeRef IntPtrType( Context context )
        {
            var typeHandle = LLVMNative.IntPtrTypeInContext( context.ContextHandle, OpaqueHandle );
            return TypeRef.FromHandle( typeHandle );
        }

        public TypeRef IntPtrType( Context context, uint addressSpace )
        {
            var typeHandle = LLVMNative.IntPtrTypeInContext( context.ContextHandle, OpaqueHandle );
            return TypeRef.FromHandle( typeHandle );
        }

        /// <summary>Returns the number of bits necessary to hold the specified type.</summary>
        /// <param name="typeRef">Type to retrieve the size of</param>
        /// <remarks>
        /// <para>This method determines the bit size of a type (e.g. the maximum number of
        /// bits required to hold a value of the given type.) This is distinct from the sorage
        /// and stack size due to various target alignment requirements. The following table
        /// illustrates the differences in sizes and their meaning for a sample set of types.</para>
        /// <list type="table">
        ///   <listheader>
        ///   <term>Type</term>
        ///   <term>SizeInBits</term>
        ///   <term>StoreSizeInBits</term>
        ///   <term>AllocSizeInBits</term>
        ///   </listheader>
        ///   <item><term>i1</term>         <term>1</term>   <term>8</term>   <term>8</term>
        ///   <item><term>i8</term>         <term>8</term>   <term>8</term>   <term>8</term>
        ///   <item><term>i19</term>        <term>19</term>  <term>24</term>  <term>32</term>
        ///   <item><term>i32</term>        <term>32</term>  <term>32</term>  <term>32</term>
        ///   <item><term>i100</term>       <term>100</term> <term>104</term> <term>128</term>
        ///   <item><term>i128</term>       <term>128</term> <term>128</term> <term>128</term>
        ///   <item><term>Float</term>      <term>32</term>  <term>32</term>  <term>32</term>
        ///   <item><term>Double</term>     <term>64</term>  <term>64</term>  <term>64</term>
        ///   <item><term>X86_FP80</term>   <term>80</term>  <term>80</term>  <term>96</term>
        /// </list>
        /// <note type="note">
        /// The alloc size depends on the alignment, and thus on the target.
        /// The values in the example table are for x86-32 linux.
        /// </note>
        ///</remarks>
        public ulong BitSizeOf( TypeRef typeRef ) => LLVMNative.SizeOfTypeInBits( OpaqueHandle, typeRef.TypeHandle );
        public ulong StoreSizeOf( TypeRef typeRef ) => LLVMNative.StoreSizeOfType( OpaqueHandle, typeRef.TypeHandle );
        public ulong AbiSizeOf( TypeRef typeRef ) => LLVMNative.ABISizeOfType( OpaqueHandle, typeRef.TypeHandle );
        public uint AbiAlignmentOf( TypeRef typeRef ) => LLVMNative.ABIAlignmentOfType( OpaqueHandle, typeRef.TypeHandle );
        public uint CallFrameAlignmentOf( TypeRef typeRef ) => LLVMNative.CallFrameAlignmentOfType( OpaqueHandle, typeRef.TypeHandle );
        public uint PreferredAlignmentOf( TypeRef typeRef ) => LLVMNative.PreferredAlignmentOfType( OpaqueHandle, typeRef.TypeHandle );
        public uint PreferredAlignmentOf( Value value ) => LLVMNative.PreferredAlignmentOfGlobal( OpaqueHandle, value.ValueHandle );
        public uint ElementAtOffset( TypeRef structType, ulong offset ) => LLVMNative.ElementAtOffset( OpaqueHandle, structType.TypeHandle, offset );
        public ulong OffsetOfElement( TypeRef structType, uint element ) => LLVMNative.OffsetOfElement( OpaqueHandle, structType.TypeHandle, element );

        public override string ToString( )
        {
            IntPtr msgPtr = LLVMNative.CopyStringRepOfTargetData( OpaqueHandle );
            if( msgPtr == IntPtr.Zero )
                return string.Empty;

            var retVal = Marshal.PtrToStringAnsi( msgPtr );
            LLVMNative.DisposeMessage( msgPtr );
            return retVal;
        }

        internal TargetData( LLVMTargetDataRef targetDataHandle, bool isDisposable )
        {
            OpaqueHandle = targetDataHandle;
            IsDisposable = isDisposable;
        }

        internal static TargetData Parse( string layoutString )
        {
            var handle = LLVMNative.CreateTargetData( layoutString );
            return FromHandle( handle, true );
        }

        internal static TargetData FromHandle( LLVMTargetDataRef targetDataRef, bool isDisposable )
        {
            lock( TargetDataMap )
            {
                TargetData retVal;
                if( TargetDataMap.TryGetValue( targetDataRef.Pointer, out retVal ) )
                    return retVal;

                retVal = new TargetData( targetDataRef, isDisposable );
                TargetDataMap.Add( targetDataRef.Pointer, retVal );
                return retVal;
            }
        }

        internal LLVMTargetDataRef OpaqueHandle { get; private set; }
        private readonly bool IsDisposable;

        private static readonly Dictionary<IntPtr, TargetData> TargetDataMap = new Dictionary<IntPtr, TargetData>( );

        #region IDisposable Support
        protected virtual void Dispose( bool disposing )
        {
            if( OpaqueHandle.Pointer != IntPtr.Zero && IsDisposable )
            {
                LLVMNative.DisposeTargetData( OpaqueHandle );
                OpaqueHandle = default(LLVMTargetDataRef);
            }
        }

        ~TargetData( )
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose( false );
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose( )
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose( true );
             GC.SuppressFinalize(this);
        }
        #endregion
    }
}
