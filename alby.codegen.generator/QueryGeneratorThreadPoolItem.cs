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
	public partial class QueryGeneratorParameters
	{
		 public Program		p ;
		 public XmlNode		query ;
		 public Exception	exception ;

	} // end class

	public partial class QueryGeneratorThreadPoolItem : MyThreadPoolItemBase
	{
		protected QueryGeneratorParameters _qgp ;

		public QueryGeneratorThreadPoolItem( QueryGeneratorParameters qgp ) 
		{
			_qgp = qgp ;
		}

		public override void Run() 
		{
			Helper h = new Helper() ;

			try
			{
				DoQuery( _qgp.p, _qgp.query ) ;	
			}	
			catch( Exception ex )
			{
				_qgp.exception = ex ;
				h.Message( "[DoQuery() EXCEPTION]\n{0}", ex ) ;
			}
		}

		protected void DoQuery( Program p, XmlNode query ) 
		{
			Helper		h	= new Helper() ;
			ColumnInfo	ci  = new ColumnInfo() ;

			string thedatabase = h.GetCsharpClassName( null, null, p._databaseName ) ;

			string csharpnamespace		= p._namespace + "." + p._querySubDirectory;
			string resourcenamespace	= p._resourceNamespace + "." + p._querySubDirectory;
						
			string csharpfile			= p._directory + @"\" + p._querySubDirectory +  @"\" + query.SelectSingleNode( "@CodeFile").Value ; 
			string theclass				= query.SelectSingleNode( "@Class" ).Value ; 

			string csharpfactoryfile	= csharpfile.Replace( ".cs", "Factory.cs" ) ;
 			string thefactoryclass		= theclass + "Factory";

			// query resource
			string queryfile			= query.SelectSingleNode( "@Select").Value ;
			string queryresource		= resourcenamespace + "." + query.SelectSingleNode("@Select").Value;

			// do class
			h.MessageVerbose( "[{0}]", csharpfile );
			using ( StreamWriter sw = new StreamWriter( csharpfile, false, UTF8Encoding.UTF8 ) )
			{
				int tab = 0;

				// header
				h.WriteCodeGenHeader(sw);
				h.WriteUsing(sw);

				// namespace
				using (NamespaceBlock nsb = new NamespaceBlock(sw, tab++, csharpnamespace)) 
				{
					using (ClassBlock cb = new ClassBlock(sw, tab++, theclass, "acr.RowBase")) 
					{
						// get columns
						var columns = ci.GetQueryColumns( queryfile ) ;
						
						// properties and constructor
						using (RowConstructorBlock conb = new RowConstructorBlock(sw, tab, theclass, columns, null, "")) 
						{}
						
					} // end class		
							
				} // end namespace
				
			} // end of file	
			
			// do class factory
			h.MessageVerbose( "[{0}]", csharpfactoryfile );
			using (StreamWriter sw = new StreamWriter(csharpfactoryfile, false, UTF8Encoding.UTF8))
			{
				int tab = 0;
				
				// header
				h.WriteCodeGenHeader(sw);
				h.WriteUsing(sw, p._namespace );

				// namespace
				using (NamespaceBlock nsb = new NamespaceBlock(sw, tab++, csharpnamespace)) 
				{
					using (ClassBlock cb = new ClassBlock(sw, tab++, thefactoryclass,
							"acr.FactoryBase< " + 
							theclass + ", " + 
							"ns." + p._databaseSubDirectory + "." + thedatabase + "DatabaseSingletonHelper, " +
							"ns." + p._databaseSubDirectory + "." + thedatabase + "Database >" 
						  ) )									
					{

						// constructor
						using (QueryFactoryConstructorBlock conb = new QueryFactoryConstructorBlock(sw, tab, queryresource, thefactoryclass )) 
						{}
						
						// method
						XmlNodeList xmlmethods = query.SelectNodes("Methods/Method");
						foreach( XmlNode xmlmethod in xmlmethods )
						{
							string themethod = xmlmethod.SelectSingleNode("@Name").InnerText;

							// where resource
							string whereresource = xmlmethod.SelectSingleNode("@Where").InnerText;

							// parameters
							List<string>				parameters			 = new List<string>() ;
							Dictionary<string,string>	parameterdictionary  = new Dictionary<string,string>() ;

							parameters = new List<string>() ;
							parameters.Add("connˡ" );

							XmlNodeList xmlparameters = xmlmethod.SelectNodes("Parameters/Parameter");
							foreach ( XmlNode xmlparameter in xmlparameters )
								parameters.Add( xmlparameter.SelectSingleNode("@Name").InnerText );

							parameters.Add("topNˡ" );
							parameters.Add("orderByˡ" );
							parameters.Add("tranˡ" );

							parameterdictionary.Add( "connˡ", "sds.SqlConnection" ) ;

							xmlparameters = xmlmethod.SelectNodes("Parameters/Parameter");
							foreach ( XmlNode xmlparameter in xmlparameters )
								parameterdictionary.Add(  xmlparameter.SelectSingleNode("@Name").InnerText, 
														  xmlparameter.SelectSingleNode("@Type").InnerText ) ;

							parameterdictionary.Add( "topNˡ",	  "int?");
							parameterdictionary.Add( "orderByˡ",  "scg.List<acr.CodeGenOrderBy>" ) ;
							parameterdictionary.Add( "tranˡ",	  "sds.SqlTransaction" ) ;
							
							// method
							h.MessageVerbose( "[{0}].[{1}].method [{2}]", csharpnamespace, theclass, themethod);

							using (QueryFactoryMethodBlock mb = new QueryFactoryMethodBlock(sw, tab, themethod, parameters, parameterdictionary, whereresource, theclass, resourcenamespace))
							{}						
						
						} // end method
					
					} // end class

				} // end namespace
			
			} // end of file
		}
	
	} // end class	

}

