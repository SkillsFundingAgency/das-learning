CREATE TABLE [dbo].[ShortCourseEpisode]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [LearningKey] UNIQUEIDENTIFIER NOT NULL,
    [Ukprn] BIGINT NOT NULL,
    [EmployerAccountId] BIGINT NOT NULL,
    [TrainingCode] NCHAR(10) NOT NULL
)
GO

ALTER TABLE dbo.ShortCourseEpisode
ADD CONSTRAINT FK_ShortCourseEpisode_ShortCourseLearning
    FOREIGN KEY (LearningKey)
    REFERENCES dbo.ShortCourseLearning ([Key])
GO

CREATE INDEX IX_ShortCourseEpisode_LearningKey
    ON [dbo].[ShortCourseEpisode] ([LearningKey])
GO

CREATE NONCLUSTERED INDEX IX_ShortCourseEpisode_Ukprn
    ON [dbo].[ShortCourseEpisode] ([Ukprn])
GO
