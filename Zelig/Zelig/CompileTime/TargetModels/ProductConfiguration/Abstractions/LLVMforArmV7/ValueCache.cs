//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using System;
using Llvm.NET.Values;
using Microsoft.Zelig.LLVM;
using IR = Microsoft.Zelig.CodeGeneration.IR;

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures
{
    internal class ValueCache
    {
        private GrowOnlyHashTable<IR.BasicBlock, Value> m_loadedValues;

        public ValueCache(IR.VariableExpression expression, _Type type)
        {
            Expression = expression;
            Type = type;
            m_loadedValues = HashTableFactory.New<IR.BasicBlock, Value>();
        }

        public ValueCache(IR.VariableExpression expression, Value address) :
            this(expression, address.GetUnderlyingType())
        {
            Address = address;
        }

        public IR.VariableExpression Expression { get; }

        public _Type Type { get; }

        public Value Address { get; }

        public bool IsAddressable
        {
            get
            {
                return Address != null;
            }
        }

        public Value GetValueFromBlock(IR.BasicBlock block)
        {
            Value value;
            if (m_loadedValues.TryGetValue(block, out value))
            {
                return value;
            }

            return null;
        }

        public void SetValueForBlock(IR.BasicBlock block, Value value)
        {
            m_loadedValues[block] = value;
        }
    }
}
