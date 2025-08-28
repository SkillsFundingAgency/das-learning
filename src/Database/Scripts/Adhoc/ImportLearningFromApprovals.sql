SET NOCOUNT ON

-- FLP-1113 Apprenticeships Migration Script
-- This script is to be run against the commitments database
-- As output, it produces another sql script that can be run against the Apprenticeships database
-- Results -> File is recommended. It may be necessary to trim execution time output from the bottom of the output file.
-- Output can be wrapped in BEGIN/COMMIT TRANSACTION statements to improve performance (recommended to do this per-chunk if splitting).

--Variables that can be modified
DECLARE @ChunkSize INT = 50000; -- This adds comment lines into the output, useful for chunking the output up into smaller bits

DECLARE @ApprenticeshipIds TABLE (Id INT);  --Holds a list of apprenticeship ids to target for export/import
INSERT INTO @ApprenticeshipIds (Id)
VALUES 
    (),    (),    (),    (); --insert ids here

-- No other variables should be modified, but a TOP x can be added into the select statement for testing purposes

-- Variables for storing values
DECLARE @ApprovalsApprenticeshipId INT;
DECLARE @Uln BIGINT;
DECLARE @FirstName NVARCHAR(50);
DECLARE @LastName NVARCHAR(50);
DECLARE @DateOfBirth DATE;
DECLARE @Ukprn INT;
DECLARE @EmployerAccountId INT;
DECLARE @FundingType NVARCHAR(20);
DECLARE @FundingEmployerAccountId INT;
DECLARE @LegalEntityName NVARCHAR(255);
DECLARE @AccountLegalEntityId INT;
DECLARE @TrainingCode NVARCHAR(50);
DECLARE @TrainingCourseVersion NVARCHAR(50);
DECLARE @StartDate DATE;
DECLARE @EndDate DATE;
DECLARE @TrainingPrice DECIMAL(18, 2);
DECLARE @EndPointAssessmentPrice DECIMAL(18, 2);
DECLARE @TotalPrice DECIMAL(18, 2);
--Other variables
DECLARE @Output TABLE (OutputLine nvarchar(max), [Order] INT)
DECLARE @Count INT = 0;
DECLARE @ChunkCount INT = 0;

-- Cursor declaration
DECLARE ApprenticeshipCursor CURSOR FAST_FORWARD FOR
SELECT 
--TOP 1000 -- A top x clause can be added for testing with smaller sets of data
    Apps.Id,
    Apps.ULN,
    Apps.FirstName,
    Apps.LastName,
    Apps.DateOfBirth,
    Com.ProviderId,
    Com.EmployerAccountId,
    CASE
        WHEN Com.TransferSenderId IS NOT NULL THEN 'Transfer'
        WHEN Com.ApprenticeshipEmployerTypeOnApproval = 0 THEN 'NonLevy'
        ELSE 'Levy'
    END AS FundingType,
    Com.TransferSenderId,
    ALE.Name,
    Com.AccountLegalEntityId,
    Apps.TrainingCode,
    Apps.TrainingCourseVersion,
    Apps.StartDate,
    Apps.EndDate,
    Apps.TrainingPrice,
    Apps.EndPointAssessmentPrice,
    Apps.Cost
FROM Apprenticeship AS Apps
LEFT JOIN Commitment AS Com ON Apps.CommitmentId = Com.Id
LEFT JOIN AccountLegalEntities AS ALE ON Com.AccountLegalEntityId = ALE.Id
WHERE Apps.Id IN (SELECT Id FROM @ApprenticeshipIds)
	
-- Open the cursor
OPEN ApprenticeshipCursor;

-- Fetch the first record
FETCH NEXT FROM ApprenticeshipCursor INTO 
    @ApprovalsApprenticeshipId,
    @Uln,
    @FirstName,
    @LastName,
    @DateOfBirth,
    @Ukprn,
    @EmployerAccountId,
    @FundingType,
    @FundingEmployerAccountId,
    @LegalEntityName,
    @AccountLegalEntityId,
    @TrainingCode,
    @TrainingCourseVersion,
    @StartDate,
    @EndDate,
    @TrainingPrice,
    @EndPointAssessmentPrice,
    @TotalPrice;

--MARK THE FIRST CHUNK
insert into @Output(OutputLine, [Order]) values ('--- CHUNK ---', 0)

-- Loop through each record
WHILE @@FETCH_STATUS = 0
BEGIN

	SET @Count = @Count + 10;
	SET @ChunkCount = @ChunkCount + 1;

	IF(@ChunkCount > @ChunkSize)
	BEGIN

		--MARK THE NEXT CHUNK
		insert into @Output(OutputLine, [Order]) values ('--- CHUNK ---', @Count-1)
		SET @ChunkCount = 0;

	END

	DECLARE @ApprenticeshipKey UNIQUEIDENTIFIER = NEWID();
	DECLARE @EpisodeKey UNIQUEIDENTIFIER = NEWID();
	DECLARE @EpisodePriceKey UNIQUEIDENTIFIER = NEWID();

	insert into @Output(OutputLine, [Order]) values
	('INSERT INTO [dbo].[Learning] ([Key], [ApprovalsApprenticeshipId], [Uln], [FirstName], [LastName], [DateOfBirth], [ApprenticeshipHashedId]) VALUES
	(''' + CONVERT(VARCHAR(36), @ApprenticeshipKey) + ''', ' + CONVERT(VARCHAR,@ApprovalsApprenticeshipId) + ', ' + CONVERT(VARCHAR,@Uln) + ', ''' + CONVERT(VARCHAR,@FirstName) + ''', ''' + CONVERT(VARCHAR,@LastName) + ''', ''' + CONVERT(VARCHAR,@DateOfBirth) + ''','''')'
	, @Count)

	insert into @Output(OutputLine, [Order]) values
	( 'INSERT INTO [dbo].[Episode] ([Key], LearningKey, Ukprn, EmployerAccountId, FundingType, FundingPlatform, FundingEmployerAccountId, LegalEntityName, AccountLegalEntityId, TrainingCode, TrainingCourseVersion, PaymentsFrozen, LearningStatus) VALUES 
	(''' + CONVERT(VARCHAR(36),@EpisodeKey) + ''','
	+ '''' + CONVERT(VARCHAR(36),@ApprenticeshipKey) + ''','
	+ '''' + CONVERT(VARCHAR(8),@Ukprn) + ''','
	+ CONVERT(VARCHAR,@EmployerAccountId) + ','
	+ '''' + @FundingType + ''','
	+ '2,'
	+ CASE WHEN @FundingEmployerAccountId IS NULL THEN 'NULL' ELSE CONVERT(VARCHAR,@FundingEmployerAccountId) END + ','
	+ '''' + @LegalEntityName + ''','
	+ CONVERT(VARCHAR(8),@AccountLegalEntityId) + ','
	+ '''' + @TrainingCode + ''','
	+ CASE WHEN @TrainingCourseVersion IS NULL THEN 'NULL' ELSE CONVERT(VARCHAR,@TrainingCourseVersion) END + ','
	+ '0,'
	+ '''Active'')', @Count + 1)

	insert into @Output(OutputLine, [Order]) values
	('INSERT INTO [dbo].[EpisodePrice] ([Key], [EpisodeKey], [StartDate], [EndDate], [TrainingPrice], [EndPointAssessmentPrice], [TotalPrice], [FundingBandMaximum]) VALUES
		(''' + CONVERT(VARCHAR(36),@EpisodePriceKey) + ''','
	+ '''' + CONVERT(VARCHAR(36),@EpisodeKey) + ''','
	+ '''' + CONVERT(VARCHAR(10),@StartDate) + ''','
	+ '''' + CONVERT(VARCHAR(10),@EndDate) + ''','
	+ CASE WHEN @TrainingPrice IS NULL THEN 'NULL' ELSE CONVERT(VARCHAR,@TrainingPrice) END + ','
	+ CASE WHEN @EndPointAssessmentPrice IS NULL THEN 'NULL' ELSE CONVERT(VARCHAR,@EndPointAssessmentPrice) END + ','
	+ CASE WHEN @TotalPrice IS NULL THEN 'NULL' ELSE CONVERT(VARCHAR,@TotalPrice) END + ','
	+ '-1)', @Count + 2)
	
    -- Fetch the next record
    FETCH NEXT FROM ApprenticeshipCursor INTO 
        @ApprovalsApprenticeshipId,
        @Uln,
        @FirstName,
        @LastName,
        @DateOfBirth,
        @Ukprn,
        @EmployerAccountId,
        @FundingType,
        @FundingEmployerAccountId,
        @LegalEntityName,
        @AccountLegalEntityId,
        @TrainingCode,
        @TrainingCourseVersion,
        @StartDate,
        @EndDate,
        @TrainingPrice,
        @EndPointAssessmentPrice,
        @TotalPrice;
END

-- Close and deallocate the cursor
CLOSE ApprenticeshipCursor;
DEALLOCATE ApprenticeshipCursor;



SELECT OutputLine from @Output
ORDER BY [Order] ASC