using System;
using Llvm.NET.Values;
using Llvm.NET.Types;

namespace Microsoft.Zelig.LLVM
{
    public static class ValueExtensions
    {
        private static readonly string TypeName = "Type";

        public static void SetDebugType(this Value value, _Type type)
        {
            value.AddExtendedPropertyValue(TypeName, type);
        }

        public static _Type GetDebugType(this Value value)
        {
            _Type type;
            if (!value.TryGetExtendedPropertyValue(TypeName, out type))
            {
                return null;
            }

            return type;
        }

        public static _Type GetUnderlyingType(this Value value)
        {
            return value.GetDebugType().UnderlyingType;
        }

        public static bool IsAnUninitializedGlobal(this Value value)
        {
            var gv = value as GlobalVariable;
            if (gv == null)
            {
                return false;
            }

            return gv.Initializer == null;
        }

        public static void SetGlobalInitializer(this Value value, Constant constVal)
        {
            var gv = value as GlobalVariable;
            if (gv != null)
            {
                gv.Initializer = constVal;
            }
        }

        public static void MergeToAndRemove(this Value value, Value targetVal)
        {
            GlobalVariable gv = value as GlobalVariable;
            if (gv != null)
            {
                gv.ReplaceAllUsesWith(targetVal);
                gv.RemoveFromParent();
            }
        }
    }
}
