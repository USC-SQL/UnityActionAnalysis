using System.Collections.Generic;
using UnityEngine;

namespace UnitySymexCrawler
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