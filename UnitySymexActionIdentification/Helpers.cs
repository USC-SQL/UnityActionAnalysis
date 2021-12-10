using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.Metadata;
using Microsoft.Z3;

namespace UnitySymexActionIdentification
{
    public class Helpers
    {
        public static IEnumerable<IField> GetInstanceFields(IType type)
        {
            return type.GetFields(f => !f.IsStatic);
        }

        public static FuncDecl FindFieldAccessor(DatatypeSort dsort, IField field)
        {
            var accessors = dsort.Accessors[0];
            FuncDecl fieldAccessor = null;
            foreach (FuncDecl acc in accessors)
            {
                if (((StringSymbol)acc.Name).String == field.Name)
                {
                    fieldAccessor = acc;
                    break;
                }
            }
            if (fieldAccessor == null)
            {
                throw new Exception("failed to find accessor for field " + field.FullName);
            }
            return fieldAccessor;
        }

        public static Expr MakeDefaultValue(IType type)
        {
            Context z3 = SymexMachine.Instance.Z3;
            if (type.Kind == TypeKind.Class || type.Kind == TypeKind.Interface || type.Kind == TypeKind.Array)
            {
                Reference r = new Reference(type);
                return r.ToExpr();
            }
            else if (type.Kind == TypeKind.Struct || type.Kind == TypeKind.Enum)
            {
                var sort = SymexMachine.Instance.SortPool.TypeToSort(type);
                if (sort is DatatypeSort) // struct with fields
                {
                    var dsort = (DatatypeSort)sort;
                    List<Expr> elems = new List<Expr>();
                    foreach (IField field in Helpers.GetInstanceFields(type))
                    {
                        elems.Add(MakeDefaultValue(field.Type));
                    }
                    FuncDecl ctor = dsort.Constructors[0];
                    return ctor.Apply(elems.ToArray());
                }
                else if (sort is BitVecSort)
                {
                    BitVecSort bvsort = (BitVecSort)sort;
                    return z3.MkBV(0, bvsort.Size);
                }
                else if (sort is FPSort)
                {
                    FPSort fpsort = (FPSort)sort;
                    return z3.MkFP(0.0, fpsort);
                }
                else
                {
                    throw new Exception("unexpected sort " + sort);
                }
            }
            else
            {
                throw new NotImplementedException("MakeDefaultValue for type of kind " + type.Kind + " not implemented");
            }
        }
    
        public static void DebugLog(string message)
        {
            using (StreamWriter sw = File.AppendText(@"C:\Users\sasha-usc\Misc\debug.log"))
            {
                sw.WriteLine(message);
            }
        }
    }
}
