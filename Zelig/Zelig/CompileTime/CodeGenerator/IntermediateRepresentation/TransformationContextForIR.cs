//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class TransformationContextForIR : TransformationContext
    {
        public abstract void Transform( ref ControlFlowGraphState                 cfg      );
                                                                                            
        public abstract void Transform( ref Operator                              op       );
        public abstract void Transform( ref Operator[]                            opArray  );
        public abstract void Transform( ref Annotation                            an       );
        public abstract void Transform( ref Annotation[]                          anArray  );
                                                                                          
        public abstract void Transform( ref Expression                            ex       );
        public abstract void Transform( ref ConstantExpression                    ex       );
        public abstract void Transform( ref VariableExpression                    ex       );
        public abstract void Transform( ref VariableExpression.DebugInfo          val      );
        public abstract void Transform( ref Expression[]                          exArray  );
        public abstract void Transform( ref VariableExpression[]                  exArray  );
        public abstract void Transform( ref List< ConstantExpression >            exLst    );
                                                                                          
        public abstract void Transform( ref BasicBlock                            bb       );
        public abstract void Transform( ref EntryBasicBlock                       bb       );
        public abstract void Transform( ref ExitBasicBlock                        bb       );
        public abstract void Transform( ref ExceptionHandlerBasicBlock            bb       );
        public abstract void Transform( ref BasicBlock[]                          bbArray  );
        public abstract void Transform( ref ExceptionHandlerBasicBlock[]          bbArray  );
        public abstract void Transform( ref BasicBlock.Qualifier                  val      );
                                                                                          
        public abstract void Transform( ref ExceptionClause                       ec       );
        public abstract void Transform( ref ExceptionClause[]                     ecArray  );
        public abstract void Transform( ref ExceptionClause.ExceptionFlag         val      );
                                                                                          
        public abstract void Transform( ref CompilationConstraints                val      );
        public abstract void Transform( ref CompilationConstraints[]              valArray );
        public abstract void Transform( ref Operator.OperatorCapabilities         val      );
        public abstract void Transform( ref Operator.OperatorLevel                val      );
        public abstract void Transform( ref AbstractBinaryOperator.ALU            val      );
        public abstract void Transform( ref AbstractUnaryOperator.ALU             val      );
        public abstract void Transform( ref CallOperator.CallKind                 val      );
        public abstract void Transform( ref CompareAndSetOperator.ActionCondition val      );
    }
}
