using System;
using System.Collections.Generic;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnitySymexActionIdentification.Operations
{
    public class Convert : Operation
    {
        private Variable valueVar;
        private IType typeTo;
        private Variable resultVar;

        public Convert(Variable valueVar, IType typeTo, Variable resultVar, ILInstruction inst) : base(inst)
        {
            this.valueVar = valueVar;
            this.typeTo = typeTo;
            this.resultVar = resultVar;
        }

        public override void Perform(SymexState state)
        {
            Context z3 = SymexMachine.Instance.Z3;
            IType typeFrom = valueVar.type;
            Sort sortFrom = SymexMachine.Instance.SortPool.TypeToSort(typeFrom);
            Sort sortTo = SymexMachine.Instance.SortPool.TypeToSort(typeTo);
            Expr valueFrom = state.MemoryRead(valueVar.address, valueVar.type);
            Expr result;

            if (sortFrom is IntSort || sortTo is IntSort)
            {
                if (!(sortFrom is IntSort) || !(sortTo is IntSort))
                {
                    throw new Exception("unexpected conversion between reference type and value type");
                }
                result = valueFrom;
            }
            else if (sortFrom is BitVecSort && sortTo is BitVecSort)
            {
                bool fromSigned = SymexMachine.Instance.SortPool.IsSigned(typeFrom);
                uint bitsTo = ((BitVecSort)sortTo).Size;
                result = z3.MkInt2BV(bitsTo, z3.MkBV2Int((BitVecExpr)valueFrom, fromSigned));
            }
            else if (sortFrom is BitVecSort && sortTo is FPSort)
            {
                bool fromSigned = SymexMachine.Instance.SortPool.IsSigned(typeFrom);
                result = z3.MkFPToFP(z3.MkFPRNE(), (BitVecExpr)valueFrom, (FPSort)sortTo, fromSigned);
            }
            else if (sortFrom is FPSort && sortTo is BitVecSort)
            {
                bool toSigned = SymexMachine.Instance.SortPool.IsSigned(typeTo);
                result = z3.MkFPToBV(z3.MkFPRTZ(), (FPExpr)valueFrom, ((BitVecSort)sortTo).Size, toSigned);
            }
            else if (sortFrom is FPSort && sortTo is FPSort)
            {
                result = z3.MkFPToFP(z3.MkFPRNE(), (FPExpr)valueFrom, (FPSort)sortTo);
            }
            else
            {
                throw new Exception("unexpected conversion from " + typeFrom.FullName + " to " + typeTo.FullName);
            }

            state.MemoryWrite(resultVar.address, result);
        }
    }
}
