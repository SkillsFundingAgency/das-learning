/*
Post-deployment script
*/

--FLP-1426 - Removing WithdrawalRequest
IF OBJECT_ID('[dbo].[WithdrawalRequest]', 'U') IS NOT NULL
    DROP TABLE [dbo].[WithdrawalRequest];
