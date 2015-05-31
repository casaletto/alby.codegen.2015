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
    public class CodeGenSaveException: CodeGenException 
    {
		public CodeGenSaveException() 
		    : base()
		{
		}

		public CodeGenSaveException( string message )
			: base( message, null, null, null, null)
		{
		}

		public CodeGenSaveException( string message, Exception innerException, string sql, List<SqlParameter> parameters, RowBase obj)
		    : base( message, innerException, sql, parameters, obj ) 
		{
		}

    
    } // end class
}
