//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.ImageBuilders
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;


    public partial class CompilationState
    {
        //
        // State
        //

        protected          SequentialRegion          m_activeCodeRegion;
        protected          SequentialRegion.Section  m_activeCodeSection;
        protected          Runtime.HardwareException m_activeHardwareException;
        protected          bool                      m_fStopProcessingOperatorsForCurrentBasicBlock;
        private   readonly Queue< Operator >         m_contextOperators = new Queue< Operator >();

        //
        // Helper Methods
        //

        public ImageAnnotation AddNewImageAnnotation( object val )
        {
            return AddNewImageAnnotation( 0, val );
        }

        public ImageAnnotation AddNewImageAnnotation( uint   size ,
                                                      object val  )
        {
            return m_activeCodeSection.AddImageAnnotation( size, val );
        }

        protected void AddOperatorContext( Operator op )
        {
            m_contextOperators.Enqueue( op );
        }

        protected void FlushOperatorContext()
        {
            while(m_contextOperators.Count > 0)
            {
                AddNewImageAnnotation( m_contextOperators.Dequeue() );
            }
        }

        protected static object NormalizeValue( object o )
        {
            if(o is ConstantExpression)
            {
                ConstantExpression ex = (ConstantExpression)o;

                o = ex.Value;
                
                ControlFlowGraphStateForCodeTransformation.ValueFragment frag;

                frag = o as ControlFlowGraphStateForCodeTransformation.ValueFragment;
                if(frag != null)
                {
                    o = frag.Value;
                }
            }

            return o;
        }

        protected static bool GetValue(     object o   ,
                                        out int    val )
        {
            o = NormalizeValue( o );

            if(o == null)
            {
                val = 0;
            }
            else if(o is UIntPtr)
            {
                val = (int)((UIntPtr)o).ToUInt32();
            }
            else if(o is IntPtr)
            {
                val = ((IntPtr)o).ToInt32();
            }
            else if(o is int)
            {
                val = (int)o;
            }
            else if(o is uint)
            {
                val = (int)(uint)o;
            }
            else if(o is short)
            {
                val = (int)(short)o;
            }
            else if(o is ushort)
            {
                val = (int)(uint)(ushort)o;
            }
            else if(o is sbyte)
            {
                val = (int)(sbyte)o;
            }
            else if(o is byte)
            {
                val = (int)(uint)(byte)o;
            }
            else if(o is bool)
            {
                val = (bool)o ? 1 : 0;
            }
            else
            {
                val = 0;
                return false;
            }

            return true;
        }

        //--//

        protected void RecordAdjacencyNeed( Operator   opSource ,
                                            BasicBlock bbTarget )
        {
            m_owner.RecordAdjacencyNeed( opSource, bbTarget );
        }

        protected Core.BranchEncodingLevel GetEncodingLevelForBranch( Operator   opSource     ,
                                                                      BasicBlock bb           ,
                                                                      bool       fConditional )
        {
            return m_owner.GetEncodingLevelForBranch( opSource, bb, fConditional );
        }

        protected Core.ConstantAddressEncodingLevel GetEncodingLevelForConstant( Operator opSource )
        {
            return m_owner.GetEncodingLevelForConstant( opSource );
        }

        //--//

        protected ImageAnnotation TrackVariable( VariableExpression var    ,
                                                 bool               fAlive )
        {
            List< ImageAnnotation > annotationList = m_activeCodeSection.Context.AnnotationList;

            for(int idx = annotationList.Count; --idx >= 0; )
            {
                TrackVariableLifetime tvl = annotationList[idx] as TrackVariableLifetime;

                if(tvl != null && tvl.Target == var)
                {
                    if(tvl.IsAlive == fAlive)
                    {
                        //
                        // No change in liveness, keep going.
                        //
                        return tvl;
                    }

                    break;
                }
            }

            return new TrackVariableLifetime( m_activeCodeSection.Context, m_activeCodeSection.Offset, var, fAlive );
        }

        //--//

        //
        // Debug Methods
        //

        public Exception NotImplemented()
        {
            using(System.IO.StreamWriter output = new System.IO.StreamWriter( "CompilationException.txt", false, System.Text.Encoding.ASCII ))
            {
                m_cfg.DumpToStream( output ); 

                DumpOpcodes( output );
            }

            return new NotImplementedException();
        }

        protected abstract void DumpOpcodes( System.IO.TextWriter textWriter );
    }
}