//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.Abstractions
{
    using System;

    using Microsoft.Zelig.Runtime;
    using Microsoft.Zelig.CodeGeneration.IR.ExternalMethodImporters;
    using Microsoft.Zelig.TargetModel.ArmProcessor;


    public class ExternalCallContext
    {
        public static void Initialize(TypeSystemForCodeTransformation typeSystem)
        {
            if (typeSystem.PlatformAbstraction.PlatformName == InstructionSetVersion.Platform_ARM)
            {
                ArmElfExternalCallContext.Initialize(typeSystem);
            }
            else
            {
                throw new ArgumentException("Unknown platform");
            }
        }

        public static void Reset()
        {
            ArmElfExternalCallContext.Reset();
        }

        public static void ResetExternalCalls()
        {
            ArmElfExternalCallContext.ResetExternCalls();
        }

        public static ExternalCallOperator.IExternalCallContext[] Create( ImportedMethodReferenceAttribute.InteropFileType fileType, string filePath, string methodName, BasicBlock owner )
        {
            switch(fileType)
            {
                case ImportedMethodReferenceAttribute.InteropFileType.ArmELF:
                    return ArmElfExternalCallContext.Load( filePath, methodName, owner );
            }

            throw new NotImplementedException();
        }
    }
}
