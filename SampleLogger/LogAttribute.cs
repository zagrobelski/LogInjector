using System;

namespace Logger
{
    [AttributeUsage( AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, AllowMultiple = false, Inherited = true )]
    public class LogAttribute : Attribute
    {
    }
}
