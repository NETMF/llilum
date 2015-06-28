//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;
    using System.Collections.Generic;

    public abstract class MetaDataMethodBase : MetaDataMethodAbstract
    {
        //
        // State
        //

        internal readonly MetaDataTypeDefinitionAbstract   m_owner;
        internal          MethodImplAttributes             m_implFlags;
        internal          MethodAttributes                 m_flags;
        internal          string                           m_name;
        internal          SignatureMethod                  m_signature;

        internal          MetaDataParam[]                  m_paramList;
        internal          SignatureType[]                  m_locals;
        internal          bool                             m_initLocals;

        internal          Instruction[]                    m_instructions;
        internal          EHClause[]                       m_ehTable;
        internal          int                              m_maxStack;

        internal          Debugging.MethodDebugInfo        m_debugInfo;

        //
        // Constructor Methods
        //

        protected MetaDataMethodBase( MetaDataTypeDefinitionAbstract owner ,
                                      int                            token ) : base( token )
        {
            m_owner = owner;
        }

        //--//

        //
        // MetaDataEquality Methods
        //

        protected bool InnerEquals( MetaDataMethodBase other )
        {
            if(m_name  == other.m_name  &&
               m_owner == other.m_owner  )
            {
                return m_signature.Equals( other.m_signature );
            }

            return false;
        }

        protected int InnerGetHashCode()
        {
            return m_name.GetHashCode();
        }

        //
        // Helper Methods
        //

        internal override MetaDataObject MakeUnique()
        {
            return MakeUnique( (IMetaDataUnique)this );
        }

        internal MetaDataObject MakeUnique( IMetaDataUnique obj )
        {
            return m_owner.MakeUnique( obj );
        }

        //--//

        internal bool Match( string          name ,
                             SignatureMethod sig  )
        {
            if(m_name == name)
            {
                return m_signature.Match( sig );
            }

            return false;
        }

        //--//

        //
        // Access Methods
        //

        public MethodImplAttributes ImplFlags
        {
            get
            {
                return m_implFlags;
            }
        }

        public MethodAttributes Flags
        {
            get
            {
                return m_flags;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public SignatureMethod Signature
        {
            get
            {
                return m_signature;
            }
        }

        public MetaDataTypeDefinitionAbstract Owner
        {
            get
            {
                return m_owner;
            }
        }

        public MetaDataParam[] ParamList
        {
            get
            {
                return m_paramList;
            }
        }

        public SignatureType[] Locals
        {
            get
            {
                return m_locals;
            }
        }

        public bool InitLocals
        {
            get
            {
                return m_initLocals;
            }
        }

        public Instruction[] Instructions
        {
            get
            {
                return m_instructions;
            }
        }

        public EHClause[] EHTable
        {
            get
            {
                return m_ehTable;
            }
        }

        public int MaxStack
        {
            get
            {
                return m_maxStack;
            }
        }

        public Debugging.MethodDebugInfo DebugInformation
        {
            get
            {
                return m_debugInfo;
            }
        }

        public override bool UsesTypeParameters
        {
            get
            {
                if(m_owner.UsesTypeParameters) return true;

                return m_signature.UsesTypeParameters;
            }
        }

        public override bool UsesMethodParameters
        {
            get
            {
                if(m_owner.UsesMethodParameters) return true;

                return m_signature.UsesMethodParameters;
            }
        }

        //--//

        public override string FullName
        {
            get
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                PrettyPrintSignature( sb );

                return sb.ToString();
            }
        }

        //--//

        //
        // Debug Methods
        //

        public void PrettyPrintSignature( System.Text.StringBuilder sb )
        {
            if(m_signature != null)
            {
                sb.Append( SignatureMethod.EnumToString( m_signature.m_callingConvention ) );
                sb.Append( m_signature.ReturnType.FullNameWithAbbreviation                 );
            }

            sb.Append( " ["                                                            );
            sb.Append( m_owner.Owner.Name                                              );
            sb.Append( "]"                                                             );
            sb.Append( m_owner.FullName                                                );
            sb.Append( "::"                                                            );
            sb.Append( m_name                                                          );

            if(this is MetaDataMethodGeneric)
            {
                MetaDataMethodGeneric        md2           = (MetaDataMethodGeneric)this;
                MetaDataGenericMethodParam[] genericParams = md2.GenericParams;

                if(genericParams != null)
                {
                    sb.Append( "<" );

                    for(int i = 0; i < genericParams.Length; i++)
                    {
                        MetaDataGenericMethodParam param = genericParams[i];

                        if(i != 0)
                        {
                            sb.Append( ", " );
                        }

                        if(param != null)
                        {
                            MetaDataTypeDefinitionAbstract[] genericParamConstraints = param.GenericParamConstraints;
                            if(genericParamConstraints != null)
                            {
                                sb.Append( "(" );

                                for(int j = 0; j < genericParamConstraints.Length; j++)
                                {
                                    MetaDataTypeDefinitionAbstract td = genericParamConstraints[j];

                                    if(j != 0)
                                    {
                                        sb.Append( ", " );
                                    }

                                    sb.Append( td.FullNameWithAbbreviation );
                                }

                                sb.Append( ")" );
                            }

                            sb.Append( param.Name );
                        }
                        else
                        {
                            sb.AppendFormat( "$Param_{0}", i );
                        }
                    }

                    sb.Append( ">" );
                }
            }

            sb.Append( "(" );

            if(m_signature != null)
            {
                SignatureType[] parameters = m_signature.Parameters;
                for(int i = 0; i < parameters.Length; i++)
                {
                    SignatureType td = parameters[i];

                    if(i != 0)
                    {
                        sb.Append( ", " );
                    }

                    sb.Append( td.FullNameWithAbbreviation );
                }
            }

            sb.Append( ")" );
        }

        //--//

        protected void InnerDump( IMetaDataDumper              writer        ,
                                  MetaDataGenericMethodParam[] genericParams )
        {
            writer = writer.PushContext( this );

            writer.WriteLine( ".method {0} {1}", TokenToString( m_token ), EnumToString( m_flags )                          );
            writer.WriteLine( "        {0}{1}" , DumpSignature( writer, genericParams, false ), EnumToString( m_implFlags ) );

            writer.IndentPush( "{" );

            if(m_customAttributes != null)
            {
                foreach(MetaDataCustomAttribute ca in m_customAttributes)
                {
                    writer.Process( ca, false );
                }
                writer.WriteLine();
            }

            writer.WriteLine( ".maxstack {0}", m_maxStack );

            if(m_locals != null)
            {
                string[] localNames = (m_debugInfo != null) ? m_debugInfo.LocalVarNames : null;

                for(int num = 0; num < m_locals.Length; num++)
                {
                    System.Text.StringBuilder sb  = new System.Text.StringBuilder( ".local " );

                    sb.Append( m_locals[num].ToStringWithAbbreviations( writer ) );

                    if(localNames != null && localNames[num] != null)
                    {
                        sb.AppendFormat( " {0}", localNames[num] );
                    }
                    else
                    {
                        sb.AppendFormat( " V_{0}", num + 1 );
                    }

                    writer.WriteLine( sb.ToString() );
                }
                writer.WriteLine();
            }

            if(m_instructions != null)
            {
                List<EHClause>[] tryBlocksStart = new List<EHClause>[m_instructions.Length];
                List<EHClause>[] tryBlocksEnd   = new List<EHClause>[m_instructions.Length];
                List<EHClause>[] handlersStart  = new List<EHClause>[m_instructions.Length];
                List<EHClause>[] handlersEnd    = new List<EHClause>[m_instructions.Length];

                if(m_ehTable != null)
                {
                    foreach(EHClause eh in m_ehTable)
                    {
                        Set( tryBlocksStart, eh.TryOffset     , eh );
                        Set( tryBlocksEnd  , eh.TryEnd     - 1, eh );
                        Set( handlersStart , eh.HandlerOffset , eh );
                        Set( handlersEnd   , eh.HandlerEnd - 1, eh );
                    }
                }

                int indent = 0;

                for(int i = 0; i < m_instructions.Length; i++)
                {
                    Instruction instr = m_instructions[i];

                    if(tryBlocksStart[i] != null)
                    {
                        EHClause ehLast = null;

                        foreach(EHClause eh in tryBlocksStart[i])
                        {
                            if(ehLast == null || ehLast.TryEnd != eh.TryEnd)
                            {
                                writer.WriteLine( ".try" );
                                writer.IndentPush( "{" );
                                indent++;
                            }

                            ehLast = eh;
                        }
                    }

                    if(handlersStart[i] != null)
                    {
                        if(handlersStart[i].Count > 1)
                        {
                            throw new NotNormalized( "Two EH handlers start at the same position" );
                        }

                        foreach(EHClause eh in handlersStart[i])
                        {
                            switch(eh.Flags)
                            {
                                case EHClause.ExceptionFlag.None:
                                    if(eh.TypeObject != null)
                                    {
                                        writer.WriteLine( ".catch({0})", eh.TypeObject.ToString( writer ) );
                                    }
                                    else
                                    {
                                        writer.WriteLine( ".catch" );
                                    }
                                    break;

                                case EHClause.ExceptionFlag.Finally:
                                    writer.WriteLine( ".finally" );
                                    break;

                                default:
                                    writer.WriteLine( ".filter" );
                                    break;
                            }

                            writer.IndentPush( "{" );
                            indent++;
                        }
                    }

                    Instruction.OpcodeInfo oi = instr.Operator;

                    switch(oi.OperandFormat)
                    {
                        case Instruction.OpcodeOperand.Branch:
                            {
                                writer.WriteLine( "IL_{0:X4}: {1,-10} IL_{2:X4}", i, instr.Operator.Name, instr.Argument );
                            }
                            break;

                        case Instruction.OpcodeOperand.Switch:
                            {
                                writer.WriteLine( "IL_{0:X4}: {1,-10} ( ", i, instr.Operator.Name );

                                foreach(int target in (int[])instr.Argument)
                                {
                                    writer.WriteLine( "                        IL_{0:X4}", target );
                                }

                                writer.WriteLine( "                      )" );
                            }
                            break;

                        case Instruction.OpcodeOperand.Type:
                            {
                                MetaDataTypeDefinitionAbstract obj = (MetaDataTypeDefinitionAbstract)instr.Argument;

                                writer.WriteLine( "IL_{0:X4}: {1,-10} {2}", i, instr.Operator.Name, obj.ToString( writer ) );
                            }
                            break;

                        case Instruction.OpcodeOperand.Method:
                            {
                                MetaDataMethodAbstract obj = (MetaDataMethodAbstract)instr.Argument;

                                writer.WriteLine( "IL_{0:X4}: {1,-10} {2}", i, instr.Operator.Name, obj.ToString( writer ) );
                            }
                            break;

                        case Instruction.OpcodeOperand.Field:
                            {
                                MetaDataFieldAbstract obj = (MetaDataFieldAbstract)instr.Argument;

                                writer.WriteLine( "IL_{0:X4}: {1,-10} {2}", i, instr.Operator.Name, obj.ToString( writer ) );
                            }
                            break;

                        case Instruction.OpcodeOperand.Token:
                            {
                                MetaDataObject obj = (MetaDataObject)instr.Argument;

                                if(obj is MetaDataTypeDefinitionAbstract) goto case Instruction.OpcodeOperand.Type;
                                if(obj is MetaDataMethodAbstract        ) goto case Instruction.OpcodeOperand.Method;
                                if(obj is MetaDataField                 ) goto case Instruction.OpcodeOperand.Field;

                                throw new NotNormalized( "Unvalid token: " + obj );
                            }

                        case Instruction.OpcodeOperand.None:
                            {
                                writer.WriteLine( "IL_{0:X4}: {1,-10}", i, instr.Operator.Name );
                            }
                            break;

                        default:
                            {
                                if(oi.OperandSize == 0) goto case Instruction.OpcodeOperand.None;

                                if(instr.Argument is MetaDataObject)
                                {
                                    throw new NotNormalized( "Unvalid argument: " + instr.Argument );
                                }

                                writer.WriteLine( "IL_{0:X4}: {1,-10} {2}", i, instr.Operator.Name, instr.Argument );
                            }
                            break;
                    }

                    if(handlersEnd[i] != null)
                    {
                        foreach(EHClause eh in handlersEnd[i])
                        {
                            writer.IndentPop( "}" );
                            indent--;
                        }
                    }

                    if(tryBlocksEnd[i] != null)
                    {
                        EHClause ehLast = null;

                        foreach(EHClause eh in tryBlocksEnd[i])
                        {
                            if(ehLast == null || ehLast.TryOffset != eh.TryOffset)
                            {
                                if(ehLast != null)
                                {
                                    throw new NotNormalized( "Two separate try blocks end at the same position" );
                                }

                                writer.IndentPop( "}" );
                                indent--;
                            }

                            ehLast = eh;
                        }
                    }
                }
            }


            writer.IndentPop( "} // end of method '" + this.FullName + "'" );

            writer.WriteLine();
        }

        private void Set( List<EHClause>[] array ,
                          int              pos   ,
                          EHClause         eh    )
        {
            if(array[pos] == null)
            {
                array[pos] = new List<EHClause>();
            }

            array[pos].Add( eh );
        }

        internal string DumpSignature( IMetaDataDumper              writer        ,
                                       MetaDataGenericMethodParam[] genericParams ,
                                       bool                         fWithOwner    )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append( SignatureMethod.EnumToString( m_signature.m_callingConvention ) );
            sb.Append( m_signature.m_returnType.ToStringWithAbbreviations( writer )    );
            sb.Append( " "                                                             );

            if(fWithOwner)
            {
                sb.Append( m_owner.ToString( writer ) );
                sb.Append( "::"                       );
            }

            sb.Append( m_name );

            if(genericParams != null)
            {
                sb.Append( "<" );

                for(int i = 0; i < genericParams.Length; i++)
                {
                    MetaDataGenericMethodParam param = genericParams[i];

                    if(i != 0)
                    {
                        sb.Append( ", " );
                    }

                    if(param.m_genericParamConstraints != null)
                    {
                        sb.Append( "(" );

                        for(int j = 0; j < param.m_genericParamConstraints.Length; j++)
                        {
                            MetaDataTypeDefinitionAbstract td = param.m_genericParamConstraints[j];

                            if(j != 0)
                            {
                                sb.Append( ", " );
                            }

                            sb.Append( td.ToStringWithAbbreviations( writer ) );
                        }

                        sb.Append( ")" );
                    }

                    sb.Append( param.m_name );
                }

                sb.Append( ">" );
            }

            sb.Append( "("    );

            for(int i = 0; i < m_signature.m_parameters.Length; i++)
            {
                SignatureType td = m_signature.m_parameters[i];

                if(i != 0)
                {
                    sb.Append( ", " );
                }

                sb.Append( td.ToStringWithAbbreviations( writer ) );

                if(m_paramList != null && i < m_paramList.Length)
                {
                    sb.Append( " "                 );
                    sb.Append( m_paramList[i].Name );
                }
            }

            sb.Append( ")" );

            return sb.ToString();
        }

        internal string InnerToString( IMetaDataDumper              writer        ,
                                       MetaDataGenericMethodParam[] genericParams )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            writer = writer.PushContext( this    );
            writer = writer.PushContext( m_owner );

            sb.Append( SignatureMethod.EnumToString( m_signature.m_callingConvention ) );
            sb.Append( m_signature.m_returnType.ToStringWithAbbreviations( writer )    );
            sb.Append( " "                                                             );
            sb.Append( m_owner.ToString( writer )                                      );
            sb.Append( "::"                                                            );
            sb.Append( m_name                                                          );

            if(genericParams != null)
            {
                sb.Append( "<" );

                for(int i = 0; i < genericParams.Length; i++)
                {
                    MetaDataGenericMethodParam param = genericParams[i];

                    if(i != 0)
                    {
                        sb.Append( ", " );
                    }

                    if(param.m_genericParamConstraints != null)
                    {
                        sb.Append( "(" );

                        for(int j = 0; j < param.m_genericParamConstraints.Length; j++)
                        {
                            MetaDataTypeDefinitionAbstract td = param.m_genericParamConstraints[j];

                            if(j != 0)
                            {
                                sb.Append( ", " );
                            }

                            sb.Append( td.ToStringWithAbbreviations( writer ) );
                        }

                        sb.Append( ")" );
                    }

                    sb.Append( param.m_name );
                }

                sb.Append( ">" );
            }

            sb.Append( "("    );

            for(int i = 0; i < m_signature.m_parameters.Length; i++)
            {
                SignatureType td = m_signature.m_parameters[i];

                if(i != 0)
                {
                    sb.Append( ", " );
                }

                sb.Append( td.ToStringWithAbbreviations( writer ) );

                if(m_paramList != null && i < m_paramList.Length)
                {
                    sb.Append( " "                 );
                    sb.Append( m_paramList[i].Name );
                }
            }

            sb.Append( ") "                     );
            sb.Append( TokenToString( m_token ) );

            return sb.ToString();
        }

        private static string EnumToString( MethodAttributes val )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if((val & MethodAttributes.MemberAccessMask) == MethodAttributes.Public      ) sb.Append( "public "        );
            if((val & MethodAttributes.MemberAccessMask) == MethodAttributes.Private     ) sb.Append( "private "       );
            if((val & MethodAttributes.MemberAccessMask) == MethodAttributes.Family      ) sb.Append( "family "        );
            if((val & MethodAttributes.MemberAccessMask) == MethodAttributes.Assem       ) sb.Append( "assembly "      );
            if((val & MethodAttributes.MemberAccessMask) == MethodAttributes.FamANDAssem ) sb.Append( "famandassem "   );
            if((val & MethodAttributes.MemberAccessMask) == MethodAttributes.FamORAssem  ) sb.Append( "famorassem "    );
            if((val & MethodAttributes.MemberAccessMask) == MethodAttributes.PrivateScope) sb.Append( "privatescope "  );
            if((val & MethodAttributes.HideBySig       ) != 0                            ) sb.Append( "hidebysig "     );
            if((val & MethodAttributes.VtableLayoutMask) == MethodAttributes.NewSlot     ) sb.Append( "newslot "       );
            if((val & MethodAttributes.SpecialName     ) != 0                            ) sb.Append( "specialname "   );
            if((val & MethodAttributes.RTSpecialName   ) != 0                            ) sb.Append( "rtspecialname " );
            if((val & MethodAttributes.Static          ) != 0                            ) sb.Append( "static "        );
            if((val & MethodAttributes.Abstract        ) != 0                            ) sb.Append( "abstract "      );
            if((val & MethodAttributes.Strict          ) != 0                            ) sb.Append( "strict "        );
            if((val & MethodAttributes.Virtual         ) != 0                            ) sb.Append( "virtual "       );
            if((val & MethodAttributes.Final           ) != 0                            ) sb.Append( "final "         );
            if((val & MethodAttributes.UnmanagedExport ) != 0                            ) sb.Append( "unmanagedexp "  );
            if((val & MethodAttributes.RequireSecObject) != 0                            ) sb.Append( "reqsecobj "     );

            return sb.ToString();
        }

        private static string EnumToString( MethodImplAttributes val )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if((val & MethodImplAttributes.CodeTypeMask) == MethodImplAttributes.Native   ) sb.Append( " native"        );
            if((val & MethodImplAttributes.CodeTypeMask) == MethodImplAttributes.IL       ) sb.Append( " cil"           );
            if((val & MethodImplAttributes.CodeTypeMask) == MethodImplAttributes.OPTIL    ) sb.Append( " optil"         );
            if((val & MethodImplAttributes.CodeTypeMask) == MethodImplAttributes.Runtime  ) sb.Append( " runtime"       );
            if((val & MethodImplAttributes.ManagedMask ) == MethodImplAttributes.Unmanaged) sb.Append( " unmanaged"     );
            if((val & MethodImplAttributes.ManagedMask ) == MethodImplAttributes.Managed  ) sb.Append( " managed"       );
            if((val & MethodImplAttributes.PreserveSig ) != 0                             ) sb.Append( " preservesig"   );
            if((val & MethodImplAttributes.ForwardRef  ) != 0                             ) sb.Append( " forwardref"    );
            if((val & MethodImplAttributes.InternalCall) != 0                             ) sb.Append( " internalcall"  );
            if((val & MethodImplAttributes.Synchronized) != 0                             ) sb.Append( " synchronized"  );
            if((val & MethodImplAttributes.NoInlining  ) != 0                             ) sb.Append( " noinlining"    );

            return sb.ToString();
        }
    }
}
