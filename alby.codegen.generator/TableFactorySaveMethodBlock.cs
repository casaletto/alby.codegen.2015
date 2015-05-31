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
using Microsoft.SqlServer.Types ;

namespace alby.codegen.generator
{
	public class TableFactorySaveMethodBlock : CodeBlockBase, IDisposable 
	{
		public TableFactorySaveMethodBlock(	StreamWriter					sw, 
											int								tabs,
											Dictionary<string,string>		parameterdictionary,
											List< Tuple<string,string> >	columns, 
											List<string>					identitycolumns,
											string							concurrencycolumn,
											string							theclass
										  )
			: base(sw, tabs ) 
		{
			Helper h = new Helper() ;

			// method header - overloaded function with force save
			h.Write(sw, tabs, "public " + theclass + " Saveˡ( sds.SqlConnection connˡ, " + theclass + " rowˡ, acr.CodeGenSaveStrategy saveStrategyˡ = acr.CodeGenSaveStrategy.Normal, bool identityProvidedˡ = false, sds.SqlTransaction tranˡ = null )" ) ;
			h.Write(sw, tabs, "{");
		
			// parameters for SqlCommand
			h.Write(sw, tabs + 1, "scg.List<sds.SqlParameter> parametersˡ = new scg.List<sds.SqlParameter>();" ) ;

			// primary key parameters
			foreach (string parameter in parameterdictionary.Keys)
			{
			    if ( parameter == "connˡ" ) continue  ;
			    if ( parameter == "tranˡ" ) continue  ;

				string csharptype	= parameterdictionary[ parameter ];
				string udtType		= h.GetUdtParameterTypeFromCsharpType(csharptype);

				if (udtType.Length > 0)
					h.Write(sw, tabs + 1, "base.AddParameterˡ( parametersˡ, \"@pk_" + parameter + "\", rowˡ.PrimaryKeyDictionaryˡ[ " + theclass + ".column!".Replace( "!", h.IdentifierSeparator ) + parameter + " ], \"" + udtType + "\" ); ");
				else
					h.Write(sw, tabs + 1, "base.AddParameterˡ( parametersˡ, \"@pk_" + parameter + "\", rowˡ.PrimaryKeyDictionaryˡ[ " + theclass + ".column!".Replace( "!", h.IdentifierSeparator ) + parameter + " ] );");
			}			

			// concurrency			
			if ( concurrencycolumn.Length > 0 )
				foreach( var column in columns )
					if ( column.Item1 == concurrencycolumn )
					{
						h.Write(sw, tabs + 1, "base.AddParameterˡ( parametersˡ, \"@concurrency_" + h.GetCsharpColumnName( column.Item1, theclass ) + "\", rowˡ.ConcurrencyTimestampˡ );");
						break ;
					} 
			
			// field parameters
			foreach( var column in columns )
			{
				string parameter = h.GetCsharpColumnName( column.Item1, theclass ) ;

				try // normal sql type
				{
					SqlDbType sqltype = h.GetSqlServerColumnType( column.Item2 );
					h.Write(sw, tabs + 1, "base.AddParameterˡ( parametersˡ, \"@" + h.GetCsharpColumnName( column.Item1, theclass ) + "\", rowˡ." + parameter + ", sd.SqlDbType." + sqltype.ToString() + " );");
				}
				catch( Exception ) // try udt type
				{
					string udttype = h.GetSqlServerUdtColumnType( column.Item2 ) ;
					h.Write(sw, tabs + 1, "base.AddParameterˡ( parametersˡ, \"@" + h.GetCsharpColumnName( column.Item1, theclass) + "\", rowˡ." + parameter + ", \"" + udttype  + "\" );" );
				}

			}
			sw.WriteLine("");

			// identityID 
			string identitytable = identitycolumns.Count > 0 ? "true" : "false";
			 
			// get the name and type of identity column
			string identityname			= "" ;
			string identitynamecsharp	= "" ;
			string identitytype			= "int?" ;

			if ( identitycolumns.Count > 0 )
				foreach( var column in columns )
					if ( column.Item1 == identitycolumns[0] )
					{
						identityname		= identitycolumns[0] ;
						identitynamecsharp	= h.GetCsharpColumnName( identityname, theclass) ;
						identitytype		= h.GetCsharpColumnType( column.Item2 );
						break ;
					}

			h.Write(sw, tabs + 1, identitytype + " identityIDˡ = null ;" ) ;
			h.Write(sw, tabs + 1, "object objˡ = null ;" ) ;
			h.Write(sw, tabs, " ");

			// execute the query and reload
			h.Write(sw, tabs + 1, "acr.SaveEnum saveResultˡ ;");
			h.Write(sw, tabs, " ");

			h.Write(sw, tabs + 1, "if ( saveStrategyˡ != acr.CodeGenSaveStrategy.Normal )");
			h.Write(sw, tabs + 1, "{");
			h.Write(sw, tabs + 2, "saveResultˡ = base.ExecuteForceSaveˡ( rowˡ, connˡ, tranˡ, parametersˡ, saveStrategyˡ, _insertˡ, _insertIdentityˡ, _updateˡ, _deleteˡ, _whereLoadPKˡ, " + identitytable + ", identityProvidedˡ, out objˡ ) ;");

			h.Write(sw, tabs + 2, "if ( objˡ != null )");
			h.Write(sw, tabs + 3, "identityIDˡ = " + identitytype.Replace("?", "") + ".Parse( objˡ.ToString() ) ;");
			h.Write(sw, tabs + 1, "}");

			h.Write(sw, tabs + 1, "else");
			h.Write(sw, tabs + 1, "{");
			h.Write(sw, tabs + 2, "saveResultˡ = base.ExecuteSaveˡ( rowˡ, connˡ, tranˡ, parametersˡ, _insertˡ, _insertIdentityˡ, _updateˡ, _deleteˡ, _whereSavePKˡ, " + identitytable + ", identityProvidedˡ, out objˡ ) ;");
			h.Write(sw, tabs + 2, "if ( objˡ != null )" ) ;
			h.Write(sw, tabs + 3, "identityIDˡ = " + identitytype.Replace( "?", "" ) + ".Parse( objˡ.ToString() ) ;");
			h.Write(sw, tabs + 1, "}");
			h.Write(sw, tabs, " ");
			
			h.Write(sw, tabs + 1, "if ( saveResultˡ == acr.SaveEnum.Update ) ");
			h.WriteTabs( sw, tabs + 2 ) ;
			sw.Write("return this.LoadByPrimaryKeyˡ( connˡ" );

			foreach (string parameter in parameterdictionary.Keys)
				if ( ! (parameter == "connˡ" || parameter == "tranˡ" ) )
					sw.Write(", rowˡ." + parameter);

			sw.Write(", tranˡ" );
			sw.Write( " ) ;" );
			sw.WriteLine( "" ) ;

			h.Write(sw, tabs + 1, "else");
			h.Write(sw, tabs + 1, "if ( saveResultˡ == acr.SaveEnum.Insert ) ");

			h.WriteTabs(sw, tabs + 2);
			sw.Write("return this.LoadByPrimaryKeyˡ( connˡ");

			foreach ( string parameter in parameterdictionary.Keys) 
			{
				if ( parameter == "connˡ" || parameter == "tranˡ" ) 
					 continue ;

				if ( parameter == identitynamecsharp )
					 sw.Write(", identityIDˡ" );
				else
					 sw.Write(", rowˡ." + parameter );
			}

			sw.Write(", tranˡ" );
			sw.Write(" ) ;");
			sw.WriteLine() ;

			h.Write(sw, tabs + 1, "else");
			h.Write(sw, tabs + 1, "if ( saveResultˡ == acr.SaveEnum.Delete )");
			h.Write(sw, tabs + 2, "return null ;");
			h.Write(sw, tabs + 1, "else");
			h.Write(sw, tabs + 2, "return rowˡ ;");
		}

		public new void Dispose()
		{
			Helper h = new Helper() ;

			h.Write(_sw, _tabs, "}\r\n");
		}
		
	}
}
