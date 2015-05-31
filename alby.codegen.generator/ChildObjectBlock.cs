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
	public class ChildObjectBlock : CodeBlockBase 
	{
		public ChildObjectBlock(StreamWriter	sw, 
								int				tabs, 
								Program			p, 
								string			fqtable,
								string			fqchildtable,
								List<string>	fkcolumns, 
								List<string>	pkcolumns, 
								string			theclass) 
			: base( sw, tabs ) 
		{
			Helper h = new Helper() ;

			Tuple<string,string> schematable		= h.SplitSchemaFromTable( fqtable ) ;
			Tuple<string,string> childeschematable	= h.SplitSchemaFromTable( fqchildtable ) ;

			string csharpclassname		= h.GetCsharpClassName( p._prefixObjectsWithSchema, schematable.Item1,       schematable.Item2 ) ;
			string csharpchildclassname = h.GetCsharpClassName( p._prefixObjectsWithSchema, childeschematable.Item1, childeschematable.Item2 ) ;
			
			string longname = h.IdentifierSeparator + "By" + h.IdentifierSeparator ;
			foreach( string fkcolumn in fkcolumns )
				longname += h.GetCsharpColumnName( fkcolumn, csharpchildclassname );

			// base method

			h.Write(sw, tabs, "public scg.List<" + csharpchildclassname + "> children" + h.IdentifierSeparator + csharpchildclassname + longname 
										+ "( sds.SqlConnection connˡ, int? topNˡ = null, scg.List<acr.CodeGenOrderBy> orderByˡ = null, sds.SqlTransaction tranˡ = null )");
			h.Write(sw, tabs, "{");

			string str = "#Factory factoryˡ = new #Factory() ; ".Replace("#", csharpchildclassname);
			h.Write(sw, tabs + 1, str );

			str = "return factoryˡ.LoadByForeignKey" + h.IdentifierSeparator + "From" + h.IdentifierSeparator + csharpclassname + longname ;
			h.Write(sw, tabs + 1, str);

			str = "(";
			h.Write(sw, tabs + 1, str);

			str = "connˡ,";
			h.Write(sw, tabs + 2, str);

			int pos = 1;
			foreach (string pkcolumn in pkcolumns )
			{
				str = "this." + h.GetCsharpColumnName( pkcolumn, theclass) + ", " ; 
				h.Write(sw, tabs + 2, str);
				pos++;
			}

			str = "topNˡ,";
			h.Write(sw, tabs + 2, str);

			str = "orderByˡ,";
			h.Write(sw, tabs + 2, str);

			str = "tranˡ";
			h.Write(sw, tabs + 2, str);
			
			str = ") ;";
			h.Write(sw, tabs + 1, str);

		} // end
	
	}
}
