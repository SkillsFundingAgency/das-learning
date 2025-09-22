CREATE TABLE [dbo].[EpisodePrice]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[EpisodeKey] UNIQUEIDENTIFIER NOT NULL, 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NOT NULL, 
    [TrainingPrice] MONEY NULL, 
    [EndPointAssessmentPrice] MONEY NULL, 
    [TotalPrice] MONEY NULL, 
    [FundingBandMaximum] INT NOT NULL
)
GO
ALTER TABLE dbo.EpisodePrice
ADD CONSTRAINT FK_EpisodePrice_Episode FOREIGN KEY (EpisodeKey) REFERENCES dbo.Episode ([Key])
GO
CREATE NONCLUSTERED INDEX [IX_StartDateEndDate] ON [dbo].[EpisodePrice] ([EpisodeKey] ASC, [StartDate] ASC,[EndDate] ASC)
GO
