/*
Post-deployment script
*/

--FLP-1426 - Removing WithdrawalRequest
IF OBJECT_ID('[dbo].[WithdrawalRequest]', 'U') IS NOT NULL
    DROP TABLE [dbo].[WithdrawalRequest];


--FLP-1321 - Move FundingBandMaximum
--This scripts copies funding band from EpisodePrice to Episode for existing records
--To be removed in a follow-up deployment once released to prod
UPDATE e
SET e.FundingBandMaximum = (
    SELECT TOP 1 ep.FundingBandMaximum
    FROM EpisodePrice ep
    WHERE ep.[EpisodeKey] = e.[Key]
)
FROM Episode e
WHERE e.FundingBandMaximum IS NULL;

