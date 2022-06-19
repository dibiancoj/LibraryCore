using BenchmarkDotNet.Attributes;
using LibraryCore.Core.EnumUtilities;
using LibraryCore.Performance.Tests.TestHarnessProvider;
using static LibraryCore.Performance.Tests.Program;

namespace LibraryCore.Performance.Tests.PerfTests.Tests;

[SimpleJob]
[Config(typeof(Config))]
[MemoryDiagnoser]
public class BitMaskBuilderPerfTest : IPerformanceTest
{
    public string CommandName => "BitMaskBuilder";
    public string Description => "Run performance test on the bit mask builder with an immutable record or mutable class";

    [Flags]
    public enum TestEnum : int
    {
        City = 0,
        State = 1,
        Country = 2,
        Planet = 4
    }

    [Benchmark(Baseline = true)]
    public TestEnum ClassBaseBuilder()
    {
        return new BitMaskBuilder<TestEnum>(TestEnum.City)
                        .AddItem(TestEnum.State)
                        .AddItem(TestEnum.Country)
                        .AddItem(TestEnum.Planet)
                        .RemoveItem(TestEnum.Country)
                        .Value;
    }

    [Benchmark]
    public TestEnum RecordBaseBuilder()
    {
        return new BitMaskBuilderRecord<TestEnum>(TestEnum.City)
                          .AddItem(TestEnum.State)
                          .AddItem(TestEnum.Country)
                          .AddItem(TestEnum.Planet)
                          .RemoveItem(TestEnum.Country)
                          .Value;
    }

}

public record BitMaskBuilderRecord<T>(T Value) where T : struct, Enum
{
    public BitMaskBuilderRecord<T> AddItem(T valueToAdd) => this with { Value = EnumUtility.BitMaskAddItem(Value, valueToAdd) };

    public BitMaskBuilderRecord<T> RemoveItem(T valueToRemove) => this with { Value = EnumUtility.BitMaskRemoveItem(Value, valueToRemove) };

    public bool ContainsValue(T valueToCheckFor) => EnumUtility.BitMaskContainsValue(Value, valueToCheckFor);

    public IEnumerable<T> SelectedItems() => EnumUtility.BitMaskSelectedItems(Value);
}
