using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnitySymexCrawler.Operations
{
    public struct BranchCase
    {
        public Variable condVar;
        public InstructionPointer IP;

        public BranchCase(Variable condVar, InstructionPointer IP)
        {
            this.condVar = condVar;
            this.IP = IP;
        }
    }

    public class Branch : Operation
    {
        private struct SATCase
        {
            public BranchCase branchCase;
            public BoolExpr condition;

            public SATCase(BranchCase branchCase, BoolExpr condition)
            {
                this.branchCase = branchCase;
                this.condition = condition;
            }
        }

        private List<BranchCase> branchCases;

        public Branch(List<BranchCase> branchCases, ILInstruction inst) : base(inst)
        {
            this.branchCases = branchCases;
        }

        public override void Perform(SymexState state)
        {
            Context z3 = SymexMachine.Instance.Z3;
            List<SATCase> satCases = new List<SATCase>();
            Solver s = z3.MkSolver();
            foreach (BoolExpr cond in state.pathCondition)
            {
                s.Assert(cond);
            }
            foreach (BranchCase branchCase in branchCases)
            {
                s.Push();
                BitVecExpr bvCond = (BitVecExpr)state.MemoryRead(branchCase.condVar.address, branchCase.condVar.type);
                BoolExpr cond = z3.MkNot(z3.MkEq(bvCond, z3.MkBV(0, bvCond.SortSize)));
                s.Assert(cond);
                Helpers.AssertAssumptions(s, z3);
                if (s.Check() == Status.SATISFIABLE)
                {
                    satCases.Add(new SATCase(branchCase, cond));
                }
                s.Pop();
            }
            s.Dispose();

            if (satCases.Count > 1)
            {
                for (int i = 0, n = satCases.Count - 1; i < n; ++i)
                {
                    SATCase satCase = satCases[i];
                    state.Fork(satCase.branchCase.IP, satCase.condition, Instruction);
                }
                SATCase lastSatCase = satCases[satCases.Count - 1];
                state.pathCondition.Add(lastSatCase.condition);
                Fetch fetchOp = new Fetch(lastSatCase.branchCase.IP, Instruction);
                fetchOp.Perform(state);
            } else
            {
                state.pathCondition.Add(satCases[0].condition);
                Fetch fetchOp = new Fetch(satCases[0].branchCase.IP, Instruction);
                fetchOp.Perform(state);
            }
        }
    }
}
