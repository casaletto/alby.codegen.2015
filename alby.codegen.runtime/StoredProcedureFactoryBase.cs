using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq ;
using System.Xml;
using System.Xml.XPath;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection ;
using System.Text.RegularExpressions ;
using Microsoft.SqlServer.Server ;
using Microsoft.SqlServer.Types ;

namespace alby.codegen.runtime
{
	public class StoredProcedureFactoryBase<H,D> 
		where H : DatabaseBaseSingletonHelper, new() 
		where D : DatabaseBase<H>, new() 		
	{
		#region state
	
		protected static D _databaseˡ ;
		
		#endregion

		static StoredProcedureFactoryBase()
		{
			_databaseˡ = new D() ;
		}

		public string Databaseˡ
		{
			get
			{ 
				return _databaseˡ.Nameˡ ;
			}
		}

		public string StoredProcedureFqˡ( string schema, string sp )
		{
			return "[" + this.Databaseˡ + "].[" + schema + "].[" + sp + "]" ;
		}

		protected SqlParameter AddParameterˡ( List<SqlParameter> parameters, 
											  string	column, 
											  object	value, 
											  SqlDbType sqltype, 
											  bool		inparam, 
											  int?		size,
											  int?		precision,
											  int?		scale  
										    ) 
		{
			SqlParameter param = new SqlParameter( column, sqltype ) ;
			param.Direction = inparam ? ParameterDirection.Input : ParameterDirection.InputOutput ;

			// out params only
			if ( ! inparam )
			{
				if ( size.HasValue )
					param.Size = size.Value  ;	

				if ( precision.HasValue )
					param.Precision = (byte) precision.Value  ;	

				if ( scale.HasValue )
					param.Scale = (byte) scale.Value  ;	
			}
			
			if (value == null)
				param.Value = System.DBNull.Value ;
			else
				param.Value = value ;
			
			parameters.Add( param );				
			return param ;			
		}

		protected SqlParameter AddParameterUdtˡ( List<SqlParameter> parameters, 
												 string column, 
												 object value, 
												 string udtTypeName, 
												 bool	inparam, 
												 int?	size )
		{
			SqlParameter param	= new SqlParameter();
			param.ParameterName = column;
			param.Direction		= inparam ? ParameterDirection.Input : ParameterDirection.InputOutput ;
			param.SqlDbType		= SqlDbType.Udt ;
			param.UdtTypeName	= udtTypeName;

			if ( ! inparam )
			{
				if ( size.HasValue )
				{
					if ( size == -1 )
						param.Size = int.MaxValue ;
					else
						param.Size = size.Value  ;	
				}
			}

			if (value == null)
				param.Value = System.DBNull.Value;
			else
				param.Value = value;

			parameters.Add( param ) ;
			return param ;			
		}

		protected SqlParameter AddParameterTableTypeˡ( List<SqlParameter> parameters, string column, object ttlist, string tableTypeName )
		{
			SqlParameter param  = new SqlParameter();
			param.ParameterName = column;
			param.Direction		= ParameterDirection.Input ;
			param.SqlDbType		= SqlDbType.Structured ;
			param.TypeName		= tableTypeName ;
			param.Value			= ttlist ;

			parameters.Add( param ) ;
			return param ;			
		}

		protected SqlParameter AddParameterReturnValueˡ( List<SqlParameter> parameters, string column )
		{
			SqlParameter param = new SqlParameter( column, SqlDbType.Int ) ;
			param.Direction = ParameterDirection.ReturnValue ;
			 
			parameters.Add( param );		
			return param ;		
		}

		protected A GetParameterValueˡ<A>( SqlParameter param ) 
		{
			object o = param.Value ;

			if ( o == null ) return default(A) ;
			if ( o == DBNull.Value ) return default(A) ;

			if ( o is INullable ) // udt's implement this guy
			{
				INullable n = o as INullable ;
				if ( n.IsNull ) return default(A) ;
			}

			return (A) o ;
		}

		protected DataSet Executeˡ(	SqlConnection			conn, 
									SqlTransaction			tran, 
									string					schema, 
									string					sp, 
									List<SqlParameter>		parameters
								  )
		{
			string sql = "" ;
			CodeGenEtc.Sql = "";

			try
			{
				if ( parameters == null ) 
					parameters = new List<SqlParameter>() ;

				sql = this.StoredProcedureFqˡ( schema, sp ) ;
				CodeGenEtc.Sql = sql ;

				using ( SqlCommand cmd = new SqlCommand() )
				{
					cmd.Connection		= conn ;
					cmd.CommandTimeout	= CodeGenEtc.Timeout;
					cmd.CommandText		= sql ;
					cmd.CommandType		= CommandType.StoredProcedure ;

					if ( tran != null )
						if ( cmd.Transaction == null )
							 cmd.Transaction = tran ;

					cmd.Parameters.AddRange( parameters.ToArray() ) ;

					using ( DataSet ds = new DataSet() )
						using (SqlDataAdapter da = new SqlDataAdapter(cmd) )
						{
							da.Fill( ds ) ;
							return ds ;
						}
				}
			}
			catch( Exception ex )
			{
			    throw new CodeGenExecuteException( ex.Message, ex, CodeGenEtc.Sql, parameters ) ;
			}
		}

		protected List<T> ToRecordsetˡ<T>( DataSet ds, int n ) where T: RowBase, new() // n = 1, 2, 3 ...
		{
			List<T> list = new List<T>() ;

            if ( ds == null )  
                throw new CodeGenExecuteException( "ToRecordset() dataset is null." ) ;

			if ( ds.Tables.Count >= n )
			{
				DataTable dt = ds.Tables[ n-1 ] ;
				foreach( DataRow dr in dt.Rows ) 
				{
					T t = new T() ;
					FillDictionaryˡ( t.Dictionaryˡ, dt, dr ) ;
					t.IsFromDatabaseˡ = true ;
					t.IsDeletedˡ = false ;
					t.IsDirtyˡ = false ;
					t.IsSavedˡ = true ;

					list.Add( t ) ;
				}
			}
			return list ;
		}

		protected void FillDictionaryˡ( Dictionary<string,object> dic, DataTable dt, DataRow datarow )
		{
			foreach( DataColumn col in dt.Columns )
			{
				if ( datarow.IsNull( col ) )
					dic[ col.ColumnName ] = null ; 
				else	
					dic[ col.ColumnName ] = datarow[ col ] ; 
			}
		}

	} // end class

}
