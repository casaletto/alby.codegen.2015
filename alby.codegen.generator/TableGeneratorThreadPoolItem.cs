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
using alby.core.threadpool ;

namespace alby.codegen.generator
{
	public partial class TableGeneratorParameters
	{
		 public Program		p ;
		 public string		fqtable ;
		 public Exception	exception ;

	} // end class

	//--------------------------------------------------------------------------------------------------------------------		
	//--------------------------------------------------------------------------------------------------------------------		
	//--------------------------------------------------------------------------------------------------------------------		

	public partial class TableGeneratorThreadPoolItem : MyThreadPoolItemBase
	{
		protected TableGeneratorParameters _tgp ;

		//--------------------------------------------------------------------------------------------------------------------		

		public TableGeneratorThreadPoolItem( TableGeneratorParameters tgp ) 
		{
			_tgp = tgp ;
		}

		//--------------------------------------------------------------------------------------------------------------------		

		public override void Run() 
		{
			Helper h = new Helper() ;

			try
			{
				DoTable( _tgp.p, _tgp.fqtable ) ; 		
			}
			catch( Exception ex )
			{
				_tgp.exception = ex ;
				h.Message( "[DoTable() EXCEPTION]\n{0}", ex ) ;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------		

		protected void DoTable( Program p, string fqtable ) 
		{
			Helper		 h  = new Helper() ;
			ColumnInfo   ci = new ColumnInfo() ;

			List<string>			   parameters			= new List<string>();
			Dictionary<string, string> parameterdictionary	= new Dictionary<string, string>();

			Tuple<string,string> schematable = h.SplitSchemaFromTable( fqtable ) ;

			string thedatabase = h.GetCsharpClassName( null, null, p._databaseName ) ; 

			string csharpnamespace		= p._namespace + "." + p._tableSubDirectory;
			string resourcenamespace	= p._resourceNamespace + "." + p._tableSubDirectory;

			string theclass				= h.GetCsharpClassName( p._prefixObjectsWithSchema, schematable.Item1, schematable.Item2 );
			string csharpfile			= p._directory + @"\" + p._tableSubDirectory + @"\" + theclass + ".cs";
			string csharpchildrenfile	= p._directory + @"\" + p._tableSubDirectory + @"\" + theclass + "Children.cs";

			string csharpfactoryfile	= csharpfile.Replace(".cs", "Factory.cs");
			string thefactoryclass		= theclass + "Factory";

			// config for this table, if any
			string  xpath	 = "/CodeGen/Tables/Table[@Class='" + theclass + "']";
			XmlNode xmltable = p._codegen.SelectSingleNode(xpath);

			// select sql
			string selectsql = "select * from {0} t "; 
	
			// get field list
			List< Tuple<string,string> > columns = ci.GetTableColumns( fqtable ) ;

			// get computed columns
			List<string> computedcolumns = p._di.ComputedColumns.Get( fqtable );

			// get identity columns
			List<string> identitycolumns = p._di.IdentityColumns.Get( fqtable ) ;

			// get timestamp columns
			List<string> timestampcolumns = p._di.TimestampColumns.Get( fqtable ) ;

			// get primary keys
			List<string> pkcolumns = p._di.PrimaryKeyColumns.Get( fqtable, "PK" ) ; 

			// do class
			h.MessageVerbose( "[{0}]", csharpfile );
			using ( StreamWriter sw = new StreamWriter( csharpfile, false, UTF8Encoding.UTF8 ))
			{
				int tab = 0;

				// header
				h.WriteCodeGenHeader(sw);
				h.WriteUsing(sw);

				// namespace
				using ( NamespaceBlock nsb = new NamespaceBlock( sw, tab++, csharpnamespace ) )
				{
					using (ClassBlock cb = new ClassBlock( sw, tab++, theclass, "acr.RowBase" ) )
					{
						// properties and constructor
						using (RowConstructorBlock conb = new RowConstructorBlock( sw, tab, theclass, columns, pkcolumns, p._concurrencyColumn ))
						{}

					} // end class		

				} // end namespace

			} // eof

			// do children objects in class
			h.MessageVerbose( "[{0}]", csharpchildrenfile );
			using ( StreamWriter sw = new StreamWriter( csharpchildrenfile, false, UTF8Encoding.UTF8) )
			{
				int tab = 0;

				// header
				h.WriteCodeGenHeader(sw);
				h.WriteUsing(sw);

				// namespace
				using ( NamespaceBlock nsb = new NamespaceBlock( sw, tab++, csharpnamespace ))
				{
					using ( ClassBlock cb = new ClassBlock( sw, tab++, theclass, "acr.RowBase" ))
					{
						// get parent objects for this object
						List< Tuple<string,string> >  parenttables = p._di.ParentTables.Get( fqtable, "PK" ) ;
						foreach( Tuple<string,string> parenttable in parenttables )
						{
							string fqparenttable = parenttable.Item1 ;
							string fkconstraint  = parenttable.Item2;

							List<string> fkcolumns = p._di.ConstraintColumns_ChildSide.Get( fqtable, fkconstraint, "PK" ) ;
							if ( fkcolumns.Count > 0 )
							{
								h.MessageVerbose("[{0}].[{1}].parent [{2}]", csharpnamespace, theclass, fqparenttable);

								using ( ParentObjectBlock pob = new ParentObjectBlock( sw, tab, p, fqparenttable, fkcolumns, theclass ))
								{}
							}
						}	
	
						// get children objects for this object  
						List< Tuple<string,string> >  childtables = p._di.ChildTables.Get( fqtable ) ;
						foreach( Tuple<string,string> childtable in childtables )
						{
							string fqchildtable = childtable.Item1 ;
							string fkconstraint = childtable.Item2;

							List<string> fkcolumns = p._di.ConstraintColumns_ChildSide.Get( fqchildtable, fkconstraint, "PK" ) ;
							if ( fkcolumns.Count > 0 )
							{
								List<string> parentcolumns = p._di.ConstraintColumns_ParentSide.Get( fqtable, fkconstraint, "PK" ) ;

								h.MessageVerbose("[{0}].[{1}].child [{2}]", csharpnamespace, theclass, fqchildtable );
								using ( ChildObjectBlock cob = new ChildObjectBlock( sw, tab, p, fqtable, fqchildtable, fkcolumns, parentcolumns, theclass ))
								{}
						    }
						}	

					} // end class		

				} // end namespace

			} // eof

			// do class factory
			h.MessageVerbose( "[{0}]", csharpfactoryfile );
			using ( StreamWriter sw = new StreamWriter( csharpfactoryfile, false, UTF8Encoding.UTF8 ))
			{
				int tab = 0;

				// header
				h.WriteCodeGenHeader(sw);
				h.WriteUsing( sw, p._namespace );

				// namespace
				using ( NamespaceBlock nsb = new NamespaceBlock( sw, tab++, csharpnamespace ))
				{
					using ( ClassBlock cb = new ClassBlock( sw, tab++, thefactoryclass,
																"acr.FactoryBase< " + 
																theclass + ", " + 
																"ns." + p._databaseSubDirectory + "." + thedatabase + "DatabaseSingletonHelper, " +
																"ns." + p._databaseSubDirectory + "." + thedatabase + "Database >" 
														  ) )
					{
						// primary key parameter dictionary
						Dictionary<string, string> parameterdictionarypk = new Dictionary<string, string>();
						foreach( string pkcolumn in pkcolumns )
						{
							foreach( var column in columns )
								if ( pkcolumn == column.Item1 )
								{
									string name = h.GetCsharpColumnName( pkcolumn, theclass );
									string type = h.GetCsharpColumnType( column.Item2 );

									parameterdictionarypk.Add(name, type);
								}
						}

						string whereloadpk = this.GetWhereClausePK( pkcolumns, parameterdictionarypk, "",                   columns, theclass ); 
						string wheresavepk = this.GetWhereClausePK( pkcolumns, parameterdictionarypk, p._concurrencyColumn, columns, theclass ); 
						
						string deletesql		 = "delete {0} ";	

						string insertsql		 = "insert {0}" + this.GetInsertClause( columns, computedcolumns, identitycolumns, timestampcolumns, theclass); 
						string insertidentitysql = "insert {0}" + this.GetInsertClause( columns, computedcolumns, new List<string>(), timestampcolumns, theclass); 

						string updatesql = this.GetUpdateClause( columns, computedcolumns, identitycolumns, timestampcolumns, theclass ); 
						if ( updatesql.Length > 0 )
							 updatesql = "update {0}" + updatesql ;						

						// constructor
						using (TableFactoryConstructorBlock conb = new TableFactoryConstructorBlock( sw, tab, fqtable, selectsql, insertsql, insertidentitysql, updatesql, deletesql, whereloadpk, wheresavepk, thefactoryclass ))
						{}

						// save method - only if table has pk
						if ( pkcolumns.Count > 0)
						{
							parameterdictionary = new Dictionary<string, string>();
							parameterdictionary.Add("connˡ", "sds.SqlConnection");

							foreach (string key in parameterdictionarypk.Keys)
								parameterdictionary.Add(key, parameterdictionarypk[key]);
	
							h.MessageVerbose("[{0}].[{1}].[{2}]", csharpnamespace, theclass, "Save" );

							// save the object
							using (TableFactorySaveMethodBlock mb = new TableFactorySaveMethodBlock( sw, tab, parameterdictionary, columns, identitycolumns, p._concurrencyColumn, theclass ))
							{}

							// save a list of objects 
							using (TableFactorySaveListMethodBlock mb1 = new TableFactorySaveListMethodBlock( sw, tab, theclass ))
							{}
						}

						// load by primary key method - only if table has pk
						if ( pkcolumns.Count > 0 )
						{
							parameters = new List<string>();
							parameters.Add("connˡ" );

							foreach (string key in parameterdictionarypk.Keys)
								parameters.Add( key ) ;
							
							parameters.Add( "tranˡ" );

							parameterdictionary = new Dictionary<string, string>();
							parameterdictionary.Add("connˡ", "sds.SqlConnection");

							foreach ( string key in parameterdictionarypk.Keys )
								parameterdictionary.Add( key, parameterdictionarypk[key] ) ;

							parameterdictionary.Add("tranˡ", "sds.SqlTransaction");
							
							h.MessageVerbose("[{0}].[{1}].[{2}]", csharpnamespace, theclass, "LoadByPrimaryKeyˡ");
							using (TableFactoryPrimaryKeyMethodBlock mb = new TableFactoryPrimaryKeyMethodBlock( sw, tab, "LoadByPrimaryKeyˡ", parameters, parameterdictionary, theclass, resourcenamespace ))
							{}
						}

						// load by foreign key methods
						List< Tuple<string,string> > foreignkeys = p._di.ForeignKeys.Get( fqtable );
						foreach ( Tuple<string,string> foreignkey in foreignkeys )
						{
							string fqfktable	= foreignkey.Item1 ; 
							string fkconstraint = foreignkey.Item2 ; 

							Tuple<string,string> fkschematable = h.SplitSchemaFromTable( fqfktable ) ;

							string fktable = h.GetSchemaAndTableName( p._prefixObjectsWithSchema, fkschematable.Item1, fkschematable.Item2 ) ;
							List<string> fkcolumns = p._di.ForeignKeyConstraintColumns.Get( fqtable, fkconstraint ) ;

							// foreign key parameter dictionary
							parameters = new List<string>() ;
							parameters.Add( "connˡ" );

							foreach ( string fkcolumn in fkcolumns )
							{
								foreach( var column in columns )
									if ( fkcolumn == column.Item1 )
									{
										string name = h.GetCsharpColumnName( fkcolumn, theclass );
										parameters.Add( name );
									}
							}

							parameters.Add( "topNˡ"    );
							parameters.Add( "orderByˡ" );
							parameters.Add( "tranˡ"    );

							Dictionary<string, string> parameterdictionaryfk = new Dictionary<string, string>();
							parameterdictionaryfk.Add("connˡ", "sds.SqlConnection");
	
							foreach ( string fkcolumn in fkcolumns )
							{
								foreach( var column in columns )
									if ( fkcolumn == column.Item1 )
									{
										string name = h.GetCsharpColumnName( fkcolumn, theclass );
										string type = h.GetCsharpColumnType( column.Item2 );

										parameterdictionaryfk.Add( name, type );
									}
							}

							parameterdictionaryfk.Add("topNˡ", "int?");
							parameterdictionaryfk.Add("orderByˡ", "scg.List<acr.CodeGenOrderBy>");
							parameterdictionaryfk.Add("tranˡ", "sds.SqlTransaction");

							string wherefk = this.GetWhereClauseFK( fkcolumns, theclass ); 

							string byparameters = "";
							foreach ( string fkcolumn in fkcolumns )
								byparameters += h.GetCsharpColumnName( fkcolumn, theclass);

							string method = "LoadByForeignKey" + h.IdentifierSeparator + "From" + h.IdentifierSeparator + fktable + h.IdentifierSeparator + "By" + h.IdentifierSeparator + byparameters ;

							h.MessageVerbose( "[{0}].[{1}].fk [{2}]", csharpnamespace, theclass, method );
							using ( TableFactoryForeignKeyMethodBlock mb = new TableFactoryForeignKeyMethodBlock( sw, tab, method, parameters, parameterdictionaryfk, wherefk, theclass, resourcenamespace ))
							{}
						}

						// load all method
						parameters = new List<string>() ;
						parameters.Add("connˡ" );
						parameters.Add("topNˡ" );
						parameters.Add("orderByˡ" );
						parameters.Add("tranˡ" );

						parameterdictionary = new Dictionary<string, string>();
						parameterdictionary.Add("connˡ", "sds.SqlConnection");
						parameterdictionary.Add("topNˡ", "int?");
						parameterdictionary.Add("orderByˡ", "scg.List<acr.CodeGenOrderBy>");
						parameterdictionary.Add("tranˡ", "sds.SqlTransaction");

						h.MessageVerbose("[{0}].[{1}].[{2}]", csharpnamespace, theclass, "Loadˡ");
						using ( ViewFactoryMethodBlock mb = new ViewFactoryMethodBlock( sw, tab, "Loadˡ", parameters, parameterdictionary, "", theclass, resourcenamespace ))
						{}

						// load by where
						parameters = new List<string>() ;
						parameters.Add("connˡ" );
						parameters.Add("whereˡ" );
						parameters.Add("parametersˡ" );
						parameters.Add("topNˡ" );
						parameters.Add("orderByˡ" );
						parameters.Add("tranˡ" );

						parameterdictionary = new Dictionary<string, string>();
						parameterdictionary.Add("connˡ", "sds.SqlConnection");
						parameterdictionary.Add("whereˡ", "string");
						parameterdictionary.Add("parametersˡ", "scg.List<sds.SqlParameter>");
						parameterdictionary.Add("topNˡ", "int?");
						parameterdictionary.Add("orderByˡ", "scg.List<acr.CodeGenOrderBy>");
						parameterdictionary.Add("tranˡ", "sds.SqlTransaction");

						h.MessageVerbose("[{0}].[{1}].[{2}]", csharpnamespace, theclass, "LoadByWhereˡ");
						using ( ViewFactoryMethodBlock mb = new ViewFactoryMethodBlock( sw, tab, "LoadByWhereˡ", parameters, parameterdictionary, "", theclass, resourcenamespace ))
						{}

						// other load methods
						if ( xmltable != null )
						{
							XmlNodeList xmlmethods = xmltable.SelectNodes("Methods/Method");
							foreach ( XmlNode xmlmethod in xmlmethods )
							{
								string method = xmlmethod.SelectSingleNode("@Name").InnerText;

								// where resource
								string whereresource = xmlmethod.SelectSingleNode("@Where").InnerText;

								// parameters
								parameters = new List<string>() ;
								parameters.Add("connˡ" );

								XmlNodeList xmlparameters = xmlmethod.SelectNodes("Parameters/Parameter");
								foreach ( XmlNode xmlparameter in xmlparameters )
									parameters.Add( xmlparameter.SelectSingleNode("@Name").InnerText );

								parameters.Add("topNˡ" );
								parameters.Add("orderByˡ" );
								parameters.Add("tranˡ" );

								parameterdictionary = new Dictionary<string, string>();
								parameterdictionary.Add("connˡ", "sds.SqlConnection");

								xmlparameters = xmlmethod.SelectNodes("Parameters/Parameter");
								foreach ( XmlNode xmlparameter in xmlparameters )
									parameterdictionary.Add( xmlparameter.SelectSingleNode("@Name").InnerText, 
															 xmlparameter.SelectSingleNode("@Type").InnerText ) ;

								parameterdictionary.Add("topNˡ", "int?");
								parameterdictionary.Add("orderByˡ", "scg.List<acr.CodeGenOrderBy>");
								parameterdictionary.Add("tranˡ", "sds.SqlTransaction");

								// method
								h.MessageVerbose( "[{0}].[{1}].method [{2}]", csharpnamespace, theclass, method );
								using (ViewFactoryMethodBlock mb = new ViewFactoryMethodBlock( sw, tab, method, parameters, parameterdictionary, whereresource, theclass, resourcenamespace ))
								{}
							}
						}

					} // end class

				} // end namespace

			} // eof		
			
		} // end do table		

		//--------------------------------------------------------------------------------------------------------------------		

		protected string GetWhereClausePK(  List<string>					primaryKeyColumns, 
											Dictionary<string,string>		paramdic, 
											string							concurrencyColumn, 
											List< Tuple<string,string> >	columns, 
											string							csharpclassname )
		{
			Helper h = new Helper() ;

			string where = "" ;

			if ( primaryKeyColumns.Count > 0)
			{
				foreach ( string primaryKeyColumn in primaryKeyColumns)
				{
					string primaryKey = h.GetCsharpColumnName( primaryKeyColumn, csharpclassname );					
					foreach (string name in paramdic.Keys)
						if (name == primaryKey)
						{
							if (where.Length > 0) where += "and ";
							where += "[" + primaryKeyColumn + "] = @pk_" + h.GetCsharpColumnName( primaryKeyColumn, csharpclassname ) + " ";
						}
				}
				
				if ( concurrencyColumn.Length > 0 )
					foreach( var column in columns )
						if ( column.Item1 == concurrencyColumn )
						{
							if (where.Length > 0) where += "and " ;
							where += "[" + concurrencyColumn + "] = @concurrency_" + h.GetCsharpColumnName( concurrencyColumn, csharpclassname ) + " ";
							break ;
						}
				
				where = "where " + where;
			}
			return where ;
		}

		//--------------------------------------------------------------------------------------------------------------------		

		protected string GetWhereClauseFK( List<string> columns, string csharpclassname )
		{
			Helper h = new Helper() ;

			string where = "";

			if ( columns.Count > 0)
			{
				foreach ( string column in columns )
				{
					string key = h.GetCsharpColumnName( column, csharpclassname );
					
					if (where.Length > 0) where += "and ";

					where += "[" + column + "] = @" + h.GetCsharpColumnName( column, csharpclassname ) + " ";
				}
				where = "where " + where;
			}
			return where;
		}

		//--------------------------------------------------------------------------------------------------------------------		

		protected string GetInsertClause(	List< Tuple<string,string> >	columns, 
											List<string>					computedcolumns, 
											List<string>					identitycolumns, 
											List<string>					timestampcolumns, 
											string							csharpclassname )
		{
			// insert into [a].[b] ( [1], [2], [3] ) values ( @1, @2, @3 )
			// exclude computed columns
			// exclude identity columns
			// exclude timestamp columns

			Helper h = new Helper() ;
			
			string sqlcolumns = "" ;
			string sqlvalues  = "" ;
			
			// columns 
			foreach( var column in columns )
			{
				if ( computedcolumns.Contains( column.Item1 ))
					 continue ;

				if ( identitycolumns.Contains( column.Item1 ))
					 continue;

				if ( timestampcolumns.Contains( column.Item1 ))
					 continue;
			
				if ( sqlcolumns.Length > 0)
					 sqlcolumns += ", ";

				sqlcolumns += "[" + column.Item1 + "]";	
			}
			
			// values - parameters
			foreach( var column in columns )
			{
				if ( computedcolumns.Contains( column.Item1 ))
					 continue;

				if ( identitycolumns.Contains( column.Item1 ))
					 continue;

				if ( timestampcolumns.Contains( column.Item1 ))
					 continue;

				if ( sqlvalues.Length > 0)
					 sqlvalues += ", ";

				sqlvalues += "@" + h.GetCsharpColumnName( column.Item1, csharpclassname );
			}
			
			if ( sqlcolumns == "" )
			 	 return " default values " ;
			else	
				 return " ( " + sqlcolumns + " ) values ( " + sqlvalues + " ) " ;
		}

		//--------------------------------------------------------------------------------------------------------------------		

		protected string GetUpdateClause(	List< Tuple<string,string> >	columns, 
											List<string>					computedcolumns, 
											List<string>					identitycolumns, 
											List<string>					timestampcolumns, 
											string							csharpclassname )
		{
			// update  [a].[b] 
			// set [1] = @p1, [2] = @p2, ...

			// exclude computed columns
			// exclude identity columns
			// exclude timestamp columns

			Helper h = new Helper() ;

			string sql = "" ;
			
			// columns 
			foreach( var column in columns )
			{
				if ( identitycolumns.Contains( column.Item1 ) )
					 continue;

				if ( computedcolumns.Contains( column.Item1 ) )
					 continue;

				if ( timestampcolumns.Contains( column.Item1 ) )
					 continue;

				if ( sql.Length > 0)
					 sql += ", " ;

				sql += "[" + column.Item1 + "] = @" + h.GetCsharpColumnName( column.Item1, csharpclassname )  ;
			}
			
			if ( sql == "" ) 
				 return "" ;
			else	
				 return " set " + sql + " " ;
		}

		//--------------------------------------------------------------------------------------------------------------------		
		
	} // end class	

}
