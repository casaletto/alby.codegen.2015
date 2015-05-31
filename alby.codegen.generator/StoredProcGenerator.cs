using System;
using System.Collections.Generic;
using System.Text;
using System.IO ;
using System.Xml;
using System.Xml.XPath;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using alby.core.threadpool ;

namespace alby.codegen.generator
{
	public partial class Program
	{
		protected void DoStoredProcs( Program p )
		{
			Helper h = new Helper() ;

			List<StoredProcGeneratorParameters> threadParamList = new List<StoredProcGeneratorParameters>() ;

			h.MessageVerbose("### Generating code gen stored procs ###");
			_storedProcsSubDirectory = _codegen.SelectSingleNode("/CodeGen/StoredProcs/@SubDirectory").Value;

			List<string> storedprocedures = p._di.StoredProcedures.Get() ;

			if ( storedprocedures.Count >= 1 ) // anything to do ?
			{
				// create a tt class and ttlist class for each table type

				foreach ( var fqtabletype in p._di.TableTypes.Get() )
				{
					Tuple<string,string> schematabletype = h.SplitSchemaFromTable( fqtabletype ) ;

					string csharpnamespace			= p._namespace + "." + p._storedProcsSubDirectory ;
					string csharptabletype			= h.GetCsharpClassName( p._prefixObjectsWithSchema, schematabletype.Item1, schematabletype.Item2 ) ;
					string thetabletypeclass		= csharptabletype + h.IdentifierSeparator + "tt" ;
					string csharptabletypefile		= p._directory + @"\" + p._storedProcsSubDirectory + @"\" + csharptabletype + ".tt.cs" ;

					string thetabletypelistclass	= csharptabletype + h.IdentifierSeparator + "ttlist" ;
					string csharptabletypelistfile	= p._directory + @"\" + p._storedProcsSubDirectory + @"\" + csharptabletype + ".ttlist.cs" ;
					string ttlistbaseclass			= "scg.List< " + thetabletypeclass + " >, scg.IEnumerable< mss.SqlDataRecord >" ;

					List<Tuple<string,string,Int16,Byte,Byte>> columns = p._di.TableTypeColumns.Get( fqtabletype ) ;

					// tt file					
					h.MessageVerbose( "[{0}]", csharptabletypefile );
					using (StreamWriter sw = new StreamWriter( csharptabletypefile, false, UTF8Encoding.UTF8))
					{
						int tab = 0;
				
						// header
						h.WriteCodeGenHeader(sw);
						h.WriteUsing( sw, p._namespace );

						// namespace
						using (NamespaceBlock nsb = new NamespaceBlock(sw, tab++, csharpnamespace))
							using (ClassBlock cb = new ClassBlock( sw, tab++, thetabletypeclass, "acr.RowBase" ) )
								using ( StoredProcedureRowConstructorBlock mb = new StoredProcedureRowConstructorBlock( sw, tab, thetabletypeclass, columns, true ) )
								{}
					}

					// tt list file
					h.MessageVerbose( "[{0}]", csharptabletypelistfile );
					using (StreamWriter sw = new StreamWriter( csharptabletypelistfile, false, UTF8Encoding.UTF8))
					{
						int tab = 0;
				
						// header
						h.WriteCodeGenHeader(sw);
						h.WriteUsing( sw, p._namespace );

						// namespace
						using (NamespaceBlock nsb = new NamespaceBlock(sw, tab++, csharpnamespace))
							using (ClassBlock cb = new ClassBlock( sw, tab++, thetabletypelistclass, ttlistbaseclass ) )
								using ( StoredProcedureTableTypeEnumeratorBlock mb = new StoredProcedureTableTypeEnumeratorBlock( sw, tab, thetabletypeclass, columns ) )
								{}
					}

				} 

				// write each stored procedure in a file

				using ( MyThreadPoolManager tpm = new MyThreadPoolManager( p._threads, storedprocedures.Count ) ) // max threads: _threads, queue length: no of sp's
				{
					foreach( string storedprocedure in storedprocedures )
					{
						StoredProcGeneratorParameters tgp = new StoredProcGeneratorParameters() ;
						threadParamList.Add( tgp ) ;

						tgp.p				  = p ;
						tgp.fqstoredprocedure = storedprocedure;

						tpm.Queue( new StoredProcGeneratorThreadPoolItem( tgp ) ) ;
					}
					tpm.WaitUntilAllStarted() ;
					tpm.WaitUntilAllFinished() ;
				}
			
			}
			h.MessageVerbose("### Generating code gen stored procs - done ###");

			// handle any thread exceptions
			foreach( StoredProcGeneratorParameters tgp in threadParamList )
				if ( tgp.exception != null )
					throw new ApplicationException( "DoStoredProcs() worker thread exception", tgp.exception ) ;
		}

	} // end class
}
 