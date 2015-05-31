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

// unit test - populate objects per table

namespace alby.codegen.generator
{
	public partial class UnitTestGenerator
	{
		// create a Populate.#.cs file for each table 
		// containing the function Populate_#( # obj, bool insert )
		
		protected void Populate( Program p, string theclass, string baseclass )
		{
			Helper h = new Helper() ;
			List<UnitTestGeneratorPopulatePerTableParameters> threadParamList = new List<UnitTestGeneratorPopulatePerTableParameters>() ;

			if ( _unitTestTables.Count >= 1 )
				using ( MyThreadPoolManager tpm = new MyThreadPoolManager( p._threads, _unitTestTables.Count ) ) // max threads: _threads, queue length: no of tables
				{
					int i = 0 ;
					foreach ( string fqtable in _unitTestTables )
					{
						i++ ;
						UnitTestGeneratorPopulatePerTableParameters utgaptp = new UnitTestGeneratorPopulatePerTableParameters() ;
						threadParamList.Add( utgaptp ) ;

						utgaptp.p					= p ;
						utgaptp.theclass			= theclass ;
						utgaptp.baseclass			= baseclass ; 
						utgaptp.fqtable				= fqtable ;
						utgaptp.identitycolumns		= _identityColumnsMap [ fqtable ] ;
						utgaptp.computedcolumns		= _computedColumnsMap [ fqtable ] ;
						utgaptp.timestampcolumns	= _timestampColumnsMap[ fqtable ] ;
						utgaptp.columns				= _columnsMap         [ fqtable ] ; 

						tpm.Queue( new UnitTestGeneratorPopulatePerTableThreadPoolItem( utgaptp ) ) ;
					}
					tpm.WaitUntilAllStarted() ;
					tpm.WaitUntilAllFinished() ;
				}

			// handle any thread exceptions
			foreach( UnitTestGeneratorPopulatePerTableParameters utgaptp in threadParamList )
				if ( utgaptp.exception != null )
					throw new ApplicationException( "Unit test Populate() worker thread exception", utgaptp.exception ) ;
		}

	} // end class

}