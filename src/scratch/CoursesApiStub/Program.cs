var builder = WebApplication.CreateBuilder(args);

// The LearnerData API expects the Courses API on https://localhost:5002
builder.WebHost.UseUrls("https://localhost:5002");

var app = builder.Build();

app.MapGet("/api/courses/standards/{standardId}", (string standardId) => 
{
    // Return a mocked StandardDetailResponse that the LearnerData API expects
    // when calculating the FundingBandMaximum
    return Results.Ok(new 
    {
        ApprenticeshipFunding = new[] 
        {
            new 
            {
                EffectiveFrom = new DateTime(2000, 1, 1),
                EffectiveTo = new DateTime(2099, 1, 1),
                MaxEmployerLevyCap = 15000
            }
        }
    });
});

app.Run();
