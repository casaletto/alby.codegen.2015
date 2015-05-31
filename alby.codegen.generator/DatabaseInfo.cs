using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection;
using acr = alby.codegen.runtime ;

namespace alby.codegen.generator
{
	public class DatabaseInfo
	{
		protected static DataSet __dsDatabaseInfo = new DataSet() ;
		protected static string  __databaseName   = "" ;

		//------------------------------------------------------------------------------------------------------------------

		public DataSet GetDatabaseInfo()
		{
			return __dsDatabaseInfo ;
		}

		//------------------------------------------------------------------------------------------------------------------

		public string GetDatabaseName()
		{
			return __databaseName ;
		}

		//------------------------------------------------------------------------------------------------------------------

		public void CreateDatabaseInfo( SqlConnection conn )
		{
			// already done ?
			if ( __dsDatabaseInfo.Tables.Count > 0 )
				return ;

			// database name
			string sql = @"select db_name() " ;

			using (SqlCommand cmd = new SqlCommand(sql, conn))
			{
				cmd.CommandTimeout = Helper.SQL_TIMEOUT;
				__databaseName = cmd.ExecuteScalar() as string ;
			}

			// lots n lots of database details
			sql = acr.Helper.LoadResource( this.GetType().Assembly, @"alby.codegen.generator.DatabaseInfo.sql" ) ;

			using (SqlCommand cmd = new SqlCommand(sql, conn))
			{
				cmd.CommandTimeout = Helper.SQL_TIMEOUT;
				cmd.ExecuteNonQuery() ;
			}

			sql = @"
select * from #tables
select * from #views
select * from #sp
select * from #pk
select * from #fk
select * from #pk2fk
select * from #fk2pk
select * from #tscol
select * from #idcol
select * from #compcol
select * from #spparam
select * from #tabletype
select * from #tabletypecol
" ;
			using (SqlCommand cmd = new SqlCommand(sql, conn))
			{
				cmd.CommandTimeout = Helper.SQL_TIMEOUT;
				using (SqlDataAdapter da = new SqlDataAdapter(cmd))
				{
					da.Fill( __dsDatabaseInfo ) ;

					int i = 0 ;
					__dsDatabaseInfo.Tables[ i++ ].TableName = "tables"        ;
					__dsDatabaseInfo.Tables[ i++ ].TableName = "views"         ;
					__dsDatabaseInfo.Tables[ i++ ].TableName = "sp"            ;
					__dsDatabaseInfo.Tables[ i++ ].TableName = "pk"            ;
					__dsDatabaseInfo.Tables[ i++ ].TableName = "fk"            ;
					__dsDatabaseInfo.Tables[ i++ ].TableName = "pk2fk"         ;
					__dsDatabaseInfo.Tables[ i++ ].TableName = "fk2pk"         ;
					__dsDatabaseInfo.Tables[ i++ ].TableName = "tscol"         ;
					__dsDatabaseInfo.Tables[ i++ ].TableName = "idcol"         ;
					__dsDatabaseInfo.Tables[ i++ ].TableName = "compcol"       ;
					__dsDatabaseInfo.Tables[ i++ ].TableName = "spparam"       ;
					__dsDatabaseInfo.Tables[ i++ ].TableName = "tabletype"     ;
					__dsDatabaseInfo.Tables[ i++ ].TableName = "tabletypecol"  ;
				}
			}

			// --------------------------------------------------------------------------------------------------

			// populate the dictionaries

			var t = __dsDatabaseInfo.Tables ;

			this.Tables.Populate( 
				t["tables"], 
				new List<string>() { "TheTable" } ) ;		

			this.Views.Populate( 
				t["views"], 
				new List<string>() { "TheView" } ) ;		

			this.StoredProcedures.Populate( 
				t["sp"], 
				new List<string>() { "TheStoredProcedure" } ) ;	
			
			this.TimestampColumns.Populate( 
				t["tscol"], 
				new List<string>() { "TheTable" }, 
				new List<string>() { "TimestampColumn" } ) ;	
			
			this.IdentityColumns.Populate( 
				t["idcol"], 
				new List<string>() { "TheTable" }, 
				new List<string>() { "IdentityColumn" } ) ;	
		
			this.ComputedColumns.Populate( 
				t["compcol"], 
				new List<string>() { "TheTable" }, 
				new List<string>() { "ComputedColumn" } ) ;	
		
			this.PrimaryKeyColumns.Populate( 
				t["pk"], 
				new List<string>() { "PkTable", "PkConstraintType" }, 
				new List<string>() { "PkTableColumnName" } ) ;	

			this.ForeignKeys.Populate( 
				t["fk"], 
				new List<string>() { "FkTable" }, 
				new List<string>() { "PkTable", "FkName" }, true, true ) ;	

			this.ForeignKeyConstraintColumns.Populate( 
				t["fk"], 
				new List<string>() { "FkTable", "FkName" }, 
				new List<string>() { "FkTableColumnName" } ) ;	

			this.ParentTables.Populate( 
				t["pk2fk"], 
				new List<string>() { "FkTable", "PkConstraintType" }, 
				new List<string>() { "PkTable", "FkName" }, true, true ) ;	

			this.ChildTables.Populate( 
				t["pk2fk"], 
				new List<string>() { "PkTable" }, 
				new List<string>() { "FkTable", "FkName" }, true, true ) ;	

			this.ConstraintColumns_ParentSide.Populate(	
				t["pk2fk"], 
				new List<string>() { "PkTable", "FkName", "PkConstraintType" }, 
				new List<string>() { "PkTableColumnName" } ) ;		

			this.ConstraintColumns_ChildSide.Populate(	
				t["pk2fk"], 
				new List<string>() { "FkTable", "FkName", "PkConstraintType" }, 
				new List<string>() { "FkTableColumnName" } ) ;

			this.ForeignKeyColumnsOfPrimaryKeyTable.Populate( 
				t["fk"], 
				new List<string>() { "PkTable" }, 
				new List<string>() { "PkTableColumnName" } ) ;	

			this.ForeignKeyColumnsOfForeignKeyTable.Populate( 
				t["fk"], 
				new List<string>() { "FkTable" }, 
				new List<string>() { "FkTableColumnName" } ) ;	

			this.ForeignKeyTableToPrimaryKeyTables.Populate( 
				t["fk"], 
				new List<string>() { "FkTable" }, 
				new List<string>() { "PkTable" }, true, true ) ;	

			this.TableTypes.Populate( 
				t["tabletype"], 
				new List<string>() { "type" }, true, true ) ;	

			this.TableTypeColumns.Populate( 
				t["tabletypecol"], 
				new List<string>() { "TableType" },	
				new List<string>() { "ColumnName", "ColumnType", "max_length", "precision", "scale" } ) ;	
		}
		
		//------------------------------------------------------------------------------------------------------------------

		#region dictionaries

		public DataTableDictionary<string> Tables
		{
			get
			{
				return __tables ;
			}
		}
		protected static DataTableDictionary<string> __tables = new DataTableDictionary<string>() ;

		//------------------------------------------------------------------------------------------------------------------

		public DataTableDictionary<string> Views
		{
			get
			{
				return __views ;
			}
		}
		protected static DataTableDictionary<string> __views = new DataTableDictionary<string>() ;

		//------------------------------------------------------------------------------------------------------------------

		public DataTableDictionary<string> StoredProcedures
		{
			get
			{
				return __storedProcedures ;
			}
		}
		protected static DataTableDictionary<string> __storedProcedures = new DataTableDictionary<string>() ;

		//------------------------------------------------------------------------------------------------------------------

		public DataTableDictionary<string> TimestampColumns
		{
			get
			{
				return __timestampColumns ;
			}
		}
		protected static DataTableDictionary<string> __timestampColumns = new DataTableDictionary<string>() ;

		//------------------------------------------------------------------------------------------------------------------

		public DataTableDictionary<string> IdentityColumns
		{
			get
			{
				return __identityColumns ;
			}
		}
		protected static DataTableDictionary<string> __identityColumns = new DataTableDictionary<string>() ;

		//------------------------------------------------------------------------------------------------------------------

		public DataTableDictionary<string> ComputedColumns
		{
			get
			{
				return __computedColumns ;
			}
		}
		protected static DataTableDictionary<string> __computedColumns = new DataTableDictionary<string>() ;

		//------------------------------------------------------------------------------------------------------------------

		public DataTableDictionary<string> PrimaryKeyColumns
		{
			get
			{
				return __primaryKeyColumns ;
			}
		}
		protected static DataTableDictionary<string> __primaryKeyColumns = new DataTableDictionary<string>() ;

		//------------------------------------------------------------------------------------------------------------------

		public DataTableDictionary<string,string> ForeignKeys
		{
			get
			{
				return __foreignKeys ;
			}
		}
		protected static DataTableDictionary<string,string> __foreignKeys = new DataTableDictionary< string,string >() ;

		//------------------------------------------------------------------------------------------------------------------

		public DataTableDictionary<string> ForeignKeyConstraintColumns
		{
			get
			{
				return __foreignKeyConstraintColumns ;
			}
		}
		protected static DataTableDictionary<string> __foreignKeyConstraintColumns = new DataTableDictionary< string >() ;

		//------------------------------------------------------------------------------------------------------------------

		public DataTableDictionary<string,string> ParentTables
		{
			get
			{
				return __parentTables ;
			}
		}
		protected static DataTableDictionary<string,string> __parentTables = new DataTableDictionary< string,string >() ;

		//------------------------------------------------------------------------------------------------------------------

		public DataTableDictionary<string,string> ChildTables
		{
			get
			{
				return __childTables ;
			}
		}
		protected static DataTableDictionary<string,string> __childTables = new DataTableDictionary< string,string >() ;

		//------------------------------------------------------------------------------------------------------------------
		
		public DataTableDictionary<string> ConstraintColumns_ParentSide
		{
			get
			{
				return __constraintColumns_ParentSide ;
			}
		}
		protected static DataTableDictionary<string> __constraintColumns_ParentSide = new DataTableDictionary<string>() ;

		//------------------------------------------------------------------------------------------------------------------

		public DataTableDictionary<string> ConstraintColumns_ChildSide
		{
			get
			{
				return __constraintColumns_ChildSide ;
			}
		}
		protected static DataTableDictionary<string> __constraintColumns_ChildSide = new DataTableDictionary<string>() ;

		//------------------------------------------------------------------------------------------------------------------

		public DataTableDictionary<string> ForeignKeyColumnsOfPrimaryKeyTable
		{
			get
			{
				return __somebodyElsesForeignKeyColumns ;
			}
		}
		protected static DataTableDictionary<string> __somebodyElsesForeignKeyColumns = new DataTableDictionary<string>() ;

		//------------------------------------------------------------------------------------------------------------------

		public DataTableDictionary<string> ForeignKeyColumnsOfForeignKeyTable
		{
			get
			{
				return __foreignKeyColumnsOfForeignKeyTable ;
			}
		}
		protected static DataTableDictionary<string> __foreignKeyColumnsOfForeignKeyTable = new DataTableDictionary<string>() ;

		//------------------------------------------------------------------------------------------------------------------

		public DataTableDictionary<string> ForeignKeyTableToPrimaryKeyTables
		{
			get
			{
				return __foreignKeyTableToPrimaryKeyTables ;
			}
		}
		protected static DataTableDictionary<string> __foreignKeyTableToPrimaryKeyTables = new DataTableDictionary<string>() ;

		//------------------------------------------------------------------------------------------------------------------

		public DataTableDictionary<string> TableTypes
		{
			get
			{
				return __tableTypes ;
			}
		}
		protected static DataTableDictionary<string> __tableTypes = new DataTableDictionary<string>() ;

		//------------------------------------------------------------------------------------------------------------------

		public DataTableDictionary<string,string,Int16,Byte,Byte> TableTypeColumns
		{
			get
			{
				return __tableTypeColumns ;
			}
		}
		protected static DataTableDictionary<string,string,Int16,Byte,Byte> __tableTypeColumns = new DataTableDictionary<string,string,Int16,Byte,Byte>() ;

		//------------------------------------------------------------------------------------------------------------------

		#endregion

	} // end class

} // end namespace

 