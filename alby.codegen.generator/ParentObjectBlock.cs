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
	public class ParentObjectBlock : CodeBlockBase 
	{
		public ParentObjectBlock(StreamWriter	sw, 
								int				tabs, 
								Program			p, 
								string			fqtable, 
								List<string>	columns, 
								string			theclass) 
			: base( sw, tabs ) 
		{
			Helper h = new Helper() ;

			Tuple<string,string> schematable = h.SplitSchemaFromTable( fqtable ) ;

			string csharpclassname = h.GetCsharpClassName( p._prefixObjectsWithSchema, schematable.Item1, schematable.Item2 ) ;
		
			string longname = h.IdentifierSeparator + "By" + h.IdentifierSeparator ;
			foreach( string column in columns ) 
				longname += h.GetCsharpColumnName( column, csharpclassname  ) ;

			// base method

			h.Write(sw, tabs, "public " + csharpclassname + " parent" + h.IdentifierSeparator + csharpclassname + longname + "( sds.SqlConnection connˡ, sds.SqlTransaction tranˡ = null )");
			h.Write(sw, tabs, "{");

			string str = "#Factory factoryˡ = new #Factory() ; ".Replace("#", csharpclassname);
			h.Write(sw, tabs + 1, str );

			str = "return factoryˡ.LoadByPrimaryKeyˡ";
			h.Write(sw, tabs + 1, str);

			str = "(";
			h.Write(sw, tabs + 1, str);

			str = "connˡ,";
			h.Write(sw, tabs + 2, str);

			int pos = 1 ;
			foreach (string column in columns)
			{
				str = "this." + h.GetCsharpColumnName( column, theclass ) + ", " ; 
				h.Write(sw, tabs + 2, str);  
				pos ++ ;
			}

			str = "tranˡ";
			h.Write(sw, tabs + 2, str);

			str = ") ;";
			h.Write(sw, tabs + 1, str);

		} // end
	
	}
}
