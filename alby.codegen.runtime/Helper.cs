using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection ;

namespace alby.codegen.runtime
{
	public class Helper
	{
		public static string LoadResource( Assembly assembly, string resourceName )
		{
			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if ( stream == null ) 
                    throw new CodeGenException( "Cant load resource [" + resourceName + "]." ) ;

				using (StreamReader reader = new StreamReader(stream))
					return reader.ReadToEnd().Trim();
            }
		}

		public static void ExecuteNonQuery( SqlConnection conn, string sql, SqlTransaction tran = null )
		{
			try
			{
				CodeGenEtc.Sql = sql ;
			 
				using (SqlCommand cmd = new SqlCommand(sql, conn))
				{
					cmd.CommandTimeout = CodeGenEtc.Timeout;

					if (tran != null)
						if (cmd.Transaction == null)
							cmd.Transaction = tran;

					cmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				throw new CodeGenException( ex.Message, ex, CodeGenEtc.Sql, null, null );
			}
		}

		public static object ExecuteScalar( SqlConnection conn, string sql, SqlTransaction tran = null )
		{
			try
			{
				CodeGenEtc.Sql = sql;

				using (SqlCommand cmd = new SqlCommand(sql, conn))
				{
					cmd.CommandTimeout = CodeGenEtc.Timeout;

					if (tran != null)
						if (cmd.Transaction == null)
							cmd.Transaction = tran;

					return cmd.ExecuteScalar();
				}
			}
			catch (Exception ex)
			{
				throw new CodeGenException( ex.Message, ex, CodeGenEtc.Sql, null, null );
			}

		}

		public static void EnableCheckConstraints( SqlConnection conn, bool enable, SqlTransaction tran = null )
		{
			string sql = "" ;

			if ( enable )
				sql = @"exec sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all' ";
			else
				sql = @"exec sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL' ";

			ExecuteNonQuery( conn, sql, tran ) ;
		}

		public static void CheckConstraints( SqlConnection conn, SqlTransaction tran = null )
		{
			string sql = "dbcc checkconstraints with all_constraints" ;

			ExecuteNonQuery( conn, sql, tran ) ;
		}

		public static void SetContextInfo( SqlConnection conn, string context, bool isNvarchar = true )
		{
			string sql = string.Format( @"
declare @string varchar(max)
select @string = '{0}'

declare @context varbinary(128)
select @context = cast( @string as varbinary)

set context_info @context
", context, isNvarchar ? "N" :"" ) ;

			ExecuteNonQuery( conn, sql ) ;
		}

		public static string GetContextInfo( SqlConnection conn, bool isNvarchar = true )
		{
			string sql = string.Format( "select cast( context_info() as {0}varchar(max) )", isNvarchar ? "N" :"" ) ;

			return ExecuteScalar( conn, sql ) as string ;
		}
		
		
					
		public static string DataSetToString( DataSet ds ) 
		{
			if ( ds == null ) return "" ;
			
			StringBuilder bob = new StringBuilder() ;

			int i = 1 ;
			foreach( DataTable dt in ds.Tables )
			{
				string s = "---------------- #" + i + " / " + ds.Tables.Count + " --------------------------------------------"; 
				bob.AppendLine( s ) ;

				s = DataTableToString( dt ) ;
				bob.AppendLine( s.Trim() ) ;

				s= "--------------------------------------------------------------------------------------" ;
				bob.AppendLine( s ) ;
				i++ ;
			}
			return bob.ToString() ;
		}

		public static string DataTableToString( DataTable dt ) 
		{
			if ( dt == null ) return "" ;

			string tab = "|" ;
			int maxlen = 20 ;
			StringBuilder bob = new StringBuilder() ;

			// header
			string s = "0" + tab ;
			foreach ( DataColumn col in dt.Columns ) 
				 s += col.ColumnName + tab ;
			bob.AppendLine( s.Trim() ) ;

			// data type
			s = "0" + tab ;
			foreach ( DataColumn col in dt.Columns ) 
				 s += col.DataType.ToString() + tab ;
			bob.AppendLine( s.Trim() ) ;

			//rows
			int i = 1 ;
			foreach ( DataRow row in dt.Rows ) 
			{
				s = i + tab ;
				foreach ( object item in row.ItemArray )
				{
					string a = item == null ? "#null#" : item.ToString().Trim() ;
					if ( a.Length > maxlen ) 
						a = a.Substring( 0, maxlen ) ;

					s +=  a.Trim() + tab ;
				}
				bob.AppendLine( s.Trim() ) ;
				i++ ;
			}

			return bob.ToString() ;
		}
	 
	} // end class
}
