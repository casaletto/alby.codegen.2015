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
	public class TableFactoryPrimaryKeyMethodBlock : CodeBlockBase, IDisposable 
	{
		public TableFactoryPrimaryKeyMethodBlock(	StreamWriter				sw, 
													int							tabs, 
													string						header, 
													List<string>				parameters,
													Dictionary<string,string>	parameterdictionary,
													string						theclass,
													string						thenamespace )
			: base(sw, tabs ) 
		{
			Helper h = new Helper() ;

			// method header - base method ----------------------------------------

			h.Write(sw, tabs, "public " + theclass + " " + header );
			h.Write(sw, tabs, "(");
			
			int pos = 1 ;
			foreach ( string parameter in parameters )
			{
				string type = parameterdictionary[ parameter ];

				string nullsuffix = "" ;

				if ( parameter == "tranˡ" ) 
					 nullsuffix = " = null" ;

				h.Write(sw, tabs + 1, type + " " + parameter + nullsuffix + (pos != parameterdictionary.Keys.Count ? "," : "") );
				pos++ ;
			}

			h.Write(sw, tabs, ")");
			h.Write(sw, tabs, "{");
		
			// parameters for SqlCommand
			h.Write(sw, tabs + 1, "scg.List<sds.SqlParameter> parametersˡ = new scg.List<sds.SqlParameter>();" ) ;
			
			foreach ( string parameter in parameters )
			{
				if ( parameter == "connˡ" ) continue ;
				if ( parameter == "tranˡ" ) continue ;

				string csharptype	= parameterdictionary[ parameter ];
				string udtType		= h.GetUdtParameterTypeFromCsharpType(csharptype);

				if ( udtType.Length > 0 )
					h.Write(sw, tabs + 1, "base.AddParameterˡ( parametersˡ, \"@pk_" + parameter + "\", " + parameter + ", \"" + udtType + "\" ); ");
				else
					h.Write(sw, tabs + 1, "base.AddParameterˡ( parametersˡ, \"@pk_" + parameter + "\", " + parameter + " );" );
			}			
			
			// execute the query
			h.Write(sw, tabs + 1, "return base.ExecuteQueryReturnOneˡ( connˡ, tranˡ, parametersˡ, _assemblyˡ, _selectˡ, false, _whereLoadPKˡ, false ) ;");
		}

		public new void Dispose()
		{
			Helper h = new Helper() ;

			h.Write(_sw, _tabs, "}\r\n");
		}
		
	}
}
