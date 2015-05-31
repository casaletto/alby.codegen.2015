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
	public class RowConstructorBlock : CodeBlockBase 
	{
		public RowConstructorBlock( StreamWriter					sw, 
									int								tabs, 
									string							theClass,  
									List< Tuple<string,string> >	columns, 
									List<string>					pkcolumns,
									string							concurrencycolumn) 
			: base( sw, tabs ) 
		{
			Helper h = new Helper() ;

			// do the column names
			foreach( var column in columns )
				h.Write(sw, tabs, "public const string column!".Replace( "!", h.IdentifierSeparator )  + h.GetCsharpColumnName( column.Item1, theClass ) + "  = \"" + column.Item1 + "\" ;");

			h.Write(sw, tabs, " ");

			// do the actual properties 
			foreach( var column in columns )
			{
				string columnname = h.GetCsharpColumnName( column.Item1, theClass );
				string columntype = h.GetCsharpColumnType( column.Item2 );

				h.Write(sw, tabs, "public " + columntype + " " + columnname);
				h.Write(sw, tabs, "{");

				h.Write(sw, tabs + 1, "get");
				h.Write(sw, tabs + 1, "{");
				h.Write(sw, tabs + 2, "return base.GetValueˡ<" + columntype + ">( _dicˡ, column!".Replace( "!", h.IdentifierSeparator ) + columnname + " ) ; " ) ;
				h.Write(sw, tabs + 1, "}");

				h.Write(sw, tabs + 1, "set");
				h.Write(sw, tabs + 1, "{");
				h.Write(sw, tabs + 2, "base.SetValueˡ<" + columntype + ">( _dicˡ, column!".Replace( "!", h.IdentifierSeparator ) + columnname + ", value, ref _dirtyˡ ) ; " ) ;
				h.Write(sw, tabs + 1, "}");

				h.Write(sw, tabs, "}");
				h.Write(sw, tabs, " ");
			}

			// do constructor
			h.Write(sw, tabs, "public " + theClass + "() : base()" );
			h.Write(sw, tabs, "{");

			// add fields 
			foreach( var column in columns )
				h.Write(sw, tabs + 1, "base._dicˡ[ column!".Replace( "!", h.IdentifierSeparator ) + h.GetCsharpColumnName( column.Item1, theClass ) + " ] = null ;");

			h.Write(sw, tabs, " ");
			
			// add primary keys 
			if ( pkcolumns != null )
				foreach( string pkcolumn in pkcolumns )
					h.Write(sw, tabs + 1, "base._dicPKˡ[ column!".Replace( "!", h.IdentifierSeparator ) + h.GetCsharpColumnName( pkcolumn, theClass  ) + " ] = null ;");

			h.Write(sw, tabs, " ");

			// concurrency field
			foreach( var column in columns )
				if ( column.Item1 == concurrencycolumn )
				{
					h.Write(sw, tabs + 1, "base.ConcurrencyColumnˡ = \"" + h.GetCsharpColumnName( column.Item1, theClass ) + "\" ;");
					break ;				
				}				

		} // end
	
	}
}
