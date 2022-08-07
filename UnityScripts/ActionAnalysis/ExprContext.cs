using System.Collections.Generic;
using UnityEngine;

namespace UnityActionAnalysis
{

public class ExprContext
{
    public readonly MonoBehaviour instance;

    public ExprContext(MonoBehaviour instance)
    {
        this.instance = instance;
    }
}

}