using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;
using Microsoft.Z3;
using UnitySymexCrawler.Operations;

namespace UnitySymexCrawler
{
    class ConfigData
    {
        public Dictionary<(string, string), int> inputVarIds;

        public ConfigData()
        {
            inputVarIds = new Dictionary<(string, string), int>();
        }

        public ConfigData(ConfigData o)
        {
            inputVarIds = new Dictionary<(string, string), int>(o.inputVarIds);
        }
    }

    public class UnityConfiguration : Configuration
    {
        private InputBranchAnalysis.Result ibaResult;

        public UnityConfiguration(InputBranchAnalysis.Result ibaResult)
        {
            this.ibaResult = ibaResult;
        }

        public override bool ShouldSkipBranchCase(BranchCase branchCase, ILInstruction branchInst, SymexState state)
        {
            IMethod method = branchCase.IP.GetCurrentMethod();
            InputBranchAnalysis.MethodAnalysisResult res = ibaResult.methodResults[method];
            if (res.inputDepBranchPoints.Contains(branchInst))
            {
                return false;
            }
            if (state.frameStack.Count > 0)
            {
                foreach (FrameStackElement fse in state.frameStack)
                {
                    var callInst = fse.opQueue.Peek().Instruction;
                    var m = Helpers.GetInstructionFunction(callInst).Method;
                    var mRes = ibaResult.methodResults[m];
                    if (mRes.leadsToInputDepBranchPoint.Contains(callInst))
                    {
                        return false;
                    }
                }
            }
            ILInstruction target = branchCase.IP.GetInstruction();
            return !res.leadsToInputDepBranchPoint.Contains(target);
        }

        public override bool IsMethodSymbolic(IMethod method)
        {
            if (!method.HasBody || IsInputAPI(method))
            {
                return true;
            }
            if (method.ParentModule != SymexMachine.Instance.CSD.TypeSystem.MainModule)
            {
                return true;
            }
            ILInstruction entryPoint = SymexMachine.Instance.MethodPool.MethodEntryPoint(method).GetInstruction();
            InputBranchAnalysis.MethodAnalysisResult res = ibaResult.methodResults[method];
            return !res.mayReturnInput && !res.leadsToInputDepBranchPoint.Contains(entryPoint);
        }

        public static bool IsInputAPI(IMethod method)
        {
            return method.DeclaringType.FullName == "UnityEngine.Input" && method.Parameters.Count == 1;
        }

        public override object NewStateCustomData()
        {
            return new ConfigData();
        }

        public override object CloneStateCustomData(object data)
        {
            return new ConfigData((ConfigData)data);
        }

        public override int SymbolicMethodResultVarId(IMethod method, List<Expr> arguments, SymexState state)
        {
            ConfigData cdata = (ConfigData)state.customData;
            string args =
                string.Join(";", arguments.Select(arg => JsonSerializer.Serialize(state.SerializeExpr(arg))));
            var p = (method.ReflectionName, args);
            if (cdata.inputVarIds.TryGetValue(p, out int varId))
            {
                return varId;
            }
            else
            {
                int result = base.SymbolicMethodResultVarId(method, arguments, state);
                cdata.inputVarIds.Add(p, result);
                return result;
            }
        }
    }
}