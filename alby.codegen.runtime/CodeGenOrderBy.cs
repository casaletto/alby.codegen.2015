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
using System.Reflection;
using System.Text.RegularExpressions;

namespace alby.codegen.runtime
{
	public class CodeGenOrderBy
	{
		public string		Table  = "" ;
		public string		Column = "" ;
		public CodeGenSort	Sort   = CodeGenSort.Asc ;

		public CodeGenOrderBy( string column )
		{
			this.Column = column;
		}

		public CodeGenOrderBy( string column, CodeGenSort sort )
		{
			this.Column = column;
			this.Sort	= sort;
		}

		public CodeGenOrderBy( string table, string column )
		{
			this.Table	= table;
			this.Column = column;
		}		 

		public CodeGenOrderBy( string table, string column, CodeGenSort sort )
		{
			this.Table	= table ;
			this.Column = column;
			this.Sort	= sort;
		}

	}
}
