using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace UnityActionAnalysis
{
    public enum SymexValueType
    {
        StringConstant = 1,
        Object = 2,
        BitVecConstant = 3,
        Variable = 4,
        Struct = 5,
        UnevaluatedMethodCall = 6,
        Unknown = 7
    }

    public abstract class SymexValue
    {
        public abstract SymexValueType GetValueType();

        public const int TYPE_STRCONST = 1;
        public const int TYPE_OBJECT = 2;
        public const int TYPE_BVCONST = 3;
        public const int TYPE_VARIABLE = 4;
        public const int TYPE_STRUCT = 5;
        public const int TYPE_METHODCALL = 6;
        public const int TYPE_UNKNOWN = 7;

        private static SymexValue ParseInternal(JObject o)
        {
            int type = o["type"].ToObject<int>();
            switch (type)
            {
                case TYPE_STRCONST:
                    return new SymexStringConstantValue(o["value"].ToObject<string>());
                case TYPE_OBJECT:
                    {
                        var jsonValue = o["value"];
                        if (jsonValue is JObject)
                        {
                            Dictionary<string, SymexValue> value = new Dictionary<string, SymexValue>();
                            JObject obj = (JObject)jsonValue;
                            foreach (var p in obj)
                            {
                                value.Add(p.Key, ParseInternal((JObject)p.Value));
                            }
                            Type objectType = Type.GetType(o["objectType"].ToObject<string>());
                            if (o.ContainsKey("symbolName"))
                            {
                                return new SymexObjectValue(value, objectType, o["symbolName"].ToObject<string>());
                            }
                            else
                            {
                                return new SymexObjectValue(value, objectType);
                            }
                        }
                        else
                        {
                            return new SymexObjectValue(null, null);
                        }
                    }
                case TYPE_BVCONST:
                    return new SymexBitVecConstantValue(o["value"].ToObject<ulong>());
                case TYPE_VARIABLE:
                    {
                        string varName = o["name"].ToObject<string>();
                        return new SymexVariableValue(varName);
                    }
                case TYPE_STRUCT:
                    {
                        Dictionary<string, SymexValue> value = new Dictionary<string, SymexValue>();
                        var jsonValue = o["value"];
                        JObject obj = (JObject)jsonValue;
                        foreach (var p in obj)
                        {
                            value.Add(p.Key, ParseInternal((JObject)p.Value));
                        }
                        string structTypeName = o["structType"].ToObject<string>();
                        Type structType = Type.GetType(structTypeName);
                        return new SymexStructValue(value, structType);
                    }
                case TYPE_METHODCALL:
                    {
                        var method = SymexHelpers.GetMethodFromSignature(o["method"].ToObject<string>());
                        List<SymexValue> args = new List<SymexValue>();
                        JArray arr = (JArray)o["arguments"];
                        foreach (var elem in arr)
                        {
                            args.Add(ParseInternal((JObject)elem));
                        }
                        return new SymexUnevaluatedMethodCallValue(method, args);
                    }
                case TYPE_UNKNOWN:
                    return new SymexUnknownValue();
                default:
                    throw new ArgumentException("unexpected type " + type);
            }
        }

        public static SymexValue Parse(string s)
        {
            return ParseInternal(JObject.Parse(s));
        }
    }

    public class SymexStringConstantValue : SymexValue
    {
        public readonly string value;

        public SymexStringConstantValue(string value)
        {
            this.value = value;
        }

        public override SymexValueType GetValueType()
        {
            return SymexValueType.StringConstant;
        }

        public override string ToString()
        {
            return "\"" + value + "\"";
        }
    }

    public class SymexObjectValue : SymexValue
    {
        public readonly Dictionary<string, SymexValue> value;
        public readonly Type objectType;
        public readonly string symbolName;

        public SymexObjectValue(Dictionary<string, SymexValue> value, Type objectType, string symbolName = null)
        {
            this.value = value;
            this.objectType = objectType;
            this.symbolName = symbolName;
        }

        public override SymexValueType GetValueType()
        {
            return SymexValueType.Object;
        }

        public override string ToString()
        {
            return objectType.FullName + " " + (symbolName != null ? symbolName + " + " : "") + "{" + string.Join(",", value.Select(p => p.Key + ": " + p.Value)) + "}";
        }
    }

    public class SymexBitVecConstantValue : SymexValue
    {
        public readonly ulong value;

        public SymexBitVecConstantValue(ulong value)
        {
            this.value = value;
        }

        public override SymexValueType GetValueType()
        {
            return SymexValueType.BitVecConstant;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class SymexVariableValue : SymexValue
    {
        public readonly string varName;

        public SymexVariableValue(string varName)
        {
            this.varName = varName;
        }

        public override SymexValueType GetValueType()
        {
            return SymexValueType.Variable;
        }

        public override string ToString()
        {
            return "|" + varName + "|";
        }
    }

    public class SymexStructValue : SymexValue
    {
        public readonly Dictionary<string, SymexValue> value;
        public Type structType;

        public SymexStructValue(Dictionary<string, SymexValue> value, Type structType)
        {
            this.value = value;
            this.structType = structType;
        }

        public override SymexValueType GetValueType()
        {
            return SymexValueType.Struct;
        }

        public override string ToString()
        {
            return structType.FullName + " {" + string.Join(",", value.Select(p => p.Key + ": " + p.Value)) + "}";
        }
    }

    public class SymexUnevaluatedMethodCallValue : SymexValue
    {
        public readonly MethodInfo method;
        public readonly List<SymexValue> arguments;

        public SymexUnevaluatedMethodCallValue(MethodInfo method, List<SymexValue> arguments)
        {
            this.method = method;
            this.arguments = arguments;
        }

        public override SymexValueType GetValueType()
        {
            return SymexValueType.UnevaluatedMethodCall;
        }

        public override string ToString()
        {
            return method.Name + "(" +
                string.Join(", ", arguments.Select(arg => arg.ToString())) + ")";
        }
    }

    public class SymexUnknownValue : SymexValue
    {
        public override SymexValueType GetValueType()
        {
            return SymexValueType.Unknown;
        }

        public override string ToString()
        {
            return "unknown";
        }
    }
}