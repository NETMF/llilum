using System;
using Llvm.NET.Values;

namespace Microsoft.Zelig.LLVM
{
    public class _Value
    {
        public _Type Type( ) => Owner.GetOrInsertType( Impl.TypeImpl );
        public _Module Module( ) => Owner;

        public bool IsInteger( ) => Impl.GetLLVMObject().Type.IsInteger();
        public bool IsFloat( ) => Impl.GetLLVMObject().Type.IsFloat( );
        public bool IsDouble( ) => Impl.GetLLVMObject().Type.IsDouble( );
        public bool IsFloatingPoint( ) => Impl.GetLLVMObject().Type.IsFloatingPoint( );
        public bool IsPointer( ) => Impl.GetLLVMObject().Type.IsPointer();
        public bool IsPointerPointer( ) => Impl.GetLLVMObject().Type.IsPointerPointer( );

        public bool IsImmediate( ) => Impl.IsImmediate;
        public bool IsZeroedValue( )
        {
            var constant = Impl.GetLLVMObject() as Constant;
            if( constant == null )
                return false;

            return constant.IsZeroValue;
        }

        public bool IsAnUninitializedGlobal( )
        {
            var gv = Impl.GetLLVMObject() as GlobalVariable;
            if( gv == null )
                return false;

            return gv.Initializer == null;
        }

        public void SetGlobalInitializer( Constant val )
        {
            var gv = Impl.GetLLVMObject() as GlobalVariable;
            if( gv != null )
            {
                gv.Initializer = val;
            }
        }

        public void MergeToAndRemove( _Value targetVal )
        {
            GlobalVariable gv = Impl.GetLLVMObject() as GlobalVariable;
            if( gv != null )
            {
                gv.ReplaceAllUsesWith( targetVal.Impl.GetLLVMObject() );
                gv.RemoveFromParent( );
            }
        }

        public void FlagAsConstant( )
        {
            var gv = Impl.GetLLVMObject() as GlobalVariable;
            if( gv != null )
            {
                gv.IsConstant = true;
                gv.Section = ".text";
                gv.UnnamedAddress = true;
            }
        }

        public void Dump( )
        {
            Impl.Dump( );
            Console.WriteLine( "{0}", IsImmediate( ) ? "IS IMMEDIATE\n" : "" );
        }

        internal _Value( _Module module )
            : this( module, null )
        {
        }

        internal _Value( _Module module, ValueImpl impl )
        {
            Owner = module;
            Impl = impl;
        }

        // in LLVM a module doesn't own a value, the context does
        // however, here a _Module is a container for the context,
        // a module and DiBuilder with assorted other state info.
        protected _Module Owner { get; }
        internal ValueImpl Impl { get; set; }
    }

    // replicates model from C++/CLI as it isn't clear if multiple
    // _Value instances might refer to the same Value_impl instance
    internal class ValueImpl
    {
        internal ValueImpl( TypeImpl typeImpl, Value value, bool isImmediate )
        {
            TypeImpl = typeImpl;
            Value = value;
            IsImmediate = isImmediate;
        }

        internal void Dump( )
        {
            Console.WriteLine( "Value: \n" );
            Console.WriteLine( Value.ToString() );
            Console.WriteLine( "Of Type: \n" );
            TypeImpl.Dump( );
        }

        internal TypeImpl TypeImpl { get; }
        internal bool IsImmediate  { get; }
        internal Value GetLLVMObject( ) => Value;

        private readonly Value Value;
    }
}
