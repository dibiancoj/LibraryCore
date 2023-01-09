using BenchmarkDotNet.Attributes;
using LibraryCore.Core.EnumUtilities;
using static LibraryCore.Performance.Tests.Program;

namespace LibraryCore.Performance.Tests.PerfTests.Tests;

[SimpleJob]
[Config(typeof(Config))]
[MemoryDiagnoser]
public class BitMaskBuilderPerfTest
{
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
