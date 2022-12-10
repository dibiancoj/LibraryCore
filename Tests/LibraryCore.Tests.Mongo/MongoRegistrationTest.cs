namespace LibraryCore.Tests.Mongo;

public class MongoRegistrationTest
{
    [InlineData("sa", "password456", "mydocdbserver", "primary", "mongodb://sa:password456@mydocdbserver:27017/?ssl=true&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference=primary&retryWrites=false")]
    [InlineData("root", "password123", "mydocdbserver", "secondary", "mongodb://root:password123@mydocdbserver:27017/?ssl=true&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference=secondary&retryWrites=false")]
    [Theory]
    public void ConnectionString(string userName, string password, string hostName, string readPreference, string expectedConnectionString)
    {
        Assert.Equal(expectedConnectionString, LibraryCore.Mongo.Registration.DocumentDbRegistration.EncryptedMongoConnectionStringBuilder(userName, password, hostName, readPreference: readPreference));
    }

    [InlineData("sa", "password456", "mydocdbserver", "mongodb://sa:password456@mydocdbserver:27017/?ssl=true&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference=primary&retryWrites=false")]
    [InlineData("root", "password123", "mydocdbserver", "mongodb://root:password123@mydocdbserver:27017/?ssl=true&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference=primary&retryWrites=false")]
    [Theory]
    public void ConnectionStringWithDefaultValue(string userName, string password, string hostName, string expectedConnectionString)
    {
        Assert.Equal(expectedConnectionString, LibraryCore.Mongo.Registration.DocumentDbRegistration.EncryptedMongoConnectionStringBuilder(userName, password, hostName));
    }

}
