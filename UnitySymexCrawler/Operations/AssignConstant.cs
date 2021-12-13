using System;
using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;
using Microsoft.Z3;

namespace UnitySymexCrawler.Operations
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
            MemoryAddress address = state.HeapAllocate("string");
            MemoryAddress stringAddress = new MemoryAddress(address.heap, address.root, new List<MemoryAddressComponent>() { new MemoryAddressString() });
            state.MemoryWrite(stringAddress, z3.MkString(value));
            IType type = SymexMachine.Instance.CSD.TypeSystem.FindType(KnownTypeCode.String);
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
            MemoryAddress address = state.HeapAllocate("membertoken");
            MemoryAddress memberAddress = new MemoryAddress(address.heap, address.root, new List<MemoryAddressComponent>() { new MemoryAddressMemberToken() });
            state.MemoryWrite(memberAddress, z3.MkString("membertoken:" + member.FullName));
            IType type = SymexMachine.Instance.CSD.TypeSystem.FindType(KnownTypeCode.Object);
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
            Context z3 = SymexMachine.Instance.Z3;
            IType type = SymexMachine.Instance.CSD.TypeSystem.FindType(KnownTypeCode.Object);
            Reference r = new Reference(type);
            state.MemoryWrite(destVar.address, r.ToExpr());
        }
    }
}
