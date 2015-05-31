using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection;
using System.IO ;

namespace alby.codegen.generator
{
	public class UnitTestConstructorBlock : CodeBlockBase 
	{
		public UnitTestConstructorBlock(StreamWriter sw, int tabs, string theClass) 
			: base( sw, tabs ) 
		{
			Helper h = new Helper() ;

			// do constructor
			h.Write(sw, tabs, "public " + theClass + "() : base()" );
			h.Write(sw, tabs, "{");

		} // end
	
	}
}

