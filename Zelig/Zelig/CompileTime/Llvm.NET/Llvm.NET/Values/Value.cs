using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Llvm.NET.Types;

namespace Llvm.NET.Values
{
    /// <summary>LLVM Value</summary>
    /// <remarks>
    /// Value is the root of a hierarchy of types representing values
    /// in LLVM. Values (and derived classes) are never constructed 
    /// directly with the new operator. Instead, they are produced by
    /// other classes in this library internally. This is becuase they
    /// are just wrappers around the LLVM-C API handles and must
    /// maintain the "uniqueing" semantics. (e.g. allowing reference
    /// equality for values that are fundamentally the same value)
    /// This is generally hidden in the internals of the Llvm.NET
    /// library so callers need not be concerned with the details 
    /// but can rely on the expected behavior that two Value instances
    /// referring to the same actual value (i.e. a function) are actually
    /// the same .NET object as well within the same <see cref="Context"/>
    /// </remarks>
    public class Value : IExtensiblePropertyContainer
    {
        /// <summary>Name of the value (if any)</summary>
        public string Name
        {
            get
            {
                var ptr = LLVMNative.GetValueName( ValueHandle );
                return Marshal.PtrToStringAnsi( ptr );
            }
            set
            {
                LLVMNative.SetValueName( ValueHandle, value );
                Debug.Assert( Name == value );
            }
        }

        /// <summary>Indicates if this value is Undefined</summary>
        public bool IsUndefined => LLVMNative.IsUndef( ValueHandle );

        /// <summary>Determines if the Value represents the NULL value for the values type</summary>
        public bool IsNull => LLVMNative.IsNull( ValueHandle );

        /// <summary>Type of the value</summary>
        public TypeRef Type => TypeRef.FromHandle( LLVMNative.TypeOf( ValueHandle ) );

        /// <summary>Generates a string representing the LLVM syntax of the value</summary>
        /// <returns>string version of the value formatted by LLVM</returns>
        public override string ToString( )
        {
            var ptr = LLVMNative.PrintValueToString( ValueHandle );
            try
            {
                return Marshal.PtrToStringAnsi( ptr );
            }
            finally
            {
                LLVMNative.DisposeMessage( ptr );
            }
        }

        public void ReplaceAllUsesWith( Value other )
        {
            LLVMNative.ReplaceAllUsesWith( ValueHandle, other.ValueHandle );
        }

        public bool TryGetExtendedPropertyValue<T>( string id, out T value ) => ExtensibleProperties.TryGetExtendedPropertyValue<T>( id, out value );
        public void AddExtendedPropertyValue( string id, object value ) => ExtensibleProperties.AddExtendedPropertyValue( id, value );

        internal Value( LLVMValueRef valueRef )
        {
            if( valueRef.Pointer == IntPtr.Zero )
                throw new ArgumentNullException( nameof( valueRef ) );

            Context.CurrentContext.AssertValueNotInterned( valueRef );
            ValueHandle = valueRef;
        }

        internal LLVMValueRef ValueHandle { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal static Value FromHandle( LLVMValueRef valueRef )
        {
            return Context.CurrentContext.GetValueFor( valueRef, StaticFactory );
        }

        private static Value StaticFactory( LLVMValueRef h )
        {
            var kind = LLVMNative.GetValueKind( h );
            switch( kind )
            {
            case ValueKind.Argument:
                return new Argument( h, true );
                
            case ValueKind.BasicBlock:
                return new BasicBlock( h, true );

            case ValueKind.Function:
                return new Function( h, true );

            case ValueKind.GlobalAlias:
                return new GlobalAlias( h, true );

            case ValueKind.GlobalVariable:
                return new GlobalVariable( h, true );

            case ValueKind.UndefValue:
                return new UndefValue( h, true );

            case ValueKind.BlockAddress:
                return new BlockAddress( h, true );

            case ValueKind.ConstantExpr:
                return new ConstantExpression( h, true );

            case ValueKind.ConstantAggregateZero:
                return new ConstantAggregateZero( h, true );

            case ValueKind.ConstantDataArray:
                return new ConstantDataArray( h, true );

            case ValueKind.ConstantDataVector:
                return new ConstantDataVector( h, true );

            case ValueKind.ConstantInt:
                return new ConstantInt( h, true );

            case ValueKind.ConstantFP:
                return new ConstantFP( h, true );

            case ValueKind.ConstantArray:
                return new ConstantArray( h, true );

            case ValueKind.ConstantStruct:
                return new ConstantStruct( h, true );

            case ValueKind.ConstantVector:
                return new ConstantVector( h, true );

            case ValueKind.ConstantPointerNull:
                return new ConstantPointerNull( h, true );

            case ValueKind.MetadataAsValue:
                return new MetadataAsValue( h, true );

            case ValueKind.InlineAsm:
                return new InlineAsm( h, true );

            case ValueKind.Instruction:
                throw new ArgumentException( "Value with kind==Instruction is not valid" );

            case ValueKind.Return:
                return new Instructions.Return( h, true );

            case ValueKind.Branch:
                return new Instructions.Branch( h, true );

            case ValueKind.Switch:
                return new Instructions.Switch( h, true );

            case ValueKind.IndirectBranch:
                return new Instructions.IndirectBranch( h, true );

            case ValueKind.Invoke:
                return new Instructions.Invoke( h, true );
                
            case ValueKind.Unreachable:
                return new Instructions.Unreachable( h, true );

            case ValueKind.Add:
            case ValueKind.FAdd:
            case ValueKind.Sub:
            case ValueKind.FSub:
            case ValueKind.Mul:
            case ValueKind.FMul:
            case ValueKind.UDiv:
            case ValueKind.SDiv:
            case ValueKind.FDiv:
            case ValueKind.URem:
            case ValueKind.SRem:
            case ValueKind.FRem:
            case ValueKind.Shl:
            case ValueKind.LShr:
            case ValueKind.AShr:
            case ValueKind.And:
            case ValueKind.Or:
            case ValueKind.Xor:
                return new Instructions.BinaryOperator( h, true );

            case ValueKind.Alloca:
                return new Instructions.Alloca( h, true );

            case ValueKind.Load:
                return new Instructions.Load( h, true );

            case ValueKind.Store:
                return new Instructions.Store( h, true );

            case ValueKind.GetElementPtr:
                return new Instructions.GetElementPtr( h, true );

            case ValueKind.Trunc:
                return new Instructions.Trunc( h, true );

            case ValueKind.ZeroExtend:
                return new Instructions.ZeroExtend( h, true );

            case ValueKind.SignExtend:
                return new Instructions.SignExtend( h, true );

            case ValueKind.FPToUI:
                return new Instructions.FPToUI( h, true );

            case ValueKind.FPToSI:
                return new Instructions.FPToSI( h, true );

            case ValueKind.UIToFP:
                return new Instructions.UIToFP( h, true );

            case ValueKind.SIToFP:
                return new Instructions.SIToFP( h, true );

            case ValueKind.FPTrunc:
                return new Instructions.FPTrunc( h, true );

            case ValueKind.FPExt:
                return new Instructions.FPExt( h, true );

            case ValueKind.PtrToInt:
                return new Instructions.PointerToInt( h, true );

            case ValueKind.IntToPtr:
                return new Instructions.IntToPointer( h, true );

            case ValueKind.BitCast:
                return new Instructions.BitCast( h, true );

            case ValueKind.AddrSpaceCast:
                return new Instructions.AddressSpaceCast( h, true );

            case ValueKind.ICmp:
                return new Instructions.IntCmp( h, true );

            case ValueKind.FCmp:
                return new Instructions.FCmp( h, true );

            case ValueKind.Phi:
                return new Instructions.PhiNode( h, true );

            case ValueKind.Call:
                return new Instructions.Call( h, true );

            case ValueKind.Select:
                return new Instructions.Select( h, true );

            case ValueKind.UserOp1:
                return new Instructions.UserOp1( h, true );

            case ValueKind.UserOp2:
                return new Instructions.UserOp2( h, true );

            case ValueKind.VaArg:
                return new Instructions.VaArg( h, true );

            case ValueKind.ExtractElement:
                return new Instructions.ExtractElement( h, true );

            case ValueKind.InsertElement:
                return new Instructions.InsertElement( h, true );

            case ValueKind.ShuffleVector:
                return new Instructions.ShuffleVector( h, true );

            case ValueKind.ExtractValue:
                return new Instructions.ExtractValue( h, true );

            case ValueKind.InsertValue:
                return new Instructions.InsertValue( h, true );

            case ValueKind.Fence:
                return new Instructions.Fence( h, true );

            case ValueKind.AtomicCmpXchg:
                return new Instructions.AtomicCmpXchg( h, true );

            case ValueKind.AtomicRMW:
                return new Instructions.AtomicRMW( h, true );

            case ValueKind.Resume:
                return new Instructions.Resume( h, true );

            case ValueKind.LandingPad:
                return new Instructions.LandingPad( h, true );

            default:
                if( kind >= ValueKind.ConstantFirstVal && kind <= ValueKind.ConstantLastVal )
                    return new Constant( h, true );
                else if( kind > ValueKind.Instruction )
                    return new Instructions.Instruction( h );
                else
                    return new Value( h );
            }
        }

        /// <summary>Used to provide common error and exception handling for constructors of derived types</summary>
        /// <param name="fromHandle">Original handle being converted</param>
        /// <param name="converter">Conversion function to convert the handle (e.g. one of the LLVM.IsaXXX functions)</param>
        /// <returns>Converted handle if it isn't null, otherwise an exception is triggered</returns>
        /// <remarks>
        /// If the converted handle is null then the conversion failed and an exception is thrown. This
        /// allows the  constructor for derived types using handles as parameters to perform the conversion
        /// and use this centralized method to validate the correct handle is passed (e.g. dynamic down
        /// casting). Using a central function ensures the error reporting and diagnostics for tracing the
        /// problem are consistent without repeating the same code everywhere.
        /// </remarks>
        internal static LLVMValueRef ValidateConversion( LLVMValueRef fromHandle, Func<LLVMValueRef,LLVMValueRef> converter )
        {
            LLVMValueRef toHandle = converter( fromHandle );
            if( toHandle.Pointer != IntPtr.Zero )
                return toHandle;

            var ex = new ArgumentException( "Incompatible handle type" );
            
            // Use LLVM to print to the debugger what the handle is for use in diagnosing the problem
            var msgString = LLVMNative.PrintValueToString( fromHandle );
            try
            {
                var txt = Marshal.PtrToStringAnsi( msgString );
                Debug.Print( txt );
                // attach the details to the exception so it is available after the fact in the debugger
                // and any logs that dump exception details.
                ex.Data.Add( "Llvm.NETNative.Handle.Dump", txt );
            }
            finally
            {
                LLVMNative.DisposeMessage( msgString );
            }
            throw ex;
        }

        private ExtensiblePropertyContainer ExtensibleProperties = new ExtensiblePropertyContainer( );
    }
}
