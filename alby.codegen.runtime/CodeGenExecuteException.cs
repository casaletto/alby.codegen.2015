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
using System.Runtime.Serialization;

namespace alby.codegen.runtime
{
    public class CodeGenExecuteException: CodeGenException 
    {
		public CodeGenExecuteException() 
		    : base()
		{
		}

		public CodeGenExecuteException( string message )
			: base( message, null, null, null, null)
		{
		}

		public CodeGenExecuteException( string message, Exception innerException, string sql, List<SqlParameter> parameters )
		    : base( message, innerException, sql, parameters, null ) 
		{
		}

    
    } // end class
}
