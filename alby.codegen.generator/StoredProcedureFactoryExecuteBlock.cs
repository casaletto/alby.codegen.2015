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

namespace alby.codegen.generator
{
	public class StoredProcedureFactoryExecuteBlock : CodeBlockBase, IDisposable 
	{
		public StoredProcedureFactoryExecuteBlock(	Program									p,
													StreamWriter							sw, 
													int										tabs, 
													string									fqstoredprocedure,
													string									csharpstoredproc, 	
													List<string>							parameters,
													StoredProcedureParameterInfo			sppi,
													ResultsetInfo							rsi )
			: base(sw, tabs ) 
		{
			Helper h = new Helper() ;

			var schemastoredprocedure = h.SplitSchemaFromTable( fqstoredprocedure ) ;

			h.Write( sw, tabs, "public int " + csharpstoredproc ) ;
			h.Write( sw, tabs, "(");

			// function interface
			int pos = 1 ;
			foreach ( string parameter in parameters )
			{
			    if ( parameter == "tranˡ" ) continue ;

			    string csharpname = h.GetCsharpColumnName( parameter, "" ) ;

			    string csharptype = "" ;
			    if ( parameter == "connˡ" )
					 csharptype = "sds.SqlConnection" ;
				else
				if ( parameter == "tranˡ" )
			         csharptype = "sds.SqlTransaction" ;
			    else
				{
					var pi = sppi.GetStoredProcedureParameterInfo( fqstoredprocedure, parameter ) ;

					if ( pi.IsTableType )
					{
						Tuple<string,string> schematabletype = h.SplitSchemaFromTable( pi.Type )  ;

						string csharptabletype = h.GetCsharpClassName( p._prefixObjectsWithSchema, schematabletype.Item1, schematabletype.Item2 ) ;
						csharptype = "scg.List<" + csharptabletype + h.IdentifierSeparator + "tt>" ;
					}
					else
						csharptype = h.GetCsharpColumnTypeForStoredProcedure( pi.Type ) ;
				}

			    string inout = "" ;
			    if ( parameter == "connˡ" )
					 inout = "" ;
				else
				if ( parameter == "tranˡ" )
					 inout = "" ;
			    else
				{
					var pi = sppi.GetStoredProcedureParameterInfo( fqstoredprocedure, parameter ) ;
					if ( pi.IsOutput )
						 inout = "ref " ;
				}

			    h.Write( sw, tabs + 1, inout + csharptype + " " + csharpname + ", " ) ; 
			    pos++ ;
			}

			// do resultsets or dataset
			if ( rsi.ErrorMessage.Length > 0 ) 
			{
				h.Write(sw, tabs + 1, "out sd.DataSet dsˡ, " ) ;
			}
			else
			if ( rsi.Resultsets.Count > 0 )
			{
				int i = 0 ;
				foreach( Resultset rs in rsi.Resultsets )
				{
					i++ ;

					string ignoreRecordset = "" ;
					if ( rsi.IgnoredResultsets.Contains( i ) ) ignoreRecordset = "//" ;

					string therecordsetclass = csharpstoredproc + h.IdentifierSeparator + "rs" + i ;
					h.Write( sw, tabs + 1, ignoreRecordset + "out scg.List<" + therecordsetclass + "> rsˡ" + i + ", " ) ;
				}
			}

			// tran - last parameter
			h.Write( sw, tabs + 1, "sds.SqlTransaction tranˡ = null" ) ;
			h.Write( sw, tabs, ")");
			h.Write( sw, tabs, "{");
			h.Write( sw, tabs+1, "const string schemaˡ = \"" + schemastoredprocedure.Item1 + "\" ; " ) ;
			h.Write( sw, tabs+1, "const string spˡ = \"" + schemastoredprocedure.Item2 + "\" ; " ) ;
			h.Write( sw, tabs+1, " " ) ;
			
			// add the parameters
			h.Write( sw, tabs+1, "scg.List<sds.SqlParameter> parametersˡ = new scg.List<sds.SqlParameter>() ;" ) ;

			foreach ( string parameter in parameters )
			{
				if ( parameter == "connˡ" || parameter == "tranˡ" || parameter == "rcˡ" ) continue ; 

				var pi = sppi.GetStoredProcedureParameterInfo( fqstoredprocedure, parameter ) ;

				string csharpname	= h.GetCsharpColumnName( parameter, "" ) ;
				string sqlparameter = "paramˡ" + csharpname ;

				if ( pi.IsTableType ) // table type
				{
						Tuple<string,string> schematabletype = h.SplitSchemaFromTable( pi.Type )  ;
						string csharptabletype = h.GetCsharpClassName( p._prefixObjectsWithSchema, schematabletype.Item1, schematabletype.Item2 ) ;

						h.Write( sw, tabs + 1, "sds.SqlParameter " 
													+ sqlparameter 
													+ " = base.AddParameterTableTypeˡ( parametersˡ, \"@" 
													+ parameter 
													+ "\", this." + csharpstoredproc + h.IdentifierSeparator + csharpname + "ˡ( " + csharpname + " ), " 
													+ "\"" 
													+ pi.Type
													+ "\"" 
													+ " ) ; " ) ;
				}
				else
					try // normal sql type
					{
						SqlDbType sqldbtype = h.GetSqlDbTypeForStoredProcedure( pi.Type ) ;

						h.Write( sw, tabs + 1, "sds.SqlParameter " 
													+ sqlparameter 
													+ " = base.AddParameterˡ( parametersˡ, \"@" 
													+ parameter 
													+ "\", " 
													+ csharpname 
													+ ", sd.SqlDbType." 
													+ sqldbtype.ToString()
													+ ", " 
													+ (!pi.IsOutput).ToString().ToLower() 
													+ ", " 
													+ (pi.MaxLength == null ? "null" : pi.MaxLength.ToString() ) 
													+ ", " 
													+ (pi.Precision == null ? "null" : pi.Precision.ToString() )
													+ ", " 
													+ (pi.Scale == null ? "null" : pi.Scale.ToString() )  
													+ " ) ; " ) ;
					}
					catch( Exception ) // try udt type
					{
						string udtType = h.GetSqlDbTypeForStoredProcedureUdt( pi.Type ) ;

						h.Write( sw, tabs + 1, "sds.SqlParameter " 
													+ sqlparameter 
													+ " = base.AddParameterUdtˡ( parametersˡ, \"@" 
													+ parameter 
													+ "\", " 
													+ csharpname 
													+ ", " 
													+ "\"" 
													+ udtType 
													+ "\""
													+ ", " 
													+ (!pi.IsOutput).ToString().ToLower() 
													+ ", " 
													+ (pi.MaxLength == null ? "null" : pi.MaxLength.ToString() )
													+ " ) ; " ) ;
					}
			}

			// add return value parameter
			h.Write( sw, tabs + 1, "sds.SqlParameter paramˡrcˡ = base.AddParameterReturnValueˡ( parametersˡ, \"@rcˡ\" ) ; " ) ;

			// the actual execute
			h.Write( sw, tabs+1, " " ) ;

			if ( rsi.ErrorMessage.Length > 0 ) 
			     h.Write( sw, tabs+1, "dsˡ = base.Executeˡ( connˡ, tranˡ, schemaˡ, spˡ, parametersˡ ) ;" ) ;
			else
			     h.Write( sw, tabs+1, "sd.DataSet dsˡ = base.Executeˡ( connˡ, tranˡ, schemaˡ, spˡ, parametersˡ ) ;" ) ;

			// the returned values
			h.Write( sw, tabs+1, " " ) ;

			foreach ( string parameter in parameters )
			{
				if ( parameter == "connˡ" || parameter == "tranˡ" || parameter == "rcˡ" ) continue ; 

				var pi = sppi.GetStoredProcedureParameterInfo( fqstoredprocedure, parameter ) ;
				if ( ! pi.IsOutput )
					continue ;

				string csharpname	= h.GetCsharpColumnName( parameter, "" ) ;
				string sqlparameter = "paramˡ" + csharpname ;
				string csharptype	= h.GetCsharpColumnTypeForStoredProcedure( pi.Type ) ;

				h.Write( sw, tabs+1, csharpname + " = base.GetParameterValueˡ<" + csharptype + ">( " + sqlparameter + " ) ;" ) ;
			}

			// the recordsets if any
			if ( rsi.Resultsets.Count > 0 )
			{
			    int i = 0 ;
				int j = 0 ;
				foreach( Resultset rs in rsi.Resultsets )
			    {
			        i++ ;

					string ignoreRecordset = "" ;
					if ( rsi.IgnoredResultsets.Contains( i ) ) 
						ignoreRecordset = "//" ;
					else
						j++ ;

				    string therecordsetclass = csharpstoredproc + h.IdentifierSeparator + "rs" + i ;
			        h.Write(sw, tabs + 1, ignoreRecordset + "rsˡ" + i + " = base.ToRecordsetˡ<" + therecordsetclass + ">( dsˡ, " + j + " ) ;" ) ;
			    }

			    h.Write(sw, tabs+1, " " ) ;
			}

			// finish up - add return value parameter
			h.Write( sw, tabs+1, "return base.GetParameterValueˡ<int>( paramˡrcˡ ) ;" ) ;
			h.Write( sw, tabs,   "}" ) ;

			// table type paramater helper function
			foreach ( string parameter in parameters )
			{
				if ( parameter == "connˡ" || parameter == "tranˡ" || parameter == "rcˡ" ) continue ; 

			    var pi = sppi.GetStoredProcedureParameterInfo( fqstoredprocedure, parameter ) ;
			    if ( ! pi.IsTableType ) continue ; // table types only

			    string csharpname = h.GetCsharpColumnName( parameter, "" ) ;

			    Tuple<string,string> schematabletype = h.SplitSchemaFromTable( pi.Type )  ;
			    string csharptabletype = h.GetCsharpClassName( p._prefixObjectsWithSchema, schematabletype.Item1, schematabletype.Item2 ) ;

				h.Write( sw, tabs  ,  " " ) ;
				h.Write( sw, tabs  , "protected object " + csharpstoredproc + h.IdentifierSeparator + csharpname + "ˡ( scg.List<" + csharptabletype + h.IdentifierSeparator + "tt> list )" ) ;
				h.Write( sw, tabs  ,  "{" ) ;
				h.Write( sw, tabs+1,  "if ( list == null ) return null ;" ) ;
				h.Write( sw, tabs+1,  "if ( list.Count == 0 ) return null ;" ) ;
				h.Write( sw, tabs+1,  "return new  ns.storedProcedure." + csharptabletype + h.IdentifierSeparator + "ttlist( list ) ;" ) ;
				h.Write( sw, tabs  ,  "}" ) ;
			}

		} // end constructor

		//------------------------------------------------------------------------------------------------------------------

		public new void Dispose()
		{
			//Helper h = new Helper() ;
			//h.Write(_sw, _tabs, "}\r\n");
		}

		//------------------------------------------------------------------------------------------------------------------


	} // end class
}
