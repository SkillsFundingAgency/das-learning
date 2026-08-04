
DECLARE @Force BIT = 1;                          -- 1 or 0
DECLARE @LearningKey UNIQUEIDENTIFIER = NULL;     -- optional
DECLARE @Ukprn BIGINT = NULL;                     -- optional

IF @LearningKey IS NULL AND @Ukprn IS NULL
BEGIN
    RAISERROR('Either @LearningKey or @Ukprn must be provided.', 16, 1);
    RETURN;
END

BEGIN TRANSACTION;


    UPDATE dbo.ShortCourseEpisode
    SET ForceEarningsSync = @Force
    WHERE (@LearningKey IS NULL OR LearningKey = @LearningKey)
      AND (@Ukprn IS NULL OR Ukprn = @Ukprn);

--COMMIT TRANSACTION;

ROLLBACK TRANSACTION;

