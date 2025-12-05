/*
Pre-deployment script
*/

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE Name = N'FundingBandMaximum'
      AND Object_ID = Object_ID(N'dbo.EpisodePrice')
)
BEGIN
    ALTER TABLE dbo.EpisodePrice
    DROP COLUMN FundingBandMaximum;
END

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE Name = N'FundingBandMaximum'
      AND Object_ID = Object_ID(N'dbo.Episode')
)
BEGIN
    ALTER TABLE dbo.Episode
    DROP COLUMN FundingBandMaximum;
END