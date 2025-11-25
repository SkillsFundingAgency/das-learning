CREATE TABLE [dbo].[EpisodeBreakInLearning]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [EpisodeKey] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME NOT NULL,
    [EndDate] DATETIME NOT NULL
);
GO

ALTER TABLE dbo.EpisodeBreakInLearning
ADD CONSTRAINT FK_EpisodeBreakInLearning_Episode 
    FOREIGN KEY (EpisodeKey) REFERENCES dbo.Episode ([Key]);
GO