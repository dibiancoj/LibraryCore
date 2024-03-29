using Confluent.Kafka;
using Confluent.Kafka.Admin;
using LibraryCore.IntegrationTests.Framework.Kafka.Api;
using LibraryCore.IntegrationTests.Framework.Kafka.KafkaProcessor;
using LibraryCore.IntegrationTests.Framework.Kafka.Registration;
using LibraryCore.Kafka;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterKakfa();
builder.Services.AddSingleton<MyIntegrationHostedAgentMockDatabase>();

//need both readers to be from the same group so its split equally
const string consumerGroupToUse = "integrationTest";
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

builder.Services.AddSingleton(sp => new KafkaNodeManager()
    .RegisterJob(MyHostedAgent.KakfaJobName, numberOfNodesOrPartitions, () => new MyKafkaJob1(sp.GetRequiredService<ILogger<MyKafkaJob1>>(),
                                                                      KafkaRegistration.TopicsToUse,
                                                                      KafkaRegistration.BuildConsumerGroup(consumerGroupToUse),
                                                                      sp.GetRequiredService<MyIntegrationHostedAgentMockDatabase>())));

builder.Services.AddSingleton<IHostedService>(sp => new MyHostedAgent(sp.GetRequiredService<KafkaNodeManager>()));
//or builder.AddHostedService<MyHostedAgent>()

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
    await adminClient.DeleteTopicsAsync(KafkaRegistration.TopicsToUse);
}
catch (Exception ex)
{
    Console.WriteLine("Topic Delete Failed With: " + ex.Message);
}

try
{
    //add 10 just to make sure we have ample slots when the old test hasn't been killed off yet.
    await adminClient.CreateTopicsAsync([new() { Name = KafkaRegistration.TopicsToUse.Single(), NumPartitions = numberOfNodesOrPartitions + 10 }]);
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

