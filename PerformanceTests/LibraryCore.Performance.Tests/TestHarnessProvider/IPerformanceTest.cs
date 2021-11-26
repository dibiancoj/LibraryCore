namespace LibraryCore.Performance.Tests.TestHarnessProvider
{
    public interface IPerformanceTest
    {
        public string CommandName { get; }
        public string Description { get; }
    }
}
