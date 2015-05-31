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
	public partial class DatabaseGenerator
	{
		public void DoDatabase( Program p )
		{
			Helper h = new Helper() ;
	
			h.MessageVerbose("### Generating database classes ###");
			p._databaseSubDirectory = p._codegen.SelectSingleNode("/CodeGen/Database/@SubDirectory").Value;

			// do class XXXXDatabaseSingletonHelper
			string csharpnamespace	= p._namespace + "." + p._databaseSubDirectory;
			string theclass			= h.GetCsharpClassName( null, null, p._databaseName ) + "DatabaseSingletonHelper" ;
			string csharpfile		= p._directory + @"\" + p._databaseSubDirectory + @"\" + theclass + ".cs";

			h.MessageVerbose( "[{0}]", csharpfile );
			using (StreamWriter sw = new StreamWriter(csharpfile, false, UTF8Encoding.UTF8))
			{
				int tab = 0;

				// header
				h.WriteCodeGenHeader(sw);
				h.WriteUsing(sw);

				// namespace
				using (NamespaceBlock nsb = new NamespaceBlock(sw, tab++, csharpnamespace))
				{
					using (ClassBlock cb = new ClassBlock(sw, tab++, theclass, "acr.DatabaseBaseSingletonHelper"))
					{} 	

				} // end namespace

			} // eof

			// do class XXXXDatabase
			theclass	= h.GetCsharpClassName( null, null, p._databaseName ) + "Database" ;
			csharpfile	= p._directory + @"\" + p._databaseSubDirectory + @"\" + theclass + ".cs";

			h.MessageVerbose( "[{0}]", csharpfile );
			using (StreamWriter sw = new StreamWriter(csharpfile, false, UTF8Encoding.UTF8))
			{
				int tab = 0;

				// header
				h.WriteCodeGenHeader(sw);
				h.WriteUsing(sw);

				// namespace
				using (NamespaceBlock nsb = new NamespaceBlock(sw, tab++, csharpnamespace))
				{
					using (ClassBlock cb = new ClassBlock(sw, tab++, theclass, "acr.DatabaseBase<" + theclass + "SingletonHelper>"))
					{  
						using (DatabaseConstructorBlock conb = new DatabaseConstructorBlock(sw, tab, theclass, p._databaseName ))
						{}

					} // end class		

				} // end namespace

			} // eof

			h.MessageVerbose("### Generating database classes - done ###");

		} // end do database

	}
}
