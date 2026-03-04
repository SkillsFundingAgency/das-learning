CREATE TABLE [dbo].[ShortCourseMilestone]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [EpisodeKey] UNIQUEIDENTIFIER NOT NULL,
    [Milestone] NVARCHAR(50) NOT NULL
)
GO

ALTER TABLE [dbo].[ShortCourseMilestone]
ADD CONSTRAINT FK_ShortCourseMilestone_ShortCourseEpisode
    FOREIGN KEY ([EpisodeKey])
    REFERENCES [dbo].[ShortCourseEpisode] ([Key])
GO

CREATE INDEX IX_ShortCourseMilestone_EpisodeKey
    ON [dbo].[ShortCourseMilestone] ([EpisodeKey])
GO