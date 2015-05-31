using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection;
using acr = alby.codegen.runtime ;

namespace alby.codegen.generator
{
	public class DataTableDictionary< A,B,C,D,E > // the type of the return value
	{
		protected bool												_initialised	= false ;
		protected string											_delimiter		= "" ;
		protected List<string>										_keyColumns		= new List<string>() ;
		protected List<string>										_valueColumns	= new List<string>() ;
		protected Dictionary< string, List< Tuple<A,B,C,D,E> > >	_dictionary		= new Dictionary< string, List< Tuple<A,B,C,D,E> > >() ;

		//------------------------------------------------------------------------------------------------------------------

		protected string Key( List<string> searchKeys )
		{
			if ( searchKeys.Count != _keyColumns.Count )
				throw new ApplicationException( "Search key length is incorrect." ) ;

			StringBuilder key = new StringBuilder( 100 ) ;

			foreach( string k in searchKeys )
				key.Append( k + _delimiter ) ;

			return key.ToString() ;

		}

		//------------------------------------------------------------------------------------------------------------------

		protected string GetKeyOfDataRow( DataRow dr )
		{
			StringBuilder key = new StringBuilder( 100 ) ;

			foreach( string column in _keyColumns )
				key.Append( dr[ column ].ToString() + _delimiter ) ;

			return key.ToString() ;
		}

		//------------------------------------------------------------------------------------------------------------------

		protected Tuple<A,B,C,D,E> GetValueOfDataRow( DataRow dr )
		{
			A a = default( A ) ;
			B b = default( B ) ;
			C c = default( C ) ;
			D d = default( D ) ;
			E e = default( E ) ;

			if ( _valueColumns.Count >= 1 )
				 a = this.GetValueOfDataRow<A>( dr, _valueColumns[0] ) ;
			
			if ( _valueColumns.Count >= 2 )
				 b = this.GetValueOfDataRow<B>( dr, _valueColumns[1] ) ;

			if ( _valueColumns.Count >= 3 )
				 c = this.GetValueOfDataRow<C>( dr, _valueColumns[2] ) ;
						
			if ( _valueColumns.Count >= 4 )
				 d = this.GetValueOfDataRow<D>( dr, _valueColumns[3] ) ;

			if ( _valueColumns.Count >= 5 )
				 e = this.GetValueOfDataRow<E>( dr, _valueColumns[4] ) ;

			return Tuple.Create<A,B,C,D,E>( a, b, c, d, e ) ;
		}

		//------------------------------------------------------------------------------------------------------------------

		protected T GetValueOfDataRow<T>( DataRow dr, string column )
		{
			if ( column.Length == 0 ) 
				 return default(T) ;

			if ( dr.IsNull( column ) )
				 return default(T) ;

			return (T)dr[  column ] ;
		}

		//------------------------------------------------------------------------------------------------------------------

		public DataTableDictionary( string delimiter = "" )
		{
			Helper h = new Helper() ;

			if ( delimiter.Length > 0 )
				 _delimiter = delimiter ;
			else
				 _delimiter = h.IdentifierSeparator ;
		}

		//------------------------------------------------------------------------------------------------------------------

		public void Populate( DataTable dt, List<string> keyColumns, List<string> valueColumns, bool uniqueValues = false, bool sort = false )
		{
			_initialised	= true ;
			_keyColumns		= keyColumns ;
			_valueColumns	= valueColumns ;

			foreach ( DataRow dr in dt.Rows )
			{
				string				key   = this.GetKeyOfDataRow( dr ) ;
				Tuple<A,B,C,D,E>	value = this.GetValueOfDataRow( dr ) ;

				if ( ! _dictionary.ContainsKey( key ) )
					   _dictionary.Add( key, new List< Tuple<A,B,C,D,E> >() ) ;

				if ( uniqueValues )
					 if ( _dictionary[ key ].Contains( value ) )
						  continue ;

				_dictionary[ key ].Add( value ) ;
			}

			if ( sort )
				this.Sort() ;
		}

		//------------------------------------------------------------------------------------------------------------------

		public void Populate( DataTable dt, List<string> valueColumns, bool uniqueValues = false, bool sort = false )
		{
			_initialised	= true ;
			_keyColumns		= null ;
			_valueColumns	= valueColumns ;

			_dictionary.Add( "BUCKET", new List< Tuple<A,B,C,D,E> >() ) ;

			foreach ( DataRow dr in dt.Rows )
			{
				Tuple<A,B,C,D,E> value = this.GetValueOfDataRow( dr ) ;

				if ( uniqueValues )
					 if ( _dictionary[ "BUCKET" ].Contains( value ) )
						  continue ;

				_dictionary[ "BUCKET" ].Add( value ) ;
			}

			if ( sort )
				this.Sort() ;
		}

		//------------------------------------------------------------------------------------------------------------------

		public List< Tuple<A,B,C,D,E> > Get( params string[] searchKeys )
		{
			if ( ! _initialised )
				throw new ApplicationException( "Dictionary is not initialised." ) ;

			string key = this.Key( new List<string>( searchKeys ) ) ;

			if ( ! _dictionary.ContainsKey( key ) )
				return new List<Tuple<A,B,C,D,E>> () ;

			return _dictionary[ key ] ;
		}

		//------------------------------------------------------------------------------------------------------------------

		public List< Tuple<A,B,C,D,E> > Get()
		{
			if ( ! _initialised )
				throw new ApplicationException( "Dictionary is not initialised." ) ;

			return _dictionary[ "BUCKET" ] ;
		}

		//------------------------------------------------------------------------------------------------------------------

		public void Sort()
		{
			foreach ( var key in _dictionary.Keys )
				_dictionary[ key ].Sort() ; 
		}

		//------------------------------------------------------------------------------------------------------------------

		public override string ToString()
		{
			StringBuilder bob = new StringBuilder() ;
			
			List<string> keylist = new List<string>( _dictionary.Keys ) ;
			keylist.Sort() ;

			foreach( var key in keylist )
			{
				var list = _dictionary[ key ] ;

				foreach ( var i  in list )
					bob.AppendLine( string.Format( "[{0}] = [{1}]", key, i ) )  ;
			}

			return bob.ToString() ;
		}

	} // end class

	//------------------------------------------------------------------------------------------------------------------
	//------------------------------------------------------------------------------------------------------------------

	public class DataTableDictionary< A,B > : DataTableDictionary< A,B,string,string,string >
	{
		public new List< Tuple<A,B> > Get( params string[] searchKeys )
		{
			var list = new List< Tuple<A,B> >() ;
			var baselist = base.Get( searchKeys ) ;

			baselist.ForEach( i => list.Add( Tuple.Create<A,B>( i.Item1, i.Item2 ) ) ) ;
			
			return list ;
		}

		//------------------------------------------------------------------------------------------------------------------

		public new List< Tuple< A,B > > Get()
		{
			var list = new List< Tuple<A,B> >() ;
			var baselist = base.Get() ;

			baselist.ForEach( i => list.Add( Tuple.Create<A,B>( i.Item1, i.Item2 ) ) ) ;
			
			return list ;
		}

	} // end class

	//------------------------------------------------------------------------------------------------------------------
	//------------------------------------------------------------------------------------------------------------------

	public class DataTableDictionary< A > : DataTableDictionary< A,string,string,string,string >
	{
		public new List< A > Get()
		{
			var list = new List< A >() ;
			var baselist = base.Get() ;

			baselist.ForEach( i => list.Add( i.Item1 ) ) ;
			
			return list ;
		}

		//------------------------------------------------------------------------------------------------------------------

		public new List< A > Get( params string[] searchKeys )
		{
			var list = new List< A >() ;
			var baselist = base.Get( searchKeys ) ;

			baselist.ForEach( i => list.Add( i.Item1 ) ) ;
			
			return list ;
		}



	} // end class

	//------------------------------------------------------------------------------------------------------------------

}
