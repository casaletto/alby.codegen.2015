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

// unit test - populate override objects per table

namespace alby.codegen.generator
{
	public partial class UnitTestGeneratorPopulateOverridePerTableParameters
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

	public partial class UnitTestGeneratorPopulateOverridePerTableThreadPoolItem : MyThreadPoolItemBase
	{
		protected UnitTestGeneratorPopulateOverridePerTableParameters _param ;

		public UnitTestGeneratorPopulateOverridePerTableThreadPoolItem( UnitTestGeneratorPopulateOverridePerTableParameters utgpptp ) 
		{
			_param = utgpptp ;
		}

		public override void Run() 
		{
			Helper h = new Helper() ;

			try
			{
				DoPopulateOverridePerTable() ;
			}
			catch( Exception ex )
			{
				_param.exception = ex ;
				h.Message( "[DoPopulateOverridePerTable() EXCEPTION]\n{0}", ex ) ;
			}
		}

		protected void DoPopulateOverridePerTable()
		{
			Helper h = new Helper() ;

			Tuple<string,string> schematable = h.SplitSchemaFromTable( _param.fqtable ) ;

			string aclass = h.GetCsharpClassName( _param.p._prefixObjectsWithSchema, schematable.Item1, schematable.Item2 ) ;

			string csharpfile = _param.p._unitTestDirectory + @"\" + _param.theclass + ".PopulateOverride." + aclass + ".cs";

			if ( ! h.IgnoreCodegenFile(csharpfile) ) // dont do this if we find /do not codegen/ in the file
			{
				h.MessageVerbose( "[{0}]", csharpfile );
				using (StreamWriter sw = new StreamWriter(csharpfile, false, UTF8Encoding.UTF8))
				{
					int tab = 0;

					// header
					h.WriteCodeGenHeader(sw);
					h.WriteUsingUnitTest(sw, _param.p._unitTestTableNamespacePrefix, _param.p._unitTestTableNamespace);

					// namespace
					using ( NamespaceBlock nsb = new NamespaceBlock( sw, tab++, _param.p._unitTestNamespace))
					{
						using ( ClassBlock cb = new ClassBlock( sw, tab++, _param.theclass, _param.baseclass ))
						{
							h.Write(sw, tab, "protected void PopulateOverride!#( bool insert, $.# obj )".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass).Replace("$", _param.p._unitTestTableNamespacePrefix));

							h.Write(sw, tab, "{") ;
							PopulateOverrideTable( aclass, sw, tab );
							h.Write(sw, tab, "}");

						} // end class		
					} // end namespace
				} // eof
			} // if
		}

		// populate override for a table 

		protected void PopulateOverrideTable( string aclass, StreamWriter sw, int tab )
		{
			Helper h = new Helper() ;

			h.Write(sw, tab+1, "if ( insert )" ) ;
			h.Write(sw, tab+1, "{");

			foreach ( var column in _param.columns)
			{
				// dont do non-writeable columns
				if ( _param.identitycolumns.Contains ( column.Item1 )) continue;
				if ( _param.computedcolumns.Contains ( column.Item1 )) continue;
				if ( _param.timestampcolumns.Contains( column.Item1 )) continue;

				string columnname = h.GetCsharpColumnName( column.Item1, aclass);

				h.Write(sw, tab+2, "// obj.# = null ;".Replace("#", columnname ));
			}
			
			h.Write(sw, tab+1, "}");
			h.Write(sw, tab+1, "else // update");
			h.Write(sw, tab+1, "{");

			foreach ( var column in _param.columns)
			{
				// dont do non-writeable columns
				if ( _param.identitycolumns.Contains ( column.Item1 )) continue;
				if ( _param.computedcolumns.Contains ( column.Item1 )) continue;
				if ( _param.timestampcolumns.Contains( column.Item1 )) continue;

				string columnname = h.GetCsharpColumnName( column.Item1, aclass);

				h.Write( sw, tab+2, "// obj.# = null ;".Replace("#", columnname ));
			}

			h.Write(sw, tab+1, "}");
			h.Write(sw, tab+1, " ");	
		}

	} // end class

}
