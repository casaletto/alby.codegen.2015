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

namespace alby.codegen.generator
{
	public class DataSetHelper
	{
		protected DataSet _ds ;

		//--------------------------------------------------------------------------------------------------------------------		

		public DataSetHelper( DataSet ds )
		{
			_ds = ds ;
		}

		//--------------------------------------------------------------------------------------------------------------------		
		
		public override string ToString()
		{
			StringBuilder bob = new StringBuilder() ;

			foreach (DataTable dt in _ds.Tables)
			{
				bob.AppendLine() ;
				bob.AppendLine( "[" + dt.TableName + "] " + dt.Rows.Count  + " rows" ) ;

				// get max widths of each column in table
				Dictionary<string, int> dic = GetColumnWidths( dt ) ;

				// pretty header line
				foreach (DataColumn column in dt.Columns)
					bob.Append( string.Format( "|{0}", new string( '_', dic[column.ColumnName] ) ) ) ; 
				bob.AppendLine( "|" );

				// the header
				foreach (DataColumn column in dt.Columns)
					bob.Append( string.Format( "|{0}", column.ColumnName.PadRight( dic[column.ColumnName] ) ) ) ; 
				bob.AppendLine( "|" );

				// pretty header line
				foreach (DataColumn column in dt.Columns)
					bob.Append( string.Format( "|{0}", new string( '_', dic[column.ColumnName] ) ) ) ; 
				bob.AppendLine( "|" );

				// rows of data
				foreach (DataRow row in dt.Rows)
				{
					foreach (DataColumn column in dt.Columns)
					{
						string columnname = column.ColumnName ;

						string val = "";
						if ( ! row.IsNull( columnname ))
							val = row[columnname].ToString().Trim();

						bob.Append( string.Format( "|{0}", val.PadRight( dic[columnname] ) ) ) ; 
					}
					bob.AppendLine( "|" );
				}

				// pretty footer line
				foreach (DataColumn column in dt.Columns)
					bob.Append( string.Format( "|{0}", new string( '_', dic[column.ColumnName] ) ) ) ; 
				bob.AppendLine( "|" );

			}

			return bob.ToString() ;
		}

		//--------------------------------------------------------------------------------------------------------------------		

		protected Dictionary<string, int> GetColumnWidths( DataTable dt )
		{
			Dictionary<string, int> dic = new Dictionary<string, int>();

			// do the header
			foreach (DataColumn column in dt.Columns)
			{
				string columnname = column.ColumnName ;
				dic.Add(columnname, columnname.Length );
			}

			// do the data
			foreach (DataRow row in dt.Rows)
				foreach (DataColumn column in dt.Columns)
				{
					string columnname = column.ColumnName ;

					string val = "";
					if ( ! row.IsNull(columnname))
						val = row[columnname].ToString().Trim();

					int len = val.Length ;
					if (len > dic[columnname])
						dic[columnname] = len;
				}

			return dic;
		}

		//--------------------------------------------------------------------------------------------------------------------		

	} // end class
}
