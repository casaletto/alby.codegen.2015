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
	public partial class ViewGenerator
	{
		public void DoViews( Program p )
		{
			Helper h = new Helper() ;

			List<ViewGeneratorParameters> threadParamList = new List<ViewGeneratorParameters>() ;

			h.MessageVerbose("### Generating code gen views ###");

			List<string> views = p._di.Views.Get() ;
			if ( views.Count >= 1 ) // anything to do ?
				using ( MyThreadPoolManager tpm = new MyThreadPoolManager( p._threads, views.Count ) ) // max threads: _threads, queue length: no of tables
				{
					foreach( string fqview in views )
					{
						ViewGeneratorParameters vgp = new ViewGeneratorParameters() ;
						threadParamList.Add( vgp ) ;

						vgp.p		= p ;
						vgp.fqview	= fqview ; 

						tpm.Queue( new ViewGeneratorThreadPoolItem( vgp ) ) ;
					}
					tpm.WaitUntilAllStarted() ;
					tpm.WaitUntilAllFinished() ;
				}

			h.MessageVerbose("### Generating code gen views - done ###");

			// handle any thread exceptions
			foreach( ViewGeneratorParameters vgp in threadParamList )
				if ( vgp.exception != null )
					throw new ApplicationException( "DoViews() worker thread exception", vgp.exception ) ;
		}
							
	} // end class
	
} // end ns	

