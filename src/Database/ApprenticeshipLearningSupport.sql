CREATE TABLE [dbo].[ApprenticeshipLearningSupport]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [LearningKey] UNIQUEIDENTIFIER NOT NULL,
    [EpisodeKey] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME NOT NULL,
    [EndDate] DATETIME NOT NULL
)
GO

ALTER TABLE [dbo].[ApprenticeshipLearningSupport]
ADD CONSTRAINT FK_ApprenticeshipLearningSupport_Learning FOREIGN KEY (LearningKey) REFERENCES dbo.ApprenticeshipLearning ([Key])
GO

CREATE INDEX IX_LearningKey ON [dbo].[ApprenticeshipLearningSupport] (LearningKey);
GO

GO
ALTER TABLE [dbo].[ApprenticeshipLearningSupport]
ADD CONSTRAINT FK_ApprenticeshipLearningSupport_Episode FOREIGN KEY (EpisodeKey) REFERENCES dbo.ApprenticeshipEpisode ([Key])
GO
