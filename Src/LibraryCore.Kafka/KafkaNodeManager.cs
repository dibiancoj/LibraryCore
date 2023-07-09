using System.Collections.Concurrent;

namespace LibraryCore.Kafka;

public class KafkaNodeManager
{
    public KafkaNodeManager()
    {
        RegisteredJobs = new();
    }

    private ConcurrentDictionary<string, RegistrationConfiguration> RegisteredJobs { get; }

    public KafkaNodeManager RegisterJob(string key, int numberOfNodes, Func<IKafkaNodeCreator> nodeCreator)
    {
        RegisteredJobs.TryAdd(key, new(nodeCreator, numberOfNodes));
        return this;
    }

    public IEnumerable<Task> CreateNodeAsync(string key, CancellationToken cancellationToken)
    {
        var configuration = RegisteredJobs[key];

        var tasks = new List<Task>();

        for (int i = 0; i < configuration.NumberOfNodes; i++)
        {
            tasks.Add(configuration.NodeCreator().CreateNodeAsync(i, key, cancellationToken));
        }

        return tasks;
    }

    public record RegistrationConfiguration(Func<IKafkaNodeCreator> NodeCreator, int NumberOfNodes);

}