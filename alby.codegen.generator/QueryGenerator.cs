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
	public partial class QueryGenerator
	{
		public void DoQueries( Program p )
		{
			Helper h = new Helper() ;
			List<QueryGeneratorParameters> threadParamList = new List<QueryGeneratorParameters>() ;

			h.MessageVerbose("### Generating code gen queries ###");

			if ( p._queries.Count >= 1 ) // anything to do ?
				using ( MyThreadPoolManager tpm = new MyThreadPoolManager( p._threads, p._queries.Count ) ) // max threads: _threads, queue length: no of tables
				{
					foreach( XmlNode query in p._queries )
					{
						QueryGeneratorParameters qgp = new QueryGeneratorParameters() ;
						threadParamList.Add( qgp ) ;

						qgp.p		= p ;
						qgp.query	= query ;

						tpm.Queue( new QueryGeneratorThreadPoolItem( qgp ) ) ;
					}
					tpm.WaitUntilAllStarted() ;
					tpm.WaitUntilAllFinished() ;
				}

			h.MessageVerbose("### Generating code gen queries - done ###");

			// handle any thread exceptions
			foreach( QueryGeneratorParameters qgp in threadParamList )
				if ( qgp.exception != null )
					throw new ApplicationException( "DoQueries() worker thread exception", qgp.exception ) ;
		}

	} // end class
}
 