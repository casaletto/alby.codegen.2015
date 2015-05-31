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

// unit test - asserts per table

namespace alby.codegen.generator
{
	public partial class UnitTestGenerator
	{
		// create an Assert.#.cs file for each table 
		// containing the function Assert.#( bool insert, # lhs, # rhs )

		protected void Assert( Program p, string theclass, string baseclass)
		{
			Helper h = new Helper() ;
			List<UnitTestGeneratorAssetPerTableParameters> threadParamList = new List<UnitTestGeneratorAssetPerTableParameters>() ;

			if ( _unitTestTables.Count >= 1 )
				using ( MyThreadPoolManager tpm = new MyThreadPoolManager( p._threads, _unitTestTables.Count ) ) // max threads: _threads, queue length: no of tables
				{
					int i = 0 ;
					foreach ( string fqtable in _unitTestTables )
					{
						i++ ;
						UnitTestGeneratorAssetPerTableParameters utgaptp = new UnitTestGeneratorAssetPerTableParameters() ;
						threadParamList.Add( utgaptp ) ;

						utgaptp.p					= p ;
						utgaptp.theclass			= theclass ;
						utgaptp.baseclass			= baseclass ;
						utgaptp.fqtable				= fqtable ;
						utgaptp.i					= i ;
						utgaptp.tablecount			= _unitTestTables.Count;
						utgaptp.identitycolumns		= _identityColumnsMap [ fqtable ] ;
						utgaptp.computedcolumns		= _computedColumnsMap [ fqtable ] ;
						utgaptp.timestampcolumns	= _timestampColumnsMap[ fqtable ] ;
						utgaptp.columns				= _columnsMap		  [ fqtable ] ;

						tpm.Queue( new UnitTestGeneratorAssetPerTableThreadPoolItem( utgaptp ) ) ;
					}
					tpm.WaitUntilAllStarted() ;
					tpm.WaitUntilAllFinished() ;
				}

			// handle any thread exceptions
			foreach( UnitTestGeneratorAssetPerTableParameters utgaptp in threadParamList )
				if ( utgaptp.exception != null )
					throw new ApplicationException( "Unit test Assert() worker thread exception", utgaptp.exception ) ;

		} // end Assert

	} // end class

}