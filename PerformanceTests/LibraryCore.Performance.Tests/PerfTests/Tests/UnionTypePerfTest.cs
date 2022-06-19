using BenchmarkDotNet.Attributes;
using LibraryCore.Core.DataTypes.Unions;
using LibraryCore.Performance.Tests.TestHarnessProvider;
using static LibraryCore.Performance.Tests.Program;

namespace LibraryCore.Performance.Tests.PerfTests.Tests;

[SimpleJob]
[Config(typeof(Config))]
[MemoryDiagnoser]
public class UnionTypePerfTest : IPerformanceTest
{
    public string CommandName => "Union";
    public string Description => "Run performance test on union object the different variations";

    [GlobalSetup]
    public void Init()
    {
        DateToUse = DateTime.Now;
    }

    private DateTime DateToUse { get; set; }

    [Benchmark(Baseline = true)]
    public string DynamicCurrentImplementation()
    {
        var unionToTest = new Union<string, DateTime>(DateToUse);

        if (unionToTest.Is<string>())
        {
            return unionToTest.As<string>();
        }
        else if (unionToTest.Is<DateTime>())
        {
            return unionToTest.As<DateTime>().ToString();
        }

        throw new Exception("Invalid");
    }

    [Benchmark]
    public string ObjectBased()
    {
        var unionToTest = new UnionNew<string, DateTime>(DateToUse);

        if (unionToTest.Is<string>())
        {
            return unionToTest.As<string>();
        }
        else if (unionToTest.Is<DateTime>())
        {
            return unionToTest.As<DateTime>().ToString();
        }

        throw new Exception("Invalid");
    }
}

#region New Implementation

public class UnionNew<T1, T2> : UnionBaseNew
{
    /// <summary>
    /// Create a union with the first type
    /// </summary>
    /// <param name="t1Value">Value of the T1 value</param>
    public UnionNew(T1 t1Value)
    {
        UnionType = typeof(T1);
        TValue = t1Value;
    }

    /// <summary>
    /// Create a union with the second type
    /// </summary>
    /// <param name="t2Value"></param>
    public UnionNew(T2 t2Value)
    {
        UnionType = typeof(T2);
        TValue = t2Value;
    }
}

public abstract class UnionBaseNew
{
    protected Type UnionType { get; init; }
    protected object TValue { get; init; }

    /// <summary>
    /// Returns true if the union contains a value of type T
    /// </summary>
    /// <remarks>The type of T must exactly match the type</remarks>
    public bool Is<T>() => typeof(T) == UnionType;

    /// <summary>
    /// Returns the union value cast to the given type.
    /// </summary>
    /// <remarks>Returns the value for the given type if it matches T. If the value does not match the the default value of T is returned</remarks>
    public T As<T>()
    {
        return Is<T>() ?
                 TValue as dynamic :
                 default(T);
    }

}

#endregion
