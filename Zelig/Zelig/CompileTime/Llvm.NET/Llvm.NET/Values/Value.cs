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
                if( Context.IsDisposed )
                    return string.Empty;

                var ptr = NativeMethods.GetValueName( ValueHandle );
                return Marshal.PtrToStringAnsi( ptr );
            }
            set
            {
                NativeMethods.SetValueName( ValueHandle, value );
                // LLVM auto adds a numeric suffix if a register with the same name already exists
                Debug.Assert( Name.StartsWith( value ) );
            }
        }

        /// <summary>Indicates if this value is Undefined</summary>
        public bool IsUndefined => NativeMethods.IsUndef( ValueHandle );

        /// <summary>Determines if the Value represents the NULL value for the values type</summary>
        public bool IsNull => NativeMethods.IsNull( ValueHandle );

        /// <summary>Type of the value</summary>
        public ITypeRef Type => TypeRef.FromHandle( NativeMethods.TypeOf( ValueHandle ) );

        public Context Context => Type.Context;

        /// <summary>Generates a string representing the LLVM syntax of the value</summary>
        /// <returns>string version of the value formatted by LLVM</returns>
        public override string ToString( )
        {
            var ptr = NativeMethods.PrintValueToString( ValueHandle );
            return NativeMethods.MarshalMsg( ptr );
        }

        public void ReplaceAllUsesWith( Value other )
        {
            NativeMethods.ReplaceAllUsesWith( ValueHandle, other.ValueHandle );
        }

        public bool TryGetExtendedPropertyValue<T>( string id, out T value ) => ExtensibleProperties.TryGetExtendedPropertyValue<T>( id, out value );
        public void AddExtendedPropertyValue( string id, object value ) => ExtensibleProperties.AddExtendedPropertyValue( id, value );

        internal Value( LLVMValueRef valueRef )
        {
            if( valueRef.Pointer == IntPtr.Zero )
                throw new ArgumentNullException( nameof( valueRef ) );

#if DEBUG
            var context = Llvm.NET.Context.GetContextFor( valueRef );
            context.AssertValueNotInterned( valueRef );
#endif
            ValueHandle = valueRef;
        }

        internal LLVMValueRef ValueHandle { get; }

        internal static Value FromHandle( LLVMValueRef valueRef ) => FromHandle<Value>( valueRef );

        internal static T FromHandle<T>( LLVMValueRef valueRef )
            where T : Value
        {
            var context = Context.GetContextFor( valueRef );
            return (T)context.GetValueFor( valueRef, StaticFactory );
        }

        private static Value StaticFactory( LLVMValueRef h )
        {
            var kind = NativeMethods.GetValueKind( h );
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
            var msgString = NativeMethods.MarshalMsg( NativeMethods.PrintValueToString( fromHandle ) );
            Debug.Print( msgString );
            // attach the details to the exception so it is available after the fact in the debugger
            // and any logs that dump exception details.
            ex.Data.Add( "Llvm.NETNative.Handle.Dump", msgString );
            throw ex;
        }

        private ExtensiblePropertyContainer ExtensibleProperties = new ExtensiblePropertyContainer( );
    }

    /// <summary>Provides extension methods to <see cref="Value"/> that cannot be achieved as members of the class</summary>
    /// <remarks>
    /// Using generic static extension methods allows for fluent coding while retaining the type of the "this" parameter.
    /// If these were members of the <see cref="Value"/> class then the only return type could be <see cref="Value"/>,
    /// thus losing the orignal type and requiring a cast to get back to it.
    /// </remarks>
    public static class ValueExtensions
    {
        /// <summary>Sets the debugging location for a value</summary>
        /// <typeparam name="T"> Type of the value to tag</typeparam>
        /// <param name="value">Value to set debug location for</param>
        /// <param name="line">Line number</param>
        /// <param name="column">Column number</param>
        /// <param name="scope">Scope for the value</param>
        /// <remarks>
        /// <para>Technically speaking only an <see cref="Instructions.Instruction"/> can have debug location
        /// information. However, since LLVM will perform constant folding in the <see cref="InstructionBuilder"/>
        /// most of the methods in <see cref="InstructionBuilder"/> return a <see cref="Value"/> rather than a
        /// more specific <see cref="Instructions.Instruction"/>. Thus, without this extension method here,
        /// code would need to know ahead of time that an actual instruction would be produced then cast the result
        /// to an <see cref="Instructions.Instruction"/> and then set the debug location. This makes the code rather
        /// ugly and tedious to manage. Placing this as a generic extension method ensures that the return type matches
        /// the original and no additional casting is needed, which would defeat the purpose of doing this. For
        /// <see cref="Value"/> types that are not instructions this does nothing. This allows for a simpler fluent
        /// style of programming where the actual type is retained even in cases where an <see cref="InstructionBuilder"/>
        /// method will always return an atual instruction.</para>
        /// <para>In order to help simplify code generation for cases where not all of the source information is
        /// available this is a NOP if <paramref name="scope"/> is null. Thus, it is safe to call even when debugging
        /// information isn't actually available. This helps to avoid cluttering calling code with test for debug info
        /// before trying to add it.</para>
        /// </remarks>
        public static T SetDebugLocation<T>( this T value, uint line, uint column, DebugInfo.DIScope scope )
            where T : Value
        {
            if( value is Instructions.Instruction && scope != null )
                NativeMethods.SetDebugLoc( value.ValueHandle, line, column, scope.MetadataHandle );

            return value;
        }

        /// <summary>Sets the virtual register name for a value</summary>
        /// <typeparam name="T"> Type of the value to set the name for</typeparam>
        /// <param name="value">Value to set register name for</param>
        /// <param name="name">Name for the virtual register the value represents</param>
        /// <remarks>
        /// <para>Technically speaking only an <see cref="Instructions.Instruction"/> can have register name
        /// information. However, since LLVM will perform constant folding in the <see cref="InstructionBuilder"/>
        /// it almost all of the methods in <see cref="InstructionBuilder"/> return a <see cref="Value"/> rather
        /// than an more specific <see cref="Instructions.Instruction"/>. Thus, without this extension method here,
        /// code would need to know ahead of time that an actual instruction would be produced then cast the result
        /// to an <see cref="Instructions.Instruction"/> and then set the debug location. This makes the code rather
        /// ugly and tedious to manage. Placing this as a generic extension method ensures that the return type matches
        /// the original and no additional casting is needed, which would defeat the purpose of doing this. For
        ///  <see cref="Value"/> types that are not instructions this does nothing. This allows for a simpler fluent
        /// style of programming where the actual type is retained even in cases where an <see cref="InstructionBuilder"/>
        /// method will always return an atual instruction.</para>
        /// <para>Since the <see cref="Value.Name"/> property is available on all <see cref="Value"/>s this is slightly
        /// redundant. It is useful for maintining the fluent style of coding along with expressing intent more clearly.
        /// (e.g. using this makes it expressly clear that the intent is to set the virtual register name and not the
        /// name of a local variable etc...) Using the fluent style allows a 50% reduction in the number of overloaded
        /// methods in <see cref="InstructionBuilder"/> to account for all variations with or without a name.
        /// </para>
        /// </remarks>
        public static T RegisterName<T>( this T value, string name )
            where T : Value
        {
            var inst = value as Instructions.Instruction;
            if( inst != null )
                value.Name = name;

            return value;
        }
    }
}
