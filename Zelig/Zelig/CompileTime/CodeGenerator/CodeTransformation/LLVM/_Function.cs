using Llvm.NET;
using Llvm.NET.DebugInfo;
using Llvm.NET.Values;
using System;
using System.Diagnostics;
using Llvm.NET.Types;
using IR = Microsoft.Zelig.CodeGeneration.IR;
using TS = Microsoft.Zelig.Runtime.TypeSystem;

namespace Microsoft.Zelig.LLVM
{
    /// <summary>Subset of AttributeKind values which apply specifically to functions.</summary>
    public enum FunctionAttribute : uint
    {
        AlwaysInline = AttributeKind.AlwaysInline,
        Builtin = AttributeKind.Builtin,
        Cold = AttributeKind.Cold,
        Convergent = AttributeKind.Convergent,
        InlineHint = AttributeKind.InlineHint,
        JumpTable = AttributeKind.JumpTable,
        MinSize = AttributeKind.MinSize,
        Naked = AttributeKind.Naked,
        NoBuiltin = AttributeKind.NoBuiltin,
        NoDuplicate = AttributeKind.NoDuplicate,
        NoImplicitFloat = AttributeKind.NoImplicitFloat,
        NoInline = AttributeKind.NoInline,
        NonLazyBind = AttributeKind.NonLazyBind,
        NoRedZone = AttributeKind.NoRedZone,
        NoReturn = AttributeKind.NoReturn,
        NoUnwind = AttributeKind.NoUnwind,
        OptimizeForSize = AttributeKind.OptimizeForSize,
        OptimizeNone = AttributeKind.OptimizeNone,
        ReadNone = AttributeKind.ReadNone,
        ReadOnly = AttributeKind.ReadOnly,
        ArgMemOnly = AttributeKind.ArgMemOnly,
        ReturnsTwice = AttributeKind.ReturnsTwice,
        StackAlignment = AttributeKind.StackAlignment,
        StackProtect = AttributeKind.StackProtect,
        StackProtectReq = AttributeKind.StackProtectReq,
        StackProtectStrong = AttributeKind.StackProtectStrong,
        SafeStack = AttributeKind.SafeStack,
        SanitizeAddress = AttributeKind.SanitizeAddress,
        SanitizeThread = AttributeKind.SanitizeThread,
        SanitizeMemory = AttributeKind.SanitizeMemory,
        UnwindTable = AttributeKind.UWTable,
    }

    // REVIEW:
    // Given that this has no data members beyond those from the base class
    // this class probably ought to be a static class with extensions methods 
    // on the Llvm.NET.Values.Function class. 
    public class _Function
    {
        internal _Function(_Module module, TS.MethodRepresentation method)
        {
            Module = module;
            Method = method;

            LlvmValue = CreateLLvmFunctionWithDebugInfo(module, method);
            LlvmValue.SetDebugType(module.Manager.GetOrInsertType(method));

            var function = ( Function )LlvmValue;
            if( function.BasicBlocks.Count == 0 )
                function.Linkage( Linkage.ExternalWeak );

            if (method.HasBuildTimeFlag(TS.MethodRepresentation.BuildTimeAttributes.Inline))
            {
                AddAttribute(FunctionAttribute.AlwaysInline);
            }

            if (method.HasBuildTimeFlag(TS.MethodRepresentation.BuildTimeAttributes.NoInline))
            {
                AddAttribute(FunctionAttribute.NoInline);
            }

            if (method.HasBuildTimeFlag(TS.MethodRepresentation.BuildTimeAttributes.BottomOfCallStack))
            {
                AddAttribute(FunctionAttribute.Naked);
            }

            if (method.HasBuildTimeFlag(TS.MethodRepresentation.BuildTimeAttributes.NoReturn))
            {
                AddAttribute(FunctionAttribute.NoReturn);
            }

            // Try to find an explicit stack alignment attribute and apply it if it exists.
            TS.WellKnownTypes wkt = module.TypeSystem.WellKnownTypes;
            TS.CustomAttributeRepresentation alignAttr = method.FindCustomAttribute(wkt.Microsoft_Zelig_Runtime_AlignmentRequirementsAttribute);
            if (alignAttr != null)
            {
                AddAttribute(FunctionAttribute.StackAlignment, (uint)alignAttr.FixedArgsValues[0]);
            }
        }

        public TS.MethodRepresentation Method { get; }

        // In LLVM, the context owns the value. However, a _Module here is a container for the
        // context, a NativeModule, and a DiBuilder with assorted other state info.
        public _Module Module
        {
            get;
        }

        public Function LlvmFunction => (Function)LlvmValue;

        private Value LlvmValue
        {
            get;
        }

        public void AddAttribute(FunctionAttribute kind)
        {
            LlvmFunction.AddAttributes((AttributeKind)kind);
        }

        public void AddAttribute(FunctionAttribute kind, ulong value)
        {
            LlvmFunction.AddAttributes(new AttributeValue((AttributeKind)kind, value));
        }

        public void RemoveAttribute(FunctionAttribute kind)
        {
            LlvmFunction.RemoveAttribute((AttributeKind)kind);
        }

        public _BasicBlock GetOrInsertBasicBlock(string blockName)
        {
            return new _BasicBlock(this, LlvmFunction.FindOrCreateNamedBlock(blockName));
        }

        public Value GetLocalStackValue(
            TS.MethodRepresentation method,
            _BasicBlock block,
            IR.VariableExpression val,
            LLVMModuleManager manager)
        {
            block.EnsureDebugInfo( manager, method );

            bool hasDebugName = !string.IsNullOrWhiteSpace( val.DebugName?.Name );

            Value retVal = block.InsertAlloca(
                hasDebugName ? val.DebugName.Name : val.ToString( ),
                manager.GetOrInsertType( val.Type ) );

            // If the local had a valid symbolic name in the source code, generate debug info for it.
            if( !hasDebugName )
            {
                return retVal;
            }

            DILocalVariable localSym;

            _Type valType = manager.GetOrInsertType( val.Type );
            var argExpression = val as IR.ArgumentVariableExpression;
            if( argExpression != null )
            {
                // Adjust index since IR form keeps slot = 0 as the "this" param for static methods it just sets that to
                // null. LLVM doesn't have any notion of that and only has a slot for an actual arg.
                var index = checked( ( ushort )argExpression.Number );
                if( method is TS.StaticMethodRepresentation )
                {
                    index--;
                }

                localSym = Module.DIBuilder.CreateArgument( block.CurDISubProgram
                                                          , val.DebugName.Name
                                                          , block.CurDILocation.Scope.File
                                                          , block.CurDILocation.Line
                                                          , valType.DIType
                                                          , true
                                                          , DebugInfoFlags.None
                                                          , checked( (ushort)(index + 1) ) // LLVM debug arg index is one based
                                                          );
            }
            else
            {
                localSym = Module.DIBuilder.CreateLocalVariable( block.CurDISubProgram
                                                               , val.DebugName.Name
                                                               , block.CurDILocation.Scope.File
                                                               , block.CurDILocation.Line 
                                                               , valType.DIType
                                                               , true
                                                               , DebugInfoFlags.None
                                                               );
            }

            // For reference types passed as a pointer, tell debugger to deref the pointer.
            DIExpression expr = Module.DIBuilder.CreateExpression( );
            if( !retVal.GetDebugType().IsValueType )
            {
                expr = Module.DIBuilder.CreateExpression( ExpressionOp.deref );
            }

            Module.DIBuilder.InsertDeclare( retVal, localSym, expr, block.CurDILocation, block.LlvmBasicBlock );
            return retVal;
        }

        public void SetExternalLinkage()
        {
            LlvmFunction.Linkage = Linkage.External;
        }

        public void SetInternalLinkage()
        {
            LlvmFunction.Linkage = Linkage.Internal;
        }

        private static Function CreateLLvmFunctionWithDebugInfo( _Module module, TS.MethodRepresentation method )
        {
            string mangledName = LLVMModuleManager.GetFullMethodName( method );
            _Type functionType = module.Manager.GetOrInsertType( method );
            Debugging.DebugInfo loc = module.Manager.GetDebugInfoFor( method );
            Debug.Assert( loc != null );

            var containingType = module.GetOrInsertType( method.OwnerType );

            // Create the DISupprogram info
            var retVal = module.LlvmModule.CreateFunction( containingType?.DIType ?? ( DIScope )module.DICompileUnit
                                                         , method.Name
                                                         , mangledName
                                                         , module.GetOrCreateDIFile( loc.SrcFileName )
                                                         , ( uint )loc.BeginLineNumber
                                                         , (DebugFunctionType)functionType.DebugType
                                                         , true
                                                         , true
                                                         , ( uint )loc.EndLineNumber
                                                         , DebugInfoFlags.None // TODO: Map Zelig accesibility info etc... to flags
                                                         , false
                                                         );
            bool isStatic = method is TS.StaticMethodRepresentation;
            // "this" is always at index 0, for static functions the name for "this" is null
            int paramBase = isStatic ? 1 : 0;

            string[] argumentNames = null;
            if(method.ArgumentNames == null)
            {
                argumentNames = new string[ method.ThisPlusArguments.Length ];

                if(method is TS.InstanceMethodRepresentation)
                {
                    argumentNames[ 0 ] = "this";
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Generic method {0} has not been instantiated correctly", method.FullyQualifiedName );
                }
            }
            else
            {
                argumentNames = method.ArgumentNames;
            }

            Debug.Assert( retVal != null && argumentNames.Length - paramBase == retVal.Parameters.Count );
            for( int i = paramBase; i < argumentNames.Length; ++i )
            {
                string name = argumentNames[ i ];
                if( string.IsNullOrWhiteSpace( name ) )
                    name = $"$ARG{i}";

                // adjust the index for the native type since there's not assumption that [0]=="this"
                retVal.Parameters[ i - paramBase ].Name = name;
            }
            return retVal;
        }
    }
}
