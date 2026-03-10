using System;

// ReSharper disable CheckNamespace

namespace Artifact.UnityUtils.Attributes
{
    /// <summary>
    /// Attribute for injection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class InjectAttribute : Attribute
    {
    }
}