using System;
using System.Reflection;

public class SymbolicMethodCall
{
    public readonly int symcallId;
    public readonly SymexPath path;
    public readonly MethodInfo method;

    public SymbolicMethodCall(int symcallId, SymexPath path, MethodInfo method)
    {
        this.symcallId = symcallId;
        this.path = path;
        this.method = method;
    }
}