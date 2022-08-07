using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class MakeArrayElementRef : Operation
    {
        private Variable refVar;
        private Variable indexVar;
        private Variable resultVar;

        public MakeArrayElementRef(Variable refVar, Variable indexVar, Variable resultVar, ILInstruction inst) : base(inst)
        {
            this.refVar = refVar;
            this.indexVar = indexVar;
            this.resultVar = resultVar;
        }

        public override void Perform(SymexState state)
        {
            Debug.Assert(refVar.IsReferenceType());
            Expr refExpr = state.MemoryRead(refVar.address, null);
            if (refExpr.FuncDecl.DeclKind != Z3_decl_kind.Z3_OP_ANUM)
            {
                throw new NotSupportedException("multi-dimensional arrays not supported");
            } else
            {
                Reference r = Reference.FromExpr(refExpr);
                BitVecExpr index = (BitVecExpr)state.MemoryRead(indexVar.address, indexVar.type);
                Debug.Assert(r.address.components.Count == 0);
                MemoryAddress address = r.address.WithComponent(new MemoryAddressArrayElement(index));
                ArrayType arrType = (ArrayType)r.type;
                Reference res = new Reference(arrType.ElementType, address);
                state.MemoryRead(res.address, res.type);
                state.MemoryWrite(resultVar.address, res.ToExpr());
            }
        }
    }
}
