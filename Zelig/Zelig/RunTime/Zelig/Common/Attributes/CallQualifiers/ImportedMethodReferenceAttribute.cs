//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_ImportedMethodReferenceAttribute" )]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ImportedMethodReferenceAttribute : Attribute
    {
        internal InteropFileType m_type;
        internal string          m_file;
        internal string          m_methodName;

        public enum InteropFileType
        {
            ArmELF,
            UserDefinedFlag = 0x80000
        }

        private ImportedMethodReferenceAttribute()
        {
        }

        public ImportedMethodReferenceAttribute(InteropFileType importFileType, string fileName) :
            this( importFileType, fileName, null )
        {
        }

        public ImportedMethodReferenceAttribute( InteropFileType importFileType, string fileName, string methodName )
        {
            m_type = importFileType;
            m_file = fileName;
            m_methodName = methodName;
        }
    }
}


