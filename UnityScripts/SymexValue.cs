using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Microsoft.Z3;

public enum SymexValueType
{
    StringConstant = 1,
    Object = 2,
    BitVecConstant = 3,
    Z3Expr = 4
}

public abstract class SymexValue
{
    public abstract SymexValueType GetValueType();

    private const int TYPE_STRCONST = 1;
    private const int TYPE_OBJECT = 2;
    private const int TYPE_BVCONST = 3;
    private const int TYPE_Z3EXPR = 4;

    private static SymexValue ParseInternal(JObject o, Context z3)
    {
        int type = o["type"].ToObject<int>();
        switch (type)
        {
            case TYPE_STRCONST:
                return new SymexStringConstantValue(o["value"].ToObject<string>());
            case TYPE_OBJECT:
                {
                    Dictionary<string, SymexValue> value = new Dictionary<string, SymexValue>();
                    JObject obj = (JObject)o["value"];
                    foreach (var p in obj)
                    {
                        value.Add(p.Key, ParseInternal((JObject)p.Value, z3));
                    }
                    return new SymexObjectValue(value);
                }
            case TYPE_BVCONST:
                return new SymexBitVecConstantValue(o["value"].ToObject<ulong>());
            case TYPE_Z3EXPR:
                return new SymexZ3ExprValue(z3.ParseSMTLIB2String(o["value"].ToObject<string>())[0].Arg(0));
            default:
                throw new ArgumentException("unexpected type " + type);
        }
    }

    public static SymexValue Parse(string s, Context z3)
    {
        return ParseInternal(JObject.Parse(s), z3);
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

    public SymexObjectValue(Dictionary<string, SymexValue> value)
    {
        this.value = value;
    }

    public override SymexValueType GetValueType()
    {
        return SymexValueType.Object;
    }

    public override string ToString()
    {
        return "{" + string.Join(",", value.Select(p => p.Key + ": " + p.Value)) + "}";
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

public class SymexZ3ExprValue : SymexValue
{
    public readonly Expr value;

    public SymexZ3ExprValue(Expr value)
    {
        this.value = value;
    }

    public override SymexValueType GetValueType()
    {
        return SymexValueType.Z3Expr;
    }

    public override string ToString()
    {
        return value.ToString();
    }
}