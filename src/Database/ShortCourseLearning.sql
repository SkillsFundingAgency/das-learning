CREATE TABLE [dbo].[ShortCourseLearning]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [LearnerKey] UNIQUEIDENTIFIER NOT NULL,
    [ExpectedEndDate] DATETIME NOT NULL,
    [WithdrawalDate] DATETIME NULL,
    [CompletionDate] DATETIME NULL,
    [IsApproved] BIT NOT NULL DEFAULT 0
)
GO