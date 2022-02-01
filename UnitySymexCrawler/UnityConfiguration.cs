using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.Z3;

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
        public override bool IsMethodSymbolic(IMethod method)
        {
            if (method.Name == "IsPossibleMovement" || method.Name == "set_currentPosition" || method.Name == "set_currentRotation"
                || method.Name == "DeletePossibleLines" || method.Name == "IsGameOver"
                || method.ReflectionName == "TetrisEngine.Playfield.Step" || method.ReflectionName == "TetriminoView.Draw"
                || method.Name == "get_mCurrentTetrimino" || method.Name == "get_PreviousRotation" || method.Name == "get_NextRotation")
            {
                return true;
            }
            if (!method.HasBody || IsInputAPI(method))
            {
                return true;
            }
            return method.ParentModule != SymexMachine.Instance.CSD.TypeSystem.MainModule;
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
            if (IsInputAPI(method))
            {
                ConfigData cdata = (ConfigData)state.customData;
                string arg = JsonSerializer.Serialize(state.SerializeExpr(arguments[0]));
                var p = (method.ReflectionName, arg);
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
            else
            {
                return base.SymbolicMethodResultVarId(method, arguments, state);
            }
        }
    }
}