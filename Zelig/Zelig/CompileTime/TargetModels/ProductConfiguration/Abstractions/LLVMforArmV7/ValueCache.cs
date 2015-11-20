//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using System;
using Microsoft.Zelig.LLVM;
using Microsoft.Zelig.CodeGeneration.IR;

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures
{
    internal class ValueCache
    {
        private GrowOnlyHashTable<_BasicBlock, _Value> m_loadedValues;

        public ValueCache(VariableExpression expression, _Type type)
        {
            Expression = expression;
            Type = type;
            m_loadedValues = HashTableFactory.New<_BasicBlock, _Value>();
        }

        public ValueCache(VariableExpression expression, _Value address) :
            this(expression, address.Type.UnderlyingType)
        {
            Address = address;
        }

        public VariableExpression Expression { get; }

        public _Type Type { get; }

        public _Value Address { get; }

        public bool IsAddressable
        {
            get
            {
                return Address != null;
            }
        }

        public _Value GetValueFromBlock(_BasicBlock block)
        {
            _Value value;
            if (m_loadedValues.TryGetValue(block, out value))
            {
                return value;
            }

            return null;
        }

        public void SetValueForBlock(_BasicBlock block, _Value value)
        {
            m_loadedValues[block] = value;
        }
    }
}
