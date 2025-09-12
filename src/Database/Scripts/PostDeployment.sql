/*
Post-deployment script
*/

IF OBJECT_ID('[dbo].[StartDateChange]', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].StartDateChange;
END

IF OBJECT_ID('[dbo].[PriceHistory]', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].PriceHistory;
END