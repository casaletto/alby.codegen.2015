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
using System.Reflection;
using System.Linq ;

namespace alby.codegen.runtime
{
	public class RowBase
	{
		#region state
		
		protected Dictionary<string, object>	_dicˡ	= new Dictionary<string,object>() ;
		protected Dictionary<string, object>	_dicPKˡ	= new Dictionary<string,object>() ;

		protected string		_concurrencyColumnˡ		= ""    ;
		protected object		_concurrencyTimestampˡ	= null  ;
		
		protected bool			_fromDatabaseˡ			= false ;
		protected bool			_dirtyˡ					= true  ;
		protected bool			_deletedˡ				= false ;
		protected bool			_forDeletionˡ			= false ;
		protected bool			_savedˡ					= false;

		#endregion
		
		#region properties

		public string ConcurrencyColumnˡ
		{
			get
			{
				return _concurrencyColumnˡ;
			}
			set
			{
				_concurrencyColumnˡ = value;
			}
		}

		public object ConcurrencyTimestampˡ
		{
			get
			{
				return _concurrencyTimestampˡ;
			}
			set
			{
				_concurrencyTimestampˡ = value;
			}
		}

		public Dictionary<string, object> PrimaryKeyDictionaryˡ
		{
			get
			{
				return _dicPKˡ;
			}
			set
			{
				_dicPKˡ = value;
			}
		}

		public Dictionary<string, object> Dictionaryˡ
		{
			get
			{
				return _dicˡ ;
			}
			set
			{
				_dicˡ = value;
			}
		}

		public bool IsDirtyˡ
		{
			get
			{
				return _dirtyˡ ;
			}
			set
			{
				_dirtyˡ = value;
			}
		}

		public bool IsDeletedˡ
		{
			get
			{
				return _deletedˡ ;
			}
			set
			{
				_deletedˡ = value;
			}
		}

		public bool MarkForDeletionˡ
		{
			get
			{
				return _forDeletionˡ ;
			}
			set
			{
				_forDeletionˡ = value;
			}
		}

		public bool IsFromDatabaseˡ
		{
			get
			{
				return _fromDatabaseˡ ;
			}
			set
			{
				_fromDatabaseˡ = value ;
			}
		}

		public bool IsSavedˡ
		{
			get
			{
				return _savedˡ ;
			}
			set
			{
				_savedˡ = value ;
			}
		}

		#endregion
		
		public RowBase()
		{
		}

		protected A GetValueˡ<A>( Dictionary<string, object> dic, string col ) 
		{
			object o = dic[ col ]  ;

			if ( o == null ) return default(A) ;
			if ( o == DBNull.Value ) return default(A) ;

			if ( o is INullable ) // udt's implement this guy
			{
				INullable n = o as INullable ;
				if ( n.IsNull ) return default(A) ;
			}
			return (A) o ;
		}

		protected void SetValueˡ<A>( Dictionary<string, object> dic, string col, A value, ref bool dirty )
		{
			// do dirty flag
			// save new value in datarow

			A orig = GetValueˡ<A>( dic, col ) ;
			if ( ! AreEqualˡ<A>( value, orig ) )
			{
				dirty = true;
				dic[col] = value ;
			}
		}

		protected bool AreEqualˡ<A>( A a, A b ) 
		{
			if ( a == null && b == null ) return true ;
			if ( a == null || b == null ) return false ;

			// compare byte arrays
			if ( a is byte[] && b is byte[] )
			{
				IEnumerable<byte> a1 = a as IEnumerable<byte> ;
				IEnumerable<byte> b1 = b as IEnumerable<byte> ;

				return a1.SequenceEqual( b1 ) ;
			}

			// compare udt's
			if ( a is INullable && b is INullable ) 
			{
				INullable a1 = a as INullable ;
				INullable b1 = b as INullable ;

				if ( a1.IsNull && b1.IsNull ) return true ;
				if ( a1.IsNull || b1.IsNull ) return false ;

				return a.ToString().Equals( b.ToString() ) ;
			}
			return a.Equals( b ) ;
		}

	} // end class
}
