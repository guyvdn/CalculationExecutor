using System;

namespace Calc
{

[AttributeUsage(AttributeTargets.Method,
    AllowMultiple = false, Inherited = false)]
public class DependsOnAttribute : Attribute
{
    public DependsOnAttribute(params string[] dependencies)
    {
        Dependencies = dependencies;
    }

    public string[] Dependencies { get; set; }
}
}
