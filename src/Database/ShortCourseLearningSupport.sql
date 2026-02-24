CREATE TABLE [dbo].[ShortCourseLearningSupport]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [LearningKey] UNIQUEIDENTIFIER NOT NULL,
    [EpisodeKey] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME NOT NULL,
    [EndDate] DATETIME NOT NULL
)
GO

ALTER TABLE [dbo].[ShortCourseLearningSupport]
ADD CONSTRAINT FK_ShortCourseLearningSupport_Learning FOREIGN KEY (LearningKey) REFERENCES dbo.ShortCourseLearning ([Key])
GO

CREATE INDEX IX_LearningKey ON [dbo].[ShortCourseLearningSupport] (LearningKey);
GO

GO
ALTER TABLE [dbo].[ShortCourseLearningSupport]
ADD CONSTRAINT FK_ShortCourseLearningSupport_Episode FOREIGN KEY (EpisodeKey) REFERENCES dbo.ShortCourseEpisode ([Key])
GO
