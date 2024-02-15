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
    /// <summary>
    /// What type of encryption at rest ca cert are you using
    /// </summary>
    public enum DocumentDbCertIdentifier
    {
        rds_ca_2019,
        rds_ca_rsa2048_g1
    }

    //to ssh with port forwarding
    //ssh -i "my-dev-ssh-key.pem" -L 27017:mycluster.us-east-1.docdb.amazonaws.com:27017 ec2-user@10.1.1.1 -N

    private static Dictionary<DocumentDbCertIdentifier, X509Certificate[]> CachedDocumentDbCertIdentifierLookup { get; } = new()
    {
        { DocumentDbCertIdentifier.rds_ca_2019, new X509Certificate[] { new (CaCertIdentifierFromResource(DocumentDbCertIdentifier.rds_ca_2019, true)) } },
        { DocumentDbCertIdentifier.rds_ca_rsa2048_g1, new X509Certificate[] { new (CaCertIdentifierFromResource(DocumentDbCertIdentifier.rds_ca_rsa2048_g1, true)) } }
    };

    /// <summary>
    /// Create a mongo client with encryption at rest. Use this overload if the cert attached to the database is not specified in the overload
    /// </summary>
    /// <param name="mongoConnectionString">Main connection string. Use EncryptedMongoConnectionStringBuilder if necessary</param>
    /// <param name="caPublicCertIdentifierContent">Ca public cert content.</param>
    /// <param name="allowInsecureTls">Are you connecting to mongo using ssh on your local host. This will set the cert chain to not verify because of the cert chain issue with the ssh tunnel</param>
    /// <returns>IMongoClient To Use</returns>
    [ExcludeFromCodeCoverage]
    public static IMongoClient CreateMongoClient(string mongoConnectionString, byte[] caPublicCertIdentifierContent, bool allowInsecureTls = false)
    {
        return CreateMongoClient(mongoConnectionString, [new X509Certificate(caPublicCertIdentifierContent)], allowInsecureTls);
    }

    /// <summary>
    /// Create a mongo client with encryption at rest
    /// </summary>
    /// <param name="mongoConnectionString">Main connection string. Use EncryptedMongoConnectionStringBuilder if necessary</param>
    /// <param name="documentDbCertIdentifier">Ca public cert identifier</param>
    /// <param name="allowInsecureTls">Are you connecting to mongo using ssh on your local host. This will set the cert chain to not verify because of the cert chain issue with the ssh tunnel</param>
    /// <returns>IMongoClient To Use</returns>
    [ExcludeFromCodeCoverage]
    public static IMongoClient CreateMongoClient(string mongoConnectionString, DocumentDbCertIdentifier documentDbCertIdentifier, bool allowInsecureTls = false)
    {
        return CreateMongoClient(mongoConnectionString, CachedDocumentDbCertIdentifierLookup[documentDbCertIdentifier], allowInsecureTls);
    }

    private static IMongoClient CreateMongoClient(string mongoConnectionString, X509Certificate[] x509CertContent, bool allowInsecureTls = false)
    {
        var settings = MongoClientSettings.FromUrl(new MongoUrl(mongoConnectionString));

        settings.SslSettings = new SslSettings
        {
            ClientCertificates = x509CertContent
        };

        settings.AllowInsecureTls = allowInsecureTls;

        return new MongoClient(settings);
    }

    /// <summary>
    /// Create a mongo connection string for document db
    /// </summary>
    /// <param name="userName">username to log into the database with</param>
    /// <param name="password">password to log into the database with</param>
    /// <param name="dbHostName">The database host name</param>
    /// <param name="readPreference">read preference which is defaulted to primary. You can optionally use a read replica</param>
    /// <param name="runningFromLocalHostWithSsh">Are you connecting to mongo using ssh on your local host. This will set the cert chain to not verify because of the cert chain issue with the ssh tunnel</param>
    /// <param name="port">Port to connect to the database which defaults to 27017</param>
    /// <param name="retryWrites">Retry writes aren't supported in document db. Always have that false unless your using the real mongo. The default value is false</param>
    /// <returns>Connection string with all the required parameters</returns>
    public static string EncryptedMongoConnectionStringBuilder(string userName,
                                                               string password,
                                                               string dbHostName,
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

        return builder.Append($"&replicaSet=rs0&readPreference={readPreference}")
                    .ToString();
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
