using System;
using System.Collections.Generic;
using System.Text;
using System.IO ;

namespace alby.codegen.generator
{
	public class QueryFactoryMethodBlock : CodeBlockBase, IDisposable  
	{
		public QueryFactoryMethodBlock(	StreamWriter				sw, 
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
			foreach (string parameter in parameters )
			{
				string type = parameterdictionary[parameter];

				string nullsuffix = "" ;
				if ( parameter == "tranˡ" || 
					 parameter == "topNˡ" || 
					 parameter == "orderByˡ" ) 
					 nullsuffix = " = null" ;

				h.Write(sw, tabs + 1, type + " " + parameter + nullsuffix + (pos != parameterdictionary.Keys.Count ? "," : "") );
				
				pos++ ;
			}
			h.Write(sw, tabs, ")");
			h.Write(sw, tabs, "{");
		
			// parameters for SqlCommand
			h.Write(sw, tabs + 1, "scg.List<sds.SqlParameter> parametersˡ = new scg.List<sds.SqlParameter>();" ) ;
			
			foreach (string parameter in parameters )
			{
				if ( parameter == "connˡ"    ) continue ;
				if ( parameter == "tranˡ"    ) continue ;
				if ( parameter == "topNˡ"    ) continue ;
				if ( parameter == "orderByˡ" ) continue ;

				string csharptype = parameterdictionary[ parameter ];
				string udtType = h.GetUdtParameterTypeFromCsharpType( csharptype ) ;

				if (udtType.Length > 0)
					h.Write(sw, tabs + 1, "base.AddParameterˡ( parametersˡ, \"@" + parameter + "\", " + parameter + ", \"" + udtType + "\" ); ");
				else
					h.Write(sw, tabs + 1, "base.AddParameterˡ( parametersˡ, \"@" + parameter + "\", " + parameter + " );");
			}			
			
			// get the where resources
			where = thenamespace + "." + where.Replace(@"\", ".");
			h.Write(sw, tabs + 1, "string whereˡ = \"" + where  + "\";");

			h.Write(sw, tabs + 1, "string sqlˡ = \"\" ; ");
			
			// execut the query
			h.Write(sw, tabs + 1, "return base.ExecuteQueryˡ( connˡ, tranˡ, parametersˡ, _assemblyˡ, _selectˡ, true, whereˡ, true, topNˡ, orderByˡ, out sqlˡ ) ;");
		}

		public new void Dispose()
		{
			Helper h = new Helper() ;

			h.Write(_sw, _tabs, "}\r\n");
		}
	
	}
}
