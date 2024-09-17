using Confluent.Kafka;
using LibraryCore.IntegrationTests.Framework.Kafka.Api;
using LibraryCore.IntegrationTests.Framework.Kafka.HostedAgents;
using LibraryCore.IntegrationTests.Framework.Kafka.Registration;
using LibraryCore.IntegrationTests.Framework.Kafka.Services;
using LibraryCore.IntegrationTests.Framework.Kafka.Settings;
using LibraryCore.Kafka.Registration;

var builder = WebApplication.CreateBuilder(args);

//need both readers to be from the same group so its split equally
builder.RegisterKakfa();

const int numberOfNodesOrPartitions = 5;

//if you need multiple hosted agents running (with the same class) - this way you end up with 2 runners (i reader isn't enough to keep up). This is needed for kafka to save the correct order (multiple consumers).
//** not using AddHostedAgent because it doesn't allow you to register the same class twice. So this AddSingleton<IHostedService> is a work around that I found.
//The key to this is you set the NumPartitions = 2...so we can go between different consumers
//if you only have 1 hosted agent - just use the normal syntax 'builder.Services.AddHostedService'
//builder.Services.AddSingleton<IHostedService>(sp => new KafkaConsumerService<string, KafkaMessageModel>(sp.GetRequiredService<ILogger<KafkaConsumerService<string, KafkaMessageModel>>>(),
//                                                                                                        new MyIntegrationHostedAgent(KafkaRegistration.BuildConsumerGroup(consumerGroupToUse),
//                                                                                                        sp.GetRequiredService<MyIntegrationHostedAgentMockDatabase>())));

//builder.Services.AddSingleton<IHostedService>(sp => new KafkaConsumerService<string, KafkaMessageModel>(sp.GetRequiredService<ILogger<KafkaConsumerService<string, KafkaMessageModel>>>(),
//                                                                                                   new MyIntegrationHostedAgent(KafkaRegistration.BuildConsumerGroup(consumerGroupToUse),
//                                                                                                   sp.GetRequiredService<MyIntegrationHostedAgentMockDatabase>())));

//or builder.AddHostedService<MyHostedAgent>()

builder.Services.AddSingleton<MockDatabase>();

builder.Services.AddKafkaConsumer<KafkaTopic1MessagePayload, SampleKafkaSettings>(builder.Configuration);
builder.Services.AddHostedService<Topic1HostedService>();

var app = builder.Build();

//create topic
var adminClient = app.Services.GetRequiredService<IAdminClient>();

//if (adminClient.GetMetadata(TimeSpan.FromSeconds(10)).Topics.Any(t => t.Topic.Equals(KafkaRegistration.TopicsToUse.Single(), StringComparison.OrdinalIgnoreCase)))
//{
//    await adminClient.DeleteTopicsAsync(KafkaRegistration.TopicsToUse);
//}

//this admin api is unstable and will change. Can't find a good way to check if a topic exists. Will run it this way.
try
{
    await adminClient.DeleteTopicsAsync(LibraryCore.IntegrationTests.Framework.Kafka.Registration.KafkaRegistration.TopicsToUse);
}
catch (Exception ex)
{
    Console.WriteLine("Topic Delete Failed With: " + ex.Message);
}

try
{
    //add 10 just to make sure we have ample slots when the old test hasn't been killed off yet.
    await adminClient.CreateTopicsAsync([new() { Name = LibraryCore.IntegrationTests.Framework.Kafka.Registration.KafkaRegistration.TopicsToUse.Single(), NumPartitions = numberOfNodesOrPartitions + 10 }]);
}
catch (Exception ex)
{
    Console.WriteLine("Topic Creation Failed With: " + ex.Message);
}


// Configure the HTTP request pipeline.


app.MapGet("/kafka", KafkaApi.Get);
app.MapGet("/kafkaMessageCount", KafkaApi.GetCount);
app.MapPost("/kafka", KafkaApi.PostAsync);

app.Run();

public partial class Program
{
    // Expose the Program class for use with WebApplicationFactory<T>
}

