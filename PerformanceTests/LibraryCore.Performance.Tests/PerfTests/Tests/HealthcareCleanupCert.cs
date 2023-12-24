using BenchmarkDotNet.Attributes;
using System.Text;
using static LibraryCore.Performance.Tests.Program;

namespace LibraryCore.Performance.Tests.PerfTests;

[SimpleJob]
[Config(typeof(Config))]
[MemoryDiagnoser]
//[ReturnValueValidator(failOnError: true)]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.SlowestToFastest)]
public class HealthcareCleanupCert
{
    internal const string PrivateKeyTester = """
                                -----BEGIN PRIVATE KEY-----
                                MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCsswIa2WMvORRU
                                0Tdmjrguo3qNmH8gOc6Jq82clI7GHzztKcy738//X6PNmBRycelXC+tDVD4AaXZ1
                                ztuGAdMp0T5rMG5+4cR+RRCHxRkaszR2WLUW64O/IMKkPKsFJni/bTey4H8zDPsK
                                1IKVwSBkuRB0EF24CTsEeCkVmzBpE8Hcg3qNOr6kWYyz8fLWkbTbmTeWTahRG2uc
                                IES8fqdHAUsMuhujWtJ/U/tRW9uBdJtxbJKKt/ksfUQZarCqFEGSWF5zLTemaeat
                                vsMjBzLke5xOnf9vz0pe5fOkAHRYmxBDFVEqJIzPkINna6rL0/k7yHgGUWPxxCHi
                                4RAfk/zXAgMBAAECggEABWi4ligHkpZnwUhUQzgRI2onZSvzlLTQIpZo+Dsxmu77
                                ijTBntjMZk6T43/x6VZmspKYVHbdAkegrYvvFZa/tb2XW5AcGzPtuEQS183KktWD
                                Z87bd9OklWXsnWxGXFZEzeoZJoHK5MnGoHjg0NlIWcnvbqiHPEO9v3lsAACTrEF5
                                qFiYxEKj3il4B/HiB7jDG9JpvZ3sjMM2j7uIp+V/K2HOFf4GP4yJNoM22jrwvrc9
                                8qUpL163tJTYLiM1wqFOkm+DAWfhWOXahsZ+MK9n8CBsFbYZt2VSFrntkfq8N8fH
                                poQ3/sjmz42RMDxlD4Y+aWl064oHfE3kPLrBjzEtEQKBgQDTWiAnhzzgoGVmhmB+
                                Z236U6OG/Y5+EqtqcIV0Yfz2x/RTqV2iShd7CUgY+ceSyQUvTt7QFbKM+WDYfh03
                                g2ZZSgceJuQkY3g8Nr/lqNvDis5P9XkK76JoLErlcXRdfTA9HSkTHngdr17/fcvX
                                dtN1Aa6z1kB5MQXsv4fTt99nhQKBgQDRLowXoordADY5dRUoaCPKlpHOMTD32zF7
                                Rh2iR50pB0j6vTnbLdjn1ZvNYECFCupkf/PMXkB8qpp0YW1pyhcZyQN/igi2CxZ8
                                vYptK1EFe7iUVYvK00XkGqh8351bzV3CUOytaphdHCfHM1d+s5+UJn6+qMF4TipP
                                as/DWqSrqwKBgHJzDpB5MmUtVluzjtNEbRCudBIogh4RPBPyhMImQdbdJpibUWIN
                                nzN4LjugHwuEobMhkZS6+ypN7LyvZmDmXekiJkKUqtxtpPFucHIMA52bL+eqtPZQ
                                F/EzcwdWbb4miZW+kbyRtlfU3Kub0mOGLDHjTFiEP/ugqWWvvfzPSEIFAoGAIRYZ
                                totRJ4+nBAOO6ys9rgeO3GxPcITNGNCIU2i8ZQbv9ikEA7UGv/S7hgqZafHGDUtn
                                xoWJ8a4SbCIU6ky6xqtS0FOG+TRbxDM+t/HICPP+yAeX6ubsAR5uVAISo7iiAeBG
                                708S9ndDm3B1gGbs8MP9/C45G8xRSOp+HkoyDa0CgYEAhXGYj0JvE6y+GTx0xhi0
                                vf4b6MF9kiF9rYT3/88Ki1Wty4Kr2DHh2AT5dC8H31xq8jZF1YsTiKJwokPCCjyZ
                                bU8JyNy7JhFqSwOftFYY0Mms7bZw74z8y46QLXWEo/oqCtCKm+x2tg+xzpTRxoia
                                G7vS5dSB0W+KFEG9RPbtgnM=
                                -----END PRIVATE KEY-----
                                """;

    [Benchmark(Baseline = true)]
    public ReadOnlySpan<byte> StringBuilder()
    {
        var stringBuilder = new StringBuilder(PrivateKeyTester)
                           .Replace("-----BEGIN PRIVATE KEY-----", string.Empty)
                           .Replace("-----END PRIVATE KEY-----", string.Empty);

        return Convert.FromBase64String(stringBuilder.ToString());
    }

    [Benchmark]
    public ReadOnlySpan<byte> StringReplaceWithSpan()
    {
        const string headerText = "-----BEGIN PRIVATE KEY-----";
        const string footerText = "-----END PRIVATE KEY-----";
        var cert = PrivateKeyTester.AsSpan();

        var topHeader = cert.IndexOf(headerText);
        var bottomHeader = cert.IndexOf(footerText);

        return Convert.FromBase64String(new string(cert[(topHeader + headerText.Length)..bottomHeader]));
    }

    [Benchmark]
    public ReadOnlySpan<byte> SpanWithNoBuffer()
    {
        var keyAsSpan = PrivateKeyTester.AsSpan();

        //Find the index after the header which has the -----BEGIN PRIVATE KEY-----
        var indexAfterHeaderContent = keyAsSpan.IndexOf(Environment.NewLine);

        //grab the last line break which has the -----END PRIVATE KEY-----
        var lastPageIndex = keyAsSpan.LastIndexOf(Environment.NewLine);

        //grab everything between the begin private key and end private key
        return Convert.FromBase64String(new string(keyAsSpan[indexAfterHeaderContent..lastPageIndex]));
    }

    [Benchmark]
    public ReadOnlySpan<byte> SpanWithBuffer()
    {
        var keyAsSpan = PrivateKeyTester.AsSpan();

        //Find the index after the header which has the -----BEGIN PRIVATE KEY-----
        var indexAfterHeaderContent = keyAsSpan.IndexOf(Environment.NewLine);

        //grab the last line break which has the -----END PRIVATE KEY-----
        var lastPageIndex = keyAsSpan.LastIndexOf(Environment.NewLine);

        //grab everything between the begin private key and end private key
        var slicedToWhatWeNeed = keyAsSpan[indexAfterHeaderContent..lastPageIndex];

        //going to get fancy and try not to allocate anything (in base64 every char encodes 6 bits, so 4 chars = 3 bytes)
        var buffer = new Span<byte>(new byte[((slicedToWhatWeNeed.Length * 3) + 3) / 4]);

        if (!Convert.TryFromBase64Chars(slicedToWhatWeNeed, buffer, out _))
        {
            throw new Exception("Cant' Convert");
        }

        return buffer;
    }
}
