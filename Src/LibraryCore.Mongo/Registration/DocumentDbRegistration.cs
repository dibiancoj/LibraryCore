using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LibraryCore.Mongo.Registration;

/// <summary>
/// Helper methods to deal with document db and encrypted in transit and at reset databases
/// </summary>
public static class DocumentDbRegistration
{
    public enum DocumentDbCertIdentifier
    {
        rds_ca_2019,
        rds_ca_rsa2048_g1
    }

    //to ssh with port forwarding
    //ssh -i "my-dev-ssh-key.pem" -L 27017:mycluster.us-east-1.docdb.amazonaws.com:27017 ec2-user@10.1.1.1 -N

    /// <summary>
    /// For encrypted at rest databases in lambdas
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static IMongoClient RegisterInLambdaEnvironment(string mongoConnectionString, DocumentDbCertIdentifier documentDbCertIdentifier, bool runningFromLocalHostWithSsh = false)
    {
        var settings = MongoClientSettings.FromUrl(new MongoUrl(mongoConnectionString));

        settings.SslSettings = new SslSettings
        {
            ClientCertificates = new X509Certificate[]
            {
                 //use the pem file here
                 new(CaCertIdentifierFromResource(documentDbCertIdentifier, true))
            }
        };

        settings.AllowInsecureTls = runningFromLocalHostWithSsh;

        return new MongoClient(settings);
    }

    /// <summary>
    /// For encrypted at rest databases in ec2, containers, or anything where you can install the certificate
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static IMongoClient RegisterInWritableEnvironment(string connectionString,
                                                             DocumentDbCertIdentifier documentDbCertIdentifier,
                                                             StoreName storeToLoad = StoreName.Root,
                                                             bool runningFromLocalHostWithSsh = false)
    {
        using var localTrustStore = new X509Store(storeToLoad); //isLocalDev ? new X509Store(StoreName.My) : new X509Store(StoreName.Root);
        var certificateCollection = new X509Certificate2Collection();
        certificateCollection.Import(CaCertIdentifierFromResource(documentDbCertIdentifier, false));

        localTrustStore.Open(OpenFlags.ReadWrite);
        localTrustStore.AddRange(certificateCollection);

        var settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));

        settings.AllowInsecureTls = runningFromLocalHostWithSsh;

        return new MongoClient(settings);
    }

    //retry writes aren't supported in document db. Always have that false unless your using the real mongo
    public static string EncryptedMongoConnectionStringBuilder(string userName,
                                                               string password,
                                                               string dbHostName,
                                                               DocumentDbCertIdentifier documentDbCertIdentifier,
                                                               string readPreference = "primary",
                                                               bool runningFromLocalHostWithSsh = false,
                                                               int port = 27017,
                                                               bool retryWrites = false)
    {
        var builder = new StringBuilder($"mongodb://{userName}:{password}@{dbHostName}:{port}/?ssl=true&retryWrites={retryWrites}");

        if (runningFromLocalHostWithSsh)
        {
            builder.Append("&sslVerifyCertificate=false");
        }
        else
        {
            var identifierToFileName = documentDbCertIdentifier == DocumentDbCertIdentifier.rds_ca_2019 ?
                                                             "rds-combined-ca-bundle.pem" :
                                                             "global-bundle.pem";

            builder.Append($"&ssl_ca_certs={identifierToFileName}&replicaSet=rs0&readPreference={readPreference}");
        }

        return builder.ToString();
    }

    private static byte[] CaCertIdentifierFromResource(DocumentDbCertIdentifier documentDbCertIdentifier, bool pemVersion) =>
        (documentDbCertIdentifier, pemVersion) switch
        {
            (DocumentDbCertIdentifier.rds_ca_2019, false) => Properties.Resources.rds_combined_ca_bundlep7b,
            (DocumentDbCertIdentifier.rds_ca_2019, true) => Properties.Resources.rds_combined_ca_bundlepem,

            (DocumentDbCertIdentifier.rds_ca_rsa2048_g1, false) => Properties.Resources.global_bundlep7b,
            (DocumentDbCertIdentifier.rds_ca_rsa2048_g1, true) => Properties.Resources.global_bundlepem,

            _ => throw new NotImplementedException()
        };

}
