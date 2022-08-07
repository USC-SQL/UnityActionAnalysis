using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;
using Microsoft.Z3;

namespace UnityActionAnalysis.Operations
{
    public class IsOp : Operation
    {
        private Variable valueVar;
        private IType type;
        private Variable resultVar;

        public IsOp(Variable valueVar, IType type, Variable resultVar, ILInstruction inst) : base(inst)
        {
            this.valueVar = valueVar;
            this.type = type;
            this.resultVar = resultVar;
        }

        public override void Perform(SymexState state)
        {
            Context z3 = SymexMachine.Instance.Z3;
            Expr refExpr = state.MemoryRead(valueVar.address, null);
            Reference r = Reference.FromExpr(refExpr);
            bool result = r.type.GetDefinition().IsDerivedFrom(type.GetDefinition());
            if (result)
            {
                state.MemoryWrite(resultVar.address, r.ToExpr());
            } else
            {
                Reference nullref = new Reference(type);
                state.MemoryWrite(resultVar.address, nullref.ToExpr());
            }
        }
    }
}
