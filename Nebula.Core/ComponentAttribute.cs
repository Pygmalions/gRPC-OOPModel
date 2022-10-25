﻿namespace Nebula.Core;

[AttributeUsage(AttributeTargets.Class)]
public class ComponentAttribute : Attribute
{
    /// <summary>
    /// Base class for server-side implementation.
    /// </summary>
    public readonly Type Base;

    /// <summary>
    /// Mark this attribute on a class derived from <see cref="Component"/> to
    /// generate binding between its methods and those of the server-side base class.
    /// </summary>
    /// <param name="baseType">Base type generated by gRPC.</param>
    public ComponentAttribute(Type baseType)
    {
        Base = baseType;
    }
}