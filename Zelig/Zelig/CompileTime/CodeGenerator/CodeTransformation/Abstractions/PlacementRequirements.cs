//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.Abstractions
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.MetaData;
    using Microsoft.Zelig.MetaData.Normalized;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class PlacementRequirements
    {
        //
        // State
        //

        private bool                       m_fContentsUninitialized;
        private bool                       m_fAllocateFromHighAddress;
        private uint                       m_alignment;
        private int                        m_alignmentOffset;
        private string[]                   m_sections;
        private Runtime.MemoryUsage[]      m_usages;
        private Runtime.MemoryAttributes[] m_memoryKinds;

        //
        // Constructor Methods
        //

        public PlacementRequirements( uint alignment       ,
                                      int  alignmentOffset )
        {
            m_alignment       = alignment;
            m_alignmentOffset = alignmentOffset;
        }

        //
        // Helper Methods
        //

        public void ApplyTransformation( TransformationContextForCodeTransformation context )
        {
            context.Push( this );

            context.Transform( ref m_fContentsUninitialized   );
            context.Transform( ref m_fAllocateFromHighAddress );
            context.Transform( ref m_alignment                );
            context.Transform( ref m_alignmentOffset          );
            context.Transform( ref m_sections                 );
            context.Transform( ref m_usages                   );
            context.Transform( ref m_memoryKinds              );

            context.Pop();
        }

        public PlacementRequirements Clone()
        {
            PlacementRequirements pr = new PlacementRequirements( m_alignment, m_alignmentOffset );

            pr.m_sections    = m_sections;
            pr.m_usages      = m_usages;
            pr.m_memoryKinds = m_memoryKinds;

            return pr;
        }

        public void AddConstraint( string section )
        {
            if(section != null)
            {
                m_sections = ArrayUtility.AddUniqueToArray( m_sections, section );
            }
        }

        public void AddConstraint( Runtime.MemoryUsage usage )
        {
            m_usages = ArrayUtility.AppendToArray( m_usages, usage );
        }

        public void AddConstraint( Runtime.MemoryAttributes memoryKind )
        {
            m_memoryKinds = ArrayUtility.AppendToArray( m_memoryKinds, memoryKind );
        }

        //--//

        public bool IsCompatible( PlacementRequirements pr )
        {
            if(pr != null)
            {
                if(Object.ReferenceEquals( this, pr ))
                {
                    return true;
                }

                if(                           this.m_alignment       == pr.m_alignment         &&
                                              this.m_alignmentOffset == pr.m_alignmentOffset   &&
                   ArrayUtility.SameContents( this.m_sections        ,  pr.m_sections        ) &&
                   ArrayUtility.SameContents( this.m_usages          ,  pr.m_usages          ) &&
                   ArrayUtility.SameContents( this.m_memoryKinds     ,  pr.m_memoryKinds     )  )
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsCompatible( string                   sectionName ,
                                  Runtime.MemoryAttributes memoryKind  ,
                                  Runtime.MemoryUsage      memoryUsage )
        {
            if(m_sections != null)
            {
                bool fGot = false;

                foreach(string name in m_sections)
                {
                    if(sectionName == name)
                    {
                        fGot = true;
                        break;
                    }
                }

                if(fGot == false)
                {
                    return false;
                }
            }

            if(m_usages != null)
            {
                bool fGot = false;

                foreach(Runtime.MemoryUsage usage in m_usages)
                {
                    if((memoryUsage & usage) == usage)
                    {
                        fGot = true;
                        break;
                    }
                }

                if(fGot == false)
                {
                    return false;
                }
            }

            if(m_memoryKinds != null)
            {
                bool fGot = false;

                foreach(Runtime.MemoryAttributes kind in m_memoryKinds)
                {
                    if((memoryKind & kind) == kind)
                    {
                        fGot = true;
                        break;
                    }
                }

                if(fGot == false)
                {
                    return false;
                }
            }

            return true;
        }


        //
        // Access Methods
        //

        public uint Alignment
        {
            get
            {
                return m_alignment;
            }

            set
            {
                m_alignment = value;
            }
        }

        public int AlignmentOffset
        {
            get
            {
                return m_alignmentOffset;
            }

            set
            {
                m_alignmentOffset = value;
            }
        }

        public string[] Sections
        {
            get
            {
                return m_sections;
            }
        }

        public Runtime.MemoryUsage[] Usages
        {
            get
            {
                return m_usages;
            }
        }

        public Runtime.MemoryAttributes[] MemoryKinds
        {
            get
            {
                return m_memoryKinds;
            }
        }

        public bool ContentsUninitialized
        {
            get
            {
                return m_fContentsUninitialized;
            }

            set
            {
                m_fContentsUninitialized = value;
            }
        }

        public bool AllocateFromHighAddress
        {
            get
            {
                return m_fAllocateFromHighAddress;
            }

            set
            {
                m_fAllocateFromHighAddress = value;
            }
        }
    }
}
