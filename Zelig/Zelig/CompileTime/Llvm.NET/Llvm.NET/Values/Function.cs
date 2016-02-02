using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Llvm.NET.Types;
using Llvm.NET.DebugInfo;
using Llvm.NET.Native;

namespace Llvm.NET.Values
{
    /// <summary>LLVM Function definition</summary>
    public class Function
        : GlobalObject
        , IAttributeSetContainer
    {
        /// <summary>Signature type of the function</summary>
        public IFunctionType Signature => TypeRef.FromHandle<IFunctionType>( NativeMethods.GetElementType( NativeMethods.TypeOf( ValueHandle ) ) );

        /// <summary>Entry block for this function</summary>
        public BasicBlock EntryBlock
        {
            get
            {
                if( NativeMethods.CountBasicBlocks( ValueHandle ) == 0 )
                    return null;

                return BasicBlock.FromHandle( NativeMethods.GetEntryBasicBlock( ValueHandle ) );
            }
        }

        /// <summary>Basic Blocks for the function</summary>
        public IReadOnlyList<BasicBlock> BasicBlocks
        {
            get
            {
                uint count = NativeMethods.CountBasicBlocks( ValueHandle );
                var buf = new LLVMBasicBlockRef[ count ];
                if( count > 0 )
                    NativeMethods.GetBasicBlocks( ValueHandle, out buf[ 0 ] );

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
                return ( CallingConvention )NativeMethods.GetFunctionCallConv( ValueHandle );
            }
            set
            {
                NativeMethods.SetFunctionCallConv( ValueHandle, ( uint )value );
            }
        }

        /// <summary>LLVM instrinsicID for the method</summary>
        public uint IntrinsicId => NativeMethods.GetIntrinsicID( ValueHandle );

        /// <summary>Flag to indicate if the method signature accepts variable arguments</summary>
        public bool IsVarArg => Signature.IsVarArg;

        /// <summary>Return type of the function</summary>
        public ITypeRef ReturnType => Signature.ReturnType;

        public Function PersonalityFunction
        {
            get
            {
                if( !NativeMethods.FunctionHasPersonalityFunction( ValueHandle ) )
                    return null;

                return FromHandle<Function>( NativeMethods.GetPersonalityFunction( ValueHandle ) );
            }

            set
            {
                NativeMethods.SetPersonalityFunction( ValueHandle, value?.ValueHandle ?? LLVMValueRef.Zero );
            }
        }

        /// <summary>Debug information for this function</summary>
        public DISubProgram DISubProgram
        {
            get
            {
                return MDNode.FromHandle<DISubProgram>( NativeMethods.FunctionGetSubprogram( ValueHandle ) );
            }
            set
            {
                if( ( value != null ) && !value.Describes( this ) )
                    throw new ArgumentException( "Subprogram does not describe this Function" );

                NativeMethods.FunctionSetSubprogram( ValueHandle, value?.MetadataHandle ?? LLVMMetadataRef.Zero );
            }
        }

        /// <summary>Garbage collection engine name that this function is generated to work with</summary>
        /// <remarks>For details on GC support in LLVM see: http://llvm.org/docs/GarbageCollection.html </remarks>
        public string GcName
        {
            get
            {
                var nativePtr = NativeMethods.GetGC( ValueHandle );
                return Marshal.PtrToStringAnsi( nativePtr );
            }
            set
            {
                NativeMethods.SetGC( ValueHandle, value );
            }
        }

        /// <summary>Verifies the function is valid and all blocks properly terminated</summary>
        public void Verify( )
        {
            IntPtr errMsgPtr;
            var status = NativeMethods.VerifyFunctionEx( ValueHandle, LLVMVerifierFailureAction.LLVMReturnStatusAction, out errMsgPtr );
            if( status )
                throw new InternalCodeGeneratorException( NativeMethods.MarshalMsg( errMsgPtr ) );
        }

        public AttributeSet Attributes
        {
            get
            {
                return new AttributeSet( NativeMethods.GetFunctionAttributeSet( ValueHandle ) );
            }

            set
            {
                // TODO: verify that the attribute set doesn't contain any attributes for parameter indices not available in this function.
                NativeMethods.SetFunctionAttributeSet( ValueHandle, value.NativeAttributeSet );
            }
        }

        /// <summary>Add a new basic block to the beginning of a function</summary>
        /// <param name="name">Name (label) for the block</param>
        /// <returns><see cref="BasicBlock"/> created and inserted at the beginning of the function</returns>
        public BasicBlock PrependBasicBlock( string name )
        {
            LLVMBasicBlockRef firstBlock = NativeMethods.GetFirstBasicBlock( ValueHandle );
            BasicBlock retVal;
            if( firstBlock.Pointer == IntPtr.Zero )
            {
                retVal = AppendBasicBlock( name );
            }
            else
            {
                var blockRef = NativeMethods.InsertBasicBlockInContext( NativeType.Context.ContextHandle, firstBlock, name );
                retVal = BasicBlock.FromHandle( blockRef );
            }
            return retVal;
        }

        /// <summary>Appends a new basic block to a function</summary>
        /// <param name="name">Name (label) of the block</param>
        /// <returns><see cref="BasicBlock"/> created and inserted onto the end of the function</returns>
        public BasicBlock AppendBasicBlock( string name )
        {
            LLVMBasicBlockRef blockRef = NativeMethods.AppendBasicBlockInContext( NativeType.Context.ContextHandle, ValueHandle, name );
            return BasicBlock.FromHandle( blockRef );
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
            : base( valueRef )
        {
        }
    }
}
