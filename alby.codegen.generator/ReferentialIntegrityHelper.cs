using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection;
using System.IO;

namespace alby.codegen.generator
{  
	public class ReferentialIntegrityHelper 
	{

		public ReferentialIntegrityHelper()
		{
		}

		//--------------------------------------------------------------------------------------------------------------------		

		public List<string>	AllTables // a list of all the tables
		{
			get
			{
				return _allTables ;
			}
		}

		public List<string>	NonRiTables // all the tables that are ri independent
		{
			get
			{
				return _nonRiTables;
			}
		}

		public Dictionary< string, List<string> > RiTables // all the tables that are ri dependant, with their depebdant tables
		{
			get
			{
				return _riTables;
			}
		}

		public List<string>	UnsortedTables // all the left over tables that have not been sorted yet 
		{
			get
			{
				return _notdoneList ;
			}
		}

		public List< Tuple<int,string> > SortedTables // all the tables, sorted in ri order 
		{
			get
			{
				List< Tuple<int,string> > list = new List<Tuple<int,string>>() ;

				List<int> sortedtables = new List<int>( _sortedTables.Keys ) ;
				sortedtables.Sort() ;

				foreach( int table in sortedtables ) 
					list.Add( Tuple.Create<int,string>( table, _sortedTables[ table ] ) ) ;

				return list ;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------		

		DatabaseInfo									_di				= null ;
		protected List< string >						_allTables		= new List< string >();
		protected List< string >						_nonRiTables	= new List< string >();
		protected List< string >						_notdoneList	= new List< string >();
		protected Dictionary< string, List<string> >	_riTables		= new Dictionary< string, List<string> >(); 
		protected Dictionary< int, string >				_sortedTables	= new Dictionary< int, string >();

		//--------------------------------------------------------------------------------------------------------------------		

		// main entry point into this class
		
		public void DetermineReferentialIntegrityOrder( DatabaseInfo di ) 
		{
			_di = di ;

			_allTables = di.Tables.Get() ;
			_allTables.Sort() ;

			_riTables = this.GetRiTables();

			_nonRiTables = this.GetListOfNonRiTables( _allTables, _riTables );
			_nonRiTables.Sort() ;

			_sortedTables.Clear() ;
			_notdoneList.Clear() ;

			// do sanity check sum counts
			if ( _allTables.Count != _riTables.Count + _nonRiTables.Count )
			{
				string str = string.Format("RI: sum of tables dont reconcile: total [{0}], ri [{1}], non-ri [{2}]", _allTables.Count, _riTables.Count, _nonRiTables.Count);
				throw new ApplicationException( str ) ;
			}

			// fail if no  tables at all,
			if (_allTables.Count == 0)
			{
				string str = string.Format("RI: no tables in database - what the ?: total [{0}], ri [{1}], non-ri [{2}]", _allTables.Count, _riTables.Count, _nonRiTables.Count);
				throw new ApplicationException(str);
			}

			// fail if no non ri tables
			if ( _nonRiTables.Count == 0 )
			{
				string str = string.Format("RI: all tables have dependancies - no good: total [{0}], ri [{1}], non-ri [{2}]", _allTables.Count, _riTables.Count, _nonRiTables.Count);
				throw new ApplicationException(str);
			}

			// fill up sorted list with the non ri tables
			foreach ( string table in _nonRiTables )
				_sortedTables.Add( _sortedTables.Count + 1, table );
			
			// fill up the notdone list with the ri tables
			foreach ( string table in _riTables.Keys )
				_notdoneList.Add( table );
			_notdoneList.Sort() ;

			// the tricky bit here
			this.DetermineReferentialIntegrityOrder2() ; 
		}

		//--------------------------------------------------------------------------------------------------------------------		

		protected Dictionary< string, List<string> > GetRiTables()
		{
			Helper			h		 = new Helper() ;
			DataTable		fk		 = _di.GetDatabaseInfo().Tables["fk"] ;
			List<string>	fktables = new List<string>() ;

			foreach( DataRow row in fk.Rows )
			{
				string fktable = row[ "FkTable" ].ToString() ;

				if ( ! fktables.Contains( fktable ) )
					fktables.Add( fktable ) ; 
			}

			// pick up all dependant tables here
			Dictionary< string, List<string> > dic = new Dictionary< string, List<string> >() ;

			foreach( string fktable in fktables )
			{
				List<string> dependants = _di.ForeignKeyTableToPrimaryKeyTables.Get( fktable ) ;
				if ( dependants.Count >= 1 )
					 dic.Add( fktable, dependants ) ;
			}

			return dic ;
		}

		//--------------------------------------------------------------------------------------------------------------------		

		// this method is the money shot
		
		protected void DetermineReferentialIntegrityOrder2() 
		{
			Helper h = new Helper() ;

			int maxLoop = 10 * _allTables.Count;
			int loop	= 0;

			while ( true )
			{
				if ( _notdoneList.Count == 0 ) return ; // finished

				loop++;
				if ( loop > maxLoop ) // oops
					throw new ApplicationException( "Maximum loop count exceeded. Referential integrity sorting algorithm has probable major stuff up." ) ;

				// find a table that has had its dependancies satisfied
				bool	found		= false ;
				string	foundtable	= "" ;
				
				foreach( string table in _notdoneList )
				{
					found = this.HaveDependantTablesForThisTableBeenResolved( table, _riTables[table], _sortedTables ) ;
					if ( found )
					{
						foundtable = table ;
						break;
					}
				}

				// no good table found - cyclic dependancies - must sort out manually	
				if (!found)
				{
					foundtable = _notdoneList[0] ;
					h.Message("!!! Table [{0}] has cyclic referential integrity dependancies. Manual intervention required.", foundtable);

					foreach ( string dependantTable in _riTables[ foundtable ] )
						if ( ! _sortedTables.ContainsValue( dependantTable ) )
							h.Message("!!! Table [{0}] depends on [{1}].", foundtable, dependantTable );
				}					

				// move the found table from the not done list to the sorted list	
				_notdoneList.Remove( foundtable ) ;
				_sortedTables.Add( _sortedTables.Count+1, foundtable ) ;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------		

		// return true if a table has had all its dependant tables already resolved

		protected bool HaveDependantTablesForThisTableBeenResolved( string table, List<string> dependantTables, Dictionary<int, string> resolvedTables )
		{
			Helper h = new Helper() ;

			foreach ( string dependantTable in dependantTables )
			{
				if ( table != dependantTable ) // ignore me if i am self referring
					if ( ! resolvedTables.ContainsValue( dependantTable ) )
					{
						//h.MessageVerbose( "TABLE DEPENDANCY UNMET YET: {0} depends on {1}", table, dependantTable  ) ;
						return false ;
					}
			}
			return true ;
		}

		//--------------------------------------------------------------------------------------------------------------------		

		// return a list of all tables with no ri dependancies

		protected List<string> GetListOfNonRiTables
		(
			List<string>						allTables,
			Dictionary< string, List<string>>	riTables
		)
		{
			List<string> list = new List<string>();
		
			foreach( string table in allTables )
				if ( ! riTables.ContainsKey( table ) )
					list.Add( table ) ;

			list.Sort() ;
			return list;		
		}

		//--------------------------------------------------------------------------------------------------------------------		

		public void DumpInfo() 
		{
			Helper h = new Helper() ;

			h.MessageVerbose("### raw referential integrity dependancies - begin ###");

			// dump out non-ri tables
			foreach ( string table in _nonRiTables )
				h.MessageVerbose( "\t[{0}]", table );

			// dump out ri tables
			h.MessageVerbose( "" );

			List<string> ritables = new List<string>( _riTables.Keys ) ;
			ritables.Sort() ;

			foreach ( string table in ritables ) 
			{
				h.MessageVerbose( "\t[{0}]", table );
				List<string> dependants = _riTables[table];
				
				foreach ( string dependant in dependants )
					h.MessageVerbose( "\t\t--> [{0}]", dependant );
			}

			h.MessageVerbose( "### raw referential integrity dependancies - end, [{0}] non ri tables, [{1}] ri tables, [{2}] total tables ###",
				_nonRiTables.Count, _riTables.Count, _nonRiTables.Count + _riTables.Count );

			// dump out final sorted tables
			h.MessageVerbose( "### sorted referential integrity dependancies - begin ###");

			foreach ( var table in this.SortedTables )
			{
				string fqtable = table.Item2 ;

				List<string> pkcolumns = _di.PrimaryKeyColumns.Get( fqtable, "PK" ) ;
				string pk = pkcolumns.Count >= 1 ? "PK" : "  " ;

				h.MessageVerbose( "\t[{0}]\t\t{1} [{2}]", table.Item1.ToString().PadLeft(4, '0'), pk, fqtable ) ;
			}

			h.MessageVerbose( "### sorted referential integrity dependancies - end, [{0}] tables ###", _sortedTables.Count );

			// dump out problem tables
			if ( _notdoneList.Count > 0 )
			{
				h.Message( "### [problematic] unsorted referential integrity tables - begin ### {0}", _notdoneList.Count > 0 ? "!!!" : "" );

				foreach ( string table in _notdoneList )
					h.Message( "\t[{0}]", table );

				h.Message( "### [problematic] unsorted referential integrity tables - end ###" ) ;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------		
		
	} // end class
}


