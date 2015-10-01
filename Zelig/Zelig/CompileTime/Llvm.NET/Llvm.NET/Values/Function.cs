using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Llvm.NET.Types;

namespace Llvm.NET.Values
{
    /// <summary>LLVM Function definition</summary>
    public class Function
        : GlobalObject
    {
        /// <summary>Signature type of the function</summary>
        public FunctionType Signature => FunctionType.FromHandle( LLVMNative.GetElementType( LLVMNative.TypeOf( ValueHandle ) ) );

        /// <summary>Entry block for this function</summary>
        public BasicBlock EntryBlock
        {
            get
            {
                if( LLVMNative.CountBasicBlocks( ValueHandle ) == 0 )
                    return null;

                return BasicBlock.FromHandle( LLVMNative.GetEntryBasicBlock( ValueHandle ) );
            }
        }

        /// <summary>Basic Blocks for the function</summary>
        public IReadOnlyList<BasicBlock> BasicBlocks
        {
            get
            {
                uint count = LLVMNative.CountBasicBlocks( ValueHandle );
                var buf = new LLVMBasicBlockRef[ count ];
                if( count > 0 )
                    LLVMNative.GetBasicBlocks( ValueHandle, out buf[ 0 ] );

                return buf.Select( BasicBlock.FromHandle )
                          .ToList( )
                          .AsReadOnly( );
            }
        }

        /// <summary>Parameters for the function including any method definition specific attributes (i.e. ByVal)</summary>
        public IReadOnlyList<Argument> Parameters => new FunctionParameterList( this );

        /// <summary>Calling convention for the method</summary>
        public CallingConvention CallingConvention
        {
            get
            {
                return ( CallingConvention )LLVMNative.GetFunctionCallConv( ValueHandle );
            }
            set
            {
                LLVMNative.SetFunctionCallConv( ValueHandle, ( uint )value );
            }
        }

        /// <summary>LLVM instrinsicID for the method</summary>
        public uint IntrinsicId => LLVMNative.GetIntrinsicID( ValueHandle );

        /// <summary>Flag to indicate if the method signature accepts variable arguments</summary>
        public bool IsVarArg => Signature.IsVarArg;

        /// <summary>Return type of the function</summary>
        public TypeRef ReturnType => Signature.ReturnType;

        /// <summary>Garbage collection engine name that this function is generated to work with</summary>
        /// <remarks>For details on GC support in LLVM see: http://llvm.org/docs/GarbageCollection.html </remarks>
        public string GcName
        {
            get
            {
                var nativePtr = LLVMNative.GetGC( ValueHandle );
                return Marshal.PtrToStringAnsi( nativePtr );
            }
            set
            {
                LLVMNative.SetGC( ValueHandle, value );
            }
        }

        public void Verify()
        {
            IntPtr errMsgPtr;
            var status = LLVMNative.VerifyFunctionEx( ValueHandle, LLVMVerifierFailureAction.LLVMReturnStatusAction, out errMsgPtr );
            if( status )
                throw new InternalCodeGeneratorException( LLVMNative.MarshalMsg( errMsgPtr) );
        }

        /// <summary>Add attribute flags to the function</summary>
        /// <param name="attrib"><see cref="AttributeKind"/> to add to the function</param>
        /// <param name="value">Value to associate with this attribute. This is usually zero, but can also be a power of
        ///     2 for StackAlignment.</param>
        public void AddAttribute( AttributeKind kind, ulong value ) => LLVMNative.AddFunctionAttr2( ValueHandle, ( LLVMAttributeKind )kind, value );

        /// <summary>Remove attribute flags from the function</summary>
        /// <param name="attrib"><see cref="AttributeKind"/> to remove from the function</param>
        public void RemoveAttribute(AttributeKind kind ) => LLVMNative.RemoveFunctionAttr2( ValueHandle, ( LLVMAttributeKind )kind );

        /// <summary>Add a new basic block to the beginning of a function</summary>
        /// <param name="name">Name (label) for the block</param>
        /// <returns><see cref="BasicBlock"/> created and insterted into the begining function</returns>
        public BasicBlock PrependBasicBlock( string name )
        {
            LLVMBasicBlockRef firstBlock = LLVMNative.GetFirstBasicBlock( ValueHandle );
            BasicBlock retVal;
            if( firstBlock.Pointer == IntPtr.Zero )
            {
                retVal = AppendBasicBlock( name );
            }
            else
            {
                var blockRef = LLVMNative.InsertBasicBlockInContext( Type.Context.ContextHandle, firstBlock, name );
                retVal = BasicBlock.FromHandle( firstBlock );
            }
            return retVal;
        }

        /// <summary>Appends a new basic block to a function</summary>
        /// <param name="name">Name (label) of the block</param>
        /// <returns><see cref="BasicBlock"/> created and insterted onto the end of the function</returns>
        public BasicBlock AppendBasicBlock( string name )
        {
            BasicBlock result;
            LLVMBasicBlockRef blockRef = LLVMNative.AppendBasicBlockInContext( Type.Context.ContextHandle, ValueHandle, name );
            result = BasicBlock.FromHandle( blockRef );
            return result;
        }

        /// <summary>Retrieves or creates  block by name</summary>
        /// <param name="name">Block name (label) to look for or create</param>
        /// <returns><see cref="BasicBlock"/> If the block was created it is appended to the end of function</returns>
        /// <remarks>
        /// This method tries to find a block by it's name and returns it if found, if not found a new block is 
        /// created and appended to the current function.
        /// </remarks>
        public BasicBlock FindOrCreateNamedBlock( string name )
        {
            var retVal = BasicBlocks.FirstOrDefault( b => b.Name == name );
            if( ReferenceEquals( retVal, null ) )
                retVal = AppendBasicBlock( name );

            Debug.Assert( retVal.ContainingFunction.ValueHandle.Pointer == ValueHandle.Pointer );
            return retVal;
        }

        internal Function( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Function( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAFunction ) )
        {
        }
    }
}
