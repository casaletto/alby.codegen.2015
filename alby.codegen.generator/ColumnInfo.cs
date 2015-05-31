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
	public class ColumnInfo
	{
		protected static Dictionary< string, List< Tuple<string,string> > > __columnDictionaryTables  = new Dictionary< string, List<Tuple<string,string>> > () ;
		protected static Dictionary< string, List< Tuple<string,string> > > __columnDictionaryViews   = new Dictionary< string, List<Tuple<string,string>> > () ;
		protected static Dictionary< string, List< Tuple<string,string> > > __columnDictionaryQueries = new Dictionary< string, List<Tuple<string,string>> > () ;

		protected List<string> _tables  ;
		protected List<string> _views   ;
		protected XmlNodeList  _queries ;

		protected int _maxWidthTable   = 0 ;
		protected int _maxWidthColumn  = 0 ;
		protected int _maxWidthType    = 0 ;

		//--------------------------------------------------------------------------------------------------------------------		

		public List< Tuple<string,string> > GetTableColumns( string table )
		{
			return __columnDictionaryTables[ table ] ;
		}

		//--------------------------------------------------------------------------------------------------------------------		

		public List< Tuple<string,string> > GetViewColumns( string view )
		{
			return __columnDictionaryViews[ view ] ;
		}

		//--------------------------------------------------------------------------------------------------------------------		

		public List< Tuple<string,string> > GetQueryColumns( string queryfile )
		{
			return __columnDictionaryQueries[ queryfile ] ;
		}

		//--------------------------------------------------------------------------------------------------------------------		

		public void CreateColumnInfo(	SqlConnection	conn, 
										List<string>	tables, 
										List<string>	views,
										string			codegendirectory,
										string			querysubdirectory, 
										XmlNodeList		xmlqueries  )
		{
			Helper h = new Helper() ;

			// already done ?
			if ( __columnDictionaryTables.Count > 0 )
				return ;

			_tables  = tables ;
			_views   = views  ;
			_queries = xmlqueries ; 

			// tables
			if ( tables.Count > 0 )
			{
				string sql = "" ;
				foreach ( string table in tables )
					sql += string.Format( "\nselect top 0 * from [{0}] where 1 > 2", table.Replace( ".", "].[" ) ) ;

				DataSet ds = new DataSet() ;
				using (SqlCommand cmd = new SqlCommand(sql, conn))
				{
					cmd.CommandTimeout = Helper.SQL_TIMEOUT;
					using (SqlDataAdapter da = new SqlDataAdapter(cmd))
						da.Fill( ds ) ;
				}

				int i = 0 ;
				tables.ForEach( t => ds.Tables[ i++ ].TableName = t ) ;

				foreach( DataTable dt in ds.Tables )
				{
					string table = dt.TableName ;

					_maxWidthTable = Math.Max( _maxWidthTable, table.Length ) ;

					List< Tuple<string,string> > list = new List<Tuple<string,string>>() ;

					foreach( DataColumn dc in dt.Columns )
					{
						string column = dc.ColumnName ;
						string type   = dc.DataType.ToString() ;

						list.Add( new Tuple<string,string>( column, type ) ) ;

						_maxWidthColumn	= Math.Max( _maxWidthColumn, column.Length ) ;
						_maxWidthType	= Math.Max( _maxWidthType,   type.Length ) ;
					}

					__columnDictionaryTables.Add( table, list ) ;
				}
			}

			// views
			if ( views.Count > 0 )
			{
				string sql = "" ;
				foreach ( string view in views )
					sql += string.Format( "\nselect top 0 * from [{0}] where 1 > 2", view.Replace( ".", "].[" ) ) ;

				DataSet ds = new DataSet() ;
				using (SqlCommand cmd = new SqlCommand(sql, conn))
				{
					cmd.CommandTimeout = Helper.SQL_TIMEOUT;
					using (SqlDataAdapter da = new SqlDataAdapter(cmd))
						da.Fill( ds ) ;
				}

				int i = 0 ;
				views.ForEach( t => ds.Tables[ i++ ].TableName = t ) ;

				foreach( DataTable dt in ds.Tables )
				{
					string view = dt.TableName ;

					_maxWidthTable = Math.Max( _maxWidthTable, view.Length ) ;

					List< Tuple<string,string> > list = new List<Tuple<string,string>>() ;

					foreach( DataColumn dc in dt.Columns )
					{
						string column = dc.ColumnName ;
						string type   = dc.DataType.ToString() ;

						list.Add( new Tuple<string,string>( column, type ) ) ;

						_maxWidthColumn	= Math.Max( _maxWidthColumn, column.Length ) ;
						_maxWidthType	= Math.Max( _maxWidthType,   type.Length ) ;
					}

					__columnDictionaryViews.Add( view, list ) ;
				}
			}

			// queries
			if ( xmlqueries.Count > 0 )
			{
				string sql = "" ;

				foreach( XmlNode xmlquery in xmlqueries )
				{
					string  queryfile  = xmlquery.SelectSingleNode( "@Select").Value ;
					string  selectfile = codegendirectory + @"\" + querysubdirectory +  @"\" + queryfile ;
					string	selectsql  = File.ReadAllText( selectfile ) ;

					sql += string.Format( "\n{0} where 1 > 2", selectsql.Trim() ) ;
				}

				DataSet ds = new DataSet() ;
				using (SqlCommand cmd = new SqlCommand(sql, conn))
				{
					cmd.CommandTimeout = Helper.SQL_TIMEOUT;
					using (SqlDataAdapter da = new SqlDataAdapter(cmd))
						da.Fill( ds ) ;
				}

				int i = 0 ;
				foreach( XmlNode xmlquery in xmlqueries )
				{
					string queryfile = xmlquery.SelectSingleNode( "@Select").Value ;
					ds.Tables[ i++ ].TableName = queryfile ;
				}			
			
				foreach( DataTable dt in ds.Tables )
				{
					string queryfile = dt.TableName ;

					_maxWidthTable = Math.Max( _maxWidthTable, queryfile.Length ) ;

					List< Tuple<string,string> > list = new List<Tuple<string,string>>() ;

					foreach( DataColumn dc in dt.Columns )
					{
						string column = dc.ColumnName ;
						string type   = dc.DataType.ToString() ;

						list.Add( new Tuple<string,string>( column, type ) ) ;

						_maxWidthColumn	= Math.Max( _maxWidthColumn, column.Length ) ;
						_maxWidthType	= Math.Max( _maxWidthType,   type.Length ) ;
					}

					__columnDictionaryQueries.Add( queryfile, list ) ;
				}
			}
		}

		//--------------------------------------------------------------------------------------------------------------------		

		public override string ToString()
		{
			StringBuilder bob = new StringBuilder() ;

			foreach( string table in _tables  )
			{
				foreach ( var column in this.GetTableColumns( table )  )
					bob.AppendLine( string.Format( "|Table|{0}|{1}|{2}|",
										table.PadRight( _maxWidthTable ),
										column.Item1.PadRight( _maxWidthColumn ), 
										column.Item2.PadRight( _maxWidthType ) ) ) ;  
			}

			foreach( string view in _views  )
			{
				foreach ( var column in this.GetViewColumns( view )  )
					bob.AppendLine( string.Format( "|View |{0}|{1}|{2}|",
										view.PadRight( _maxWidthTable ),
										column.Item1.PadRight( _maxWidthColumn ), 
										column.Item2.PadRight( _maxWidthType ) ) ) ;  
			}

			foreach( string queryfile in __columnDictionaryQueries.Keys   )
			{
				foreach ( var column in this.GetQueryColumns( queryfile )  )
					bob.AppendLine( string.Format( "|Query|{0}|{1}|{2}|",
										queryfile.PadRight( _maxWidthTable ),
										column.Item1.PadRight( _maxWidthColumn ), 
										column.Item2.PadRight( _maxWidthType ) ) ) ;  
			}

			return bob.ToString() ;
		}

		//--------------------------------------------------------------------------------------------------------------------		

	} // end class

}
