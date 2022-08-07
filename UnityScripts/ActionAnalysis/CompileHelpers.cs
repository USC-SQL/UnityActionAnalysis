using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityActionAnalysis
{
    public static class CompileHelpers
    {

        public static bool Equals(object x, object y)
        {
            object yc = ChangeType(y, x.GetType());
            return x.Equals(yc);
        }

        public static object ChangeType(object val, Type type)
        {
            if (type.IsEnum)
            {
                var enumUnderlying = type.GetEnumUnderlyingType();
                return Enum.ToObject(type, Convert.ChangeType(val, enumUnderlying));
            }
            else
            {
                return Convert.ChangeType(val, type);
            }
        }

        public static double ToDouble(object val)
        {
            return (double)Convert.ChangeType(val, typeof(double));
        }

        public static ulong ToUlong(object val)
        {
            return (ulong)Convert.ChangeType(val, typeof(ulong));
        }

        public static bool ToBool(object val)
        {
            return (bool)Convert.ChangeType(val, typeof(bool));
        }

        public static object IfThenElse(object cond, object trueVal, object falseVal)
        {
            return ToBool(cond) ? trueVal : falseVal;
        }

        public static ulong Xor(ulong a, ulong b)
        {
            return a ^ b;
        }

        public static ulong Shl(ulong a, ulong b)
        {
            return a << (int)b;
        }

        public static ulong Shr(ulong a, ulong b)
        {
            return a >> (int)b;
        }
    }
}
