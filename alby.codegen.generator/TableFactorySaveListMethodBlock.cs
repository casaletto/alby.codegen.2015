using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace alby.codegen.generator
{
	public class TableFactorySaveListMethodBlock : CodeBlockBase, IDisposable 
	{
		public TableFactorySaveListMethodBlock(	StreamWriter sw, int tabs, string theclass )
			: base(sw, tabs ) 
		{
			Helper h = new Helper() ;

			// method header - real method
			h.Write(sw, tabs, "public scg.List<" + theclass + "> Saveˡ( sds.SqlConnection connˡ, scg.List<" + theclass + "> rowListˡ, acr.CodeGenSaveStrategy saveStrategyˡ = acr.CodeGenSaveStrategy.Normal,  bool identityProvidedˡ = false, sds.SqlTransaction tranˡ = null )");

			h.Write(sw, tabs, "{");
			h.Write(sw, tabs + 1, "scg.List<" + theclass + "> rowList2ˡ = new scg.List<" + theclass + ">();");

			// execute the query
			h.Write(sw, tabs + 1, "foreach( " + theclass + " rowˡ in rowListˡ )");
			h.Write(sw, tabs + 1, "{");
			h.Write(sw, tabs + 2, theclass + " row2ˡ = this.Saveˡ( connˡ, rowˡ, saveStrategyˡ, identityProvidedˡ, tranˡ ) ;");
			h.Write(sw, tabs + 2, "if ( row2ˡ != null )	rowList2ˡ.Add( row2ˡ ) ;");
			h.Write(sw, tabs + 1, "}");

			h.Write(sw, tabs + 1, "return rowList2ˡ ;");			
		}

		public new void Dispose()
		{
			Helper h = new Helper() ;

			h.Write(_sw, _tabs, "}\r\n");
		}
		
	}
}
