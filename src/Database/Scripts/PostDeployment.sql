/*
Post-deployment script
*/

IF OBJECT_ID('[dbo].[Apprenticeship]', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[Apprenticeship];
END