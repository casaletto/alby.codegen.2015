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
	public partial class TableGenerator
	{
		public void DoTables( Program p )
		{
			Helper h = new Helper() ;

			List<TableGeneratorParameters> list = new List<TableGeneratorParameters>() ;

			h.MessageVerbose( "### Generating code gen tables ###" );

			List<string> tables = p._di.Tables.Get() ;

			if ( tables.Count >= 1 ) // anything to do ?
				using ( MyThreadPoolManager tpm = new MyThreadPoolManager( p._threads, tables.Count ) ) // max threads: _threads, queue length: no of tables
				{
					foreach( string table in tables )
					{
						TableGeneratorParameters tgp = new TableGeneratorParameters() ;
						list.Add( tgp ) ;

						tgp.p		= p ;
						tgp.fqtable	= table ;

						tpm.Queue( new TableGeneratorThreadPoolItem( tgp ) ) ;
					}
					tpm.WaitUntilAllStarted() ;
					tpm.WaitUntilAllFinished() ;
				}

			h.MessageVerbose( "### Generating code gen tables - done ###" );

			// handle any thread exceptions
			foreach( TableGeneratorParameters tgp in list )
				if ( tgp.exception != null )
					throw new ApplicationException( "DoTables() worker thread exception", tgp.exception ) ;
				
		} // end do tables

	} // end class
		
} // end ns

