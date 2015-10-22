using System;
using System.Collections.Generic;
using Llvm.NET.Types;
using Llvm.NET.Values;

namespace Llvm.NET
{
    /// <summary>Provides access to LLVM target data layout information</summary>
    /// <remarks>
    /// <para>There is a distinction between various sizes and alignment for a given type
    /// that are target dependent.</para>
    /// <para>The following table illustrates the differences in sizes and their meaning
    ///  for a sample set of types.</para>
    /// <list type="table">
    ///   <listheader>
    ///   <term>Type</term>
    ///   <term>SizeInBits</term>
    ///   <term>StoreSizeInBits</term>
    ///   <term>AllocSizeInBits</term>
    ///   </listheader>
    ///   <item><term>i1</term>         <term>1</term>   <term>8</term>   <term>8</term></item>
    ///   <item><term>i8</term>         <term>8</term>   <term>8</term>   <term>8</term></item>
    ///   <item><term>i19</term>        <term>19</term>  <term>24</term>  <term>32</term></item>
    ///   <item><term>i32</term>        <term>32</term>  <term>32</term>  <term>32</term></item>
    ///   <item><term>i100</term>       <term>100</term> <term>104</term> <term>128</term></item>
    ///   <item><term>i128</term>       <term>128</term> <term>128</term> <term>128</term></item>
    ///   <item><term>Float</term>      <term>32</term>  <term>32</term>  <term>32</term></item>
    ///   <item><term>Double</term>     <term>64</term>  <term>64</term>  <term>64</term></item>
    ///   <item><term>X86_FP80</term>   <term>80</term>  <term>80</term>  <term>96</term></item>
    /// </list>
    /// <note type="note">
    /// The alloc size depends on the alignment, and thus on the target.
    /// The values in the example table are for x86-32-linux.
    /// </note>
    /// </remarks>
    public class TargetData
        : IDisposable
    {
        /// <summary>Retrives the byte ordering for this target</summary>
        public ByteOrdering Endianess => (ByteOrdering)NativeMethods.ByteOrder( OpaqueHandle );

        /// <summary>Retrieves the size of a pointer for the default address space of the target</summary>
        /// <returns>Size of a pointer to the default address space</returns>
        public uint PointerSize() => NativeMethods.PointerSize( OpaqueHandle );

        /// <summary>Retrieves the size of a pointer for a given address space of the target</summary>
        /// <param name="addressSpace">Address space for the pointer</param>
        /// <returns>Size of a pointer</returns>
        public uint PointerSize( uint addressSpace ) => NativeMethods.PointerSizeForAS( OpaqueHandle, addressSpace );

        /// <summary>Retrives an LLVM integer type with the same bit width as
        /// a pointer for the default address sapce of the target</summary>
        /// <returns>Integer type matching the bit width of a native pointer in the target's default address space</returns>
        public ITypeRef IntPtrType() => TypeRef.FromHandle( NativeMethods.IntPtrType( OpaqueHandle ) );

        /// <summary>Retrives an LLVM integer type with the same bit width as
        /// a pointer for the given address sapce of the target</summary>
        /// <returns>Integer type matching the bit width of a native pointer in the target's address space</returns>
        public ITypeRef IntPtrType( uint addressSpace )
        {
            var typeHandle = NativeMethods.IntPtrTypeForAS( OpaqueHandle, addressSpace );
            return TypeRef.FromHandle( typeHandle );
        }

        /// <summary>Returns the number of bits necessary to hold the specified type.</summary>
        /// <param name="typeRef">Type to retrieve the size of</param>
        /// <remarks>
        /// <para>This method determines the bit size of a type (e.g. the minimum number of
        /// bits required to represent any value of the given type.) This is distinct from the storage
        /// and stack size due to various target alignment requirements.</para>
        ///</remarks>
        public ulong BitSizeOf( ITypeRef typeRef )
        {
            VerifySized( typeRef, nameof( typeRef ) );
            return NativeMethods.SizeOfTypeInBits( OpaqueHandle, typeRef.GetTypeRef() );
        }

        /// <summary>Retrieves the number of bits required to store a value of the given type</summary>
        /// <param name="typeRef">Type to retrive the storage size of</param>
        /// <returns>Number of bits required to store a value of the given type in the target</returns>
        /// <remarks>This method retrives the storage size in bits of a given type. The storage size
        /// includes any trailing padding bits that may be needed if the target requres reading a wider
        /// word size. (e.g. most systems can't write a single bit value for an LLVM i1, thus the
        /// storage size is whatever the minimum number of bits that the target requires to store a value
        /// of the given type)
        /// </remarks>
        public ulong StoreSizeOf( ITypeRef typeRef )
        {
            VerifySized( typeRef, nameof( typeRef ) );
            return NativeMethods.StoreSizeOfType( OpaqueHandle, typeRef.GetTypeRef() );
        }

        /// <summary>Retrieves the ABI specified size of the given type</summary>
        /// <param name="typeRef">Type to ge the size from</param>
        /// <returns>Size of the type</returns>
        public ulong AbiSizeOf( ITypeRef typeRef )
        {
            VerifySized( typeRef, nameof( typeRef ) );
            return NativeMethods.ABISizeOfType( OpaqueHandle, typeRef.GetTypeRef() );
        }

        /// <summary>Retrieves the ABI specified alignment, in bytes, for a specified type</summary>
        /// <param name="typeRef">Type to get the alignment for</param>
        /// <returns>ABI specified alignment</returns>
        public uint AbiAlignmentOf( ITypeRef typeRef )
        {
            VerifySized( typeRef, nameof( typeRef ) );
            return NativeMethods.ABIAlignmentOfType( OpaqueHandle, typeRef.GetTypeRef() );
        }

        /// <summary>Retrieves the call frame alignment for a given type</summary>
        /// <param name="typeRef">type to get the alignment of</param>
        /// <returns>Alignment for the type</returns>
        public uint CallFrameAlignmentOf( ITypeRef typeRef )
        {
            VerifySized( typeRef, nameof( typeRef ) );
            return NativeMethods.CallFrameAlignmentOfType( OpaqueHandle, typeRef.GetTypeRef() );
        }

        public uint PreferredAlignmentOf( ITypeRef typeRef )
        {
            VerifySized( typeRef, nameof( typeRef ) );
            return NativeMethods.PreferredAlignmentOfType( OpaqueHandle, typeRef.GetTypeRef() );
        }

        public uint PreferredAlignmentOf( Value value )
        {
            VerifySized( value.Type, nameof( value ) );
            return NativeMethods.PreferredAlignmentOfGlobal( OpaqueHandle, value.ValueHandle );
        }

        public uint ElementAtOffset( IStructType structType, ulong offset )
        {
            VerifySized( structType, nameof( structType ) );
            return NativeMethods.ElementAtOffset( OpaqueHandle, structType.GetTypeRef(), offset );
        }

        public ulong OffsetOfElement( IStructType structType, uint element )
        {
            VerifySized( structType, nameof( structType ) );
            return NativeMethods.OffsetOfElement( OpaqueHandle, structType.GetTypeRef(), element );
        }

        public override string ToString( )
        {
            IntPtr msgPtr = NativeMethods.CopyStringRepOfTargetData( OpaqueHandle );
            return NativeMethods.MarshalMsg( msgPtr );
        }

        public ulong ByteSizeOf( ITypeRef llvmType ) => BitSizeOf( llvmType ) / 8u;

        public uint PreferredBitAlignementOf( ITypeRef llvmType ) => PreferredAlignmentOf( llvmType ) * 8;

        public uint AbiBitAlignmentOf( ITypeRef llvmType ) => AbiAlignmentOf( llvmType ) * 8;

        public ulong BitOffsetOfElement( IStructType llvmType, uint element ) => OffsetOfElement( llvmType, element ) * 8;

        /// <summary>Parses an LLVM target layout string</summary>
        /// <param name="layoutString">string to parse</param>
        /// <returns>Parsed target data</returns>
        /// <remarks>For full details on the syntax of the string see <a href="http://llvm.org/releases/3.7.0/docs/LangRef.html#data-layout">http://llvm.org/releases/3.7.0/docs/LangRef.html#data-layout</a></remarks>
        public static TargetData Parse( string layoutString )
        {
            var handle = NativeMethods.CreateTargetData( layoutString );
            return FromHandle( handle, true );
        }

        internal TargetData( LLVMTargetDataRef targetDataHandle, bool isDisposable )
        {
            OpaqueHandle = targetDataHandle;
            IsDisposable = isDisposable;
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

        private static void VerifySized( ITypeRef type, string name )
        {
            if( !type.IsSized )
                throw new ArgumentException( "Type must be sized to get target size information", name );
        }

        private static readonly Dictionary<IntPtr, TargetData> TargetDataMap = new Dictionary<IntPtr, TargetData>( );

        #region IDisposable Support
        private readonly bool IsDisposable;

        protected virtual void Dispose( bool disposing )
        {
            if( OpaqueHandle.Pointer != IntPtr.Zero && IsDisposable )
            {
                NativeMethods.DisposeTargetData( OpaqueHandle );
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
