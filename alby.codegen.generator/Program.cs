using System;
using System.Collections.Generic;
using System.Text;
using System.Xml ;
using System.Xml.XPath ;
using System.Data ;
using System.Data.Sql ;
using System.Data.SqlClient ;
using System.Data.SqlTypes ;

namespace alby.codegen.generator
{
	public partial class Program
	{
		static int Main(string[] args)
		{
			int rc = 0 ;
			Console.OutputEncoding = Encoding.UTF8 ;

			Helper h = new Helper() ;
			DateTime start = DateTime.Now;

			h.Message("[CODEGEN START]");
			Program program = new Program(args);

			try
			{
				program.ValidateParameters();
				program.Init();

				if ( program._performAll || program._performReferentialIntegrity )
					 program.DoReferentialIntegrity();

				DatabaseGenerator dbg = new DatabaseGenerator() ;
				dbg.DoDatabase( program );

				if ( program._performAll || program._performTables )
				{
				    TableGenerator tg = new TableGenerator() ;
				    tg.DoTables( program );
				}

				if ( program._performAll || program._performViews )
				{
				    ViewGenerator vg = new ViewGenerator() ;
				    vg.DoViews( program );
				}

				if ( program._performAll || program._performQueries )
				{
				    QueryGenerator qg = new QueryGenerator() ;
				    qg.DoQueries( program ) ;
				}

				if ( program._performAll || program._performStoredProcs )
				     program.DoStoredProcs( program );

				if ( program._performAll || program._performUnitTests )
				{
				    UnitTestGenerator utg = new UnitTestGenerator() ;
				    utg.DoUnitTest( program ) ;
				}
			}
			catch( Exception ex ) 
			{
				rc = 1 ;
				program.DumpException(ex);
				program.DumpUsage() ;
			}
			finally
			{}
			
			DateTime finish = DateTime.Now;
			TimeSpan ts = finish - start ;
			h.Message( "[CODEGEN FINISH] [{0}] [{1} secs]", rc, Math.Round( ts.TotalSeconds, 2 ) ) ;
			return rc;		
		}
		
		protected void DumpException( Exception ex )
		{
			Helper h = new Helper() ;

			h.Message("[CODEGEN EXCEPTION]");
			h.Message(ex.Message);
			h.Message(ex.StackTrace);
		}

		protected void DumpUsage()
		{
			Helper h = new Helper() ;

			h.Message("[USAGE]");
			h.Message("alby.codegen.generator.exe [codegen.xml] [--ri --unit-tests --queries --tables --views --storedProcs --verbose]");
		}

		#region state
		
		// common stuff
		public string[]						_args;
		public string						_codegenFile ;
		public XmlDocument					_codegen = new XmlDocument() ;
		public string						_connectionString ;
		public SqlConnection				_connection ;
		public string						_directory ;
		public string						_namespace;
		public string						_resourceNamespace;
		public string						_concurrencyColumn;
        public string						_databaseName ;
		public List<string>					_prefixObjectsWithSchema ;

		// unit test config
		public string						_unitTestNamespace ;
		public string						_unitTestDirectory ;
		public string						_unitTestTableNamespace ;
		public string						_unitTestTableNamespacePrefix ;
		
		// query gen stuff
		public string						_querySubDirectory ;		
		public XmlNodeList					_queries ;		
		
		// view gen stuff
		public string						_viewSubDirectory;		
		
		// table gen stuff
		public string						_tableSubDirectory;		

		// database gen stuff
		public string						_databaseSubDirectory;		

		// sp gen stuff
		public string						_storedProcsSubDirectory;		
		
		// referentail integrity stuff
		public ReferentialIntegrityHelper	_rihelper = new ReferentialIntegrityHelper(); 

		//database information
		public DatabaseInfo					_di = new DatabaseInfo() ;
		
		// parameters
		public bool							_verbose						= false ;
		public int							_threads						= 1 ;
		public bool							_performAll						= true  ;
		public bool							_performReferentialIntegrity	= false ;
		public bool							_performUnitTests				= false ;
		public bool							_performQueries					= false ;
		public bool							_performTables					= false ;
		public bool							_performViews					= false ;
		public bool							_performStoredProcs				= false ;

		#endregion

		protected Program(string[] args)
		{
			_args = args;
		}		

		protected void Init() 
		{
			Helper h = new Helper() ;
			Settings settings = new Settings() ;

			_codegenFile = _args[0] ;
			h.MessageVerbose( "Config file [{0}]", _codegenFile ) ;
			
			// load the xml configuration file
			_codegen.Load( _codegenFile ) ;

			// basic codegen info
			_namespace = _codegen.SelectSingleNode("/CodeGen/Namespace").InnerText;
			h.MessageVerbose("Namespace [{0}]", _namespace);

			_resourceNamespace = _codegen.SelectSingleNode("/CodeGen/ResourceNamespace").InnerText;
			h.MessageVerbose("ResourceNamespace [{0}]", _resourceNamespace);

			_directory = _codegen.SelectSingleNode("/CodeGen/Directory").InnerText;
			h.MessageVerbose("Directory [{0}]", _directory);

			_connectionString = _codegen.SelectSingleNode("/CodeGen/ConnectionString").InnerText ;
			h.MessageVerbose("ConnectionString [{0}]", _connectionString );

			_concurrencyColumn = _codegen.SelectSingleNode("/CodeGen/ConcurrencyColumn").InnerText;
			h.MessageVerbose("ConcurrencyColumn [{0}]", _concurrencyColumn);

			string prefixes = _codegen.SelectSingleNode("/CodeGen/PrefixObjectsWithSchema").InnerText;
			h.MessageVerbose("PrefixObjectsWithSchema [{0}]", prefixes);
			_prefixObjectsWithSchema = new List<string>( prefixes.ToUpper().Split( ',' ) ) ;

			// items related to auto codegened unit test
			_unitTestNamespace = _codegen.SelectSingleNode("/CodeGen/UnitTest/Namespace").InnerText;
			h.MessageVerbose("Unit test namespace [{0}]", _unitTestNamespace);

			_unitTestDirectory = _codegen.SelectSingleNode("/CodeGen/UnitTest/Directory").InnerText;
			h.MessageVerbose("Unit test directory [{0}]", _unitTestDirectory);

			_unitTestTableNamespace = _codegen.SelectSingleNode("/CodeGen/UnitTest/TableNamespace").InnerText;
			h.MessageVerbose("Unit test table namespace [{0}]", _unitTestTableNamespace);

			_unitTestTableNamespacePrefix = _codegen.SelectSingleNode("/CodeGen/UnitTest/TableNamespacePrefix" ).InnerText;
			h.MessageVerbose("Unit test table namespace prefix [{0}]", _unitTestTableNamespacePrefix );		

			_threads = settings.Threads ;
			h.MessageVerbose("Threads [{0}]", _threads );

			_tableSubDirectory = _codegen.SelectSingleNode( "/CodeGen/Tables/@SubDirectory"  ).Value;
			h.MessageVerbose("Table subdirectory [{0}]", _tableSubDirectory );

			_viewSubDirectory = _codegen.SelectSingleNode( "/CodeGen/Views/@SubDirectory"   ).Value;
			h.MessageVerbose("View subdirectory [{0}]", _viewSubDirectory );
			
			_querySubDirectory = _codegen.SelectSingleNode( "/CodeGen/Queries/@SubDirectory" ).Value;
			h.MessageVerbose("Query subdirectory [{0}]", _querySubDirectory );
			
			_queries = _codegen.SelectNodes( "/CodeGen/Queries/Query" ) ; 
			
			// connect to the database so we can do some code jen !
			_connection = new SqlConnection( _connectionString );
			_connection.Open();		

			// get all the database metadata that we may need !
			_di.CreateDatabaseInfo( _connection ) ;

            _databaseName = _di.GetDatabaseName() ;
			h.MessageVerbose("DatabaseName [{0}]", _databaseName );

			DataSetHelper dsh = new DataSetHelper( _di.GetDatabaseInfo() ) ;
			h.MessageVerbose("Database information:\n\n{0}", dsh.ToString() );		

			ColumnInfo ci = new ColumnInfo() ;
			ci.CreateColumnInfo( _connection, _di.Tables.Get(), _di.Views.Get(), _directory, _querySubDirectory, _queries ) ;
			h.MessageVerbose("Table, view and query column information:\n\n{0}", ci.ToString() ) ;		

			StoredProcedureParameterInfo sppi = new StoredProcedureParameterInfo() ;
			sppi.CreateStoredProcedureInfo( _di ) ; 

			StoredProcedureResultsetInfo sprsi = new StoredProcedureResultsetInfo() ;
			sprsi.CreateStoredProcedureInfo( _connection, _di, _codegen, sppi ) ; 
			h.MessageVerbose("Stored procedure resultset information:\n\n{0}", sprsi.ToString() ) ;		
		}

		protected void ValidateParameters()
		{
			int i = 0;
			foreach (string arg in _args)
			{
				if ( i == 0 ) 
				{
				    ; // skip xml file name
				}
				else
				if (arg == "--verbose")
				{
					_verbose = true ;
					Helper.__verbose = _verbose ;
				}
				else
				if (arg == "--ri")
				{
				    _performReferentialIntegrity = true ;
				    _performAll					 = false ;
				}
				else
				if (arg == "--unit-tests")
				{
				    _performUnitTests	= true ;
				    _performAll			= false;
				}
				else 
				if (arg == "--queries" )
				{
				    _performQueries = true ;
				    _performAll		= false;
				}
				else
				if (arg == "--tables")
				{
				    _performTables	= true ;
				    _performAll		= false;
				}
				else
				if (arg == "--views" )
				{
				    _performViews	= true ;
				    _performAll		= false;
				}
				else
				if (arg == "--storedProcs" )
				{
				    _performStoredProcs	= true ;
				    _performAll			= false;
				}
				else	
				    throw new ApplicationException(string.Format("[{0}] unknown parameter.", arg));

				i++ ;
			}

			// unit tests need ri
			if ( _performUnitTests )
				_performReferentialIntegrity = true;
		}
		
		protected void DoReferentialIntegrity() 
		{
			Helper h = new Helper() ;

			h.MessageVerbose("### Determining referential integrity dependancies ###");
		
			try
			{
				_rihelper.DetermineReferentialIntegrityOrder( _di ) ;
			}			
			catch( Exception )
			{
				throw ;
			}
			finally
			{
				_rihelper.DumpInfo();			
			}

			h.MessageVerbose("### Determining referential integrity dependancies - done ###");
		}

	}
}

