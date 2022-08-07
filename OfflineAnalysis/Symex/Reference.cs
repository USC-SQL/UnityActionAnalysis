using System;
using System.Diagnostics.CodeAnalysis;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.Z3;

namespace UnityActionAnalysis
{
    public class Reference : IEquatable<Reference>
    {
        public IType type;
        public MemoryAddress address = null;
        public int? storageId = null;

        // null reference
        public Reference(IType type)
        {
            this.type = type;
        }

        // reference to object
        public Reference(IType type, MemoryAddress address)
        {
            this.type = type;
            this.address = address;
        }

        public Expr ToExpr()
        {
            var z3 = SymexMachine.Instance.Z3;
            SymexMachine.Instance.RefStorage.Store(this);
            return z3.MkInt(storageId.Value);
        }

        public static Reference FromExpr(Expr expr)
        {
            IntExpr iexpr = (IntExpr)expr;
            if (int.TryParse(iexpr.ToString(), out int storageId))
            {
                return SymexMachine.Instance.RefStorage.Find(storageId);
            } else
            {
                throw new ArgumentException("expected constant integer, got " + expr);
            }
        }

        public bool Equals(Reference other)
        {
            if (address == null && other.address == null)
            {
                return true;
            }
            
            if ((address == null && other.address != null) || (address != null && other.address == null))
            {
                return false;
            }

            if (!address.Equals(other.address))
            {
                return false;
            }

            return true;
        }
    }
}
