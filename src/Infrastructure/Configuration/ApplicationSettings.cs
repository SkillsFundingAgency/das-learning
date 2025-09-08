using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Learning.Infrastructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public class ApplicationSettings
    {
        public string AzureWebJobsStorage { get; set; }
        public string NServiceBusConnectionString { get; set; }
        public string NServiceBusLicense { get; set; }
        public string LearningTransportStorageDirectory { get; set; }
        public string DbConnectionString { get; set; }
        public ApprenticeshipsOuterApiConfiguration ApprenticeshipsOuterApiConfiguration { get; set; }
    }
}
