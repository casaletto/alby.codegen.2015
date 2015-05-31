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

// unit test - main controller

namespace alby.codegen.generator
{
	public partial class UnitTestGenerator
	{
		// unit test class and base class
		protected string _theclass	= "CodeGenUnitTestClass" ;
		protected string _baseclass = "acr.CodeGenUnitTestBase"  ;


		// list of tables for which to include in unit test
		protected List<string> _unitTestTables			= new List<string>();
		protected List<string> _unitTestTablesReverse	= new List<string>(); // reverse ri order

		// dictionaries of tables vs their fields
		protected Dictionary<string, List< Tuple<string,string> > >		_columnsMap				= new Dictionary<string, List< Tuple<string,string> > >() ; 
		protected Dictionary<string, List<string> >						_computedColumnsMap		= new Dictionary<string, List<string>>();
		protected Dictionary<string, List<string> >						_identityColumnsMap		= new Dictionary<string, List<string>>();
		protected Dictionary<string, List<string> >						_timestampColumnsMap	= new Dictionary<string, List<string>>();
			
		public void DoUnitTest( Program p )
		{
			Helper h = new Helper() ;

			h.MessageVerbose( "### Generating codegen unit test ###" );
			DoUnitTest2( p ) ;
			h.MessageVerbose( "### Generating codegen unit test - done ###" );
		}

		protected void DoUnitTest2( Program p )
		{
			Helper		h  = new Helper() ;
			ColumnInfo	ci = new ColumnInfo() ;

			// see if we want the unit test - is there something in the config file?
			XmlNode node = p._codegen.SelectSingleNode("/CodeGen/UnitTest") ;
			if ( node == null )
			{
				h.MessageVerbose("No unit test block found - not generating unit test.");
				return ;
			}

			// get list of tables in ri order - onlt tables that can be saved [ie have a primary key] are eligible 						
			// get tables that have primary keys - and associated field information

			foreach( var table in p._rihelper.SortedTables ) 
			{
				string		 fqtablename = table.Item2 ; ;
				List<string> pkcolumns   = p._di.PrimaryKeyColumns.Get( fqtablename, "PK" ) ;

				if ( pkcolumns.Count > 0 )
				{
					// we want this table
					_unitTestTables.Add( fqtablename );
					_unitTestTablesReverse.Add( fqtablename );
					
					// we want all its columns
					_columnsMap.Add( fqtablename, ci.GetTableColumns( fqtablename ) ) ;

					// we want its computed columns
					_computedColumnsMap.Add( fqtablename, p._di.ComputedColumns.Get( fqtablename ) ) ;

					// we want its identity columns
					_identityColumnsMap.Add( fqtablename, p._di.IdentityColumns.Get( fqtablename ) ) ; 

					// we want its timestamp columns
					_timestampColumnsMap.Add( fqtablename, p._di.TimestampColumns.Get( fqtablename ) ) ;
				}
			}

			_unitTestTablesReverse.Reverse() ; // same list but in reverse order, for deleting
			 
			// dump list of unit test tables
			h.MessageVerbose("### The following tables are supported for auto unit test [they have a primary key]: ###");
			foreach (string atable in _unitTestTables)
				h.MessageVerbose( "\t[{0}]", atable );
			h.MessageVerbose("[{0}] unit test tables", _unitTestTables.Count );

			//
			// plumbing 
			//

			// generate CodeGenUnitTestClass.cs
			EntryPoint( p, _theclass, _baseclass);

			// generate CodeGenUnitTestClass.State.cs
			State( p, _theclass, _baseclass);

			// generate CodeGenUnitTestClass.CodegenRunTimeSettings.cs
			CodegenRunTimeSettings( p, _theclass, _baseclass);

			// generate CodeGenUnitTestClass.AssertObjectsNew.cs
			AssertObjectsNew( p, _theclass, _baseclass);

			//
			// object population and insert and update 
			//

			// generate CodeGenUnitTestClass.PopulateObjectsForInsert.cs
			PopulateObjectsForInsert( p, _theclass, _baseclass);

			// generate CodeGenUnitTestClass.PopulateObjectsForUpdate.cs
			PopulateObjectsForUpdate( p, _theclass, _baseclass);

			// create a Populate.#.cs file for each table
			// containing the function Populate_#( bool insert, # obj,  )
			Populate( p, _theclass, _baseclass);

			// create a PopulateOverride.#.cs file for each table
			// containing the function PopulateOverride_#( bool insert, # obj  )
			PopulateOverride( p, _theclass, _baseclass);

			//
			// object deletion 
			//

			// generate CodeGenUnitTestClass.DeleteObjects.cs
			DeleteObjects( p, _theclass, _baseclass);

			//
			// field level assertions 
			//

			// create a AssertAfterSave.#.cs file for each table
			// containing the function AssertAfterSave_#( bool insert, # lhs, # rhs )
			Assert( p, _theclass, _baseclass);

			// generate CodeGenUnitTestClass.AssertObjectsAfterInsert.cs
			AssertObjectsAfterInsert( p, _theclass, _baseclass);

			// generate CodeGenUnitTestClass.AssertObjectsAfterUpdate.cs
			AssertObjectsAfterUpdate( p, _theclass, _baseclass);					
		}	
		
	} // end class
		
} // end ns

