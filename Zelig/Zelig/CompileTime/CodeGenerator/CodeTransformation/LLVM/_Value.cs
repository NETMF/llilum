using System;
using Llvm.NET.Values;

namespace Microsoft.Zelig.LLVM
{
   // REVIEW:
   // This class should probably become a static class with extension methods
   // for the few properties it keeps the Llvm.NET Extensible propery mechanism
   // should suffice to handle them. This would eliminate the need for this
   // extra layer of abstraction (and source of confusion).
    public class _Value
    {
        internal _Value( _Module module, _Type type, Value value )
        {
            Module = module;
            Type = type;
            LlvmValue = value;
        }

        public _Type Type { get; }

        // in LLVM a module doesn't own a value, the context does
        // however, here a _Module is a container for the context,
        // a module and DiBuilder with assorted other state info.
        public _Module Module { get; }

        public bool IsInteger => LlvmValue.NativeType.IsInteger;

        public bool IsFloatingPoint => LlvmValue.NativeType.IsFloatingPoint;

        public bool IsPointer => LlvmValue.NativeType.IsPointer;

        public bool IsAnUninitializedGlobal( )
        {
            var gv = LlvmValue as GlobalVariable;
            if( gv == null )
                return false;

            return gv.Initializer == null;
        }

        public void SetGlobalInitializer( Constant val )
        {
            var gv = LlvmValue as GlobalVariable;
            if( gv != null )
            {
                gv.Initializer = val;
            }
        }

        public void MergeToAndRemove( _Value targetVal )
        {
            GlobalVariable gv = LlvmValue as GlobalVariable;
            if( gv != null )
            {
                gv.ReplaceAllUsesWith( targetVal.LlvmValue );
                gv.RemoveFromParent( );
            }
        }

        public void FlagAsConstant( )
        {
            var gv = LlvmValue as GlobalVariable;
            if( gv != null )
            {
                gv.IsConstant = true;
                gv.Section = ".text";
                gv.UnnamedAddress = true;
            }
        }

        public Value LlvmValue { get; }
    }
}
