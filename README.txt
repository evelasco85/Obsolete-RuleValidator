GENERAL

*Format:
		MMDDYYY_HHMM_[ScriptName].SQL
*Execution order is by date and time in ascending order, starting with 'SQL Scripts\Base Scripts' then 'SQL Scripts\Altered Scripts'
*All database related sctructure should have a corresponding SQL script
*NOTE: Every changes should correspond to a per script/file
*NOTE: Do not include 'USE [Database Name]' keyword
-----------------------------------------------------------------
TABLES

*Use 'Create' scripts for newly created table (Base)
		--Store in 'SQL Scripts\Base Scripts\Tables' folder
*Use 'Alter' scripts for applying changes on top of created tables
		--Store in 'SQL Scripts\Altered Scripts\Tables'

-----------------------------------------------------------------
STORED PROCEDURES, FUNCTIONS, OR NON-TABLE MODIFICATION SCRIPTS

*All stored procedure scripts are to be placed in 'SQL Scripts\Altered Scripts\Stored Procedures' for newly created and/or altered scripts
*All stored procedure should have a 'drop if exist' flow prior to creation

-----------------------------------------------------------------
DATA INSERTION

*All data for addition/insertion to tables are to be placed into 'Data Insertion Scripts'



-----------------------------------------------------------------

Quick Question(s):

*When to place 'Create Table' scripts in 'Base Script' or 'Altered Script' folders?
	-If the intended create script exists in staging or production then place it to 'Altered Script' else 'Base Script' folder