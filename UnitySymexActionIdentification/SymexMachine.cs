﻿using System;
using System.Linq;
using System.Collections.Generic;

using UnitySymexActionIdentification.Operations;

using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;

using Microsoft.Z3;

namespace UnitySymexActionIdentification
{
    public class SymexMachine : IDisposable
    {
        public static SymexMachine Instance { get; set; }

        public CSharpDecompiler CSD { get => csd; }
        public Configuration Config { get; private set; }
        public MethodPool MethodPool { get; set; }
        public Context Z3 { get => z3; }
        public SortPool SortPool { get; set; }
        public ReferenceStorage RefStorage { get; set; }
        public List<SymexState> States { get => states;  }

        private CSharpDecompiler csd;

        private List<SymexState> states;
        private List<SymexState> statesToAdd;
        private Context z3;

        public SymexMachine(CSharpDecompiler csd, IMethod entrypoint, Configuration config)
        {
            if (Instance != null)
            {
                throw new Exception("Only one SymexMachine supported at a time");
            }
            Instance = this;

            this.csd = csd;
            Config = config;
            MethodPool = new MethodPool();
            states = new List<SymexState>();
            statesToAdd = new List<SymexState>();

            z3 = new Context(new Dictionary<string, string>());
            SortPool = new SortPool(z3);

            RefStorage = new ReferenceStorage();

            var initialState = new SymexState(z3);
            initialState.opQueue.Enqueue(new Fetch(MethodPool.MethodEntryPoint(entrypoint), null));

            states.Add(initialState);
        }

        private bool Step()
        {
            foreach (SymexState state in states)
            {
                if (state.execStatus == ExecutionStatus.ACTIVE)
                {
                    Operation op = state.opQueue.Dequeue();
                    op.Perform(state);
                }
            }
            states.AddRange(statesToAdd);
            statesToAdd.Clear();
            return states.Where(s => s.execStatus == ExecutionStatus.ACTIVE).Count() == 0;
        }

        public void ScheduleAddState(SymexState s)
        {
            statesToAdd.Add(s);
        }

        public void Run()
        {
            while (!Step()) ;
        }

        public void Dispose()
        {
            z3.Dispose();
            Instance = null;
        }
    }
}
