using System.Linq;
using System.Collections.Generic;
using System;
using Llvm.NET.Types;

namespace Llvm.NET.Values
{
    /// <summary>LLVM Constant Array</summary>
    /// <remarks>
    /// Due to how LLVM treats constant arrays internally creating a constant array
    /// with the From method overloads may not actually produce a ConstantArray
    /// instance. At the least it will produce a Constant. LLVM will determine the
    /// appropriate internal representation based on the input types and values
    /// </remarks>
    public class ConstantArray 
        : Constant
    {
        /// <summary>Create a constant array of values of a given type</summary>
        /// <param name="elementType">Type of elements in the array</param>
        /// <param name="values">Values to initialize the array</param>
        /// <returns>Constant representing the array</returns>
        public static Constant From( ITypeRef elementType, params Constant[ ] values )
        {
            return From( elementType, ( IEnumerable<Constant> )values );
        }

        /// <summary>Create a constant array of values of a given type</summary>
        /// <param name="elementType">Type of elements in the array</param>
        /// <param name="values">Values to initialize the array</param>
        /// <returns>Constant representing the array</returns>
        public static Constant From( ITypeRef elementType, IEnumerable<Constant> values )
        {
            if( values.Any( v => v.Type.TypeHandle != elementType.TypeHandle ) )
                throw new ArgumentException( "One or more value(s) type does not match specified array element type" );

            var valueHandles = values.Select( v => v.ValueHandle ).ToArray( );
            var argCount = valueHandles.Length;
            if( argCount == 0 )
                valueHandles = new LLVMValueRef[ 1 ];

            var handle = NativeMethods.ConstArray( elementType.GetTypeRef(), out valueHandles[ 0 ], (uint)argCount );
            return FromHandle<Constant>( handle );
        }

        internal ConstantArray( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal ConstantArray( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAConstantArray ) )
        {
        }
    }
}
