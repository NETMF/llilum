using Llvm.NET.Types;
using System.Collections.Generic;
using System.Linq;

namespace Llvm.NET.DebugInfo
{
    /// <summary>
    /// DebugFunction type is mostly a wrapper around an existing IFunctionType
    /// </summary>
    public class DebugFunctionType
        : DebugType<IFunctionType, DISubroutineType>
        , IFunctionType
    {
        public DebugFunctionType( IFunctionType llvmType
                                , Module module
                                , DIFile diFile
                                , DebugInfoFlags flags
                                , DebugType<ITypeRef,DIType> retType
                                , params DebugType<ITypeRef, DIType>[ ] argTypes
                                )
            : base( llvmType
                  , module.DIBuilder.CreateSubroutineType( diFile
                                                         , (uint)flags
                                                         , retType.DIType
                                                         , argTypes.Select( t=>t.DIType )
                                                         )
                  )  
        {
        }

        internal DebugFunctionType( IFunctionType rawType, DISubroutineType sub )
            : base( rawType, sub )
        {
        }

        public bool IsVarArg => ((IFunctionType)NativeType).IsVarArg;

        public ITypeRef ReturnType => ((IFunctionType)NativeType ).ReturnType;

        public IReadOnlyList<ITypeRef> ParameterTypes => ((IFunctionType)NativeType).ParameterTypes;
    }
}
