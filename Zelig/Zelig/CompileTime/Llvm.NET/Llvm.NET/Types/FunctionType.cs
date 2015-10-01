using System.Linq;
using System.Collections.Generic;

namespace Llvm.NET.Types
{
    /// <summary>Class to repersent the LLVM type of a function (e.g. a signature)</summary>
    public class FunctionType
        : TypeRef
    {
        internal FunctionType( LLVMTypeRef typeRef )
            : base( typeRef )
        {
        }

        /// <summary>Flag to indicate if this signature is for a variadic function</summary>
        public bool IsVarArg => LLVMNative.IsFunctionVarArg( TypeHandle );

        /// <summary></summary>
        public TypeRef ReturnType => TypeRef.FromHandle( LLVMNative.GetReturnType( TypeHandle ) );
        public IReadOnlyList<TypeRef> ParameterTypes
        {
            get
            {
                var paramCount = LLVMNative.CountParamTypes( TypeHandle );
                if( paramCount == 0 )
                    return new List<TypeRef>().AsReadOnly();

                var paramTypes = new LLVMTypeRef[ paramCount ];
                LLVMNative.GetParamTypes( TypeHandle, out paramTypes[ 0 ] );
                return paramTypes.Select( TypeRef.FromHandle )
                                 .ToList( )
                                 .AsReadOnly( );
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static FunctionType FromHandle( LLVMTypeRef typeRef )
        {
            return ( FunctionType )Context.CurrentContext.GetTypeFor( typeRef, ( h ) => new FunctionType( h ) );
        }
    }
}
