using System;
using System.Collections.Generic;
using System.Text;
using System.IO ;

namespace alby.codegen.generator
{
	public class TableFactoryConstructorBlock : CodeBlockBase  
	{
		public TableFactoryConstructorBlock(StreamWriter	sw, 
											int				tabs, 
											string			fqtable, 
											string			selectsql, 
											string			insertsql, 
											string			insertidentitysql, 
											string			updatesql, 
											string			deletesql, 
											string			whereloadpk, 
											string			wheresavepk, 
											string			theclass) 
			: base( sw, tabs ) 
		{
			Helper h = new Helper() ;
			Tuple<string,string> schematable = h.SplitSchemaFromTable( fqtable ) ;

			h.Write(sw, tabs, "static " + theclass + "() " );
			h.Write(sw, tabs, "{");

			h.Write(sw, tabs + 1, "_assemblyˡ = sr.Assembly.GetExecutingAssembly() ;");
			h.Write(sw, tabs + 1, "_schemaˡ = \"" + schematable.Item1 + "\" ;");
			h.Write(sw, tabs + 1, "_tableˡ = \"" + schematable.Item2 + "\" ;");
			h.Write(sw, tabs + 1, "_selectˡ = \"" + selectsql + "\" ;");
			h.Write(sw, tabs + 1, "_insertˡ = \"" + insertsql + "\" ;");
			h.Write(sw, tabs + 1, "_insertIdentityˡ = \"" + insertidentitysql + "\" ;");
			h.Write(sw, tabs + 1, "_updateˡ = \"" + updatesql + "\" ;");
			h.Write(sw, tabs + 1, "_deleteˡ = \"" + deletesql + "\" ;");			
			h.Write(sw, tabs + 1, "_whereLoadPKˡ = \"" + whereloadpk + "\" ;");
			h.Write(sw, tabs + 1, "_whereSavePKˡ = \"" + wheresavepk + "\" ;");
		}
		
	
	}
}
