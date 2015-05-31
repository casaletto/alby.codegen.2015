using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Transactions;
using System.Diagnostics ;
using Microsoft.SqlServer.Types;

using NUnit.Framework;

namespace alby.codegen.runtime
{
	public abstract class CodeGenUnitTestBase
	{
		protected TransactionScope	_transaction;
		protected SqlConnection		_connection;
		protected bool				_commitTransaction;
		protected DateTime			_startTime ;
		
		public CodeGenUnitTestBase()
		{
		}

		public abstract string ConnectionString { get ; }
		
		public SqlConnection Connection
		{
			get
			{
				return _connection ;
			}
			set
			{
				_connection = value ;
			}
		}
		
		public TransactionScope TransactionScope
		{
			get
			{
				return _transaction ;
			}
		}
				
		public virtual IsolationLevel TransactionIsolationLevel
		{
			get
			{
				return IsolationLevel.Serializable ;
			}
		}

		public virtual bool QuietMode
		{
			get
			{
				return false ;
			}
		}

		public virtual bool DisableCheckConstraints
		{
			get
			{
				return true ;
			}
		}

		public virtual bool DisableTriggers
		{
			get
			{
				return true ;
			}
		}

		[SetUp]
		public virtual void SetUp()
		{
			CloseConnectionAndTransaction() ;		
			_commitTransaction = false;

			TransactionOptions t = new TransactionOptions();
			t.IsolationLevel = this.TransactionIsolationLevel ;

			// new transaction
			_transaction = new TransactionScope(TransactionScopeOption.RequiresNew, t);
			CodeGenEtc.ConsoleMessage("[CODEGEN UNIT TEST] transaction [{0}] start", Transaction.Current.TransactionInformation.LocalIdentifier);
 
			// new connection
			CodeGenEtc.ConsoleMessage("[CODEGEN UNIT TEST] connection string [{0}]", this.ConnectionString);
			_connection = new SqlConnection(this.ConnectionString);
			_connection.Open();

			// adjust the volume
			if ( this.QuietMode )
				CodeGenEtc.DebugSql = false;
			else
				CodeGenEtc.DebugSql = true;

			// neutralise nasties
			if ( this.DisableCheckConstraints )
				AlterCheckConstraints( false ) ;

			if ( this.DisableTriggers )
				AlterTriggers( false ) ;

			_startTime = DateTime.Now ;
		}

		[TearDown]
		public virtual void TearDown()
		{
			DateTime _finishTime = DateTime.Now;
			TimeSpan ts = _finishTime - _startTime ;
			CodeGenEtc.ConsoleMessage("[CODEGEN UNIT TEST] time taken [{0}] secs", ts.TotalSeconds.ToString("0.00"));

			// re-enable nasties
			if ( this.DisableCheckConstraints )
				AlterCheckConstraints( true );

			if ( this.DisableTriggers )
				AlterTriggers( true );

			CloseConnectionAndTransaction();
		}

		protected void CloseConnectionAndTransaction()
		{
			if ( Transaction.Current != null )
				CodeGenEtc.ConsoleMessage("[CODEGEN UNIT TEST] transaction [{0}] {1}", Transaction.Current.TransactionInformation.LocalIdentifier, _commitTransaction ? "commit" : "rollback");

			// close connection
			if (_connection != null)
			{
				_connection.Close();
				_connection.Dispose();
				_connection = null ;
			}

			// close transaction
			if (_transaction != null)
			{
				if ( _commitTransaction )
					 _transaction.Complete();

				_transaction.Dispose();
				_transaction = null ;
			}

			Assert.IsNull(_connection); 
			Assert.IsNull(_transaction);			
		}

		[TestFixtureSetUp]
		public virtual void TestFixtureSetUp()
		{
		}

		[TestFixtureTearDown]
		public virtual void TestFixtureTearDown()
		{
		}

		// ----------------------------------------------------------------------------------------

		protected virtual void AlterCheckConstraints( bool enable )
		{
			string token = enable ? "check" : "nocheck" ;

			string sql = "alter table ? " + token + " constraint all" ;
			sql = "dbo.sp_MSForEachTable '" + sql + "'" ;

			Helper.ExecuteNonQuery( _connection, sql );
		}

		protected virtual void AlterTriggers( bool enable )
		{
			string token = enable ? "enable" : "disable";

			string sql = token + " trigger all on ?";
			sql = "dbo.sp_MSForEachTable '" + sql + "'";
	
			Helper.ExecuteNonQuery(_connection, sql );
		}

		protected void AssertFlagsObjectNew(RowBase obj)
		{
			Assert.AreEqual(obj.IsSavedˡ, false);
			Assert.AreEqual(obj.IsDeletedˡ, false);
			Assert.AreEqual(obj.IsDirtyˡ, true);
			Assert.AreEqual(obj.IsFromDatabaseˡ, false);
			Assert.AreEqual(obj.MarkForDeletionˡ, false);
		}

		protected void AssertFlagsObjectLoaded(RowBase obj)
		{
			Assert.AreEqual(obj.IsSavedˡ, false);
			Assert.AreEqual(obj.IsDeletedˡ, false);
			Assert.AreEqual(obj.IsDirtyˡ, false);
			Assert.AreEqual(obj.IsFromDatabaseˡ, true);
			Assert.AreEqual(obj.MarkForDeletionˡ, false);
		}

		protected void AssertFlagsBeforeInsert(RowBase obj)
		{
			Assert.AreEqual(obj.IsSavedˡ, false);
			Assert.AreEqual(obj.IsDeletedˡ, false);
			Assert.AreEqual(obj.IsDirtyˡ, true);
			Assert.AreEqual(obj.IsFromDatabaseˡ, false);
			Assert.AreEqual(obj.MarkForDeletionˡ, false);
		}

		protected void AssertFlagsAfterInsert(RowBase obj)
		{
			Assert.AreEqual(obj.IsSavedˡ, true);
			Assert.AreEqual(obj.IsDeletedˡ, false);
			Assert.AreEqual(obj.IsDirtyˡ, false);
			Assert.AreEqual(obj.IsFromDatabaseˡ, false);
			Assert.AreEqual(obj.MarkForDeletionˡ, false);
		}

		protected void AssertFlagsBeforeUpdate(RowBase obj)
		{
			Assert.AreEqual(obj.IsSavedˡ, false);
			Assert.AreEqual(obj.IsDeletedˡ, false);
			Assert.AreEqual(obj.IsDirtyˡ, true);
			Assert.AreEqual(obj.IsFromDatabaseˡ, true);
			Assert.AreEqual(obj.MarkForDeletionˡ, false);
		}

		protected void AssertFlagsAfterUpdate(RowBase obj)
		{
			Assert.AreEqual(obj.IsSavedˡ, true);
			Assert.AreEqual(obj.IsDeletedˡ, false);
			Assert.AreEqual(obj.IsDirtyˡ, false);
			Assert.AreEqual(obj.IsFromDatabaseˡ, true);
			Assert.AreEqual(obj.MarkForDeletionˡ, false);
		}

		protected void AssertFlagsBeforeDelete(RowBase obj)
		{
			Assert.AreEqual(obj.IsSavedˡ, false);
			Assert.AreEqual(obj.IsDeletedˡ, false);
			//Assert.AreEqual(obj.IsDirty, false);
			Assert.AreEqual(obj.IsFromDatabaseˡ, true);
			Assert.AreEqual(obj.MarkForDeletionˡ, true);
		}

		protected void AssertFlagsAfterDelete(RowBase obj)
		{
			Assert.AreEqual(obj.IsSavedˡ, true);
			Assert.AreEqual(obj.IsDeletedˡ, true);
			Assert.AreEqual(obj.IsDirtyˡ, false);
			Assert.AreEqual(obj.IsFromDatabaseˡ, true);
			Assert.AreEqual(obj.MarkForDeletionˡ, false);
		}

		// ----------------------------------------------------------------------------------------

		protected void AssertAreEqual(bool? newobj, bool? oldobj, string fieldname)
		{
			if ( newobj == null && oldobj == null ) 
				return ;

			Assert.IsNotNull(newobj, fieldname);
			Assert.IsNotNull(oldobj, fieldname);

			Assert.AreEqual(newobj.Value, oldobj.Value, fieldname);
		}

		protected void AssertAreEqual(byte? newobj, byte? oldobj, string fieldname)
		{
			if (newobj == null && oldobj == null)
				return ;

			Assert.IsNotNull(newobj, fieldname);
			Assert.IsNotNull(oldobj, fieldname);

			Assert.AreEqual(newobj.Value, oldobj.Value, fieldname);
		}

		protected void AssertAreEqual(byte[] newobj, byte[] oldobj, string fieldname)
		{
			if (newobj == null && oldobj == null)
				return ;

			Assert.IsNotNull(newobj, fieldname);
			Assert.IsNotNull(oldobj, fieldname);

			Assert.IsTrue(newobj.Length > 0, fieldname);
			Assert.IsTrue(oldobj.Length > 0, fieldname);

			Assert.AreEqual(newobj[0], oldobj[0], fieldname);
		}

		protected void AssertAreEqual(decimal? newobj, decimal? oldobj, string fieldname)
		{
			if (newobj == null && oldobj == null)
				return ;

			Assert.IsNotNull(newobj, fieldname);
			Assert.IsNotNull(oldobj, fieldname);

			Assert.AreEqual(newobj.Value, oldobj.Value, fieldname);
		}

		protected void AssertAreEqual(double? newobj, double? oldobj, string fieldname)
		{
			if (newobj == null && oldobj == null)
				return ;

			Assert.IsNotNull(newobj, fieldname);
			Assert.IsNotNull(oldobj, fieldname);

			Assert.AreEqual(	Math.Round(newobj.Value, 4).ToString(),
								Math.Round(oldobj.Value, 4).ToString(),
								fieldname);
		}

		protected void AssertAreEqual(float? newobj, float? oldobj, string fieldname)
		{
			if (newobj == null && oldobj == null)
				return ;

			Assert.IsNotNull(newobj, fieldname);
			Assert.IsNotNull(oldobj, fieldname);

			Assert.AreEqual(	Math.Round(newobj.Value, 4).ToString(), 
								Math.Round(oldobj.Value, 4).ToString(), 
								fieldname);
		}

		protected void AssertAreEqual(int? newobj, int? oldobj, string fieldname)
		{
			if (newobj == null && oldobj == null)
				return ;

			Assert.IsNotNull(newobj, fieldname);
			Assert.IsNotNull(oldobj, fieldname);

			Assert.AreEqual(newobj.Value, oldobj.Value, fieldname);
		}

		protected void AssertAreEqual(long? newobj, long? oldobj, string fieldname)
		{
			if (newobj == null && oldobj == null)
				return ;

			Assert.IsNotNull(newobj, fieldname);
			Assert.IsNotNull(oldobj, fieldname);

			Assert.AreEqual(newobj.Value, oldobj.Value, fieldname);
		}

		protected void AssertAreEqual(short? newobj, short? oldobj, string fieldname)
		{
			if (newobj == null && oldobj == null)
				return ;

			Assert.IsNotNull(newobj, fieldname);
			Assert.IsNotNull(oldobj, fieldname);

			Assert.AreEqual(newobj.Value, oldobj.Value, fieldname);
		}

		protected void AssertAreEqual(string newobj, string oldobj, string fieldname)
		{
			if (newobj == null && oldobj == null)
				return ;

			Assert.IsNotNull(newobj, fieldname);
			Assert.IsNotNull(oldobj, fieldname);

			Assert.AreEqual(newobj.Trim(), oldobj.Trim(), fieldname);
		}

		protected void AssertAreEqual(Guid? newobj, Guid? oldobj, string fieldname)
		{
			if (newobj == null && oldobj == null)
				return ;

			Assert.IsNotNull(newobj, fieldname);
			Assert.IsNotNull(oldobj, fieldname);

			Assert.AreEqual(newobj.Value, oldobj.Value, fieldname);
		}

		protected void AssertAreEqual( DateTime? newobj, DateTime? oldobj, string fieldname)
		{
			if (newobj == null && oldobj == null)
				return ;

			Assert.IsNotNull(newobj, fieldname);
			Assert.IsNotNull(oldobj, fieldname);

			Assert.AreEqual(newobj.Value, oldobj.Value, fieldname);
		}

		protected void AssertAreEqual(DateTimeOffset? newobj, DateTimeOffset? oldobj, string fieldname)
		{
			if (newobj == null && oldobj == null)
				return;

			Assert.IsNotNull(newobj, fieldname);
			Assert.IsNotNull(oldobj, fieldname);

			Assert.AreEqual(newobj.Value, oldobj.Value, fieldname);
		}

		protected void AssertAreEqual( TimeSpan? newobj, TimeSpan? oldobj, string fieldname )
		{
			if (newobj == null && oldobj == null)
				return;

			Assert.IsNotNull(newobj, fieldname);
			Assert.IsNotNull(oldobj, fieldname);

			Assert.AreEqual(newobj.Value, oldobj.Value, fieldname);
		}

		protected void AssertAreEqual( SqlGeography newobj, SqlGeography oldobj, string fieldname)
		{
			if (newobj == null && oldobj == null)
				return;

			Assert.IsNotNull(newobj, fieldname);
			Assert.IsNotNull(oldobj, fieldname);

			Assert.AreEqual( newobj.ToString(), oldobj.ToString(), fieldname ) ;
		}

		protected void AssertAreEqual( SqlGeometry newobj, SqlGeometry oldobj, string fieldname)
		{
			if (newobj == null && oldobj == null)
				return;

			Assert.IsNotNull(newobj, fieldname);
			Assert.IsNotNull(oldobj, fieldname);

			Assert.AreEqual( newobj.ToString(), oldobj.ToString(), fieldname ) ;
		}

		protected void AssertAreEqual(SqlHierarchyId? newobj, SqlHierarchyId? oldobj, string fieldname)
		{
			if (newobj == null && oldobj == null)
				return;

			Assert.IsNotNull(newobj, fieldname);
			Assert.IsNotNull(oldobj, fieldname);

			Assert.AreEqual( newobj.ToString(), oldobj.ToString(), fieldname ) ;
		}

		// ----------------------------------------------------------------------------------------

		protected bool? tobool(string str)
		{
			if (str == null) return null;

			bool result;

			if (bool.TryParse(str, out result))
				return result;

			throw new CodeGenException("Cant parse [" + str + "] to bool");
		}

		protected byte? tobyte(string str)
		{
			if (str == null) return null;

			byte result;

			if (byte.TryParse(str, out result))
				return result;

			throw new CodeGenException("Cant parse [" + str + "] to byte");
		}

		protected decimal? todecimal(string str)
		{
			if (str == null) return null;
			
			decimal result ;
			
			if ( decimal.TryParse( str, out result ) )
				return result ;
			
			throw new CodeGenException( "Cant parse [" + str + "] to decimal" ) ;
		}

		protected double? todouble(string str)
		{
			if (str == null) return null;

			double result;

			if (double.TryParse(str, out result))
				return result;

			throw new CodeGenException("Cant parse [" + str + "] to double");
		}

		protected float? tofloat(string str)
		{
			if (str == null) return null;

			float result;

			if (float.TryParse(str, out result))
				return result;

			throw new CodeGenException("Cant parse [" + str + "] to float");
		}

		protected int? toint(string str)
		{
			if (str == null) return null;

			int result;

			if (int.TryParse(str, out result))
				return result;

			throw new CodeGenException("Cant parse [" + str + "] to int");
		}

		protected long? tolong(string str)
		{
			if (str == null) return null;

			long result;

			if (long.TryParse(str, out result))
				return result;

			throw new CodeGenException("Cant parse [" + str + "] to long");
		}

		protected short? toshort(string str)
		{
			if (str == null) return null;

			short result;

			if (short.TryParse(str, out result))
				return result;

			throw new CodeGenException("Cant parse [" + str + "] to short");
		}

		protected string tostring(string str)
		{
			return str ;
		}

		protected Guid? toGuid(string str)
		{
			if (str == null) return null;

			return new Guid( str ) ;
		}

		protected DateTime? toDateTime(string str)
		{
			if (str == null) return null;

			DateTime result;

			if (DateTime.TryParse(str, out result))
				return result;

			throw new CodeGenException("Cant parse [" + str + "] to DateTime");
		}

		protected DateTimeOffset? toDateTimeOffset(string str)
		{
			if (str == null) return null;

			DateTimeOffset result;

			if (DateTimeOffset.TryParse(str, out result))
				return result;

			throw new CodeGenException("Cant parse [" + str + "] to DateTimeOffset");
		}

		protected TimeSpan? toTimeSpan(string str)
		{
			if (str == null) return null;

			TimeSpan result;

			if (TimeSpan.TryParse(str, out result))
				return result;

			throw new CodeGenException("Cant parse [" + str + "] to TimeSpan");
		}

		protected SqlGeography toSqlGeography( string str )
		{
			if (str == null) return null;

			return SqlGeography.Parse(str);
		}

		protected SqlGeometry toSqlGeometry(string str)
		{
			if (str == null) return null;

			return SqlGeometry.Parse(str);
		}

	}
}
