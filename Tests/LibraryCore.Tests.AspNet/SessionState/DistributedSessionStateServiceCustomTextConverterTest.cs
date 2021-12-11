using LibraryCore.AspNet.SessionState;
using LibraryCore.Tests.AspNet.Framework;
using System.Text.Json;

namespace LibraryCore.Tests.AspNet.SessionState;

public class DistributedSessionStateServiceCustomTextConverterTest
{
    public class CustomConverterModel
    {
        public string Id { get; set; } = null!;
    }

    public class CustomJsonConverter : System.Text.Json.Serialization.JsonConverter<CustomConverterModel>
    {
        public override CustomConverterModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var modelToReturn = new CustomConverterModel();

            string currentPropertyName = string.Empty;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    currentPropertyName = reader.GetString() ?? throw new Exception("Property Name Value Not Set");
                }
                else if (currentPropertyName == "PrefixId" && reader.TokenType == JsonTokenType.String)
                {
                    modelToReturn.Id = reader.GetString() ?? throw new Exception("Id Property Value Not Set");
                }
            }

            return modelToReturn;
        }

        public override void Write(Utf8JsonWriter writer, CustomConverterModel value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("PrefixId");
            writer.WriteStringValue(value.Id);
            writer.WriteEndObject();
        }
    }

    [Fact]
    public async Task JsonConverterInConstructorTest()
    {
        var sessionStateServiceToUse = new DistributedSessionStateService(FullMockSessionState.BuildContextWithSession().MockContextAccessor.Object,
                                                                          new List<System.Text.Json.Serialization.JsonConverter>
        {
                new CustomJsonConverter()
        });

        await sessionStateServiceToUse.SetObjectAsync("test", new CustomConverterModel { Id = "Test123" });

        var valueInSession = await sessionStateServiceToUse.GetOrSetAsync<CustomConverterModel>("test", () => throw new NotImplementedException());

        Assert.Equal("Test123", valueInSession.Id);
    }

    [Fact]
    public async Task JsonConverterCheckIfNullConvertersIsHandled()
    {
        var sessionStateServiceToUse = new DistributedSessionStateService(FullMockSessionState.BuildContextWithSession().MockContextAccessor.Object, null);

        await sessionStateServiceToUse.SetObjectAsync("test", "test123");

        Assert.Equal("test123", await sessionStateServiceToUse.GetObjectAsync<string>("test"));
    }

}
