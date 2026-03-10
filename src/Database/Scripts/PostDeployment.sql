--/*
--Post-deployment script
--*/

-- Drop old MathsAndEnglish tables (renamed to EnglishAndMaths). DACPAC does not drop these automatically.
IF OBJECT_ID('dbo.MathsAndEnglishBreakInLearning', 'U') IS NOT NULL
    DROP TABLE dbo.MathsAndEnglishBreakInLearning;
GO

IF OBJECT_ID('dbo.MathsAndEnglish', 'U') IS NOT NULL
    DROP TABLE dbo.MathsAndEnglish;
GO
