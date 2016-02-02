using System;
using System.Collections.Generic;
using Llvm.NET.Native;
using Llvm.NET.Types;
using Llvm.NET.Values;

namespace Llvm.NET
{
    // TODO: rename this to DataLayout to match underlying LLVM
    // TODO: update this to reflect the deprecation of retrieving a pointer to the layout in 3.7.0
    //       (e.g. the layout should be copy constructed, rather than holding a pointer/handle)

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
    public class DataLayout
        : IDisposable
    {
        /// <summary>Context used for this data (in particular, for retrieving pointer types)</summary>
        public Context Context { get; }

        /// <summary>Retrieves the byte ordering for this target</summary>
        public ByteOrdering Endianess => ( ByteOrdering )NativeMethods.ByteOrder( OpaqueHandle );

        /// <summary>Retrieves the size of a pointer for the default address space of the target</summary>
        /// <returns>Size of a pointer to the default address space</returns>
        public uint PointerSize( ) => NativeMethods.PointerSize( OpaqueHandle );

        /// <summary>Retrieves the size of a pointer for a given address space of the target</summary>
        /// <param name="addressSpace">Address space for the pointer</param>
        /// <returns>Size of a pointer</returns>
        public uint PointerSize( uint addressSpace ) => NativeMethods.PointerSizeForAS( OpaqueHandle, addressSpace );

        /// <summary>Retrieves an LLVM integer type with the same bit width as
        /// a pointer for the default address space of the target</summary>
        /// <returns>Integer type matching the bit width of a native pointer in the target's default address space</returns>
        public ITypeRef IntPtrType( ) => TypeRef.FromHandle( NativeMethods.IntPtrTypeInContext( Context.ContextHandle, OpaqueHandle ) );

        /// <summary>Retrieves an LLVM integer type with the same bit width as
        /// a pointer for the given address space of the target</summary>
        /// <returns>Integer type matching the bit width of a native pointer in the target's address space</returns>
        public ITypeRef IntPtrType( uint addressSpace )
        {
            var typeHandle = NativeMethods.IntPtrTypeForASInContext( Context.ContextHandle, OpaqueHandle, addressSpace );
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
            return NativeMethods.SizeOfTypeInBits( OpaqueHandle, typeRef.GetTypeRef( ) );
        }

        /// <summary>Retrieves the number of bits required to store a value of the given type</summary>
        /// <param name="typeRef">Type to retrieve the storage size of</param>
        /// <returns>Number of bits required to store a value of the given type in the target</returns>
        /// <remarks>This method retrieves the storage size in bits of a given type. The storage size
        /// includes any trailing padding bits that may be needed if the target requires reading a wider
        /// word size. (e.g. most systems can't write a single bit value for an LLVM i1, thus the
        /// storage size is whatever the minimum number of bits that the target requires to store a value
        /// of the given type)
        /// </remarks>
        public ulong StoreSizeOf( ITypeRef typeRef )
        {
            VerifySized( typeRef, nameof( typeRef ) );
            return NativeMethods.StoreSizeOfType( OpaqueHandle, typeRef.GetTypeRef( ) );
        }

        /// <summary>Retrieves the ABI specified size of the given type</summary>
        /// <param name="typeRef">Type to get the size from</param>
        /// <returns>Size of the type</returns>
        public ulong AbiSizeOf( ITypeRef typeRef )
        {
            VerifySized( typeRef, nameof( typeRef ) );
            return NativeMethods.ABISizeOfType( OpaqueHandle, typeRef.GetTypeRef( ) );
        }

        /// <summary>Retrieves the ABI specified alignment, in bytes, for a specified type</summary>
        /// <param name="typeRef">Type to get the alignment for</param>
        /// <returns>ABI specified alignment</returns>
        public uint AbiAlignmentOf( ITypeRef typeRef )
        {
            VerifySized( typeRef, nameof( typeRef ) );
            return NativeMethods.ABIAlignmentOfType( OpaqueHandle, typeRef.GetTypeRef( ) );
        }

        /// <summary>Retrieves the call frame alignment for a given type</summary>
        /// <param name="typeRef">type to get the alignment of</param>
        /// <returns>Alignment for the type</returns>
        public uint CallFrameAlignmentOf( ITypeRef typeRef )
        {
            VerifySized( typeRef, nameof( typeRef ) );
            return NativeMethods.CallFrameAlignmentOfType( OpaqueHandle, typeRef.GetTypeRef( ) );
        }

        public uint PreferredAlignmentOf( ITypeRef typeRef )
        {
            VerifySized( typeRef, nameof( typeRef ) );
            return NativeMethods.PreferredAlignmentOfType( OpaqueHandle, typeRef.GetTypeRef( ) );
        }

        public uint PreferredAlignmentOf( Value value )
        {
            if( value == null )
                throw new ArgumentNullException( nameof( value ) );

            VerifySized( value.NativeType, nameof( value ) );
            return NativeMethods.PreferredAlignmentOfGlobal( OpaqueHandle, value.ValueHandle );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Specific type required by interop call" )]
        public uint ElementAtOffset( IStructType structType, ulong offset )
        {
            VerifySized( structType, nameof( structType ) );
            return NativeMethods.ElementAtOffset( OpaqueHandle, structType.GetTypeRef( ), offset );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Specific type required by interop call" )]
        public ulong OffsetOfElement( IStructType structType, uint element )
        {
            VerifySized( structType, nameof( structType ) );
            return NativeMethods.OffsetOfElement( OpaqueHandle, structType.GetTypeRef( ), element );
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
        /// <param name="context"><see cref="Context"/> for types created by the new <see cref="DataLayout"/></param>
        /// <param name="layout">string to parse</param>
        /// <returns>Parsed target data</returns>
        /// <remarks>For full details on the syntax of the string see <a href="http://llvm.org/releases/3.7.0/docs/LangRef.html#data-layout">http://llvm.org/releases/3.7.0/docs/LangRef.html#data-layout</a></remarks>
        public static DataLayout Parse( Context context, string layout )
        {
            var handle = NativeMethods.CreateTargetData( layout );
            return FromHandle( context, handle, true );
        }

        #region IDisposable Support
        private readonly bool IsDisposable;

        protected virtual void Dispose( bool disposing )
        {
            if( OpaqueHandle.Pointer != IntPtr.Zero && IsDisposable )
            {
                NativeMethods.DisposeTargetData( OpaqueHandle );
                OpaqueHandle = default( LLVMTargetDataRef );
            }
        }

        ~DataLayout( )
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose( false );
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose( )
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose( true );
            GC.SuppressFinalize( this );
        }
        #endregion

        internal DataLayout( Context context, LLVMTargetDataRef targetDataHandle, bool isDisposable )
        {
            OpaqueHandle = targetDataHandle;
            IsDisposable = isDisposable;
            Context = context;
        }

        internal static DataLayout FromHandle( Context context, LLVMTargetDataRef targetDataRef, bool isDisposable )
        {
            lock ( TargetDataMap )
            {
                DataLayout retVal;
                if( TargetDataMap.TryGetValue( targetDataRef.Pointer, out retVal ) )
                    return retVal;

                retVal = new DataLayout( context, targetDataRef, isDisposable );
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

        private static readonly Dictionary<IntPtr, DataLayout> TargetDataMap = new Dictionary<IntPtr, DataLayout>( );
    }
}
