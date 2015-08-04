using System;

namespace Llvm.NET
{
    /// <summary>An LLVM Global Variable</summary>
    public class GlobalVariable
        : GlobalObject
    {
        /// <summary>Flag to indicate if this variable is initialized in an external module</summary>
        public bool IsExternallyInitialized
        {
            get
            {
                return LLVMNative.IsExternallyInitialized( ValueHandle );
            }

            set
            {
                LLVMNative.SetExternallyInitialized( ValueHandle, value );
            }
        }

        /// <summary>Gets or sets if this global is a Constant</summary>
        public bool IsConstant
        {
            get
            {
                return LLVMNative.IsGlobalConstant( ValueHandle );
            }

            set
            {
                LLVMNative.SetGlobalConstant( ValueHandle, value );
            }
        }

        /// <summary>Flag to indicate if this global is stored per thread</summary>
        public bool IsThreadLocal
        {
            get
            {
                return LLVMNative.IsThreadLocal( ValueHandle );
            }

            set
            {
                LLVMNative.SetThreadLocal( ValueHandle, value );
            }
        }

        /// <summary>Initial value for the variable</summary>
        public Constant Initializer
        {
            get
            {
                var handle = LLVMNative.GetInitializer( ValueHandle );
                if( handle.Pointer == IntPtr.Zero )
                    return null;

                return Constant.FromHandle( handle );
            }

            set
            {
                LLVMNative.SetInitializer( ValueHandle, value.ValueHandle );
            }
        }

        /// <summary>Removes the value from its parent module, but does not delete it</summary>
        public void RemoveFromParent()
        {
            LLVMNative.RemoveGlobalFromParent( ValueHandle );
        }

        internal GlobalVariable( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal GlobalVariable( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAGlobalVariable ) )
        {
        }
    }
}
