using System;
using System.Collections.Generic;
using System.Text;
using System.IO ;

namespace alby.codegen.generator
{
	public class QueryFactoryConstructorBlock : CodeBlockBase 
	{
		public QueryFactoryConstructorBlock( StreamWriter sw, int tabs, string selectresource, string theclass ) 
			: base( sw, tabs ) 
		{
			Helper h = new Helper() ;

			h.Write(sw, tabs, "static " + theclass + "() " );
			h.Write(sw, tabs, "{");
			
			h.Write(sw, tabs + 1, "_assemblyˡ = sr.Assembly.GetExecutingAssembly() ;");

			selectresource = selectresource.Replace(@"\", ".");
			h.Write(sw, tabs + 1, "_selectˡ = \"" + selectresource + "\" ;" );
		}
	
	}
}
