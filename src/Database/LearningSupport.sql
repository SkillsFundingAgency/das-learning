CREATE TABLE [dbo].[LearningSupport]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [LearningKey] UNIQUEIDENTIFIER NOT NULL,
    [EpisodeKey] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME NOT NULL,
    [EndDate] DATETIME NOT NULL
)
GO

ALTER TABLE [dbo].[LearningSupport]
ADD CONSTRAINT FK_LearningSupport_Learning FOREIGN KEY (LearningKey) REFERENCES dbo.ApprenticeshipLearning ([Key])
GO

CREATE INDEX IX_LearningKey ON [dbo].[LearningSupport] (LearningKey);
GO

GO
ALTER TABLE [dbo].[LearningSupport]
ADD CONSTRAINT FK_LearningSupport_Episode FOREIGN KEY (EpisodeKey) REFERENCES dbo.ApprenticeshipEpisode ([Key])
GO
