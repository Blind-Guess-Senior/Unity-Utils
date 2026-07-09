using System;

namespace Artifact.Utils.Attributes.UnityAttributes
{
    /// <summary>
    /// Attribute for injection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class InjectAttribute : Attribute
    {
    }
}