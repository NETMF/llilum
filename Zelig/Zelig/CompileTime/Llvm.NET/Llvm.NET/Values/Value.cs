using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Llvm.NET.DebugInfo;
using Llvm.NET.Instructions;
using Llvm.NET.Native;
using Llvm.NET.Types;

namespace Llvm.NET.Values
{
    /// <summary>LLVM Value</summary>
    /// <remarks>
    /// Value is the root of a hierarchy of types representing values
    /// in LLVM. Values (and derived classes) are never constructed
    /// directly with the new operator. Instead, they are produced by
    /// other classes in this library internally. This is because they
    /// are just wrappers around the LLVM-C API handles and must
    /// maintain the "uniqueing" semantics. (e.g. allowing reference
    /// equality for values that are fundamentally the same value)
    /// This is generally hidden in the internals of the Llvm.NET
    /// library so callers need not be concerned with the details
    /// but can rely on the expected behavior that two Value instances
    /// referring to the same actual value (i.e. a function) are actually
    /// the same .NET object as well within the same <see cref="Context"/>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling" )]
    public class Value
        : IExtensiblePropertyContainer
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
                Debug.Assert( Name.StartsWith( value, StringComparison.Ordinal ) );
            }
        }

        /// <summary>Indicates if this value is Undefined</summary>
        public bool IsUndefined => NativeMethods.IsUndef( ValueHandle );

        /// <summary>Determines if the Value represents the NULL value for the values type</summary>
        public bool IsNull => NativeMethods.IsNull( ValueHandle );

        /// <summary>Type of the value</summary>
        public ITypeRef NativeType => TypeRef.FromHandle( NativeMethods.TypeOf( ValueHandle ) );

        public Context Context => NativeType.Context;

        /// <summary>Generates a string representing the LLVM syntax of the value</summary>
        /// <returns>string version of the value formatted by LLVM</returns>
        public override string ToString( )
        {
            var ptr = NativeMethods.PrintValueToString( ValueHandle );
            return NativeMethods.MarshalMsg( ptr );
        }

        /// <summary>Replace all uses of a <see cref="Value"/> with another one</summary>
        /// <param name="other">New value</param>
        public void ReplaceAllUsesWith( Value other )
        {
            if( other == null )
                throw new ArgumentNullException( nameof( other ) );

            NativeMethods.ReplaceAllUsesWith( ValueHandle, other.ValueHandle );
        }

        /// <inheritdoc/>
        public bool TryGetExtendedPropertyValue<T>( string id, out T value ) => ExtensibleProperties.TryGetExtendedPropertyValue<T>( id, out value );

        /// <inheritdoc/>
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

        /// <summary>Gets an Llvm.NET managed wrapper for a LibLLVM value handle</summary>
        /// <param name="valueRef">Value handle to wrap</param>
        /// <returns>LLVM.NET managed instance for the handle</returns>
        /// <remarks>
        /// This method uses a cached mapping to ensure that two calls given the same
        /// input handle returns the same managed instance so that reference equality
        /// works as expected.
        /// </remarks>
        internal static Value FromHandle( LLVMValueRef valueRef ) => FromHandle<Value>( valueRef );

        /// <summary>Gets an Llvm.NET managed wrapper for a LibLLVM value handle</summary>
        /// <typeparam name="T">Required type for the handle</typeparam>
        /// <param name="valueRef">Value handle to wrap</param>
        /// <returns>LLVM.NET managed instance for the handle</returns>
        /// <remarks>
        /// This method uses a cached mapping to ensure that two calls given the same
        /// input handle returns the same managed instance so that reference equality
        /// works as expected.
        /// </remarks>
        /// <exception cref="InvalidCastException">When the handle is for a different type of handle than specified by <typeparamref name="T"/></exception>
        internal static T FromHandle<T>( LLVMValueRef valueRef )
            where T : Value
        {
            var context = Context.GetContextFor( valueRef );
            return ( T )context.GetValueFor( valueRef, StaticFactory );
        }

        /// <summary>Central factory for creating instances of <see cref="Value"/> and all derived types</summary>
        /// <param name="h">LibLLVM handle for the value</param>
        /// <returns>New Value or derived type instance that wraps the underlying LibLLVM handle</returns>
        /// <remarks>
        /// This method will determine the correct type for the handle and construct an instance of that
        /// type wrapping the handle.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling" )]
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity" )]
        private static Value StaticFactory( LLVMValueRef h )
        {
            var kind = NativeMethods.GetValueKind( h );
            switch( kind )
            {
            case ValueKind.Argument:
                return new Argument( h );

            case ValueKind.BasicBlock:
                return new BasicBlock( h );

            case ValueKind.Function:
                return new Function( h );

            case ValueKind.GlobalAlias:
                return new GlobalAlias( h );

            case ValueKind.GlobalVariable:
                return new GlobalVariable( h );

            case ValueKind.UndefValue:
                return new UndefValue( h );

            case ValueKind.BlockAddress:
                return new BlockAddress( h );

            case ValueKind.ConstantExpr:
                return new ConstantExpression( h );

            case ValueKind.ConstantAggregateZero:
                return new ConstantAggregateZero( h );

            case ValueKind.ConstantDataArray:
                return new ConstantDataArray( h );

            case ValueKind.ConstantDataVector:
                return new ConstantDataVector( h );

            case ValueKind.ConstantInt:
                return new ConstantInt( h );

            case ValueKind.ConstantFP:
                return new ConstantFP( h );

            case ValueKind.ConstantArray:
                return new ConstantArray( h );

            case ValueKind.ConstantStruct:
                return new ConstantStruct( h );

            case ValueKind.ConstantVector:
                return new ConstantVector( h );

            case ValueKind.ConstantPointerNull:
                return new ConstantPointerNull( h );

            case ValueKind.MetadataAsValue:
                return new MetadataAsValue( h );

            case ValueKind.InlineAsm:
                return new InlineAsm( h );

            case ValueKind.Instruction:
                throw new ArgumentException( "Value with kind==Instruction is not valid" );

            case ValueKind.Return:
                return new Instructions.ReturnInstruction( h );

            case ValueKind.Branch:
                return new Instructions.Branch( h );

            case ValueKind.Switch:
                return new Instructions.Switch( h );

            case ValueKind.IndirectBranch:
                return new Instructions.IndirectBranch( h );

            case ValueKind.Invoke:
                return new Instructions.Invoke( h );

            case ValueKind.Unreachable:
                return new Instructions.Unreachable( h );

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
                return new Instructions.BinaryOperator( h );

            case ValueKind.Alloca:
                return new Instructions.Alloca( h );

            case ValueKind.Load:
                return new Instructions.Load( h );

            case ValueKind.Store:
                return new Instructions.Store( h );

            case ValueKind.GetElementPtr:
                return new Instructions.GetElementPtr( h );

            case ValueKind.Trunc:
                return new Instructions.Trunc( h );

            case ValueKind.ZeroExtend:
                return new Instructions.ZeroExtend( h );

            case ValueKind.SignExtend:
                return new Instructions.SignExtend( h );

            case ValueKind.FPToUI:
                return new Instructions.FPToUI( h );

            case ValueKind.FPToSI:
                return new Instructions.FPToSI( h );

            case ValueKind.UIToFP:
                return new Instructions.UIToFP( h );

            case ValueKind.SIToFP:
                return new Instructions.SIToFP( h );

            case ValueKind.FPTrunc:
                return new Instructions.FPTrunc( h );

            case ValueKind.FPExt:
                return new Instructions.FPExt( h );

            case ValueKind.PtrToInt:
                return new Instructions.PointerToInt( h );

            case ValueKind.IntToPtr:
                return new Instructions.IntToPointer( h );

            case ValueKind.BitCast:
                return new Instructions.BitCast( h );

            case ValueKind.AddrSpaceCast:
                return new Instructions.AddressSpaceCast( h );

            case ValueKind.ICmp:
                return new Instructions.IntCmp( h );

            case ValueKind.FCmp:
                return new Instructions.FCmp( h );

            case ValueKind.Phi:
                return new Instructions.PhiNode( h );

            case ValueKind.Call:
                return new Instructions.CallInstruction( h );

            case ValueKind.Select:
                return new Instructions.Select( h );

            case ValueKind.UserOp1:
                return new Instructions.UserOp1( h );

            case ValueKind.UserOp2:
                return new Instructions.UserOp2( h );

            case ValueKind.VaArg:
                return new Instructions.VaArg( h );

            case ValueKind.ExtractElement:
                return new Instructions.ExtractElement( h );

            case ValueKind.InsertElement:
                return new Instructions.InsertElement( h );

            case ValueKind.ShuffleVector:
                return new Instructions.ShuffleVector( h );

            case ValueKind.ExtractValue:
                return new Instructions.ExtractValue( h );

            case ValueKind.InsertValue:
                return new Instructions.InsertValue( h );

            case ValueKind.Fence:
                return new Instructions.Fence( h );

            case ValueKind.AtomicCmpXchg:
                return new Instructions.AtomicCmpXchg( h );

            case ValueKind.AtomicRMW:
                return new Instructions.AtomicRMW( h );

            case ValueKind.Resume:
                return new Instructions.ResumeInstruction( h );

            case ValueKind.LandingPad:
                return new Instructions.LandingPad( h );

            case ValueKind.CleanUpReturn:
                return new Instructions.CleanupReturn( h );

            case ValueKind.CatchReturn:
                return new Instructions.CatchReturn( h );

            case ValueKind.CatchPad:
                return new Instructions.CatchPad( h );

            case ValueKind.CleanupPad:
                return new Instructions.CleanupPad( h );

            case ValueKind.CatchSwitch:
                return new Instructions.CatchSwitch( h );

            default:
                if( kind >= ValueKind.ConstantFirstVal && kind <= ValueKind.ConstantLastVal )
                    return new Constant( h );

                return kind > ValueKind.Instruction ? new Instructions.Instruction( h ) : new Value( h );
            }
        }

        private readonly ExtensiblePropertyContainer ExtensibleProperties = new ExtensiblePropertyContainer( );
    }

    /// <summary>Provides extension methods to <see cref="Value"/> that cannot be achieved as members of the class</summary>
    /// <remarks>
    /// Using generic static extension methods allows for fluent coding while retaining the type of the "this" parameter.
    /// If these were members of the <see cref="Value"/> class then the only return type could be <see cref="Value"/>,
    /// thus losing the original type and requiring a cast to get back to it.
    /// </remarks>
    public static class ValueExtensions
    {
        /// <summary>Sets the debugging location for a value</summary>
        /// <typeparam name="T"> Type of the value to tag</typeparam>
        /// <param name="value">Value to set debug location for</param>
        /// <param name="location">Debug location information</param>
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
        /// method will always return an actual instruction.</para>
        /// <para>In order to help simplify code generation for cases where not all of the source information is
        /// available this is a NOP if <paramref name="location"/> is null. Thus, it is safe to call even when debugging
        /// information isn't actually available. This helps to avoid cluttering calling code with test for debug info
        /// before trying to add it.</para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Specific type required by interop call" )]
        public static T SetDebugLocation<T>( this T value, DILocation location )
            where T : Value
        {
            if( value == null )
                throw new ArgumentNullException( nameof( value ) );

            if( location == null )
                return value;

            var instruction = value as Instructions.Instruction;
            if( instruction != null )
            {
                if( !location.Scope.SubProgram.Describes( instruction.ContainingBlock.ContainingFunction ) )
                    throw new ArgumentException( "Location does not describe the function containing the provided instruction", nameof( location ) );

                NativeMethods.SetDILocation( value.ValueHandle, location.MetadataHandle );
            }

            return value;
        }

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
        /// method will always return an actual instruction.</para>
        /// <para>In order to help simplify code generation for cases where not all of the source information is
        /// available this is a NOP if <paramref name="scope"/> is null. Thus, it is safe to call even when debugging
        /// information isn't actually available. This helps to avoid cluttering calling code with test for debug info
        /// before trying to add it.</para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Specific type required by interop call" )]
        public static T SetDebugLocation<T>( this T value, uint line, uint column, DebugInfo.DILocalScope scope )
            where T : Value
        {
            if( scope == null )
                return value;

            var instruction = value as Instructions.Instruction;
            if( instruction != null )
            {
                if( !scope.SubProgram.Describes( instruction.ContainingBlock.ContainingFunction ) )
                    throw new ArgumentException( "scope does not describe the function containing the provided instruction", nameof( scope ) );

                NativeMethods.SetDebugLoc( value.ValueHandle, line, column, scope.MetadataHandle );
            }

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
        /// method will always return an actual instruction.</para>
        /// <para>Since the <see cref="Value.Name"/> property is available on all <see cref="Value"/>s this is slightly
        /// redundant. It is useful for maintaining the fluent style of coding along with expressing intent more clearly.
        /// (e.g. using this makes it expressly clear that the intent is to set the virtual register name and not the
        /// name of a local variable etc...) Using the fluent style allows a significant reduction in the number of
        /// overloaded methods in <see cref="InstructionBuilder"/> to account for all variations with or without a name.
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
