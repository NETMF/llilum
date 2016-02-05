using Llvm.NET.Types;
using System.Collections.Generic;
using System.Linq;

namespace Llvm.NET.DebugInfo
{
    /// <summary>This class provides debug information binding for an <see cref="IFunctionType"/>
    /// and a <see cref="DISubroutineType"/>
    /// </summary>
    /// <remarks>
    /// <para>Function signatures are unnamed interned types in LLVM. While there is usually a one
    /// to one mapping between an LLVM function signature type and the source language debug
    /// signature type that isn't always true. In particular, when passing data by value. In
    /// cases where the address of a by value structure is needed a common pattern is to use
    /// a pointer to the structure in the signature, then perform an Alloca + memcpy. The
    /// actual approach taken depends on the calling conventions of the target. In these cases
    /// you get an LLVM signature that doesn't match the source and could actually match another
    /// source function where a pointer to the structure is actually used in the source.</para>
    /// <para>For example, the following two C language functions might use the same LLVM signature:
    /// <code>void foo(struct bar)</code>
    /// <code>void foo2(struct bar*)</code>
    /// Implementing both of those might be done in LLVM with a single signature:
    /// <code>void (%struct.bar*)</code></para>
    /// <para>This class is designed to provide mapping between the debug signature type
    /// and the underlying LLVM type</para>
    /// <note type="note">It is important to keep in mind that signatures are only concerned
    /// with types. That is, they do not include names of parameters. Parameter information is
    /// provided by <see cref="DebugInfoBuilder.CreateArgument(DIScope, string, DIFile, uint, DIType, bool, DebugInfoFlags, ushort)"/>
    /// and <see cref="O:Llvm.NET.DebugInfo.DebugInfoBuilder.InsertDeclare"/></note>
    /// </remarks>
    public class DebugFunctionType
        : DebugType<IFunctionType, DISubroutineType>
        , IFunctionType
    {
        /// <summary>Constructs a new <see cref="DebugFunctionType"/></summary>
        /// <param name="llvmType">Native LLVM function signature</param>
        /// <param name="module"><see cref="NativeModule"/> to use when construction debug information</param>
        /// <param name="debugFlags"><see cref="DebugInfoFlags"/> for this signature</param>
        /// <param name="retType">Return type for the function</param>
        /// <param name="argTypes">Potentially empty set of argument types for the signature</param>
        public DebugFunctionType( IFunctionType llvmType
                                , NativeModule module
                                , DebugInfoFlags debugFlags
                                , DebugType<ITypeRef,DIType> retType
                                , params DebugType<ITypeRef, DIType>[ ] argTypes
                                )
            : base( llvmType.VerifyArgNotNull( nameof( llvmType ) )
                  , module.VerifyArgNotNull( nameof( module ) )
                          .DIBuilder
                          .CreateSubroutineType( debugFlags
                                               , retType.VerifyArgNotNull( nameof( retType ) ).DIType
                                               , argTypes.Select( t=>t.DIType )
                                               )
                  )  
        {
        }

        /// <inheritdoc/>
        public bool IsVarArg => NativeType.IsVarArg;

        /// <inheritdoc/>
        public ITypeRef ReturnType => NativeType.ReturnType;

        /// <inheritdoc/>
        public IReadOnlyList<ITypeRef> ParameterTypes => NativeType.ParameterTypes;

        internal DebugFunctionType( IFunctionType rawType, DISubroutineType sub )
            : base( rawType, sub )
        {
        }
    }
}
