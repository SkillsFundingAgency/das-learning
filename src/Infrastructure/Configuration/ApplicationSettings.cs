using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Learning.Infrastructure.Configuration;

#pragma warning disable CS8618
[ExcludeFromCodeCoverage]
public class ApplicationSettings
{
    public string AzureWebJobsStorage { get; set; }
    public string NServiceBusConnectionString { get; set; }
    public string NServiceBusLicense { get; set; }
    public string LearningTransportStorageDirectory { get; set; }
    public string DbConnectionString { get; set; }
    public LearningOuterApiConfiguration LearningOuterApiConfiguration { get; set; }
}
#pragma warning restore CS8618