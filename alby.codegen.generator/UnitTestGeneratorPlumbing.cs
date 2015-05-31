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

// unit test - plumbing: generate entry point, connection string, member variables

namespace alby.codegen.generator
{
	public partial class UnitTestGenerator
	{
		// generate CodeGenUnitTestClass.cs - entry point    
		
		protected void EntryPoint( Program p, string theclass, string baseclass )
		{
			Helper h = new Helper() ;

			string csharpfile = p._unitTestDirectory + @"\" + theclass + ".cs";

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
					h.Write(sw, tab, "[nu.TestFixture]");
					using (ClassBlock cb = new ClassBlock(sw, tab++, theclass, baseclass))
					{
						// create constructor 
						using (UnitTestConstructorBlock conb = new UnitTestConstructorBlock(sw, tab, theclass))
						{}

						// unit test entry point
						h.Write(sw, tab, "[nu.Test]");
						h.Write(sw, tab, "public void CodegenUnitTest_InsertUpdateDelete()");
						h.Write(sw, tab, "{");
						h.Write(sw, tab+1, "this.AssertObjectsNew() ;");
						h.Write(sw, tab+1, " ");
						h.Write(sw, tab+1, "this.PopulateObjectsForInsert() ;");
						h.Write(sw, tab+1, "this.AssertObjectsAfterInsert() ;");
						h.Write(sw, tab+1, " ");
						h.Write(sw, tab+1, "this.PopulateObjectsForUpdate() ;");
						h.Write(sw, tab+1, "this.AssertObjectsAfterUpdate() ;");
						h.Write(sw, tab+1, " ");
						h.Write(sw, tab+1, "this.DeleteObjects() ;");
						h.Write(sw, tab, "}");

						h.Write(sw, tab,   " ");
						h.Write(sw, tab,   "public override void SetUp()");
						h.Write(sw, tab,   "{");
						h.Write(sw, tab+1, "base.SetUp();");
						h.Write(sw, tab+1, "this.UnitTestSetUp();");
						h.Write(sw, tab,   "}");

						h.Write(sw, tab+1, " ");
						h.Write(sw, tab,   "public override void TearDown()");
						h.Write(sw, tab,   "{");
						h.Write(sw, tab+1, "this.UnitTestTearDown();");
						h.Write(sw, tab+1, "base.TearDown();");
						h.Write(sw, tab,   "}");

					} // end class		
				} // end namespace
			} // eof		
		}

		// generate CodeGenUnitTestClass.CodegenRunTimeSettings.cs 
		// this file in NOT overwritten if it exists
		
		protected void CodegenRunTimeSettings( Program p, string theclass, string baseclass)
		{
			Helper h = new Helper() ;
			 
			string csharpfile = p._unitTestDirectory + @"\" + theclass + ".CodegenRunTimeSettings.cs";
			if (!File.Exists(csharpfile))
			{
				h.MessageVerbose( "[{0}]", csharpfile );
				using (StreamWriter sw = new StreamWriter(csharpfile, false, UTF8Encoding.UTF8))
				{
					int tab = 0;

					// header
					h.WriteCodeGenHeader2(sw);
					h.WriteUsingUnitTest(sw, p._unitTestTableNamespacePrefix, p._unitTestTableNamespace);

					// namespace
					using (NamespaceBlock nsb = new NamespaceBlock(sw, tab++, p._unitTestNamespace))
					{
						using (ClassBlock cb = new ClassBlock(sw, tab++, theclass, baseclass))
						{
							h.Write(sw, tab, "public override string ConnectionString");
							h.Write(sw, tab, "{");
							h.Write(sw, tab + 1, "get");
							h.Write(sw, tab + 1, "{");
							h.Write(sw, tab + 2, "return @\"" + p._connectionString + "\" ;");
							h.Write(sw, tab + 1, "}");
							h.Write(sw, tab, "}");

							h.Write(sw, tab, " ");
							h.Write(sw, tab, "public override bool QuietMode");
							h.Write(sw, tab, "{");
							h.Write(sw, tab + 1, "get");
							h.Write(sw, tab + 1, "{");
							h.Write(sw, tab + 2, "return false ;");
							h.Write(sw, tab + 1, "}");
							h.Write(sw, tab, "}");

							h.Write(sw, tab, " ");
							h.Write(sw, tab, "public override bool DisableCheckConstraints");
							h.Write(sw, tab, "{");
							h.Write(sw, tab + 1, "get");
							h.Write(sw, tab + 1, "{");
							h.Write(sw, tab + 2, "return true ;");
							h.Write(sw, tab + 1, "}");
							h.Write(sw, tab, "}");

							h.Write(sw, tab, " ");
							h.Write(sw, tab, "public override bool DisableTriggers");
							h.Write(sw, tab, "{");
							h.Write(sw, tab + 1, "get");
							h.Write(sw, tab + 1, "{");
							h.Write(sw, tab + 2, "return true ;");
							h.Write(sw, tab + 1, "}");
							h.Write(sw, tab, "}");

							h.Write(sw, tab, " ");
							h.Write(sw, tab, "public override System.Transactions.IsolationLevel TransactionIsolationLevel");
							h.Write(sw, tab, "{");
							h.Write(sw, tab + 1, "get");
							h.Write(sw, tab + 1, "{");
							h.Write(sw, tab + 2, "return System.Transactions.IsolationLevel.Serializable ;");
							h.Write(sw, tab + 1, "}");
							h.Write(sw, tab, "}");

							h.Write(sw, tab, " ");
							h.Write(sw, tab, "public void UnitTestSetUp()");
							h.Write(sw, tab, "{");
							h.Write(sw, tab, "}");

							h.Write(sw, tab, " ");
							h.Write(sw, tab, "public void UnitTestTearDown()");
							h.Write(sw, tab, "{");
							h.Write(sw, tab, "}");

						} // end class		
					} // end namespace
				} // eof
			}
		}

		// generate CodeGenUnitTestClass.State.cs 

		protected void State( Program p, string theclass, string baseclass)
		{
			Helper h = new Helper() ;

			string csharpfile = p._unitTestDirectory + @"\" + theclass + ".State.cs";

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
						// factory for every table
						List<string> allTables = p._di.Tables.Get() ; 
						foreach ( string fqtable in allTables )
						{
							Tuple<string,string> schematable = h.SplitSchemaFromTable( fqtable ) ;

							string aclass = h.GetCsharpClassName( p._prefixObjectsWithSchema, schematable.Item1, schematable.Item2 );

							h.Write(sw, tab, "// #".Replace("#", aclass));
							h.Write(sw, tab, "protected $.#Factory factory!#Factory = new $.#Factory() ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass).Replace("$",p._unitTestTableNamespacePrefix));
							h.Write(sw, tab, " ");
							h.Write(sw, tab, "protected $.# obj0!# = new $.#() ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass).Replace("$", p._unitTestTableNamespacePrefix));
							h.Write(sw, tab, "protected $.# obj1!# = null ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass).Replace("$", p._unitTestTableNamespacePrefix));
							h.Write(sw, tab, "protected $.# obj2!# = null ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass).Replace("$", p._unitTestTableNamespacePrefix));
							h.Write(sw, tab, "protected $.# obj3!# = null ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass).Replace("$", p._unitTestTableNamespacePrefix));
							h.Write(sw, tab, " ");
							h.Write(sw, tab, "protected int rowcount0!# = 0 ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab, "protected int rowcount1!# = 0 ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab, "protected int rowcount2!# = 0 ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab, "protected int rowcount3!# = 0 ;".Replace( "!", h.IdentifierSeparator ).Replace("#", aclass));
							h.Write(sw, tab, " ");
						}
					} // end class		
				} // end namespace
			} // eof		
		}
	
	} // end class

} // end ns

