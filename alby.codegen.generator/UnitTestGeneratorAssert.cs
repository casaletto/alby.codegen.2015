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

// unit test - asserts: new, after an insert or update

namespace alby.codegen.generator
{
	public partial class UnitTestGenerator
	{
		// generate CodeGenUnitTestClass.AssertObjectsNew.cs 

		protected void AssertObjectsNew( Program p, string theclass, string baseclass )
		{
			Helper h = new Helper() ;

			string csharpfile = p._unitTestDirectory + @"\" + theclass + ".AssertObjectsNew.cs";

			h.MessageVerbose( "[{0}]", csharpfile );
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
						h.Write(sw, tab, "protected void AssertObjectsNew()");
						h.Write(sw, tab, "{");

						foreach (string fqtable in _unitTestTables)
						{
							Tuple<string,string> schematable = h.SplitSchemaFromTable( fqtable ) ;

							string aclass = h.GetCsharpClassName( p._prefixObjectsWithSchema, schematable.Item1, schematable.Item2 );

							h.Write(sw, tab + 1, "// #".Replace("#", aclass));
							h.Write(sw, tab + 1, "this.AssertFlagsObjectNew( obj0!# ) ; ".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, " ");
						}
						h.Write(sw, tab, "}");

					} // end class		
				} // end namespace
			} // eof
		}

		// generate CodeGenUnitTestClass.AssertObjectsAfterInsert.cs 

		protected void AssertObjectsAfterInsert( Program p, string theclass, string baseclass)
		{
			Helper h = new Helper() ;

			string csharpfile = p._unitTestDirectory + @"\" + theclass + ".AssertObjectsAfterInsert.cs";

			h.MessageVerbose( "[{0}]", csharpfile );
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
						h.Write(sw, tab, "protected void AssertObjectsAfterInsert()");
						h.Write(sw, tab, "{");

						foreach ( string fqtable in _unitTestTables )
						{
							Tuple<string,string> schematable = h.SplitSchemaFromTable( fqtable ) ;
						
							string aclass = h.GetCsharpClassName( p._prefixObjectsWithSchema, schematable.Item1, schematable.Item2 );

							h.Write(sw, tab + 1, "// #".Replace("#", aclass));
							h.Write(sw, tab + 1, "this.Assert!#( true, obj1!#, obj0!# ) ; ".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, " ");
						}
						h.Write(sw, tab, "}");

					} // end class		
				} // end namespace
			} // eof
		}

		// generate CodeGenUnitTestClass.AssertObjectsAfterUpdate.cs 

		protected void AssertObjectsAfterUpdate( Program p, string theclass, string baseclass)
		{
			Helper h = new Helper() ;

			string csharpfile = p._unitTestDirectory + @"\" + theclass + ".AssertObjectsAfterUpdate.cs";

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
						h.Write(sw, tab, "protected void AssertObjectsAfterUpdate()");
						h.Write(sw, tab, "{");

						foreach ( string fqtable in _unitTestTables )
						{
							Tuple<string,string> schematable = h.SplitSchemaFromTable( fqtable ) ;

							string aclass = h.GetCsharpClassName( p._prefixObjectsWithSchema, schematable.Item1, schematable.Item2 );

							h.Write(sw, tab + 1, "// #".Replace("#", aclass));
							h.Write(sw, tab + 1, "this.Assert!#( false, obj2!#, obj1!# ) ; ".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab + 1, " ");
						}
						h.Write(sw, tab, "}");

					} // end class		
				} // end namespace
			} // eof
		}

	} // end class

} // end ns

