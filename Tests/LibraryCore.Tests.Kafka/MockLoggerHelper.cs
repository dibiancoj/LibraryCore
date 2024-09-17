using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryCore.Tests.Kafka;

public static class MockLoggerHelper
{
    public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, Times times, string? expectedMessage = null)
    {
        Func<object, Type, bool> messageState = (value, type) =>
        {
            if (value == null) { return true; }
            return value.ToString()!.Equals(expectedMessage);
        };

        logger.Verify(
            log => log.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((value, type) => messageState(value, type)),
                It.IsAny<Exception?>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((value, type) => true)), times);
    }
}
