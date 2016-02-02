using System.Collections.Generic;
using System.Linq;
using Llvm.NET.Native;

namespace Llvm.NET.Types
{
    /// <summary>Interface to represent the LLVM type of a function (e.g. a signature)</summary>
    public interface IFunctionType
        : ITypeRef
    {
        /// <summary>Flag to indicate if this signature is for a variadic function</summary>
        bool IsVarArg { get; }
        
        /// <summary>Return type of the function</summary>
        ITypeRef ReturnType { get; }

        /// <summary>Collection of types of the parameters for the function</summary>
        IReadOnlyList<ITypeRef> ParameterTypes { get; }
    }

    /// <summary>Class to represent the LLVM type of a function (e.g. a signature)</summary>
    internal class FunctionType
        : TypeRef
        , IFunctionType
    {
        internal FunctionType( LLVMTypeRef typeRef )
            : base( typeRef )
        {
        }

        /// <inheritdoc/>
        public bool IsVarArg => NativeMethods.IsFunctionVarArg( TypeHandle_ );

        /// <inheritdoc/>
        public ITypeRef ReturnType => FromHandle<ITypeRef>( NativeMethods.GetReturnType( TypeHandle_ ) );

        /// <inheritdoc/>
        public IReadOnlyList<ITypeRef> ParameterTypes
        {
            get
            {
                var paramCount = NativeMethods.CountParamTypes( TypeHandle_ );
                if( paramCount == 0 )
                    return new List<TypeRef>().AsReadOnly();

                var paramTypes = new LLVMTypeRef[ paramCount ];
                NativeMethods.GetParamTypes( TypeHandle_, out paramTypes[ 0 ] );
                return paramTypes.Select( FromHandle<TypeRef> )
                                 .ToList( )
                                 .AsReadOnly( );
            }
        }
    }
}
