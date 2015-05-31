using System;
using System.Collections.Generic;
using System.Text;
using System.IO ;

namespace alby.codegen.generator
{
	public class DatabaseConstructorBlock : CodeBlockBase  
	{
		public DatabaseConstructorBlock( StreamWriter sw, int tabs, string theclass, string databasename ) : base( sw, tabs ) 
		{
			Helper h = new Helper() ;

			h.Write(sw, tabs, "static " + theclass + "()" );
			h.Write(sw, tabs, "{" ) ;
			h.Write(sw, tabs+1, "Initˡ( \"" + databasename + "\" ) ; " ) ;
		}

	}
}
