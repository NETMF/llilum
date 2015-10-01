using System;

namespace Llvm.NET
{
    public class TargetMachine : IDisposable
    {

        public Target Target => Target.FromHandle( LLVMNative.GetTargetMachineTarget( TargetMachineHandle ) );
        public string Triple => LLVMNative.MarshalMsg( LLVMNative.GetTargetMachineTriple( TargetMachineHandle ) );
        public string Cpu => LLVMNative.MarshalMsg( LLVMNative.GetTargetMachineCPU( TargetMachineHandle ) );
        public string Features => LLVMNative.MarshalMsg( LLVMNative.GetTargetMachineFeatureString( TargetMachineHandle ) );
        public TargetData TargetData => TargetData.FromHandle( LLVMNative.GetTargetMachineData( TargetMachineHandle), isDisposable: false );

        public void EmitToFile( Module module, string path, CodeGenFileType fileType )
        {
            if( module == null )
                throw new ArgumentNullException( nameof( module ) );

            if( string.IsNullOrWhiteSpace( path ) )
                throw new ArgumentException( "Null or empty paths ar not valid", nameof( path ) );

            if( module.TargetTriple != null && Triple != module.TargetTriple )
                throw new ArgumentException( "Triple specifed for the module doesn't match target machine", nameof( module ) );

            IntPtr errMsg;
            if( 0 != LLVMNative.TargetMachineEmitToFile( TargetMachineHandle, module.ModuleHandle, path, (LLVMCodeGenFileType)fileType, out errMsg ).Value )
            {
                var errTxt = LLVMNative.MarshalMsg( errMsg );
                throw new InternalCodeGeneratorException( errTxt );
            }
        }

        internal TargetMachine( LLVMTargetMachineRef targetMachineHandle )
        {
            TargetMachineHandle = targetMachineHandle;
        }
       
        #region IDisposable Support
        private bool IsDisposed => TargetMachineHandle.Pointer == IntPtr.Zero;

        protected virtual void Dispose( bool disposing )
        {
            if( !IsDisposed )
            {
                // no managed state to dispose here
                //if( disposing )
                //{
                //    // dispose managed state (managed objects).
                //}
                LLVMNative.DisposeTargetMachine( TargetMachineHandle );
                TargetMachineHandle = default( LLVMTargetMachineRef );
            }
        }

        ~TargetMachine( )
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose( false );
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose( )
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose( true );
            GC.SuppressFinalize(this);
        }
        #endregion

        internal LLVMTargetMachineRef TargetMachineHandle { get; private set; }

    }
}
