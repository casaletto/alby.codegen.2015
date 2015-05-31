using System;
using System.Collections.Generic;
using System.Text;
using System.IO ;

namespace alby.codegen.generator
{
	public class CodeBlockBase : IDisposable 
	{
		protected StreamWriter	_sw ;
		protected int			_tabs = 0  ;
		
		public CodeBlockBase( StreamWriter sw, int tabs ) 
		{
			_sw = sw;
			_tabs = tabs;
		}

		public void Dispose()
		{
			Helper h = new Helper() ;

			h.Write(_sw, _tabs, "}\r\n");
		}
	}
}
