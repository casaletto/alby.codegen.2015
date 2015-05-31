using System;
using System.Collections.Generic;
using System.Text;
using System.IO ;

namespace alby.codegen.generator
{
	public class ViewFactoryConstructorBlock : CodeBlockBase 
	{
		public ViewFactoryConstructorBlock(	StreamWriter	sw, 
											int				tabs, 
											string			fqview, 
											string			selectsql, 
											string			theclass) 
			: base( sw, tabs ) 
		{
			Helper h = new Helper() ;

			Tuple<string,string> schemaview = h.SplitSchemaFromTable( fqview ) ;

			h.Write(sw, tabs, "static " + theclass + "() " );
			h.Write(sw, tabs, "{");
			
			h.Write(sw, tabs + 1, "_assemblyˡ = sr.Assembly.GetExecutingAssembly() ;");
			h.Write(sw, tabs + 1, "_schemaˡ = \"" + schemaview.Item1 + "\" ;");
			h.Write(sw, tabs + 1, "_tableˡ = \"" + schemaview.Item2 + "\" ;");
			h.Write(sw, tabs + 1, "_selectˡ = \"" + selectsql + " \" ;");
		}
	
	}
}
