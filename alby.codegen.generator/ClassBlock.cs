using System;
using System.Collections.Generic;
using System.Text;
using System.IO ;

namespace alby.codegen.generator
{
	public class ClassBlock : CodeBlockBase 
	{
		public ClassBlock( StreamWriter sw, int tabs, string header, string baseclass ) 
			: base( sw, tabs )
		{
			Helper h = new Helper() ;

			h.Write(sw, tabs, "public partial class " + header + " : " + baseclass);
			h.Write(sw, tabs, "{");
		}
	
	}
}
