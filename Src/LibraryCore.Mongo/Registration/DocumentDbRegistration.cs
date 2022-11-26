using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;

namespace LibraryCore.Mongo.Registration;

[ExcludeFromCodeCoverage]
public static class DocumentDbRegistration
{
    public static void LoadAwsRdsCertForDocumentDb(StoreName storeToLoad = StoreName.Root)
    {
        // string pathToCAFile = Path.Combine("AwsDbResources", "rds-combined-ca-bundle.p7b");

        //if (!File.Exists(pathToCAFile))
        //{
        //    Console.WriteLine("Can't find RDS Bundle. Linux is case sensitive! Path Searched = " + pathToCAFile);
        //}

        using var localTrustStore = new X509Store(storeToLoad); //isLocalDev ? new X509Store(StoreName.My) : new X509Store(StoreName.Root);
        var certificateCollection = new X509Certificate2Collection();
        certificateCollection.Import(Properties.Resources.rds_combined_ca_bundle);// pathToCAFile);

        localTrustStore.Open(OpenFlags.ReadWrite);
        localTrustStore.AddRange(certificateCollection);
    }
}
