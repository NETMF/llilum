//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;
    using Microsoft.Zelig.Runtime;
    using Microsoft.Zelig.CodeGeneration.IR.Abstractions;
    using System.IO;
    

    public sealed class ImplementExternalMethods
    {
        //
        // State
        //

        private TypeSystemForCodeTransformation m_typeSystem;

        //
        // Constructor Methods
        //

        public ImplementExternalMethods( TypeSystemForCodeTransformation typeSystem )
        {
            m_typeSystem = typeSystem;
        }

        public static void ResetExternalDataDescriptors()
        {
            ExternalCallContext.Reset( );
        }

        //
        // Helper Methods
        //

        public void Prepare()
        {
            ExternalCallContext.Initialize( m_typeSystem );
            ExternalCallContext.ResetExternalCalls();
            List<MethodRepresentation> importedMethods = new List<MethodRepresentation>();

            foreach(TypeRepresentation td in m_typeSystem.Types)
            {
                //
                // Implement code for external "C/ASM" calls that have been marked as imported references
                //
                foreach(MethodRepresentation md in td.Methods)
                {
                    if(( md.BuildTimeFlags & MethodRepresentation.BuildTimeAttributes.Exported ) != 0)
                    {
                        m_typeSystem.ExportedMethods.Add( md );
                    }
                    else if(( md.BuildTimeFlags & MethodRepresentation.BuildTimeAttributes.Imported ) != 0)
                    {
                        importedMethods.Add( md );
                    }
                }
            }
            foreach(MethodRepresentation md in importedMethods)
            {
                ImplementExternalMethodStub( md, this.m_typeSystem );
            }
        }

        //
        // Helper Methods
        //

        private void ImplementExternalMethodStub( MethodRepresentation md, TypeSystemForCodeTransformation typeSys )
        {
            ControlFlowGraphStateForCodeTransformation cfg = TypeSystemForCodeTransformation.GetCodeForMethod( md );

            if(cfg == null)
            {
                cfg = (ControlFlowGraphStateForCodeTransformation)m_typeSystem.CreateControlFlowGraphState( md );
            }

            if( cfg.Method.CustomAttributes.Length > 0)
            {
                var bb = cfg.FirstBasicBlock;

                CustomAttributeRepresentation ca = md.CustomAttributes[0].CustomAttribute;

                string filePath = ca.FixedArgsValues[1] as string;
                string methodName = ca.FixedArgsValues.Length >= 3 ? ca.FixedArgsValues[2] as string : md.Name;

                filePath = Environment.ExpandEnvironmentVariables( filePath );
                ImportedMethodReferenceAttribute.InteropFileType fileType = (ImportedMethodReferenceAttribute.InteropFileType)ca.FixedArgsValues[0];

                if(!Path.IsPathRooted( filePath ))
                {
                    string tmp = Path.Combine( Directory.GetCurrentDirectory(), filePath );
                    if(File.Exists( tmp ))
                    {
                        filePath = tmp;
                    }
                    else
                    {
                        foreach(string p in typeSys.NativeImportDirectories)
                        {
                            tmp = Path.Combine( p, filePath );

                            if(File.Exists( tmp ))
                            {
                                filePath = tmp;
                                break;
                            }
                        }
                    }
                }


                if(!typeSys.NativeImportLibraries.Contains( filePath.ToLower() ))
                {
                    typeSys.NativeImportLibraries.Add( filePath.ToLower() );
                }

                ExternalCallOperator.IExternalCallContext[] ctxs = ExternalCallContext.Create( fileType, filePath, methodName, bb );

                int len = ctxs.Length;

                for(int i = 0; i < len; i++)
                {
                    ExternalCallOperator op = ExternalCallOperator.New( ctxs[i] );

                    bb.AddOperator( op );

                    op.Context.Owner = op;
                }

                md.RemoveCustomAttribute( ca );
            }
        }
    }
}
