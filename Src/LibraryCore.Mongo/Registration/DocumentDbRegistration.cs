using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;

namespace LibraryCore.Mongo.Registration;

/// <summary>
/// Helper methods to deal with document db and encrypted in transit and at reset databases
/// </summary>
public static class DocumentDbRegistration
{
    /// <summary>
    /// For encrypted at rest databases in lambdas
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static IMongoClient RegisterInLambdaEnvironment(string mongoConnectionString)
    {
        var settings = MongoClientSettings.FromUrl(new MongoUrl(mongoConnectionString));

        settings.SslSettings = new SslSettings
        {
            ClientCertificates = new X509Certificate[]
            {
                 //use the pem file here
                 new X509Certificate(Properties.Resources.rds_combined_ca_bundlepem)
            }
        };

        return new MongoClient(settings);
    }

    /// <summary>
    /// For encrypted at rest databases in ec2, containers, or anything where you can install the certificate
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static IMongoClient RegisterInWritableEnvironment(string connectionString, StoreName storeToLoad = StoreName.Root, bool allowInsecureTlsForSshIntoAws = false)
    {
        using var localTrustStore = new X509Store(storeToLoad); //isLocalDev ? new X509Store(StoreName.My) : new X509Store(StoreName.Root);
        var certificateCollection = new X509Certificate2Collection();
        certificateCollection.Import(Properties.Resources.rds_combined_ca_bundlep7b);

        localTrustStore.Open(OpenFlags.ReadWrite);
        localTrustStore.AddRange(certificateCollection);

        var settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));

        settings.AllowInsecureTls = allowInsecureTlsForSshIntoAws;

        return new MongoClient(settings);
    }

    public static string EncryptedMongoConnectionStringBuilder(string userName, string password, string dbHostName, string readPreference = "primary") =>
        $"mongodb://{userName}:{password}@{dbHostName}:27017/?ssl=true&ssl_ca_certs=rds-combined-ca-bundle.pem&replicaSet=rs0&readPreference={readPreference}&retryWrites=false";

}
