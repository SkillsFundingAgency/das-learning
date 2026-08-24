DECLARE @Uln NVARCHAR(10) = '9999999999';
DECLARE @LearnerKey UNIQUEIDENTIFIER;

SELECT @LearnerKey = [Key]
FROM dbo.Learner
WHERE Uln = @Uln;

IF @LearnerKey IS NULL
BEGIN
    PRINT CONCAT('No learner found for ULN ', @Uln, '. Nothing to delete.');
    RETURN;
END

DECLARE @ApprenticeshipLearningKeys TABLE ([Key] UNIQUEIDENTIFIER PRIMARY KEY);
DECLARE @ShortCourseLearningKeys TABLE ([Key] UNIQUEIDENTIFIER PRIMARY KEY);

INSERT INTO @ApprenticeshipLearningKeys ([Key])
SELECT [Key]
FROM dbo.ApprenticeshipLearning
WHERE LearnerKey = @LearnerKey;

INSERT INTO @ShortCourseLearningKeys ([Key])
SELECT [Key]
FROM dbo.ShortCourseLearning
WHERE LearnerKey = @LearnerKey;

BEGIN TRANSACTION;

    DELETE ebl
    FROM dbo.EpisodeBreakInLearning ebl
    INNER JOIN dbo.ApprenticeshipEpisode ae ON ae.[Key] = ebl.EpisodeKey
    INNER JOIN @ApprenticeshipLearningKeys alk ON alk.[Key] = ae.LearningKey;

    DELETE ep
    FROM dbo.EpisodePrice ep
    INNER JOIN dbo.ApprenticeshipEpisode ae ON ae.[Key] = ep.EpisodeKey
    INNER JOIN @ApprenticeshipLearningKeys alk ON alk.[Key] = ae.LearningKey;

    DELETE embil
    FROM dbo.EnglishAndMathsBreakInLearning embil
    INNER JOIN dbo.EnglishAndMaths em ON em.[Key] = embil.EnglishAndMathsKey
    INNER JOIN @ApprenticeshipLearningKeys alk ON alk.[Key] = em.LearningKey;

    DELETE als
    FROM dbo.ApprenticeshipLearningSupport als
    INNER JOIN @ApprenticeshipLearningKeys alk ON alk.[Key] = als.LearningKey;

    DELETE em
    FROM dbo.EnglishAndMaths em
    INNER JOIN @ApprenticeshipLearningKeys alk ON alk.[Key] = em.LearningKey;

    DELETE lh
    FROM [History].[LearningHistory] lh
    INNER JOIN @ApprenticeshipLearningKeys alk ON alk.[Key] = lh.LearningId;

    DELETE ae
    FROM dbo.ApprenticeshipEpisode ae
    INNER JOIN @ApprenticeshipLearningKeys alk ON alk.[Key] = ae.LearningKey;

    DELETE al
    FROM dbo.ApprenticeshipLearning al
    INNER JOIN @ApprenticeshipLearningKeys alk ON alk.[Key] = al.[Key];

    DELETE scls
    FROM dbo.ShortCourseLearningSupport scls
    INNER JOIN @ShortCourseLearningKeys sclk ON sclk.[Key] = scls.LearningKey;

    DELETE scm
    FROM dbo.ShortCourseMilestone scm
    INNER JOIN dbo.ShortCourseEpisode sce ON sce.[Key] = scm.EpisodeKey
    INNER JOIN @ShortCourseLearningKeys sclk ON sclk.[Key] = sce.LearningKey;

    DELETE sce
    FROM dbo.ShortCourseEpisode sce
    INNER JOIN @ShortCourseLearningKeys sclk ON sclk.[Key] = sce.LearningKey;

    DELETE scl
    FROM dbo.ShortCourseLearning scl
    INNER JOIN @ShortCourseLearningKeys sclk ON sclk.[Key] = scl.[Key];

    DELETE l
    FROM dbo.Learner l
    WHERE l.[Key] = @LearnerKey;

--COMMIT TRANSACTION;
ROLLBACK TRANSACTION;
