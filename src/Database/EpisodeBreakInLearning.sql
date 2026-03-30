CREATE TABLE [dbo].[EpisodeBreakInLearning]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [EpisodeKey] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME NOT NULL,
    [EndDate] DATETIME NOT NULL, 
    [PriorPeriodExpectedEndDate] DATETIME NOT NULL
);
GO

ALTER TABLE dbo.EpisodeBreakInLearning
ADD CONSTRAINT FK_EpisodeBreakInLearning_Episode
    FOREIGN KEY (EpisodeKey) REFERENCES dbo.ApprenticeshipEpisode ([Key]);
GO

CREATE NONCLUSTERED INDEX IX_EpisodeBreakInLearning_EpisodeKey
    ON [dbo].[EpisodeBreakInLearning] ([EpisodeKey]);
GO