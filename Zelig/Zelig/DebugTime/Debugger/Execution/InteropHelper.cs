//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.IO;
    using System.Windows.Forms;
    using System.Threading;

    using EncDef             = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition;
    using InstructionSet     = Microsoft.Zelig.TargetModel.ArmProcessor.InstructionSet;
    using IR                 = Microsoft.Zelig.CodeGeneration.IR;
    using RT                 = Microsoft.Zelig.Runtime;
    using TS                 = Microsoft.Zelig.Runtime.TypeSystem;


    public class InteropHelper
    {
        //
        // State
        //

        private ImageInformation                   m_imageInformation;
        private Emulation.Hosting.AbstractHost     m_host;

        private IR.TypeSystemForCodeTransformation m_typeSystem;

        //
        // Constructor Methods
        //

        public InteropHelper( ImageInformation               imageInformation ,
                              Emulation.Hosting.AbstractHost host             )
        {
            m_imageInformation = imageInformation;
            m_host             = host;
            m_typeSystem       = imageInformation.TypeSystem;
        }

        //
        // Helper Methods
        //

        public void SetInteropOnWellKnownMethod( string                             name            ,
                                                 bool                               fPostProcessing ,
                                                 Emulation.Hosting.Interop.Callback ftn             )
        {
            TS.MethodRepresentation md = m_typeSystem.GetWellKnownMethodNoThrow( name );

            if(md is TS.VirtualMethodRepresentation)
            {
                for(TS.TypeRepresentation td = m_typeSystem.FindSingleConcreteImplementation( md.OwnerType ); td != null; td = td.Extends)
                {
                    TS.MethodRepresentation md2 = td.FindMatch( md, null );

                    if(md2 != null)
                    {
                        md = md2;
                        break;
                    }
                }
            }

            if(md != null)
            {
                IR.ImageBuilders.SequentialRegion reg = m_imageInformation.ResolveMethodToRegion( md );
                if(reg != null)
                {
                    Emulation.Hosting.Interop svc;

                    if(m_host.GetHostingService( out svc ))
                    {
                        svc.SetInterop( reg.ExternalAddress, true, fPostProcessing, ftn );
                    }
                }
            }
        }

        public bool SetInteropOnAddress( uint                               pc  ,
                                         Emulation.Hosting.Interop.Callback ftn )
        {
            Emulation.Hosting.Interop svc;

            if(m_host.GetHostingService( out svc ))
            {
                svc.SetInterop( pc, true, false, ftn );

                return true;
            }

            return false;
        }

        public bool SetTemporaryInteropOnReturn( Emulation.Hosting.Interop.Callback ftn )
        {
            Emulation.Hosting.Interop svc;

            if(m_host.GetHostingService( out svc ))
            {
                uint lr = GetRegisterUInt32( EncDef.c_register_lr );
                uint sp = GetRegisterUInt32( EncDef.c_register_sp );

                svc.SetInterop( lr, true, false, delegate()
                {
                    if(sp == GetRegisterUInt32( EncDef.c_register_sp ))
                    {
                        return ftn() | Emulation.Hosting.Interop.CallbackResponse.RemoveDetour;
                    }

                    return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
                } );

                return true;
            }

            return false;
        }

        public Emulation.Hosting.Interop.CallbackResponse SkipCall()
        {
            uint pc = GetRegisterUInt32( EncDef.c_register_lr );

            Emulation.Hosting.ProcessorStatus svc; m_host.GetHostingService( out svc );

            svc.ProgramCounter = pc;

            svc.RaiseExternalProgramFlowChange();

            return Emulation.Hosting.Interop.CallbackResponse.NextInstruction;
        }

        public uint GetRegisterUInt32( uint idx )
        {
            Emulation.Hosting.ProcessorStatus svc; m_host.GetHostingService( out svc );

            var bb = svc.GetRegister( m_imageInformation.TypeSystem.PlatformAbstraction.GetRegisterForEncoding( idx ) );

            return bb != null ? bb.ReadUInt32( 0 ) : 0;
        }
    }
}
