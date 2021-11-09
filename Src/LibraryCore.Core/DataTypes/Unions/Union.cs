namespace LibraryCore.Core.DataTypes.Unions;

public class Union<T1, T2> : UnionBase
{
    /// <summary>
    /// Create a union with the first type
    /// </summary>
    /// <param name="t1Value">Value of the T1 value</param>
    public Union(T1 t1Value)
        : base(typeof(T1), t1Value)
    {
    }

    /// <summary>
    /// Create a union with the second type
    /// </summary>
    /// <param name="t2Value"></param>
    public Union(T2 t2Value)
        : base(typeof(T2), t2Value)
    {
    }
}

public class Union<T1, T2, T3> : UnionBase
{
    public Union(T1 t1Value)
        : base(typeof(T1), t1Value)
    {
    }

    public Union(T2 t2Value)
        : base(typeof(T2), t2Value)
    {
    }

    public Union(T3 t3Value)
         : base(typeof(T3), t3Value)
    {
    }
}
