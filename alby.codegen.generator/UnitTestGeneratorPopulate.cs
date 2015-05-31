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

// unit test - populate objects and insert and update them

namespace alby.codegen.generator
{
	public partial class UnitTestGenerator
	{
		// generate CodeGenUnitTestClass.PopulateObjectsForInsert.cs 

		protected void PopulateObjectsForInsert( Program p, string theclass, string baseclass)
		{
			Helper h = new Helper() ;

			string csharpfile = p._unitTestDirectory + @"\" + theclass + ".PopulateObjectsForInsert.cs";

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
						h.Write(sw, tab, "protected void PopulateObjectsForInsert()");
						h.Write(sw, tab, "{");

						int i = 0;
						foreach ( string fqtable in _unitTestTables )
						{
							i++;

							Tuple<string,string> schematable = h.SplitSchemaFromTable( fqtable ) ;

							string aclass = h.GetCsharpClassName( p._prefixObjectsWithSchema, schematable.Item1, schematable.Item2 ) ;

							h.Write(sw, tab + 1, "// #".Replace("#", aclass));
							h.Write(sw, tab + 1, string.Format("acr.CodeGenEtc.ConsoleMessage( ! this.QuietMode, \"[{0}/{1}] # - insert\" ) ;".Replace("#", aclass), i, _unitTestTables.Count));
							h.Write(sw, tab + 1, "this.Populate!#( true, obj0!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, "this.PopulateOverride!#( true, obj0!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));

							// do the insert code here
							h.Write(sw, tab + 1, "base.AssertFlagsBeforeInsert( obj0!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, "rowcount0!# = factory!#Factory.GetRowCountˡ( _connection ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, "obj1!# = factory!#Factory.Saveˡ( _connection, obj0!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, "nu.Assert.IsNotNull( obj1!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, "base.AssertFlagsObjectLoaded( obj1!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, "base.AssertFlagsAfterInsert( obj0!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, "rowcount1!# = factory!#Factory.GetRowCountˡ( _connection ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, "nu.Assert.AreEqual( rowcount1!#, rowcount0!# + 1 ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, " ");
						}
						h.Write(sw, tab, "}");

					} // end class		
				} // end namespace
			} // eof
		}

		// generate CodeGenUnitTestClass.PopulateObjectsForUpdate.cs 

		protected void PopulateObjectsForUpdate( Program p, string theclass, string baseclass)
		{
			Helper h = new Helper() ;

			string csharpfile = p._unitTestDirectory + @"\" + theclass + ".PopulateObjectsForUpdate.cs";

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
						h.Write(sw, tab, "protected void PopulateObjectsForUpdate()");
						h.Write(sw, tab, "{");

						int i = 0;
						foreach ( string fqtable in _unitTestTables )
						{
							i++;

							Tuple<string,string> schematable = h.SplitSchemaFromTable( fqtable ) ;

							string aclass = h.GetCsharpClassName( p._prefixObjectsWithSchema, schematable.Item1, schematable.Item2 ) ;

							h.Write(sw, tab + 1, "// #".Replace("#", aclass));
							h.Write(sw, tab + 1, string.Format("acr.CodeGenEtc.ConsoleMessage( ! this.QuietMode, \"[{0}/{1}] # - update\" ) ;".Replace("#", aclass), i, _unitTestTables.Count));
							h.Write(sw, tab + 1, "this.Populate!#( false, obj1!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, "this.PopulateOverride!#( false, obj1!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));

							// do the update code here
							h.Write(sw, tab + 1, "base.AssertFlagsBeforeUpdate( obj1!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, "obj2!# = factory!#Factory.Saveˡ( _connection, obj1!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, "nu.Assert.IsNotNull( obj2!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, "base.AssertFlagsObjectLoaded( obj2!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, "base.AssertFlagsAfterUpdate( obj1!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, "rowcount2!# = factory!#Factory.GetRowCountˡ( _connection ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, "nu.Assert.AreEqual( rowcount2!#, rowcount1!# ) ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, " ");
						}
						h.Write(sw, tab, "}");

					} // end class		
				} // end namespace
			} // eof
		}		

	} // end class

} // end ns

