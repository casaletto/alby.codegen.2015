using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq ;
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
	//--------------------------------------------------------------------------------------------------------------------		

	public class Resultset
	{
		public List< Tuple<string,string> > Columns = new List< Tuple<string,string> >() ;
	}

	//--------------------------------------------------------------------------------------------------------------------		

	public class ResultsetInfo
	{
		protected string			_errorMessage	= "";
		protected DataSet			_ds				= new DataSet() ;
		protected string			_type			= "" ;
		protected List<Resultset>	_resultsets		= new List< Resultset >() ;
		protected List<int>			_ignoredResultsets = new List<int>() ;

		public ResultsetInfo( string errorMessage, DataSet ds )
		{
			_errorMessage = errorMessage ;
			_ds = ds ;
		}

		public string ErrorMessage
		{
			get
			{
				return _errorMessage ;
			}
		}

		public DataSet DataSet
		{
			get
			{
				return _ds ;
			}
		}

		public string Type
		{
			get
			{
				if ( _errorMessage.Length > 0 ) 
					 return "UNKNOWN" ;

				if ( _ds.Tables.Count > 0 )
					 return "RECORDSET" ; 

				return "NORECORDSET" ;
			}	
		}

		public List<Resultset> Resultsets
		{
			get
			{
				return _resultsets ;
			}
		}

		public List<int> IgnoredResultsets
		{
			get
			{
				return _ignoredResultsets ;
			}
		}

	} // end class

	//--------------------------------------------------------------------------------------------------------------------		
	//--------------------------------------------------------------------------------------------------------------------		
	//--------------------------------------------------------------------------------------------------------------------		

	public class StoredProcedureResultsetInfo
	{
		protected static Dictionary< string, ResultsetInfo > __dictionary = new Dictionary< string, ResultsetInfo > () ;

		protected static int __maxWidthStoredProcedure	= 0 ;
		protected static int __maxWidthColumn			= "No resultsets".Length ; 
		protected static int __maxWidthType				= 0 ;

		//--------------------------------------------------------------------------------------------------------------------		

		public ResultsetInfo GetResultsetInfo( string storedprocedure )
		{
			if ( ! __dictionary.ContainsKey( storedprocedure ) )
				 return null ;

			return __dictionary[ storedprocedure ] ;
		}

		//--------------------------------------------------------------------------------------------------------------------		

		public void CreateStoredProcedureInfo(	SqlConnection conn, DatabaseInfo di, XmlDocument xmlconfig, StoredProcedureParameterInfo sppi )
		{
			Helper h = new Helper() ;

			// already done ?
			if ( __dictionary.Count > 0 )
				 return ;

			foreach ( string storedprocedure in di.StoredProcedures.Get() )
			{
				__maxWidthStoredProcedure = Math.Max( __maxWidthStoredProcedure, storedprocedure.Length ) ;

				StringBuilder bob = new StringBuilder() ;

				var parameters = sppi.GetStoredProcedureParameterInfo( storedprocedure ) ;
				if ( parameters != null )
				{
					int i = 1 ;
					foreach( var parameter in parameters )
						bob.AppendLine( string.Format( "declare @p{0} {1}", i++, this.GetTsqLocalVariableType( parameter.Type ) ) ) ; 
				}

				bob.AppendLine() ; 
				bob.AppendLine( "set fmtonly on" ) ; 

				bob.Append( string.Format( "exec [{0}]", storedprocedure.Replace( ".", "].[" ) ) ) ;

				if ( parameters != null )
				{
					int i = 1 ;
					foreach ( var parameter in parameters )
						bob.Append( string.Format( ", @p{0}", i++ ) ) ;
				}

				bob.AppendLine() ; 
				bob.AppendLine( "set fmtonly off" ) ; 

				string sql = bob.ToString() ;
				sql = sql.Replace( "],", "] " ) ;
				h.MessageVerbose( sql ) ;

				DataSet ds = new DataSet() ;
				string errorMessage = this.GetMetaDataSets( conn, sql, ds ) ;
				
				if ( errorMessage.Length > 0 )
					h.MessageVerbose( "ERRORMESSAGE:" +errorMessage ) ;

				ResultsetInfo rsi = new ResultsetInfo( errorMessage, ds ) ;
				__dictionary.Add( storedprocedure, rsi ) ;

				foreach ( DataTable dt in rsi.DataSet.Tables )
				{
					Resultset rs = new Resultset() ;

					foreach( DataColumn dc in dt.Columns )
					{
						string column = dc.ColumnName ;
						string type   = dc.DataType.ToString() ;

						rs.Columns.Add( new Tuple<string,string>( column, type ) ) ;

						__maxWidthColumn = Math.Max( __maxWidthColumn, column.Length ) ;
						__maxWidthType	 = Math.Max( __maxWidthType,   type.Length  ) ;
					}

					rsi.Resultsets.Add( rs ) ;
				}

				// recordsets to be ignoreed
				string xpath = string.Format( "/CodeGen/StoredProcs/StoredProc[ @Name='{0}' ]/@IgnoreRecordsets", storedprocedure ) ;
				var node = xmlconfig.SelectSingleNode( xpath ) ;
				if ( node != null )
				{
					string items = node.Value.Trim() ;
					if ( items.Length > 0 )
						 rsi.IgnoredResultsets.AddRange( items.Split( ',' ).ToList().ConvertAll<int>( i => int.Parse( i ) ) ) ;	// convert string list to int list
				}

			} // end foreach sp
		
		}

		//--------------------------------------------------------------------------------------------------------------------		

		protected string GetMetaDataSets( SqlConnection conn, string sql, DataSet ds )
		{
			try
			{
				using ( SqlCommand cmd = new SqlCommand() )
				{
					cmd.Connection		= conn ;
					cmd.CommandTimeout	= Helper.SQL_TIMEOUT ;
					cmd.CommandText		= sql ;
					cmd.CommandType		= CommandType.Text ;

					using ( SqlDataAdapter da = new SqlDataAdapter(cmd) )
						da.Fill( ds ) ;

					return "" ; // no errors !
				}
			}
			catch( Exception ex ) 
			{
				return string.Format( "Cant get metadata: {0}", ex.Message.Replace( "\r\n", " " ) ) ;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------		

		public override string ToString()
		{
			StringBuilder bob = new StringBuilder() ;

			foreach( string storedprocedure in __dictionary.Keys )
			{
				var rsi = this.GetResultsetInfo( storedprocedure ) ;

				if ( rsi.ErrorMessage.Length > 0 )
				{				
					bob.AppendLine( string.Format( "|{0}|{1}|{2}|",
										storedprocedure.PadRight( __maxWidthStoredProcedure ), 
										"0000",
										rsi.ErrorMessage.PadRight( 1 + __maxWidthColumn + __maxWidthType ) ) 
								  ) ;
				}
				if ( rsi.Resultsets.Count > 0 )
				{
					int i = 0 ;

					foreach( var rs in rsi.Resultsets )
					{
						i++ ;
						foreach( var column in rs.Columns )
						{
							bob.AppendLine( string.Format( "|{0}|{1}|{2}|{3}|",
												storedprocedure.PadRight( __maxWidthStoredProcedure ), 
												i.ToString().PadLeft( 4, '0' ),
												column.Item1.PadRight( __maxWidthColumn ), 
												column.Item2.PadRight( __maxWidthType ) ) 										  
										) ;
						}
					}
				}
				else
				{	 
					bob.AppendLine( string.Format( "|{0}|{1}|{2}|{3}|",
										storedprocedure.PadRight( __maxWidthStoredProcedure ), 
										"0000",
										"No resultsets".PadRight( __maxWidthColumn ),
										"".PadRight( __maxWidthType ) ) 										  
								  ) ;
				}
			}

			return bob.ToString() ;
		}

		//--------------------------------------------------------------------------------------------------------------------	
		
		protected string GetTsqLocalVariableType( string type )
		{
			if ( type == "text"  ) return "varchar(max)"   ;
			if ( type == "ntext" ) return "nvarchar(max)"  ;
			if ( type == "image" ) return "varbinary(max)" ;

			return type ;
		}

		//--------------------------------------------------------------------------------------------------------------------	

	} //end class

} 

