using System;

namespace Llvm.NET
{
    /// <summary>Target specific code generation information</summary>
    public class TargetMachine : IDisposable
    {
        /// <summary>Retrieves the Target that owns this <see cref="TargetMachine"/></summary>
        public Target Target => Target.FromHandle( NativeMethods.GetTargetMachineTarget( TargetMachineHandle ) );

        /// <summary>Target triple describing this machine</summary>
        public string Triple => NativeMethods.MarshalMsg( NativeMethods.GetTargetMachineTriple( TargetMachineHandle ) );

        /// <summary>CPU Type for this machine</summary>
        public string Cpu => NativeMethods.MarshalMsg( NativeMethods.GetTargetMachineCPU( TargetMachineHandle ) );

        /// <summary>CPU specific features for this machine</summary>
        public string Features => NativeMethods.MarshalMsg( NativeMethods.GetTargetMachineFeatureString( TargetMachineHandle ) );

        /// <summary>Layout information for this machine</summary>
        /// <remarks>
        /// <note type="warning">This property is currently planned for removal in LLVM v4.0.
        /// Though it is unclear what the replacement will be as there is no other way to get
        /// the layout information if it isn't already known.</note>
        /// </remarks>
        public DataLayout TargetData
        {
            get
            {
                var handle = NativeMethods.GetTargetMachineData( TargetMachineHandle );
                if( handle.Pointer == IntPtr.Zero )
                    return null;

                return DataLayout.FromHandle( Context, handle, isDisposable: false );
            }
        }

        /// <summary>Generate code for the target machine from a module</summary>
        /// <param name="module"><see cref="NativeModule"/> to generate the code from</param>
        /// <param name="path">Path to the output file</param>
        /// <param name="fileType">Type of file to emit</param>
        public void EmitToFile( NativeModule module, string path, CodeGenFileType fileType )
        {
            if( module == null )
                throw new ArgumentNullException( nameof( module ) );

            if( string.IsNullOrWhiteSpace( path ) )
                throw new ArgumentException( "Null or empty paths ar not valid", nameof( path ) );

            if( module.TargetTriple != null && Triple != module.TargetTriple )
                throw new ArgumentException( "Triple specifed for the module doesn't match target machine", nameof( module ) );

            IntPtr errMsg;
            if( 0 != NativeMethods.TargetMachineEmitToFile( TargetMachineHandle, module.ModuleHandle, path, (LLVMCodeGenFileType)fileType, out errMsg ).Value )
            {
                var errTxt = NativeMethods.MarshalMsg( errMsg );
                throw new InternalCodeGeneratorException( errTxt );
            }
        }

        public Context Context { get; }

        internal TargetMachine( Context context, LLVMTargetMachineRef targetMachineHandle )
        {
            TargetMachineHandle = targetMachineHandle;
            Context = context;
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
                NativeMethods.DisposeTargetMachine( TargetMachineHandle );
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
