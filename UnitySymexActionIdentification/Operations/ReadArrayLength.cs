﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnitySymexActionIdentification.Operations
{
    public class ReadArrayLength : Operation
    {
        private Variable refVar;
        private Variable resultVar;

        public ReadArrayLength(Variable refVar, Variable resultVar, ILInstruction inst) : base(inst)
        {
            this.refVar = refVar;
            this.resultVar = resultVar;
        }

        public override void Perform(SymexState state)
        {
            Debug.Assert(refVar.IsReferenceType());
            Expr refExpr = state.MemoryRead(refVar.address, null);
            Reference r = Reference.FromExpr(refExpr);
            Debug.Assert(r.address.components.Count == 0);
            MemoryAddress lengthAddress = new MemoryAddress(r.address.root, new List<MemoryAddressComponent>() { new MemoryAddressArrayLength() });
            Expr length = state.MemoryRead(lengthAddress, resultVar.type);
            state.MemoryWrite(resultVar.address, length);
        }
    }
}
