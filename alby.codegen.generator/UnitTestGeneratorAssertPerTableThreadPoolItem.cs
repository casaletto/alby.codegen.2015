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

namespace alby.codegen.generator
{
	public partial class UnitTestGeneratorAssetPerTableParameters
	{
		 public Program							p ;
		 public string							theclass ;
		 public string							baseclass ;
		 public string							fqtable ;
		 public int								i ;
		 public int								tablecount ;
		 public List<string>					identitycolumns ;
		 public List<string>					computedcolumns ;
		 public List<string>					timestampcolumns ;
		 public List< Tuple<string,string> >	columns ;
		 public Exception						exception ;

	} // end class

	public partial class UnitTestGeneratorAssetPerTableThreadPoolItem : MyThreadPoolItemBase
	{
		protected UnitTestGeneratorAssetPerTableParameters _param ;

		public UnitTestGeneratorAssetPerTableThreadPoolItem( UnitTestGeneratorAssetPerTableParameters utgaptp ) 
		{
			_param = utgaptp ;
		}

		public override void Run() 
		{
			Helper h = new Helper() ;

			try
			{
				DoAssetPerTable() ;
			}
			catch( Exception ex )
			{
				_param.exception = ex ;
				h.Message( "[DoAssetPerTable() EXCEPTION]\n{0}", ex ) ;
			}
		}

		protected void DoAssetPerTable()
		{
			Helper h = new Helper() ;

			Tuple<string,string> schematable = h.SplitSchemaFromTable( _param.fqtable ) ;

			string aclass = h.GetCsharpClassName( _param.p._prefixObjectsWithSchema, schematable.Item1, schematable.Item2 );
			
			string csharpfile = _param.p._unitTestDirectory + @"\" + _param.theclass + ".Assert." + aclass + ".cs";

			if ( ! h.IgnoreCodegenFile(csharpfile) ) // dont do this if we find /do not codegen/ in the file
			{
				h.MessageVerbose("[{0}]", csharpfile);
				using (StreamWriter sw = new StreamWriter(csharpfile, false, UTF8Encoding.UTF8))
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
							h.Write(sw, tab, "protected void Assert!#( bool insert, $.# newobj, $.# oldobj )"
													.Replace("!", h.IdentifierSeparator )							
													.Replace("#", aclass)
													.Replace("$", _param.p._unitTestTableNamespacePrefix));
							h.Write(sw, tab, "{");

							h.Write(sw, tab + 1, string.Format("acr.CodeGenEtc.ConsoleMessage( ! this.QuietMode, \"[{0}/{1}] # - assert\" ) ;".Replace("#", aclass), _param.i, _param.tablecount ));
							h.Write(sw, tab, " ");

							// always: assert newobj fields not null
							// Assert.IsNotNull( newobj.field ) ;

							foreach ( var column in _param.columns )
							{
								string columnname = h.GetCsharpColumnName( column.Item1, aclass);
								h.Write(sw, tab + 1, "nu.Assert.IsNotNull( newobj.#, \"@.#\" ) ;".Replace("#", columnname ).Replace("@", aclass));
							}

							h.Write(sw, tab, " ");

							// always: assert newojj fields = obldobjfields
							// AssertAreEqual( type, type )
							// dont do timestamp or computed columns or identity 

							foreach ( var column in _param.columns )
							{
								if ( _param.identitycolumns.Contains ( column.Item1 )) continue;
								if ( _param.computedcolumns.Contains ( column.Item1 )) continue;
								if ( _param.timestampcolumns.Contains( column.Item1 )) continue;

								string columnname = h.GetCsharpColumnName( column.Item1, aclass);
								h.Write(sw, tab + 1, "base.AssertAreEqual( newobj.#, oldobj.#, \"@.#\" ) ;".Replace("#", columnname ).Replace("@", aclass));
							}
							h.Write(sw, tab, " ");
							h.Write(sw, tab, "}");

						} // end class		
					} // end namespace
				} // eof

			}				

		} // end DoAssetPerTable()

	} //end class

}
