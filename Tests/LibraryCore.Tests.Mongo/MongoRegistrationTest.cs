namespace LibraryCore.Tests.Mongo;

public class MongoRegistrationTest
{
    [InlineData("sa", "password456", "mydocdbserver", "primary", false, "mongodb://sa:password456@mydocdbserver:27017/?ssl=true&retryWrites=false&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference=primary")]
    [InlineData("root", "password123", "mydocdbserver", "secondary", false, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&retryWrites=false&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference=secondary")]
    [InlineData("root", "password123", "mydocdbserver", "secondary", true, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&retryWrites=false")]
    [Theory]
    public void ConnectionString(string userName, string password, string hostName, string readPreference, bool fromLocalHostWithSsh, string expectedConnectionString)
    {
        Assert.Equal(expectedConnectionString, LibraryCore.Mongo.Registration.DocumentDbRegistration.EncryptedMongoConnectionStringBuilder(userName, password, hostName, readPreference: readPreference, runningFromLocalHostWithSsh: fromLocalHostWithSsh));
    }

    [InlineData("sa", "password456", "mydocdbserver", false, 27017, "mongodb://sa:password456@mydocdbserver:27017/?ssl=true&retryWrites=false&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference=primary")]
    [InlineData("root", "password123", "mydocdbserver", false, 27017, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&retryWrites=false&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference=primary")]
    [InlineData("root", "password123", "mydocdbserver", false, 9999, "mongodb://root:password123@mydocdbserver:9999/?ssl=true&retryWrites=false&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference=primary")]
    [InlineData("root", "password123", "mydocdbserver", true, 27017, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&retryWrites=false")]
    [Theory]
    public void ConnectionStringWithDefaultValue(string userName, string password, string hostName, bool fromLocalHostWithSsh, int hostPort, string expectedConnectionString)
    {
        Assert.Equal(expectedConnectionString, LibraryCore.Mongo.Registration.DocumentDbRegistration.EncryptedMongoConnectionStringBuilder(userName, password, hostName, port: hostPort, runningFromLocalHostWithSsh: fromLocalHostWithSsh));
    }

}
