﻿namespace LibraryCore.Core.DataTypes.Unions;

/// <summary>
/// Base Union type
/// </summary>
public abstract class UnionBase(Type unionType, dynamic? currentValue)
{
    protected Type UnionType { get; } = unionType;
    protected dynamic? CurrentValue { get; } = currentValue;

    /// <summary>
    /// Returns true if the union contains a value of type T
    /// </summary>
    /// <remarks>The type of T must exactly match the type</remarks>
    public bool Is<T>() => typeof(T) == UnionType;

    /// <summary>
    /// Returns the union value cast to the given type.
    /// </summary>
    /// <remarks>Returns the value for the given type if it matches T. If the value does not match the the default value of T is returned</remarks>
    public T? As<T>()
    {
        return Is<T>() ?
                 CurrentValue :
                 default(T);
    }

}

