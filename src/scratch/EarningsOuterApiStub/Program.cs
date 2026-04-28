var builder = WebApplication.CreateBuilder(args);

// The Earnings Inner API expects the Earnings Outer API on https://localhost:7101
builder.WebHost.UseUrls("https://localhost:7101");

var app = builder.Build();

app.MapGet("/TrainingCourses/standards/{courseCode}", (string courseCode) => 
{
    // Return a mocked GetStandardResponse that the Earnings Inner API expects
    return Results.Ok(new 
    {
        StandardUId = $"ST{courseCode}_1.0",
        IfateReferenceNumber = $"ST{courseCode}",
        LarsCode = int.TryParse(courseCode, out var code) ? code : 123,
        Status = "Approved for delivery",
        Title = "Mock Standard",
        Level = 3,
        Version = "1.0",
        VersionMajor = 1,
        VersionMinor = 0,
        MaxFunding = 15000,
        EffectiveFrom = new DateTime(2020, 1, 1),
        ApprenticeshipFunding = new[] 
        {
            new 
            {
                MaxEmployerLevyCap = 15000,
                EffectiveFrom = new DateTime(2020, 1, 1),
                EffectiveTo = (DateTime?)null,
                Duration = 24
            }
        }
    });
});

app.Run();
