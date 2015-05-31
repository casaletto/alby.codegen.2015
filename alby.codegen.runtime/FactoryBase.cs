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
using System.Reflection ;
using System.Text.RegularExpressions ;

namespace alby.codegen.runtime
{
	public class FactoryBase<T,H,D> 
		where T : RowBase, new()
		where H : DatabaseBaseSingletonHelper, new() 
		where D : DatabaseBase<H>, new() 					
	{
		#region state
	
		protected static Assembly	_assemblyˡ			; 
		protected static D			_databaseˡ			;
		protected static string		_schemaˡ			;
		protected static string		_tableˡ				;
		protected static string		_selectˡ			;
		protected static string		_insertˡ			;
		protected static string		_insertIdentityˡ    ;
		protected static string		_updateˡ			;
		protected static string		_deleteˡ			;
		protected static string		_whereLoadPKˡ		;
		protected static string		_whereSavePKˡ		;
		
		#endregion
	
		static FactoryBase()
		{
			_databaseˡ = new D() ;
		}

		//-----------------------------------------------------------------------------------------

		public string Databaseˡ
		{
			get
			{ 
				return _databaseˡ.Nameˡ ;
			}
		}

		//-----------------------------------------------------------------------------------------

		public string Schemaˡ
		{
			get
			{ 
				return _schemaˡ ;
			}
		}

		//-----------------------------------------------------------------------------------------
		 
		public string Tableˡ
		{
			get
			{ 
				return _tableˡ ;
			}
		}

		//-----------------------------------------------------------------------------------------

		public string TableFqˡ
		{
			get
			{ 
				return "[" + this.Databaseˡ + "].[" + this.Schemaˡ + "].[" + this.Tableˡ + "]" ;
			}
		}

		//-----------------------------------------------------------------------------------------

		protected T ExecuteQueryReturnOneˡ(	SqlConnection			conn,
											SqlTransaction			tran,
											List<SqlParameter>		parameters,
											Assembly				assembly,
											string					select, 
											bool					selectResource,
											string					where,
											bool					whereResource
											)
		{
			string sql = "" ;

			List<T> list = ExecuteQueryˡ(conn, tran, parameters, assembly, select, selectResource, where, whereResource, null, null, out sql);
				
			if ( list.Count > 1 )
				throw new CodeGenLoadException( string.Format( "{0} records found, expected exactly 1.", list.Count ), null, sql, parameters, null ) ; 
			
			if (list.Count == 1)
				return list[0];
			else
				return null;
		}

		//-----------------------------------------------------------------------------------------

		protected List<T> ExecuteQueryˡ(	SqlConnection			conn, 
											SqlTransaction			tran,
											List<SqlParameter>		parameters, 
											Assembly				assembly, 
											string					select, 
											bool					selectResource, 
											string					where, 
											bool					whereResource,
											int?					topN,	
											List<CodeGenOrderBy>	orderByList,
																																																														out string sql )
		{
			try
			{
				if ( parameters == null ) 
					parameters = new List<SqlParameter>() ;

				if ( selectResource)
					select = Helper.LoadResource(assembly, select);

				select = string.Format( select, this.TableFqˡ ) ;

				if ( topN != null )
					select = Regex.Replace(select, "^select ", string.Format("select top {0} ", topN.Value ));

				if (whereResource)
					where = Helper.LoadResource(assembly, where);

				string order = "" ;
				if (orderByList != null)
				{
					foreach (CodeGenOrderBy orderBy in orderByList)
					{
						if (order.Length >= 1)
							order += ", ";
						if ( orderBy.Table.Length > 0 )
							order += orderBy.Table + "." ;
						order +=  "[" + orderBy.Column + "] " + Enum.GetName(typeof(CodeGenSort), orderBy.Sort ) ;
					}
					order = "order by " + order;
				}

				sql = select ;
				if ( where.Length >= 1)
					sql += "\n" + where;
				if ( order.Length >= 1 )
					sql += "\n" + order ;
				CodeGenEtc.Sql = sql;

				using (SqlCommand cmd = new SqlCommand(sql, conn))
				{
					cmd.CommandTimeout = CodeGenEtc.Timeout;

					if (tran != null)
						if (cmd.Transaction == null)
							cmd.Transaction = tran;

					cmd.Parameters.AddRange(parameters.ToArray());

					using (DataTable dt = new DataTable())
					{
						using (SqlDataAdapter da = new SqlDataAdapter(cmd))
							da.Fill(dt);

						return ToQueryRowListˡ(dt);
					}
				}
			}
			catch (Exception ex)
			{
				throw new CodeGenLoadException( ex.Message, ex, CodeGenEtc.Sql, parameters, null );
			}
		}
		//-----------------------------------------------------------------------------------------

		protected void AddParameterˡ(List<SqlParameter> parameters, string column, object value)
		{
			//Print( "column [{0}] type [{1}]", column, typeof( ValueType ) ) ;

			if (value == null)
				parameters.Add(new SqlParameter(column, System.DBNull.Value));

			else
				parameters.Add(new SqlParameter(column, value));
		}

		//-----------------------------------------------------------------------------------------

		protected void AddParameterˡ(List<SqlParameter> parameters, string column, object value, SqlDbType sqltype)
		{
			//Print("COLUMN [{0}] TYPE [{1}]", column, sqltype);

			SqlParameter param = new SqlParameter( column, sqltype ) ;
			
			if (value == null)
				param.Value = System.DBNull.Value ;
			else
				param.Value = value ;
			
			parameters.Add( param );				
		}

		//-----------------------------------------------------------------------------------------
		
		protected void AddParameterˡ( List<SqlParameter> parameters, string column, object value, string udtTypeName )
		{
			SqlParameter param = new SqlParameter();
			
			param.ParameterName = column;

			if (value == null)
				param.Value = System.DBNull.Value;
			else
			{
				param.Value = value;
				if (udtTypeName.Length > 0)
					param.UdtTypeName = udtTypeName;
			}

			parameters.Add( param ) ;
		}

		//-----------------------------------------------------------------------------------------

		protected List<T> ToQueryRowListˡ( DataTable dt ) 
		{
			List<T> list = new List<T>() ;

			if ( dt != null )			
				foreach( DataRow datarow in dt.Rows )
				{
					T t = new T() ;
					
					FillDictionaryˡ( t.Dictionaryˡ, dt, datarow ) ;
					
					FillPrimaryKeyDictionaryˡ( t.PrimaryKeyDictionaryˡ, dt, datarow);
					
					if ( t.ConcurrencyColumnˡ.Length > 0 )
						t.ConcurrencyTimestampˡ = datarow[ t.ConcurrencyColumnˡ ] ;
					else
						t.ConcurrencyTimestampˡ = null ;
					
					t.IsDirtyˡ = false;
					t.IsFromDatabaseˡ = true ;
					t.IsSavedˡ = false ;
					t.IsDeletedˡ = false ;
					t.MarkForDeletionˡ = false ;

					list.Add( t ) ;
				}
				
			return list ;
		}

		//-----------------------------------------------------------------------------------------

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

		//-----------------------------------------------------------------------------------------

		protected void FillPrimaryKeyDictionaryˡ(Dictionary<string, object> dic, DataTable dt, DataRow datarow)
		{
			foreach (DataColumn col in dt.Columns)
				if( dic.ContainsKey( col.ColumnName ) )
					dic[col.ColumnName] = datarow[col]; 
		}
		
		//-----------------------------------------------------------------------------------------

		protected SaveEnum ExecuteSaveˡ( T					row,
										SqlConnection		conn, 
										SqlTransaction		tran,
										List<SqlParameter>	parameters, 
										string				insert,
										string				insertIdentity,
										string				update,
										string				delete,
										string				whereSavePK,
										bool				identityTable,
										bool				identityProvided,
										out object			identityID )
		{		
			string sql = "" ;
			CodeGenEtc.Sql = "";

			insert			= string.Format( insert, this.TableFqˡ ) ;
			insertIdentity	= string.Format( insertIdentity, this.TableFqˡ ) ;
			update			= string.Format( update, this.TableFqˡ ) ;
			delete			= string.Format( delete, this.TableFqˡ ) ;

			SaveEnum result = SaveEnum.NoSave ;	
			identityID = null ;
			
			try 
			{
				if ( row.IsDirtyˡ ) // ok - do something
				{}	
				else
				if ( row.MarkForDeletionˡ ) // clean object but required delete
				{}
				else
					return result; // do nothing

				if ( row.IsSavedˡ ) // already saved - dont save again
					throw new ApplicationException( "This object has already been saved." );

				if ( row.IsDeletedˡ ) // already deleted
					throw new ApplicationException("This object has already been deleted.");

				if ( row.MarkForDeletionˡ ) // cant delete memory object
					if ( ! row.IsFromDatabaseˡ )
						throw new ApplicationException("This object cannot be deleted because it is not from the database.");

				// 	! row._IsSaved
				//	! row._IsDeleted			
				// IsDirty || row._MarkForDeletion
				
				if ( row.MarkForDeletionˡ ) // delete has higher precedence
				{
					result = SaveEnum.Delete;
					sql = delete + whereSavePK;
				}
				else
				if ( row.IsFromDatabaseˡ )
				{
					result = SaveEnum.Update;
					sql = update + whereSavePK;
				}
				else 
				{
					result = SaveEnum.Insert;
					if ( identityTable )
					{
						if ( identityProvided ) 
							sql = insertIdentity + " ; select scope_identity() ";
						else
							sql = insert + " ; select scope_identity() ";
					}
					else
						sql = insert ;
				}
				CodeGenEtc.Sql = sql;

				// do it
				using (SqlCommand cmd = new SqlCommand(sql, conn))
				{
					cmd.CommandTimeout = CodeGenEtc.Timeout;

					if (tran != null)
						if (cmd.Transaction == null)
							cmd.Transaction = tran;
	
					cmd.Parameters.AddRange(parameters.ToArray());

					if ( result == SaveEnum.Delete) // delete has higher precedence
					{
						int rowsEffected = cmd.ExecuteNonQuery();
						if (rowsEffected != 1)
							throw new ApplicationException(string.Format("{0} rows deleted, expected exactly 1 to be deleted.", rowsEffected));
					}
					else
					if (result == SaveEnum.Update)
					{
						if ( update.Length > 0 ) // only update if there are fields to update
						{
							int rowsEffected = cmd.ExecuteNonQuery();
							if (rowsEffected != 1)
								throw new ApplicationException(string.Format("{0} rows updated, expected exactly 1 to be updated.", rowsEffected));
						}		
					}
					else
					if (result == SaveEnum.Insert)
					{
						if ( identityTable )
						{
							object id = cmd.ExecuteScalar();
							if (id == null)
								throw new ApplicationException("0 rows inserted, expected exactly 1 to be inserted.");

							identityID = id ; 
						}
						else
						{
							int rowsEffected = cmd.ExecuteNonQuery();
							if (rowsEffected != 1)
								throw new ApplicationException(string.Format("{0} rows inserted, expected exactly 1 to be inserted.", rowsEffected));
						}
					}
					else
						return SaveEnum.NoSave ;
				}

				// save was good

				if ( row.MarkForDeletionˡ )
					row.IsDeletedˡ = true;

				row.IsSavedˡ = true;
				row.IsDirtyˡ = false;
				row.MarkForDeletionˡ = false;
			}
			catch( Exception ex )
			{
			    throw new CodeGenSaveException( ex.Message, ex, CodeGenEtc.Sql, parameters, row ) ;
			}
			
			return result ;
		}

		//-----------------------------------------------------------------------------------------

		protected SaveEnum ExecuteForceSaveˡ(	T					row,
												SqlConnection		conn, 
												SqlTransaction		tran,
												List<SqlParameter>	parameters, 
												CodeGenSaveStrategy saveStrategy,
												string				insert,
												string				insertIdentity,
												string				update,
												string				delete,
												string				whereSavePK, // no concurrency column here, just PK
												bool				identityTable,
												bool				identityProvided,
												out object			identityID ) 
		{
			// ignore all flags except _MarkForDeletion

			identityID = null ;

			string sql = "" ;
			CodeGenEtc.Sql = "" ;

			insert			= string.Format( insert, this.TableFqˡ ) ;
			insertIdentity	= string.Format( insertIdentity, this.TableFqˡ ) ;
			update			= string.Format( update, this.TableFqˡ ) ;
			delete			= string.Format( delete, this.TableFqˡ ) ;
			
			try
			{
				using (SqlCommand cmd = new SqlCommand())
				{
					cmd.Connection = conn ;

					if (tran != null)
						if (cmd.Transaction == null)
							cmd.Transaction = tran;

					cmd.CommandTimeout = CodeGenEtc.Timeout;
					
					cmd.Parameters.AddRange(parameters.ToArray());

					// do delete 
					if (row.MarkForDeletionˡ) 
					{
						sql = delete + whereSavePK;
						CodeGenEtc.Sql = sql;

						cmd.CommandText = sql ;

						int rowsEffected = cmd.ExecuteNonQuery();
						if (rowsEffected > 1)
							throw new ApplicationException( string.Format("{0} rows deleted, expected 0 or 1 to be deleted.", rowsEffected ) );

						return SaveEnum.Delete;
					}

					// do update then insert
					if ( saveStrategy == CodeGenSaveStrategy.ForceSaveTryUpdateFirstThenInsert )
						return this.ExecuteForceSave_TryUpdateFirstˡ( cmd, row, conn, tran, parameters, saveStrategy, insert, insertIdentity, update, delete, whereSavePK, identityTable, identityProvided, out identityID ) ;


					// do insert then update
					if ( saveStrategy == CodeGenSaveStrategy.ForceSaveTryInsertFirstThenUpdate )
						return this.ExecuteForceSave_TryInsertFirstˡ( cmd , row, conn, tran, parameters, saveStrategy, insert, insertIdentity, update, delete, whereSavePK, identityTable, identityProvided, out identityID ) ;

					throw new ApplicationException( "Unexpected force save strategy" ) ;
				}		
			}
			catch (Exception ex)
			{
				throw new CodeGenSaveException( ex.Message, ex, CodeGenEtc.Sql, parameters, row );
			}
		}
		
		//-----------------------------------------------------------------------------------------

		protected SaveEnum ExecuteForceSave_TryUpdateFirstˡ(	SqlCommand			cmd,
																T					row,
																SqlConnection		conn, 
																SqlTransaction		tran,
																List<SqlParameter>	parameters, 
																CodeGenSaveStrategy saveStrategy,
																string				insert,
																string				insertIdentity,
																string				update,
																string				delete,
																string				whereSavePK, // no concurrency column here, just PK
																bool				identityTable,
																bool				identityProvided,
																out object			identityID ) 
		{
			string sql = "" ;
			identityID = null ;

			// see if update effects 1 row
			if ( update.Length > 0 ) // only update if there are fields to update
			{
				sql = update + whereSavePK;
				CodeGenEtc.Sql = sql;

				cmd.CommandText = sql ;

				int rowsEffected = cmd.ExecuteNonQuery();
				if (rowsEffected > 1)
					throw new ApplicationException( string.Format("{0} rows updated, expected 0 or 1 to be updated.", rowsEffected ));

				if (rowsEffected == 1) // update ok
					return SaveEnum.Update;
			}

			// last chance - try insert 
			if ( identityTable )
			{
				if ( identityProvided ) 
					sql = insertIdentity + " ; select scope_identity() ";
				else
					sql = insert + " ; select scope_identity() ";
			}
			else
				sql = insert;

			CodeGenEtc.Sql = sql;
			cmd.CommandText = sql;

			if (identityTable)
			{
				object id = cmd.ExecuteScalar();
				if (id == null)
					throw new ApplicationException("0 rows inserted, expected exactly 1 to be inserted.");

				identityID = id ; 
			}
			else
			{
				int rowsEffected = cmd.ExecuteNonQuery();
				if (rowsEffected != 1)
					throw new ApplicationException(string.Format("{0} rows inserted, expected exactly 1 to be inserted.", rowsEffected));
			}

			return SaveEnum.Insert;
		}

		//-----------------------------------------------------------------------------------------

		protected SaveEnum ExecuteForceSave_TryInsertFirstˡ(	SqlCommand			cmd,
																T					row,
																SqlConnection		conn, 
																SqlTransaction		tran,
																List<SqlParameter>	parameters, 
																CodeGenSaveStrategy saveStrategy,
																string				insert,
																string				insertIdentity,
																string				update,
																string				delete,
																string				whereSavePK, // no concurrency column here, just PK
																bool				identityTable,
																bool				identityProvided,
																out object			identityID ) 
		{
			string sql = "" ;
			identityID = null ;

			// try insert
			if ( identityTable )
			{
				if ( identityProvided ) 
					sql = insertIdentity + " ; select scope_identity() ";
				else
					sql = insert + " ; select scope_identity() ";
			}
			else
				sql = insert;

			CodeGenEtc.Sql = sql;
			cmd.CommandText = sql;

			if (identityTable)
			{
				object id = cmd.ExecuteScalar();
				if (id == null)
					throw new ApplicationException("0 rows inserted, expected exactly 1 to be inserted.");

				identityID = id ; 
				return SaveEnum.Insert;
			}
			else
			{
				int rowsEffected = cmd.ExecuteNonQuery();
				if (rowsEffected == 1)
					return SaveEnum.Insert;
			}

			// last chance - try update
			if ( update.Length > 0 ) // only update if there are fields to update
			{
				sql = update + whereSavePK;

				CodeGenEtc.Sql = sql;
				cmd.CommandText = sql ;

				int rowsEffected = cmd.ExecuteNonQuery();
				if (rowsEffected > 1)
					throw new ApplicationException( string.Format("{0} rows updated, expected 0 or 1 to be updated.", rowsEffected ));

				if (rowsEffected == 1) // update ok
					return SaveEnum.Update;

				throw new ApplicationException("0 rows updated, expected exactly 1 to be updated.");
			}
			else
				throw new ApplicationException( "Nothing was saved - no update fields" ) ;	
		}

		//-----------------------------------------------------------------------------------------

		public int GetRowCountˡ(SqlConnection conn, string where = null, SqlTransaction tran = null ) 
		{
			string sql = "select count(1) from " + this.TableFqˡ ;
			if ( ! string.IsNullOrEmpty( where ) )
				sql += " " + where ;

			return (int) Helper.ExecuteScalar( conn, sql, tran ) ;
		}

		//-----------------------------------------------------------------------------------------
		
		public void DeleteAllˡ( SqlConnection conn, string where = null, SqlTransaction tran = null ) 
		{
			string sql = "delete from " + this.TableFqˡ ;
			if ( ! string.IsNullOrEmpty( where ) )
				sql += " " + where ;

			ExecuteNonQueryˡ(conn, sql, tran);
		}

		//-----------------------------------------------------------------------------------------

		public string GetMaxValueˡ(SqlConnection conn, string col, string where = null, SqlTransaction tran = null )
		{
			string sql = "select top 1 [" + col + "] from " + this.TableFqˡ ;
			if ( ! string.IsNullOrEmpty( where ) )
				sql += " " + where ;
			sql += " order by 1 desc" ;

			object o = Helper.ExecuteScalar( conn, sql, tran );
			if (o == null) return null;
			return o.ToString();
		}

		//-----------------------------------------------------------------------------------------
				
		public string GetMinValueˡ(SqlConnection conn, string col, string where = null, SqlTransaction tran = null )  
		{
			string sql = "select top 1 [" + col + "] from " + this.TableFqˡ  ;
			if ( ! string.IsNullOrEmpty( where ) )
				sql += " " + where ;
			sql += " order by 1 asc" ;

			object o = Helper.ExecuteScalar(conn, sql, tran);
			if (o == null) return null;
			return o.ToString();
		}	

		//-----------------------------------------------------------------------------------------

		public void Truncateˡ(SqlConnection conn, SqlTransaction tran = null )
		{
			string sql = "truncate table " + this.TableFqˡ ;

			ExecuteNonQueryˡ( conn, sql, tran ) ;
		}

		//-----------------------------------------------------------------------------------------

		public void ExecuteNonQueryˡ(SqlConnection conn, string sql, SqlTransaction tran = null )
		{
			Helper.ExecuteNonQuery( conn, sql, tran ) ;
		}

		//-----------------------------------------------------------------------------------------

		public void ExecuteScalarˡ(SqlConnection conn, string sql, SqlTransaction tran = null )
		{
			Helper.ExecuteScalar(conn, sql, tran);
		}

		//-----------------------------------------------------------------------------------------

		public long GetNextIdˡ( SqlConnection conn, string col, bool holdlock, SqlTransaction tran = null ) 
		{
			string sql = "select distinct( isnull( max([" + col + "]),0) + 1  ) from " + this.TableFqˡ ;
			if ( holdlock )
				sql += " with ( tablockx )" ;

			object o = Helper.ExecuteScalar(conn, sql, tran);
			string str = o.ToString();

			double result;
			if (double.TryParse(str, out result))
				return (long)Math.Floor(result);

			throw new CodeGenException("Cant parse [" + str + "] to double");
		}

		//-----------------------------------------------------------------------------------------

		public void SetIdentitySeedˡ( SqlConnection conn, long seed, SqlTransaction tran = null )
		{
			string sql = "dbcc checkident( '" + this.TableFqˡ + "', reseed, " + seed + " ) " ;
			Helper.ExecuteNonQuery( conn, sql, tran ) ;
		}

		//-----------------------------------------------------------------------------------------

		public void ManualIdentityInsertˡ( SqlConnection conn, bool manual, SqlTransaction tran = null )
		{
			string sql = "" ;

			if ( manual )
				sql = "set identity_insert " + this.TableFqˡ + " on  ; " ;
			else
				sql = "set identity_insert " + this.TableFqˡ + " off ; " ;

			Helper.ExecuteNonQuery( conn, sql, tran ) ;
		}

		//-----------------------------------------------------------------------------------------

		public void SetCheckConstraintsˡ( SqlConnection conn, bool on, bool withCheck = true, SqlTransaction tran = null )
		{
			// "ALTER TABLE ? NOCHECK CONSTRAINT all"
			// "ALTER TABLE ? CHECK CONSTRAINT all"
			// "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"

			string sql = "" ;

			if ( on )
			{
				if ( withCheck )
					 sql = "alter table " + this.TableFqˡ + " with check check constraint all " ;
				else
					 sql = "alter table " + this.TableFqˡ + " check constraint all " ;
			}
			else
			{
				sql = "alter table " + this.TableFqˡ + " nocheck constraint all " ;
			}

			Helper.ExecuteNonQuery( conn, sql, tran ) ;
		}

		//-----------------------------------------------------------------------------------------
		
	} // end class
}

