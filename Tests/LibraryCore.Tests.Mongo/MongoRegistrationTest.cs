namespace LibraryCore.Tests.Mongo;

public class MongoRegistrationTest
{
    [InlineData("sa", "password456", "mydocdbserver", "primary", false, "mongodb://sa:password456@mydocdbserver:27017/?ssl=true&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference=primary&retryWrites=false")]
    [InlineData("root", "password123", "mydocdbserver", "secondary", false, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference=secondary&retryWrites=false")]
    [InlineData("root", "password123", "mydocdbserver", "secondary", true, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&ssl_ca_certs=rds-combined-ca-bundle.pem")]
    [Theory]
    public void ConnectionString(string userName, string password, string hostName, string readPreference, bool fromLocalHostWithSsh, string expectedConnectionString)
    {
        Assert.Equal(expectedConnectionString, LibraryCore.Mongo.Registration.DocumentDbRegistration.EncryptedMongoConnectionStringBuilder(userName, password, hostName, readPreference: readPreference, runningFromLocalHostWithSsh: fromLocalHostWithSsh));
    }

    [InlineData("sa", "password456", "mydocdbserver", false, "mongodb://sa:password456@mydocdbserver:27017/?ssl=true&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference=primary&retryWrites=false")]
    [InlineData("root", "password123", "mydocdbserver", false, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference=primary&retryWrites=false")]
    [InlineData("root", "password123", "mydocdbserver", true, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&ssl_ca_certs=rds-combined-ca-bundle.pem")]
    [Theory]
    public void ConnectionStringWithDefaultValue(string userName, string password, string hostName, bool fromLocalHostWithSsh, string expectedConnectionString)
    {
        Assert.Equal(expectedConnectionString, LibraryCore.Mongo.Registration.DocumentDbRegistration.EncryptedMongoConnectionStringBuilder(userName, password, hostName, runningFromLocalHostWithSsh: fromLocalHostWithSsh));
    }

}
