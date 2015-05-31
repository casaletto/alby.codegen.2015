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

// unit test - delete objects

namespace alby.codegen.generator
{
	public partial class UnitTestGenerator
	{
		// generate CodeGenUnitTestClass.DeleteObjects.cs 
		// have to delete objects in reverse referential integrity order

		protected void DeleteObjects( Program p, string theclass, string baseclass )
		{
			Helper h = new Helper() ;

			string csharpfile = p._unitTestDirectory + @"\" + theclass + ".DeleteObjects.cs";

			if (!h.IgnoreCodegenFile(csharpfile)) // dont do this if we find /do not codegen/ in the file
			{
				h.MessageVerbose( "[{0}]", csharpfile ) ;
				using (StreamWriter sw = new StreamWriter(csharpfile, false, UTF8Encoding.UTF8))
				{
					int tab = 0;

					// header
					h.WriteCodeGenHeader(sw);
					h.WriteUsingUnitTest(sw, p._unitTestTableNamespacePrefix, p._unitTestTableNamespace);

					// namespace
					using (NamespaceBlock nsb = new NamespaceBlock(sw, tab++, p._unitTestNamespace))
					{
						using (ClassBlock cb = new ClassBlock(sw, tab++, theclass, baseclass))
						{
							h.Write(sw, tab, "protected void DeleteObjects()");
							h.Write(sw, tab, "{");

							int i = 0;
							foreach (string fqtable in _unitTestTablesReverse)
							{
								i++;

								Tuple<string,string> schematable = h.SplitSchemaFromTable( fqtable ) ;

								string aclass = h.GetCsharpClassName( p._prefixObjectsWithSchema, schematable.Item1, schematable.Item2 );

								h.Write(sw, tab + 1, "// #".Replace("#", aclass));
								h.Write(sw, tab + 1, string.Format("acr.CodeGenEtc.ConsoleMessage( ! this.QuietMode, \"[{0}/{1}] # - delete\" ) ;".Replace("#", aclass), i, _unitTestTables.Count));

								// do the delete code here
								h.Write(sw, tab + 1, "obj2!#.MarkForDeletionˡ = true ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
								h.Write(sw, tab + 1, "base.AssertFlagsBeforeDelete( obj2!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
								h.Write(sw, tab + 1, "obj3!# = factory!#Factory.Saveˡ( _connection, obj2!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
								h.Write(sw, tab + 1, "nu.Assert.IsNull( obj3!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
								h.Write(sw, tab + 1, "base.AssertFlagsAfterDelete( obj2!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
								h.Write(sw, tab + 1, "rowcount3!# = factory!#Factory.GetRowCountˡ( _connection ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
								h.Write(sw, tab + 1, "nu.Assert.AreEqual( rowcount3!#, rowcount0!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
								h.Write(sw, tab + 1, " ");
							}
							h.Write(sw, tab, "}");

						} // end class		
					} // end namespace
				} // eof
			}

		}
	}

}
