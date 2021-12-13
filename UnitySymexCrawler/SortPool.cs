using System;
using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.Z3;

namespace UnitySymexCrawler
{
    public class SortPool
    {
        private Context z3;
        private Dictionary<string, Sort> pool;

        public SortPool(Context z3)
        {
            this.z3 = z3;
            pool = new Dictionary<string, Sort>();
        }

        public bool IsSigned(IType type)
        {
            switch (type.Kind)
            {
                case TypeKind.Struct:
                    switch (type.FullName)
                    {
                        case "System.Boolean":
                        case "System.Byte":
                        case "System.Char":
                        case "System.UInt16":
                        case "System.UInt32":
                        case "System.UIntPtr":
                        case "System.UInt64":
                            return false;
                        case "System.SByte":
                        case "System.Int16":
                        case "System.Int32":
                        case "System.IntPtr":
                        case "System.Int64":
                            return true;
                    }
                    break;
                case TypeKind.Enum:
                    return IsSigned(type.GetEnumUnderlyingType());
            }
            throw new ArgumentException("unexpected IsSigned call on " + type.FullName);
        }

        public Sort TypeToSort(IType type)
        {
            string typeFullName = type.FullName;
            Sort result;
            if (pool.TryGetValue(typeFullName, out result))
            {
                return result;
            } else
            {
                switch (type.Kind)
                {
                    case TypeKind.Struct:
                        switch (typeFullName)
                        {
                            case "System.Boolean":
                                result = z3.MkBitVecSort(1);
                                break;
                            case "System.Byte":
                                result = z3.MkBitVecSort(8);
                                break;
                            case "System.SByte":
                                result = z3.MkBitVecSort(8);
                                break;
                            case "System.Char":
                                result = z3.MkBitVecSort(16);
                                break;
                            case "System.Double":
                                result = z3.MkFPSortDouble();
                                break;
                            case "System.Single":
                                result = z3.MkFPSortSingle();
                                break;
                            case "System.Int32":
                            case "System.IntPtr":
                                result = z3.MkBitVecSort(32);
                                break;
                            case "System.UInt32":
                            case "System.UIntPtr":
                                result = z3.MkBitVecSort(32);
                                break;
                            case "System.Int64":
                                result = z3.MkBitVecSort(64);
                                break;
                            case "System.UInt64":
                                result = z3.MkBitVecSort(64);
                                break;
                            case "System.Int16":
                                result = z3.MkBitVecSort(16);
                                break;
                            case "System.UInt16":
                                result = z3.MkBitVecSort(16);
                                break;
                            default:
                                {
                                    List<string> fields = new List<string>();
                                    List<Sort> sorts = new List<Sort>();
                                    foreach (IField f in Helpers.GetInstanceFields(type))
                                    {
                                        fields.Add(f.Name);
                                        sorts.Add(TypeToSort(f.Type));
                                    }
                                    Constructor ctor = z3.MkConstructor(typeFullName, "is_" + typeFullName, fields.ToArray(), sorts.ToArray(), null);
                                    result = z3.MkDatatypeSort(typeFullName, new Constructor[] { ctor });
                                    break;
                                }
                        }
                        break;
                    case TypeKind.Enum:
                        result = TypeToSort(type.GetEnumUnderlyingType());
                        break;
                    case TypeKind.Class:
                    case TypeKind.Interface:
                    case TypeKind.Array:
                        result = z3.MkIntSort(); // constant integer handle to reference
                        break;
                    case TypeKind.Pointer:
                        result = z3.MkBitVecSort(32);
                        break;
                    default:
                        throw new NotImplementedException(typeFullName + " (kind " + type.Kind + ") unsupported");
                }
                pool.Add(typeFullName, result);
                return result;
            }
        }

    }
}
