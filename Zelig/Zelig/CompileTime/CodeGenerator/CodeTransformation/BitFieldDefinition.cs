//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using Microsoft.Zelig.MetaData;
    using Microsoft.Zelig.MetaData.Normalized;

    using Microsoft.Zelig.Runtime.TypeSystem;


    [AllowCompileTimeIntrospection]
    public sealed class BitFieldDefinition
    {
        [AllowCompileTimeIntrospection]
        public sealed class Section
        {
            //
            // State
            //

            public uint                     Position;
            public uint                     Size;
            public uint                     Offset;
            public Runtime.BitFieldModifier Modifiers;
            public uint                     ReadsAs;
            public uint                     WritesAs;
        }

        //
        // State
        //

        public static readonly Section[] SharedEmptyArray = new Section[0];

        private Section[] m_sections = SharedEmptyArray;

        //
        // Helper Methods
        //

        public void AddSection( Section sec )
        {
            int i = m_sections.Length;
            
            while(i > 0)
            {
                var sec2 = m_sections[i-1];

                if(sec2.Offset < sec.Offset)
                {
                    break;
                }

                i--;
            }

            m_sections = ArrayUtility.InsertAtPositionOfNotNullArray( m_sections, i, sec );
        }

        //
        // Access Methods
        //

        public Section[] Sections
        {
            get
            {
                return m_sections;
            }
        }

        public uint TotalSize
        {
            get
            {
                uint size = 0;

                foreach(var sec in m_sections)
                {
                    size += sec.Size;
                }

                return size;
            }
        }
    }
}
