using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq ;
using System.Reflection;
using System.IO ;

namespace alby.codegen.generator
{
	public class StoredProcedureRowConstructorBlock : CodeBlockBase 
	{
		//----------------------------------------------------------------------------------------------------------------------------------------------------------------

		protected static Tuple<string,string > ConvertT5toT2( Tuple<string,string,Int16,Byte,Byte> t5 ) 
		{
			return new Tuple<string,string>( t5.Item1, t5.Item2 ) ;
		}

		//----------------------------------------------------------------------------------------------------------------------------------------------------------------

		public StoredProcedureRowConstructorBlock(	StreamWriter								sw, 
													int											tabs, 
													string										theclass, 
													List<Tuple<string,string,Int16,Byte,Byte>>	columns,
													bool										isSqlTypes = false ) :
			this(	sw, 
					tabs, 
					theclass, 
					columns.ConvertAll< Tuple<string,string > >( ConvertT5toT2 ), 
					isSqlTypes ) 
		{
		}

		//----------------------------------------------------------------------------------------------------------------------------------------------------------------

		public StoredProcedureRowConstructorBlock(	StreamWriter					sw, 
													int								tabs, 
													string							theclass, 
													List< Tuple<string,string> >	columns,
													bool							isSqlTypes = false ) 
			: base( sw, tabs ) 
		{
			Helper h = new Helper() ;

			// do the column names
			foreach ( Tuple<string,string> column in columns )
				h.Write(sw, tabs, "public const string column!".Replace( "!", h.IdentifierSeparator ) + h.GetCsharpColumnName( column.Item1, theclass ) + "  = \"" + column.Item1 + "\" ;");

			h.Write(sw, tabs, " ");

			// do the actual properties 
			foreach ( Tuple<string,string> column in columns ) 
			{
				string columnname = h.GetCsharpColumnName( column.Item1, theclass );

				string columntype = "" ;
				if ( isSqlTypes )
					 columntype = h.GetCsharpColumnTypeForStoredProcedure( column.Item2 ) ;
				else
					 columntype = h.GetCsharpColumnType( column.Item2 );

				h.Write(sw, tabs, "public " + columntype + " " + columnname );
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
			h.Write(sw, tabs, "public " + theclass + "() : base()" );
			h.Write(sw, tabs, "{");

			// add columns 
			foreach ( Tuple<string,string> column in columns )
				h.Write( sw, tabs + 1, "base._dicˡ[ column!".Replace( "!", h.IdentifierSeparator ) + h.GetCsharpColumnName( column.Item1, theclass ) + " ] = null ;");

			h.Write(sw, tabs, " ");
			
		} // end
	
	}
}

