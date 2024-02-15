using LibraryCore.Mongo.Registration;
using static LibraryCore.Mongo.Registration.DocumentDbRegistration;

namespace LibraryCore.Tests.Mongo;

public class MongoRegistrationTest
{
    [InlineData("sa", "password456", "mydocdbserver", "primary", false, "mongodb://sa:password456@mydocdbserver:27017/?ssl=true&retryWrites=False&replicaSet=rs0&readPreference=primary")]
    [InlineData("root", "password123", "mydocdbserver", "secondary", false, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&retryWrites=False&replicaSet=rs0&readPreference=secondary")]
    [InlineData("root", "password123", "mydocdbserver", "secondary", true, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&retryWrites=False&sslVerifyCertificate=false&replicaSet=rs0&readPreference=secondary")]
    [Theory]
    public void ConnectionString(string userName, string password, string hostName, string readPreference, bool fromLocalHostWithSsh, string expectedConnectionString)
    {
        Assert.Equal(expectedConnectionString, EncryptedMongoConnectionStringBuilder(userName, password, hostName, readPreference: readPreference, runningFromLocalHostWithSsh: fromLocalHostWithSsh));
    }

    [InlineData("sa", "password456", "mydocdbserver", false, 27017, "mongodb://sa:password456@mydocdbserver:27017/?ssl=true&retryWrites=False&replicaSet=rs0&readPreference=primary")]
    [InlineData("root", "password123", "mydocdbserver", false, 27017, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&retryWrites=False&replicaSet=rs0&readPreference=primary")]
    [InlineData("root", "password123", "mydocdbserver", false, 9999, "mongodb://root:password123@mydocdbserver:9999/?ssl=true&retryWrites=False&replicaSet=rs0&readPreference=primary")]
    [InlineData("root", "password123", "mydocdbserver", true, 27017, "mongodb://root:password123@mydocdbserver:27017/?ssl=true&retryWrites=False&sslVerifyCertificate=false&replicaSet=rs0&readPreference=primary")]
    [Theory]
    public void ConnectionStringWithDefaultValue(string userName, string password, string hostName, bool fromLocalHostWithSsh, int hostPort, string expectedConnectionString)
    {
        Assert.Equal(expectedConnectionString, EncryptedMongoConnectionStringBuilder(userName, password, hostName, port: hostPort, runningFromLocalHostWithSsh: fromLocalHostWithSsh));
    }

    [InlineData(DocumentDbCertIdentifier.rds_ca_2019, false)]
    [InlineData(DocumentDbCertIdentifier.rds_ca_2019, true)]
    [InlineData(DocumentDbCertIdentifier.rds_ca_rsa2048_g1, false)]
    [InlineData(DocumentDbCertIdentifier.rds_ca_rsa2048_g1, true)]
    [Theory]
    public void MongoClientWithEnumForCa(DocumentDbCertIdentifier documentDbCertIdentifier, bool runningFromSsh)
    {
        var mongoClient = CreateMongoClient("mongodb://sa:password456@mydocdbserver:27017/?ssl=true&retryWrites=False&replicaSet=rs0&readPreference=primary",
                                                                  documentDbCertIdentifier,
                                                                  allowInsecureTls: runningFromSsh);

        static string PublicKeyForCert(DocumentDbCertIdentifier documentDbCertIdentifier) =>
            documentDbCertIdentifier switch
            {
                DocumentDbCertIdentifier.rds_ca_2019 => "3082010A0282010100E709014A1FC89B81B8FAF92121E2FC76B979DF21DDE19DCE7E0C069EABCAD3CE83D35519142C3BDDDF3460882AC513EA03EE93ED5FD6770C4D5F18D0575FE1AA865A67BE2A19B764E3F75752221A756AD14DB188E9A7490E10ECBA1E402E250BACE9A021D52720E3678A409274107EC5DBEFB4E881DB7FB4AE5A85EBF40FE437802C00A01A0E0C7289EADF36D2A3E2EB701E19B26BB1F3632CA8979453C3523E011A2AF8754CE311E1FBFE1C77EE76B55C6FF711CC4FDFDF5C4F993BB18FC247D91CB4C954C841143503A6862FE5CB52E0B62866DD07E32FA2BD6862C53705F7E41921F5A2F6174E7CBBE9FC0CFFDD3DE29401C24A8906549B3B9E9F99F9EF1F0203010001",
                DocumentDbCertIdentifier.rds_ca_rsa2048_g1 => "3082010A0282010100C11DB7E75B8F31968993680C193FB5B24F0713D310876ADCFEC0A76F11A7DA9D5CECE8BD6816DDFC68858A37896F6057BC753E4CEAB7777263A9EA6AF2D6A95D5B78A236D324DC8227913BAFB2A34E4B4A74BC6D044D4C2B6E5A63E211263B5D53ABC6DCA88F2944F59DA3B7C6CDF2B7417BE9656E162BDDF4E1EFD9AF8A1EF3E4454588C699801EE318F52208326119B5513B6191B1E5A87488EE09CC8B4771B551D32D0F047E796ABA4505D7692151C269CA751543F14845A43A923BAB401A538C5F5F47DED415791D96112A904E229D53B595AF243F978C652D2F5044B13E6F37983D324A9DB58AC2FB748DA4093856DA6F26B763ADA23F861BDF797367030203010001",
                _ => throw new NotImplementedException()
            };

        var expectedPublicKey = PublicKeyForCert(documentDbCertIdentifier);

        Assert.Equal(runningFromSsh, mongoClient.Settings.AllowInsecureTls);
        Assert.Contains(mongoClient.Settings.SslSettings.ClientCertificates, x => x.GetPublicKeyString() == expectedPublicKey);
    }

    [InlineData(false)]
    [InlineData(true)]
    [Theory]
    public void MongoClientWithRawByteArray(bool runningFromSsh)
    {
        var mongoClient = CreateMongoClient("mongodb://sa:password456@mydocdbserver:27017/?ssl=true&retryWrites=False&replicaSet=rs0&readPreference=primary",
                                                                   Properties.Resources.global_bundle,
                                                                   allowInsecureTls: runningFromSsh);

        const string publicKeyString = "3082010A0282010100C11DB7E75B8F31968993680C193FB5B24F0713D310876ADCFEC0A76F11A7DA9D5CECE8BD6816DDFC68858A37896F6057BC753E4CEAB7777263A9EA6AF2D6A95D5B78A236D324DC8227913BAFB2A34E4B4A74BC6D044D4C2B6E5A63E211263B5D53ABC6DCA88F2944F59DA3B7C6CDF2B7417BE9656E162BDDF4E1EFD9AF8A1EF3E4454588C699801EE318F52208326119B5513B6191B1E5A87488EE09CC8B4771B551D32D0F047E796ABA4505D7692151C269CA751543F14845A43A923BAB401A538C5F5F47DED415791D96112A904E229D53B595AF243F978C652D2F5044B13E6F37983D324A9DB58AC2FB748DA4093856DA6F26B763ADA23F861BDF797367030203010001";

        Assert.Equal(runningFromSsh, mongoClient.Settings.AllowInsecureTls);
        Assert.Contains(mongoClient.Settings.SslSettings.ClientCertificates, x => x.GetPublicKeyString() == publicKeyString);
    }
}
