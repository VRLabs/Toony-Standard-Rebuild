using System;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ChainableAttribute : Attribute
    {}

    [AttributeUsage(AttributeTargets.Constructor)]
    public class LimitAccessScopeAttribute : Attribute
    {
        public Type BaseType { get; }
        public LimitAccessScopeAttribute(Type type)
        {
            BaseType = type;
        }
    }
}