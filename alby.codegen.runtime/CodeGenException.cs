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
    public class CodeGenException: ApplicationException 
    {
		protected string				_myMessage	= ""  ;
		protected string				_sql		= ""   ;
		protected RowBase				_rowBase	= null ;
		protected List<SqlParameter>	_parameters = new List<SqlParameter>() ;

		public string Sql
		{
			get
			{
				return _sql;
			}
		}

		public RowBase RowBase
		{
			get
			{
				return _rowBase;
			}
		}

		public List<SqlParameter> Parameters
		{
			get
			{
				return _parameters ;
			}
		} 
		
		public CodeGenException() 
		    : base()
		{
		}

		public CodeGenException( string message ) 
		    : this( message, null, null, null, null ) 
		{
		}

		public override string Message
		{
			get
			{
				return _myMessage ;
			}
		}

		public CodeGenException( string message, Exception innerException, string sql, List<SqlParameter> parameters, RowBase obj )
		    : base( message, innerException ) 
		{
			_sql = sql ?? "" ;
			_rowBase = obj ;
			_parameters = parameters ;

			StringBuilder bob = new StringBuilder() ;
			bob.Append(this.GetType().ToString());
			bob.Append("\n");
			bob.Append(message ?? "");
			bob.Append("\n");

			if (_rowBase != null)
			{
				bob.Append("object: ");
				bob.Append(_rowBase.GetType().ToString());
				bob.Append("\n");
			}

			if (_sql.Length > 0)
			{
				bob.Append("sql: ");
				bob.Append(_sql);
				bob.Append("\n");
			}
			_myMessage = "\n" + bob.ToString().Trim() + "\n" ;

			CodeGenEtc.DebugMessage( "======================================================================================");
			CodeGenEtc.DebugMessage( "CODEGEN EXCEPTION" );
			CodeGenEtc.DebugMessage( _myMessage.Trim() );
			CodeGenEtc.DebugMessage( "--------------------------------------------------------------------------------------");
			if ( innerException != null )
				CodeGenEtc.DebugMessage( innerException.ToString() );
			CodeGenEtc.DebugMessage( "======================================================================================");
			CodeGenEtc.DebugMessage( "======================================================================================\n");
		}
    
    } // end class
}
