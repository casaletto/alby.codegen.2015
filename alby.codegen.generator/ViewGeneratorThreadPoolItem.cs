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
	public partial class ViewGeneratorParameters
	{
		 public Program		p ;
		 public string		fqview ;
		 public Exception	exception ;

	} // end class

	public partial class ViewGeneratorThreadPoolItem : MyThreadPoolItemBase
	{
		protected ViewGeneratorParameters _vgp ;

		public ViewGeneratorThreadPoolItem( ViewGeneratorParameters vgp ) 
		{
			_vgp = vgp ;
		}

		public override void Run() 
		{
			Helper h = new Helper() ;

			try
			{
				DoView( _vgp.p, _vgp.fqview ) ;	
			}	
			catch( Exception ex )
			{
				_vgp.exception = ex ;
				h.Message( "[DoTable() EXCEPTION]\n{0}", ex ) ;
			}
		}

		protected void DoView( Program p, string fqview ) 
		{
			Helper	   h  = new Helper() ;
			ColumnInfo ci = new ColumnInfo() ;

			Tuple<string,string> schemaview = h.SplitSchemaFromTable( fqview ) ;

			string thedatabase = h.GetCsharpClassName( null, null, p._databaseName ) ;

			string csharpnamespace		= p._namespace + "." + p._viewSubDirectory;
			string resourcenamespace	= p._resourceNamespace + "." + p._viewSubDirectory;

			string theclass				= h.GetCsharpClassName( p._prefixObjectsWithSchema, schemaview.Item1, schemaview.Item2);
			string csharpfile			= p._directory + @"\" + p._viewSubDirectory + @"\" + theclass + ".cs" ;

			string csharpfactoryfile	= csharpfile.Replace(".cs", "Factory.cs");
			string thefactoryclass		= theclass + "Factory";

			// config for this view, if any
			string  xpath = "/CodeGen/Views/View[@Class='" + theclass + "']" ;
			XmlNode view  = p._codegen.SelectSingleNode(xpath); 

			// select sql
			string selectsql = "select * from {0} t ";

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
						List< Tuple<string,string> > columns = ci.GetViewColumns( fqview ) ;

						// properties and constructor
						using (RowConstructorBlock conb = new RowConstructorBlock(sw, tab, theclass, columns, null, ""))
						{}

					} // end class		

				} // end namespace
				
			} // eof

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
						using (ViewFactoryConstructorBlock conb = new ViewFactoryConstructorBlock( sw, tab, fqview, selectsql, thefactoryclass))
						{}
					
						// default load all method
						List<string> parameters = new List<string>();
						Dictionary<string, string> parameterdictionary  = new Dictionary<string, string>();

						parameters = new List<string>() ;
						parameters.Add("connˡ" );
						parameters.Add("topNˡ" );
						parameters.Add("orderByˡ" );
						parameters.Add("tranˡ" );

						parameterdictionary.Add("connˡ", "sds.SqlConnection");
						parameterdictionary.Add("topNˡ", "int?");
						parameterdictionary.Add("orderByˡ", "scg.List<acr.CodeGenOrderBy>");
						parameterdictionary.Add("tranˡ", "sds.SqlTransaction");

						// method
						h.MessageVerbose("[{0}].[{1}].[{2}]", csharpnamespace, theclass, "Loadˡ");
						using (ViewFactoryMethodBlock mb = new ViewFactoryMethodBlock(sw, tab, "Loadˡ", parameters, parameterdictionary, "", theclass, resourcenamespace))
						{}

						// load by where
						parameters = new List<string>() ;
						parameters.Add("connˡ" );
						parameters.Add("whereˡ" );
						parameters.Add("parametersˡ" );
						parameters.Add("topNˡ" );
						parameters.Add("orderByˡ" );
						parameters.Add("tranˡ" );

						parameterdictionary = new Dictionary<string, string>();
						parameterdictionary.Add("connˡ", "sds.SqlConnection");
						parameterdictionary.Add("whereˡ", "string");
						parameterdictionary.Add("parametersˡ", "scg.List<sds.SqlParameter>");
						parameterdictionary.Add("topNˡ", "int?");
						parameterdictionary.Add("orderByˡ", "scg.List<acr.CodeGenOrderBy>");
						parameterdictionary.Add("tranˡ", "sds.SqlTransaction");

						h.MessageVerbose("[{0}].[{1}].[{2}]", csharpnamespace, theclass, "LoadByWhereˡ");
						using (ViewFactoryMethodBlock mb = new ViewFactoryMethodBlock(sw, tab, "LoadByWhereˡ", parameters, parameterdictionary, "", theclass, resourcenamespace))
						{}

						// other methods
						if ( view != null )
						{
							XmlNodeList xmlmethods = view.SelectNodes("Methods/Method");
							foreach ( XmlNode xmlmethod in xmlmethods )
							{
								string themethod = xmlmethod.SelectSingleNode("@Name").InnerText;

								// where resource
								string whereresource = xmlmethod.SelectSingleNode("@Where").InnerText;

								// parameters
								parameters = new List<string>() ;
								parameters.Add("connˡ" );

								XmlNodeList xmlparameters = xmlmethod.SelectNodes("Parameters/Parameter");
								foreach ( XmlNode xmlparameter in xmlparameters )
									parameters.Add( xmlparameter.SelectSingleNode("@Name").InnerText );

								parameters.Add("topNˡ" );
								parameters.Add("orderByˡ" );
								parameters.Add("tranˡ" );

								parameterdictionary = new Dictionary<string, string>();
								parameterdictionary.Add("connˡ", "sds.SqlConnection");

								xmlparameters = xmlmethod.SelectNodes("Parameters/Parameter");
								foreach ( XmlNode xmlparameter in xmlparameters )
									parameterdictionary.Add( xmlparameter.SelectSingleNode("@Name").InnerText, 
															 xmlparameter.SelectSingleNode("@Type").InnerText );

								parameterdictionary.Add("topNˡ", "int?");
								parameterdictionary.Add("orderByˡ", "scg.List<acr.CodeGenOrderBy>");
								parameterdictionary.Add("tranˡ", "sds.SqlTransaction");

								// method
								h.MessageVerbose( "[{0}].[{1}].method [{2}]", csharpnamespace, theclass, themethod );

								using (ViewFactoryMethodBlock mb = new ViewFactoryMethodBlock(sw, tab, themethod, parameters, parameterdictionary, whereresource, theclass, resourcenamespace))
								{}
							}
						}
						
					} // end class

				} // end namespace
				
			} // eof		
		
		} // end do view
		
	} // end class	

}