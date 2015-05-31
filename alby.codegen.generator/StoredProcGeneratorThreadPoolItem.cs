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
	public partial class StoredProcGeneratorParameters
	{
		 public Program		p ;
		 public string		fqstoredprocedure ;
		 public Exception	exception ;

	} // end class

	//--------------------------------------------------------------------------------------------------------------------		

	public partial class StoredProcGeneratorThreadPoolItem : MyThreadPoolItemBase
	{
		protected StoredProcGeneratorParameters _qgp ;

		//--------------------------------------------------------------------------------------------------------------------		

		public StoredProcGeneratorThreadPoolItem( StoredProcGeneratorParameters qgp ) 
		{
			_qgp = qgp ;
		}

		//--------------------------------------------------------------------------------------------------------------------		

		public override void Run() 
		{
			Helper h = new Helper() ;

			try
			{
				DoStoredProc( _qgp.p, _qgp.fqstoredprocedure ) ;	
			}	
			catch( Exception ex )
			{
				_qgp.exception = ex ;
				h.Message( "[DoStoredProc() EXCEPTION]\n{0}", ex ) ;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------		

		protected void DoStoredProc( Program p, string fqstoredprocedure )  
		{
			Helper						 h		= new Helper() ;
			StoredProcedureParameterInfo sppi	= new StoredProcedureParameterInfo() ;
			StoredProcedureResultsetInfo sprsi	= new StoredProcedureResultsetInfo() ;

			var schemastoredprocedure   = h.SplitSchemaFromTable( fqstoredprocedure ) ;

			string thedatabase			= h.GetCsharpClassName( null, null, p._databaseName ) ;
			string csharpnamespace		= p._namespace + "." + p._storedProcsSubDirectory;

			string csharpstoredproc		= h.GetCsharpClassName( p._prefixObjectsWithSchema, schemastoredprocedure.Item1, schemastoredprocedure.Item2 );
			string csharpfile			= p._directory + @"\" + p._storedProcsSubDirectory + @"\" + csharpstoredproc + ".cs";

			string thefactoryclass		= "StoredProcedureFactory";
			string csharpfactoryfile	= p._directory + @"\" + p._storedProcsSubDirectory + @"\" + thefactoryclass + ".cs" ;

			// do each stored proc in a separate file, but same factory class
			h.MessageVerbose( "[{0}]", csharpfile );
			
			// check for dud parameters - cursor, table - ignore the sp if so 
			if ( sppi.HasDudParameterStoredProcedure( fqstoredprocedure ) )
			{
			    h.MessageVerbose( "[{0}] Ignoring stored procedure because it has dud parameters.", fqstoredprocedure );
			    return ;
			}

			// get sp parameters
			List<string> parameters = new List<string>() ;
			parameters.Add( "connˡ" );

			var spparameters = sppi.GetStoredProcedureParameterInfo( fqstoredprocedure ) ;
			if ( spparameters != null )
				foreach( var spparameter in spparameters )
					parameters.Add( spparameter.Name ) ;

			parameters.Add( "tranˡ" ) ;

			// the results sets
			var rsi = sprsi.GetResultsetInfo( fqstoredprocedure ) ;

			// write out the stored proc wrapper to its file
			using ( StreamWriter sw = new StreamWriter( csharpfile, false, UTF8Encoding.UTF8 ) )
			{
				int tab = 0;
				
				// header
				h.WriteCodeGenHeader(sw);
				h.WriteUsing( sw, p._namespace );

				// namespace
				using (NamespaceBlock nsb = new NamespaceBlock(sw, tab++, csharpnamespace))
				{
					using ( ClassBlock cb = new ClassBlock(
								sw, 
								tab++, 
								thefactoryclass,
								"acr.StoredProcedureFactoryBase< " + "ns." + p._databaseSubDirectory + "." + thedatabase + "DatabaseSingletonHelper, " + "ns." + p._databaseSubDirectory + "." + thedatabase + "Database >" ) )					
					{
						// execute block
						h.MessageVerbose("[{0}].[{1}]", csharpnamespace, csharpstoredproc );

						using ( StoredProcedureFactoryExecuteBlock mb = new StoredProcedureFactoryExecuteBlock( 
						                                                        p, 
						                                                        sw, 
						                                                        tab, 
						                                                        fqstoredprocedure, 
						                                                        csharpstoredproc, 
						                                                        parameters, 
																				sppi,
						                                                        rsi ) )
						{}

					} // end class

				} // end namespace
				
			} // eof		

			// write out the classes for stored proc result sets, if any
			int i = 0 ;
			foreach( var rs in rsi.Resultsets )
			{
				i++ ;
				string therecordsetclass   = csharpstoredproc + h.IdentifierSeparator + "rs" + i ;
				string csharprecordsetfile = p._directory + @"\" + p._storedProcsSubDirectory + @"\" + csharpstoredproc + ".rs" + i + ".cs";

				h.MessageVerbose( "[{0}]", csharprecordsetfile );
				using (StreamWriter sw = new StreamWriter( csharprecordsetfile, false, UTF8Encoding.UTF8))
				{
					int tab = 0;
				
					// header
					h.WriteCodeGenHeader(sw);
					h.WriteUsing(sw, p._namespace );
						
					// namespace
					using (NamespaceBlock nsb = new NamespaceBlock(sw, tab++, csharpnamespace))
					{
						using (ClassBlock cb = new ClassBlock( sw, tab++, therecordsetclass, "acr.RowBase" ) )
						{
							h.MessageVerbose("[{0}].[{1}] row", csharpnamespace, therecordsetclass );
							
							using ( StoredProcedureRowConstructorBlock mb = new StoredProcedureRowConstructorBlock( sw, tab, therecordsetclass, rs.Columns ) )
							{}

						} // end class

					} // end namespace

				} // eof
			}

		} // end do sp
	
	} // end class	

}

