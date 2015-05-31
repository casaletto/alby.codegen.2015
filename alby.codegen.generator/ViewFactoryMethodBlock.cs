using System;
using System.Collections.Generic;
using System.Text;
using System.IO ;

namespace alby.codegen.generator
{
	public class ViewFactoryMethodBlock : CodeBlockBase, IDisposable  
	{
		public ViewFactoryMethodBlock(	StreamWriter				sw, 
										int							tabs, 
										string						header, 
										List<string>				parameters,
										Dictionary<string,string>	parameterdictionary, 
										string						where, 
										string						theclass,
										string						thenamespace )
			: base(sw, tabs ) 
		{
			Helper h = new Helper() ;

			// method header - base method
			
			h.Write(sw, tabs, "public scg.List<" + theclass + "> " + header );
			h.Write(sw, tabs, "(");
			
			int pos = 1 ;
			foreach( string parameter in parameters )
			{
				string type = parameterdictionary[ parameter ];

				string nullsuffix = "" ;

				if ( parameter == "tranˡ"    || 
					 parameter == "topNˡ"    || 
					 parameter == "orderByˡ" || 
					 parameter == "parametersˡ" ) 
					 nullsuffix = " = null" ;

				h.Write(sw, tabs + 1, type + " " + parameter + nullsuffix + (pos != parameterdictionary.Keys.Count ? "," : "") );
				
				pos++ ;
			}
			h.Write(sw, tabs, ")");
			h.Write(sw, tabs, "{");
		
			// parameters for SqlCommand
			if ( ! parameterdictionary.ContainsKey( "parametersˡ" ) )
				   h.Write(sw, tabs + 1, "scg.List<sds.SqlParameter> parametersˡ = new scg.List<sds.SqlParameter>();" ) ;
			
			int param = 1 ;
			foreach ( string parameter in parameters )
			{
				if ( parameter == "connˡ"		) continue;
				if ( parameter == "whereˡ"		) continue;

				if ( parameter == "parametersˡ"	) continue;
				if ( parameter == "topNˡ"		) continue;
				if ( parameter == "orderByˡ"	) continue;
				if ( parameter == "tranˡ"		) continue;

				string csharptype	= parameterdictionary[ parameter ];
				string udttype		= h.GetUdtParameterTypeFromCsharpType(csharptype);

				if ( udttype.Length > 0)
					 h.Write(sw, tabs + 1, "base.AddParameterˡ( parametersˡ, \"@" + parameter + "\", " + parameter + ", \"" + udttype + "\" ); ");
				else
					 h.Write(sw, tabs + 1, "base.AddParameterˡ( parametersˡ, \"@" + parameter + "\", " + parameter + " );");

				param++ ;
			}			
			
			// get the where resources
			if ( ! parameterdictionary.ContainsKey( "whereˡ" ) )
			{
				if ( where.Length > 0 )
					 where = thenamespace + "." + where.Replace(@"\", ".");

				h.Write(sw, tabs + 1, "string whereˡ = \"" + where  + "\";");
			}

			h.Write(sw, tabs + 1, "string sqlˡ = \"\" ; ");
			
			// execut the query
			if ( parameterdictionary.ContainsKey( "whereˡ" ) )
				 h.Write(sw, tabs + 1, "return base.ExecuteQueryˡ( connˡ, tranˡ, parametersˡ, _assemblyˡ, _selectˡ, false, whereˡ, false, topNˡ, orderByˡ, out sqlˡ ) ;");
			else
			if ( where.Length > 0 )
				 h.Write(sw, tabs + 1, "return base.ExecuteQueryˡ( connˡ, tranˡ, parametersˡ, _assemblyˡ, _selectˡ, false, whereˡ, true, topNˡ, orderByˡ, out sqlˡ ) ;");
			else
				 h.Write(sw, tabs + 1, "return base.ExecuteQueryˡ( connˡ, tranˡ, parametersˡ, _assemblyˡ, _selectˡ, false, whereˡ, false, topNˡ, orderByˡ, out sqlˡ ) ;");			
		}

		public new void Dispose()
		{
			Helper h = new Helper() ;

			h.Write(_sw, _tabs, "}\r\n");
		}
	
	}
}
