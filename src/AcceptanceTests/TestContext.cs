using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Newtonsoft.Json;
using SFA.DAS.Encoding;
using SFA.DAS.Learning.TestHelpers;

namespace SFA.DAS.Learning.AcceptanceTests
{
    public class TestContext : IDisposable
    {
        public TestFunction? TestFunction { get; set; }
        public SqlDatabase? SqlDatabase { get; set; }
        public TestMessageSession MessageSession { get; set; }
        public TestInnerApi TestInnerApi { get; set; }

        public void Dispose()
        {
            TestFunction?.Dispose();
            SqlDatabase?.Dispose();
        }

#pragma warning disable CS8618 // Non-nullable fields - are initialized externally
        public TestContext()
        {
            MessageSession = new TestMessageSession();
        }
#pragma warning restore CS8618 // Non-nullable 

        internal IConfigurationRoot GenerateConfiguration()
        {
            var configSource = new MemoryConfigurationSource
            {
                InitialData = new[]
                {
                    new KeyValuePair<string, string?>("EnvironmentName", "LOCAL_ACCEPTANCE_TESTS"),
                    new KeyValuePair<string, string?>("AzureWebJobsStorage", "UseDevelopmentStorage=true"),
                    new KeyValuePair<string, string?>("AzureWebJobsServiceBus", "UseDevelopmentStorage=true"),
                    new KeyValuePair<string, string?>("ApplicationSettings:NServiceBusConnectionString", "UseLearningEndpoint=true"),
                    new KeyValuePair<string, string?>("ApplicationSettings:LogLevel", "DEBUG"),
                    new KeyValuePair<string, string?>("ApplicationSettings:DbConnectionString", SqlDatabase?.DatabaseInfo.ConnectionString!),
                    new KeyValuePair<string, string?>("SFA.DAS.Encoding", MockEncodingConfig())
                }
            };

            var provider = new MemoryConfigurationProvider(configSource);
            return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
        }

        private static string MockEncodingConfig()
        {
            var config = new List<object>();

            foreach (var encodingType in Enum.GetValues(typeof(EncodingType)))
            {
                config.Add(new { EncodingType = encodingType, Salt = "AnyString", MinHashLength = 6, Alphabet = "ABCDEFGHJKMNPRSTUVWXYZ23456789" });
            }

            return JsonConvert.SerializeObject(new { Encodings = config });
        }
    }
}
