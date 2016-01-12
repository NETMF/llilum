//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using System;
using Llvm.NET.Values;
using Microsoft.Zelig.LLVM;
using Microsoft.Zelig.CodeGeneration.IR;

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures
{
    internal class ValueCache
    {
        private GrowOnlyHashTable<_BasicBlock, Value> m_loadedValues;

        public ValueCache(VariableExpression expression, _Type type)
        {
            Expression = expression;
            Type = type;
            m_loadedValues = HashTableFactory.New<_BasicBlock, Value>();
        }

        public ValueCache(VariableExpression expression, Value address) :
            this(expression, address.GetUnderlyingType())
        {
            Address = address;
        }

        public VariableExpression Expression { get; }

        public _Type Type { get; }

        public Value Address { get; }

        public bool IsAddressable
        {
            get
            {
                return Address != null;
            }
        }

        public Value GetValueFromBlock(_BasicBlock block)
        {
            Value value;
            if (m_loadedValues.TryGetValue(block, out value))
            {
                return value;
            }

            return null;
        }

        public void SetValueForBlock(_BasicBlock block, Value value)
        {
            m_loadedValues[block] = value;
        }
    }
}
