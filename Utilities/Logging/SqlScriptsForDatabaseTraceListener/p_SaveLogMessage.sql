-- STORED PROC p_SaveLogMessage

-- ANSI_NULLS and QUOTED_IDENTIFIER settings are remembered from the time an object is 
--	created or altered.  If the settings are changed subsequently in the database the object 
--	will ignore the new settings.  Set them explicitly here for consistency as the default 
--	settings will be different when this script is run manually than when it is run via sqlcmd
--	(the SQL Server Command-line Utility, used to run SQL Server scripts from batch files).
SET ANSI_NULLS ON
GO
-- QUOTED_IDENTIFIER normally set OFF (since developers should be enclosing identifiers which  
--	include spaces in square brackets, not quotes).  However, if an XML data type is referenced 
--	in this object then QUOTED_IDENTIFIER must be set ON (requirement for XML data type).
SET QUOTED_IDENTIFIER OFF
GO

-- CREATE A DUMMY PROCEDURE IF IT DOESN'T EXIST.
-- This allows the main code block to be ALTER rather than CREATE procedure.  Once 
--	created initially the procedure will never be dropped, only altered.
DECLARE @ObjName NVARCHAR(50)
SET @ObjName = 'p_SaveLogMessage'

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = @ObjName AND type = 'P')
BEGIN
	DECLARE @DummyCreateStatement NVARCHAR(200)	
	SET @DummyCreateStatement = N'CREATE PROCEDURE ' + @ObjName + ' AS RETURN 0'
	EXEC sp_executesql @DummyCreateStatement
END
GO

ALTER PROCEDURE p_SaveLogMessage
	@Message			NVARCHAR(MAX), 
	@DetailedMessage	NVARCHAR(MAX) = NULL, 
	@Category			NVARCHAR(100) = NULL, 
	@EventID			INT = NULL, 
	@ProcedureName		NVARCHAR(100) = NULL,
	@Source				NVARCHAR(100) = NULL, 
	@Application		NVARCHAR(100) = NULL, 
	@ProcessID			INT = NULL, 
	@ThreadID			NVARCHAR(50) = NULL, 
	@RelatedActivityID	NVARCHAR(50) = NULL, 
	@CallStack			NVARCHAR(4000) = NULL, 
	@LogicalOperationStack			NVARCHAR(4000) = NULL
AS

/**************************************************************************************************
* PROJECT:	
*		Custom .NET Trace Listeners - Database Trace Listener

* DESCRIPTION:  
*		Saves a log message to the log message table.

* INPUT PARAMETERS: 
*		@Message				- Message that will be logged.
*		@DetailedMessage:		- Detailed error message when an error occurs in the calling 
*									application.
*		@Category:				- For messages logged via Trace.Write or .WriteLine this is the 
*									category argument.  For messages logged via 
*									TraceSource.TraceEvent or .TraceData this is the TraceEventType. 
*		@EventID:				- An event ID that uniquely identifies the event that created the 
*									log message.
*		@ProcedureName:			- The name of the procedure, function or method that the message 
*									is being logged from.
*		@Source:				- The name of the TraceSource that raised the log event.
*		@Application:			- The name of the application that logged the message.
*		@ProcessID:				- The ID of the process that logged the message.
*		@ThreadID:				- The ID of the managed thread that logged the message.
*		@RelatedActivityID:		- GUID that identifies a related activity.  Used in the 
*									TraceSource.TraceTransfer method.
*		@CallStack:				- The method call stack.  Used when there is an error.
*		@LogicalOperationStack:	- The logical operation stack output by 
*									CorrelationManager.LogicalOperationStack.

* OUTPUT PARAMETERS:
*		None.

*
* RETURN VALUES:
*		0:		Stored procedure executed successfully.
*		-1:		Error occurred during stored procedure execution.

* RESULT SET:
*		None. 

* NOTES:		
*		
***************************************************************************************************/
-- 			VISUAL SOURCESAFE REVISION LIST
--			===============================
-- $History: p_SaveLogMessage.sql $.
-- 
-- *****************  Version 1  *****************
-- User: Simone       Date: 5/04/09    Time: 3:17p
-- Created in $/UtilitiesClassLibrary_DENG/Utilities.DatabaseLogging/SqlScriptsForDatabaseTraceListener
-- Stored procedure that saves a log message to the log message table.
-- 
-- *****************  Version 3  *****************
-- User: Simone       Date: 20/06/08   Time: 5:14p
-- Updated in $/UtilitiesClassLibrary_DENG/Utilities.Logging/SqlScriptsForDatabaseTraceListener
-- Increase size for Message and DetailedMessage columns to NVARCHAR(MAX).
-- 
-- *****************  Version 2  *****************
-- User: Simone       Date: 26/02/08   Time: 10:28a
-- Updated in $/UtilitiesClassLibrary_DENG/Utilities.Logging/SqlScriptsForDatabaseTraceListener
-- Remove USE statement from top of script so it will execute in the
-- current database, whichever database that is.
-- 
-- *****************  Version 1  *****************
-- User: Simone       Date: 18/02/08   Time: 9:29a
-- Created in $/UtilitiesClassLibrary_DENG/Utilities.Logging/SqlScriptsForDatabaseTraceListener
-- ************************************************************************************************

DECLARE 
	@RetVal		INT, 
	@ErrNum		INT,
	@ErrMsg		VARCHAR(127), 
	@ProcName	VARCHAR(254)

SET 	@RetVal = 0
SET 	@ErrNum = 0
SET 	@ErrMsg = ''
SET	@ProcName = 'p_SaveLogMessage'

SET NOCOUNT ON

BEGIN TRANSACTION

INSERT INTO ServerLog (Message, DetailedMessage, Category, EventID, ProcedureName, Source, Application, 
						ProcessID, ThreadID, RelatedActivityID, CallStack, LogicalOperationStack)
			VALUES (@Message, @DetailedMessage, @Category, @EventID, @ProcedureName, @Source, @Application, 
						@ProcessID, @ThreadID, @RelatedActivityID, @CallStack, @LogicalOperationStack)
	
SET @ErrNum = @@ERROR
IF @ErrNum <> 0 
	GOTO ErrHandler

-- If reach this point there cannot have been any errors so commit transaction.
COMMIT TRANSACTION

Finish:
SET NOCOUNT OFF
RETURN @RetVal

ErrHandler:
	
	IF @@TRANCOUNT > 0
		ROLLBACK TRANSACTION

	IF @ErrNum <> 0
	BEGIN
		SET @ErrMsg = @ErrMsg + 
			'Error %d (please look up error number for information).  Source: ''%s''.' 

		RAISERROR (@ErrMsg, 16, 1, @ErrNum, @ProcName)
	END

	-- IF NO ERROR NUMBER THEN ERROR CAUSED BY SUB-PROCEDURE.  DO NOTHING AS 
	--	SUB-PROCEDURE WILL RAISE ITS OWN ERROR.

	SET @RetVal = -1
	GOTO Finish

GO

-- GRANT PERMISSIONS
--REVOKE ALL ON p_SaveLogMessage FROM public
--GRANT EXECUTE ON p_SaveLogMessage TO mRouteUser
GO