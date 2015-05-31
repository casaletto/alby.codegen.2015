using System;
using System.Collections.Generic;
using System.Text;
using System.IO ;

namespace alby.codegen.generator
{
	public class NamespaceBlock : CodeBlockBase 
	{
		public NamespaceBlock( StreamWriter sw, int tabs, string header ) 
			: base( sw, tabs ) 
		{
			Helper h = new Helper() ;

			h.Write(sw, tabs, "namespace " + header);
			h.Write(sw, tabs, "{");
		}
	
	}
}
