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
using alby.core.threadpool ;

// unit test - populate objects per table

namespace alby.codegen.generator
{
	public partial class UnitTestGeneratorPopulatePerTableParameters
	{
		 public Program							p ;
		 public string							theclass ;
		 public string							baseclass ;
		 public string							fqtable ;
		 public List<string>					identitycolumns ;
		 public List<string>					computedcolumns ;
		 public List<string>					timestampcolumns ;
		 public List< Tuple<string,string> >	columns ;
		 public Exception						exception ;

	} // end class

	//---------------------------------------------------------------------------------------------------------------------------
	//---------------------------------------------------------------------------------------------------------------------------
	//---------------------------------------------------------------------------------------------------------------------------

	public partial class UnitTestGeneratorPopulatePerTableThreadPoolItem : MyThreadPoolItemBase
	{
		protected UnitTestGeneratorPopulatePerTableParameters _param ;

		public UnitTestGeneratorPopulatePerTableThreadPoolItem( UnitTestGeneratorPopulatePerTableParameters utgpptp ) 
		{
			_param = utgpptp ;
		}

		//---------------------------------------------------------------------------------------------------------------------------

		public override void Run() 
		{
			Helper h = new Helper() ;

			try
			{
				this.DoPopulatePerTable() ;
			}
			catch( Exception ex )
			{
				_param.exception = ex ;
				h.Message( "[DoPopulatePerTable() EXCEPTION]\n{0}", ex ) ;
			}
		}

		//---------------------------------------------------------------------------------------------------------------------------

		protected void DoPopulatePerTable()
		{
			Helper h = new Helper() ;

			Tuple<string,string> schematable = h.SplitSchemaFromTable( _param.fqtable ) ;

			string aclass = h.GetCsharpClassName( _param.p._prefixObjectsWithSchema, schematable.Item1, schematable.Item2 ) ;

			string csharpfile = _param.p._unitTestDirectory + @"\" + _param.theclass + ".Populate." + aclass + ".cs";

			h.MessageVerbose( "[{0}]", csharpfile );
			using (StreamWriter sw = new StreamWriter( csharpfile, false, UTF8Encoding.UTF8))
			{
				int tab = 0;

				// header
				h.WriteCodeGenHeader(sw);
				h.WriteUsingUnitTest(sw, _param.p._unitTestTableNamespacePrefix, _param.p._unitTestTableNamespace);

				// namespace
				using (NamespaceBlock nsb = new NamespaceBlock(sw, tab++, _param.p._unitTestNamespace))
				{
					using (ClassBlock cb = new ClassBlock(sw, tab++, _param.theclass, _param.baseclass))
					{
						h.Write(sw, tab, "protected void Populate!#( bool insert, $.# obj )".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass).Replace("$", _param.p._unitTestTableNamespacePrefix));
						
						h.Write(sw, tab, "{");							
						this.PopulateTable( aclass, sw, tab ) ;	
						h.Write(sw, tab, "}");
						
					} // end class		
				} // end namespace
			} // eof
		}

		//---------------------------------------------------------------------------------------------------------------------------

		// populate the field for a table 

		protected void PopulateTable( string aclass, StreamWriter sw, int tab ) 
		{
			Helper h = new Helper() ;

			Tuple<string,string> schematable = h.SplitSchemaFromTable( _param.fqtable ) ;

			// get list of pk columns in this table				
			List<string> pkmap = _param.p._di.PrimaryKeyColumns.Get( _param.fqtable, "PK" );
			
			// get list of fk columns in this table
			List<string> fkmap = _param.p._di.ForeignKeyColumnsOfForeignKeyTable.Get( _param.fqtable );

			// these are the columns in this table are foreign keys in dependant tables
			List<string> fkcolumns = _param.p._di.ForeignKeyColumnsOfPrimaryKeyTable.Get( _param.fqtable ) ; 
	
			h.Write(sw, tab+1, "if ( insert )" ) ;
			h.Write(sw, tab+1, "{");
						
			foreach ( Tuple<string,string> column in _param.columns )
			{
				// dont do non-writeable columns
				if ( _param.identitycolumns.Contains ( column.Item1  ) ) continue;
				if ( _param.computedcolumns.Contains ( column.Item1  ) ) continue;
				if ( _param.timestampcolumns.Contains( column.Item1  ) ) continue;

				string columnname = h.GetCsharpColumnName( column.Item1, aclass ) ;
				string columntype = h.GetCsharpColumnType( column.Item2 ) ;

				string type = columntype.Replace("?", "");

				if ( fkmap.Contains( column.Item1 ) )
				{
					Tuple<string,string> pk = this.GetParentTableColumnOfForeignKeyTableColumn( _param.fqtable, column.Item1 ) ; 
					string pkcolumn = pk.Item2 ;

					Tuple<string,string> pkschematable = h.SplitSchemaFromTable( pk.Item1 ) ;

					string pkcsharpclass  = h.GetCsharpClassName( _param.p._prefixObjectsWithSchema, pkschematable.Item1, pkschematable.Item2 ) ;
					string pkcsharpcolumn = h.GetCsharpColumnName( pkcolumn, pkcsharpclass ) ;

					h.Write(sw, tab + 2, "obj.# = obj1*@ != null ? obj1*@.^ : obj0*@.^ ;"
												.Replace( "*", h.IdentifierSeparator )
												.Replace("#", columnname)
												.Replace("@", pkcsharpclass)
												.Replace("^", pkcsharpcolumn ));
				}
				else
				if ( pkmap.Contains( column.Item1 ) &&
					 this.IsNumericType( columntype ) )
				{
					h.Write(sw, tab + 2,
									"obj.# = to!( factory*@Factory.GetNextIdˡ( _connection, $.@.column*#, true ).ToString() ) ;"
										.Replace( "*", h.IdentifierSeparator )
										.Replace( "!", type ) 
										.Replace( "#", columnname )
										.Replace( "@", aclass  )
										.Replace( "$", _param.p._unitTestTableNamespacePrefix )
								);
				}
				else				
				{
					string value = this.GetTestValueForColumnType( columntype, true ) ; // insert part
					h.Write(sw, tab+2, "obj.# = @ ;".Replace("#", columnname ).Replace( "@", value ) );
				}
			}

			h.Write(sw, tab+1, "}");
			h.Write(sw, tab+1, "else // update");
			h.Write(sw, tab+1, "{");

			int updatecolumns = 0 ;
			foreach ( Tuple<string,string> column in _param.columns )
			{
				// dont do non-writeable columns
				if ( _param.identitycolumns.Contains ( column.Item1 ) ) continue;
				if ( _param.computedcolumns.Contains ( column.Item1 ) ) continue;
				if ( _param.timestampcolumns.Contains( column.Item1 ) ) continue;
				
				// dont do any fields in the table that are foreign key fields to other tables
				if ( fkcolumns.Contains( column.Item1 ) ) continue ;

				// dont do primary key columns for update
				if ( pkmap.Contains( column.Item1 ) ) continue;

				// dont do foreign key fields 
				if ( fkmap.Contains( column.Item1 ) ) continue ;

				updatecolumns++ ;

				string columnname = h.GetCsharpColumnName( column.Item1, aclass);
				string columntype = h.GetCsharpColumnType( column.Item2 );

				string value = this.GetTestValueForColumnType( columntype, false); // update part
				h.Write(sw, tab + 2, "obj.# = @ ;".Replace("#", columnname ).Replace("@", value));
			}

			if ( updatecolumns == 0 )
			{
				h.Write(sw, tab + 2, "obj.IsDirtyˡ = true ;" );
			}

			h.Write(sw, tab+1, "}");
			h.Write(sw, tab+1, " ");
		}

		//---------------------------------------------------------------------------------------------------------------------------

		protected string GetTestValueForColumnType(string type, bool insert)
		{
			if ( insert )
			{
				if (type == "bool?")				return "true";
				if (type == "byte?")				return "0xFE";
				if (type == "decimal?")				return "-1.0m";
				if (type == "double?")				return "-1.0d";
				if (type == "float?")				return "-1.0f";
				if (type == "int?")					return "-100";
				if (type == "long?")				return "-100L";
				if (type == "short?")				return "-100";
				if (type == "string")				return "\"@\"";
				if (type == "Guid?")				return "new Guid( \"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa\" ) ";
				if (type == "DateTime?")			return "new DateTime(1900,1,1)";
				if (type == "DateTimeOffset?")		return "new DateTimeOffset( new DateTime(1900,1,3) )";
				if (type == "byte[]")				return "new byte[] { 0x01 }";
				if (type == "TimeSpan?")			return "new TimeSpan(1)";
				if (type == "mst.SqlGeography")		return "mst.SqlGeography.Parse( \"POINT (1 1)\" )" ;
				if (type == "mst.SqlGeometry")		return "mst.SqlGeometry.Parse( \"POINT (3 3)\" )";
				if (type == "mst.SqlHierarchyId?")	return "mst.SqlHierarchyId.Parse( \"/1/2/3/\" )";
			}
			else // update
			{
				if (type == "bool?")				return "false";
				if (type == "byte?")				return "0xFF";
				if (type == "decimal?")				return "-2.0m";
				if (type == "double?")				return "-2.0d";
				if (type == "float?")				return "-2.0f";
				if (type == "int?")					return "-200";
				if (type == "long?")				return "-200L";
				if (type == "short?")				return "-200";
				if (type == "string")				return "\"#\"";
				if (type == "Guid?")				return "new Guid( \"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb\" ) ";
				if (type == "DateTime?")			return "new DateTime(1901,12,31)";
				if (type == "DateTimeOffset?")		return "new DateTimeOffset( new DateTime(1901,12,29) )";
				if (type == "byte[]")				return "new byte[] { 0x02 }";
				if (type == "TimeSpan?")			return "new TimeSpan(2)";
				if (type == "mst.SqlGeography")		return "mst.SqlGeography.Parse( \"POINT (2 2)\" )" ;
				if (type == "mst.SqlGeometry")		return "mst.SqlGeometry.Parse( \"POINT (4 4)\" )";
				if (type == "mst.SqlHierarchyId?")	return "mst.SqlHierarchyId.Parse( \"/3/4/5/\" )";
			}		

			// unknown type - return soething that cant compile?
			throw new ApplicationException(string.Format("[{0}] unknown type for which to get test value.", type ));
		}

		//------------------------------------------------------------------------------------------------------------------

		protected bool IsNumericType( string type ) 
		{
			if (type == "byte?")		return true ;
			if (type == "decimal?")		return true ;
			if (type == "double?")		return true ;
			if (type == "float?")		return true ;
			if (type == "int?")			return true ;
			if (type == "long?")		return true ;
			if (type == "short?")		return true ;
		
			return false ;
		}

		//------------------------------------------------------------------------------------------------------------------

		protected Tuple<string,string> GetParentTableColumnOfForeignKeyTableColumn( string table, string column ) 
		{
			// dont match on self referring tables if we can avoid it
			Helper	  h  = new Helper() ;
			DataTable dt = _param.p._di.GetDatabaseInfo().Tables[ "fk" ] ;

			foreach( DataRow row in dt.Rows )
			{
				if ( table == row[ "FkTable" ].ToString() )
					if ( column == row[ "FkTableColumnName" ].ToString() )
						if ( table != row[ "PkTable" ].ToString() )	
							return new Tuple<string,string>( row[ "PkTable" ].ToString(), row[ "PkTableColumnName" ].ToString() ) ;
			}
		
			// have to match on self referring table, if available
			foreach( DataRow row in dt.Rows )
			{
				if ( table == row[ "FkTable" ].ToString() )
					if ( column == row[ "FkTableColumnName" ].ToString() )
						return new Tuple<string,string>( row[ "PkTable" ].ToString(), row[ "PkTableColumnName" ].ToString() ) ;
			}

			// cant find the parent key
			throw new ApplicationException( string.Format("Cant find parent key of foreign key [{0}].[{1}]", table, column ));
		}

		//---------------------------------------------------------------------------------------------------------------------------

	} // end class

}

