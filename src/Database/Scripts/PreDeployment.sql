/*
Pre-deployment script
*/


-- FLP-1289 (delete this script after 1289 deployed to prod)
IF OBJECT_ID('[Domain].[EnglishAndMaths]', 'U') IS NULL
    RETURN;

IF COL_LENGTH('[Domain].[EnglishAndMaths]', 'PriorLearningPercentage') IS NOT NULL
BEGIN
    EXEC('UPDATE [Domain].[EnglishAndMaths]
          SET [PriorLearningPercentage] = NULL
          WHERE [PriorLearningPercentage] IS NOT NULL');
END

