/*
Pre-deployment script
*/

IF OBJECT_ID('[Domain].[EnglishAndMaths]', 'U') IS NULL
    RETURN;

-- EmployerAccountId
IF COL_LENGTH('[Domain].[EnglishAndMaths]', 'PriorLearningPercentage') IS NOT NULL
BEGIN
    EXEC('UPDATE [Domain].[EnglishAndMaths]
          SET [PriorLearningPercentage] = NULL
          WHERE [PriorLearningPercentage] IS NOT NULL');
END

