using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection;
using System.IO ;

namespace alby.codegen.generator
{
	public class StoredProcedureTableTypeEnumeratorBlock : CodeBlockBase 
	{
		public StoredProcedureTableTypeEnumeratorBlock(	StreamWriter									sw, 
														int												tabs, 
														string											theclass, 
														List< Tuple<string,string,Int16,Byte,Byte> >	columns ) 
			: base( sw, tabs ) 
		{
			Helper h = new Helper() ;

			h.Write( sw, tabs, "scg.List< " + theclass + " > _list = null ;" ) ;
			h.Write( sw, tabs, " " ) ;

			h.Write( sw, tabs  , "public " + theclass + "list( scg.List< " + theclass + " > list )" ) ;
			h.Write( sw, tabs  , "{" ) ;
			h.Write( sw, tabs+1, "_list = list ;" ) ;
			h.Write( sw, tabs  , "}" ) ;
			h.Write( sw, tabs  , " " ) ;

			h.Write( sw, tabs,   "scg.IEnumerator< mss.SqlDataRecord > scg.IEnumerable< mss.SqlDataRecord >.GetEnumerator()" ) ;
			h.Write( sw, tabs,   "{" ) ;
			h.Write( sw, tabs+1, "var sdr = new mss.SqlDataRecord" ) ;
			h.Write( sw, tabs+1, "(" ) ;

			int i = 0 ;
			foreach ( Tuple<string,string,Int16,Byte,Byte> column in columns ) 
			{
				i++ ;

				string comma = "" ;
				if ( i != columns.Count )
					comma = "," ;

				var columnname  = h.GetCsharpColumnName( column.Item1, theclass );

				SqlDbType sqltype ;
				string	  udttype ;

				try
				{
					sqltype = h.GetSqlDbTypeForStoredProcedure( column.Item2 ) ;
					udttype = "" ;
				}
				catch( Exception )
				{
					sqltype = SqlDbType.Udt ;
					udttype = h.GetSqlDbTypeForStoredProcedureUdtTableType( column.Item2 )  ;
				}

				var sqlmetadata = this.GetSqlMetaDataConstructor( column.Item1, sqltype, udttype, column.Item3, column.Item4, column.Item5 ) ;   

				h.Write( sw, tabs+2, sqlmetadata + comma ) ;
			}
			h.Write( sw, tabs+1, ") ;" );
			h.Write( sw, tabs+1, " " );


			h.Write( sw, tabs+1, "foreach ( var i in this._list ) " );
			h.Write( sw, tabs+1, "{" );

			i = 0 ;
			foreach ( Tuple<string,string,Int16,Byte,Byte> column in columns ) 
			{
				var tabletypesettype = h.GetStoredProcedureTableTypeSetType( column.Item2 ) ; 

				var csharpcolumnname = h.GetCsharpColumnName( column.Item1, theclass );
				var csharpcolumntype = h.GetCsharpColumnTypeForStoredProcedure( column.Item2 ) ;

				h.Write( sw, tabs+2, "if ( i." + csharpcolumnname + " == null )" ) ;
				h.Write( sw, tabs+3, "sdr.SetDBNull( " + i + " ) ;" ) ;
				h.Write( sw, tabs+2, "else" ) ;

				// udt hack
				if ( tabletypesettype == "SetValue" ) 
					if ( csharpcolumntype != "TimeSpan?" )
					{
						csharpcolumnname += ".ToString()" ;
						csharpcolumntype = csharpcolumntype.Replace( "?", "" ) ;
					}

				if ( csharpcolumntype.EndsWith( "?" ) ) 
					 h.Write( sw, tabs+3, "sdr." + tabletypesettype + "( " + i + ", i." + csharpcolumnname + ".Value ) ; " ) ; 
				else
				if ( tabletypesettype == "SetBytes" )
					 h.Write( sw, tabs+3, "sdr." + tabletypesettype + "( " + i + ", 0, i." + csharpcolumnname + ", 0, i." + csharpcolumnname + ".Length ) ; " ) ;
				else
					 h.Write( sw, tabs+3, "sdr." + tabletypesettype + "( " + i + ", i." + csharpcolumnname + " ) ; " ) ;

				h.Write( sw, tabs+1, " " );
				i++ ;
			}

			h.Write( sw, tabs+2, "yield return sdr ; " );
			h.Write( sw, tabs+1, "}" );
			h.Write( sw, tabs+1, " " );
		}

		protected string GetSqlMetaDataConstructor( string name, SqlDbType sqltype, string udttype, int maxlength, int precision, int scale )
		{
			string str = "new mss.SqlMetaData( \"" + name + "\", sd.SqlDbType." + sqltype.ToString() ;

			switch( sqltype )
			{
				case SqlDbType.Timestamp:
					str = "new mss.SqlMetaData( \"" + name + "\", sd.SqlDbType.Timestamp, true, false, sds.SortOrder.Unspecified, -1"  ;
					break ;

				case SqlDbType.Udt:
					str = "new mss.SqlMetaData( \"" + name + "\", sd.SqlDbType.NVarChar, -1"  ;
					break ;

				case SqlDbType.Decimal:
					str += ", " + precision + ", " + scale ;
					break ;

				case SqlDbType.NChar:
				case SqlDbType.NVarChar:
					if ( maxlength == -1  )
						str += ", -1" ;
					else
						str += ", " + maxlength / 2 ;
					break ;

				case SqlDbType.Binary:
				case SqlDbType.Char:
				case SqlDbType.VarBinary:
				case SqlDbType.VarChar:
					str += ", " + maxlength ;
					break ;

				//case SqlDbType.NText:
				//case SqlDbType.Image:
				//case SqlDbType.Text:
				default:
					break ;
			}

			str += " ) " ;
			return str ;
		}

	} // end class
}
