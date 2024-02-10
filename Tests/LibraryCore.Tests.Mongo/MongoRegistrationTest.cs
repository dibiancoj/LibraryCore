using LibraryCore.Mongo.Registration;
using static LibraryCore.Mongo.Registration.DocumentDbRegistration;

namespace LibraryCore.Tests.Mongo;

public class MongoRegistrationTest
{
    [InlineData("sa", "password456", "mydocdbserver", "primary", false, DocumentDbCertIdentifier.rds_ca_2019, "mongodb://sa:password456@mydocdbserver:27017/?ssl=true&retryWrites=False&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference=primary")]
    [InlineData("root", "password123", "mydocdbserver", "secondary", false, DocumentDbCertIdentifier.rds_ca_2019, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&retryWrites=False&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference=secondary")]
    [InlineData("root", "password123", "mydocdbserver", "secondary", true, DocumentDbCertIdentifier.rds_ca_2019, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&retryWrites=False&sslVerifyCertificate=false")]
    [InlineData("sa", "password456", "mydocdbserver", "primary", false, DocumentDbCertIdentifier.rds_ca_rsa2048_g1, "mongodb://sa:password456@mydocdbserver:27017/?ssl=true&retryWrites=False&ssl_ca_certs=global-bundle.pem&replicaSet=rs0&readPreference=primary")]
    [InlineData("root", "password123", "mydocdbserver", "secondary", false, DocumentDbCertIdentifier.rds_ca_rsa2048_g1, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&retryWrites=False&ssl_ca_certs=global-bundle.pem&replicaSet=rs0&readPreference=secondary")]
    [InlineData("root", "password123", "mydocdbserver", "secondary", true, DocumentDbCertIdentifier.rds_ca_rsa2048_g1, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&retryWrites=False&sslVerifyCertificate=false")]

    [Theory]
    public void ConnectionString(string userName, string password, string hostName, string readPreference, bool fromLocalHostWithSsh, DocumentDbCertIdentifier documentDbRegistration, string expectedConnectionString)
    {
        Assert.Equal(expectedConnectionString, EncryptedMongoConnectionStringBuilder(userName, password, hostName, documentDbRegistration, readPreference: readPreference, runningFromLocalHostWithSsh: fromLocalHostWithSsh));
    }

    [InlineData("sa", "password456", "mydocdbserver", false, 27017, DocumentDbCertIdentifier.rds_ca_2019, "mongodb://sa:password456@mydocdbserver:27017/?ssl=true&retryWrites=False&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference=primary")]
    [InlineData("root", "password123", "mydocdbserver", false, 27017, DocumentDbCertIdentifier.rds_ca_2019, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&retryWrites=False&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference=primary")]
    [InlineData("root", "password123", "mydocdbserver", false, 9999, DocumentDbCertIdentifier.rds_ca_2019, "mongodb://root:password123@mydocdbserver:9999/?ssl=true&retryWrites=False&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference=primary")]
    [InlineData("root", "password123", "mydocdbserver", true, 27017, DocumentDbCertIdentifier.rds_ca_2019, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&retryWrites=False&sslVerifyCertificate=false")]
    [InlineData("sa", "password456", "mydocdbserver", false, 27017, DocumentDbCertIdentifier.rds_ca_rsa2048_g1, "mongodb://sa:password456@mydocdbserver:27017/?ssl=true&retryWrites=False&ssl_ca_certs=global-bundle.pem&replicaSet=rs0&readPreference=primary")]
    [InlineData("root", "password123", "mydocdbserver", false, 27017, DocumentDbCertIdentifier.rds_ca_rsa2048_g1, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&retryWrites=False&ssl_ca_certs=global-bundle.pem&replicaSet=rs0&readPreference=primary")]
    [InlineData("root", "password123", "mydocdbserver", false, 9999, DocumentDbCertIdentifier.rds_ca_rsa2048_g1, "mongodb://root:password123@mydocdbserver:9999/?ssl=true&retryWrites=False&ssl_ca_certs=global-bundle.pem&replicaSet=rs0&readPreference=primary")]
    [InlineData("root", "password123", "mydocdbserver", true, 27017, DocumentDbCertIdentifier.rds_ca_rsa2048_g1, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&retryWrites=False&sslVerifyCertificate=false")]
    [Theory]
    public void ConnectionStringWithDefaultValue(string userName, string password, string hostName, bool fromLocalHostWithSsh, int hostPort, DocumentDbCertIdentifier documentDbRegistration, string expectedConnectionString)
    {
        Assert.Equal(expectedConnectionString, EncryptedMongoConnectionStringBuilder(userName, password, hostName, documentDbRegistration, port: hostPort, runningFromLocalHostWithSsh: fromLocalHostWithSsh));
    }

}
