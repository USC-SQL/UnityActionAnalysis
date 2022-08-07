using System;
using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;
using Microsoft.Z3;

namespace UnityActionAnalysis.Operations
{
    public class AssignConstantFloat : Operation
    {
        private Variable destVar;
        private float value;

        public AssignConstantFloat(Variable destVar, float value, ILInstruction inst) : base(inst)
        {
            this.destVar = destVar;
            this.value = value;
        }

        public override void Perform(SymexState state)
        {
            Context z3 = SymexMachine.Instance.Z3;
            state.MemoryWrite(destVar.address, z3.MkFP(value, z3.MkFPSortSingle()));
        }
    }

    public class AssignConstantDouble : Operation
    {
        private Variable destVar;
        private double value;

        public AssignConstantDouble(Variable destVar, double value, ILInstruction inst) : base(inst)
        {
            this.destVar = destVar;
            this.value = value;
        }

        public override void Perform(SymexState state)
        {
            Context z3 = SymexMachine.Instance.Z3;
            state.MemoryWrite(destVar.address, z3.MkFP(value, z3.MkFPSortDouble()));
        }
    }

    public class AssignConstantInt32 : Operation
    {
        private Variable destVar;
        private int value;

        public AssignConstantInt32(Variable destVar, int value, ILInstruction inst) : base(inst)
        {
            this.destVar = destVar;
            this.value = value;
        }

        public override void Perform(SymexState state)
        {
            Context z3 = SymexMachine.Instance.Z3;
            state.MemoryWrite(destVar.address, z3.MkBV(value, 32));
        }
    }

    public class AssignConstantInt64 : Operation
    {
        private Variable destVar;
        private long value;

        public AssignConstantInt64(Variable destVar, long value, ILInstruction inst) : base(inst)
        {
            this.destVar = destVar;
            this.value = value;
        }

        public override void Perform(SymexState state)
        {
            Context z3 = SymexMachine.Instance.Z3;
            state.MemoryWrite(destVar.address, z3.MkBV(value, 64));
        }
    }

    public class AssignConstantString : Operation
    {
        private Variable destVar;
        private string value;

        public AssignConstantString(Variable destVar, string value, ILInstruction inst) : base(inst)
        {
            this.destVar = destVar;
            this.value = value;
        }

        public override void Perform(SymexState state)
        {
            Context z3 = SymexMachine.Instance.Z3;
            IType type = SymexMachine.Instance.CSD.TypeSystem.FindType(KnownTypeCode.String);
            MemoryAddress address = state.HeapAllocate(type, "string");
            MemoryAddress stringAddress = address.WithComponent(new MemoryAddressString());
            state.MemoryWrite(stringAddress, z3.MkString(value));
            Reference r = new Reference(type, address);
            state.MemoryWrite(destVar.address, r.ToExpr());
        }
    }

    public class AssignConstantMemberToken : Operation
    {
        private Variable destVar;
        private IMember member;

        public AssignConstantMemberToken(Variable destVar, IMember member, ILInstruction inst) : base(inst)
        {
            this.destVar = destVar;
            this.member = member;
        }

        public override void Perform(SymexState state)
        {
            Context z3 = SymexMachine.Instance.Z3;
            IType type = SymexMachine.Instance.CSD.TypeSystem.FindType(KnownTypeCode.Object);
            MemoryAddress address = state.HeapAllocate(type, "membertoken");
            MemoryAddress memberAddress = address.WithComponent(new MemoryAddressMemberToken());
            state.MemoryWrite(memberAddress, z3.MkString("membertoken:" + member.FullName));
            Reference r = new Reference(type, address);
            state.MemoryWrite(destVar.address, r.ToExpr());
        }
    }

    public class AssignNull : Operation
    {
        private Variable destVar;

        public AssignNull(Variable destVar, ILInstruction inst) : base(inst)
        {
            this.destVar = destVar;
        }

        public override void Perform(SymexState state)
        {
            IType type = SymexMachine.Instance.CSD.TypeSystem.FindType(KnownTypeCode.Object);
            Reference r = new Reference(type);
            state.MemoryWrite(destVar.address, r.ToExpr());
        }
    }
}
