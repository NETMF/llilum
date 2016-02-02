using System;
using Llvm.NET.Native;
using Llvm.NET.Values;

namespace Llvm.NET
{
    public class MetadataAsValue : Value
    {
        internal MetadataAsValue( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }

        internal static LLVMValueRef IsAMetadataAsValue( LLVMValueRef value )
        {
            if( value.Pointer == IntPtr.Zero )
                return value;

            return NativeMethods.GetValueKind( value ) == ValueKind.MetadataAsValue ? value : default( LLVMValueRef );
        }

        //public static implicit operator Metadata( MetadataAsValue self )
        //{
        //    // TODO: Add support to get the metadata ref from the value...
        //    // e.g. call C++ MetadataAsValue.getMetadata()
        //    throw new NotImplementedException();
        //}
    }
}