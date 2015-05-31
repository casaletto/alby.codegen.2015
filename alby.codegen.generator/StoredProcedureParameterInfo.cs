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
	//--------------------------------------------------------------------------------------------------------------------		

	public class ParameterInfo
	{
		public string	StoredProcedure		{ get ; set ; }
		public string	Name				{ get ; set ; }
		public string	Type				{ get ; set ; }
		public bool		IsTableType			{ get ; set ; }
		public bool		IsOutput			{ get ; set ; }
		public int?		MaxLength			{ get ; set ; }
		public int?		Precision			{ get ; set ; }
		public int?		Scale				{ get ; set ; }
		public bool		DudParameter		{ get ; set ; }

		public ParameterInfo( DataRow dr )
		{
			this.StoredProcedure = dr[ "TheStoredProcedure" ].ToString() ;
			this.Name			 = dr[ "name"               ].ToString().Replace( "@", "" ) ;

			this.IsTableType = bool.Parse( dr[ "is_table_type" ].ToString() ) ;
			this.IsOutput	 = bool.Parse( dr[ "is_output"     ].ToString() ) ;

			if ( ! dr.IsNull( "character_maximum_length" ) )
				this.MaxLength = int.Parse( dr[ "character_maximum_length" ].ToString() ) ;
			else
				this.MaxLength = null ;

			if ( ! dr.IsNull( "numeric_precision" ) )
				this.Precision = int.Parse( dr[ "numeric_precision" ].ToString() ) ;
			else
				this.Precision = null ;		
			
			if ( ! dr.IsNull( "numeric_scale" ) )
				this.Scale = int.Parse ( dr[ "numeric_scale" ].ToString() ) ;
			else
				this.Scale = null ;		

			if ( ! dr.IsNull( "type2" ) )
				this.Type = dr[ "type2" ].ToString() ;
			else
				this.Type = dr[ "type" ].ToString() ;

			// dud data types 
			this.DudParameter = ( this.Type == "cursor" ) ; // yeh baby || this.IsTableType ) ;
		}

	} // end class

	//--------------------------------------------------------------------------------------------------------------------		
	//--------------------------------------------------------------------------------------------------------------------		
	//--------------------------------------------------------------------------------------------------------------------		

	public class StoredProcedureParameterInfo
	{
		protected static Dictionary< string, ParameterInfo >		__dictionaryParameter			= new Dictionary< string, ParameterInfo > () ;
		protected static Dictionary< string, List<ParameterInfo> >	__dictionaryStoredProcedure		= new Dictionary< string, List<ParameterInfo> > () ;
		protected static List<string>								__dudParameterStoredProcedure	= new List<string> () ;

		//--------------------------------------------------------------------------------------------------------------------		

		public void CreateStoredProcedureInfo( DatabaseInfo di )
		{
			Helper h = new Helper() ;

			// already done ?
			if ( __dictionaryParameter.Count > 0 )
				 return ;

			DataTable dt = di.GetDatabaseInfo().Tables[ "spparam" ] ;
			foreach ( DataRow dr in dt.Rows )
			{
				var pi = new ParameterInfo( dr ) ;

				string key = this.GetDictionaryKey( pi.StoredProcedure, pi.Name ) ;
				__dictionaryParameter.Add( key, pi ) ;

				if ( ! __dictionaryStoredProcedure.ContainsKey( pi.StoredProcedure ) )
					   __dictionaryStoredProcedure.Add( pi.StoredProcedure, new List<ParameterInfo>() ) ; 

				__dictionaryStoredProcedure[ pi.StoredProcedure ].Add( pi ) ; 

				if ( pi.DudParameter )
					if ( ! __dudParameterStoredProcedure.Contains( pi.StoredProcedure ) )
						__dudParameterStoredProcedure.Add( pi.StoredProcedure ) ;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------		

		public List<ParameterInfo> GetStoredProcedureParameterInfo( string storedprocedure )
		{
			if ( ! __dictionaryStoredProcedure.ContainsKey( storedprocedure ) )
				 return null ;

			return __dictionaryStoredProcedure[ storedprocedure ] ;
		}

		//--------------------------------------------------------------------------------------------------------------------		

		public ParameterInfo GetStoredProcedureParameterInfo( string storedprocedure, string parameter )
		{
			string key = this.GetDictionaryKey( storedprocedure, parameter ) ;

			if ( ! __dictionaryParameter.ContainsKey( key ) )
				 return null ;

			return __dictionaryParameter[ key ] ;
		}

		//--------------------------------------------------------------------------------------------------------------------		
	
		public bool HasDudParameterStoredProcedure( string storedprocedure )
		{
			return __dudParameterStoredProcedure.Contains( storedprocedure ) ;
		}

		//--------------------------------------------------------------------------------------------------------------------		

		protected string GetDictionaryKey( string storedprocedure, string parameter )
		{
			return "[" + storedprocedure + "].[" + parameter + "]" ;
		}

		//--------------------------------------------------------------------------------------------------------------------		

	} // end class
}
